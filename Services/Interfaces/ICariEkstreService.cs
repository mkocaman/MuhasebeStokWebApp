using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using MuhasebeStokWebApp.Data.Entities;
using MuhasebeStokWebApp.Models;

namespace MuhasebeStokWebApp.Services.Interfaces
{
    public interface ICariEkstreService
    {
        /// <summary>
        /// Belirli bir cari için ekstre raporu oluşturur
        /// </summary>
        /// <param name="cariId">Cari ID</param>
        /// <param name="baslangicTarihi">Başlangıç tarihi</param>
        /// <param name="bitisTarihi">Bitiş tarihi</param>
        /// <param name="paraBirimiId">İsteğe bağlı para birimi ID</param>
        /// <returns>Ekstre rapor modeli</returns>
        Task<CariEkstreViewModel> GetEkstreRaporuAsync(
            Guid cariId, 
            DateTime baslangicTarihi, 
            DateTime bitisTarihi, 
            Guid? paraBirimiId = null);
        
        /// <summary>
        /// Bakiye hesaplayarak cari ekstresi için hareketleri döndürür
        /// </summary>
        /// <param name="cariId">Cari ID</param>
        /// <param name="baslangicTarihi">Başlangıç tarihi</param>
        /// <param name="bitisTarihi">Bitiş tarihi</param>
        /// <param name="paraBirimiId">İsteğe bağlı para birimi ID</param>
        /// <returns>Kümülatif bakiye hesaplanmış hareket listesi ve toplam bakiye</returns>
        Task<(decimal Bakiye, List<CariHareketViewModel> Hareketler)> GetCariHareketleriWithBakiyeAsync(
            Guid cariId,
            DateTime baslangicTarihi,
            DateTime bitisTarihi,
            Guid? paraBirimiId = null);
        
        /// <summary>
        /// Cari ekstre raporu yazdırma işlemi için gerekli verileri hazırlar
        /// </summary>
        /// <param name="cariId">Cari ID</param>
        /// <param name="baslangicTarihi">Başlangıç tarihi</param>
        /// <param name="bitisTarihi">Bitiş tarihi</param>
        /// <param name="paraBirimiId">İsteğe bağlı para birimi ID</param>
        /// <returns>Yazdırma için hazırlanmış ekstre modeli</returns>
        Task<CariEkstreViewModel> PrepareEkstreForPrintAsync(
            Guid cariId,
            DateTime baslangicTarihi,
            DateTime bitisTarihi,
            Guid? paraBirimiId = null);
    }
} 