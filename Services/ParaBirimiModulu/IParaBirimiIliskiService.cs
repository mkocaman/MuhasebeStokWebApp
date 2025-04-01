using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using MuhasebeStokWebApp.Data.Entities.ParaBirimiModulu;
using MuhasebeStokWebApp.ViewModels.ParaBirimiModulu;

namespace MuhasebeStokWebApp.Services.ParaBirimiModulu
{
    public interface IParaBirimiIliskiService
    {
        Task<IEnumerable<ParaBirimiIliski>> GetAllAsync();
        Task<ParaBirimiIliski> GetByIdAsync(Guid id);
        Task<IEnumerable<ParaBirimiIliski>> GetByKaynakParaBirimiIdAsync(Guid kaynakParaBirimiId);
        Task<IEnumerable<ParaBirimiIliski>> GetByHedefParaBirimiIdAsync(Guid hedefParaBirimiId);
        Task<ParaBirimiIliski> AddAsync(ParaBirimiIliski paraBirimiIliski);
        Task UpdateAsync(ParaBirimiIliski paraBirimiIliski);
        Task DeleteAsync(Guid id);
        Task<ParaBirimiIliskiViewModel> GetViewModelByIdAsync(Guid id);
        Task<IEnumerable<ParaBirimiIliskiViewModel>> GetAllViewModelsAsync();
    }
} 