using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using MuhasebeStokWebApp.Data.Entities;

namespace MuhasebeStokWebApp.Data.Repositories
{
    public class IrsaliyeDetayRepository : Repository<IrsaliyeDetay>, IIrsaliyeDetayRepository
    {
        private readonly ApplicationDbContext _context;

        public IrsaliyeDetayRepository(ApplicationDbContext context) : base(context)
        {
            _context = context;
        }

        public async Task<IrsaliyeDetay> GetIrsaliyeDetayWithDetailsAsync(Guid irsaliyeDetayId)
        {
            return await _context.Set<IrsaliyeDetay>()
                .Include(id => id.Irsaliye)
                .Include(id => id.Urun)
                .FirstOrDefaultAsync(id => id.IrsaliyeDetayID == irsaliyeDetayId && !id.Silindi);
        }
    }
} 