using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using System.Xml;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using MuhasebeStokWebApp.Data;
using MuhasebeStokWebApp.Data.Entities;
using MuhasebeStokWebApp.Data.Repositories;
using MuhasebeStokWebApp.ViewModels.Kur;

namespace MuhasebeStokWebApp.Services
{
    public class KurService : IKurService
    {
        private readonly ApplicationDbContext _context;
        private readonly IRepository<ParaBirimi> _paraBirimiRepository;
        private readonly IRepository<KurDegeri> _kurDegeriRepository;
        private readonly IRepository<DovizIliski> _dovizIliskiRepository;
        private readonly IRepository<SistemAyarlari> _sistemAyarlariRepository;
        private readonly ILogger<KurService> _logger;
        private readonly HttpClient _httpClient;

        public KurService(
            ApplicationDbContext context,
            IRepository<ParaBirimi> paraBirimiRepository,
            IRepository<KurDegeri> kurDegeriRepository,
            IRepository<DovizIliski> dovizIliskiRepository,
            IRepository<SistemAyarlari> sistemAyarlariRepository,
            ILogger<KurService> logger,
            IHttpClientFactory httpClientFactory)
        {
            _context = context;
            _paraBirimiRepository = paraBirimiRepository;
            _kurDegeriRepository = kurDegeriRepository;
            _dovizIliskiRepository = dovizIliskiRepository;
            _sistemAyarlariRepository = sistemAyarlariRepository;
            _logger = logger;
            _httpClient = httpClientFactory.CreateClient("KurAPI");
        }

        #region Para Birimi İşlemleri
        public async Task<List<ParaBirimi>> GetParaBirimleriAsync()
        {
            try
            {
                return await _paraBirimiRepository.GetAllAsync(pb => !pb.SoftDelete, q => q.OrderBy(pb => pb.Kod));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Para birimleri getirilirken hata oluştu");
                throw;
            }
        }

        public async Task<ParaBirimi> GetParaBirimiByIdAsync(Guid id)
        {
            try
            {
                return await _paraBirimiRepository.GetByIdAsync(id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "ID ile para birimi getirilirken hata oluştu. ID: {Id}", id);
                throw;
            }
        }

        public async Task<ParaBirimi> GetParaBirimiByKodAsync(string kod)
        {
            try
            {
                return await _paraBirimiRepository.GetFirstOrDefaultAsync(pb => pb.Kod == kod && !pb.SoftDelete);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Kod ile para birimi getirilirken hata oluştu. Kod: {Kod}", kod);
                throw;
            }
        }

        public async Task<ParaBirimi> AddParaBirimiAsync(ParaBirimi paraBirimi)
        {
            try
            {
                // ID olmayan para birimleri için yeni GUID oluştur
                if (paraBirimi.ParaBirimiID == Guid.Empty)
                {
                    paraBirimi.ParaBirimiID = Guid.NewGuid();
                }

                // Oluşturma tarihi ekle
                paraBirimi.OlusturmaTarihi = DateTime.Now;
                paraBirimi.GuncellemeTarihi = DateTime.Now;

                await _paraBirimiRepository.AddAsync(paraBirimi);
                return paraBirimi;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Para birimi eklenirken hata oluştu. Para Birimi: {Kod}", paraBirimi.Kod);
                throw;
            }
        }

        public async Task<ParaBirimi> UpdateParaBirimiAsync(ParaBirimi paraBirimi)
        {
            try
            {
                // Mevcut para birimini al
                var mevcut = await _paraBirimiRepository.GetByIdAsync(paraBirimi.ParaBirimiID);
                if (mevcut == null)
                {
                    throw new Exception($"Güncellenecek para birimi bulunamadı. ID: {paraBirimi.ParaBirimiID}");
                }

                // Güncelleme tarihi ekle
                paraBirimi.GuncellemeTarihi = DateTime.Now;
                paraBirimi.OlusturmaTarihi = mevcut.OlusturmaTarihi; // Oluşturma tarihini koruyoruz

                await _paraBirimiRepository.UpdateAsync(paraBirimi);
                return paraBirimi;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Para birimi güncellenirken hata oluştu. ID: {Id}", paraBirimi.ParaBirimiID);
                throw;
            }
        }

        public async Task DeleteParaBirimiAsync(Guid id)
        {
            try
            {
                // İlgili para birimini al
                var paraBirimi = await _paraBirimiRepository.GetByIdAsync(id);
                if (paraBirimi == null)
                {
                    throw new Exception($"Silinecek para birimi bulunamadı. ID: {id}");
                }

                // Bu para birimiyle ilişkili kur değerleri ve ilişkiler var mı?
                var kurDegerleri = await _kurDegeriRepository.GetAllAsync(kd => kd.ParaBirimiID == id && !kd.SoftDelete);
                var dovizIliskileri = await _dovizIliskiRepository.GetAllAsync(di => 
                    (di.KaynakParaBirimiID == id || di.HedefParaBirimiID == id) && !di.SoftDelete);

                // İlişkili kayıtları soft delete yap
                foreach (var kurDegeri in kurDegerleri)
                {
                    kurDegeri.SoftDelete = true;
                    kurDegeri.GuncellemeTarihi = DateTime.Now;
                    await _kurDegeriRepository.UpdateAsync(kurDegeri);
                }

                foreach (var dovizIliski in dovizIliskileri)
                {
                    dovizIliski.SoftDelete = true;
                    dovizIliski.GuncellemeTarihi = DateTime.Now;
                    await _dovizIliskiRepository.UpdateAsync(dovizIliski);
                }

                // Para birimini soft delete yap
                paraBirimi.SoftDelete = true;
                paraBirimi.GuncellemeTarihi = DateTime.Now;
                await _paraBirimiRepository.UpdateAsync(paraBirimi);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Para birimi silinirken hata oluştu. ID: {Id}", id);
                throw;
            }
        }
        #endregion

        #region Kur Değeri İşlemleri
        public async Task<List<KurDegeri>> GetKurDegerleriAsync()
        {
            try
            {
                return await _context.KurDegerleri
                    .Include(kd => kd.ParaBirimi)
                    .Where(kd => !kd.SoftDelete)
                    .OrderByDescending(kd => kd.Tarih)
                    .ThenBy(kd => kd.ParaBirimi.Kod)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Kur değerleri getirilirken hata oluştu");
                throw;
            }
        }

        public async Task<KurDegeri> GetKurDegeriByIdAsync(Guid id)
        {
            try
            {
                return await _context.KurDegerleri
                    .Include(kd => kd.ParaBirimi)
                    .FirstOrDefaultAsync(kd => kd.KurDegeriID == id && !kd.SoftDelete);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "ID ile kur değeri getirilirken hata oluştu. ID: {Id}", id);
                throw;
            }
        }

        public async Task<KurDegeri> GetKurDegeriAsync(string kaynakKod, string hedefKod)
        {
            try
            {
                // Para birimlerini bul
                var kaynakParaBirimi = await _paraBirimiRepository.GetFirstOrDefaultAsync(pb => pb.Kod == kaynakKod && !pb.SoftDelete);
                var hedefParaBirimi = await _paraBirimiRepository.GetFirstOrDefaultAsync(pb => pb.Kod == hedefKod && !pb.SoftDelete);

                if (kaynakParaBirimi == null || hedefParaBirimi == null)
                {
                    throw new Exception($"Para birimleri bulunamadı. Kaynak: {kaynakKod}, Hedef: {hedefKod}");
                }

                // İlişkiyi kontrol et
                var dovizIliski = await _dovizIliskiRepository.GetFirstOrDefaultAsync(di =>
                    di.KaynakParaBirimiID == kaynakParaBirimi.ParaBirimiID &&
                    di.HedefParaBirimiID == hedefParaBirimi.ParaBirimiID &&
                    di.Aktif && !di.SoftDelete);

                if (dovizIliski == null)
                {
                    throw new Exception($"Para birimleri arasında aktif bir ilişki bulunamadı. Kaynak: {kaynakKod}, Hedef: {hedefKod}");
                }

                // En güncel kur değerini getir
                var kurDegeri = await _context.KurDegerleri
                    .Where(kd => kd.ParaBirimiID == hedefParaBirimi.ParaBirimiID && !kd.SoftDelete && kd.Aktif)
                    .OrderByDescending(kd => kd.Tarih)
                    .FirstOrDefaultAsync();

                if (kurDegeri == null)
                {
                    throw new Exception($"Güncel kur değeri bulunamadı. Hedef: {hedefKod}");
                }

                return kurDegeri;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Kur değeri getirilirken hata oluştu. Kaynak: {KaynakKod}, Hedef: {HedefKod}", kaynakKod, hedefKod);
                throw;
            }
        }

        public async Task<KurDegeri> AddKurDegeriAsync(KurDegeri kurDegeri)
        {
            try
            {
                // ID olmayan kur değerleri için yeni GUID oluştur
                if (kurDegeri.KurDegeriID == Guid.Empty)
                {
                    kurDegeri.KurDegeriID = Guid.NewGuid();
                }

                // Oluşturma tarihi ekle
                kurDegeri.OlusturmaTarihi = DateTime.Now;
                kurDegeri.GuncellemeTarihi = DateTime.Now;

                await _kurDegeriRepository.AddAsync(kurDegeri);
                return kurDegeri;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Kur değeri eklenirken hata oluştu. Kur Değeri ID: {Id}", kurDegeri.KurDegeriID);
                throw;
            }
        }

        public async Task<KurDegeri> UpdateKurDegeriAsync(KurDegeri kurDegeri)
        {
            try
            {
                // Mevcut kur değerini al
                var mevcut = await _kurDegeriRepository.GetByIdAsync(kurDegeri.KurDegeriID);
                if (mevcut == null)
                {
                    throw new Exception($"Güncellenecek kur değeri bulunamadı. ID: {kurDegeri.KurDegeriID}");
                }

                // Güncelleme tarihi ekle
                kurDegeri.GuncellemeTarihi = DateTime.Now;
                kurDegeri.OlusturmaTarihi = mevcut.OlusturmaTarihi; // Oluşturma tarihini koruyoruz

                await _kurDegeriRepository.UpdateAsync(kurDegeri);
                return kurDegeri;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Kur değeri güncellenirken hata oluştu. ID: {Id}", kurDegeri.KurDegeriID);
                throw;
            }
        }

        public async Task DeleteKurDegeriAsync(Guid id)
        {
            try
            {
                // İlgili kur değerini al
                var kurDegeri = await _kurDegeriRepository.GetByIdAsync(id);
                if (kurDegeri == null)
                {
                    throw new Exception($"Silinecek kur değeri bulunamadı. ID: {id}");
                }

                // Kur değerini soft delete yap
                kurDegeri.SoftDelete = true;
                kurDegeri.GuncellemeTarihi = DateTime.Now;
                await _kurDegeriRepository.UpdateAsync(kurDegeri);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Kur değeri silinirken hata oluştu. ID: {Id}", id);
                throw;
            }
        }
        #endregion

        #region Döviz İlişkileri İşlemleri
        public async Task<List<DovizIliski>> GetDovizIliskileriAsync()
        {
            try
            {
                return await _context.DovizIliskileri
                    .Include(di => di.KaynakParaBirimi)
                    .Include(di => di.HedefParaBirimi)
                    .Where(di => !di.SoftDelete)
                    .OrderBy(di => di.KaynakParaBirimi.Kod)
                    .ThenBy(di => di.HedefParaBirimi.Kod)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Döviz ilişkileri getirilirken hata oluştu");
                throw;
            }
        }

        public async Task<DovizIliski> GetDovizIliskiByIdAsync(Guid id)
        {
            try
            {
                return await _context.DovizIliskileri
                    .Include(di => di.KaynakParaBirimi)
                    .Include(di => di.HedefParaBirimi)
                    .FirstOrDefaultAsync(di => di.DovizIliskiID == id && !di.SoftDelete);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "ID ile döviz ilişkisi getirilirken hata oluştu. ID: {Id}", id);
                throw;
            }
        }

        public async Task<DovizIliski> AddDovizIliskiAsync(DovizIliski dovizIliski)
        {
            try
            {
                // ID olmayan döviz ilişkileri için yeni GUID oluştur
                if (dovizIliski.DovizIliskiID == Guid.Empty)
                {
                    dovizIliski.DovizIliskiID = Guid.NewGuid();
                }

                // Oluşturma tarihi ekle
                dovizIliski.OlusturmaTarihi = DateTime.Now;
                dovizIliski.GuncellemeTarihi = DateTime.Now;

                await _dovizIliskiRepository.AddAsync(dovizIliski);
                return dovizIliski;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Döviz ilişkisi eklenirken hata oluştu. ID: {Id}", dovizIliski.DovizIliskiID);
                throw;
            }
        }

        public async Task<DovizIliski> UpdateDovizIliskiAsync(DovizIliski dovizIliski)
        {
            try
            {
                // Mevcut döviz ilişkisini al
                var mevcut = await _dovizIliskiRepository.GetByIdAsync(dovizIliski.DovizIliskiID);
                if (mevcut == null)
                {
                    throw new Exception($"Güncellenecek döviz ilişkisi bulunamadı. ID: {dovizIliski.DovizIliskiID}");
                }

                // Güncelleme tarihi ekle
                dovizIliski.GuncellemeTarihi = DateTime.Now;
                dovizIliski.OlusturmaTarihi = mevcut.OlusturmaTarihi; // Oluşturma tarihini koruyoruz

                await _dovizIliskiRepository.UpdateAsync(dovizIliski);
                return dovizIliski;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Döviz ilişkisi güncellenirken hata oluştu. ID: {Id}", dovizIliski.DovizIliskiID);
                throw;
            }
        }

        public async Task DeleteDovizIliskiAsync(Guid id)
        {
            try
            {
                // İlgili döviz ilişkisini al
                var dovizIliski = await _dovizIliskiRepository.GetByIdAsync(id);
                if (dovizIliski == null)
                {
                    throw new Exception($"Silinecek döviz ilişkisi bulunamadı. ID: {id}");
                }

                // Döviz ilişkisini soft delete yap
                dovizIliski.SoftDelete = true;
                dovizIliski.GuncellemeTarihi = DateTime.Now;
                await _dovizIliskiRepository.UpdateAsync(dovizIliski);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Döviz ilişkisi silinirken hata oluştu. ID: {Id}", id);
                throw;
            }
        }
        #endregion

        #region Kur Ayarları
        public async Task<SistemAyarlari> GetKurAyarlariAsync()
        {
            try
            {
                return await _sistemAyarlariRepository.GetFirstAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Kur ayarları getirilirken hata oluştu");
                throw;
            }
        }

        public async Task<SistemAyarlari> UpdateKurAyarlariAsync(SistemAyarlari sistemAyarlari)
        {
            try
            {
                // Mevcut sistem ayarlarını kontrol et
                var mevcut = await _sistemAyarlariRepository.GetFirstOrDefaultAsync();
                
                if (mevcut == null)
                {
                    // Yeni sistem ayarları oluştur
                    sistemAyarlari.SistemAyarlariID = Guid.NewGuid();
                    sistemAyarlari.OlusturmaTarihi = DateTime.Now;
                    sistemAyarlari.GuncellemeTarihi = DateTime.Now;
                    await _sistemAyarlariRepository.AddAsync(sistemAyarlari);
                }
                else
                {
                    // Mevcut sistem ayarlarını güncelle
                    sistemAyarlari.SistemAyarlariID = mevcut.SistemAyarlariID;
                    sistemAyarlari.OlusturmaTarihi = mevcut.OlusturmaTarihi;
                    sistemAyarlari.GuncellemeTarihi = DateTime.Now;
                    await _sistemAyarlariRepository.UpdateAsync(sistemAyarlari);
                }

                return sistemAyarlari;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Kur ayarları güncellenirken hata oluştu");
                throw;
            }
        }
        #endregion

        #region Dış Kaynaklı Kur Güncelleme
        public async Task<bool> UpdateKurlarFromMerkezBankasiAsync()
        {
            try
            {
                _logger.LogInformation("TCMB'den kurlar güncelleniyor...");

                // TCMB XML servisi URL
                string tcmbUrl = "https://www.tcmb.gov.tr/kurlar/today.xml";

                HttpResponseMessage response = await _httpClient.GetAsync(tcmbUrl);
                response.EnsureSuccessStatusCode();

                string xmlContent = await response.Content.ReadAsStringAsync();
                XmlDocument xmlDoc = new XmlDocument();
                xmlDoc.LoadXml(xmlContent);

                XmlNodeList currencyNodes = xmlDoc.SelectNodes("//Currency");
                
                if (currencyNodes == null || currencyNodes.Count == 0)
                {
                    _logger.LogWarning("TCMB'den para birimi verisi alınamadı.");
                    return false;
                }

                int processedCount = 0;
                
                foreach (XmlNode currencyNode in currencyNodes)
                {
                    string currencyCode = currencyNode.Attributes["CurrencyCode"]?.Value;
                    if (string.IsNullOrEmpty(currencyCode)) continue;

                    // Parabirimi veritabanında var mı kontrol et
                    var paraBirimi = await _paraBirimiRepository.GetFirstOrDefaultAsync(pb => pb.Kod == currencyCode && !pb.SoftDelete);
                    
                    if (paraBirimi == null)
                    {
                        // Yeni para birimi ekle
                        string currencyName = currencyNode.SelectSingleNode("CurrencyName")?.InnerText ?? currencyCode;
                        
                        paraBirimi = new ParaBirimi
                        {
                            ParaBirimiID = Guid.NewGuid(),
                            Kod = currencyCode,
                            Ad = currencyName,
                            Sembol = currencyCode,
                            Aktif = true,
                            OlusturmaTarihi = DateTime.Now,
                            GuncellemeTarihi = DateTime.Now
                        };
                        
                        await _paraBirimiRepository.AddAsync(paraBirimi);
                        _logger.LogInformation("Yeni para birimi eklendi: {Code}", currencyCode);
                    }

                    // Kur değerlerini al
                    decimal forexBuying = 0;
                    decimal.TryParse(currencyNode.SelectSingleNode("ForexBuying")?.InnerText.Replace(".", ","), out forexBuying);
                    
                    decimal forexSelling = 0;
                    decimal.TryParse(currencyNode.SelectSingleNode("ForexSelling")?.InnerText.Replace(".", ","), out forexSelling);

                    // Kur değeri ekle
                    var kurDegeri = new KurDegeri
                    {
                        KurDegeriID = Guid.NewGuid(),
                        ParaBirimiID = paraBirimi.ParaBirimiID,
                        AlisDegeri = forexBuying,
                        SatisDegeri = forexSelling,
                        Tarih = DateTime.Now,
                        Kaynak = "TCMB",
                        Aktif = true,
                        OlusturmaTarihi = DateTime.Now,
                        GuncellemeTarihi = DateTime.Now
                    };
                    
                    await _kurDegeriRepository.AddAsync(kurDegeri);
                    processedCount++;
                }

                // Sistem ayarlarında son güncelleme tarihini güncelle
                var sistemAyarlari = await _sistemAyarlariRepository.GetFirstAsync();
                sistemAyarlari.SonDovizGuncellemeTarihi = DateTime.Now;
                sistemAyarlari.GuncellemeTarihi = DateTime.Now;
                await _sistemAyarlariRepository.UpdateAsync(sistemAyarlari);

                _logger.LogInformation("TCMB'den {Count} adet kur güncellendi.", processedCount);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "TCMB'den kurlar güncellenirken hata oluştu");
                return false;
            }
        }

        public async Task<bool> UpdateKurlarFromUzbekistanMBAsync()
        {
            try
            {
                _logger.LogInformation("Özbekistan MB'den kurlar güncelleniyor...");

                // Özbekistan MB API URL (örnek, gerçek API URL'si farklı olabilir)
                string uzbUrl = "https://cbu.uz/en/arkhiv-kursov-valyut/json/";

                HttpResponseMessage response = await _httpClient.GetAsync(uzbUrl);
                response.EnsureSuccessStatusCode();

                // Örnek olarak, gerçek implementasyonda bu kısım değişebilir
                // Burada Özbekistan MB'nin API'si kullanılarak kurlar güncellenecek
                
                // Sistem ayarlarında son güncelleme tarihini güncelle
                var sistemAyarlari = await _sistemAyarlariRepository.GetFirstAsync();
                sistemAyarlari.SonDovizGuncellemeTarihi = DateTime.Now;
                sistemAyarlari.GuncellemeTarihi = DateTime.Now;
                await _sistemAyarlariRepository.UpdateAsync(sistemAyarlari);

                _logger.LogInformation("Özbekistan MB'den kurlar güncellendi.");
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Özbekistan MB'den kurlar güncellenirken hata oluştu");
                return false;
            }
        }
        #endregion

        #region Para Birimi Çevirme
        public async Task<decimal> ConvertParaBirimiAsync(string kaynakKod, string hedefKod, decimal miktar)
        {
            try
            {
                if (kaynakKod == hedefKod) return miktar;

                var kurDegeri = await GetKurDegeriAsync(kaynakKod, hedefKod);
                if (kurDegeri == null)
                {
                    throw new Exception($"Kur değeri bulunamadı. Kaynak: {kaynakKod}, Hedef: {hedefKod}");
                }

                return miktar * kurDegeri.AlisDegeri;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Para birimi çevirisinde hata oluştu. Kaynak: {KaynakKod}, Hedef: {HedefKod}, Miktar: {Miktar}", 
                    kaynakKod, hedefKod, miktar);
                throw;
            }
        }

        public async Task<decimal> GetGuncelKur(string kaynakParaBirimi, string hedefParaBirimi)
        {
            try
            {
                var kurDegeri = await GetKurDegeriAsync(kaynakParaBirimi, hedefParaBirimi);
                return kurDegeri?.AlisDegeri ?? 0;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Güncel kur alınırken hata oluştu. Kaynak: {KaynakKod}, Hedef: {HedefKod}", 
                    kaynakParaBirimi, hedefParaBirimi);
                return 0;
            }
        }
        #endregion
    }
} 