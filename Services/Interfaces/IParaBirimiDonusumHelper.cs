using System.Threading.Tasks;

namespace MuhasebeStokWebApp.Services.Interfaces
{
    /// <summary>
    /// Para birimi dönüşüm işlemlerini sağlayan arabirim
    /// </summary>
    public interface IParaBirimiDonusumHelper
    {
        /// <summary>
        /// Bir para biriminden USD'ye dönüşümü gerçekleştirir
        /// </summary>
        /// <param name="paraBirimi">Kaynak para birimi</param>
        /// <param name="birimFiyat">Dönüştürülecek tutar</param>
        /// <param name="dovizKuru">Manuel döviz kuru (null ise servis üzerinden alınır)</param>
        /// <returns>USD cinsinden tutar</returns>
        Task<decimal> ToUsdAsync(string paraBirimi, decimal birimFiyat, decimal? dovizKuru = null);

        /// <summary>
        /// Bir para biriminden UZS'ye dönüşümü gerçekleştirir
        /// </summary>
        /// <param name="paraBirimi">Kaynak para birimi</param>
        /// <param name="birimFiyat">Dönüştürülecek tutar</param>
        /// <param name="usdTutar">USD cinsinden tutar (hesaplanmışsa)</param>
        /// <returns>UZS cinsinden tutar</returns>
        Task<decimal> ToUzsAsync(string paraBirimi, decimal birimFiyat, decimal? usdTutar = null);

        /// <summary>
        /// Bir para biriminden başka bir para birimine dönüşümü gerçekleştirir
        /// </summary>
        /// <param name="kaynakParaBirimi">Kaynak para birimi</param>
        /// <param name="hedefParaBirimi">Hedef para birimi</param>
        /// <param name="tutar">Dönüştürülecek tutar</param>
        /// <returns>Hedef para birimi cinsinden tutar</returns>
        Task<decimal> ParaBirimiDonusturAsync(string kaynakParaBirimi, string hedefParaBirimi, decimal tutar);

        /// <summary>
        /// İki para birimi arasındaki güncel kur değerini alır
        /// </summary>
        /// <param name="kaynakParaBirimi">Kaynak para birimi</param>
        /// <param name="hedefParaBirimi">Hedef para birimi</param>
        /// <returns>Kur değeri</returns>
        Task<decimal> GetKurDegeriAsync(string kaynakParaBirimi, string hedefParaBirimi);
    }
} 