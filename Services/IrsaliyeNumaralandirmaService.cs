using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using MuhasebeStokWebApp.Data.Repositories;
using MuhasebeStokWebApp.Services.Interfaces;

namespace MuhasebeStokWebApp.Services
{
    /// <summary>
    /// İrsaliye numarası oluşturma işlemlerini yönetir
    /// </summary>
    public class IrsaliyeNumaralandirmaService : IIrsaliyeNumaralandirmaService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<IrsaliyeNumaralandirmaService> _logger;
        
        public IrsaliyeNumaralandirmaService(
            IUnitOfWork unitOfWork,
            ILogger<IrsaliyeNumaralandirmaService> logger)
        {
            _unitOfWork = unitOfWork;
            _logger = logger;
        }
        
        /// <summary>
        /// Yeni irsaliye numarası oluşturur
        /// </summary>
        public async Task<string> GenerateIrsaliyeNumarasiAsync()
        {
            var today = DateTime.Now;
            var year = today.Year.ToString().Substring(2);
            var month = today.Month.ToString().PadLeft(2, '0');
            var day = today.Day.ToString().PadLeft(2, '0');
            
            var prefix = $"IRS-{year}{month}{day}-";
            
            return await GenerateUniqueNumaraAsync(prefix);
        }
        
        /// <summary>
        /// Verilen prefix'e göre benzersiz bir irsaliye numarası oluşturur
        /// </summary>
        public async Task<string> GenerateUniqueNumaraAsync(string prefix, int basamakSayisi = 4)
        {
            _logger.LogDebug("Yeni irsaliye numarası oluşturuluyor. Prefix: {Prefix}", prefix);
            
            // Son numara sırasını bul
            var sonIrsaliye = await _unitOfWork.IrsaliyeRepository.GetAll()
                .Where(i => i.IrsaliyeNumarasi != null && i.IrsaliyeNumarasi.StartsWith(prefix))
                .OrderByDescending(i => i.IrsaliyeNumarasi)
                .FirstOrDefaultAsync();
            
            int sequence = 1;
            if (sonIrsaliye != null && sonIrsaliye.IrsaliyeNumarasi != null)
            {
                var parts = sonIrsaliye.IrsaliyeNumarasi.Split('-');
                if (parts.Length >= 3 && int.TryParse(parts[parts.Length - 1], out int lastSeq))
                {
                    sequence = lastSeq + 1;
                    _logger.LogDebug("Son numara bulundu. Sıra: {Sequence}", sequence);
                }
            }
            
            var yeniNumara = $"{prefix}{sequence.ToString().PadLeft(basamakSayisi, '0')}";
            _logger.LogInformation("Yeni irsaliye numarası oluşturuldu: {Numara}", yeniNumara);
            
            return yeniNumara;
        }
    }
} 