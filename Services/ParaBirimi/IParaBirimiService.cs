using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using MuhasebeStokWebApp.Data.Entities;

namespace MuhasebeStokWebApp.Services
{
    public interface IParaBirimiService
    {
        Task<List<ParaBirimi>> GetAllParaBirimleriAsync();
        Task<List<ParaBirimi>> GetAktifParaBirimleriAsync();
        Task<ParaBirimi> GetParaBirimiByIdAsync(Guid paraBirimiId);
        Task<ParaBirimi> GetParaBirimiByKodAsync(string kod);
        Task<ParaBirimi> AddParaBirimiAsync(ParaBirimi paraBirimi);
        Task<ParaBirimi> UpdateParaBirimiAsync(ParaBirimi paraBirimi);
        Task<bool> DeleteParaBirimiAsync(Guid paraBirimiId);
        
        Task<List<ParaBirimiIliski>> GetAllParaBirimiIliskileriAsync();
        Task<List<ParaBirimiIliski>> GetAktifParaBirimiIliskileriAsync();
        Task<ParaBirimiIliski> GetParaBirimiIliskiByIdAsync(Guid paraBirimiIliskiId);
        Task<ParaBirimiIliski> UpdateParaBirimiIliskiAsync(ParaBirimiIliski paraBirimiIliski);
        Task<bool> DeleteParaBirimiIliskiAsync(Guid paraBirimiIliskiId);
        
        Task<ParaBirimiIliski> AddParaBirimiIliskiAsync(ParaBirimiIliski paraBirimiIliski);
        Task<List<ParaBirimiIliski>> GetParaBirimiIliskileriAsync(Guid paraBirimiId);
        Task<bool> UpdateParaBirimiSiralamaAsync(List<Guid> paraBirimiIdListesi);
        Task<bool> VarsayilanParaBirimleriniEkleAsync();
        Task<ParaBirimiIliski> GetIliskiByParaBirimleriAsync(Guid kaynakId, Guid hedefId);
        Task<bool> HasParaBirimiIliskiAsync(Guid paraBirimiId);
    }
} 