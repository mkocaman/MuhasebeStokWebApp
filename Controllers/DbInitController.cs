using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MuhasebeStokWebApp.Data;
using MuhasebeStokWebApp.Data.Entities;

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
                    // SistemAyarlariID otomatik olarak oluşturulacak (identity column)
                    AnaDovizKodu = "USD",
                    SirketAdi = "Muhasebe Stok Web App",
                    SirketAdresi = "İstanbul, Türkiye",
                    SirketTelefon = "+90 212 123 4567",
                    SirketEmail = "info@muhasebe-stok.com",
                    SirketVergiNo = "1234567890",
                    SirketVergiDairesi = "İstanbul Vergi Dairesi",
                    OtomatikDovizGuncelleme = true,
                    DovizGuncellemeSikligi = 24,
                    SonDovizGuncellemeTarihi = DateTime.Now,
                    AktifParaBirimleri = "USD,EUR,TRY,GBP,UZS",
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
                    .AnyAsync(d => d.Aktif && !d.SoftDelete);

                if (existingRates)
                {
                    return Ok("Kur değerleri zaten mevcut.");
                }

                // Para birimleri var mı kontrol et
                bool paraBirimleriExists = await _context.ParaBirimleri.AnyAsync();
                if (!paraBirimleriExists)
                {
                    // Para birimleri yoksa ekle
                    await InitParaBirimleri();
                }

                // Para birimlerini al
                var usd = await _context.ParaBirimleri.FirstOrDefaultAsync(p => p.Kod == "USD");
                var try_ = await _context.ParaBirimleri.FirstOrDefaultAsync(p => p.Kod == "TRY");
                var eur = await _context.ParaBirimleri.FirstOrDefaultAsync(p => p.Kod == "EUR");
                var gbp = await _context.ParaBirimleri.FirstOrDefaultAsync(p => p.Kod == "GBP");

                if (usd == null || try_ == null || eur == null || gbp == null)
                {
                    return BadRequest("Para birimleri bulunamadı. Önce para birimlerini ekleyin.");
                }

                // Yeni kur değerleri oluştur
                var kurDegerleri = new List<KurDegeri>();
                
                // Önce döviz ilişkilerini oluşturalım
                var usdTryIliski = new DovizIliski
                {
                    DovizIliskiID = Guid.NewGuid(),
                    KaynakParaBirimiID = usd.ParaBirimiID,
                    HedefParaBirimiID = try_.ParaBirimiID,
                    Aktif = true
                };
                
                var eurTryIliski = new DovizIliski
                {
                    DovizIliskiID = Guid.NewGuid(),
                    KaynakParaBirimiID = eur.ParaBirimiID,
                    HedefParaBirimiID = try_.ParaBirimiID,
                    Aktif = true
                };
                
                var gbpTryIliski = new DovizIliski
                {
                    DovizIliskiID = Guid.NewGuid(),
                    KaynakParaBirimiID = gbp.ParaBirimiID,
                    HedefParaBirimiID = try_.ParaBirimiID,
                    Aktif = true
                };
                
                _context.DovizIliskileri.Add(usdTryIliski);
                _context.DovizIliskileri.Add(eurTryIliski);
                _context.DovizIliskileri.Add(gbpTryIliski);
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
                    SoftDelete = false
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
                    SoftDelete = false
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
                    SoftDelete = false
                };
                
                kurDegerleri.Add(usdTryCross);
                kurDegerleri.Add(eurTryCross);
                kurDegerleri.Add(gbpTryCross);

                _context.KurDegerleri.AddRange(kurDegerleri);
                await _context.SaveChangesAsync();

                return Ok($"{kurDegerleri.Count} adet kur değeri başarıyla oluşturuldu.");
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
                var existingCurrencies = await _context.ParaBirimleri.AnyAsync();

                if (existingCurrencies)
                {
                    return Ok("Para birimleri zaten mevcut.");
                }

                // Yeni para birimleri oluştur
                var paraBirimleri = new List<ParaBirimi>
                {
                    new ParaBirimi
                    {
                        ParaBirimiID = Guid.NewGuid(),
                        Kod = "USD",
                        Ad = "Amerikan Doları",
                        Sembol = "$",
                        Aktif = true,
                        SoftDelete = false
                    },
                    new ParaBirimi
                    {
                        ParaBirimiID = Guid.NewGuid(),
                        Kod = "EUR",
                        Ad = "Euro",
                        Sembol = "€",
                        Aktif = true,
                        SoftDelete = false
                    },
                    new ParaBirimi
                    {
                        ParaBirimiID = Guid.NewGuid(),
                        Kod = "TRY",
                        Ad = "Türk Lirası",
                        Sembol = "₺",
                        Aktif = true,
                        SoftDelete = false
                    },
                    new ParaBirimi
                    {
                        ParaBirimiID = Guid.NewGuid(),
                        Kod = "GBP",
                        Ad = "İngiliz Sterlini",
                        Sembol = "£",
                        Aktif = true,
                        SoftDelete = false
                    },
                    new ParaBirimi
                    {
                        ParaBirimiID = Guid.NewGuid(),
                        Kod = "UZS",
                        Ad = "Özbek Somu",
                        Sembol = "UZS",
                        Aktif = true,
                        SoftDelete = false
                    }
                };

                _context.ParaBirimleri.AddRange(paraBirimleri);
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
                // Sırasıyla her fonksiyonu çağır
                var sistemAyarlariResult = await InitSistemAyarlari();
                var paraBirimleriResult = await InitParaBirimleri();
                var kurDegerleriResult = await InitKurDegerleri();

                return Ok("Tüm tablolar başarıyla başlatıldı.");
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Hata oluştu: {ex.Message}");
            }
        }
    }
} 