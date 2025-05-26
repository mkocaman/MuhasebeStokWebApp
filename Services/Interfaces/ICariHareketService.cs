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
        /// Fatura için cari hareket kaydı oluşturur
        /// </summary>
        Task<CariHareket> CreateFromFaturaAsync(Fatura fatura, Guid? kullaniciId);

        /// <summary>
        /// Fatura silme işlemi için cari hareket kaydını iptal eder
        /// </summary>
        Task<bool> IptalEtCariHareketAsync(Guid faturaId);

        /// <summary>
        /// ID'ye göre cari hareket getirir
        /// </summary>
        Task<CariHareket> GetByIdAsync(Guid id);

        /// <summary>
        /// Cari ID'ye göre hareketleri getirir
        /// </summary>
        Task<IEnumerable<CariHareket>> GetByCariIdAsync(Guid cariId);

        /// <summary>
        /// Cari hareket kaydını günceller
        /// </summary>
        Task<CariHareket> UpdateHareketAsync(CariHareket hareket);

        /// <summary>
        /// Cari hareket kaydını siler (soft delete)
        /// </summary>
        Task<bool> DeleteHareketAsync(Guid id);

        /// <summary>
        /// Referans bilgilerine göre cari hareket kaydını bulur (KasaHareket, BankaHareket vs.)
        /// </summary>
        Task<CariHareket> GetByReferenceAsync(Guid referansId, string referansTuru);

        /// <summary>
        /// Kasa hareketi silme işlemi için bağlı cari hareket kaydını iptal eder
        /// </summary>
        Task<bool> IptalEtCariHareketFromKasaAsync(Guid kasaHareketId);

        /// <summary>
        /// Banka hareketi silme işlemi için bağlı cari hareket kaydını iptal eder
        /// </summary>
        Task<bool> IptalEtCariHareketFromBankaAsync(Guid bankaHareketId);
    }
} 