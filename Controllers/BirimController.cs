using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MuhasebeStokWebApp.Data;
using MuhasebeStokWebApp.Data.Entities;
using MuhasebeStokWebApp.ViewModels.Birim;

namespace MuhasebeStokWebApp.Controllers
{
    public class BirimController : Controller
    {
        private readonly ApplicationDbContext _context;

        public BirimController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Birim
        public async Task<IActionResult> Index()
        {
            var birimler = await _context.Birimler
                .Where(b => !b.SoftDelete)
                .OrderBy(b => b.BirimAdi)
                .Select(b => new BirimViewModel
                {
                    BirimID = b.BirimID,
                    BirimAdi = b.BirimAdi,
                    Aciklama = b.Aciklama,
                    Aktif = b.Aktif
                })
                .ToListAsync();

            return View(birimler);
        }

        // GET: Birim/Details/5
        public async Task<IActionResult> Details(Guid? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var birim = await _context.Birimler
                .FirstOrDefaultAsync(b => b.BirimID == id && !b.SoftDelete);

            if (birim == null)
            {
                return NotFound();
            }

            var viewModel = new BirimViewModel
            {
                BirimID = birim.BirimID,
                BirimAdi = birim.BirimAdi,
                Aciklama = birim.Aciklama,
                Aktif = birim.Aktif
            };

            return View(viewModel);
        }

        // GET: Birim/Create
        public IActionResult Create()
        {
            if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
            {
                return PartialView("_CreatePartial", new BirimCreateViewModel());
            }
            return View();
        }

        // POST: Birim/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(BirimCreateViewModel model)
        {
            if (ModelState.IsValid)
            {
                var birim = new Birim
                {
                    BirimID = Guid.NewGuid(),
                    BirimAdi = model.BirimAdi,
                    Aciklama = model.Aciklama,
                    Aktif = model.Aktif,
                    SoftDelete = false
                };

                _context.Add(birim);
                await _context.SaveChangesAsync();
                
                TempData["SuccessMessage"] = "Birim başarıyla oluşturuldu.";
                return RedirectToAction(nameof(Index));
            }
            
            if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
            {
                return PartialView("_CreatePartial", model);
            }
            return View(model);
        }

        // GET: Birim/Edit/5
        public async Task<IActionResult> Edit(Guid? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var birim = await _context.Birimler
                .FirstOrDefaultAsync(b => b.BirimID == id && !b.SoftDelete);

            if (birim == null)
            {
                return NotFound();
            }

            var viewModel = new BirimEditViewModel
            {
                BirimID = birim.BirimID,
                BirimAdi = birim.BirimAdi,
                Aciklama = birim.Aciklama,
                Aktif = birim.Aktif
            };

            if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
            {
                return PartialView("_EditPartial", viewModel);
            }
            return View(viewModel);
        }

        // POST: Birim/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(Guid id, BirimEditViewModel model)
        {
            if (id != model.BirimID)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    var birim = await _context.Birimler
                        .FirstOrDefaultAsync(b => b.BirimID == id && !b.SoftDelete);

                    if (birim == null)
                    {
                        return NotFound();
                    }

                    birim.BirimAdi = model.BirimAdi;
                    birim.Aciklama = model.Aciklama;
                    birim.Aktif = model.Aktif;

                    _context.Update(birim);
                    await _context.SaveChangesAsync();
                    
                    TempData["SuccessMessage"] = "Birim başarıyla güncellendi.";
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!BirimExists(model.BirimID))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            
            if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
            {
                return PartialView("_EditPartial", model);
            }
            return View(model);
        }

        // GET: Birim/Delete/5
        public async Task<IActionResult> Delete(Guid? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var birim = await _context.Birimler
                .FirstOrDefaultAsync(b => b.BirimID == id && !b.SoftDelete);

            if (birim == null)
            {
                return NotFound();
            }

            var viewModel = new BirimViewModel
            {
                BirimID = birim.BirimID,
                BirimAdi = birim.BirimAdi,
                Aciklama = birim.Aciklama,
                Aktif = birim.Aktif
            };

            return View(viewModel);
        }

        // POST: Birim/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(Guid id)
        {
            var birim = await _context.Birimler
                .Include(b => b.Urunler)
                .FirstOrDefaultAsync(b => b.BirimID == id && !b.SoftDelete);

            if (birim == null)
            {
                return NotFound();
            }

            try
            {
                bool iliskiliKayitVar = false;
                
                // Birime bağlı ürünlerin olup olmadığını kontrol et
                if (birim.Urunler != null && birim.Urunler.Any())
                {
                    iliskiliKayitVar = true;
                    
                    // İlişkili kayıtlar varsa, birimi silme, sadece pasife al
                    birim.Aktif = false;
                    _context.Update(birim);
                    await _context.SaveChangesAsync();
                    
                    TempData["Warning"] = $"'{birim.BirimAdi}' birimi pasife alındı. Bu birim ürünlerde kullanıldığı için tamamen silinemez.";
                    return RedirectToAction(nameof(Index));
                }
                
                // İlişkili kayıt yoksa soft delete yap
                birim.SoftDelete = true;
                birim.Aktif = false;
                _context.Update(birim);
                await _context.SaveChangesAsync();
                
                TempData["SuccessMessage"] = $"'{birim.BirimAdi}' birimi başarıyla silindi.";
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"Birim silinirken bir hata oluştu: {ex.Message}";
            }
            
            return RedirectToAction(nameof(Index));
        }

        private bool BirimExists(Guid id)
        {
            return _context.Birimler.Any(e => e.BirimID == id && !e.SoftDelete);
        }
    }
} 