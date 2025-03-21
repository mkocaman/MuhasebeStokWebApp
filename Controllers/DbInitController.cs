using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MuhasebeStokWebApp.Data;
using MuhasebeStokWebApp.Data.Entities;
using MuhasebeStokWebApp.Models;

namespace MuhasebeStokWebApp.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class DbInitController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public DbInitController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: api/DbInit/InitSistemAyarlari
        [HttpGet("InitSistemAyarlari")]
        public async Task<IActionResult> InitSistemAyarlari()
        {
            try
            {
                // SistemAyarlari tablosunda aktif kayıt var mı kontrol et
                var existingSettings = await _context.SistemAyarlari
                    .FirstOrDefaultAsync(s => s.Aktif && !s.SoftDelete);

                if (existingSettings != null)
                {
                    return Ok($"SistemAyarlari zaten mevcut. ID: {existingSettings.SistemAyarlariID}");
                }

                // Yeni SistemAyarlari oluştur
                var sistemAyarlari = new SistemAyarlari
                {
                    // SistemAyarlariID otomatik atanacak
                    AnaDovizKodu = "TRY",
                    SirketAdi = "Şirketim",
                    SirketAdresi = "İstanbul, Türkiye",
                    SirketTelefon = "+90 212 123 4567",
                    SirketEmail = "info@muhasebe-stok.com",
                    SirketVergiNo = "1234567890",
                    SirketVergiDairesi = "İstanbul Vergi Dairesi",
                    OtomatikDovizGuncelleme = true,
                    DovizGuncellemeSikligi = 24,
                    SonDovizGuncellemeTarihi = DateTime.Now,
                    Aktif = true,
                    SoftDelete = false,
                    OlusturmaTarihi = DateTime.Now
                };

                _context.SistemAyarlari.Add(sistemAyarlari);
                await _context.SaveChangesAsync();

                return Ok($"SistemAyarlari başarıyla oluşturuldu. ID: {sistemAyarlari.SistemAyarlariID}");
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Hata oluştu: {ex.Message}");
            }
        }

        // GET: api/DbInit/InitKurDegerleri
        [HttpGet("InitKurDegerleri")]
        public async Task<IActionResult> InitKurDegerleri()
        {
            try
            {
                // KurDegerleri tablosunda aktif kayıt var mı kontrol et
                var existingRates = await _context.KurDegerleri
                    .Where(k => k.Aktif && !k.Silindi)
                    .AnyAsync();

                if (existingRates)
                {
                    return Ok("Döviz kurları zaten mevcut.");
                }

                // Para birimleri var mı kontrol et
                bool paraBirimleriExists = await _context.Set<Data.Entities.ParaBirimi>().AnyAsync();
                if (!paraBirimleriExists)
                {
                    // Para birimleri yoksa ekle
                    await InitParaBirimleri();
                }

                // Para birimlerini al
                var usd = await _context.Set<Data.Entities.ParaBirimi>().FirstOrDefaultAsync(p => p.Kod == "USD");
                var try_ = await _context.Set<Data.Entities.ParaBirimi>().FirstOrDefaultAsync(p => p.Kod == "TRY");
                var eur = await _context.Set<Data.Entities.ParaBirimi>().FirstOrDefaultAsync(p => p.Kod == "EUR");
                var gbp = await _context.Set<Data.Entities.ParaBirimi>().FirstOrDefaultAsync(p => p.Kod == "GBP");

                if (usd == null || try_ == null || eur == null || gbp == null)
                {
                    return BadRequest("Para birimleri bulunamadı. Önce para birimlerini ekleyin.");
                }

                // Yeni kur değerleri oluştur
                var kurDegerleri = new List<KurDegeri>();
                
                // Önce para birimi ilişkilerini oluşturalım
                var usdTryIliski = new ParaBirimiIliski
                {
                    ParaBirimiIliskiID = Guid.NewGuid(),
                    KaynakParaBirimiID = usd.ParaBirimiID,
                    HedefParaBirimiID = try_.ParaBirimiID,
                    Carpan = 28.5m, // USD/TRY
                    Aktif = true,
                    Silindi = false,
                    OlusturmaTarihi = DateTime.Now,
                    GuncellemeTarihi = DateTime.Now
                };
                
                var eurTryIliski = new ParaBirimiIliski
                {
                    ParaBirimiIliskiID = Guid.NewGuid(),
                    KaynakParaBirimiID = eur.ParaBirimiID,
                    HedefParaBirimiID = try_.ParaBirimiID,
                    Carpan = 31.2m, // EUR/TRY
                    Aktif = true,
                    Silindi = false,
                    OlusturmaTarihi = DateTime.Now,
                    GuncellemeTarihi = DateTime.Now
                };
                
                var gbpTryIliski = new ParaBirimiIliski
                {
                    ParaBirimiIliskiID = Guid.NewGuid(),
                    KaynakParaBirimiID = gbp.ParaBirimiID,
                    HedefParaBirimiID = try_.ParaBirimiID,
                    Carpan = 36.5m, // GBP/TRY
                    Aktif = true,
                    Silindi = false,
                    OlusturmaTarihi = DateTime.Now,
                    GuncellemeTarihi = DateTime.Now
                };
                
                _context.Set<ParaBirimiIliski>().Add(usdTryIliski);
                _context.Set<ParaBirimiIliski>().Add(eurTryIliski);
                _context.Set<ParaBirimiIliski>().Add(gbpTryIliski);
                await _context.SaveChangesAsync(); // İlişkileri kaydet
                
                // Şimdi kur değerlerini oluştur
                var usdTryCross = new KurDegeri
                {
                    KurDegeriID = Guid.NewGuid(),
                    ParaBirimiID = usd.ParaBirimiID,
                    AlisDegeri = 28.4m,
                    SatisDegeri = 28.6m,
                    Tarih = DateTime.Now,
                    Kaynak = "Manuel",
                    Aktif = true,
                    Silindi = false
                };
                
                var eurTryCross = new KurDegeri
                {
                    KurDegeriID = Guid.NewGuid(),
                    ParaBirimiID = eur.ParaBirimiID,
                    AlisDegeri = 31.1m,
                    SatisDegeri = 31.3m,
                    Tarih = DateTime.Now,
                    Kaynak = "Manuel",
                    Aktif = true,
                    Silindi = false
                };
                
                var gbpTryCross = new KurDegeri
                {
                    KurDegeriID = Guid.NewGuid(),
                    ParaBirimiID = gbp.ParaBirimiID,
                    AlisDegeri = 36.4m,
                    SatisDegeri = 36.6m,
                    Tarih = DateTime.Now,
                    Kaynak = "Manuel",
                    Aktif = true,
                    Silindi = false
                };
                
                kurDegerleri.Add(usdTryCross);
                kurDegerleri.Add(eurTryCross);
                kurDegerleri.Add(gbpTryCross);

                // KurDegeri'leri DovizKuru'ya dönüştür
                var dovizKurlari = new List<MuhasebeStokWebApp.Data.Entities.DovizKuru>();
                
                foreach (var kurDegeri in kurDegerleri)
                {
                    var paraBirimi = await _context.Set<Data.Entities.ParaBirimi>().FindAsync(kurDegeri.ParaBirimiID);
                    
                    dovizKurlari.Add(new MuhasebeStokWebApp.Data.Entities.DovizKuru
                    {
                        ParaBirimi = paraBirimi.Kod,
                        BazParaBirimi = "TRY",
                        Alis = kurDegeri.AlisDegeri,
                        Satis = kurDegeri.SatisDegeri,
                        EfektifAlis = kurDegeri.AlisDegeri * 0.99m, // Örnek efektif alış değeri
                        EfektifSatis = kurDegeri.SatisDegeri * 1.01m, // Örnek efektif satış değeri
                        Tarih = kurDegeri.Tarih,
                        Kaynak = kurDegeri.Kaynak,
                        Aciklama = $"{paraBirimi.Kod}/TRY kurları ({DateTime.Now})",
                        Aktif = true,
                        SoftDelete = false,
                        OlusturmaTarihi = DateTime.Now
                    });
                }

                _context.DovizKurlari.AddRange(dovizKurlari);
                await _context.SaveChangesAsync();

                // Kur değerlerini ekle
                _context.KurDegerleri.AddRange(kurDegerleri);
                await _context.SaveChangesAsync();

                return Ok($"{dovizKurlari.Count} adet döviz kuru başarıyla oluşturuldu.");
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Hata oluştu: {ex.Message}");
            }
        }
        
        // GET: api/DbInit/InitParaBirimleri
        [HttpGet("InitParaBirimleri")]
        public async Task<IActionResult> InitParaBirimleri()
        {
            try
            {
                // ParaBirimleri tablosunda aktif kayıt var mı kontrol et
                var existingCurrencies = await _context.Set<Data.Entities.ParaBirimi>().AnyAsync();

                if (existingCurrencies)
                {
                    return Ok("Para birimleri zaten mevcut.");
                }

                // Yeni para birimleri oluştur
                var paraBirimleri = new List<Data.Entities.ParaBirimi>();
                
                var usd = new Data.Entities.ParaBirimi
                {
                    ParaBirimiID = Guid.NewGuid(),
                    Kod = "USD",
                    Ad = "ABD Doları",
                    Sembol = "$",
                    Aktif = true,
                    Silindi = false,
                    OlusturmaTarihi = DateTime.Now,
                    GuncellemeTarihi = DateTime.Now
                };
                
                var try_ = new Data.Entities.ParaBirimi
                {
                    ParaBirimiID = Guid.NewGuid(),
                    Kod = "TRY",
                    Ad = "Türk Lirası",
                    Sembol = "₺",
                    Aktif = true,
                    Silindi = false,
                    OlusturmaTarihi = DateTime.Now,
                    GuncellemeTarihi = DateTime.Now
                };
                
                var eur = new Data.Entities.ParaBirimi
                {
                    ParaBirimiID = Guid.NewGuid(),
                    Kod = "EUR",
                    Ad = "Euro",
                    Sembol = "€",
                    Aktif = true,
                    Silindi = false,
                    OlusturmaTarihi = DateTime.Now,
                    GuncellemeTarihi = DateTime.Now
                };
                
                var gbp = new Data.Entities.ParaBirimi
                {
                    ParaBirimiID = Guid.NewGuid(),
                    Kod = "GBP",
                    Ad = "İngiliz Sterlini",
                    Sembol = "£",
                    Aktif = true,
                    Silindi = false,
                    OlusturmaTarihi = DateTime.Now,
                    GuncellemeTarihi = DateTime.Now
                };
                
                var uzs = new Data.Entities.ParaBirimi
                {
                    ParaBirimiID = Guid.NewGuid(),
                    Kod = "UZS",
                    Ad = "Özbekistan Somu",
                    Sembol = "Soʻm",
                    Aktif = true,
                    Silindi = false,
                    OlusturmaTarihi = DateTime.Now,
                    GuncellemeTarihi = DateTime.Now
                };
                
                paraBirimleri.Add(usd);
                paraBirimleri.Add(try_);
                paraBirimleri.Add(eur);
                paraBirimleri.Add(gbp);
                paraBirimleri.Add(uzs);

                _context.Set<Data.Entities.ParaBirimi>().AddRange(paraBirimleri);
                await _context.SaveChangesAsync();

                return Ok($"{paraBirimleri.Count} adet para birimi başarıyla oluşturuldu.");
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Hata oluştu: {ex.Message}");
            }
        }
        
        // GET: api/DbInit/InitAll
        [HttpGet("InitAll")]
        public async Task<IActionResult> InitAll()
        {
            try
            {
                var paraResult = await InitParaBirimleri();
                var sistemAyarlariResult = await InitSistemAyarlari();
                var kurResult = await InitKurDegerleri();
                
                return Ok("Tüm başlangıç verileri oluşturuldu.");
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Hata oluştu: {ex.Message}");
            }
        }
    }
} 