using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using MuhasebeStokWebApp.Data;
using MuhasebeStokWebApp.Data.Entities;
using MuhasebeStokWebApp.Services;
using MuhasebeStokWebApp.ViewModels.DovizKuru;
using MuhasebeStokWebApp.ViewModels.ParaBirimi;

namespace MuhasebeStokWebApp.Controllers
{
    // [Authorize] - Geçici olarak kaldırıldı
    public class ParaBirimiController : Controller
    {
        private readonly IParaBirimiService _paraBirimiService;
        private readonly ILogService _logService;

        public ParaBirimiController(IParaBirimiService paraBirimiService, ILogService logService)
        {
            _paraBirimiService = paraBirimiService;
            _logService = logService;
        }

        // GET: ParaBirimi
        public async Task<IActionResult> Index()
        {
            var paraBirimleri = await _paraBirimiService.GetAllParaBirimleriAsync();
            return View(paraBirimleri);
        }

        // GET: ParaBirimi/Details/5
        public async Task<IActionResult> Details(Guid id)
        {
            var paraBirimi = await _paraBirimiService.GetParaBirimiByIdAsync(id);
            if (paraBirimi == null || paraBirimi.Silindi)
            {
                return NotFound();
            }

            var viewModel = new ParaBirimiViewModel
            {
                ParaBirimiID = paraBirimi.ParaBirimiID,
                Kod = paraBirimi.Kod,
                Ad = paraBirimi.Ad,
                Sembol = paraBirimi.Sembol,
                Aktif = paraBirimi.Aktif,
                OlusturmaTarihi = paraBirimi.OlusturmaTarihi,
                GuncellemeTarihi = paraBirimi.GuncellemeTarihi
            };

            return View(viewModel);
        }

        // GET: ParaBirimi/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: ParaBirimi/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(ParaBirimi paraBirimi)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    await _paraBirimiService.AddParaBirimiAsync(paraBirimi);
                    TempData["SuccessMessage"] = "Para birimi başarıyla oluşturuldu.";
                    return RedirectToAction(nameof(Index));
                }
                catch (Exception ex)
                {
                    TempData["ErrorMessage"] = ex.Message;
                    ModelState.AddModelError("", ex.Message);
                }
            }
            return View(paraBirimi);
        }

        // GET: ParaBirimi/Edit/5
        public async Task<IActionResult> Edit(Guid? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var paraBirimi = await _paraBirimiService.GetParaBirimiByIdAsync(id.Value);
            if (paraBirimi == null)
            {
                return NotFound();
            }
            return View(paraBirimi);
        }

        // POST: ParaBirimi/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(Guid id, ParaBirimi paraBirimi)
        {
            if (id != paraBirimi.ParaBirimiID)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    await _paraBirimiService.UpdateParaBirimiAsync(paraBirimi);
                    TempData["SuccessMessage"] = "Para birimi başarıyla güncellendi.";
                    return RedirectToAction(nameof(Index));
                }
                catch (Exception ex)
                {
                    TempData["ErrorMessage"] = ex.Message;
                    ModelState.AddModelError("", ex.Message);
                }
            }
            return View(paraBirimi);
        }

        // GET: ParaBirimi/Delete/5
        public async Task<IActionResult> Delete(Guid? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var paraBirimi = await _paraBirimiService.GetParaBirimiByIdAsync(id.Value);
            if (paraBirimi == null)
            {
                return NotFound();
            }

            return View(paraBirimi);
        }

        // POST: ParaBirimi/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(Guid id)
        {
            try
            {
                await _paraBirimiService.DeleteParaBirimiAsync(id);
                TempData["SuccessMessage"] = "Para birimi başarıyla silindi.";
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = ex.Message;
            }

            return RedirectToAction(nameof(Index));
        }

        // GET: ParaBirimi/VarsayilanEkle
        public IActionResult VarsayilanEkle()
        {
            return View();
        }

        // POST: ParaBirimi/VarsayilanEkle
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> VarsayilanEkle(bool confirm)
        {
            if (confirm)
            {
                try
                {
                    bool eklendi = await _paraBirimiService.VarsayilanParaBirimleriniEkleAsync();
                    if (eklendi)
                    {
                        TempData["SuccessMessage"] = "Varsayılan para birimleri başarıyla eklendi.";
                    }
                    else
                    {
                        TempData["InfoMessage"] = "Varsayılan para birimleri zaten mevcut.";
                    }
                }
                catch (Exception ex)
                {
                    TempData["ErrorMessage"] = ex.Message;
                }
            }

            return RedirectToAction(nameof(Index));
        }

        // GET: ParaBirimi/ParaBirimiIliskileri
        public async Task<IActionResult> ParaBirimiIliskileri()
        {
            var paraBirimleri = await _paraBirimiService.GetAktifParaBirimleriAsync();
            return View(paraBirimleri);
        }

        // GET: ParaBirimi/ParaBirimiIliskileri/5
        public async Task<IActionResult> ParaBirimiIliskileriById(Guid? id)
        {
            if (id == null)
            {
                return BadRequest();
            }

            var paraBirimi = await _paraBirimiService.GetParaBirimiByIdAsync(id.Value);
            if (paraBirimi == null)
            {
                return NotFound();
            }

            var iliskiler = await _paraBirimiService.GetParaBirimiIliskileriAsync(id.Value);

            var viewModel = new ParaBirimiIliskileriViewModel
            {
                ParaBirimi = paraBirimi,
                Iliskiler = iliskiler
            };

            return View("ParaBirimiIliskileriById", viewModel);
        }

        // GET: ParaBirimi/IliskiEkle
        public async Task<IActionResult> IliskiEkle(Guid? id)
        {
            if (id == null)
            {
                return BadRequest();
            }

            var kaynakParaBirimi = await _paraBirimiService.GetParaBirimiByIdAsync(id.Value);
            if (kaynakParaBirimi == null)
            {
                return NotFound();
            }

            // Tüm para birimlerini al (kendisi hariç ve aktif olanlar)
            var tumParaBirimleri = await _paraBirimiService.GetAktifParaBirimleriAsync();
            var digerParaBirimleri = tumParaBirimleri
                .Where(p => p.ParaBirimiID != id)
                .Select(p => new SelectListItem { Value = p.ParaBirimiID.ToString(), Text = $"{p.Ad} ({p.Kod})" })
                .ToList();

            var viewModel = new ParaBirimiIliskiViewModel
            {
                KaynakParaBirimiID = kaynakParaBirimi.ParaBirimiID,
                KaynakParaBirimiKodu = kaynakParaBirimi.Kod,
                KaynakParaBirimiAdi = kaynakParaBirimi.Ad,
                HedefParaBirimleri = digerParaBirimleri,
                Carpan = 1.0m
            };

            return View(viewModel);
        }

        // POST: ParaBirimi/IliskiEkle
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> IliskiEkle(ParaBirimiIliskiViewModel viewModel)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    var iliski = new ParaBirimiIliski
                    {
                        KaynakParaBirimiID = viewModel.KaynakParaBirimiID,
                        HedefParaBirimiID = viewModel.HedefParaBirimiID,
                        Carpan = viewModel.Carpan,
                        Aktif = true
                    };

                    await _paraBirimiService.AddParaBirimiIliskiAsync(iliski);
                    TempData["SuccessMessage"] = "Para birimi ilişkisi başarıyla eklendi.";
                    return RedirectToAction(nameof(ParaBirimiIliskileriById), new { id = viewModel.KaynakParaBirimiID });
                }
                catch (Exception ex)
                {
                    TempData["ErrorMessage"] = ex.Message;
                    ModelState.AddModelError("", ex.Message);
                }
            }

            // Hata durumunda HedefParaBirimleri yeniden doldurmamız gerekiyor
            var kaynakParaBirimi = await _paraBirimiService.GetParaBirimiByIdAsync(viewModel.KaynakParaBirimiID);
            var tumParaBirimleri = await _paraBirimiService.GetAktifParaBirimleriAsync();
            var digerParaBirimleri = tumParaBirimleri
                .Where(p => p.ParaBirimiID != viewModel.KaynakParaBirimiID)
                .Select(p => new SelectListItem { Value = p.ParaBirimiID.ToString(), Text = $"{p.Ad} ({p.Kod})" })
                .ToList();

            viewModel.KaynakParaBirimiKodu = kaynakParaBirimi?.Kod;
            viewModel.KaynakParaBirimiAdi = kaynakParaBirimi?.Ad;
            viewModel.HedefParaBirimleri = digerParaBirimleri;

            return View(viewModel);
        }

        // GET: ParaBirimi/IliskiDuzenle/5
        public async Task<IActionResult> IliskiDuzenle(Guid? id)
        {
            if (id == null)
            {
                return BadRequest();
            }

            var iliski = await _paraBirimiService.GetParaBirimiIliskiByIdAsync(id.Value);
            if (iliski == null)
            {
                return NotFound();
            }

            var viewModel = new ParaBirimiIliskiViewModel
            {
                IliskiID = iliski.ParaBirimiIliskiID,
                KaynakParaBirimiID = iliski.KaynakParaBirimiID,
                KaynakParaBirimiKodu = iliski.KaynakParaBirimi?.Kod,
                KaynakParaBirimiAdi = iliski.KaynakParaBirimi?.Ad,
                HedefParaBirimiID = iliski.HedefParaBirimiID,
                HedefParaBirimiKodu = iliski.HedefParaBirimi?.Kod,
                HedefParaBirimiAdi = iliski.HedefParaBirimi?.Ad,
                Carpan = iliski.Carpan,
                Aktif = iliski.Aktif
            };

            return View(viewModel);
        }

        // POST: ParaBirimi/IliskiDuzenle/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> IliskiDuzenle(Guid id, ParaBirimiIliskiViewModel viewModel)
        {
            if (id != viewModel.IliskiID)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    var iliski = await _paraBirimiService.GetParaBirimiIliskiByIdAsync(id);
                    if (iliski == null)
                    {
                        return NotFound();
                    }

                    iliski.Carpan = viewModel.Carpan;
                    iliski.Aktif = viewModel.Aktif;

                    await _paraBirimiService.UpdateParaBirimiIliskiAsync(iliski);
                    TempData["SuccessMessage"] = "Para birimi ilişkisi başarıyla güncellendi.";
                    return RedirectToAction(nameof(ParaBirimiIliskileriById), new { id = iliski.KaynakParaBirimiID });
                }
                catch (Exception ex)
                {
                    TempData["ErrorMessage"] = ex.Message;
                    ModelState.AddModelError("", ex.Message);
                }
            }

            // Eğer buraya geldiyse, model geçerli değildi, view'i tekrar göster
            return View(viewModel);
        }

        // POST: ParaBirimi/IliskiSil/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> IliskiSil(Guid id, Guid paraBirimiId)
        {
            try
            {
                await _paraBirimiService.DeleteParaBirimiIliskiAsync(id);
                TempData["SuccessMessage"] = "Para birimi ilişkisi başarıyla silindi.";
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = ex.Message;
            }

            return RedirectToAction(nameof(ParaBirimiIliskileriById), new { id = paraBirimiId });
        }

        // POST: ParaBirimi/UpdateSiralama
        [HttpPost]
        public async Task<IActionResult> UpdateSiralama([FromBody] List<Guid> paraBirimiSiralama)
        {
            if (paraBirimiSiralama == null || !paraBirimiSiralama.Any())
            {
                return BadRequest(new { success = false, message = "Geçersiz sıralama listesi." });
            }

            try
            {
                await _paraBirimiService.UpdateParaBirimiSiralamaAsync(paraBirimiSiralama);
                return Json(new { success = true, message = "Para birimi sıralaması güncellendi." });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        // GET: ParaBirimi/IliskiDetay
        public async Task<IActionResult> IliskiDetay(Guid kaynakId, Guid hedefId)
        {
            if (kaynakId == Guid.Empty || hedefId == Guid.Empty)
            {
                return BadRequest("Kaynak ve hedef para birimi ID'leri geçerli değil.");
            }
            
            if (kaynakId == hedefId)
            {
                return BadRequest("Kaynak ve hedef para birimi aynı olamaz.");
            }
            
            try 
            {
                var kaynakParaBirimi = await _paraBirimiService.GetParaBirimiByIdAsync(kaynakId);
                var hedefParaBirimi = await _paraBirimiService.GetParaBirimiByIdAsync(hedefId);
                
                if (kaynakParaBirimi == null || hedefParaBirimi == null)
                {
                    return NotFound("Para birimi bulunamadı.");
                }
                
                // Doğrudan ilişkiyi ara
                var iliskiDogrudan = await _paraBirimiService.GetIliskiByParaBirimleriAsync(kaynakId, hedefId);
                
                // Ters ilişkiyi ara (hedef -> kaynak)
                var iliskiTers = await _paraBirimiService.GetIliskiByParaBirimleriAsync(hedefId, kaynakId);
                
                var viewModel = new ParaBirimiIliskiDetayViewModel
                {
                    KaynakParaBirimi = kaynakParaBirimi,
                    HedefParaBirimi = hedefParaBirimi,
                    DogruIliski = iliskiDogrudan,
                    TersIliski = iliskiTers
                };
                
                return PartialView("_IliskiDetay", viewModel);
            }
            catch (Exception ex)
            {
                _logService.AddLogAsync("Para Birimi", "İlişki Detay Hatası", 
                    $"Kaynak ID: {kaynakId}, Hedef ID: {hedefId}, Hata: {ex.Message}", "").Wait();
                
                return PartialView("Error", "Para birimi ilişki detayları alınırken bir hata oluştu: " + ex.Message);
            }
        }
    }
} 