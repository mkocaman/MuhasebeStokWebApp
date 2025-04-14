using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using MuhasebeStokWebApp.Data;
using MuhasebeStokWebApp.Data.Entities;
using MuhasebeStokWebApp.Enums;
using MuhasebeStokWebApp.Services.Interfaces;
using MuhasebeStokWebApp.ViewModels;

namespace MuhasebeStokWebApp.Services
{
    public class StokService : IStokService
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<StokService> _logger;

        public StokService(ApplicationDbContext context, ILogger<StokService> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<IEnumerable<StokViewModel>> GetAllStokDurumu()
        {
            var stokDurumu = await _context.StokHareketleri
                .Include(sh => sh.Urun)
                    .ThenInclude(u => u.Birim)
                .AsSplitQuery()
                .Where(sh => !sh.Silindi)
                .GroupBy(sh => new { sh.UrunID, sh.Urun.UrunAdi, sh.Urun.UrunKodu, sh.Urun.Birim.BirimAdi })
                .Select(g => new StokViewModel
                {
                    UrunID = g.Key.UrunID,
                    UrunAdi = g.Key.UrunAdi,
                    UrunKodu = g.Key.UrunKodu,
                    BirimAdi = g.Key.BirimAdi,
                    GirisMiktari = g.Where(sh => sh.HareketTuru == StokHareketiTipi.Giris).Sum(sh => sh.Miktar),
                    CikisMiktari = g.Where(sh => sh.HareketTuru == StokHareketiTipi.Cikis).Sum(sh => Math.Abs(sh.Miktar))
                })
                .ToListAsync();

            return stokDurumu;
        }

        public async Task<StokHareket> GetStokHareketByIdAsync(Guid id)
        {
            return await _context.StokHareketleri
                .Include(s => s.Urun)
                .AsSplitQuery()
                .Where(s => s.StokHareketID == id)
                .OrderBy(s => s.StokHareketID)
                .FirstOrDefaultAsync();
        }

        public async Task<List<StokHareket>> GetStokGirisCikisAsync(Guid urunId, DateTime baslangicTarihi, DateTime bitisTarihi)
        {
            return await _context.StokHareketleri
                .Include(sh => sh.Urun)
                    .ThenInclude(u => u.Birim)
                .AsSplitQuery()
                .Where(sh => sh.UrunID == urunId && sh.Tarih >= baslangicTarihi && sh.Tarih <= bitisTarihi && !sh.Silindi)
                .OrderBy(sh => sh.Tarih)
                .ToListAsync();
        }

        /// <summary>
        /// Bir ürünün dinamik stok miktarını hesaplar. Bu metot ürün için yapılan giriş ve çıkış hareketlerini
        /// toplayarak dinamik stok miktarını hesaplar. StokMiktar Urun sınıfında statik olarak tutulmaz.
        /// </summary>
        /// <param name="urunID">Stok miktarı hesaplanacak ürünün ID'si</param>
        /// <param name="depoID">İsteğe bağlı depo filtresi. Eğer belirtilirse, sadece o depodaki stok hesaplanır</param>
        /// <returns>Ürünün dinamik stok miktarı</returns>
        public async Task<decimal> GetDinamikStokMiktari(Guid urunID, Guid? depoID = null)
        {
            try
            {
                var stokHareketleriQuery = _context.StokHareketleri
                    .Where(sh => sh.UrunID == urunID && !sh.Silindi);

                if (depoID.HasValue)
                {
                    stokHareketleriQuery = stokHareketleriQuery.Where(sh => sh.DepoID == depoID);
                }

                var girisler = await stokHareketleriQuery
                    .Where(sh => sh.HareketTuru == StokHareketiTipi.Giris)
                    .SumAsync(sh => sh.Miktar);

                var cikislar = await stokHareketleriQuery
                    .Where(sh => sh.HareketTuru == StokHareketiTipi.Cikis)
                    .SumAsync(sh => Math.Abs(sh.Miktar)); // Mutlak değer olarak alıyoruz

                return girisler - cikislar; // Çıkışları çıkararak hesaplıyoruz
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Stok miktarı hesaplanırken hata oluştu: UrunID={urunID}");
                return 0;
            }
        }
    }
} 