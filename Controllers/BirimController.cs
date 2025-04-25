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
using MuhasebeStokWebApp.Data.Repositories;
using AutoMapper;

namespace MuhasebeStokWebApp.Controllers
{
    [Authorize]
    public class BirimController : BaseController
    {
        private readonly ApplicationDbContext _context;
        private readonly IBirimService _birimService;
        private readonly ILogger<BirimController> _logger;
        private readonly IValidationService _validationService;
        private readonly new UserManager<ApplicationUser> _userManager;
        private readonly IRepository<Birim> _birimRepository;
        private readonly IMapper _mapper;

        public BirimController(
            ApplicationDbContext context,
            IBirimService birimService,
            ILogger<BirimController> logger,
            IValidationService validationService,
            IMenuService menuService,
            UserManager<ApplicationUser> userManager,
            RoleManager<IdentityRole> roleManager,
            ILogService logService,
            IRepository<Birim> birimRepository,
            IMapper mapper)
            : base(menuService, userManager, roleManager, logService)
        {
            _context = context;
            _birimService = birimService;
            _logger = logger;
            _validationService = validationService;
            _userManager = userManager;
            _birimRepository = birimRepository;
            _mapper = mapper;
        }

        // GET: Birim
        public async Task<IActionResult> Index(string tab = "aktif")
        {
            try
            {
                var birimler = await _birimService.GetAllAsync();
                var aktifBirimler = birimler.Where(x => x.Aktif && !x.Silindi).ToList();
                var pasifBirimler = birimler.Where(x => !x.Aktif && !x.Silindi).ToList();
                var silinmisBirimler = birimler.Where(x => x.Silindi).ToList();

                _logger.LogInformation($"Birim sayısı: Aktif={aktifBirimler.Count}, Pasif={pasifBirimler.Count}, Silinmiş={silinmisBirimler.Count}");

                var viewModel = new BirimListViewModel();
                
                switch (tab.ToLower())
                {
                    case "pasif":
                        viewModel.Birimler = _mapper.Map<List<BirimViewModel>>(pasifBirimler);
                        break;
                    case "silindi":
                        viewModel.Birimler = _mapper.Map<List<BirimViewModel>>(silinmisBirimler);
                        break;
                    default: // "aktif"
                        viewModel.Birimler = _mapper.Map<List<BirimViewModel>>(aktifBirimler);
                        tab = "aktif"; // Varsayılan sekmeyi belirle
                        break;
                }

                ViewBag.AktifTab = tab;
                return View(viewModel);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Birimler listelenirken bir hata oluştu");
                return View("Error");
            }
        }

        // GET: Birim/Details/5
        public async Task<IActionResult> Details(Guid? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var birim = await _context.Birimler
                .IgnoreQueryFilters() // Global filtreleri devre dışı bırak
                .FirstOrDefaultAsync(b => b.BirimID == id);

            if (birim == null)
            {
                return NotFound();
            }

            var viewModel = _mapper.Map<BirimViewModel>(birim);
            
            // AJAX isteği için modal içeriği döndür
            if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
            {
                return PartialView("_DetailsPartial", viewModel);
            }
            
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
                if (!ModelState.IsValid)
                {
                    var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage);
                    return Json(new { success = false, message = "Form alanlarını doğru şekilde doldurunuz.", errors = errors });
                }

                var currentUserId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
                
                if (string.IsNullOrEmpty(currentUserId))
                {
                    return Json(new { success = false, message = "Kullanıcı bilgisi bulunamadı." });
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
                    OlusturanKullaniciID = Guid.TryParse(currentUserId, out Guid kullaniciGuid) ? kullaniciGuid : (Guid?)null,
                    BirimKodu = model.BirimAdi.Length > 3 ? model.BirimAdi.Substring(0, 3).ToUpper() : model.BirimAdi.ToUpper(),
                    Silindi = false,
                    BirimSembol = model.BirimAdi.Length > 3 ? model.BirimAdi.Substring(0, 3).ToUpper() : model.BirimAdi.ToUpper()
                };

                // Önce birim ekle, sonra log oluştur
                _context.Add(birim);
                
                try
                {
                    // Log işlemini burada yapmak yerine, önce kaydet
                    await _context.SaveChangesAsync();
                    
                    return Json(new { success = true, message = "Birim başarıyla oluşturuldu." });
                }
                catch (DbUpdateException ex)
                {
                    var message = "Birim kaydedilirken bir hata oluştu. Lütfen tekrar deneyin.";
                    return Json(new { success = false, message = message });
                }
            }
            catch (Exception ex)
            {
                var message = "Birim oluşturulurken bir hata oluştu.";
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
                .IgnoreQueryFilters() // Global filtreleri devre dışı bırak
                .FirstOrDefaultAsync(b => b.BirimID == id);

            if (birim == null)
            {
                return NotFound();
            }

            var viewModel = _mapper.Map<BirimEditViewModel>(birim);

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
                    .IgnoreQueryFilters() // Global filtreleri devre dışı bırak
                    .FirstOrDefaultAsync(b => b.BirimID == id);

                if (birim == null)
                {
                    return Json(new { success = false, message = "Birim bulunamadı." });
                }

                // Model'den entity'ye değerleri aktarma
                birim.BirimAdi = model.BirimAdi.Trim();
                birim.Aciklama = model.Aciklama?.Trim() ?? string.Empty;

                // Aktif değerini doğru bir şekilde al
                birim.Aktif = model.Aktif;

                birim.GuncellemeTarihi = DateTime.Now;
                
                // OlusturanKullaniciID'nin null olup olmadığını kontrol et
                if (birim.OlusturanKullaniciID == null)
                {
                    var currentUserId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
                    birim.OlusturanKullaniciID = string.IsNullOrEmpty(currentUserId) ? null : (Guid?)Guid.Parse(currentUserId);
                }
                
                // Güncelleyen kullanıcı ID'sini al
                var updateUserId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
                birim.SonGuncelleyenKullaniciID = string.IsNullOrEmpty(updateUserId) ? null : (Guid?)Guid.Parse(updateUserId);

                _context.Update(birim);
                await _context.SaveChangesAsync();
                
                return Json(new { success = true, message = "Birim başarıyla güncellendi." });
            }
            catch (DbUpdateConcurrencyException ex)
            {
                _logger.LogError("Birim güncellenirken veri tutarsızlığı oluştu.", ex);
                return Json(new { success = false, message = "Birim daha önce başka bir kullanıcı tarafından değiştirilmiş olabilir." });
            }
            catch (Exception ex)
            {
                _logger.LogError("Birim güncellenirken hata oluştu.", ex);
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

            var viewModel = _mapper.Map<BirimViewModel>(birim);
            
            if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
            {
                return Json(viewModel);
            }
            
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
            birim.GuncellemeTarihi = DateTime.Now;
            
            // Güncelleyen kullanıcı ID'sini al
            var updateUserId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            birim.SonGuncelleyenKullaniciID = string.IsNullOrEmpty(updateUserId) ? null : (Guid?)Guid.Parse(updateUserId);
            
            _context.Update(birim);
            await _context.SaveChangesAsync();
            
            // Önbelleği temizle
            await _birimService.ClearCacheAsync();
            
            if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
            {
                return Json(new { success = true, message = "Birim başarıyla silindi." });
            }
            
            TempData["SuccessMessage"] = "Birim başarıyla silindi.";
            return RedirectToAction(nameof(Index));
        }
        
        // AJAX için silme metodu
        [HttpPost]
        public async Task<IActionResult> DeleteBirim(Guid id)
        {
            try
            {
                var birim = await _context.Birimler.FindAsync(id);
                if (birim == null)
                {
                    return Json(new { success = false, message = "Birim bulunamadı." });
                }
                
                // Soft delete işlemi
                birim.Silindi = true;
                birim.Aktif = false;
                birim.GuncellemeTarihi = DateTime.Now;
                
                // Güncelleyen kullanıcı ID'sini al
                var updateUserId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
                birim.SonGuncelleyenKullaniciID = string.IsNullOrEmpty(updateUserId) ? null : (Guid?)Guid.Parse(updateUserId);
                
                _context.Update(birim);
                await _context.SaveChangesAsync();
                
                // Önbelleği temizle
                await _birimService.ClearCacheAsync();
                
                return Json(new { success = true, message = "Birim başarıyla silindi." });
            }
            catch (Exception ex)
            {
                var message = "Birim silinirken bir hata oluştu.";
                _logger.LogError(message, ex);
                return Json(new { success = false, message = message });
            }
        }

        [HttpPost]
        public async Task<IActionResult> GetBirimDetails(Guid id)
        {
            try 
            {
                var birim = await _context.Birimler
                    .IgnoreQueryFilters() // Global filtreleri devre dışı bırak
                    .AsNoTracking()
                    .FirstOrDefaultAsync(b => b.BirimID == id);
                    
                if (birim == null)
                {
                    return Json(new { success = false, message = "Birim bulunamadı." });
                }
                
                var viewModel = _mapper.Map<BirimViewModel>(birim);
                return Json(new { success = true, data = viewModel });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Birim detayları alınırken hata oluştu");
                return Json(new { success = false, message = "Birim bilgileri yüklenirken bir hata oluştu." });
            }
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
            try
            {
                var birim = await _context.Birimler
                    .IgnoreQueryFilters() // Global filtreleri devre dışı bırak
                    .FirstOrDefaultAsync(b => b.BirimID == id);
                    
                if (birim == null)
                {
                    return Json(new { success = false, message = "Birim bulunamadı." });
                }

                // Durumu tersine çevir
                birim.Aktif = !birim.Aktif;
                birim.GuncellemeTarihi = DateTime.Now;
                
                // Güncelleyen kullanıcı ID'sini al
                var updateUserId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
                birim.SonGuncelleyenKullaniciID = string.IsNullOrEmpty(updateUserId) ? null : (Guid?)Guid.Parse(updateUserId);

                _context.Update(birim);
                await _context.SaveChangesAsync();
                
                // Önbelleği temizle
                await _birimService.ClearCacheAsync();
                
                var durum = birim.Aktif ? "aktif" : "pasif";
                return Json(new { success = true, message = $"Birim başarıyla {durum} edildi." });
            }
            catch (Exception ex)
            {
                var message = "Birim durumu değiştirilirken bir hata oluştu.";
                _logger.LogError(message, ex);
                return Json(new { success = false, message = message });
            }
        }

        [HttpPost]
        public async Task<IActionResult> IsBirimInUse(Guid id)
        {
            // Stub metod, gerçek implementasyona göre doldurulacak
            return Json(new { inUse = false });
        }

        // Pasife alınan veya silinen birimleri geri alma işlemi
        [HttpPost]
        public async Task<IActionResult> RestoreUnit(Guid id)
        {
            try
            {
                var birim = await _context.Birimler
                    .IgnoreQueryFilters() // Global filtreleri devre dışı bırak
                    .FirstOrDefaultAsync(b => b.BirimID == id);
                    
                if (birim == null)
                {
                    return Json(new { success = false, message = "Birim bulunamadı." });
                }

                // Birimi aktif et
                birim.Aktif = true;
                birim.Silindi = false; // Eğer silindiyse, silindi durumunu da false yap
                birim.GuncellemeTarihi = DateTime.Now;
                
                // Güncelleyen kullanıcı ID'sini al
                var updateUserId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
                birim.SonGuncelleyenKullaniciID = string.IsNullOrEmpty(updateUserId) ? null : (Guid?)Guid.Parse(updateUserId);

                await _context.SaveChangesAsync();
                
                // Önbelleği temizle
                await _birimService.ClearCacheAsync();
                
                return Json(new { success = true, message = "Birim başarıyla aktif edildi." });
            }
            catch (Exception ex)
            {
                var message = "Birim aktif edilirken bir hata oluştu.";
                _logger.LogError(message, ex);
                return Json(new { success = false, message = message });
            }
        }

        private bool BirimExists(Guid id)
        {
            return _context.Birimler.Any(e => e.BirimID == id);
        }
    }
} 