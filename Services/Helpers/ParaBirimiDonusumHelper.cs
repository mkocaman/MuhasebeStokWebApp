using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using MuhasebeStokWebApp.Services.Interfaces;

namespace MuhasebeStokWebApp.Services.Helpers
{
    /// <summary>
    /// Para birimi dönüşüm işlemlerini gerçekleştiren yardımcı sınıf
    /// </summary>
    public class ParaBirimiDonusumHelper : IParaBirimiDonusumHelper
    {
        private readonly IDovizKuruService _dovizKuruService;
        private readonly ILogger<ParaBirimiDonusumHelper> _logger;

        /// <summary>
        /// ParaBirimiDonusumHelper sınıfı için yeni bir örnek oluşturur
        /// </summary>
        /// <param name="dovizKuruService">Döviz kuru servisini sağlar</param>
        /// <param name="logger">Loglama servisini sağlar</param>
        public ParaBirimiDonusumHelper(IDovizKuruService dovizKuruService, ILogger<ParaBirimiDonusumHelper> logger)
        {
            _dovizKuruService = dovizKuruService ?? throw new ArgumentNullException(nameof(dovizKuruService));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <summary>
        /// Bir para biriminden USD'ye dönüşümü gerçekleştirir
        /// </summary>
        /// <param name="paraBirimi">Kaynak para birimi</param>
        /// <param name="birimFiyat">Dönüştürülecek tutar</param>
        /// <param name="dovizKuru">Manuel döviz kuru (null ise servis üzerinden alınır)</param>
        /// <returns>USD cinsinden tutar</returns>
        /// <exception cref="InvalidOperationException">Döviz kuru bulunamadığında fırlatılır</exception>
        public async Task<decimal> ToUsdAsync(string paraBirimi, decimal birimFiyat, decimal? dovizKuru = null)
        {
            if (string.IsNullOrWhiteSpace(paraBirimi))
                throw new ArgumentException("Para birimi belirtilmemiş", nameof(paraBirimi));

            if (paraBirimi == "USD")
                return birimFiyat;

            try
            {
                decimal kurToUSD = 0;
                
                if (dovizKuru.HasValue && dovizKuru.Value > 0)
                {
                    kurToUSD = dovizKuru.Value;
                    _logger.LogInformation($"{paraBirimi} -> USD için manuel kur kullanılıyor: {kurToUSD}");
                }
                else
                {
                    kurToUSD = await _dovizKuruService.GetGuncelKurAsync(paraBirimi, "USD");
                    _logger.LogInformation($"{paraBirimi} -> USD için API'den kur alındı: {kurToUSD}");
                }

                if (kurToUSD <= 0)
                {
                    _logger.LogError($"{paraBirimi} -> USD için kur bilgisi bulunamadı");
                    throw new InvalidOperationException($"{paraBirimi} para biriminden USD'ye dönüşüm için geçerli bir kur değeri bulunamadı.");
                }

                // UZS gibi büyük kurlar için böl, diğerleri için çarp
                decimal usdTutar = paraBirimi == "UZS" 
                    ? birimFiyat / kurToUSD 
                    : birimFiyat * kurToUSD;
                
                _logger.LogInformation($"{birimFiyat} {paraBirimi} = {usdTutar} USD, Kur: {kurToUSD}, İşlem: {(paraBirimi == "UZS" ? "Bölme" : "Çarpma")}");
                
                return usdTutar;
            }
            catch (Exception ex) when (!(ex is InvalidOperationException))
            {
                _logger.LogError(ex, $"{paraBirimi} -> USD dönüşümü sırasında hata oluştu");
                throw new InvalidOperationException($"{paraBirimi} para biriminden USD'ye dönüşüm sırasında bir hata oluştu: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// Bir para biriminden UZS'ye dönüşümü gerçekleştirir
        /// </summary>
        /// <param name="paraBirimi">Kaynak para birimi</param>
        /// <param name="birimFiyat">Dönüştürülecek tutar</param>
        /// <param name="usdTutar">USD cinsinden tutar (hesaplanmışsa)</param>
        /// <returns>UZS cinsinden tutar</returns>
        public async Task<decimal> ToUzsAsync(string paraBirimi, decimal birimFiyat, decimal? usdTutar = null)
        {
            if (string.IsNullOrWhiteSpace(paraBirimi))
                throw new ArgumentException("Para birimi belirtilmemiş", nameof(paraBirimi));

            if (paraBirimi == "UZS")
                return birimFiyat;

            try
            {
                // Doğrudan dönüşüm dene
                decimal kurToUZS = await _dovizKuruService.GetGuncelKurAsync(paraBirimi, "UZS");
                
                if (kurToUZS > 0)
                {
                    decimal uzsTutar = birimFiyat * kurToUZS;
                    _logger.LogInformation($"{birimFiyat} {paraBirimi} = {uzsTutar} UZS, Direkt kur: {kurToUZS}");
                    return uzsTutar;
                }
                
                // Doğrudan dönüşüm bulunamadı, USD üzerinden dönüşüm yap
                decimal usdToUZS = await _dovizKuruService.GetGuncelKurAsync("USD", "UZS");
                
                if (usdToUZS <= 0)
                {
                    _logger.LogWarning($"USD -> UZS için kur bilgisi bulunamadı. UZS değeri 0 olarak dönüyor.");
                    return 0;
                }
                
                // USD değeri verilmişse kullan, yoksa hesapla
                decimal usd = usdTutar ?? await ToUsdAsync(paraBirimi, birimFiyat);
                
                if (usd <= 0)
                {
                    _logger.LogWarning($"{paraBirimi} -> USD dönüşümü sıfır veya negatif. UZS değeri 0 olarak dönüyor.");
                    return 0;
                }
                
                decimal uzsTutarFromUsd = usd * usdToUZS;
                _logger.LogInformation($"{birimFiyat} {paraBirimi} = {uzsTutarFromUsd} UZS (USD üzerinden), USD: {usd}, USD->UZS Kur: {usdToUZS}");
                
                return uzsTutarFromUsd;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"{paraBirimi} -> UZS dönüşümü sırasında hata oluştu");
                // Kritik olmayan hata, 0 döndür
                return 0;
            }
        }

        /// <summary>
        /// Bir para biriminden başka bir para birimine dönüşümü gerçekleştirir
        /// </summary>
        /// <param name="kaynakParaBirimi">Kaynak para birimi</param>
        /// <param name="hedefParaBirimi">Hedef para birimi</param>
        /// <param name="tutar">Dönüştürülecek tutar</param>
        /// <returns>Hedef para birimi cinsinden tutar</returns>
        public async Task<decimal> ParaBirimiDonusturAsync(string kaynakParaBirimi, string hedefParaBirimi, decimal tutar)
        {
            if (string.IsNullOrWhiteSpace(kaynakParaBirimi))
                throw new ArgumentException("Kaynak para birimi belirtilmemiş", nameof(kaynakParaBirimi));
                
            if (string.IsNullOrWhiteSpace(hedefParaBirimi))
                throw new ArgumentException("Hedef para birimi belirtilmemiş", nameof(hedefParaBirimi));
                
            if (kaynakParaBirimi == hedefParaBirimi)
                return tutar;
                
            try
            {
                // Doğrudan dönüşüm dene
                decimal kur = await _dovizKuruService.GetGuncelKurAsync(kaynakParaBirimi, hedefParaBirimi);
                
                if (kur > 0)
                {
                    decimal sonuc = tutar * kur;
                    _logger.LogInformation($"{tutar} {kaynakParaBirimi} = {sonuc} {hedefParaBirimi}, Kur: {kur}");
                    return sonuc;
                }
                
                // Doğrudan dönüşüm bulunamadı, USD üzerinden dönüşüm yap
                decimal usdTutar = await ToUsdAsync(kaynakParaBirimi, tutar);
                
                if (hedefParaBirimi == "USD")
                    return usdTutar;
                    
                decimal hedefKur = await _dovizKuruService.GetGuncelKurAsync("USD", hedefParaBirimi);
                
                if (hedefKur <= 0)
                {
                    _logger.LogWarning($"USD -> {hedefParaBirimi} için kur bilgisi bulunamadı. 0 değeri dönüyor.");
                    return 0;
                }
                
                decimal sonucViaUsd = usdTutar * hedefKur;
                _logger.LogInformation($"{tutar} {kaynakParaBirimi} = {sonucViaUsd} {hedefParaBirimi} (USD üzerinden), USD: {usdTutar}, USD->{hedefParaBirimi} Kur: {hedefKur}");
                
                return sonucViaUsd;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"{kaynakParaBirimi} -> {hedefParaBirimi} dönüşümü sırasında hata oluştu");
                throw new InvalidOperationException($"{kaynakParaBirimi} para biriminden {hedefParaBirimi} para birimine dönüşüm sırasında bir hata oluştu: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// İki para birimi arasındaki güncel kur değerini alır
        /// </summary>
        /// <param name="kaynakParaBirimi">Kaynak para birimi</param>
        /// <param name="hedefParaBirimi">Hedef para birimi</param>
        /// <returns>Kur değeri</returns>
        public async Task<decimal> GetKurDegeriAsync(string kaynakParaBirimi, string hedefParaBirimi)
        {
            if (string.IsNullOrWhiteSpace(kaynakParaBirimi))
                throw new ArgumentException("Kaynak para birimi belirtilmemiş", nameof(kaynakParaBirimi));
                
            if (string.IsNullOrWhiteSpace(hedefParaBirimi))
                throw new ArgumentException("Hedef para birimi belirtilmemiş", nameof(hedefParaBirimi));
                
            if (kaynakParaBirimi == hedefParaBirimi)
                return 1m;
                
            decimal kur = await _dovizKuruService.GetGuncelKurAsync(kaynakParaBirimi, hedefParaBirimi);
            
            // Doğrudan kur bulunamadıysa, USD üzerinden hesapla
            if (kur <= 0)
            {
                decimal kaynakToUsd = await _dovizKuruService.GetGuncelKurAsync(kaynakParaBirimi, "USD");
                decimal usdToHedef = await _dovizKuruService.GetGuncelKurAsync("USD", hedefParaBirimi);
                
                if (kaynakToUsd > 0 && usdToHedef > 0)
                {
                    // UZS gibi büyük kurlar için özel hesaplama
                    if (kaynakParaBirimi == "UZS")
                        kaynakToUsd = 1 / kaynakToUsd;
                        
                    kur = kaynakToUsd * usdToHedef;
                    _logger.LogInformation($"{kaynakParaBirimi} -> {hedefParaBirimi} kuru USD üzerinden hesaplandı: {kur} (Kaynak->USD: {kaynakToUsd}, USD->Hedef: {usdToHedef})");
                }
                else
                {
                    _logger.LogWarning($"{kaynakParaBirimi} -> {hedefParaBirimi} kuru bulunamadı ve USD üzerinden hesaplanamadı");
                    kur = 0;
                }
            }
            
            return kur;
        }
    }
} 