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
using AutoMapper;

namespace MuhasebeStokWebApp.Controllers
{
    [Authorize]
    public class DepoController : BaseController
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<DepoController> _logger;
        private new readonly ILogService _logService;
        private readonly IMapper _mapper;

        public DepoController(
            ApplicationDbContext context,
            IMenuService menuService,
            UserManager<ApplicationUser> userManager,
            RoleManager<IdentityRole> roleManager,
            ILogService logService,
            ILogger<DepoController> logger,
            IMapper mapper)
            : base(menuService, userManager, roleManager, logService)
        {
            _context = context;
            _logger = logger;
            _logService = logService;
            _mapper = mapper;
        }

        // GET: Depo
        public async Task<IActionResult> Index(string tab = "aktif")
        {
            var tumDepolar = await _context.Depolar
                .IgnoreQueryFilters()
                .ToListAsync();
            
            _logger.LogInformation($"Toplam depo sayısı: {tumDepolar.Count}");
            _logger.LogInformation($"Aktif depo sayısı: {tumDepolar.Count(d => d.Aktif && !d.Silindi)}");
            _logger.LogInformation($"Pasif depo sayısı: {tumDepolar.Count(d => !d.Aktif && !d.Silindi)}");
            _logger.LogInformation($"Silinmiş depo sayısı: {tumDepolar.Count(d => d.Silindi)}");
            
            var viewModel = new DepoListViewModel
            {
                Depolar = tumDepolar.Select(d => new DepoViewModel
                {
                    DepoID = d.DepoID,
                    DepoAdi = d.DepoAdi,
                    DepoKodu = null,
                    Aciklama = null,
                    Adres = d.Adres,
                    Aktif = d.Aktif,
                    OlusturmaTarihi = d.OlusturmaTarihi ?? DateTime.MinValue,
                    Silindi = d.Silindi
                }).ToList(),
                AktifSekme = tab
            };
            
            if (tab == "aktif")
            {
                viewModel.Depolar = viewModel.Depolar.Where(d => d.Aktif && !d.Silindi).ToList();
            }
            else if (tab == "pasif")
            {
                viewModel.Depolar = viewModel.Depolar.Where(d => !d.Aktif && !d.Silindi).ToList();
            }
            else if (tab == "silindi" && User.IsInRole("Admin"))
            {
                viewModel.Depolar = viewModel.Depolar.Where(d => d.Silindi).ToList();
            }

            ViewBag.AktifTab = tab;
            
            return View(viewModel);
        }

        // GET: Depo/Details/5
        public async Task<IActionResult> Details(Guid? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var depo = await _context.Depolar
                .IgnoreQueryFilters()
                .FirstOrDefaultAsync(d => d.DepoID == id);

            if (depo == null)
            {
                return NotFound();
            }

            var viewModel = _mapper.Map<DepoViewModel>(depo);
            
            // AJAX isteği için modal içeriği döndür
            if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
            {
                return PartialView("_DetailsPartial", viewModel);
            }
            
            return View(viewModel);
        }

        // GET: Depo/Create
        public IActionResult Create()
        {
            var model = new DepoCreateViewModel
            {
                Aktif = true
            };
            return PartialView("_CreatePartial", model);
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
                        return PartialView("_CreatePartial", model);
                    }

                    var depo = new Depo
                    {
                        DepoID = Guid.NewGuid(),
                        DepoAdi = model.DepoAdi,
                        Adres = model.Adres,
                        OlusturmaTarihi = DateTime.Now,
                        OlusturanKullaniciID = kullaniciId,
                        Silindi = false,
                        Aktif = model.Aktif
                    };

                    _context.Add(depo);
                    await _context.SaveChangesAsync();
                    
                    // Log kaydı ekle
                    await _logService.DepoOlusturmaLogOlustur(depo.DepoID.ToString(), depo.DepoAdi);
                    
                    // AJAX isteği için başarılı sonuç döndür
                    return Json(new { success = true, message = "Depo başarıyla oluşturuldu." });
                }

                foreach (var state in ModelState)
                {
                    foreach (var error in state.Value.Errors)
                    {
                        _logger.LogWarning($"Hata - {state.Key}: {error.ErrorMessage}");
                    }
                }
                
                // AJAX isteği için hata sonucu döndür
                return Json(new { success = false, message = "Depo oluşturulurken hatalar oluştu.", errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage) });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Depo oluşturulurken hata oluştu");
                ModelState.AddModelError("", "Depo oluşturulurken bir hata oluştu. Lütfen tekrar deneyin.");
                
                // AJAX isteği için hata sonucu döndür
                return Json(new { success = false, message = "Depo oluşturulurken bir hata oluştu: " + ex.Message });
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
                .IgnoreQueryFilters()
                .FirstOrDefaultAsync(d => d.DepoID == id);

            if (depo == null)
            {
                return NotFound();
            }

            var viewModel = _mapper.Map<DepoEditViewModel>(depo);

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

                    // Log kaydı ekle
                    await _logService.DepoGuncellemeLogOlustur(depo.DepoID.ToString(), depo.DepoAdi);

                    if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
                    {
                        return Json(new { success = true, message = "Depo başarıyla güncellendi." });
                    }

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

            if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
            {
                return Json(new { success = false, message = "Formda hatalar var.", errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage) });
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

            if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
            {
                return Json(model);
            }

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

            try
            {
                // Önce ilişkili stok hareketleri kontrol et
                var stokHareketleri = await _context.StokHareketleri
                    .Where(s => s.DepoID == id && !s.Silindi)
                    .ToListAsync();

                // Depo soft-delete
                depo.Silindi = true;
                depo.Aktif = false;
                depo.GuncellemeTarihi = DateTime.Now;
                depo.SonGuncelleyenKullaniciID = GetCurrentUserId();
                
                _context.Update(depo);
                await _context.SaveChangesAsync();

                // Log kaydı ekle
                await _logService.DepoSilmeLogOlustur(depo.DepoID.ToString(), depo.DepoAdi);

                if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
                {
                    return Json(new { success = true, message = "Depo başarıyla silindi." });
                }
                
                TempData["SuccessMessage"] = "Depo başarıyla silindi.";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Depo silinirken hata oluştu");
                
                if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
                {
                    return Json(new { success = false, message = "Depo silinirken bir hata oluştu: " + ex.Message });
                }
                
                TempData["ErrorMessage"] = "Depo silinirken bir hata oluştu.";
                return RedirectToAction(nameof(Index));
            }
        }

        // AJAX için DeleteDepo metodu
        [HttpPost]
        public async Task<IActionResult> DeleteDepo(Guid id)
        {
            try
            {
                var depo = await _context.Depolar.FindAsync(id);
                if (depo == null)
                {
                    return Json(new { success = false, message = "Depo bulunamadı." });
                }

                // Depoyu soft-delete yap
                depo.Silindi = true;
                depo.Aktif = false;
                depo.GuncellemeTarihi = DateTime.Now;
                depo.SonGuncelleyenKullaniciID = GetCurrentUserId();

                await _context.SaveChangesAsync();
                
                // Log kaydı ekle
                await _logService.DepoSilmeLogOlustur(depo.DepoID.ToString(), depo.DepoAdi);

                return Json(new { success = true, message = "Depo başarıyla silindi." });
            }
            catch (Exception ex)
            {
                var message = "Depo silinirken bir hata oluştu.";
                await _logService.LogErrorAsync(message, ex);
                return Json(new { success = false, message = message });
            }
        }

        // Pasife alınan veya silinen depoları geri alma işlemi
        [HttpPost]
        public async Task<IActionResult> RestoreDepo(Guid id)
        {
            try
            {
                var depo = await _context.Depolar
                    .IgnoreQueryFilters() // Global filtreleri devre dışı bırak
                    .FirstOrDefaultAsync(d => d.DepoID == id);
                    
                if (depo == null)
                {
                    return Json(new { success = false, message = "Depo bulunamadı." });
                }

                // Depoyu aktif et
                depo.Aktif = true;
                depo.Silindi = false; // Eğer silindiyse, silindi durumunu da false yap
                depo.GuncellemeTarihi = DateTime.Now;
                
                // Güncelleyen kullanıcı ID'sini al
                var updateUserId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
                depo.SonGuncelleyenKullaniciID = string.IsNullOrEmpty(updateUserId) ? null : (Guid?)Guid.Parse(updateUserId);

                await _context.SaveChangesAsync();
                
                // Log kaydı ekle
                await _logService.DepoOlusturmaLogOlustur(depo.DepoID.ToString(), depo.DepoAdi + " (geri yüklendi)");
                
                // AJAX isteği için başarılı sonuç döndür
                return Json(new { success = true, message = "Depo başarıyla geri yüklendi." });
            }
            catch (Exception ex)
            {
                var message = "Depo aktif edilirken bir hata oluştu.";
                _logger.LogError(message, ex);
                return Json(new { success = false, message = message });
            }
        }

        // Depo var mı kontrolü
        [NonAction]
        private async Task<bool> DepoExists(Guid id)
        {
            return await _context.Depolar.AnyAsync(d => d.DepoID == id && !d.Silindi);
        }

        // Aktifleştirme veya pasifleştirme için toggle metodu
        [HttpPost]
        public async Task<IActionResult> ToggleActive(Guid id)
        {
            try
            {
                var depo = await _context.Depolar
                    .IgnoreQueryFilters() // Global filtreleri devre dışı bırak
                    .FirstOrDefaultAsync(d => d.DepoID == id);
                    
                if (depo == null)
                {
                    return Json(new { success = false, message = "Depo bulunamadı." });
                }

                // Durumu tersine çevir
                depo.Aktif = !depo.Aktif;
                depo.GuncellemeTarihi = DateTime.Now;
                
                // Güncelleyen kullanıcı ID'sini al
                var updateUserId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
                depo.SonGuncelleyenKullaniciID = string.IsNullOrEmpty(updateUserId) ? null : (Guid?)Guid.Parse(updateUserId);

                _context.Update(depo);
                await _context.SaveChangesAsync();
                
                // Log kaydı ekle
                await _logService.DepoDurumDegisikligiLogOlustur(depo.DepoID.ToString(), depo.DepoAdi, depo.Aktif);
                
                // AJAX isteği için başarılı sonuç döndür
                var durumMesaj = depo.Aktif ? "aktif" : "pasif";
                return Json(new { success = true, message = $"Depo başarıyla {durumMesaj} edildi." });
            }
            catch (Exception ex)
            {
                var message = "Depo durumu değiştirilirken bir hata oluştu.";
                _logger.LogError(ex, message);
                return Json(new { success = false, message = message });
            }
        }

        // Kullanıcı ID'sini al
        private new Guid GetCurrentUserId()
        {
            try
            {
                var userIdClaim = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier);
                if (userIdClaim != null && !string.IsNullOrEmpty(userIdClaim.Value))
                {
                    return Guid.Parse(userIdClaim.Value);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "GetCurrentUserId: {Message}", ex.Message);
            }
            return Guid.Empty;
        }

        [HttpPost]
        public async Task<IActionResult> GetDepoDetails(Guid id)
        {
            try
            {
                var depo = await _context.Depolar
                    .IgnoreQueryFilters()
                    .AsNoTracking()
                    .FirstOrDefaultAsync(d => d.DepoID == id);
                    
                if (depo == null)
                {
                    return Json(new { success = false, message = "Depo bulunamadı." });
                }
                
                var viewModel = _mapper.Map<DepoViewModel>(depo);
                return Json(new { success = true, data = viewModel });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Depo detayları alınırken hata oluştu");
                return Json(new { success = false, message = "Depo bilgileri yüklenirken bir hata oluştu." });
            }
        }
    }
} 