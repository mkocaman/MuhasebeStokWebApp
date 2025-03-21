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
using System.Security.Claims;

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
                .FirstOrDefaultAsync(m => m.DepoID == id && !m.SoftDelete);
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
                // Aktif değeri için manuol kontrol ekle
                if (!ModelState.IsValid)
                {
                    // Hataları yazdır
                    foreach (var state in ModelState)
                    {
                        if (state.Key == "Aktif" && state.Value.Errors.Count > 0)
                        {
                            // Aktif alanı için varsayılan değeri true olarak ayarla
                            model.Aktif = true;
                            ModelState.Remove("Aktif");
                        }
                        else
                        {
                            foreach (var error in state.Value.Errors)
                            {
                                Console.WriteLine($"Hata - {state.Key}: {error.ErrorMessage}");
                            }
                        }
                    }
                }

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
                        SoftDelete = false,
                        GuncellemeTarihi = DateTime.Now,
                        SonGuncelleyenKullaniciID = GetCurrentUserId()
                    };

                    _context.Add(depo);
                    await _context.SaveChangesAsync();
                    return RedirectToAction(nameof(Index));
                }
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", $"Bir hata oluştu: {ex.Message}");
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
                .FirstOrDefaultAsync(d => d.DepoID == id && !d.SoftDelete);

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

            // İlişkili kayıt var mı kontrol et
            var iliskiliKayitVar = await _context.StokHareketleri.AnyAsync(s => s.DepoID == id && !s.SoftDelete);
            if (iliskiliKayitVar)
            {
                // Eğer ilişkili kayıt varsa, hata mesajı göster
                TempData["ErrorMessage"] = "Bu depo, stok hareketlerinde kullanıldığı için silinemez.";
                return RedirectToAction(nameof(Index));
            }

            // Soft delete işlemi
            depo.SoftDelete = true;
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
            return await _context.Depolar.AnyAsync(e => e.DepoID == id && !e.SoftDelete);
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