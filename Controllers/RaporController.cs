using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using MuhasebeStokWebApp.Data;
using MuhasebeStokWebApp.Data.Entities;
using MuhasebeStokWebApp.ViewModels.Rapor;
using System.Security.Claims;
using Microsoft.Extensions.Logging;
using MuhasebeStokWebApp.Services;
using Microsoft.AspNetCore.Identity;
using MuhasebeStokWebApp.Services.Interfaces;

namespace MuhasebeStokWebApp.Controllers
{
    [Authorize]
    public class RaporController : BaseController
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<RaporController> _logger;

        public RaporController(
            ApplicationDbContext context, 
            ILogger<RaporController> logger,
            IMenuService menuService,
            UserManager<ApplicationUser> userManager,
            RoleManager<IdentityRole> roleManager,
            ILogService logService)
            : base(menuService, userManager, roleManager, logService)
        {
            _context = context;
            _logger = logger;
        }

        // Ana rapor sayfası
        public IActionResult Index()
        {
            return View();
        }

        // Kasa Raporu
        public async Task<IActionResult> Kasa()
        {
            ViewBag.Kasalar = await _context.Kasalar
                .Where(k => !k.Silindi && k.Aktif)
                .Select(k => new SelectListItem
                {
                    Value = k.KasaID.ToString(),
                    Text = k.KasaAdi
                }).ToListAsync();

            var model = new RaporFiltreViewModel();
            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> KasaRaporu(RaporFiltreViewModel filtre)
        {
            var model = new KasaRaporViewModel
            {
                RaporAdi = "Kasa Hareket Raporu",
                RaporTarihi = DateTime.Now,
                KullaniciAdi = User.Identity?.Name ?? "Kullanıcı",
                Aciklama = $"{filtre.BaslangicTarihi:dd.MM.yyyy} - {filtre.BitisTarihi:dd.MM.yyyy} tarihleri arası kasa hareketleri"
            };

            var query = _context.KasaHareketleri
                .Include(k => k.Kasa)
                .Where(k => !k.Silindi)
                .Where(k => k.Tarih >= filtre.BaslangicTarihi && k.Tarih <= filtre.BitisTarihi.AddDays(1).AddSeconds(-1));

            if (filtre.KasaID.HasValue)
            {
                query = query.Where(k => k.KasaID == filtre.KasaID.Value);
            }

            if (!string.IsNullOrEmpty(filtre.HareketTuru))
            {
                query = query.Where(k => k.HareketTuru == filtre.HareketTuru);
            }

            var hareketler = await query.OrderBy(k => k.Tarih).ToListAsync();

            model.KasaHareketleri = hareketler.Select(h => new KasaHareketRaporViewModel
            {
                KasaAdi = h.Kasa?.KasaAdi ?? "Bilinmeyen Kasa",
                Tarih = h.Tarih,
                HareketTuru = h.HareketTuru,
                Tutar = h.Tutar,
                ReferansNo = h.ReferansNo,
                ReferansTuru = h.ReferansTuru,
                Aciklama = h.Aciklama
            }).ToList();

            model.ToplamGiris = hareketler.Where(h => h.HareketTuru == "Giriş" || h.HareketTuru == "Açılış Bakiyesi" || h.HareketTuru == "Bakiye Artışı").Sum(h => h.Tutar);
            model.ToplamCikis = hareketler.Where(h => h.HareketTuru == "Çıkış" || h.HareketTuru == "Bakiye Azalışı").Sum(h => h.Tutar);

            return View(model);
        }

        // Banka Raporu
        public async Task<IActionResult> Banka()
        {
            ViewBag.Bankalar = await _context.Bankalar
                .Where(b => !b.Silindi && b.Aktif)
                .Select(b => new SelectListItem
                {
                    Value = b.BankaID.ToString(),
                    Text = $"{b.BankaAdi} - {b.HesapNo} ({b.ParaBirimi})"
                }).ToListAsync();

            var model = new RaporFiltreViewModel();
            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> BankaRaporu(RaporFiltreViewModel filtre)
        {
            var model = new BankaRaporViewModel
            {
                RaporAdi = "Banka Hareket Raporu",
                RaporTarihi = DateTime.Now,
                KullaniciAdi = User.Identity?.Name ?? "Kullanıcı",
                Aciklama = $"{filtre.BaslangicTarihi:dd.MM.yyyy} - {filtre.BitisTarihi:dd.MM.yyyy} tarihleri arası banka hareketleri"
            };

            var query = _context.BankaHareketleri
                .Include(b => b.Banka)
                .Where(b => !b.Silindi)
                .Where(b => b.Tarih >= filtre.BaslangicTarihi && b.Tarih <= filtre.BitisTarihi.AddDays(1).AddSeconds(-1));

            if (filtre.BankaID.HasValue)
            {
                query = query.Where(b => b.BankaID == filtre.BankaID.Value);
            }

            if (!string.IsNullOrEmpty(filtre.HareketTuru))
            {
                query = query.Where(b => b.HareketTuru == filtre.HareketTuru);
            }

            var hareketler = await query.OrderBy(b => b.Tarih).ToListAsync();

            model.BankaHareketleri = hareketler.Select(h => new BankaHareketRaporViewModel
            {
                BankaAdi = h.Banka?.BankaAdi ?? "Bilinmeyen Banka",
                Tarih = h.Tarih,
                HareketTuru = h.HareketTuru,
                Tutar = h.Tutar,
                ParaBirimi = h.Banka?.ParaBirimi ?? "TL",
                ReferansNo = h.ReferansNo,
                ReferansTuru = h.ReferansTuru,
                DekontNo = h.DekontNo,
                Aciklama = h.Aciklama
            }).ToList();

            model.ToplamGiris = hareketler.Where(h => h.HareketTuru == "Giriş" || h.HareketTuru == "Açılış Bakiyesi" || h.HareketTuru == "Bakiye Artışı").Sum(h => h.Tutar);
            model.ToplamCikis = hareketler.Where(h => h.HareketTuru == "Çıkış" || h.HareketTuru == "Bakiye Azalışı").Sum(h => h.Tutar);

            // Para birimine göre toplamları hesapla
            var paraBirimiGruplari = hareketler
                .GroupBy(h => h.Banka?.ParaBirimi ?? "TL")
                .Select(g => new
                {
                    ParaBirimi = g.Key,
                    ToplamGiris = g.Where(h => h.HareketTuru == "Giriş" || h.HareketTuru == "Açılış Bakiyesi" || h.HareketTuru == "Bakiye Artışı").Sum(h => h.Tutar),
                    ToplamCikis = g.Where(h => h.HareketTuru == "Çıkış" || h.HareketTuru == "Bakiye Azalışı").Sum(h => h.Tutar)
                });

            foreach (var grup in paraBirimiGruplari)
            {
                model.ParaBirimiToplamlari[grup.ParaBirimi] = grup.ToplamGiris - grup.ToplamCikis;
            }

            return View(model);
        }

        // Cari Raporu
        public async Task<IActionResult> Cari()
        {
            try
            {
                ViewBag.Cariler = await _context.Cariler
                    .Where(c => !c.Silindi && c.AktifMi)
                    .Select(c => new SelectListItem
                    {
                        Value = c.CariID.ToString(),
                        Text = c.Ad
                    }).ToListAsync();
            }
            catch (Exception ex)
            {
                // Hata durumunda boş liste ata
                ViewBag.Cariler = new List<SelectListItem>();
                // Hatayı loglayabilirsiniz
                _logger.LogError(ex, "Cariler yüklenirken hata oluştu: {Message}", ex.Message);
                ViewBag.Cariler = await _context.Cariler
                    .Where(c => !c.Silindi && c.AktifMi)
                    .Select(c => new SelectListItem
                    {
                        Value = c.CariID.ToString(),
                        Text = c.Ad
                    }).ToListAsync();
            }

            var model = new RaporFiltreViewModel();
            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> CariRaporu(RaporFiltreViewModel filtre)
        {
            var model = new CariRaporViewModel
            {
                RaporAdi = "Cari Hareket Raporu",
                RaporTarihi = DateTime.Now,
                KullaniciAdi = User.Identity?.Name ?? "Kullanıcı",
                Aciklama = $"{filtre.BaslangicTarihi:dd.MM.yyyy} - {filtre.BitisTarihi:dd.MM.yyyy} tarihleri arası cari hareketleri"
            };

            var query = _context.CariHareketler
                .Include(c => c.Cari)
                .Where(c => !c.Silindi)
                .Where(c => c.Tarih >= filtre.BaslangicTarihi && c.Tarih <= filtre.BitisTarihi.AddDays(1).AddSeconds(-1));

            if (filtre.CariID.HasValue)
            {
                var cariId = filtre.CariID.Value;
                query = query.Where(c => c.CariId.ToString().Contains(cariId.ToString()));
            }

            if (!string.IsNullOrEmpty(filtre.HareketTuru))
            {
                query = query.Where(c => c.HareketTuru == filtre.HareketTuru);
            }

            var hareketler = await query.OrderBy(c => c.Tarih).ToListAsync();

            model.CariHareketleri = hareketler.Select(h => new CariHareketRaporViewModel
            {
                CariAdi = h.Cari?.Ad ?? "Bilinmeyen Cari",
                Tarih = h.Tarih,
                HareketTuru = h.HareketTuru,
                Tutar = h.Tutar,
                ReferansNo = h.ReferansNo,
                ReferansTuru = h.ReferansTuru,
                Aciklama = h.Aciklama
            }).ToList();

            model.ToplamBorc = hareketler.Where(h => h.HareketTuru == "Borç").Sum(h => h.Tutar);
            model.ToplamAlacak = hareketler.Where(h => h.HareketTuru == "Alacak").Sum(h => h.Tutar);

            return View(model);
        }

        // Stok Raporu
        public async Task<IActionResult> Stok()
        {
            try
            {
                ViewBag.Urunler = await _context.Urunler
                    .Where(u => !u.Silindi && u.Aktif)
                    .Select(u => new SelectListItem
                    {
                        Value = u.UrunID.ToString(),
                        Text = u.UrunAdi
                    }).ToListAsync();
            }
            catch (Exception ex)
            {
                // Hata durumunda boş liste ata
                ViewBag.Urunler = new List<SelectListItem>();
                // Hatayı loglayabilirsiniz
                _logger.LogError(ex, "Ürünler yüklenirken hata oluştu: {Message}", ex.Message);
                TempData["ErrorMessage"] = "Ürün listesi raporu yüklenirken bir hata oluştu: " + ex.Message;
            }

            var model = new RaporFiltreViewModel();
            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> StokRaporu(RaporFiltreViewModel filtre)
        {
            var model = new StokRaporViewModel
            {
                RaporAdi = "Stok Hareket Raporu",
                RaporTarihi = DateTime.Now,
                KullaniciAdi = User.Identity?.Name ?? "Kullanıcı",
                Aciklama = $"{filtre.BaslangicTarihi:dd.MM.yyyy} - {filtre.BitisTarihi:dd.MM.yyyy} tarihleri arası stok hareketleri"
            };

            var query = _context.StokHareketleri
                .Include(s => s.Urun)
                .Where(s => !s.Silindi)
                .Where(s => s.Tarih >= filtre.BaslangicTarihi && s.Tarih <= filtre.BitisTarihi.AddDays(1).AddSeconds(-1));

            if (filtre.UrunID.HasValue)
            {
                query = query.Where(s => s.UrunID == filtre.UrunID.Value);
            }

            if (!string.IsNullOrEmpty(filtre.HareketTuru))
            {
                query = query.Where(s => s.HareketTuru == filtre.HareketTuru);
            }

            var hareketler = await query.OrderBy(s => s.Tarih).ToListAsync();

            model.StokHareketleri = hareketler.Select(h => new StokHareketRaporViewModel
            {
                UrunAdi = h.Urun?.UrunAdi ?? "Bilinmeyen Ürün",
                Tarih = h.Tarih,
                HareketTuru = h.HareketTuru,
                Miktar = h.Miktar,
                Birim = h.Birim ?? "Adet",
                BirimFiyat = h.BirimFiyat ?? 0,
                ToplamTutar = h.Miktar * (h.BirimFiyat ?? 0),
                ReferansNo = h.ReferansNo,
                ReferansTuru = h.ReferansTuru,
                Aciklama = h.Aciklama
            }).ToList();

            // Manuel toplama yaparak Miktar değerlerini toplayalım
            decimal toplamGiris = 0;
            decimal toplamCikis = 0;

            foreach (var hareket in hareketler)
            {
                if (hareket.HareketTuru == "Giriş")
                {
                    toplamGiris += hareket.Miktar;
                }
                else if (hareket.HareketTuru == "Çıkış")
                {
                    toplamCikis += hareket.Miktar;
                }
            }

            model.ToplamGiris = toplamGiris;
            model.ToplamCikis = toplamCikis;

            return View(model);
        }

        // Satış Raporu
        public async Task<IActionResult> Satis()
        {
            try
            {
                ViewBag.Cariler = await _context.Cariler
                    .Where(c => !c.Silindi && c.AktifMi)
                    .Select(c => new SelectListItem
                    {
                        Value = c.CariID.ToString(),
                        Text = c.Ad
                    }).ToListAsync();
            }
            catch (Exception ex)
            {
                // Hata durumunda boş liste ata
                ViewBag.Cariler = new List<SelectListItem>();
                // Hatayı loglayabilirsiniz
                _logger.LogError(ex, "Cariler yüklenirken hata oluştu: {Message}", ex.Message);
                TempData["ErrorMessage"] = "Cariler yüklenirken bir hata oluştu: " + ex.Message;
            }

            try
            {
                ViewBag.Urunler = await _context.Urunler
                    .Where(u => !u.Silindi && u.Aktif)
                    .Select(u => new SelectListItem
                    {
                        Value = u.UrunID.ToString(),
                        Text = u.UrunAdi
                    }).ToListAsync();
            }
            catch (Exception ex)
            {
                // Hata durumunda boş liste ata
                ViewBag.Urunler = new List<SelectListItem>();
                // Hatayı loglayabilirsiniz
                _logger.LogError(ex, "Ürünler yüklenirken hata oluştu: {Message}", ex.Message);
                TempData["ErrorMessage"] = "Ürün listesi raporu yüklenirken bir hata oluştu: " + ex.Message;
            }

            var model = new RaporFiltreViewModel();
            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> SatisRaporu(RaporFiltreViewModel filtre)
        {
            var model = new SatisRaporViewModel
            {
                RaporAdi = "Satış Raporu",
                RaporTarihi = DateTime.Now,
                KullaniciAdi = User.Identity?.Name ?? "Kullanıcı",
                Aciklama = $"{filtre.BaslangicTarihi:dd.MM.yyyy} - {filtre.BitisTarihi:dd.MM.yyyy} tarihleri arası satış raporu"
            };

            var query = _context.Faturalar
                .Include(f => f.Cari)
                .Include(f => f.FaturaDetaylari)
                .ThenInclude(fd => fd.Urun)
                .Include(f => f.FaturaTuru)
                .Where(f => !f.Silindi)
                .Where(f => f.FaturaTarihi >= filtre.BaslangicTarihi && f.FaturaTarihi <= filtre.BitisTarihi.AddDays(1).AddSeconds(-1));

            if (filtre.CariID.HasValue)
            {
                var cariId = filtre.CariID.Value;
                query = query.Where(f => f.CariID.HasValue && f.CariID.Value.ToString().Contains(cariId.ToString()));
            }

            var faturalar = await query.OrderBy(f => f.FaturaTarihi).ToListAsync();
            
            // Sadece satış faturalarını filtrele
            faturalar = faturalar.Where(f => f.FaturaTuru != null && 
                                       (f.FaturaTuru.FaturaTuruAdi?.ToLower() == "satış" || 
                                        f.FaturaTuru.FaturaTuruAdi?.ToLower() == "satis"))
                               .ToList();

            var detaylar = new List<SatisDetayRaporViewModel>();

            foreach (var fatura in faturalar)
            {
                if (fatura.FaturaDetaylari == null) continue;
                
                foreach (var detay in fatura.FaturaDetaylari.Where(fd => !fd.Silindi))
                {
                    if (filtre.UrunID.HasValue && detay.UrunID != filtre.UrunID.Value)
                        continue;

                    // KDV tutarı ve indirim tutarı hesaplama
                    decimal kdvTutari = (detay.SatirToplam ?? 0) * detay.KdvOrani / 100;
                    decimal indirimTutari = (detay.BirimFiyat * detay.Miktar * detay.IndirimOrani) / 100;
                    decimal toplamTutar = (detay.SatirToplam ?? 0) + kdvTutari;

                    detaylar.Add(new SatisDetayRaporViewModel
                    {
                        Tarih = fatura.FaturaTarihi.HasValue ? fatura.FaturaTarihi.Value : DateTime.Now,
                        FaturaNo = fatura.FaturaNumarasi,
                        CariAdi = fatura.Cari?.Ad ?? "Bilinmeyen Cari",
                        UrunAdi = detay.Urun?.UrunAdi ?? "Bilinmeyen Ürün",
                        Miktar = detay.Miktar,
                        Birim = detay.Birim ?? "Adet",
                        BirimFiyat = detay.BirimFiyat,
                        KdvOrani = detay.KdvOrani,
                        KdvTutari = kdvTutari,
                        IndirimOrani = detay.IndirimOrani,
                        IndirimTutari = indirimTutari,
                        ToplamTutar = toplamTutar
                    });
                }
            }

            model.SatisDetaylari = detaylar;
            model.ToplamSatisTutari = detaylar.Any() ? detaylar.Sum(d => d.BirimFiyat * d.Miktar) : 0m;
            model.ToplamKdvTutari = detaylar.Any() ? detaylar.Sum(d => d.KdvTutari) : 0m;
            model.ToplamIndirimTutari = detaylar.Any() ? detaylar.Sum(d => d.IndirimTutari) : 0m;

            return View(model);
        }

        // Özet Rapor
        public IActionResult Ozet()
        {
            var model = new RaporFiltreViewModel();
            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> OzetRaporu(RaporFiltreViewModel filtre)
        {
            var model = new OzetRaporViewModel
            {
                RaporAdi = "Özet Rapor",
                RaporTarihi = DateTime.Now,
                KullaniciAdi = User.Identity?.Name ?? "Kullanıcı",
                Aciklama = $"{filtre.BaslangicTarihi:dd.MM.yyyy} - {filtre.BitisTarihi:dd.MM.yyyy} tarihleri arası özet rapor"
            };

            // Satış ve alış toplamları
            var faturalar = await _context.Faturalar
                .Include(f => f.FaturaTuru)
                .Where(f => !f.Silindi)
                .Where(f => f.FaturaTarihi >= filtre.BaslangicTarihi && f.FaturaTarihi <= filtre.BitisTarihi.AddDays(1).AddSeconds(-1))
                .ToListAsync();

            // Satış faturalarını filtrele 
            var satisFaturalari = faturalar.Where(f => f.FaturaTuru != null && 
                                               (f.FaturaTuru.FaturaTuruAdi?.ToLower() == "satış" || 
                                                f.FaturaTuru.FaturaTuruAdi?.ToLower() == "satis"))
                                        .ToList();
            
            // Alış faturalarını filtrele
            var alisFaturalari = faturalar.Where(f => f.FaturaTuru != null && 
                                           (f.FaturaTuru.FaturaTuruAdi?.ToLower() == "alış" || 
                                            f.FaturaTuru.FaturaTuruAdi?.ToLower() == "alis"))
                                     .ToList();

            model.ToplamSatis = satisFaturalari.Sum(f => f.GenelToplam ?? 0);
            model.ToplamAlis = alisFaturalari.Sum(f => f.GenelToplam ?? 0);

            // Tahsilat ve ödeme toplamları
            var kasaHareketleri = await _context.KasaHareketleri
                .Where(k => !k.Silindi)
                .Where(k => k.Tarih >= filtre.BaslangicTarihi && k.Tarih <= filtre.BitisTarihi.AddDays(1).AddSeconds(-1))
                .ToListAsync();

            model.ToplamTahsilat = kasaHareketleri.Where(k => k.HareketTuru == "Giriş").Sum(k => k.Tutar);
            model.ToplamOdeme = kasaHareketleri.Where(k => k.HareketTuru == "Çıkış").Sum(k => k.Tutar);

            // Cari alacak ve borç toplamları
            var cariHareketleri = await _context.CariHareketler
                .Where(c => !c.Silindi)
                .Where(c => c.Tarih >= filtre.BaslangicTarihi && c.Tarih <= filtre.BitisTarihi.AddDays(1).AddSeconds(-1))
                .ToListAsync();

            model.ToplamCariAlacak = cariHareketleri.Where(c => c.HareketTuru == "Alacak").Sum(c => c.Tutar);
            model.ToplamCariBorclar = cariHareketleri.Where(c => c.HareketTuru == "Borç").Sum(c => c.Tutar);

            // Aylık özet
            var baslangic = new DateTime(filtre.BaslangicTarihi.Year, filtre.BaslangicTarihi.Month, 1);
            var bitis = filtre.BitisTarihi;
            var aylar = new List<DateTime>();

            for (var tarih = baslangic; tarih <= bitis; tarih = tarih.AddMonths(1))
            {
                aylar.Add(tarih);
            }

            foreach (var ay in aylar)
            {
                var ayBaslangic = new DateTime(ay.Year, ay.Month, 1);
                var aySonu = ayBaslangic.AddMonths(1).AddDays(-1);

                var aylikFaturalar = faturalar.Where(f => f.FaturaTarihi >= ayBaslangic && f.FaturaTarihi <= aySonu).ToList();
                var aylikKasaHareketleri = kasaHareketleri.Where(k => k.Tarih >= ayBaslangic && k.Tarih <= aySonu).ToList();

                var aylikSatisFaturalari = aylikFaturalar.Where(f => f.FaturaTuru != null && 
                                                           (f.FaturaTuru.FaturaTuruAdi?.ToLower() == "satış" || 
                                                            f.FaturaTuru.FaturaTuruAdi?.ToLower() == "satis"))
                                                    .ToList();
                
                var aylikAlisFaturalari = aylikFaturalar.Where(f => f.FaturaTuru != null && 
                                                         (f.FaturaTuru.FaturaTuruAdi?.ToLower() == "alış" || 
                                                          f.FaturaTuru.FaturaTuruAdi?.ToLower() == "alis"))
                                                  .ToList();

                var aylikOzet = new AylikOzetViewModel
                {
                    Ay = $"{ay:MMMM yyyy}",
                    Satis = aylikSatisFaturalari.Sum(f => f.GenelToplam ?? 0),
                    Alis = aylikAlisFaturalari.Sum(f => f.GenelToplam ?? 0),
                    Tahsilat = aylikKasaHareketleri.Where(k => k.HareketTuru == "Giriş").Sum(k => k.Tutar),
                    Odeme = aylikKasaHareketleri.Where(k => k.HareketTuru == "Çıkış").Sum(k => k.Tutar)
                };

                model.AylikOzetler.Add(aylikOzet);
            }

            return View(model);
        }
    }
} 