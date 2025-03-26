using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using MuhasebeStokWebApp.Data;
using MuhasebeStokWebApp.Data.Entities;
using MuhasebeStokWebApp.Models;
using MuhasebeStokWebApp.Services.Interfaces;
using MuhasebeStokWebApp.ViewModels.Cari;
using MuhasebeStokWebApp.Services;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Linq;

namespace MuhasebeStokWebApp.Controllers
{
    [Authorize]
    public class CariController : BaseController
    {
        private readonly ApplicationDbContext _context;
        private readonly ICariService _cariService;

        public CariController(
            ApplicationDbContext context, 
            ICariService cariService, 
            ILogService logService,
            IMenuService menuService,
            UserManager<ApplicationUser> userManager,
            RoleManager<IdentityRole> roleManager) 
            : base(menuService, userManager, roleManager, logService)
        {
            _context = context;
            _cariService = cariService;
        }

        public async Task<IActionResult> Index()
        {
            try
            {
                var cariler = await _cariService.GetAllAsync();
                return View(cariler);
            }
            catch (Exception ex)
            {
                await _logService.AddLogAsync("Hata", $"Cari listesi alınırken hata oluştu: {ex.Message}", "CariController/Index");
                TempData["ErrorMessage"] = "Cari listesi alınırken bir hata oluştu.";
                return View(new List<Cari>());
            }
        }

        [HttpGet]
        public IActionResult Create()
        {
            var model = new CariCreateViewModel
            {
                Ad = "",
                VergiNo = "",
                Telefon = "",
                Email = "",
                Yetkili = "",
                BaslangicBakiye = 0,
                Adres = "",
                Aciklama = "",
                CariKodu = "",
                CariTipi = "",
                VergiDairesi = "",
                Il = "",
                Ilce = "",
                PostaKodu = "",
                Ulke = "Türkiye",
                WebSitesi = "",
                Notlar = "",
                Aktif = true
            };
            
            return PartialView("_CreatePartial", model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CariCreateViewModel model)
        {
            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values
                    .SelectMany(v => v.Errors)
                    .Select(e => e.ErrorMessage)
                    .ToList();

                return Json(new { success = false, message = "Form doğrulama hatası!", errors = errors });
            }

            try
            {
                var cari = new Cari
                {
                    CariID = Guid.NewGuid(),
                    Ad = model.Ad,
                    VergiNo = model.VergiNo,
                    Telefon = model.Telefon,
                    Email = model.Email,
                    Yetkili = model.Yetkili,
                    BaslangicBakiye = model.BaslangicBakiye,
                    Adres = model.Adres,
                    Aciklama = model.Aciklama,
                    CariKodu = model.CariKodu ?? "",
                    CariTipi = model.CariTipi ?? "",
                    VergiDairesi = model.VergiDairesi ?? "",
                    Il = model.Il ?? "",
                    Ilce = model.Ilce ?? "",
                    PostaKodu = model.PostaKodu ?? "",
                    Ulke = model.Ulke ?? "",
                    WebSitesi = model.WebSitesi ?? "",
                    Notlar = model.Notlar ?? "",
                    AktifMi = model.Aktif,
                    OlusturmaTarihi = DateTime.Now
                };

                try
                {
                    await _cariService.AddAsync(cari);
                    try
                    {
                        await _logService.AddLogAsync("Bilgi", $"{cari.Ad} adlı cari eklendi.", "CariController/Create");
                    }
                    catch (Exception logEx)
                    {
                        // Loglama hatası, ama cari kaydedildi o yüzden devam et
                        Console.WriteLine($"Loglama hatası: {logEx.Message}");
                    }

                    return Json(new { success = true, message = "Cari başarıyla eklendi." });
                }
                catch (Exception ex)
                {
                    string message = $"Cari eklenirken veritabanı hatası oluştu: {ex.Message}";
                    if (ex.InnerException != null)
                    {
                        message += $" İç hata: {ex.InnerException.Message}";
                    }
                    
                    await _logService.LogErrorAsync("CariController.Create", ex);
                    return Json(new { success = false, message = message });
                }
            }
            catch (Exception ex)
            {
                string message = $"Cari eklenirken beklenmeyen bir hata oluştu: {ex.Message}";
                if (ex.InnerException != null)
                {
                    message += $" İç hata: {ex.InnerException.Message}";
                }
                
                try
                {
                    await _logService.LogErrorAsync("CariController.Create", ex);
                }
                catch {}
                
                return Json(new { success = false, message = message });
            }
        }

        [HttpGet]
        public async Task<IActionResult> Edit(Guid id)
        {
            try
            {
                var cari = await _cariService.GetByIdAsync(id);
                if (cari == null)
                {
                    return Json(new { success = false, message = "Cari bulunamadı." });
                }

                var model = new CariEditViewModel
                {
                    Id = cari.CariID,
                    Ad = cari.Ad,
                    VergiNo = cari.VergiNo,
                    Telefon = cari.Telefon,
                    Email = cari.Email,
                    Yetkili = cari.Yetkili,
                    BaslangicBakiye = cari.BaslangicBakiye,
                    Adres = cari.Adres,
                    Aciklama = cari.Aciklama
                };

                return PartialView("_EditPartial", model);
            }
            catch (Exception ex)
            {
                await _logService.AddLogAsync("Hata", $"Cari düzenleme formunu açarken hata oluştu: {ex.Message}", "CariController/Edit");
                return Json(new { success = false, message = "Cari bilgileri alınırken bir hata oluştu." });
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(CariEditViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return Json(new { success = false, message = "Form doğrulama hatası!" });
            }

            try
            {
                var cari = await _cariService.GetByIdAsync(model.Id);
                if (cari == null)
                {
                    return Json(new { success = false, message = "Cari bulunamadı." });
                }

                cari.Ad = model.Ad;
                cari.VergiNo = model.VergiNo;
                cari.Telefon = model.Telefon;
                cari.Email = model.Email;
                cari.Yetkili = model.Yetkili;
                cari.BaslangicBakiye = model.BaslangicBakiye;
                cari.Adres = model.Adres;
                cari.Aciklama = model.Aciklama;
                cari.GuncellemeTarihi = DateTime.Now;

                await _cariService.UpdateAsync(cari);
                await _logService.AddLogAsync("Bilgi", $"{cari.Ad} adlı cari güncellendi.", "CariController/Edit");

                return Json(new { success = true, message = "Cari başarıyla güncellendi." });
            }
            catch (Exception ex)
            {
                await _logService.AddLogAsync("Hata", $"Cari güncellenirken hata oluştu: {ex.Message}", "CariController/Edit");
                return Json(new { success = false, message = "Cari güncellenirken bir hata oluştu." });
            }
        }

        [HttpPost]
        public async Task<IActionResult> Delete(Guid id)
        {
            try
            {
                var cari = await _cariService.GetByIdAsync(id);
                if (cari == null)
                {
                    return Json(new { success = false, message = "Cari bulunamadı." });
                }

                await _cariService.DeleteAsync(cari);
                await _logService.AddLogAsync("Bilgi", $"{cari.Ad} adlı cari silindi.", "CariController/Delete");

                return Json(new { success = true, message = "Cari başarıyla silindi." });
            }
            catch (Exception ex)
            {
                await _logService.AddLogAsync("Hata", $"Cari silinirken hata oluştu: {ex.Message}", "CariController/Delete");
                return Json(new { success = false, message = "Cari silinirken bir hata oluştu." });
            }
        }
    }
}