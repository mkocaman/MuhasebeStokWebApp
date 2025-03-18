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
        private readonly IRepository<Doviz> _dovizRepository;

        public DovizService(
            ApplicationDbContext context,
            ILogger<DovizService> logger,
            HttpClient httpClient,
            IRepository<KurDegeri> kurDegeriRepository,
            IRepository<SistemAyarlari> sistemAyariRepository,
            IRepository<DovizIliski> dovizIliskiRepository,
            ILogService logService,
            IRepository<Doviz> dovizRepository)
        {
            _context = context;
            _logger = logger;
            _httpClient = httpClient;
            _kurDegeriRepository = kurDegeriRepository;
            _sistemAyariRepository = sistemAyariRepository;
            _dovizIliskiRepository = dovizIliskiRepository;
            _logService = logService;
            _dovizRepository = dovizRepository;
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
                var sistemAyari = await _sistemAyariRepository.GetFirstAsync();
                return sistemAyari?.AnaDovizKodu ?? "USD";
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ana döviz kodu alınırken hata oluştu: {Message}", ex.Message);
                return "USD";
            }
        }

        public async Task<bool> SetAnaDovizKoduAsync(string dovizKodu)
        {
            try
            {
                var sistemAyari = await _sistemAyariRepository.GetFirstAsync();
                if (sistemAyari == null)
                {
                    sistemAyari = new SistemAyarlari
                    {
                        AnaDovizKodu = dovizKodu
                    };
                    await _sistemAyariRepository.AddAsync(sistemAyari);
                }
                else
                {
                    sistemAyari.AnaDovizKodu = dovizKodu;
                    await _sistemAyariRepository.UpdateAsync(sistemAyari);
                }

                await _logService.LogEkleAsync($"Ana döviz kodu {dovizKodu} olarak güncellendi.", Models.LogTuru.Bilgi);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ana döviz kodu güncellenirken hata oluştu: {Message}", ex.Message);
                await _logService.LogEkleAsync($"Ana döviz kodu güncellenirken hata oluştu: {ex.Message}", Models.LogTuru.Hata);
                return false;
            }
        }

        #region Döviz İşlemleri
        
        public async Task<List<DovizViewModel>> GetAllDovizlerAsync()
        {
            var dovizler = await _context.Dovizler
                .Where(d => !d.SoftDelete)
                .OrderBy(d => d.DovizKodu)
                .ToListAsync();
                
            return dovizler.Select(d => DovizViewModel.FromEntity(d)).ToList();
        }
        
        public async Task<List<DovizViewModel>> GetActiveDovizsAsync()
        {
            var dovizler = await _context.Dovizler
                .Where(d => d.Aktif && !d.SoftDelete)
                .OrderBy(d => d.DovizKodu)
                .ToListAsync();
                
            return dovizler.Select(d => DovizViewModel.FromEntity(d)).ToList();
        }
        
        public async Task<DovizViewModel> GetDovizByIdAsync(int id)
        {
            var doviz = await _context.Dovizler.FindAsync(id);
            if (doviz == null || doviz.SoftDelete)
            {
                return null;
            }
            
            return DovizViewModel.FromEntity(doviz);
        }
        
        public async Task<Doviz> GetDovizEntityByIdAsync(int id)
        {
            return await _context.Dovizler.FindAsync(id);
        }
        
        public async Task<Doviz> GetDovizByKodAsync(string kod)
        {
            return await _context.Dovizler
                .Where(d => d.DovizKodu == kod && !d.SoftDelete)
                .FirstOrDefaultAsync();
        }
        
        public async Task<DovizViewModel> AddDovizAsync(DovizViewModel model)
        {
            // Döviz kodu kontrolü
            var existingDoviz = await _context.Dovizler
                .FirstOrDefaultAsync(d => d.DovizKodu == model.DovizKodu && !d.SoftDelete);
                
            if (existingDoviz != null)
            {
                throw new Exception($"{model.DovizKodu} kodlu döviz zaten mevcut.");
            }
            
            var entity = model.ToEntity();
            entity.OlusturmaTarihi = DateTime.Now;
            
            await _dovizRepository.AddAsync(entity);
            
            model.DovizID = entity.DovizID;
            return model;
        }
        
        public async Task<DovizViewModel> UpdateDovizAsync(DovizViewModel model)
        {
            var entity = await _context.Dovizler.FindAsync(model.DovizID);
            if (entity == null || entity.SoftDelete)
            {
                throw new Exception("Döviz bulunamadı.");
            }
            
            // Döviz kodu değiştirilmişse, kod kontrolü yap
            if (entity.DovizKodu != model.DovizKodu)
            {
                var existingDoviz = await _context.Dovizler
                    .FirstOrDefaultAsync(d => d.DovizKodu == model.DovizKodu && d.DovizID != model.DovizID && !d.SoftDelete);
                    
                if (existingDoviz != null)
                {
                    throw new Exception($"{model.DovizKodu} kodlu döviz zaten mevcut.");
                }
            }
            
            entity.DovizKodu = model.DovizKodu;
            entity.DovizAdi = model.DovizAdi;
            entity.Sembol = model.Sembol;
            entity.Aktif = model.Aktif;
            entity.GuncellemeTarihi = DateTime.Now;
            
            await _dovizRepository.UpdateAsync(entity);
            
            return model;
        }
        
        public async Task DeleteDovizAsync(int id)
        {
            var entity = await _context.Dovizler.FindAsync(id);
            if (entity == null)
            {
                throw new Exception("Döviz bulunamadı.");
            }
            
            // İlişkili kullanım kontrolü
            var iliskiKayitlari = await _context.DovizIliskileri
                .Where(di => di.KaynakParaBirimiID == id || di.HedefParaBirimiID == id)
                .AnyAsync();
                
            if (iliskiKayitlari)
            {
                throw new Exception("Bu döviz, döviz ilişkilerinde kullanılmaktadır. Önce ilişkili kayıtları silmelisiniz.");
            }
            
            // Soft delete
            entity.SoftDelete = true;
            entity.Aktif = false;
            entity.GuncellemeTarihi = DateTime.Now;
            
            await _dovizRepository.UpdateAsync(entity);
        }
        
        #endregion
        
        #region Döviz İlişki İşlemleri
        
        public async Task<List<DovizIliskiViewModel>> GetAllDovizIliskileriAsync()
        {
            var iliskiler = await _context.DovizIliskileri
                .Include(di => di.KaynakParaBirimi)
                .Include(di => di.HedefParaBirimi)
                .Where(di => !di.SoftDelete)
                .OrderBy(di => di.KaynakParaBirimi.DovizKodu)
                .ThenBy(di => di.HedefParaBirimi.DovizKodu)
                .ToListAsync();
                
            return iliskiler.Select(di => DovizIliskiViewModel.FromEntity(di)).ToList();
        }
        
        public async Task<List<DovizIliskiViewModel>> GetActiveDovizIliskileriAsync()
        {
            var iliskiler = await _context.DovizIliskileri
                .Include(di => di.KaynakParaBirimi)
                .Include(di => di.HedefParaBirimi)
                .Where(di => di.Aktif && !di.SoftDelete)
                .OrderBy(di => di.KaynakParaBirimi.DovizKodu)
                .ThenBy(di => di.HedefParaBirimi.DovizKodu)
                .ToListAsync();
                
            return iliskiler.Select(di => DovizIliskiViewModel.FromEntity(di)).ToList();
        }
        
        public async Task<DovizIliskiViewModel> GetDovizIliskiByIdAsync(int id)
        {
            var iliski = await _context.DovizIliskileri
                .Include(di => di.KaynakParaBirimi)
                .Include(di => di.HedefParaBirimi)
                .FirstOrDefaultAsync(di => di.DovizIliskiID == id && !di.SoftDelete);
                
            if (iliski == null)
            {
                return null;
            }
            
            return DovizIliskiViewModel.FromEntity(iliski);
        }
        
        public async Task<DovizIliski> GetDovizIliskiEntityByIdAsync(int id)
        {
            return await _context.DovizIliskileri
                .Include(di => di.KaynakParaBirimi)
                .Include(di => di.HedefParaBirimi)
                .FirstOrDefaultAsync(di => di.DovizIliskiID == id);
        }
        
        public async Task<DovizIliski> GetDovizIliskiByParaBirimleriAsync(int kaynakId, int hedefId)
        {
            return await _context.DovizIliskileri
                .Include(di => di.KaynakParaBirimi)
                .Include(di => di.HedefParaBirimi)
                .FirstOrDefaultAsync(di => 
                    di.KaynakParaBirimiID == kaynakId && 
                    di.HedefParaBirimiID == hedefId && 
                    !di.SoftDelete);
        }
        
        public async Task<DovizIliskiViewModel> AddDovizIliskiAsync(DovizIliskiViewModel model)
        {
            // Para birimleri kontrolü
            if (model.KaynakParaBirimiID == model.HedefParaBirimiID)
            {
                throw new Exception("Kaynak ve hedef para birimi aynı olamaz.");
            }
            
            // Para birimleri gerçekten var mı?
            var kaynakParaBirimi = await _context.Dovizler.FindAsync(model.KaynakParaBirimiID);
            var hedefParaBirimi = await _context.Dovizler.FindAsync(model.HedefParaBirimiID);
            
            if (kaynakParaBirimi == null || kaynakParaBirimi.SoftDelete)
            {
                throw new Exception("Kaynak para birimi bulunamadı.");
            }
            
            if (hedefParaBirimi == null || hedefParaBirimi.SoftDelete)
            {
                throw new Exception("Hedef para birimi bulunamadı.");
            }
            
            // Aynı ilişki var mı?
            var existingIliski = await _context.DovizIliskileri
                .FirstOrDefaultAsync(di => 
                    di.KaynakParaBirimiID == model.KaynakParaBirimiID && 
                    di.HedefParaBirimiID == model.HedefParaBirimiID && 
                    !di.SoftDelete);
                    
            if (existingIliski != null)
            {
                throw new Exception($"{kaynakParaBirimi.DovizKodu}-{hedefParaBirimi.DovizKodu} ilişkisi zaten tanımlı.");
            }
            
            var entity = model.ToEntity();
            entity.OlusturmaTarihi = DateTime.Now;
            
            await _dovizIliskiRepository.AddAsync(entity);
            
            model.DovizIliskiID = entity.DovizIliskiID;
            model.KaynakParaBirimiKodu = kaynakParaBirimi.DovizKodu;
            model.HedefParaBirimiKodu = hedefParaBirimi.DovizKodu;
            
            return model;
        }
        
        public async Task<DovizIliskiViewModel> UpdateDovizIliskiAsync(DovizIliskiViewModel model)
        {
            var entity = await _context.DovizIliskileri.FindAsync(model.DovizIliskiID);
            if (entity == null || entity.SoftDelete)
            {
                throw new Exception("Döviz ilişkisi bulunamadı.");
            }
            
            // Para birimleri kontrolü
            if (model.KaynakParaBirimiID == model.HedefParaBirimiID)
            {
                throw new Exception("Kaynak ve hedef para birimi aynı olamaz.");
            }
            
            // Para birimleri gerçekten var mı?
            var kaynakParaBirimi = await _context.Dovizler.FindAsync(model.KaynakParaBirimiID);
            var hedefParaBirimi = await _context.Dovizler.FindAsync(model.HedefParaBirimiID);
            
            if (kaynakParaBirimi == null || kaynakParaBirimi.SoftDelete)
            {
                throw new Exception("Kaynak para birimi bulunamadı.");
            }
            
            if (hedefParaBirimi == null || hedefParaBirimi.SoftDelete)
            {
                throw new Exception("Hedef para birimi bulunamadı.");
            }
            
            // Kaynak veya hedef değişmişse, ilişki kontrolü yap
            if (entity.KaynakParaBirimiID != model.KaynakParaBirimiID || entity.HedefParaBirimiID != model.HedefParaBirimiID)
            {
                var existingIliski = await _context.DovizIliskileri
                    .FirstOrDefaultAsync(di => 
                        di.KaynakParaBirimiID == model.KaynakParaBirimiID && 
                        di.HedefParaBirimiID == model.HedefParaBirimiID && 
                        di.DovizIliskiID != model.DovizIliskiID && 
                        !di.SoftDelete);
                        
                if (existingIliski != null)
                {
                    throw new Exception($"{kaynakParaBirimi.DovizKodu}-{hedefParaBirimi.DovizKodu} ilişkisi zaten tanımlı.");
                }
            }
            
            entity.KaynakParaBirimiID = model.KaynakParaBirimiID;
            entity.HedefParaBirimiID = model.HedefParaBirimiID;
            entity.Aktif = model.Aktif;
            entity.GuncellemeTarihi = DateTime.Now;
            
            await _dovizIliskiRepository.UpdateAsync(entity);
            
            model.KaynakParaBirimiKodu = kaynakParaBirimi.DovizKodu;
            model.HedefParaBirimiKodu = hedefParaBirimi.DovizKodu;
            
            return model;
        }
        
        public async Task DeleteDovizIliskiAsync(int id)
        {
            var entity = await _context.DovizIliskileri.FindAsync(id);
            if (entity == null)
            {
                throw new Exception("Döviz ilişkisi bulunamadı.");
            }
            
            // İlişkili kur değerleri kontrolü
            var kurDegerleri = await _context.KurDegerleri
                .Where(kd => kd.DovizIliskiID == id && !kd.SoftDelete)
                .AnyAsync();
                
            if (kurDegerleri)
            {
                throw new Exception("Bu döviz ilişkisine ait kur değerleri bulunmaktadır. Önce kur değerlerini silmelisiniz.");
            }
            
            // Soft delete
            entity.SoftDelete = true;
            entity.Aktif = false;
            entity.GuncellemeTarihi = DateTime.Now;
            
            await _dovizIliskiRepository.UpdateAsync(entity);
        }
        
        #endregion
    }
}