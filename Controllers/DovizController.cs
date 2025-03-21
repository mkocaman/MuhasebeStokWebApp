using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using MuhasebeStokWebApp.Services;
using MuhasebeStokWebApp.ViewModels.DovizKuru;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using MuhasebeStokWebApp.Data;
using Microsoft.AspNetCore.Identity;

namespace MuhasebeStokWebApp.Controllers
{
    // [Authorize] - Geçici olarak kaldırıldı
    public class DovizController : BaseController
    {
        private readonly ILogService _logService;
        private readonly IDovizKuruService _dovizKuruService;
        private readonly IParaBirimiService _paraBirimiService;

        public DovizController(
            ILogService logService,
            IDovizKuruService dovizKuruService,
            IParaBirimiService paraBirimiService,
            IMenuService menuService,
            UserManager<IdentityUser> userManager,
            RoleManager<IdentityRole> roleManager) : base(menuService, userManager, roleManager, logService)
        {
            _logService = logService;
            _dovizKuruService = dovizKuruService;
            _paraBirimiService = paraBirimiService;
        }

        // GET: Doviz
        public IActionResult Index()
        {
            return RedirectToAction("Index", "ParaBirimi");
        }

        // GET: Doviz/Ayarlar
        public async Task<IActionResult> Ayarlar()
        {
            return RedirectToAction("ParaBirimiAyarlari", "DovizKuru");
        }

        // GET: Doviz/Cevirici
        public IActionResult Cevirici()
        {
            return View();
        }
        
        // GET: Doviz/ManuelEkle
        public async Task<IActionResult> ManuelEkle()
        {
            var viewModel = new DovizKuruEkleViewModel
            {
                Tarih = DateTime.Now,
                KayitTarihi = DateTime.Now
            };
            
            var paraBirimleri = await _paraBirimiService.GetAllParaBirimleriAsync();
            ViewBag.ParaBirimleri = paraBirimleri;
            
            return View(viewModel);
        }
        
        // POST: Doviz/ManuelEkle
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ManuelEkle(DovizKuruEkleViewModel viewModel)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    await _dovizKuruService.AddDovizKuruManuelAsync(viewModel);
                    TempData["SuccessMessage"] = "Döviz kuru başarıyla eklendi.";
                    return RedirectToAction(nameof(ManuelEkle));
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError("", $"Döviz kuru eklenirken hata oluştu: {ex.Message}");
                }
            }
            
            var paraBirimleri = await _paraBirimiService.GetAllParaBirimleriAsync();
            ViewBag.ParaBirimleri = paraBirimleri;
            
            return View(viewModel);
        }
    }
} 