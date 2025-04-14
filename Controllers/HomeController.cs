using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using MuhasebeStokWebApp.Models;
using MuhasebeStokWebApp.Data.Repositories;
using MuhasebeStokWebApp.Data.Entities;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Newtonsoft.Json;
using Microsoft.AspNetCore.Authorization;
using MuhasebeStokWebApp.Services;
using MuhasebeStokWebApp.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using MuhasebeStokWebApp.ViewModels.Menu;
using MuhasebeStokWebApp.Services.Interfaces;
using MuhasebeStokWebApp.Services.Report;
using MuhasebeStokWebApp.Services.Notification;
using System.Globalization;

namespace MuhasebeStokWebApp.Controllers
{
    [Authorize]  // Bu controller'a erişim için kimlik doğrulama gerektirir
    public class HomeController : BaseController
    {
        private readonly ILogger<HomeController> _logger;
        private readonly IUnitOfWork _unitOfWork;
        protected new readonly UserManager<ApplicationUser> _userManager;
        protected new readonly RoleManager<IdentityRole> _roleManager;
        private readonly IDovizKuruService _dovizKuruService;
        private readonly ApplicationDbContext _context;
        private readonly INotificationService _notificationService;

        // Constructor: Dependency Injection ile gerekli servisleri alır
        public HomeController(
            ILogger<HomeController> logger, 
            IUnitOfWork unitOfWork,
            UserManager<ApplicationUser> userManager,
            RoleManager<IdentityRole> roleManager,
            IMenuService menuService,
            IDovizKuruService dovizKuruService,
            ApplicationDbContext context,
            INotificationService notificationService,
            ILogService logService)
            : base(menuService, userManager, roleManager, logService)
        {
            _logger = logger;
            _unitOfWork = unitOfWork;
            _userManager = userManager;
            _roleManager = roleManager;
            _dovizKuruService = dovizKuruService;
            _context = context;
            _notificationService = notificationService;
        }

        // Ana Dashboard sayfasını hazırlar
        public async Task<IActionResult> Index(string auth = null)
        {
            // Auth parametresi varsa login başarılı olmuş demektir, loglama yapabiliriz
            if (!string.IsNullOrEmpty(auth))
            {
                _logger.LogInformation($"Home/Index'e auth parametresi ile erişildi: {auth}");
            }
            
            // Session kontrolü
            if (HttpContext.Session.Keys.Contains("UserId"))
            {
                var userId = HttpContext.Session.GetString("UserId");
                var userName = HttpContext.Session.GetString("UserName");
                _logger.LogInformation($"Aktif session bulundu: UserID={userId}, UserName={userName}");
                
                // ViewBag ile kullanıcı bilgisini view'a gönderiyoruz
                ViewBag.UserName = userName;
                ViewBag.IsLoggedIn = true;
                
                // Admin mi kontrol ediyoruz
                if (HttpContext.Session.Keys.Contains("IsAdmin"))
                {
                    ViewBag.IsAdmin = true;
                    _logger.LogInformation("Admin kullanıcısı tespit edildi");
                }
            }
            else
            {
                _logger.LogWarning("Session bulunamadı, kullanıcı giriş yapmamış olabilir");
                ViewBag.IsLoggedIn = false;
            }
            
            // User.Identity kontrolü
            if (User.Identity is not null && User.Identity.IsAuthenticated)
            {
                _logger.LogInformation($"Kimlik doğrulanmış kullanıcı: {User.Identity.Name}");
            }
            else
            {
                _logger.LogWarning("Kimlik doğrulanmamış kullanıcı");
            }
            
            try
            {
                // Dashboard için varsayılan değerleri önce ayarla
                SetDefaultDashboardValues();
                
                // Dashboard için gerekli verileri yüklemeye çalışıyoruz
                await LoadDashboardDataAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Dashboard verileri yüklenirken hata oluştu");
                ViewBag.ErrorMessage = "Dashboard verileri yüklenirken bir hata oluştu: " + ex.Message;
                
                // Varsayılan değerler zaten ayarlandı
            }
            
            return View();
        }

        // Gizlilik sayfası - Kimlik doğrulama gerektirmez
        [AllowAnonymous]
        public IActionResult Privacy()
        {
            return View();
        }

        // Hata sayfası - Kimlik doğrulama gerektirmez
        [AllowAnonymous]
        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }

        // HTTP durum kodu sayfaları - Kimlik doğrulama gerektirmez
        [AllowAnonymous]
        public new IActionResult StatusCode(int code)
        {
            try 
            {
                // Tüm durum kodlarında menü gösterilmesi için ViewBag ayarları
                if (User?.Identity?.IsAuthenticated == true)
                {
                    var userId = User?.FindFirstValue(ClaimTypes.NameIdentifier);
                    if (!string.IsNullOrEmpty(userId))
                    {
                        // Burada varsayılan menüleri kullanıyoruz çünkü BaseController bu sayfada aktif değil
                        ViewBag.MenuItems = GetDefaultMenuItems();
                        ViewBag.IsAuthenticated = true;
                        ViewBag.RequiresLogin = false;
                    }
                }
                
                switch (code)
                {
                    case 404:
                        return View("NotFound");
                    case 403:
                        return View("Forbidden");
                    default:
                        return View("Error", new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "StatusCode sayfası gösterilirken hata oluştu.");
                return View("Error", new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
            }
        }

        // Dashboard için varsayılan değerleri atar
        private void SetDefaultDashboardValues()
        {
            ViewBag.ToplamCariSayisi = 0;
            ViewBag.ToplamUrunSayisi = 0;
            ViewBag.ToplamFaturaSayisi = 0;
            ViewBag.ToplamCiro = 0m;
            ViewBag.SonKurlar = new List<MuhasebeStokWebApp.Models.DovizKuru>();
            ViewBag.AylikSatisVerileri = JsonConvert.SerializeObject(new decimal[12]);
            ViewBag.AylikGiderVerileri = JsonConvert.SerializeObject(new decimal[12]);
            ViewBag.CariBakiyeTipleri = JsonConvert.SerializeObject(new[] { "Borçlu", "Alacaklı", "Sıfır Bakiye" });
            ViewBag.CariBakiyeDagilimi = JsonConvert.SerializeObject(new[] { 0, 0, 0 });
            ViewBag.StokKategorileri = JsonConvert.SerializeObject(new string[0]);
            ViewBag.StokMiktarlari = JsonConvert.SerializeObject(new decimal[0]);
            
            // Eksik ViewBag değerleri için varsayılanlar
            ViewBag.TotalRevenue = 0m;
            ViewBag.TotalExpense = 0m;
            ViewBag.TopProducts = new List<dynamic>();
            ViewBag.MonthlyData = new List<dynamic>();
            ViewBag.MonthlyExpenses = new List<dynamic>();
            ViewBag.Days = JsonConvert.SerializeObject(new string[0]);
            ViewBag.Revenues = JsonConvert.SerializeObject(new decimal[0]);
            ViewBag.Expenses = JsonConvert.SerializeObject(new decimal[0]);
            ViewBag.Categories = JsonConvert.SerializeObject(new string[0]);
            ViewBag.CategoryRevenues = JsonConvert.SerializeObject(new decimal[0]);
            ViewBag.Last6MonthLabels = JsonConvert.SerializeObject(new string[0]);
            ViewBag.TrendRevenues = JsonConvert.SerializeObject(new decimal[0]);
            ViewBag.TrendExpenses = JsonConvert.SerializeObject(new decimal[0]);
            
            // Yeni grafik verileri için varsayılan değerler
            ViewBag.KasaBankaBakiyeleri = JsonConvert.SerializeObject(new decimal[0]);
            ViewBag.KasaBankaLabels = JsonConvert.SerializeObject(new string[0]);
            ViewBag.AcikAlacakTutari = 0m;
            ViewBag.AcikBorcTutari = 0m;
            ViewBag.EnCokSatilanUrunler = JsonConvert.SerializeObject(new string[0]);
            ViewBag.EnCokSatilanUrunMiktarlari = JsonConvert.SerializeObject(new decimal[0]);
            ViewBag.AlacakYaslari = JsonConvert.SerializeObject(new string[] { "0-30 Gün", "31-60 Gün", "61-90 Gün", "91+ Gün" });
            ViewBag.AlacakYaslariMiktarlari = JsonConvert.SerializeObject(new decimal[4]);
            ViewBag.NakitAkisProj = JsonConvert.SerializeObject(new decimal[3]);
            ViewBag.NakitAkisProjAylar = JsonConvert.SerializeObject(new string[3]);
        }

        // Dashboard verilerini yükler
        private async Task LoadDashboardDataAsync()
        {
            // Toplam cari sayısı hesaplama
            var cariler = await _unitOfWork.Repository<Data.Entities.Cari>().GetAllAsync();
            ViewBag.ToplamCariSayisi = cariler?.Count() ?? 0;
            
            // Toplam ürün sayısı hesaplama
            var urunler = await _unitOfWork.Repository<Data.Entities.Urun>().GetAllAsync();
            ViewBag.ToplamUrunSayisi = urunler?.Count() ?? 0;
            
            // Toplam fatura sayısı hesaplama
            var faturalar = await _unitOfWork.Repository<Data.Entities.Fatura>().GetAllAsync();
            ViewBag.ToplamFaturaSayisi = faturalar?.Count() ?? 0;
            
            // USD kur değerini al
            decimal tryToUsdKur = 1;
            decimal uzsToUsdKur = 1;
            try 
            {
                // TRY -> USD kur
                tryToUsdKur = await _dovizKuruService.GetGuncelKurAsync("TRY", "USD");
                // UZS -> USD kur
                uzsToUsdKur = await _dovizKuruService.GetGuncelKurAsync("UZS", "USD");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Döviz kuru alınırken hata oluştu. Varsayılan değerler kullanılacak.");
            }
            
            // Toplam ciro hesaplama (sadece satış faturalarının toplamı - USD cinsinden)
            var satisFaturalari = faturalar?.Where(f => f.FaturaTuru?.FaturaTuruAdi == "Satış").ToList() ?? new List<Data.Entities.Fatura>();
            decimal toplamCiro = 0;
            
            foreach (var fatura in satisFaturalari)
            {
                decimal tutarUSD = 0;
                
                // Para birimi türüne göre USD'ye çevir
                if (fatura.ParaBirimi == "TRY" && fatura.GenelToplam.HasValue)
                {
                    tutarUSD = fatura.GenelToplam.Value * tryToUsdKur;
                }
                else if (fatura.ParaBirimi == "UZS" && fatura.GenelToplam.HasValue)
                {
                    tutarUSD = fatura.GenelToplam.Value * uzsToUsdKur;
                }
                else if (fatura.ParaBirimi == "USD" && fatura.GenelToplam.HasValue)
                {
                    tutarUSD = fatura.GenelToplam.Value;
                }
                else if (fatura.GenelToplam.HasValue)
                {
                    // Diğer para birimleri için doğrudan çevir
                    try 
                    {
                        tutarUSD = await _dovizKuruService.CevirmeTutarByKodAsync(fatura.GenelToplam.Value, fatura.ParaBirimi ?? "TRY", "USD");
                    }
                    catch
                    {
                        tutarUSD = fatura.GenelToplam.Value;
                    }
                }
                
                toplamCiro += tutarUSD;
            }
            
            ViewBag.ToplamCiro = toplamCiro;
            ViewBag.ParaBirimi = "USD";
            
            // Son döviz kurlarını getir (son 5 güne ait)
            var sonKurlar = await _dovizKuruService.GetLatestRatesAsync(5);
            ViewBag.SonKurlar = sonKurlar;
            
            // Aylık satış ve gider verileri için diziler oluştur
            var aylikSatisVerileri = new decimal[12];
            var aylikGiderVerileri = new decimal[12];
            
            var buYil = DateTime.Now.Year;
            
            // Satış faturaları - Bu yıl içinde kesilmiş olanlar
            var yillikSatisFaturalari = faturalar?
                .Where(f => f.FaturaTuru?.FaturaTuruAdi == "Satış" && f.FaturaTarihi?.Year == buYil)
                .ToList() ?? new List<Data.Entities.Fatura>();
            
            // Alış faturaları - Bu yıl içinde kesilmiş olanlar
            var yillikAlisFaturalari = faturalar?
                .Where(f => f.FaturaTuru?.FaturaTuruAdi == "Alış" && f.FaturaTarihi?.Year == buYil)
                .ToList() ?? new List<Data.Entities.Fatura>();
            
            // Her ay için satış ve gider toplamlarını hesapla
            for (int ay = 1; ay <= 12; ay++)
            {
                // O aydaki faturalar
                var aydakiSatislar = yillikSatisFaturalari.Where(f => f.FaturaTarihi?.Month == ay).ToList();
                var aydakiAlislar = yillikAlisFaturalari.Where(f => f.FaturaTarihi?.Month == ay).ToList();
                
                decimal aylikSatisToplam = 0;
                decimal aylikAlisToplam = 0;
                
                // Satışları USD'ye çevir
                foreach (var fatura in aydakiSatislar)
                {
                    decimal tutarUSD = 0;
                    
                    if (fatura.ParaBirimi == "TRY" && fatura.GenelToplam.HasValue)
                    {
                        tutarUSD = fatura.GenelToplam.Value * tryToUsdKur;
                    }
                    else if (fatura.ParaBirimi == "UZS" && fatura.GenelToplam.HasValue)
                    {
                        tutarUSD = fatura.GenelToplam.Value * uzsToUsdKur;
                    }
                    else if (fatura.ParaBirimi == "USD" && fatura.GenelToplam.HasValue)
                    {
                        tutarUSD = fatura.GenelToplam.Value;
                    }
                    else if (fatura.GenelToplam.HasValue)
                    {
                        try 
                        {
                            tutarUSD = await _dovizKuruService.CevirmeTutarByKodAsync(fatura.GenelToplam.Value, fatura.ParaBirimi ?? "TRY", "USD");
                        }
                        catch
                        {
                            tutarUSD = fatura.GenelToplam.Value;
                        }
                    }
                    
                    aylikSatisToplam += tutarUSD;
                }
                
                // Alışları USD'ye çevir
                foreach (var fatura in aydakiAlislar)
                {
                    decimal tutarUSD = 0;
                    
                    if (fatura.ParaBirimi == "TRY" && fatura.GenelToplam.HasValue)
                    {
                        tutarUSD = fatura.GenelToplam.Value * tryToUsdKur;
                    }
                    else if (fatura.ParaBirimi == "UZS" && fatura.GenelToplam.HasValue)
                    {
                        tutarUSD = fatura.GenelToplam.Value * uzsToUsdKur;
                    }
                    else if (fatura.ParaBirimi == "USD" && fatura.GenelToplam.HasValue)
                    {
                        tutarUSD = fatura.GenelToplam.Value;
                    }
                    else if (fatura.GenelToplam.HasValue)
                    {
                        try 
                        {
                            tutarUSD = await _dovizKuruService.CevirmeTutarByKodAsync(fatura.GenelToplam.Value, fatura.ParaBirimi ?? "TRY", "USD");
                        }
                        catch
                        {
                            tutarUSD = fatura.GenelToplam.Value;
                        }
                    }
                    
                    aylikAlisToplam += tutarUSD;
                }
                
                aylikSatisVerileri[ay - 1] = aylikSatisToplam;
                aylikGiderVerileri[ay - 1] = aylikAlisToplam;
            }
            
            // Grafik için JSON formatına dönüştür
            ViewBag.AylikSatisVerileri = JsonConvert.SerializeObject(aylikSatisVerileri);
            ViewBag.AylikGiderVerileri = JsonConvert.SerializeObject(aylikGiderVerileri);
            
            // Cari bakiye dağılımı hesaplama
            // Her cari için bakiyeyi hesapla
            var cariHareketler = await _unitOfWork.Repository<CariHareket>().GetAllAsync();
            var cariBakiyeleri = new Dictionary<Guid, decimal>();
            
            if (cariler != null)
            {
                foreach (var cari in cariler)
                {
                    if (cari.Silindi) continue;
                    
                    // Cari id'ye göre hareketleri filtrele
                    var caridekiHareketler = cariHareketler?.Where(h => h.CariID == cari.CariID && !h.Silindi).ToList();
                    
                    // Alacak ve borç toplamlarını hesapla
                    decimal toplamAlacak = caridekiHareketler?.Where(h => h.HareketTuru == "Alacak").Sum(h => h.Tutar) ?? 0;
                    decimal toplamBorc = caridekiHareketler?.Where(h => h.HareketTuru == "Borç").Sum(h => h.Tutar) ?? 0;
                    decimal bakiye = toplamAlacak - toplamBorc;
                    
                    // Cari bakiyeyi kaydet
                    cariBakiyeleri[cari.CariID] = bakiye;
                }
            }
            
            // Bakiye durumlarına göre sayıları hesapla
            int borcluCariSayisi = cariBakiyeleri.Count(p => p.Value < 0);
            int alacakliCariSayisi = cariBakiyeleri.Count(p => p.Value > 0);
            int sifirBakiyeliCariSayisi = cariBakiyeleri.Count(p => p.Value == 0);
            
            var cariBakiyeTipleri = new[] { "Borçlu", "Alacaklı", "Sıfır Bakiye" };
            var cariBakiyeDagilimi = new[] { borcluCariSayisi, alacakliCariSayisi, sifirBakiyeliCariSayisi };
            
            ViewBag.CariBakiyeTipleri = JsonConvert.SerializeObject(cariBakiyeTipleri);
            ViewBag.CariBakiyeDagilimi = JsonConvert.SerializeObject(cariBakiyeDagilimi);
            
            // Stok kategorilerine göre ürün miktarları hesaplama
            var kategoriAdlari = new List<string>();
            var stokMiktarlari = new List<decimal>();
            
            var kategoriler = await _unitOfWork.Repository<Data.Entities.UrunKategori>().GetAllAsync();
            if (kategoriler != null)
            {
                foreach (var kategori in kategoriler)
                {
                    kategoriAdlari.Add(kategori.KategoriAdi ?? "İsimsiz Kategori");
                    stokMiktarlari.Add(urunler?
                        .Where(u => u.KategoriID == kategori.KategoriID)
                        .Sum(u => u.StokMiktar) ?? 0);
                }
            }
            
            // Stok grafiği için verileri JSON formatına dönüştür
            ViewBag.StokKategorileri = JsonConvert.SerializeObject(kategoriAdlari);
            ViewBag.StokMiktarlari = JsonConvert.SerializeObject(stokMiktarlari);
            
            // 1. Kasa ve Banka Bakiyeleri için veri hazırlama
            var kasalar = await _unitOfWork.Repository<Data.Entities.Kasa>().GetAllAsync();
            var bankalar = await _unitOfWork.Repository<Data.Entities.Banka>().GetAllAsync();
            var bankaHesaplar = await _unitOfWork.Repository<Data.Entities.BankaHesap>().GetAllAsync();
            
            var kasaBankaLabels = new List<string>();
            var kasaBankaBakiyeleri = new List<decimal>();
            
            // Kasa bakiyeleri
            foreach (var kasa in kasalar.Where(k => !k.Silindi && k.Aktif))
            {
                kasaBankaLabels.Add($"Kasa: {kasa.KasaAdi}");
                decimal bakiyeUSD = kasa.GuncelBakiye;
                try
                {
                    // Kasa para birimi USD değilse çevir
                    if (kasa.ParaBirimi != "USD")
                    {
                        bakiyeUSD = await _dovizKuruService.CevirmeTutarByKodAsync(
                            kasa.GuncelBakiye, 
                            kasa.ParaBirimi, 
                            "USD");
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, $"Kasa bakiyesi {kasa.ParaBirimi} -> USD dönüşümünde hata: {ex.Message}");
                    if (kasa.ParaBirimi == "TRY" && tryToUsdKur > 0)
                        bakiyeUSD = kasa.GuncelBakiye * tryToUsdKur;
                    else if (kasa.ParaBirimi == "UZS" && uzsToUsdKur > 0)
                        bakiyeUSD = kasa.GuncelBakiye * uzsToUsdKur;
                }
                kasaBankaBakiyeleri.Add(bakiyeUSD);
            }
            
            // Banka bakiyeleri
            foreach (var hesap in bankaHesaplar.Where(h => !h.Silindi && h.Aktif))
            {
                var banka = bankalar.FirstOrDefault(b => b.BankaID == hesap.BankaID);
                string bankaAdi = banka?.BankaAdi ?? "Bilinmeyen Banka";
                kasaBankaLabels.Add($"{bankaAdi}: {hesap.HesapAdi}");
                
                decimal bakiyeUSD = hesap.GuncelBakiye;
                try
                {
                    // Banka hesap para birimi USD değilse çevir
                    if (hesap.ParaBirimi != "USD")
                    {
                        bakiyeUSD = await _dovizKuruService.CevirmeTutarByKodAsync(
                            hesap.GuncelBakiye, 
                            hesap.ParaBirimi, 
                            "USD");
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, $"Banka bakiyesi {hesap.ParaBirimi} -> USD dönüşümünde hata: {ex.Message}");
                    if (hesap.ParaBirimi == "TRY" && tryToUsdKur > 0)
                        bakiyeUSD = hesap.GuncelBakiye * tryToUsdKur;
                    else if (hesap.ParaBirimi == "UZS" && uzsToUsdKur > 0)
                        bakiyeUSD = hesap.GuncelBakiye * uzsToUsdKur;
                }
                kasaBankaBakiyeleri.Add(bakiyeUSD);
            }
            
            ViewBag.KasaBankaLabels = JsonConvert.SerializeObject(kasaBankaLabels);
            ViewBag.KasaBankaBakiyeleri = JsonConvert.SerializeObject(kasaBankaBakiyeleri);
            
            // 2. Açık Alacak ve Borç Tutarları
            decimal acikAlacakTutari = 0;
            decimal acikBorcTutari = 0;
            
            // Vadesi geçmiş veya ödenmemiş faturaları bul
            var bugun = DateTime.Today;
            var acikSatisFaturalari = satisFaturalari
                .Where(f => (f.OdemeDurumu == "Ödenmedi" || f.OdemeDurumu == "Kısmi Ödeme") && (f.VadeTarihi <= bugun))
                .ToList();
            
            var acikAlisFaturalari = yillikAlisFaturalari
                .Where(f => (f.OdemeDurumu == "Ödenmedi" || f.OdemeDurumu == "Kısmi Ödeme") && (f.VadeTarihi <= bugun))
                .ToList();
            
            acikAlacakTutari = acikSatisFaturalari.Sum(f => f.GenelToplam ?? 0) - 
                               acikSatisFaturalari.Sum(f => f.OdenenTutar ?? 0);
            
            acikBorcTutari = acikAlisFaturalari.Sum(f => f.GenelToplam ?? 0) - 
                             acikAlisFaturalari.Sum(f => f.OdenenTutar ?? 0);
            
            ViewBag.AcikAlacakTutari = acikAlacakTutari;
            ViewBag.AcikBorcTutari = acikBorcTutari;
            
            // 3. En Çok Satılan 5 Ürün (Son 30 Gün)
            var sonOtuzGun = DateTime.Now.AddDays(-30);
            var faturaDetaylar = await _unitOfWork.Repository<Data.Entities.FaturaDetay>().GetAllAsync();
            
            // Son 30 günlük faturaları bul
            var sonOtuzGunSatisFaturalari = faturalar?
                .Where(f => f.FaturaTuru?.FaturaTuruAdi == "Satış" && f.FaturaTarihi >= sonOtuzGun && !f.Silindi)
                .ToList() ?? new List<Data.Entities.Fatura>();
            
            // Bu faturaların detaylarını topla
            var sonOtuzGunDetaylar = faturaDetaylar?
                .Where(d => !d.Silindi && sonOtuzGunSatisFaturalari.Any(f => f.FaturaID == d.FaturaID))
                .ToList() ?? new List<Data.Entities.FaturaDetay>();
            
            // Ürün bazında satış miktarlarını hesapla
            var urunSatislar = sonOtuzGunDetaylar
                .GroupBy(d => d.UrunID)
                .Select(g => new { 
                    UrunID = g.Key,
                    ToplamMiktar = g.Sum(d => d.Miktar),
                    ToplamTutar = g.Sum(d => (d.BirimFiyat * d.Miktar) * (1 - d.IndirimOrani / 100))
                })
                .OrderByDescending(x => x.ToplamTutar)
                .Take(5)
                .ToList();
            
            var enCokSatilanUrunler = new List<string>();
            var enCokSatilanMiktarlar = new List<decimal>();
            var topProducts = new List<dynamic>();
            
            foreach (var urunSatis in urunSatislar)
            {
                var urun = urunler?.FirstOrDefault(u => u.UrunID == urunSatis.UrunID);
                if (urun != null)
                {
                    enCokSatilanUrunler.Add(urun.UrunAdi);
                    enCokSatilanMiktarlar.Add(urunSatis.ToplamMiktar);
                    
                    topProducts.Add(new { 
                        ProductName = urun.UrunAdi, 
                        TotalSales = urunSatis.ToplamTutar, 
                        TotalQuantity = urunSatis.ToplamMiktar 
                    });
                }
            }
            
            ViewBag.EnCokSatilanUrunler = JsonConvert.SerializeObject(enCokSatilanUrunler);
            ViewBag.EnCokSatilanUrunMiktarlari = JsonConvert.SerializeObject(enCokSatilanMiktarlar);
            ViewBag.TopProducts = topProducts;
            
            // 4. Alacak Yaşlandırma Analizi
            var alacakYaslari = new decimal[4] { 0, 0, 0, 0 }; // 0-30, 31-60, 61-90, 91+
            
            // Sadece satış faturalarını ve ödenmemiş olanları al
            var odenmemisSatisFaturalari = satisFaturalari.Where(f => 
                (f.OdemeDurumu == "Ödenmedi" || f.OdemeDurumu == "Kısmi Ödeme") && 
                f.VadeTarihi.HasValue).ToList();
            
            foreach (var fatura in odenmemisSatisFaturalari)
            {
                decimal kalanTutar = (fatura.GenelToplam ?? 0) - (fatura.OdenenTutar ?? 0);
                if (kalanTutar <= 0) continue;
                
                TimeSpan fark = bugun - fatura.VadeTarihi.Value;
                int gunFarki = fark.Days;
                
                if (gunFarki <= 30)
                    alacakYaslari[0] += kalanTutar;
                else if (gunFarki <= 60)
                    alacakYaslari[1] += kalanTutar;
                else if (gunFarki <= 90)
                    alacakYaslari[2] += kalanTutar;
                else
                    alacakYaslari[3] += kalanTutar;
            }
            
            ViewBag.AlacakYaslariMiktarlari = JsonConvert.SerializeObject(alacakYaslari);
            
            // 5. Nakit Akış Projeksiyonu (Gelecek 3 Ay)
            var gelecekUcAy = new decimal[3];
            var gelecekUcAyLabels = new string[3];
            
            for (int i = 0; i < 3; i++)
            {
                var ayBasi = DateTime.Today.AddMonths(i).AddDays(1 - DateTime.Today.Day);
                var aySonu = ayBasi.AddMonths(1).AddDays(-1);
                
                // Ay adını belirle
                gelecekUcAyLabels[i] = ayBasi.ToString("MMMM yyyy", new CultureInfo("tr-TR"));
                
                // Bu ay içinde vadesi gelecek satışları topla (tahsilat)
                decimal tahsilat = satisFaturalari
                    .Where(f => f.VadeTarihi >= ayBasi && f.VadeTarihi <= aySonu && 
                           (f.OdemeDurumu == "Ödenmedi" || f.OdemeDurumu == "Kısmi Ödeme"))
                    .Sum(f => (f.GenelToplam ?? 0) - (f.OdenenTutar ?? 0));
                
                // Bu ay içinde vadesi gelecek alışları topla (ödeme)
                decimal odeme = yillikAlisFaturalari
                    .Where(f => f.VadeTarihi >= ayBasi && f.VadeTarihi <= aySonu && 
                          (f.OdemeDurumu == "Ödenmedi" || f.OdemeDurumu == "Kısmi Ödeme"))
                    .Sum(f => (f.GenelToplam ?? 0) - (f.OdenenTutar ?? 0));
                
                // Net nakit akışı hesapla (tahsilat - ödeme)
                gelecekUcAy[i] = tahsilat - odeme;
            }
            
            ViewBag.NakitAkisProj = JsonConvert.SerializeObject(gelecekUcAy);
            ViewBag.NakitAkisProjAylar = JsonConvert.SerializeObject(gelecekUcAyLabels);
            
            // Temel veri modelini oluştur
            var buAy = DateTime.Now;
            var buAyBasi = new DateTime(buAy.Year, buAy.Month, 1);
            var buAySonu = buAyBasi.AddMonths(1).AddDays(-1);
            
            // Günlük satış ve gider verileri
            var buAyGunler = Enumerable.Range(1, buAySonu.Day)
                .Select(gun => new DateTime(buAy.Year, buAy.Month, gun).ToString("dd MMM"))
                .ToArray();
            
            var buAySatislar = new decimal[buAySonu.Day];
            var buAyGiderler = new decimal[buAySonu.Day];
            
            // Bu ay içindeki satış faturaları
            var buAySatisFaturalari = faturalar?
                .Where(f => f.FaturaTuru?.FaturaTuruAdi == "Satış" && 
                      f.FaturaTarihi >= buAyBasi && f.FaturaTarihi <= buAySonu && !f.Silindi)
                .ToList() ?? new List<Data.Entities.Fatura>();
            
            // Bu ay içindeki alış faturaları
            var buAyAlisFaturalari = faturalar?
                .Where(f => f.FaturaTuru?.FaturaTuruAdi == "Alış" && 
                      f.FaturaTarihi >= buAyBasi && f.FaturaTarihi <= buAySonu && !f.Silindi)
                .ToList() ?? new List<Data.Entities.Fatura>();
            
            // Günlük satış ve gider toplamlarını hesapla
            for (int gun = 1; gun <= buAySonu.Day; gun++)
            {
                var tarih = new DateTime(buAy.Year, buAy.Month, gun);
                
                buAySatislar[gun - 1] = buAySatisFaturalari
                    .Where(f => f.FaturaTarihi?.Day == gun)
                    .Sum(f => f.GenelToplam ?? 0);
                
                buAyGiderler[gun - 1] = buAyAlisFaturalari
                    .Where(f => f.FaturaTarihi?.Day == gun)
                    .Sum(f => f.GenelToplam ?? 0);
            }
            
            ViewBag.Days = JsonConvert.SerializeObject(buAyGunler);
            ViewBag.Revenues = JsonConvert.SerializeObject(buAySatislar);
            ViewBag.Expenses = JsonConvert.SerializeObject(buAyGiderler);
            
            // Kategori bazında satış dağılımı
            var kategoriSatislar = new Dictionary<Guid, decimal>();
            
            foreach (var detay in sonOtuzGunDetaylar)
            {
                var urun = urunler?.FirstOrDefault(u => u.UrunID == detay.UrunID);
                if (urun != null && urun.KategoriID.HasValue)
                {
                    decimal satisTutari = detay.BirimFiyat * detay.Miktar * (1 - detay.IndirimOrani / 100);
                    
                    if (kategoriSatislar.ContainsKey(urun.KategoriID.Value))
                        kategoriSatislar[urun.KategoriID.Value] += satisTutari;
                    else
                        kategoriSatislar[urun.KategoriID.Value] = satisTutari;
                }
            }
            
            var kategorilerSatisDagilimi = new List<string>();
            var kategoriSatisTutarlari = new List<decimal>();
            
            foreach (var kategoriSatis in kategoriSatislar.OrderByDescending(ks => ks.Value).Take(5))
            {
                var kategori = kategoriler?.FirstOrDefault(k => k.KategoriID == kategoriSatis.Key);
                if (kategori != null)
                {
                    kategorilerSatisDagilimi.Add(kategori.KategoriAdi ?? "İsimsiz Kategori");
                    kategoriSatisTutarlari.Add(kategoriSatis.Value);
                }
            }
            
            ViewBag.Categories = JsonConvert.SerializeObject(kategorilerSatisDagilimi);
            ViewBag.CategoryRevenues = JsonConvert.SerializeObject(kategoriSatisTutarlari);
            
            // Son 6 aylık trend verileri
            var son6Ay = Enumerable.Range(0, 6)
                .Select(i => DateTime.Now.AddMonths(-i))
                .OrderBy(d => d.Year)
                .ThenBy(d => d.Month)
                .ToList();
            
            var son6AyLabels = son6Ay.Select(d => d.ToString("MMM yyyy", new CultureInfo("tr-TR"))).ToArray();
            var son6AySatislar = new decimal[6];
            var son6AyGiderler = new decimal[6];
            
            for (int i = 0; i < 6; i++)
            {
                var ay = son6Ay[i];
                var ayBasi = new DateTime(ay.Year, ay.Month, 1);
                var aySonu = ayBasi.AddMonths(1).AddDays(-1);
                
                son6AySatislar[i] = satisFaturalari
                    .Where(f => f.FaturaTarihi >= ayBasi && f.FaturaTarihi <= aySonu)
                    .Sum(f => f.GenelToplam ?? 0);
                
                son6AyGiderler[i] = yillikAlisFaturalari
                    .Where(f => f.FaturaTarihi >= ayBasi && f.FaturaTarihi <= aySonu)
                    .Sum(f => f.GenelToplam ?? 0);
            }
            
            ViewBag.Last6MonthLabels = JsonConvert.SerializeObject(son6AyLabels);
            ViewBag.TrendRevenues = JsonConvert.SerializeObject(son6AySatislar);
            ViewBag.TrendExpenses = JsonConvert.SerializeObject(son6AyGiderler);
            
            ViewBag.TotalRevenue = son6AySatislar.Sum();
            ViewBag.TotalExpense = son6AyGiderler.Sum();
            
            // Para birimi bilgisini tüm view'larda kullanılabilmesi için ViewBag'e ekle
            ViewBag.ParaBirimi = "USD";
            ViewBag.TryToUsdKur = tryToUsdKur;
            ViewBag.UzsToUsdKur = uzsToUsdKur;
        }

        // Varsayılan menüleri oluşturan yardımcı metot
        private List<MenuViewModel> GetDefaultMenuItems()
        {
            var result = new List<MenuViewModel>();
            
            // Dashboard menüsü
            result.Add(new MenuViewModel
            {
                MenuID = Guid.NewGuid(),
                Ad = "Dashboard",
                Icon = "material-icons",
                Controller = "Home",
                Action = "Index",
                AktifMi = true,
                Sira = 1
            });
            
            // Cariler menüsü
            result.Add(new MenuViewModel
            {
                MenuID = Guid.NewGuid(),
                Ad = "Cariler",
                Icon = "material-icons",
                Controller = "Cari",
                Action = "Index",
                AktifMi = true,
                Sira = 2
            });
            
            // Stok Yönetimi menüsü (üst menü)
            var stokYonetimiMenu = new MenuViewModel
            {
                MenuID = Guid.NewGuid(),
                Ad = "Stok Yönetimi",
                Icon = "material-icons",
                AktifMi = true,
                Sira = 3
            };
            
            // Stok alt menüleri
            stokYonetimiMenu.AltMenuler.Add(new MenuViewModel
            {
                MenuID = Guid.NewGuid(),
                Ad = "Ürünler",
                Controller = "Urun",
                Action = "Index",
                AktifMi = true,
                Sira = 1,
                UstMenuID = stokYonetimiMenu.MenuID
            });
            
            stokYonetimiMenu.AltMenuler.Add(new MenuViewModel
            {
                MenuID = Guid.NewGuid(),
                Ad = "Kategoriler",
                Controller = "UrunKategori",
                Action = "Index",
                AktifMi = true,
                Sira = 2,
                UstMenuID = stokYonetimiMenu.MenuID
            });
            
            stokYonetimiMenu.AltMenuler.Add(new MenuViewModel
            {
                MenuID = Guid.NewGuid(),
                Ad = "Stok Durumu",
                Controller = "Stok",
                Action = "StokDurumu",
                AktifMi = true,
                Sira = 3,
                UstMenuID = stokYonetimiMenu.MenuID
            });
            
            result.Add(stokYonetimiMenu);
            
            // Faturalar menüsü
            result.Add(new MenuViewModel
            {
                MenuID = Guid.NewGuid(),
                Ad = "Faturalar",
                Icon = "material-icons",
                Controller = "Fatura",
                Action = "Index",
                AktifMi = true,
                Sira = 4
            });
            
            // İrsaliyeler menüsü
            result.Add(new MenuViewModel
            {
                MenuID = Guid.NewGuid(),
                Ad = "İrsaliyeler",
                Icon = "material-icons",
                Controller = "Irsaliye",
                Action = "Index",
                AktifMi = true,
                Sira = 5
            });
            
            // Finans menüsü
            var finansMenu = new MenuViewModel
            {
                MenuID = Guid.NewGuid(),
                Ad = "Finans",
                Icon = "material-icons",
                AktifMi = true,
                Sira = 6
            };
            
            // Finans Alt Menüleri
            finansMenu.AltMenuler.Add(new MenuViewModel
            {
                MenuID = Guid.NewGuid(),
                Ad = "Kasa İşlemleri",
                Controller = "Kasa",
                Action = "Index",
                AktifMi = true,
                Sira = 1,
                UstMenuID = finansMenu.MenuID
            });
            
            finansMenu.AltMenuler.Add(new MenuViewModel
            {
                MenuID = Guid.NewGuid(),
                Ad = "Banka İşlemleri",
                Controller = "Banka",
                Action = "Index",
                AktifMi = true,
                Sira = 2,
                UstMenuID = finansMenu.MenuID
            });
            
            finansMenu.AltMenuler.Add(new MenuViewModel
            {
                MenuID = Guid.NewGuid(),
                Ad = "Döviz Kurları",
                Controller = "DovizKuru",
                Action = "Liste",
                AktifMi = true,
                Sira = 3,
                UstMenuID = finansMenu.MenuID
            });
            
            finansMenu.AltMenuler.Add(new MenuViewModel
            {
                MenuID = Guid.NewGuid(),
                Ad = "Para Birimleri",
                Controller = "ParaBirimi",
                Action = "Index",
                AktifMi = true,
                Sira = 4,
                UstMenuID = finansMenu.MenuID
            });
            
            result.Add(finansMenu);
            
            // Sistem Ayarları menüsü
            var sistemMenu = new MenuViewModel
            {
                MenuID = Guid.NewGuid(),
                Ad = "Sistem Ayarları",
                Icon = "material-icons",
                AktifMi = true,
                Sira = 7
            };
            
            // Sistem Alt Menüleri
            sistemMenu.AltMenuler.Add(new MenuViewModel
            {
                MenuID = Guid.NewGuid(),
                Ad = "Depolar",
                Controller = "Depo",
                Action = "Index",
                AktifMi = true,
                Sira = 1,
                UstMenuID = sistemMenu.MenuID
            });
            
            sistemMenu.AltMenuler.Add(new MenuViewModel
            {
                MenuID = Guid.NewGuid(),
                Ad = "Sistem Logları",
                Controller = "SistemLog",
                Action = "Index",
                AktifMi = true,
                Sira = 2,
                UstMenuID = sistemMenu.MenuID
            });
            
            sistemMenu.AltMenuler.Add(new MenuViewModel
            {
                MenuID = Guid.NewGuid(),
                Ad = "Menü Yönetimi",
                Controller = "Menu",
                Action = "Index",
                AktifMi = true,
                Sira = 3,
                UstMenuID = sistemMenu.MenuID
            });
            
            result.Add(sistemMenu);
            
            // Kullanıcı Yönetimi menüsü
            result.Add(new MenuViewModel
            {
                MenuID = Guid.NewGuid(),
                Ad = "Kullanıcı Yönetimi",
                Icon = "material-icons",
                Controller = "Account",
                Action = "UserManagement",
                AktifMi = true,
                Sira = 8
            });
            
            return result;
        }

        [HttpPost]
        public async Task<IActionResult> TestNotification()
        {
            try
            {
                await _notificationService.SendCriticalNotificationAsync(
                    "Test Bildirimi",
                    "Bu bir test bildirimidir. Bildirim sistemi başarıyla çalışıyor."
                );

                return Json(new { success = true, message = "Test bildirimi başarıyla gönderildi." });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Bildirim gönderilirken hata oluştu: " + ex.Message });
            }
        }

        private async Task<string> GetCariAdiByIdAsync(Guid cariId)
        {
            var cari = await _unitOfWork.CariRepository.GetByIdAsync(cariId);
            if (cari != null)
            {
                return cari.Ad;
            }
            return "Bilinmeyen Cari";
        }
    }
}
