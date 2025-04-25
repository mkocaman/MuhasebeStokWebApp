using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using MuhasebeStokWebApp.Data;
using MuhasebeStokWebApp.Data.Entities;
using MuhasebeStokWebApp.Services.Interfaces;
using MuhasebeStokWebApp.ViewModels.Fatura;

namespace MuhasebeStokWebApp.Services
{
    public class FaturaService : IFaturaService
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<FaturaService> _logger;

        public FaturaService(ApplicationDbContext context, ILogger<FaturaService> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<IEnumerable<Fatura>> GetAllAsync()
        {
            return await _context.Faturalar
                .Include(f => f.Cari)
                .Include(f => f.FaturaDetaylari)
                .Where(f => !f.Silindi)
                .ToListAsync();
        }

        public async Task<Fatura> GetByIdAsync(Guid id)
        {
            return await _context.Faturalar
                .Include(f => f.Cari)
                .Include(f => f.FaturaDetaylari)
                    .ThenInclude(fk => fk.Urun)
                .FirstOrDefaultAsync(f => f.FaturaID == id && !f.Silindi);
        }

        public async Task<Fatura> AddAsync(Fatura fatura)
        {
            await _context.Faturalar.AddAsync(fatura);
            await _context.SaveChangesAsync();
            return fatura;
        }

        public async Task<Fatura> UpdateAsync(Fatura fatura)
        {
            _context.Faturalar.Update(fatura);
            await _context.SaveChangesAsync();
            return fatura;
        }

        public async Task<bool> DeleteAsync(Guid id)
        {
            var fatura = await _context.Faturalar.FindAsync(id);
            if (fatura == null)
                return false;

            fatura.Silindi = true;
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<FaturaDetailViewModel> GetFaturaDetailViewModelAsync(Guid id)
        {
            var fatura = await GetByIdAsync(id);
            if (fatura == null)
                return null;

            var viewModel = new FaturaDetailViewModel
            {
                FaturaID = fatura.FaturaID,
                FaturaNumarasi = fatura.FaturaNumarasi,
                FaturaTarihi = fatura.FaturaTarihi,
                VadeTarihi = fatura.VadeTarihi,
                CariAdi = fatura.Cari?.Ad ?? string.Empty,
                CariID = fatura.CariID ?? Guid.Empty,
                CariVergiNo = fatura.Cari?.VergiNo ?? string.Empty,
                CariVergiDairesi = fatura.Cari?.VergiDairesi,
                CariAdres = fatura.Cari?.Adres ?? string.Empty,
                CariTelefon = fatura.Cari?.Telefon ?? string.Empty,
                AraToplam = fatura.AraToplam ?? 0,
                KdvTutari = fatura.KDVToplam ?? 0,
                IndirimTutari = fatura.IndirimTutari ?? 0,
                GenelToplam = fatura.GenelToplam ?? 0,
                AraToplamDoviz = fatura.AraToplamDoviz ?? 0,
                KdvTutariDoviz = fatura.KDVToplamDoviz ?? 0,
                IndirimTutariDoviz = fatura.IndirimTutariDoviz ?? 0,
                GenelToplamDoviz = fatura.GenelToplamDoviz ?? 0,
                OdemeDurumu = fatura.OdemeDurumu,
                DovizTuru = fatura.DovizTuru,
                DovizKuru = fatura.DovizKuru ?? 1,
                Aktif = fatura.Aktif,
                OlusturmaTarihi = fatura.OlusturmaTarihi,
                GuncellemeTarihi = fatura.GuncellemeTarihi,
                FaturaTuru = fatura.FaturaTuru?.FaturaTuruAdi ?? string.Empty,
                FaturaTuruID = fatura.FaturaTuruID,
                ResmiMi = fatura.ResmiMi,
                SiparisNumarasi = fatura.SiparisNumarasi,
                FaturaKalemleri = fatura.FaturaDetaylari?.Select(fk => new FaturaKalemDetailViewModel
                {
                    KalemID = fk.FaturaDetayID,
                    UrunID = fk.UrunID,
                    UrunKodu = fk.Urun?.UrunKodu ?? string.Empty,
                    UrunAdi = fk.Urun?.UrunAdi ?? string.Empty,
                    Miktar = fk.Miktar,
                    Birim = fk.Birim ?? string.Empty,
                    BirimFiyat = fk.BirimFiyat,
                    KdvOrani = (int)fk.KdvOrani,
                    IndirimOrani = (int)fk.IndirimOrani,
                    Tutar = fk.Tutar ?? 0,
                    KdvTutari = fk.KdvTutari ?? 0,
                    IndirimTutari = fk.IndirimTutari ?? 0,
                    NetTutar = fk.NetTutar ?? 0,
                    BirimFiyatDoviz = fk.BirimFiyatDoviz,
                    TutarDoviz = fk.TutarDoviz ?? 0,
                    KdvTutariDoviz = fk.KdvTutariDoviz ?? 0,
                    IndirimTutariDoviz = fk.IndirimTutariDoviz ?? 0,
                    NetTutarDoviz = fk.NetTutarDoviz ?? 0
                }).ToList() ?? new List<FaturaKalemDetailViewModel>()
            };

            return viewModel;
        }

        public async Task<List<FaturaViewModel>> GetAllFaturaViewModelsAsync()
        {
            var faturalar = await GetAllAsync();
            
            return faturalar.Select(f => new FaturaViewModel
            {
                FaturaID = f.FaturaID.ToString(),
                FaturaNumarasi = f.FaturaNumarasi,
                FaturaTarihi = f.FaturaTarihi,
                CariAdi = f.Cari?.Ad ?? string.Empty,
                GenelToplam = f.GenelToplam ?? 0,
                FaturaTuru = f.FaturaTuru?.FaturaTuruAdi ?? string.Empty,
                OdemeDurumu = f.OdemeDurumu ?? string.Empty,
                Aciklama = f.FaturaNotu ?? string.Empty,
                DovizTuru = f.DovizTuru,
                DovizKuru = f.DovizKuru ?? 1
            }).ToList();
        }

        public async Task<bool> IsFaturaInUseAsync(Guid id)
        {
            // Faturanın kullanımda olup olmadığını kontrol etme mantığı
            // Örneğin, ödeme kayıtlarında veya başka bir ilişkili tabloda kullanılıyorsa true dönebilir
            
            var isUsedInPayments = await _context.FaturaOdemeleri
                .AnyAsync(o => o.FaturaID == id && !o.Silindi);
                
            return isUsedInPayments;
        }
    }
} 