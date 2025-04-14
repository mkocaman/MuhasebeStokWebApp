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
using MuhasebeStokWebApp.Services.Interfaces;
using Microsoft.Extensions.Logging;
using Microsoft.Data.SqlClient;

namespace MuhasebeStokWebApp.Services.Menu
{
    public class MenuService : IMenuService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ApplicationDbContext _context;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly ILogService _logService;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ILogger<MenuService> _logger;

        public MenuService(
            IUnitOfWork unitOfWork,
            ApplicationDbContext context,
            RoleManager<IdentityRole> roleManager,
            ILogService logService,
            UserManager<ApplicationUser> userManager,
            ILogger<MenuService> logger)
        {
            _unitOfWork = unitOfWork;
            _context = context;
            _roleManager = roleManager;
            _logService = logService;
            _userManager = userManager;
            _logger = logger;
        }

        public async Task<List<MenuViewModel>> GetMenuHierarchyAsync()
        {
            // Tüm menüleri getir
            var menuler = await _context.Menuler
                .Include(m => m.MenuRoller)
                .Where(m => !m.Silindi)  // Silinmemiş menüleri getir
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

                // Kullanıcı rol IDlerini HashSet olarak alalım
                var userRoleIds = new HashSet<string>(roller.Keys);
                
                // Admin kontrolü
                bool isAdmin = roller.Values.Any(r => r == "Admin");
                
                // Hiyerarşik olarak view model oluştur
                var result = rootMenuler.Select(m => CreateSidebarMenuViewModel(m, menuler, isAdmin, userRoleIds, 0)).ToList();

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
                // Menu nesnesinin gerekli alanlarının kontrolü
                if (menu == null)
                {
                    await _logService.LogErrorAsync("MenuService.AddMenuAsync", "Menu nesnesi null olamaz");
                    return false;
                }

                if (menu.MenuID == Guid.Empty)
                {
                    menu.MenuID = Guid.NewGuid();
                    await _logService.LogInfoAsync("MenuService.AddMenuAsync", $"Yeni MenuID oluşturuldu: {menu.MenuID}");
                }

                if (string.IsNullOrEmpty(menu.Ad))
                {
                    await _logService.LogErrorAsync("MenuService.AddMenuAsync", "Menü adı boş olamaz");
                    return false;
                }

                // Oluşturma tarihi kontrolü
                if (menu.OlusturmaTarihi == default(DateTime))
                {
                    menu.OlusturmaTarihi = DateTime.Now;
                    await _logService.LogInfoAsync("MenuService.AddMenuAsync", $"Oluşturma tarihi ayarlandı: {menu.OlusturmaTarihi}");
                }

                await _logService.LogInfoAsync("MenuService.AddMenuAsync", $"Menü ekleniyor: {menu.Ad} (ID: {menu.MenuID})");
                
                _context.Menuler.Add(menu);
                await _context.SaveChangesAsync();
                
                await _logService.LogInfoAsync("MenuService.AddMenuAsync", $"Menü başarıyla eklendi: {menu.Ad} (ID: {menu.MenuID})");
                return true;
            }
            catch (DbUpdateException dbEx)
            {
                var innerException = dbEx.InnerException?.Message ?? "Bilinmeyen veritabanı hatası";
                await _logService.LogErrorAsync("MenuService.AddMenuAsync", $"Veritabanına kaydederken hata: {innerException}, SQL: {dbEx.ToString()}");
                
                // DB hatalarını daha detaylı logla
                if (dbEx.InnerException != null)
                {
                    await _logService.LogErrorAsync("MenuService.AddMenuAsync", $"İç hata detayı: {dbEx.InnerException.StackTrace}");
                }
                
                return false;
            }
            catch (Exception ex)
            {
                await _logService.LogErrorAsync("MenuService.AddMenuAsync", $"Menü eklenirken hata: {ex.Message}, Stack: {ex.StackTrace}");
                return false;
            }
        }

        public async Task<bool> UpdateMenuAsync(Data.Entities.Menu menu)
        {
            try
            {
                // Menu nesnesinin gerekli alanlarının kontrolü
                if (menu == null)
                {
                    await _logService.LogErrorAsync("MenuService.UpdateMenuAsync", "Menu nesnesi null olamaz");
                    return false;
                }

                if (menu.MenuID == Guid.Empty)
                {
                    await _logService.LogErrorAsync("MenuService.UpdateMenuAsync", "Menü ID geçerli değil");
                    return false;
                }

                if (string.IsNullOrEmpty(menu.Ad))
                {
                    await _logService.LogErrorAsync("MenuService.UpdateMenuAsync", "Menü adı boş olamaz");
                    return false;
                }

                await _logService.LogInfoAsync("MenuService.UpdateMenuAsync", $"Menü güncelleniyor: {menu.Ad} (ID: {menu.MenuID})");

                // Menünün veritabanında var olup olmadığını kontrol et
                var existingMenu = await _context.Menuler.FindAsync(menu.MenuID);
                if (existingMenu == null)
                {
                    await _logService.LogErrorAsync("MenuService.UpdateMenuAsync", $"Menü ID: {menu.MenuID} veritabanında bulunamadı");
                    return false;
                }

                // Güncelleme tarihi kontrolü
                menu.GuncellemeTarihi = DateTime.Now;
                await _logService.LogInfoAsync("MenuService.UpdateMenuAsync", $"Güncelleme tarihi ayarlandı: {menu.GuncellemeTarihi}");
                
                // Eğer OlusturmaTarihi boşsa, mevcut değeri koru
                if (menu.OlusturmaTarihi == default(DateTime))
                {
                    menu.OlusturmaTarihi = existingMenu.OlusturmaTarihi;
                    await _logService.LogInfoAsync("MenuService.UpdateMenuAsync", $"Mevcut oluşturma tarihi korundu: {menu.OlusturmaTarihi}");
                }

                // Entity state'i güncelleyelim
                _context.Entry(existingMenu).State = EntityState.Detached;
                _context.Entry(menu).State = EntityState.Modified;

                await _context.SaveChangesAsync();
                await _logService.LogInfoAsync("MenuService.UpdateMenuAsync", $"Menü başarıyla güncellendi: {menu.Ad} (ID: {menu.MenuID})");
                return true;
            }
            catch (DbUpdateException dbEx)
            {
                var innerException = dbEx.InnerException?.Message ?? "Bilinmeyen veritabanı hatası";
                await _logService.LogErrorAsync("MenuService.UpdateMenuAsync", $"Veritabanına kaydederken hata: {innerException}, SQL: {dbEx.ToString()}");
                
                // DB hatalarını daha detaylı logla
                if (dbEx.InnerException != null)
                {
                    await _logService.LogErrorAsync("MenuService.UpdateMenuAsync", $"İç hata detayı: {dbEx.InnerException.StackTrace}");
                }
                
                return false;
            }
            catch (Exception ex)
            {
                await _logService.LogErrorAsync("MenuService.UpdateMenuAsync", $"Menü güncellenirken hata: {ex.Message}, Stack: {ex.StackTrace}");
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
                _logger.LogInformation("Varsayılan menüleri oluşturma işlemi başlatıldı");
                
                // Mevcut menüleri temizle
                try
                {
                    var existingMenus = await _unitOfWork.MenuRepository.GetAllAsync();
                    if (existingMenus != null && existingMenus.Any())
                    {
                        _logger.LogInformation("Mevcut {MenuCount} menü temizleniyor", existingMenus.Count());
                        foreach (var menu in existingMenus)
                        {
                            await _unitOfWork.MenuRepository.RemoveAsync(menu);
                        }
                        await _unitOfWork.SaveAsync();
                        _logger.LogInformation("Mevcut menüler başarıyla temizlendi");
                    }
                    else
                    {
                        _logger.LogInformation("Temizlenecek mevcut menü bulunamadı");
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Mevcut menüleri temizlerken hata oluştu, işleme devam ediliyor: {Message}", ex.Message);
                }

                try 
                {
                    await _logService.LogInfoAsync("MenuService.InitDefaultMenusAsync", "Varsayılan menüler oluşturuluyor...");
                } 
                catch (Exception logEx)
                {
                    // Log hatası olsa bile işleme devam et
                    _logger.LogWarning(logEx, "Log kaydetme işlemi sırasında hata oluştu, işleme devam ediliyor: {Message}", logEx.Message);
                }
                
                // Admin rolünün ID'sini al
                var adminRole = await _roleManager.FindByNameAsync("Admin");
                if (adminRole == null)
                {
                    _logger.LogError("Admin rolü bulunamadı");
                    return false;
                }

                // Diğer rolleri de al (varsa)
                var managerRole = await _roleManager.FindByNameAsync("Manager");
                var accountantRole = await _roleManager.FindByNameAsync("Accountant");

                // Transaction içinde menüleri oluştur
                using (var transaction = await _context.Database.BeginTransactionAsync())
                {
                    try
                    {
                        // 1. Ana Sayfa Menüsü
                        var anaSayfaMenu = CreateMenu(
                            "Ana Sayfa", 
                            "fas fa-home", 
                            "Home", 
                            "Index", 
                            1, 
                            null, 
                            "/Home/Index"
                        );
                        await _context.Menuler.AddAsync(anaSayfaMenu);

                        // Ana sayfa için tüm rollere izin ver
                        await _context.MenuRoller.AddAsync(new MenuRol { MenuRolID = Guid.NewGuid(), MenuId = anaSayfaMenu.MenuID, RolId = adminRole.Id });
                        if (managerRole != null)
                            await _context.MenuRoller.AddAsync(new MenuRol { MenuRolID = Guid.NewGuid(), MenuId = anaSayfaMenu.MenuID, RolId = managerRole.Id });
                        if (accountantRole != null)
                            await _context.MenuRoller.AddAsync(new MenuRol { MenuRolID = Guid.NewGuid(), MenuId = anaSayfaMenu.MenuID, RolId = accountantRole.Id });

                        // 2. Cari Hesaplar Menüsü
                        var cariHesaplarMenu = CreateMenu(
                            "Cari Hesaplar", 
                            "fas fa-users", 
                            "", 
                            "", 
                            2, 
                            null, 
                            "#"
                        );
                        await _context.Menuler.AddAsync(cariHesaplarMenu);
                        await _context.MenuRoller.AddAsync(new MenuRol { MenuRolID = Guid.NewGuid(), MenuId = cariHesaplarMenu.MenuID, RolId = adminRole.Id });
                        if (managerRole != null)
                            await _context.MenuRoller.AddAsync(new MenuRol { MenuRolID = Guid.NewGuid(), MenuId = cariHesaplarMenu.MenuID, RolId = managerRole.Id });
                        if (accountantRole != null)
                            await _context.MenuRoller.AddAsync(new MenuRol { MenuRolID = Guid.NewGuid(), MenuId = cariHesaplarMenu.MenuID, RolId = accountantRole.Id });

                        // Cari Hesaplar Alt Menüleri
                        var cariKartListesi = CreateMenu(
                            "Cari Kart Listesi", 
                            "fas fa-list", 
                            "Cari", 
                            "Index", 
                            1, 
                            cariHesaplarMenu.MenuID, 
                            "/Cari/Index"
                        );
                        await _context.Menuler.AddAsync(cariKartListesi);
                        await _context.MenuRoller.AddAsync(new MenuRol { MenuRolID = Guid.NewGuid(), MenuId = cariKartListesi.MenuID, RolId = adminRole.Id });
                        if (managerRole != null)
                            await _context.MenuRoller.AddAsync(new MenuRol { MenuRolID = Guid.NewGuid(), MenuId = cariKartListesi.MenuID, RolId = managerRole.Id });
                        if (accountantRole != null)
                            await _context.MenuRoller.AddAsync(new MenuRol { MenuRolID = Guid.NewGuid(), MenuId = cariKartListesi.MenuID, RolId = accountantRole.Id });

                        var yeniCariKart = CreateMenu(
                            "Yeni Cari Kart", 
                            "fas fa-plus", 
                            "Cari", 
                            "Create", 
                            2, 
                            cariHesaplarMenu.MenuID, 
                            "/Cari/Create"
                        );
                        await _context.Menuler.AddAsync(yeniCariKart);
                        await _context.MenuRoller.AddAsync(new MenuRol { MenuRolID = Guid.NewGuid(), MenuId = yeniCariKart.MenuID, RolId = adminRole.Id });
                        if (managerRole != null)
                            await _context.MenuRoller.AddAsync(new MenuRol { MenuRolID = Guid.NewGuid(), MenuId = yeniCariKart.MenuID, RolId = managerRole.Id });
                        if (accountantRole != null)
                            await _context.MenuRoller.AddAsync(new MenuRol { MenuRolID = Guid.NewGuid(), MenuId = yeniCariKart.MenuID, RolId = accountantRole.Id });

                        var cariHareketRaporu = CreateMenu(
                            "Cari Hareket Raporu", 
                            "fas fa-chart-line", 
                            "Rapor", 
                            "CariHareketler", 
                            3, 
                            cariHesaplarMenu.MenuID, 
                            "/Rapor/CariHareketler"
                        );
                        await _context.Menuler.AddAsync(cariHareketRaporu);
                        await _context.MenuRoller.AddAsync(new MenuRol { MenuRolID = Guid.NewGuid(), MenuId = cariHareketRaporu.MenuID, RolId = adminRole.Id });
                        if (managerRole != null)
                            await _context.MenuRoller.AddAsync(new MenuRol { MenuRolID = Guid.NewGuid(), MenuId = cariHareketRaporu.MenuID, RolId = managerRole.Id });
                        if (accountantRole != null)
                            await _context.MenuRoller.AddAsync(new MenuRol { MenuRolID = Guid.NewGuid(), MenuId = cariHareketRaporu.MenuID, RolId = accountantRole.Id });

                        // 3. Stok & Ürün Yönetimi Menüsü
                        var stokUrunYonetimiMenu = CreateMenu(
                            "Stok & Ürün Yönetimi", 
                            "fas fa-boxes", 
                            "", 
                            "", 
                            3, 
                            null, 
                            "#"
                        );
                        await _context.Menuler.AddAsync(stokUrunYonetimiMenu);
                        await _context.MenuRoller.AddAsync(new MenuRol { MenuRolID = Guid.NewGuid(), MenuId = stokUrunYonetimiMenu.MenuID, RolId = adminRole.Id });
                        if (managerRole != null)
                            await _context.MenuRoller.AddAsync(new MenuRol { MenuRolID = Guid.NewGuid(), MenuId = stokUrunYonetimiMenu.MenuID, RolId = managerRole.Id });

                        // Stok & Ürün Yönetimi Alt Menüleri
                        var urunListesi = CreateMenu(
                            "Ürün Listesi", 
                            "fas fa-box", 
                            "Urun", 
                            "Index", 
                            1, 
                            stokUrunYonetimiMenu.MenuID, 
                            "/Urun/Index"
                        );
                        await _context.Menuler.AddAsync(urunListesi);
                        await _context.MenuRoller.AddAsync(new MenuRol { MenuRolID = Guid.NewGuid(), MenuId = urunListesi.MenuID, RolId = adminRole.Id });
                        if (managerRole != null)
                            await _context.MenuRoller.AddAsync(new MenuRol { MenuRolID = Guid.NewGuid(), MenuId = urunListesi.MenuID, RolId = managerRole.Id });

                        var yeniUrun = CreateMenu(
                            "Yeni Ürün", 
                            "fas fa-plus", 
                            "Urun", 
                            "Create", 
                            2, 
                            stokUrunYonetimiMenu.MenuID, 
                            "/Urun/Create"
                        );
                        await _context.Menuler.AddAsync(yeniUrun);
                        await _context.MenuRoller.AddAsync(new MenuRol { MenuRolID = Guid.NewGuid(), MenuId = yeniUrun.MenuID, RolId = adminRole.Id });
                        if (managerRole != null)
                            await _context.MenuRoller.AddAsync(new MenuRol { MenuRolID = Guid.NewGuid(), MenuId = yeniUrun.MenuID, RolId = managerRole.Id });

                        var urunKategorileri = CreateMenu(
                            "Ürün Kategorileri", 
                            "fas fa-tags", 
                            "UrunKategori", 
                            "Index", 
                            3, 
                            stokUrunYonetimiMenu.MenuID, 
                            "/UrunKategori/Index"
                        );
                        await _context.Menuler.AddAsync(urunKategorileri);
                        await _context.MenuRoller.AddAsync(new MenuRol { MenuRolID = Guid.NewGuid(), MenuId = urunKategorileri.MenuID, RolId = adminRole.Id });
                        if (managerRole != null)
                            await _context.MenuRoller.AddAsync(new MenuRol { MenuRolID = Guid.NewGuid(), MenuId = urunKategorileri.MenuID, RolId = managerRole.Id });

                        var depoListesi = CreateMenu(
                            "Depo Listesi", 
                            "fas fa-warehouse", 
                            "Depo", 
                            "Index", 
                            4, 
                            stokUrunYonetimiMenu.MenuID, 
                            "/Depo/Index"
                        );
                        await _context.Menuler.AddAsync(depoListesi);
                        await _context.MenuRoller.AddAsync(new MenuRol { MenuRolID = Guid.NewGuid(), MenuId = depoListesi.MenuID, RolId = adminRole.Id });
                        if (managerRole != null)
                            await _context.MenuRoller.AddAsync(new MenuRol { MenuRolID = Guid.NewGuid(), MenuId = depoListesi.MenuID, RolId = managerRole.Id });

                        var birimTanimlari = CreateMenu(
                            "Birim Tanımları", 
                            "fas fa-ruler", 
                            "Birim", 
                            "Index", 
                            5, 
                            stokUrunYonetimiMenu.MenuID, 
                            "/Birim/Index"
                        );
                        await _context.Menuler.AddAsync(birimTanimlari);
                        await _context.MenuRoller.AddAsync(new MenuRol { MenuRolID = Guid.NewGuid(), MenuId = birimTanimlari.MenuID, RolId = adminRole.Id });
                        if (managerRole != null)
                            await _context.MenuRoller.AddAsync(new MenuRol { MenuRolID = Guid.NewGuid(), MenuId = birimTanimlari.MenuID, RolId = managerRole.Id });

                        var stokHareketleri = CreateMenu(
                            "Stok Hareketleri", 
                            "fas fa-exchange-alt", 
                            "Stok", 
                            "Index", 
                            6, 
                            stokUrunYonetimiMenu.MenuID, 
                            "/Stok/Index"
                        );
                        await _context.Menuler.AddAsync(stokHareketleri);
                        await _context.MenuRoller.AddAsync(new MenuRol { MenuRolID = Guid.NewGuid(), MenuId = stokHareketleri.MenuID, RolId = adminRole.Id });
                        if (managerRole != null)
                            await _context.MenuRoller.AddAsync(new MenuRol { MenuRolID = Guid.NewGuid(), MenuId = stokHareketleri.MenuID, RolId = managerRole.Id });
                        if (accountantRole != null)
                            await _context.MenuRoller.AddAsync(new MenuRol { MenuRolID = Guid.NewGuid(), MenuId = stokHareketleri.MenuID, RolId = accountantRole.Id });

                        var urunFiyatlari = CreateMenu(
                            "Ürün Fiyatları", 
                            "fas fa-tag", 
                            "UrunFiyat", 
                            "Index", 
                            7, 
                            stokUrunYonetimiMenu.MenuID, 
                            "/UrunFiyat/Index"
                        );
                        await _context.Menuler.AddAsync(urunFiyatlari);
                        await _context.MenuRoller.AddAsync(new MenuRol { MenuRolID = Guid.NewGuid(), MenuId = urunFiyatlari.MenuID, RolId = adminRole.Id });
                        if (managerRole != null)
                            await _context.MenuRoller.AddAsync(new MenuRol { MenuRolID = Guid.NewGuid(), MenuId = urunFiyatlari.MenuID, RolId = managerRole.Id });

                        // 4. Finans Yönetimi Menüsü
                        var finansYonetimiMenu = CreateMenu(
                            "Finans Yönetimi", 
                            "fas fa-money-bill-wave", 
                            "", 
                            "", 
                            4, 
                            null, 
                            "#"
                        );
                        await _context.Menuler.AddAsync(finansYonetimiMenu);
                        await _context.MenuRoller.AddAsync(new MenuRol { MenuRolID = Guid.NewGuid(), MenuId = finansYonetimiMenu.MenuID, RolId = adminRole.Id });
                        if (managerRole != null)
                            await _context.MenuRoller.AddAsync(new MenuRol { MenuRolID = Guid.NewGuid(), MenuId = finansYonetimiMenu.MenuID, RolId = managerRole.Id });
                        if (accountantRole != null)
                            await _context.MenuRoller.AddAsync(new MenuRol { MenuRolID = Guid.NewGuid(), MenuId = finansYonetimiMenu.MenuID, RolId = accountantRole.Id });

                        // Finans Yönetimi Alt Menüleri
                        var faturalar = CreateMenu(
                            "Faturalar", 
                            "fas fa-file-invoice", 
                            "Fatura", 
                            "Index", 
                            1, 
                            finansYonetimiMenu.MenuID, 
                            "/Fatura/Index"
                        );
                        await _context.Menuler.AddAsync(faturalar);
                        await _context.MenuRoller.AddAsync(new MenuRol { MenuRolID = Guid.NewGuid(), MenuId = faturalar.MenuID, RolId = adminRole.Id });
                        if (managerRole != null)
                            await _context.MenuRoller.AddAsync(new MenuRol { MenuRolID = Guid.NewGuid(), MenuId = faturalar.MenuID, RolId = managerRole.Id });
                        if (accountantRole != null)
                            await _context.MenuRoller.AddAsync(new MenuRol { MenuRolID = Guid.NewGuid(), MenuId = faturalar.MenuID, RolId = accountantRole.Id });

                        var yeniFatura = CreateMenu(
                            "Yeni Fatura", 
                            "fas fa-plus", 
                            "Fatura", 
                            "Create", 
                            2, 
                            finansYonetimiMenu.MenuID, 
                            "/Fatura/Create"
                        );
                        await _context.Menuler.AddAsync(yeniFatura);
                        await _context.MenuRoller.AddAsync(new MenuRol { MenuRolID = Guid.NewGuid(), MenuId = yeniFatura.MenuID, RolId = adminRole.Id });
                        if (managerRole != null)
                            await _context.MenuRoller.AddAsync(new MenuRol { MenuRolID = Guid.NewGuid(), MenuId = yeniFatura.MenuID, RolId = managerRole.Id });
                        if (accountantRole != null)
                            await _context.MenuRoller.AddAsync(new MenuRol { MenuRolID = Guid.NewGuid(), MenuId = yeniFatura.MenuID, RolId = accountantRole.Id });

                        var irsaliyeler = CreateMenu(
                            "İrsaliyeler", 
                            "fas fa-truck-loading", 
                            "Irsaliye", 
                            "Index", 
                            3, 
                            finansYonetimiMenu.MenuID, 
                            "/Irsaliye/Index"
                        );
                        await _context.Menuler.AddAsync(irsaliyeler);
                        await _context.MenuRoller.AddAsync(new MenuRol { MenuRolID = Guid.NewGuid(), MenuId = irsaliyeler.MenuID, RolId = adminRole.Id });
                        if (managerRole != null)
                            await _context.MenuRoller.AddAsync(new MenuRol { MenuRolID = Guid.NewGuid(), MenuId = irsaliyeler.MenuID, RolId = managerRole.Id });

                        var yeniIrsaliye = CreateMenu(
                            "Yeni İrsaliye", 
                            "fas fa-plus", 
                            "Irsaliye", 
                            "Create", 
                            4, 
                            finansYonetimiMenu.MenuID, 
                            "/Irsaliye/Create"
                        );
                        await _context.Menuler.AddAsync(yeniIrsaliye);
                        await _context.MenuRoller.AddAsync(new MenuRol { MenuRolID = Guid.NewGuid(), MenuId = yeniIrsaliye.MenuID, RolId = adminRole.Id });
                        if (managerRole != null)
                            await _context.MenuRoller.AddAsync(new MenuRol { MenuRolID = Guid.NewGuid(), MenuId = yeniIrsaliye.MenuID, RolId = managerRole.Id });

                        var kasaIslemleri = CreateMenu(
                            "Kasa İşlemleri", 
                            "fas fa-cash-register", 
                            "Kasa", 
                            "Index", 
                            5, 
                            finansYonetimiMenu.MenuID, 
                            "/Kasa/Index"
                        );
                        await _context.Menuler.AddAsync(kasaIslemleri);
                        await _context.MenuRoller.AddAsync(new MenuRol { MenuRolID = Guid.NewGuid(), MenuId = kasaIslemleri.MenuID, RolId = adminRole.Id });
                        if (managerRole != null)
                            await _context.MenuRoller.AddAsync(new MenuRol { MenuRolID = Guid.NewGuid(), MenuId = kasaIslemleri.MenuID, RolId = managerRole.Id });
                        if (accountantRole != null)
                            await _context.MenuRoller.AddAsync(new MenuRol { MenuRolID = Guid.NewGuid(), MenuId = kasaIslemleri.MenuID, RolId = accountantRole.Id });

                        var bankaHesaplari = CreateMenu(
                            "Banka Hesapları", 
                            "fas fa-university", 
                            "Banka", 
                            "Index", 
                            6, 
                            finansYonetimiMenu.MenuID, 
                            "/Banka/Index"
                        );
                        await _context.Menuler.AddAsync(bankaHesaplari);
                        await _context.MenuRoller.AddAsync(new MenuRol { MenuRolID = Guid.NewGuid(), MenuId = bankaHesaplari.MenuID, RolId = adminRole.Id });
                        if (managerRole != null)
                            await _context.MenuRoller.AddAsync(new MenuRol { MenuRolID = Guid.NewGuid(), MenuId = bankaHesaplari.MenuID, RolId = managerRole.Id });
                        if (accountantRole != null)
                            await _context.MenuRoller.AddAsync(new MenuRol { MenuRolID = Guid.NewGuid(), MenuId = bankaHesaplari.MenuID, RolId = accountantRole.Id });

                        var paraBirimleri = CreateMenu(
                            "Para Birimleri", 
                            "fas fa-dollar-sign", 
                            "ParaBirimi", 
                            "Index", 
                            7, 
                            finansYonetimiMenu.MenuID, 
                            "/ParaBirimi/Index"
                        );
                        await _context.Menuler.AddAsync(paraBirimleri);
                        await _context.MenuRoller.AddAsync(new MenuRol { MenuRolID = Guid.NewGuid(), MenuId = paraBirimleri.MenuID, RolId = adminRole.Id });
                        if (managerRole != null)
                            await _context.MenuRoller.AddAsync(new MenuRol { MenuRolID = Guid.NewGuid(), MenuId = paraBirimleri.MenuID, RolId = managerRole.Id });
                        if (accountantRole != null)
                            await _context.MenuRoller.AddAsync(new MenuRol { MenuRolID = Guid.NewGuid(), MenuId = paraBirimleri.MenuID, RolId = accountantRole.Id });

                        // 5. Raporlar Menüsü
                        var raporlarMenu = CreateMenu(
                            "Raporlar", 
                            "fas fa-chart-bar", 
                            "Rapor", 
                            "Index", 
                            5, 
                            null, 
                            "/Rapor/Index"
                        );
                        await _context.Menuler.AddAsync(raporlarMenu);
                        await _context.MenuRoller.AddAsync(new MenuRol { MenuRolID = Guid.NewGuid(), MenuId = raporlarMenu.MenuID, RolId = adminRole.Id });
                        if (managerRole != null)
                            await _context.MenuRoller.AddAsync(new MenuRol { MenuRolID = Guid.NewGuid(), MenuId = raporlarMenu.MenuID, RolId = managerRole.Id });
                        if (accountantRole != null)
                            await _context.MenuRoller.AddAsync(new MenuRol { MenuRolID = Guid.NewGuid(), MenuId = raporlarMenu.MenuID, RolId = accountantRole.Id });

                        // 6. Sistem Yönetimi Menüsü
                        var sistemYonetimiMenu = CreateMenu(
                            "Sistem Yönetimi", 
                            "fas fa-cogs", 
                            "", 
                            "", 
                            6, 
                            null, 
                            "#"
                        );
                        await _context.Menuler.AddAsync(sistemYonetimiMenu);
                        await _context.MenuRoller.AddAsync(new MenuRol { MenuRolID = Guid.NewGuid(), MenuId = sistemYonetimiMenu.MenuID, RolId = adminRole.Id });

                        // Sistem Yönetimi Alt Menüleri
                        var kullanicilar = CreateMenu(
                            "Kullanıcılar", 
                            "fas fa-user-cog", 
                            "Kullanici", 
                            "Index", 
                            1, 
                            sistemYonetimiMenu.MenuID, 
                            "/Kullanici/Index"
                        );
                        await _context.Menuler.AddAsync(kullanicilar);
                        await _context.MenuRoller.AddAsync(new MenuRol { MenuRolID = Guid.NewGuid(), MenuId = kullanicilar.MenuID, RolId = adminRole.Id });

                        var sistemAyarlari = CreateMenu(
                            "Sistem Ayarları", 
                            "fas fa-cog", 
                            "SistemAyar", 
                            "Index", 
                            2, 
                            sistemYonetimiMenu.MenuID, 
                            "/SistemAyar/Index"
                        );
                        await _context.Menuler.AddAsync(sistemAyarlari);
                        await _context.MenuRoller.AddAsync(new MenuRol { MenuRolID = Guid.NewGuid(), MenuId = sistemAyarlari.MenuID, RolId = adminRole.Id });

                        var menuYonetimi = CreateMenu(
                            "Menü Yönetimi", 
                            "fas fa-bars", 
                            "Menu", 
                            "Index", 
                            3, 
                            sistemYonetimiMenu.MenuID, 
                            "/Menu/Index"
                        );
                        await _context.Menuler.AddAsync(menuYonetimi);
                        await _context.MenuRoller.AddAsync(new MenuRol { MenuRolID = Guid.NewGuid(), MenuId = menuYonetimi.MenuID, RolId = adminRole.Id });

                        var sistemLoglari = CreateMenu(
                            "Sistem Logları", 
                            "fas fa-list-alt", 
                            "SistemLog", 
                            "Index", 
                            4, 
                            sistemYonetimiMenu.MenuID, 
                            "/SistemLog/Index"
                        );
                        await _context.Menuler.AddAsync(sistemLoglari);
                        await _context.MenuRoller.AddAsync(new MenuRol { MenuRolID = Guid.NewGuid(), MenuId = sistemLoglari.MenuID, RolId = adminRole.Id });

                        var dilAyarlari = CreateMenu(
                            "Dil Ayarları", 
                            "fas fa-language", 
                            "Language", 
                            "Index", 
                            5, 
                            sistemYonetimiMenu.MenuID, 
                            "/Language/Index"
                        );
                        await _context.Menuler.AddAsync(dilAyarlari);
                        await _context.MenuRoller.AddAsync(new MenuRol { MenuRolID = Guid.NewGuid(), MenuId = dilAyarlari.MenuID, RolId = adminRole.Id });

                        // Değişiklikleri kaydet
                        await _context.SaveChangesAsync();
                        
                        // Transaction'ı commit et
                        await transaction.CommitAsync();
                        
                        _logger.LogInformation("Varsayılan menüler başarıyla oluşturuldu");
                        return true;
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Varsayılan menüleri oluştururken hata: {Message}", ex.Message);
                        return false;
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Varsayılan menüleri oluşturma işlemi başarısız oldu: {Message}", ex.Message);
                
                // İç hatayı da logla
                if (ex.InnerException != null)
                {
                    _logger.LogError(ex.InnerException, "İç hata: {Message}", ex.InnerException.Message);
                }
                
                // Log servisinde hata olabileceği için try-catch içine al
                try
                {
                    await _logService.LogErrorAsync("MenuService.InitDefaultMenusAsync", ex.Message + "\n" + ex.StackTrace);
                }
                catch (Exception logEx)
                {
                    _logger.LogWarning(logEx, "Hata log kaydı sırasında ikincil bir hata oluştu: {Message}", logEx.Message);
                }
                
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
                            .AnyAsync(mr => mr.MenuId == menu.MenuID && mr.RolId == rol.Id);
                            
                        // Eğer yoksa, Admin rolüne tüm izinleri ekle, diğer roller için sadece görüntüleme izni ekle
                        if (!menuRolVar)
                        {
                            var menuRol = new MenuRol
                            {
                                MenuId = menu.MenuID,
                                RolId = rol.Id
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
                List<string> userRoles = new List<string>{"User"}; // Varsayılan olarak User rolü
                
                if (!string.IsNullOrEmpty(userId))
                {
                    var user = await _userManager.FindByIdAsync(userId);
                    if (user != null)
                    {
                        userRoles = (await _userManager.GetRolesAsync(user)).ToList();
                    }
                }
                
                bool isAdmin = userRoles.Contains("Admin");
                
                // Tüm menüleri ve rol ilişkilerini tek seferde getir
                List<Data.Entities.Menu> allMenus = null;
                
                try
                {
                    // Önce SoftDelete'in var olup olmadığını kontrol edelim
                    string query = "SELECT AktifMi FROM Menuler WHERE 1=0";
                    try
                    {
                        // SoftDelete mevcut mu kontrol et
                        await _context.Database.ExecuteSqlRawAsync("SELECT Silindi FROM Menuler WHERE 1=0");
                        
                        // SoftDelete mevcutsa normal sorguyu kullan
                        allMenus = await _context.Menuler
                            .Include(m => m.MenuRoller)
                            .Where(m => m.AktifMi && !m.Silindi)
                            .OrderBy(m => m.Sira)
                            .ToListAsync();
                    }
                    catch (Exception)
                    {
                        // SoftDelete sütunu yoksa onu hariç tut
                        _logger.LogWarning("Silindi sütunu kullanılıyor");
                        allMenus = await _context.Menuler
                            .Include(m => m.MenuRoller)
                            .Where(m => m.AktifMi && !m.Silindi)
                            .OrderBy(m => m.Sira)
                            .ToListAsync();
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Menuler tablosunda sorgulama yapılırken hata: {Message}", ex.Message);
                    
                    if (ex.Message.Contains("no such column: m.Silindi"))
                    {
                        // Silindi sütunu eksik, migration çalıştırılmalı
                        _logger.LogWarning("Menüler tablosunda Silindi sütunu eksik, varsayılan menüler kullanılıyor");
                        return GetDefaultMenuItems();
                    }
                    
                    // Diğer hatalar için de varsayılan menüleri döndür
                    return GetDefaultMenuItems();
                }
                        
                if (allMenus == null || !allMenus.Any())
                {
                    _logger.LogWarning("Veritabanında aktif menü bulunamadı, varsayılan menüler kullanılıyor");
                    return GetDefaultMenuItems();
                }
                
                // Root menüleri bul
                var rootMenus = allMenus.Where(m => m.UstMenuID == null).ToList();
                var result = new List<MenuViewModel>();
                
                if (!rootMenus.Any())
                {
                    _logger.LogWarning("Veritabanında kök menü bulunamadı, varsayılan menüler kullanılıyor");
                    return GetDefaultMenuItems();
                }
                
                // Admin için tüm root menüleri ekle
                if (isAdmin)
                {
                    try
                    {
                        // Her bir root menü için
                        foreach (var rootMenu in rootMenus)
                        {
                            var menuViewModel = new MenuViewModel
                            {
                                MenuID = rootMenu.MenuID,
                                Ad = rootMenu.Ad,
                                Icon = rootMenu.Icon ?? "material-icons",
                                Url = rootMenu.Url ?? "#",
                                Controller = rootMenu.Controller ?? "",
                                Action = rootMenu.Action ?? "",
                                AktifMi = rootMenu.AktifMi,
                                Sira = rootMenu.Sira,
                                UstMenuID = null,
                                Level = 0,
                                AltMenuler = new List<MenuViewModel>()
                            };
                            
                            // Alt menüleri bul
                            var childMenus = allMenus.Where(m => m.UstMenuID == rootMenu.MenuID).ToList();
                            foreach (var childMenu in childMenus)
                            {
                                var childViewModel = new MenuViewModel
                                {
                                    MenuID = childMenu.MenuID,
                                    Ad = childMenu.Ad,
                                    Icon = childMenu.Icon ?? "material-icons",
                                    Url = childMenu.Url ?? "#",
                                    Controller = childMenu.Controller ?? "",
                                    Action = childMenu.Action ?? "",
                                    AktifMi = childMenu.AktifMi,
                                    Sira = childMenu.Sira,
                                    UstMenuID = rootMenu.MenuID,
                                    Level = 1,
                                    AltMenuler = new List<MenuViewModel>()
                                };
                                
                                menuViewModel.AltMenuler.Add(childViewModel);
                            }
                            
                            result.Add(menuViewModel);
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Menü hiyerarşisi oluşturulurken hata: {Message}", ex.Message);
                        return GetDefaultMenuItems();
                    }
                }
                else
                {
                    // Normal kullanıcılar için rollerine uygun menüler
                    try
                    {
                        var userRoleIds = new HashSet<string>(userRoles);
                        
                        // Kullanıcının rollerine uygun menüleri getir
                        foreach (var rootMenu in rootMenus)
                        {
                            // Menünün rollerini kontrol et
                            bool menuHasAccess = rootMenu.MenuRoller.Any(mr => mr != null && 
                                                                         mr.RolId != null && 
                                                                         userRoleIds.Contains(mr.RolId));
                            
                            if (menuHasAccess)
                            {
                                var menuViewModel = new MenuViewModel
                                {
                                    MenuID = rootMenu.MenuID,
                                    Ad = rootMenu.Ad,
                                    Icon = rootMenu.Icon ?? "material-icons",
                                    Url = rootMenu.Url ?? "#",
                                    Controller = rootMenu.Controller ?? "",
                                    Action = rootMenu.Action ?? "",
                                    AktifMi = rootMenu.AktifMi,
                                    Sira = rootMenu.Sira,
                                    UstMenuID = null,
                                    Level = 0,
                                    AltMenuler = new List<MenuViewModel>()
                                };
                                
                                // Alt menüleri bul
                                var childMenus = allMenus.Where(m => m.UstMenuID == rootMenu.MenuID).ToList();
                                foreach (var childMenu in childMenus)
                                {
                                    bool childMenuHasAccess = childMenu.MenuRoller.Any(mr => userRoleIds.Contains(mr.RolId));
                                    
                                    if (childMenuHasAccess)
                                    {
                                        var childViewModel = new MenuViewModel
                                        {
                                            MenuID = childMenu.MenuID,
                                            Ad = childMenu.Ad,
                                            Icon = childMenu.Icon ?? "material-icons",
                                            Url = childMenu.Url ?? "#",
                                            Controller = childMenu.Controller ?? "",
                                            Action = childMenu.Action ?? "",
                                            AktifMi = childMenu.AktifMi,
                                            Sira = childMenu.Sira,
                                            UstMenuID = rootMenu.MenuID,
                                            Level = 1,
                                            AltMenuler = new List<MenuViewModel>()
                                        };
                                        
                                        menuViewModel.AltMenuler.Add(childViewModel);
                                    }
                                }
                                
                                // Sadece alt menüleri olan veya doğrudan URL'si olan menüleri ekle
                                if (menuViewModel.AltMenuler.Any() || !string.IsNullOrEmpty(menuViewModel.Url))
                                {
                                    result.Add(menuViewModel);
                                }
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Kullanıcı menüleri oluşturulurken hata: {Message}", ex.Message);
                        return GetDefaultMenuItems();
                    }
                }
                
                // Menüleri sırala
                return result.OrderBy(m => m.Sira).ToList();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "GetActiveSidebarMenusAsync metodunda beklenmeyen hata: {Message}", ex.Message);
                return GetDefaultMenuItems();
            }
        }

        // Varsayılan menü öğelerini döndüren yardımcı metot
        private List<MenuViewModel> GetDefaultMenuItems()
        {
            var result = new List<MenuViewModel>();
            
            // Dashboard menüsü
            result.Add(new MenuViewModel
            {
                MenuID = Guid.NewGuid(),
                Ad = "Dashboard",
                Icon = "fas fa-tachometer-alt",
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
                Icon = "fas fa-users",
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
                Icon = "fas fa-boxes",
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
                Ad = "Stok Hareketleri",
                Controller = "StokHareket",
                Action = "Index",
                AktifMi = true,
                Sira = 2,
                UstMenuID = stokYonetimiMenu.MenuID,
                Url = "/StokHareket/Index",
                AltMenuler = new List<MenuViewModel>()
            });
            
            // Yeni alt menüler
            stokYonetimiMenu.AltMenuler.Add(new MenuViewModel
            {
                MenuID = Guid.NewGuid(),
                Ad = "Kategoriler",
                Controller = "Kategori",
                Action = "Index",
                AktifMi = true,
                Sira = 3,
                UstMenuID = stokYonetimiMenu.MenuID,
                Url = "/Kategori/Index",
                AltMenuler = new List<MenuViewModel>()
            });
            
            stokYonetimiMenu.AltMenuler.Add(new MenuViewModel
            {
                MenuID = Guid.NewGuid(),
                Ad = "Depolar",
                Controller = "Depo",
                Action = "Index",
                AktifMi = true,
                Sira = 4,
                UstMenuID = stokYonetimiMenu.MenuID,
                Url = "/Depo/Index",
                AltMenuler = new List<MenuViewModel>()
            });
            
            stokYonetimiMenu.AltMenuler.Add(new MenuViewModel
            {
                MenuID = Guid.NewGuid(),
                Ad = "Birimler",
                Controller = "Birim",
                Action = "Index",
                AktifMi = true,
                Sira = 5,
                UstMenuID = stokYonetimiMenu.MenuID,
                Url = "/Birim/Index",
                AltMenuler = new List<MenuViewModel>()
            });
            
            result.Add(stokYonetimiMenu);
            
            // Faturalar menüsü
            result.Add(new MenuViewModel
            {
                MenuID = Guid.NewGuid(),
                Ad = "Faturalar",
                Icon = "fas fa-file-invoice",
                Controller = "Fatura",
                Action = "Index",
                AktifMi = true,
                Sira = 4,
                Url = "/Fatura/Index",
                AltMenuler = new List<MenuViewModel>()
            });
            
            // Raporlar menüsü (üst menü)
            var raporlarMenu = new MenuViewModel
            {
                MenuID = Guid.NewGuid(),
                Ad = "Raporlar",
                Icon = "fas fa-chart-bar",
                AktifMi = true,
                Sira = 5,
                Url = "#",
                AltMenuler = new List<MenuViewModel>()
            };
            
            // Rapor alt menüleri
            raporlarMenu.AltMenuler.Add(new MenuViewModel
            {
                MenuID = Guid.NewGuid(),
                Ad = "Stok Raporu",
                Controller = "Stok",
                Action = "StokRapor",
                AktifMi = true,
                Sira = 1,
                UstMenuID = raporlarMenu.MenuID,
                Url = "/Stok/StokRapor",
                AltMenuler = new List<MenuViewModel>()
            });
            
            raporlarMenu.AltMenuler.Add(new MenuViewModel
            {
                MenuID = Guid.NewGuid(),
                Ad = "Satış Raporu",
                Controller = "Rapor",
                Action = "SatisRapor",
                AktifMi = true,
                Sira = 2,
                UstMenuID = raporlarMenu.MenuID,
                Url = "/Rapor/SatisRapor",
                AltMenuler = new List<MenuViewModel>()
            });
            
            result.Add(raporlarMenu);
            
            // Kullanıcılar menüsü
            result.Add(new MenuViewModel
            {
                MenuID = Guid.NewGuid(),
                Ad = "Kullanıcılar",
                Icon = "fas fa-users-cog",
                Controller = "User",
                Action = "Index",
                AktifMi = true,
                Sira = 6,
                Url = "/User/Index",
                AltMenuler = new List<MenuViewModel>()
            });
            
            // Şirket Yönetimi menüsü
            var sirketMenu = new MenuViewModel
            {
                MenuID = Guid.NewGuid(),
                Ad = "Şirket Yönetimi",
                Icon = "fas fa-building",
                AktifMi = true,
                Sira = 7,
                Url = "#",
                AltMenuler = new List<MenuViewModel>()
            };
            
            // Şirket alt menüleri
            sirketMenu.AltMenuler.Add(new MenuViewModel
            {
                MenuID = Guid.NewGuid(),
                Ad = "Şirket Ayarları",
                Controller = "Sirket",
                Action = "Index",
                AktifMi = true,
                Sira = 1,
                UstMenuID = sirketMenu.MenuID,
                Url = "/Sirket/Index",
                AltMenuler = new List<MenuViewModel>()
            });
            
            sirketMenu.AltMenuler.Add(new MenuViewModel
            {
                MenuID = Guid.NewGuid(),
                Ad = "Şubeler",
                Controller = "Sube",
                Action = "Index",
                AktifMi = true,
                Sira = 2,
                UstMenuID = sirketMenu.MenuID,
                Url = "/Sube/Index",
                AltMenuler = new List<MenuViewModel>()
            });
            
            sirketMenu.AltMenuler.Add(new MenuViewModel
            {
                MenuID = Guid.NewGuid(),
                Ad = "Departmanlar",
                Controller = "Departman",
                Action = "Index",
                AktifMi = true,
                Sira = 3,
                UstMenuID = sirketMenu.MenuID,
                Url = "/Departman/Index",
                AltMenuler = new List<MenuViewModel>()
            });
            
            sirketMenu.AltMenuler.Add(new MenuViewModel
            {
                MenuID = Guid.NewGuid(),
                Ad = "Personel",
                Controller = "Personel",
                Action = "Index",
                AktifMi = true,
                Sira = 4,
                UstMenuID = sirketMenu.MenuID,
                Url = "/Personel/Index",
                AltMenuler = new List<MenuViewModel>()
            });
            
            result.Add(sirketMenu);
            
            // Finans Yönetimi menüsü
            var finansMenu = new MenuViewModel
            {
                MenuID = Guid.NewGuid(),
                Ad = "Finans",
                Icon = "fas fa-money-check-alt",
                AktifMi = true,
                Sira = 8,
                Url = "#",
                AltMenuler = new List<MenuViewModel>()
            };
            
            // Finans alt menüleri
            finansMenu.AltMenuler.Add(new MenuViewModel
            {
                MenuID = Guid.NewGuid(),
                Ad = "Gelirler",
                Controller = "Gelir",
                Action = "Index",
                AktifMi = true,
                Sira = 1,
                UstMenuID = finansMenu.MenuID,
                Url = "/Gelir/Index",
                AltMenuler = new List<MenuViewModel>()
            });
            
            finansMenu.AltMenuler.Add(new MenuViewModel
            {
                MenuID = Guid.NewGuid(),
                Ad = "Giderler",
                Controller = "Gider",
                Action = "Index",
                AktifMi = true,
                Sira = 2,
                UstMenuID = finansMenu.MenuID,
                Url = "/Gider/Index",
                AltMenuler = new List<MenuViewModel>()
            });
            
            finansMenu.AltMenuler.Add(new MenuViewModel
            {
                MenuID = Guid.NewGuid(),
                Ad = "Kasa İşlemleri",
                Controller = "Kasa",
                Action = "Index",
                AktifMi = true,
                Sira = 3,
                UstMenuID = finansMenu.MenuID,
                Url = "/Kasa/Index",
                AltMenuler = new List<MenuViewModel>()
            });
            
            finansMenu.AltMenuler.Add(new MenuViewModel
            {
                MenuID = Guid.NewGuid(),
                Ad = "Banka Hesapları",
                Controller = "BankaHesap",
                Action = "Index",
                AktifMi = true,
                Sira = 4,
                UstMenuID = finansMenu.MenuID,
                Url = "/BankaHesap/Index",
                AltMenuler = new List<MenuViewModel>()
            });
            
            finansMenu.AltMenuler.Add(new MenuViewModel
            {
                MenuID = Guid.NewGuid(),
                Ad = "Ödemeler",
                Controller = "Odeme",
                Action = "Index",
                AktifMi = true,
                Sira = 5,
                UstMenuID = finansMenu.MenuID,
                Url = "/Odeme/Index",
                AltMenuler = new List<MenuViewModel>()
            });
            
            result.Add(finansMenu);
            
            // Döviz İşlemleri menüsü
            var dovizMenu = new MenuViewModel
            {
                MenuID = Guid.NewGuid(),
                Ad = "Döviz İşlemleri",
                Icon = "fas fa-dollar-sign",
                AktifMi = true,
                Sira = 9,
                Url = "#",
                AltMenuler = new List<MenuViewModel>()
            };
            
            // Döviz İşlemleri alt menüleri
            dovizMenu.AltMenuler.Add(new MenuViewModel
            {
                MenuID = Guid.NewGuid(),
                Ad = "Döviz Kurları",
                Controller = "ParaBirimi",
                Action = "Kurlar",
                AktifMi = true,
                Sira = 1,
                UstMenuID = dovizMenu.MenuID,
                Url = "/ParaBirimi/Kurlar",
                AltMenuler = new List<MenuViewModel>()
            });
            
            dovizMenu.AltMenuler.Add(new MenuViewModel
            {
                MenuID = Guid.NewGuid(),
                Ad = "Para Birimleri",
                Controller = "ParaBirimi",
                Action = "Index",
                AktifMi = true,
                Sira = 2,
                UstMenuID = dovizMenu.MenuID,
                Url = "/ParaBirimi/Index",
                AltMenuler = new List<MenuViewModel>()
            });
            
            dovizMenu.AltMenuler.Add(new MenuViewModel
            {
                MenuID = Guid.NewGuid(),
                Ad = "Para Birimi İlişkileri",
                Controller = "ParaBirimi",
                Action = "Iliskiler",
                AktifMi = true,
                Sira = 3,
                UstMenuID = dovizMenu.MenuID,
                Url = "/ParaBirimi/Iliskiler",
                AltMenuler = new List<MenuViewModel>()
            });
            
            result.Add(dovizMenu);
            
            // Satın Alma menüsü
            var satinAlmaMenu = new MenuViewModel
            {
                MenuID = Guid.NewGuid(),
                Ad = "Satın Alma",
                Icon = "fas fa-shopping-cart",
                AktifMi = true,
                Sira = 10,
                Url = "#",
                AltMenuler = new List<MenuViewModel>()
            };
            
            // Satın Alma alt menüleri
            satinAlmaMenu.AltMenuler.Add(new MenuViewModel
            {
                MenuID = Guid.NewGuid(),
                Ad = "Tedarikçiler",
                Controller = "Tedarikci",
                Action = "Index",
                AktifMi = true,
                Sira = 1,
                UstMenuID = satinAlmaMenu.MenuID,
                Url = "/Tedarikci/Index",
                AltMenuler = new List<MenuViewModel>()
            });
            
            satinAlmaMenu.AltMenuler.Add(new MenuViewModel
            {
                MenuID = Guid.NewGuid(),
                Ad = "Satın Alma Siparişleri",
                Controller = "SatinAlmaSiparis",
                Action = "Index",
                AktifMi = true,
                Sira = 2,
                UstMenuID = satinAlmaMenu.MenuID,
                Url = "/SatinAlmaSiparis/Index",
                AltMenuler = new List<MenuViewModel>()
            });
            
            satinAlmaMenu.AltMenuler.Add(new MenuViewModel
            {
                MenuID = Guid.NewGuid(),
                Ad = "Alım Faturaları",
                Controller = "AlimFatura",
                Action = "Index",
                AktifMi = true,
                Sira = 3,
                UstMenuID = satinAlmaMenu.MenuID,
                Url = "/AlimFatura/Index",
                AltMenuler = new List<MenuViewModel>()
            });
            
            result.Add(satinAlmaMenu);
            
            // Sistem Ayarları menüsü
            var sistemMenu = new MenuViewModel
            {
                MenuID = Guid.NewGuid(),
                Ad = "Sistem Ayarları",
                Icon = "fas fa-cogs",
                AktifMi = true,
                Sira = 11,
                Url = "#",
                AltMenuler = new List<MenuViewModel>()
            };
            
            // Sistem alt menüleri
            sistemMenu.AltMenuler.Add(new MenuViewModel
            {
                MenuID = Guid.NewGuid(),
                Ad = "Menü Yönetimi",
                Controller = "Menu",
                Action = "Index",
                AktifMi = true,
                Sira = 1,
                UstMenuID = sistemMenu.MenuID,
                Url = "/Menu/Index",
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
                Ad = "Veritabanı Yönetimi",
                Controller = "DbInit",
                Action = "Index",
                AktifMi = true,
                Sira = 3,
                UstMenuID = sistemMenu.MenuID,
                Url = "/DbInit/Index",
                AltMenuler = new List<MenuViewModel>()
            });
            
            result.Add(sistemMenu);
            
            return result;
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
                    if (roles.TryGetValue(menuRol.RolId, out string roleName))
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

        private MenuViewModel CreateSidebarMenuViewModel(Data.Entities.Menu menu, List<Data.Entities.Menu> allMenus, 
            bool isAdmin, HashSet<string> userRoleIds, int level)
        {
            try
            {
                // Menü null kontrolü
                if (menu == null) 
                {
                    _logService.LogWarningAsync("MenuService.CreateSidebarMenuViewModel", "Null menü gönderildi").Wait();
                    return null;
                }
                
                // Menü view model oluştur
                var viewModel = new MenuViewModel
                {
                    MenuID = menu.MenuID,
                    Ad = menu.Ad ?? "İsimsiz Menü", // Ad null ise varsayılan değer
                    Icon = menu.Icon ?? "material-icons", // Icon null ise varsayılan icon kullan
                    Url = menu.Url ?? "#", // Url null ise varsayılan değer
                    Controller = menu.Controller ?? "", // Controller null ise boş string
                    Action = menu.Action ?? "", // Action null ise boş string
                    AktifMi = menu.AktifMi,
                    Sira = menu.Sira,
                    UstMenuID = menu.UstMenuID,
                    Level = level,
                    AltMenuler = new List<MenuViewModel>() // Her zaman boş bir liste ile başlat
                };

                _logService.LogInfoAsync("MenuService.CreateSidebarMenuViewModel", 
                    $"Menü oluşturuluyor: MenuID: {menu.MenuID}, Ad: {viewModel.Ad}, Controller: {viewModel.Controller}, Action: {viewModel.Action}").Wait();

                // MenuRoller null kontrolü
                if (menu.MenuRoller == null)
                {
                    _logService.LogWarningAsync("MenuService.CreateSidebarMenuViewModel", 
                        $"MenuID: {menu.MenuID}, Ad: {viewModel.Ad} için MenuRoller null. Boş liste ile başlatılıyor.").Wait();
                    menu.MenuRoller = new List<MenuRol>();
                }

                // Alt menüleri ekle (kullanıcının yetkisi olan)
                // allMenus null kontrolü
                if (allMenus == null)
                {
                    _logService.LogWarningAsync("MenuService.CreateSidebarMenuViewModel", "allMenus listesi null").Wait();
                    return viewModel;
                }
                
                var altMenuler = allMenus
                    .Where(m => m.UstMenuID == menu.MenuID && m.AktifMi)
                    .OrderBy(m => m.Sira)
                    .ToList();
                
                // Alt menüler null kontrolü (Where sorgusu bazen null dönebilir)
                if (altMenuler == null)
                {
                    _logService.LogWarningAsync("MenuService.CreateSidebarMenuViewModel", 
                        $"MenuID: {menu.MenuID}, Ad: {viewModel.Ad} için alt menüler null. Boş liste kullanılacak.").Wait();
                    altMenuler = new List<Data.Entities.Menu>();
                }
                
                _logService.LogInfoAsync("MenuService.CreateSidebarMenuViewModel", 
                    $"MenuID: {menu.MenuID}, Ad: {viewModel.Ad} için {altMenuler.Count} adet alt menü bulundu.").Wait();
                
                if (altMenuler.Any())
                {
                    foreach (var altMenu in altMenuler)
                    {
                        // Alt menü null kontrolü
                        if (altMenu == null) 
                        {
                            _logService.LogWarningAsync("MenuService.CreateSidebarMenuViewModel", "Null alt menü atlanıyor").Wait();
                            continue;
                        }
                        
                        // Alt menü MenuRoller null kontrolü
                        if (altMenu.MenuRoller == null)
                        {
                            _logService.LogWarningAsync("MenuService.CreateSidebarMenuViewModel", 
                                $"Alt menü MenuID: {altMenu.MenuID}, Ad: {altMenu.Ad ?? "İsimsiz"} için MenuRoller null. Boş liste ile başlatılıyor.").Wait();
                            altMenu.MenuRoller = new List<MenuRol>();
                        }
                        
                        // userRoleIds null kontrolü
                        if (userRoleIds == null)
                        {
                            _logService.LogWarningAsync("MenuService.CreateSidebarMenuViewModel", "userRoleIds listesi null").Wait();
                            userRoleIds = new HashSet<string>();
                        }
                        
                        bool userHasAccess = false;
                        
                        // MenuRoller içindeki öğelerin de null kontrolünü yap
                        if (altMenu.MenuRoller.Any())
                        {
                            userHasAccess = altMenu.MenuRoller.Any(mr => mr != null && 
                                                                         mr.RolId != null && 
                                                                         userRoleIds.Contains(mr.RolId));
                        }
                        
                        // Admin tüm alt menüleri görebilir veya kullanıcının bu alt menüye rolü varsa ekle
                        if (isAdmin || userHasAccess)
                        {
                            try
                            {
                                var altViewModel = CreateSidebarMenuViewModel(altMenu, allMenus, isAdmin, userRoleIds, level + 1);
                                if (altViewModel != null)
                                {
                                    viewModel.AltMenuler.Add(altViewModel);
                                    _logService.LogInfoAsync("MenuService.CreateSidebarMenuViewModel", 
                                        $"Alt menü eklendi: MenuID: {altMenu.MenuID}, Ad: {altMenu.Ad ?? "İsimsiz"}").Wait();
                                }
                                else
                                {
                                    _logService.LogWarningAsync("MenuService.CreateSidebarMenuViewModel", 
                                        $"Alt menü için oluşturulan viewModel null: MenuID: {altMenu.MenuID}, Ad: {altMenu.Ad ?? "İsimsiz"}").Wait();
                                }
                            }
                            catch (Exception ex)
                            {
                                _logService.LogErrorAsync("MenuService.CreateSidebarMenuViewModel", 
                                    $"Alt menü oluşturulurken hata: {ex.Message}, MenuID: {altMenu.MenuID}, Ad: {altMenu.Ad ?? "İsimsiz"}").Wait();
                                // Alt menü oluşturmada hata olursa devam et, tüm menüyü iptal etme
                                continue;
                            }
                        }
                    }
                }

                return viewModel;
            }
            catch (Exception ex)
            {
                // Hata durumunda hatayı logla ve null döndür
                _logService.LogErrorAsync("MenuService.CreateSidebarMenuViewModel", 
                    $"Menü oluşturulurken hata: {ex.Message}, MenuID: {menu?.MenuID}, MenuAd: {menu?.Ad ?? "İsimsiz"}, Stack: {ex.StackTrace}").Wait();
                
                // Hata durumunda basit bir menü oluştur ve döndür, böylece tüm menü sistemi çökmez
                if (menu != null)
                {
                    return new MenuViewModel {
                        MenuID = menu.MenuID,
                        Ad = menu.Ad ?? "Hata - Menü",
                        Icon = "error",
                        Url = "#",
                        AktifMi = true,
                        Sira = menu.Sira,
                        AltMenuler = new List<MenuViewModel>()
                    };
                }
                
                return null;
            }
        }

        // Yardımcı metot - Menu oluşturma işlemini kolaylaştırır ve null kontrollerini otomatikleştirir
        private Data.Entities.Menu CreateMenu(string ad, string icon, string controller, string action, int sira, Guid? ustMenuId, string url)
        {
            return new Data.Entities.Menu
            {
                MenuID = Guid.NewGuid(),
                Ad = ad,
                Icon = icon,
                Controller = controller,
                Action = action,
                Sira = sira,
                UstMenuID = ustMenuId,
                Url = url,
                AktifMi = true,
                OlusturmaTarihi = DateTime.Now
            };
        }

        /// <summary>
        /// Veritabanı bağlantısını ve gerekli kolonların varlığını kontrol eder
        /// </summary>
        /// <returns>Sonuç bilgisi</returns>
        public async Task<bool> CheckDatabaseConnection()
        {
            try
            {
                _logger.LogInformation("Veritabanı bağlantısı kontrol ediliyor...");
                
                // Veritabanına bağlanabildiğimizi doğrula
                bool canConnect = await _context.Database.CanConnectAsync();
                
                if (!canConnect)
                {
                    _logger.LogError("Veritabanına bağlantı sağlanamadı.");
                    return false;
                }
                
                _logger.LogInformation("Veritabanına başarıyla bağlanıldı.");
                
                // Silindi kolonunun varlığını kontrol et
                bool hasSilindiColumn = false;
                
                try {
                    // SQL Injection'a karşı parametre kullanarak sorgu
                    string sql = "SELECT COUNT(*) FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = @tableName AND COLUMN_NAME = @columnName";
                    var parameters = new[] { 
                        new SqlParameter("@tableName", "Menuler"),
                        new SqlParameter("@columnName", "Silindi")
                    };
                    
                    var result = await _context.Database.ExecuteSqlRawAsync(sql, parameters);
                    hasSilindiColumn = result > 0;
                    
                    _logger.LogInformation($"'Silindi' kolonu kontrolü: {(hasSilindiColumn ? "Var" : "Yok")}");
                }
                catch (Exception ex) {
                    _logger.LogError(ex, "Silindi kolonu kontrol edilirken hata oluştu");
                    // Hatayı yut, bu sadece kontrol amaçlı
                }
                
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Veritabanı bağlantı kontrolü sırasında hata oluştu");
                return false;
            }
        }
    }
} 