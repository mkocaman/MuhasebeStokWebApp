using Microsoft.EntityFrameworkCore;
using MuhasebeStokWebApp.Data;
using MuhasebeStokWebApp.Data.Entities;
using MuhasebeStokWebApp.Data.Repositories;
using MuhasebeStokWebApp.Services.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MuhasebeStokWebApp.Services
{
    public class CariHareketService : ICariHareketService
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogService _logService;
        private readonly IUnitOfWork _unitOfWork;

        public CariHareketService(
            ApplicationDbContext context, 
            ILogService logService,
            IUnitOfWork unitOfWork)
        {
            _context = context;
            _logService = logService;
            _unitOfWork = unitOfWork;
        }

        /// <summary>
        /// Cari hareket kaydı oluşturur
        /// </summary>
        public async Task<CariHareket> CreateHareketAsync(CariHareket hareket)
        {
            if (hareket == null)
                throw new ArgumentNullException(nameof(hareket));
            
            // CariID alan kontrolü
            if (hareket.CariID == Guid.Empty)
                throw new ArgumentException("Cari ID geçerli değil", nameof(hareket.CariID));
            
            await _context.CariHareketler.AddAsync(hareket);
            await _context.SaveChangesAsync();
            
            await _logService.Log(
                $"Cari hareket oluşturuldu: Cari ID: {hareket.CariID}, Tutar: {hareket.Tutar}, Türü: {hareket.HareketTuru}",
                Enums.LogTuru.Bilgi
            );
            
            return hareket;
        }

        /// <summary>
        /// Kasa hareketi için cari hareket kaydı oluşturur
        /// </summary>
        public async Task<CariHareket> CreateFromKasaHareketAsync(KasaHareket kasaHareket, bool borcMu)
        {
            if (kasaHareket == null)
                throw new ArgumentNullException(nameof(kasaHareket));
            
            if (kasaHareket.CariID == null || kasaHareket.CariID == Guid.Empty)
                throw new ArgumentException("Kasa hareketinde geçerli bir Cari ID yok", nameof(kasaHareket.CariID));
            
            var cariHareket = new CariHareket
            {
                CariHareketID = Guid.NewGuid(),
                CariID = kasaHareket.CariID.Value,
                Tarih = kasaHareket.Tarih,
                Aciklama = $"Kasa hareketi: {kasaHareket.Aciklama}",
                ReferansNo = $"KH-{kasaHareket.KasaHareketID}",
                ReferansTuru = "KasaHareket",
                ReferansID = kasaHareket.KasaHareketID,
                HareketTuru = kasaHareket.HareketTuru == "Giriş" ? "Tahsilat" : "Ödeme",
                Borc = borcMu ? kasaHareket.Tutar : 0,
                Alacak = borcMu ? 0 : kasaHareket.Tutar,
                Tutar = kasaHareket.Tutar,
                OlusturmaTarihi = DateTime.Now,
                OlusturanKullaniciID = kasaHareket.IslemYapanKullaniciID
            };
            
            await _context.CariHareketler.AddAsync(cariHareket);
            await _context.SaveChangesAsync();
            
            await _logService.Log(
                $"Kasa hareketinden cari hareket oluşturuldu: Cari ID: {cariHareket.CariID}, Tutar: {cariHareket.Tutar}, Türü: {cariHareket.HareketTuru}",
                Enums.LogTuru.Bilgi
            );
            
            return cariHareket;
        }

        /// <summary>
        /// Banka hareketi için cari hareket kaydı oluşturur
        /// </summary>
        public async Task<CariHareket> CreateFromBankaHareketAsync(BankaHesapHareket bankaHareket, bool borcMu)
        {
            if (bankaHareket == null)
                throw new ArgumentNullException(nameof(bankaHareket));
            
            if (bankaHareket.CariID == null || bankaHareket.CariID == Guid.Empty)
                throw new ArgumentException("Banka hareketinde geçerli bir Cari ID yok", nameof(bankaHareket.CariID));
            
            var cariHareket = new CariHareket
            {
                CariHareketID = Guid.NewGuid(),
                CariID = bankaHareket.CariID.Value,
                Tarih = bankaHareket.Tarih,
                Aciklama = $"Banka hareketi: {bankaHareket.Aciklama}",
                ReferansNo = $"BH-{bankaHareket.BankaHesapHareketID}",
                ReferansTuru = "BankaHareket",
                ReferansID = bankaHareket.BankaHesapHareketID,
                HareketTuru = bankaHareket.HareketTuru == "Para Yatırma" ? "Tahsilat" : "Ödeme",
                Borc = borcMu ? bankaHareket.Tutar : 0,
                Alacak = borcMu ? 0 : bankaHareket.Tutar,
                Tutar = bankaHareket.Tutar,
                OlusturmaTarihi = DateTime.Now,
                OlusturanKullaniciID = bankaHareket.IslemYapanKullaniciID
            };
            
            await _context.CariHareketler.AddAsync(cariHareket);
            await _context.SaveChangesAsync();
            
            await _logService.Log(
                $"Banka hareketinden cari hareket oluşturuldu: Cari ID: {cariHareket.CariID}, Tutar: {cariHareket.Tutar}, Türü: {cariHareket.HareketTuru}",
                Enums.LogTuru.Bilgi
            );
            
            return cariHareket;
        }

        /// <summary>
        /// ID'ye göre cari hareket getirir
        /// </summary>
        public async Task<CariHareket> GetByIdAsync(Guid id)
        {
            return await _context.CariHareketler
                .Include(c => c.Cari)
                .FirstOrDefaultAsync(c => c.CariHareketID == id && !c.Silindi);
        }

        /// <summary>
        /// Cari ID'ye göre hareketleri getirir
        /// </summary>
        public async Task<IEnumerable<CariHareket>> GetByCariIdAsync(Guid cariId)
        {
            return await _context.CariHareketler
                .Where(c => c.CariID == cariId && !c.Silindi)
                .OrderByDescending(c => c.Tarih)
                .ToListAsync();
        }
    }
} 