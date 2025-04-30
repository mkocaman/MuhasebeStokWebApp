using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using MuhasebeStokWebApp.Data;
using MuhasebeStokWebApp.Data.Entities;
using MuhasebeStokWebApp.Data.Repositories;
using MuhasebeStokWebApp.Enums;
using MuhasebeStokWebApp.Services.Interfaces;
using MuhasebeStokWebApp.ViewModels.Fatura;

namespace MuhasebeStokWebApp.Services
{
    public class StokHareketService : IStokHareketService
    {
        private readonly ApplicationDbContext _context;
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<StokHareketService> _logger;
        private readonly IStokFifoService _stokFifoService;

        public StokHareketService(
            ApplicationDbContext context,
            IUnitOfWork unitOfWork,
            ILogger<StokHareketService> logger,
            IStokFifoService stokFifoService)
        {
            _context = context;
            _unitOfWork = unitOfWork;
            _logger = logger;
            _stokFifoService = stokFifoService;
        }

        /// <summary>
        /// Fatura detaylarından stok hareketi oluşturur
        /// </summary>
        public async Task<List<StokHareket>> CreateStokHareket(Fatura fatura, List<FaturaDetay> faturaDetaylari, Guid? kullaniciId = null)
        {
            if (fatura == null) throw new ArgumentNullException(nameof(fatura));
            if (faturaDetaylari == null) throw new ArgumentNullException(nameof(faturaDetaylari));

            // Fatura türüne göre stok hareket tipini belirle
            var stokHareketTipi = StokHareketiTipi.Giris; // Varsayılan değer
            
            // Fatura türünü yükle (eğer yüklenmemişse)
            if (fatura.FaturaTuru == null && fatura.FaturaTuruID.HasValue)
            {
                await _context.Entry(fatura).Reference(f => f.FaturaTuru).LoadAsync();
            }
            
            if (fatura.FaturaTuru != null)
            {
                stokHareketTipi = fatura.FaturaTuru.HareketTuru == "Çıkış" 
                    ? StokHareketiTipi.Cikis 
                    : StokHareketiTipi.Giris;
            }
            
            string irsaliyeTuruStr = stokHareketTipi == StokHareketiTipi.Giris ? "Giriş" : "Çıkış";
            
            var stokHareketleri = new List<StokHareket>();
            foreach (var detay in faturaDetaylari)
            {
                var stokHareket = new StokHareket
                {
                    StokHareketID = Guid.NewGuid(),
                    UrunID = detay.UrunID,
                    DepoID = null, // Fatura'da DepoID olmadığı için null
                    Miktar = stokHareketTipi == StokHareketiTipi.Cikis ? -detay.Miktar : detay.Miktar,
                    Birim = detay.Birim ?? "Adet",
                    HareketTuru = stokHareketTipi,
                    Tarih = fatura.FaturaTarihi ?? DateTime.Now,
                    ReferansNo = fatura.FaturaNumarasi ?? "",
                    ReferansTuru = "Fatura",
                    ReferansID = fatura.FaturaID,
                    FaturaID = fatura.FaturaID,
                    Aciklama = $"{fatura.FaturaNumarasi} numaralı fatura",
                    BirimFiyat = detay.BirimFiyat,
                    OlusturmaTarihi = DateTime.Now,
                    IslemYapanKullaniciID = kullaniciId,
                    IrsaliyeID = null,
                    IrsaliyeTuru = irsaliyeTuruStr,
                    ParaBirimi = fatura.DovizTuru ?? "TRY"
                };
                
                stokHareketleri.Add(stokHareket);
            }
            
            // Stok hareketlerini ekle
            await _unitOfWork.StokHareketRepository.AddRangeAsync(stokHareketleri);
            
            return stokHareketleri;
        }

        /// <summary>
        /// Mevcut Fatura için stok seviyesini hesaplar
        /// </summary>
        public async Task UpdateStokSeviyeleri(Fatura fatura)
        {
            if (fatura == null) throw new ArgumentNullException(nameof(fatura));
            
            // Fatura detaylarını yükle
            if (!_context.Entry(fatura).Collection(f => f.FaturaDetaylari).IsLoaded)
            {
                await _context.Entry(fatura).Collection(f => f.FaturaDetaylari).LoadAsync();
            }
            
            // Fatura türünü yükle
            if (fatura.FaturaTuru == null && fatura.FaturaTuruID.HasValue)
            {
                await _context.Entry(fatura).Reference(f => f.FaturaTuru).LoadAsync();
            }
            
            if (fatura.FaturaDetaylari == null || !fatura.FaturaDetaylari.Any())
            {
                _logger.LogWarning($"Fatura ID: {fatura.FaturaID} için stok seviyeleri güncellenemedi, fatura detayları bulunamadı.");
                return;
            }
            
            var stokHareketTipi = StokHareketiTipi.Giris; // Varsayılan değer
            
            if (fatura.FaturaTuru != null)
            {
                stokHareketTipi = fatura.FaturaTuru.HareketTuru == "Çıkış" 
                    ? StokHareketiTipi.Cikis 
                    : StokHareketiTipi.Giris;
            }
            
            foreach (var detay in fatura.FaturaDetaylari)
            {
                var urun = await _context.Urunler.FindAsync(detay.UrunID);
                if (urun != null)
                {
                    // Stok hareketine göre miktarı ekle veya çıkar
                    if (stokHareketTipi == StokHareketiTipi.Giris)
                    {
                        urun.StokMiktar += detay.Miktar;
                    }
                    else
                    {
                        urun.StokMiktar -= detay.Miktar;
                    }
                    
                    // Ürünü güncelle
                    _context.Urunler.Update(urun);
                }
            }
        }

        /// <summary>
        /// Fatura detaylarından stok çıkışı yapar
        /// </summary>
        public async Task<decimal> StokCikisiYap(Guid urunId, decimal miktar, Guid? referansId = null, string aciklama = null)
        {
            var result = await _stokFifoService.StokCikisiYap(
                urunId,
                miktar,
                referansId.HasValue ? referansId.ToString() : "Manuel",
                "StokCikis",
                referansId,
                aciklama ?? $"Stok çıkışı: {miktar}"
            );
            
            // Ürün stok miktarını güncelle
            var urun = await _context.Urunler.FindAsync(urunId);
            if (urun != null)
            {
                urun.StokMiktar -= miktar;
                _context.Urunler.Update(urun);
            }
            
            return result.ToplamMaliyet;
        }

        /// <summary>
        /// Fatura detaylarından stok girişi yapar
        /// </summary>
        public async Task<decimal> StokGirisiYap(Guid urunId, decimal miktar, Guid? referansId = null, string aciklama = null)
        {
            // Ürünün son birim fiyatını al
            var sonFiyat = await _context.StokHareketleri
                .Where(sh => sh.UrunID == urunId && !sh.Silindi)
                .OrderByDescending(sh => sh.Tarih)
                .Select(sh => sh.BirimFiyat)
                .FirstOrDefaultAsync() ?? 0;
            
            var fifoKaydi = await _stokFifoService.StokGirisiYap(
                urunId,
                miktar,
                sonFiyat,
                "Adet",
                referansId.HasValue ? referansId.ToString() : "Manuel",
                "StokGiris",
                referansId,
                aciklama ?? $"Stok girişi: {miktar}",
                "TRY",
                1
            );
            
            // Ürün stok miktarını güncelle
            var urun = await _context.Urunler.FindAsync(urunId);
            if (urun != null)
            {
                urun.StokMiktar += miktar;
                _context.Urunler.Update(urun);
            }
            
            return sonFiyat * miktar;
        }

        /// <summary>
        /// Fatura silme sırasında ilişkili stok hareketlerini iptal eder
        /// </summary>
        public async Task IptalEtStokHareketleri(Guid faturaId)
        {
            // Faturaya ait stok hareketlerini bul
            var stokHareketleri = await _context.StokHareketleri
                .Where(sh => sh.FaturaID == faturaId && !sh.Silindi)
                .ToListAsync();
            
            if (stokHareketleri == null || !stokHareketleri.Any())
            {
                _logger.LogWarning($"FaturaID: {faturaId} için iptal edilecek stok hareketi bulunamadı.");
                return;
            }
            
            foreach (var hareket in stokHareketleri)
            {
                // Stok hareketini iptal et
                hareket.Silindi = true;
                hareket.GuncellemeTarihi = DateTime.Now;
                hareket.Aciklama += " (İptal edildi)";
                
                // Ürün stok miktarını güncelle
                var urun = await _context.Urunler.FindAsync(hareket.UrunID);
                if (urun != null)
                {
                    urun.StokMiktar -= hareket.Miktar; // Tersini uygula
                    _context.Urunler.Update(urun);
                }
            }
            
            // FIFO kayıtlarını iptal et
            await _stokFifoService.FifoKayitlariniIptalEt(
                faturaId, 
                "Fatura", 
                "Fatura silindi",
                null);
            
            // Stok hareketlerini güncelle
            _context.StokHareketleri.UpdateRange(stokHareketleri);
        }
    }
} 