using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using MuhasebeStokWebApp.Data.Entities;

namespace MuhasebeStokWebApp.Data.Repositories.EntityRepositories
{
    public class FaturaRepository : Repository<Fatura>, IFaturaRepository
    {
        private readonly ApplicationDbContext _context;

        public FaturaRepository(ApplicationDbContext context) : base(context)
        {
            _context = context;
        }

        /// <summary>
        /// Faturayı tüm detaylarıyla birlikte getirir
        /// </summary>
        public async Task<Fatura> GetFaturaWithDetailsAsync(Guid id)
        {
            return await _context.Faturalar
                .Include(f => f.Cari)
                .Include(f => f.FaturaTuru)
                .Include(f => f.FaturaDetaylari)
                    .ThenInclude(fd => fd.Urun)
                .Include(f => f.FaturaOdemeleri)
                .FirstOrDefaultAsync(f => f.FaturaID == id && !f.Silindi);
        }

        /// <summary>
        /// Fatura numarasına göre fatura getirir
        /// </summary>
        public async Task<Fatura> GetFaturaByNumaraAsync(string faturaNumarasi)
        {
            return await _context.Faturalar
                .Include(f => f.Cari)
                .Include(f => f.FaturaTuru)
                .FirstOrDefaultAsync(f => f.FaturaNumarasi == faturaNumarasi && !f.Silindi);
        }

        /// <summary>
        /// Cari ID'ye göre faturaları getirir
        /// </summary>
        public async Task<List<Fatura>> GetFaturalarByCariAsync(Guid cariId)
        {
            return await _context.Faturalar
                .Include(f => f.FaturaTuru)
                .Where(f => f.CariID == cariId && !f.Silindi)
                .OrderByDescending(f => f.FaturaTarihi)
                .ToListAsync();
        }

        /// <summary>
        /// Belirli tarih aralığındaki faturaları getirir
        /// </summary>
        public async Task<List<Fatura>> GetFaturalarByDateRangeAsync(DateTime startDate, DateTime endDate)
        {
            return await _context.Faturalar
                .Include(f => f.Cari)
                .Include(f => f.FaturaTuru)
                .Where(f => f.FaturaTarihi >= startDate && f.FaturaTarihi <= endDate && !f.Silindi)
                .OrderByDescending(f => f.FaturaTarihi)
                .ToListAsync();
        }

        /// <summary>
        /// Ödenmemiş faturaları getirir
        /// </summary>
        public async Task<List<Fatura>> GetOdenmemisFaturalarAsync()
        {
            return await _context.Faturalar
                .Include(f => f.Cari)
                .Include(f => f.FaturaTuru)
                .Include(f => f.FaturaOdemeleri)
                .Where(f => !f.Silindi && f.OdemeDurumu != "Ödendi")
                .OrderByDescending(f => f.FaturaTarihi)
                .ToListAsync();
        }

        /// <summary>
        /// Vadesi geçmiş faturaları getirir
        /// </summary>
        public async Task<List<Fatura>> GetVadesiGecmisFaturalarAsync()
        {
            var bugun = DateTime.Today;
            return await _context.Faturalar
                .Include(f => f.Cari)
                .Include(f => f.FaturaTuru)
                .Include(f => f.FaturaOdemeleri)
                .Where(f => !f.Silindi && 
                       f.OdemeDurumu != "Ödendi" && 
                       f.VadeTarihi.HasValue && 
                       f.VadeTarihi.Value < bugun)
                .OrderBy(f => f.VadeTarihi)
                .ToListAsync();
        }

        /// <summary>
        /// Fatura türüne göre faturaları getirir
        /// </summary>
        public async Task<List<Fatura>> GetFaturalarByFaturaTuruAsync(string faturaTuru)
        {
            return await _context.Faturalar
                .Include(f => f.Cari)
                .Include(f => f.FaturaTuru)
                .Where(f => !f.Silindi && f.FaturaTuru.FaturaTuruAdi == faturaTuru)
                .OrderByDescending(f => f.FaturaTarihi)
                .ToListAsync();
        }

        /// <summary>
        /// Belirtilen arama terimine göre faturaları filtreler
        /// </summary>
        public async Task<List<Fatura>> SearchFaturalarAsync(string searchTerm)
        {
            if (string.IsNullOrWhiteSpace(searchTerm))
                return await _context.Faturalar
                    .Include(f => f.Cari)
                    .Include(f => f.FaturaTuru)
                    .Where(f => !f.Silindi)
                    .OrderByDescending(f => f.FaturaTarihi)
                    .Take(50)
                    .ToListAsync();

            return await _context.Faturalar
                .Include(f => f.Cari)
                .Include(f => f.FaturaTuru)
                .Where(f => !f.Silindi && (
                    f.FaturaNumarasi.Contains(searchTerm) ||
                    f.FaturaNotu.Contains(searchTerm) ||
                    f.FaturaTuru.FaturaTuruAdi.Contains(searchTerm) ||
                    f.Cari.Ad.Contains(searchTerm) ||
                    f.Cari.CariUnvani.Contains(searchTerm)
                ))
                .OrderByDescending(f => f.FaturaTarihi)
                .ToListAsync();
        }

        /// <summary>
        /// Faturaya ilişkin detayları getirir
        /// </summary>
        public async Task<List<FaturaDetay>> GetFaturaDetaylarAsync(Guid faturaId)
        {
            return await _context.FaturaDetaylari
                .Include(fd => fd.Urun)
                .Where(fd => fd.FaturaID == faturaId && !fd.Silindi)
                .ToListAsync();
        }

        public async Task<IEnumerable<Fatura>> GetAllWithIncludesAsync()
        {
            return await _context.Faturalar
                .Include(f => f.Cari)
                .Include(f => f.FaturaDetaylari)
                .Include(f => f.FaturaTuru)
                .Where(f => !f.Silindi)
                .OrderByDescending(f => f.OlusturmaTarihi)
                .ToListAsync();
        }

        public async Task<Fatura> GetByIdWithIncludesAsync(Guid id)
        {
            return await _context.Faturalar
                .Include(f => f.Cari)
                .Include(f => f.FaturaTuru)
                .Include(f => f.FaturaDetaylari)
                    .ThenInclude(fd => fd.Urun)
                        .ThenInclude(u => u.Birim)
                .Include(f => f.FaturaDetaylari)
                    .ThenInclude(fd => fd.Urun)
                        .ThenInclude(u => u.Kategori)
                .FirstOrDefaultAsync(f => f.FaturaID == id && !f.Silindi);
        }

        public async Task<IEnumerable<Fatura>> GetFaturaByDateRangeAsync(DateTime startDate, DateTime endDate)
        {
            return await _context.Faturalar
                .Include(f => f.Cari)
                .Include(f => f.FaturaTuru)
                .Where(f => f.FaturaTarihi >= startDate && f.FaturaTarihi <= endDate && !f.Silindi)
                .OrderByDescending(f => f.FaturaTarihi)
                .ToListAsync();
        }

        public async Task<IEnumerable<Fatura>> GetFaturaByCariIdAsync(Guid cariId)
        {
            return await _context.Faturalar
                .Include(f => f.FaturaTuru)
                .Where(f => f.CariID == cariId && !f.Silindi)
                .OrderByDescending(f => f.FaturaTarihi)
                .ToListAsync();
        }

        public async Task<IEnumerable<Fatura>> GetUnpaidFaturasAsync()
        {
            return await _context.Faturalar
                .Include(f => f.Cari)
                .Include(f => f.FaturaTuru)
                .Where(f => f.OdemeDurumu == "Ödenmedi" && !f.Silindi)
                .OrderByDescending(f => f.VadeTarihi)
                .ToListAsync();
        }

        public async Task<decimal> GetFaturaTotalAmountByDateRangeAsync(DateTime startDate, DateTime endDate)
        {
            return await _context.Faturalar
                .Where(f => f.FaturaTarihi >= startDate && f.FaturaTarihi <= endDate && !f.Silindi)
                .SumAsync(f => f.GenelToplam ?? 0);
        }

        public async Task<IEnumerable<Fatura>> SearchFaturaAsync(string searchTerm)
        {
            if (string.IsNullOrWhiteSpace(searchTerm))
                return new List<Fatura>();

            return await _context.Faturalar
                .Include(f => f.Cari)
                .Include(f => f.FaturaTuru)
                .Where(f => (f.FaturaNumarasi != null && f.FaturaNumarasi.Contains(searchTerm)) ||
                           (f.Cari != null && f.Cari.Ad != null && f.Cari.Ad.Contains(searchTerm)) ||
                           (f.SiparisNumarasi != null && f.SiparisNumarasi.Contains(searchTerm)) && 
                           !f.Silindi)
                .OrderByDescending(f => f.OlusturmaTarihi)
                .ToListAsync();
        }
    }
} 