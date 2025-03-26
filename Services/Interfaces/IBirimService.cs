using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using MuhasebeStokWebApp.Data.Entities;

namespace MuhasebeStokWebApp.Services.Interfaces
{
    public interface IBirimService
    {
        Task<IEnumerable<UrunBirim>> GetAllAsync();
        Task<UrunBirim> GetByIdAsync(int id);
        Task<UrunBirim> AddAsync(UrunBirim birim);
        Task<UrunBirim> UpdateAsync(UrunBirim birim);
        Task<bool> DeleteAsync(int id);
    }
} 