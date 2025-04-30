using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using MuhasebeStokWebApp.Data;
using MuhasebeStokWebApp.Data.Entities;
using MuhasebeStokWebApp.Data.Repositories;
using MuhasebeStokWebApp.Enums;
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
        private readonly ILogger<CariHareketService> _logger;

        public CariHareketService(
            ApplicationDbContext context, 
            ILogService logService,
            IUnitOfWork unitOfWork,
            ILogger<CariHareketService> logger)
        {
            _context = context;
            _logService = logService;
            _unitOfWork = unitOfWork;
            _logger = logger;
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

        /// <summary>
        /// Fatura için cari hareket kaydı oluşturur
        /// </summary>
        public async Task<CariHareket> CreateFromFaturaAsync(Fatura fatura, Guid? kullaniciId)
        {
            if (fatura == null)
                throw new ArgumentNullException(nameof(fatura));
            
            if (fatura.CariID == null || fatura.CariID == Guid.Empty)
            {
                _logger.LogWarning($"Faturada geçerli bir Cari ID yok. FaturaID: {fatura.FaturaID}");
                return null;
            }
            
            // Fatura türünü yükle (eğer yüklenmemişse)
            if (fatura.FaturaTuru == null && fatura.FaturaTuruID.HasValue)
            {
                await _context.Entry(fatura).Reference(f => f.FaturaTuru).LoadAsync();
            }
            
            // Hareket türünü belirle
            string hareketTuru = fatura.FaturaTuru?.FaturaTuruAdi ?? "Bilinmiyor";
            bool borcMu = fatura.FaturaTuru?.HareketTuru == "Çıkış"; // Satış faturası ise Borç, Alış faturası ise Alacak
            
            var cariHareket = new CariHareket
            {
                CariHareketID = Guid.NewGuid(),
                CariID = fatura.CariID.Value,
                Tarih = fatura.FaturaTarihi ?? DateTime.Now,
                Aciklama = $"{fatura.FaturaNumarasi} numaralı {hareketTuru} faturası",
                ReferansNo = fatura.FaturaNumarasi,
                ReferansTuru = "Fatura",
                ReferansID = fatura.FaturaID,
                HareketTuru = hareketTuru,
                Borc = borcMu ? fatura.GenelToplam ?? 0 : 0,
                Alacak = !borcMu ? fatura.GenelToplam ?? 0 : 0,
                Tutar = fatura.GenelToplam ?? 0,
                OlusturmaTarihi = DateTime.Now,
                OlusturanKullaniciID = kullaniciId
            };
            
            await _unitOfWork.CariHareketRepository.AddAsync(cariHareket);
            
            _logger.LogInformation($"Faturadan cari hareket oluşturuldu: FaturaID: {fatura.FaturaID}, CariID: {cariHareket.CariID}, Tutar: {cariHareket.Tutar}");
            
            return cariHareket;
        }

        /// <summary>
        /// Fatura silme işlemi için cari hareket kaydını iptal eder
        /// </summary>
        public async Task<bool> IptalEtCariHareketAsync(Guid faturaId)
        {
            // Faturaya ait cari hareketleri bul
            var cariHareketler = await _context.CariHareketler
                .Where(ch => ch.ReferansID == faturaId && ch.ReferansTuru == "Fatura" && !ch.Silindi)
                .ToListAsync();
            
            if (cariHareketler == null || !cariHareketler.Any())
            {
                _logger.LogWarning($"FaturaID: {faturaId} için iptal edilecek cari hareket bulunamadı.");
                return false;
            }
            
            foreach (var hareket in cariHareketler)
            {
                // Cari hareketi silinmiş olarak işaretle
                hareket.Silindi = true;
                hareket.GuncellemeTarihi = DateTime.Now;
                hareket.Aciklama += " (İptal edildi)";
                
                _context.CariHareketler.Update(hareket);
                
                _logger.LogInformation($"Cari hareket iptal edildi: CariHareketID: {hareket.CariHareketID}, FaturaID: {faturaId}");
            }
            
            await _context.SaveChangesAsync();
            
            await _logService.Log(
                $"Fatura silme işlemi için {cariHareketler.Count} adet cari hareket iptal edildi. FaturaID: {faturaId}",
                LogTuru.Bilgi
            );
            
            return true;
        }
    }
} 