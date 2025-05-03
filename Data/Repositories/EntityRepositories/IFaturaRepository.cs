using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using MuhasebeStokWebApp.Data.Entities;

namespace MuhasebeStokWebApp.Data.Repositories.EntityRepositories
{
    public interface IFaturaRepository : IRepository<Fatura>
    {
        /// <summary>
        /// Faturayı tüm detaylarıyla birlikte getirir
        /// </summary>
        Task<Fatura> GetFaturaWithDetailsAsync(Guid id);

        /// <summary>
        /// Fatura numarasına göre fatura getirir
        /// </summary>
        Task<Fatura> GetFaturaByNumaraAsync(string faturaNumarasi);

        /// <summary>
        /// Cari ID'ye göre faturaları getirir
        /// </summary>
        Task<List<Fatura>> GetFaturalarByCariAsync(Guid cariId);

        /// <summary>
        /// Belirli tarih aralığındaki faturaları getirir
        /// </summary>
        Task<List<Fatura>> GetFaturalarByDateRangeAsync(DateTime startDate, DateTime endDate);

        /// <summary>
        /// Ödenmemiş faturaları getirir
        /// </summary>
        Task<List<Fatura>> GetOdenmemisFaturalarAsync();

        /// <summary>
        /// Vadesi geçmiş faturaları getirir
        /// </summary>
        Task<List<Fatura>> GetVadesiGecmisFaturalarAsync();

        /// <summary>
        /// Fatura türüne göre faturaları getirir
        /// </summary>
        Task<List<Fatura>> GetFaturalarByFaturaTuruAsync(string faturaTuru);

        /// <summary>
        /// Belirtilen arama terimine göre faturaları filtreler
        /// </summary>
        Task<List<Fatura>> SearchFaturalarAsync(string searchTerm);

        /// <summary>
        /// Faturaya ilişkin detayları getirir
        /// </summary>
        Task<List<FaturaDetay>> GetFaturaDetaylarAsync(Guid faturaId);

        // Faturaları ilişkili tablolarla birlikte getirir
        Task<IEnumerable<Fatura>> GetAllWithIncludesAsync();
        
        // Belirli bir faturayı ilişkili tablolarla birlikte getirir
        Task<Fatura> GetByIdWithIncludesAsync(Guid id);
        
        // Diğer metotlar
        Task<IEnumerable<Fatura>> GetFaturaByDateRangeAsync(DateTime startDate, DateTime endDate);
        Task<IEnumerable<Fatura>> GetFaturaByCariIdAsync(Guid cariId);
        Task<IEnumerable<Fatura>> GetUnpaidFaturasAsync();
        Task<decimal> GetFaturaTotalAmountByDateRangeAsync(DateTime startDate, DateTime endDate);
        Task<IEnumerable<Fatura>> SearchFaturaAsync(string searchTerm);
    }
} 