using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using MuhasebeStokWebApp.Data.Entities;

namespace MuhasebeStokWebApp.Services.Interfaces
{
    public interface ICariService
    {
        Task<IEnumerable<Cari>> GetAllAsync();
        Task<Cari> GetByIdAsync(Guid id);
        Task<Cari> AddAsync(Cari cari);
        Task<Cari> UpdateAsync(Cari cari);
        Task DeleteAsync(Cari cari);
    }
} 