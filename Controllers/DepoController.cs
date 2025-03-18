using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MuhasebeStokWebApp.Data;
using MuhasebeStokWebApp.Data.Entities;
using MuhasebeStokWebApp.ViewModels.Depo;

namespace MuhasebeStokWebApp.Controllers
{
    public class DepoController : Controller
    {
        private readonly ApplicationDbContext _context;

        public DepoController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Depo
        public async Task<IActionResult> Index()
        {
            var depolar = await _context.Depolar
                .Where(d => !d.SoftDelete)
                .OrderBy(d => d.DepoAdi)
                .Select(d => new DepoViewModel
                {
                    DepoID = d.DepoID,
                    DepoAdi = d.DepoAdi,
                    Adres = d.Adres,
                    Aktif = d.Aktif,
                    OlusturmaTarihi = d.OlusturmaTarihi
                })
                .ToListAsync();

            return View(depolar);
        }

        // GET: Depo/Details/5
        public async Task<IActionResult> Details(Guid? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var depo = await _context.Depolar
                .FirstOrDefaultAsync(d => d.DepoID == id && !d.SoftDelete);

            if (depo == null)
            {
                return NotFound();
            }

            var viewModel = new DepoViewModel
            {
                DepoID = depo.DepoID,
                DepoAdi = depo.DepoAdi,
                Adres = depo.Adres,
                Aktif = depo.Aktif,
                OlusturmaTarihi = depo.OlusturmaTarihi
            };

            return View(viewModel);
        }

        // GET: Depo/Create
        public IActionResult Create()
        {
            if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
            {
                return PartialView("_CreatePartial", new DepoCreateViewModel());
            }
            return View();
        }

        // POST: Depo/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(DepoCreateViewModel model)
        {
            if (ModelState.IsValid)
            {
                var depo = new Depo
                {
                    DepoID = Guid.NewGuid(),
                    DepoAdi = model.DepoAdi,
                    Adres = model.Adres,
                    Aktif = model.Aktif,
                    OlusturanKullaniciID = GetCurrentUserId(),
                    OlusturmaTarihi = DateTime.Now,
                    SoftDelete = false
                };

                _context.Add(depo);
                await _context.SaveChangesAsync();
                
                TempData["SuccessMessage"] = "Depo başarıyla oluşturuldu.";
                return RedirectToAction(nameof(Index));
            }
            
            if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
            {
                return PartialView("_CreatePartial", model);
            }
            return View(model);
        }

        // GET: Depo/Edit/5
        public async Task<IActionResult> Edit(Guid? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var depo = await _context.Depolar
                .FirstOrDefaultAsync(d => d.DepoID == id && !d.SoftDelete);

            if (depo == null)
            {
                return NotFound();
            }

            var viewModel = new DepoEditViewModel
            {
                DepoID = depo.DepoID,
                DepoAdi = depo.DepoAdi,
                Adres = depo.Adres,
                Aktif = depo.Aktif
            };

            if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
            {
                return PartialView("_EditPartial", viewModel);
            }
            return View(viewModel);
        }

        // POST: Depo/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(Guid id, DepoEditViewModel model)
        {
            if (id != model.DepoID)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    var depo = await _context.Depolar
                        .FirstOrDefaultAsync(d => d.DepoID == id && !d.SoftDelete);

                    if (depo == null)
                    {
                        return NotFound();
                    }

                    depo.DepoAdi = model.DepoAdi;
                    depo.Adres = model.Adres;
                    depo.Aktif = model.Aktif;
                    depo.SonGuncelleyenKullaniciID = GetCurrentUserId();
                    depo.GuncellemeTarihi = DateTime.Now;

                    _context.Update(depo);
                    await _context.SaveChangesAsync();
                    
                    TempData["SuccessMessage"] = "Depo başarıyla güncellendi.";
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!DepoExists(model.DepoID))
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

        // GET: Depo/Delete/5
        public async Task<IActionResult> Delete(Guid? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var depo = await _context.Depolar
                .FirstOrDefaultAsync(d => d.DepoID == id && !d.SoftDelete);

            if (depo == null)
            {
                return NotFound();
            }

            var viewModel = new DepoViewModel
            {
                DepoID = depo.DepoID,
                DepoAdi = depo.DepoAdi,
                Adres = depo.Adres,
                Aktif = depo.Aktif,
                OlusturmaTarihi = depo.OlusturmaTarihi
            };

            return View(viewModel);
        }

        // POST: Depo/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(Guid id)
        {
            var depo = await _context.Depolar
                .Include(d => d.StokHareketleri)
                .FirstOrDefaultAsync(d => d.DepoID == id && !d.SoftDelete);

            if (depo == null)
            {
                return NotFound();
            }

            try
            {
                // Depoya bağlı stok hareketlerini soft delete yap
                if (depo.StokHareketleri != null && depo.StokHareketleri.Any())
                {
                    foreach (var hareket in depo.StokHareketleri.ToList())
                    {
                        hareket.SoftDelete = true;
                        _context.Update(hareket);
                    }
                    
                    // Depoyu soft delete yap
                    depo.SoftDelete = true;
                    depo.Aktif = false;
                    _context.Update(depo);
                    await _context.SaveChangesAsync();
                    
                    TempData["Warning"] = $"'{depo.DepoAdi}' deposu ve ilişkili stok hareketleri başarıyla silindi.";
                }
                else
                {
                    // Depoyu soft delete yap
                    depo.SoftDelete = true;
                    depo.Aktif = false;
                    _context.Update(depo);
                    await _context.SaveChangesAsync();
                    
                    TempData["SuccessMessage"] = $"'{depo.DepoAdi}' deposu başarıyla silindi.";
                }
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"Depo silinirken bir hata oluştu: {ex.Message}";
            }
            
            return RedirectToAction(nameof(Index));
        }

        private bool DepoExists(Guid id)
        {
            return _context.Depolar.Any(e => e.DepoID == id && !e.SoftDelete);
        }
        
        private Guid? GetCurrentUserId()
        {
            // Eğer kimlik doğrulama sistemi varsa, mevcut kullanıcının ID'sini alın
            // Şimdilik null dönelim
            return null;
        }
    }
} 