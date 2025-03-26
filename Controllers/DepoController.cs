using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using MuhasebeStokWebApp.Data;
using MuhasebeStokWebApp.Data.Entities;
using MuhasebeStokWebApp.ViewModels.Depo;
using MuhasebeStokWebApp.Services;
using Microsoft.AspNetCore.Identity;
using System.Security.Claims;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Authorization;
using MuhasebeStokWebApp.Models;
using MuhasebeStokWebApp.Services.Interfaces;

namespace MuhasebeStokWebApp.Controllers
{
    [Authorize]
    public class DepoController : BaseController
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<DepoController> _logger;
        private readonly ILogService _logService;

        public DepoController(
            ApplicationDbContext context,
            IMenuService menuService,
            UserManager<ApplicationUser> userManager,
            RoleManager<IdentityRole> roleManager,
            ILogService logService,
            ILogger<DepoController> logger)
            : base(menuService, userManager, roleManager, logService)
        {
            _context = context;
            _logger = logger;
            _logService = logService;
        }

        // GET: Depo
        public async Task<IActionResult> Index()
        {
            var depolar = await _context.Depolar
                .Where(d => !d.Silindi)
                .OrderBy(d => d.DepoAdi)
                .Select(d => new DepoViewModel
                {
                    DepoID = d.DepoID,
                    DepoAdi = d.DepoAdi,
                    Adres = d.Adres,
                    Aktif = d.Aktif,
                    OlusturmaTarihi = d.OlusturmaTarihi ?? DateTime.MinValue
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
                .FirstOrDefaultAsync(m => m.DepoID == id && !m.Silindi);
            if (depo == null)
            {
                return NotFound();
            }

            var model = new DepoViewModel
            {
                DepoID = depo.DepoID,
                DepoAdi = depo.DepoAdi,
                Adres = depo.Adres,
                Aktif = depo.Aktif,
                OlusturmaTarihi = depo.OlusturmaTarihi ?? DateTime.MinValue
            };

            return View(model);
        }

        // GET: Depo/Create
        public IActionResult Create()
        {
            var model = new DepoCreateViewModel
            {
                Aktif = true
            };
            return View(model);
        }

        // POST: Depo/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(DepoCreateViewModel model)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    var kullaniciId = GetCurrentUserId();
                    
                    if (kullaniciId == Guid.Empty)
                    {
                        ModelState.AddModelError("", "Kullanıcı bilgisi alınamadı.");
                        return View(model);
                    }

                    var depo = new Depo
                    {
                        DepoID = Guid.NewGuid(),
                        DepoAdi = model.DepoAdi,
                        Adres = model.Adres,
                        OlusturmaTarihi = DateTime.Now,
                        OlusturanKullaniciID = kullaniciId,
                        Silindi = false,
                        Aktif = true
                    };

                    _context.Add(depo);
                    await _context.SaveChangesAsync();
                    return RedirectToAction(nameof(Index));
                }

                foreach (var state in ModelState)
                {
                    foreach (var error in state.Value.Errors)
                    {
                        _logger.LogWarning($"Hata - {state.Key}: {error.ErrorMessage}");
                    }
                }
                return View(model);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Depo oluşturulurken hata oluştu");
                ModelState.AddModelError("", "Depo oluşturulurken bir hata oluştu. Lütfen tekrar deneyin.");
                return View(model);
            }
        }

        // GET: Depo/Edit/5
        public async Task<IActionResult> Edit(Guid? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var depo = await _context.Depolar
                .FirstOrDefaultAsync(d => d.DepoID == id && !d.Silindi);
            if (depo == null)
            {
                return NotFound();
            }

            var model = new DepoEditViewModel
            {
                DepoID = depo.DepoID,
                DepoAdi = depo.DepoAdi,
                Adres = depo.Adres,
                Aktif = depo.Aktif
            };

            return View(model);
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
                    var depo = await _context.Depolar.FindAsync(id);
                    if (depo == null)
                    {
                        return NotFound();
                    }

                    depo.DepoAdi = model.DepoAdi;
                    depo.Adres = model.Adres;
                    depo.Aktif = model.Aktif;
                    depo.GuncellemeTarihi = DateTime.Now;
                    depo.SonGuncelleyenKullaniciID = GetCurrentUserId();

                    _context.Update(depo);
                    await _context.SaveChangesAsync();

                    return RedirectToAction(nameof(Index));
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!await DepoExists(id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
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
                .FirstOrDefaultAsync(d => d.DepoID == id && !d.Silindi);

            if (depo == null)
            {
                return NotFound();
            }

            var model = new DepoViewModel
            {
                DepoID = depo.DepoID,
                DepoAdi = depo.DepoAdi,
                Adres = depo.Adres,
                Aktif = depo.Aktif,
                OlusturmaTarihi = depo.OlusturmaTarihi ?? DateTime.MinValue
            };

            return View(model);
        }

        // POST: Depo/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(Guid id)
        {
            var depo = await _context.Depolar.FindAsync(id);
            
            if (depo == null)
            {
                return NotFound();
            }

            // Önce ilişkili stok hareketleri soft-delete yap
            var stokHareketleri = await _context.StokHareketleri
                .Where(s => s.DepoID == id && !s.Silindi)
                .ToListAsync();

            // Depo soft-delete
            depo.Silindi = true;
            depo.GuncellemeTarihi = DateTime.Now;
            depo.SonGuncelleyenKullaniciID = GetCurrentUserId();
            
            _context.Update(depo);
            await _context.SaveChangesAsync();
            
            TempData["SuccessMessage"] = "Depo başarıyla silindi.";
            return RedirectToAction(nameof(Index));
        }

        // Depo var mı kontrolü
        [NonAction]
        private async Task<bool> DepoExists(Guid id)
        {
            return await _context.Depolar.AnyAsync(d => d.DepoID == id && !d.Silindi);
        }

        // Kullanıcı ID'sini al
        private Guid GetCurrentUserId()
        {
            try
            {
                var userIdClaim = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier);
                if (userIdClaim != null && !string.IsNullOrEmpty(userIdClaim.Value))
                {
                    return Guid.Parse(userIdClaim.Value);
                }
            }
            catch (FormatException)
            {
                // Geçersiz GUID formatı hatası durumunda loglama yapılabilir
                // Şimdilik sadece Guid.Empty dönüyoruz
            }
            return Guid.Empty;
        }
    }
} 