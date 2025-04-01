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
        Task<KurDegeri> AddAsync(KurDegeri kurDegeri);
        Task UpdateAsync(KurDegeri kurDegeri);
        Task DeleteAsync(Guid id);
        Task<bool> UpdateExchangeRatesFromApiAsync();
        Task<KurDegeriViewModel> GetViewModelByIdAsync(Guid id);
        Task<IEnumerable<KurDegeriViewModel>> GetAllViewModelsAsync();
    }
} 