using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MuhasebeStokWebApp.Data;
using MuhasebeStokWebApp.Data.Entities;
using MuhasebeStokWebApp.ViewModels.UrunKategori;
using MuhasebeStokWebApp.Services;
using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MuhasebeStokWebApp.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using AutoMapper;

namespace MuhasebeStokWebApp.Controllers
{
    // Ürün kategorileri yönetimi için controller sınıfı
    public class UrunKategoriController : BaseController
    {
        private readonly ApplicationDbContext _context;
        private readonly IMapper _mapper;

        // Constructor: Veritabanı bağlantısını ve base controller servislerini DI ile alır
        public UrunKategoriController(
            ApplicationDbContext context,
            IMenuService menuService,
            UserManager<ApplicationUser> userManager,
            RoleManager<IdentityRole> roleManager,
            ILogService logService,
            IMapper mapper) 
            : base(menuService, userManager, roleManager, logService)
        {
            _context = context;
            _mapper = mapper;
        }

        // GET: UrunKategori
        public async Task<IActionResult> Index(string tab = "aktif")
        {
            var tumKategoriler = await _context.UrunKategorileri
                .IgnoreQueryFilters()
                .ToListAsync();

            var viewModel = new UrunKategoriListViewModel
            {
                Kategoriler = tumKategoriler.Select(k => new UrunKategoriViewModel
                {
                    KategoriID = k.KategoriID,
                    KategoriAdi = k.KategoriAdi,
                    Aciklama = k.Aciklama,
                    Aktif = k.Aktif,
                    OlusturmaTarihi = k.OlusturmaTarihi ?? DateTime.MinValue,
                    Silindi = k.Silindi
                }).ToList()
            };

            ViewBag.AktifTab = tab;

            if (tab == "aktif")
            {
                viewModel.Kategoriler = viewModel.Kategoriler.Where(k => k.Aktif && !k.Silindi).ToList();
            }
            else if (tab == "pasif")
            {
                viewModel.Kategoriler = viewModel.Kategoriler.Where(k => !k.Aktif && !k.Silindi).ToList();
            }
            else if (tab == "silindi" && User.IsInRole("Admin"))
            {
                viewModel.Kategoriler = viewModel.Kategoriler.Where(k => k.Silindi).ToList();
            }

            return View(viewModel);
        }

        // Yeni kategori oluşturma işlemi
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(UrunKategoriCreateViewModel model)
        {
            // ModelState'i temizle ve manuel olarak doğrula
            ModelState.Clear();
            
            if (model != null && !string.IsNullOrEmpty(model.KategoriAdi))
            {
                try
                {
                    // Yeni kategori nesnesi oluştur
                    var kategori = new UrunKategori
                    {
                        KategoriID = Guid.NewGuid(),
                        KategoriAdi = model.KategoriAdi,
                        Aciklama = model.Aciklama ?? "", // Açıklama null ise boş string olarak ayarla
                        Aktif = model.Aktif, // Model'den doğrudan al
                        OlusturmaTarihi = DateTime.Now,
                        Silindi = false
                    };

                    // Kategoriyi veritabanına ekle ve değişiklikleri kaydet
                    _context.Add(kategori);
                    await _context.SaveChangesAsync();
            
                    if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
                    {
                        return Json(new { success = true, message = "Kategori başarıyla oluşturuldu." });
                    }
            
                    TempData["Success"] = "Kategori başarıyla oluşturuldu.";
                    return RedirectToAction(nameof(Index));
                }
                catch (Exception ex)
                {
                    if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
                    {
                        return Json(new { success = false, message = $"Kategori oluşturulurken bir hata oluştu: {ex.Message}" });
                    }
            
                    TempData["Error"] = $"Kategori oluşturulurken bir hata oluştu: {ex.Message}";
                }
            }
            else
            {
                if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
                {
                    return Json(new { success = false, message = "Kategori oluşturulurken bir hata oluştu: Kategori adı zorunludur." });
                }
            
                TempData["Error"] = "Kategori oluşturulurken bir hata oluştu: Kategori adı zorunludur.";
            }
            return RedirectToAction(nameof(Index));
        }

        // Kategori detaylarını getirir
        [HttpGet]
        public async Task<IActionResult> Details(Guid? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var kategori = await _context.UrunKategorileri
                .IgnoreQueryFilters() // Global filtreleri devre dışı bırak
                .FirstOrDefaultAsync(k => k.KategoriID == id);

            if (kategori == null)
            {
                return NotFound();
            }

            var viewModel = _mapper.Map<UrunKategoriViewModel>(kategori);
            
            // AJAX isteği için modal içeriği döndür
            if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
            {
                return PartialView("_DetailsPartial", viewModel);
            }
            
            return View(viewModel);
        }

        // Kategori düzenleme formunu getirir
        [HttpGet]
        public async Task<IActionResult> Edit(Guid? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var kategori = await _context.UrunKategorileri
                .IgnoreQueryFilters() // Global filtreleri devre dışı bırak
                .FirstOrDefaultAsync(k => k.KategoriID == id);

            if (kategori == null)
            {
                return NotFound();
            }

            var viewModel = _mapper.Map<UrunKategoriEditViewModel>(kategori);

            if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
            {
                return PartialView("_EditPartial", viewModel);
            }
            return View(viewModel);
        }

        // Kategori düzenleme işlemi
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(UrunKategoriEditViewModel model)
        {
            // Açıklama alanı için validasyonu kaldır
            ModelState.Remove("Aciklama");
            
            if (ModelState.IsValid)
            {
                try
                {
                    // Mevcut kategoriyi getir
                    var existingKategori = await _context.UrunKategorileri
                        .FirstOrDefaultAsync(k => k.KategoriID == model.KategoriID && !k.Silindi);
                        
                    if (existingKategori == null)
                    {
                        if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
                        {
                            return Json(new { success = false, message = "Kategori bulunamadı." });
                        }
                        
                        return NotFound();
                    }

                    // Kategori bilgilerini güncelle
                    existingKategori.KategoriAdi = model.KategoriAdi;
                    existingKategori.Aciklama = model.Aciklama ?? ""; // Açıklama null ise boş string olarak ayarla
                    existingKategori.Aktif = model.Aktif;
                    existingKategori.GuncellemeTarihi = DateTime.Now;

                    // Değişiklikleri kaydet
                    await _context.SaveChangesAsync();
            
                    if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
                    {
                        return Json(new { success = true, message = "Kategori başarıyla güncellendi." });
                    }
            
                    TempData["Success"] = "Kategori başarıyla güncellendi.";
                    return RedirectToAction(nameof(Index));
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!KategoriExists(model.KategoriID))
                    {
                        if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
                        {
                            return Json(new { success = false, message = "Kategori bulunamadı." });
                        }
                        
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                catch (Exception ex)
                {
                    if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
                    {
                        return Json(new { success = false, message = $"Kategori güncellenirken bir hata oluştu: {ex.Message}" });
                    }
            
                    TempData["Error"] = $"Kategori güncellenirken bir hata oluştu: {ex.Message}";
                    return RedirectToAction(nameof(Index));
                }
            }

            if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
            {
                return Json(new { success = false, message = "Geçersiz veri girişi, lütfen formu kontrol ediniz." });
            }
            
            return View(model);
        }

        // Kategori silme işlemi
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(Guid id)
        {
            try
            {
                var kategori = await _context.UrunKategorileri
                    .FirstOrDefaultAsync(k => k.KategoriID == id && !k.Silindi);
                    
                if (kategori == null)
                {
                    if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
                    {
                        return Json(new { success = false, message = "Kategori bulunamadı." });
                    }
                    
                    return NotFound();
                }

                // İlişkili ürünler var mı kontrol et
                var iliskiliUrunSayisi = await _context.Urunler
                    .CountAsync(u => u.KategoriID == id && !u.Silindi);
                    
                if (iliskiliUrunSayisi > 0)
                {
                    if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
                    {
                        return Json(new { success = false, message = "Bu kategoriye bağlı ürünler olduğu için silinemez. Önce bu ürünleri başka bir kategoriye taşıyın veya silin." });
                    }
                    
                    TempData["Error"] = "Bu kategoriye bağlı ürünler olduğu için silinemez. Önce bu ürünleri başka bir kategoriye taşıyın veya silin.";
                    return RedirectToAction(nameof(Index));
                }

                // Soft delete: Kategorinin 'Silindi' alanını true yap
                kategori.Silindi = true;
                kategori.Aktif = false;
                kategori.GuncellemeTarihi = DateTime.Now;
                
                await _context.SaveChangesAsync();
                
                if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
                {
                    return Json(new { success = true, message = "Kategori başarıyla silindi." });
                }
                
                TempData["Success"] = "Kategori başarıyla silindi.";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
                {
                    return Json(new { success = false, message = $"Kategori silinirken bir hata oluştu: {ex.Message}" });
                }
                
                TempData["Error"] = $"Kategori silinirken bir hata oluştu: {ex.Message}";
                return RedirectToAction(nameof(Index));
            }
        }

        // Kategori aktifleştirme işlemi
        [HttpPost]
        [Authorize(Roles = "Admin,StokYonetici")]
        public async Task<IActionResult> SetActive(Guid id)
        {
            try
            {
                // Doğrudan DbContext kullanarak IgnoreQueryFilters ile kategoriyi bul
                var kategori = await _context.UrunKategorileri
                    .IgnoreQueryFilters()
                    .FirstOrDefaultAsync(u => u.KategoriID == id);
                
                if (kategori == null)
                {
                    return Json(new { success = false, message = "Kategori bulunamadı." });
                }
                
                // Aktif durumu tersine çevir
                kategori.Aktif = !kategori.Aktif;
                kategori.Silindi = false; // Aktifleştirme işleminde silindi bayrağını kaldır
                kategori.GuncellemeTarihi = DateTime.Now;
                kategori.SonGuncelleyenKullaniciID = GetCurrentUserId();
                
                await _context.SaveChangesAsync();
                
                string durumMesaji = kategori.Aktif ? "aktifleştirildi" : "pasifleştirildi";
                return Json(new { 
                    success = true, 
                    message = $"{kategori.KategoriAdi} kategorisi başarıyla {durumMesaji}." 
                });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = $"Kategori durumu değiştirilirken bir hata oluştu: {ex.Message}" });
            }
        }

        // Silinen kategorileri geri getirme işlemi
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Restore(Guid id)
        {
            try
            {
                var kategori = await _context.UrunKategorileri
                    .FirstOrDefaultAsync(k => k.KategoriID == id && k.Silindi);
                    
                if (kategori == null)
                {
                    if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
                    {
                        return Json(new { success = false, message = "Silinmiş kategori bulunamadı." });
                    }
                    
                    return NotFound();
                }

                // Kategoriyi geri getir
                kategori.Silindi = false;
                kategori.Aktif = true;
                kategori.GuncellemeTarihi = DateTime.Now;
                
                await _context.SaveChangesAsync();
                
                if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
                {
                    return Json(new { success = true, message = "Kategori başarıyla geri getirildi." });
                }
                
                TempData["Success"] = "Kategori başarıyla geri getirildi.";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
                {
                    return Json(new { success = false, message = $"Kategori geri getirilirken bir hata oluştu: {ex.Message}" });
                }
                
                TempData["Error"] = $"Kategori geri getirilirken bir hata oluştu: {ex.Message}";
                return RedirectToAction(nameof(Index));
            }
        }

        // Kategori mevcut mu kontrolü
        private bool KategoriExists(Guid id)
        {
            return _context.UrunKategorileri.Any(e => e.KategoriID == id && !e.Silindi);
        }

        [HttpPost]
        [Authorize(Roles = "Admin,StokYonetici")]
        public async Task<IActionResult> SetDelete(Guid id)
        {
            try
            {
                // Doğrudan DbContext kullanarak IgnoreQueryFilters ile kategoriyi bul
                var kategori = await _context.UrunKategorileri
                    .IgnoreQueryFilters()
                    .FirstOrDefaultAsync(u => u.KategoriID == id);
                
                if (kategori == null)
                {
                    return Json(new { success = false, message = "Kategori bulunamadı." });
                }
                
                // Kategori kullanımda mı kontrol et
                bool kategoriKullaniliyor = await _context.Urunler.AnyAsync(u => u.KategoriID == id && !u.Silindi);
                
                if (kategoriKullaniliyor)
                {
                    // İlişkili kayıtlar varsa silme, pasife al
                    kategori.Aktif = false;
                    kategori.Silindi = true;
                    kategori.GuncellemeTarihi = DateTime.Now;
                    kategori.SonGuncelleyenKullaniciID = GetCurrentUserId();
                    
                    _context.UrunKategorileri.Update(kategori);
                    await _context.SaveChangesAsync();
                    
                    return Json(new { 
                        success = true, 
                        message = $"{kategori.KategoriAdi} kategorisi ürünlerde kullanıldığı için pasife alındı." 
                    });
                }
                else
                {
                    // İlişkili kayıt yoksa soft delete uygula
                    kategori.Silindi = true;
                    kategori.GuncellemeTarihi = DateTime.Now;
                    kategori.SonGuncelleyenKullaniciID = GetCurrentUserId();
                    
                    _context.UrunKategorileri.Update(kategori);
                    await _context.SaveChangesAsync();
                    
                    return Json(new { 
                        success = true, 
                        message = $"{kategori.KategoriAdi} kategorisi başarıyla silindi." 
                    });
                }
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = $"Kategori silinirken bir hata oluştu: {ex.Message}" });
            }
        }

        [HttpPost]
        public async Task<IActionResult> GetKategoriDetails(Guid id)
        {
            try
            {
                var kategori = await _context.UrunKategorileri
                    .IgnoreQueryFilters() // Global filtreleri devre dışı bırak
                    .AsNoTracking()
                    .FirstOrDefaultAsync(k => k.KategoriID == id);
                    
                if (kategori == null)
                {
                    return Json(new { success = false, message = "Kategori bulunamadı." });
                }
                
                var viewModel = _mapper.Map<UrunKategoriViewModel>(kategori);
                return Json(new { success = true, data = viewModel });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Kategori bilgileri yüklenirken bir hata oluştu." });
            }
        }

        // Pasife alınan veya silinen kategorileri geri alma işlemi
        [HttpPost]
        public async Task<IActionResult> RestoreKategori(Guid id)
        {
            try
            {
                var kategori = await _context.UrunKategorileri
                    .IgnoreQueryFilters() // Global filtreleri devre dışı bırak
                    .FirstOrDefaultAsync(k => k.KategoriID == id);
                    
                if (kategori == null)
                {
                    return Json(new { success = false, message = "Kategori bulunamadı." });
                }

                // Kategoriyi aktif et
                kategori.Aktif = true;
                kategori.Silindi = false; // Eğer silindiyse, silindi durumunu da false yap
                kategori.GuncellemeTarihi = DateTime.Now;
                
                // Güncelleyen kullanıcı ID'sini al
                var updateUserId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
                kategori.SonGuncelleyenKullaniciID = string.IsNullOrEmpty(updateUserId) ? null : (Guid?)Guid.Parse(updateUserId);

                await _context.SaveChangesAsync();
                
                return Json(new { success = true, message = "Kategori başarıyla aktif edildi." });
            }
            catch (Exception ex)
            {
                var message = "Kategori aktif edilirken bir hata oluştu.";
                return Json(new { success = false, message = message });
            }
        }
    }
} 