using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using MuhasebeStokWebApp.Services;
using MuhasebeStokWebApp.ViewModels.Menu;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Authorization;
using MuhasebeStokWebApp.Data.Entities;
using MuhasebeStokWebApp.Models;
using MuhasebeStokWebApp.Services.Interfaces;

namespace MuhasebeStokWebApp.Controllers
{
    [Authorize]
    public abstract class BaseController : Controller
    {
        private readonly IMenuService _menuService;
        protected readonly UserManager<ApplicationUser> _userManager;
        protected readonly RoleManager<IdentityRole> _roleManager;
        protected readonly ILogService _logService;
        
        // Constructor: Tüm controller'ların ihtiyaç duyduğu temel servisleri alır
        protected BaseController(
            IMenuService menuService = null,
            UserManager<ApplicationUser> userManager = null,
            RoleManager<IdentityRole> roleManager = null,
            ILogService logService = null)
        {
            _menuService = menuService;
            _userManager = userManager;
            _roleManager = roleManager;
            _logService = logService;
        }
        
        // Action çalıştırılmadan önce devreye giren metot - Menü ve yetkilendirme işlemleri burada yapılır
        public override async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
        {
            try 
            {
                // Kullanıcı kimliği ve rol bilgilerini al
                var userId = User?.FindFirstValue(ClaimTypes.NameIdentifier);
                var userRoles = User?.FindAll(ClaimTypes.Role)?.Select(c => c.Value)?.ToList();
                
                // Kullanıcı giriş yapmışsa ve rolleri varsa
                if (!string.IsNullOrEmpty(userId) && userRoles != null && userRoles.Any())
                {
                    // Admin rolüne sahip kullanıcılar için özel işlemler
                    bool isAdmin = userRoles.Contains("Admin");
                    ViewBag.IsAdmin = isAdmin;
                    
                    try
                    {
                        // Kullanıcının rollerine göre menü öğelerini getir
                        var menuItems = await _menuService.GetActiveSidebarMenusAsync(userId);
                        
                        if (menuItems == null || !menuItems.Any())
                        {
                            // Alternatif bir yöntem dene: varsayılan menüleri manuel olarak oluştur
                            await _logService.LogWarningAsync("BaseController.OnActionExecutionAsync", 
                                "Menüler boş geldi. Varsayılan menüler kullanılıyor.");
                            menuItems = CreateDefaultMenuItems();
                            
                            // Debugging için ek bilgi
                            ViewBag.UsingDefaultMenus = true;
                        }
                        
                        // Menüleri düzgün sıraya koy
                        menuItems = menuItems.OrderBy(m => m.Sira).ToList();
                        
                        // Stil sınıflarını ekleyerek menü öğelerinin görünümünü iyileştir
                        string currentController = context.RouteData.Values["controller"]?.ToString();
                        string currentAction = context.RouteData.Values["action"]?.ToString();
                        
                        foreach (var menuItem in menuItems)
                        {
                            // Controller adı mevcut controller adı ile eşleşiyorsa aktif olarak işaretle
                            if (!string.IsNullOrEmpty(menuItem.Controller) && 
                                menuItem.Controller.Equals(currentController, StringComparison.OrdinalIgnoreCase))
                            {
                                menuItem.Active = true;
                            }
                            
                            // Alt menüler için de kontrolü yap
                            foreach (var subItem in menuItem.AltMenuler)
                            {
                                if (!string.IsNullOrEmpty(subItem.Controller) && 
                                    subItem.Controller.Equals(currentController, StringComparison.OrdinalIgnoreCase))
                                {
                                    subItem.Active = true;
                                    menuItem.Active = true; // Üst menüyü de aktif yap
                                }
                            }
                        }
                        
                        // ViewBag'e menüleri aktar
                        ViewBag.MenuItems = menuItems;
                        ViewBag.IsAuthenticated = true;
                        ViewBag.RequiresLogin = false;
                    }
                    catch (Exception ex)
                    {
                        // Hata durumunda varsayılan menüleri oluştur ve hatayı logla
                        await _logService.LogErrorAsync("BaseController.OnActionExecutionAsync", 
                            $"Menüler getirilirken hata: {ex.Message}");
                        ViewBag.MenuItems = CreateDefaultMenuItems();
                        ViewBag.UsingDefaultMenus = true;
                        ViewBag.MenuLoadError = ex.Message;
                    }
                }
                else
                {
                    // Giriş yapmamış kullanıcılar için
                    ViewBag.MenuItems = new List<MenuViewModel>();
                    ViewBag.IsAuthenticated = false;
                    ViewBag.RequiresLogin = true;
                }
            }
            catch (Exception ex)
            {
                // Genel hata durumunda
                await _logService.LogErrorAsync("BaseController.OnActionExecutionAsync", 
                    $"Beklenmeyen hata: {ex.Message}");
                
                // Yine de varsayılan menüleri oluşturup görüntüleyelim
                ViewBag.MenuItems = CreateDefaultMenuItems();
                ViewBag.IsAuthenticated = true;
                ViewBag.RequiresLogin = false;
                ViewBag.UsingDefaultMenus = true;
                ViewBag.MenuLoadError = ex.Message;
            }

            // İşleme devam et
            await next();
        }
        
        // Varsayılan menüleri oluşturan yardımcı metot
        private List<MenuViewModel> CreateDefaultMenuItems()
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
                Sira = 1,
                Url = "/Home/Index",
                AltMenuler = new List<MenuViewModel>()
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
                Sira = 2,
                Url = "/Cari/Index",
                AltMenuler = new List<MenuViewModel>()
            });
            
            // Stok Yönetimi menüsü (üst menü)
            var stokYonetimiMenu = new MenuViewModel
            {
                MenuID = Guid.NewGuid(),
                Ad = "Stok Yönetimi",
                Icon = "material-icons",
                AktifMi = true,
                Sira = 3,
                Url = "#",
                AltMenuler = new List<MenuViewModel>() // Alt menü listesini başlat
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
                UstMenuID = stokYonetimiMenu.MenuID,
                Url = "/Urun/Index",
                AltMenuler = new List<MenuViewModel>()
            });
            
            stokYonetimiMenu.AltMenuler.Add(new MenuViewModel
            {
                MenuID = Guid.NewGuid(),
                Ad = "Kategoriler",
                Controller = "UrunKategori",
                Action = "Index",
                AktifMi = true,
                Sira = 2,
                UstMenuID = stokYonetimiMenu.MenuID,
                Url = "/UrunKategori/Index",
                AltMenuler = new List<MenuViewModel>()
            });
            
            stokYonetimiMenu.AltMenuler.Add(new MenuViewModel
            {
                MenuID = Guid.NewGuid(),
                Ad = "Stok Durumu",
                Controller = "Stok",
                Action = "StokDurumu",
                AktifMi = true,
                Sira = 3,
                UstMenuID = stokYonetimiMenu.MenuID,
                Url = "/Stok/StokDurumu",
                AltMenuler = new List<MenuViewModel>()
            });
            
            result.Add(stokYonetimiMenu);
            
            // Döviz İşlemleri menüsü (üst menü)
            var dovizIslemleriMenu = new MenuViewModel
            {
                MenuID = Guid.NewGuid(),
                Ad = "Döviz İşlemleri",
                Icon = "fas fa-money-bill-wave",
                AktifMi = true,
                Sira = 4,
                Url = "#",
                AltMenuler = new List<MenuViewModel>() // Alt menü listesini başlat
            };
            
            // Döviz alt menüleri
            dovizIslemleriMenu.AltMenuler.Add(new MenuViewModel
            {
                MenuID = Guid.NewGuid(),
                Ad = "Para Birimleri",
                Controller = "Doviz",
                Action = "Index",
                AktifMi = true,
                Sira = 1,
                UstMenuID = dovizIslemleriMenu.MenuID,
                Url = "/Doviz/Index",
                AltMenuler = new List<MenuViewModel>()
            });
            
            dovizIslemleriMenu.AltMenuler.Add(new MenuViewModel
            {
                MenuID = Guid.NewGuid(),
                Ad = "Döviz Kurları",
                Controller = "Kur",
                Action = "Liste",
                AktifMi = true,
                Sira = 2,
                UstMenuID = dovizIslemleriMenu.MenuID,
                Url = "/Kur/Liste",
                AltMenuler = new List<MenuViewModel>()
            });
            
            dovizIslemleriMenu.AltMenuler.Add(new MenuViewModel
            {
                MenuID = Guid.NewGuid(),
                Ad = "Kur Güncelleme",
                Controller = "DovizKuru",
                Action = "KurlariGuncelle",
                AktifMi = true,
                Sira = 3,
                UstMenuID = dovizIslemleriMenu.MenuID,
                Url = "/DovizKuru/KurlariGuncelle",
                AltMenuler = new List<MenuViewModel>()
            });
            
            result.Add(dovizIslemleriMenu);
            
            // Faturalar menüsü
            result.Add(new MenuViewModel
            {
                MenuID = Guid.NewGuid(),
                Ad = "Faturalar",
                Icon = "material-icons",
                Controller = "Fatura",
                Action = "Index",
                AktifMi = true,
                Sira = 5,
                Url = "/Fatura/Index",
                AltMenuler = new List<MenuViewModel>()
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
                Sira = 6,
                Url = "/Irsaliye/Index",
                AltMenuler = new List<MenuViewModel>()
            });
            
            // Finans menüsü
            var finansMenu = new MenuViewModel
            {
                MenuID = Guid.NewGuid(),
                Ad = "Finans",
                Icon = "material-icons",
                AktifMi = true,
                Sira = 7,
                Url = "#",
                AltMenuler = new List<MenuViewModel>()
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
                UstMenuID = finansMenu.MenuID,
                Url = "/Kasa/Index",
                AltMenuler = new List<MenuViewModel>()
            });
            
            finansMenu.AltMenuler.Add(new MenuViewModel
            {
                MenuID = Guid.NewGuid(),
                Ad = "Banka İşlemleri",
                Controller = "Banka",
                Action = "Index",
                AktifMi = true,
                Sira = 2,
                UstMenuID = finansMenu.MenuID,
                Url = "/Banka/Index",
                AltMenuler = new List<MenuViewModel>()
            });
            
            finansMenu.AltMenuler.Add(new MenuViewModel
            {
                MenuID = Guid.NewGuid(),
                Ad = "Döviz Kurları",
                Controller = "DovizKuru",
                Action = "Index",
                AktifMi = true,
                Sira = 3,
                UstMenuID = finansMenu.MenuID,
                Url = "/DovizKuru/Index",
                AltMenuler = new List<MenuViewModel>()
            });
            
            finansMenu.AltMenuler.Add(new MenuViewModel
            {
                MenuID = Guid.NewGuid(),
                Ad = "Para Birimleri",
                Controller = "ParaBirimi",
                Action = "Index",
                AktifMi = true,
                Sira = 4,
                UstMenuID = finansMenu.MenuID,
                Url = "/ParaBirimi/Index",
                AltMenuler = new List<MenuViewModel>()
            });
            
            result.Add(finansMenu);
            
            // Sistem Ayarları menüsü
            var sistemMenu = new MenuViewModel
            {
                MenuID = Guid.NewGuid(),
                Ad = "Sistem Ayarları",
                Icon = "material-icons",
                AktifMi = true,
                Sira = 8,
                Url = "#",
                AltMenuler = new List<MenuViewModel>()
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
                UstMenuID = sistemMenu.MenuID,
                Url = "/Depo/Index",
                AltMenuler = new List<MenuViewModel>()
            });
            
            sistemMenu.AltMenuler.Add(new MenuViewModel
            {
                MenuID = Guid.NewGuid(),
                Ad = "Sistem Logları",
                Controller = "SistemLog",
                Action = "Index",
                AktifMi = true,
                Sira = 2,
                UstMenuID = sistemMenu.MenuID,
                Url = "/SistemLog/Index",
                AltMenuler = new List<MenuViewModel>()
            });
            
            sistemMenu.AltMenuler.Add(new MenuViewModel
            {
                MenuID = Guid.NewGuid(),
                Ad = "Menü Yönetimi",
                Controller = "Menu",
                Action = "Index",
                AktifMi = true,
                Sira = 3,
                UstMenuID = sistemMenu.MenuID,
                Url = "/Menu/Index",
                AltMenuler = new List<MenuViewModel>()
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
                Sira = 9,
                Url = "/Account/UserManagement",
                AltMenuler = new List<MenuViewModel>()
            });
            
            return result;
        }

        protected Guid GetCurrentUserId()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!string.IsNullOrEmpty(userId))
            {
                return Guid.Parse(userId);
            }
            return Guid.Empty;
        }

        protected async Task<ApplicationUser> GetCurrentUserAsync()
        {
            return await _userManager.GetUserAsync(User);
        }
        
        protected int GetCurrentUserNumericId()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!string.IsNullOrEmpty(userId))
            {
                // Kullanıcı ID'sinden sayısal değer elde etmek için basit bir yaklaşım
                return Math.Abs(userId.GetHashCode() % 1000); // 0-999 arası bir sayı üret
            }
            return 1; // Varsayılan kullanıcı ID (sistem kullanıcısı)
        }
    }
} 