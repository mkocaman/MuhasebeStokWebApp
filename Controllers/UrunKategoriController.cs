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
            // Silinmemiş tüm kategorileri getir
            var kategoriler = await _context.UrunKategorileri
                .Where(k => !k.Silindi)
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
                    OlusturmaTarihi = k.OlusturmaTarihi,
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
                    TempData["Success"] = "Kategori başarıyla oluşturuldu.";
                    return RedirectToAction(nameof(Index));
                }
                catch (Exception ex)
                {
                    TempData["Error"] = $"Kategori oluşturulurken bir hata oluştu: {ex.Message}";
                }
            }
            else
            {
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
                OlusturmaTarihi = kategori.OlusturmaTarihi,
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
                        return NotFound();
                    }

                    // Kategori bilgilerini güncelle
                    existingKategori.KategoriAdi = model.KategoriAdi;
                    existingKategori.Aciklama = model.Aciklama ?? ""; // Açıklama null ise boş string olarak ayarla
                    existingKategori.Aktif = model.Aktif;
                    existingKategori.GuncellemeTarihi = DateTime.Now;

                    // Değişiklikleri kaydet
                    await _context.SaveChangesAsync();
                    TempData["Success"] = "Kategori başarıyla güncellendi.";
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!KategoriExists(model.KategoriID))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                catch (Exception ex)
                {
                    TempData["Error"] = $"Kategori güncellenirken bir hata oluştu: {ex.Message}";
                    return RedirectToAction(nameof(Index));
                }
                return RedirectToAction(nameof(Index));
            }
            
            // Validasyon hataları
            var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage);
            TempData["Error"] = $"Kategori güncellenirken bir hata oluştu: {string.Join(", ", errors)}";
            return RedirectToAction(nameof(Index));
        }

        // Kategori silme işlemi (soft delete)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(Guid id)
        {
            // Kategori ve bağlı ürünleri getir
            var kategori = await _context.UrunKategorileri
                .Include(k => k.Urunler)
                .FirstOrDefaultAsync(k => k.KategoriID == id && !k.Silindi);
                
            if (kategori == null)
            {
                return NotFound();
            }

            try
            {
                // Eğer kategoriye bağlı ürün varsa, kategoriyi silme, sadece pasife al
                if (kategori.Urunler.Any())
                {
                    // Kategoriyi pasife al
                    kategori.Aktif = false;
                    _context.Update(kategori);
                    await _context.SaveChangesAsync();
                    
                    TempData["Warning"] = $"'{kategori.KategoriAdi}' kategorisi pasife alındı. Bu kategori ürünlerde kullanıldığı için tamamen silinemez.";
                }
                else
                {
                    // Eğer kategoriye bağlı ürün yoksa, soft delete yap
                    kategori.Silindi = true;
                    kategori.Aktif = false;
                    _context.Update(kategori);
                    await _context.SaveChangesAsync();
                    
                    TempData["Success"] = $"'{kategori.KategoriAdi}' kategorisi başarıyla silindi.";
                }
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Kategori silinirken bir hata oluştu: {ex.Message}";
            }

            return RedirectToAction(nameof(Index));
        }

        // Pasif kategoriyi aktifleştirme
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Activate(Guid id)
        {
            // Kategori bilgisini getir
            var kategori = await _context.UrunKategorileri
                .FirstOrDefaultAsync(k => k.KategoriID == id && !k.Silindi);
                
            if (kategori == null)
            {
                return NotFound();
            }

            try
            {
                // Kategoriyi aktif yap
                kategori.Aktif = true;
                kategori.GuncellemeTarihi = DateTime.Now;
                
                _context.Update(kategori);
                await _context.SaveChangesAsync();
                
                TempData["Success"] = $"'{kategori.KategoriAdi}' kategorisi başarıyla aktifleştirildi.";
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Kategori aktifleştirilirken bir hata oluştu: {ex.Message}";
            }

            return RedirectToAction(nameof(Index));
        }

        // Kategori mevcut mu kontrolü
        private bool KategoriExists(Guid id)
        {
            return _context.UrunKategorileri.Any(e => e.KategoriID == id && !e.Silindi);
        }
    }
} 