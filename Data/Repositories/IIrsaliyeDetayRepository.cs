using System;
using System.Threading.Tasks;
using MuhasebeStokWebApp.Data.Entities;

namespace MuhasebeStokWebApp.Data.Repositories
{
    public interface IIrsaliyeDetayRepository : IRepository<IrsaliyeDetay>
    {
        Task<IrsaliyeDetay> GetIrsaliyeDetayWithDetailsAsync(Guid irsaliyeDetayId);
    }
} 