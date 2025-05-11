using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using MuhasebeStokWebApp.Data;
using MuhasebeStokWebApp.Data.Entities;
using MuhasebeStokWebApp.Data.Repositories;
using MuhasebeStokWebApp.Enums;
using MuhasebeStokWebApp.Services;
using MuhasebeStokWebApp.Services.Interfaces;
using Microsoft.Extensions.Logging;
using System.Threading;
using MuhasebeStokWebApp.Models;

namespace MuhasebeStokWebApp.Services
{
    /// <summary>
    /// FIFO işlemi sırasında oluşabilecek concurrency ihlalleri için özel hata sınıfı
    /// </summary>
    public class FifoConcurrencyException : Exception
    {
        public Guid StokFifoID { get; }
        public int RetryCount { get; }

        public FifoConcurrencyException(Guid stokFifoID, int retryCount, string message) 
            : base(message)
        {
            StokFifoID = stokFifoID;
            RetryCount = retryCount;
        }
    }

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
    /// FIFO (First In, First Out) stok yönetimi için refactor edilmiş servis sınıfı.
    /// Bu sınıf, ürünlerin stok giriş ve çıkışlarını FIFO prensibine göre yönetir.
    /// </summary>
    public class StokFifoService : IStokFifoService
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<StokFifoService> _logger;
        private readonly IMaliyetHesaplamaService _maliyetHesaplamaService;
        private readonly IDovizKuruService _dovizKuruService;

        public StokFifoService(
            ApplicationDbContext context,
            ILogger<StokFifoService> logger,
            IMaliyetHesaplamaService maliyetHesaplamaService,
            IDovizKuruService dovizKuruService)
        {
            _context = context;
            _logger = logger;
            _maliyetHesaplamaService = maliyetHesaplamaService;
            _dovizKuruService = dovizKuruService;
        }

        #region Stok FIFO işlemleri

        /// <summary>
        /// Stok girişi yapan detaylı metot. Tüm parametreleri alır ve FIFO kaydı oluşturur.
        /// </summary>
        public async Task<StokFifo> StokGirisiYap(Guid urunID, decimal miktar, decimal birimFiyat, string birim, string referansNo, string referansTuru, Guid? referansID, string aciklama, string paraBirimi = "USD", decimal? dovizKuru = null)
        {
            // Transaction yönetimi - iç içe transaction olup olmadığını kontrol et
            bool hasExistingTransaction = _context.Database.CurrentTransaction != null;
            IDbContextTransaction transaction = null;
            
            try
            {
                // Sadece dış transaction yoksa yeni bir transaction başlat
                if (!hasExistingTransaction)
                {
                    transaction = await _context.Database.BeginTransactionAsync(System.Data.IsolationLevel.ReadCommitted);
                    _logger.LogInformation($"StokGirisiYap - Yeni transaction başlatıldı (Isolation Level: ReadCommitted): Ürün {urunID}, Miktar {miktar}");
                }
                else
                {
                    _logger.LogInformation($"StokGirisiYap - Mevcut transaction kullanılıyor: Ürün {urunID}, Miktar {miktar}");
                }
                
                // Null kontrolü ve varsayılan değerler
                referansNo = referansNo ?? string.Empty;
                referansTuru = referansTuru ?? string.Empty;
                aciklama = aciklama ?? $"Stok girişi: {miktar} {birim}";
                paraBirimi = string.IsNullOrEmpty(paraBirimi) ? "USD" : paraBirimi;
                
                // Döviz kuru kontrolü
                if (paraBirimi != "USD" && (!dovizKuru.HasValue || dovizKuru.Value <= 0))
                {
                    throw new ArgumentException("USD dışında bir para birimi için döviz kuru zorunludur", nameof(dovizKuru));
                }

                // Stok borçlanma/negatif kayıtları kontrol et
                decimal girisKalanMiktar = miktar;
                decimal mahsuplasilanMiktar = 0;

                // Ürünün negatif FIFO kayıtlarını getir
                var negatifKayitlar = await _context.StokFifoKayitlari
                    .Where(f => f.UrunID == urunID && f.Aktif && !f.Iptal && !f.Silindi && f.KalanMiktar < 0)
                    .OrderBy(f => f.GirisTarihi)
                    .ToListAsync();

                // Negatif kayıtları mahsuplaştır
                foreach (var negatifKayit in negatifKayitlar)
                {
                    if (girisKalanMiktar <= 0)
                        break;
                    
                    // Negatif miktarın mutlak değeri
                    decimal negatifMiktar = Math.Abs(negatifKayit.KalanMiktar);
                    // Mahsuplaşılacak miktar
                    decimal mahsupMiktari = Math.Min(negatifMiktar, girisKalanMiktar);
                    
                    // Negatif FIFO kaydının kalan miktarını güncelle
                    negatifKayit.KalanMiktar += mahsupMiktari; // Negatif değere eklenince sıfıra yaklaşır
                    negatifKayit.GuncellemeTarihi = DateTime.Now;
                    if (negatifKayit.KalanMiktar == 0)
                    {
                        // Tamamen mahsuplaşmış, artık aktif değil
                        negatifKayit.Aktif = false;
                    }
                    
                    _context.StokFifoKayitlari.Update(negatifKayit);
                    
                    // Giriş yapılacak kalanı azalt
                    girisKalanMiktar -= mahsupMiktari;
                    mahsuplasilanMiktar += mahsupMiktari;
                    
                    _logger.LogInformation($"Negatif stok mahsuplaşması yapıldı: UrunID={urunID}, MahsupMiktari={mahsupMiktari}, NegatifFifoID={negatifKayit.StokFifoID}");
                }
                
                // StokFifo girişi oluştur (eğer hala kalan miktar varsa)
                StokFifo stokFifo = null;
                if (girisKalanMiktar > 0)
                {
                    stokFifo = new StokFifo
                    {
                        StokFifoID = Guid.NewGuid(),
                        UrunID = urunID,
                        Miktar = girisKalanMiktar,
                        KalanMiktar = girisKalanMiktar,
                        BirimFiyat = birimFiyat,
                        // Para birimine göre doğru dönüşüm hesaplamaları
                        BirimFiyatUSD = paraBirimi == "USD" ? birimFiyat : birimFiyat / (dovizKuru ?? 13000m),
                        BirimFiyatUZS = paraBirimi == "UZS" ? birimFiyat : birimFiyat * (dovizKuru ?? 13000m),
                        ParaBirimi = paraBirimi,
                        DovizKuru = dovizKuru.HasValue ? dovizKuru.Value : 1m,
                        GirisTarihi = DateTime.Now,
                        OlusturmaTarihi = DateTime.Now,
                        Aktif = true,
                        Iptal = false,
                        Silindi = false,
                        ReferansNo = referansNo,
                        ReferansTuru = referansTuru,
                        ReferansID = referansID,
                        Aciklama = mahsuplasilanMiktar > 0 
                            ? $"{aciklama} (Negatif stok mahsuplaşması: {mahsuplasilanMiktar})" 
                            : aciklama
                    };
                    
                    _context.StokFifoKayitlari.Add(stokFifo);
                }
                else if (mahsuplasilanMiktar > 0)
                {
                    // Tüm miktar mahsuplaştırıldı, hiç giriş kaydı oluşturmadık
                    _logger.LogInformation($"Tüm stok girişi negatif bakiyelerle mahsuplaştı: UrunID={urunID}, Miktar={miktar}");
                    
                    // Sadece log için bir sahte giriş nesnesi oluştur
                    stokFifo = new StokFifo
                    {
                        StokFifoID = Guid.NewGuid(),
                        UrunID = urunID,
                        Miktar = miktar,
                        KalanMiktar = 0, // Tamamen mahsuplaştı
                        BirimFiyat = birimFiyat,
                        // Para birimine göre doğru dönüşüm hesaplamaları
                        BirimFiyatUSD = paraBirimi == "USD" ? birimFiyat : birimFiyat / (dovizKuru ?? 13000m),
                        BirimFiyatUZS = paraBirimi == "UZS" ? birimFiyat : birimFiyat * (dovizKuru ?? 13000m),
                        ParaBirimi = paraBirimi,
                        DovizKuru = dovizKuru.HasValue ? dovizKuru.Value : 1m,
                        GirisTarihi = DateTime.Now,
                        OlusturmaTarihi = DateTime.Now,
                        Aktif = false, // Aktif değil çünkü hepsi mahsuplaştı
                        Iptal = false,
                        Silindi = false,
                        ReferansNo = referansNo,
                        ReferansTuru = referansTuru,
                        ReferansID = referansID,
                        Aciklama = $"Tamamen mahsuplaşan stok girişi: {miktar} {birim}"
                    };
                    
                    // Bu kayıt veritabanına eklenmeyecek, sadece log ve dönüş değeri için
                }
                
                await _context.SaveChangesAsync();
                
                // Sadece biz başlattıysak transaction'ı commit et
                if (!hasExistingTransaction && transaction != null)
                {
                    await transaction.CommitAsync();
                    _logger.LogInformation($"StokGirisiYap - Transaction commit edildi: {urunID}, {miktar}, {birimFiyat}");
                }
                
                _logger.LogInformation($"Stok girişi başarıyla tamamlandı: UrunID={urunID}, Miktar={miktar}, BirimFiyat={birimFiyat}, ParaBirimi={paraBirimi}, Mahsuplaşan={mahsuplasilanMiktar}");
                return stokFifo;
            }
            catch (Exception ex)
            {
                // Sadece biz başlattıysak transaction'ı geri al
                if (!hasExistingTransaction && transaction != null)
                {
                    await transaction.RollbackAsync();
                    _logger.LogWarning($"StokGirisiYap - Transaction geri alındı: {urunID}, {miktar}, {birimFiyat}");
                }
                
                _logger.LogError(ex, $"Stok girişi sırasında hata oluştu: UrunID={urunID}, Miktar={miktar}, BirimFiyat={birimFiyat}");
                throw;
            }
            finally
            {
                // Sadece biz başlattıysak transaction'ı dispose et
                if (!hasExistingTransaction && transaction != null)
                {
                    await transaction.DisposeAsync();
                }
            }
        }

        /// <summary>
        /// Stok girişi yapan basit metot - interface uyumluluğu için
        /// </summary>
        public async Task<bool> StokGirisAsync(StokFifo stokFifo)
        {
            try
            {
                // Giriş tarihi ve oluşturma tarihi kontrolü
                if (stokFifo.GirisTarihi == default)
                    stokFifo.GirisTarihi = DateTime.Now;
                
                stokFifo.OlusturmaTarihi = DateTime.Now;
                stokFifo.KalanMiktar = stokFifo.Miktar; // Başlangıçta kalan miktar, giriş miktarı ile aynıdır
                
                // Para birimi dönüşümleri
                if (stokFifo.ParaBirimi != "USD")
                {
                    // Döviz kuru servisi ile USD'ye çevirme
                    if (stokFifo.ParaBirimi == "UZS")
                    {
                        // UZS -> USD dönüşümü için bölme işlemi yapılmalı
                        // Kullanıcı kur değeri belirttiyse onu kullan
                        if (stokFifo.DovizKuru > 0)
                        {
                            stokFifo.BirimFiyatUSD = stokFifo.BirimFiyat / stokFifo.DovizKuru;
                            _logger.LogInformation($"UZS -> USD birim fiyat hesaplandı (kullanıcının girdiği kur ile): {stokFifo.BirimFiyat} / {stokFifo.DovizKuru} = {stokFifo.BirimFiyatUSD} USD");
                        }
                        else
                        {
                            // API'den kur değeri al
                            var kurUZStoUSD = await _dovizKuruService.GetGuncelKurAsync("UZS", "USD");
                            if (kurUZStoUSD > 0)
                            {
                                stokFifo.BirimFiyatUSD = stokFifo.BirimFiyat / kurUZStoUSD;
                                _logger.LogInformation($"UZS -> USD birim fiyat hesaplandı (API kuru ile): {stokFifo.BirimFiyat} / {kurUZStoUSD} = {stokFifo.BirimFiyatUSD} USD");
                            }
                            else
                            {
                                // Son çare olarak varsayılan bir kur değeri kullan
                                stokFifo.BirimFiyatUSD = stokFifo.BirimFiyat / 13000.0m;
                                _logger.LogWarning($"UZS -> USD için kur bilgisi bulunamadı, varsayılan değer kullanıldı: {stokFifo.BirimFiyat} / 13000.0 = {stokFifo.BirimFiyatUSD} USD");
                            }
                        }
                        
                        // UZS için direkt değer ata
                        stokFifo.BirimFiyatUZS = stokFifo.BirimFiyat;
                        _logger.LogInformation($"UZS birim fiyat direkt atandı (para birimi zaten UZS): {stokFifo.BirimFiyatUZS} UZS");
                    }
                    else
                    {
                        // Desteklenmeyen para birimi, varsayılan olarak USD kullan
                        _logger.LogWarning($"Desteklenmeyen para birimi: {stokFifo.ParaBirimi}. Sadece USD ve UZS desteklenmektedir. USD olarak devam ediliyor.");
                        stokFifo.ParaBirimi = "USD";
                        stokFifo.BirimFiyatUSD = stokFifo.BirimFiyat;
                        
                        // UZS dönüşümü için kur değeri al
                        if (stokFifo.DovizKuru > 0)
                        {
                            stokFifo.BirimFiyatUZS = stokFifo.BirimFiyat * stokFifo.DovizKuru;
                            _logger.LogInformation($"USD -> UZS birim fiyat hesaplandı (kullanıcının girdiği kur ile): {stokFifo.BirimFiyat} * {stokFifo.DovizKuru} = {stokFifo.BirimFiyatUZS} UZS");
                        }
                        else
                        {
                            var kurUSDtoUZS = await _dovizKuruService.GetGuncelKurAsync("USD", "UZS");
                            if (kurUSDtoUZS > 0)
                            {
                                stokFifo.BirimFiyatUZS = stokFifo.BirimFiyat * kurUSDtoUZS;
                                _logger.LogInformation($"USD -> UZS birim fiyat hesaplandı (API kuru ile): {stokFifo.BirimFiyat} * {kurUSDtoUZS} = {stokFifo.BirimFiyatUZS} UZS");
                            }
                            else
                            {
                                stokFifo.BirimFiyatUZS = stokFifo.BirimFiyat * 13000.0m;
                                _logger.LogWarning($"USD -> UZS için kur bilgisi bulunamadı, varsayılan değer kullanıldı: {stokFifo.BirimFiyat} * 13000.0 = {stokFifo.BirimFiyatUZS} UZS");
                            }
                        }
                    }
                }
                else
                {
                    // Para birimi USD
                    stokFifo.BirimFiyatUSD = stokFifo.BirimFiyat;
                    _logger.LogInformation($"USD birim fiyat direkt atandı (para birimi zaten USD): {stokFifo.BirimFiyatUSD} USD");
                    
                    // UZS dönüşümü için kur değeri al
                    if (stokFifo.DovizKuru > 0)
                    {
                        stokFifo.BirimFiyatUZS = stokFifo.BirimFiyat * stokFifo.DovizKuru;
                        _logger.LogInformation($"USD -> UZS birim fiyat hesaplandı (kullanıcının girdiği kur ile): {stokFifo.BirimFiyat} * {stokFifo.DovizKuru} = {stokFifo.BirimFiyatUZS} UZS");
                    }
                    else
                    {
                        var kurUSDtoUZS = await _dovizKuruService.GetGuncelKurAsync("USD", "UZS");
                        if (kurUSDtoUZS > 0)
                        {
                            stokFifo.BirimFiyatUZS = stokFifo.BirimFiyat * kurUSDtoUZS;
                            _logger.LogInformation($"USD -> UZS birim fiyat hesaplandı (API kuru ile): {stokFifo.BirimFiyat} * {kurUSDtoUZS} = {stokFifo.BirimFiyatUZS} UZS");
                        }
                        else
                        {
                            stokFifo.BirimFiyatUZS = stokFifo.BirimFiyat * 13000.0m;
                            _logger.LogWarning($"USD -> UZS için kur bilgisi bulunamadı, varsayılan değer kullanıldı: {stokFifo.BirimFiyat} * 13000.0 = {stokFifo.BirimFiyatUZS} UZS");
                        }
                    }
                }
                
                // SaveChangesAsync must be called from the outer transaction context (e.g. Controller) to avoid nested transaction issues
                await _context.StokFifoKayitlari.AddAsync(stokFifo);
                // SaveChanges dışarıdan yönetilecek transaction kapsamında çağrılmalı
                
                _logger.LogInformation($"Stok girişi yapıldı: Ürün {stokFifo.UrunID}, Miktar: {stokFifo.Miktar}, Birim Fiyat: {stokFifo.BirimFiyat} {stokFifo.ParaBirimi}");
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Stok girişi yapılırken hata oluştu: {ex.Message}");
                
                if (ex.InnerException != null)
                {
                    _logger.LogError($"İç hata detayı: {ex.InnerException.Message}");
                }
                
                return false;
            }
        }

        /// <summary>
        /// Stok çıkışı yapan basit metot - interface uyumluluğu için
        /// </summary>
        public async Task<bool> StokCikisAsync(Guid urunId, decimal miktar, string islemTuru, Guid islemId)
        {
            try
            {
                var result = await StokCikisiYap(urunId, miktar, islemTuru, islemTuru, islemId, $"{islemTuru} işlemi ile stok çıkışı (ID: {islemId})");
                return result.KullanilanFifoKayitlari.Any();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Stok çıkışı yapılırken hata oluştu: Ürün {urunId}, Miktar: {miktar}");
                return false;
            }
        }

        /// <summary>
        /// Stok çıkışı yapar
        /// </summary>
        public async Task<StokCikisInfo> StokCikisiYap(Guid urunID, decimal miktar, StokHareketTipi hareketTipi, 
            Guid? referansID = null, string aciklama = null, string paraBirimi = "USD", bool useBatch = false, decimal? dovizKuru = null)
        {
            // Transaction yönetimi için mevcut transaction kontrolü
            bool hasExistingTransaction = _context.Database.CurrentTransaction != null;
            IDbContextTransaction transaction = null;

            try
            {
                _logger.LogInformation($"StokCikisiYap başladı: Ürün {urunID}, Miktar {miktar}, Hareket Tipi {hareketTipi}");
                
                // Sadece dış transaction yoksa yeni bir transaction başlat
                if (!hasExistingTransaction)
                {
                    // ReadCommitted isolation level kullanarak deadlock riskini azalt
                    transaction = await _context.Database.BeginTransactionAsync(System.Data.IsolationLevel.ReadCommitted);
                    _logger.LogInformation($"StokCikisiYap - Yeni transaction başlatıldı (Isolation Level: ReadCommitted): Ürün {urunID}, Miktar {miktar}");
                }
                else
                {
                    _logger.LogInformation($"StokCikisiYap - Mevcut transaction kullanılıyor: Ürün {urunID}, Miktar {miktar}");
                }
                
                // Ürünün mevcut stok durumunu kontrol et
                var urun = await _context.Urunler.FindAsync(urunID);
                if (urun == null)
                {
                    _logger.LogWarning($"Stok çıkışı yapılamadı: Ürün bulunamadı (ID: {urunID})");
                    throw new Exception($"Ürün bulunamadı (ID: {urunID})");
                }
                
                // Ürünün aktif FIFO kayıtlarını getir (en eski tarihli önce)
                var fifoKayitlari = await _context.StokFifoKayitlari
                    .Where(f => f.UrunID == urunID && f.Aktif && !f.Iptal && !f.Silindi && f.KalanMiktar > 0)
                    .OrderBy(f => f.GirisTarihi)  // FIFO sıralaması - önce en eski kayıtlar
                    .ThenBy(f => f.OlusturmaTarihi) // Aynı tarihli kayıtlar için ikincil sıralama
                    .ToListAsync();

                // Toplam stok miktarını hesapla
                decimal toplamStok = fifoKayitlari.Sum(f => f.KalanMiktar);
                bool stokYetersiz = false;
                decimal eksikMiktar = 0;
                
                if (!fifoKayitlari.Any() || toplamStok < miktar)
                {
                    // Stok yetersiz durumunda hata fırlatmak yerine eksik stok durumunu kaydet ve devam et
                    stokYetersiz = true;
                    eksikMiktar = miktar - toplamStok;
                    _logger.LogWarning($"Stok yetersiz ancak işleme devam ediliyor: Ürün {urunID}, Talep: {miktar}, Mevcut: {toplamStok}, Eksik: {eksikMiktar}");
                }
                
                // Çıkış işlemi için gerekli değişkenleri hazırla
                decimal kalanMiktar = miktar;
                decimal toplamMaliyet = 0;
                var kullanılanFifoKayitlari = new List<(StokFifo Fifo, decimal KullanilanMiktar)>();
                var cikisDetaylari = new List<StokCikisDetay>();
                
                // Mevcut FIFO kayıtlarından çıkış yap (eğer varsa)
                foreach (var fifo in fifoKayitlari)
                {
                    if (kalanMiktar <= 0)
                        break;
                    
                    // Bu FIFO kaydından ne kadar kullanılacak
                    decimal kullanilanMiktar = Math.Min(fifo.KalanMiktar, kalanMiktar);
                    
                    // StokCikisDetay tablosu yoksa, bunu oluştur
                    var stokCikisDetay = new StokCikisDetay
                    {
                        StokCikisDetayID = Guid.NewGuid(),
                        StokFifoID = fifo.StokFifoID,
                        CikisMiktari = kullanilanMiktar,
                        CikisTarihi = DateTime.Now,
                        HareketTipi = hareketTipi.ToString(),
                        ReferansID = referansID ?? Guid.Empty,
                        ReferansTuru = "Stok",
                        ReferansNo = aciklama ?? $"{hareketTipi} hareketi",
                        BirimFiyat = fifo.BirimFiyat,
                        BirimFiyatUSD = fifo.BirimFiyatUSD,
                        BirimFiyatUZS = fifo.BirimFiyatUZS,
                        ToplamMaliyetUSD = kullanilanMiktar * fifo.BirimFiyatUSD,
                        BirimMaliyet = fifo.BirimFiyatUSD,
                        ToplamMaliyet = kullanilanMiktar * fifo.BirimFiyatUSD,
                        ParaBirimi = paraBirimi,
                        DovizKuru = dovizKuru, // Faturadan gelen döviz kuru kullanımı
                        Aciklama = aciklama ?? $"{hareketTipi} hareketi ile stok çıkışı (FIFO)",
                        OlusturmaTarihi = DateTime.Now,
                        Iptal = false,
                        IptalAciklama = "" // Boş string ile başlatma
                    };
                    
                    cikisDetaylari.Add(stokCikisDetay);
                    
                    // FIFO kaydının kalan miktarını güncelle - concurrency yönetimi ile
                    decimal kullanilacakMiktar = kullanilanMiktar; // Yerel değişkene kopyala
                    
                    bool fifoUpdateSuccess = await ProcessFifoEntryWithRetry(fifo, async (fifoToUpdate) => {
                        // Snapshot isolation level kullanıldığında dirty reads oluşmaz
                        fifoToUpdate.KalanMiktar -= kullanilacakMiktar;
                        fifoToUpdate.SonCikisTarihi = DateTime.Now;
                        fifoToUpdate.GuncellemeTarihi = DateTime.Now;
                        _context.StokFifoKayitlari.Update(fifoToUpdate);
                    }, maxRetries: 5); // Daha fazla deneme şansı ver
                    
                    if (!fifoUpdateSuccess)
                    {
                        _logger.LogError($"FIFO kaydı güncellenemedi. StokFifoID: {fifo.StokFifoID}");
                        throw new FifoConcurrencyException(fifo.StokFifoID, 5, 
                            $"FIFO kaydı 5 deneme sonrasında güncellenemedi. StokFifoID: {fifo.StokFifoID}");
                    }
                    
                    kullanılanFifoKayitlari.Add((fifo, kullanilanMiktar));
                    toplamMaliyet += stokCikisDetay.ToplamMaliyet;
                    kalanMiktar -= kullanilanMiktar;
                }
                
                // Eksik stok durumunda eksi bakiye kaydı oluştur
                if (stokYetersiz && kalanMiktar > 0)
                {
                    // Eksik miktarı kaydetmek için stok borçlanması oluştur
                    // Birim fiyat ve maliyet hesaplamak için var olan kayıtların ortalamasını kullan
                    decimal ortalamaBirimFiyat = 0;
                    if (cikisDetaylari.Any())
                    {
                        ortalamaBirimFiyat = cikisDetaylari.Sum(d => d.ToplamMaliyet) / cikisDetaylari.Sum(d => d.CikisMiktari);
                    }
                    else
                    {
                        // Eğer hiç FIFO kaydı yoksa, ürünün son satış fiyatını almaya çalış
                        var urunFiyat = await _context.UrunFiyatlari
                            .Where(uf => uf.UrunID == urunID && !uf.Silindi)
                            .OrderByDescending(uf => uf.GecerliTarih)
                            .FirstOrDefaultAsync();
                        
                        ortalamaBirimFiyat = urunFiyat?.Fiyat ?? 0;
                    }
                    
                    // Eksik stok için çıkış detayı oluştur
                    var eksikStokCikisDetay = new StokCikisDetay
                    {
                        StokCikisDetayID = Guid.NewGuid(),
                        StokFifoID = null, // Henüz FIFO kaydı yok
                        CikisMiktari = kalanMiktar,
                        CikisTarihi = DateTime.Now,
                        HareketTipi = $"{hareketTipi}-EksikStok",
                        ReferansID = referansID ?? Guid.Empty,
                        ReferansTuru = "EksikStok",
                        ReferansNo = aciklama ?? $"{hareketTipi}-EksikStok", // ReferansNo için null kontrolü
                        BirimFiyat = ortalamaBirimFiyat,
                        BirimFiyatUSD = ortalamaBirimFiyat, // USD varsayılan
                        BirimFiyatUZS = paraBirimi == "UZS" ? ortalamaBirimFiyat : ortalamaBirimFiyat * (dovizKuru ?? await GetKurDegeri("USD", "UZS")),
                        ToplamMaliyetUSD = kalanMiktar * ortalamaBirimFiyat,
                        BirimMaliyet = ortalamaBirimFiyat,
                        ToplamMaliyet = kalanMiktar * ortalamaBirimFiyat,
                        ParaBirimi = paraBirimi,
                        DovizKuru = dovizKuru, // Faturadan gelen döviz kuru kullanımı
                        Aciklama = $"Stok yetersiz - Borçlanma ({hareketTipi}): {kalanMiktar}",
                        OlusturmaTarihi = DateTime.Now,
                        Iptal = false,
                        IptalAciklama = "" // Boş string ile başlatma
                    };
                    
                    cikisDetaylari.Add(eksikStokCikisDetay);
                    toplamMaliyet += eksikStokCikisDetay.ToplamMaliyet;
                    
                    // Negatif FIFO kaydı oluştur
                    var negatifFifo = new StokFifo
                    {
                        StokFifoID = Guid.NewGuid(),
                        UrunID = urunID,
                        Miktar = -kalanMiktar, // Negatif miktar
                        KalanMiktar = -kalanMiktar, // Negatif kalan miktar
                        BirimFiyat = ortalamaBirimFiyat,
                        BirimFiyatUSD = ortalamaBirimFiyat,
                        BirimFiyatUZS = paraBirimi == "UZS" ? ortalamaBirimFiyat : ortalamaBirimFiyat * (dovizKuru ?? await GetKurDegeri("USD", "UZS")),
                        ParaBirimi = paraBirimi,
                        DovizKuru = dovizKuru ?? ((paraBirimi == "UZS") ? await GetKurDegeri("UZS", "USD") : await GetKurDegeri("USD", "UZS")),
                        GirisTarihi = DateTime.Now,
                        OlusturmaTarihi = DateTime.Now,
                        Aktif = true,
                        Iptal = false,
                        Silindi = false,
                        ReferansNo = aciklama ?? $"{hareketTipi}-EksikStok",
                        ReferansTuru = "NegatifStok",
                        ReferansID = referansID,
                        Aciklama = $"Negatif stok kaydı - Eksik miktar: {kalanMiktar}"
                    };
                    
                    _context.StokFifoKayitlari.Add(negatifFifo);
                    _logger.LogInformation($"Negatif stok FIFO kaydı oluşturuldu: UrunID={urunID}, Miktar={-kalanMiktar}, Fiyat={ortalamaBirimFiyat}");
                    
                    _logger.LogInformation($"Eksik stok kaydı oluşturuldu: UrunID={urunID}, Miktar={kalanMiktar}, BirimFiyat={ortalamaBirimFiyat}");
                }
                
                // StokCikisDetay kayıtlarını toplu ekle
                await _context.StokCikisDetaylari.AddRangeAsync(cikisDetaylari);
                
                // Değişiklikleri kaydet
                await _context.SaveChangesAsync();
                
                // Toplam maliyeti istenen para birimine çevir
                decimal maliyet = toplamMaliyet;
                if (paraBirimi != "USD" && toplamMaliyet > 0)
                {
                    try
                    {
                        decimal kurDegeri = await GetKurDegeri("USD", paraBirimi);
                        maliyet = toplamMaliyet * kurDegeri;
                    }
                    catch (Exception ex)
                    {
                        _logger.LogWarning($"Para birimi dönüşümü yapılamadı: {ex.Message}. USD cinsinden maliyet kullanılacak.");
                    }
                }
                
                // Sonuç nesnesini hazırla
                var result = new StokCikisInfo
                {
                    Miktar = miktar,
                    ToplamMaliyet = maliyet,
                    ParaBirimi = paraBirimi,
                    KullanilanFifoKayitlari = kullanılanFifoKayitlari.Select(k => k.Fifo.StokFifoID).ToList(),
                    CikisDetaylari = cikisDetaylari.Select(d => d.StokCikisDetayID).ToList(),
                    StokYetersiz = stokYetersiz,
                    EksikMiktar = eksikMiktar
                };
                
                // Sadece biz başlattıysak transaction'ı commit et
                if (!hasExistingTransaction && transaction != null)
                {
                    await transaction.CommitAsync();
                    _logger.LogInformation($"StokCikisiYap - Transaction commit edildi: Ürün {urunID}, Miktar {miktar}");
                }
                
                _logger.LogInformation($"Stok çıkışı başarıyla tamamlandı: Ürün {urunID}, Miktar {miktar}, Maliyet {maliyet} {paraBirimi}");
                return result;
            }
            catch (Exception ex)
            {
                // Sadece biz başlattıysak transaction'ı geri al
                if (!hasExistingTransaction && transaction != null)
                {
                    await transaction.RollbackAsync();
                    _logger.LogWarning($"StokCikisiYap - Transaction geri alındı: Ürün {urunID}, Miktar {miktar}");
                }
                
                if (ex is StokYetersizException || ex is FifoConcurrencyException)
                {
                    // Özel hata tiplerimiz, tekrar fırlat
                    throw;
                }
                
                _logger.LogError(ex, $"Stok çıkışı sırasında hata oluştu: Ürün {urunID}, Miktar {miktar}");
                throw new Exception($"Stok çıkışı işlemi başarısız oldu: {ex.Message}", ex);
            }
            finally
            {
                // Sadece biz başlattıysak transaction'ı dispose et
                if (!hasExistingTransaction && transaction != null)
                {
                    await transaction.DisposeAsync();
                }
            }
        }

        /// <summary>
        /// Stok çıkışı yapan genişletilmiş metot. Referans bilgileri ile çıkış yapar.
        /// </summary>
        public async Task<(List<StokFifo> KullanilanFifoKayitlari, decimal ToplamMaliyet)> StokCikisiYap(Guid urunID, decimal miktar, string referansNo, string referansTuru, Guid? referansID, string aciklama)
        {
            // Transaction yönetimi - iç içe transaction olup olmadığını kontrol et
            bool hasExistingTransaction = _context.Database.CurrentTransaction != null;
            IDbContextTransaction transaction = null;
            
            try
            {
                // Sadece dış transaction yoksa yeni bir transaction başlat
                if (!hasExistingTransaction)
                {
                    transaction = await _context.Database.BeginTransactionAsync();
                    _logger.LogInformation($"StokCikisiYap - Yeni transaction başlatıldı: {urunID}, {miktar}");
                }
                else
                {
                    _logger.LogInformation($"StokCikisiYap - Mevcut transaction kullanılıyor: {urunID}, {miktar}");
                }
                
                // Null kontrolü ve varsayılan değerler
                referansNo = referansNo ?? string.Empty;
                referansTuru = referansTuru ?? string.Empty;
                aciklama = aciklama ?? $"Stok çıkışı: {miktar} birim";
                
                // Kullanılacak FIFO kayıtlarını bul
                var kullanilacakFifoKayitlari = await _context.StokFifoKayitlari
                    .Where(f => f.UrunID == urunID && f.KalanMiktar > 0 && !f.Iptal && !f.Silindi)
                    .OrderBy(f => f.OlusturmaTarihi)
                    .ToListAsync();
                    
                if (!kullanilacakFifoKayitlari.Any())
                {
                    throw new InvalidOperationException($"Ürün için yeterli stok bulunamadı: UrunID={urunID}");
                }
                
                decimal kalanMiktar = miktar;
                decimal toplamMaliyet = 0;
                var kullanilanFifoKayitlari = new List<StokFifo>();
                
                foreach (var fifo in kullanilacakFifoKayitlari)
                {
                    if (kalanMiktar <= 0) break;
                    
                    decimal kullanilacakMiktar = Math.Min(kalanMiktar, fifo.KalanMiktar);
                    decimal maliyet = kullanilacakMiktar * fifo.BirimFiyat;
                    
                    fifo.KalanMiktar -= kullanilacakMiktar;
                    fifo.GuncellemeTarihi = DateTime.Now;
                    
                    kalanMiktar -= kullanilacakMiktar;
                    toplamMaliyet += maliyet;
                    kullanilanFifoKayitlari.Add(fifo);
                    
                    _logger.LogInformation($"FIFO kaydı kullanıldı: FifoID={fifo.StokFifoID}, Kullanılan miktar={kullanilacakMiktar}, Kalan miktar={fifo.KalanMiktar}");
                }
                
                if (kalanMiktar > 0)
                {
                    throw new InvalidOperationException($"Yeterli stok bulunamadı. Eksik miktar: {kalanMiktar}");
                }
                
                await _context.SaveChangesAsync();
                
                // Sadece biz başlattıysak transaction'ı commit et
                if (!hasExistingTransaction && transaction != null)
                {
                    await transaction.CommitAsync();
                    _logger.LogInformation($"StokCikisiYap - Transaction commit edildi: {urunID}, {miktar}");
                }
                
                _logger.LogInformation($"Stok çıkışı başarıyla tamamlandı: UrunID={urunID}, Miktar={miktar}, ToplamMaliyet={toplamMaliyet}");
                return (kullanilanFifoKayitlari, toplamMaliyet);
            }
            catch (Exception ex)
            {
                // Sadece biz başlattıysak transaction'ı geri al
                if (!hasExistingTransaction && transaction != null)
                {
                    await transaction.RollbackAsync();
                    _logger.LogWarning($"StokCikisiYap - Transaction geri alındı: {urunID}, {miktar}");
                }
                
                _logger.LogError(ex, $"Stok çıkışı sırasında hata oluştu: UrunID={urunID}, Miktar={miktar}");
                throw;
            }
            finally
            {
                // Sadece biz başlattıysak transaction'ı dispose et
                if (!hasExistingTransaction && transaction != null)
                {
                    await transaction.DisposeAsync();
                }
            }
        }

        /// <summary>
        /// Belirli bir referansa ait FIFO kayıtlarını iptal eder.
        /// </summary>
        public async Task<List<StokFifo>> FifoKayitlariniIptalEt(Guid referansID, string referansTuru, string iptalAciklama, Guid? iptalEdenKullaniciID = null)
        {
            IDbContextTransaction transaction = null;
            bool hasExistingTransaction = _context.Database.CurrentTransaction != null;

            try
            {
                _logger.LogInformation($"FIFO kayıtları iptal edilmeye çalışılıyor: ReferansID={referansID}, ReferansTuru={referansTuru}");
                
                // Mevcut transaction var ise, yeni bir transaction başlatma
                if (!hasExistingTransaction)
                {
                    transaction = await _context.Database.BeginTransactionAsync();
                    _logger.LogInformation($"FifoKayitlariniIptalEt - Yeni transaction başlatıldı: ReferansID={referansID}");
                }
                else
                {
                    _logger.LogInformation($"FifoKayitlariniIptalEt - Mevcut transaction kullanılıyor: ReferansID={referansID}");
                }
                
                // İlgili FIFO kayıtlarını bul
                var fifoKayitlari = await _context.StokFifoKayitlari
                    .Where(f => f.ReferansID == referansID && f.ReferansTuru == referansTuru && !f.Silindi && !f.Iptal)
                    .ToListAsync();
                    
                if (!fifoKayitlari.Any())
                {
                    _logger.LogWarning($"İptal edilecek FIFO kaydı bulunamadı: ReferansID={referansID}, ReferansTuru={referansTuru}");
                    
                    // Sadece biz başlattıysak transaction'ı commit et (boş durumda bile)
                    if (!hasExistingTransaction && transaction != null)
                    {
                        await transaction.CommitAsync();
                        _logger.LogInformation($"FifoKayitlariniIptalEt - Transaction commit edildi (boş): ReferansID={referansID}");
                    }
                    
                    return new List<StokFifo>();
                }
                
                _logger.LogInformation($"FIFO kayıtları iptal ediliyor. Toplam {fifoKayitlari.Count} kayıt bulundu.");
                
                foreach (var fifoKayit in fifoKayitlari)
                {
                    // İptal bilgilerini doldur
                    fifoKayit.Iptal = true;
                    fifoKayit.Aciklama = $"{fifoKayit.Aciklama} (İptal: {iptalAciklama})";
                    fifoKayit.GuncellemeTarihi = DateTime.Now;
                    
                    _context.StokFifoKayitlari.Update(fifoKayit);
                }
                
                await _context.SaveChangesAsync();
                
                // Ayrıca ilgili çıkış kayıtlarını da iptal et (varsa)
                if (_context.Model.FindEntityType(typeof(StokCikisDetay)) != null)
                {
                    // Bu entity var mı emin değiliz, o yüzden dinamik olarak kontrol ediyoruz
                    var cikisDetaylar = await _context.StokCikisDetaylari
                        .Where(c => c.ReferansID == referansID && c.ReferansTuru == referansTuru)
                        .ToListAsync();
                        
                    foreach (var cikis in cikisDetaylar)
                    {
                        // İlgili FIFO kaydının kalan miktarını geri artır
                        if (cikis.StokFifoID.HasValue)
                        {
                            var fifo = await _context.StokFifoKayitlari.FindAsync(cikis.StokFifoID.Value);
                            if (fifo != null)
                            {
                                var cikisMiktari = cikis.CikisMiktari;
                                var basarili = await ProcessFifoEntryWithRetry(fifo, async (f) => {
                                    f.KalanMiktar += cikisMiktari;
                                    _logger.LogInformation($"FIFO kayıtı kalan miktarı güncellendi: FifoID={f.StokFifoID}, Eski miktar={f.KalanMiktar - cikisMiktari}, Yeni miktar={f.KalanMiktar}");
                                    f.GuncellemeTarihi = DateTime.Now;
                                });
                                
                                if (!basarili)
                                {
                                    _logger.LogWarning($"FIFO kaydı güncellenirken concurrency hatası oluştu ve maksimum deneme sayısı aşıldı: FifoID={fifo.StokFifoID}, CikisMiktari={cikisMiktari}");
                                }
                            }
                        }
                        
                        // Çıkış kaydını işaretle
                        cikis.Iptal = true;
                        cikis.IptalTarihi = DateTime.Now;
                        cikis.IptalAciklama = iptalAciklama;
                        cikis.OlusturmaTarihi = DateTime.Now;
                        
                        _context.StokCikisDetaylari.Update(cikis);
                    }
                    
                    if (cikisDetaylar.Any())
                    {
                        await _context.SaveChangesAsync();
                        _logger.LogInformation($"Stok çıkış detayları iptal edildi. Toplam {cikisDetaylar.Count} kayıt güncellendi.");
                    }
                }
                
                // Sadece biz başlattıysak transaction'ı commit et
                if (!hasExistingTransaction && transaction != null)
                {
                    await transaction.CommitAsync();
                    _logger.LogInformation($"FifoKayitlariniIptalEt - Transaction commit edildi: ReferansID={referansID}");
                }
                
                return fifoKayitlari;
            }
            catch (Exception ex)
            {
                // Sadece biz başlattıysak transaction'ı geri al
                if (!hasExistingTransaction && transaction != null)
                {
                    await transaction.RollbackAsync();
                    _logger.LogWarning($"FifoKayitlariniIptalEt - Transaction geri alındı: ReferansID={referansID}");
                }
                
                _logger.LogError(ex, $"FIFO kayıtları iptal edilirken hata oluştu: ReferansID={referansID}, Hata={ex.Message}");
                
                if (ex.InnerException != null)
                {
                    _logger.LogError($"İç hata detayı: {ex.InnerException.Message}");
                }
                
                throw;
            }
            finally
            {
                // Sadece biz başlattıysak transaction'ı dispose et
                if (!hasExistingTransaction && transaction != null)
                {
                    await transaction.DisposeAsync();
                }
            }
        }

        /// <summary>
        /// Ortalama maliyet hesaplaması
        /// </summary>
        private async Task<decimal> HesaplaOrtalamaMaliyet(Guid urunID, string paraBirimi = "USD")
        {
            try
            {
                // Sadece aktif giriş kayıtlarını al
                var kayitlar = await _context.StokFifoKayitlari
                    .Where(f => f.UrunID == urunID 
                           && f.KalanMiktar > 0 
                           && f.Aktif 
                           && !f.Iptal 
                           && !f.Silindi
                           && f.Miktar > 0) // Miktar > 0 olan kayıtlar giriş kayıtlarıdır
                    .ToListAsync();
                
                if (!kayitlar.Any())
                {
                    return 0;
                }
                
                decimal toplamMiktar = kayitlar.Sum(f => f.KalanMiktar);
                
                if (toplamMiktar <= 0)
                {
                    return 0;
                }
                
                decimal toplamMaliyet = 0;
                
                foreach (var kayit in kayitlar)
                {
                    // Doğrudan BirimFiyatUSD veya BirimFiyatUZS alanlarını kullan
                    switch (paraBirimi.ToUpper())
                    {
                        case "USD":
                            toplamMaliyet += kayit.KalanMiktar * kayit.BirimFiyatUSD;
                            break;
                        case "UZS":
                            toplamMaliyet += kayit.KalanMiktar * kayit.BirimFiyatUZS;
                            break;
                        default:
                            // Varsayılan olarak USD kullan
                            toplamMaliyet += kayit.KalanMiktar * kayit.BirimFiyatUSD;
                            break;
                    }
                }
                
                return toplamMaliyet / toplamMiktar;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Ortalama maliyet hesaplanırken hata oluştu: {ex.Message}");
                return 0;
            }
        }

        /// <summary>
        /// Bir ürünün ortalama maliyetini hesaplar. Kalan FIFO kayıtlarına göre ağırlıklı ortalama alınır.
        /// </summary>
        public async Task<decimal> GetOrtalamaMaliyet(Guid urunID, string paraBirimi = "USD")
        {
            try
            {
                // Sadece aktif giriş kayıtlarını al
                var fifoKayitlari = await _context.StokFifoKayitlari
                    .Where(f => f.UrunID == urunID 
                           && f.KalanMiktar > 0 
                           && f.Aktif 
                           && !f.Iptal 
                           && !f.Silindi
                           && f.Miktar > 0) // Miktar > 0 olan kayıtlar giriş kayıtlarıdır
                    .ToListAsync();
                    
                if (!fifoKayitlari.Any() || fifoKayitlari.Sum(f => f.KalanMiktar) == 0)
                    return 0;

                decimal toplamMaliyet = 0;
                decimal toplamMiktar = 0;

                foreach (var fifo in fifoKayitlari)
                {
                    // Doğrudan BirimFiyatUSD veya BirimFiyatUZS alanlarını kullan
                    switch (paraBirimi.ToUpper())
                    {
                        case "USD":
                            toplamMaliyet += fifo.KalanMiktar * fifo.BirimFiyatUSD;
                            break;
                        case "UZS":
                            toplamMaliyet += fifo.KalanMiktar * fifo.BirimFiyatUZS;
                            break;
                        default:
                            // Varsayılan olarak USD kullan
                            toplamMaliyet += fifo.KalanMiktar * fifo.BirimFiyatUSD;
                            break;
                    }
                    
                    toplamMiktar += fifo.KalanMiktar;
                }

                return toplamMiktar > 0 ? toplamMaliyet / toplamMiktar : 0;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Ortalama maliyet hesaplanırken hata oluştu: Ürün {urunID}, Para Birimi: {paraBirimi}");
                return 0;
            }
        }

        /// <summary>
        /// Bir ürünün aktif (kalan miktarı olan) FIFO kayıtlarını getirir.
        /// </summary>
        public async Task<List<StokFifo>> GetAktifFifoKayitlari(Guid urunID)
        {
            try
            {
                return await _context.StokFifoKayitlari
                    .Where(f => f.UrunID == urunID && f.KalanMiktar > 0 && f.Aktif && !f.Iptal && !f.Silindi)
                    .OrderBy(f => f.GirisTarihi) // FIFO sırasına göre
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Aktif FIFO kayıtları getirilirken hata oluştu: Ürün {urunID}");
                return new List<StokFifo>();
            }
        }

        /// <summary>
        /// Belirli bir referansa göre FIFO kayıtlarını getirir.
        /// </summary>
        public async Task<List<StokFifo>> GetReferansaGoreFifoKayitlari(Guid referansID, string referansTuru)
        {
            try 
            {
                return await _context.StokFifoKayitlari
                    .Where(f => f.ReferansID == referansID && f.ReferansTuru == referansTuru && !f.Silindi)
                    .OrderBy(f => f.GirisTarihi)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Referansa göre FIFO kayıtları getirilirken hata oluştu: Referans ID: {referansID}, Tür: {referansTuru}");
                return new List<StokFifo>();
            }
        }

        /// <summary>
        /// Belirli bir tarihteki bir stok kaleminin maliyetini hesaplar.
        /// </summary>
        public async Task<decimal> HesaplaMaliyetAsync(Guid stokId, DateTime tarih, string paraBirimi = "USD")
        {
            try
            {
                // Belirli bir tarihe kadar olan sadece giriş kayıtlarını alarak maliyeti hesapla
                var fifoKayitlari = await _context.StokFifoKayitlari
                    .Where(f => f.UrunID == stokId 
                           && f.GirisTarihi <= tarih 
                           && f.Aktif 
                           && !f.Iptal 
                           && !f.Silindi
                           && f.Miktar > 0) // Miktar > 0 olan kayıtlar giriş kayıtlarıdır
                    .ToListAsync();
                
                // StokCikisDetay tablosu varsa, o tarihteki çıkışları da hesaba kat
                decimal toplamMiktar = 0;
                decimal toplamMaliyet = 0;
                bool cikisDetayVarMi = _context.Model.FindEntityType(typeof(StokCikisDetay)) != null;
                
                foreach (var fifo in fifoKayitlari)
                {
                    decimal mevcutMiktar = fifo.Miktar;
                    
                    // Çıkışları hesapla
                    if (cikisDetayVarMi)
                    {
                        try
                        {
                            var cikislar = await _context.StokCikisDetaylari
                                .Where(c => c.StokFifoID == fifo.StokFifoID && c.CikisTarihi <= tarih && !c.Iptal)
                                .SumAsync(c => c.CikisMiktari);
                                
                            mevcutMiktar -= cikislar;
                        }
                        catch (Exception ex)
                        {
                            _logger.LogWarning(ex, "StokCikisDetay erişimi sırasında hata oluştu. Bu bir kritik hata değil, işlem devam ediyor.");
                        }
                    }
                    
                    // Negatif olamaz
                    mevcutMiktar = Math.Max(0, mevcutMiktar);
                    
                    // Maliyet hesapla - doğrudan BirimFiyatUSD veya BirimFiyatUZS kullan
                    decimal birimFiyat = 0;
                    switch (paraBirimi.ToUpper())
                    {
                        case "USD":
                            birimFiyat = fifo.BirimFiyatUSD;
                            break;
                        case "UZS":
                            birimFiyat = fifo.BirimFiyatUZS;
                            break;
                        default:
                            // Varsayılan olarak USD kullan
                            birimFiyat = fifo.BirimFiyatUSD;
                            break;
                    }
                    
                    toplamMiktar += mevcutMiktar;
                    toplamMaliyet += mevcutMiktar * birimFiyat;
                }
                
                return toplamMiktar > 0 ? toplamMaliyet / toplamMiktar : 0;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Belirli bir tarihteki maliyet hesaplanırken hata oluştu: Stok {stokId}, Tarih: {tarih}, Para Birimi: {paraBirimi}");
                return 0;
            }
        }

        /// <summary>
        /// Stok girişi kaydı oluşturur. FIFO yöntemine göre stok girişi yapar.
        /// </summary>
        public async Task<StokFifo> StokGirisi(
            Guid urunID, 
            decimal miktar, 
            decimal birimFiyat,
            Guid? referansID, 
            Guid? detayID,
            string referansNo, 
            string referansTuru, 
            string paraBirimi = "USD", 
            decimal? dovizKuru = null,
            Guid? currentUserId = null)
        {
            _logger.LogInformation($"Stok girişi yapılıyor. UrunID: {urunID}, Miktar: {miktar}, BirimFiyat: {birimFiyat}");
            
            // Stok FIFO kaydı oluştur
            var stokFifo = new StokFifo
            {
                StokFifoID = Guid.NewGuid(),
                UrunID = urunID,
                Miktar = miktar,
                KalanMiktar = miktar,
                BirimFiyat = birimFiyat,
                BirimFiyatUSD = birimFiyat,
                BirimFiyatUZS = birimFiyat * await GetKurDegeri("USD", "UZS"),
                ParaBirimi = paraBirimi ?? "USD",
                DovizKuru = dovizKuru.HasValue ? dovizKuru.Value : 1m,
                GirisTarihi = DateTime.Now,
                OlusturmaTarihi = DateTime.Now,
                Aktif = true,
                Iptal = false,
                Silindi = false,
                ReferansNo = referansNo,
                ReferansTuru = referansTuru,
                ReferansID = referansID,
                Aciklama = $"Stok girişi: {miktar} birim"
            };
            
            await _context.StokFifoKayitlari.AddAsync(stokFifo);
            await _context.SaveChangesAsync();
            
            return stokFifo;
        }

        /// <summary>
        /// Stok çıkışı kaydı oluşturur. FIFO yöntemine göre stok çıkışı yapar.
        /// </summary>
        public async Task StokCikisi(
            Guid urunID, 
            decimal miktar,
            Guid? referansID, 
            Guid? detayID,
            string referansNo, 
            string referansTuru, 
            string paraBirimi = "USD", 
            decimal? dovizKuru = null,
            Guid? currentUserId = null)
        {
            _logger.LogInformation($"Stok çıkışı yapılıyor. UrunID: {urunID}, Miktar: {miktar}");
            
            // FIFO kuralına göre stok çıkış kaydı yap
            // En eski girişten başlayarak stok çıkışı yap
            var fifoKayitlari = await _context.StokFifoKayitlari
                .Where(f => f.UrunID == urunID && f.KalanMiktar > 0 && f.Aktif && !f.Iptal && !f.Silindi)
                .OrderBy(f => f.GirisTarihi)
                .ToListAsync();
            
            if (fifoKayitlari == null || !fifoKayitlari.Any())
            {
                _logger.LogWarning($"Stok çıkışı yapılamadı: Yeterli stok bulunamadı. UrunID: {urunID}, Miktar: {miktar}");
                throw new Exception($"Stok çıkışı yapılamadı: Yeterli stok bulunamadı. UrunID: {urunID}, Miktar: {miktar}");
            }
            
            // Toplam kullanılabilir stok miktarını hesapla
            decimal toplamKalanMiktar = fifoKayitlari.Sum(f => f.KalanMiktar);
            
            if (toplamKalanMiktar < miktar)
            {
                _logger.LogWarning($"Stok çıkışı yapılamadı: Yeterli stok bulunamadı. İstenen: {miktar}, Mevcut: {toplamKalanMiktar}");
                throw new Exception($"Stok çıkışı yapılamadı: Yeterli stok bulunamadı. İstenen: {miktar}, Mevcut: {toplamKalanMiktar}");
            }
            
            // Kalan çıkış miktarı
            decimal kalanCikisMiktari = miktar;
            
            foreach (var fifoKaydi in fifoKayitlari)
            {
                if (kalanCikisMiktari <= 0)
                    break;
                
                decimal cikisMiktari = Math.Min(fifoKaydi.KalanMiktar, kalanCikisMiktari);
                
                // FIFO kaydını güncelle
                fifoKaydi.KalanMiktar -= cikisMiktari;
                fifoKaydi.SonCikisTarihi = DateTime.Now;
                
                if (currentUserId.HasValue)
                {
                    fifoKaydi.GuncellemeTarihi = DateTime.Now;
                }
                
                _context.StokFifoKayitlari.Update(fifoKaydi);
                
                // StokFifoCikis kaydı oluştur
                var stokFifoCikis = new StokFifoCikis
                {
                    StokFifoCikisID = Guid.NewGuid(),
                    StokFifoID = fifoKaydi.StokFifoID,
                    ReferansID = referansID,
                    DetayID = detayID,
                    ReferansNo = referansNo,
                    ReferansTuru = referansTuru,
                    CikisMiktar = cikisMiktari,
                    CikisTarihi = DateTime.Now,
                    BirimFiyatUSD = fifoKaydi.BirimFiyatUSD,
                    BirimFiyatUZS = fifoKaydi.BirimFiyatUZS,
                    DovizKuru = dovizKuru,
                    ParaBirimi = paraBirimi
                };
                
                await _context.StokFifoCikislari.AddAsync(stokFifoCikis);
                
                // Kalan çıkış miktarını güncelle
                kalanCikisMiktari -= cikisMiktari;
            }
            
            await _context.SaveChangesAsync();
            
            _logger.LogInformation($"Stok çıkışı başarıyla tamamlandı. UrunID: {urunID}, Miktar: {miktar}");
        }

        #endregion

        #region Yardımcı Metotlar

        /// <summary>
        /// Para birimleri arasındaki kur değerini alır
        /// </summary>
        private async Task<decimal> GetKurDegeri(string kaynakParaBirimi, string hedefParaBirimi)
        {
            try
            {
                // Aynı para birimleri için 1 dön
                if (kaynakParaBirimi == hedefParaBirimi)
                {
                    return 1.0m;
                }

                // USD -> UZS için varsayılan değer
                if (kaynakParaBirimi == "USD" && hedefParaBirimi == "UZS")
                {
                    var kur = await _dovizKuruService.GetGuncelKurAsync(kaynakParaBirimi, hedefParaBirimi);
                    if (kur <= 0)
                    {
                        _logger.LogWarning($"USD -> UZS için geçerli kur değeri bulunamadı. Varsayılan değer (13000) kullanılacak.");
                        return 13000.0m; // Varsayılan USD -> UZS kuru
                    }
                    return kur;
                }

                // UZS -> USD için varsayılan değer
                if (kaynakParaBirimi == "UZS" && hedefParaBirimi == "USD")
                {
                    var kur = await _dovizKuruService.GetGuncelKurAsync(kaynakParaBirimi, hedefParaBirimi);
                    if (kur <= 0)
                    {
                        _logger.LogWarning($"UZS -> USD için geçerli kur değeri bulunamadı. Varsayılan değer (1/13000) kullanılacak.");
                        return 1.0m / 13000.0m; // Varsayılan UZS -> USD kuru
                    }
                    return kur;
                }

                // Diğer para birimleri için kur servisini kullan
                var genelKur = await _dovizKuruService.GetGuncelKurAsync(kaynakParaBirimi, hedefParaBirimi);
                if (genelKur <= 0)
                {
                    _logger.LogWarning($"Geçerli kur değeri bulunamadı: {kaynakParaBirimi} -> {hedefParaBirimi}. Varsayılan değer (1.0) kullanılacak.");
                    return 1.0m; // Varsayılan değer
                }
                return genelKur;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Kur değeri alınırken hata oluştu: {kaynakParaBirimi} -> {hedefParaBirimi}");
                return 1.0m; // Varsayılan değer
            }
        }

        /// <summary>
        /// Concurrency yönetimi ile FIFO kaydı işleme
        /// </summary>
        /// <param name="fifoEntry">İşlenecek FIFO kaydı</param>
        /// <param name="processAction">FIFO kaydı üzerinde yapılacak işlem</param>
        /// <param name="maxRetries">Maksimum yeniden deneme sayısı</param>
        /// <returns>İşlem sonucu</returns>
        public async Task<bool> ProcessFifoEntryWithRetry(StokFifo fifoEntry, Func<StokFifo, Task> processAction, int maxRetries = 3)
        {
            bool retryNeeded = true;
            int retryCount = 0;
            int baseWaitTime = 100;
            while (retryNeeded && retryCount < maxRetries)
            {
                try
                {
                    await processAction(fifoEntry);
                    await _context.SaveChangesAsync();
                    retryNeeded = false;
                    return true;
                }
                catch (DbUpdateConcurrencyException ex)
                {
                    retryCount++;
                    _logger.LogWarning($"FIFO işlemi sırasında concurrency hatası oluştu. StokFifoID: {fifoEntry.StokFifoID}, Deneme: {retryCount}/{maxRetries}");
                    if (retryCount >= maxRetries)
                    {
                        _logger.LogError($"FIFO işlemi maksimum deneme sayısını aştı. StokFifoID: {fifoEntry.StokFifoID}, Hata: {ex.Message}");
                        throw new FifoConcurrencyException(fifoEntry.StokFifoID, retryCount, $"FIFO işlemi {maxRetries} deneme sonrasında başarısız oldu: {ex.Message}");
                    }
                    var entry = ex.Entries.Single();
                    var databaseValues = await entry.GetDatabaseValuesAsync();
                    if (databaseValues == null)
                    {
                        _logger.LogWarning($"FIFO kaydı {fifoEntry.StokFifoID} veritabanında bulunamadı, işlem iptal edildi.");
                        return false;
                    }
                    entry.OriginalValues.SetValues(databaseValues);
                    int waitTime = baseWaitTime * (int)Math.Pow(2, retryCount - 1);
                    waitTime += new Random().Next(0, 50);
                    _logger.LogInformation($"FIFO işlemi yeniden deneniyor. StokFifoID: {fifoEntry.StokFifoID}, Bekleme süresi: {waitTime}ms");
                    await Task.Delay(waitTime);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, $"FIFO işlemi sırasında beklenmeyen hata: StokFifoID: {fifoEntry.StokFifoID}, Hata: {ex.Message}");
                    throw;
                }
            }
            return false;
        }

        public async Task<StokFifo> ProcessFifoEntryWithRetryAndReturn(StokFifo fifoEntry, Func<StokFifo, Task> processAction, int maxRetries = 3)
        {
            bool retryNeeded = true;
            int retryCount = 0;
            int baseWaitTime = 100;
            while (retryNeeded && retryCount < maxRetries)
            {
                try
                {
                    await processAction(fifoEntry);
                    await _context.SaveChangesAsync();
                    retryNeeded = false;
                    return fifoEntry;
                }
                catch (DbUpdateConcurrencyException ex)
                {
                    retryCount++;
                    _logger.LogWarning($"FIFO işlemi sırasında concurrency hatası oluştu. StokFifoID: {fifoEntry.StokFifoID}, Deneme: {retryCount}/{maxRetries}");
                    if (retryCount >= maxRetries)
                    {
                        _logger.LogError($"FIFO işlemi maksimum deneme sayısını aştı. StokFifoID: {fifoEntry.StokFifoID}, Hata: {ex.Message}");
                        throw new FifoConcurrencyException(fifoEntry.StokFifoID, retryCount, $"FIFO işlemi {maxRetries} deneme sonrasında başarısız oldu: {ex.Message}");
                    }
                    var entry = ex.Entries.Single();
                    var databaseValues = await entry.GetDatabaseValuesAsync();
                    if (databaseValues == null)
                    {
                        _logger.LogWarning($"FIFO kaydı {fifoEntry.StokFifoID} veritabanında bulunamadı, işlem iptal edildi.");
                        return null;
                    }
                    entry.OriginalValues.SetValues(databaseValues);
                    int waitTime = baseWaitTime * (int)Math.Pow(2, retryCount - 1);
                    waitTime += new Random().Next(0, 50);
                    _logger.LogInformation($"FIFO işlemi yeniden deneniyor. StokFifoID: {fifoEntry.StokFifoID}, Bekleme süresi: {waitTime}ms");
                    await Task.Delay(waitTime);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, $"FIFO işlemi sırasında beklenmeyen hata: StokFifoID: {fifoEntry.StokFifoID}, Hata: {ex.Message}");
                    throw;
                }
            }
            return null;
        }

        #endregion
    }
} 