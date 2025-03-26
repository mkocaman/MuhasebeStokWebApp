using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using MuhasebeStokWebApp.Data;
using MuhasebeStokWebApp.Data.Entities;
using MuhasebeStokWebApp.Services;
using MuhasebeStokWebApp.Services.Interfaces;
using MuhasebeStokWebApp.ViewModels.Menu;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Extensions.Logging;

namespace MuhasebeStokWebApp.Controllers
{
    [Authorize(Roles = "Admin")]
    public class MenuController : BaseController
    {
        private readonly IMenuService _menuService;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly ILogService _logService;
        private readonly ApplicationDbContext _context;
        private readonly ILogger<MenuController> _logger;

        public MenuController(
            IMenuService menuService,
            RoleManager<IdentityRole> roleManager,
            ILogService logService,
            ApplicationDbContext context,
            UserManager<ApplicationUser> userManager,
            ILogger<MenuController> logger)
            : base(menuService, userManager, roleManager, logService)
        {
            _menuService = menuService;
            _roleManager = roleManager;
            _logService = logService;
            _context = context;
            _logger = logger;
        }

        // GET: Menu
        public async Task<IActionResult> Index()
        {
            try
            {
                var menuler = await _menuService.GetMenuHierarchyAsync();
                return View(menuler);
            }
            catch (Exception ex)
            {
                await _logService.AddLogAsync("Hata", $"Menü listesi alınırken hata oluştu: {ex.Message}", "MenuController/Index");
                return View(new List<MenuViewModel>());
            }
        }

        // GET: Menu/Details/5
        public async Task<IActionResult> Details(Guid id)
        {
            var menu = await _menuService.GetMenuByIdAsync(id);
            if (menu == null)
            {
                return NotFound();
            }

            return View(menu);
        }

        // GET: Menu/Create
        public async Task<IActionResult> Create(Guid? parentId)
        {
            try
            {
                var viewModel = new MenuCreateViewModel();
                
                if (parentId.HasValue)
                {
                    viewModel.UstMenuID = parentId.Value;
                    var parentMenu = await _menuService.GetMenuByIdAsync(parentId.Value);
                    if (parentMenu != null)
                    {
                        viewModel.UstMenuAdi = parentMenu.Ad;
                    }
                }
                
                // Rolleri ve mevcut menüleri al
                var roller = await _roleManager.Roles.ToListAsync();
                var tumMenuler = await _menuService.GetMenuHierarchyAsync();
                
                // ViewBag'e ekle
                ViewBag.Roller = roller.Select(r => new SelectListItem { Value = r.Id, Text = r.Name }).ToList();
                ViewBag.Menuler = tumMenuler.Select(m => new SelectListItem { Value = m.MenuID.ToString(), Text = m.Ad }).ToList();
                
                return View(viewModel);
            }
            catch (Exception ex)
            {
                await _logService.AddLogAsync("Hata", $"Menü oluşturma sayfası açılırken hata oluştu: {ex.Message}", "MenuController/Create");
                return RedirectToAction(nameof(Index));
            }
        }

        // POST: Menu/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(MenuCreateViewModel viewModel)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    var menu = new Menu
                    {
                        MenuID = Guid.NewGuid(),
                        Ad = viewModel.Ad,
                        Action = viewModel.Action,
                        Controller = viewModel.Controller,
                        Icon = viewModel.Icon,
                        Sira = viewModel.Sira,
                        UstMenuID = viewModel.UstMenuID,
                        Url = viewModel.Url,
                        AktifMi = viewModel.AktifMi,
                        OlusturmaTarihi = DateTime.Now
                    };
                    
                    // Menüyü ekle
                    await _menuService.AddMenuAsync(menu);
                    await _logService.AddLogAsync("Bilgi", $"{menu.Ad} adlı menü eklendi.", "MenuController/Create");
                    
                    // Menü-Rol ilişkilerini ekle
                    if (viewModel.SeciliRoller != null && viewModel.SeciliRoller.Any())
                    {
                        foreach (var rolId in viewModel.SeciliRoller)
                        {
                            var rol = await _roleManager.FindByIdAsync(rolId);
                            if (rol != null)
                            {
                                var menuRol = new MenuRol
                                {
                                    MenuRolID = Guid.NewGuid(),
                                    MenuId = menu.MenuID,
                                    RolId = rolId
                                };
                                await _menuService.AddMenuRolAsync(menuRol);
                                await _logService.AddLogAsync("Bilgi", $"{menu.Ad} menüsüne {rol.Name} rolü eklendi.", "MenuController/Create");
                            }
                        }
                    }
                    
                    return RedirectToAction(nameof(Index));
                }
                catch (Exception ex)
                {
                    await _logService.AddLogAsync("Hata", $"Menü oluşturulurken hata: {ex.Message}", "MenuController/Create");
                    ModelState.AddModelError("", $"Menü oluşturulamadı: {ex.Message}");
                }
            }
            
            // Hata durumunda rolleri ve menüleri tekrar yükle
            try
            {
                var roller = await _roleManager.Roles.ToListAsync();
                var tumMenuler = await _menuService.GetMenuHierarchyAsync();
                
                // ViewBag'e ekle
                ViewBag.Roller = roller.Select(r => new SelectListItem { Value = r.Id, Text = r.Name }).ToList();
                ViewBag.Menuler = tumMenuler.Select(m => new SelectListItem { Value = m.MenuID.ToString(), Text = m.Ad }).ToList();
            }
            catch (Exception ex)
            {
                await _logService.AddLogAsync("Hata", $"Menü oluşturma sayfası yeniden yüklenirken hata: {ex.Message}", "MenuController/Create");
            }
            
            return View(viewModel);
        }

        // GET: Menu/Edit/5
        public async Task<IActionResult> Edit(Guid id)
        {
            var menu = await _menuService.GetMenuByIdAsync(id);
            if (menu == null)
            {
                return NotFound();
            }

            var viewModel = new MenuEditViewModel
            {
                MenuID = menu.MenuID,
                Ad = menu.Ad,
                Icon = menu.Icon,
                Url = menu.Url,
                Controller = menu.Controller,
                Action = menu.Action,
                Aktif = menu.AktifMi,
                Sira = menu.Sira,
                ParentId = menu.UstMenuID,
                ParentMenu = menu.UstMenuID.HasValue ? await _menuService.GetMenuByIdAsync(menu.UstMenuID.Value) : null,
                SelectedRoleIds = menu.MenuRoller?.Select(mr => mr.RolId).ToList() ?? new List<string>()
            };

            // Tüm rolleri getir
            viewModel.Roles = _roleManager.Roles.ToList();

            // Tüm menüleri getir ve üst menü seçenekleri olarak ekle
            // Kendisi ve alt menüleri üst menü olarak seçilemesin
            var allMenus = await _context.Menuler
                .Where(m => !m.Silindi && m.MenuID != id) // Kendisi olmasın
                .OrderBy(m => m.Ad)
                .ToListAsync();

            // Alt menülerini bul
            var childMenuIds = await _context.Menuler
                .Where(m => m.UstMenuID == id)
                .Select(m => m.MenuID)
                .ToListAsync();

            // Alt menülerini ve kendisini listeden çıkar
            var filteredMenus = allMenus
                .Where(m => !childMenuIds.Contains(m.MenuID))
                .ToList();

            ViewBag.ParentMenuOptions = filteredMenus.Select(m => new 
            {
                Value = m.MenuID.ToString(),
                Text = m.Ad
            }).ToList();

            return View(viewModel);
        }

        // POST: Menu/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(Guid id, MenuEditViewModel viewModel)
        {
            // Model durumunu detaylı olarak logla
            await _logService.LogInfoAsync("MenuController.Edit", $"Edit metodu çağrıldı. MenuID: {id}, Model geçerli mi: {ModelState.IsValid}");
            
            if (viewModel == null)
            {
                await _logService.LogErrorAsync("MenuController.Edit", "ViewModel null olarak gönderildi");
                TempData["ErrorMessage"] = "Geçersiz form verisi.";
                return RedirectToAction("Index");
            }
            
            if (id != viewModel.MenuID)
            {
                await _logService.LogErrorAsync("MenuController.Edit", $"URL'deki ID ({id}) ile viewModel.MenuID ({viewModel.MenuID}) eşleşmiyor.");
                return NotFound();
            }

            await _logService.LogInfoAsync("MenuController.Edit", $"Gelen form verileri: MenuID={viewModel.MenuID}, Ad={viewModel.Ad}, Controller={viewModel.Controller}, Action={viewModel.Action}");

            // Model state içindeki tüm hataları logla
            if (!ModelState.IsValid)
            {
                await _logService.LogInfoAsync("MenuController.Edit", "ModelState geçerli değil, hatalar:");
                foreach (var key in ModelState.Keys)
                {
                    var state = ModelState[key];
                    if (state.Errors.Count > 0)
                    {
                        await _logService.LogInfoAsync("MenuController.Edit", $"Alan: {key}, Hatalar: {string.Join(", ", state.Errors.Select(e => e.ErrorMessage))}");
                    }
                }
                
                // Menü düzenleme işlemi için kritik olmayan model doğrulama hatalarını temizle
                ModelState.Clear(); // Tüm hata durumunu temizle
            }
            
            try
            {
                await _logService.LogInfoAsync("MenuController.Edit", $"Menü güncelleniyor: {viewModel.Ad}, ID: {viewModel.MenuID}");
                
                // Mevcut menüyü getir
                var existingMenu = await _menuService.GetMenuByIdAsync(id);
                if (existingMenu == null)
                {
                    await _logService.LogErrorAsync("MenuController.Edit", $"MenuID: {id} için menü bulunamadı.");
                    return NotFound();
                }
                
                await _logService.LogInfoAsync("MenuController.Edit", $"Mevcut menü bulundu: {existingMenu.Ad}, OlusturmaTarihi: {existingMenu.OlusturmaTarihi}");
                
                // Güncellenen menü verileri
                var menu = new Menu
                {
                    MenuID = viewModel.MenuID,
                    Ad = viewModel.Ad,
                    Icon = viewModel.Icon,
                    Url = viewModel.Url ?? "#", // Null değer için varsayılan ekle
                    Controller = viewModel.Controller ?? "", // Null kontrolü
                    Action = viewModel.Action ?? "", // Null kontrolü
                    AktifMi = viewModel.Aktif,
                    Sira = viewModel.Sira,
                    UstMenuID = viewModel.ParentId,
                    OlusturmaTarihi = existingMenu.OlusturmaTarihi,  // Mevcut oluşturma tarihini koru
                    GuncellemeTarihi = DateTime.Now
                };

                await _logService.LogInfoAsync("MenuController.Edit", $"Menü nesnesi güncellendi, UpdateMenuAsync metodunu çağırıyor");
                bool menuUpdated = await _menuService.UpdateMenuAsync(menu);
                
                if (!menuUpdated)
                {
                    await _logService.LogErrorAsync("MenuController.Edit", "Menü güncelleme başarısız oldu, MenuService.UpdateMenuAsync false döndü");
                    throw new Exception("Menü güncellenirken bir hata oluştu. Lütfen sistem loglarını kontrol edin.");
                }

                await _logService.LogInfoAsync("MenuController.Edit", $"Menü başarıyla güncellendi. MenuID: {menu.MenuID}");
                
                // Mevcut rol-menü ilişkilerini sil ve yenilerini ekle
                await _context.Database.ExecuteSqlRawAsync($"DELETE FROM \"MenuRoller\" WHERE \"MenuID\" = '{id}'");
                await _logService.LogInfoAsync("MenuController.Edit", $"Menü ID: {id} için mevcut tüm rol ilişkileri silindi.");
                
                // Yeni rol-menü ilişkilerini ekle
                int basariliRolSayisi = 0;
                if (viewModel.SelectedRoleIds != null && viewModel.SelectedRoleIds.Any())
                {
                    await _logService.LogInfoAsync("MenuController.Edit", $"{viewModel.SelectedRoleIds.Count} adet rol ilişkisi eklenecek.");
                    
                    foreach (var roleId in viewModel.SelectedRoleIds)
                    {
                        var menuRol = new MenuRol
                        {
                            MenuRolID = Guid.NewGuid(),
                            MenuId = viewModel.MenuID,
                            RolId = roleId,
                            OlusturmaTarihi = DateTime.Now
                        };
                        
                        await _logService.LogInfoAsync("MenuController.Edit", $"Menü-Rol ilişkisi ekleniyor: RoleId={roleId}");
                        bool roleAdded = await _menuService.AddMenuRolAsync(menuRol);
                        
                        if (!roleAdded)
                        {
                            await _logService.LogWarningAsync("MenuController.Edit", $"RoleId: {roleId} için menü-rol ilişkisi eklenemedi.");
                        }
                        else
                        {
                            basariliRolSayisi++;
                        }
                    }
                    
                    await _logService.LogInfoAsync("MenuController.Edit", $"Toplam {basariliRolSayisi}/{viewModel.SelectedRoleIds.Count} rol ilişkisi başarıyla eklendi.");
                }
                else
                {
                    // Varsayılan olarak admin rolünü ekle
                    try {
                        var adminRole = await _roleManager.FindByNameAsync("Admin");
                        if (adminRole != null)
                        {
                            var menuRol = new MenuRol
                            {
                                MenuRolID = Guid.NewGuid(),
                                MenuId = viewModel.MenuID,
                                RolId = adminRole.Id,
                                OlusturmaTarihi = DateTime.Now
                            };
                            
                            await _logService.LogInfoAsync("MenuController.Edit", $"Hiç rol seçilmedi, varsayılan olarak Admin rolü ekleniyor: RoleId={adminRole.Id}");
                            bool roleAdded = await _menuService.AddMenuRolAsync(menuRol);
                            if (roleAdded) {
                                basariliRolSayisi = 1;
                            }
                        }
                    }
                    catch (Exception ex) {
                        await _logService.LogWarningAsync("MenuController.Edit", $"Varsayılan Admin rolü eklenirken hata: {ex.Message}");
                    }
                }

                TempData["SuccessMessage"] = $"Menü başarıyla güncellendi. ({basariliRolSayisi} rol ilişkisi eklendi)";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                await _logService.LogErrorAsync("MenuController.Edit", $"Menü güncellenirken hata: {ex.Message}\nStack trace: {ex.StackTrace}");
                TempData["ErrorMessage"] = $"Menü güncellenirken bir hata oluştu: {ex.Message}";
                ModelState.AddModelError("", ex.Message);
            }

            // Tüm rolleri getir
            viewModel.Roles = _roleManager.Roles.ToList();
            
            // Üst menü bilgisini getir
            if (viewModel.ParentId.HasValue)
            {
                viewModel.ParentMenu = await _menuService.GetMenuByIdAsync(viewModel.ParentId.Value);
            }

            // Tüm menüleri getir
            var allMenus = await _context.Menuler
                .Where(m => !m.Silindi && m.MenuID != id)
                .OrderBy(m => m.Ad)
                .ToListAsync();

            // Alt menülerini bul
            var childMenuIds = await _context.Menuler
                .Where(m => m.UstMenuID == id)
                .Select(m => m.MenuID)
                .ToListAsync();

            // Alt menülerini ve kendisini listeden çıkar
            var filteredMenus = allMenus
                .Where(m => !childMenuIds.Contains(m.MenuID))
                .ToList();

            ViewBag.ParentMenuOptions = filteredMenus.Select(m => new 
            {
                Value = m.MenuID.ToString(),
                Text = m.Ad
            }).ToList();

            // Hata durumunda aynı view'a dön
            return View(viewModel);
        }

        // GET: Menu/Delete/5
        public async Task<IActionResult> Delete(Guid id)
        {
            var menu = await _menuService.GetMenuByIdAsync(id);
            if (menu == null)
            {
                return NotFound();
            }

            return View(menu);
        }

        // POST: Menu/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(Guid id)
        {
            try
            {
                await _menuService.DeleteMenuAsync(id);
                TempData["SuccessMessage"] = "Menü başarıyla silindi.";
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "Menü silinirken bir hata oluştu: " + ex.Message;
            }

            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        public async Task<IActionResult> InitDefaults()
        {
            try
            {
                await _logService.LogInfoAsync("MenuController.InitDefaults", "Varsayılan menüler oluşturulmaya başlanıyor...");
                
                // Veritabanı bağlantısını kontrol et
                try {
                    await _context.Database.CanConnectAsync();
                    await _logService.LogInfoAsync("MenuController.InitDefaults", "Veritabanına başarıyla bağlanıldı.");
                    
                    // Veritabanı tipini kontrol et
                    var dbType = _context.Database.ProviderName;
                    await _logService.LogInfoAsync("MenuController.InitDefaults", $"Veritabanı sağlayıcısı: {dbType}");
                }
                catch (Exception ex) {
                    await _logService.LogErrorAsync("MenuController.InitDefaults", $"Veritabanına bağlanırken hata: {ex.Message}");
                    throw;
                }
                
                // Kullanıcıya uyarı sayfası göster
                TempData["InitMenuMessage"] = "DİKKAT: Bu işlemi onaylarsanız, mevcut tüm menüler silinecek ve varsayılan menüler yeniden oluşturulacaktır. Devam etmek istiyor musunuz?";
                return View("InitDefaultsConfirm");
            }
            catch (Exception ex)
            {
                await _logService.LogErrorAsync("MenuController.InitDefaults", $"Hata: {ex.Message}\n{ex.StackTrace}");
                TempData["Error"] = "Varsayılan menüler oluşturulurken bir hata oluştu: " + ex.Message;
                return RedirectToAction("Index");
            }
        }

        [HttpPost]
        [Route("Menu/InitDefaultsConfirm")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> InitDefaultsConfirm(bool confirm)
        {
            if (!confirm)
            {
                TempData["Info"] = "Varsayılan menü oluşturma işlemi iptal edildi.";
                return RedirectToAction("Index");
            }
            
            try
            {
                await _logService.LogInfoAsync("MenuController.InitDefaultsConfirm", "Kullanıcı varsayılan menüleri oluşturmayı onayladı.");
                
                // Menu Service'deki metodu çağır
                await _logService.LogInfoAsync("MenuController.InitDefaultsConfirm", "InitDefaultMenusAsync metodu çağrılıyor...");
                bool result = await _menuService.InitDefaultMenusAsync();
                
                // Oluşturma sonrası menü sayısını kontrol et
                try {
                    var menuSayisi = await _context.Menuler.CountAsync();
                    var menuRolSayisi = await _context.MenuRoller.CountAsync();
                    await _logService.LogInfoAsync("MenuController.InitDefaultsConfirm", 
                        $"İşlem sonrası durum - Veritabanında {menuSayisi} adet menü ve {menuRolSayisi} adet menü-rol ilişkisi bulunuyor.");
                }
                catch (Exception ex) {
                    await _logService.LogErrorAsync("MenuController.InitDefaultsConfirm", $"Menü sayısı kontrol edilirken hata: {ex.Message}");
                }
                
                if (result)
                {
                    TempData["Success"] = "Varsayılan menüler başarıyla oluşturuldu.";
                    await _logService.LogInfoAsync("MenuController.InitDefaultsConfirm", "Varsayılan menüler başarıyla oluşturuldu.");
                    return RedirectToAction("Index", "Menu");
                }
                else
                {
                    TempData["Error"] = "Varsayılan menüler oluşturulurken bir hata oluştu.";
                    await _logService.LogErrorAsync("MenuController.InitDefaultsConfirm", "Varsayılan menüler oluşturulamadı.");
                    return RedirectToAction("Index", "Menu");
                }
            }
            catch (Exception ex)
            {
                await _logService.LogErrorAsync("MenuController.InitDefaultsConfirm", $"Hata: {ex.Message}\n{ex.StackTrace}");
                if (ex.InnerException != null)
                {
                    await _logService.LogErrorAsync("MenuController.InitDefaultsConfirm", $"İç Hata: {ex.InnerException.Message}");
                }
                // Kullanıcıya daha açıklayıcı bir hata mesajı gösterilecek
                TempData["Error"] = $"Varsayılan menüler oluşturulurken bir hata oluştu: {ex.Message}";
                if (ex.InnerException != null && ex.InnerException.Message.Contains("NOT NULL constraint failed: Menuler.Icon"))
                {
                    TempData["Error"] = "Menü oluşturulurken ikon alanları boş bırakılamaz. Lütfen destek ekibiyle iletişime geçin.";
                }
                return RedirectToAction("Index");
            }
        }

        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> InitDefaultMenus()
        {
            try
            {
                try
                {
                    _logger.LogInformation("Varsayılan menüler oluşturulmaya başlanıyor...");
                    bool result = await _menuService.InitDefaultMenusAsync();
                    
                    if (result)
                    {
                        _logger.LogInformation("Varsayılan menüler başarıyla oluşturuldu.");
                        TempData["SuccessMessage"] = "Varsayılan menüler başarıyla oluşturuldu.";
                        await _logService.Log("Varsayılan menüler başarıyla oluşturuldu", Enums.LogTuru.Bilgi);
                    }
                    else
                    {
                        _logger.LogError("Varsayılan menüler oluşturulamadı.");
                        TempData["ErrorMessage"] = "Varsayılan menüler oluşturulurken bir hata oluştu.";
                        await _logService.Log("Varsayılan menüler oluşturulurken bir hata oluştu", Enums.LogTuru.Hata);
                    }
                    return RedirectToAction("Index");
                }
                catch (Exception ex)
                {
                    string errorMessage = $"Varsayılan menüler oluşturulurken bir hata oluştu: {ex.Message}";
                    if (ex.InnerException != null)
                    {
                        errorMessage += $" - İç Hata: {ex.InnerException.Message}";
                    }
                    
                    _logger.LogError(ex, "Varsayılan menüler oluşturulurken hata: {Message}", errorMessage);
                    TempData["ErrorMessage"] = errorMessage;
                    await _logService.Log("Varsayılan menüler oluşturulurken hata", Enums.LogTuru.Hata, ex.ToString());
                    
                    // Hata detaylarını konsola yazdıralım
                    Console.WriteLine($"HATA: {errorMessage}");
                    Console.WriteLine($"STACK TRACE: {ex.StackTrace}");
                    
                    return RedirectToAction("Index");
                }
            }
            catch (Exception ex)
            {
                // En üst seviye hata yakalama - diğer tüm hatalar için yedek
                string detailedError = $"Kritik hata: {ex.Message}";
                
                if (ex.InnerException != null)
                {
                    detailedError += $"\nİç hata: {ex.InnerException.Message}";
                }
                
                _logger.LogCritical(ex, "Menü işlemi sırasında kritik hata: {Error}", detailedError);
                TempData["ErrorMessage"] = "Beklenmedik bir hata oluştu. Sistem yöneticisine başvurun.";
                await _logService.Log("Menü işlemi sırasında kritik hata", Enums.LogTuru.Hata, detailedError);
                
                Console.WriteLine($"KRİTİK HATA: {detailedError}");
                Console.WriteLine($"STACK TRACE: {ex.StackTrace}");
                
                return RedirectToAction("Index");
            }
        }
    }
} 