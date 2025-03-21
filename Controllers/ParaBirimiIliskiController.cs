using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using MuhasebeStokWebApp.Data;
using MuhasebeStokWebApp.Data.Entities;
using MuhasebeStokWebApp.ViewModels.ParaBirimi;
using MuhasebeStokWebApp.Services;
using MuhasebeStokWebApp.Enums;

namespace MuhasebeStokWebApp.Controllers
{
    public class ParaBirimiIliskiController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogService _logService;

        public ParaBirimiIliskiController(ApplicationDbContext context, ILogService logService)
        {
            _context = context;
            _logService = logService;
        }

        // GET: ParaBirimiIliski/Index
        public async Task<IActionResult> Index()
        {
            // Para birimi ilişkilerini ve para birimlerini getir
            var paraBirimleri = await _context.ParaBirimleri
                .Where(p => !p.Silindi)
                .OrderBy(p => p.Kod)
                .ToListAsync();

            ViewBag.ParaBirimleri = paraBirimleri;

            var viewModel = new ParaBirimiIliskiViewModel
            {
                KaynakParaBirimiID = Guid.Empty,
                HedefParaBirimiID = Guid.Empty,
                CevrimKatsayisi = 1.0m
            };

            return View(viewModel);
        }

        // GET: ParaBirimiIliski/GetParaBirimiIliskileri
        [HttpGet]
        public async Task<IActionResult> GetParaBirimiIliskileri()
        {
            try
            {
                var paraBirimiIliskileri = await _context.ParaBirimiIliskileri
                    .Where(p => !p.Silindi)
                    .Include(p => p.KaynakParaBirimi)
                    .Include(p => p.HedefParaBirimi)
                    .OrderBy(p => p.KaynakParaBirimi.Kod)
                    .ThenBy(p => p.HedefParaBirimi.Kod)
                    .Select(p => new
                    {
                        p.ParaBirimiIliskiID,
                        p.KaynakParaBirimiID,
                        KaynakParaBirimiKodu = p.KaynakParaBirimi.Kod,
                        KaynakParaBirimiAdi = p.KaynakParaBirimi.Ad,
                        p.HedefParaBirimiID,
                        HedefParaBirimiKodu = p.HedefParaBirimi.Kod,
                        HedefParaBirimiAdi = p.HedefParaBirimi.Ad,
                        Carpan = p.Carpan,
                        p.Aktif,
                        p.OlusturmaTarihi,
                        p.GuncellemeTarihi
                    })
                    .ToListAsync();

                return Json(paraBirimiIliskileri);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { hata = ex.Message });
            }
        }

        // GET: ParaBirimiIliski/GetAllParaBirimiIliskileri
        [HttpGet]
        public async Task<IActionResult> GetAllParaBirimiIliskileri()
        {
            try
            {
                var paraBirimiIliskileri = await _context.ParaBirimiIliskileri
                    .Where(p => !p.Silindi)
                    .Include(p => p.KaynakParaBirimi)
                    .Include(p => p.HedefParaBirimi)
                    .OrderBy(p => p.KaynakParaBirimi.Kod)
                    .ThenBy(p => p.HedefParaBirimi.Kod)
                    .Select(p => new
                    {
                        paraBirimiIliskiID = p.ParaBirimiIliskiID,
                        kaynakParaBirimiID = p.KaynakParaBirimiID,
                        kaynakParaBirimiKodu = p.KaynakParaBirimi.Kod,
                        kaynakParaBirimiAdi = p.KaynakParaBirimi.Ad,
                        hedefParaBirimiID = p.HedefParaBirimiID,
                        hedefParaBirimiKodu = p.HedefParaBirimi.Kod,
                        hedefParaBirimiAdi = p.HedefParaBirimi.Ad,
                        cevrimKatsayisi = p.Carpan,
                        aktif = p.Aktif,
                        olusturmaTarihi = p.OlusturmaTarihi,
                        guncellemeTarihi = p.GuncellemeTarihi
                    })
                    .ToListAsync();

                return Json(new { data = paraBirimiIliskileri });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { hata = ex.Message });
            }
        }

        // POST: ParaBirimiIliski/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(ParaBirimiIliskiViewModel viewModel)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    if (viewModel.KaynakParaBirimiID == viewModel.HedefParaBirimiID)
                    {
                        TempData["ErrorMessage"] = "Kaynak ve hedef para birimi aynı olamaz.";
                        return RedirectToAction(nameof(Index));
                    }

                    // Para birimlerini kontrol et
                    var kaynakParaBirimi = await _context.ParaBirimleri.FindAsync(viewModel.KaynakParaBirimiID);
                    var hedefParaBirimi = await _context.ParaBirimleri.FindAsync(viewModel.HedefParaBirimiID);

                    if (kaynakParaBirimi == null || hedefParaBirimi == null)
                    {
                        TempData["ErrorMessage"] = "Belirtilen para birimleri bulunamadı.";
                        return RedirectToAction(nameof(Index));
                    }

                    // İlişkiyi oluştur ve kaydet
                    var paraBirimiIliski = new ParaBirimiIliski
                    {
                        ParaBirimiIliskiID = Guid.NewGuid(),
                        KaynakParaBirimiID = viewModel.KaynakParaBirimiID,
                        HedefParaBirimiID = viewModel.HedefParaBirimiID,
                        Carpan = viewModel.CevrimKatsayisi,
                        OlusturmaTarihi = DateTime.Now,
                        GuncellemeTarihi = DateTime.Now,
                        Aktif = true
                    };

                    _context.ParaBirimiIliskileri.Add(paraBirimiIliski);
                    await _context.SaveChangesAsync();

                    await _logService.AddLogAsync("ParaBirimiIliski", "Ekleme", 
                        $"Para Birimi İlişkisi eklendi: {kaynakParaBirimi.Kod} -> {hedefParaBirimi.Kod}, Katsayı: {viewModel.CevrimKatsayisi}", 
                        paraBirimiIliski.ParaBirimiIliskiID.ToString());

                    TempData["SuccessMessage"] = "Para birimi ilişkisi başarıyla eklendi.";
                    return RedirectToAction(nameof(Index));
                }
                catch (Exception ex)
                {
                    TempData["ErrorMessage"] = $"Hata: {ex.Message}";
                }
            }

            // Validasyon hatası durumunda para birimlerini tekrar yükle
            var paraBirimleri = await _context.ParaBirimleri
                .Where(p => !p.Silindi)
                .OrderBy(p => p.Kod)
                .ToListAsync();

            ViewBag.ParaBirimleri = paraBirimleri;
            return View("Index", viewModel);
        }

        // GET: ParaBirimiIliski/Details/5
        public async Task<IActionResult> Details(Guid id)
        {
            if (id == Guid.Empty)
            {
                return NotFound();
            }

            var paraBirimiIliski = await _context.ParaBirimiIliskileri
                .Include(p => p.KaynakParaBirimi)
                .Include(p => p.HedefParaBirimi)
                .FirstOrDefaultAsync(m => m.ParaBirimiIliskiID == id);

            if (paraBirimiIliski == null)
            {
                return NotFound();
            }

            return View(paraBirimiIliski);
        }

        // GET: ParaBirimiIliski/Edit/5
        public async Task<IActionResult> Edit(Guid id)
        {
            if (id == Guid.Empty)
            {
                return NotFound();
            }

            var paraBirimiIliski = await _context.ParaBirimiIliskileri
                .Include(p => p.KaynakParaBirimi)
                .Include(p => p.HedefParaBirimi)
                .FirstOrDefaultAsync(m => m.ParaBirimiIliskiID == id);

            if (paraBirimiIliski == null)
            {
                return NotFound();
            }

            // Para birimlerini getir
            var paraBirimleri = await _context.ParaBirimleri
                .Where(p => !p.Silindi)
                .OrderBy(p => p.Kod)
                .ToListAsync();

            ViewBag.KaynakParaBirimleri = new SelectList(paraBirimleri, "ParaBirimiID", "Kod", paraBirimiIliski.KaynakParaBirimiID);
            ViewBag.HedefParaBirimleri = new SelectList(paraBirimleri, "ParaBirimiID", "Kod", paraBirimiIliski.HedefParaBirimiID);

            var viewModel = new ParaBirimiIliskiViewModel
            {
                ParaBirimiIliskiID = paraBirimiIliski.ParaBirimiIliskiID,
                KaynakParaBirimiID = paraBirimiIliski.KaynakParaBirimiID,
                HedefParaBirimiID = paraBirimiIliski.HedefParaBirimiID,
                CevrimKatsayisi = paraBirimiIliski.Carpan,
                Aktif = paraBirimiIliski.Aktif
            };

            return View(viewModel);
        }

        // POST: ParaBirimiIliski/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(Guid id, ParaBirimiIliskiViewModel viewModel)
        {
            if (id != viewModel.ParaBirimiIliskiID)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    if (viewModel.KaynakParaBirimiID == viewModel.HedefParaBirimiID)
                    {
                        TempData["ErrorMessage"] = "Kaynak ve hedef para birimi aynı olamaz.";
                        
                        var paraBirimleri = await _context.ParaBirimleri
                            .Where(p => !p.Silindi)
                            .OrderBy(p => p.Kod)
                            .ToListAsync();

                        ViewBag.KaynakParaBirimleri = new SelectList(paraBirimleri, "ParaBirimiID", "Kod", viewModel.KaynakParaBirimiID);
                        ViewBag.HedefParaBirimleri = new SelectList(paraBirimleri, "ParaBirimiID", "Kod", viewModel.HedefParaBirimiID);
                        
                        return View(viewModel);
                    }

                    var paraBirimiIliski = await _context.ParaBirimiIliskileri.FindAsync(viewModel.ParaBirimiIliskiID);
            
                    if (paraBirimiIliski == null)
                    {
                        return NotFound();
                    }

                    // Orijinal değerleri sakla (log için)
                    var eskiKaynakParaBirimiID = paraBirimiIliski.KaynakParaBirimiID;
                    var eskiHedefParaBirimiID = paraBirimiIliski.HedefParaBirimiID;
                    var eskiCarpan = paraBirimiIliski.Carpan;
                    var eskiAktif = paraBirimiIliski.Aktif;

                    // Değerleri güncelle
                    paraBirimiIliski.KaynakParaBirimiID = viewModel.KaynakParaBirimiID;
                    paraBirimiIliski.HedefParaBirimiID = viewModel.HedefParaBirimiID;
                    paraBirimiIliski.Carpan = viewModel.CevrimKatsayisi;
                    paraBirimiIliski.Aktif = viewModel.Aktif;
                    paraBirimiIliski.GuncellemeTarihi = DateTime.Now;

                    _context.Update(paraBirimiIliski);
                    await _context.SaveChangesAsync();

                    // Para birimi bilgilerini getir (log için)
                    var kaynakParaBirimi = await _context.ParaBirimleri.FindAsync(viewModel.KaynakParaBirimiID);
                    var hedefParaBirimi = await _context.ParaBirimleri.FindAsync(viewModel.HedefParaBirimiID);

                    await _logService.AddLogAsync("ParaBirimiIliski", "Güncelleme", 
                        $"Para Birimi İlişkisi güncellendi: {kaynakParaBirimi.Kod} -> {hedefParaBirimi.Kod}, Katsayı: {viewModel.CevrimKatsayisi}", 
                        paraBirimiIliski.ParaBirimiIliskiID.ToString());

                    TempData["SuccessMessage"] = "Para birimi ilişkisi başarıyla güncellendi.";
                    return RedirectToAction(nameof(Index));
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!await ParaBirimiIliskiExists(viewModel.ParaBirimiIliskiID))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                catch (Exception ex)
                {
                    TempData["ErrorMessage"] = $"Hata: {ex.Message}";
                }
            }

            // Validasyon hatası durumunda para birimlerini tekrar yükle
            var paraBirimleriList = await _context.ParaBirimleri
                .Where(p => !p.Silindi)
                .OrderBy(p => p.Kod)
                .ToListAsync();

            ViewBag.KaynakParaBirimleri = new SelectList(paraBirimleriList, "ParaBirimiID", "Kod", viewModel.KaynakParaBirimiID);
            ViewBag.HedefParaBirimleri = new SelectList(paraBirimleriList, "ParaBirimiID", "Kod", viewModel.HedefParaBirimiID);
            
            return View(viewModel);
        }

        private async Task<bool> ParaBirimiIliskiExists(Guid id)
        {
            return await _context.ParaBirimiIliskileri.AnyAsync(e => e.ParaBirimiIliskiID == id);
        }

        // GET: ParaBirimiIliski/Delete/5
        public async Task<IActionResult> Delete(Guid id)
        {
            if (id == Guid.Empty)
            {
                return NotFound();
            }

            var paraBirimiIliski = await _context.ParaBirimiIliskileri
                .Include(p => p.KaynakParaBirimi)
                .Include(p => p.HedefParaBirimi)
                .FirstOrDefaultAsync(m => m.ParaBirimiIliskiID == id);

            if (paraBirimiIliski == null)
            {
                return NotFound();
            }

            return View(paraBirimiIliski);
        }

        // POST: ParaBirimiIliski/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(Guid id)
        {
            var paraBirimiIliski = await _context.ParaBirimiIliskileri.FindAsync(id);
            
            if (paraBirimiIliski != null)
            {
                // Fiziksel silme yerine mantıksal silme işlemi yap
                paraBirimiIliski.Silindi = true;
                paraBirimiIliski.GuncellemeTarihi = DateTime.Now;
                
                _context.ParaBirimiIliskileri.Update(paraBirimiIliski);
                await _context.SaveChangesAsync();

                await _logService.AddLogAsync("ParaBirimiIliski", "Silme", 
                    $"Para Birimi İlişkisi silindi: ID {id}", 
                    id.ToString());
            }

            TempData["SuccessMessage"] = "Para birimi ilişkisi başarıyla silindi.";
            return RedirectToAction(nameof(Index));
        }
    }
} 