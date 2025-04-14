using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using MuhasebeStokWebApp.Data.Entities;

namespace MuhasebeStokWebApp.Services.Interfaces
{
    public interface ICariHareketService
    {
        /// <summary>
        /// Cari hareket kaydı oluşturur
        /// </summary>
        Task<CariHareket> CreateHareketAsync(CariHareket hareket);

        /// <summary>
        /// Kasa hareketi için cari hareket kaydı oluşturur
        /// </summary>
        Task<CariHareket> CreateFromKasaHareketAsync(KasaHareket kasaHareket, bool borcMu);

        /// <summary>
        /// Banka hareketi için cari hareket kaydı oluşturur
        /// </summary>
        Task<CariHareket> CreateFromBankaHareketAsync(BankaHesapHareket bankaHareket, bool borcMu);

        /// <summary>
        /// ID'ye göre cari hareket getirir
        /// </summary>
        Task<CariHareket> GetByIdAsync(Guid id);

        /// <summary>
        /// Cari ID'ye göre hareketleri getirir
        /// </summary>
        Task<IEnumerable<CariHareket>> GetByCariIdAsync(Guid cariId);
    }
} 