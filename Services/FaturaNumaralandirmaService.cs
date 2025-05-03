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
    /// Fatura ve sipariş numarası oluşturma işlemlerini yönetir
    /// </summary>
    public class FaturaNumaralandirmaService : IFaturaNumaralandirmaService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<FaturaNumaralandirmaService> _logger;
        
        public FaturaNumaralandirmaService(
            IUnitOfWork unitOfWork,
            ILogger<FaturaNumaralandirmaService> logger)
        {
            _unitOfWork = unitOfWork;
            _logger = logger;
        }
        
        /// <summary>
        /// Yeni fatura numarası oluşturur
        /// </summary>
        public async Task<string> GenerateFaturaNumarasiAsync()
        {
            var today = DateTime.Now;
            var year = today.Year.ToString().Substring(2);
            var month = today.Month.ToString().PadLeft(2, '0');
            var day = today.Day.ToString().PadLeft(2, '0');
            
            var prefix = $"FTR-{year}{month}{day}-";
            
            return await GenerateUniqueNumaraAsync(prefix);
        }
        
        /// <summary>
        /// Yeni sipariş numarası oluşturur
        /// </summary>
        public async Task<string> GenerateSiparisNumarasiAsync()
        {
            var today = DateTime.Now;
            var year = today.Year.ToString().Substring(2);
            var month = today.Month.ToString().PadLeft(2, '0');
            var day = today.Day.ToString().PadLeft(2, '0');
            
            var prefix = $"SIP-{year}{month}{day}-";
            
            return await GenerateUniqueNumaraAsync(prefix);
        }
        
        /// <summary>
        /// Verilen prefix'e göre benzersiz bir fatura numarası oluşturur
        /// </summary>
        public async Task<string> GenerateUniqueNumaraAsync(string prefix, int basamakSayisi = 3)
        {
            _logger.LogDebug("Yeni numara oluşturuluyor. Prefix: {Prefix}", prefix);
            
            // Son numara sırasını bul
            var sonNumara = await _unitOfWork.EntityFaturaRepository.GetAll()
                .Where(f => f.FaturaNumarasi != null && f.FaturaNumarasi.StartsWith(prefix))
                .OrderByDescending(f => f.FaturaNumarasi)
                .FirstOrDefaultAsync();
            
            int sequence = 1;
            if (sonNumara != null && sonNumara.FaturaNumarasi != null)
            {
                var parts = sonNumara.FaturaNumarasi.Split('-');
                if (parts.Length >= 3 && int.TryParse(parts[parts.Length - 1], out int lastSeq))
                {
                    sequence = lastSeq + 1;
                    _logger.LogDebug("Son numara bulundu. Sıra: {Sequence}", sequence);
                }
            }
            
            var yeniNumara = $"{prefix}{sequence.ToString().PadLeft(basamakSayisi, '0')}";
            _logger.LogInformation("Yeni numara oluşturuldu: {Numara}", yeniNumara);
            
            return yeniNumara;
        }
    }
} 