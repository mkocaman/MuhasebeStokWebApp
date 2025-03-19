using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using System.Xml;
using Microsoft.Extensions.Logging;
using MuhasebeStokWebApp.Data;
using MuhasebeStokWebApp.Data.Repositories;
using MuhasebeStokWebApp.Data.Entities;
using MuhasebeStokWebApp.Enums;
using Microsoft.EntityFrameworkCore;
using MuhasebeStokWebApp.Models;
using MuhasebeStokWebApp.ViewModels.Kur;

namespace MuhasebeStokWebApp.Services
{
    public class DovizService : IDovizService
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<DovizService> _logger;
        private readonly HttpClient _httpClient;
        private readonly IRepository<KurDegeri> _kurDegeriRepository;
        private readonly IRepository<SistemAyarlari> _sistemAyariRepository;
        private readonly IRepository<DovizIliski> _dovizIliskiRepository;
        private readonly ILogService _logService;
        private readonly IRepository<ParaBirimi> _paraBirimiRepository;

        public DovizService(
            ApplicationDbContext context,
            ILogger<DovizService> logger,
            HttpClient httpClient,
            IRepository<KurDegeri> kurDegeriRepository,
            IRepository<SistemAyarlari> sistemAyariRepository,
            IRepository<DovizIliski> dovizIliskiRepository,
            ILogService logService,
            IRepository<ParaBirimi> paraBirimiRepository)
        {
            _context = context;
            _logger = logger;
            _httpClient = httpClient;
            _kurDegeriRepository = kurDegeriRepository;
            _sistemAyariRepository = sistemAyariRepository;
            _dovizIliskiRepository = dovizIliskiRepository;
            _logService = logService;
            _paraBirimiRepository = paraBirimiRepository;
        }

        public async Task<bool> GuncelleKurlariAsync()
        {
            try
            {
                _logger.LogInformation("Döviz kurları güncelleniyor...");
                
                // TCMB'den güncel kurları al
                var tcmbKurlar = await GetTCMBKurlariAsync();
                if (tcmbKurlar == null || !tcmbKurlar.Any())
                {
                    _logger.LogWarning("TCMB'den kur bilgileri alınamadı.");
                    return false;
                }

                // Kurları veritabanına kaydet
                foreach (var kur in tcmbKurlar)
                {
                    await _kurDegeriRepository.AddAsync(kur);
                }

                await _logService.LogEkleAsync("Döviz kurları güncellendi.", Models.LogTuru.Bilgi);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Döviz kurları güncellenirken hata oluştu: {Message}", ex.Message);
                await _logService.LogEkleAsync($"Döviz kurları güncellenirken hata oluştu: {ex.Message}", Models.LogTuru.Hata);
                return false;
            }
        }

        private async Task<List<KurDegeri>> GetTCMBKurlariAsync()
        {
            try
            {
                var response = await _httpClient.GetStringAsync("https://www.tcmb.gov.tr/kurlar/today.xml");
                var xmlDoc = new XmlDocument();
                xmlDoc.LoadXml(response);

                var kurlar = new List<KurDegeri>();
                var tarih = DateTime.Now.Date;

                var kurNodes = xmlDoc.SelectNodes("//Currency");
                if (kurNodes == null)
                {
                    _logger.LogWarning("TCMB XML'inde kur bilgileri bulunamadı.");
                    return kurlar;
                }

                foreach (XmlNode node in kurNodes)
                {
                    var kod = node.Attributes["Kod"]?.Value;
                    if (string.IsNullOrEmpty(kod)) continue;

                    var birim = int.Parse(node.SelectSingleNode("Unit")?.InnerText ?? "1");
                    var isim = node.SelectSingleNode("Isim")?.InnerText;
                    var forexBuyingStr = node.SelectSingleNode("ForexBuying")?.InnerText;
                    var forexSellingStr = node.SelectSingleNode("ForexSelling")?.InnerText;

                    if (!decimal.TryParse(forexBuyingStr, NumberStyles.Any, CultureInfo.InvariantCulture, out var forexBuying) ||
                        !decimal.TryParse(forexSellingStr, NumberStyles.Any, CultureInfo.InvariantCulture, out var forexSelling))
                    {
                        continue;
                    }

                    // Veritabanında bu para birimi var mı kontrol et
                    var paraBirimi = await _context.ParaBirimleri.FirstOrDefaultAsync(p => p.Kod == kod);
                    if (paraBirimi == null)
                    {
                        // Para birimi yoksa ekle
                        paraBirimi = new ParaBirimi
                        {
                            Kod = kod,
                            Ad = isim,
                            Sembol = kod,
                            Aktif = true
                        };
                        await _context.ParaBirimleri.AddAsync(paraBirimi);
                        await _context.SaveChangesAsync();
                    }

                    // TRY para birimi var mı kontrol et
                    var tryParaBirimi = await _context.ParaBirimleri.FirstOrDefaultAsync(p => p.Kod == "TRY");
                    if (tryParaBirimi == null)
                    {
                        // TRY yoksa ekle
                        tryParaBirimi = new ParaBirimi
                        {
                            Kod = "TRY",
                            Ad = "Türk Lirası",
                            Sembol = "₺",
                            Aktif = true
                        };
                        await _context.ParaBirimleri.AddAsync(tryParaBirimi);
                        await _context.SaveChangesAsync();
                    }

                    // Para birimleri arasındaki ilişkiyi kontrol et ve gerekirse ekle
                    var dovizIliski = await _context.DovizIliskileri
                        .FirstOrDefaultAsync(di => 
                            di.KaynakParaBirimiID == paraBirimi.ParaBirimiID && 
                            di.HedefParaBirimiID == tryParaBirimi.ParaBirimiID);
                    
                    if (dovizIliski == null)
                    {
                        dovizIliski = new DovizIliski
                        {
                            KaynakParaBirimiID = paraBirimi.ParaBirimiID,
                            HedefParaBirimiID = tryParaBirimi.ParaBirimiID,
                            Aktif = true
                        };
                        await _context.DovizIliskileri.AddAsync(dovizIliski);
                        await _context.SaveChangesAsync();
                    }

                    // Kur değeri ekle
                    var kurDegeri = new KurDegeri
                    {
                        ParaBirimiID = paraBirimi.ParaBirimiID,
                        AlisDegeri = forexBuying / birim,
                        SatisDegeri = forexSelling / birim,
                        Tarih = tarih,
                        Kaynak = "TCMB",
                        Aktif = true
                    };

                    kurlar.Add(kurDegeri);
                }

                return kurlar;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "TCMB'den kur bilgileri alınırken hata oluştu: {Message}", ex.Message);
                return new List<KurDegeri>();
            }
        }

        public async Task<decimal> ParaBirimiCevirAsync(string kaynakParaBirimi, string hedefParaBirimi, decimal miktar)
        {
            try
            {
                if (kaynakParaBirimi == hedefParaBirimi)
                    return miktar;

                // Para birimlerini bul
                var kaynakPB = await _context.ParaBirimleri.FirstOrDefaultAsync(p => p.Kod == kaynakParaBirimi);
                var hedefPB = await _context.ParaBirimleri.FirstOrDefaultAsync(p => p.Kod == hedefParaBirimi);

                if (kaynakPB == null || hedefPB == null)
                {
                    _logger.LogWarning("Para birimi bulunamadı: Kaynak={Kaynak}, Hedef={Hedef}", kaynakParaBirimi, hedefParaBirimi);
                    return 0;
                }

                // İlişki var mı kontrol et
                var iliskiDogrudan = await _context.DovizIliskileri
                    .FirstOrDefaultAsync(di => 
                        di.KaynakParaBirimiID == kaynakPB.ParaBirimiID && 
                        di.HedefParaBirimiID == hedefPB.ParaBirimiID && 
                        di.Aktif);

                if (iliskiDogrudan != null)
                {
                    // Bu ilişki için en güncel kur değerini bul
                    var kurDogrudan = await _context.KurDegerleri
                        .Where(k => k.ParaBirimiID == kaynakPB.ParaBirimiID && k.Aktif)
                        .OrderByDescending(k => k.Tarih)
                        .FirstOrDefaultAsync();

                    if (kurDogrudan != null)
                    {
                        return miktar * ((kurDogrudan.AlisDegeri + kurDogrudan.SatisDegeri) / 2);
                    }
                }

                // Ters ilişki var mı kontrol et
                var iliskiTers = await _context.DovizIliskileri
                    .FirstOrDefaultAsync(di => 
                        di.KaynakParaBirimiID == hedefPB.ParaBirimiID && 
                        di.HedefParaBirimiID == kaynakPB.ParaBirimiID && 
                        di.Aktif);

                if (iliskiTers != null)
                {
                    // Bu ilişki için en güncel kur değerini bul
                    var kurTers = await _context.KurDegerleri
                        .Where(k => k.ParaBirimiID == hedefPB.ParaBirimiID && k.Aktif)
                        .OrderByDescending(k => k.Tarih)
                        .FirstOrDefaultAsync();

                    if (kurTers != null)
                    {
                        return miktar / ((kurTers.AlisDegeri + kurTers.SatisDegeri) / 2);
                    }
                }

                // Çapraz kur hesapla (USD üzerinden)
                var usdParaBirimi = await _context.ParaBirimleri.FirstOrDefaultAsync(p => p.Kod == "USD");
                if (usdParaBirimi == null)
                {
                    _logger.LogWarning("USD para birimi bulunamadı");
                    return 0;
                }

                // Kaynak para biriminden USD'ye çevir
                var kaynakUsdKur = await _context.KurDegerleri
                    .Where(k => k.ParaBirimiID == kaynakPB.ParaBirimiID && k.Aktif)
                    .OrderByDescending(k => k.Tarih)
                    .FirstOrDefaultAsync();

                // USD'den hedef para birimine çevir
                var hedefUsdKur = await _context.KurDegerleri
                    .Where(k => k.ParaBirimiID == hedefPB.ParaBirimiID && k.Aktif)
                    .OrderByDescending(k => k.Tarih)
                    .FirstOrDefaultAsync();

                if (kaynakUsdKur != null && hedefUsdKur != null)
                {
                    var kaynakOrtalama = (kaynakUsdKur.AlisDegeri + kaynakUsdKur.SatisDegeri) / 2;
                    var hedefOrtalama = (hedefUsdKur.AlisDegeri + hedefUsdKur.SatisDegeri) / 2;
                    return miktar * (kaynakOrtalama / hedefOrtalama);
                }

                _logger.LogWarning("Kur değeri bulunamadı");
                return 0;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Para birimi çevirme işlemi sırasında hata oluştu: {Message}", ex.Message);
                return 0;
            }
        }

        public async Task<string> GetAnaDovizKoduAsync()
        {
            try
            {
                var sistemAyarlari = await _sistemAyariRepository.GetFirstOrDefaultAsync();
                return sistemAyarlari?.AnaDovizKodu ?? "USD";
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ana döviz kodu getirilirken hata oluştu: {Message}", ex.Message);
                return "USD";
            }
        }

        public async Task<bool> SetAnaDovizKoduAsync(string dovizKodu)
        {
            try
            {
                var sistemAyarlari = await _sistemAyariRepository.GetFirstOrDefaultAsync();
                if (sistemAyarlari == null)
                {
                    sistemAyarlari = new SistemAyarlari
                    {
                        AnaDovizKodu = dovizKodu,
                        OlusturmaTarihi = DateTime.Now
                    };
                    await _sistemAyariRepository.AddAsync(sistemAyarlari);
                }
                else
                {
                    sistemAyarlari.AnaDovizKodu = dovizKodu;
                    sistemAyarlari.GuncellemeTarihi = DateTime.Now;
                    await _sistemAyariRepository.UpdateAsync(sistemAyarlari);
                }
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ana döviz kodu ayarlanırken hata oluştu: {Message}", ex.Message);
                return false;
            }
        }

        #region Döviz İşlemleri
        
        public async Task<List<ParaBirimiViewModel>> GetAllDovizlerAsync()
        {
            try
            {
                var dovizler = await _paraBirimiRepository.GetAllAsync();
                return dovizler.Select(ParaBirimiViewModel.FromEntity).ToList();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Tüm dövizler getirilirken hata oluştu: {Message}", ex.Message);
                return new List<ParaBirimiViewModel>();
            }
        }
        
        public async Task<List<ParaBirimiViewModel>> GetActiveDovizsAsync()
        {
            try
            {
                var dovizler = await _paraBirimiRepository.GetAllAsync(d => d.Aktif && !d.SoftDelete);
                return dovizler.Select(ParaBirimiViewModel.FromEntity).ToList();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Aktif dövizler getirilirken hata oluştu: {Message}", ex.Message);
                return new List<ParaBirimiViewModel>();
            }
        }
        
        public async Task<ParaBirimiViewModel> GetDovizByIdAsync(Guid id)
        {
            try
            {
                var doviz = await _paraBirimiRepository.GetByIdAsync(id);
                return doviz != null ? ParaBirimiViewModel.FromEntity(doviz) : null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "ID ile döviz getirilirken hata oluştu: {Message}", ex.Message);
                return null;
            }
        }
        
        public async Task<ParaBirimi> GetDovizEntityByIdAsync(Guid id)
        {
            return await _paraBirimiRepository.GetByIdAsync(id);
        }
        
        public async Task<ParaBirimi> GetDovizByKodAsync(string kod)
        {
            return await _paraBirimiRepository.GetFirstOrDefaultAsync(d => d.Kod == kod && !d.SoftDelete);
        }
        
        public async Task<ParaBirimiViewModel> AddDovizAsync(ParaBirimiViewModel model)
        {
            try
            {
                // Kod kontrolü yap
                var existing = await _paraBirimiRepository.GetFirstOrDefaultAsync(d => d.Kod == model.Kod && !d.SoftDelete);
                if (existing != null)
                {
                    throw new Exception($"{model.Kod} kodlu para birimi zaten mevcut.");
                }

                var entity = model.ToEntity();
                entity.ParaBirimiID = Guid.NewGuid();
                entity.OlusturmaTarihi = DateTime.Now;
                entity.GuncellemeTarihi = DateTime.Now;

                await _paraBirimiRepository.AddAsync(entity);
                await _logService.LogEkleAsync($"{model.Kod} kodlu para birimi eklendi.", Models.LogTuru.Bilgi);

                return ParaBirimiViewModel.FromEntity(entity);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Para birimi eklenirken hata oluştu: {Message}", ex.Message);
                throw;
            }
        }
        
        public async Task<ParaBirimiViewModel> UpdateDovizAsync(ParaBirimiViewModel model)
        {
            try
            {
                var entity = await _paraBirimiRepository.GetByIdAsync(model.ParaBirimiID);
                if (entity == null)
                {
                    throw new Exception($"Güncellenecek para birimi bulunamadı. ID: {model.ParaBirimiID}");
                }

                // Aynı kod başka bir para biriminde kullanılıyor mu kontrol et
                var duplicate = await _paraBirimiRepository.GetFirstOrDefaultAsync(
                    d => d.Kod == model.Kod && d.ParaBirimiID != model.ParaBirimiID && !d.SoftDelete);

                if (duplicate != null)
                {
                    throw new Exception($"{model.Kod} kodlu başka bir para birimi zaten mevcut.");
                }

                entity.Kod = model.Kod;
                entity.Ad = model.Ad;
                entity.Sembol = model.Sembol;
                entity.Aktif = model.Aktif;
                entity.GuncellemeTarihi = DateTime.Now;

                await _paraBirimiRepository.UpdateAsync(entity);
                await _logService.LogEkleAsync($"{model.Kod} kodlu para birimi güncellendi.", Models.LogTuru.Bilgi);

                return ParaBirimiViewModel.FromEntity(entity);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Para birimi güncellenirken hata oluştu: {Message}", ex.Message);
                throw;
            }
        }
        
        public async Task DeleteDovizAsync(Guid id)
        {
            try
            {
                var entity = await _paraBirimiRepository.GetByIdAsync(id);
                if (entity == null)
                {
                    throw new Exception($"Silinecek para birimi bulunamadı. ID: {id}");
                }

                // İlişkiler kontrol edilmeli
                var kurDegerleri = await _kurDegeriRepository.GetAllAsync(k => k.ParaBirimiID == id && !k.SoftDelete);
                var dovizIliskileri = await _dovizIliskiRepository.GetAllAsync(
                    di => (di.KaynakParaBirimiID == id || di.HedefParaBirimiID == id) && !di.SoftDelete);

                if (kurDegerleri.Any())
                {
                    throw new Exception($"Bu para birimine ait kur değerleri bulunmaktadır. Önce ilişkili kur değerlerini siliniz.");
                }

                if (dovizIliskileri.Any())
                {
                    throw new Exception($"Bu para birimine ait döviz ilişkileri bulunmaktadır. Önce ilişkili döviz ilişkilerini siliniz.");
                }

                entity.SoftDelete = true;
                entity.GuncellemeTarihi = DateTime.Now;

                await _paraBirimiRepository.UpdateAsync(entity);
                await _logService.LogEkleAsync($"{entity.Kod} kodlu para birimi silindi.", Models.LogTuru.Bilgi);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Para birimi silinirken hata oluştu: {Message}", ex.Message);
                throw;
            }
        }
        
        #endregion
        
        #region Döviz İlişki İşlemleri
        
        public async Task<List<DovizIliskiViewModel>> GetAllDovizIliskileriAsync()
        {
            try
            {
                var iliskiler = await _dovizIliskiRepository.GetAllAsync();
                return iliskiler.Select(DovizIliskiViewModel.FromEntity).ToList();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Tüm döviz ilişkileri getirilirken hata oluştu: {Message}", ex.Message);
                return new List<DovizIliskiViewModel>();
            }
        }
        
        public async Task<List<DovizIliskiViewModel>> GetActiveDovizIliskileriAsync()
        {
            try
            {
                var iliskiler = await _dovizIliskiRepository.GetAllAsync(di => di.Aktif && !di.SoftDelete);
                return iliskiler.Select(DovizIliskiViewModel.FromEntity).ToList();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Aktif döviz ilişkileri getirilirken hata oluştu: {Message}", ex.Message);
                return new List<DovizIliskiViewModel>();
            }
        }
        
        public async Task<DovizIliskiViewModel> GetDovizIliskiByIdAsync(Guid id)
        {
            try
            {
                var iliski = await _dovizIliskiRepository.GetByIdAsync(id);
                return iliski != null ? DovizIliskiViewModel.FromEntity(iliski) : null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "ID ile döviz ilişkisi getirilirken hata oluştu: {Message}", ex.Message);
                return null;
            }
        }
        
        public async Task<DovizIliski> GetDovizIliskiEntityByIdAsync(Guid id)
        {
            return await _dovizIliskiRepository.GetByIdAsync(id);
        }
        
        public async Task<DovizIliski> GetDovizIliskiByParaBirimleriAsync(Guid kaynakId, Guid hedefId)
        {
            return await _dovizIliskiRepository.GetFirstOrDefaultAsync(
                di => di.KaynakParaBirimiID == kaynakId && di.HedefParaBirimiID == hedefId && !di.SoftDelete);
        }
        
        public async Task<DovizIliskiViewModel> AddDovizIliskiAsync(DovizIliskiViewModel model)
        {
            try
            {
                // Kaynak ve hedef para birimleri var mı kontrol et
                var kaynakParaBirimi = await _paraBirimiRepository.GetByIdAsync(model.KaynakParaBirimiID);
                var hedefParaBirimi = await _paraBirimiRepository.GetByIdAsync(model.HedefParaBirimiID);

                if (kaynakParaBirimi == null || hedefParaBirimi == null)
                {
                    throw new Exception("Geçerli para birimleri seçilmelidir.");
                }

                // Aynı ilişki var mı kontrol et
                var existingIliski = await _dovizIliskiRepository.GetFirstOrDefaultAsync(
                    di => di.KaynakParaBirimiID == model.KaynakParaBirimiID && 
                          di.HedefParaBirimiID == model.HedefParaBirimiID && 
                          !di.SoftDelete);

                if (existingIliski != null)
                {
                    throw new Exception($"Bu para birimleri arasında zaten bir ilişki tanımlanmış.");
                }

                var entity = model.ToEntity();
                entity.DovizIliskiID = Guid.NewGuid();
                entity.OlusturmaTarihi = DateTime.Now;
                entity.GuncellemeTarihi = DateTime.Now;

                await _dovizIliskiRepository.AddAsync(entity);
                await _logService.LogEkleAsync(
                    $"{kaynakParaBirimi.Kod} -> {hedefParaBirimi.Kod} para birimi ilişkisi eklendi.", 
                    Models.LogTuru.Bilgi);

                return DovizIliskiViewModel.FromEntity(entity);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Döviz ilişkisi eklenirken hata oluştu: {Message}", ex.Message);
                throw;
            }
        }
        
        public async Task<DovizIliskiViewModel> UpdateDovizIliskiAsync(DovizIliskiViewModel model)
        {
            try
            {
                var entity = await _dovizIliskiRepository.GetByIdAsync(model.DovizIliskiID);
                if (entity == null)
                {
                    throw new Exception($"Güncellenecek döviz ilişkisi bulunamadı. ID: {model.DovizIliskiID}");
                }

                // Kaynak ve hedef para birimleri var mı kontrol et
                var kaynakParaBirimi = await _paraBirimiRepository.GetByIdAsync(model.KaynakParaBirimiID);
                var hedefParaBirimi = await _paraBirimiRepository.GetByIdAsync(model.HedefParaBirimiID);

                if (kaynakParaBirimi == null || hedefParaBirimi == null)
                {
                    throw new Exception("Geçerli para birimleri seçilmelidir.");
                }

                // Aynı ilişki başka bir kayıtta var mı kontrol et
                var existingIliski = await _dovizIliskiRepository.GetFirstOrDefaultAsync(
                    di => di.DovizIliskiID != model.DovizIliskiID && 
                          di.KaynakParaBirimiID == model.KaynakParaBirimiID && 
                          di.HedefParaBirimiID == model.HedefParaBirimiID && 
                          !di.SoftDelete);

                if (existingIliski != null)
                {
                    throw new Exception($"Bu para birimleri arasında zaten başka bir ilişki tanımlanmış.");
                }

                entity.KaynakParaBirimiID = model.KaynakParaBirimiID;
                entity.HedefParaBirimiID = model.HedefParaBirimiID;
                entity.Aktif = model.Aktif;
                entity.GuncellemeTarihi = DateTime.Now;

                await _dovizIliskiRepository.UpdateAsync(entity);
                await _logService.LogEkleAsync(
                    $"{kaynakParaBirimi.Kod} -> {hedefParaBirimi.Kod} para birimi ilişkisi güncellendi.", 
                    Models.LogTuru.Bilgi);

                return DovizIliskiViewModel.FromEntity(entity);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Döviz ilişkisi güncellenirken hata oluştu: {Message}", ex.Message);
                throw;
            }
        }
        
        public async Task DeleteDovizIliskiAsync(Guid id)
        {
            try
            {
                var entity = await _dovizIliskiRepository.GetByIdAsync(id);
                if (entity == null)
                {
                    throw new Exception($"Silinecek döviz ilişkisi bulunamadı. ID: {id}");
                }

                // İlişkiyi kullanan kur değerleri var mı kontrol et
                var kurDegerleri = await _kurDegeriRepository.GetAllAsync(
                    k => (k.ParaBirimiID == entity.KaynakParaBirimiID || k.ParaBirimiID == entity.HedefParaBirimiID) && 
                         !k.SoftDelete);

                if (kurDegerleri.Any())
                {
                    throw new Exception($"Bu ilişkiye ait kur değerleri bulunmaktadır. Önce ilişkili kur değerlerini siliniz.");
                }

                entity.SoftDelete = true;
                entity.GuncellemeTarihi = DateTime.Now;

                await _dovizIliskiRepository.UpdateAsync(entity);

                // İlişkili para birimlerini al
                var kaynakParaBirimi = await _paraBirimiRepository.GetByIdAsync(entity.KaynakParaBirimiID);
                var hedefParaBirimi = await _paraBirimiRepository.GetByIdAsync(entity.HedefParaBirimiID);

                await _logService.LogEkleAsync(
                    $"{kaynakParaBirimi?.Kod} -> {hedefParaBirimi?.Kod} para birimi ilişkisi silindi.", 
                    Models.LogTuru.Bilgi);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Döviz ilişkisi silinirken hata oluştu: {Message}", ex.Message);
                throw;
            }
        }
        
        #endregion
    }
}