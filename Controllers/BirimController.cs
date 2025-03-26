using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MuhasebeStokWebApp.Data;
using MuhasebeStokWebApp.Data.Entities;
using MuhasebeStokWebApp.ViewModels.Birim;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using MuhasebeStokWebApp.Services;
using MuhasebeStokWebApp.Services.Interfaces;

namespace MuhasebeStokWebApp.Controllers
{
    [Authorize]
    public class BirimController : BaseController
    {
        private readonly ApplicationDbContext _context;
        private readonly IBirimService _birimService;
        private readonly new ILogService _logService;

        public BirimController(
            ApplicationDbContext context,
            IBirimService birimService,
            ILogService logService,
            IMenuService menuService,
            UserManager<ApplicationUser> userManager,
            RoleManager<IdentityRole> roleManager)
            : base(menuService, userManager, roleManager, logService)
        {
            _context = context;
            _birimService = birimService;
            _logService = logService;
        }

        // GET: Birim
        public async Task<IActionResult> Index()
        {
            var birimler = await _context.Birimler
                .Where(b => !b.Silindi)
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
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var birim = await _context.Birimler
                .FirstOrDefaultAsync(b => b.BirimID == id && b.Aktif);

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
            var model = new BirimCreateViewModel
            {
                BirimAdi = string.Empty,
                Aciklama = string.Empty,
                Aktif = true
            };
            
            return View(model);
        }

        // POST: Birim/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(BirimCreateViewModel model)
        {
            try
            {
                // Model doğrulama hatalarını kontrol et ve logla
                if (!ModelState.IsValid)
                {
                    foreach (var state in ModelState)
                    {
                        if (state.Value.Errors.Count > 0)
                        {
                            foreach (var error in state.Value.Errors)
                            {
                                await _logService.LogWarningAsync($"Hata - {state.Key}: {error.ErrorMessage}");
                            }
                        }
                    }
                }

                if (ModelState.IsValid)
                {
                    var birim = new UrunBirim
                    {
                        BirimAdi = model.BirimAdi,
                        Aciklama = model.Aciklama,
                        Aktif = model.Aktif,
                        OlusturmaTarihi = DateTime.Now,
                        Silindi = false
                    };

                    _context.Add(birim);
                    await _context.SaveChangesAsync();
                    
                    await _logService.LogInfoAsync($"Yeni birim oluşturuldu: {birim.BirimID}");
                    TempData["SuccessMessage"] = "Birim başarıyla oluşturuldu.";
                    return RedirectToAction(nameof(Index));
                }
            }
            catch (Exception ex)
            {
                await _logService.LogErrorAsync("Birim oluşturulurken hata oluştu", ex);
                ModelState.AddModelError("", $"Bir hata oluştu: {ex.Message}");
            }
            
            if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
            {
                return PartialView("_CreatePartial", model);
            }
            return View(model);
        }

        // GET: Birim/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var birim = await _context.Birimler
                .FirstOrDefaultAsync(b => b.BirimID == id && b.Aktif);

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
        public async Task<IActionResult> Edit(int id, BirimEditViewModel model)
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
                        .FirstOrDefaultAsync(b => b.BirimID == id && b.Aktif);

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
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var birim = await _context.Birimler
                .AsNoTracking()
                .FirstOrDefaultAsync(m => m.BirimID == id);
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
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var birim = await _context.Birimler.FindAsync(id);
            
            if (birim == null)
            {
                return NotFound();
            }
            
            // Soft delete işlemi
            birim.Silindi = true;
            birim.Aktif = false;
            
            _context.Update(birim);
            await _context.SaveChangesAsync();
            
            TempData["SuccessMessage"] = "Birim başarıyla silindi.";
            return RedirectToAction(nameof(Index));
        }

        private bool BirimExists(int id)
        {
            return _context.Birimler.Any(e => e.BirimID == id);
        }
    }
} 