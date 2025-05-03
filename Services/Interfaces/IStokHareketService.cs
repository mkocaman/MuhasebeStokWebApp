using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using MuhasebeStokWebApp.Data.Entities;
using MuhasebeStokWebApp.ViewModels.Fatura;

namespace MuhasebeStokWebApp.Services.Interfaces
{
    public interface IStokHareketService
    {
        /// <summary>
        /// Fatura detaylarından stok hareketi oluşturur
        /// </summary>
        Task<List<StokHareket>> CreateStokHareket(Fatura fatura, List<FaturaDetay> faturaDetaylari, Guid? kullaniciId = null);
        
        /// <summary>
        /// Mevcut Fatura için stok seviyesini hesaplar
        /// </summary>
        Task UpdateStokSeviyeleri(Fatura fatura);
        
        /// <summary>
        /// Fatura detaylarından stok çıkışı yapar
        /// </summary>
        Task<decimal> StokCikisiYap(Guid urunId, decimal miktar, Guid? referansId = null, string aciklama = null);
        
        /// <summary>
        /// Fatura detaylarından stok girişi yapar
        /// </summary>
        Task<decimal> StokGirisiYap(Guid urunId, decimal miktar, Guid? referansId = null, string aciklama = null);
        
        /// <summary>
        /// Fatura silme sırasında ilişkili stok hareketlerini iptal eder
        /// </summary>
        Task IptalEtStokHareketleri(Guid faturaId);
    }
} 