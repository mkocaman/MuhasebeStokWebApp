using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using MuhasebeStokWebApp.Data;
using MuhasebeStokWebApp.Data.Entities;
using MuhasebeStokWebApp.Data.Repositories;
using MuhasebeStokWebApp.Services;
using MuhasebeStokWebApp.ViewModels.Menu;

namespace MuhasebeStokWebApp.Services.Menu
{
    public class MenuService : IMenuService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ApplicationDbContext _context;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly ILogService _logService;
        private readonly UserManager<IdentityUser> _userManager;

        public MenuService(
            IUnitOfWork unitOfWork,
            ApplicationDbContext context,
            RoleManager<IdentityRole> roleManager,
            ILogService logService,
            UserManager<IdentityUser> userManager)
        {
            _unitOfWork = unitOfWork;
            _context = context;
            _roleManager = roleManager;
            _logService = logService;
            _userManager = userManager;
        }

        public async Task<List<MenuViewModel>> GetMenuHierarchyAsync()
        {
            // Tüm menüleri getir
            var menuler = await _context.Menuler
                .Include(m => m.MenuRoller)
                .OrderBy(m => m.Sira)
                .ToListAsync();

            // Rol bilgilerini getir
            var roller = await _roleManager.Roles.ToDictionaryAsync(r => r.Id, r => r.Name);

            // Root menüleri bul (UstMenuId = null)
            var rootMenuler = menuler.Where(m => m.UstMenuID == null).ToList();

            // Hiyerarşik olarak view modeli oluştur
            var result = rootMenuler.Select(m => CreateMenuViewModel(m, menuler, roller, 0)).ToList();

            return result;
        }

        public async Task<List<MenuViewModel>> GetSidebarMenuByRolIdAsync(string rolId)
        {
            try
            {
                // Tüm menüleri getir (geçici çözüm olarak MenuRoller tablosu olmadığı için)
                var menuler = await _context.Menuler
                    .Where(m => m.AktifMi)
                    .OrderBy(m => m.Sira)
                    .ToListAsync();

                if (!menuler.Any())
                {
                    return new List<MenuViewModel>();
                }

                // Root menüleri bul
                var rootMenuler = menuler.Where(m => m.UstMenuID == null).ToList();

                // Rol bilgilerini getir
                var roller = await _roleManager.Roles.ToDictionaryAsync(r => r.Id, r => r.Name);

                // Hiyerarşik olarak view model oluştur
                var result = rootMenuler.Select(m => CreateSidebarMenuViewModel(m, menuler, roller, 0)).ToList();

                return result;
            }
            catch (Exception ex)
            {
                await _logService.LogErrorAsync("MenuService.GetSidebarMenuByRolIdAsync", $"Hata: {ex.Message}");
                return new List<MenuViewModel>();
            }
        }

        public async Task<Data.Entities.Menu> GetMenuByIdAsync(Guid id)
        {
            return await _context.Menuler
                .Include(m => m.UstMenu)
                .Include(m => m.AltMenuler)
                .Include(m => m.MenuRoller)
                .FirstOrDefaultAsync(m => m.MenuID == id);
        }

        public async Task<bool> AddMenuAsync(Data.Entities.Menu menu)
        {
            try
            {
                _context.Menuler.Add(menu);
                await _context.SaveChangesAsync();
                return true;
            }
            catch (Exception ex)
            {
                await _logService.LogErrorAsync("MenuService.AddMenuAsync", ex.Message + "\n" + ex.StackTrace);
                return false;
            }
        }

        public async Task<bool> UpdateMenuAsync(Data.Entities.Menu menu)
        {
            try
            {
                _context.Menuler.Update(menu);
                await _context.SaveChangesAsync();
                return true;
            }
            catch (Exception ex)
            {
                await _logService.LogErrorAsync("MenuService.UpdateMenuAsync", ex.Message + "\n" + ex.StackTrace);
                return false;
            }
        }

        public async Task<bool> DeleteMenuAsync(Guid id)
        {
            try
            {
                // Menüyü getir
                var menu = await _context.Menuler
                    .Include(m => m.AltMenuler)
                    .Include(m => m.MenuRoller)
                    .FirstOrDefaultAsync(m => m.MenuID == id);

                if (menu == null)
                {
                    return false;
                }

                // Alt menüleri varsa silme
                if (menu.AltMenuler.Any())
                {
                    throw new Exception("Bu menünün alt menüleri var. Önce alt menüleri silmelisiniz.");
                }

                // Menü-Rol ilişkilerini sil
                _context.MenuRoller.RemoveRange(menu.MenuRoller);

                // Menüyü sil
                _context.Menuler.Remove(menu);
                await _context.SaveChangesAsync();
                return true;
            }
            catch (Exception ex)
            {
                await _logService.LogErrorAsync("MenuService.DeleteMenuAsync", ex.Message + "\n" + ex.StackTrace);
                throw;
            }
        }

        public async Task<bool> AddMenuRolAsync(MenuRol menuRol)
        {
            try
            {
                _context.MenuRoller.Add(menuRol);
                await _context.SaveChangesAsync();
                return true;
            }
            catch (Exception ex)
            {
                await _logService.LogErrorAsync("MenuService.AddMenuRolAsync", ex.Message + "\n" + ex.StackTrace);
                return false;
            }
        }

        public async Task<bool> DeleteMenuRolAsync(Guid menuRolId)
        {
            try
            {
                var menuRol = await _context.MenuRoller.FindAsync(menuRolId);
                if (menuRol == null)
                {
                    return false;
                }

                _context.MenuRoller.Remove(menuRol);
                await _context.SaveChangesAsync();
                return true;
            }
            catch (Exception ex)
            {
                await _logService.LogErrorAsync("MenuService.DeleteMenuRolAsync", ex.Message + "\n" + ex.StackTrace);
                return false;
            }
        }

        public async Task<bool> InitDefaultMenusAsync()
        {
            try
            {
                // Eğer menüler zaten varsa, işlem yapma
                if (await _context.Menuler.AnyAsync())
                {
                    return false;
                }

                // Admin rolünün ID'sini al
                var adminRole = await _roleManager.FindByNameAsync("Admin");
                if (adminRole == null)
                {
                    throw new Exception("Admin rolü bulunamadı. Önce Admin rolü oluşturulmalıdır.");
                }

                // Varsayılan menüleri oluştur
                var dashboardMenu = new Data.Entities.Menu
                {
                    MenuID = Guid.NewGuid(),
                    Ad = "Ana Sayfa",
                    Icon = "fas fa-home",
                    Controller = "Home",
                    Action = "Index",
                    AktifMi = true,
                    Sira = 1
                };

                var urunlerMenu = new Data.Entities.Menu
                {
                    MenuID = Guid.NewGuid(),
                    Ad = "Ürünler",
                    Icon = "fas fa-box",
                    Controller = "Urun",
                    Action = "Index",
                    AktifMi = true,
                    Sira = 2
                };

                var stokMenu = new Data.Entities.Menu
                {
                    MenuID = Guid.NewGuid(),
                    Ad = "Stok",
                    Icon = "fas fa-warehouse",
                    Controller = "Stok",
                    Action = "Index",
                    AktifMi = true,
                    Sira = 3
                };

                var carilerMenu = new Data.Entities.Menu
                {
                    MenuID = Guid.NewGuid(),
                    Ad = "Cariler",
                    Icon = "fas fa-users",
                    Controller = "Cari",
                    Action = "Index",
                    AktifMi = true,
                    Sira = 4
                };

                var faturalarMenu = new Data.Entities.Menu
                {
                    MenuID = Guid.NewGuid(),
                    Ad = "Faturalar",
                    Icon = "fas fa-file-invoice",
                    Controller = "Fatura",
                    Action = "Index",
                    AktifMi = true,
                    Sira = 5
                };

                var ayarlarMenu = new Data.Entities.Menu
                {
                    MenuID = Guid.NewGuid(),
                    Ad = "Ayarlar",
                    Icon = "fas fa-cogs",
                    AktifMi = true,
                    Sira = 6
                };

                var menulerMenu = new Data.Entities.Menu
                {
                    MenuID = Guid.NewGuid(),
                    Ad = "Menü Yönetimi",
                    Icon = "fas fa-bars",
                    Controller = "Menu",
                    Action = "Index",
                    AktifMi = true,
                    Sira = 1,
                    UstMenuID = ayarlarMenu.MenuID
                };

                var sistemAyarlariMenu = new Data.Entities.Menu
                {
                    MenuID = Guid.NewGuid(),
                    Ad = "Sistem Ayarları",
                    Icon = "fas fa-sliders-h",
                    Controller = "SistemAyarlari",
                    Action = "Index",
                    AktifMi = true,
                    Sira = 2,
                    UstMenuID = ayarlarMenu.MenuID
                };

                // Menüleri ekle
                _context.Menuler.AddRange(
                    dashboardMenu, 
                    urunlerMenu, 
                    stokMenu, 
                    carilerMenu, 
                    faturalarMenu, 
                    ayarlarMenu, 
                    menulerMenu, 
                    sistemAyarlariMenu
                );

                // Menü-Rol ilişkilerini ekle (Tüm menüleri Admin rolüne bağla)
                var menuRoller = new List<MenuRol>
                {
                    new MenuRol { MenuRolID = Guid.NewGuid(), MenuID = dashboardMenu.MenuID, RolID = adminRole.Id },
                    new MenuRol { MenuRolID = Guid.NewGuid(), MenuID = urunlerMenu.MenuID, RolID = adminRole.Id },
                    new MenuRol { MenuRolID = Guid.NewGuid(), MenuID = stokMenu.MenuID, RolID = adminRole.Id },
                    new MenuRol { MenuRolID = Guid.NewGuid(), MenuID = carilerMenu.MenuID, RolID = adminRole.Id },
                    new MenuRol { MenuRolID = Guid.NewGuid(), MenuID = faturalarMenu.MenuID, RolID = adminRole.Id },
                    new MenuRol { MenuRolID = Guid.NewGuid(), MenuID = ayarlarMenu.MenuID, RolID = adminRole.Id },
                    new MenuRol { MenuRolID = Guid.NewGuid(), MenuID = menulerMenu.MenuID, RolID = adminRole.Id },
                    new MenuRol { MenuRolID = Guid.NewGuid(), MenuID = sistemAyarlariMenu.MenuID, RolID = adminRole.Id }
                };

                _context.MenuRoller.AddRange(menuRoller);

                await _context.SaveChangesAsync();
                return true;
            }
            catch (Exception ex)
            {
                await _logService.LogErrorAsync("MenuService.InitDefaultMenusAsync", ex.Message + "\n" + ex.StackTrace);
                return false;
            }
        }

        public async Task<bool> InitAllMenuPermissionsAsync()
        {
            try
            {
                // Mevcut tüm menüleri al
                var menuler = await _context.Menuler.ToListAsync();
                
                // Mevcut tüm rolleri al
                var roller = await _roleManager.Roles.ToListAsync();
                
                // Admin rolünü al
                var adminRole = await _roleManager.FindByNameAsync("Admin");
                if (adminRole == null)
                {
                    throw new Exception("Admin rolü bulunamadı!");
                }
                
                // Her menü için
                foreach (var menu in menuler)
                {
                    // Her rol için
                    foreach (var rol in roller)
                    {
                        // MenuRol zaten var mı kontrol et
                        var menuRolVar = await _context.MenuRoller
                            .AnyAsync(mr => mr.MenuID == menu.MenuID && mr.RolID == rol.Id);
                            
                        // Eğer yoksa, Admin rolüne tüm izinleri ekle, diğer roller için sadece görüntüleme izni ekle
                        if (!menuRolVar)
                        {
                            var menuRol = new MenuRol
                            {
                                MenuRolID = Guid.NewGuid(),
                                MenuID = menu.MenuID,
                                RolID = rol.Id
                            };
                            
                            _context.MenuRoller.Add(menuRol);
                        }
                    }
                }
                
                await _context.SaveChangesAsync();
                return true;
            }
            catch (Exception ex)
            {
                await _logService.LogErrorAsync("MenuService.InitAllMenuPermissionsAsync", ex.Message + "\n" + ex.StackTrace);
                return false;
            }
        }

        public async Task<List<MenuViewModel>> GetActiveSidebarMenusAsync(string? userId)
        {
            try
            {
                // Kullanıcının rollerini al
                var user = await _userManager.FindByIdAsync(userId ?? "");
                if (user == null)
                {
                    return new List<MenuViewModel>();
                }

                var userRoles = await _userManager.GetRolesAsync(user);
                
                // Tüm menüleri getir
                var menuler = await _context.Menuler
                    .Include(m => m.UstMenu)
                    .Include(m => m.AltMenuler)
                    .Include(m => m.MenuRoller)
                    .OrderBy(m => m.Sira)
                    .ToListAsync();
                
                // Roller sözlüğü oluştur
                var roller = new Dictionary<string, string>();
                foreach (var role in userRoles)
                {
                    var roleId = (await _roleManager.FindByNameAsync(role))?.Id;
                    if (roleId != null)
                    {
                        roller.Add(roleId, role);
                    }
                }
                
                // Kök menüleri bul
                var rootMenuler = menuler.Where(m => m.UstMenuID == null && m.AktifMi).ToList();
                
                // Hiyerarşik olarak view model oluştur
                var result = rootMenuler.Select(m => CreateSidebarMenuViewModel(m, menuler, roller, 0)).ToList();

                return result;
            }
            catch (Exception ex)
            {
                await _logService.LogErrorAsync("MenuService.GetActiveSidebarMenusAsync", ex.Message + "\n" + ex.StackTrace);
                return new List<MenuViewModel>();
            }
        }

        // Yardımcı metotlar
        private MenuViewModel CreateMenuViewModel(Data.Entities.Menu menu, List<Data.Entities.Menu> allMenus, Dictionary<string, string> roles, int level)
        {
            // Menü view model oluştur
            var viewModel = new MenuViewModel
            {
                MenuID = menu.MenuID,
                Ad = menu.Ad,
                Icon = menu.Icon,
                Url = menu.Url,
                Controller = menu.Controller,
                Action = menu.Action,
                AktifMi = menu.AktifMi,
                Sira = menu.Sira,
                UstMenuID = menu.UstMenuID,
                Level = level,
                UstMenuAdi = menu.UstMenu?.Ad
            };

            // Rol bilgilerini ekle
            if (menu.MenuRoller != null)
            {
                foreach (var menuRol in menu.MenuRoller)
                {
                    if (roles.TryGetValue(menuRol.RolID, out string roleName))
                    {
                        viewModel.Roller.Add(roleName);
                    }
                }
            }

            // Alt menüleri ekle
            var altMenuler = allMenus.Where(m => m.UstMenuID == menu.MenuID).OrderBy(m => m.Sira).ToList();
            if (altMenuler.Any())
            {
                viewModel.AltMenuler = altMenuler.Select(m => CreateMenuViewModel(m, allMenus, roles, level + 1)).ToList();
            }

            return viewModel;
        }

        private MenuViewModel CreateSidebarMenuViewModel(Data.Entities.Menu menu, List<Data.Entities.Menu> allMenus, Dictionary<string, string> roles, int level)
        {
            // Menü view model oluştur
            var viewModel = new MenuViewModel
            {
                MenuID = menu.MenuID,
                Ad = menu.Ad,
                Icon = menu.Icon,
                Url = menu.Url,
                Controller = menu.Controller,
                Action = menu.Action,
                AktifMi = menu.AktifMi,
                Sira = menu.Sira,
                UstMenuID = menu.UstMenuID,
                Level = level
            };

            // Alt menüleri ekle (rolü olanlar)
            var altMenuler = allMenus.Where(m => m.UstMenuID == menu.MenuID && m.AktifMi).OrderBy(m => m.Sira).ToList();
            if (altMenuler.Any())
            {
                viewModel.AltMenuler = altMenuler.Select(m => CreateSidebarMenuViewModel(m, allMenus, roles, level + 1)).ToList();
            }

            return viewModel;
        }
    }
} 