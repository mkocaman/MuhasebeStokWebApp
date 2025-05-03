using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Linq;
using MuhasebeStokWebApp.Data.Entities.DovizModulu;
using MuhasebeStokWebApp.Models;
using MuhasebeStokWebApp.Services.Interfaces;
using Microsoft.Extensions.Logging;
using MuhasebeStokWebApp.Services.ParaBirimiModulu;
using Microsoft.Extensions.Caching.Memory;
using System.Text.RegularExpressions;

namespace MuhasebeStokWebApp.Services.ParaBirimiModulu
{
    /// <summary>
    /// IDovizKuruService için adaptör sınıfı.
    /// ParaBirimiService'i IDovizKuruService'e uyumlu hale getirir.
    /// </summary>
    public class DovizKuruServiceAdapter : IDovizKuruService
    {
        private readonly IParaBirimiService _paraBirimiService;
        private readonly IKurDegeriService _kurDegeriService;
        private readonly ILogger<DovizKuruServiceAdapter> _logger;
        private readonly IMemoryCache _cache;
        
        // Önbellekleme sabitleri
        private const string CACHE_KUR_DEGERLERI = "KurDegerleri_";
        private const string CACHE_SON_KUR = "SonKur_";
        private const string CACHE_KUR_HESAPLAMA = "KurHesaplama_";
        private const int CACHE_DURATION_MINUTES = 30;

        public DovizKuruServiceAdapter(
            IParaBirimiService paraBirimiService,
            IKurDegeriService kurDegeriService,
            ILogger<DovizKuruServiceAdapter> logger,
            IMemoryCache cache)
        {
            _paraBirimiService = paraBirimiService;
            _kurDegeriService = kurDegeriService;
            _logger = logger;
            _cache = cache;
        }

        public async Task<decimal> CevirmeTutarAsync(decimal tutar, Guid kaynakParaBirimiId, Guid hedefParaBirimiId, DateTime? tarih = null)
        {
            return await _paraBirimiService.HesaplaKurDegeriAsync(kaynakParaBirimiId, hedefParaBirimiId, tarih) * tutar;
        }

        public async Task<decimal> CevirmeTutarByKodAsync(decimal tutar, string kaynakKod, string hedefKod, DateTime? tarih = null)
        {
            var kaynakParaBirimi = await _paraBirimiService.GetParaBirimiByKodAsync(kaynakKod);
            var hedefParaBirimi = await _paraBirimiService.GetParaBirimiByKodAsync(hedefKod);

            if (kaynakParaBirimi == null || hedefParaBirimi == null)
                throw new ArgumentException("Para birimi kodu bulunamadı.");

            return await CevirmeTutarAsync(tutar, kaynakParaBirimi.ParaBirimiID, hedefParaBirimi.ParaBirimiID, tarih);
        }

        public async Task<decimal> GetGuncelKurAsync(string kaynakKod, string hedefKod, DateTime? tarih = null)
        {
            // Aynı para birimi ise 1 dön
            if (kaynakKod == hedefKod)
                return 1m;
                
            // Önbellekte veriyi ara
            string cacheKey = $"{CACHE_KUR_HESAPLAMA}{kaynakKod}_{hedefKod}_{tarih?.ToString("yyyyMMdd") ?? "current"}";
            
            if (_cache.TryGetValue(cacheKey, out decimal cachedRate))
            {
                _logger.LogInformation($"Kur değeri önbellekten alındı: {kaynakKod} -> {hedefKod}");
                return cachedRate;
            }
            
            try
            {
                // Veri yoksa hesapla
                var kurDegeri = await HesaplaKurDegeriByKodAsync(kaynakKod, hedefKod, tarih);
                
                // Güncel kurlar için daha uzun süreli önbellekleme yapılandırması
                var cacheOptions = new MemoryCacheEntryOptions()
                    .SetSlidingExpiration(TimeSpan.FromMinutes(CACHE_DURATION_MINUTES * 2)) // 2 katı süre
                    .SetAbsoluteExpiration(TimeSpan.FromHours(24)); // 24 saat maksimum
                
                // TRY ve USD gibi standart kurlar için daha uzun süre
                if ((kaynakKod == "TRY" && hedefKod == "USD") || 
                    (kaynakKod == "USD" && hedefKod == "TRY") ||
                    (kaynakKod == "UZS" && hedefKod == "USD") || 
                    (kaynakKod == "USD" && hedefKod == "UZS"))
                {
                    cacheOptions = new MemoryCacheEntryOptions()
                        .SetSlidingExpiration(TimeSpan.FromHours(1)) // 1 saat
                        .SetAbsoluteExpiration(TimeSpan.FromDays(1)); // 1 gün maksimum
                }
                
                _cache.Set(cacheKey, kurDegeri, cacheOptions);
                return kurDegeri;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Kur hesaplanırken hata oluştu: {kaynakKod} -> {hedefKod}");
                throw;
            }
        }

        public async Task<List<DovizKuru>> GetLatestRatesAsync(int count = 5)
        {
            var kurDegerleri = await _paraBirimiService.GetAllKurDegerleriAsync();
            var result = new List<DovizKuru>();

            foreach (var kurDegeri in kurDegerleri.Take(count))
            {
                // KurMarj bilgisini kontrol et ve varsa kullan
                decimal alis = kurDegeri.Alis;
                decimal satis = kurDegeri.Satis;
                
                // Efektif değerleri hesapla (varsayılan olarak alış %2 düşük, satış %3 yüksek)
                decimal efektifAlis = Math.Round(alis * 0.98m, 6);
                decimal efektifSatis = Math.Round(satis * 1.03m, 6);
                
                try
                {
                    var kurMarjTask = _kurDegeriService.GetKurMarjAsync();
                    var kurMarj = Task.Run(() => kurMarjTask).Result;
                    
                    if (kurMarj != null)
                    {
                        // Satış marjı üzerinden efektif değerleri hesapla
                        decimal marjOrani = kurMarj.SatisMarji / 100m; // % olarak verildiği için 100'e bölüyoruz
                        efektifAlis = Math.Round(alis * (1 - marjOrani), 6);
                        efektifSatis = Math.Round(satis * (1 + marjOrani), 6);
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Kur marj bilgisi alınamadı, varsayılan değerler kullanılıyor.");
                }

                result.Add(new DovizKuru
                {
                    DovizKuruID = Guid.NewGuid(),
                    ParaBirimiID = kurDegeri.ParaBirimiID ?? Guid.Empty,
                    ParaBirimiKodu = kurDegeri.ParaBirimi?.Kod ?? string.Empty,
                    ParaBirimiAdi = kurDegeri.ParaBirimi?.Ad ?? string.Empty,
                    ParaBirimiSembol = kurDegeri.ParaBirimi?.Sembol ?? string.Empty,
                    Alis = kurDegeri.Alis,
                    Satis = kurDegeri.Satis,
                    EfektifAlis = efektifAlis,
                    EfektifSatis = efektifSatis,
                    Tarih = kurDegeri.Tarih,
                    GuncellemeTarihi = kurDegeri.GuncellemeTarihi ?? DateTime.Now,
                    Aktif = kurDegeri.Aktif,
                    Kaynak = "ParaBirimiService"
                });
            }

            return result;
        }

        /// <summary>
        /// Belirli bir tarih için tüm döviz kurlarını getirir
        /// </summary>
        public async Task<List<KurDegeri>> GetKurDegerleriByTarihAsync(DateTime? tarih)
        {
            // Önbellekte veriyi ara
            string cacheKey = $"{CACHE_KUR_DEGERLERI}{tarih?.ToString("yyyyMMdd") ?? "current"}";
            
            if (_cache.TryGetValue(cacheKey, out List<KurDegeri> cachedKurDegerleri))
            {
                _logger.LogInformation($"Kur değerleri önbellekten alındı: {tarih?.ToString("yyyy-MM-dd") ?? "güncel"}");
                return cachedKurDegerleri;
            }
            
            try
            {
                // Doğrudan GetAllKurDegerleriAsync metoduna DateTime? tipindeki tarih parametresini geçiyoruz
                var paraKurDegerleri = await _paraBirimiService.GetAllKurDegerleriAsync(tarih);
                var kurDegerleri = paraKurDegerleri.Select(pk => ConvertToKurDegeri(pk)).ToList();
                
                // Önbelleğe ekle
                var cacheOptions = new MemoryCacheEntryOptions()
                    .SetSlidingExpiration(TimeSpan.FromMinutes(CACHE_DURATION_MINUTES))
                    .SetAbsoluteExpiration(TimeSpan.FromHours(1));
                
                _cache.Set(cacheKey, kurDegerleri, cacheOptions);
                return kurDegerleri;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Kur değerleri getirilirken hata oluştu: {tarih?.ToString("yyyy-MM-dd") ?? "güncel"}");
                throw;
            }
        }

        public async Task<KurDegeri?> GetSonKurDegeriByKodAsync(string kod)
        {
            // Önbellekte veriyi ara
            string cacheKey = $"{CACHE_SON_KUR}{kod}";
            
            if (_cache.TryGetValue(cacheKey, out KurDegeri cachedKurDegeri))
            {
                _logger.LogInformation($"Son kur değeri önbellekten alındı: {kod}");
                return cachedKurDegeri;
            }
            
            try
            {
                var paraBirimi = await _paraBirimiService.GetParaBirimiByKodAsync(kod);
                if (paraBirimi == null)
                    return null;

                var paraKurDegeri = await _paraBirimiService.GetSonKurDegeriByParaBirimiAsync(paraBirimi.ParaBirimiID);
                if (paraKurDegeri == null)
                    return null;
                    
                var kurDegeri = ConvertToKurDegeri(paraKurDegeri);
                
                // Önbelleğe ekle
                var cacheOptions = new MemoryCacheEntryOptions()
                    .SetSlidingExpiration(TimeSpan.FromMinutes(CACHE_DURATION_MINUTES))
                    .SetAbsoluteExpiration(TimeSpan.FromHours(1));
                
                _cache.Set(cacheKey, kurDegeri, cacheOptions);
                return kurDegeri;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Son kur değeri getirilirken hata oluştu: {kod}");
                return null;
            }
        }

        public async Task<KurDegeri?> GetSonKurDegeriByParaBirimiAsync(Guid paraBirimiId)
        {
            // Önbellekte veriyi ara
            string cacheKey = $"{CACHE_SON_KUR}{paraBirimiId}";
            
            if (_cache.TryGetValue(cacheKey, out KurDegeri cachedKurDegeri))
            {
                _logger.LogInformation($"Son kur değeri önbellekten alındı: {paraBirimiId}");
                return cachedKurDegeri;
            }
            
            try
            {
                var paraKurDegeri = await _paraBirimiService.GetSonKurDegeriByParaBirimiAsync(paraBirimiId);
                if (paraKurDegeri == null)
                    return null;
                    
                var kurDegeri = ConvertToKurDegeri(paraKurDegeri);
                
                // Önbelleğe ekle
                var cacheOptions = new MemoryCacheEntryOptions()
                    .SetSlidingExpiration(TimeSpan.FromMinutes(CACHE_DURATION_MINUTES))
                    .SetAbsoluteExpiration(TimeSpan.FromHours(1));
                
                _cache.Set(cacheKey, kurDegeri, cacheOptions);
                return kurDegeri;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Son kur değeri getirilirken hata oluştu: {paraBirimiId}");
                return null;
            }
        }

        public async Task<decimal> HesaplaKurDegeriAsync(Guid kaynakParaBirimiId, Guid hedefParaBirimiId, DateTime? tarih = null)
        {
            // Aynı para birimi ise 1 dön
            if (kaynakParaBirimiId == hedefParaBirimiId)
                return 1m;
                
            // Önbellekte veriyi ara
            string cacheKey = $"{CACHE_KUR_HESAPLAMA}{kaynakParaBirimiId}_{hedefParaBirimiId}_{tarih?.ToString("yyyyMMdd") ?? "current"}";
            
            if (_cache.TryGetValue(cacheKey, out decimal cachedRate))
            {
                _logger.LogInformation($"Kur değeri önbellekten alındı: {kaynakParaBirimiId} -> {hedefParaBirimiId}");
                return cachedRate;
            }
            
            try
            {
                // Zaman aşımı kontrolü ekle (5 saniye)
                using var cts = new System.Threading.CancellationTokenSource(TimeSpan.FromSeconds(5));
                var kurTask = _paraBirimiService.HesaplaKurDegeriAsync(kaynakParaBirimiId, hedefParaBirimiId, tarih);
                
                // Task başarıyla tamamlanana kadar bekle, zaman aşımı olursa exception fırlat
                var completedTask = await Task.WhenAny(kurTask, Task.Delay(5000, cts.Token));
                if (completedTask != kurTask)
                {
                    _logger.LogWarning($"Kur hesaplama servisi zaman aşımına uğradı: {kaynakParaBirimiId} -> {hedefParaBirimiId}");
                    throw new TimeoutException($"Kur hesaplama zaman aşımına uğradı");
                }
                
                var kurDegeri = await kurTask;
                
                // Önbelleğe ekle
                var cacheOptions = new MemoryCacheEntryOptions()
                    .SetSlidingExpiration(TimeSpan.FromMinutes(CACHE_DURATION_MINUTES))
                    .SetAbsoluteExpiration(TimeSpan.FromHours(1));
                
                _cache.Set(cacheKey, kurDegeri, cacheOptions);
                return kurDegeri;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Kur hesaplanırken hata oluştu: {kaynakParaBirimiId} -> {hedefParaBirimiId}");
                throw;
            }
        }

        public async Task<decimal> HesaplaKurDegeriByKodAsync(string kaynakKod, string hedefKod, DateTime? tarih = null)
        {
            // Aynı para birimi ise 1 dön
            if (kaynakKod == hedefKod)
                return 1m;
                
            // Önbellekte veriyi ara
            string cacheKey = $"{CACHE_KUR_HESAPLAMA}{kaynakKod}_{hedefKod}_{tarih?.ToString("yyyyMMdd") ?? "current"}";
            
            if (_cache.TryGetValue(cacheKey, out decimal cachedRate))
            {
                _logger.LogInformation($"Kur değeri önbellekten alındı: {kaynakKod} -> {hedefKod}");
                return cachedRate;
            }
            
            try
            {
                var kaynakParaBirimi = await _paraBirimiService.GetParaBirimiByKodAsync(kaynakKod);
                var hedefParaBirimi = await _paraBirimiService.GetParaBirimiByKodAsync(hedefKod);

                if (kaynakParaBirimi == null || hedefParaBirimi == null)
                    throw new ArgumentException("Para birimi kodu bulunamadı.");
                
                // Zaman aşımı kontrolü ekle (5 saniye)
                using var cts = new System.Threading.CancellationTokenSource(TimeSpan.FromSeconds(5));
                var kurTask = _paraBirimiService.HesaplaKurDegeriAsync(kaynakParaBirimi.ParaBirimiID, hedefParaBirimi.ParaBirimiID, tarih);
                
                // Task başarıyla tamamlanana kadar bekle, zaman aşımı olursa exception fırlat
                var completedTask = await Task.WhenAny(kurTask, Task.Delay(5000, cts.Token));
                if (completedTask != kurTask)
                {
                    _logger.LogWarning($"Kur hesaplama servisi zaman aşımına uğradı: {kaynakKod} -> {hedefKod}");
                    throw new TimeoutException($"Kur hesaplama zaman aşımına uğradı");
                }
                
                var kurDegeri = await kurTask;
                
                // Önbelleğe ekle
                var cacheOptions = new MemoryCacheEntryOptions()
                    .SetSlidingExpiration(TimeSpan.FromMinutes(CACHE_DURATION_MINUTES))
                    .SetAbsoluteExpiration(TimeSpan.FromHours(1));
                
                _cache.Set(cacheKey, kurDegeri, cacheOptions);
                return kurDegeri;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Kur hesaplanırken hata oluştu: {kaynakKod} -> {hedefKod}");
                throw;
            }
        }

        public async Task<decimal> ParaBirimiCevirAsync(decimal tutar, string kaynakKod, string hedefKod, DateTime? tarih = null)
        {
            return await CevirmeTutarByKodAsync(tutar, kaynakKod, hedefKod, tarih);
        }

        /// <summary>
        /// Döviz kurlarını dış servislerden (örneğin TCMB) güncellemek için metod
        /// </summary>
        public async Task<bool> GuncelleKurDegerleriniFromTCMBAsync()
        {
            try
            {
                // ParaBirimiService'deki GuncelleKurDegerleriniFromAPIAsync metodu kullanılabilir
                var result = await _paraBirimiService.GuncelleKurDegerleriniFromAPIAsync();
                
                if (result)
                {
                    // Başarılı güncelleme sonrası önbelleği temizle
                    ClearCache();
                }
                
                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Döviz kurları güncellenirken hata oluştu");
                return false;
            }
        }

        /// <summary>
        /// Döviz kuru önbelleğini temizler
        /// </summary>
        public async Task<bool> ClearCacheAsync()
        {
            try 
            {
                ClearCache();
                // API'den en son kurları yeniden yükle
                await _paraBirimiService.GuncelleKurDegerleriniFromAPIAsync();
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Döviz kuru önbelleği temizlenirken hata oluştu");
                return false;
            }
        }

        /// <summary>
        /// Önbelleği temizler
        /// </summary>
        private void ClearCache()
        {
            try
            {
                _logger.LogInformation("Döviz kuru önbelleğini temizleme işlemi başlatıldı");
                
                // Basitleştirilmiş yaklaşım - doğrudan bilinen önbellek anahtarlarını temizle
                // TRY ve USD gibi standart kurlar
                _cache.Remove($"{CACHE_KUR_HESAPLAMA}TRY_USD_current");
                _cache.Remove($"{CACHE_KUR_HESAPLAMA}USD_TRY_current");
                _cache.Remove($"{CACHE_KUR_HESAPLAMA}UZS_USD_current");
                _cache.Remove($"{CACHE_KUR_HESAPLAMA}USD_UZS_current");
                
                // SonKur değerleri
                _cache.Remove($"{CACHE_SON_KUR}TRY");
                _cache.Remove($"{CACHE_SON_KUR}USD");
                _cache.Remove($"{CACHE_SON_KUR}UZS");
                
                // Güncel tarihli kurlar
                _cache.Remove($"{CACHE_KUR_DEGERLERI}current");
                
                // Bugünün tarihine göre kur değerleri
                var today = DateTime.Today.ToString("yyyyMMdd");
                _cache.Remove($"{CACHE_KUR_DEGERLERI}{today}");
                
                _logger.LogInformation("Döviz kuru önbelleği temizleme işlemi tamamlandı");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Önbellek temizlenirken hata oluştu");
            }
        }
        
        /// <summary>
        /// Belirli bir desene uyan tüm önbellek girişlerini temizler
        /// </summary>
        private void RemoveCacheByPattern(Regex pattern)
        {
            // Bu metod, reflection kullanarak IMemoryCache içindeki _entries field'ını erişerek
            // pattern'e uyan tüm cache anahtarlarını temizler
            // Not: Bu yaklaşım Microsoft'un memory cache implementasyonuna bağlıdır
            var cacheEntriesCollectionDefinition = typeof(MemoryCache).GetProperty("EntriesCollection", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            
            if (cacheEntriesCollectionDefinition != null)
            {
                var cacheEntriesCollection = cacheEntriesCollectionDefinition.GetValue(_cache) as dynamic;
                
                if (cacheEntriesCollection != null)
                {
                    List<string> keysToRemove = new List<string>();
                    
                    foreach (dynamic cacheItem in cacheEntriesCollection)
                    {
                        var cacheItemKey = Convert.ToString(cacheItem.GetType().GetProperty("Key").GetValue(cacheItem, null));
                        
                        if (pattern.IsMatch(cacheItemKey))
                        {
                            keysToRemove.Add(cacheItemKey);
                        }
                    }
                    
                    foreach (var key in keysToRemove)
                    {
                        _cache.Remove(key);
                        _logger.LogDebug($"Önbellekten temizlendi: {key}");
                    }
                    
                    _logger.LogInformation($"{keysToRemove.Count} adet önbellek girdisi temizlendi");
                }
            }
        }

        // ParaBirimiModulu.KurDegeri'yi DovizModulu.KurDegeri'ye dönüştürme yardımcı metodu
        private KurDegeri ConvertToKurDegeri(Data.Entities.ParaBirimiModulu.KurDegeri paraKurDegeri)
        {
            return new KurDegeri
            {
                KurDegeriID = paraKurDegeri.KurDegeriID,
                ParaBirimiID = paraKurDegeri.ParaBirimiID ?? Guid.Empty,
                Tarih = paraKurDegeri.Tarih,
                Alis = paraKurDegeri.Alis,
                Satis = paraKurDegeri.Satis,
                Aktif = paraKurDegeri.Aktif,
                OlusturmaTarihi = paraKurDegeri.OlusturmaTarihi,
                GuncellemeTarihi = paraKurDegeri.GuncellemeTarihi,
                OlusturanKullaniciID = paraKurDegeri.OlusturanKullaniciID,
                Aciklama = paraKurDegeri.Aciklama,
                Silindi = paraKurDegeri.Silindi
            };
        }
    }
} 