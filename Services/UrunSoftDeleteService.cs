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
    /// Urun entity'si için özelleştirilmiş SoftDeleteService
    /// </summary>
    public class UrunSoftDeleteService : SoftDeleteService<Urun>
    {
        public UrunSoftDeleteService(
            ApplicationDbContext context,
            ILogger<UrunSoftDeleteService> logger) 
            : base(context, logger)
        {
        }

        /// <summary>
        /// Ürünün ilişkili kayıtları olup olmadığını kontrol eder
        /// </summary>
        public override async Task<bool> HasRelatedRecordsAsync(object id)
        {
            if (id == null)
            {
                return false;
            }

            try
            {
                Guid urunId = (Guid)id;
                
                // FaturaDetay ilişkisi kontrolü
                bool hasFaturaDetay = await _context.FaturaDetaylari
                    .AnyAsync(fd => fd.UrunID == urunId && !fd.Silindi);
                
                if (hasFaturaDetay)
                {
                    _logger.LogInformation($"Urun ID: {urunId} için ilişkili FaturaDetay kayıtları bulundu");
                    return true;
                }
                
                // IrsaliyeDetay ilişkisi kontrolü
                bool hasIrsaliyeDetay = await _context.IrsaliyeDetaylari
                    .AnyAsync(id => id.UrunID == urunId && !id.Silindi);
                
                if (hasIrsaliyeDetay)
                {
                    _logger.LogInformation($"Urun ID: {urunId} için ilişkili IrsaliyeDetay kayıtları bulundu");
                    return true;
                }
                
                // StokHareket ilişkisi kontrolü
                bool hasStokHareket = await _context.StokHareketleri
                    .AnyAsync(sh => sh.UrunID == urunId && !sh.Silindi);
                
                if (hasStokHareket)
                {
                    _logger.LogInformation($"Urun ID: {urunId} için ilişkili StokHareket kayıtları bulundu");
                    return true;
                }
                
                // StokFifo ilişkisi kontrolü
                bool hasStokFifo = await _context.StokFifoKayitlari
                    .AnyAsync(sf => sf.UrunID == urunId && !sf.Silindi);
                
                if (hasStokFifo)
                {
                    _logger.LogInformation($"Urun ID: {urunId} için ilişkili StokFifo kayıtları bulundu");
                    return true;
                }
                
                // Ürün fiyatları kontrolü
                bool hasUrunFiyat = await _context.UrunFiyatlari
                    .AnyAsync(uf => uf.UrunID == urunId && !uf.Silindi);
                
                if (hasUrunFiyat)
                {
                    _logger.LogInformation($"Urun ID: {urunId} için ilişkili UrunFiyat kayıtları bulundu");
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