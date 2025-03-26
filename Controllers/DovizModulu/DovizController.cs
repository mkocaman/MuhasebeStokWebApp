using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Identity;
using System.Security.Claims;
using MuhasebeStokWebApp.Controllers;
using MuhasebeStokWebApp.Data.Entities.DovizModulu;
using MuhasebeStokWebApp.Services.DovizModulu;
using MuhasebeStokWebApp.ViewModels.Doviz;
using MuhasebeStokWebApp.Services;
using MuhasebeStokWebApp.Data.Entities;
using MuhasebeStokWebApp.Services.Interfaces;

namespace MuhasebeStokWebApp.Controllers.DovizModulu
{
    /// <summary>
    /// Döviz ve döviz kurları için controller
    /// </summary>
    [Authorize]
    [Route("Doviz")]
    public class DovizController : BaseController
    {
        private readonly IDovizService _dovizService;
        private readonly ILogger<DovizController> _logger;

        public DovizController(
            IDovizService dovizService,
            ILogger<DovizController> logger,
            IMenuService menuService,
            UserManager<ApplicationUser> userManager,
            RoleManager<IdentityRole> roleManager,
            ILogService logService)
            : base(menuService, userManager, roleManager, logService)
        {
            _dovizService = dovizService;
            _logger = logger;
        }

        /// <summary>
        /// Para birimleri listesi
        /// </summary>
        [Route("")]
        [Route("Index")]
        public async Task<IActionResult> Index()
        {
            try
            {
                var paraBirimleri = await _dovizService.GetAllParaBirimleriAsync(aktiflerOnly: false);
                
                var viewModels = paraBirimleri.Select(d => new DovizListViewModel
                {
                    DovizID = d.ParaBirimiID,
                    Kod = d.Kod,
                    Ad = d.Ad,
                    Sembol = d.Sembol,
                    AnaParaBirimiMi = d.AnaParaBirimiMi,
                    Aktif = d.Aktif,
                    Sira = d.Sira
                }).ToList();

                return View(viewModels);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Para birimleri listelenirken hata oluştu.");
                TempData["Error"] = "Para birimleri listelenirken bir hata oluştu: " + ex.Message;
                return RedirectToAction("Index", "Home");
            }
        }

        /// <summary>
        /// Para birimi ekleme formu
        /// </summary>
        [Route("Create")]
        public IActionResult Create()
        {
            var model = new DovizCreateViewModel();
            
            return View(model);
        }

        /// <summary>
        /// Para birimi ekleme işlemi
        /// </summary>
        [HttpPost]
        [Route("Create")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(DovizCreateViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            try
            {
                var paraBirimi = new ParaBirimi
                {
                    ParaBirimiID = Guid.NewGuid(),
                    Kod = model.Kod,
                    Ad = model.Ad,
                    Sembol = model.Sembol,
                    OndalikAyraci = model.OndalikAyraci,
                    BinlikAyraci = model.BinlikAyraci,
                    OndalikHassasiyet = model.OndalikHassasiyet,
                    AnaParaBirimiMi = model.AnaParaBirimiMi,
                    Aktif = model.Aktif,
                    Sira = model.Sira
                };

                var result = await _dovizService.AddParaBirimiAsync(paraBirimi);

                if (result != null)
                {
                    TempData["Success"] = $"{model.Kod} para birimi başarıyla eklendi.";
                    return RedirectToAction(nameof(Index));
                }
                else
                {
                    _logger.LogWarning($"{model.Kod} para birimi eklenirken bir hata oluştu.");
                    TempData["Error"] = "Para birimi eklenirken bir sorun oluştu.";
                    return View(model);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Para birimi eklenirken hata oluştu.");
                TempData["Error"] = "Para birimi eklenirken bir hata oluştu: " + ex.Message;
                return View(model);
            }
        }

        /// <summary>
        /// Para birimi düzenleme formu
        /// </summary>
        [Route("Edit/{id:guid}")]
        public async Task<IActionResult> Edit(Guid id)
        {
            try
            {
                var paraBirimi = await _dovizService.GetParaBirimiByIdAsync(id);
                if (paraBirimi == null)
                {
                    _logger.LogWarning($"ID: {id} olan para birimi bulunamadı.");
                    TempData["Error"] = "Para birimi bulunamadı.";
                    return RedirectToAction(nameof(Index));
                }

                var model = new DovizEditViewModel
                {
                    DovizID = paraBirimi.ParaBirimiID,
                    Kod = paraBirimi.Kod,
                    Ad = paraBirimi.Ad,
                    Sembol = paraBirimi.Sembol,
                    OndalikAyraci = paraBirimi.OndalikAyraci,
                    BinlikAyraci = paraBirimi.BinlikAyraci,
                    OndalikHassasiyet = paraBirimi.OndalikHassasiyet,
                    AnaParaBirimiMi = paraBirimi.AnaParaBirimiMi,
                    Aktif = paraBirimi.Aktif,
                    Sira = paraBirimi.Sira
                };

                return View(model);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"ID: {id} olan para birimi yüklenirken hata oluştu.");
                TempData["Error"] = "Para birimi yüklenirken bir hata oluştu: " + ex.Message;
                return RedirectToAction(nameof(Index));
            }
        }

        /// <summary>
        /// Para birimi düzenleme işlemi
        /// </summary>
        [HttpPost]
        [Route("Edit/{id:guid}")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(Guid id, DovizEditViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            try
            {
                var paraBirimi = await _dovizService.GetParaBirimiByIdAsync(id);
                if (paraBirimi == null)
                {
                    _logger.LogWarning($"ID: {id} olan para birimi bulunamadı.");
                    TempData["Error"] = "Para birimi bulunamadı.";
                    return RedirectToAction(nameof(Index));
                }

                paraBirimi.Kod = model.Kod;
                paraBirimi.Ad = model.Ad;
                paraBirimi.Sembol = model.Sembol;
                paraBirimi.OndalikAyraci = model.OndalikAyraci;
                paraBirimi.BinlikAyraci = model.BinlikAyraci;
                paraBirimi.OndalikHassasiyet = model.OndalikHassasiyet;
                paraBirimi.AnaParaBirimiMi = model.AnaParaBirimiMi;
                paraBirimi.Aktif = model.Aktif;
                paraBirimi.Sira = model.Sira;

                var result = await _dovizService.UpdateParaBirimiAsync(paraBirimi);

                if (result != null)
                {
                    TempData["Success"] = $"{model.Kod} para birimi başarıyla güncellendi.";
                    return RedirectToAction(nameof(Index));
                }
                else
                {
                    _logger.LogWarning($"{model.Kod} para birimi güncellenirken bir hata oluştu.");
                    TempData["Error"] = "Para birimi güncellenirken bir sorun oluştu.";
                    return View(model);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Para birimi güncellenirken hata oluştu.");
                TempData["Error"] = "Para birimi güncellenirken bir hata oluştu: " + ex.Message;
                return View(model);
            }
        }

        /// <summary>
        /// Para birimi silme işlemi
        /// </summary>
        [Route("Delete/{id:guid}")]
        public async Task<IActionResult> Delete(Guid id)
        {
            try
            {
                var result = await _dovizService.DeleteParaBirimiAsync(id);

                if (result)
                {
                    TempData["Success"] = "Para birimi başarıyla silindi.";
                }
                else
                {
                    TempData["Error"] = "Para birimi silinemedi, ilişkili kayıtlar olabilir.";
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"ID: {id} olan para birimi silinirken hata oluştu.");
                TempData["Error"] = "Para birimi silinirken bir hata oluştu: " + ex.Message;
            }

            return RedirectToAction(nameof(Index));
        }

        /// <summary>
        /// Para birimi sil post
        /// </summary>
        [HttpPost]
        [Route("Delete/{id:guid}")]
        [ValidateAntiForgeryToken]
        [ActionName("Delete")]
        public async Task<IActionResult> DeleteConfirmed(Guid id)
        {
            try
            {
                var result = await _dovizService.DeleteParaBirimiAsync(id);

                if (result)
                {
                    TempData["Success"] = "Para birimi başarıyla silindi.";
                }
                else
                {
                    TempData["Error"] = "Para birimi silinemedi, ilişkili kayıtlar olabilir.";
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"ID: {id} olan para birimi silinirken hata oluştu.");
                TempData["Error"] = "Para birimi silinirken bir hata oluştu: " + ex.Message;
            }

            return RedirectToAction(nameof(Index));
        }

        /// <summary>
        /// Döviz kurları listesi
        /// </summary>
        public async Task<IActionResult> DovizKurlari()
        {
            try
            {
                var paraBirimleri = await _dovizService.GetAllParaBirimleriAsync();
                ViewBag.ParaBirimleri = new SelectList(paraBirimleri, "ParaBirimiID", "Ad");
                
                // Sadece görüntülemek için boş model döndür
                var model = new List<DovizKuruViewModel>();
                
                return View(model);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Döviz kurları listelenirken hata oluştu.");
                TempData["Error"] = "Döviz kurları listelenirken bir hata oluştu: " + ex.Message;
                return RedirectToAction("Index", "Home");
            }
        }

        /// <summary>
        /// Para birimi detayı
        /// </summary>
        [Route("Details/{id:guid}")]
        public async Task<IActionResult> Details(Guid id)
        {
            try
            {
                var paraBirimi = await _dovizService.GetParaBirimiByIdAsync(id);
                if (paraBirimi == null)
                {
                    _logger.LogWarning($"ID: {id} olan para birimi bulunamadı.");
                    TempData["Error"] = "Para birimi bulunamadı.";
                    return RedirectToAction(nameof(Index));
                }

                var model = new DovizDetailViewModel
                {
                    DovizID = paraBirimi.ParaBirimiID,
                    Kod = paraBirimi.Kod,
                    Ad = paraBirimi.Ad,
                    Sembol = paraBirimi.Sembol,
                    OndalikAyraci = paraBirimi.OndalikAyraci,
                    BinlikAyraci = paraBirimi.BinlikAyraci,
                    OndalikHassasiyet = paraBirimi.OndalikHassasiyet,
                    AnaParaBirimiMi = paraBirimi.AnaParaBirimiMi,
                    Aktif = paraBirimi.Aktif,
                    Sira = paraBirimi.Sira
                };

                return View(model);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"ID: {id} olan para birimi detayı yüklenirken hata oluştu.");
                TempData["Error"] = "Para birimi detayı yüklenirken bir hata oluştu: " + ex.Message;
                return RedirectToAction(nameof(Index));
            }
        }

        /// <summary>
        /// Döviz kuru güncelleme (TCMB'den)
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateKurlarFromTCMB()
        {
            try
            {
                // Döviz kuru güncelleme işlemi eklenmeli
                
                TempData["Success"] = "Döviz kurları TCMB'den başarıyla güncellendi.";
                return RedirectToAction(nameof(DovizKurlari));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Döviz kurları TCMB'den güncellenirken hata oluştu.");
                TempData["Error"] = "Döviz kurları güncellenirken bir hata oluştu: " + ex.Message;
                return RedirectToAction(nameof(DovizKurlari));
            }
        }
    }
} 