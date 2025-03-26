using System;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using MuhasebeStokWebApp.Data.Entities;
using MuhasebeStokWebApp.Models;

namespace MuhasebeStokWebApp.Data.Repositories
{
    public class IrsaliyeRepository : Repository<Irsaliye>, IIrsaliyeRepository
    {
        private readonly ApplicationDbContext _context;

        public IrsaliyeRepository(ApplicationDbContext context) : base(context)
        {
            _context = context;
        }

        public async Task<Irsaliye> GetIrsaliyeWithDetailsAsync(Guid id)
        {
            return await _context.Irsaliyeler
                .Include(i => i.Cari)
                .Include(i => i.Fatura)
                .Include(i => i.IrsaliyeDetaylari)
                    .ThenInclude(d => d.Urun)
                .FirstOrDefaultAsync(i => i.IrsaliyeID.Equals(id) && i.Aktif);
        }

        public async Task<List<IrsaliyeDetay>> GetIrsaliyeDetaylariAsync(Guid irsaliyeId)
        {
            return await _context.IrsaliyeDetaylari
                .Include(d => d.Urun)
                .Where(d => d.IrsaliyeID.Equals(irsaliyeId) && d.Aktif)
                .ToListAsync();
        }

        public async Task<List<Irsaliye>> GetIrsaliyeListWithDetailsAsync(
            DateTime? startDate = null,
            DateTime? endDate = null,
            string searchTerm = null)
        {
            var query = _context.Irsaliyeler
                .AsNoTracking()
                .Include(i => i.Cari)
                .Include(i => i.Fatura)
                .Where(i => i.Aktif);

            if (startDate.HasValue)
                query = query.Where(i => i.IrsaliyeTarihi >= startDate.Value);

            if (endDate.HasValue)
                query = query.Where(i => i.IrsaliyeTarihi <= endDate.Value);

            if (!string.IsNullOrWhiteSpace(searchTerm))
                query = query.Where(i => 
                    i.IrsaliyeNumarasi.Contains(searchTerm) ||
                    i.IrsaliyeTuru.Contains(searchTerm) ||
                    i.Aciklama.Contains(searchTerm) ||
                    i.Cari.Ad.Contains(searchTerm));

            return await query
                .OrderByDescending(i => i.IrsaliyeTarihi)
                .ToListAsync();
        }

        public async Task<bool> IrsaliyeExistsAsync(Guid id)
        {
            return await _context.Irsaliyeler
                .AnyAsync(i => i.IrsaliyeID.Equals(id) && i.Aktif);
        }
    }
} 