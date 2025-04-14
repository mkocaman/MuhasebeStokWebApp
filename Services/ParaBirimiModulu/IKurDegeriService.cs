using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using MuhasebeStokWebApp.Data.Entities.ParaBirimiModulu;
using MuhasebeStokWebApp.ViewModels.ParaBirimiModulu;

namespace MuhasebeStokWebApp.Services.ParaBirimiModulu
{
    public interface IKurDegeriService
    {
        Task<IEnumerable<KurDegeri>> GetAllAsync();
        Task<KurDegeri> GetByIdAsync(Guid id);
        Task<IEnumerable<KurDegeri>> GetByParaBirimiIdAsync(Guid paraBirimiId);
        Task<IEnumerable<KurDegeri>> GetLatestRatesAsync();
        Task<IEnumerable<KurDegeri>> GetByDateAsync(DateTime date);
        Task<KurDegeri> AddAsync(KurDegeriViewModel viewModel);
        Task<KurDegeri> UpdateAsync(KurDegeriViewModel viewModel);
        Task DeleteAsync(Guid id);
        Task<bool> UpdateExchangeRatesFromApiAsync();
        Task<KurDegeriViewModel> GetViewModelByIdAsync(Guid id);
        Task<IEnumerable<KurDegeriViewModel>> GetAllViewModelsAsync();
        
        // Kur marj işlemleri için metodlar
        Task<KurMarj> GetKurMarjAsync();
        Task<KurMarj> GetVarsayilanKurMarjAsync();
        Task<KurMarj> AddKurMarjAsync(KurMarj kurMarj);
        Task UpdateKurMarjAsync(KurMarj kurMarj);
        Task DeleteKurMarjAsync(Guid id);
        
        // ViewModel metotları
        Task<KurMarjViewModel> GetKurMarjViewModelAsync();
        Task<KurMarjViewModel> GetVarsayilanKurMarjViewModelAsync();
        Task<IEnumerable<KurMarjViewModel>> GetAllKurMarjViewModelsAsync();
        Task<KurMarjViewModel> AddKurMarjAsync(KurMarjViewModel viewModel);
        Task UpdateKurMarjAsync(KurMarjViewModel viewModel);
    }
} 