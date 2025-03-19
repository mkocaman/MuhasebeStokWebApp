using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using MuhasebeStokWebApp.Data.Entities;
using MuhasebeStokWebApp.ViewModels.Kur;

namespace MuhasebeStokWebApp.Services
{
    public interface IParaBirimiService
    {
        // Para birimi işlemleri
        Task<List<ParaBirimiViewModel>> GetAllParaBirimleriAsync();
        Task<List<ParaBirimiViewModel>> GetActiveParaBirimleriAsync();
        Task<ParaBirimiViewModel> GetParaBirimiByIdAsync(Guid id);
        Task<ParaBirimiViewModel> GetParaBirimiByKodAsync(string kod);
        Task<ParaBirimi> GetParaBirimiEntityByIdAsync(Guid id);
        Task<bool> AddParaBirimiAsync(ParaBirimiViewModel model);
        Task<bool> UpdateParaBirimiAsync(ParaBirimiViewModel model);
        Task<bool> DeleteParaBirimiAsync(Guid id);
        
        // Para birimi ilişki işlemleri
        Task<List<ParaBirimiIliskiViewModel>> GetAllParaBirimiIliskileriAsync();
        Task<List<ParaBirimiIliskiViewModel>> GetActiveParaBirimiIliskileriAsync();
        Task<ParaBirimiIliskiViewModel> GetParaBirimiIliskiByIdAsync(Guid id);
        Task<ParaBirimiIliski> GetParaBirimiIliskiEntityByIdAsync(Guid id);
        Task<ParaBirimiIliskiViewModel> GetParaBirimiIliskiByParaBirimleriAsync(Guid kaynakId, Guid hedefId);
        Task<bool> AddParaBirimiIliskiAsync(ParaBirimiIliskiViewModel model);
        Task<bool> UpdateParaBirimiIliskiAsync(ParaBirimiIliskiViewModel model);
        Task<bool> DeleteParaBirimiIliskiAsync(Guid id);
    }
} 