using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using MuhasebeStokWebApp.ViewModels.Cari;

namespace MuhasebeStokWebApp.Services.Interfaces
{
    public interface ICariEkstreService
    {
        /// <summary>
        /// Verilen cari için ekstre oluşturur.
        /// </summary>
        /// <param name="cariId">Cari ID</param>
        /// <param name="baslangicTarihi">Başlangıç tarihi</param>
        /// <param name="bitisTarihi">Bitiş tarihi</param>
        /// <param name="paraBirimiId">Para birimi ID (opsiyonel)</param>
        /// <returns>Cari ekstre modeli</returns>
        Task<CariEkstreRaporViewModel> GetCariEkstreAsync(
            Guid cariId, 
            DateTime? baslangicTarihi = null,
            DateTime? bitisTarihi = null,
            Guid? paraBirimiId = null);

        /// <summary>
        /// Cari hareket listesinden bakiye hesaplar.
        /// </summary>
        /// <param name="cariId">Cari ID</param>
        /// <param name="tarih">Tarih (belirtilen tarihe kadar olan bakiye)</param>
        /// <returns>Hesaplanan bakiye tutarı</returns>
        Task<decimal> HesaplaBakiyeAsync(Guid cariId, DateTime tarih);

        /// <summary>
        /// Cari hareketleri verilen tarih aralığına göre filtreler.
        /// </summary>
        /// <param name="cariId">Cari ID</param>
        /// <param name="baslangicTarihi">Başlangıç tarihi</param>
        /// <param name="bitisTarihi">Bitiş tarihi</param>
        /// <returns>Cari hareket listesi</returns>
        Task<List<CariEkstreRaporViewModel.CariEkstreHareketViewModel>> GetCariHareketlerAsync(
            Guid cariId,
            DateTime baslangicTarihi,
            DateTime bitisTarihi);

        /// <summary>
        /// Rapor formatında cari ekstre oluşturur.
        /// </summary>
        /// <param name="cariId">Cari ID</param>
        /// <param name="baslangicTarihi">Başlangıç tarihi</param>
        /// <param name="bitisTarihi">Bitiş tarihi</param>
        /// <param name="paraBirimiId">Para birimi ID (opsiyonel)</param>
        /// <returns>Rapor formatında cari ekstre</returns>
        Task<CariEkstreRaporViewModel> GetCariEkstreRaporAsync(
            Guid cariId,
            DateTime baslangicTarihi,
            DateTime bitisTarihi,
            Guid? paraBirimiId = null);
    }
} 