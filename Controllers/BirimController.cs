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
        private readonly IValidationService _validationService;
        private readonly new UserManager<ApplicationUser> _userManager;

        public BirimController(
            ApplicationDbContext context,
            IBirimService birimService,
            ILogService logService,
            IValidationService validationService,
            IMenuService menuService,
            UserManager<ApplicationUser> userManager,
            RoleManager<IdentityRole> roleManager)
            : base(menuService, userManager, roleManager, logService)
        {
            _context = context;
            _birimService = birimService;
            _logService = logService;
            _validationService = validationService;
            _userManager = userManager;
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
        public async Task<IActionResult> Details(Guid? id)
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
                // Log the incoming model
                await _logService.LogInfoAsync($"Gelen model: BirimAdi={model.BirimAdi}, Aciklama={model.Aciklama}, Aktif={model.Aktif}");

                // Birim adının boş olup olmadığını kontrol et
                _validationService.ValidateRequiredString(ModelState, model.BirimAdi, nameof(model.BirimAdi), "Birim adı zorunludur.");

                if (!ModelState.IsValid)
                {
                    return Json(await _validationService.HandleValidationErrorAsync(ModelState, "Birim oluşturma"));
                }

                // Kullanıcı ID'sini al
                var currentUserId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(currentUserId))
                {
                    return Json(await _validationService.HandleErrorAsync("Kullanıcı bilgisi alınamadı.", "Birim oluşturma"));
                }

                // Aynı isimde birim olup olmadığını kontrol et
                var existingBirim = await _context.Birimler
                    .FirstOrDefaultAsync(b => b.BirimAdi.ToLower() == model.BirimAdi.ToLower().Trim() && !b.Silindi);
                
                if (existingBirim != null)
                {
                    return Json(await _validationService.HandleErrorAsync("Bu isimde bir birim zaten mevcut.", "Birim oluşturma"));
                }

                // Yeni birim nesnesi oluştur
                var birim = new Birim
                {
                    BirimID = Guid.NewGuid(),
                    BirimAdi = model.BirimAdi.Trim(),
                    Aciklama = model.Aciklama?.Trim(),
                    Aktif = model.Aktif,
                    OlusturmaTarihi = DateTime.Now,
                    OlusturanKullaniciID = currentUserId,
                    BirimKodu = model.BirimAdi.Length > 3 ? model.BirimAdi.Substring(0, 3).ToUpper() : model.BirimAdi.ToUpper(),
                    Silindi = false,
                    BirimSembol = model.BirimAdi.Length > 3 ? model.BirimAdi.Substring(0, 3).ToUpper() : model.BirimAdi.ToUpper()
                };

                // Birim ekleme işlemini gerçekleştir
                await _logService.LogInfoAsync($"Birim oluşturuluyor: {birim.BirimAdi}, Kullanıcı ID: {birim.OlusturanKullaniciID}");
                _context.Add(birim);
                
                try
                {
                    await _context.SaveChangesAsync();
                    return Json(new { success = true, message = "Birim başarıyla oluşturuldu." });
                }
                catch (DbUpdateException ex)
                {
                    var message = "Birim kaydedilirken bir hata oluştu. Lütfen tekrar deneyin.";
                    await _logService.LogErrorAsync($"Veritabanı güncellenirken hata oluştu. Kullanıcı ID: {birim.OlusturanKullaniciID}", ex);
                    return Json(new { success = false, message = message });
                }
            }
            catch (Exception ex)
            {
                var message = "Birim oluşturulurken bir hata oluştu.";
                await _logService.LogErrorAsync(message, ex);
                return Json(new { success = false, message = message });
            }
        }

        // GET: Birim/Edit/5
        public async Task<IActionResult> Edit(Guid? id)
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
        public async Task<IActionResult> Edit(Guid id, BirimEditViewModel model)
        {
            if (id != model.BirimID)
            {
                return Json(new { success = false, message = "Geçersiz ID bilgisi." });
            }

            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage);
                return Json(new { success = false, message = "Form alanlarını doğru şekilde doldurunuz.", errors = errors });
            }

            try
            {
                var birim = await _context.Birimler
                    .FirstOrDefaultAsync(b => b.BirimID == id && !b.Silindi);

                if (birim == null)
                {
                    return Json(new { success = false, message = "Birim bulunamadı." });
                }

                // Model'den entity'ye değerleri aktarma
                birim.BirimAdi = model.BirimAdi.Trim();
                birim.Aciklama = model.Aciklama?.Trim() ?? string.Empty;

                // Aktif değerini doğru bir şekilde al
                birim.Aktif = model.Aktif;

                // Debug için log ekleyelim
                await _logService.LogInfoAsync($"Güncellenen Birim - BirimID: {birim.BirimID}, Aktif: {birim.Aktif}");

                birim.GuncellemeTarihi = DateTime.Now;
                
                // OlusturanKullaniciID'nin null olup olmadığını kontrol et
                if (string.IsNullOrEmpty(birim.OlusturanKullaniciID))
                {
                    var currentUserId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
                    birim.OlusturanKullaniciID = currentUserId ?? "system";
                }
                
                // SonGuncelleyenKullaniciID güncelleme
                var updateUserId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
                if (!string.IsNullOrEmpty(updateUserId))
                {
                    birim.SonGuncelleyenKullaniciID = Guid.Parse(updateUserId);
                }

                _context.Update(birim);
                await _context.SaveChangesAsync();
                
                await _logService.LogInfoAsync($"Birim güncellendi: {birim.BirimID} - {birim.BirimAdi} - Aktif: {birim.Aktif}");
                
                return Json(new { success = true, message = "Birim başarıyla güncellendi." });
            }
            catch (DbUpdateConcurrencyException ex)
            {
                await _logService.LogErrorAsync("Birim güncellenirken veri tutarsızlığı oluştu.", ex);
                return Json(new { success = false, message = "Birim daha önce başka bir kullanıcı tarafından değiştirilmiş olabilir." });
            }
            catch (Exception ex)
            {
                await _logService.LogErrorAsync("Birim güncellenirken hata oluştu.", ex);
                return Json(new { success = false, message = "Birim güncellenirken bir hata oluştu: " + ex.Message });
            }
        }

        // GET: Birim/Delete/5
        public async Task<IActionResult> Delete(Guid id)
        {
            if (id == Guid.Empty)
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
        public async Task<IActionResult> DeleteConfirmed(Guid id)
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

        [HttpPost]
        public async Task<IActionResult> GetBirimDetails(Guid id)
        {
            // Stub metod, gerçek implementasyona göre doldurulacak
            return Json(new { success = false, message = "Not implemented" });
        }

        [HttpPost]
        public async Task<JsonResult> IsBirimNameExists(string birimAdi, Guid? excludeBirimId)
        {
            // Stub metod, gerçek implementasyona göre doldurulacak
            return Json(new { exists = false });
        }

        [HttpPost]
        public async Task<IActionResult> ToggleActive(Guid id)
        {
            // Stub metod, gerçek implementasyona göre doldurulacak
            return Json(new { success = false, message = "Not implemented" });
        }

        [HttpPost]
        public async Task<IActionResult> IsBirimInUse(Guid id)
        {
            // Stub metod, gerçek implementasyona göre doldurulacak
            return Json(new { inUse = false });
        }

        private bool BirimExists(Guid id)
        {
            return _context.Birimler.Any(e => e.BirimID == id);
        }
    }
} 