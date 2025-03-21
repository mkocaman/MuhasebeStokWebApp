using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MuhasebeStokWebApp.Data.Entities;
using MuhasebeStokWebApp.Data.Repositories;
using MuhasebeStokWebApp.Services;
using MuhasebeStokWebApp.ViewModels.Cari;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MuhasebeStokWebApp.Controllers
{
    public class CariController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly SistemLogService _logService;

        public CariController(IUnitOfWork unitOfWork, SistemLogService logService)
        {
            _unitOfWork = unitOfWork;
            _logService = logService;
        }

        // GET: Cari
        public async Task<IActionResult> Index(string searchString)
        {
            // Cari repository'sini al
            var cariRepository = _unitOfWork.Repository<Cari>();
            
            // Cari hareketleri repository'sini al
            var cariHareketRepository = _unitOfWork.Repository<CariHareket>();
            
            // Cari listesini getir (filtreleme yaparak)
            var cariler = await cariRepository.GetAsync(
                filter: c => c.SoftDelete == false && 
                    (string.IsNullOrEmpty(searchString) || 
                     c.CariAdi.Contains(searchString) || 
                     c.VergiNo.Contains(searchString) || 
                     c.Telefon.Contains(searchString)),
                orderBy: q => q.OrderBy(c => c.CariAdi)
            );
            
            // Her cari için bakiye hesapla
            var cariBakiyeleri = new Dictionary<Guid, decimal>();
            
            foreach (var cari in cariler)
            {
                // Cari hareketlerini getir
                var cariHareketler = await cariHareketRepository.GetAsync(
                    filter: h => h.CariID == cari.CariID && h.SoftDelete == false
                );
                
                // Bakiyeyi hesapla
                decimal bakiye = 0;
                foreach (var hareket in cariHareketler)
                {
                    if (hareket.HareketTuru == "Tahsilat" || hareket.HareketTuru == "Alacak")
                    {
                        bakiye += hareket.Tutar;
                    }
                    else if (hareket.HareketTuru == "Ödeme" || hareket.HareketTuru == "Borç")
                    {
                        bakiye -= hareket.Tutar;
                    }
                }
                
                cariBakiyeleri.Add(cari.CariID, bakiye);
            }
            
            // ViewBag'e bakiyeleri ekle
            ViewBag.CariBakiyeleri = cariBakiyeleri;
            
            return View(cariler);
        }

        // GET: Cari/Details/5
        public async Task<IActionResult> Details(Guid id)
        {
            // Cari repository'sini al
            var cariRepository = _unitOfWork.Repository<Cari>();
            
            // Cari hareketleri repository'sini al
            var cariHareketRepository = _unitOfWork.Repository<CariHareket>();
            
            // Fatura repository'sini al
            var faturaRepository = _unitOfWork.Repository<Fatura>();
            
            // Cari bilgilerini getir
            var cari = await cariRepository.GetFirstOrDefaultAsync(c => c.CariID == id && c.SoftDelete == false);
            if (cari == null)
            {
                return NotFound();
            }
            
            decimal toplamBorc = 0;
            decimal toplamAlacak = 0;
            
            // Cari hareketlerini getir
            var cariHareketler = await cariHareketRepository.GetAsync(
                filter: h => h.CariID == cari.CariID && h.SoftDelete == false,
                orderBy: q => q.OrderByDescending(h => h.Tarih)
            );
            
            // Cari hareketlerinden bakiye hesapla
            var hareketViewModels = new List<CariHareketViewModel>();
            foreach (var hareket in cariHareketler)
            {
                if (hareket.HareketTuru == "Tahsilat" || hareket.HareketTuru == "Alacak")
                {
                    toplamAlacak += hareket.Tutar;
                }
                else if (hareket.HareketTuru == "Ödeme" || hareket.HareketTuru == "Borç")
                {
                    toplamBorc += hareket.Tutar;
                }
                
                // Hareket ViewModel'e ekle
                hareketViewModels.Add(new CariHareketViewModel
                {
                    HareketID = hareket.CariHareketID,
                    Tarih = hareket.Tarih,
                    HareketTuru = hareket.HareketTuru,
                    Tutar = hareket.Tutar,
                    Aciklama = hareket.Aciklama,
                    EvrakNo = hareket.ReferansNo ?? ""
                });
            }
            
            // Bakiyeyi hesapla (Alacak - Borç)
            decimal bakiye = toplamAlacak - toplamBorc;
            
            // Faturaları getir
            var faturalar = await faturaRepository.GetAsync(
                filter: f => f.CariID == cari.CariID && f.SoftDelete == false,
                orderBy: q => q.OrderByDescending(f => f.FaturaTarihi),
                includeProperties: "FaturaTuru"
            );
            
            var faturaViewModels = faturalar.Take(5).Select(f => new FaturaViewModel
            {
                FaturaID = f.FaturaID,
                FaturaNumarasi = f.FaturaNumarasi,
                FaturaTarihi = f.FaturaTarihi.HasValue ? f.FaturaTarihi.Value : DateTime.Now,
                VadeTarihi = f.VadeTarihi,
                GenelToplam = f.GenelToplam,
                FaturaTuru = f.FaturaTuru?.FaturaTuruAdi
            }).ToList();
            
            // ViewModel oluştur
            var viewModel = new CariDetailViewModel
            {
                CariID = cari.CariID,
                CariAdi = cari.CariAdi,
                VergiNo = cari.VergiNo,
                Telefon = cari.Telefon,
                Email = cari.Email,
                Adres = cari.Adres,
                Yetkili = cari.Yetkili,
                Aciklama = cari.Aciklama,
                Aktif = cari.Aktif,
                OlusturmaTarihi = cari.OlusturmaTarihi,
                GuncellemeTarihi = cari.GuncellemeTarihi,
                Bakiye = bakiye,
                CariHareketleri = hareketViewModels,
                SonFaturalar = faturaViewModels
            };
            
            return View(viewModel);
        }

        // GET: Cari/Create
        [HttpGet]
        public IActionResult Create()
        {
            if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
            {
                return PartialView("_CreatePartial", new CariCreateViewModel());
            }
            
            return View(new CariCreateViewModel());
        }

        // POST: Cari/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CariCreateViewModel model)
        {
            if (ModelState.IsValid)
            {
                var cariRepository = _unitOfWork.Repository<Cari>();
                
                var cari = new Cari
                {
                    CariID = Guid.NewGuid(),
                    CariAdi = model.CariAdi,
                    VergiNo = model.VergiNo,
                    Telefon = model.Telefon,
                    Email = model.Email,
                    Adres = model.Adres,
                    Yetkili = model.Yetkili,
                    Aciklama = model.Aciklama,
                    Aktif = model.Aktif,
                    OlusturmaTarihi = DateTime.Now,
                    SoftDelete = false
                };
                
                await cariRepository.AddAsync(cari);
                await _unitOfWork.SaveAsync();
                
                // Cari Hareket Ekleme
                if (model.Bakiye != 0)
                {
                    var cariHareket = new CariHareket
                    {
                        CariHareketID = Guid.NewGuid(),
                        CariID = cari.CariID,
                        Tutar = Math.Abs(model.Bakiye),
                        HareketTuru = model.Bakiye > 0 ? "Alacak" : "Borç",
                        Tarih = DateTime.Now,
                        ReferansNo = "Yeni Cari",
                        ReferansTuru = "Cari",
                        ReferansID = cari.CariID,
                        Aciklama = "Yeni cari kaydı oluşturuldu",
                        OlusturmaTarihi = DateTime.Now,
                        SoftDelete = false
                    };
                    
                    await _unitOfWork.Repository<CariHareket>().AddAsync(cariHareket);
                    await _unitOfWork.SaveAsync();
                }
                
                // AJAX isteği için başarılı sonuç döndür
                if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
                {
                    return Json(new { success = true });
                }
                
                return RedirectToAction(nameof(Index));
            }
            
            // AJAX isteği için başarısız sonuç döndür
            if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
            {
                return PartialView("_CreatePartial", model);
            }
            
            return View(model);
        }

        // GET: Cari/Edit/5
        [HttpGet]
        public async Task<IActionResult> Edit(Guid id)
        {
            var cariRepository = _unitOfWork.Repository<Cari>();
            var cariHareketRepository = _unitOfWork.Repository<CariHareket>();
            
            var cari = await cariRepository.GetFirstOrDefaultAsync(c => c.CariID == id && c.SoftDelete == false);
            if (cari == null)
            {
                return NotFound();
            }
            
            // Cari hareketlerini getir
            var hareketler = await cariHareketRepository.GetAsync(
                filter: h => h.CariID == cari.CariID && h.SoftDelete == false
            );
            
            // Bakiyeyi hesapla (Alacak - Borç)
            decimal bakiye = 0;
            foreach (var hareket in hareketler)
            {
                if (hareket.HareketTuru == "Tahsilat" || hareket.HareketTuru == "Alacak")
                {
                    bakiye += hareket.Tutar;
                }
                else if (hareket.HareketTuru == "Ödeme" || hareket.HareketTuru == "Borç")
                {
                    bakiye -= hareket.Tutar;
                }
            }
            
            var viewModel = new CariEditViewModel
            {
                CariID = cari.CariID,
                CariAdi = cari.CariAdi,
                VergiNo = cari.VergiNo,
                Telefon = cari.Telefon,
                Email = cari.Email,
                Adres = cari.Adres,
                Yetkili = cari.Yetkili,
                Aciklama = cari.Aciklama,
                Aktif = cari.Aktif,
                Bakiye = bakiye
            };
            
            if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
            {
                return PartialView("_EditPartial", viewModel);
            }
            
            return View(viewModel);
        }

        // POST: Cari/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(Guid id, CariEditViewModel model)
        {
            if (id != model.CariID)
            {
                return NotFound();
            }
            
            if (ModelState.IsValid)
            {
                try
                {
                    var cariRepository = _unitOfWork.Repository<Cari>();
                    var cariHareketRepository = _unitOfWork.Repository<CariHareket>();
                    
                    var cari = await cariRepository.GetFirstOrDefaultAsync(c => c.CariID == id && c.SoftDelete == false);
                    if (cari == null)
                    {
                        return NotFound();
                    }
                    
                    // Mevcut bakiyeyi hesapla
                    var hareketler = await cariHareketRepository.GetAsync(
                        filter: h => h.CariID == cari.CariID && h.SoftDelete == false
                    );
                    
                    decimal mevcutBakiye = 0;
                    foreach (var hareket in hareketler)
                    {
                        if (hareket.HareketTuru == "Tahsilat" || hareket.HareketTuru == "Alacak")
                        {
                            mevcutBakiye += hareket.Tutar;
                        }
                        else if (hareket.HareketTuru == "Ödeme" || hareket.HareketTuru == "Borç")
                        {
                            mevcutBakiye -= hareket.Tutar;
                        }
                    }
                    
                    // Cari bilgilerini güncelle
                    cari.CariAdi = model.CariAdi;
                    cari.VergiNo = model.VergiNo;
                    cari.Telefon = model.Telefon;
                    cari.Email = model.Email;
                    cari.Adres = model.Adres;
                    cari.Yetkili = model.Yetkili;
                    cari.Aciklama = model.Aciklama;
                    cari.Aktif = model.Aktif;
                    cari.GuncellemeTarihi = DateTime.Now;
                    
                    await cariRepository.UpdateAsync(cari);
                    await _unitOfWork.SaveAsync();
                    
                    // Bakiye değişikliği varsa hareket ekle
                    if (model.Bakiye != mevcutBakiye)
                    {
                        decimal fark = model.Bakiye - mevcutBakiye;
                        
                        var cariHareket = new CariHareket
                        {
                            CariHareketID = Guid.NewGuid(),
                            CariID = cari.CariID,
                            Tutar = Math.Abs(fark),
                            HareketTuru = fark > 0 ? "Alacak" : "Borç",
                            Tarih = DateTime.Now,
                            ReferansNo = "Bakiye Düzeltme",
                            ReferansTuru = "Cari",
                            ReferansID = cari.CariID,
                            Aciklama = "Bakiye düzeltme işlemi yapıldı",
                            OlusturmaTarihi = DateTime.Now,
                            SoftDelete = false
                        };
                        
                        await cariHareketRepository.AddAsync(cariHareket);
                        await _unitOfWork.SaveAsync();
                    }
                    
                    // AJAX isteği için başarılı sonuç döndür
                    if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
                    {
                        return Json(new { success = true });
                    }
                    
                    return RedirectToAction(nameof(Index));
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError("", "Güncelleme sırasında bir hata oluştu: " + ex.Message);
                }
            }
            
            // AJAX isteği için başarısız sonuç döndür
            if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
            {
                return PartialView("_EditPartial", model);
            }
            
            return View(model);
        }

        // GET: Cari/Delete/5
        public async Task<IActionResult> Delete(Guid id)
        {
            var cariRepository = _unitOfWork.Repository<Cari>();
            
            var cari = await cariRepository.GetFirstOrDefaultAsync(c => c.CariID == id && c.SoftDelete == false);
            if (cari == null)
            {
                return NotFound();
            }
            
            return View(cari);
        }

        // POST: Cari/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(Guid id)
        {
            var cariRepository = _unitOfWork.Repository<Cari>();
            var cariHareketRepository = _unitOfWork.Repository<CariHareket>();
            var faturaRepository = _unitOfWork.Repository<Fatura>();
            
            var cari = await cariRepository.GetFirstOrDefaultAsync(c => c.CariID == id && c.SoftDelete == false);
            if (cari == null)
            {
                return NotFound();
            }
            
            // Cari hareketlerini kontrol et
            var hareketler = await cariHareketRepository.GetAsync(h => h.CariID == id && h.SoftDelete == false);
            var hareketSayisi = hareketler.Count();
            
            // Faturaları kontrol et
            var faturalar = await faturaRepository.GetAsync(f => f.CariID == id && f.SoftDelete == false);
            var faturaSayisi = faturalar.Count();
            
            // Eğer cari hareket veya fatura varsa, pasife al
            if (hareketSayisi > 0 || faturaSayisi > 0)
            {
                cari.Aktif = false;
                cari.GuncellemeTarihi = DateTime.Now;
                
                await cariRepository.UpdateAsync(cari);
                await _unitOfWork.SaveAsync();
                
                // Sistem log kaydı oluştur
                await _logService.CariPasifeAlmaLogOlustur(
                    cariID: cari.CariID,
                    cariAdi: cari.CariAdi,
                    aciklama: $"Cari kaydı pasife alındı. Hareket sayısı: {hareketSayisi}, Fatura sayısı: {faturaSayisi}"
                );
                
                TempData["SuccessMessage"] = $"{cari.CariAdi} carisi hareket veya fatura kaydı olduğu için pasife alındı.";
            }
            else
            {
                // Soft delete
                cari.SoftDelete = true;
                cari.GuncellemeTarihi = DateTime.Now;
                
                await cariRepository.UpdateAsync(cari);
                await _unitOfWork.SaveAsync();
                
                // Sistem log kaydı oluştur
                await _logService.LogOlustur(
                    islemTuru: "Cari Silme",
                    kayitID: cari.CariID,
                    tabloAdi: "Cariler",
                    kayitAdi: cari.CariAdi,
                    aciklama: "Cari kaydı silindi"
                );
                
                TempData["SuccessMessage"] = $"{cari.CariAdi} carisi başarıyla silindi.";
            }
            
            return RedirectToAction(nameof(Index));
        }

        // GET: Cari/Ekstre/5
        public async Task<IActionResult> Ekstre(Guid id, DateTime? baslangicTarihi = null, DateTime? bitisTarihi = null)
        {
            // Repository'leri al
            var cariRepository = _unitOfWork.Repository<Cari>();
            var cariHareketRepository = _unitOfWork.Repository<CariHareket>();
            
            // Cari bilgilerini getir
            var cari = await cariRepository.GetFirstOrDefaultAsync(c => c.CariID == id && c.SoftDelete == false);
            if (cari == null)
            {
                return NotFound();
            }
            
            // Tarih aralığını belirle
            var baslangic = baslangicTarihi ?? DateTime.Now.AddMonths(-1);
            var bitis = bitisTarihi ?? DateTime.Now;
            
            // ViewModel oluştur
            var viewModel = new CariEkstreViewModel
            {
                CariID = cari.CariID,
                CariAdi = cari.CariAdi,
                VergiNo = cari.VergiNo,
                Adres = cari.Adres,
                BaslangicTarihi = baslangic,
                BitisTarihi = bitis,
                RaporTarihi = DateTime.Now
            };
            
            // Cari hareketlerini getir
            var cariHareketler = await cariHareketRepository.GetAsync(
                filter: h => h.CariID == cari.CariID && h.SoftDelete == false && 
                             h.Tarih >= baslangic && h.Tarih <= bitis,
                orderBy: q => q.OrderBy(h => h.Tarih)
            );
            
            // Tüm hareketleri birleştir
            var tumHareketler = new List<CariEkstreHareketViewModel>();
            
            // Cari hareketlerini ekle
            decimal bakiye = 0;
            decimal toplamBorc = 0;
            decimal toplamAlacak = 0;
            
            foreach (var hareket in cariHareketler)
            {
                var ekstreHareket = new CariEkstreHareketViewModel
                {
                    Tarih = hareket.Tarih,
                    Aciklama = hareket.Aciklama,
                    IslemTuru = hareket.HareketTuru,
                    EvrakNo = hareket.ReferansNo,
                    Kaynak = "Cari"
                };
                
                if (hareket.HareketTuru == "Tahsilat" || hareket.HareketTuru == "Alacak")
                {
                    ekstreHareket.Alacak = hareket.Tutar;
                    bakiye += hareket.Tutar;
                    toplamAlacak += hareket.Tutar;
                }
                else if (hareket.HareketTuru == "Ödeme" || hareket.HareketTuru == "Borç")
                {
                    ekstreHareket.Borc = hareket.Tutar;
                    bakiye -= hareket.Tutar;
                    toplamBorc += hareket.Tutar;
                }
                
                ekstreHareket.Bakiye = bakiye;
                tumHareketler.Add(ekstreHareket);
            }
            
            viewModel.Hareketler = tumHareketler;
            viewModel.ToplamAlacak = toplamAlacak;
            viewModel.ToplamBorc = toplamBorc;
            viewModel.Bakiye = bakiye;
            
            return View(viewModel);
        }

        // GET: Cari/HareketEkle/5
        [HttpGet]
        public async Task<IActionResult> HareketEkle(Guid id)
        {
            var cariRepository = _unitOfWork.Repository<Cari>();
            
            var cari = await cariRepository.GetFirstOrDefaultAsync(c => c.CariID == id && c.SoftDelete == false);
            if (cari == null)
            {
                return NotFound();
            }
            
            var viewModel = new CariHareketCreateViewModel
            {
                CariID = cari.CariID,
                CariAdi = cari.CariAdi,
                Tarih = DateTime.Now,
                HareketTuru = "Tahsilat" // Varsayılan değer
            };
            
            if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
            {
                return PartialView("_HareketEklePartial", viewModel);
            }
            
            return View(viewModel);
        }

        // POST: Cari/HareketEkle
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> HareketEkle(CariHareketCreateViewModel model)
        {
            if (ModelState.IsValid)
            {
                var cariRepository = _unitOfWork.Repository<Cari>();
                var cariHareketRepository = _unitOfWork.Repository<CariHareket>();
                
                var cari = await cariRepository.GetFirstOrDefaultAsync(c => c.CariID == model.CariID && c.SoftDelete == false);
                if (cari == null)
                {
                    return NotFound();
                }
                
                var cariHareket = new CariHareket
                {
                    CariHareketID = Guid.NewGuid(),
                    CariID = model.CariID,
                    Tutar = model.Tutar,
                    HareketTuru = model.HareketTuru,
                    Tarih = model.Tarih,
                    ReferansNo = model.ReferansNo,
                    ReferansTuru = model.ReferansTuru,
                    ReferansID = model.ReferansID,
                    Aciklama = model.Aciklama,
                    OlusturmaTarihi = DateTime.Now,
                    SoftDelete = false
                };
                
                await cariHareketRepository.AddAsync(cariHareket);
                await _unitOfWork.SaveAsync();
                
                // AJAX isteği için başarılı sonuç döndür
                if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
                {
                    return Json(new { success = true });
                }
                
                return RedirectToAction(nameof(Details), new { id = model.CariID });
            }
            
            // AJAX isteği için başarısız sonuç döndür
            if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
            {
                return PartialView("_HareketEklePartial", model);
            }
            
            return View(model);
        }

        // GET: Cari/Activate/5
        public async Task<IActionResult> Activate(Guid id)
        {
            var cariRepository = _unitOfWork.Repository<Cari>();
            
            var cari = await cariRepository.GetFirstOrDefaultAsync(c => c.CariID == id && c.SoftDelete == false);
            if (cari == null)
            {
                return NotFound();
            }
            
            // Cariyi aktif yap
            cari.Aktif = true;
            cari.GuncellemeTarihi = DateTime.Now;
            
            await cariRepository.UpdateAsync(cari);
            await _unitOfWork.SaveAsync();
            
            // Sistem log kaydı oluştur
            await _logService.CariAktifleştirmeLogOlustur(
                cariID: cari.CariID,
                cariAdi: cari.CariAdi,
                aciklama: "Cari kaydı aktifleştirildi"
            );
            
            TempData["SuccessMessage"] = $"{cari.CariAdi} carisi başarıyla aktifleştirildi.";
            
            return RedirectToAction(nameof(Index));
        }
    }
}