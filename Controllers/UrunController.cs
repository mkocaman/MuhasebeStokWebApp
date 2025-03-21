using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MuhasebeStokWebApp.Data;
using MuhasebeStokWebApp.Data.Entities;
using MuhasebeStokWebApp.Data.Repositories;
using MuhasebeStokWebApp.ViewModels.Urun;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace MuhasebeStokWebApp.Controllers
{
    // [Authorize] - Geçici olarak kaldırıldı
    public class UrunController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ApplicationDbContext _context;

        public UrunController(IUnitOfWork unitOfWork, ApplicationDbContext context)
        {
            _unitOfWork = unitOfWork;
            _context = context;
        }

        // GET: Urun
        public async Task<IActionResult> Index()
        {
            var urunRepository = _unitOfWork.Repository<Urun>();
            var urunFiyatRepository = _unitOfWork.Repository<UrunFiyat>();
            var kategoriRepository = _unitOfWork.Repository<UrunKategori>();

            var urunler = await urunRepository.GetAllAsync();
            urunler = urunler.Where(u => u.SoftDelete == false).ToList();

            var kategoriler = await kategoriRepository.GetAllAsync();
            kategoriler = kategoriler.Where(k => k.SoftDelete == false).ToList();

            var viewModel = new UrunListViewModel
            {
                Urunler = urunler.Select(u => new UrunViewModel
                {
                    UrunID = u.UrunID,
                    UrunKodu = u.UrunKodu,
                    UrunAdi = u.UrunAdi,
                    Birim = u.Birim != null ? u.Birim.BirimAdi : string.Empty,
                    StokMiktar = u.StokMiktar,
                    ListeFiyati = urunFiyatRepository.GetAllAsync().Result
                        .Where(uf => uf.UrunID == u.UrunID && uf.FiyatTipiID == 1 && uf.SoftDelete == false)
                        .OrderByDescending(uf => uf.GecerliTarih)
                        .FirstOrDefault()?.Fiyat ?? 0m,
                    MaliyetFiyati = urunFiyatRepository.GetAllAsync().Result
                        .Where(uf => uf.UrunID == u.UrunID && uf.FiyatTipiID == 2 && uf.SoftDelete == false)
                        .OrderByDescending(uf => uf.GecerliTarih)
                        .FirstOrDefault()?.Fiyat ?? 0m,
                    SatisFiyati = urunFiyatRepository.GetAllAsync().Result
                        .Where(uf => uf.UrunID == u.UrunID && uf.FiyatTipiID == 3 && uf.SoftDelete == false)
                        .OrderByDescending(uf => uf.GecerliTarih)
                        .FirstOrDefault()?.Fiyat ?? 0m,
                    Aktif = u.Aktif,
                    OlusturmaTarihi = u.OlusturmaTarihi,
                    KategoriID = u.KategoriID,
                    KategoriAdi = u.KategoriID.HasValue ? 
                        kategoriler.FirstOrDefault(k => k.KategoriID.ToString() == u.KategoriID.ToString())?.KategoriAdi : "Kategorisiz"
                }).ToList()
            };

            return View(viewModel);
        }

        // GET: Urun/Details/5
        public async Task<IActionResult> Details(Guid id)
        {
            var urunRepository = _unitOfWork.Repository<Urun>();
            var urunFiyatRepository = _unitOfWork.Repository<UrunFiyat>();
            var kategoriRepository = _unitOfWork.Repository<UrunKategori>();

            var urun = await urunRepository.GetFirstOrDefaultAsync(u => u.UrunID == id && u.SoftDelete == false);
            if (urun == null)
            {
                return NotFound();
            }

            var kategori = urun.KategoriID.HasValue ? 
                await kategoriRepository.GetFirstOrDefaultAsync(k => k.KategoriID.ToString() == urun.KategoriID.ToString()) : null;

            var viewModel = new UrunViewModel
            {
                UrunID = urun.UrunID,
                UrunKodu = urun.UrunKodu,
                UrunAdi = urun.UrunAdi,
                Birim = urun.Birim != null ? urun.Birim.BirimAdi : string.Empty,
                StokMiktar = urun.StokMiktar,
                ListeFiyati = urunFiyatRepository.GetAllAsync().Result
                    .Where(uf => uf.UrunID == urun.UrunID && uf.FiyatTipiID == 1 && uf.SoftDelete == false)
                    .OrderByDescending(uf => uf.GecerliTarih)
                    .FirstOrDefault()?.Fiyat ?? 0m,
                MaliyetFiyati = urunFiyatRepository.GetAllAsync().Result
                    .Where(uf => uf.UrunID == urun.UrunID && uf.FiyatTipiID == 2 && uf.SoftDelete == false)
                    .OrderByDescending(uf => uf.GecerliTarih)
                    .FirstOrDefault()?.Fiyat ?? 0m,
                SatisFiyati = urunFiyatRepository.GetAllAsync().Result
                    .Where(uf => uf.UrunID == urun.UrunID && uf.FiyatTipiID == 3 && uf.SoftDelete == false)
                    .OrderByDescending(uf => uf.GecerliTarih)
                    .FirstOrDefault()?.Fiyat ?? 0m,
                Aktif = urun.Aktif,
                OlusturmaTarihi = urun.OlusturmaTarihi,
                KategoriID = urun.KategoriID,
                KategoriAdi = kategori?.KategoriAdi ?? "Kategorisiz"
            };

            return View(viewModel);
        }

        // GET: Urun/Create
        public async Task<IActionResult> Create()
        {
            var kategoriRepository = _unitOfWork.Repository<UrunKategori>();
            var kategoriler = await kategoriRepository.GetAsync(
                filter: k => k.SoftDelete == false && k.Aktif,
                orderBy: q => q.OrderBy(k => k.KategoriAdi)
            );
            
            var birimRepository = _unitOfWork.Repository<Birim>();
            var birimler = await birimRepository.GetAsync(
                filter: b => b.SoftDelete == false && b.Aktif,
                orderBy: q => q.OrderBy(b => b.BirimAdi)
            );
            
            ViewBag.Kategoriler = new SelectList(kategoriler, "KategoriID", "KategoriAdi");
            ViewBag.Birimler = new SelectList(birimler, "BirimID", "BirimAdi");
            
            if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
            {
                return PartialView("_CreatePartial", new UrunCreateViewModel());
            }
            
            return View();
        }

        // POST: Urun/Create
        [HttpPost]
        public async Task<IActionResult> Create(UrunCreateViewModel model)
        {
            try
            {
                // Debug bilgisi
                Console.WriteLine($"Model: UrunKodu={model.UrunKodu}, UrunAdi={model.UrunAdi}, BirimID={model.BirimID}, Aktif={model.Aktif}");
                
                if (!model.BirimID.HasValue)
                {
                    ModelState.AddModelError("BirimID", "Birim seçimi zorunludur.");
                }
                
                // BirimListesi ve KategoriListesi alanlarının validasyonunu kaldır
                ModelState.Remove("BirimListesi");
                ModelState.Remove("KategoriListesi");
                ModelState.Remove("Birim");
                
                // Validasyon hatalarını Türkçeleştir
                if (!ModelState.IsValid)
                {
                    foreach (var key in ModelState.Keys.ToList())
                    {
                        var errors = ModelState[key].Errors.ToList();
                        for (int i = 0; i < errors.Count; i++)
                        {
                            var error = errors[i];
                            if (error.ErrorMessage.Contains("field is required"))
                            {
                                ModelState.Remove(key);
                                ModelState.AddModelError(key, $"{key} alanı zorunludur.");
                            }
                        }
                    }
                }
                
                if (ModelState.IsValid)
                {
                    var urunRepository = _unitOfWork.Repository<Urun>();
            
                    var urun = new Urun
                    {
                        UrunID = Guid.NewGuid(),
                        UrunKodu = model.UrunKodu,
                        UrunAdi = model.UrunAdi,
                        BirimID = model.BirimID,
                        Aktif = model.Aktif,
                        KategoriID = model.KategoriID,
                        OlusturmaTarihi = DateTime.Now,
                        SoftDelete = false,
                        StokMiktar = 0,
                        KDVOrani = model.KDVOrani
                    };

                    await urunRepository.AddAsync(urun);
                    await _unitOfWork.SaveAsync();
            
                    TempData["SuccessMessage"] = $"{model.UrunAdi} ürünü başarıyla eklendi. Stok girişini 'Stok Yönetimi > Stok Girişi' menüsünden yapabilirsiniz.";

                    // AJAX isteği için başarılı sonuç döndür
                    if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
                    {
                        return Json(new { success = true, message = $"{model.UrunAdi} ürünü başarıyla eklendi." });
                    }

                    return RedirectToAction(nameof(Index));
                }
                
                // ModelState geçersizse
                var kategoriRepository = _unitOfWork.Repository<UrunKategori>();
                var kategoriler = await kategoriRepository.GetAsync(
                    filter: k => k.SoftDelete == false && k.Aktif,
                    orderBy: q => q.OrderBy(k => k.KategoriAdi)
                );
                
                var birimRepository = _unitOfWork.Repository<Birim>();
                var birimler = await birimRepository.GetAsync(
                    filter: b => b.SoftDelete == false && b.Aktif,
                    orderBy: q => q.OrderBy(b => b.BirimAdi)
                );
                
                ViewBag.Kategoriler = new SelectList(kategoriler, "KategoriID", "KategoriAdi", model.KategoriID);
                ViewBag.Birimler = new SelectList(birimler, "BirimID", "BirimAdi", model.BirimID);
                
                // AJAX isteği için hata mesajları döndür
                if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
                {
                    return Json(new 
                    { 
                        success = false, 
                        message = "Ürün eklenirken hatalar oluştu.", 
                        errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList() 
                    });
                }
                
                return View(model);
            }
            catch (Exception ex)
            {
                // Hata log
                Console.WriteLine($"Ürün ekleme hatası: {ex.Message}");
                TempData["ErrorMessage"] = $"Ürün eklenirken bir hata oluştu: {ex.Message}";
                
                // AJAX isteği için hata mesajı döndür
                if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
                {
                    return Json(new { success = false, message = $"Ürün eklenirken bir hata oluştu: {ex.Message}" });
                }
                
                return RedirectToAction(nameof(Index));
            }
        }

        // GET: Urun/Edit/5
        [HttpGet]
        public async Task<IActionResult> Edit(Guid id)
        {
            var urunRepository = _unitOfWork.Repository<Urun>();
            var kategoriRepository = _unitOfWork.Repository<UrunKategori>();

            var urun = await urunRepository.GetFirstOrDefaultAsync(u => u.UrunID == id && u.SoftDelete == false);
            if (urun == null)
            {
                return NotFound();
            }

            var viewModel = new UrunEditViewModel
            {
                UrunID = urun.UrunID,
                UrunKodu = urun.UrunKodu,
                UrunAdi = urun.UrunAdi,
                BirimID = urun.BirimID,
                StokMiktar = urun.StokMiktar,
                Aktif = urun.Aktif,
                KategoriID = urun.KategoriID
            };

            var kategoriler = await kategoriRepository.GetAsync(
                filter: k => k.SoftDelete == false && k.Aktif,
                orderBy: q => q.OrderBy(k => k.KategoriAdi)
            );
            
            var birimRepository = _unitOfWork.Repository<Birim>();
            var birimler = await birimRepository.GetAsync(
                filter: b => b.SoftDelete == false && b.Aktif,
                orderBy: q => q.OrderBy(b => b.BirimAdi)
            );
            
            ViewBag.Kategoriler = new SelectList(kategoriler, "KategoriID", "KategoriAdi", viewModel.KategoriID);
            ViewBag.Birimler = new SelectList(birimler, "BirimID", "BirimAdi", viewModel.BirimID);
            
            if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
            {
                return PartialView("_EditPartial", viewModel);
            }
            
            return View(viewModel);
        }

        // POST: Urun/Edit/5
        [HttpPost]
        public async Task<IActionResult> Edit(Guid id, UrunEditViewModel model)
        {
            if (id != model.UrunID)
            {
                return NotFound();
            }

            // BirimListesi ve KategoriListesi alanlarının validasyonunu kaldır
            ModelState.Remove("BirimListesi");
            ModelState.Remove("KategoriListesi");
            ModelState.Remove("Birim");
            
            // Validasyon hatalarını Türkçeleştir
            if (!ModelState.IsValid)
            {
                foreach (var key in ModelState.Keys.ToList())
                {
                    var errors = ModelState[key].Errors.ToList();
                    for (int i = 0; i < errors.Count; i++)
                    {
                        var error = errors[i];
                        if (error.ErrorMessage.Contains("field is required"))
                        {
                            ModelState.Remove(key);
                            ModelState.AddModelError(key, $"{key} alanı zorunludur.");
                        }
                    }
                }
            }

            if (ModelState.IsValid)
            {
                var urunRepository = _unitOfWork.Repository<Urun>();

                var urun = await urunRepository.GetFirstOrDefaultAsync(u => u.UrunID == id && u.SoftDelete == false);
                if (urun == null)
                {
                    return NotFound();
                }

                urun.UrunKodu = model.UrunKodu;
                urun.UrunAdi = model.UrunAdi;
                urun.BirimID = model.BirimID;
                urun.StokMiktar = model.StokMiktar;
                urun.Aktif = model.Aktif;
                urun.KategoriID = model.KategoriID;
                urun.KDVOrani = model.KDVOrani;
                urun.GuncellemeTarihi = DateTime.Now;

                await urunRepository.UpdateAsync(urun);
                await _unitOfWork.SaveAsync();

                TempData["SuccessMessage"] = $"{model.UrunAdi} ürünü başarıyla güncellendi.";

                // AJAX isteği için başarılı sonuç döndür
                if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
                {
                    return Json(new { success = true });
                }

                return RedirectToAction(nameof(Index));
            }
            
            var kategoriRepository = _unitOfWork.Repository<UrunKategori>();
            var kategoriler = await kategoriRepository.GetAsync(
                filter: k => k.SoftDelete == false && k.Aktif,
                orderBy: q => q.OrderBy(k => k.KategoriAdi)
            );
            
            var birimRepository = _unitOfWork.Repository<Birim>();
            var birimler = await birimRepository.GetAsync(
                filter: b => b.SoftDelete == false && b.Aktif,
                orderBy: q => q.OrderBy(b => b.BirimAdi)
            );
            
            ViewBag.Kategoriler = new SelectList(kategoriler, "KategoriID", "KategoriAdi", model.KategoriID);
            ViewBag.Birimler = new SelectList(birimler, "BirimID", "BirimAdi", model.BirimID);
            
            // AJAX isteği için başarısız sonuç döndür
            if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
            {
                return PartialView("_EditPartial", model);
            }
            
            return View(model);
        }

        // GET: Urun/Delete/5
        public async Task<IActionResult> Delete(Guid id)
        {
            var urunRepository = _unitOfWork.Repository<Urun>();
            var urunFiyatRepository = _unitOfWork.Repository<UrunFiyat>();
            var kategoriRepository = _unitOfWork.Repository<UrunKategori>();

            var urun = await urunRepository.GetFirstOrDefaultAsync(u => u.UrunID == id && u.SoftDelete == false);
            if (urun == null)
            {
                return NotFound();
            }

            var kategori = urun.KategoriID.HasValue ? 
                await kategoriRepository.GetFirstOrDefaultAsync(k => k.KategoriID.ToString() == urun.KategoriID.ToString()) : null;

            var viewModel = new UrunViewModel
            {
                UrunID = urun.UrunID,
                UrunKodu = urun.UrunKodu,
                UrunAdi = urun.UrunAdi,
                Birim = urun.Birim != null ? urun.Birim.BirimAdi : string.Empty,
                StokMiktar = urun.StokMiktar,
                ListeFiyati = urunFiyatRepository.GetAllAsync().Result
                    .Where(uf => uf.UrunID == urun.UrunID && uf.FiyatTipiID == 1 && uf.SoftDelete == false)
                    .OrderByDescending(uf => uf.GecerliTarih)
                    .FirstOrDefault()?.Fiyat ?? 0m,
                MaliyetFiyati = urunFiyatRepository.GetAllAsync().Result
                    .Where(uf => uf.UrunID == urun.UrunID && uf.FiyatTipiID == 2 && uf.SoftDelete == false)
                    .OrderByDescending(uf => uf.GecerliTarih)
                    .FirstOrDefault()?.Fiyat ?? 0m,
                SatisFiyati = urunFiyatRepository.GetAllAsync().Result
                    .Where(uf => uf.UrunID == urun.UrunID && uf.FiyatTipiID == 3 && uf.SoftDelete == false)
                    .OrderByDescending(uf => uf.GecerliTarih)
                    .FirstOrDefault()?.Fiyat ?? 0m,
                Aktif = urun.Aktif,
                OlusturmaTarihi = urun.OlusturmaTarihi,
                KategoriID = urun.KategoriID,
                KategoriAdi = kategori?.KategoriAdi ?? "Kategorisiz"
            };

            return View(viewModel);
        }

        // POST: Urun/Delete/5
        [HttpPost, ActionName("Delete")]
        public async Task<IActionResult> DeleteConfirmed(Guid id)
        {
            var urunRepository = _unitOfWork.Repository<Urun>();
            var stokHareketRepository = _unitOfWork.Repository<StokHareket>();
            var faturaDetayRepository = _unitOfWork.Repository<FaturaDetay>();
            var urunFiyatRepository = _unitOfWork.Repository<UrunFiyat>();

            var urun = await urunRepository.GetFirstOrDefaultAsync(u => u.UrunID == id && u.SoftDelete == false);
            if (urun == null)
            {
                return NotFound();
            }

            try
            {
                // Daha kapsamlı kontrol mekanizması - Doğrudan veritabanından sorgu ile kontrol edelim
                var faturaDetayVarMi = await _context.FaturaDetaylari
                    .AnyAsync(fd => fd.UrunID == id && fd.SoftDelete == false);
                
                Console.WriteLine($"Ürün ID: {id}, Fatura Detayı Var Mı: {faturaDetayVarMi}");
                
                // Eğer ürün herhangi bir faturada kullanılmışsa, sadece pasife al
                if (faturaDetayVarMi)
                {
                    urun.Aktif = false;
                    urun.GuncellemeTarihi = DateTime.Now;
                    
                    await urunRepository.UpdateAsync(urun);
                    await _unitOfWork.SaveAsync();
                    
                    TempData["Warning"] = $"'{urun.UrunAdi}' ürünü faturalarda kullanıldığı için silinemedi, sadece pasife alındı.";
                    return RedirectToAction(nameof(Index));
                }
                
                // İkinci bir kontrol daha yapalım - Repository kullanarak
                var faturaDetaylari = await faturaDetayRepository.GetAllAsync();
                var urunKullanilmis = faturaDetaylari.Any(fd => fd.UrunID == id && !fd.SoftDelete);
                
                Console.WriteLine($"Repository Kontrolü - Ürün Kullanılmış: {urunKullanilmis}");
                
                if (urunKullanilmis)
                {
                    urun.Aktif = false;
                    urun.GuncellemeTarihi = DateTime.Now;
                    
                    await urunRepository.UpdateAsync(urun);
                    await _unitOfWork.SaveAsync();
                    
                    TempData["Warning"] = $"'{urun.UrunAdi}' ürünü faturalarda kullanıldığı için silinemedi, sadece pasife alındı.";
                    return RedirectToAction(nameof(Index));
                }
                
                // Stok hareketlerini soft delete yap
                var stokHareketleri = await stokHareketRepository.GetAsync(h => h.UrunID == id && h.SoftDelete == false);
                foreach (var hareket in stokHareketleri)
                {
                    hareket.SoftDelete = true;
                    hareket.GuncellemeTarihi = DateTime.Now;
                    await stokHareketRepository.UpdateAsync(hareket);
                }
                
                // Ürün fiyatlarını soft delete yap
                var urunFiyatlari = await urunFiyatRepository.GetAsync(uf => uf.UrunID == id && uf.SoftDelete == false);
                foreach (var fiyat in urunFiyatlari)
                {
                    fiyat.SoftDelete = true;
                    await urunFiyatRepository.UpdateAsync(fiyat);
                }

                // Ürünü soft delete yap
                urun.SoftDelete = true;
                urun.Aktif = false;
                urun.GuncellemeTarihi = DateTime.Now;
                
                await urunRepository.UpdateAsync(urun);
                await _unitOfWork.SaveAsync();
                
                TempData["SuccessMessage"] = $"'{urun.UrunAdi}' ürünü ve ilişkili tüm kayıtları başarıyla silindi.";
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ürün silme hatası: {ex.Message}");
                Console.WriteLine($"Hata detayı: {ex.StackTrace}");
                TempData["ErrorMessage"] = $"Ürün silinirken bir hata oluştu: {ex.Message}";
            }

            return RedirectToAction(nameof(Index));
        }

        // GET: Urun/Activate/5
        public async Task<IActionResult> Activate(Guid id)
        {
            var urunRepository = _unitOfWork.Repository<Urun>();
            
            var urun = await urunRepository.GetFirstOrDefaultAsync(u => u.UrunID == id && u.SoftDelete == false);
            if (urun == null)
            {
                return NotFound();
            }
            
            // Ürünü aktif yap
            urun.Aktif = true;
            urun.GuncellemeTarihi = DateTime.Now;
            
            await urunRepository.UpdateAsync(urun);
            await _unitOfWork.SaveAsync();
            
            TempData["SuccessMessage"] = $"{urun.UrunAdi} ürünü başarıyla aktifleştirildi.";
            
            return RedirectToAction(nameof(Index));
        }

        // AJAX: Urun/GetUrunDetails/5
        [HttpGet]
        public async Task<IActionResult> GetUrunDetails(Guid id)
        {
            var urunRepository = _unitOfWork.Repository<Urun>();
            var urunFiyatRepository = _unitOfWork.Repository<UrunFiyat>();

            var urun = await urunRepository.GetFirstOrDefaultAsync(u => u.UrunID == id && u.SoftDelete == false);
            if (urun == null)
            {
                return NotFound();
            }

            var fiyatlar = await urunFiyatRepository.GetAsync(
                filter: f => f.UrunID == urun.UrunID && f.SoftDelete == false,
                includeProperties: "FiyatTipi"
            );

            var listeFiyati = fiyatlar.Where(f => f.FiyatTipi.FiyatTipiID == 1)
                .OrderByDescending(f => f.GecerliTarih)
                .FirstOrDefault()?.Fiyat;

            var maliyetFiyati = fiyatlar.Where(f => f.FiyatTipi.FiyatTipiID == 2)
                .OrderByDescending(f => f.GecerliTarih)
                .FirstOrDefault()?.Fiyat;

            var satisFiyati = fiyatlar.Where(f => f.FiyatTipi.FiyatTipiID == 3)
                .OrderByDescending(f => f.GecerliTarih)
                .FirstOrDefault()?.Fiyat;

            var model = new UrunViewModel
            {
                UrunID = urun.UrunID,
                UrunKodu = urun.UrunKodu,
                UrunAdi = urun.UrunAdi,
                Birim = urun.Birim != null ? urun.Birim.BirimAdi : string.Empty,
                StokMiktar = urun.StokMiktar,
                Aktif = urun.Aktif,
                ListeFiyati = listeFiyati ?? 0m,
                MaliyetFiyati = maliyetFiyati ?? 0m,
                SatisFiyati = satisFiyati ?? 0m,
                OlusturmaTarihi = urun.OlusturmaTarihi,
                GuncellemeTarihi = urun.GuncellemeTarihi
            };

            return Json(model);
        }

        // AJAX: Ürünleri JSON formatında getir
        [HttpGet]
        public async Task<IActionResult> GetUrunlerJson()
        {
            var urunler = await _unitOfWork.Repository<Urun>().GetAsync(
                filter: u => u.Aktif && !u.SoftDelete);

            var result = urunler.Select(u => new
            {
                urunID = u.UrunID,
                urunKodu = u.UrunKodu,
                urunAdi = u.UrunAdi,
                birim = u.Birim != null ? u.Birim.BirimAdi : string.Empty,
                stokMiktar = u.StokMiktar
            });

            return Json(result);
        }
        
        // AJAX: Ürün bilgilerini getir
        [HttpGet]
        public async Task<IActionResult> GetUrunBilgileri(Guid id)
        {
            var urun = await _unitOfWork.Repository<Urun>().GetFirstOrDefaultAsync(
                filter: u => u.UrunID == id && u.Aktif && !u.SoftDelete,
                includeProperties: "Birim");

            if (urun == null)
            {
                return NotFound();
            }

            return Json(new
            {
                urunID = urun.UrunID,
                urunKodu = urun.UrunKodu,
                urunAdi = urun.UrunAdi,
                birim = urun.Birim != null ? urun.Birim.BirimAdi : "Adet",
                stokMiktar = urun.StokMiktar
            });
        }
    }
} 