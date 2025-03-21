using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using MuhasebeStokWebApp.Data;
using MuhasebeStokWebApp.Data.Entities;
using MuhasebeStokWebApp.Data.Repositories;
using MuhasebeStokWebApp.Services;
using MuhasebeStokWebApp.ViewModels.UrunFiyat;

namespace MuhasebeStokWebApp.Controllers
{
    public class UrunFiyatController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly UserManager _userManager;

        public UrunFiyatController(IUnitOfWork unitOfWork, UserManager userManager)
        {
            _unitOfWork = unitOfWork;
            _userManager = userManager;
        }

        // GET: UrunFiyat/UrunFiyatlar
        public async Task<IActionResult> Index(Guid? urunId)
        {
            var viewModel = new UrunFiyatListViewModel();
            
            var urunFiyatRepository = _unitOfWork.Repository<UrunFiyat>();
            var urunRepository = _unitOfWork.Repository<Urun>();
            var fiyatTipiRepository = _unitOfWork.Repository<FiyatTipi>();
            
            if (urunId.HasValue)
            {
                // Belirli bir ürün için fiyatlar listeleniyor
                var urun = await urunRepository.GetFirstOrDefaultAsync(u => u.UrunID == urunId && u.SoftDelete == false);
                if (urun == null)
                {
                    return NotFound();
                }
                
                viewModel.SelectedUrunID = urunId;
                viewModel.UrunAdi = urun.UrunAdi;
                
                // Ürüne ait tüm fiyatları getir
                var fiyatlar = await urunFiyatRepository.GetAsync(
                    filter: f => f.UrunID == urunId && f.SoftDelete == false,
                    includeProperties: "Urun,FiyatTipi",
                    orderBy: q => q.OrderByDescending(f => f.GecerliTarih)
                );
                
                viewModel.UrunFiyatlari = fiyatlar.Select(f => new UrunFiyatViewModel
                {
                    FiyatID = f.FiyatID,
                    UrunID = f.UrunID,
                    UrunKodu = f.Urun.UrunKodu,
                    UrunAdi = f.Urun.UrunAdi,
                    Fiyat = f.Fiyat,
                    GecerliTarih = f.GecerliTarih,
                    FiyatTipiID = f.FiyatTipiID,
                    FiyatTipiAdi = f.FiyatTipi?.TipAdi,
                    OlusturmaTarihi = f.OlusturmaTarihi,
                    Aktif = !f.SoftDelete
                }).ToList();
            }
            else
            {
                // Tüm fiyatlar listeleniyor
                var fiyatlar = await urunFiyatRepository.GetAsync(
                    filter: f => f.SoftDelete == false,
                    includeProperties: "Urun,FiyatTipi",
                    orderBy: q => q.OrderByDescending(f => f.GecerliTarih)
                );
                
                viewModel.UrunFiyatlari = fiyatlar.Select(f => new UrunFiyatViewModel
                {
                    FiyatID = f.FiyatID,
                    UrunID = f.UrunID,
                    UrunKodu = f.Urun?.UrunKodu,
                    UrunAdi = f.Urun?.UrunAdi,
                    Fiyat = f.Fiyat,
                    GecerliTarih = f.GecerliTarih,
                    FiyatTipiID = f.FiyatTipiID,
                    FiyatTipiAdi = f.FiyatTipi?.TipAdi,
                    OlusturmaTarihi = f.OlusturmaTarihi,
                    Aktif = !f.SoftDelete
                }).ToList();
            }
            
            return View(viewModel);
        }

        // GET: UrunFiyat/Create/[UrunID]
        public async Task<IActionResult> Create(Guid? urunId)
        {
            var urunRepository = _unitOfWork.Repository<Urun>();
            var fiyatTipiRepository = _unitOfWork.Repository<FiyatTipi>();
            
            var model = new UrunFiyatCreateViewModel
            {
                UrunID = urunId,
                GecerliTarih = DateTime.Now
            };
            
            // Ürün ve fiyat tipi seçim listeleri
            await LoadSelectLists(urunId);
            
            if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
            {
                return PartialView("_CreatePartial", model);
            }
            
            return View(model);
        }
        
        [HttpPost]
        public async Task<IActionResult> Create(UrunFiyatCreateViewModel model)
        {
            if (ModelState.IsValid)
            {
                var kullaniciID = _userManager.GetCurrentUserId();
                var urunFiyatRepository = _unitOfWork.Repository<UrunFiyat>();
                
                var yeniFiyat = new UrunFiyat
                {
                    UrunID = model.UrunID,
                    Fiyat = model.Fiyat,
                    GecerliTarih = model.GecerliTarih,
                    FiyatTipiID = model.FiyatTipiID,
                    OlusturanKullaniciID = kullaniciID,
                    OlusturmaTarihi = DateTime.Now,
                    SoftDelete = false
                };
                
                await urunFiyatRepository.AddAsync(yeniFiyat);
                await _unitOfWork.SaveAsync();
                
                TempData["SuccessMessage"] = "Ürün fiyatı başarıyla eklendi.";
                
                // AJAX isteği için başarılı sonuç döndür
                if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
                {
                    return Json(new { success = true, message = "Ürün fiyatı başarıyla eklendi." });
                }
                
                // Ürün ID varsa ürünün fiyat listesine yönlendir
                if (model.UrunID.HasValue)
                {
                    return RedirectToAction("Index", new { urunId = model.UrunID });
                }
                
                return RedirectToAction("Index");
            }
            
            // ModelState geçersizse
            await LoadSelectLists(model.UrunID);
            
            // AJAX isteği için hata mesajları döndür
            if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
            {
                return PartialView("_CreatePartial", model);
            }
            
            return View(model);
        }
        
        // Düzenleme için GET metodu
        public async Task<IActionResult> Edit(int id)
        {
            var urunFiyatRepository = _unitOfWork.Repository<UrunFiyat>();
            
            var fiyat = await urunFiyatRepository.GetFirstOrDefaultAsync(
                f => f.FiyatID == id && f.SoftDelete == false,
                includeProperties: "Urun,FiyatTipi"
            );
            
            if (fiyat == null)
            {
                return NotFound();
            }
            
            var viewModel = new UrunFiyatViewModel
            {
                FiyatID = fiyat.FiyatID,
                UrunID = fiyat.UrunID,
                UrunKodu = fiyat.Urun?.UrunKodu,
                UrunAdi = fiyat.Urun?.UrunAdi,
                Fiyat = fiyat.Fiyat,
                GecerliTarih = fiyat.GecerliTarih,
                FiyatTipiID = fiyat.FiyatTipiID,
                FiyatTipiAdi = fiyat.FiyatTipi?.TipAdi,
                OlusturmaTarihi = fiyat.OlusturmaTarihi,
                Aktif = !fiyat.SoftDelete
            };
            
            await LoadSelectLists(fiyat.UrunID);
            
            if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
            {
                return PartialView("_EditPartial", viewModel);
            }
            
            return View(viewModel);
        }
        
        [HttpPost]
        public async Task<IActionResult> Edit(int id, UrunFiyatViewModel model)
        {
            if (id != model.FiyatID)
            {
                return NotFound();
            }
            
            if (ModelState.IsValid)
            {
                var kullaniciID = _userManager.GetCurrentUserId();
                var urunFiyatRepository = _unitOfWork.Repository<UrunFiyat>();
                
                var fiyat = await urunFiyatRepository.GetFirstOrDefaultAsync(
                    f => f.FiyatID == id && f.SoftDelete == false
                );
                
                if (fiyat == null)
                {
                    return NotFound();
                }
                
                fiyat.Fiyat = model.Fiyat;
                fiyat.GecerliTarih = model.GecerliTarih;
                fiyat.FiyatTipiID = model.FiyatTipiID;
                fiyat.SonGuncelleyenKullaniciID = kullaniciID;
                fiyat.GuncellemeTarihi = DateTime.Now;
                fiyat.SoftDelete = !model.Aktif;
                
                await urunFiyatRepository.UpdateAsync(fiyat);
                await _unitOfWork.SaveAsync();
                
                TempData["SuccessMessage"] = "Ürün fiyatı başarıyla güncellendi.";
                
                // AJAX isteği için başarılı sonuç döndür
                if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
                {
                    return Json(new { success = true, message = "Ürün fiyatı başarıyla güncellendi." });
                }
                
                // Ürün ID varsa ürünün fiyat listesine yönlendir
                if (model.UrunID.HasValue)
                {
                    return RedirectToAction("Index", new { urunId = model.UrunID });
                }
                
                return RedirectToAction("Index");
            }
            
            // ModelState geçersizse
            await LoadSelectLists(model.UrunID);
            
            // AJAX isteği için hata mesajları döndür
            if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
            {
                return PartialView("_EditPartial", model);
            }
            
            return View(model);
        }
        
        // Silme işlemi GET (Onay sayfası)
        public async Task<IActionResult> Delete(int id)
        {
            var urunFiyatRepository = _unitOfWork.Repository<UrunFiyat>();
            
            var fiyat = await urunFiyatRepository.GetFirstOrDefaultAsync(
                f => f.FiyatID == id && f.SoftDelete == false,
                includeProperties: "Urun,FiyatTipi"
            );
            
            if (fiyat == null)
            {
                return NotFound();
            }
            
            var viewModel = new UrunFiyatViewModel
            {
                FiyatID = fiyat.FiyatID,
                UrunID = fiyat.UrunID,
                UrunKodu = fiyat.Urun?.UrunKodu,
                UrunAdi = fiyat.Urun?.UrunAdi,
                Fiyat = fiyat.Fiyat,
                GecerliTarih = fiyat.GecerliTarih,
                FiyatTipiID = fiyat.FiyatTipiID,
                FiyatTipiAdi = fiyat.FiyatTipi?.TipAdi,
                OlusturmaTarihi = fiyat.OlusturmaTarihi,
                Aktif = !fiyat.SoftDelete
            };
            
            return View(viewModel);
        }
        
        // Silme işlemi POST
        [HttpPost, ActionName("Delete")]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var urunFiyatRepository = _unitOfWork.Repository<UrunFiyat>();
            
            var fiyat = await urunFiyatRepository.GetFirstOrDefaultAsync(
                f => f.FiyatID == id && f.SoftDelete == false
            );
            
            if (fiyat == null)
            {
                return NotFound();
            }
            
            // Soft delete
            fiyat.SoftDelete = true;
            fiyat.GuncellemeTarihi = DateTime.Now;
            fiyat.SonGuncelleyenKullaniciID = _userManager.GetCurrentUserId();
            
            await urunFiyatRepository.UpdateAsync(fiyat);
            await _unitOfWork.SaveAsync();
            
            TempData["SuccessMessage"] = "Ürün fiyatı başarıyla silindi.";
            
            // Ürün ID varsa ürünün fiyat listesine yönlendir
            if (fiyat.UrunID.HasValue)
            {
                return RedirectToAction("Index", new { urunId = fiyat.UrunID });
            }
            
            return RedirectToAction("Index");
        }
        
        // Drop-down listeleri doldur
        private async Task LoadSelectLists(Guid? urunId = null)
        {
            var urunRepository = _unitOfWork.Repository<Urun>();
            var fiyatTipiRepository = _unitOfWork.Repository<FiyatTipi>();
            
            // Ürün listesi
            var urunler = await urunRepository.GetAsync(
                filter: u => u.SoftDelete == false && u.Aktif,
                orderBy: q => q.OrderBy(u => u.UrunKodu)
            );
            
            ViewBag.Urunler = new SelectList(
                urunler.Select(u => new { u.UrunID, UrunBilgisi = $"{u.UrunKodu} - {u.UrunAdi}" }),
                "UrunID", 
                "UrunBilgisi", 
                urunId
            );
            
            // Fiyat tipi listesi
            var fiyatTipleri = await fiyatTipiRepository.GetAllAsync();
            ViewBag.FiyatTipleri = new SelectList(fiyatTipleri, "FiyatTipiID", "TipAdi");
        }
    }
} 