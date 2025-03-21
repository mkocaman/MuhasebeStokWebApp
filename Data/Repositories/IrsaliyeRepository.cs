using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using MuhasebeStokWebApp.Data.Entities;

namespace MuhasebeStokWebApp.Data.Repositories
{
    public class IrsaliyeRepository : Repository<Irsaliye>, IIrsaliyeRepository
    {
        private readonly ApplicationDbContext _context;

        public IrsaliyeRepository(ApplicationDbContext context) : base(context)
        {
            _context = context;
        }

        public async Task<Irsaliye> GetIrsaliyeWithDetailsAsync(Guid irsaliyeId)
        {
            return await _context.Set<Irsaliye>()
                .Include(i => i.Cari)
                .Include(i => i.Fatura)
                .Include(i => i.IrsaliyeDetaylari)
                    .ThenInclude(id => id.Urun)
                .FirstOrDefaultAsync(i => i.IrsaliyeID == irsaliyeId && !i.SoftDelete);
        }
    }
} 