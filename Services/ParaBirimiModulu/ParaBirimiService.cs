using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using MuhasebeStokWebApp.Data;
using MuhasebeStokWebApp.Data.Entities.ParaBirimiModulu;
using System.Net.Http;
using System.Text.Json;
using Microsoft.Extensions.Logging;
using MuhasebeStokWebApp.Models;
using MuhasebeStokWebApp.Models.ParaBirimiModels;
using System.Threading;

namespace MuhasebeStokWebApp.Services.ParaBirimiModulu
{
    /// <summary>
    /// Para birimi ve kur işlemleri için servis implementasyonu
    /// </summary>
    public class ParaBirimiService : IParaBirimiService
    {
        private readonly ApplicationDbContext _context;
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly string _apiKey = "645b5bebcab7cef56e1b609c";
        private readonly string _apiBaseUrl = "https://v6.exchangerate-api.com/v6/";
        private readonly ILogger<ParaBirimiService> _logger;

        public ParaBirimiService(ApplicationDbContext context, IHttpClientFactory httpClientFactory, ILogger<ParaBirimiService> logger)
        {
            _context = context;
            _httpClientFactory = httpClientFactory;
            _logger = logger;
        }

        #region Para Birimi İşlemleri
        /// <summary>
        /// Tüm para birimlerini döndürür
        /// </summary>
        public async Task<List<MuhasebeStokWebApp.Data.Entities.ParaBirimiModulu.ParaBirimi>> GetAllParaBirimleriAsync(bool aktiflerOnly = true)
        {
            var query = _context.ParaBirimleri
                .Where(p => !p.Silindi);
                
            if (aktiflerOnly)
                query = query.Where(p => p.Aktif);
                
            return await query
                .OrderBy(p => p.Sira)
                .ThenBy(p => p.Kod)
                .ToListAsync();
        }
        
        /// <summary>
        /// Tüm para birimlerini IParaBirimiService.GetAllAsync için döndürür
        /// </summary>
        public async Task<IEnumerable<MuhasebeStokWebApp.Data.Entities.ParaBirimiModulu.ParaBirimi>> GetAllAsync()
        {
            return await GetAllParaBirimleriAsync(false);
        }
        
        /// <summary>
        /// ID'ye göre para birimi döndürür
        /// </summary>
        public async Task<MuhasebeStokWebApp.Data.Entities.ParaBirimiModulu.ParaBirimi> GetParaBirimiByIdAsync(Guid paraBirimiId)
        {
            return await _context.ParaBirimleri
                .FirstOrDefaultAsync(p => p.ParaBirimiID == paraBirimiId && !p.Silindi);
        }
        
        /// <summary>
        /// Koda göre para birimi döndürür
        /// </summary>
        public async Task<MuhasebeStokWebApp.Data.Entities.ParaBirimiModulu.ParaBirimi> GetParaBirimiByKodAsync(string kod)
        {
            return await _context.ParaBirimleri
                .FirstOrDefaultAsync(p => p.Kod == kod && !p.Silindi);
        }
        
        /// <summary>
        /// Para birimi ekler
        /// </summary>
        public async Task<MuhasebeStokWebApp.Data.Entities.ParaBirimiModulu.ParaBirimi> AddParaBirimiAsync(MuhasebeStokWebApp.Data.Entities.ParaBirimiModulu.ParaBirimi paraBirimi)
        {
            if (paraBirimi.AnaParaBirimiMi)
            {
                // Diğer ana para birimlerini deaktive et
                var anaParaBirimleri = await _context.ParaBirimleri
                    .Where(p => p.AnaParaBirimiMi && !p.Silindi)
                    .ToListAsync();
                    
                foreach (var anaParaBirimi in anaParaBirimleri)
                {
                    anaParaBirimi.AnaParaBirimiMi = false;
                    _context.ParaBirimleri.Update(anaParaBirimi);
                }
            }
            
            await _context.ParaBirimleri.AddAsync(paraBirimi);
            await _context.SaveChangesAsync();
            
            return paraBirimi;
        }
        
        /// <summary>
        /// Para birimi günceller
        /// </summary>
        public async Task<MuhasebeStokWebApp.Data.Entities.ParaBirimiModulu.ParaBirimi> UpdateParaBirimiAsync(MuhasebeStokWebApp.Data.Entities.ParaBirimiModulu.ParaBirimi paraBirimi)
        {
            if (paraBirimi.AnaParaBirimiMi)
            {
                // Diğer ana para birimlerini deaktive et
                var anaParaBirimleri = await _context.ParaBirimleri
                    .Where(p => p.ParaBirimiID != paraBirimi.ParaBirimiID && p.AnaParaBirimiMi && !p.Silindi)
                    .ToListAsync();
                    
                foreach (var anaParaBirimi in anaParaBirimleri)
                {
                    anaParaBirimi.AnaParaBirimiMi = false;
                    _context.ParaBirimleri.Update(anaParaBirimi);
                }
            }
            
            _context.ParaBirimleri.Update(paraBirimi);
            await _context.SaveChangesAsync();
            
            return paraBirimi;
        }
        
        /// <summary>
        /// Para birimi siler
        /// </summary>
        public async Task<bool> DeleteParaBirimiAsync(Guid paraBirimiId)
        {
            var paraBirimi = await GetParaBirimiByIdAsync(paraBirimiId);
            
            if (paraBirimi == null)
                return false;
                
            // İlişkili kayıtları kontrol et
            bool hasRelatedRecords = await HasRelatedRecordsAsync(paraBirimiId);
            
            if (hasRelatedRecords)
                return false;
                
            // Soft delete
            paraBirimi.Silindi = true;
            _context.ParaBirimleri.Update(paraBirimi);
            await _context.SaveChangesAsync();
            
            return true;
        }
        
        /// <summary>
        /// Ana para birimi kodunu döndürür
        /// </summary>
        public async Task<string> GetAnaParaBirimiKoduAsync()
        {
            var anaParaBirimi = await GetAnaParaBirimiAsync();
            return anaParaBirimi?.Kod ?? "TRY";
        }
        
        /// <summary>
        /// Ana para birimini döndürür
        /// </summary>
        public async Task<MuhasebeStokWebApp.Data.Entities.ParaBirimiModulu.ParaBirimi> GetAnaParaBirimiAsync()
        {
            return await _context.ParaBirimleri
                .FirstOrDefaultAsync(p => p.AnaParaBirimiMi && !p.Silindi);
        }
        
        /// <summary>
        /// Yerel para birimini (TRY) döndürür
        /// Yerel para birimi bulunamazsa ana para birimini döndürür
        /// </summary>
        public async Task<MuhasebeStokWebApp.Data.Entities.ParaBirimiModulu.ParaBirimi> GetYerelParaBirimiAsync()
        {
            // Önce TRY kodlu para birimini ara
            var yerelParaBirimi = await _context.ParaBirimleri
                .FirstOrDefaultAsync(p => p.Kod == "TRY" && !p.Silindi);
                
            // TRY bulunamazsa ana para birimini döndür
            if (yerelParaBirimi == null)
                return await GetAnaParaBirimiAsync();
                
            return yerelParaBirimi;
        }
        
        /// <summary>
        /// Ana para birimini değiştirir
        /// </summary>
        public async Task<bool> SetAnaParaBirimiAsync(Guid paraBirimiId)
        {
            // Tüm para birimlerini al
            var paraBirimleri = await _context.ParaBirimleri
                .Where(p => !p.Silindi)
                .ToListAsync();
                
            // Hepsinin ana para birimi özelliğini kapat
            foreach (var p in paraBirimleri)
            {
                p.AnaParaBirimiMi = false;
                _context.Update(p);
            }
            
            // Seçilen para birimini ana para birimi yap
            var yeniAnaParaBirimi = paraBirimleri.FirstOrDefault(p => p.ParaBirimiID == paraBirimiId);
            
            if (yeniAnaParaBirimi == null)
                return false;
                
            yeniAnaParaBirimi.AnaParaBirimiMi = true;
            _context.Update(yeniAnaParaBirimi);
            
            await _context.SaveChangesAsync();
            
            return true;
        }
        
        /// <summary>
        /// Para biriminin ilişkili kayıtları var mı kontrol eder
        /// </summary>
        private async Task<bool> HasRelatedRecordsAsync(Guid paraBirimiId)
        {
            // Kur değerleri ilişkisi kontrol et
            if (await _context.KurDegerleri.AnyAsync(k => k.ParaBirimiID == paraBirimiId && !k.Silindi))
                return true;
                
            // Para birimi ilişkileri kontrol et
            if (await _context.Set<ParaBirimiIliski>().AnyAsync(i => 
                (i.KaynakParaBirimiID == paraBirimiId || i.HedefParaBirimiID == paraBirimiId) && !i.Silindi))
                return true;
                
            return false;
        }
        #endregion
        
        #region Kur Değeri İşlemleri
        /// <summary>
        /// Tüm kur değerlerini döndürür
        /// </summary>
        public async Task<List<KurDegeri>> GetAllKurDegerleriAsync(DateTime? tarih = null)
        {
            var query = _context.KurDegerleri
                .Include(k => k.ParaBirimi)
                .Where(k => !k.Silindi);
                
            if (tarih.HasValue)
                query = query.Where(k => k.Tarih.Date == tarih.Value.Date);
                
            return await query
                .OrderByDescending(k => k.Tarih)
                .ThenBy(k => k.ParaBirimi.Kod)
                .ToListAsync();
        }
        
        /// <summary>
        /// Para birimine göre kur değerlerini döndürür
        /// </summary>
        public async Task<List<KurDegeri>> GetKurDegerleriByParaBirimiAsync(Guid paraBirimiId, DateTime? baslangicTarihi = null, DateTime? bitisTarihi = null)
        {
            var query = _context.KurDegerleri
                .Include(k => k.ParaBirimi)
                .Where(k => k.ParaBirimiID == paraBirimiId && !k.Silindi);
                
            if (baslangicTarihi.HasValue)
                query = query.Where(k => k.Tarih.Date >= baslangicTarihi.Value.Date);
                
            if (bitisTarihi.HasValue)
                query = query.Where(k => k.Tarih.Date <= bitisTarihi.Value.Date);
                
            return await query
                .OrderByDescending(k => k.Tarih)
                .ToListAsync();
        }
        
        /// <summary>
        /// Son kur değerini döndürür
        /// </summary>
        public async Task<KurDegeri> GetSonKurDegeriByParaBirimiAsync(Guid paraBirimiId)
        {
            return await _context.KurDegerleri
                .Include(k => k.ParaBirimi)
                .Where(k => k.ParaBirimiID == paraBirimiId && !k.Silindi)
                .OrderByDescending(k => k.Tarih)
                .FirstOrDefaultAsync();
        }
        
        /// <summary>
        /// Kur değeri ekler
        /// </summary>
        public async Task<KurDegeri> AddKurDegeriAsync(KurDegeri kurDegeri)
        {
            // Aynı gün, aynı para birimi için kur kontrolü
            var existingKur = await _context.KurDegerleri
                .FirstOrDefaultAsync(k => 
                    k.ParaBirimiID == kurDegeri.ParaBirimiID && 
                    k.Tarih.Date == kurDegeri.Tarih.Date && 
                    !k.Silindi);
                    
            if (existingKur != null)
            {
                // Mevcut kaydı güncelle
                existingKur.Alis = kurDegeri.Alis;
                existingKur.Satis = kurDegeri.Satis;
                existingKur.Aktif = kurDegeri.Aktif;
                existingKur.Aciklama = kurDegeri.Aciklama;
                existingKur.GuncellemeTarihi = DateTime.Now;
                existingKur.SonGuncelleyenKullaniciID = kurDegeri.OlusturanKullaniciID;
                
                _context.KurDegerleri.Update(existingKur);
                await _context.SaveChangesAsync();
                
                return existingKur;
            }
            
            await _context.KurDegerleri.AddAsync(kurDegeri);
            await _context.SaveChangesAsync();
            
            return kurDegeri;
        }
        
        /// <summary>
        /// Kur değerlerini toplu olarak ekler
        /// </summary>
        public async Task<List<KurDegeri>> AddKurDegerleriAsync(List<KurDegeri> kurDegerleri)
        {
            foreach (var kurDegeri in kurDegerleri)
            {
                await AddKurDegeriAsync(kurDegeri);
            }
            
            return kurDegerleri;
        }
        
        /// <summary>
        /// Kur değeri günceller
        /// </summary>
        public async Task<KurDegeri> UpdateKurDegeriAsync(KurDegeri kurDegeri)
        {
            _context.KurDegerleri.Update(kurDegeri);
            await _context.SaveChangesAsync();
            
            return kurDegeri;
        }
        
        /// <summary>
        /// Kur değeri siler
        /// </summary>
        public async Task<bool> DeleteKurDegeriAsync(Guid kurDegeriId)
        {
            var kurDegeri = await _context.KurDegerleri
                .FirstOrDefaultAsync(k => k.KurDegeriID == kurDegeriId);
                
            if (kurDegeri == null)
                return false;
                
            // Soft delete
            kurDegeri.Silindi = true;
            _context.KurDegerleri.Update(kurDegeri);
            await _context.SaveChangesAsync();
            
            return true;
        }
        
        /// <summary>
        /// Kur değerlerini harici API'den günceller
        /// </summary>
        public async Task<bool> GuncelleKurDegerleriniFromAPIAsync()
        {
            try
            {
                var httpClient = _httpClientFactory.CreateClient();
                
                // USD'yi baz para birimi olarak kullan - API USD bazlı çalışıyor
                string bazParaBirimiKodu = "USD";
                
                // API endpoint'i oluştur
                string apiEndpoint = $"{_apiBaseUrl}{_apiKey}/latest/{bazParaBirimiKodu}";
                
                // API'den verileri al
                var response = await httpClient.GetAsync(apiEndpoint);
                
                if (!response.IsSuccessStatusCode)
                    return false;
                    
                // JSON yanıtını oku
                var jsonString = await response.Content.ReadAsStringAsync();
                using var jsonDoc = JsonDocument.Parse(jsonString);
                var root = jsonDoc.RootElement;
                
                // Sonuç kontrolü
                if (root.GetProperty("result").GetString() != "success")
                    return false;
                    
                // Kurları al
                var tarih = DateTime.Now;
                var conversionRates = root.GetProperty("conversion_rates");
                
                // Tüm para birimlerini al
                var paraBirimleri = await GetAllParaBirimleriAsync();
                
                // Ana para birimimizi bul - genellikle TRY
                var anaParaBirimi = await GetAnaParaBirimiAsync();
                string anaParaBirimiKodu = anaParaBirimi?.Kod ?? "TRY";

                foreach (var paraBirimi in paraBirimleri)
                {
                    // Para biriminin API'den kurunu al
                    if (conversionRates.TryGetProperty(paraBirimi.Kod, out var rate))
                    {
                        decimal kurDegeri = rate.GetDecimal();

                        // USD ana para birimi ise 1 olarak ayarla (baz para birimi)
                        if (paraBirimi.Kod == bazParaBirimiKodu)
                        {
                            if (paraBirimi.AnaParaBirimiMi)
                            {
                                kurDegeri = 1;
                            }
                            else
                            {
                                // USD ana para birimi değilse, API'den gelen USD/TRY kuruyla işlem yap
                                if (conversionRates.TryGetProperty(anaParaBirimiKodu, out var tryRate))
                                {
                                    kurDegeri = tryRate.GetDecimal();
                                }
                            }
                        }
                        else if (paraBirimi.AnaParaBirimiMi)
                        {
                            // Ana para birimi (TRY) için API'den gelen değeri al
                            kurDegeri = rate.GetDecimal();
                        }
                        else
                        {
                            // Diğer para birimleri için çapraz kur hesapla (TRY baz alınarak)
                            if (conversionRates.TryGetProperty(anaParaBirimiKodu, out var tryRate))
                            {
                                decimal tryDegeri = tryRate.GetDecimal();
                                kurDegeri = rate.GetDecimal() / tryDegeri;
                            }
                        }

                        var kurDegeriEntity = new KurDegeri
                        {
                            KurDegeriID = Guid.NewGuid(),
                            ParaBirimiID = paraBirimi.ParaBirimiID,
                            Tarih = tarih,
                            Alis = kurDegeri,
                            Satis = kurDegeri * 1.02m, // %2 farkla satış kuru
                            Aktif = true,
                            OlusturmaTarihi = DateTime.Now,
                            OlusturanKullaniciID = "Sistem",
                            Aciklama = "Exchange Rate API'den otomatik güncellendi"
                        };
                        
                        await AddKurDegeriAsync(kurDegeriEntity);
                    }
                }
                
                return true;
            }
            catch (Exception ex)
            {
                // Hata logla
                Console.WriteLine($"API hata: {ex.Message}");
                return false;
            }
        }
        #endregion
        
        #region Para Birimi İlişkileri
        /// <summary>
        /// Tüm para birimi ilişkilerini döndürür
        /// </summary>
        public async Task<List<ParaBirimiIliski>> GetAllParaBirimiIliskileriAsync(bool aktiflerOnly = true)
        {
            var query = _context.Set<ParaBirimiIliski>()
                .Include(i => i.KaynakParaBirimi)
                .Include(i => i.HedefParaBirimi)
                .Where(i => !i.Silindi);
                
            if (aktiflerOnly)
                query = query.Where(i => i.Aktif);
                
            return await query
                .OrderBy(i => i.KaynakParaBirimi.Kod)
                .ThenBy(i => i.HedefParaBirimi.Kod)
                .ToListAsync();
        }
        
        /// <summary>
        /// ID'ye göre para birimi ilişkisini döndürür
        /// </summary>
        public async Task<ParaBirimiIliski> GetParaBirimiIliskiByIdAsync(Guid iliskiId)
        {
            return await _context.Set<ParaBirimiIliski>()
                .Include(i => i.KaynakParaBirimi)
                .Include(i => i.HedefParaBirimi)
                .FirstOrDefaultAsync(i => i.ParaBirimiIliskiID == iliskiId && !i.Silindi);
        }
        
        /// <summary>
        /// İki para birimi arasındaki ilişkiyi döndürür
        /// </summary>
        public async Task<ParaBirimiIliski> GetIliskiByParaBirimleriAsync(Guid kaynakId, Guid hedefId)
        {
            return await _context.Set<ParaBirimiIliski>()
                .Include(i => i.KaynakParaBirimi)
                .Include(i => i.HedefParaBirimi)
                .FirstOrDefaultAsync(i => 
                    i.KaynakParaBirimiID == kaynakId && 
                    i.HedefParaBirimiID == hedefId && 
                    !i.Silindi);
        }
        
        /// <summary>
        /// Para birimi ilişkisi ekler
        /// </summary>
        public async Task<ParaBirimiIliski> AddParaBirimiIliskiAsync(ParaBirimiIliski paraBirimiIliski)
        {
            // Aynı ilişki var mı kontrol et
            var existingIliski = await GetIliskiByParaBirimleriAsync(
                paraBirimiIliski.KaynakParaBirimiID, 
                paraBirimiIliski.HedefParaBirimiID);
                
            if (existingIliski != null)
            {
                // Silinmiş ilişkiyi geri aktifleme
                if (existingIliski.Silindi)
                {
                    existingIliski.Silindi = false;
                    existingIliski.Aktif = paraBirimiIliski.Aktif;
                    existingIliski.GuncellemeTarihi = DateTime.Now;
                    existingIliski.SonGuncelleyenKullaniciID = paraBirimiIliski.OlusturanKullaniciID;
                    
                    _context.Update(existingIliski);
                    await _context.SaveChangesAsync();
                    
                    return existingIliski;
                }
                
                return existingIliski;
            }
            
            await _context.AddAsync(paraBirimiIliski);
            await _context.SaveChangesAsync();
            
            return paraBirimiIliski;
        }
        
        /// <summary>
        /// Para birimi ilişkisi günceller
        /// </summary>
        public async Task<ParaBirimiIliski> UpdateParaBirimiIliskiAsync(ParaBirimiIliski paraBirimiIliski)
        {
            _context.Update(paraBirimiIliski);
            await _context.SaveChangesAsync();
            
            return paraBirimiIliski;
        }
        
        /// <summary>
        /// Para birimi ilişkisi siler
        /// </summary>
        public async Task<bool> DeleteParaBirimiIliskiAsync(Guid iliskiId)
        {
            var iliski = await GetParaBirimiIliskiByIdAsync(iliskiId);
            
            if (iliski == null)
                return false;
                
            // Soft delete
            iliski.Silindi = true;
            _context.Update(iliski);
            await _context.SaveChangesAsync();
            
            return true;
        }
        #endregion
        
        #region Hesaplama İşlemleri
        /// <summary>
        /// Para birimi için eksik kur değeri oluşturur
        /// </summary>
        private async Task<bool> CreateDefaultKurDegeriIfNotExistsAsync(Guid paraBirimiId)
        {
            try
            {
                // Para birimini kontrol et
                var paraBirimi = await _context.ParaBirimleri
                    .FirstOrDefaultAsync(p => p.ParaBirimiID == paraBirimiId && !p.Silindi);

                if (paraBirimi == null)
                {
                    _logger.LogWarning($"Para birimi bulunamadı: {paraBirimiId}");
                    return false;
                }

                // Kur değeri var mı kontrol et
                var existingKur = await _context.KurDegerleri
                    .Where(k => k.ParaBirimiID == paraBirimiId && !k.Silindi)
                    .OrderByDescending(k => k.Tarih)
                    .FirstOrDefaultAsync();

                if (existingKur != null)
                {
                    _logger.LogInformation($"Para birimi için zaten kur değeri var: {paraBirimiId}, Tarih: {existingKur.Tarih}");
                    return true;
                }

                // Para birimi ana para birimi ise kur değeri oluşturmaya gerek yok
                if (paraBirimi.AnaParaBirimiMi)
                {
                    _logger.LogInformation($"Para birimi ana para birimi olduğu için kur değeri oluşturulmadı: {paraBirimiId}");
                    return true;
                }

                // Yeni kur değeri oluştur
                var newKurDegeri = new KurDegeri
                {
                    KurDegeriID = Guid.NewGuid(),
                    ParaBirimiID = paraBirimiId,
                    Tarih = DateTime.Now,
                    Alis = 1.0m,
                    Satis = 1.0m,
                    Aktif = true,
                    Silindi = false,
                    OlusturmaTarihi = DateTime.Now,
                    Aciklama = "Otomatik oluşturuldu - Varsayılan değer"
                };

                await _context.KurDegerleri.AddAsync(newKurDegeri);
                await _context.SaveChangesAsync();
                
                _logger.LogInformation($"Para birimi için varsayılan kur değeri oluşturuldu: {paraBirimiId}");
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Para birimi için varsayılan kur değeri oluşturulurken hata: {paraBirimiId}");
                return false;
            }
        }

        /// <summary>
        /// İki para birimi arasındaki kur değerini hesaplar
        /// </summary>
        public async Task<decimal> HesaplaKurDegeriAsync(Guid kaynakParaBirimiId, Guid hedefParaBirimiId, DateTime? tarih = null)
        {
            try
            {
                // Aynı para birimleri için dönüşüm 1'dir
                if (kaynakParaBirimiId == hedefParaBirimiId)
                    return 1.0m;
                
                var tarihValue = tarih ?? DateTime.Now;
                
                // Ana para birimini al
                var anaParaBirimi = await GetAnaParaBirimiAsync();
                if (anaParaBirimi == null)
                {
                    _logger.LogWarning("Ana para birimi bulunamadı, varsayılan değer 1.0 kullanılıyor.");
                    return 1.0m;
                }
                
                // Kaynak ve hedef para birimleri için kur değerlerini al
                var kaynakKur = await _context.KurDegerleri
                    .Where(k => k.ParaBirimiID == kaynakParaBirimiId && !k.Silindi && k.Aktif && k.Tarih <= tarihValue)
                    .OrderByDescending(k => k.Tarih)
                    .FirstOrDefaultAsync();
                    
                var hedefKur = await _context.KurDegerleri
                    .Where(k => k.ParaBirimiID == hedefParaBirimiId && !k.Silindi && k.Aktif && k.Tarih <= tarihValue)
                    .OrderByDescending(k => k.Tarih)
                    .FirstOrDefaultAsync();
                
                // Kur değerleri yoksa, varsayılan değerler oluştur
                if (kaynakKur == null && kaynakParaBirimiId != anaParaBirimi.ParaBirimiID)
                {
                    _logger.LogWarning($"Kaynak para birimi ({kaynakParaBirimiId}) için kur değeri bulunamadı. Varsayılan değer oluşturuluyor.");
                    await CreateDefaultKurDegeriIfNotExistsAsync(kaynakParaBirimiId);
                    
                    // Yeni oluşturulan kur değerini tekrar al
                    kaynakKur = await _context.KurDegerleri
                        .Where(k => k.ParaBirimiID == kaynakParaBirimiId && !k.Silindi && k.Aktif)
                        .OrderByDescending(k => k.Tarih)
                        .FirstOrDefaultAsync();
                }
                
                if (hedefKur == null && hedefParaBirimiId != anaParaBirimi.ParaBirimiID)
                {
                    _logger.LogWarning($"Hedef para birimi ({hedefParaBirimiId}) için kur değeri bulunamadı. Varsayılan değer oluşturuluyor.");
                    await CreateDefaultKurDegeriIfNotExistsAsync(hedefParaBirimiId);
                    
                    // Yeni oluşturulan kur değerini tekrar al
                    hedefKur = await _context.KurDegerleri
                        .Where(k => k.ParaBirimiID == hedefParaBirimiId && !k.Silindi && k.Aktif)
                        .OrderByDescending(k => k.Tarih)
                        .FirstOrDefaultAsync();
                }

                // Sonsuz döngüye girme riski var, kontrol ekleyelim:
                // Eğer kaynak ya da hedef zaten ana para birimiyse ve diğer para biriminin kur değeri yoksa,
                // varsayılan değer kullanarak dönelim.
                if (kaynakParaBirimiId == anaParaBirimi.ParaBirimiID)
                {
                    if (hedefKur == null)
                    {
                        // Hedef para birimi için kur yok, varsayılan değer döndür
                        _logger.LogWarning($"Hedef para birimi ({hedefParaBirimiId}) için kur değeri bulunamadı. Varsayılan değer 1.0 kullanılıyor.");
                        return 1.0m;
                    }
                    return hedefKur.Alis; // Ana para birimi baz alınır 
                }
                else if (hedefParaBirimiId == anaParaBirimi.ParaBirimiID)
                {
                    if (kaynakKur == null)
                    {
                        // Kaynak para birimi için kur yok, varsayılan değer döndür
                        _logger.LogWarning($"Kaynak para birimi ({kaynakParaBirimiId}) için kur değeri bulunamadı. Varsayılan değer 1.0 kullanılıyor.");
                        return 1.0m;
                    }
                    return 1.0m / kaynakKur.Alis; // Ana para birimine çevirme
                }
                    
                // Her iki para birimi için de kur değeri bulunamadıysa
                if (kaynakKur == null && hedefKur == null)
                {
                    _logger.LogWarning($"Hem kaynak ({kaynakParaBirimiId}) hem de hedef ({hedefParaBirimiId}) para birimleri için kur değerleri bulunamadı. Varsayılan değer 1.0 kullanılıyor.");
                    return 1.0m;
                }

                // Sadece bir tanesinin kur değeri varsa, o değeri kullan
                if (kaynakKur != null && hedefKur == null)
                {
                    return 1.0m / kaynakKur.Alis;
                }
                else if (kaynakKur == null && hedefKur != null)
                {
                    return hedefKur.Alis;
                }
                    
                // En son çare olarak, ana para birimi üzerinden dönüşüm yap
                // Sonsuz döngü ihtimaline karşı doğrudan veritabanından sorgulama yapıyoruz
                var kaynakToAna = await _context.KurDegerleri
                    .Where(k => k.ParaBirimiID == kaynakParaBirimiId && k.Aktif && !k.Silindi)
                    .OrderByDescending(k => k.Tarih)
                    .Select(k => k.Alis)
                    .FirstOrDefaultAsync();
                
                var anaToHedef = await _context.KurDegerleri
                    .Where(k => k.ParaBirimiID == hedefParaBirimiId && k.Aktif && !k.Silindi)
                    .OrderByDescending(k => k.Tarih)
                    .Select(k => k.Alis)
                    .FirstOrDefaultAsync();
                
                if (kaynakToAna > 0 && anaToHedef > 0)
                {
                    return (1.0m / kaynakToAna) * anaToHedef;
                }
                
                // Hiçbir şekilde kur bulunamadıysa, varsayılan değer döndür
                _logger.LogWarning("Hiçbir kur değeri bulunamadı, varsayılan değer 1.0 kullanılıyor.");
                return 1.0m;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Para birimi dönüşümü sırasında hata oluştu.");
                return 1.0m;
            }
        }
        
        /// <summary>
        /// İki para birimi arasında dönüşüm yapar
        /// </summary>
        public async Task<decimal> CevirmeTutarAsync(decimal tutar, Guid kaynakParaBirimiId, Guid hedefParaBirimiId, DateTime? tarih = null)
        {
            if (kaynakParaBirimiId == hedefParaBirimiId)
                return tutar;
                
            var kurDegeri = await HesaplaKurDegeriAsync(kaynakParaBirimiId, hedefParaBirimiId, tarih);
            return tutar * kurDegeri;
        }

        public async Task<decimal?> UpdateEfektifDegerleriAsync(KurDegeri kurDegeri)
        {
            // Bu metod artık entity'de efektif değerleri saklamak yerine
            // runtime'da hesaplanan değerleri kullanacak şekilde değiştirildi
            try
            {
                if (kurDegeri == null)
                    return null;
                
                // KurMarj bilgisini al (varsayılan %0.5)
                decimal marj = 0.005m; // Varsayılan değer
                
                // async metod için bir await işlemi ekliyoruz
                await Task.CompletedTask;
                
                // Efektif değerler ihtiyaç duyulduğunda hesaplanacak 
                // ve entity'de saklanmayacak
                return marj;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Kur efektif değerleri hesaplanırken hata oluştu");
                return null;
            }
        }

        public async Task<decimal> GetCurrentExchangeRateAsync(string sourceCurrency, string targetCurrency)
        {
            try
            {
                var client = _httpClientFactory.CreateClient();
                
                // Timeout ekle - 10 saniye
                using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(10));
                
                // Zaman aşımı kontrolüyle HTTP isteği gönder
                var response = await client.GetAsync($"https://api.exchangerate-api.com/v4/latest/{sourceCurrency}", cts.Token);
                
                if (response.IsSuccessStatusCode)
                {
                    var content = await response.Content.ReadAsStringAsync();
                    var data = System.Text.Json.JsonSerializer.Deserialize<MuhasebeStokWebApp.Models.ParaBirimiModels.ExchangeRateResponse>(content);
                    
                    if (data?.Rates != null && data.Rates.ContainsKey(targetCurrency))
                    {
                        return data.Rates[targetCurrency];
                    }
                    else
                    {
                        _logger.LogWarning($"Hedef para birimi ({targetCurrency}) kur değeri bulunamadı. Mevcut para birimleri: {(data?.Rates != null ? string.Join(", ", data.Rates.Keys) : "Rates null")}");
                        return 0;
                    }
                }
                else
                {
                    _logger.LogError($"API yanıtı başarısız: HTTP {(int)response.StatusCode} - {response.ReasonPhrase}");
                    return 0;
                }
            }
            catch (OperationCanceledException ex)
            {
                _logger.LogError($"Döviz kuru alınırken zaman aşımı oluştu: {sourceCurrency} to {targetCurrency}. Süre: 10 saniye");
                return 0;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Döviz kuru alınırken hata oluştu: {sourceCurrency} to {targetCurrency}. Hata: {ex.Message}");
                return 0;
            }
        }
        #endregion
    }
} 