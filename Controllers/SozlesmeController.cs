using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using MuhasebeStokWebApp.Data;
using MuhasebeStokWebApp.Data.Entities;
using MuhasebeStokWebApp.Services;
using MuhasebeStokWebApp.Services.Interfaces;
using MuhasebeStokWebApp.ViewModels.Sozlesme;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace MuhasebeStokWebApp.Controllers
{
    [Authorize]
    public class SozlesmeController : Controller
    {
        private readonly ISozlesmeService _sozlesmeService;
        private readonly ILogger<SozlesmeController> _logger;
        private readonly IWebHostEnvironment _hostingEnvironment;
        private readonly IDropdownService _dropdownService;

        public SozlesmeController(
            ISozlesmeService sozlesmeService,
            ILogger<SozlesmeController> logger,
            IWebHostEnvironment hostingEnvironment,
            IDropdownService dropdownService)
        {
            _sozlesmeService = sozlesmeService;
            _logger = logger;
            _hostingEnvironment = hostingEnvironment;
            _dropdownService = dropdownService;
        }

        // GET: Sozlesme
        public async Task<IActionResult> Index()
        {
            try
            {
                var model = await _sozlesmeService.GetSozlesmeListViewModelsAsync();
                return View(model);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Sözleşme listesi getirilirken hata oluştu");
                ModelState.AddModelError("", "Sözleşme listesi getirilirken bir hata oluştu: " + ex.Message);
                return View(new List<SozlesmeListViewModel>());
            }
        }

        // GET: Sozlesme/Details/5
        public async Task<IActionResult> Details(Guid? id)
        {
            try
            {
                if (id == null)
                {
                    return NotFound();
                }

                var model = await _sozlesmeService.GetSozlesmeDetailAsync(id.Value);
                if (model == null)
                {
                    _logger.LogWarning("Sözleşme bulunamadı: {Id}", id);
                    TempData["Error"] = "Sözleşme bulunamadı.";
                    return RedirectToAction(nameof(Index));
                }

                // Dosya yollarını URL'e dönüştür
                if (!string.IsNullOrEmpty(model.SozlesmeDosyaYolu))
                {
                    model.SozlesmeBelgesiYolu = Url.Content(model.SozlesmeDosyaYolu);
                }
                
                if (!string.IsNullOrEmpty(model.VekaletnameDosyaYolu))
                {
                    model.VekaletnameYolu = Url.Content(model.VekaletnameDosyaYolu);
                }

                return View(model);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Sözleşme detayları getirilirken hata oluştu. ID: {Id}", id);
                TempData["Error"] = "Sözleşme detayları getirilirken bir hata oluştu.";
                return RedirectToAction(nameof(Index));
            }
        }

        // GET: Sozlesme/Create
        public async Task<IActionResult> Create()
        {
            try
            {
                ViewBag.Cariler = await _dropdownService.GetCariSelectList();
                return View();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Sözleşme oluşturma sayfası yüklenirken hata oluştu");
                TempData["ErrorMessage"] = "Sözleşme oluşturma sayfası yüklenirken bir hata oluştu.";
                return RedirectToAction(nameof(Index));
            }
        }

        // POST: Sozlesme/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(SozlesmeViewModel viewModel)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    var sozlesme = new Sozlesme
                    {
                        SozlesmeNo = viewModel.SozlesmeNo,
                        SozlesmeTarihi = viewModel.SozlesmeTarihi,
                        BitisTarihi = viewModel.BitisTarihi,
                        CariID = !string.IsNullOrEmpty(viewModel.CariID) ? Guid.Parse(viewModel.CariID) : Guid.Empty,
                        VekaletGeldiMi = viewModel.VekaletGeldiMi,
                        ResmiFaturaKesildiMi = viewModel.ResmiFaturaKesildiMi,
                        Aciklama = viewModel.Aciklama,
                        SozlesmeTutari = viewModel.SozlesmeTutari,
                        SozlesmeDovizTuru = viewModel.SozlesmeDovizTuru ?? "TL",
                        AktifMi = true,
                        OlusturmaTarihi = DateTime.Now
                    };

                    // Dosya yükleme işlemleri
                    if (viewModel.SozlesmeBelgesi != null)
                    {
                        sozlesme.SozlesmeDosyaYolu = await DosyaYukle(viewModel.SozlesmeBelgesi, "sozlesmeler");
                    }

                    if (viewModel.Vekaletname != null)
                    {
                        sozlesme.VekaletnameDosyaYolu = await DosyaYukle(viewModel.Vekaletname, "vekaletler");
                    }

                    var result = await _sozlesmeService.AddSozlesmeAsync(sozlesme);
                    if (result)
                    {
                        TempData["SuccessMessage"] = "Sözleşme başarıyla oluşturuldu.";
                        return RedirectToAction(nameof(Index));
                    }
                    else
                    {
                        TempData["ErrorMessage"] = "Sözleşme oluşturulurken bir hata oluştu.";
                    }
                }
                
                ViewBag.Cariler = await _dropdownService.GetCariSelectList(viewModel.CariID != null ? Guid.Parse(viewModel.CariID) : (Guid?)null);
                return View(viewModel);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Sözleşme oluşturulurken hata oluştu");
                TempData["ErrorMessage"] = "Sözleşme oluşturulurken bir hata oluştu.";
                
                ViewBag.Cariler = await _dropdownService.GetCariSelectList(viewModel.CariID != null ? Guid.Parse(viewModel.CariID) : (Guid?)null);
                return View(viewModel);
            }
        }

        // GET: Sozlesme/Edit/5
        [HttpGet]
        public async Task<IActionResult> Edit(Guid id)
        {
            try
            {
                var model = await _sozlesmeService.GetSozlesmeDetailAsync(id);
                if (model == null)
                {
                    _logger.LogWarning("Sözleşme bulunamadı: {Id}", id);
                    TempData["Error"] = "Sözleşme bulunamadı.";
                    return RedirectToAction(nameof(Index));
                }

                ViewBag.Cariler = await _dropdownService.GetCariSelectList();
                return View(model);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Sözleşme düzenleme sayfası yüklenirken hata oluştu. ID: {Id}", id);
                ModelState.AddModelError("", "Sözleşme düzenleme sayfası yüklenirken bir hata oluştu: " + ex.Message);
                return RedirectToAction(nameof(Index));
            }
        }

        // POST: Sozlesme/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(Guid id, SozlesmeViewModel viewModel)
        {
            try
            {
                if (id.ToString() != viewModel.SozlesmeID)
                {
                    return NotFound();
                }

                if (ModelState.IsValid)
                {
                    var sozlesme = await _sozlesmeService.GetSozlesmeByIdAsync(id);
                    if (sozlesme == null)
                    {
                        return NotFound();
                    }

                    sozlesme.SozlesmeNo = viewModel.SozlesmeNo;
                    sozlesme.SozlesmeTarihi = viewModel.SozlesmeTarihi;
                    sozlesme.BitisTarihi = viewModel.BitisTarihi;
                    sozlesme.CariID = !string.IsNullOrEmpty(viewModel.CariID) ? Guid.Parse(viewModel.CariID) : Guid.Empty;
                    sozlesme.VekaletGeldiMi = viewModel.VekaletGeldiMi;
                    sozlesme.ResmiFaturaKesildiMi = viewModel.ResmiFaturaKesildiMi;
                    sozlesme.Aciklama = viewModel.Aciklama;
                    sozlesme.SozlesmeTutari = viewModel.SozlesmeTutari;
                    sozlesme.SozlesmeDovizTuru = viewModel.SozlesmeDovizTuru ?? "TL";
                    sozlesme.AktifMi = viewModel.AktifMi;
                    sozlesme.GuncellemeTarihi = DateTime.Now;

                    // Dosya yükleme işlemleri
                    if (viewModel.SozlesmeBelgesi != null)
                    {
                        // Eski dosyayı sil
                        if (!string.IsNullOrEmpty(sozlesme.SozlesmeDosyaYolu))
                        {
                            var eskiDosyaYolu = Path.Combine(_hostingEnvironment.WebRootPath, sozlesme.SozlesmeDosyaYolu.TrimStart('/'));
                            if (System.IO.File.Exists(eskiDosyaYolu))
                            {
                                System.IO.File.Delete(eskiDosyaYolu);
                            }
                        }

                        // Yeni dosyayı yükle
                        sozlesme.SozlesmeDosyaYolu = await DosyaYukle(viewModel.SozlesmeBelgesi, "sozlesmeler");
                    }

                    if (viewModel.Vekaletname != null)
                    {
                        // Eski dosyayı sil
                        if (!string.IsNullOrEmpty(sozlesme.VekaletnameDosyaYolu))
                        {
                            var eskiDosyaYolu = Path.Combine(_hostingEnvironment.WebRootPath, sozlesme.VekaletnameDosyaYolu.TrimStart('/'));
                            if (System.IO.File.Exists(eskiDosyaYolu))
                            {
                                System.IO.File.Delete(eskiDosyaYolu);
                            }
                        }

                        // Yeni dosyayı yükle
                        sozlesme.VekaletnameDosyaYolu = await DosyaYukle(viewModel.Vekaletname, "vekaletler");
                    }

                    var result = await _sozlesmeService.UpdateSozlesmeAsync(sozlesme);
                    if (result)
                    {
                        TempData["SuccessMessage"] = "Sözleşme başarıyla güncellendi.";
                        return RedirectToAction(nameof(Index));
                    }
                    else
                    {
                        TempData["ErrorMessage"] = "Sözleşme güncellenirken bir hata oluştu.";
                    }
                }
                
                ViewBag.Cariler = await _dropdownService.GetCariSelectList(viewModel.CariID != null ? Guid.Parse(viewModel.CariID) : (Guid?)null);
                return View(viewModel);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Sözleşme güncellenirken hata oluştu. Sözleşme ID: {Id}", id);
                TempData["ErrorMessage"] = "Sözleşme güncellenirken bir hata oluştu.";
                
                ViewBag.Cariler = await _dropdownService.GetCariSelectList(viewModel.CariID != null ? Guid.Parse(viewModel.CariID) : (Guid?)null);
                return View(viewModel);
            }
        }

        // GET: Sozlesme/Delete/5
        [HttpGet]
        public async Task<IActionResult> SoftDelete(Guid id)
        {
            try
            {
                var model = await _sozlesmeService.GetSozlesmeDetailAsync(id);
                if (model == null)
                {
                    _logger.LogWarning("Sözleşme bulunamadı: {Id}", id);
                    TempData["Error"] = "Sözleşme bulunamadı.";
                    return RedirectToAction(nameof(Index));
                }

                return View(model);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Sözleşme silme sayfası yüklenirken hata oluştu. ID: {Id}", id);
                ModelState.AddModelError("", "Sözleşme silme sayfası yüklenirken bir hata oluştu: " + ex.Message);
                return RedirectToAction(nameof(Index));
            }
        }

        // POST: Sozlesme/Delete/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SoftDeleteConfirmed(Guid id)
        {
            try
            {
                var result = await _sozlesmeService.DeleteSozlesmeAsync(id);
                if (result)
                {
                    _logger.LogInformation("Sözleşme başarıyla silindi. ID: {Id}", id);
                    TempData["Success"] = "Sözleşme başarıyla silindi.";
                }
                else
                {
                    _logger.LogWarning("Sözleşme silinemedi. ID: {Id}", id);
                    TempData["Error"] = "Sözleşme silinemedi. İlişkili kayıtlar olabilir.";
                }
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Sözleşme silinirken hata oluştu. ID: {Id}", id);
                ModelState.AddModelError("", "Sözleşme silinirken bir hata oluştu: " + ex.Message);
                return RedirectToAction(nameof(Index));
            }
        }

        // AklamaRaporu metodu şimdilik kaldırıldı (FaturaAklamaService bağımlılığı nedeniyle)
        /*
        public async Task<IActionResult> AklamaRaporu(Guid id)
        {
            try
            {
                var rapor = await _faturaAklamaService.GetAklamaRaporuBySozlesmeIdAsync(id);
                if (rapor == null)
                {
                    return NotFound();
                }
                return View(rapor);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Sözleşme aklama raporu sayfası yüklenirken hata oluştu. Sözleşme ID: {Id}", id);
                TempData["ErrorMessage"] = "Sözleşme aklama raporu sayfası yüklenirken bir hata oluştu.";
                return RedirectToAction(nameof(Index));
            }
        }
        */

        // Dosya Yükleme Yardımcı Metodu
        private async Task<string> DosyaYukle(IFormFile file, string klasorAdi)
        {
            if (file == null || file.Length == 0)
                return null;

            var uploads = Path.Combine(_hostingEnvironment.WebRootPath, "uploads", klasorAdi);
            if (!Directory.Exists(uploads))
            {
                Directory.CreateDirectory(uploads);
            }

            var uniqueFileName = $"{Guid.NewGuid()}_{Path.GetFileName(file.FileName)}";
            var filePath = Path.Combine(uploads, uniqueFileName);

            using (var fileStream = new FileStream(filePath, FileMode.Create))
            {
                await file.CopyToAsync(fileStream);
            }

            // Web tarafından erişilebilir yol döndür
            return $"/uploads/{klasorAdi}/{uniqueFileName}";
        }

        [HttpGet]
        public async Task<JsonResult> GetSozlesmelerByCariId(Guid cariId)
        {
            try
            {
                var sozlesmeler = await _sozlesmeService.GetSozlesmeListViewModelsAsync(cariId);
                return Json(new { success = true, data = sozlesmeler });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Cari ID'ye göre sözleşmeler getirilirken hata oluştu. Cari ID: {CariId}", cariId);
                return Json(new { success = false, message = "Sözleşmeler getirilirken bir hata oluştu: " + ex.Message });
            }
        }

        [HttpGet]
        public async Task<JsonResult> GetSozlesmeDetay(Guid sozlesmeId)
        {
            try
            {
                var sozlesme = await _sozlesmeService.GetSozlesmeDetailAsync(sozlesmeId);
                if (sozlesme == null)
                {
                    return Json(new { success = false, message = "Sözleşme bulunamadı." });
                }
                return Json(new { success = true, data = sozlesme });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Sözleşme detayları getirilirken hata oluştu. Sözleşme ID: {SozlesmeId}", sozlesmeId);
                return Json(new { success = false, message = "Sözleşme detayları getirilirken bir hata oluştu: " + ex.Message });
            }
        }
    }
} 