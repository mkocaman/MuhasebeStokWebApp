using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using MuhasebeStokWebApp.Data.Entities;

namespace MuhasebeStokWebApp.Services.Interfaces
{
    public interface IBirimService
    {
        Task<IEnumerable<Birim>> GetAllAsync();
        Task<Birim> GetByIdAsync(Guid id);
        Task<Birim> AddAsync(Birim birim);
        Task<Birim> UpdateAsync(Birim birim);
        Task<bool> DeleteAsync(Guid id);
        Task<bool> IsBirimInUseAsync(Guid id);
        Task<bool> IsBirimNameExistsAsync(string birimAdi, Guid? excludeBirimId = null);
        Task<bool> ClearCacheAsync();
    }
} 