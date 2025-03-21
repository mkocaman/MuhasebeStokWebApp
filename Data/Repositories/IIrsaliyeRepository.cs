using System;
using System.Threading.Tasks;
using MuhasebeStokWebApp.Data.Entities;

namespace MuhasebeStokWebApp.Data.Repositories
{
    public interface IIrsaliyeRepository : IRepository<Irsaliye>
    {
        Task<Irsaliye> GetIrsaliyeWithDetailsAsync(Guid irsaliyeId);
    }
}