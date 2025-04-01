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
using Microsoft.AspNetCore.Identity;
using MuhasebeStokWebApp.Services;
using MuhasebeStokWebApp.Services.Interfaces;

namespace MuhasebeStokWebApp.Controllers
{
    // Ürün yönetimi işlemlerini yöneten controller sınıfı
    // [Authorize] - Geçici olarak kaldırıldı
    public class UrunController : BaseController
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ApplicationDbContext _context;

        // Constructor: Repository ve veritabanı bağlantısını DI ile alır
        public UrunController(
            IUnitOfWork unitOfWork, 
            ApplicationDbContext context,
            IMenuService menuService,
            UserManager<ApplicationUser> userManager,
            RoleManager<IdentityRole> roleManager,
            ILogService logService)
            : base(menuService, userManager, roleManager, logService)
        {
            _unitOfWork = unitOfWork;
            _context = context;
        }

        // Ürünlerin listelendiği ana sayfa
        public async Task<IActionResult> Index()
        {
            var urunRepository = _unitOfWork.Repository<Urun>();
            var urunFiyatRepository = _unitOfWork.Repository<UrunFiyat>();
            var kategoriRepository = _unitOfWork.Repository<UrunKategori>();

            // Silinmemiş tüm ürünleri getir
            var urunler = await urunRepository.GetAllAsync();
            urunler = urunler.Where(u => u.Silindi == false).ToList();

            // Silinmemiş tüm kategorileri getir
            var kategoriler = await kategoriRepository.GetAllAsync();
            kategoriler = kategoriler.Where(k => k.Silindi == false).ToList();

            // Ürün listesi görünüm modeli oluştur
            var viewModel = new UrunListViewModel
            {
                Urunler = urunler.Select(u => new UrunViewModel
                {
                    UrunID = u.UrunID,
                    UrunKodu = u.UrunKodu,
                    UrunAdi = u.UrunAdi,
                    Birim = u.Birim != null ? u.Birim.BirimAdi : string.Empty,
                    StokMiktar = u.StokMiktar,
                    // Her ürün için en güncel liste fiyatını getir
                    ListeFiyati = urunFiyatRepository.GetAllAsync().Result
                        .Where(uf => uf.UrunID == u.UrunID && uf.FiyatTipiID == 1 && uf.Silindi == false)
                        .OrderByDescending(uf => uf.GecerliTarih)
                        .FirstOrDefault()?.Fiyat ?? 0m,
                    // Her ürün için en güncel maliyet fiyatını getir
                    MaliyetFiyati = urunFiyatRepository.GetAllAsync().Result
                        .Where(uf => uf.UrunID == u.UrunID && uf.FiyatTipiID == 2 && uf.Silindi == false)
                        .OrderByDescending(uf => uf.GecerliTarih)
                        .FirstOrDefault()?.Fiyat ?? 0m,
                    // Her ürün için en güncel satış fiyatını getir
                    SatisFiyati = urunFiyatRepository.GetAllAsync().Result
                        .Where(uf => uf.UrunID == u.UrunID && uf.FiyatTipiID == 3 && uf.Silindi == false)
                        .OrderByDescending(uf => uf.GecerliTarih)
                        .FirstOrDefault()?.Fiyat ?? 0m,
                    Aktif = u.Aktif,
                    OlusturmaTarihi = u.OlusturmaTarihi,
                    KategoriID = u.KategoriID,
                    // Ürün kategorisinin adını bul
                    KategoriAdi = u.KategoriID.HasValue ? 
                        kategoriler.FirstOrDefault(k => k.KategoriID.ToString() == u.KategoriID.ToString())?.KategoriAdi : "Kategorisiz"
                }).ToList()
            };

            return View(viewModel);
        }

        // Ürün detaylarını gösterir
        public async Task<IActionResult> Details(Guid id)
        {
            var urunRepository = _unitOfWork.Repository<Urun>();
            var urunFiyatRepository = _unitOfWork.Repository<UrunFiyat>();
            var kategoriRepository = _unitOfWork.Repository<UrunKategori>();

            // Ürün bilgisini getir
            var urun = await urunRepository.GetFirstOrDefaultAsync(u => u.UrunID == id && u.Silindi == false);
            if (urun == null)
            {
                return NotFound();
            }

            // Kategori bilgisini getir
            var kategori = urun.KategoriID.HasValue ? 
                await kategoriRepository.GetFirstOrDefaultAsync(k => k.KategoriID.ToString() == urun.KategoriID.ToString()) : null;

            // Ürün detay görünüm modeli oluştur
            var viewModel = new UrunViewModel
            {
                UrunID = urun.UrunID,
                UrunKodu = urun.UrunKodu,
                UrunAdi = urun.UrunAdi,
                Birim = urun.Birim != null ? urun.Birim.BirimAdi : string.Empty,
                StokMiktar = urun.StokMiktar,
                // Ürün için en güncel liste fiyatını getir
                ListeFiyati = urunFiyatRepository.GetAllAsync().Result
                    .Where(uf => uf.UrunID == urun.UrunID && uf.FiyatTipiID == 1 && uf.Silindi == false)
                    .OrderByDescending(uf => uf.GecerliTarih)
                    .FirstOrDefault()?.Fiyat ?? 0m,
                // Ürün için en güncel maliyet fiyatını getir
                MaliyetFiyati = urunFiyatRepository.GetAllAsync().Result
                    .Where(uf => uf.UrunID == urun.UrunID && uf.FiyatTipiID == 2 && uf.Silindi == false)
                    .OrderByDescending(uf => uf.GecerliTarih)
                    .FirstOrDefault()?.Fiyat ?? 0m,
                // Ürün için en güncel satış fiyatını getir
                SatisFiyati = urunFiyatRepository.GetAllAsync().Result
                    .Where(uf => uf.UrunID == urun.UrunID && uf.FiyatTipiID == 3 && uf.Silindi == false)
                    .OrderByDescending(uf => uf.GecerliTarih)
                    .FirstOrDefault()?.Fiyat ?? 0m,
                Aktif = urun.Aktif,
                OlusturmaTarihi = urun.OlusturmaTarihi,
                KategoriID = urun.KategoriID,
                KategoriAdi = kategori?.KategoriAdi ?? "Kategorisiz"
            };

            return View(viewModel);
        }

        // Yeni ürün oluşturma formu
        public async Task<IActionResult> Create()
        {
            // Aktif kategorileri getir
            var kategoriRepository = _unitOfWork.Repository<UrunKategori>();
            var kategoriler = await kategoriRepository.GetAsync(
                filter: k => k.Silindi == false && k.Aktif,
                orderBy: q => q.OrderBy(k => k.KategoriAdi)
            );
            
            // Aktif birimleri getir
            var birimRepository = _unitOfWork.Repository<Birim>();
            var birimler = await birimRepository.GetAsync(
                filter: b => b.Silindi == false && b.Aktif,
                orderBy: q => q.OrderBy(b => b.BirimAdi)
            );
            
            // Dropdown listelerini hazırla
            ViewBag.Kategoriler = new SelectList(kategoriler, "KategoriID", "KategoriAdi");
            ViewBag.Birimler = new SelectList(birimler, "BirimID", "BirimAdi");
            
            var model = new UrunCreateViewModel
            {
                Aktif = true
            };
            
            // AJAX isteğiyse partial view döndür
            if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
            {
                return PartialView("_CreateUrun", model);
            }
            
            return View(model);
        }

        // Yeni ürün oluşturma işlemi
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(UrunCreateViewModel model)
        {
            try
            {
                // Birim seçimi zorunlu
                if (!model.BirimID.HasValue)
                {
                    ModelState.AddModelError("BirimID", "Birim seçimi zorunludur.");
                }
                
                // Gereksiz validasyon alanlarını kaldır
                ModelState.Remove("BirimListesi");
                ModelState.Remove("KategoriListesi");
                ModelState.Remove("Birim");
                
                // Validasyon hatalarını Türkçeleştir
                if (!ModelState.IsValid)
                {
                    TurkceHataEkle();
                }
                
                if (ModelState.IsValid)
                {
                    var urunRepository = _unitOfWork.Repository<Urun>();
            
                    // Yeni ürün nesnesi oluştur
                    var urun = new Urun
                    {
                        UrunID = Guid.NewGuid(),
                        UrunKodu = model.UrunKodu,
                        UrunAdi = model.UrunAdi,
                        BirimID = model.BirimID,
                        KategoriID = model.KategoriID,
                        KDVOrani = (int)model.KDVOrani,
                        StokMiktar = model.StokMiktar,
                        Aktif = model.Aktif,
                        OlusturmaTarihi = DateTime.Now,
                        OlusturanKullaniciID = GetCurrentUserId()
                    };
                    
                    // Ürünü veritabanına ekle
                    await urunRepository.AddAsync(urun);
                    await _unitOfWork.SaveAsync();
                    
                    // Başarı durumunu don
                    if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
                    {
                        return Json(new { success = true, message = "Ürün başarıyla oluşturuldu." });
                    }
                    
                    TempData["SuccessMessage"] = "Ürün başarıyla oluşturuldu.";
                    return RedirectToAction(nameof(Index));
                }
                
                // Hata durumunda dropdown listelerini tekrar doldur
                await ListeleriDoldur(model.KategoriID, model.BirimID);
                
                // AJAX isteği için hata mesajlarını döndür
                if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
                {
                    return Json(new { 
                        success = false, 
                        message = "Ürün oluşturulurken hatalar oluştu.", 
                        errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList() 
                    });
                }
                
                return PartialView("_CreateUrun", model);
            }
            catch (Exception ex)
            {
                // Hata log'u
                await _logService.LogErrorAsync("UrunController.Create", $"Ürün oluşturulurken hata: {ex.Message}");
                
                // Hata durumunda dropdown listelerini tekrar doldur
                await ListeleriDoldur(model.KategoriID, model.BirimID);
                
                // AJAX isteği için hata mesajı döndür
                if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
                {
                    return Json(new { success = false, message = "Ürün oluşturulurken bir hata oluştu: " + ex.Message });
                }
                
                ModelState.AddModelError("", "Ürün oluşturulurken beklenmeyen bir hata oluştu.");
                return PartialView("_CreateUrun", model);
            }
        }

        // Ürün düzenleme formu
        [HttpGet]
        public async Task<IActionResult> Edit(Guid id)
        {
            var urunRepository = _unitOfWork.Repository<Urun>();
            var kategoriRepository = _unitOfWork.Repository<UrunKategori>();

            // Ürün bilgisini getir
            var urun = await urunRepository.GetFirstOrDefaultAsync(u => u.UrunID == id && u.Silindi == false);
            if (urun == null)
            {
                return NotFound();
            }

            // Düzenleme görünüm modeli oluştur
            var viewModel = new UrunEditViewModel
            {
                UrunID = urun.UrunID,
                UrunKodu = urun.UrunKodu,
                UrunAdi = urun.UrunAdi,
                BirimID = urun.BirimID,
                StokMiktar = urun.StokMiktar,
                Aktif = urun.Aktif,
                KategoriID = urun.KategoriID,
                KDVOrani = urun.KDVOrani
            };

            // Dropdown listelerini hazırla
            await ListeleriDoldur(viewModel.KategoriID, viewModel.BirimID);
            
            // AJAX isteğiyse partial view döndür
            if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
            {
                return PartialView("_EditPartial", viewModel);
            }
            
            return View(viewModel);
        }

        // Ürün düzenleme işlemi
        [HttpPost]
        public async Task<IActionResult> Edit(Guid id, UrunEditViewModel model)
        {
            if (id != model.UrunID)
            {
                return NotFound();
            }

            // Gereksiz validasyon alanlarını kaldır
            ModelState.Remove("BirimListesi");
            ModelState.Remove("KategoriListesi");
            ModelState.Remove("Birim");
            
            // Validasyon hatalarını Türkçeleştir
            if (!ModelState.IsValid)
            {
                TurkceHataEkle();
            }

            if (ModelState.IsValid)
            {
                var urunRepository = _unitOfWork.Repository<Urun>();

                // Ürün bilgisini getir
                var urun = await urunRepository.GetFirstOrDefaultAsync(u => u.UrunID == id && u.Silindi == false);
                if (urun == null)
                {
                    return NotFound();
                }

                // Ürün bilgilerini güncelle
                urun.UrunKodu = model.UrunKodu;
                urun.UrunAdi = model.UrunAdi;
                urun.BirimID = model.BirimID;
                urun.StokMiktar = model.StokMiktar;
                urun.Aktif = model.Aktif;
                urun.KategoriID = model.KategoriID;
                urun.KDVOrani = model.KDVOrani;
                urun.GuncellemeTarihi = DateTime.Now;

                // Ürünü güncelle
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
            
            // ModelState geçersizse dropdown listelerini tekrar doldur
            await ListeleriDoldur(model.KategoriID, model.BirimID);
            
            // AJAX isteği için başarısız sonuç döndür
            if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
            {
                return PartialView("_EditPartial", model);
            }
            
            return View(model);
        }

        // Ürün silme onay sayfası
        public async Task<IActionResult> Delete(Guid id)
        {
            var urunRepository = _unitOfWork.Repository<Urun>();
            var urunFiyatRepository = _unitOfWork.Repository<UrunFiyat>();
            var kategoriRepository = _unitOfWork.Repository<UrunKategori>();

            // Ürün bilgisini getir
            var urun = await urunRepository.GetFirstOrDefaultAsync(u => u.UrunID == id && u.Silindi == false);
            if (urun == null)
            {
                return NotFound();
            }

            // Kategori bilgisini getir
            var kategori = urun.KategoriID.HasValue ? 
                await kategoriRepository.GetFirstOrDefaultAsync(k => k.KategoriID.ToString() == urun.KategoriID.ToString()) : null;

            // Ürün görünüm modeli oluştur
            var viewModel = new UrunViewModel
            {
                UrunID = urun.UrunID,
                UrunKodu = urun.UrunKodu,
                UrunAdi = urun.UrunAdi,
                Birim = urun.Birim != null ? urun.Birim.BirimAdi : string.Empty,
                StokMiktar = urun.StokMiktar,
                ListeFiyati = urunFiyatRepository.GetAllAsync().Result
                    .Where(uf => uf.UrunID == urun.UrunID && uf.FiyatTipiID == 1 && uf.Silindi == false)
                    .OrderByDescending(uf => uf.GecerliTarih)
                    .FirstOrDefault()?.Fiyat ?? 0m,
                MaliyetFiyati = urunFiyatRepository.GetAllAsync().Result
                    .Where(uf => uf.UrunID == urun.UrunID && uf.FiyatTipiID == 2 && uf.Silindi == false)
                    .OrderByDescending(uf => uf.GecerliTarih)
                    .FirstOrDefault()?.Fiyat ?? 0m,
                SatisFiyati = urunFiyatRepository.GetAllAsync().Result
                    .Where(uf => uf.UrunID == urun.UrunID && uf.FiyatTipiID == 3 && uf.Silindi == false)
                    .OrderByDescending(uf => uf.GecerliTarih)
                    .FirstOrDefault()?.Fiyat ?? 0m,
                Aktif = urun.Aktif,
                OlusturmaTarihi = urun.OlusturmaTarihi,
                KategoriID = urun.KategoriID,
                KategoriAdi = kategori?.KategoriAdi ?? "Kategorisiz"
            };

            return View(viewModel);
        }

        // Ürün silme işlemi (soft delete)
        [HttpPost, ActionName("Delete")]
        public async Task<IActionResult> DeleteConfirmed(Guid id)
        {
            var urunRepository = _unitOfWork.Repository<Urun>();
            var stokHareketRepository = _unitOfWork.Repository<StokHareket>();
            var faturaDetayRepository = _unitOfWork.Repository<FaturaDetay>();
            var urunFiyatRepository = _unitOfWork.Repository<UrunFiyat>();

            // Ürün bilgisini getir
            var urun = await urunRepository.GetFirstOrDefaultAsync(u => u.UrunID == id && u.Silindi == false);
            if (urun == null)
            {
                return NotFound();
            }

            try
            {
                // Fatura detaylarında kullanılıp kullanılmadığını kontrol et
                var faturaDetayVarMi = await _context.FaturaDetaylari
                    .AnyAsync(fd => fd.UrunID == id && fd.Silindi == false);
                
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
                
                // İkinci bir kontrol daha yap - Repository kullanarak
                var faturaDetaylari = await faturaDetayRepository.GetAllAsync();
                var urunKullanilmis = faturaDetaylari.Any(fd => fd.UrunID == id && !fd.Silindi);
                
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
                var stokHareketleri = await stokHareketRepository.GetAsync(h => h.UrunID == id && h.Silindi == false);
                foreach (var hareket in stokHareketleri)
                {
                    hareket.Silindi = true;
                    hareket.GuncellemeTarihi = DateTime.Now;
                    await stokHareketRepository.UpdateAsync(hareket);
                }
                
                // Ürün fiyatlarını soft delete yap
                var urunFiyatlari = await urunFiyatRepository.GetAsync(uf => uf.UrunID == id && uf.Silindi == false);
                foreach (var fiyat in urunFiyatlari)
                {
                    fiyat.Silindi = true;
                    await urunFiyatRepository.UpdateAsync(fiyat);
                }

                // Ürünü soft delete yap
                urun.Silindi = true;
                urun.Aktif = false;
                urun.GuncellemeTarihi = DateTime.Now;
                
                await urunRepository.UpdateAsync(urun);
                await _unitOfWork.SaveAsync();
                
                TempData["SuccessMessage"] = $"'{urun.UrunAdi}' ürünü ve ilişkili tüm kayıtları başarıyla silindi.";
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"Ürün silinirken bir hata oluştu: {ex.Message}";
            }

            return RedirectToAction(nameof(Index));
        }

        // Pasif ürünü aktifleştirme
        public async Task<IActionResult> Activate(Guid id)
        {
            var urunRepository = _unitOfWork.Repository<Urun>();
            
            // Ürün bilgisini getir
            var urun = await urunRepository.GetFirstOrDefaultAsync(u => u.UrunID == id && u.Silindi == false);
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

        // AJAX: Ürün detaylarını JSON formatında getir
        [HttpGet]
        public async Task<IActionResult> GetUrunDetails(Guid id)
        {
            var urunRepository = _unitOfWork.Repository<Urun>();
            var urunFiyatRepository = _unitOfWork.Repository<UrunFiyat>();

            // Ürün bilgisini getir
            var urun = await urunRepository.GetFirstOrDefaultAsync(u => u.UrunID == id && u.Silindi == false);
            if (urun == null)
            {
                return NotFound();
            }

            // Ürün fiyatlarını getir
            var fiyatlar = await urunFiyatRepository.GetAsync(
                filter: f => f.UrunID == urun.UrunID && f.Silindi == false,
                includeProperties: "FiyatTipi"
            );

            // En güncel fiyatları bul
            var listeFiyati = fiyatlar.Where(f => f.FiyatTipi.FiyatTipiID == 1)
                .OrderByDescending(f => f.GecerliTarih)
                .FirstOrDefault()?.Fiyat;

            var maliyetFiyati = fiyatlar.Where(f => f.FiyatTipi.FiyatTipiID == 2)
                .OrderByDescending(f => f.GecerliTarih)
                .FirstOrDefault()?.Fiyat;

            var satisFiyati = fiyatlar.Where(f => f.FiyatTipi.FiyatTipiID == 3)
                .OrderByDescending(f => f.GecerliTarih)
                .FirstOrDefault()?.Fiyat;

            // Ürün görünüm modeli oluştur
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

        // AJAX: Tüm aktif ürünleri JSON formatında getir
        [HttpGet]
        public async Task<IActionResult> GetUrunlerJson()
        {
            var urunler = await _unitOfWork.Repository<Urun>().GetAsync(
                filter: u => u.Aktif && !u.Silindi,
                includeProperties: "Birim");

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
        
        // AJAX: Belirli bir ürünün bilgilerini getir
        [HttpGet]
        public async Task<IActionResult> GetUrunBilgileri(Guid id)
        {
            var urun = await _unitOfWork.Repository<Urun>().GetFirstOrDefaultAsync(
                filter: u => u.UrunID == id && u.Aktif && !u.Silindi,
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
        
        #region Yardımcı Metodlar
        
        // Validasyon hatalarını Türkçeleştir
        private void TurkceHataEkle()
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
        
        // Dropdown listelerini doldur
        private async Task ListeleriDoldur(Guid? kategoriID = null, Guid? birimID = null)
        {
            var kategoriRepository = _unitOfWork.Repository<UrunKategori>();
            var kategoriler = await kategoriRepository.GetAsync(
                filter: k => k.Silindi == false && k.Aktif,
                orderBy: q => q.OrderBy(k => k.KategoriAdi)
            );
            
            var birimRepository = _unitOfWork.Repository<Birim>();
            var birimler = await birimRepository.GetAsync(
                filter: b => b.Silindi == false && b.Aktif,
                orderBy: q => q.OrderBy(b => b.BirimAdi)
            );
            
            ViewBag.Kategoriler = new SelectList(kategoriler, "KategoriID", "KategoriAdi", kategoriID);
            ViewBag.Birimler = new SelectList(birimler, "BirimID", "BirimAdi", birimID);
        }
        
        #endregion
    }
} 