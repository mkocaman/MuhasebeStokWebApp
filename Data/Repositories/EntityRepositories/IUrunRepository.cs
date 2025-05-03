using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using MuhasebeStokWebApp.Data.Entities;

namespace MuhasebeStokWebApp.Data.Repositories.EntityRepositories
{
    public interface IUrunRepository : IRepository<Urun>
    {
        /// <summary>
        /// Ürünü tüm ilişkili verilerle birlikte getirir
        /// </summary>
        Task<Urun> GetUrunWithDetailsAsync(Guid id);

        /// <summary>
        /// Ürün koduna göre ürün bilgisini getirir
        /// </summary>
        Task<Urun> GetUrunByKodAsync(string urunKodu);

        /// <summary>
        /// Kategori ID'sine göre ürünleri getirir
        /// </summary>
        Task<List<Urun>> GetUrunlerByKategoriAsync(Guid kategoriId);

        /// <summary>
        /// Stok miktarı belirtilen değerin altında olan ürünleri getirir
        /// </summary>
        Task<List<Urun>> GetDusukStokluUrunlerAsync(decimal minStokMiktari);

        /// <summary>
        /// Belirtilen arama terimine göre ürünleri filtreler
        /// </summary>
        Task<List<Urun>> SearchUrunlerAsync(string searchTerm);
    }
} 