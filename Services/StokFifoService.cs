using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using MuhasebeStokWebApp.Data;
using MuhasebeStokWebApp.Data.Entities;
using MuhasebeStokWebApp.Data.Repositories;
using MuhasebeStokWebApp.Services;
using MuhasebeStokWebApp.Services.Interfaces;
using Microsoft.Extensions.Logging;

namespace MuhasebeStokWebApp.Services
{
    /// <summary>
    /// Stok yetersizliği durumunda fırlatılan özel hata sınıfı
    /// </summary>
    public class StokYetersizException : Exception
    {
        public Guid UrunID { get; }
        public string UrunAdi { get; }
        public string UrunKodu { get; }
        public decimal TalepEdilenMiktar { get; }
        public decimal MevcutMiktar { get; }

        public StokYetersizException(Guid urunID, string urunAdi, string urunKodu, decimal talepEdilenMiktar, decimal mevcutMiktar)
            : base($"Stok yetersiz. Ürün: {urunAdi} ({urunKodu}), Talep edilen: {talepEdilenMiktar}, Mevcut: {mevcutMiktar}")
        {
            UrunID = urunID;
            UrunAdi = urunAdi;
            UrunKodu = urunKodu;
            TalepEdilenMiktar = talepEdilenMiktar;
            MevcutMiktar = mevcutMiktar;
        }
    }

    /// <summary>
    /// FIFO (First In, First Out) stok yönetimi için servis sınıfı.
    /// Bu sınıf, ürünlerin stok giriş ve çıkışlarını FIFO prensibine göre yönetir.
    /// </summary>
    public class StokFifoService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ApplicationDbContext _context;
        private readonly IDovizKuruService _dovizKuruService;
        private readonly ILogger<StokFifoService> _logger;

        public StokFifoService(IUnitOfWork unitOfWork, ApplicationDbContext context, IDovizKuruService dovizKuruService, ILogger<StokFifoService> logger)
        {
            _unitOfWork = unitOfWork;
            _context = context;
            _dovizKuruService = dovizKuruService;
            _logger = logger;
        }

        /// <summary>
        /// Stok girişi yapar ve FIFO kaydını oluşturur.
        /// </summary>
        /// <param name="urunID">Ürün ID</param>
        /// <param name="miktar">Giriş miktarı</param>
        /// <param name="birimFiyat">Birim fiyat</param>
        /// <param name="birim">Birim (adet, kg, vb.)</param>
        /// <param name="referansNo">Referans numarası (irsaliye no vb.)</param>
        /// <param name="referansTuru">Referans türü (İrsaliye, Sayım, vb.)</param>
        /// <param name="referansID">Referans ID (İrsaliye ID vb.)</param>
        /// <param name="aciklama">Açıklama</param>
        /// <param name="paraBirimi">Para birimi (USD, TRY, UZS, vb.)</param>
        /// <param name="dovizKuru">Döviz kuru (USD karşılığı)</param>
        /// <returns>Oluşturulan FIFO kaydı</returns>
        public async Task<StokFifo> StokGirisiYap(
            Guid urunID, 
            decimal miktar, 
            decimal birimFiyat, 
            string birim, 
            string referansNo, 
            string referansTuru, 
            Guid? referansID, 
            string aciklama,
            string paraBirimi = "USD",
            decimal? dovizKuru = null)
        {
            // Transaction başlat
            using var transaction = await _context.Database.BeginTransactionAsync();
            
            try
            {
                // Ürün bilgilerini al
                var urun = await _unitOfWork.Repository<Urun>().GetByIdAsync(urunID);
                if (urun == null)
                {
                    throw new ArgumentException($"Ürün bulunamadı. UrunID: {urunID}");
                }
                
                // Döviz kuru kontrolü
                decimal kur = 1;
                if (paraBirimi != "USD")
                {
                    if (!dovizKuru.HasValue || dovizKuru.Value <= 0)
                    {
                        // Güncel kuru veritabanından al
                        var kurDegeri = await _context.KurDegerleri
                            .Where(k => k.ParaBirimiID == Guid.Parse(paraBirimi) && k.Tarih <= DateTime.Now && k.Aktif && !k.Silindi)
                            .OrderByDescending(k => k.Tarih)
                            .FirstOrDefaultAsync();
                            
                            if (kurDegeri != null)
                            {
                                kur = kurDegeri.Satis;
                            }
                            else
                            {
                                throw new ArgumentException($"Geçerli bir döviz kuru bulunamadı. ParaBirimi: {paraBirimi}");
                            }
                    }
                    else
                    {
                        kur = dovizKuru.Value;
                    }
                }
                
                // USD cinsinden birim fiyat hesapla
                decimal usdBirimFiyat;
                if (paraBirimi == "USD")
                {
                    usdBirimFiyat = birimFiyat;
                }
                else
                {
                    usdBirimFiyat = birimFiyat / kur;
                }
                
                // TL ve UZS cinsinden birim fiyatları hesapla
                decimal tlBirimFiyat = 0;
                decimal uzsBirimFiyat = 0;
                
                // TL kurunu al
                decimal tlKurDegeri = 0;
                try
                {
                    tlKurDegeri = await _dovizKuruService.GetGuncelKurAsync("USD", "TRY");
                    tlBirimFiyat = usdBirimFiyat * tlKurDegeri;
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "TL kur değeri alınırken hata oluştu.");
                }
                
                // UZS kurunu al
                decimal uzsKurDegeri = 0;
                try
                {
                    uzsKurDegeri = await _dovizKuruService.GetGuncelKurAsync("USD", "UZS");
                    uzsBirimFiyat = usdBirimFiyat * uzsKurDegeri;
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "UZS kur değeri alınırken hata oluştu.");
                }

                // Yeni FIFO kaydı oluştur
                var fifoKaydi = new StokFifo
                {
                    StokFifoID = Guid.NewGuid(),
                    UrunID = urunID,
                    Miktar = miktar,
                    KalanMiktar = miktar, // Başlangıçta kalan miktar, toplam miktar ile aynıdır
                    BirimFiyat = birimFiyat,
                    ParaBirimi = paraBirimi,
                    DovizKuru = kur,
                    USDBirimFiyat = usdBirimFiyat,
                    TLBirimFiyat = tlBirimFiyat,
                    UZSBirimFiyat = uzsBirimFiyat,
                    GirisTarihi = DateTime.Now,
                    Birim = birim,
                    ReferansNo = referansNo,
                    ReferansTuru = referansTuru,
                    ReferansID = referansID!.Value,
                    Aciklama = aciklama,
                    Aktif = true,
                    OlusturmaTarihi = DateTime.Now
                };

                // FIFO kaydını veritabanına ekle
                await _unitOfWork.Repository<StokFifo>().AddAsync(fifoKaydi);
                await _unitOfWork.SaveAsync();
                
                // Transaction'ı commit et
                await transaction.CommitAsync();

                return fifoKaydi;
            }
            catch (Exception)
            {
                // Hata durumunda rollback yap
                await transaction.RollbackAsync();
                throw;
            }
        }

        /// <summary>
        /// Stok çıkışı yapar ve FIFO prensibine göre stok kayıtlarını günceller.
        /// </summary>
        /// <param name="urunID">Ürün ID</param>
        /// <param name="miktar">Çıkış miktarı</param>
        /// <param name="referansNo">Referans numarası (irsaliye no vb.)</param>
        /// <param name="referansTuru">Referans türü (İrsaliye, Sayım, vb.)</param>
        /// <param name="referansID">Referans ID (İrsaliye ID vb.)</param>
        /// <param name="aciklama">Açıklama</param>
        /// <returns>Çıkış işlemi sonucunda kullanılan FIFO kayıtları ve maliyet bilgisi (USD cinsinden)</returns>
        public async Task<(List<StokFifo> KullanilanFifoKayitlari, decimal ToplamMaliyet)> StokCikisiYap(
            Guid urunID, 
            decimal miktar, 
            string referansNo, 
            string referansTuru, 
            Guid? referansID, 
            string aciklama)
        {
            // Transaction başlat
            using var transaction = await _context.Database.BeginTransactionAsync();
            
            try
            {
                // Ürün bilgilerini al
                var urun = await _unitOfWork.Repository<Urun>().GetByIdAsync(urunID);
                if (urun == null)
                {
                    throw new ArgumentException($"Ürün bulunamadı. UrunID: {urunID}");
                }
                
                // Çıkış yapılacak toplam miktar
                decimal kalanCikisMiktari = miktar;
                
                // Kullanılan FIFO kayıtlarını tutacak liste
                var kullanilanFifoKayitlari = new List<StokFifo>();
                
                // Toplam maliyeti hesaplamak için (USD cinsinden)
                decimal toplamMaliyet = 0;

                // FIFO prensibine göre en eski girişten başlayarak stok çıkışı yap
                var fifoKayitlari = await _context.Set<StokFifo>()
                    .Where(f => f.UrunID == urunID && f.KalanMiktar > 0 && f.Aktif && !f.Silindi && !f.Iptal)
                    .OrderBy(f => f.GirisTarihi)
                    .ToListAsync();

                // Toplam mevcut stok miktarını hesapla
                decimal mevcutStok = fifoKayitlari.Sum(f => f.KalanMiktar);
                
                // Stok yetersizse hata fırlat
                if (mevcutStok < miktar)
                {
                    throw new StokYetersizException(
                        urunID, 
                        urun.UrunAdi, 
                        urun.UrunKodu, 
                        miktar, 
                        mevcutStok);
                }
                
                // Her bir FIFO kaydı için işlem yap
                foreach (var fifoKaydi in fifoKayitlari)
                {
                    // Bu kayıttaki tüm miktar kullanılabilir mi?
                    if (fifoKaydi.KalanMiktar <= kalanCikisMiktari)
                    {
                        // Tüm miktarı kullan
                        decimal kullanilanMiktar = fifoKaydi.KalanMiktar;
                        kalanCikisMiktari -= kullanilanMiktar;
                        
                        // Maliyet hesapla (USD cinsinden)
                        decimal maliyet = kullanilanMiktar * fifoKaydi.USDBirimFiyat;
                        toplamMaliyet += maliyet;
                        
                        // Kaydı güncelle
                        fifoKaydi.KalanMiktar = 0; // Artık bu kayıtta kalan stok yok
                        fifoKaydi.GuncellemeTarihi = DateTime.Now;
                        
                        // Kullanılan kayıtları listeye ekle
                        kullanilanFifoKayitlari.Add(fifoKaydi);
                        
                        // Eğer tüm çıkış tamamlandıysa döngüden çık
                        if (kalanCikisMiktari <= 0)
                            break;
                    }
                    else
                    {
                        // Kısmen kullan
                        decimal kullanilanMiktar = kalanCikisMiktari;
                        fifoKaydi.KalanMiktar -= kullanilanMiktar;
                        kalanCikisMiktari = 0; // Çıkış tamamlandı
                        
                        // Maliyet hesapla (USD cinsinden)
                        decimal maliyet = kullanilanMiktar * fifoKaydi.USDBirimFiyat;
                        toplamMaliyet += maliyet;
                        
                        // Kaydı güncelle
                        fifoKaydi.GuncellemeTarihi = DateTime.Now;
                        
                        // Kullanılan kayıtları listeye ekle
                        kullanilanFifoKayitlari.Add(fifoKaydi);
                        
                        // Çıkış tamamlandı, döngüden çık
                        break;
                    }
                }
                
                // Kullanılan FIFO kayıtlarını veritabanında güncelle
                foreach (var kayit in kullanilanFifoKayitlari)
                {
                    _context.Entry(kayit).State = EntityState.Modified;
                }
                
                // Çıkış hareketini kaydet
                var stokHareket = new StokHareket
                {
                    StokHareketID = Guid.NewGuid(),
                    UrunID = urunID,
                    HareketTuru = "Çıkış",
                    Miktar = miktar,
                    Birim = urun.Birim?.BirimAdi ?? "Adet",
                    Tarih = DateTime.Now,
                    ReferansNo = referansNo,
                    ReferansTuru = referansTuru,
                    ReferansID = referansID,
                    Aciklama = aciklama,
                    BirimFiyat = toplamMaliyet / miktar, // USD cinsinden birim maliyet
                    OlusturmaTarihi = DateTime.Now,
                    Silindi = false
                };
                
                await _unitOfWork.Repository<StokHareket>().AddAsync(stokHareket);
                await _unitOfWork.SaveAsync();
                
                // Transaction'ı commit et
                await transaction.CommitAsync();
                
                return (kullanilanFifoKayitlari, toplamMaliyet);
            }
            catch (Exception)
            {
                // Hata durumunda rollback yap
                await transaction.RollbackAsync();
                throw;
            }
        }

        /// <summary>
        /// İptal edilen bir işlem için FIFO kayıtlarını geri sarar.
        /// </summary>
        /// <param name="referansID">İptal edilen işlemin referans ID'si</param>
        /// <param name="referansTuru">İptal edilen işlemin referans türü</param>
        /// <param name="iptalAciklama">İptal açıklama</param>
        /// <param name="iptalEdenKullaniciID">İptal eden kullanıcı ID'si</param>
        /// <returns>İptal edilen FIFO kayıtları</returns>
        public async Task<List<StokFifo>> FifoKayitlariniIptalEt(
            Guid referansID, 
            string referansTuru, 
            string iptalAciklama, 
            Guid? iptalEdenKullaniciID = null)
        {
            // Transaction başlat
            using var transaction = await _context.Database.BeginTransactionAsync();
            
            try
            {
                // İlgili referansa göre FIFO kayıtlarını al
                var fifoKayitlari = await _context.StokFifo
                    .Where(f => f.ReferansID == referansID && f.ReferansTuru == referansTuru && f.Aktif && !f.Silindi && !f.Iptal)
                    .ToListAsync();
                    
                if (!fifoKayitlari.Any())
                {
                    return new List<StokFifo>();
                }
                
                // Tüm FIFO kayıtlarını iptal et
                foreach (var fifo in fifoKayitlari)
                {
                    fifo.Iptal = true;
                    fifo.IptalTarihi = DateTime.Now;
                    fifo.IptalAciklama = iptalAciklama;
                    fifo.IptalEdenKullaniciID = iptalEdenKullaniciID;
                }
                
                // Değişiklikleri kaydet
                await _unitOfWork.SaveAsync();
                
                // Transaction'ı commit et
                await transaction.CommitAsync();
                
                return fifoKayitlari;
            }
            catch (Exception)
            {
                // Hata durumunda rollback yap
                await transaction.RollbackAsync();
                throw;
            }
        }

        /// <summary>
        /// Belirli bir ürün için mevcut FIFO kayıtlarını getirir.
        /// </summary>
        /// <param name="urunID">Ürün ID</param>
        /// <returns>FIFO kayıtları listesi</returns>
        public async Task<List<StokFifo>> GetFifoKayitlari(Guid urunID)
        {
            return await _context.Set<StokFifo>()
                .Where(f => f.UrunID == urunID && !f.Silindi)
                .OrderBy(f => f.GirisTarihi)
                .ToListAsync();
        }

        /// <summary>
        /// Belirli bir ürün için aktif FIFO kayıtlarını getirir (kalan miktarı 0'dan büyük olanlar).
        /// </summary>
        /// <param name="urunID">Ürün ID</param>
        /// <returns>Aktif FIFO kayıtları listesi</returns>
        public async Task<List<StokFifo>> GetAktifFifoKayitlari(Guid urunID)
        {
            return await _context.Set<StokFifo>()
                .Where(f => f.UrunID == urunID && f.KalanMiktar > 0 && f.Aktif && !f.Silindi && !f.Iptal)
                .OrderBy(f => f.GirisTarihi)
                .ToListAsync();
        }

        /// <summary>
        /// Belirli bir ürün için ortalama maliyet hesaplar.
        /// </summary>
        /// <param name="urunID">Ürün ID</param>
        /// <param name="paraBirimi">Maliyet hesaplanacak para birimi (USD, TRY, UZS)</param>
        /// <returns>Ortalama maliyet (belirtilen para biriminde)</returns>
        public async Task<decimal> GetOrtalamaMaliyet(Guid urunID, string paraBirimi = "USD")
        {
            var aktifKayitlar = await GetAktifFifoKayitlari(urunID);
            
            if (!aktifKayitlar.Any())
                return 0;

            decimal toplamMaliyet = 0;
            decimal toplamMiktar = aktifKayitlar.Sum(f => f.KalanMiktar);

            // Para birimine göre maliyet hesapla
            switch (paraBirimi.ToUpper())
            {
                case "USD":
                    toplamMaliyet = aktifKayitlar.Sum(f => f.KalanMiktar * f.USDBirimFiyat);
                    break;
                case "TRY":
                    toplamMaliyet = aktifKayitlar.Sum(f => f.KalanMiktar * f.TLBirimFiyat);
                    break;
                case "UZS":
                    toplamMaliyet = aktifKayitlar.Sum(f => f.KalanMiktar * f.UZSBirimFiyat);
                    break;
                default:
                    toplamMaliyet = aktifKayitlar.Sum(f => f.KalanMiktar * f.USDBirimFiyat);
                    break;
            }

            return toplamMiktar > 0 ? toplamMaliyet / toplamMiktar : 0;
        }
        
        /// <summary>
        /// Belirli bir referans için FIFO kayıtlarını getirir.
        /// </summary>
        /// <param name="referansID">Referans ID</param>
        /// <param name="referansTuru">Referans türü</param>
        /// <returns>FIFO kayıtları listesi</returns>
        public async Task<List<StokFifo>> GetReferansaGoreFifoKayitlari(Guid referansID, string referansTuru)
        {
            return await _context.Set<StokFifo>()
                .Where(f => f.ReferansID == referansID && f.ReferansTuru == referansTuru && !f.Silindi)
                .OrderBy(f => f.GirisTarihi)
                .ToListAsync();
        }

        /// <summary>
        /// Para birimi dönüştürme işlemi için yöntem.
        /// </summary>
        /// <param name="deger">Dönüştürülecek değer</param>
        /// <param name="kaynakParaBirimi">Kaynak para birimi</param>
        /// <param name="hedefParaBirimi">Hedef para birimi</param>
        /// <returns>Dönüştürülmüş değer</returns>
        public async Task<decimal> ParaBirimiCevirAsync(decimal deger, string kaynakParaBirimi, string hedefParaBirimi)
        {
            if (kaynakParaBirimi == hedefParaBirimi)
                return deger;

            // UZS'yi TL'ye çevirme örneği
            if (kaynakParaBirimi == "2" && hedefParaBirimi == "1") // UZS -> TL
            {
                var uzsKurDegeri = await _context.KurDegerleri
                    .Where(k => k.ParaBirimiID == Guid.Parse("00000000-0000-0000-0000-000000000002")) // UZS para birimi ID'si
                    .OrderByDescending(k => k.Tarih)
                    .FirstOrDefaultAsync();

                if (uzsKurDegeri == null)
                    throw new Exception("UZS için kur değeri bulunamadı.");

                // Önce UZS'yi USD'ye çevir
                var usdDegeri = deger / uzsKurDegeri.Satis;

                // Sonra USD'yi TL'ye çevir
                var tlKurDegeri = await _context.KurDegerleri
                    .Where(k => k.ParaBirimiID == Guid.Parse(hedefParaBirimi)) // TL para birimi ID'si
                    .OrderByDescending(k => k.Tarih)
                    .FirstOrDefaultAsync();

                if (tlKurDegeri == null)
                    throw new Exception("TL için kur değeri bulunamadı.");

                return usdDegeri * tlKurDegeri.Satis;
            }
            else
            {
                // Diğer para birimi çevrimleri için benzer mantık uygulanabilir
                return deger; // Şimdilik basit bir değer dönüşü
            }
        }

        // DovizKurlari referansları olan metodlar
        
        // TRY/USD kuru alımı için
        public async Task<decimal> GetTryToUsdKurAsync()
        {
            try 
            {
                return await _dovizKuruService.GetGuncelKurAsync("TRY", "USD");
            }
            catch
            {
                return 0.03m; // Default değer
            }
        }
        
        // USD/TL kuru alımı için
        public async Task<decimal> GetUsdToTryKurAsync()
        {
            try
            {
                return await _dovizKuruService.GetGuncelKurAsync("USD", "TRY");
            }
            catch
            {
                return 33.0m; // Default değer
            }
        }
        
        // UZS/TL kuru alımı için
        public async Task<decimal> GetUzsToTryKurAsync()
        {
            try
            {
                return await _dovizKuruService.GetGuncelKurAsync("UZS", "TRY");
            }
            catch
            {
                return 0.0025m; // Default değer
            }
        }

        // Stok maliyeti hesapla metodu içindeki kurDegeri değişkeni tanımlama
        public async Task<decimal> HesaplaMaliyetAsync(Guid stokId, DateTime tarih)
        {
            var stokFifoKayitlari = await _context.StokFifo
                .Where(s => s.UrunID == stokId && s.GirisTarihi <= tarih && s.KalanMiktar > 0)
                .OrderBy(s => s.GirisTarihi)
                .ToListAsync();

            if (stokFifoKayitlari == null || !stokFifoKayitlari.Any())
            {
                return 0;
            }

            decimal toplamMaliyet = 0;
            decimal toplamMiktar = 0;

            foreach (var kayit in stokFifoKayitlari)
            {
                toplamMaliyet += kayit.BirimFiyat * kayit.KalanMiktar;
                toplamMiktar += kayit.KalanMiktar;
            }

            if (toplamMiktar > 0)
            {
                return toplamMaliyet / toplamMiktar;
            }

            return 0;
        }

        public async Task<decimal> HesaplaMaliyetDovizliAsync(Guid stokId, DateTime tarih, Guid? dovizId = null)
        {
            decimal maliyet = await HesaplaMaliyetAsync(stokId, tarih);
            decimal kur = 1;

            if (dovizId.HasValue)
            {
                var dovizKuru = await _context.KurDegerleri
                    .Where(k => k.ParaBirimiID == dovizId.Value && k.Tarih <= tarih && k.Aktif && !k.Silindi)
                    .OrderByDescending(k => k.Tarih)
                    .FirstOrDefaultAsync();

                if (dovizKuru != null)
                {
                    kur = dovizKuru.Satis;
                }
            }

            return Math.Round(maliyet / kur, 2);
        }

        public async Task<List<StokFifo>> GetStokFifoAsync(Guid stokId)
        {
            return await _context.StokFifo
                .Where(s => s.UrunID == stokId && s.KalanMiktar > 0)
                .OrderBy(s => s.GirisTarihi)
                .ToListAsync();
        }

        public async Task<StokFifo> AddStokFifoAsync(StokFifo stokFifo)
        {
            await _context.StokFifo.AddAsync(stokFifo);
            await _context.SaveChangesAsync();
            return stokFifo;
        }

        public async Task UpdateStokFifoAsync(StokFifo stokFifo)
        {
            _context.StokFifo.Update(stokFifo);
            await _context.SaveChangesAsync();
        }

        public async Task<decimal> DovizliFiyatHesapla(decimal fiyat, string kaynakParaBirimi, string hedefParaBirimi)
        {
            if (kaynakParaBirimi == hedefParaBirimi)
                return fiyat;

            // Kaynak para biriminden USD'ye çevir
            var kaynakKurDegeri = await _context.KurDegerleri
                .Where(k => k.ParaBirimiID == Guid.Parse(kaynakParaBirimi))
                .OrderByDescending(k => k.Tarih)
                .FirstOrDefaultAsync();

            if (kaynakKurDegeri == null)
                throw new Exception($"{kaynakParaBirimi} para birimi için kur değeri bulunamadı.");

            decimal usdFiyat = fiyat / kaynakKurDegeri.Satis;

            // USD'den hedef para birimine çevir
            var hedefKurDegeri = await _context.KurDegerleri
                .Where(k => k.ParaBirimiID == Guid.Parse(hedefParaBirimi))
                .OrderByDescending(k => k.Tarih)
                .FirstOrDefaultAsync();

            if (hedefKurDegeri == null)
                throw new Exception($"{hedefParaBirimi} para birimi için kur değeri bulunamadı.");

            return usdFiyat * hedefKurDegeri.Satis;
        }
    }
} 