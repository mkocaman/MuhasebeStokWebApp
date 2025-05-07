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
            var stokHareketTipi = StokHareketTipi.Giris; // Varsayılan değer
            
            // Fatura türünü yükle (eğer yüklenmemişse)
            if (fatura.FaturaTuru == null && fatura.FaturaTuruID.HasValue)
            {
                await _context.Entry(fatura).Reference(f => f.FaturaTuru).LoadAsync();
            }
            
            if (fatura.FaturaTuru != null)
            {
                stokHareketTipi = fatura.FaturaTuru.HareketTuru == "Çıkış" 
                    ? StokHareketTipi.Cikis 
                    : StokHareketTipi.Giris;
            }
            else
            {
                // Fatura türü bilinmiyor, ID'ye göre belirle (1: Alış = Giriş, 2: Satış = Çıkış)
                stokHareketTipi = fatura.FaturaTuruID == 1 
                    ? StokHareketTipi.Giris 
                    : StokHareketTipi.Cikis;
            }
            
            string irsaliyeTuruStr = stokHareketTipi == StokHareketTipi.Giris ? "Giriş" : "Çıkış";
            
            var stokHareketleri = new List<StokHareket>();
            foreach (var detay in faturaDetaylari)
            {
                // Döviz hesaplamaları
                decimal birimFiyatUSD, birimFiyatUZS;
                
                // Para birimi USD ise: BirimFiyat × DovizKuru = UZS BirimFiyat
                // Para birimi UZS ise: BirimFiyat ÷ DovizKuru = USD BirimFiyat
                if (fatura.ParaBirimi == "USD")
                {
                    birimFiyatUSD = detay.BirimFiyat;
                    birimFiyatUZS = detay.BirimFiyat * (fatura.DovizKuru ?? 13000); // Varsayılan kur değeri
                    _logger.LogInformation($"USD -> UZS dönüşümü: {detay.BirimFiyat} USD * {fatura.DovizKuru ?? 13000} = {birimFiyatUZS} UZS");
                }
                else if (fatura.ParaBirimi == "UZS")
                {
                    birimFiyatUZS = detay.BirimFiyat;
                    birimFiyatUSD = detay.BirimFiyat / (fatura.DovizKuru ?? 13000); // Varsayılan kur değeri
                    _logger.LogInformation($"UZS -> USD dönüşümü: {detay.BirimFiyat} UZS / {fatura.DovizKuru ?? 13000} = {birimFiyatUSD} USD");
                }
                else
                {
                    // Desteklenmeyen para birimi
                    birimFiyatUSD = detay.BirimFiyat;
                    birimFiyatUZS = detay.BirimFiyat * (fatura.DovizKuru ?? 13000);
                    _logger.LogWarning($"Desteklenmeyen para birimi: {fatura.ParaBirimi}. USD olarak devam ediliyor.");
                }
                
                var stokHareket = new StokHareket
                {
                    StokHareketID = Guid.NewGuid(),
                    UrunID = detay.UrunID,
                    DepoID = null, // Fatura'da DepoID olmadığı için null
                    Miktar = stokHareketTipi == StokHareketTipi.Cikis ? -detay.Miktar : detay.Miktar,
                    Birim = detay.Birim ?? "Adet",
                    HareketTuru = stokHareketTipi,
                    Tarih = fatura.FaturaTarihi ?? DateTime.Now,
                    ReferansNo = fatura.FaturaNumarasi ?? "",
                    ReferansTuru = "Fatura",
                    ReferansID = fatura.FaturaID,
                    FaturaID = fatura.FaturaID,
                    Aciklama = $"{fatura.FaturaNumarasi} numaralı fatura",
                    BirimFiyat = detay.BirimFiyat,
                    BirimFiyatUSD = birimFiyatUSD,
                    BirimFiyatUZS = birimFiyatUZS,
                    OlusturmaTarihi = DateTime.Now,
                    IslemYapanKullaniciID = kullaniciId,
                    IrsaliyeID = null,
                    IrsaliyeTuru = irsaliyeTuruStr,
                    ParaBirimi = fatura.ParaBirimi ?? "USD"
                };
                
                stokHareketleri.Add(stokHareket);
                
                _logger.LogInformation($"Stok hareketi oluşturuldu: UrunID={detay.UrunID}, Miktar={(stokHareketTipi == StokHareketTipi.Cikis ? -detay.Miktar : detay.Miktar)}, BirimFiyat={detay.BirimFiyat} {fatura.ParaBirimi}, USD={birimFiyatUSD}, UZS={birimFiyatUZS}");
            }
            
            // Stok hareketlerini ekle
            await _unitOfWork.StokHareketRepository.AddRangeAsync(stokHareketleri);
            // Transaction dıştan yönetiliyorsa değişiklikleri kaydet
            if (_context.Database.CurrentTransaction != null)
            {
                _logger.LogInformation($"Stok hareketleri mevcut transaction içinde kaydedildi: {stokHareketleri.Count} adet");
            }
            else
            {
                // Transaction yoksa hemen kaydet
                await _unitOfWork.SaveChangesAsync();
                _logger.LogInformation($"Stok hareketleri veritabanına kaydedildi: {stokHareketleri.Count} adet");
            }
            
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
            
            var stokHareketTipi = StokHareketTipi.Giris; // Varsayılan değer
            
            if (fatura.FaturaTuru != null)
            {
                stokHareketTipi = fatura.FaturaTuru.HareketTuru == "Çıkış" 
                    ? StokHareketTipi.Cikis 
                    : StokHareketTipi.Giris;
            }
            
            foreach (var detay in fatura.FaturaDetaylari)
            {
                var urun = await _context.Urunler.FindAsync(detay.UrunID);
                if (urun != null)
                {
                    // Stok hareketine göre miktarı ekle veya çıkar
                    if (stokHareketTipi == StokHareketTipi.Giris)
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
                StokHareketTipi.Cikis,
                referansId,
                aciklama ?? $"Stok çıkışı: {miktar}",
                "USD", // Varsayılan para birimi USD
                false
            );
            
            // Ürün stok miktarını güncelle
            var urun = await _context.Urunler.FindAsync(urunId);
            if (urun != null)
            {
                urun.StokMiktar -= miktar;
                _context.Urunler.Update(urun);
                
                // Stok hareketi oluştur
                var stokHareket = new StokHareket
                {
                    StokHareketID = Guid.NewGuid(),
                    UrunID = urunId,
                    DepoID = null,
                    Miktar = -miktar, // Eksi değer (çıkış)
                    Birim = urun.Birim?.ToString() ?? "Adet",
                    HareketTuru = StokHareketTipi.Cikis,
                    Tarih = DateTime.Now,
                    ReferansNo = referansId?.ToString() ?? "Manuel",
                    ReferansTuru = "StokCikis",
                    ReferansID = referansId ?? Guid.Empty,
                    Aciklama = aciklama ?? $"Stok çıkışı: {miktar}",
                    BirimFiyat = result.ToplamMaliyet / miktar, // Birim maliyet
                    BirimFiyatUSD = result.ToplamMaliyet / miktar, // USD cinsinden maliyet
                    BirimFiyatUZS = (result.ToplamMaliyet / miktar) * 13000, // UZS cinsinden maliyet (varsayılan kur)
                    OlusturmaTarihi = DateTime.Now,
                    ParaBirimi = "USD"
                };
                
                // Stok hareketini ekle
                await _unitOfWork.StokHareketRepository.AddAsync(stokHareket);
                _logger.LogInformation($"Stok çıkış hareketi oluşturuldu: UrunID={urunId}, Miktar={-miktar}, Maliyet={result.ToplamMaliyet}");
            }
            
            return result.ToplamMaliyet;
        }

        /// <summary>
        /// Fatura detaylarından stok girişi yapar
        /// </summary>
        public async Task<decimal> StokGirisiYap(Guid urunId, decimal miktar, decimal birimFiyat, string paraBirimi = "USD", decimal? dovizKuru = null, Guid? referansId = null, string aciklama = null)
        {
            // Döviz kuru kontrolü
            dovizKuru = dovizKuru ?? 13000; // Varsayılan kur değeri
            
            // Döviz hesaplamaları
            decimal birimFiyatUSD, birimFiyatUZS;
            
            // Para birimi USD ise: BirimFiyat × DovizKuru = UZS BirimFiyat
            // Para birimi UZS ise: BirimFiyat ÷ DovizKuru = USD BirimFiyat
            if (paraBirimi == "USD")
            {
                birimFiyatUSD = birimFiyat;
                birimFiyatUZS = birimFiyat * dovizKuru.Value;
            }
            else if (paraBirimi == "UZS")
            {
                birimFiyatUZS = birimFiyat;
                birimFiyatUSD = birimFiyat / dovizKuru.Value;
            }
            else
            {
                // Desteklenmeyen para birimi
                birimFiyatUSD = birimFiyat;
                birimFiyatUZS = birimFiyat * dovizKuru.Value;
                _logger.LogWarning($"Desteklenmeyen para birimi: {paraBirimi}. USD olarak devam ediliyor.");
                paraBirimi = "USD";
            }
            
            var fifoKaydi = await _stokFifoService.StokGirisiYap(
                urunId,
                miktar,
                birimFiyat,
                "Adet",
                referansId.HasValue ? referansId.ToString() : "Manuel",
                "StokGiris",
                referansId,
                aciklama ?? $"Stok girişi: {miktar}",
                paraBirimi,
                dovizKuru
            );
            
            // Ürün stok miktarını güncelle
            var urun = await _context.Urunler.FindAsync(urunId);
            if (urun != null)
            {
                urun.StokMiktar += miktar;
                _context.Urunler.Update(urun);
                
                // Stok hareketi oluştur
                var stokHareket = new StokHareket
                {
                    StokHareketID = Guid.NewGuid(),
                    UrunID = urunId,
                    DepoID = null,
                    Miktar = miktar, // Artı değer (giriş)
                    Birim = urun.Birim?.ToString() ?? "Adet",
                    HareketTuru = StokHareketTipi.Giris,
                    Tarih = DateTime.Now,
                    ReferansNo = referansId?.ToString() ?? "Manuel",
                    ReferansTuru = "StokGiris",
                    ReferansID = referansId ?? Guid.Empty,
                    Aciklama = aciklama ?? $"Stok girişi: {miktar}",
                    BirimFiyat = birimFiyat,
                    BirimFiyatUSD = birimFiyatUSD,
                    BirimFiyatUZS = birimFiyatUZS,
                    OlusturmaTarihi = DateTime.Now,
                    ParaBirimi = paraBirimi
                };
                
                // Stok hareketini ekle
                await _unitOfWork.StokHareketRepository.AddAsync(stokHareket);
                _logger.LogInformation($"Stok giriş hareketi oluşturuldu: UrunID={urunId}, Miktar={miktar}, BirimFiyat={birimFiyat} {paraBirimi}, USD={birimFiyatUSD}, UZS={birimFiyatUZS}");
            }
            
            return birimFiyat * miktar;
        }

        // Eski metot için overload, eski kodları etkilememek için
        public async Task<decimal> StokGirisiYap(Guid urunId, decimal miktar, Guid? referansId = null, string aciklama = null)
        {
            // Ürünün son birim fiyatını al
            var sonFiyat = await _context.StokHareketleri
                .Where(sh => sh.UrunID == urunId && !sh.Silindi)
                .OrderByDescending(sh => sh.Tarih)
                .Select(sh => sh.BirimFiyat)
                .FirstOrDefaultAsync() ?? 0;
                
            return await StokGirisiYap(urunId, miktar, sonFiyat, "USD", null, referansId, aciklama);
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