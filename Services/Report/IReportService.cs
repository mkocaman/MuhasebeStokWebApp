using System;
using System.Threading.Tasks;

namespace MuhasebeStokWebApp.Services.Report
{
    public interface IReportService
    {
        Task<byte[]> GenerateFaturaReportAsync(Guid faturaId);
        Task<byte[]> GenerateIrsaliyeReportAsync(Guid irsaliyeId);
    }
} 