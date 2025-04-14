using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using MuhasebeStokWebApp.Data.Entities;

namespace MuhasebeStokWebApp.Services.Interfaces
{
    /// <summary>
    /// Maliyet hesaplama işlemlerini yöneten servis arayüzü
    /// </summary>
    public interface IMaliyetHesaplamaService
    {
        /// <summary>
        /// Belirli bir ürün için ortalama maliyet hesaplar
        /// </summary>
        /// <param name="urunID">Ürün ID</param>
        /// <param name="paraBirimi">Para birimi (USD, TRY, UZS)</param>
        /// <returns>Ortalama maliyet değeri</returns>
        Task<decimal> GetOrtalamaMaliyetAsync(Guid urunID, string paraBirimi = "USD");
        
        /// <summary>
        /// Belirli bir tutar için para birimi dönüşümü yapar
        /// </summary>
        /// <param name="deger">Dönüştürülecek değer</param>
        /// <param name="kaynakParaBirimi">Kaynak para birimi</param>
        /// <param name="hedefParaBirimi">Hedef para birimi</param>
        /// <returns>Dönüştürülmüş değer</returns>
        Task<decimal> ParaBirimiCevirAsync(decimal deger, string kaynakParaBirimi, string hedefParaBirimi);
        
        /// <summary>
        /// USD para birimi cinsinden birim fiyat hesaplar
        /// </summary>
        /// <param name="birimFiyat">Orijinal birim fiyat</param>
        /// <param name="paraBirimi">Para birimi</param>
        /// <param name="dovizKuru">Döviz kuru (Belirtilmezse güncel kur alınır)</param>
        /// <returns>USD cinsinden birim fiyat</returns>
        Task<decimal> HesaplaUsdBirimFiyatAsync(decimal birimFiyat, string paraBirimi, decimal? dovizKuru = null);
        
        /// <summary>
        /// TL para birimi cinsinden birim fiyat hesaplar
        /// </summary>
        /// <param name="usdBirimFiyat">USD cinsinden birim fiyat</param>
        /// <returns>TL cinsinden birim fiyat</returns>
        Task<decimal> HesaplaTlBirimFiyatAsync(decimal usdBirimFiyat);
        
        /// <summary>
        /// UZS para birimi cinsinden birim fiyat hesaplar
        /// </summary>
        /// <param name="usdBirimFiyat">USD cinsinden birim fiyat</param>
        /// <returns>UZS cinsinden birim fiyat</returns>
        Task<decimal> HesaplaUzsBirimFiyatAsync(decimal usdBirimFiyat);
        
        /// <summary>
        /// FIFO kayıtları üzerinden toplam maliyet hesaplar
        /// </summary>
        /// <param name="fifoKayitlari">FIFO kayıtları</param>
        /// <param name="paraBirimi">Para birimi</param>
        /// <returns>Toplam maliyet</returns>
        decimal HesaplaToplamMaliyet(IEnumerable<StokFifo> fifoKayitlari, string paraBirimi = "USD");
    }
} 