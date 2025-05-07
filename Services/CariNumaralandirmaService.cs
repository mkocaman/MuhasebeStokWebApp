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
    /// Cari kodu oluşturma işlemlerini yönetir
    /// </summary>
    public class CariNumaralandirmaService : ICariNumaralandirmaService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<CariNumaralandirmaService> _logger;
        
        public CariNumaralandirmaService(
            IUnitOfWork unitOfWork,
            ILogger<CariNumaralandirmaService> logger)
        {
            _unitOfWork = unitOfWork;
            _logger = logger;
        }
        
        /// <summary>
        /// Yeni cari kodu oluşturur
        /// </summary>
        public async Task<string> GenerateCariKoduAsync()
        {
            var today = DateTime.Now;
            var year = today.Year.ToString().Substring(2);
            var month = today.Month.ToString().PadLeft(2, '0');
            var day = today.Day.ToString().PadLeft(2, '0');
            
            var prefix = $"CRI-{year}{month}{day}-";
            
            return await GenerateUniqueNumaraAsync(prefix);
        }
        
        /// <summary>
        /// Verilen prefix'e göre benzersiz bir cari kodu oluşturur
        /// </summary>
        public async Task<string> GenerateUniqueNumaraAsync(string prefix, int basamakSayisi = 3)
        {
            _logger.LogDebug("Yeni cari kodu oluşturuluyor. Prefix: {Prefix}", prefix);
            
            // Son numara sırasını bul
            var sonCari = await _unitOfWork.CariRepository.GetAll()
                .Where(c => c.CariKodu != null && c.CariKodu.StartsWith(prefix))
                .OrderByDescending(c => c.CariKodu)
                .FirstOrDefaultAsync();
            
            int sequence = 1;
            if (sonCari != null && sonCari.CariKodu != null)
            {
                var parts = sonCari.CariKodu.Split('-');
                if (parts.Length >= 3 && int.TryParse(parts[parts.Length - 1], out int lastSeq))
                {
                    sequence = lastSeq + 1;
                    _logger.LogDebug("Son numara bulundu. Sıra: {Sequence}", sequence);
                }
            }
            
            var yeniNumara = $"{prefix}{sequence.ToString().PadLeft(basamakSayisi, '0')}";
            _logger.LogInformation("Yeni cari kodu oluşturuldu: {Numara}", yeniNumara);
            
            return yeniNumara;
        }
    }
} 