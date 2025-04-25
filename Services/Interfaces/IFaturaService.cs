using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using MuhasebeStokWebApp.Data.Entities;
using MuhasebeStokWebApp.ViewModels.Fatura;

namespace MuhasebeStokWebApp.Services.Interfaces
{
    public interface IFaturaService
    {
        Task<IEnumerable<Fatura>> GetAllAsync();
        Task<Fatura> GetByIdAsync(Guid id);
        Task<Fatura> AddAsync(Fatura fatura);
        Task<Fatura> UpdateAsync(Fatura fatura);
        Task<bool> DeleteAsync(Guid id);
        
        Task<FaturaDetailViewModel> GetFaturaDetailViewModelAsync(Guid id);
        Task<List<FaturaViewModel>> GetAllFaturaViewModelsAsync();
        Task<bool> IsFaturaInUseAsync(Guid id);
    }
} 