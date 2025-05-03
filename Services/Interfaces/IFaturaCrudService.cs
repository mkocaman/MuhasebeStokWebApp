using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using MuhasebeStokWebApp.Data.Entities;
using MuhasebeStokWebApp.ViewModels.Fatura;

namespace MuhasebeStokWebApp.Services.Interfaces
{
    /// <summary>
    /// Fatura temel CRUD işlemlerini yönetir
    /// </summary>
    public interface IFaturaCrudService
    {
        /// <summary>
        /// Tüm faturaları getirir
        /// </summary>
        /// <returns>Fatura listesi</returns>
        Task<IEnumerable<Fatura>> GetAllAsync();
        
        /// <summary>
        /// ID'ye göre fatura getirir
        /// </summary>
        /// <param name="id">Fatura ID</param>
        /// <returns>Fatura</returns>
        Task<Fatura> GetByIdAsync(Guid id);
        
        /// <summary>
        /// Yeni fatura ekler
        /// </summary>
        /// <param name="fatura">Fatura</param>
        /// <returns>Eklenen fatura</returns>
        Task<Fatura> AddAsync(Fatura fatura);
        
        /// <summary>
        /// Faturayı günceller
        /// </summary>
        /// <param name="fatura">Fatura</param>
        /// <returns>Güncellenen fatura</returns>
        Task<Fatura> UpdateAsync(Fatura fatura);
        
        /// <summary>
        /// Faturayı siler
        /// </summary>
        /// <param name="id">Fatura ID</param>
        /// <returns>İşlem başarılı ise true, değilse false</returns>
        Task<bool> DeleteAsync(Guid id);
        
        /// <summary>
        /// Fatura detay view model'i getirir
        /// </summary>
        /// <param name="id">Fatura ID</param>
        /// <returns>Fatura detay view model</returns>
        Task<FaturaDetailViewModel> GetFaturaDetailViewModelAsync(Guid id);
        
        /// <summary>
        /// Tüm faturaların view model listesini getirir
        /// </summary>
        /// <returns>Fatura view model listesi</returns>
        Task<List<FaturaViewModel>> GetAllFaturaViewModelsAsync();
        
        /// <summary>
        /// Faturanın kullanımda olup olmadığını kontrol eder
        /// </summary>
        /// <param name="id">Fatura ID</param>
        /// <returns>Kullanımda ise true, değilse false</returns>
        Task<bool> IsFaturaInUseAsync(Guid id);
    }
} 