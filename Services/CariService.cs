using Microsoft.EntityFrameworkCore;
using MuhasebeStokWebApp.Data;
using MuhasebeStokWebApp.Data.Entities;
using MuhasebeStokWebApp.Services.Interfaces;
using MuhasebeStokWebApp.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MuhasebeStokWebApp.Services
{
    public class CariService : ICariService
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogService _logService;

        public CariService(ApplicationDbContext context, ILogService logService)
        {
            _context = context;
            _logService = logService;
        }

        public async Task<IEnumerable<Cari>> GetAllAsync()
        {
            return await _context.Cariler
                .Where(c => !c.Silindi)
                .OrderBy(c => c.Ad)
                .ToListAsync();
        }

        public async Task<Cari> GetByIdAsync(Guid id)
        {
            return await _context.Cariler
                .FirstOrDefaultAsync(c => c.CariID == id && !c.Silindi);
        }

        public async Task<Cari> AddAsync(Cari cari)
        {
            await _context.Cariler.AddAsync(cari);
            await _context.SaveChangesAsync();
            return cari;
        }

        public async Task<Cari> UpdateAsync(Cari cari)
        {
            _context.Entry(cari).State = EntityState.Modified;
            await _context.SaveChangesAsync();
            return cari;
        }

        public async Task DeleteAsync(Cari cari)
        {
            cari.Silindi = true;
            cari.GuncellemeTarihi = DateTime.Now;
            await _context.SaveChangesAsync();
        }
    }
} 