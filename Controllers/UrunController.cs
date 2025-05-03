using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MuhasebeStokWebApp.Data;
using MuhasebeStokWebApp.Data.Entities;
using MuhasebeStokWebApp.Data.Repositories;
using MuhasebeStokWebApp.ViewModels.Urun;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Identity;
using MuhasebeStokWebApp.Services;
using MuhasebeStokWebApp.Services.Interfaces;
using MuhasebeStokWebApp.Enums;
using Microsoft.Extensions.Logging;
using ClosedXML.Excel;
using System.IO;

namespace MuhasebeStokWebApp.Controllers
{
    // Ürün yönetimi işlemlerini yöneten controller sınıfı
    [Authorize]
    public class UrunController : BaseController
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ApplicationDbContext _context;
        private readonly ILogger<UrunController> _logger;
        private readonly IDropdownService _dropdownService;

        // Constructor: Repository ve veritabanı bağlantısını DI ile alır
        public UrunController(
            IUnitOfWork unitOfWork, 
            ApplicationDbContext context,
            IMenuService menuService,
            UserManager<ApplicationUser> userManager,
            RoleManager<IdentityRole> roleManager,
            ILogService logService,
            ILogger<UrunController> logger,
            IDropdownService dropdownService)
            : base(menuService, userManager, roleManager, logService)
        {
            _unitOfWork = unitOfWork;
            _context = context;
            _logger = logger;
            _dropdownService = dropdownService;
        }

        // Ürünlerin listelendiği ana sayfa
        public async Task<IActionResult> Index(Guid? kategoriID = null, string urunAdi = null, string urunKodu = null, bool? aktifMi = null, string tab = "aktif")
        {
            var urunRepository = _unitOfWork.Repository<Urun>();
            var urunFiyatRepository = _unitOfWork.Repository<UrunFiyat>();
            var kategoriRepository = _unitOfWork.Repository<UrunKategori>();

            // ApplicationDbContext'te HasQueryFilter ile varsayılan filtre uygulandığı için
            // doğrudan DbContext'i kullanarak IgnoreQueryFilters() ile tüm ürünleri getirelim
            var tümUrunler = await _context.Urunler
                .IgnoreQueryFilters()
                .Include("Birim")
                .ToListAsync();
            
            // Silinmemiş tüm kategorileri getir
            var kategoriler = await kategoriRepository.GetAsync(
                filter: k => k.Silindi == false,
                orderBy: q => q.OrderBy(k => k.KategoriAdi)
            );
            
            // Ürün fiyatlarını getir
            var urunFiyatlar = await urunFiyatRepository.GetAllAsync();
            
            // Ürün listesi görünüm modeli oluştur
            var viewModel = new UrunListViewModel
            {
                Urunler = tümUrunler.Select(u => new UrunViewModel
                    {
                        UrunID = u.UrunID,
                        UrunKodu = u.UrunKodu,
                        UrunAdi = u.UrunAdi,
                        Birim = u.Birim != null ? u.Birim.BirimAdi : string.Empty,
                        Miktar = u.StokMiktar,
                        // Her ürün için en güncel liste fiyatını getir
                        ListeFiyati = urunFiyatlar
                            .Where(uf => uf.UrunID == u.UrunID && uf.FiyatTipiID == 1 && uf.Silindi == false)
                            .OrderByDescending(uf => uf.GecerliTarih)
                            .FirstOrDefault()?.Fiyat ?? 0m,
                        // Her ürün için en güncel maliyet fiyatını getir
                        MaliyetFiyati = urunFiyatlar
                            .Where(uf => uf.UrunID == u.UrunID && uf.FiyatTipiID == 2 && uf.Silindi == false)
                            .OrderByDescending(uf => uf.GecerliTarih)
                            .FirstOrDefault()?.Fiyat ?? 0m,
                        // Her ürün için en güncel satış fiyatını getir
                        SatisFiyati = urunFiyatlar
                            .Where(uf => uf.UrunID == u.UrunID && uf.FiyatTipiID == 3 && uf.Silindi == false)
                            .OrderByDescending(uf => uf.GecerliTarih)
                            .FirstOrDefault()?.Fiyat ?? 0m,
                        Aktif = u.Aktif,
                        Silindi = u.Silindi,
                        OlusturmaTarihi = u.OlusturmaTarihi,
                        KategoriID = u.KategoriID,
                        // Ürün kategorisinin adını bul
                        KategoriAdi = u.KategoriID.HasValue ? 
                            kategoriler.FirstOrDefault(k => k.KategoriID == u.KategoriID)?.KategoriAdi : "Kategorisiz"
                }).ToList(),
                
                // Filtre özellikleri ViewModel'e taşındı
                KategoriID = kategoriID,
                UrunAdi = urunAdi,
                UrunKodu = urunKodu,
                AktifMi = aktifMi,
                AktifTab = tab
            };
            
            // Kategorileri DropdownService üzerinden al
            viewModel.Kategoriler = await _dropdownService.GetKategoriSelectItemsAsync(kategoriID);

            // Önce tab filtresi uygula - Aktif, Pasif ve Silinmiş ürünleri sekmelerine göre filtrele
            if (tab == "aktif")
            {
                viewModel.Urunler = viewModel.Urunler.Where(u => u.Aktif && !u.Silindi).ToList();
            }
            else if (tab == "pasif")
            {
                viewModel.Urunler = viewModel.Urunler.Where(u => !u.Aktif && !u.Silindi).ToList();
            }
            else if (tab == "silindi")
            {
                viewModel.Urunler = viewModel.Urunler.Where(u => u.Silindi).ToList();
            }

            // Diğer filtreleri uygula
            // Kategori filtresi varsa uygula
            if (kategoriID.HasValue)
            {
                viewModel.Urunler = viewModel.Urunler.Where(u => u.KategoriID == kategoriID.Value).ToList();
            }
            
            // Ürün adı filtresi varsa uygula
            if (!string.IsNullOrWhiteSpace(urunAdi))
            {
                viewModel.Urunler = viewModel.Urunler.Where(u => u.UrunAdi.Contains(urunAdi, StringComparison.OrdinalIgnoreCase)).ToList();
            }
            
            // Ürün kodu filtresi varsa uygula
            if (!string.IsNullOrWhiteSpace(urunKodu))
            {
                viewModel.Urunler = viewModel.Urunler.Where(u => u.UrunKodu.Contains(urunKodu, StringComparison.OrdinalIgnoreCase)).ToList();
            }
            
            // Aktiflik durumu filtresi varsa uygula
            if (aktifMi.HasValue)
            {
                viewModel.Urunler = viewModel.Urunler.Where(u => u.Aktif == aktifMi.Value).ToList();
            }

            return View(viewModel);
        }

        // Ürün detaylarını gösterir
        public async Task<IActionResult> Details(Guid id, bool returnView = true)
        {
            var urunRepository = _unitOfWork.Repository<Urun>();
            var kategoriRepository = _unitOfWork.Repository<UrunKategori>();
            var urunFiyatRepository = _unitOfWork.Repository<UrunFiyat>();

            var urun = await urunRepository.GetFirstOrDefaultAsync(u => u.UrunID == id && u.Silindi == false, includeProperties: "Birim");
            if (urun == null)
            {
                return NotFound();
            }

            // Kategori bilgisi
            UrunKategori? kategori = null;
            if (urun.KategoriID.HasValue)
            {
                kategori = await kategoriRepository.GetByIdAsync(urun.KategoriID.Value);
            }
            
            var viewModel = new UrunViewModel
            {
                UrunID = urun.UrunID,
                UrunKodu = urun.UrunKodu,
                UrunAdi = urun.UrunAdi,
                Birim = urun.Birim != null ? urun.Birim.BirimAdi : string.Empty,
                Miktar = urun.StokMiktar,
                // Ürün için en güncel liste fiyatını getir
                ListeFiyati = urunFiyatRepository.GetAllAsync().Result
                    .Where(uf => uf.UrunID == urun.UrunID && uf.FiyatTipiID == 1 && uf.Silindi == false)
                    .OrderByDescending(uf => uf.GecerliTarih)
                    .FirstOrDefault()?.Fiyat ?? 0m,
                // Ürün için en güncel maliyet fiyatını getir
                MaliyetFiyati = urunFiyatRepository.GetAllAsync().Result
                    .Where(uf => uf.UrunID == urun.UrunID && uf.FiyatTipiID == 2 && uf.Silindi == false)
                    .OrderByDescending(uf => uf.GecerliTarih)
                    .FirstOrDefault()?.Fiyat ?? 0m,
                // Ürün için en güncel satış fiyatını getir
                SatisFiyati = urunFiyatRepository.GetAllAsync().Result
                    .Where(uf => uf.UrunID == urun.UrunID && uf.FiyatTipiID == 3 && uf.Silindi == false)
                    .OrderByDescending(uf => uf.GecerliTarih)
                    .FirstOrDefault()?.Fiyat ?? 0m,
                KategoriAdi = kategori?.KategoriAdi ?? "Kategorisiz",
                OlusturmaTarihi = urun.OlusturmaTarihi,
                GuncellemeTarihi = urun.GuncellemeTarihi,
                Aktif = urun.Aktif,
                KdvOrani = urun.KDVOrani,
                KritikStokSeviyesi = urun.KritikStokSeviyesi,
                Aciklama = urun.Aciklama
            };
            
            // AJAX isteği için modal içeriği döndür
            if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
            {
                return PartialView("_DetailsPartial", viewModel);
            }
            
            if (!returnView)
            {
                return Ok(viewModel);
            }
            return View(viewModel);
        }

        // Yeni ürün oluşturma formu
        public async Task<IActionResult> Create()
        {
            // DropdownService üzerinden kategori ve birim listelerini al
            var kategoriler = await _dropdownService.GetKategoriSelectItemsAsync();
            var birimler = await _dropdownService.GetBirimSelectItemsAsync();
            
            // ViewModel oluştur ve içine kategorileri ve birimleri ekle
            var model = new UrunCreateViewModel
            {
                Aktif = true,
                KDVOrani = 12, // Varsayılan KDV oranı 12 olarak ayarlandı
                KritikStokSeviyesi = 100, // Varsayılan kritik stok seviyesi 100 olarak ayarlandı
                Kategoriler = kategoriler,
                Birimler = birimler,
                // Geriye uyumluluk için eski listeler de doldurulur
                KategoriListesi = kategoriler,
                BirimListesi = birimler
            };
            
            // AJAX isteğiyse partial view döndür
            if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
            {
                return PartialView("_CreatePartial", model);
            }
            
            return View(model);
        }

        // Yeni ürün oluşturma işlemi
        [Authorize(Roles = "Admin,StokYonetici,Kullanici")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(UrunCreateViewModel model)
        {
            // İşlem başlangıcında logging
            _logger.LogInformation($"Yeni ürün oluşturma işlemi başlatılıyor: Kullanıcı={User.Identity.Name}");
            
            // Birim ve kategori zorunlu alanlar - Guid için null kontrolü
            if (model.BirimID == Guid.Empty)
            {
                ModelState.AddModelError("BirimID", "Birim seçimi zorunludur.");
            }
            
            if (model.KategoriID == Guid.Empty)
            {
                ModelState.AddModelError("KategoriID", "Kategori seçimi zorunludur.");
            }
            
            // Gereksiz validasyon alanlarını kaldır
            ModelState.Remove("BirimListesi");
            ModelState.Remove("KategoriListesi");
            ModelState.Remove("Aciklama"); // Açıklamayı zorunlu olmaktan çıkar
            
            if (!ModelState.IsValid)
            {
                // Eğer AJAX isteği ise hata mesajlarını döndür
                if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
                {
                    return Json(new { success = false, message = "Validasyon hatası", errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList() });
                }
                
                // Dropdown list verilerini hazırla
                await ListeleriDoldur();
                return View(model);
            }

            try
                {
                    var urunRepository = _unitOfWork.Repository<Urun>();
                    
                // Ürün kodu kontrol et
                var existingUrun = await urunRepository.GetFirstOrDefaultAsync(u => u.UrunKodu == model.UrunKodu && !u.Silindi);
                    if (existingUrun != null)
                    {
                    ModelState.AddModelError("UrunKodu", "Bu ürün kodu zaten kullanılıyor.");
                    
                    // Eğer AJAX isteği ise hata mesajı döndür
                    if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
                    {
                        return Json(new { success = false, message = "Bu ürün kodu zaten kullanılıyor.", errors = new List<string> { "Bu ürün kodu zaten kullanılıyor." } });
                    }
                    
                    // Dropdown list verilerini hazırla
                    await ListeleriDoldur();
                        return View(model);
                    }
                    
                    // Yeni ürün oluştur
                var yeniUrun = new Urun
                    {
                        UrunID = Guid.NewGuid(),
                        UrunKodu = model.UrunKodu,
                        UrunAdi = model.UrunAdi,
                        Aciklama = model.Aciklama ?? string.Empty,
                        BirimID = model.BirimID,
                        KategoriID = model.KategoriID,
                        KDVOrani = (int)model.KDVOrani,
                        KritikStokSeviyesi = model.KritikStokSeviyesi,
                        Aktif = model.Aktif,
                        OlusturmaTarihi = DateTime.Now,
                        OlusturanKullaniciID = GetCurrentUserId()
                    };
                    
                await urunRepository.AddAsync(yeniUrun);
                
                // Veritabanına kaydet
                    await _unitOfWork.SaveAsync();
                    
                // Log oluştur - Daha detaylı bir log için UrunOlusturmaLogOlustur metodunu kullanıyoruz
                await _logService.UrunOlusturmaLogOlustur(
                    yeniUrun.UrunID.ToString(),
                    yeniUrun.UrunAdi,
                    $"Ürün Kodu: {yeniUrun.UrunKodu}, Kategori: {model.KategoriID}, Birim: {model.BirimID}, KDV Oranı: %{yeniUrun.KDVOrani}, Kritik Stok: {yeniUrun.KritikStokSeviyesi}, Açıklama: {yeniUrun.Aciklama}"
                );
                
                // Eğer AJAX isteği ise başarı mesajı döndür
                if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
                {
                    return Json(new { success = true, message = "Ürün başarıyla oluşturuldu.", id = yeniUrun.UrunID });
                }
                
                // Normal post ise, başarı mesajını ekleyip Index sayfasına yönlendir
                TempData["SuccessMessage"] = $"{yeniUrun.UrunAdi} ürünü başarıyla oluşturuldu.";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ürün oluşturma hatası: {Message}", ex.Message);
                
                // Eğer AJAX isteği ise hata mesajı döndür
                if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
                {
                    return Json(new { success = false, message = $"Ürün oluşturulurken bir hata oluştu: {ex.Message}" });
                }
                
                // Normal post ise, hata mesajını ekleyip dropdown list verilerini hazırla
                TempData["ErrorMessage"] = $"Ürün oluşturulurken bir hata oluştu: {ex.Message}";
                await ListeleriDoldur();
                return View(model);
            }
        }

        // Ürün düzenleme formu
        public async Task<IActionResult> Edit(Guid id)
        {
            var urunRepository = _unitOfWork.Repository<Urun>();

            // Ürün bilgisini getir
            var urun = await urunRepository.GetFirstOrDefaultAsync(u => u.UrunID == id && u.Silindi == false);
            if (urun == null)
            {
                return NotFound();
            }

            // DropdownService üzerinden kategori ve birim listelerini al
            var kategoriler = await _dropdownService.GetKategoriSelectItemsAsync(urun.KategoriID);
            var birimler = await _dropdownService.GetBirimSelectItemsAsync(urun.BirimID);
            
            // ViewModel oluştur
            var viewModel = new UrunEditViewModel
            {
                UrunID = urun.UrunID,
                UrunKodu = urun.UrunKodu,
                UrunAdi = urun.UrunAdi,
                Aciklama = urun.Aciklama,
                KategoriID = urun.KategoriID,
                BirimID = urun.BirimID,
                KDVOrani = urun.KDVOrani,
                KritikStokSeviyesi = urun.KritikStokSeviyesi,
                Aktif = urun.Aktif,
                
                // Dropdown listeleri doğrudan ViewModel içine ekle
                Birimler = birimler,
                Kategoriler = kategoriler,
                
                // Geriye uyumluluk için BirimListesi ve KategoriListesi de doldur
                BirimListesi = birimler,
                KategoriListesi = kategoriler
            };

            // AJAX isteği ise partial view döndür
            if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
            {
                return PartialView("_EditPartial", viewModel);
            }
            
            return View(viewModel);
        }

        // Ürün düzenleme işlemi
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(UrunEditViewModel model)
        {
            // Birim seçimi kontrolü - Guid için null kontrolü
            if (model.BirimID == Guid.Empty)
            {
                ModelState.AddModelError("BirimID", "Birim seçimi zorunludur.");
            }
            
            // Gereksiz validasyon alanlarını kaldır
            ModelState.Remove("BirimListesi");
            ModelState.Remove("KategoriListesi");
            ModelState.Remove("Birim");
            ModelState.Remove("Aciklama"); // Açıklamayı zorunlu olmaktan çıkar
            
            if (!ModelState.IsValid)
            {
                // AJAX isteği ise hata mesajı döndür
                if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
                {
                    return Json(new { success = false, message = "Validasyon hatası", errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList() });
                }
                
                // Tekrar dropdown verilerini hazırla
                await ListeleriDoldur();
                
                return View(model);
            }

            try
            {
                var urunRepository = _unitOfWork.Repository<Urun>();
                
                // Ürün bilgisini getir
                var urun = await urunRepository.GetFirstOrDefaultAsync(u => u.UrunID == model.UrunID && !u.Silindi);
                    if (urun == null)
                {
                    // AJAX isteği ise hata mesajı döndür
                    if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
                    {
                        return Json(new { success = false, message = "Ürün bulunamadı." });
                    }
                    
                    return NotFound();
                }

                // Ürün bilgilerini güncelle
                    urun.UrunKodu = model.UrunKodu;
                    urun.UrunAdi = model.UrunAdi;
                urun.Aciklama = model.Aciklama ?? string.Empty;
                    urun.KategoriID = model.KategoriID;
                urun.BirimID = model.BirimID;
                    urun.KDVOrani = (int)model.KDVOrani;
                    urun.KritikStokSeviyesi = model.KritikStokSeviyesi;
                    urun.Aktif = model.Aktif;
                    urun.GuncellemeTarihi = DateTime.Now;
                    urun.SonGuncelleyenKullaniciID = GetCurrentUserId();
                
                await urunRepository.UpdateAsync(urun);
                
                    await _unitOfWork.SaveAsync();
                    
                    // Log oluştur - Daha detaylı bir log için UrunGuncellemeLogOlustur metodunu kullanıyoruz
                    await _logService.UrunGuncellemeLogOlustur(
                        urun.UrunID.ToString(),
                        urun.UrunAdi,
                        $"Ürün Kodu: {urun.UrunKodu}, Kategori: {model.KategoriID}, Birim: {model.BirimID}, KDV Oranı: %{urun.KDVOrani}, Kritik Stok: {urun.KritikStokSeviyesi}, Aktif: {urun.Aktif}, Açıklama: {urun.Aciklama}"
                    );
                    
                    // AJAX isteği ise başarı mesajı döndür
                    if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
                    {
                        return Json(new { success = true, message = "Ürün başarıyla güncellendi." });
                    }
                    
                    TempData["SuccessMessage"] = $"{urun.UrunAdi} ürünü başarıyla güncellendi.";
                    return RedirectToAction(nameof(Index));
                }
                catch (Exception ex)
                {
                _logger.LogError(ex, "Ürün güncelleme hatası: {Message}", ex.Message);
                
                // AJAX isteği ise hata mesajı döndür
                if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
                {
                    return Json(new { success = false, message = $"Ürün güncellenirken bir hata oluştu: {ex.Message}" });
                }
                
                TempData["ErrorMessage"] = $"Ürün güncellenirken bir hata oluştu: {ex.Message}";
                
                // Tekrar dropdown verilerini hazırla
                await ListeleriDoldur();
                
                return View(model);
            }
        }

        // Ürün silme onay sayfası
        public async Task<IActionResult> Delete(Guid id)
        {
            var urunRepository = _unitOfWork.Repository<Urun>();
            var urunFiyatRepository = _unitOfWork.Repository<UrunFiyat>();
            var kategoriRepository = _unitOfWork.Repository<UrunKategori>();

            // Ürün bilgisini getir
            var urun = await urunRepository.GetFirstOrDefaultAsync(u => u.UrunID == id && u.Silindi == false);
            if (urun == null)
            {
                return NotFound();
            }

            // Kategori bilgisini getir
            var kategori = urun.KategoriID.HasValue ? 
                await kategoriRepository.GetFirstOrDefaultAsync(k => k.KategoriID.ToString() == urun.KategoriID.ToString()) : null;

            // Ürün görünüm modeli oluştur
            var viewModel = new UrunViewModel
            {
                UrunID = urun.UrunID,
                UrunKodu = urun.UrunKodu,
                UrunAdi = urun.UrunAdi,
                Birim = urun.Birim != null ? urun.Birim.BirimAdi : string.Empty,
                Miktar = urun.StokMiktar,
                ListeFiyati = urunFiyatRepository.GetAllAsync().Result
                    .Where(uf => uf.UrunID == urun.UrunID && uf.FiyatTipiID == 1 && uf.Silindi == false)
                    .OrderByDescending(uf => uf.GecerliTarih)
                    .FirstOrDefault()?.Fiyat ?? 0m,
                MaliyetFiyati = urunFiyatRepository.GetAllAsync().Result
                    .Where(uf => uf.UrunID == urun.UrunID && uf.FiyatTipiID == 2 && uf.Silindi == false)
                    .OrderByDescending(uf => uf.GecerliTarih)
                    .FirstOrDefault()?.Fiyat ?? 0m,
                SatisFiyati = urunFiyatRepository.GetAllAsync().Result
                    .Where(uf => uf.UrunID == urun.UrunID && uf.FiyatTipiID == 3 && uf.Silindi == false)
                    .OrderByDescending(uf => uf.GecerliTarih)
                    .FirstOrDefault()?.Fiyat ?? 0m,
                Aktif = urun.Aktif,
                OlusturmaTarihi = urun.OlusturmaTarihi,
                KategoriID = urun.KategoriID,
                KategoriAdi = kategori?.KategoriAdi ?? "Kategorisiz"
            };

            return View(viewModel);
        }

        // Ürün silme işlemi (soft delete)
        [Authorize(Roles = "Admin,StokYonetici")]
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(Guid id)
        {
            // Kullanıcının yetkilerini kontrol et
            if (!User.IsInRole("Admin") && !User.IsInRole("StokYonetici"))
            {
                _logger.LogWarning($"Yetkisiz ürün silme girişimi: Kullanıcı={User.Identity.Name}, UrunID={id}");
                return RedirectToAction("AccessDenied", "Account");
            }
            
            try
            {
                // İşlem öncesi logging
                _logger.LogInformation($"Ürün silme işlemi başlatılıyor: UrunID={id}, Kullanıcı={User.Identity.Name}");
                
                // SoftDeleteService üzerinden işlem yap
                var softDeleteService = HttpContext.RequestServices.GetService<ISoftDeleteService<Urun>>();
                if (softDeleteService == null)
                {
                    TempData["ErrorMessage"] = "Silme işlemi için gerekli servis bulunamadı.";
                    return RedirectToAction(nameof(Index));
                }
                
                // İlişkili kayıtlar kontrolü ve silme işlemi
                bool success = await softDeleteService.SoftDeleteByIdAsync(id);
                
                if (success)
                {
                    _logger.LogInformation($"Ürün başarıyla silindi: UrunID={id}, Kullanıcı={User.Identity.Name}");
                    TempData["SuccessMessage"] = "Ürün başarıyla silindi.";
                }
                else
                {
                    _logger.LogWarning($"Ürün silinemedi: UrunID={id}, Kullanıcı={User.Identity.Name}");
                    TempData["WarningMessage"] = "Ürün silme işlemi gerçekleştirilemedi.";
                }
                
                return RedirectToAction(nameof(Index), new { tab = "silindi" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Ürün silme işlemi sırasında hata: UrunID={id}, Kullanıcı={User.Identity.Name}");
                TempData["ErrorMessage"] = "Ürün silinirken bir hata oluştu: " + ex.Message;
                return RedirectToAction(nameof(Index));
            }
        }

        // Pasif ürünü aktifleştirme
        public async Task<IActionResult> Activate(Guid id)
        {
            var urunRepository = _unitOfWork.Repository<Urun>();
            
            // Ürün bilgisini getir
            var urun = await urunRepository.GetFirstOrDefaultAsync(u => u.UrunID == id && u.Silindi == false);
            if (urun == null)
            {
                return NotFound();
            }
            
            // Ürünü aktif yap
            urun.Aktif = true;
            urun.GuncellemeTarihi = DateTime.Now;
            
            await urunRepository.UpdateAsync(urun);
            await _unitOfWork.SaveAsync();
            
            TempData["SuccessMessage"] = $"{urun.UrunAdi} ürünü başarıyla aktifleştirildi.";
            
            return RedirectToAction(nameof(Index));
        }

        // AJAX: Ürün detaylarını JSON formatında getir
        [HttpGet]
        public async Task<IActionResult> GetUrunDetails(Guid id)
        {
            var urunRepository = _unitOfWork.Repository<Urun>();
            var urunFiyatRepository = _unitOfWork.Repository<UrunFiyat>();

            // Ürün bilgisini getir
            var urun = await urunRepository.GetFirstOrDefaultAsync(u => u.UrunID == id && u.Silindi == false);
            if (urun == null)
            {
                return NotFound();
            }

            // Ürün fiyatlarını getir
            var fiyatlar = await urunFiyatRepository.GetAsync(
                filter: f => f.UrunID == urun.UrunID && f.Silindi == false,
                includeProperties: "FiyatTipi"
            );

            // En güncel fiyatları bul
            var listeFiyati = fiyatlar.Where(f => f.FiyatTipi.FiyatTipiID == 1)
                .OrderByDescending(f => f.GecerliTarih)
                .FirstOrDefault()?.Fiyat;

            var maliyetFiyati = fiyatlar.Where(f => f.FiyatTipi.FiyatTipiID == 2)
                .OrderByDescending(f => f.GecerliTarih)
                .FirstOrDefault()?.Fiyat;

            var satisFiyati = fiyatlar.Where(f => f.FiyatTipi.FiyatTipiID == 3)
                .OrderByDescending(f => f.GecerliTarih)
                .FirstOrDefault()?.Fiyat;

            // Ürün görünüm modeli oluştur
            var model = new UrunViewModel
            {
                UrunID = urun.UrunID,
                UrunKodu = urun.UrunKodu,
                UrunAdi = urun.UrunAdi,
                Birim = urun.Birim != null ? urun.Birim.BirimAdi : string.Empty,
                Miktar = urun.StokMiktar,
                Aktif = urun.Aktif,
                ListeFiyati = listeFiyati ?? 0m,
                MaliyetFiyati = maliyetFiyati ?? 0m,
                SatisFiyati = satisFiyati ?? 0m,
                OlusturmaTarihi = urun.OlusturmaTarihi,
                GuncellemeTarihi = urun.GuncellemeTarihi
            };

            return Json(model);
        }

        // AJAX: Tüm aktif ürünleri JSON formatında getir
        [HttpGet]
        public async Task<IActionResult> GetUrunlerJson()
        {
            var urunler = await _unitOfWork.Repository<Urun>().GetAsync(
                filter: u => u.Aktif && !u.Silindi,
                includeProperties: "Birim");

            var result = urunler.Select(u => new
            {
                urunID = u.UrunID,
                urunKodu = u.UrunKodu,
                urunAdi = u.UrunAdi,
                birim = u.Birim != null ? u.Birim.BirimAdi : string.Empty,
                stokMiktar = u.StokMiktar
            });

            return Json(result);
        }
        
        // AJAX: Belirli bir ürünün bilgilerini getir
        [HttpGet]
        public async Task<IActionResult> GetUrunBilgileri(Guid id)
            {
                var urun = await _unitOfWork.Repository<Urun>().GetFirstOrDefaultAsync(
                    filter: u => u.UrunID == id && u.Aktif && !u.Silindi,
                    includeProperties: "Birim");

                if (urun == null)
                {
                return Json(new { success = false, message = "Ürün bulunamadı" });
            }

                return Json(new
                {
                    urunID = urun.UrunID,
                    urunKodu = urun.UrunKodu,
                    urunAdi = urun.UrunAdi,
                    birim = urun.Birim != null ? urun.Birim.BirimAdi : "Adet",
                stokMiktar = urun.StokMiktar
            });
        }
        
        #region Yardımcı Metodlar
        
        // Validasyon hatalarını Türkçeleştir
        private void TurkceHataEkle()
        {
            foreach (var key in ModelState.Keys.ToList())
            {
                var errors = ModelState[key].Errors.ToList();
                for (int i = 0; i < errors.Count; i++)
                {
                    var error = errors[i];
                    if (error.ErrorMessage.Contains("field is required"))
                    {
                        ModelState.Remove(key);
                        ModelState.AddModelError(key, $"{key} alanı zorunludur.");
                    }
                }
            }
        }
        
        // Dropdown listelerini doldur
        private async Task ListeleriDoldur(Guid? kategoriID = null, Guid? birimID = null)
        {
            try
            {
                // Bu metot artık DropdownService ile güncellenebilir
                var dropdownData = await _dropdownService.PrepareUrunDropdownsAsync(birimID, kategoriID);
                
                ViewBag.Kategoriler = dropdownData["Kategoriler"];
                ViewBag.Birimler = dropdownData["Birimler"];
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "ListeleriDoldur hatası: {Message}", ex.Message);
                
                // Hata durumunda boş listeler oluştur
                ViewBag.Kategoriler = new SelectList(new List<SelectListItem>());
                ViewBag.Birimler = new SelectList(new List<SelectListItem>());
            }
        }
        
        #endregion
        
        private async Task<decimal> GetDinamikStokMiktari(Guid urunID, Guid? depoID = null)
        {
            try
            {
                var stokHareketleriQuery = _context.StokHareketleri
                    .Where(sh => sh.UrunID == urunID && !sh.Silindi);

                if (depoID.HasValue)
                {
                    stokHareketleriQuery = stokHareketleriQuery.Where(sh => sh.DepoID == depoID);
                }

                var girisler = await stokHareketleriQuery
                    .Where(sh => sh.HareketTuru == StokHareketiTipi.Giris)
                    .SumAsync(sh => sh.Miktar);

                var cikislar = await stokHareketleriQuery
                    .Where(sh => sh.HareketTuru == StokHareketiTipi.Cikis)
                    .SumAsync(sh => Math.Abs(sh.Miktar)); // Mutlak değer olarak alıyoruz

                return girisler - cikislar; // Çıkışları çıkararak hesaplıyoruz
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Stok miktarı hesaplanırken hata oluştu: UrunID={urunID}");
                return 0;
            }
        }

        // Ürün pasife alma metodu (SetPassive adıyla)
        [HttpPost]
        public async Task<IActionResult> SetPassive(Guid id)
        {
            try
            {
                var urunRepository = _unitOfWork.Repository<Urun>();
                var urun = await urunRepository.GetFirstOrDefaultAsync(u => u.UrunID == id && !u.Silindi);
                
                if (urun == null)
                {
                    return Json(new { success = false, message = "Ürün bulunamadı." });
                }
                
                urun.Aktif = false;
                urun.GuncellemeTarihi = DateTime.Now;
                urun.SonGuncelleyenKullaniciID = GetCurrentUserId();
                
                await urunRepository.UpdateAsync(urun);
                await _unitOfWork.SaveAsync();
                
                // Pasife alma işlemi için log kaydı
                await _logService.UrunPasifleştirmeLogOlustur(urun.UrunID.ToString(), urun.UrunAdi);
                
                return Json(new { 
                    success = true, 
                    message = $"{urun.UrunAdi} ürünü başarıyla pasife alındı." 
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ürün pasife alınırken hata: {Message}", ex.Message);
                return Json(new { success = false, message = $"Ürün pasife alınırken bir hata oluştu: {ex.Message}" });
            }
        }
        
        // Ürün aktife alma metodu
        [HttpPost]
        public async Task<IActionResult> SetActive(Guid id)
        {
            try
            {
                // Doğrudan DbContext kullanarak IgnoreQueryFilters ile ürünü bul
                var urun = await _context.Urunler
                    .IgnoreQueryFilters()
                    .FirstOrDefaultAsync(u => u.UrunID == id && !u.Silindi);
                
                if (urun == null)
                {
                    return Json(new { success = false, message = "Ürün bulunamadı." });
                }
                
                urun.Aktif = true;
                urun.GuncellemeTarihi = DateTime.Now;
                urun.SonGuncelleyenKullaniciID = GetCurrentUserId();
                
                _context.Urunler.Update(urun);
                await _context.SaveChangesAsync();
                
                // Aktife alma işlemi için log kaydı
                await _logService.UrunAktifleştirmeLogOlustur(urun.UrunID.ToString(), urun.UrunAdi);
                
                return Json(new { 
                    success = true, 
                    message = $"{urun.UrunAdi} ürünü başarıyla aktifleştirildi." 
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ürün aktifleştirilirken hata: {Message}", ex.Message);
                return Json(new { success = false, message = $"Ürün aktifleştirilirken bir hata oluştu: {ex.Message}" });
            }
        }
        
        // Ürün silme metodu (Ajax çağrısıyla)
        [HttpPost]
        [Authorize(Roles = "Admin,StokYonetici")]
        public async Task<IActionResult> SetDelete(Guid id)
        {
            try
            {
                // Doğrudan DbContext kullanarak IgnoreQueryFilters ile ürünü bul
                var urun = await _context.Urunler
                    .IgnoreQueryFilters()
                    .FirstOrDefaultAsync(u => u.UrunID == id);
                
                if (urun == null)
                {
                    return Json(new { success = false, message = "Ürün bulunamadı." });
                }
                
                // Ürün kullanımda mı kontrol et
                bool urunKullaniliyor = await _context.FaturaDetaylari.AnyAsync(fd => fd.UrunID == id && !fd.Silindi);
                urunKullaniliyor = urunKullaniliyor || await _context.IrsaliyeDetaylari.AnyAsync(item => item.UrunID == id && !item.Silindi);
                urunKullaniliyor = urunKullaniliyor || await _context.StokHareketleri.AnyAsync(sh => sh.UrunID == id && !sh.Silindi);
                
                if (urunKullaniliyor)
                {
                    // İlişkili kayıtlar varsa silme, pasife al
                    urun.Aktif = false;
                    urun.Silindi = true;
                    urun.GuncellemeTarihi = DateTime.Now;
                    urun.SonGuncelleyenKullaniciID = GetCurrentUserId();
                    
                    _context.Urunler.Update(urun);
                    await _context.SaveChangesAsync();
                    
                    _logger.LogInformation($"Urun ID: {id} soft delete yapıldı");
                    
                    // İlişkili kayıtlarla beraber silme işlemi için log kaydı
                    await _logService.UrunSilmeLogOlustur(
                        urun.UrunID.ToString(), 
                        urun.UrunAdi, 
                        "İlişkili kayıtlar nedeniyle soft delete yapıldı");
                    
                    return Json(new { 
                        success = true, 
                        message = $"{urun.UrunAdi} ürünü diğer kayıtlarda kullanıldığı için pasife alındı." 
                    });
                }
                else
                {
                    // İlişkili kayıt yoksa soft delete uygula
                    urun.Silindi = true;
                    urun.GuncellemeTarihi = DateTime.Now;
                    urun.SonGuncelleyenKullaniciID = GetCurrentUserId();
                    
                    _context.Urunler.Update(urun);
                    await _context.SaveChangesAsync();
                    
                    _logger.LogInformation($"Urun ID: {id} soft delete yapıldı");
                    
                    // Normal silme işlemi için log kaydı
                    await _logService.UrunSilmeLogOlustur(
                        urun.UrunID.ToString(), 
                        urun.UrunAdi, 
                        "Soft delete yapıldı");
                    
                    return Json(new { 
                        success = true, 
                        message = $"{urun.UrunAdi} ürünü başarıyla silindi." 
                    });
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ürün silinirken hata: {Message}", ex.Message);
                return Json(new { success = false, message = $"Ürün silinirken bir hata oluştu: {ex.Message}" });
            }
        }
        
        // Excel'e aktarma işlemi
        public async Task<IActionResult> ExportToExcel()
        {
            try
            {
                var urunRepository = _unitOfWork.Repository<Urun>();
                var urunFiyatRepository = _unitOfWork.Repository<UrunFiyat>();
            var kategoriRepository = _unitOfWork.Repository<UrunKategori>();
                
                // Ürünleri getir (silinen ürünler hariç)
                var urunler = await urunRepository.GetAsync(
                    filter: u => !u.Silindi,
                    includeProperties: "Birim"
                );
                
                // Silinmemiş tüm kategorileri getir
            var kategoriler = await kategoriRepository.GetAsync(
                    filter: k => k.Silindi == false
                );
                
                // Verileri Excel formatında hazırla
                using (var workbook = new ClosedXML.Excel.XLWorkbook())
                {
                    var worksheet = workbook.Worksheets.Add("Ürünler");
                    
                    // Başlık satırı
                    worksheet.Cell(1, 1).Value = "Ürün Kodu";
                    worksheet.Cell(1, 2).Value = "Ürün Adı";
                    worksheet.Cell(1, 3).Value = "Kategori";
                    worksheet.Cell(1, 4).Value = "Birim";
                    worksheet.Cell(1, 5).Value = "Liste Fiyatı";
                    worksheet.Cell(1, 6).Value = "Satış Fiyatı";
                    worksheet.Cell(1, 7).Value = "Stok Miktarı";
                    worksheet.Cell(1, 8).Value = "Durumu";
                    
                    // Başlık formatları
                    var headerRow = worksheet.Row(1);
                    headerRow.Style.Font.Bold = true;
                    headerRow.Style.Fill.BackgroundColor = ClosedXML.Excel.XLColor.LightBlue;
                    
                    // Veri satırları
                    int row = 2;
                    foreach (var urun in urunler)
                    {
                        // Kategori adını bul
                        var kategoriAdi = urun.KategoriID.HasValue ? 
                            kategoriler.FirstOrDefault(k => k.KategoriID == urun.KategoriID)?.KategoriAdi : "Kategorisiz";
                        
                        // Fiyatları bul
                        var listeFiyati = urunFiyatRepository.GetAllAsync().Result
                            .Where(uf => uf.UrunID == urun.UrunID && uf.FiyatTipiID == 1 && uf.Silindi == false)
                            .OrderByDescending(uf => uf.GecerliTarih)
                            .FirstOrDefault()?.Fiyat ?? 0m;
                            
                        var satisFiyati = urunFiyatRepository.GetAllAsync().Result
                            .Where(uf => uf.UrunID == urun.UrunID && uf.FiyatTipiID == 3 && uf.Silindi == false)
                            .OrderByDescending(uf => uf.GecerliTarih)
                            .FirstOrDefault()?.Fiyat ?? 0m;
                        
                        // Veri doldur
                        worksheet.Cell(row, 1).Value = urun.UrunKodu;
                        worksheet.Cell(row, 2).Value = urun.UrunAdi;
                        worksheet.Cell(row, 3).Value = kategoriAdi;
                        worksheet.Cell(row, 4).Value = urun.Birim?.BirimAdi ?? "";
                        worksheet.Cell(row, 5).Value = listeFiyati;
                        worksheet.Cell(row, 6).Value = satisFiyati;
                        worksheet.Cell(row, 7).Value = urun.StokMiktar;
                        worksheet.Cell(row, 8).Value = urun.Aktif ? "Aktif" : "Pasif";
                        
                        row++;
                    }
                    
                    // Kolon genişliklerini ayarla
                    worksheet.Columns().AdjustToContents();
                    
                    // Excel dosyasını oluştur
                    using (var stream = new MemoryStream())
                    {
                        workbook.SaveAs(stream);
                        var content = stream.ToArray();
                        
                        return File(
                            content,
                            "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                            $"Urunler_{DateTime.Now:yyyyMMdd}.xlsx"
                        );
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Excel'e aktarma işlemi sırasında hata: {Message}", ex.Message);
                TempData["ErrorMessage"] = $"Excel'e aktarma işlemi sırasında bir hata oluştu: {ex.Message}";
                return RedirectToAction(nameof(Index));
            }
        }
        
        // Excel'den içe aktarma önizleme
        [HttpPost]
        public async Task<IActionResult> ImportPreview(IFormFile excelFile)
        {
            if (excelFile == null || excelFile.Length == 0)
            {
                TempData["ErrorMessage"] = "Lütfen bir Excel dosyası seçin.";
                return RedirectToAction(nameof(Index));
            }
            
            try
            {
                var urunRepository = _unitOfWork.Repository<Urun>();
            var kategoriRepository = _unitOfWork.Repository<UrunKategori>();
            var birimRepository = _unitOfWork.Repository<Birim>();
                
                // Mevcut ürünleri getir
                var mevcutUrunler = await urunRepository.GetAllAsync();
                
                // Kategorileri getir
            var kategoriler = await kategoriRepository.GetAsync(
                    filter: k => k.Silindi == false && k.Aktif
            );
            
                // Birimleri getir
            var birimler = await birimRepository.GetAsync(
                    filter: b => b.Silindi == false && b.Aktif
                );
                
                var urunListesi = new List<UrunImportViewModel>();
                
                using (var stream = new MemoryStream())
                {
                    await excelFile.CopyToAsync(stream);
                    using (var workbook = new ClosedXML.Excel.XLWorkbook(stream))
                    {
                        var worksheet = workbook.Worksheets.FirstOrDefault();
                        if (worksheet == null)
                        {
                            TempData["ErrorMessage"] = "Excel dosyasında çalışma sayfası bulunamadı.";
                            return RedirectToAction(nameof(Index));
                        }
                        
                        // Başlık satırını atla, 2. satırdan başla
                        var rows = worksheet.RowsUsed().Skip(1);
                        
                        foreach (var row in rows)
                        {
                            try
                            {
                                var urunKodu = row.Cell(1).GetString().Trim();
                                var urunAdi = row.Cell(2).GetString().Trim();
                                var kategoriAdi = row.Cell(3).GetString().Trim();
                                var birimAdi = row.Cell(4).GetString().Trim();
                                
                                // KDV ve fiyat bilgilerini decimal olarak çek
                                decimal kdvOrani = 0;
                                decimal.TryParse(row.Cell(5).GetString().Replace("%", "").Trim(), out kdvOrani);
                                
                                decimal listeFiyati = 0;
                                decimal.TryParse(row.Cell(6).GetString().Replace("₺", "").Trim(), out listeFiyati);
                                
                                decimal satisFiyati = 0;
                                decimal.TryParse(row.Cell(7).GetString().Replace("₺", "").Trim(), out satisFiyati);
                                
                                // Aktif durumu
                                var aktifStr = row.Cell(8).GetString().Trim().ToLower();
                                var aktif = aktifStr == "aktif" || aktifStr == "evet" || aktifStr == "1";
                                
                                // Boş ürün kodu ve adı kontrolü
                                if (string.IsNullOrWhiteSpace(urunKodu) || string.IsNullOrWhiteSpace(urunAdi))
                                {
                                    continue;
                                }
                                
                                // Mevcut ürün kontrolü
                                var mevcutUrun = mevcutUrunler.FirstOrDefault(u => u.UrunKodu == urunKodu && !u.Silindi);
                                
                                var viewModel = new UrunImportViewModel
                                {
                                    UrunKodu = urunKodu,
                                    UrunAdi = urunAdi,
                                    KategoriAdi = kategoriAdi,
                                    BirimAdi = birimAdi,
                                    KDVOrani = (int)kdvOrani,
                                    ListeFiyati = listeFiyati,
                                    SatisFiyati = satisFiyati,
                                    Aktif = aktif,
                                    Secili = true,
                                    MevcutMu = mevcutUrun != null
                                };
                                
                                if (mevcutUrun != null)
                                {
                                    viewModel.MevcutUrunBilgisi = $"{mevcutUrun.UrunAdi} (Mevcut kod: {mevcutUrun.UrunKodu})";
                                }
                                
                                urunListesi.Add(viewModel);
                            }
                            catch (Exception ex)
                            {
                                _logger.LogWarning(ex, "Excel satırı okunurken hata oluştu: {Message}", ex.Message);
                                continue; // Hatalı satırı atla
                            }
                        }
                    }
                }
                
                var model = new UrunImportListViewModel
                {
                    Urunler = urunListesi
                };
                
                return View("ImportPreview", model);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Excel'den içe aktarma önizlemesi sırasında hata: {Message}", ex.Message);
                TempData["ErrorMessage"] = $"Excel'den içe aktarma sırasında bir hata oluştu: {ex.Message}";
                return RedirectToAction(nameof(Index));
            }
        }
        
        // Excel'den içe aktarma onaylama
        [HttpPost]
        public async Task<IActionResult> ConfirmImport(UrunImportListViewModel model)
        {
            if (model?.Urunler == null || !model.Urunler.Any(u => u.Secili))
            {
                TempData["ErrorMessage"] = "İçe aktarılacak ürün seçilmedi.";
                return RedirectToAction(nameof(Index));
            }
            
            try
            {
                var urunRepository = _unitOfWork.Repository<Urun>();
                var kategoriRepository = _unitOfWork.Repository<UrunKategori>();
                var birimRepository = _unitOfWork.Repository<Birim>();
                var urunFiyatRepository = _unitOfWork.Repository<UrunFiyat>();
                
                // Sadece seçili ürünleri al
                var seciliUrunler = model.Urunler.Where(u => u.Secili).ToList();
                
                // Toplu işlem için sayaçlar
                int eklenenCount = 0;
                int guncellenenCount = 0;
                
                foreach (var urunModel in seciliUrunler)
                {
                    try
                    {
                        // Kategori kontrolü ve eklemesi
                        var kategori = await kategoriRepository.GetFirstOrDefaultAsync(
                            k => k.KategoriAdi.ToLower() == urunModel.KategoriAdi.ToLower() && !k.Silindi
                        );
                        
                        if (kategori == null && !string.IsNullOrWhiteSpace(urunModel.KategoriAdi))
                        {
                            // Yeni kategori oluştur
                            kategori = new UrunKategori
                            {
                                KategoriID = Guid.NewGuid(),
                                KategoriAdi = urunModel.KategoriAdi,
                                Aktif = true,
                                OlusturmaTarihi = DateTime.Now,
                                OlusturanKullaniciID = GetCurrentUserId()
                            };
                            
                            await kategoriRepository.AddAsync(kategori);
                        }
                        
                        // Birim kontrolü ve eklemesi
                        var birim = await birimRepository.GetFirstOrDefaultAsync(
                            b => b.BirimAdi.ToLower() == urunModel.BirimAdi.ToLower() && !b.Silindi
                        );
                        
                        if (birim == null && !string.IsNullOrWhiteSpace(urunModel.BirimAdi))
                        {
                            // Yeni birim oluştur
                            birim = new Birim
                            {
                                BirimID = Guid.NewGuid(),
                                BirimAdi = urunModel.BirimAdi,
                                Aktif = true,
                                OlusturmaTarihi = DateTime.Now,
                                OlusturanKullaniciID = GetCurrentUserId()
                            };
                            
                            await birimRepository.AddAsync(birim);
                        }
                        
                        // Mevcut ürün kontrolü
                        var mevcutUrun = await urunRepository.GetFirstOrDefaultAsync(
                            u => u.UrunKodu == urunModel.UrunKodu && !u.Silindi
                        );
                        
                        if (mevcutUrun != null)
                        {
                            // Ürünü güncelle
                            mevcutUrun.UrunAdi = urunModel.UrunAdi;
                            mevcutUrun.KategoriID = kategori?.KategoriID;
                            mevcutUrun.BirimID = birim?.BirimID;
                            mevcutUrun.KDVOrani = urunModel.KDVOrani;
                            mevcutUrun.Aktif = urunModel.Aktif;
                            mevcutUrun.GuncellemeTarihi = DateTime.Now;
                            mevcutUrun.SonGuncelleyenKullaniciID = GetCurrentUserId();
                            
                            await urunRepository.UpdateAsync(mevcutUrun);
                            
                            // Fiyat ekle - Liste Fiyatı
                            if (urunModel.ListeFiyati > 0)
                            {
                                var yeniListeFiyat = new UrunFiyat
                                {
                                    UrunID = mevcutUrun.UrunID,
                                    FiyatTipiID = 1, // Liste fiyatı
                                    Fiyat = urunModel.ListeFiyati,
                                    GecerliTarih = DateTime.Now,
                                    OlusturmaTarihi = DateTime.Now,
                                    OlusturanKullaniciID = GetCurrentUserId()
                                };
                                
                                await urunFiyatRepository.AddAsync(yeniListeFiyat);
                            }
                            
                            // Fiyat ekle - Satış Fiyatı
                            if (urunModel.SatisFiyati > 0)
                            {
                                var yeniSatisFiyat = new UrunFiyat
                                {
                                    UrunID = mevcutUrun.UrunID,
                                    FiyatTipiID = 3, // Satış fiyatı
                                    Fiyat = urunModel.SatisFiyati,
                                    GecerliTarih = DateTime.Now,
                                    OlusturmaTarihi = DateTime.Now,
                                    OlusturanKullaniciID = GetCurrentUserId()
                                };
                                
                                await urunFiyatRepository.AddAsync(yeniSatisFiyat);
                            }
                            
                            guncellenenCount++;
                        }
                        else
                        {
                            // Yeni ürün oluştur
                            var yeniUrun = new Urun
                            {
                                UrunID = Guid.NewGuid(),
                                UrunKodu = urunModel.UrunKodu,
                                UrunAdi = urunModel.UrunAdi,
                                KategoriID = kategori?.KategoriID,
                                BirimID = birim?.BirimID,
                                KDVOrani = urunModel.KDVOrani,
                                StokMiktar = 0, // Başlangıç stok miktarı 0
                                Aktif = urunModel.Aktif,
                                OlusturmaTarihi = DateTime.Now,
                                OlusturanKullaniciID = GetCurrentUserId()
                            };
                            
                            await urunRepository.AddAsync(yeniUrun);
                            
                            // Fiyat ekle - Liste Fiyatı
                            if (urunModel.ListeFiyati > 0)
                            {
                                var yeniListeFiyat = new UrunFiyat
                                {
                                    UrunID = yeniUrun.UrunID,
                                    FiyatTipiID = 1, // Liste fiyatı
                                    Fiyat = urunModel.ListeFiyati,
                                    GecerliTarih = DateTime.Now,
                                    OlusturmaTarihi = DateTime.Now,
                                    OlusturanKullaniciID = GetCurrentUserId()
                                };
                                
                                await urunFiyatRepository.AddAsync(yeniListeFiyat);
                            }
                            
                            // Fiyat ekle - Satış Fiyatı
                            if (urunModel.SatisFiyati > 0)
                            {
                                var yeniSatisFiyat = new UrunFiyat
                                {
                                    UrunID = yeniUrun.UrunID,
                                    FiyatTipiID = 3, // Satış fiyatı
                                    Fiyat = urunModel.SatisFiyati,
                                    GecerliTarih = DateTime.Now,
                                    OlusturmaTarihi = DateTime.Now,
                                    OlusturanKullaniciID = GetCurrentUserId()
                                };
                                
                                await urunFiyatRepository.AddAsync(yeniSatisFiyat);
                            }
                            
                            eklenenCount++;
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Ürün içe aktarılırken hata: {UrunKodu} - {Hata}", 
                            urunModel.UrunKodu, ex.Message);
                    }
                }
                
                await _unitOfWork.SaveAsync();
                
                TempData["SuccessMessage"] = $"Excel içe aktarma işlemi tamamlandı. {eklenenCount} adet ürün eklendi, {guncellenenCount} adet ürün güncellendi.";
                
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Excel'den içe aktarma onaylama sırasında hata: {Message}", ex.Message);
                TempData["ErrorMessage"] = $"Excel'den içe aktarma sırasında bir hata oluştu: {ex.Message}";
                return RedirectToAction(nameof(Index));
            }
        }
        
        // Excel şablonu indirme
        public IActionResult DownloadExcelTemplate()
        {
            try
            {
                using (var workbook = new ClosedXML.Excel.XLWorkbook())
                {
                    var worksheet = workbook.Worksheets.Add("Ürünler");
                    
                    // Başlık satırı
                    worksheet.Cell(1, 1).Value = "Ürün Kodu";
                    worksheet.Cell(1, 2).Value = "Ürün Adı";
                    worksheet.Cell(1, 3).Value = "Kategori";
                    worksheet.Cell(1, 4).Value = "Birim";
                    worksheet.Cell(1, 5).Value = "KDV %";
                    worksheet.Cell(1, 6).Value = "Liste Fiyatı";
                    worksheet.Cell(1, 7).Value = "Satış Fiyatı";
                    worksheet.Cell(1, 8).Value = "Durum";
                    
                    // Başlık formatları
                    var headerRow = worksheet.Row(1);
                    headerRow.Style.Font.Bold = true;
                    headerRow.Style.Fill.BackgroundColor = ClosedXML.Excel.XLColor.LightBlue;
                    
                    // Örnek satır
                    worksheet.Cell(2, 1).Value = "U001";
                    worksheet.Cell(2, 2).Value = "Örnek Ürün";
                    worksheet.Cell(2, 3).Value = "Genel";
                    worksheet.Cell(2, 4).Value = "Adet";
                    worksheet.Cell(2, 5).Value = "12";
                    worksheet.Cell(2, 6).Value = "100.00";
                    worksheet.Cell(2, 7).Value = "120.00";
                    worksheet.Cell(2, 8).Value = "Aktif";
                    
                    // Kolon genişliklerini ayarla
                    worksheet.Columns().AdjustToContents();
                    
                    // Excel dosyasını oluştur
                    using (var stream = new MemoryStream())
                    {
                        workbook.SaveAs(stream);
                        var content = stream.ToArray();
                        
                        return File(
                            content,
                            "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                            "UrunSablonu.xlsx"
                        );
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Excel şablonu indirme işlemi sırasında hata: {Message}", ex.Message);
                TempData["ErrorMessage"] = $"Excel şablonu oluşturulurken bir hata oluştu: {ex.Message}";
                return RedirectToAction(nameof(Index));
            }
        }
        
        // Silinmiş ürünü geri getirme metodu
        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Restore(Guid id)
        {
            try
            {
                // Doğrudan DbContext kullanarak IgnoreQueryFilters ile silinmiş ürünü bul
                var entity = await _context.Urunler
                    .IgnoreQueryFilters()
                    .FirstOrDefaultAsync(u => u.UrunID == id && u.Silindi);
                
                if (entity == null)
                {
                    return Json(new { success = false, message = "Silinmiş ürün bulunamadı." });
                }

                // Entity'yi güncelle
                entity.Silindi = false;
                entity.Aktif = true; // Geri yüklenen ürünü aktif yap
                entity.GuncellemeTarihi = DateTime.Now;
                entity.SonGuncelleyenKullaniciID = GetCurrentUserId();

                _context.Urunler.Update(entity);
                await _context.SaveChangesAsync();
                
                _logger.LogInformation($"Ürün geri getirildi: UrunID={id}, Kullanıcı={User.Identity.Name}");
                
                return Json(new { 
                    success = true, 
                    message = $"{entity.UrunAdi} ürünü başarıyla geri getirildi." 
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ürün geri getirilirken hata: {Message}", ex.Message);
                return Json(new { success = false, message = $"Ürün geri getirilirken bir hata oluştu: {ex.Message}" });
            }
        }
        
        // Silinmiş ürünü geri getirme metodu (RestoreDeleted adıyla da çalışması için eklendi)
        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> RestoreDeleted(Guid id)
        {
            try
            {
                // Doğrudan DbContext kullanarak IgnoreQueryFilters ile silinmiş ürünü bul
                var entity = await _context.Urunler
                    .IgnoreQueryFilters()
                    .FirstOrDefaultAsync(u => u.UrunID == id && u.Silindi);
                
                if (entity == null)
                {
                    return Json(new { success = false, message = "Silinmiş ürün bulunamadı." });
                }

                // Entity'yi güncelle
                entity.Silindi = false;
                entity.Aktif = true; // Geri yüklenen ürünü aktif yap
                entity.GuncellemeTarihi = DateTime.Now;
                entity.SonGuncelleyenKullaniciID = GetCurrentUserId();

                _context.Urunler.Update(entity);
                await _context.SaveChangesAsync();
                
                _logger.LogInformation($"Ürün geri getirildi: UrunID={id}, Kullanıcı={User.Identity.Name}");
                
                return Json(new { 
                    success = true, 
                    message = $"{entity.UrunAdi} ürünü başarıyla geri getirildi." 
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ürün geri getirilirken hata: {Message}", ex.Message);
                return Json(new { success = false, message = $"Ürün geri getirilirken bir hata oluştu: {ex.Message}" });
            }
        }
    }
} 