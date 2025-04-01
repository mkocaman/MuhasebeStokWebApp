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
            
            // Toplam ciro hesaplama (sadece satış faturalarının toplamı)
            var satisFaturalari = faturalar?.Where(f => f.FaturaTuru?.FaturaTuruAdi == "Satış").ToList() ?? new List<Data.Entities.Fatura>();
            decimal toplamCiro = satisFaturalari.Sum(f => f.GenelToplam ?? 0);
            ViewBag.ToplamCiro = toplamCiro;
            
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
                aylikSatisVerileri[ay - 1] = yillikSatisFaturalari
                    .Where(f => f.FaturaTarihi?.Month == ay)
                    .Sum(f => f.GenelToplam ?? 0);
                
                aylikGiderVerileri[ay - 1] = yillikAlisFaturalari
                    .Where(f => f.FaturaTarihi?.Month == ay)
                    .Sum(f => f.GenelToplam ?? 0);
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
                    // CariID artık Guid tipinde
                    var hareketler = cariHareketler?.Where(h => h.CariId == cari.CariID).ToList() ?? new List<CariHareket>();
                    decimal bakiye = 0;
                    
                    // Her cari hareket türüne göre bakiyeyi güncelle
                    foreach (var hareket in hareketler)
                    {
                        if (hareket.HareketTuru == "Borç")
                        {
                            bakiye -= hareket.Tutar;
                        }
                        else if (hareket.HareketTuru == "Alacak")
                        {
                            bakiye += hareket.Tutar;
                        }
                    }
                    
                    cariBakiyeleri[cari.CariID] = bakiye;
                }
            }
            
            // Borçlu, alacaklı ve sıfır bakiyeli cari sayılarını hesapla
            int borcluCariSayisi = cariBakiyeleri.Count(c => c.Value < 0);
            int alacakliCariSayisi = cariBakiyeleri.Count(c => c.Value > 0);
            int sifirBakiyeCariSayisi = cariBakiyeleri.Count(c => c.Value == 0);
            
            // Pasta grafik için verileri hazırla
            ViewBag.CariBakiyeTipleri = JsonConvert.SerializeObject(new[] { "Borçlu", "Alacaklı", "Sıfır Bakiye" });
            ViewBag.CariBakiyeDagilimi = JsonConvert.SerializeObject(new[] { borcluCariSayisi, alacakliCariSayisi, sifirBakiyeCariSayisi });
            
            // Stok kategorileri ve miktarlarını hesapla
            var kategoriRepository = _unitOfWork.Repository<UrunKategori>();
            var kategoriler = await kategoriRepository.GetAllAsync();
            
            var kategoriAdlari = new List<string>();
            var stokMiktarlari = new List<decimal>();
            
            // Her kategori için toplam stok miktarını hesapla
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
