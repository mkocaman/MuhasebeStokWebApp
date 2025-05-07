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
            try
            {
                // Aynı para birimi ise tutarı doğrudan döndür
                if (string.Equals(kaynakKod, hedefKod, StringComparison.OrdinalIgnoreCase))
                {
                    return tutar;
                }

                // Kodları standartlaştır
                kaynakKod = kaynakKod?.ToUpperInvariant() ?? "TRY";
                hedefKod = hedefKod?.ToUpperInvariant() ?? "TRY";

                // TRY özel durumu için kontrol
                if (kaynakKod == "TRY" || hedefKod == "TRY")
                {
                    // TRY için para birimi kaydı olmayabileceğinden önbellekte veya API'de kayıt aramak yerine
                    // doğrudan kur hesapla
                    var tryKurDegeri = await HesaplaKurDegeriByKodAsync(kaynakKod, hedefKod, tarih);
                    return decimal.Round(tutar * tryKurDegeri, 4);
                }

                // Normal kur çevrimi
                var kaynakParaBirimi = await _paraBirimiService.GetParaBirimiByKodAsync(kaynakKod);
                var hedefParaBirimi = await _paraBirimiService.GetParaBirimiByKodAsync(hedefKod);

                if (kaynakParaBirimi == null)
                {
                    _logger.LogWarning($"Kaynak para birimi kodu bulunamadı: {kaynakKod}. Varsayılan değer kullanılıyor.");
                    return tutar; // Para birimi bulunamadığında dönüşüm yapmadan orijinal değeri döndür
                }

                if (hedefParaBirimi == null)
                {
                    _logger.LogWarning($"Hedef para birimi kodu bulunamadı: {hedefKod}. Varsayılan değer kullanılıyor.");
                    return tutar; // Para birimi bulunamadığında dönüşüm yapmadan orijinal değeri döndür
                }

                // Önbellekte kur değeri var mı diye kontrol et
                string cacheKey = $"{CACHE_KUR_HESAPLAMA}{kaynakKod}_{hedefKod}_{tarih?.ToString("yyyyMMdd") ?? "current"}_{tutar}";
                
                if (_cache.TryGetValue(cacheKey, out decimal cachedAmount))
                {
                    _logger.LogInformation($"Çevrilen tutar önbellekten alındı: {kaynakKod} -> {hedefKod}, Tutar: {tutar}, Sonuç: {cachedAmount}");
                    return cachedAmount;
                }

                // Kur değerini hesapla
                decimal kurDegeri = await GetGuncelKurAsync(kaynakKod, hedefKod, tarih);
                
                // Tutarı çevir
                decimal cevrilenTutar = decimal.Round(tutar * kurDegeri, 4);
                
                // Sonucu önbelleğe al
                var cacheOptions = new MemoryCacheEntryOptions()
                    .SetAbsoluteExpiration(TimeSpan.FromHours(1));
                
                _cache.Set(cacheKey, cevrilenTutar, cacheOptions);
                
                _logger.LogInformation($"Çevrilen tutar hesaplandı: {kaynakKod} -> {hedefKod}, Tutar: {tutar}, Sonuç: {cevrilenTutar}, Kur: {kurDegeri}");
                
                return cevrilenTutar;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Tutar çevrilirken hata oluştu. Tutar: {tutar}, Kaynak: {kaynakKod}, Hedef: {hedefKod}, Tarih: {tarih?.ToString("yyyy-MM-dd") ?? "Current"}");
                return tutar; // Hata durumunda orijinal tutarı döndür
            }
        }

        public async Task<decimal> GetGuncelKurAsync(string kaynakKod, string hedefKod, DateTime? tarih = null)
        {
            try
            {
                // Giriş değerlerini normalleştir
                kaynakKod = kaynakKod?.ToUpperInvariant() ?? "TRY";
                hedefKod = hedefKod?.ToUpperInvariant() ?? "TRY";
                
                // Aynı para birimi ise 1 dön
                if (kaynakKod == hedefKod)
                    return 1m;
                    
                // Önbellekte veriyi ara
                string cacheKey = $"{CACHE_KUR_HESAPLAMA}{kaynakKod}_{hedefKod}_{tarih?.ToString("yyyyMMdd") ?? "current"}";
                
                if (_cache.TryGetValue(cacheKey, out decimal cachedRate))
                {
                    _logger.LogInformation($"Kur değeri önbellekten alındı: {kaynakKod} -> {hedefKod}, Değer: {cachedRate}");
                    return cachedRate;
                }

                // TRY özel durumu
                if (kaynakKod == "TRY" || hedefKod == "TRY")
                {
                    // TRY durumunda kur hesapla - HesaplaKurDegeriByKodAsync kullanarak
                    var kurDegeri = await HesaplaKurDegeriByKodAsync(kaynakKod, hedefKod, tarih);
                    
                    // Değeri önbelleğe al
                    var cacheOptions = new MemoryCacheEntryOptions()
                        .SetAbsoluteExpiration(TimeSpan.FromHours(1));
                    
                    _cache.Set(cacheKey, kurDegeri, cacheOptions);
                    
                    _logger.LogInformation($"TRY dönüşümü için kur değeri hesaplandı: {kaynakKod} -> {hedefKod}, Değer: {kurDegeri}");
                    return kurDegeri;
                }
                
                // TRY dışındaki para birimleri için çapraz kur hesaplaması
                decimal rate = await HesaplaKurDegeriByKodAsync(kaynakKod, hedefKod, tarih);
                
                // Değeri önbelleğe al
                var options = new MemoryCacheEntryOptions()
                    .SetAbsoluteExpiration(TimeSpan.FromHours(1));
                
                _cache.Set(cacheKey, rate, options);
                
                _logger.LogInformation($"Kur değeri hesaplandı: {kaynakKod} -> {hedefKod}, Değer: {rate}");
                
                return rate;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Kur değeri alınırken hata oluştu. Kaynak: {kaynakKod}, Hedef: {hedefKod}, Tarih: {tarih?.ToString("yyyy-MM-dd") ?? "Current"}");
                
                // Hata durumunda varsayılan 1 değerini döndür
                _logger.LogWarning($"Kur değeri alınırken hata oluştu, varsayılan değer (1) kullanılıyor. Kaynak: {kaynakKod}, Hedef: {hedefKod}");
                return 1m;
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
            try
            {
                // Giriş değerlerini normalleştir
                kaynakKod = kaynakKod?.ToUpperInvariant() ?? "TRY";
                hedefKod = hedefKod?.ToUpperInvariant() ?? "TRY";
                
                // Aynı para birimi ise 1 döndür
                if (string.Equals(kaynakKod, hedefKod, StringComparison.OrdinalIgnoreCase))
                {
                    return 1m;
                }

                // TRY özel durumu - TRY için varsayılan değerler
                if (kaynakKod == "TRY" || hedefKod == "TRY")
                {
                    // TRY'den USD'ye veya USD'den TRY'ye dönüşüm için sabit oranlar
                    // Bu değerler yerine gerçek servislerden alınan değerler kullanılmalıdır
                    if (kaynakKod == "TRY" && hedefKod == "USD")
                        return 0.03m; // Örnek TRY → USD dönüşüm oranı
                    else if (kaynakKod == "USD" && hedefKod == "TRY")
                        return 33.33m; // Örnek USD → TRY dönüşüm oranı
                    
                    // Diğer para birimleri için USD üzerinden çapraz kur hesapla
                    try {
                        // Yerel para birimi kontrolü
                        var tryParaBirimi = await _paraBirimiService.GetParaBirimiByKodAsync("TRY");
                        
                        if (tryParaBirimi == null)
                        {
                            // TRY para birimi bulunamadıysa, varsayılan değerleri kullan
                            _logger.LogWarning("TRY para birimi bulunamadı, varsayılan kur değerleri kullanılıyor.");
                            return kaynakKod == "TRY" ? 0.03m : 33.33m;
                        }
                        
                        // USD kullanarak çapraz kur hesapla
                        if (kaynakKod == "TRY")
                        {
                            // TRY → Hedef Kur Hesaplaması: 1 / (USD → TRY) * (USD → Hedef)
                            var usdToTry = 33.33m; // Varsayılan USD → TRY değeri
                            var usdToHedef = await GetOrEstimateKur("USD", hedefKod, tarih);
                            return (1 / usdToTry) * usdToHedef;
                        }
                        else
                        {
                            // Kaynak → TRY Kur Hesaplaması: (Kaynak → USD) * (USD → TRY)
                            var kaynakToUsd = await GetOrEstimateKur(kaynakKod, "USD", tarih);
                            var usdToTry = 33.33m; // Varsayılan USD → TRY değeri
                            return kaynakToUsd * usdToTry;
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "TRY kur hesaplama hatası. Varsayılan değerler kullanılıyor.");
                        return kaynakKod == "TRY" ? 0.03m : 33.33m;
                    }
                }

                // Normal kur hesaplaması için para birimlerini bul
                var kaynakParaBirimi = await _paraBirimiService.GetParaBirimiByKodAsync(kaynakKod);
                var hedefParaBirimi = await _paraBirimiService.GetParaBirimiByKodAsync(hedefKod);

                // Para birimleri bulunamadıysa hata fırlatmak yerine varsayılan değer döndür
                if (kaynakParaBirimi == null)
                {
                    _logger.LogWarning($"Kaynak para birimi kodu bulunamadı: {kaynakKod}. Varsayılan değer kullanılıyor.");
                    return 1m;
                }

                if (hedefParaBirimi == null)
                {
                    _logger.LogWarning($"Hedef para birimi kodu bulunamadı: {hedefKod}. Varsayılan değer kullanılıyor.");
                    return 1m;
                }

                // Para birimlerinin Guid değerlerini al
                Guid kaynakId = kaynakParaBirimi.ParaBirimiID;
                Guid hedefId = hedefParaBirimi.ParaBirimiID;

                // ParaBirimiService üzerinden kur değerini hesapla
                var kurDegeri = await _paraBirimiService.HesaplaKurDegeriAsync(kaynakId, hedefId, tarih);
                return kurDegeri;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Kur hesaplanırken hata oluştu: {kaynakKod} -> {hedefKod}");
                return 1m; // Hata durumunda varsayılan değer
            }
        }

        // Yardımcı metod: Kur değerini ya servisden alır ya da tahmin eder
        private async Task<decimal> GetOrEstimateKur(string kaynakKod, string hedefKod, DateTime? tarih)
        {
            try
            {
                var kaynakParaBirimi = await _paraBirimiService.GetParaBirimiByKodAsync(kaynakKod);
                var hedefParaBirimi = await _paraBirimiService.GetParaBirimiByKodAsync(hedefKod);
                
                if (kaynakParaBirimi != null && hedefParaBirimi != null)
                {
                    return await _paraBirimiService.HesaplaKurDegeriAsync(
                        kaynakParaBirimi.ParaBirimiID, 
                        hedefParaBirimi.ParaBirimiID, 
                        tarih);
                }
                
                // Para birimleri bulunamadığında varsayılan tahminler
                if (kaynakKod == "USD" && hedefKod == "EUR") return 0.92m;
                if (kaynakKod == "EUR" && hedefKod == "USD") return 1.09m;
                if (kaynakKod == "USD" && hedefKod == "GBP") return 0.78m;
                if (kaynakKod == "GBP" && hedefKod == "USD") return 1.28m;
                
                return 1m; // Bilinmeyen durumlar için varsayılan
            }
            catch
            {
                return 1m; // Hata durumunda varsayılan
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