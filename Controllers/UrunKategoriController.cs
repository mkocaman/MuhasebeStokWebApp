using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MuhasebeStokWebApp.Data;
using MuhasebeStokWebApp.Data.Entities;
using MuhasebeStokWebApp.ViewModels.UrunKategori;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MuhasebeStokWebApp.Controllers
{
    public class UrunKategoriController : Controller
    {
        private readonly ApplicationDbContext _context;

        public UrunKategoriController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: UrunKategori
        public async Task<IActionResult> Index()
        {
            var kategoriler = await _context.UrunKategorileri
                .Where(k => !k.SoftDelete)
                .OrderByDescending(k => k.OlusturmaTarihi)
                .ToListAsync();

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

        // POST: UrunKategori/Create
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
                    var kategori = new UrunKategori
                    {
                        KategoriID = Guid.NewGuid(),
                        KategoriAdi = model.KategoriAdi,
                        Aciklama = model.Aciklama ?? "", // Açıklama null ise boş string olarak ayarla
                        Aktif = model.Aktif, // Model'den doğrudan al
                        OlusturmaTarihi = DateTime.Now,
                        SoftDelete = false
                    };

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

        // GET: UrunKategori/Details/5
        [HttpGet]
        public async Task<IActionResult> Details(Guid id)
        {
            var kategori = await _context.UrunKategorileri
                .FirstOrDefaultAsync(k => k.KategoriID == id && !k.SoftDelete);
                
            if (kategori == null)
            {
                return NotFound();
            }

            var viewModel = new UrunKategoriViewModel
            {
                KategoriID = kategori.KategoriID,
                KategoriAdi = kategori.KategoriAdi,
                Aciklama = kategori.Aciklama,
                Aktif = kategori.Aktif,
                OlusturmaTarihi = kategori.OlusturmaTarihi,
                GuncellemeTarihi = kategori.GuncellemeTarihi
            };

            return Json(viewModel);
        }

        // GET: UrunKategori/Edit/5
        [HttpGet]
        public async Task<IActionResult> Edit(Guid id)
        {
            var kategori = await _context.UrunKategorileri
                .FirstOrDefaultAsync(k => k.KategoriID == id && !k.SoftDelete);
                
            if (kategori == null)
            {
                return NotFound();
            }

            var viewModel = new UrunKategoriEditViewModel
            {
                KategoriID = kategori.KategoriID,
                KategoriAdi = kategori.KategoriAdi,
                Aciklama = kategori.Aciklama,
                Aktif = kategori.Aktif
            };

            return PartialView("_EditPartial", viewModel);
        }

        // POST: UrunKategori/Edit/5
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
                    var existingKategori = await _context.UrunKategorileri
                        .FirstOrDefaultAsync(k => k.KategoriID == model.KategoriID && !k.SoftDelete);
                        
                    if (existingKategori == null)
                    {
                        return NotFound();
                    }

                    existingKategori.KategoriAdi = model.KategoriAdi;
                    existingKategori.Aciklama = model.Aciklama ?? ""; // Açıklama null ise boş string olarak ayarla
                    existingKategori.Aktif = model.Aktif;
                    existingKategori.GuncellemeTarihi = DateTime.Now;

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
            
            var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage);
            TempData["Error"] = $"Kategori güncellenirken bir hata oluştu: {string.Join(", ", errors)}";
            return RedirectToAction(nameof(Index));
        }

        // POST: UrunKategori/Delete/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(Guid id)
        {
            var kategori = await _context.UrunKategorileri
                .Include(k => k.Urunler)
                .FirstOrDefaultAsync(k => k.KategoriID == id && !k.SoftDelete);
                
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
                    kategori.SoftDelete = true;
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

        // POST: UrunKategori/Activate/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Activate(Guid id)
        {
            var kategori = await _context.UrunKategorileri
                .FirstOrDefaultAsync(k => k.KategoriID == id && !k.SoftDelete);
                
            if (kategori == null)
            {
                return NotFound();
            }

            try
            {
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

        private bool KategoriExists(Guid id)
        {
            return _context.UrunKategorileri.Any(e => e.KategoriID == id && !e.SoftDelete);
        }
    }
} 