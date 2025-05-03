using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using MuhasebeStokWebApp.Data.Entities;

namespace MuhasebeStokWebApp.Services.Interfaces
{
    /// <summary>
    /// Stok sorgulama işlemlerini yöneten servis arayüzü
    /// </summary>
    public interface IStokSorguService
    {
        /// <summary>
        /// Ortalama maliyet hesaplar
        /// </summary>
        /// <param name="urunID">Ürün ID</param>
        /// <param name="paraBirimi">Para birimi (varsayılan: USD)</param>
        /// <returns>Ortalama maliyet</returns>
        Task<decimal> GetOrtalamaMaliyet(Guid urunID, string paraBirimi = "USD");
        
        /// <summary>
        /// Aktif FIFO kayıtlarını getirir
        /// </summary>
        /// <param name="urunID">Ürün ID</param>
        /// <returns>Aktif FIFO kayıtları</returns>
        Task<List<StokFifo>> GetAktifFifoKayitlari(Guid urunID);
        
        /// <summary>
        /// Referansa göre FIFO kayıtlarını getirir
        /// </summary>
        /// <param name="referansID">Referans ID</param>
        /// <param name="referansTuru">Referans türü</param>
        /// <returns>FIFO kayıtları</returns>
        Task<List<StokFifo>> GetReferansaGoreFifoKayitlari(Guid referansID, string referansTuru);
        
        /// <summary>
        /// Belirlii bir tarihte stok maliyetini hesaplar
        /// </summary>
        /// <param name="stokId">Stok ID</param>
        /// <param name="tarih">Hesaplama tarihi</param>
        /// <param name="paraBirimi">Para birimi (varsayılan: USD)</param>
        /// <returns>Hesaplanan maliyet</returns>
        Task<decimal> HesaplaMaliyetAsync(Guid stokId, DateTime tarih, string paraBirimi = "USD");
    }
} 