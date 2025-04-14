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

namespace MuhasebeStokWebApp.Controllers
{
    // Ürün kategorileri yönetimi için controller sınıfı
    public class UrunKategoriController : BaseController
    {
        private readonly ApplicationDbContext _context;

        // Constructor: Veritabanı bağlantısını ve base controller servislerini DI ile alır
        public UrunKategoriController(
            ApplicationDbContext context,
            IMenuService menuService,
            UserManager<ApplicationUser> userManager,
            RoleManager<IdentityRole> roleManager,
            ILogService logService) 
            : base(menuService, userManager, roleManager, logService)
        {
            _context = context;
        }

        // Kategorilerin listelendiği ana sayfa
        public async Task<IActionResult> Index()
        {
            // Tüm kategorileri getir (silindi=false filtresi kaldırıldı)
            var kategoriler = await _context.UrunKategorileri
                .OrderByDescending(k => k.OlusturmaTarihi)
                .ToListAsync();

            // Kategori listesi görünüm modeli oluştur
            var viewModel = new UrunKategoriListViewModel
            {
                Kategoriler = kategoriler.Select(k => new UrunKategoriViewModel
                {
                    KategoriID = k.KategoriID,
                    KategoriAdi = k.KategoriAdi,
                    Aciklama = k.Aciklama,
                    Aktif = k.Aktif,
                    Silindi = k.Silindi,
                    OlusturmaTarihi = k.OlusturmaTarihi ?? DateTime.MinValue,
                    GuncellemeTarihi = k.GuncellemeTarihi
                }).ToList()
            };

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
        public async Task<IActionResult> Details(Guid id)
        {
            // Kategori bilgisini getir
            var kategori = await _context.UrunKategorileri
                .FirstOrDefaultAsync(k => k.KategoriID == id && !k.Silindi);
                
            if (kategori == null)
            {
                return NotFound();
            }

            // Kategori görünüm modeli oluştur
#pragma warning disable CS8601 // Possible null reference assignment.
            var viewModel = new UrunKategoriViewModel
            {
                KategoriID = kategori.KategoriID,
                KategoriAdi = kategori.KategoriAdi,
                Aciklama = kategori.Aciklama,
                Aktif = kategori.Aktif,
                OlusturmaTarihi = kategori.OlusturmaTarihi ?? DateTime.MinValue,
                GuncellemeTarihi = kategori.GuncellemeTarihi
            };
#pragma warning restore CS8601 // Possible null reference assignment.

            return Json(viewModel);
        }

        // Kategori düzenleme formunu getirir
        [HttpGet]
        public async Task<IActionResult> Edit(Guid id)
        {
            // Kategori bilgisini getir
            var kategori = await _context.UrunKategorileri
                .FirstOrDefaultAsync(k => k.KategoriID == id && !k.Silindi);
                
            if (kategori == null)
            {
                return NotFound();
            }

            // Düzenleme görünüm modeli oluştur
            var viewModel = new UrunKategoriEditViewModel
            {
                KategoriID = kategori.KategoriID,
                KategoriAdi = kategori.KategoriAdi,
                Aciklama = kategori.Aciklama,
                Aktif = kategori.Aktif
            };

            return PartialView("_EditPartial", viewModel);
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
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Activate(Guid id)
        {
            try
            {
                var kategori = await _context.UrunKategorileri
                    .FirstOrDefaultAsync(k => k.KategoriID == id);
                    
                if (kategori == null)
                {
                    if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
                    {
                        return Json(new { success = false, message = "Kategori bulunamadı." });
                    }
                    
                    return NotFound();
                }

                // Kategoriyi aktifleştir
                kategori.Aktif = true;
                kategori.GuncellemeTarihi = DateTime.Now;
                
                await _context.SaveChangesAsync();
                
                if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
                {
                    return Json(new { success = true, message = "Kategori başarıyla aktifleştirildi." });
                }
                
                TempData["Success"] = "Kategori başarıyla aktifleştirildi.";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
                {
                    return Json(new { success = false, message = $"Kategori aktifleştirilirken bir hata oluştu: {ex.Message}" });
                }
                
                TempData["Error"] = $"Kategori aktifleştirilirken bir hata oluştu: {ex.Message}";
                return RedirectToAction(nameof(Index));
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
    }
} 