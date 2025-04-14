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

                // Transaction içinde menüleri oluştur
                using (var transaction = await _context.Database.BeginTransactionAsync())
                {
                    try
                    {
                        // 1. Ana Sayfa Menüsü
                        var dashboardMenu = CreateMenu(
                            "Dashboard", 
                            "fas fa-tachometer-alt", 
                            "Home", 
                            "Index", 
                            1, 
                            null, 
                            "/Home/Index"
                        );

                        // 2. Tanımlamalar Menüsü (Üst menü)
                        var tanimlamalarMenu = CreateMenu(
                            "Tanımlamalar", 
                            "fas fa-tags", 
                            "", 
                            "", 
                            2, 
                            null, 
                            "#"
                        );

                        // Tanımlamalar Alt Menüleri
                        var birimMenu = CreateMenu(
                            "Birimler",
                            "fas fa-ruler",
                            "Birim",
                            "Index",
                            1,
                            tanimlamalarMenu.MenuID,
                            "/Birim/Index"
                        );

                        var depoMenu = CreateMenu(
                            "Depolar",
                            "fas fa-warehouse",
                            "Depo",
                            "Index",
                            2,
                            tanimlamalarMenu.MenuID,
                            "/Depo/Index"
                        );

                        var kategoriMenu = CreateMenu(
                            "Ürün Kategorileri",
                            "fas fa-sitemap",
                            "UrunKategori",
                            "Index",
                            3,
                            tanimlamalarMenu.MenuID,
                            "/UrunKategori/Index"
                        );

                        // 3. Stok Yönetimi Menüsü (Üst menü)
                        var stokYonetimiMenu = CreateMenu(
                            "Stok Yönetimi", 
                            "fas fa-boxes", 
                            "", 
                            "", 
                            3, 
                            null, 
                            "#"
                        );

                        // Stok Yönetimi Alt Menüleri
                        var urunlerMenu = CreateMenu(
                            "Ürünler", 
                            "fas fa-box-open", 
                            "Urun", 
                            "Index", 
                            1, 
                            stokYonetimiMenu.MenuID, 
                            "/Urun/Index"
                        );

                        var stokHareketleriMenu = CreateMenu(
                            "Stok Kartları", 
                            "fas fa-clipboard-list", 
                            "Stok", 
                            "Index", 
                            2, 
                            stokYonetimiMenu.MenuID, 
                            "/Stok/Index"
                        );

                        var stokDurumuMenu = CreateMenu(
                            "Stok Durumu", 
                            "fas fa-chart-pie", 
                            "Stok", 
                            "StokDurumu", 
                            3, 
                            stokYonetimiMenu.MenuID, 
                            "/Stok/StokDurumu"
                        );

                        // Stok Hareketleri alt menü
                        var stokIslemMenu = CreateMenu(
                            "Stok Hareketleri", 
                            "fas fa-exchange-alt", 
                            "", 
                            "", 
                            4, 
                            stokYonetimiMenu.MenuID, 
                            "#"
                        );

                        // Stok Hareketleri alt menüleri
                        var stokGirisMenu = CreateMenu(
                            "Stok Giriş", 
                            "fas fa-plus-circle", 
                            "Stok", 
                            "StokGiris", 
                            1, 
                            stokIslemMenu.MenuID, 
                            "/Stok/StokGiris"
                        );

                        var stokCikisMenu = CreateMenu(
                            "Stok Çıkış", 
                            "fas fa-minus-circle", 
                            "Stok", 
                            "StokCikis", 
                            2, 
                            stokIslemMenu.MenuID, 
                            "/Stok/StokCikis"
                        );

                        var stokTransferMenu = CreateMenu(
                            "Stok Transfer", 
                            "fas fa-random", 
                            "Stok", 
                            "StokTransfer", 
                            3, 
                            stokIslemMenu.MenuID, 
                            "/Stok/StokTransfer"
                        );

                        var stokSayimMenu = CreateMenu(
                            "Stok Sayım", 
                            "fas fa-tasks", 
                            "Stok", 
                            "StokSayim", 
                            4, 
                            stokIslemMenu.MenuID, 
                            "/Stok/StokSayim"
                        );

                        // Ürün Fiyat menüsü
                        var urunFiyatMenu = CreateMenu(
                            "Ürün Fiyatları",
                            "fas fa-tag",
                            "UrunFiyat",
                            "Index",
                            5,
                            stokYonetimiMenu.MenuID,
                            "/UrunFiyat/Index"
                        );

                        // 4. Cari Hesap Menüsü
                        var cariHesapMenu = CreateMenu(
                            "Cari Hesap", 
                            "fas fa-users", 
                            "", 
                            "", 
                            4, 
                            null, 
                            "#"
                        );

                        // Cari Hesap alt menüleri
                        var tumCarilerMenu = CreateMenu(
                            "Tüm Cariler", 
                            "fas fa-address-book", 
                            "Cari", 
                            "Index", 
                            1, 
                            cariHesapMenu.MenuID, 
                            "/Cari/Index"
                        );

                        var musterilerMenu = CreateMenu(
                            "Müşteriler", 
                            "fas fa-user-tie", 
                            "Cari", 
                            "Musteriler", 
                            2, 
                            cariHesapMenu.MenuID, 
                            "/Cari/Musteriler"
                        );

                        var tedarikcilerMenu = CreateMenu(
                            "Tedarikçiler", 
                            "fas fa-truck", 
                            "Cari", 
                            "Tedarikciler", 
                            3, 
                            cariHesapMenu.MenuID, 
                            "/Cari/Tedarikciler"
                        );

                        // 5. Belgeler Menüsü (Üst menü)
                        var belgelerMenu = CreateMenu(
                            "Belgeler", 
                            "fas fa-file-alt", 
                            "", 
                            "", 
                            5, 
                            null, 
                            "#"
                        );

                        // Belgeler alt menüleri
                        var faturalarMenu = CreateMenu(
                            "Faturalar", 
                            "fas fa-file-invoice-dollar", 
                            "Fatura", 
                            "Index", 
                            1, 
                            belgelerMenu.MenuID, 
                            "/Fatura/Index"
                        );

                        // İrsaliye alt menüsü
                        var irsaliyeMenu = CreateMenu(
                            "İrsaliyeler",
                            "fas fa-truck-loading",
                            "Irsaliye",
                            "Index",
                            2,
                            belgelerMenu.MenuID,
                            "/Irsaliye/Index"
                        );

                        var sozlesmelerMenu = CreateMenu(
                            "Sözleşmeler",
                            "fas fa-file-signature",
                            "Sozlesme",
                            "Index",
                            3,
                            belgelerMenu.MenuID,
                            "/Sozlesme/Index"
                        );

                        var faturaAklamaMenu = CreateMenu(
                            "Fatura Aklama",
                            "fas fa-check-double",
                            "Fatura",
                            "Aklama",
                            4,
                            belgelerMenu.MenuID,
                            "/Fatura/Aklama"
                        );

                        // 6. Finans Yönetimi (Üst menü)
                        var finansMenu = CreateMenu(
                            "Finans Yönetimi", 
                            "fas fa-money-bill-wave", 
                            "", 
                            "", 
                            6, 
                            null, 
                            "#"
                        );

                        // Finans alt menüleri
                        var kasaMenu = CreateMenu(
                            "Kasa İşlemleri", 
                            "fas fa-cash-register", 
                            "Kasa", 
                            "Index", 
                            1, 
                            finansMenu.MenuID, 
                            "/Kasa/Index"
                        );

                        // Banka alt menüsü
                        var bankaMenu = CreateMenu(
                            "Banka İşlemleri", 
                            "fas fa-university", 
                            "Banka", 
                            "Index", 
                            2, 
                            finansMenu.MenuID, 
                            "/Banka/Index"
                        );

                        var bankaHesaplarMenu = CreateMenu(
                            "Banka Hesapları", 
                            "fas fa-landmark", 
                            "Banka", 
                            "Hesaplar", 
                            3, 
                            finansMenu.MenuID, 
                            "/Banka/Hesaplar"
                        );

                        // 7. Para Birimi Yönetimi (Üst menü)
                        var paraBirimiYonetimMenu = CreateMenu(
                            "Para Birimi Yönetimi", 
                            "fas fa-coins", 
                            "", 
                            "", 
                            7, 
                            null, 
                            "#"
                        );

                        // Para Birimi alt menüleri
                        var paraBirimiMenu = CreateMenu(
                            "Para Birimleri", 
                            "fas fa-dollar-sign", 
                            "ParaBirimi", 
                            "Index", 
                            1, 
                            paraBirimiYonetimMenu.MenuID, 
                            "/ParaBirimi/Index"
                        );

                        var dovizKurlariMenu = CreateMenu(
                            "Döviz Kurları", 
                            "fas fa-exchange-alt", 
                            "ParaBirimi", 
                            "Kurlar", 
                            2, 
                            paraBirimiYonetimMenu.MenuID, 
                            "/ParaBirimi/Kurlar"
                        );

                        var paraBirimiIliskiMenu = CreateMenu(
                            "Para Birimi İlişkileri", 
                            "fas fa-link", 
                            "ParaBirimi", 
                            "Iliskiler", 
                            3, 
                            paraBirimiYonetimMenu.MenuID, 
                            "/ParaBirimi/Iliskiler"
                        );

                        // 8. Raporlar Menüsü (Üst menü)
                        var raporlarMenu = CreateMenu(
                            "Raporlar", 
                            "fas fa-chart-bar", 
                            "", 
                            "", 
                            8, 
                            null, 
                            "#"
                        );

                        // Raporlar alt menüleri
                        var genelBakisRaporMenu = CreateMenu(
                            "Genel Bakış", 
                            "fas fa-chart-line", 
                            "Rapor", 
                            "Index", 
                            1, 
                            raporlarMenu.MenuID, 
                            "/Rapor/Index"
                        );

                        var stokRaporMenu = CreateMenu(
                            "Stok Raporu", 
                            "fas fa-boxes", 
                            "Stok", 
                            "StokRapor", 
                            2, 
                            raporlarMenu.MenuID, 
                            "/Stok/StokRapor"
                        );

                        // Satış raporu alt menüsü
                        var satisRaporMenu = CreateMenu(
                            "Satış Raporu", 
                            "fas fa-chart-line", 
                            "Rapor", 
                            "SatisRaporu", 
                            3, 
                            raporlarMenu.MenuID, 
                            "/Rapor/SatisRaporu"
                        );

                        var cariRaporMenu = CreateMenu(
                            "Cari Raporu", 
                            "fas fa-users", 
                            "Rapor", 
                            "CariRaporu", 
                            4, 
                            raporlarMenu.MenuID, 
                            "/Rapor/CariRaporu"
                        );

                        var kasaRaporMenu = CreateMenu(
                            "Kasa Raporu", 
                            "fas fa-cash-register", 
                            "Rapor", 
                            "KasaRaporu", 
                            5, 
                            raporlarMenu.MenuID, 
                            "/Rapor/KasaRaporu"
                        );

                        var bankaRaporMenu = CreateMenu(
                            "Banka Raporu", 
                            "fas fa-university", 
                            "Rapor", 
                            "BankaRaporu", 
                            6, 
                            raporlarMenu.MenuID, 
                            "/Rapor/BankaRaporu"
                        );

                        // 9. Yönetim Paneli (Üst menü)
                        var yonetimPaneliMenu = CreateMenu(
                            "Yönetim Paneli", 
                            "fas fa-cogs", 
                            "", 
                            "", 
                            9, 
                            null, 
                            "#"
                        );

                        // Yönetim alt menüleri
                        var kullaniciYonetimMenu = CreateMenu(
                            "Kullanıcı Yönetimi", 
                            "fas fa-users-cog", 
                            "Kullanici", 
                            "Index", 
                            1, 
                            yonetimPaneliMenu.MenuID, 
                            "/Kullanici/Index"
                        );

                        var rolYonetimMenu = CreateMenu(
                            "Rol Yönetimi", 
                            "fas fa-user-tag", 
                            "Kullanici", 
                            "Roller", 
                            2, 
                            yonetimPaneliMenu.MenuID, 
                            "/Kullanici/Roller"
                        );

                        var menuYonetimMenu = CreateMenu(
                            "Menü Yönetimi", 
                            "fas fa-bars", 
                            "Menu", 
                            "Index", 
                            3, 
                            yonetimPaneliMenu.MenuID, 
                            "/Menu/Index"
                        );

                        var sistemAyarlariMenu = CreateMenu(
                            "Sistem Ayarları", 
                            "fas fa-sliders-h", 
                            "SistemAyar", 
                            "Index", 
                            4, 
                            yonetimPaneliMenu.MenuID, 
                            "/SistemAyar/Index"
                        );

                        var sistemLogMenu = CreateMenu(
                            "Sistem Logları", 
                            "fas fa-history", 
                            "SistemLog", 
                            "Index", 
                            5, 
                            yonetimPaneliMenu.MenuID, 
                            "/SistemLog/Index"
                        );

                        var dbYonetimMenu = CreateMenu(
                            "Veritabanı Yönetimi", 
                            "fas fa-database", 
                            "DbInit", 
                            "Index", 
                            6, 
                            yonetimPaneliMenu.MenuID, 
                            "/DbInit/Index"
                        );

                        // Tüm menüleri veritabanına ekle
                        var menuler = new List<Data.Entities.Menu>
                        {
                            // Üst menüler
                            dashboardMenu,
                            tanimlamalarMenu,
                            stokYonetimiMenu,
                            cariHesapMenu,
                            belgelerMenu,
                            finansMenu,
                            paraBirimiYonetimMenu,
                            raporlarMenu,
                            yonetimPaneliMenu,
                            
                            // 2. Tanımlamalar alt menüleri
                            birimMenu,
                            depoMenu,
                            kategoriMenu,
                            
                            // 3. Stok alt menüleri
                            urunlerMenu,
                            stokHareketleriMenu,
                            stokDurumuMenu,
                            stokIslemMenu,
                            stokGirisMenu,
                            stokCikisMenu,
                            stokTransferMenu,
                            stokSayimMenu,
                            urunFiyatMenu,
                            
                            // 4. Cari alt menüleri
                            tumCarilerMenu,
                            musterilerMenu,
                            tedarikcilerMenu,
                            
                            // 5. Belgeler alt menüleri
                            faturalarMenu,
                            irsaliyeMenu,
                            sozlesmelerMenu,
                            faturaAklamaMenu,
                            
                            // 6. Finans alt menüleri
                            kasaMenu,
                            bankaMenu,
                            bankaHesaplarMenu,
                            
                            // 7. Para birimi alt menüleri
                            paraBirimiMenu,
                            dovizKurlariMenu,
                            paraBirimiIliskiMenu,
                            
                            // 8. Raporlar alt menüleri
                            genelBakisRaporMenu,
                            stokRaporMenu,
                            satisRaporMenu,
                            cariRaporMenu,
                            kasaRaporMenu,
                            bankaRaporMenu,
                            
                            // 9. Yönetim alt menüleri
                            kullaniciYonetimMenu,
                            rolYonetimMenu,
                            menuYonetimMenu,
                            sistemAyarlariMenu,
                            sistemLogMenu,
                            dbYonetimMenu
                        };

                        foreach (var menu in menuler)
                        {
                            _context.Menuler.Add(menu);
                        }

                        // Önce menüleri kaydet
                        await _context.SaveChangesAsync();

                        // Sonra menü-rol ilişkilerini oluştur
                        List<MenuRol> menuRoller = new List<MenuRol>();

                        foreach (var menu in menuler)
                        {
                            var menuRol = new MenuRol
                            {
                                MenuRolID = Guid.NewGuid(),
                                MenuId = menu.MenuID,
                                RolId = adminRole.Id,
                                OlusturmaTarihi = DateTime.Now
                            };
                            
                            menuRoller.Add(menuRol);
                        }

                        // Menü-rol ilişkilerini kaydet
                        foreach (var menuRol in menuRoller)
                        {
                            _context.MenuRoller.Add(menuRol);
                        }

                        await _context.SaveChangesAsync();
                        
                        // Transaction'ı onaylayarak işlemi tamamla
                        await transaction.CommitAsync();
                        
                        try 
                        {
                            await _logService.LogInfoAsync("MenuService.InitDefaultMenusAsync", "Varsayılan menüler başarıyla oluşturuldu!");
                        } 
                        catch (Exception logEx)
                        {
                            // Log hatası olsa bile işlemi başarılı sayalım
                            _logger.LogWarning(logEx, "Log kaydetme işlemi sırasında hata oluştu, ancak menüler başarıyla oluşturuldu: {Message}", logEx.Message);
                        }
                        
                        return true;
                    }
                    catch (Exception ex)
                    {
                        // Hata durumunda transaction'ı geri al
                        await transaction.RollbackAsync();
                        
                        string errorMessage = $"Menüler kaydedilirken hata: {ex.Message}";
                        if (ex.InnerException != null)
                        {
                            errorMessage += $"\nİç Hata: {ex.InnerException.Message}";
                        }
                        
                        _logger.LogError(ex, "Menüler kaydedilirken hata oluştu: {Message}", errorMessage);
                        await _logService.LogErrorAsync("MenuService.InitDefaultMenusAsync", errorMessage);
                        
                        throw; // Hatayı dışarı fırlat
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
                            bool menuHasAccess = rootMenu.MenuRoller.Any(mr => userRoleIds.Contains(mr.RolId));
                            
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

        public async Task<List<Data.Entities.Menu>> GetAllMenusWithIncludesAsync()
        {
            return await _context.Menuler
                .Include(m => m.MenuRoller)
                .AsSplitQuery()
                .OrderBy(m => m.Sira)
                .ToListAsync();
        }

        public async Task<Data.Entities.Menu?> GetMenuByIdWithIncludesAsync(Guid id)
        {
            return await _context.Menuler
                .Include(m => m.UstMenu)
                .Include(m => m.AltMenuler)
                .Include(m => m.MenuRoller)
                .AsSplitQuery()
                .FirstOrDefaultAsync(m => m.MenuID == id);
        }

        public async Task<List<Data.Entities.Menu>> GetMenusByRolAsync(string rolAdi)
        {
            return await _context.Menuler
                .Include(m => m.AltMenuler)
                .Include(m => m.MenuRoller)
                .AsSplitQuery()
                .Where(m => m.MenuRoller.Any(mr => mr.Rol.Name == rolAdi))
                .OrderBy(m => m.Sira)
                .ToListAsync();
        }

        public async Task<IEnumerable<Data.Entities.Menu>> GetActiveMenusByRolAsync(string rolAdi)
        {
            var menuler = await _context.Menuler
                .Include(m => m.MenuRoller)
                .AsSplitQuery()
                .Where(m => m.AktifMi && m.MenuRoller.Any(mr => mr.Rol.Name == rolAdi))
                .OrderBy(m => m.Sira)
                .ToListAsync();

            return menuler;
        }

        public async Task<List<MenuItem>> GetMenuItemsAsync(string rolYetki)
        {
            try
            {
                var menuItems = new List<MenuItem>();
                
                // Tüm menü öğelerini veritabanından çek
                var menuler = await _context.Menuler
                    .Include(m => m.MenuRoller)
                        .ThenInclude(mr => mr.Rol)
                    .Where(m => !m.Silindi)
                    .OrderBy(m => m.Sira)  // MenuSirasi yerine Sira kullanıldı
                    .ToListAsync();

                if (menuler == null || !menuler.Any())
                {
                    _logger.LogWarning("Hiç menü bulunamadı");
                    return new List<MenuItem>();
                }

                // Yetkiye göre filtreleme
                if (!string.IsNullOrEmpty(rolYetki))
                {
                    menuler = menuler
                        .Where(m => m.AktifMi && m.MenuRoller.Any(mr => mr.Rol.Name == rolYetki))  // Aktif yerine AktifMi, RolAdi yerine Rol.Name
                        .OrderBy(m => m.Sira)  // MenuSirasi yerine Sira
                        .ToList();
                }

                // Yalnızca üst seviye menüleri al (UstMenuID null)
                var ustMenuler = menuler.Where(m => m.UstMenuID == null).ToList();

                foreach (var menu in ustMenuler)
                {
                    var menuItem = new MenuItem
                    {
                        Id = menu.MenuID.ToString(),
                        Text = menu.Ad,
                        Icon = menu.Icon,
                        Url = menu.Url,
                        Action = menu.Action,
                        Controller = menu.Controller
                    };

                    // Alt menüleri ekle
                    AddSubMenuItems(menuItem, menuler, menu.MenuID);
                    menuItems.Add(menuItem);
                }

                return menuItems;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "GetMenuItemsAsync metodu içinde bir hata oluştu");
                return new List<MenuItem>();
            }
        }

        private void AddSubMenuItems(MenuItem parentItem, List<Data.Entities.Menu> allMenus, Guid parentMenuId)
        {
            var childMenus = allMenus.Where(m => m.UstMenuID == parentMenuId).OrderBy(m => m.Sira).ToList();
            
            foreach (var childMenu in childMenus)
            {
                var childItem = new MenuItem
                {
                    Id = childMenu.MenuID.ToString(),
                    Text = childMenu.Ad,
                    Icon = childMenu.Icon,
                    Url = childMenu.Url,
                    Action = childMenu.Action,
                    Controller = childMenu.Controller
                };
                
                // Recursively add sub-items
                AddSubMenuItems(childItem, allMenus, childMenu.MenuID);
                parentItem.Items.Add(childItem);
            }
        }
    }
} 