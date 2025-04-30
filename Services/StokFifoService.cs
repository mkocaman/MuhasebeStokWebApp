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
            // Transaction yönetimi - dış transaction olup olmadığını kontrol et
            bool hasExistingTransaction = _context.Database.CurrentTransaction != null;
            IDbContextTransaction transaction = null;

            try
            {
                _logger.LogInformation($"StokFifoService.StokGirisiYap başladı: UrunID={urunID}, Miktar={miktar}, BirimFiyat={birimFiyat}, ParaBirimi={paraBirimi}");

                // Sadece dış transaction yoksa yeni bir transaction başlat
                if (!hasExistingTransaction)
                {
                    transaction = await _context.Database.BeginTransactionAsync(System.Data.IsolationLevel.ReadCommitted);
                    _logger.LogInformation($"StokGirisiYap - Yeni transaction başlatıldı: Ürün {urunID}, Miktar {miktar}");
                }
                
                // Null kontrolü ve varsayılan değer atamaları
                birim = birim ?? "Adet";
                referansNo = referansNo ?? "";
                referansTuru = referansTuru ?? "Fatura";
                aciklama = aciklama ?? "";
                paraBirimi = paraBirimi ?? "USD";
                dovizKuru = dovizKuru ?? 1m;
                
                _logger.LogInformation($"Parametreler normalize edildi: Birim={birim}, ReferansNo={referansNo}, ParaBirimi={paraBirimi}, DovizKuru={dovizKuru}");
                
                var stokFifo = new StokFifo
                {
                    StokFifoID = Guid.NewGuid(),
                    UrunID = urunID,
                    Miktar = miktar,
                    KalanMiktar = miktar,
                    BirimFiyat = birimFiyat,
                    Birim = birim,
                    ReferansNo = referansNo,
                    ReferansTuru = referansTuru,
                    ReferansID = referansID ?? Guid.Empty,
                    Aciklama = aciklama,
                    ParaBirimi = paraBirimi,
                    DovizKuru = dovizKuru.Value,
                    GirisTarihi = DateTime.Now,
                    OlusturmaTarihi = DateTime.Now,
                    Aktif = true,
                    Iptal = false,
                    Silindi = false
                };
                
                _logger.LogInformation($"FIFO nesnesi oluşturuldu: StokFifoID={stokFifo.StokFifoID}");
                
                // Para birimi dönüşümleri - optimize edilmiş
                try 
                {
                    // Para birimi dönüşümlerini dovizKuruService üzerinden yapılandır
                    if (paraBirimi != "USD")
                    {
                        // USD dönüşümü
                        decimal kurToUSD = await _dovizKuruService.GetGuncelKurAsync(paraBirimi, "USD");
                        if (kurToUSD > 0)
                        {
                            // UZS gibi büyük kurlar için böl, diğerleri için çarp
                            stokFifo.USDBirimFiyat = paraBirimi == "UZS" 
                                ? birimFiyat / kurToUSD 
                                : birimFiyat * kurToUSD;
                            
                            _logger.LogInformation($"{paraBirimi} -> USD birim fiyat hesaplandı: {birimFiyat} {(paraBirimi == "UZS" ? "/" : "*")} {kurToUSD} = {stokFifo.USDBirimFiyat} USD");
                        }
                        else if (dovizKuru.HasValue && dovizKuru.Value > 0)
                        {
                            // Faturada belirtilen kur değerini kullan
                            stokFifo.USDBirimFiyat = paraBirimi == "UZS" 
                                ? birimFiyat / dovizKuru.Value 
                                : birimFiyat * dovizKuru.Value;
                            
                            _logger.LogWarning($"{paraBirimi} -> USD için API'den kur alınamadı, faturadaki kur değeri kullanıldı: {birimFiyat} {(paraBirimi == "UZS" ? "/" : "*")} {dovizKuru.Value} = {stokFifo.USDBirimFiyat} USD");
                        }
                        else
                        {
                            // Kur değeri bulunamadı, hata fırlat
                            _logger.LogError($"{paraBirimi} -> USD için kur bilgisi bulunamadı ve faturada belirtilmemiş");
                            throw new InvalidOperationException($"{paraBirimi} para biriminden USD'ye dönüşüm için geçerli bir kur değeri bulunamadı. Lütfen faturada geçerli bir döviz kuru belirtin veya sistem yöneticinize başvurun.");
                        }
                    }
                    else
                    {
                        stokFifo.USDBirimFiyat = birimFiyat;
                        _logger.LogInformation($"USD birim fiyat direkt atandı (para birimi zaten USD): {stokFifo.USDBirimFiyat} USD");
                    }
                    
                    // UZS dönüşümü
                    if (paraBirimi != "UZS")
                    {
                        decimal kurToUZS = await _dovizKuruService.GetGuncelKurAsync(paraBirimi, "UZS");
                        if (kurToUZS > 0)
                        {
                            stokFifo.UZSBirimFiyat = birimFiyat * kurToUZS;
                            _logger.LogInformation($"{paraBirimi} -> UZS birim fiyat hesaplandı: {birimFiyat} * {kurToUZS} = {stokFifo.UZSBirimFiyat} UZS");
                        }
                        else
                        {
                            // USD değeri üzerinden UZS'ye çevir
                            decimal usdToUZS = await _dovizKuruService.GetGuncelKurAsync("USD", "UZS");
                            if (usdToUZS > 0 && stokFifo.USDBirimFiyat > 0)
                            {
                                stokFifo.UZSBirimFiyat = stokFifo.USDBirimFiyat * usdToUZS;
                                _logger.LogInformation($"{paraBirimi} -> UZS dönüşümü USD üzerinden yapıldı: {stokFifo.USDBirimFiyat} * {usdToUZS} = {stokFifo.UZSBirimFiyat} UZS");
                            }
                            else
                            {
                                _logger.LogWarning($"{paraBirimi} -> UZS ve USD -> UZS için kur bilgisi bulunamadı. UZS değeri 0 olarak kaydedilecek.");
                                stokFifo.UZSBirimFiyat = 0;
                            }
                        }
                    }
                    else
                    {
                        stokFifo.UZSBirimFiyat = birimFiyat;
                        _logger.LogInformation($"UZS birim fiyat direkt atandı (para birimi zaten UZS): {stokFifo.UZSBirimFiyat} UZS");
                    }
                    
                    // TRY dönüşümü
                    if (paraBirimi != "TRY")
                    {
                        decimal kurToTRY = await _dovizKuruService.GetGuncelKurAsync(paraBirimi, "TRY");
                        if (kurToTRY > 0)
                        {
                            stokFifo.TLBirimFiyat = birimFiyat * kurToTRY;
                            _logger.LogInformation($"{paraBirimi} -> TRY birim fiyat hesaplandı: {birimFiyat} * {kurToTRY} = {stokFifo.TLBirimFiyat} TRY");
                        }
                        else
                        {
                            // USD değeri üzerinden TRY'ye çevir
                            decimal usdToTRY = await _dovizKuruService.GetGuncelKurAsync("USD", "TRY");
                            if (usdToTRY > 0 && stokFifo.USDBirimFiyat > 0)
                            {
                                stokFifo.TLBirimFiyat = stokFifo.USDBirimFiyat * usdToTRY;
                                _logger.LogInformation($"{paraBirimi} -> TRY dönüşümü USD üzerinden yapıldı: {stokFifo.USDBirimFiyat} * {usdToTRY} = {stokFifo.TLBirimFiyat} TRY");
                            }
                            else
                            {
                                _logger.LogWarning($"{paraBirimi} -> TRY ve USD -> TRY için kur bilgisi bulunamadı. TRY değeri 0 olarak kaydedilecek.");
                                stokFifo.TLBirimFiyat = 0;
                            }
                        }
                    }
                    else
                    {
                        stokFifo.TLBirimFiyat = birimFiyat;
                        _logger.LogInformation($"TRY birim fiyat direkt atandı (para birimi zaten TRY): {stokFifo.TLBirimFiyat} TRY");
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, $"Para birimi dönüşümleri sırasında hata oluştu: {ex.Message}");
                    
                    // Ana para birimi (USD) dönüşümünde hata olduysa hata fırlat, kritik bir hatadır
                    if (paraBirimi != "USD" && stokFifo.USDBirimFiyat <= 0)
                    {
                        throw new InvalidOperationException($"Stok kaydı için gerekli USD dönüşümü yapılamadı: {ex.Message}", ex);
                    }
                    
                    // Diğer para birimleri için varsayılan 0 değerini kullan ama loglama yap
                    if (stokFifo.TLBirimFiyat <= 0)
                    {
                        _logger.LogWarning("TRY dönüşümü yapılamadı, değer 0 olarak atandı.");
                        stokFifo.TLBirimFiyat = 0;
                    }
                    
                    if (stokFifo.UZSBirimFiyat <= 0)
                    {
                        _logger.LogWarning("UZS dönüşümü yapılamadı, değer 0 olarak atandı.");
                        stokFifo.UZSBirimFiyat = 0;
                    }
                }
                
                // Veritabanına kaydet
                await _context.StokFifoKayitlari.AddAsync(stokFifo);
                await _context.SaveChangesAsync();
                
                _logger.LogInformation($"StokFifo kaydı başarıyla oluşturuldu: ID={stokFifo.StokFifoID}, Ürün={urunID}, Miktar={miktar}");
                
                // Transaction commit
                if (!hasExistingTransaction && transaction != null)
                {
                    await transaction.CommitAsync();
                    _logger.LogInformation($"StokGirisiYap - Transaction commit edildi: Ürün {urunID}, Miktar {miktar}");
                }
                
                return stokFifo;
            }
            catch (Exception ex)
            {
                // Transaction rollback
                if (!hasExistingTransaction && transaction != null)
                {
                    await transaction.RollbackAsync();
                    _logger.LogWarning($"StokGirisiYap - Transaction rollback edildi: Ürün {urunID}, Miktar {miktar}");
                }
                
                _logger.LogError(ex, $"Stok girişi yapılırken hata oluştu: {ex.Message}");
                throw;
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
                        // Veritabanından en güncel UZS/USD alış kuru alınmalı
                        var kurUZStoUSD = await _dovizKuruService.GetGuncelKurAsync("UZS", "USD");
                        if (kurUZStoUSD > 0)
                        {
                            stokFifo.USDBirimFiyat = stokFifo.BirimFiyat / kurUZStoUSD;
                            _logger.LogInformation($"UZS -> USD birim fiyat hesaplandı: {stokFifo.BirimFiyat} / {kurUZStoUSD} = {stokFifo.USDBirimFiyat} USD");
                        }
                        else if (stokFifo.DovizKuru > 0)
                        {
                            // Belirtilen kur değerini kullan
                            stokFifo.USDBirimFiyat = stokFifo.BirimFiyat / stokFifo.DovizKuru;
                            _logger.LogWarning($"UZS -> USD için API'den kur alınamadı, belirtilen kur değeri kullanıldı: {stokFifo.BirimFiyat} / {stokFifo.DovizKuru} = {stokFifo.USDBirimFiyat} USD");
                        }
                        else
                        {
                            // Son çare olarak varsayılan bir kur değeri kullan
                            // Örnek: 1 USD = 13000 UZS için
                            stokFifo.USDBirimFiyat = stokFifo.BirimFiyat / 13000.0m;
                            _logger.LogWarning($"UZS -> USD için kur bilgisi bulunamadı, varsayılan değer kullanıldı: {stokFifo.BirimFiyat} / 13000.0 = {stokFifo.USDBirimFiyat} USD");
                        }
                    }
                    else
                    {
                        // Diğer para birimleri için çarpma işlemi
                        var kurToUSD = await _dovizKuruService.GetGuncelKurAsync(stokFifo.ParaBirimi, "USD");
                        if (kurToUSD > 0)
                        {
                            stokFifo.USDBirimFiyat = stokFifo.BirimFiyat * kurToUSD;
                            _logger.LogInformation($"{stokFifo.ParaBirimi} -> USD birim fiyat hesaplandı: {stokFifo.BirimFiyat} * {kurToUSD} = {stokFifo.USDBirimFiyat} USD");
                        }
                        else if (stokFifo.DovizKuru > 0)
                        {
                            stokFifo.USDBirimFiyat = stokFifo.BirimFiyat * stokFifo.DovizKuru;
                            _logger.LogWarning($"{stokFifo.ParaBirimi} -> USD için API'den kur alınamadı, belirtilen kur değeri kullanıldı: {stokFifo.BirimFiyat} * {stokFifo.DovizKuru} = {stokFifo.USDBirimFiyat} USD");
                        }
                        else
                        {
                            stokFifo.USDBirimFiyat = stokFifo.BirimFiyat; // Varsayılan değer olarak 1:1 kullan
                            _logger.LogWarning($"{stokFifo.ParaBirimi} -> USD için kur alınamadı, 1:1 varsayılan değer kullanıldı");
                        }
                    }
                }
                else
                {
                    stokFifo.USDBirimFiyat = stokFifo.BirimFiyat;
                    _logger.LogInformation($"USD birim fiyat direkt atandı (para birimi zaten USD): {stokFifo.USDBirimFiyat} USD");
                }
                
                // TL dönüşümleri - Sonsuz döngü önleme için try-catch bloğu ekleyerek
                try {
                    // UZS dönüşümü
                    if (stokFifo.ParaBirimi == "UZS")
                    {
                        stokFifo.UZSBirimFiyat = stokFifo.BirimFiyat;
                        _logger.LogInformation($"UZS birim fiyat direkt atandı (para birimi zaten UZS): {stokFifo.UZSBirimFiyat} UZS");
                    }
                    else if (stokFifo.ParaBirimi == "USD")
                    {
                        // USD -> UZS çevirimi için özel hesaplama
                        var kurUSDtoUZS = await _dovizKuruService.GetGuncelKurAsync("USD", "UZS");
                        if (kurUSDtoUZS > 0)
                        {
                            // USD'den UZS'ye dönüşüm için çarpma işlemi yapılır
                            stokFifo.UZSBirimFiyat = stokFifo.BirimFiyat * kurUSDtoUZS;
                            _logger.LogInformation($"USD -> UZS birim fiyat hesaplandı: {stokFifo.BirimFiyat} * {kurUSDtoUZS} = {stokFifo.UZSBirimFiyat} UZS");
                        }
                        else if (stokFifo.DovizKuru > 0)
                        {
                            // Belirtilen kur değerini kullan
                            stokFifo.UZSBirimFiyat = stokFifo.BirimFiyat * stokFifo.DovizKuru;
                            _logger.LogWarning($"USD -> UZS için API'den kur alınamadı, belirtilen kur değeri kullanıldı: {stokFifo.BirimFiyat} * {stokFifo.DovizKuru} = {stokFifo.UZSBirimFiyat} UZS");
                        }
                        else
                        {
                            // Son çare olarak varsayılan bir kur değeri kullan
                            stokFifo.UZSBirimFiyat = stokFifo.BirimFiyat * 13000.0m;
                            _logger.LogWarning($"USD -> UZS için kur bilgisi bulunamadı, varsayılan değer kullanıldı: {stokFifo.BirimFiyat} * 13000.0 = {stokFifo.UZSBirimFiyat} UZS");
                        }
                    }
                    else
                    {
                        // Diğer para birimleri -> UZS çevrimi
                        var kurToUZS = await _dovizKuruService.GetGuncelKurAsync(stokFifo.ParaBirimi, "UZS");
                        if (kurToUZS > 0)
                        {
                            stokFifo.UZSBirimFiyat = stokFifo.BirimFiyat * kurToUZS;
                            _logger.LogInformation($"{stokFifo.ParaBirimi} -> UZS birim fiyat hesaplandı: {stokFifo.BirimFiyat} * {kurToUZS} = {stokFifo.UZSBirimFiyat} UZS");
                        }
                        else
                        {
                            // USD üzerinden çevirme yap
                            stokFifo.UZSBirimFiyat = stokFifo.USDBirimFiyat * 13000.0m; // Varsayılan USD/UZS kurunu kullan
                            _logger.LogWarning($"{stokFifo.ParaBirimi} -> UZS için kur alınamadı, USD üzerinden hesaplama yapıldı: {stokFifo.USDBirimFiyat} * 13000.0 = {stokFifo.UZSBirimFiyat} UZS");
                        }
                    }
                
                    // TL dönüşümü
                    if (stokFifo.ParaBirimi == "TRY") 
                    {
                        stokFifo.TLBirimFiyat = stokFifo.BirimFiyat;
                    }
                    else
                    {
                        var kurTL = await _dovizKuruService.GetGuncelKurAsync(stokFifo.ParaBirimi, "TRY");
                        if (kurTL > 0)
                        {
                            stokFifo.TLBirimFiyat = stokFifo.BirimFiyat * kurTL;
                            _logger.LogInformation($"{stokFifo.ParaBirimi} -> TRY birim fiyat hesaplandı: {stokFifo.BirimFiyat} * {kurTL} = {stokFifo.TLBirimFiyat} TRY");
                        }
                        else
                        {
                            // USD değeri üzerinden TL değeri hesapla
                            var kurUSDtoTRY = await _dovizKuruService.GetGuncelKurAsync("USD", "TRY");
                            if (kurUSDtoTRY > 0)
                            {
                                stokFifo.TLBirimFiyat = stokFifo.USDBirimFiyat * kurUSDtoTRY;
                            }
                            else
                            {
                                stokFifo.TLBirimFiyat = stokFifo.USDBirimFiyat * 30.0m; // Varsayılan USD/TRY kuru
                                _logger.LogWarning($"USD -> TRY için kur alınamadı, varsayılan değer kullanıldı.");
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Para birimi dönüşümü sırasında hata. Varsayılan değerler kullanılıyor.");
                    // Varsayılan değerler kullanarak devam et
                    if (stokFifo.TLBirimFiyat <= 0) stokFifo.TLBirimFiyat = stokFifo.BirimFiyat * 30.0m;
                    if (stokFifo.UZSBirimFiyat <= 0) stokFifo.UZSBirimFiyat = stokFifo.BirimFiyat * 12000.0m;
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
        /// Stok çıkışı yapan temel metot. FIFO prensibine göre en eski stokları kullanır.
        /// </summary>
        public async Task<(List<StokFifo> KullanilanFifoKayitlari, decimal ToplamMaliyet)> StokCikisiYap(Guid urunID, decimal miktar, Guid? referansID = null, StokHareketiTipi hareketTipi = StokHareketiTipi.Cikis)
        {
            // Transaction yönetimi - dış transaction olup olmadığını kontrol et
            bool hasExistingTransaction = _context.Database.CurrentTransaction != null;
            IDbContextTransaction transaction = null;

            try
            {
                _logger.LogInformation($"StokCikisiYap başladı: Ürün {urunID}, Miktar {miktar}, Hareket Tipi {hareketTipi}");
                
                // Sadece dış transaction yoksa yeni bir transaction başlat
                if (!hasExistingTransaction)
                {
                    transaction = await _context.Database.BeginTransactionAsync(System.Data.IsolationLevel.ReadCommitted);
                    _logger.LogInformation($"StokCikisiYap - Yeni transaction başlatıldı: Ürün {urunID}, Miktar {miktar}");
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

                if (!fifoKayitlari.Any())
                {
                    _logger.LogWarning($"Stok çıkışı yapılamadı: FIFO kaydı bulunamadı (Ürün ID: {urunID})");
                    throw new Exception($"FIFO kaydı bulunamadı (Ürün ID: {urunID})");
                }
                
                // FIFO kayıtlarındaki toplam kalan miktarı kontrol et
                decimal toplamFifoMiktari = fifoKayitlari.Sum(f => f.KalanMiktar);
                if (toplamFifoMiktari < miktar)
                {
                    _logger.LogWarning($"Yetersiz stok: Ürün {urun.UrunAdi} (ID: {urunID}), FIFO Stok: {toplamFifoMiktari}, İstenen: {miktar}");
                    
                    // Ayrıntılı hata fırlat
                    throw new StokYetersizException(urunID, urun.UrunAdi, urun.UrunKodu, miktar, toplamFifoMiktari);
                }
                
                _logger.LogInformation($"FIFO kayıtları bulundu: {fifoKayitlari.Count} adet kayıt, toplam kalan miktar: {toplamFifoMiktari}");
                
                // Çıkış işlemi için kullanılacak FIFO kayıtları ve toplam maliyet
                var kullanilanFifoKayitlari = new List<StokFifo>();
                decimal toplamMaliyet = 0;
                decimal kalanMiktar = miktar;
                
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
                        BirimMaliyet = fifo.USDBirimFiyat,
                        ToplamMaliyet = kullanilanMiktar * fifo.USDBirimFiyat,
                        ParaBirimi = "USD",
                        Aciklama = $"{hareketTipi} hareketi ile stok çıkışı (FIFO)",
                        OlusturmaTarihi = DateTime.Now,
                        Iptal = false
                    };
                    
                    await _context.StokCikisDetaylari.AddAsync(stokCikisDetay);
                    
                    // FIFO kaydının kalan miktarını güncelle - concurrency yönetimi ile
                    decimal kullanilacakMiktar = kullanilanMiktar; // Yerel değişkene kopyala
                    
                    bool fifoUpdateSuccess = await ProcessFifoEntryWithRetry(fifo, async (fifoToUpdate) => {
                        fifoToUpdate.KalanMiktar -= kullanilacakMiktar;
                        fifoToUpdate.SonCikisTarihi = DateTime.Now;
                        fifoToUpdate.GuncellemeTarihi = DateTime.Now;
                        _context.StokFifoKayitlari.Update(fifoToUpdate);
                    });
                    
                    if (!fifoUpdateSuccess)
                    {
                        _logger.LogWarning($"FIFO kaydı güncellenemedi: ID {fifo.StokFifoID}, Çıkış işlemi tamamlanamayabilir");
                        throw new Exception($"FIFO kaydı güncellenemedi: ID {fifo.StokFifoID}");
                    }
                    
                    // Toplam maliyeti hesapla (kullanılan miktar * birim fiyat)
                    decimal hareketMaliyeti = kullanilanMiktar * fifo.USDBirimFiyat;
                    toplamMaliyet += hareketMaliyeti;
                    
                    _logger.LogInformation($"FIFO kaydı kullanıldı: ID {fifo.StokFifoID}, Miktar: {kullanilanMiktar}, Maliyet: {hareketMaliyeti:N2} USD, Kalan: {fifo.KalanMiktar}");
                    
                    kullanilanFifoKayitlari.Add(fifo);
                    kalanMiktar -= kullanilanMiktar;
                }
                
                if (kalanMiktar > 0)
                {
                    _logger.LogWarning($"Stok çıkışı tam yapılamadı: Talep edilen {miktar}, kalan {kalanMiktar}");
                    throw new Exception($"Yetersiz stok: Talep edilen {miktar}, kalan {kalanMiktar}");
                }
                
                // Değişiklikleri veritabanına kaydet
                await _context.SaveChangesAsync();
                
                // Transaction commit
                if (!hasExistingTransaction && transaction != null)
                {
                    await transaction.CommitAsync();
                    _logger.LogInformation($"StokCikisiYap - Transaction commit edildi: Ürün {urunID}, Miktar {miktar}");
                }
                
                _logger.LogInformation($"Stok çıkışı işlemi tamamlandı: Ürün {urunID}, toplam çıkış miktarı {miktar}, toplam maliyet {toplamMaliyet:N2} USD");
                
                return (kullanilanFifoKayitlari, toplamMaliyet);
            }
            catch (Exception ex)
            {
                // Transaction rollback
                if (!hasExistingTransaction && transaction != null)
                {
                    await transaction.RollbackAsync();
                    _logger.LogWarning($"StokCikisiYap - Transaction rollback edildi: Ürün {urunID}, Miktar {miktar}");
                }
                
                _logger.LogError(ex, $"Stok çıkışı yapılırken hata oluştu: {ex.Message}");
                throw;
            }
        }

        /// <summary>
        /// Stok çıkışı yapan genişletilmiş metot. Referans bilgileri ile çıkış yapar.
        /// </summary>
        public async Task<(List<StokFifo> KullanilanFifoKayitlari, decimal ToplamMaliyet)> StokCikisiYap(Guid urunID, decimal miktar, string referansNo, string referansTuru, Guid? referansID, string aciklama)
        {
            try
            {
                _logger.LogInformation($"StokCikisiYap (genişletilmiş) başladı: Ürün {urunID}, Miktar {miktar}, Ref: {referansTuru}/{referansNo}");
                
                // Basit stok çıkışı metodunu çağır
                var result = await StokCikisiYap(urunID, miktar, referansID, StokHareketiTipi.Cikis);
                
                // Ek log kaydı - referans bilgileri
                _logger.LogInformation($"Stok çıkışı referans bilgileri: Tür: {referansTuru}, No: {referansNo}, ID: {referansID}, Açıklama: {aciklama}");
                
                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Stok çıkışı yapılırken hata oluştu: {ex.Message}");
                throw;
            }
        }

        /// <summary>
        /// Belirli bir referansa ait FIFO kayıtlarını iptal eder.
        /// </summary>
        public async Task<List<StokFifo>> FifoKayitlariniIptalEt(Guid referansID, string referansTuru, string iptalAciklama, Guid? iptalEdenKullaniciID = null)
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
                    _logger.LogInformation($"FifoKayitlariniIptalEt - Yeni transaction başlatıldı: {referansID}, {referansTuru}");
                }
                else
                {
                    _logger.LogInformation($"FifoKayitlariniIptalEt - Mevcut transaction kullanılıyor: {referansID}, {referansTuru}");
                }
                
                // İptal edilecek FIFO kayıtlarını bulma
                var fifoKayitlari = await _context.StokFifoKayitlari
                    .Where(f => f.ReferansID == referansID && f.ReferansTuru == referansTuru && !f.Iptal && !f.Silindi)
                    .ToListAsync();
                    
                foreach (var fifo in fifoKayitlari)
                {
                    fifo.Iptal = true;
                    fifo.IptalTarihi = DateTime.Now;
                    fifo.IptalAciklama = iptalAciklama;
                    fifo.IptalEdenKullaniciID = iptalEdenKullaniciID;
                    fifo.GuncellemeTarihi = DateTime.Now;
                }
                
                // Eğer StokCikisDetay entity'si mevcutsa, ilgili çıkış kayıtlarını da iptal et
                try
                {
                    if (_context.Model.FindEntityType(typeof(StokCikisDetay)) != null)
                    {
                        // Bu entity var mı emin değiliz, o yüzden dinamik olarak kontrol ediyoruz
                        var cikisDetaylar = await _context.StokCikisDetaylari
                            .Where(c => c.ReferansID == referansID && c.ReferansTuru == referansTuru)
                            .ToListAsync();
                            
                        foreach (var cikis in cikisDetaylar)
                        {
                            // İlgili FIFO kaydının kalan miktarını geri artır
                            var fifo = await _context.StokFifoKayitlari.FindAsync(cikis.StokFifoID);
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
                            
                            // Çıkış kaydını işaretle
                            cikis.Iptal = true;
                            cikis.IptalTarihi = DateTime.Now;
                            cikis.IptalAciklama = iptalAciklama;
                        }
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "StokCikisDetay iptal sırasında hata oluştu. Bu bir kritik hata değil, işlem devam ediyor.");
                }
                
                await _context.SaveChangesAsync();
                
                // Sadece biz başlattıysak transaction'ı commit et
                if (!hasExistingTransaction && transaction != null)
                {
                    await transaction.CommitAsync();
                    _logger.LogInformation($"FifoKayitlariniIptalEt - Transaction commit edildi: {referansID}, {referansTuru}");
                }
                
                _logger.LogInformation($"FIFO kayıtları iptal edildi: Referans ID: {referansID}, Tür: {referansTuru}, İptal edilen kayıt sayısı: {fifoKayitlari.Count}");
                return fifoKayitlari;
            }
            catch (Exception ex)
            {
                // Sadece biz başlattıysak transaction'ı geri al
                if (!hasExistingTransaction && transaction != null)
                {
                    await transaction.RollbackAsync();
                    _logger.LogWarning($"FifoKayitlariniIptalEt - Transaction geri alındı: {referansID}, {referansTuru}");
                }
                
                _logger.LogError(ex, $"FIFO kayıtları iptal edilirken hata oluştu: Referans ID: {referansID}, Tür: {referansTuru}");
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
        /// Bir ürünün ortalama maliyetini hesaplar. Kalan FIFO kayıtlarına göre ağırlıklı ortalama alınır.
        /// </summary>
        public async Task<decimal> GetOrtalamaMaliyet(Guid urunID, string paraBirimi = "USD")
        {
            try
            {
                // Aktif FIFO kayıtlarını alıp ortalama maliyet hesaplama
                var fifoKayitlari = await _context.StokFifoKayitlari
                    .Where(f => f.UrunID == urunID && f.KalanMiktar > 0 && f.Aktif && !f.Iptal && !f.Silindi)
                    .ToListAsync();
                    
                if (!fifoKayitlari.Any() || fifoKayitlari.Sum(f => f.KalanMiktar) == 0)
                    return 0;

                decimal toplamMaliyet = 0;
                decimal toplamMiktar = 0;

                foreach (var fifo in fifoKayitlari)
                {
                    switch (paraBirimi.ToUpper())
                    {
                        case "USD":
                            toplamMaliyet += fifo.KalanMiktar * fifo.USDBirimFiyat;
                            break;
                        case "TRY":
                            toplamMaliyet += fifo.KalanMiktar * fifo.TLBirimFiyat;
                            break;
                        case "UZS":
                            toplamMaliyet += fifo.KalanMiktar * fifo.UZSBirimFiyat;
                            break;
                        default:
                            toplamMaliyet += fifo.KalanMiktar * fifo.USDBirimFiyat;
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
                // Belirli bir tarihe kadar olan stok hareketlerini hesaplayarak maliyeti bul
                var fifoKayitlari = await _context.StokFifoKayitlari
                    .Where(f => f.UrunID == stokId && f.GirisTarihi <= tarih && f.Aktif && !f.Iptal && !f.Silindi)
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
                    
                    // Maliyet hesapla
                    decimal birimFiyat = 0;
                    switch (paraBirimi.ToUpper())
                    {
                        case "USD":
                            birimFiyat = fifo.USDBirimFiyat;
                            break;
                        case "TRY":
                            birimFiyat = fifo.TLBirimFiyat;
                            break;
                        case "UZS":
                            birimFiyat = fifo.UZSBirimFiyat;
                            break;
                        default:
                            birimFiyat = fifo.USDBirimFiyat;
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

        #endregion

        #region Yardımcı Metotlar

        /// <summary>
        /// İki para birimi arasındaki döviz kurunu getirir.
        /// </summary>
        private async Task<decimal> GetParaBirimiKuru(string kaynakParaBirimi, string hedefParaBirimi, DateTime tarih)
        {
            try
            {
                // Aynı para birimi ise 1 dön
                if (kaynakParaBirimi == hedefParaBirimi)
                    return 1m;
                
                _logger.LogInformation($"GetParaBirimiKuru çalıştırılıyor: {kaynakParaBirimi} -> {hedefParaBirimi}, Tarih: {tarih}");
                
                // Döviz kuru servisi kontrol
                if (_dovizKuruService == null)
                {
                    _logger.LogError("Döviz kuru servisi bulunamadı - DI yapılandırmasını kontrol edin");
                    throw new InvalidOperationException("Döviz kuru servisi bulunamadı. Sistem yöneticinizle iletişime geçin.");
                }
                
                try 
                {
                    // Servis çağrısına bir timeout ekleyelim
                    using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(5));
                    var kurTask = _dovizKuruService.GetGuncelKurAsync(kaynakParaBirimi, hedefParaBirimi, tarih);
                    
                    // Task tamamlanmadan önce timeout olursa, hata fırlat
                    var completedTask = await Task.WhenAny(kurTask, Task.Delay(5000, cts.Token));
                    if (completedTask != kurTask)
                    {
                        _logger.LogWarning($"Döviz kuru servisi yanıt vermedi (timeout): {kaynakParaBirimi} -> {hedefParaBirimi}");
                        cts.Cancel();
                        throw new TimeoutException($"Döviz kuru servisi yanıt vermedi: {kaynakParaBirimi} -> {hedefParaBirimi}");
                    }
                    
                    var kurDegeri = await kurTask;
                    
                    if (kurDegeri > 0)
                    {
                        _logger.LogInformation($"Kur değeri başarıyla alındı: {kaynakParaBirimi} -> {hedefParaBirimi} = {kurDegeri}");
                        return kurDegeri;
                    }
                    else
                    {
                        _logger.LogError($"Geçersiz kur değeri alındı: {kaynakParaBirimi} -> {hedefParaBirimi} = {kurDegeri}");
                        throw new InvalidOperationException($"Geçersiz kur değeri alındı (0 veya negatif): {kaynakParaBirimi} -> {hedefParaBirimi}");
                    }
                }
                catch (TaskCanceledException ex)
                {
                    _logger.LogWarning($"Döviz kuru servisi timeout: {ex.Message}");
                    throw new TimeoutException($"Döviz kuru servisi yanıt vermedi: {kaynakParaBirimi} -> {hedefParaBirimi}", ex);
                }
                catch (TimeoutException)
                {
                    throw;
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, $"Döviz kuru servisinden değer alınırken hata: {kaynakParaBirimi} -> {hedefParaBirimi}");
                    throw new InvalidOperationException($"Döviz kuru alınırken hata oluştu: {kaynakParaBirimi} -> {hedefParaBirimi}", ex);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Döviz kuru alınırken hata oluştu: {kaynakParaBirimi} -> {hedefParaBirimi}, Tarih: {tarih}");
                throw new InvalidOperationException($"Döviz kuru alınırken hata oluştu: {kaynakParaBirimi} -> {hedefParaBirimi}, Tarih: {tarih}", ex);
            }
        }

        /// <summary>
        /// Concurrency yönetimi ile FIFO kaydı işleme ve güncellenen kaydı döndürme
        /// </summary>
        /// <param name="fifoEntry">İşlenecek FIFO kaydı</param>
        /// <param name="processAction">FIFO kaydı üzerinde yapılacak işlem</param>
        /// <param name="maxRetries">Maksimum yeniden deneme sayısı</param>
        /// <returns>Güncellenmiş FIFO kaydı veya null</returns>
        public async Task<StokFifo> ProcessFifoEntryWithRetryAndReturn(StokFifo fifoEntry, Func<StokFifo, Task> processAction, int maxRetries = 3)
        {
            bool retryNeeded = true;
            int retryCount = 0;
            
            while (retryNeeded && retryCount < maxRetries)
            {
                try
                {
                    // FIFO kaydı işleme
                    await processAction(fifoEntry);
                    
                    // Değişiklikleri kaydet
                    await _context.SaveChangesAsync();
                    
                    // İşlem başarılı, retry gerek yok
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
                        throw new FifoConcurrencyException(fifoEntry.StokFifoID, retryCount, 
                            $"FIFO işlemi {maxRetries} deneme sonrasında başarısız oldu: {ex.Message}");
                    }
                    
                    // Veritabanındaki güncel değerleri al
                    var entry = ex.Entries.Single();
                    var databaseValues = await entry.GetDatabaseValuesAsync();
                    
                    if (databaseValues == null)
                    {
                        // Kayıt silinmiş, retry gereksiz
                        _logger.LogWarning($"FIFO kaydı {fifoEntry.StokFifoID} veritabanında bulunamadı, işlem iptal edildi.");
                        return null;
                    }
                    
                    // Değerleri güncelle ve yeniden dene
                    entry.OriginalValues.SetValues(databaseValues);
                    
                    // Bekle ve tekrar dene (exponential backoff - artan bekleme süresi)
                    int waitTime = (int)Math.Pow(2, retryCount) * 100; // 100ms, 200ms, 400ms...
                    await Task.Delay(waitTime);
                }
                catch (Exception ex)
                {
                    _logger.LogError($"FIFO işlemi sırasında beklenmeyen hata: {ex.Message}");
                    throw;
                }
            }
            
            return null;
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
            
            while (retryNeeded && retryCount < maxRetries)
            {
                try
                {
                    // FIFO kaydı işleme
                    await processAction(fifoEntry);
                    
                    // Değişiklikleri kaydet
                    await _context.SaveChangesAsync();
                    
                    // İşlem başarılı, retry gerek yok
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
                        throw new FifoConcurrencyException(fifoEntry.StokFifoID, retryCount, 
                            $"FIFO işlemi {maxRetries} deneme sonrasında başarısız oldu: {ex.Message}");
                    }
                    
                    // Veritabanındaki güncel değerleri al
                    var entry = ex.Entries.Single();
                    var databaseValues = await entry.GetDatabaseValuesAsync();
                    
                    if (databaseValues == null)
                    {
                        // Kayıt silinmiş, retry gereksiz
                        _logger.LogWarning($"FIFO kaydı {fifoEntry.StokFifoID} veritabanında bulunamadı, işlem iptal edildi.");
                        return false;
                    }
                    
                    // Değerleri güncelle ve yeniden dene
                    entry.OriginalValues.SetValues(databaseValues);
                    
                    // Bekle ve tekrar dene (exponential backoff - artan bekleme süresi)
                    int waitTime = (int)Math.Pow(2, retryCount) * 100; // 100ms, 200ms, 400ms...
                    await Task.Delay(waitTime);
                }
                catch (Exception ex)
                {
                    _logger.LogError($"FIFO işlemi sırasında beklenmeyen hata: {ex.Message}");
                    throw;
                }
            }
            
            return false;
        }

        #endregion
    }
} 