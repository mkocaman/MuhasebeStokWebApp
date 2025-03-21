using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using MuhasebeStokWebApp.Data;
using MuhasebeStokWebApp.Data.Entities;
using MuhasebeStokWebApp.Services;
using MuhasebeStokWebApp.ViewModels.DovizKuru;

namespace MuhasebeStokWebApp.Controllers
{
    public class DovizKuruController : Controller
    {
        private readonly IDovizKuruService _dovizKuruService;
        private readonly IParaBirimiService _paraBirimiService;
        private readonly ILogService _logService;
        private readonly ApplicationDbContext _context;

        public DovizKuruController(
            IDovizKuruService dovizKuruService,
            IParaBirimiService paraBirimiService,
            ILogService logService,
            ApplicationDbContext context)
        {
            _dovizKuruService = dovizKuruService;
            _paraBirimiService = paraBirimiService;
            _logService = logService;
            _context = context;
        }

        // GET: DovizKuru
        public async Task<IActionResult> Index()
        {
            var kurDegerleri = await _dovizKuruService.GetGuncelKurlarAsync();
            return View(kurDegerleri);
        }

        // GET: DovizKuru/ParaBirimiKurlari/5
        public async Task<IActionResult> ParaBirimiKurlari(Guid id)
        {
            var paraBirimi = await _paraBirimiService.GetParaBirimiByIdAsync(id);
            if (paraBirimi == null)
            {
                return NotFound();
            }

            var kurDegerleri = await _dovizKuruService.GetParaBirimiKurDegerleriAsync(id);
            
            var viewModel = new ParaBirimiKurlariViewModel
            {
                ParaBirimiID = paraBirimi.ParaBirimiID,
                ParaBirimiKodu = paraBirimi.Kod,
                ParaBirimiAdi = paraBirimi.Ad,
                KurDegerleri = kurDegerleri.Select(k => new DovizKuruViewModel
                {
                    KurDegeriID = k.KurDegeriID,
                    ParaBirimiID = k.ParaBirimiID,
                    ParaBirimiKodu = paraBirimi.Kod,
                    ParaBirimiAdi = paraBirimi.Ad,
                    AlisDegeri = k.AlisDegeri,
                    SatisDegeri = k.SatisDegeri,
                    Tarih = k.Tarih,
                    Kaynak = k.Kaynak,
                    Aktif = k.Aktif
                }).ToList()
            };

            return View(viewModel);
        }

        // GET: DovizKuru/Create
        public async Task<IActionResult> Create(Guid? paraBirimiId = null)
        {
            var paraBirimleri = await _paraBirimiService.GetAktifParaBirimleriAsync();
            
            var viewModel = new DovizKuruCreateViewModel
            {
                Tarih = DateTime.Now,
                Kaynak = "Manuel Giriş",
                ParaBirimleri = paraBirimleri.Select(p => new SelectListItem
                {
                    Value = p.ParaBirimiID.ToString(),
                    Text = $"{p.Ad} ({p.Kod})",
                    Selected = paraBirimiId.HasValue && p.ParaBirimiID == paraBirimiId.Value
                }).ToList()
            };

            return View(viewModel);
        }

        // POST: DovizKuru/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(DovizKuruCreateViewModel viewModel)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    // Kur değerini ekle
                    await _dovizKuruService.KurEkleAsync(
                        viewModel.ParaBirimiID,
                        viewModel.AlisDegeri,
                        viewModel.SatisDegeri,
                        viewModel.Kaynak,
                        viewModel.Tarih);

                    TempData["SuccessMessage"] = "Döviz kuru başarıyla eklendi.";
                    
                    if (viewModel.ParaBirimiID != Guid.Empty)
                    {
                        return RedirectToAction(nameof(ParaBirimiKurlari), new { id = viewModel.ParaBirimiID });
                    }
                    
                    return RedirectToAction(nameof(Index));
                }
                catch (Exception ex)
                {
                    TempData["ErrorMessage"] = ex.Message;
                    ModelState.AddModelError("", ex.Message);
                    await _logService.LogErrorAsync("Döviz Kuru Ekleme", ex);
                }
            }

            // Validation hatası durumunda ParaBirimleri listesini tekrar doldurmamız gerekiyor
            var paraBirimleri = await _paraBirimiService.GetAktifParaBirimleriAsync();
            viewModel.ParaBirimleri = paraBirimleri.Select(p => new SelectListItem
            {
                Value = p.ParaBirimiID.ToString(),
                Text = $"{p.Ad} ({p.Kod})",
                Selected = p.ParaBirimiID == viewModel.ParaBirimiID
            }).ToList();

            return View(viewModel);
        }

        // GET: DovizKuru/Edit/5
        public async Task<IActionResult> Edit(Guid id)
        {
            var kurDegeri = await _dovizKuruService.GetKurDegeriByIdAsync(id);
            if (kurDegeri == null)
            {
                return NotFound();
            }

            var paraBirimi = await _paraBirimiService.GetParaBirimiByIdAsync(kurDegeri.ParaBirimiID);

            var viewModel = new DovizKuruEditViewModel
            {
                KurDegeriID = kurDegeri.KurDegeriID,
                ParaBirimiID = kurDegeri.ParaBirimiID,
                ParaBirimiKodu = paraBirimi?.Kod ?? "",
                ParaBirimiAdi = paraBirimi?.Ad ?? "",
                AlisDegeri = kurDegeri.AlisDegeri,
                SatisDegeri = kurDegeri.SatisDegeri,
                Tarih = kurDegeri.Tarih,
                Kaynak = kurDegeri.Kaynak,
                Aktif = kurDegeri.Aktif
            };

            return View(viewModel);
        }

        // POST: DovizKuru/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(Guid id, DovizKuruEditViewModel viewModel)
        {
            if (id != viewModel.KurDegeriID)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    var kurDegeri = await _dovizKuruService.GetKurDegeriByIdAsync(id);
                    if (kurDegeri == null)
                    {
                        return NotFound();
                    }

                    // Değerleri güncelle
                    kurDegeri.AlisDegeri = viewModel.AlisDegeri;
                    kurDegeri.SatisDegeri = viewModel.SatisDegeri;
                    kurDegeri.Tarih = viewModel.Tarih;
                    kurDegeri.Kaynak = viewModel.Kaynak;
                    kurDegeri.Aktif = viewModel.Aktif;
                    kurDegeri.GuncellemeTarihi = DateTime.Now;

                    // Veritabanında güncelleme
                    await _context.SaveChangesAsync();

                    TempData["SuccessMessage"] = "Döviz kuru başarıyla güncellendi.";
                    return RedirectToAction(nameof(ParaBirimiKurlari), new { id = kurDegeri.ParaBirimiID });
                }
                catch (Exception ex)
                {
                    TempData["ErrorMessage"] = ex.Message;
                    ModelState.AddModelError("", ex.Message);
                    await _logService.LogErrorAsync("Döviz Kuru Güncelleme", ex);
                }
            }

            return View(viewModel);
        }

        // GET: DovizKuru/Delete/5
        public async Task<IActionResult> Delete(Guid id)
        {
            // KurDegeri ID ile ilgili kuru bul
            var kurDegeri = await _dovizKuruService.GetKurDegeriByIdAsync(id);
            
            if (kurDegeri == null)
            {
                return NotFound();
            }

            var paraBirimi = await _paraBirimiService.GetParaBirimiByIdAsync(kurDegeri.ParaBirimiID);

            var viewModel = new DovizKuruViewModel
            {
                KurDegeriID = kurDegeri.KurDegeriID,
                ParaBirimiID = kurDegeri.ParaBirimiID,
                ParaBirimiKodu = paraBirimi?.Kod ?? "",
                ParaBirimiAdi = paraBirimi?.Ad ?? "",
                AlisDegeri = kurDegeri.AlisDegeri,
                SatisDegeri = kurDegeri.SatisDegeri,
                Tarih = kurDegeri.Tarih,
                Kaynak = kurDegeri.Kaynak,
                Aktif = kurDegeri.Aktif
            };

            return View(viewModel);
        }

        // POST: DovizKuru/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(Guid id)
        {
            try
            {
                await _dovizKuruService.DeleteKurDegeriAsync(id);
                TempData["SuccessMessage"] = "Döviz kuru başarıyla silindi.";
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = ex.Message;
                await _logService.LogErrorAsync("Döviz Kuru Silme", ex);
            }

            return RedirectToAction(nameof(Index));
        }

        // GET: DovizKuru/Sync - API ile kurları güncellemek için redirect metodu
        public async Task<IActionResult> Sync()
        {
            try
            {
                var eklenenKurlar = await _dovizKuruService.KurlariGuncelleAsync();
                TempData["SuccessMessage"] = $"Kurlar başarıyla güncellendi. {eklenenKurlar.Count} yeni kur eklendi.";
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"Kur güncelleme işlemi sırasında hata oluştu: {ex.Message}";
                await _logService.LogErrorAsync("Kur Güncelleme", ex);
            }

            return RedirectToAction(nameof(Index));
        }
    }
} 