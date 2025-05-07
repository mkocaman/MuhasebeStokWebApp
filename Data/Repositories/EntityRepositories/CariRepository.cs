using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using MuhasebeStokWebApp.Data.Entities;

namespace MuhasebeStokWebApp.Data.Repositories.EntityRepositories
{
    public class CariRepository : Repository<Cari>, ICariRepository
    {
        private readonly ApplicationDbContext _context;

        public CariRepository(ApplicationDbContext context) : base(context)
        {
            _context = context;
        }

        /// <summary>
        /// Cariyi tüm ilişkili verilerle birlikte getirir
        /// </summary>
        public async Task<Cari> GetCariWithDetailsAsync(Guid id)
        {
            return await _context.Cariler
                .Include(c => c.VarsayilanParaBirimi)
                .Include(c => c.CariHareketler)
                .Include(c => c.Faturalar)
                .Include(c => c.Irsaliyeler)
                .FirstOrDefaultAsync(c => c.CariID == id && !c.Silindi);
        }

        /// <summary>
        /// Cari koduna göre cari bilgisini getirir
        /// </summary>
        public async Task<Cari> GetCariByKodAsync(string cariKodu)
        {
            return await _context.Cariler
                .Include(c => c.VarsayilanParaBirimi)
                .FirstOrDefaultAsync(c => c.CariKodu == cariKodu && !c.Silindi);
        }

        /// <summary>
        /// Belirtilen cari tipine göre carileri getirir
        /// </summary>
        public async Task<List<Cari>> GetCarilerByTipAsync(string cariTipi)
        {
            return await _context.Cariler
                .Where(c => c.CariTipi == cariTipi && !c.Silindi)
                .OrderBy(c => c.Ad)
                .ToListAsync();
        }

        /// <summary>
        /// Belirtilen ile göre carileri getirir
        /// </summary>
        public async Task<List<Cari>> GetCarilerByIlAsync(string il)
        {
            return await _context.Cariler
                .Where(c => c.Il == il && !c.Silindi)
                .OrderBy(c => c.Ad)
                .ToListAsync();
        }

        /// <summary>
        /// Vadesi geçmiş bakiyesi olan carileri getirir
        /// </summary>
        public async Task<List<Cari>> GetVadesiGecmisBakiyesiOlanCarilerAsync()
        {
            var bugun = DateTime.Today;
            
            // Bu sorgu, cariyi baz alarak vadesi geçmiş faturaları olan carileri bulur
            var carilerWithVadesiGecmisFaturalar = await _context.Cariler
                .Include(c => c.Faturalar.Where(f => !f.Silindi && 
                                          f.OdemeDurumu != "Ödendi" && 
                                          f.VadeTarihi.HasValue && 
                                          f.VadeTarihi.Value < bugun))
                .Where(c => !c.Silindi && c.Faturalar.Any(f => !f.Silindi && 
                                                        f.OdemeDurumu != "Ödendi" && 
                                                        f.VadeTarihi.HasValue && 
                                                        f.VadeTarihi.Value < bugun))
                .ToListAsync();

            return carilerWithVadesiGecmisFaturalar;
        }

        /// <summary>
        /// Belirtilen arama terimine göre carileri filtreler
        /// </summary>
        public async Task<List<Cari>> SearchCarilerAsync(string searchTerm)
        {
            if (string.IsNullOrWhiteSpace(searchTerm))
                return await _context.Cariler
                    .Where(c => !c.Silindi)
                    .OrderBy(c => c.Ad)
                    .Take(50)
                    .ToListAsync();

            return await _context.Cariler
                .Where(c => !c.Silindi && (
                    c.Ad.Contains(searchTerm) ||
                    c.CariKodu.Contains(searchTerm) ||
                    c.Telefon.Contains(searchTerm) ||
                    c.Email.Contains(searchTerm) ||
                    c.Yetkili.Contains(searchTerm) ||
                    c.VergiNo.Contains(searchTerm) ||
                    c.Il.Contains(searchTerm) ||
                    c.Ilce.Contains(searchTerm)
                ))
                .OrderBy(c => c.Ad)
                .ToListAsync();
        }

        /// <summary>
        /// Cariye ilişkin hareketleri getirir
        /// </summary>
        public async Task<List<CariHareket>> GetCariHareketlerAsync(Guid cariId)
        {
            return await _context.CariHareketler
                .Where(ch => ch.CariID == cariId && !ch.Silindi)
                .OrderByDescending(ch => ch.Tarih)
                .ToListAsync();
        }
    }
} 