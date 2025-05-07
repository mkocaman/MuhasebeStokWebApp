using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using MuhasebeStokWebApp.Data.Entities;
using MuhasebeStokWebApp.Models;

namespace MuhasebeStokWebApp.Services.Interfaces
{
    public interface ICariService
    {
        Task<IEnumerable<Cari>> GetAllAsync();
        Task<Cari> GetByIdAsync(Guid id);
        Task<Cari> AddAsync(Cari cari);
        Task<Cari> UpdateAsync(Cari cari);
        Task DeleteAsync(Cari cari);
        
        /// <summary>
        /// Belirtilen carinin bakiyesini hesaplar
        /// </summary>
        /// <param name="cariId">Cari ID</param>
        /// <param name="tarih">İsteğe bağlı tarih sınırı (bu tarihe kadar hesaplanır)</param>
        /// <param name="paraBirimiId">İsteğe bağlı para birimi ID</param>
        /// <returns>Hesaplanan bakiye değeri</returns>
        Task<decimal> CariBakiyeHesaplaAsync(Guid cariId, DateTime? tarih = null, Guid? paraBirimiId = null);
        
        /// <summary>
        /// Cari ekstresi için kümülatif hareketleri hesaplar ve döndürür
        /// </summary>
        /// <param name="cariId">Cari ID</param>
        /// <param name="baslangicTarihi">Başlangıç tarihi</param>
        /// <param name="bitisTarihi">Bitiş tarihi</param>
        /// <param name="paraBirimiId">İsteğe bağlı para birimi ID</param>
        /// <returns>Kümülatif bakiye hesaplanmış cari hareket listesi</returns>
        Task<(decimal BakiyeToplamı, List<Data.Entities.CariHareket> Hareketler)> GetCariEkstreAsync(
            Guid cariId, 
            DateTime? baslangicTarihi = null, 
            DateTime? bitisTarihi = null, 
            Guid? paraBirimiId = null);
            
        /// <summary>
        /// Cari detay bilgilerini getirir
        /// </summary>
        Task<CariDetayViewModel> GetCariDetaylar(Guid cariID);
        
        /// <summary>
        /// Tüm aktif carileri listeler
        /// </summary>
        Task<List<CariListModel>> GetAllActiveCariler();
        
        /// <summary>
        /// Tüm pasif carileri listeler
        /// </summary>
        Task<List<CariListModel>> GetAllPasifCariler();
        
        /// <summary>
        /// Silinmiş carileri listeler
        /// </summary>
        Task<List<CariListModel>> GetSilinmisCariler();
        
        /// <summary>
        /// Aktif müşterileri listeler
        /// </summary>
        Task<List<CariListModel>> GetMusteriler();
        
        /// <summary>
        /// Aktif tedarikçileri listeler
        /// </summary>
        Task<List<CariListModel>> GetTedarikciler();
        
        /// <summary>
        /// Pasif müşterileri listeler
        /// </summary>
        Task<List<CariListModel>> GetPasifMusteriler();
        
        /// <summary>
        /// Pasif tedarikçileri listeler
        /// </summary>
        Task<List<CariListModel>> GetPasifTedarikciler();
    }
} 