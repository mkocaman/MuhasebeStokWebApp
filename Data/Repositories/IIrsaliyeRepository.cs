using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using MuhasebeStokWebApp.Data.Entities;

namespace MuhasebeStokWebApp.Data.Repositories
{
    public interface IIrsaliyeRepository : IRepository<Irsaliye>
    {
        Task<Irsaliye> GetIrsaliyeWithDetailsAsync(Guid id);
        Task<List<IrsaliyeDetay>> GetIrsaliyeDetaylariAsync(Guid irsaliyeId);
        Task<List<Irsaliye>> GetIrsaliyeListWithDetailsAsync(DateTime? startDate = null, DateTime? endDate = null, string searchTerm = null);
        Task<bool> IrsaliyeExistsAsync(Guid id);
    }
}