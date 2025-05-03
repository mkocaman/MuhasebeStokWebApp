using System;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using MuhasebeStokWebApp.Data;
using MuhasebeStokWebApp.Data.Entities;
using MuhasebeStokWebApp.Data.Repositories;

namespace MuhasebeStokWebApp.Services
{
    /// <summary>
    /// Cari entity'si için özelleştirilmiş SoftDeleteService
    /// </summary>
    public class CariSoftDeleteService : SoftDeleteService<Cari>
    {
        public CariSoftDeleteService(
            IUnitOfWork unitOfWork,
            ILogger<CariSoftDeleteService> logger,
            ApplicationDbContext context) 
            : base(unitOfWork, logger, context)
        {
        }

        /// <summary>
        /// Cari kaydının ilişkili kayıtları olup olmadığını kontrol eder
        /// </summary>
        public override async Task<bool> HasRelatedRecordsAsync(object id)
        {
            if (id == null)
            {
                return false;
            }

            try
            {
                Guid cariId = (Guid)id;
                
                // CariHareket ilişkisi kontrolü
                bool hasCariHareket = await _context.CariHareketler
                    .AnyAsync(ch => ch.CariID == cariId && !ch.Silindi);
                
                if (hasCariHareket)
                {
                    _logger.LogInformation($"Cari ID: {cariId} için ilişkili CariHareket kayıtları bulundu");
                    return true;
                }
                
                // Fatura ilişkisi kontrolü
                bool hasFatura = await _context.Faturalar
                    .AnyAsync(f => f.CariID == cariId && !f.Silindi);
                
                if (hasFatura)
                {
                    _logger.LogInformation($"Cari ID: {cariId} için ilişkili Fatura kayıtları bulundu");
                    return true;
                }
                
                // Irsaliye ilişkisi kontrolü
                bool hasIrsaliye = await _context.Irsaliyeler
                    .AnyAsync(i => i.CariID == cariId && !i.Silindi);
                
                if (hasIrsaliye)
                {
                    _logger.LogInformation($"Cari ID: {cariId} için ilişkili Irsaliye kayıtları bulundu");
                    return true;
                }
                
                // Sozlesme ilişkisi kontrolü
                bool hasSozlesme = await _context.Sozlesmeler
                    .AnyAsync(s => s.CariID == cariId && !s.Silindi);
                
                if (hasSozlesme)
                {
                    _logger.LogInformation($"Cari ID: {cariId} için ilişkili Sozlesme kayıtları bulundu");
                    return true;
                }
                
                // İlişkili kayıt bulunamadı
                return false;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"HasRelatedRecordsAsync hatası: {ex.Message}");
                return false;
            }
        }
    }
} 