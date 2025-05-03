using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using MuhasebeStokWebApp.Data.Entities;

namespace MuhasebeStokWebApp.Data.Repositories.EntityRepositories
{
    public interface ICariRepository : IRepository<Cari>
    {
        /// <summary>
        /// Cariyi tüm ilişkili verilerle birlikte getirir
        /// </summary>
        Task<Cari> GetCariWithDetailsAsync(Guid id);

        /// <summary>
        /// Cari koduna göre cari bilgisini getirir
        /// </summary>
        Task<Cari> GetCariByKodAsync(string cariKodu);

        /// <summary>
        /// Belirtilen cari tipine göre carileri getirir
        /// </summary>
        Task<List<Cari>> GetCarilerByTipAsync(string cariTipi);

        /// <summary>
        /// Belirtilen ile göre carileri getirir
        /// </summary>
        Task<List<Cari>> GetCarilerByIlAsync(string il);

        /// <summary>
        /// Vadesi geçmiş bakiyesi olan carileri getirir
        /// </summary>
        Task<List<Cari>> GetVadesiGecmisBakiyesiOlanCarilerAsync();

        /// <summary>
        /// Belirtilen arama terimine göre carileri filtreler
        /// </summary>
        Task<List<Cari>> SearchCarilerAsync(string searchTerm);

        /// <summary>
        /// Cariye ilişkin hareketleri getirir
        /// </summary>
        Task<List<CariHareket>> GetCariHareketlerAsync(Guid cariId);
    }
} 