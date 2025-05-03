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
            // Önce sadece irsaliye detayı getir
            var irsaliyeDetay = await _context.Set<IrsaliyeDetay>()
                .FirstOrDefaultAsync(id => id.IrsaliyeDetayID == irsaliyeDetayId && !id.Silindi);
                
            if (irsaliyeDetay == null)
                return null;
                
            // İlişkili verileri ayrı sorgularda getir
            await _context.Entry(irsaliyeDetay)
                .Reference(id => id.Irsaliye)
                .LoadAsync();
                
            await _context.Entry(irsaliyeDetay)
                .Reference(id => id.Urun)
                .LoadAsync();
                
            return irsaliyeDetay;
        }
    }
} 