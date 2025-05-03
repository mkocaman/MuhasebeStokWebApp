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
using System.Net;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Localization;
using MuhasebeStokWebApp.ViewModels;
using MuhasebeStokWebApp.Data;
using MuhasebeStokWebApp.Services.Menu;

namespace MuhasebeStokWebApp.Controllers
{
    [Authorize]
    public abstract class BaseController : Controller
    {
        private readonly IMenuService _menuService;
        protected readonly UserManager<ApplicationUser> _userManager;
        protected readonly RoleManager<IdentityRole> _roleManager;
        protected readonly ILogService _logService;
        private readonly IStringLocalizer<BaseController> _localizer;
        private readonly ILanguageService _languageService;
        
        // Constructor: Tüm controller'ların ihtiyaç duyduğu temel servisleri alır
        public BaseController(
            IMenuService menuService = null,
            UserManager<ApplicationUser> userManager = null,
            RoleManager<IdentityRole> roleManager = null,
            ILogService logService = null,
            IStringLocalizer<BaseController> localizer = null,
            ILanguageService languageService = null)
        {
            _menuService = menuService;
            _userManager = userManager;
            _roleManager = roleManager;
            _logService = logService;
            _localizer = localizer;
            _languageService = languageService;
        }
        
        // Action çalıştırılmadan önce devreye giren metot - Menü ve yetkilendirme işlemleri burada yapılır
        public override async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
        {
            try 
            {
                // Kullanıcı ve rol bilgisini Template Method deseni kullanarak al
                var (userId, userRoles, isAdmin) = await GetUserRoleInfoAsync();
                ViewBag.IsAdmin = isAdmin;
                
                // Kullanıcı giriş yapmışsa ve rolleri varsa
                if (!string.IsNullOrEmpty(userId) && userRoles != null && userRoles.Any())
                {
                    // Alt sınıflara özel hook
                    await OnAuthenticatedUserAsync(userId, userRoles, isAdmin);
                    
                    try
                    {
                        // Kullanıcının rollerine göre menü öğelerini getir
                        var menuItems = await GetMenuItemsAsync(userId);
                        
                        if (menuItems == null || !menuItems.Any())
                        {
                            // Alternatif bir yöntem dene: varsayılan menüleri manuel olarak oluştur
                            await LogWarningAsync("Menüler boş geldi. Varsayılan menüler kullanılıyor.");
                            menuItems = CreateDefaultMenuItems();
                            
                            // Debugging için ek bilgi
                            ViewBag.UsingDefaultMenus = true;
                        }
                        
                        // Menüleri düzgün sıraya koy
                        menuItems = menuItems.OrderBy(m => m.Sira).ToList();
                        
                        // Stil sınıflarını ekleyerek menü öğelerinin görünümünü iyileştir
                        MarkActiveMenuItems(menuItems, context);
                        
                        // ViewBag'e menüleri aktar
                        ViewBag.MenuItems = menuItems;
                        ViewBag.IsAuthenticated = true;
                        ViewBag.RequiresLogin = false;
                    }
                    catch (Exception ex)
                    {
                        // Hata durumunda varsayılan menüleri oluştur ve hatayı logla
                        await LogErrorAsync("Menüler getirilirken hata: " + ex.Message, ex);
                        ViewBag.MenuItems = CreateDefaultMenuItems();
                        ViewBag.UsingDefaultMenus = true;
                        ViewBag.MenuLoadError = ex.Message;
                    }
                }
                else
                {
                    // Giriş yapmamış kullanıcılar için
                    await OnUnauthenticatedUserAsync();
                    ViewBag.MenuItems = new List<MenuViewModel>();
                    ViewBag.IsAuthenticated = false;
                    ViewBag.RequiresLogin = true;
                }
                
                // Alt sınıflar için hook (hem giriş yapmış hem de yapmamış kullanıcılar için)
                await OnBeforeActionExecutionAsync(context);
            }
            catch (Exception ex)
            {
                // Genel hata durumunda
                await LogErrorAsync("Beklenmeyen hata: " + ex.Message, ex);
                
                // Yine de varsayılan menüleri oluşturup görüntüleyelim
                ViewBag.MenuItems = CreateDefaultMenuItems();
                ViewBag.IsAuthenticated = true;
                ViewBag.RequiresLogin = false;
                ViewBag.UsingDefaultMenus = true;
                ViewBag.MenuLoadError = ex.Message;
            }

            // İşleme devam et
            var resultContext = await next();
            
            // Action çalıştırıldıktan sonra devreye giren kod
            await OnAfterActionExecutionAsync(context, resultContext);
        }
        
        /// <summary>
        /// Loglama işlemini yönetir - Override edilebilir
        /// </summary>
        protected virtual async Task LogWarningAsync(string message)
        {
            if (_logService != null)
            {
                await _logService.LogWarningAsync("BaseController.OnActionExecutionAsync", message);
            }
        }
        
        /// <summary>
        /// Hata logunu yönetir - Override edilebilir
        /// </summary>
        protected virtual async Task LogErrorAsync(string message, Exception ex = null)
        {
            if (_logService != null)
            {
                await _logService.LogErrorAsync("BaseController.OnActionExecutionAsync", message, ex);
            }
        }
        
        /// <summary>
        /// Kullanıcı ve rol bilgilerini alır - Override edilebilir template method
        /// </summary>
        protected virtual async Task<(string userId, List<string> userRoles, bool isAdmin)> GetUserRoleInfoAsync()
        {
            // Kullanıcı kimliği ve rol bilgilerini al
            var userId = User?.FindFirstValue(ClaimTypes.NameIdentifier);
            var userRoles = User?.FindAll(ClaimTypes.Role)?.Select(c => c.Value)?.ToList() ?? new List<string>();
            
            // Admin rolüne sahip kullanıcılar için özel işlemler
            bool isAdmin = userRoles.Contains("Admin");
            
            return (userId, userRoles, isAdmin);
        }
        
        /// <summary>
        /// Kullanıcı rollerine göre menü öğelerini getirir - Override edilebilir template method
        /// </summary>
        protected virtual async Task<List<MenuViewModel>> GetMenuItemsAsync(string userId)
        {
            if (_menuService == null)
                return CreateDefaultMenuItems();
                
            return await _menuService.GetActiveSidebarMenusAsync(userId);
        }
        
        /// <summary>
        /// Aktif menü öğelerini işaretler - Override edilebilir
        /// </summary>
        protected virtual void MarkActiveMenuItems(List<MenuViewModel> menuItems, ActionExecutingContext context)
        {
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
        }
        
        /// <summary>
        /// Giriş yapmış kullanıcılar için özelleştirme hook'u - Alt sınıflar override edebilir
        /// </summary>
        protected virtual async Task OnAuthenticatedUserAsync(string userId, List<string> userRoles, bool isAdmin)
        {
            // Alt sınıflar override edebilir
            await Task.CompletedTask;
        }
        
        /// <summary>
        /// Giriş yapmamış kullanıcılar için özelleştirme hook'u - Alt sınıflar override edebilir
        /// </summary>
        protected virtual async Task OnUnauthenticatedUserAsync()
        {
            // Alt sınıflar override edebilir
            await Task.CompletedTask;
        }
        
        /// <summary>
        /// Action çalıştırılmadan önce çağrılan hook - Alt sınıflar override edebilir
        /// </summary>
        protected virtual async Task OnBeforeActionExecutionAsync(ActionExecutingContext context)
        {
            // Alt sınıflar override edebilir
            await Task.CompletedTask;
        }
        
        /// <summary>
        /// Action çalıştırıldıktan sonra çağrılan hook - Alt sınıflar override edebilir
        /// </summary>
        protected virtual async Task OnAfterActionExecutionAsync(ActionExecutingContext context, ActionExecutedContext resultContext)
        {
            // Alt sınıflar override edebilir
            await Task.CompletedTask;
        }
        
        // Varsayılan menüleri oluşturan yardımcı metot
        private List<MenuViewModel> CreateDefaultMenuItems()
        {
            var menuItems = new List<MenuViewModel>();

            // 1. Dashboard
            menuItems.Add(new MenuViewModel
            {
                MenuID = Guid.NewGuid(),
                MenuAdi = "Dashboard",
                Icon = "fas fa-tachometer-alt",
                Controller = "Home",
                Action = "Index",
                Sira = 1,
                AltMenuler = new List<MenuViewModel>()
            });

            // 2. Tanımlamalar (Üst Menü)
            var tanimlamalarMenu = new MenuViewModel
            {
                MenuID = Guid.NewGuid(),
                MenuAdi = "Tanımlamalar",
                Icon = "fas fa-tags",
                Sira = 2,
                AltMenuler = new List<MenuViewModel>()
            };
            menuItems.Add(tanimlamalarMenu);

            tanimlamalarMenu.AltMenuler.Add(new MenuViewModel
            {
                MenuID = Guid.NewGuid(),
                MenuAdi = "Birimler",
                Icon = "fas fa-ruler",
                Controller = "Birim",
                Action = "Index",
                Sira = 1
            });

            tanimlamalarMenu.AltMenuler.Add(new MenuViewModel
            {
                MenuID = Guid.NewGuid(),
                MenuAdi = "Depolar",
                Icon = "fas fa-warehouse",
                Controller = "Depo",
                Action = "Index",
                Sira = 2
            });

            tanimlamalarMenu.AltMenuler.Add(new MenuViewModel
            {
                MenuID = Guid.NewGuid(),
                MenuAdi = "Ürün Kategorileri",
                Icon = "fas fa-sitemap",
                Controller = "UrunKategori",
                Action = "Index",
                Sira = 3
            });

            // 3. Stok Yönetimi (Üst Menü)
            var stokYonetimiMenu = new MenuViewModel
            {
                MenuID = Guid.NewGuid(),
                MenuAdi = "Stok Yönetimi",
                Icon = "fas fa-boxes",
                Sira = 3,
                AltMenuler = new List<MenuViewModel>()
            };
            menuItems.Add(stokYonetimiMenu);

            stokYonetimiMenu.AltMenuler.Add(new MenuViewModel
            {
                MenuID = Guid.NewGuid(),
                MenuAdi = "Ürünler",
                Icon = "fas fa-box-open",
                Controller = "Urun",
                Action = "Index",
                Sira = 1
            });

            stokYonetimiMenu.AltMenuler.Add(new MenuViewModel
            {
                MenuID = Guid.NewGuid(),
                MenuAdi = "Stok Kartları",
                Icon = "fas fa-clipboard-list",
                Controller = "Stok",
                Action = "Index",
                Sira = 2
            });

            stokYonetimiMenu.AltMenuler.Add(new MenuViewModel
            {
                MenuID = Guid.NewGuid(),
                MenuAdi = "Stok Durumu",
                Icon = "fas fa-chart-pie",
                Controller = "Stok",
                Action = "StokDurumu",
                Sira = 3
            });

            var stokHareketleriMenu = new MenuViewModel
            {
                MenuID = Guid.NewGuid(),
                MenuAdi = "Stok Hareketleri",
                Icon = "fas fa-exchange-alt",
                Sira = 4,
                AltMenuler = new List<MenuViewModel>()
            };
            stokYonetimiMenu.AltMenuler.Add(stokHareketleriMenu);

            stokHareketleriMenu.AltMenuler.Add(new MenuViewModel
            {
                MenuID = Guid.NewGuid(),
                MenuAdi = "Stok Giriş",
                Icon = "fas fa-plus-circle",
                Controller = "Stok",
                Action = "StokGiris",
                Sira = 1
            });

            stokHareketleriMenu.AltMenuler.Add(new MenuViewModel
            {
                MenuID = Guid.NewGuid(),
                MenuAdi = "Stok Çıkış",
                Icon = "fas fa-minus-circle",
                Controller = "Stok",
                Action = "StokCikis",
                Sira = 2
            });

            stokHareketleriMenu.AltMenuler.Add(new MenuViewModel
            {
                MenuID = Guid.NewGuid(),
                MenuAdi = "Stok Transfer",
                Icon = "fas fa-random",
                Controller = "Stok",
                Action = "StokTransfer",
                Sira = 3
            });

            stokHareketleriMenu.AltMenuler.Add(new MenuViewModel
            {
                MenuID = Guid.NewGuid(),
                MenuAdi = "Stok Sayım",
                Icon = "fas fa-tasks",
                Controller = "Stok",
                Action = "StokSayim",
                Sira = 4
            });

            stokYonetimiMenu.AltMenuler.Add(new MenuViewModel
            {
                MenuID = Guid.NewGuid(),
                MenuAdi = "Ürün Fiyatları",
                Icon = "fas fa-tags",
                Controller = "UrunFiyat",
                Action = "Index",
                Sira = 5
            });

            // 4. Cari Hesap (Üst Menü)
            var cariHesapMenu = new MenuViewModel
            {
                MenuID = Guid.NewGuid(),
                MenuAdi = "Cari Hesap",
                Icon = "fas fa-users",
                Sira = 4,
                AltMenuler = new List<MenuViewModel>()
            };
            menuItems.Add(cariHesapMenu);

            cariHesapMenu.AltMenuler.Add(new MenuViewModel
            {
                MenuID = Guid.NewGuid(),
                MenuAdi = "Tüm Cariler",
                Icon = "fas fa-address-book",
                Controller = "Cari",
                Action = "Index",
                Sira = 1
            });

            cariHesapMenu.AltMenuler.Add(new MenuViewModel
            {
                MenuID = Guid.NewGuid(),
                MenuAdi = "Müşteriler",
                Icon = "fas fa-user-tie",
                Controller = "Cari",
                Action = "Musteriler",
                Sira = 2
            });

            cariHesapMenu.AltMenuler.Add(new MenuViewModel
            {
                MenuID = Guid.NewGuid(),
                MenuAdi = "Tedarikçiler",
                Icon = "fas fa-truck",
                Controller = "Cari",
                Action = "Tedarikciler",
                Sira = 3
            });

            // 5. Belgeler (Üst Menü)
            var belgelerMenu = new MenuViewModel
            {
                MenuID = Guid.NewGuid(),
                MenuAdi = "Belgeler",
                Icon = "fas fa-file-alt",
                Sira = 5,
                AltMenuler = new List<MenuViewModel>()
            };
            menuItems.Add(belgelerMenu);

            belgelerMenu.AltMenuler.Add(new MenuViewModel
            {
                MenuID = Guid.NewGuid(),
                MenuAdi = "Faturalar",
                Icon = "fas fa-file-invoice-dollar",
                Controller = "Fatura",
                Action = "Index",
                Sira = 1
            });

            belgelerMenu.AltMenuler.Add(new MenuViewModel
            {
                MenuID = Guid.NewGuid(),
                MenuAdi = "İrsaliyeler",
                Icon = "fas fa-truck-loading",
                Controller = "Irsaliye",
                Action = "Index",
                Sira = 2
            });

            belgelerMenu.AltMenuler.Add(new MenuViewModel
            {
                MenuID = Guid.NewGuid(),
                MenuAdi = "Sözleşmeler",
                Icon = "fas fa-file-signature",
                Controller = "Sozlesme",
                Action = "Index",
                Sira = 3
            });

            belgelerMenu.AltMenuler.Add(new MenuViewModel
            {
                MenuID = Guid.NewGuid(),
                MenuAdi = "Fatura Aklama",
                Icon = "fas fa-check-double",
                Controller = "Fatura",
                Action = "Aklama",
                Sira = 4
            });

            // 6. Finans Yönetimi (Üst Menü)
            var finansMenu = new MenuViewModel
            {
                MenuID = Guid.NewGuid(),
                MenuAdi = "Finans Yönetimi",
                Icon = "fas fa-money-bill-wave",
                Sira = 6,
                AltMenuler = new List<MenuViewModel>()
            };
            menuItems.Add(finansMenu);

            finansMenu.AltMenuler.Add(new MenuViewModel
            {
                MenuID = Guid.NewGuid(),
                MenuAdi = "Kasa İşlemleri",
                Icon = "fas fa-cash-register",
                Controller = "Kasa",
                Action = "Index",
                Sira = 1
            });

            finansMenu.AltMenuler.Add(new MenuViewModel
            {
                MenuID = Guid.NewGuid(),
                MenuAdi = "Banka İşlemleri",
                Icon = "fas fa-university",
                Controller = "Banka",
                Action = "Index",
                Sira = 2
            });

            finansMenu.AltMenuler.Add(new MenuViewModel
            {
                MenuID = Guid.NewGuid(),
                MenuAdi = "Banka Hesapları",
                Icon = "fas fa-landmark",
                Controller = "Banka",
                Action = "Hesaplar",
                Sira = 3
            });

            // 7. Para Birimi Yönetimi (Üst Menü)
            var paraBirimiMenu = new MenuViewModel
            {
                MenuID = Guid.NewGuid(),
                MenuAdi = "Para Birimi Yönetimi",
                Icon = "fas fa-coins",
                Sira = 7,
                AltMenuler = new List<MenuViewModel>()
            };
            menuItems.Add(paraBirimiMenu);

            paraBirimiMenu.AltMenuler.Add(new MenuViewModel
            {
                MenuID = Guid.NewGuid(),
                MenuAdi = "Para Birimleri",
                Icon = "fas fa-dollar-sign",
                Controller = "ParaBirimi",
                Action = "Index",
                Sira = 1
            });

            paraBirimiMenu.AltMenuler.Add(new MenuViewModel
            {
                MenuID = Guid.NewGuid(),
                MenuAdi = "Döviz Kurları",
                Icon = "fas fa-exchange-alt",
                Controller = "ParaBirimi",
                Action = "Kurlar",
                Sira = 2
            });

            paraBirimiMenu.AltMenuler.Add(new MenuViewModel
            {
                MenuID = Guid.NewGuid(),
                MenuAdi = "Para Birimi İlişkileri",
                Icon = "fas fa-link",
                Controller = "ParaBirimi",
                Action = "Iliskiler",
                Sira = 3
            });

            // 8. Raporlar (Üst Menü)
            var raporlarMenu = new MenuViewModel
            {
                MenuID = Guid.NewGuid(),
                MenuAdi = "Raporlar",
                Icon = "fas fa-chart-bar",
                Sira = 8,
                AltMenuler = new List<MenuViewModel>()
            };
            menuItems.Add(raporlarMenu);

            raporlarMenu.AltMenuler.Add(new MenuViewModel
            {
                MenuID = Guid.NewGuid(),
                MenuAdi = "Genel Bakış",
                Icon = "fas fa-chart-line",
                Controller = "Rapor",
                Action = "Index",
                Sira = 1
            });

            raporlarMenu.AltMenuler.Add(new MenuViewModel
            {
                MenuID = Guid.NewGuid(),
                MenuAdi = "Stok Raporu",
                Icon = "fas fa-boxes",
                Controller = "Stok",
                Action = "StokRapor",
                Sira = 2
            });

            raporlarMenu.AltMenuler.Add(new MenuViewModel
            {
                MenuID = Guid.NewGuid(),
                MenuAdi = "Satış Raporu",
                Icon = "fas fa-chart-line",
                Controller = "Rapor",
                Action = "SatisRaporu",
                Sira = 3
            });

            raporlarMenu.AltMenuler.Add(new MenuViewModel
            {
                MenuID = Guid.NewGuid(),
                MenuAdi = "Cari Raporu",
                Icon = "fas fa-users",
                Controller = "Rapor",
                Action = "CariRaporu",
                Sira = 4
            });

            raporlarMenu.AltMenuler.Add(new MenuViewModel
            {
                MenuID = Guid.NewGuid(),
                MenuAdi = "Kasa Raporu",
                Icon = "fas fa-cash-register",
                Controller = "Rapor",
                Action = "KasaRaporu",
                Sira = 5
            });

            raporlarMenu.AltMenuler.Add(new MenuViewModel
            {
                MenuID = Guid.NewGuid(),
                MenuAdi = "Banka Raporu",
                Icon = "fas fa-university",
                Controller = "Rapor",
                Action = "BankaRaporu",
                Sira = 6
            });

            // 9. Yönetim Paneli (Üst Menü)
            var yonetimMenu = new MenuViewModel
            {
                MenuID = Guid.NewGuid(),
                MenuAdi = "Yönetim Paneli",
                Icon = "fas fa-cogs",
                Sira = 9,
                AltMenuler = new List<MenuViewModel>()
            };
            menuItems.Add(yonetimMenu);

            yonetimMenu.AltMenuler.Add(new MenuViewModel
            {
                MenuID = Guid.NewGuid(),
                MenuAdi = "Kullanıcı Yönetimi",
                Icon = "fas fa-users-cog",
                Controller = "Kullanici",
                Action = "Index",
                Sira = 1
            });

            yonetimMenu.AltMenuler.Add(new MenuViewModel
            {
                MenuID = Guid.NewGuid(),
                MenuAdi = "Rol Yönetimi",
                Icon = "fas fa-user-tag",
                Controller = "Kullanici",
                Action = "Roller",
                Sira = 2
            });

            yonetimMenu.AltMenuler.Add(new MenuViewModel
            {
                MenuID = Guid.NewGuid(),
                MenuAdi = "Menü Yönetimi",
                Icon = "fas fa-bars",
                Controller = "Menu",
                Action = "Index",
                Sira = 3
            });

            yonetimMenu.AltMenuler.Add(new MenuViewModel
            {
                MenuID = Guid.NewGuid(),
                MenuAdi = "Sistem Ayarları",
                Icon = "fas fa-sliders-h",
                Controller = "SistemAyar",
                Action = "Index",
                Sira = 4
            });

            yonetimMenu.AltMenuler.Add(new MenuViewModel
            {
                MenuID = Guid.NewGuid(),
                MenuAdi = "Sistem Logları",
                Icon = "fas fa-history",
                Controller = "SistemLog",
                Action = "Index",
                Sira = 5
            });

            yonetimMenu.AltMenuler.Add(new MenuViewModel
            {
                MenuID = Guid.NewGuid(),
                MenuAdi = "Veritabanı Yönetimi",
                Icon = "fas fa-database",
                Controller = "DbInit",
                Action = "Index",
                Sira = 6
            });
                
            return menuItems;
        }
        
        /// <summary>
        /// Başarılı işlem sonucunda mesajları göstermek için kullanılan yardımcı metot
        /// </summary>
        protected IActionResult SuccessResult(string message, object data = null, string url = null)
        {
            var result = new
            {
                success = true,
                message = message,
                data = data,
                url = url
            };

            if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
            {
                return Json(result);
            }

            if (!string.IsNullOrEmpty(url))
            {
                return Redirect(url);
            }

            TempData["SuccessMessage"] = message;
            return RedirectToAction("Index");
        }
        
        /// <summary>
        /// Hata durumunda mesajları göstermek için kullanılan yardımcı metot
        /// </summary>
        protected IActionResult ErrorResult(string message, Exception ex = null, HttpStatusCode statusCode = HttpStatusCode.BadRequest)
        {
            if (ex != null)
            {
                _logService?.LogErrorAsync("BaseController.ErrorResult", message, ex);
            }

            if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
            {
                return StatusCode((int)statusCode, new { success = false, message = message });
            }

            TempData["ErrorMessage"] = message;
            return RedirectToAction("Index");
        }
        
        /// <summary>
        /// ModelState hatalarını birleştirerek tek bir metin haline getirir
        /// </summary>
        protected string GetModelStateErrors()
        {
            return string.Join("; ", ModelState.Values
                .SelectMany(x => x.Errors)
                .Select(x => x.ErrorMessage));
        }
        
        /// <summary>
        /// API isteklerinde başarılı sonuçları JSON formatında döndürür
        /// </summary>
        protected IActionResult ApiOk(object data = null, string message = "İşlem başarılı")
        {
            return Json(new { success = true, message = message, data = data });
        }
        
        /// <summary>
        /// API isteklerinde hatalı sonuçları JSON formatında döndürür
        /// </summary>
        protected IActionResult ApiBadRequest(string message = "İşlem sırasında bir hata oluştu", object errors = null)
        {
            return StatusCode((int)HttpStatusCode.BadRequest, new { success = false, message = message, errors = errors });
        }
        
        /// <summary>
        /// Mevcut kullanıcının ID'sini döndürür
        /// </summary>
        protected Guid? GetCurrentUserId()
        {
            var userIdStr = User?.FindFirstValue(ClaimTypes.NameIdentifier);
            return !string.IsNullOrEmpty(userIdStr) && Guid.TryParse(userIdStr, out Guid userId) ? userId : (Guid?)null;
        }

        protected async Task<BaseViewModel> GetBaseViewModel()
        {
            var viewModel = new BaseViewModel();
            
            // Kullanıcı kimliği ve rol bilgilerini al
            var userIdStr = User?.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!string.IsNullOrEmpty(userIdStr) && Guid.TryParse(userIdStr, out Guid userId))
            {
                viewModel.CurrentUserId = userId;
                
                if (_userManager != null)
                {
                    var user = await _userManager.FindByIdAsync(userIdStr);
                    if (user != null)
                    {
                        viewModel.CurrentUserName = user.UserName;
                        viewModel.CurrentUserFullName = $"{user.Name} {user.Surname}".Trim();
                        viewModel.CurrentUserEmail = user.Email;
                        
                        if (_userManager != null)
                        {
                            var roles = await _userManager.GetRolesAsync(user);
                            viewModel.CurrentUserRoles = roles.ToList();
                            viewModel.IsAdmin = roles.Contains("Admin");
                        }
                    }
                }
            }
            
            // Sistem ayarlarından varsayılan değerleri al (Şirket adı, logo, vs)
            // TODO: Add settings service
            
            return viewModel;
        }

        /// <summary>
        /// İşlemin bir AJAX isteği olup olmadığını kontrol eder - Override edilebilir
        /// </summary>
        protected virtual bool IsAjaxRequest()
        {
            return Request.Headers["X-Requested-With"] == "XMLHttpRequest";
        }
    }
} 