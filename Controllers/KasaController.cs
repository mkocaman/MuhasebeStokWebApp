using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using MuhasebeStokWebApp.Data;
using MuhasebeStokWebApp.Data.Entities;
using MuhasebeStokWebApp.ViewModels.Kasa;

namespace MuhasebeStokWebApp.Controllers
{
    // [Authorize] attribute'ü kaldırıldı - geliştirme sürecinde geçici olarak
    public class KasaController : Controller
    {
        private readonly ApplicationDbContext _context;

        public KasaController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Kasa
        public async Task<IActionResult> Index()
        {
            var tumKasalar = await _context.Kasalar
                .Where(k => !k.SoftDelete)
                .OrderByDescending(k => k.OlusturmaTarihi)
                .Select(k => new KasaViewModel
                {
                    KasaID = k.KasaID,
                    KasaAdi = k.KasaAdi,
                    KasaTuru = k.KasaTuru,
                    ParaBirimi = k.ParaBirimi,
                    AcilisBakiye = k.AcilisBakiye,
                    GuncelBakiye = k.GuncelBakiye,
                    Aciklama = k.Aciklama,
                    Aktif = k.Aktif,
                    OlusturmaTarihi = k.OlusturmaTarihi
                })
                .ToListAsync();

            var aktifKasalar = tumKasalar.Where(k => k.Aktif).ToList();
            var pasifKasalar = tumKasalar.Where(k => !k.Aktif).ToList();

            // Para birimine göre toplamları hesapla
            var paraBirimiToplamlari = aktifKasalar
                .GroupBy(k => k.ParaBirimi)
                .ToDictionary(g => g.Key, g => g.Sum(k => k.GuncelBakiye));

            var model = new KasaListViewModel
            {
                Kasalar = aktifKasalar,
                PasifKasalar = pasifKasalar,
                ToplamBakiye = aktifKasalar.Sum(k => k.GuncelBakiye),
                ParaBirimiToplamlari = paraBirimiToplamlari
            };

            return View(model);
        }

        // GET: Kasa/Details/5
        public async Task<IActionResult> Details(Guid? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var kasa = await _context.Kasalar
                .FirstOrDefaultAsync(k => k.KasaID == id && !k.SoftDelete);

            if (kasa == null)
            {
                return NotFound();
            }

            var model = new KasaViewModel
            {
                KasaID = kasa.KasaID,
                KasaAdi = kasa.KasaAdi,
                KasaTuru = kasa.KasaTuru,
                ParaBirimi = kasa.ParaBirimi,
                AcilisBakiye = kasa.AcilisBakiye,
                GuncelBakiye = kasa.GuncelBakiye,
                Aciklama = kasa.Aciklama,
                Aktif = kasa.Aktif,
                OlusturmaTarihi = kasa.OlusturmaTarihi
            };

            return View(model);
        }

        // GET: Kasa/Create
        public async Task<IActionResult> Create()
        {
            try
            {
                // Para birimlerini veritabanından al
                var paraBirimleri = new List<string> { "TRY" }; // Varsayılan olarak TL ekle
                
                try
                {
                    // Para birimlerini getir
                    var paraBirimiListesi = await _context.ParaBirimleri
                        .Where(p => p.Aktif)
                        .Select(p => p.Kod)
                        .ToListAsync();
                    
                    // Benzersiz para birimlerini ekle
                    foreach (var kod in paraBirimiListesi)
                    {
                        if (!paraBirimleri.Contains(kod))
                        {
                            paraBirimleri.Add(kod);
                        }
                    }
                }
                catch (Exception ex)
                {
                    // Hata durumunda varsayılan para birimlerini kullan
                    if (!paraBirimleri.Contains("USD")) paraBirimleri.Add("USD");
                    if (!paraBirimleri.Contains("EUR")) paraBirimleri.Add("EUR");
                    if (!paraBirimleri.Contains("GBP")) paraBirimleri.Add("GBP");
                }
                
                ViewBag.ParaBirimleri = paraBirimleri;
                
                return View(new KasaCreateViewModel());
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"Kasa oluşturma sayfası yüklenirken bir hata oluştu: {ex.Message}";
                return RedirectToAction(nameof(Index));
            }
        }

        // POST: Kasa/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(KasaCreateViewModel model)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    var kasa = new Kasa
                    {
                        KasaID = Guid.NewGuid(),
                        KasaAdi = model.KasaAdi,
                        KasaTuru = model.KasaTuru,
                        ParaBirimi = model.ParaBirimi,
                        Aciklama = model.Aciklama,
                        Aktif = model.Aktif,
                        AcilisBakiye = 0, // Açılış bakiyesi 0 olarak ayarla
                        GuncelBakiye = 0, // Güncel bakiye 0 olarak ayarla
                        OlusturanKullaniciID = GetCurrentUserId(),
                        OlusturmaTarihi = DateTime.Now,
                        SoftDelete = false
                    };

                    _context.Add(kasa);
                    await _context.SaveChangesAsync();

                    TempData["SuccessMessage"] = "Kasa başarıyla oluşturuldu.";
                    return RedirectToAction(nameof(Index));
                }
                
                // Para birimlerini tekrar yükle
                var paraBirimleri = new List<string> { "TRY" };
                
                try
                {
                    var paraBirimiListesi = await _context.ParaBirimleri
                        .Where(p => p.Aktif)
                        .Select(p => p.Kod)
                        .ToListAsync();
                    
                    foreach (var kod in paraBirimiListesi)
                    {
                        if (!paraBirimleri.Contains(kod))
                        {
                            paraBirimleri.Add(kod);
                        }
                    }
                }
                catch
                {
                    if (!paraBirimleri.Contains("USD")) paraBirimleri.Add("USD");
                    if (!paraBirimleri.Contains("EUR")) paraBirimleri.Add("EUR");
                    if (!paraBirimleri.Contains("GBP")) paraBirimleri.Add("GBP");
                }
                
                ViewBag.ParaBirimleri = paraBirimleri;
                
                return View(model);
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"Kasa oluşturulurken bir hata oluştu: {ex.Message}";
                return RedirectToAction(nameof(Index));
            }
        }

        // GET: Kasa/Edit/5
        public async Task<IActionResult> Edit(Guid? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var kasa = await _context.Kasalar
                .FirstOrDefaultAsync(k => k.KasaID == id && !k.SoftDelete);

            if (kasa == null)
            {
                return NotFound();
            }

            var model = new KasaEditViewModel
            {
                KasaID = kasa.KasaID,
                KasaAdi = kasa.KasaAdi,
                KasaTuru = kasa.KasaTuru,
                ParaBirimi = kasa.ParaBirimi,
                AcilisBakiye = kasa.AcilisBakiye,
                GuncelBakiye = kasa.GuncelBakiye,
                Aciklama = kasa.Aciklama,
                Aktif = kasa.Aktif
            };

            return View(model);
        }

        // POST: Kasa/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(Guid id, KasaEditViewModel model)
        {
            if (id != model.KasaID)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    var kasa = await _context.Kasalar
                        .FirstOrDefaultAsync(k => k.KasaID == id && !k.SoftDelete);

                    if (kasa == null)
                    {
                        return NotFound();
                    }

                    // Açılış bakiyesi değiştiyse, güncel bakiyeyi de güncelle
                    decimal bakiyeFarki = model.AcilisBakiye - kasa.AcilisBakiye;
                    kasa.GuncelBakiye += bakiyeFarki;

                    kasa.KasaAdi = model.KasaAdi;
                    kasa.KasaTuru = model.KasaTuru;
                    kasa.ParaBirimi = model.ParaBirimi;
                    kasa.AcilisBakiye = model.AcilisBakiye;
                    kasa.Aciklama = model.Aciklama;
                    kasa.Aktif = model.Aktif;
                    kasa.SonGuncelleyenKullaniciID = GetCurrentUserId();
                    kasa.GuncellemeTarihi = DateTime.Now;

                    _context.Update(kasa);
                    await _context.SaveChangesAsync();

                    // Açılış bakiyesi değiştiyse, bir kasa hareketi ekleyelim
                    if (bakiyeFarki != 0)
                    {
                        var kasaHareket = new KasaHareket
                        {
                            KasaHareketID = Guid.NewGuid(),
                            KasaID = kasa.KasaID,
                            Tutar = Math.Abs(bakiyeFarki),
                            HareketTuru = bakiyeFarki > 0 ? "Bakiye Artışı" : "Bakiye Azalışı",
                            Tarih = DateTime.Now,
                            ReferansNo = "DUZENLE-" + DateTime.Now.ToString("yyyyMMddHHmmss"),
                            ReferansTuru = "Düzenleme",
                            Aciklama = "Kasa açılış bakiyesi düzeltme",
                            IslemYapanKullaniciID = GetCurrentUserId(),
                            OlusturmaTarihi = DateTime.Now,
                            SoftDelete = false
                        };

                        _context.Add(kasaHareket);
                        await _context.SaveChangesAsync();
                    }

                    TempData["SuccessMessage"] = "Kasa başarıyla güncellendi.";
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!KasaExists(model.KasaID))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            return View(model);
        }

        // GET: Kasa/Delete/5
        public async Task<IActionResult> Delete(Guid? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var kasa = await _context.Kasalar
                .FirstOrDefaultAsync(k => k.KasaID == id && !k.SoftDelete);

            if (kasa == null)
            {
                return NotFound();
            }

            var model = new KasaViewModel
            {
                KasaID = kasa.KasaID,
                KasaAdi = kasa.KasaAdi,
                KasaTuru = kasa.KasaTuru,
                ParaBirimi = "TRY",
                AcilisBakiye = kasa.AcilisBakiye,
                GuncelBakiye = kasa.GuncelBakiye,
                Aciklama = kasa.Aciklama,
                Aktif = kasa.Aktif,
                OlusturmaTarihi = kasa.OlusturmaTarihi
            };

            return View(model);
        }

        // POST: Kasa/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(Guid id)
        {
            var kasa = await _context.Kasalar
                .FirstOrDefaultAsync(k => k.KasaID == id && !k.SoftDelete);

            if (kasa == null)
            {
                return NotFound();
            }

            // Silme işlemi yerine pasife alma işlemi
            kasa.Aktif = false;
            kasa.GuncellemeTarihi = DateTime.Now;
            
            _context.Update(kasa);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Kasa başarıyla pasife alındı.";
            return RedirectToAction(nameof(Index));
        }

        // GET: Kasa/Hareketler/5
        [HttpGet]
        [Route("Kasa/Hareketler/{id}")]
        public async Task<IActionResult> Hareketler(Guid id, DateTime? baslangicTarihi = null, DateTime? bitisTarihi = null, string? hareketTuru = null)
        {
            var kasa = await _context.Kasalar
                .FirstOrDefaultAsync(k => k.KasaID == id && !k.SoftDelete);
            if (kasa == null)
            {
                return NotFound();
            }

            var viewModel = new KasaHareketlerViewModel
            {
                KasaID = kasa.KasaID,
                KasaAdi = kasa.KasaAdi,
                KasaTuru = kasa.KasaTuru,
                ParaBirimi = kasa.ParaBirimi,
                AcilisBakiye = kasa.AcilisBakiye,
                GuncelBakiye = kasa.GuncelBakiye,
                BaslangicTarihi = baslangicTarihi ?? DateTime.Now.AddMonths(-1),
                BitisTarihi = bitisTarihi ?? DateTime.Now,
                Hareketler = new List<KasaHareketViewModel>()
            };

            var query = _context.KasaHareketleri
                .Include(h => h.Cari)
                .Include(h => h.HedefKasa)
                .Include(h => h.HedefBanka)
                .Include(h => h.KaynakBanka)
                .Where(h => h.KasaID == id && !h.SoftDelete);

            if (baslangicTarihi.HasValue)
            {
                query = query.Where(h => h.Tarih >= baslangicTarihi.Value);
            }

            if (bitisTarihi.HasValue)
            {
                query = query.Where(h => h.Tarih <= bitisTarihi.Value.AddDays(1).AddSeconds(-1));
            }

            if (!string.IsNullOrEmpty(hareketTuru))
            {
                query = query.Where(h => h.HareketTuru == hareketTuru);
            }

            var hareketler = await query.OrderByDescending(h => h.Tarih).ToListAsync();

            decimal toplamGiris = 0;
            decimal toplamCikis = 0;

            foreach (var hareket in hareketler)
            {
                var hareketViewModel = new KasaHareketViewModel
                {
                    KasaHareketID = hareket.KasaHareketID,
                    KasaID = hareket.KasaID,
                    KasaAdi = kasa.KasaAdi,
                    Tutar = hareket.Tutar,
                    HareketTuru = hareket.HareketTuru,
                    Tarih = hareket.Tarih,
                    ReferansNo = hareket.ReferansNo,
                    ReferansTuru = hareket.ReferansTuru,
                    Aciklama = hareket.Aciklama,
                    DovizKuru = hareket.DovizKuru,
                    ParaBirimi = kasa.ParaBirimi,
                    KarsiParaBirimi = hareket.KarsiParaBirimi,
                    TransferID = hareket.TransferID,
                    HedefKasaID = hareket.HedefKasaID,
                    HedefKasaAdi = hareket.HedefKasa?.KasaAdi,
                    IslemTuru = hareket.IslemTuru,
                    CariID = hareket.CariID,
                    CariAdi = hareket.Cari?.CariAdi,
                    HedefBankaID = hareket.HedefBankaID,
                    HedefBankaAdi = hareket.HedefBanka?.BankaAdi,
                    KaynakBankaID = hareket.KaynakBankaID,
                    KaynakBankaAdi = hareket.KaynakBanka?.BankaAdi
                };

                viewModel.Hareketler.Add(hareketViewModel);

                if (hareket.HareketTuru == "Giriş")
                {
                    toplamGiris += hareket.Tutar;
                }
                else if (hareket.HareketTuru == "Çıkış")
                {
                    toplamCikis += hareket.Tutar;
                }
            }

            viewModel.ToplamGiris = toplamGiris;
            viewModel.ToplamCikis = toplamCikis;

            return View(viewModel);
        }

        // GET: Kasa/HareketlerTarih
        [HttpGet]
        [Route("Kasa/HareketlerTarih")]
        public async Task<IActionResult> HareketlerTarih(Guid? id, DateTime? startDate, DateTime? endDate)
        {
            if (id == null)
            {
                return NotFound();
            }

            var kasa = await _context.Kasalar
                .FirstOrDefaultAsync(k => k.KasaID == id && !k.SoftDelete);

            if (kasa == null)
            {
                return NotFound();
            }

            IQueryable<KasaHareket> query = _context.KasaHareketleri
                .Where(h => h.KasaID == id && !h.SoftDelete);

            // Tarih filtreleme
            if (startDate.HasValue)
            {
                query = query.Where(h => h.Tarih >= startDate.Value);
            }

            if (endDate.HasValue)
            {
                query = query.Where(h => h.Tarih <= endDate.Value.AddDays(1).AddSeconds(-1));
            }

            var hareketler = await query
                .OrderByDescending(h => h.Tarih)
                .ToListAsync();

            ViewBag.Kasa = new KasaViewModel
            {
                KasaID = kasa.KasaID,
                KasaAdi = kasa.KasaAdi,
                KasaTuru = kasa.KasaTuru,
                ParaBirimi = kasa.ParaBirimi,
                AcilisBakiye = kasa.AcilisBakiye,
                GuncelBakiye = kasa.GuncelBakiye
            };

            ViewBag.StartDate = startDate;
            ViewBag.EndDate = endDate;

            return View(hareketler);
        }

        // GET: Kasa/YeniHareket
        public async Task<IActionResult> YeniHareket()
        {
            try
            {
                // Aktif kasaları getir
                var kasalar = await _context.Kasalar
                    .Where(k => !k.SoftDelete && k.Aktif)
                    .OrderBy(k => k.KasaAdi)
                    .ToListAsync();

                // Aktif müşterileri getir
                var cariler = await _context.Cariler
                    .Where(c => !c.SoftDelete && c.Aktif)
                    .OrderBy(c => c.CariAdi)
                    .ToListAsync();

                // Otomatik hareket numarası oluştur
                var today = DateTime.Now;
                var hareketNo = $"HRK-{today:yyMMdd}-{new Random().Next(1000, 9999)}";

                // ViewModel oluştur
                var viewModel = new KasaHareketCreateViewModel
                {
                    Tarih = DateTime.Now,
                    HareketNo = hareketNo,
                    Kasalar = kasalar.Select(k => new SelectListItem
                    {
                        Value = k.KasaID.ToString(),
                        Text = $"{k.KasaAdi} ({k.ParaBirimi}) - Bakiye: {k.GuncelBakiye:N2}"
                    }).ToList(),
                    Cariler = cariler.Select(c => new SelectListItem
                    {
                        Value = c.CariID.ToString(),
                        Text = c.CariAdi
                    }).ToList()
                };
                
                return View(viewModel);
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"Kasa hareketi sayfası yüklenirken bir hata oluştu: {ex.Message}";
                return RedirectToAction(nameof(Index));
            }
        }

        // POST: Kasa/YeniHareket
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> YeniHareket(KasaHareketCreateViewModel model)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    var kasa = await _context.Kasalar
                        .FirstOrDefaultAsync(k => k.KasaID == model.KasaID && !k.SoftDelete);

                    if (kasa == null)
                    {
                        return NotFound();
                    }

                    // KasaHareket oluştur
                    var hareket = new KasaHareket
                    {
                        KasaHareketID = Guid.NewGuid(),
                        KasaID = model.KasaID,
                        CariID = model.CariID,
                        Tutar = model.Tutar,
                        HareketTuru = model.HareketTuru,
                        IslemTuru = "Normal",
                        Tarih = model.Tarih,
                        ReferansNo = model.HareketNo,
                        ReferansTuru = model.HareketTuru,
                        Aciklama = model.Aciklama,
                        IslemYapanKullaniciID = GetCurrentUserId(),
                        OlusturmaTarihi = DateTime.Now,
                        SoftDelete = false,
                        DovizKuru = 1,
                        KarsiParaBirimi = kasa.ParaBirimi
                    };
                    
                    _context.KasaHareketleri.Add(hareket);

                    // Kasa bakiyesini güncelle
                    if (model.HareketTuru == "Tahsilat")
                    {
                        kasa.GuncelBakiye += model.Tutar;
                    }
                    else if (model.HareketTuru == "Ödeme")
                    {
                        kasa.GuncelBakiye -= model.Tutar;
                    }
                    
                    _context.Update(kasa);
                    
                    // Eğer cari seçilmişse, cari hareketini de ekle
                    if (model.CariID.HasValue)
                    {
                        var cari = await _context.Cariler.FindAsync(model.CariID);
                        if (cari != null)
                        {
                            // Cari hareketi oluştur
                            var cariHareket = new CariHareket
                            {
                                CariHareketID = Guid.NewGuid(),
                                CariID = cari.CariID,
                                Tutar = model.Tutar,
                                HareketTuru = model.HareketTuru,
                                Tarih = model.Tarih,
                                ReferansNo = model.HareketNo,
                                ReferansTuru = "Kasa",
                                ReferansID = hareket.KasaHareketID,
                                Aciklama = model.Aciklama,
                                IslemYapanKullaniciID = GetCurrentUserId(),
                                OlusturmaTarihi = DateTime.Now,
                                SoftDelete = false
                            };
                            
                            _context.CariHareketler.Add(cariHareket);
                        }
                    }

                    await _context.SaveChangesAsync();

                    TempData["SuccessMessage"] = "Kasa hareketi başarıyla eklendi.";
                    return RedirectToAction(nameof(Hareketler), new { id = hareket.KasaID });
                }
                
                // Validasyon hatası varsa
                var kasalar = await _context.Kasalar
                    .Where(k => !k.SoftDelete && k.Aktif)
                    .OrderBy(k => k.KasaAdi)
                    .ToListAsync();
                    
                var cariler = await _context.Cariler
                    .Where(c => !c.SoftDelete && c.Aktif)
                    .OrderBy(c => c.CariAdi)
                    .ToListAsync();

                // ViewModel'i yeniden doldur
                model.Kasalar = kasalar.Select(k => new SelectListItem
                {
                    Value = k.KasaID.ToString(),
                    Text = $"{k.KasaAdi} ({k.ParaBirimi}) - Bakiye: {k.GuncelBakiye:N2}",
                    Selected = k.KasaID == model.KasaID
                }).ToList();
                
                model.Cariler = cariler.Select(c => new SelectListItem
                {
                    Value = c.CariID.ToString(),
                    Text = c.CariAdi,
                    Selected = c.CariID == model.CariID
                }).ToList();
                
                return View(model);
            }
            catch (Exception ex)
            {
                // Hata durumunda
                ModelState.AddModelError("", "Kasa hareketi eklenirken bir hata oluştu: " + ex.Message);
                
                var kasalar = await _context.Kasalar
                    .Where(k => !k.SoftDelete && k.Aktif)
                    .OrderBy(k => k.KasaAdi)
                    .ToListAsync();
                    
                var cariler = await _context.Cariler
                    .Where(c => !c.SoftDelete && c.Aktif)
                    .OrderBy(c => c.CariAdi)
                    .ToListAsync();

                // ViewModel'i yeniden doldur
                model.Kasalar = kasalar.Select(k => new SelectListItem
                {
                    Value = k.KasaID.ToString(),
                    Text = $"{k.KasaAdi} ({k.ParaBirimi}) - Bakiye: {k.GuncelBakiye:N2}",
                    Selected = k.KasaID == model.KasaID
                }).ToList();
                
                model.Cariler = cariler.Select(c => new SelectListItem
                {
                    Value = c.CariID.ToString(),
                    Text = c.CariAdi,
                    Selected = c.CariID == model.CariID
                }).ToList();
                
                return View(model);
            }
        }
        
        // GET: Kasa/Transfer
        public async Task<IActionResult> Transfer()
        {
            var kasalar = await _context.Kasalar
                .Where(k => !k.SoftDelete && k.Aktif)
                .OrderBy(k => k.KasaAdi)
                .ToListAsync();
                
            var paraBirimleri = await _context.ParaBirimleri
                .Where(p => p.Aktif)
                .Select(p => p.Kod)
                .ToListAsync();
                
            var bankalar = await _context.Bankalar
                .Where(b => !b.SoftDelete && b.Aktif)
                .OrderBy(b => b.BankaAdi)
                .ToListAsync();

            ViewBag.Kasalar = kasalar;
            ViewBag.ParaBirimleri = paraBirimleri;
            ViewBag.Bankalar = bankalar;
            
            return View(new KasaTransferViewModel());
        }
        
        // POST: Kasa/Transfer
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Transfer(KasaTransferViewModel model)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    // Transfer tipine göre işlem yap
                    switch (model.TransferTipi)
                    {
                        case "KasaToKasa":
                            return await KasadanKasayaTransfer(model);
                        case "KasaToBanka":
                            return await KasadanBankayaTransfer(model);
                        case "BankaToKasa":
                            return await BankadanKasayaTransfer(model);
                        default:
                            ModelState.AddModelError("", "Geçersiz transfer tipi.");
                            TempData["ErrorMessage"] = "Geçersiz transfer tipi.";
                            break;
                    }
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError("", "Transfer işlemi sırasında bir hata oluştu: " + ex.Message);
                    TempData["ErrorMessage"] = "Transfer işlemi sırasında bir hata oluştu: " + ex.Message;
                }
            }
            
            var kasalar = await _context.Kasalar
                .Where(k => !k.SoftDelete && k.Aktif)
                .OrderBy(k => k.KasaAdi)
                .ToListAsync();
                
            var paraBirimleri = await _context.ParaBirimleri
                .Where(p => p.Aktif)
                .Select(p => p.Kod)
                .ToListAsync();
                
            var bankalar = await _context.Bankalar
                .Where(b => !b.SoftDelete && b.Aktif)
                .OrderBy(b => b.BankaAdi)
                .ToListAsync();

            ViewBag.Kasalar = kasalar;
            ViewBag.ParaBirimleri = paraBirimleri;
            ViewBag.Bankalar = bankalar;
            
            return View(model);
        }

        // Kasadan kasaya transfer işlemi
        private async Task<IActionResult> KasadanKasayaTransfer(KasaTransferViewModel model)
        {
            var kaynakKasa = await _context.Kasalar.FindAsync(model.KaynakKasaID);
            var hedefKasa = await _context.Kasalar.FindAsync(model.HedefKasaID);
            
            if (kaynakKasa == null || hedefKasa == null)
            {
                ModelState.AddModelError("", "Kaynak veya hedef kasa bulunamadı.");
                TempData["ErrorMessage"] = "Kaynak veya hedef kasa bulunamadı.";
                return View(model);
            }
            
            if (kaynakKasa.KasaID == hedefKasa.KasaID)
            {
                ModelState.AddModelError("", "Aynı kasalar arasında transfer yapılamaz.");
                TempData["ErrorMessage"] = "Aynı kasalar arasında transfer yapılamaz.";
                return View(model);
            }
            
            if (kaynakKasa.GuncelBakiye < model.KaynakTutar)
            {
                ModelState.AddModelError("", "Kaynak kasadaki bakiye yetersiz.");
                TempData["ErrorMessage"] = "Kaynak kasadaki bakiye yetersiz.";
                return View(model);
            }
            
            // Benzersiz transfer ID oluştur
            var transferID = Guid.NewGuid();
            
            // Referans no oluştur
            string referansNo = $"KTR-{DateTime.Now:yyMMddHHmmss}";
            
            // Kaynak kasadan çıkış hareketi
            var kaynakHareket = new KasaHareket
            {
                KasaHareketID = Guid.NewGuid(),
                KasaID = kaynakKasa.KasaID,
                HedefKasaID = hedefKasa.KasaID,
                Tutar = model.KaynakTutar,
                HareketTuru = "Çıkış",
                IslemTuru = "KasaTransfer",
                Tarih = DateTime.Now,
                ReferansNo = referansNo,
                ReferansTuru = "KasaTransfer",
                Aciklama = model.Aciklama ?? $"{kaynakKasa.KasaAdi}'dan {hedefKasa.KasaAdi}'ya transfer",
                IslemYapanKullaniciID = GetCurrentUserId(),
                OlusturmaTarihi = DateTime.Now,
                SoftDelete = false,
                DovizKuru = model.KurDegeri,
                KarsiParaBirimi = hedefKasa.ParaBirimi,
                TransferID = transferID
            };
            
            // Hedef kasaya giriş hareketi
            var hedefHareket = new KasaHareket
            {
                KasaHareketID = Guid.NewGuid(),
                KasaID = hedefKasa.KasaID,
                HedefKasaID = kaynakKasa.KasaID,
                Tutar = model.HedefTutar,
                HareketTuru = "Giriş",
                IslemTuru = "KasaTransfer",
                Tarih = DateTime.Now,
                ReferansNo = referansNo,
                ReferansTuru = "KasaTransfer",
                Aciklama = model.Aciklama ?? $"{kaynakKasa.KasaAdi}'dan {hedefKasa.KasaAdi}'ya transfer",
                IslemYapanKullaniciID = GetCurrentUserId(),
                OlusturmaTarihi = DateTime.Now,
                SoftDelete = false,
                DovizKuru = 1 / model.KurDegeri,
                KarsiParaBirimi = kaynakKasa.ParaBirimi,
                TransferID = transferID
            };
            
            // Bakiyeleri güncelle
            kaynakKasa.GuncelBakiye -= model.KaynakTutar;
            hedefKasa.GuncelBakiye += model.HedefTutar;
            
            // Veritabanına kaydet
            _context.KasaHareketleri.Add(kaynakHareket);
            _context.KasaHareketleri.Add(hedefHareket);
            _context.Update(kaynakKasa);
            _context.Update(hedefKasa);
            
            await _context.SaveChangesAsync();
            
            TempData["SuccessMessage"] = "Kasadan kasaya transfer işlemi başarıyla tamamlandı.";
            return RedirectToAction(nameof(Index));
        }

        // Kasadan bankaya transfer işlemi
        private async Task<IActionResult> KasadanBankayaTransfer(KasaTransferViewModel model)
        {
            var kaynakKasa = await _context.Kasalar.FindAsync(model.KaynakKasaID);
            var hedefBanka = await _context.Bankalar.FindAsync(model.HedefBankaID);
            
            if (kaynakKasa == null || hedefBanka == null)
            {
                ModelState.AddModelError("", "Kaynak kasa veya hedef banka bulunamadı.");
                TempData["ErrorMessage"] = "Kaynak kasa veya hedef banka bulunamadı.";
                return View(model);
            }
            
            if (kaynakKasa.GuncelBakiye < model.KaynakTutar)
            {
                ModelState.AddModelError("", "Kaynak kasadaki bakiye yetersiz.");
                TempData["ErrorMessage"] = "Kaynak kasadaki bakiye yetersiz.";
                return View(model);
            }
            
            // Benzersiz transfer ID oluştur
            var transferID = Guid.NewGuid();
            
            // Referans no oluştur
            string referansNo = $"KBT-{DateTime.Now:yyMMddHHmmss}";
            
            // Kaynak kasadan çıkış hareketi
            var kasaHareket = new KasaHareket
            {
                KasaHareketID = Guid.NewGuid(),
                KasaID = kaynakKasa.KasaID,
                HedefBankaID = hedefBanka.BankaID,
                Tutar = model.KaynakTutar,
                HareketTuru = "Çıkış",
                IslemTuru = "BankaTransfer",
                Tarih = DateTime.Now,
                ReferansNo = referansNo,
                ReferansTuru = "BankaTransfer",
                Aciklama = model.Aciklama ?? $"{kaynakKasa.KasaAdi}'dan {hedefBanka.BankaAdi}'ya transfer",
                IslemYapanKullaniciID = GetCurrentUserId(),
                OlusturmaTarihi = DateTime.Now,
                SoftDelete = false,
                DovizKuru = model.KurDegeri,
                KarsiParaBirimi = hedefBanka.ParaBirimi,
                TransferID = transferID
            };
            
            // Hedef bankaya giriş hareketi
            var bankaHareket = new BankaHareket
            {
                BankaHareketID = Guid.NewGuid(),
                BankaID = hedefBanka.BankaID,
                KaynakKasaID = kaynakKasa.KasaID,
                Tutar = model.HedefTutar,
                HareketTuru = "Giriş",
                Tarih = DateTime.Now,
                ReferansNo = referansNo,
                ReferansTuru = "KasaTransfer",
                DekontNo = $"DKN-{DateTime.Now:yyMMddHHmmss}",
                Aciklama = model.Aciklama ?? $"{kaynakKasa.KasaAdi}'dan {hedefBanka.BankaAdi}'ya transfer",
                KarsiUnvan = kaynakKasa.KasaAdi,
                IslemYapanKullaniciID = GetCurrentUserId(),
                OlusturmaTarihi = DateTime.Now,
                SoftDelete = false,
                TransferID = transferID
            };
            
            // Bakiyeleri güncelle
            kaynakKasa.GuncelBakiye -= model.KaynakTutar;
            hedefBanka.GuncelBakiye += model.HedefTutar;
            
            // Veritabanına kaydet
            _context.KasaHareketleri.Add(kasaHareket);
            _context.BankaHareketleri.Add(bankaHareket);
            _context.Update(kaynakKasa);
            _context.Update(hedefBanka);
            
            await _context.SaveChangesAsync();
            
            TempData["SuccessMessage"] = "Kasadan bankaya transfer işlemi başarıyla tamamlandı.";
            return RedirectToAction(nameof(Index));
        }

        // Bankadan kasaya transfer işlemi
        private async Task<IActionResult> BankadanKasayaTransfer(KasaTransferViewModel model)
        {
            var kaynakBanka = await _context.Bankalar.FindAsync(model.KaynakBankaID);
            var hedefKasa = await _context.Kasalar.FindAsync(model.HedefKasaID);
            
            if (kaynakBanka == null || hedefKasa == null)
            {
                ModelState.AddModelError("", "Kaynak banka veya hedef kasa bulunamadı.");
                TempData["ErrorMessage"] = "Kaynak banka veya hedef kasa bulunamadı.";
                return View(model);
            }
            
            if (kaynakBanka.GuncelBakiye < model.KaynakTutar)
            {
                ModelState.AddModelError("", "Kaynak bankadaki bakiye yetersiz.");
                TempData["ErrorMessage"] = "Kaynak bankadaki bakiye yetersiz.";
                return View(model);
            }
            
            // Benzersiz transfer ID oluştur
            var transferID = Guid.NewGuid();
            
            // Referans no oluştur
            string referansNo = $"BKT-{DateTime.Now:yyMMddHHmmss}";
            
            // Kaynak bankadan çıkış hareketi
            var bankaHareket = new BankaHareket
            {
                BankaHareketID = Guid.NewGuid(),
                BankaID = kaynakBanka.BankaID,
                HedefKasaID = hedefKasa.KasaID,
                Tutar = model.KaynakTutar,
                HareketTuru = "Çıkış",
                Tarih = DateTime.Now,
                ReferansNo = referansNo,
                ReferansTuru = "KasaTransfer",
                DekontNo = $"DKN-{DateTime.Now:yyMMddHHmmss}",
                Aciklama = model.Aciklama ?? $"{kaynakBanka.BankaAdi}'dan {hedefKasa.KasaAdi}'ya transfer",
                KarsiUnvan = hedefKasa.KasaAdi,
                IslemYapanKullaniciID = GetCurrentUserId(),
                OlusturmaTarihi = DateTime.Now,
                SoftDelete = false,
                TransferID = transferID
            };
            
            // Hedef kasaya giriş hareketi
            var kasaHareket = new KasaHareket
            {
                KasaHareketID = Guid.NewGuid(),
                KasaID = hedefKasa.KasaID,
                KaynakBankaID = kaynakBanka.BankaID,
                Tutar = model.HedefTutar,
                HareketTuru = "Giriş",
                IslemTuru = "BankaTransfer",
                Tarih = DateTime.Now,
                ReferansNo = referansNo,
                ReferansTuru = "BankaTransfer",
                Aciklama = model.Aciklama ?? $"{kaynakBanka.BankaAdi}'dan {hedefKasa.KasaAdi}'ya transfer",
                IslemYapanKullaniciID = GetCurrentUserId(),
                OlusturmaTarihi = DateTime.Now,
                SoftDelete = false,
                DovizKuru = model.KurDegeri,
                KarsiParaBirimi = kaynakBanka.ParaBirimi,
                TransferID = transferID
            };
            
            // Bakiyeleri güncelle
            kaynakBanka.GuncelBakiye -= model.KaynakTutar;
            hedefKasa.GuncelBakiye += model.HedefTutar;
            
            // Veritabanına kaydet
            _context.BankaHareketleri.Add(bankaHareket);
            _context.KasaHareketleri.Add(kasaHareket);
            _context.Update(kaynakBanka);
            _context.Update(hedefKasa);
            
            await _context.SaveChangesAsync();
            
            TempData["SuccessMessage"] = "Bankadan kasaya transfer işlemi başarıyla tamamlandı.";
            return RedirectToAction(nameof(Index));
        }

        // GET: Kasa/DeleteTransfer/{id}
        public async Task<IActionResult> DeleteTransfer(Guid? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var hareketler = await _context.KasaHareketleri
                .Where(h => h.TransferID == id && !h.SoftDelete)
                .ToListAsync();

            if (hareketler == null || hareketler.Count == 0)
            {
                return NotFound();
            }

            return View(hareketler);
        }

        // POST: Kasa/DeleteTransfer/{id}
        [HttpPost, ActionName("DeleteTransfer")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteTransferConfirmed(Guid id)
        {
            var hareketler = await _context.KasaHareketleri
                .Where(h => h.TransferID == id && !h.SoftDelete)
                .ToListAsync();

            if (hareketler == null || hareketler.Count == 0)
            {
                return NotFound();
            }

            // Her hareket için bakiyeleri geri al
            foreach (var hareket in hareketler)
            {
                var kasa = await _context.Kasalar.FindAsync(hareket.KasaID);
                if (kasa != null)
                {
                    if (hareket.HareketTuru == "Giriş")
                    {
                        kasa.GuncelBakiye -= hareket.Tutar;
                    }
                    else if (hareket.HareketTuru == "Çıkış")
                    {
                        kasa.GuncelBakiye += hareket.Tutar;
                    }
                    _context.Update(kasa);
                }

                hareket.SoftDelete = true;
                _context.Update(hareket);
            }

            await _context.SaveChangesAsync();
            TempData["SuccessMessage"] = "Transfer işlemi başarıyla silindi.";
            return RedirectToAction(nameof(Index));
        }

        // GET: Kasa/EditTransfer/{id}
        public async Task<IActionResult> EditTransfer(Guid? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var hareketler = await _context.KasaHareketleri
                .Where(h => h.TransferID == id && !h.SoftDelete)
                .ToListAsync();

            if (hareketler == null || hareketler.Count != 2)
            {
                return NotFound();
            }

            var kaynakHareket = hareketler.FirstOrDefault(h => h.HareketTuru == "Çıkış");
            var hedefHareket = hareketler.FirstOrDefault(h => h.HareketTuru == "Giriş");

            if (kaynakHareket == null || hedefHareket == null)
            {
                return NotFound();
            }

            var model = new KasaTransferViewModel
            {
                TransferID = id.Value,
                KaynakKasaID = kaynakHareket.KasaID,
                HedefKasaID = hedefHareket.KasaID,
                KaynakTutar = kaynakHareket.Tutar,
                HedefTutar = hedefHareket.Tutar,
                KurDegeri = kaynakHareket.DovizKuru,
                Aciklama = kaynakHareket.Aciklama
            };

            var kasalar = await _context.Kasalar
                .Where(k => !k.SoftDelete && k.Aktif)
                .OrderBy(k => k.KasaAdi)
                .ToListAsync();

            var paraBirimleri = await _context.ParaBirimleri
                .Where(p => p.Aktif)
                .Select(p => p.Kod)
                .ToListAsync();

            ViewBag.Kasalar = kasalar;
            ViewBag.ParaBirimleri = paraBirimleri;

            return View(model);
        }

        // POST: Kasa/EditTransfer/{id}
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditTransfer(Guid id, KasaTransferViewModel model)
        {
            if (id != model.TransferID)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    var hareketler = await _context.KasaHareketleri
                        .Where(h => h.TransferID == id && !h.SoftDelete)
                        .ToListAsync();

                    if (hareketler == null || hareketler.Count != 2)
                    {
                        return NotFound();
                    }

                    var kaynakHareket = hareketler.FirstOrDefault(h => h.HareketTuru == "Çıkış");
                    var hedefHareket = hareketler.FirstOrDefault(h => h.HareketTuru == "Giriş");

                    if (kaynakHareket == null || hedefHareket == null)
                    {
                        return NotFound();
                    }

                    // Eski bakiyeleri geri al
                    var eskiKaynakKasa = await _context.Kasalar.FindAsync(kaynakHareket.KasaID);
                    var eskiHedefKasa = await _context.Kasalar.FindAsync(hedefHareket.KasaID);

                    if (eskiKaynakKasa != null)
                    {
                        eskiKaynakKasa.GuncelBakiye += kaynakHareket.Tutar;
                    }

                    if (eskiHedefKasa != null)
                    {
                        eskiHedefKasa.GuncelBakiye -= hedefHareket.Tutar;
                    }

                    // Yeni kasaları bul
                    var yeniKaynakKasa = await _context.Kasalar.FindAsync(model.KaynakKasaID);
                    var yeniHedefKasa = await _context.Kasalar.FindAsync(model.HedefKasaID);

                    if (yeniKaynakKasa == null || yeniHedefKasa == null)
                    {
                        ModelState.AddModelError("", "Kaynak veya hedef kasa bulunamadı.");
                        return View(model);
                    }

                    // Aynı kasa kontrolü
                    if (yeniKaynakKasa.KasaID == yeniHedefKasa.KasaID)
                    {
                        ModelState.AddModelError("", "Aynı kasaya transfer yapamazsınız.");
                        return View(model);
                    }

                    // Bakiye kontrolü
                    if (yeniKaynakKasa.GuncelBakiye < model.KaynakTutar)
                    {
                        ModelState.AddModelError("", "Kaynak kasadaki bakiye yetersiz.");
                        return View(model);
                    }

                    // Hareketleri güncelle
                    kaynakHareket.KasaID = model.KaynakKasaID;
                    kaynakHareket.HedefKasaID = model.HedefKasaID;
                    kaynakHareket.Tutar = model.KaynakTutar;
                    kaynakHareket.Aciklama = model.Aciklama;
                    kaynakHareket.DovizKuru = model.KurDegeri;
                    kaynakHareket.KarsiParaBirimi = yeniHedefKasa.ParaBirimi;

                    hedefHareket.KasaID = model.HedefKasaID;
                    hedefHareket.HedefKasaID = model.KaynakKasaID;
                    hedefHareket.Tutar = model.HedefTutar;
                    hedefHareket.Aciklama = model.Aciklama;
                    hedefHareket.DovizKuru = 1 / model.KurDegeri;
                    hedefHareket.KarsiParaBirimi = yeniKaynakKasa.ParaBirimi;

                    // Yeni bakiyeleri güncelle
                    yeniKaynakKasa.GuncelBakiye -= model.KaynakTutar;
                    yeniHedefKasa.GuncelBakiye += model.HedefTutar;

                    // Veritabanını güncelle
                    _context.Update(kaynakHareket);
                    _context.Update(hedefHareket);
                    
                    if (eskiKaynakKasa != null) _context.Update(eskiKaynakKasa);
                    if (eskiHedefKasa != null) _context.Update(eskiHedefKasa);
                    
                    _context.Update(yeniKaynakKasa);
                    _context.Update(yeniHedefKasa);

                    await _context.SaveChangesAsync();

                    TempData["SuccessMessage"] = "Transfer işlemi başarıyla güncellendi.";
                    return RedirectToAction(nameof(Index));
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError("", "Transfer güncellenirken bir hata oluştu: " + ex.Message);
                }
            }

            var kasalar = await _context.Kasalar
                .Where(k => !k.SoftDelete && k.Aktif)
                .OrderBy(k => k.KasaAdi)
                .ToListAsync();

            var paraBirimleri = await _context.ParaBirimleri
                .Where(p => p.Aktif)
                .Select(p => p.Kod)
                .ToListAsync();

            ViewBag.Kasalar = kasalar;
            ViewBag.ParaBirimleri = paraBirimleri;

            return View(model);
        }

        // API: Kasa/GetKasaParaBirimi/{id}
        [HttpGet]
        public async Task<IActionResult> GetKasaParaBirimi(Guid id)
        {
            var kasa = await _context.Kasalar
                .FirstOrDefaultAsync(k => k.KasaID == id && !k.SoftDelete);
                
            if (kasa == null)
            {
                return NotFound(new { message = "Kasa bulunamadı." });
            }
            
            return Json(new { parabirimi = kasa.ParaBirimi });
        }

        // API: Kasa/GetDovizKuru
        [HttpGet]
        public async Task<IActionResult> GetDovizKuru(string kaynakParaBirimi, string hedefParaBirimi)
        {
            try
            {
                if (string.IsNullOrEmpty(kaynakParaBirimi) || string.IsNullOrEmpty(hedefParaBirimi))
                    return Json(new { success = false, message = "Para birimi bilgileri eksik." });

                if (kaynakParaBirimi == hedefParaBirimi)
                    return Json(new { success = true, kur = 1.0M });

                // Para birimlerini bul
                var kaynakPB = await _context.ParaBirimleri
                    .FirstOrDefaultAsync(p => p.Kod == kaynakParaBirimi && p.Aktif);

                var hedefPB = await _context.ParaBirimleri
                    .FirstOrDefaultAsync(p => p.Kod == hedefParaBirimi && p.Aktif);

                if (kaynakPB == null || hedefPB == null)
                    return Json(new { success = false, message = "Para birimlerinden biri veya her ikisi de bulunamadı." });

                // Döviz ilişkisini kontrol et
                var dovizIliski = await _context.DovizIliskileri
                    .FirstOrDefaultAsync(di => 
                        di.KaynakParaBirimiID == kaynakPB.ParaBirimiID && 
                        di.HedefParaBirimiID == hedefPB.ParaBirimiID && 
                        di.Aktif);

                decimal kur = 0;

                if (dovizIliski != null)
                {
                    // Kur değerini bul
                    var kurDegeri = await _context.KurDegerleri
                        .Where(k => k.ParaBirimiID == kaynakPB.ParaBirimiID && k.Aktif)
                        .OrderByDescending(k => k.Tarih)
                        .FirstOrDefaultAsync();

                    if (kurDegeri != null)
                    {
                        // Kur değeri bulundu, alış ve satış değerlerinin ortalamasını al
                        kur = (kurDegeri.AlisDegeri + kurDegeri.SatisDegeri) / 2;
                        return Json(new { success = true, kur = kur });
                    }
                }

                // Ters ilişkiyi kontrol et
                var tersDovizIliski = await _context.DovizIliskileri
                    .FirstOrDefaultAsync(di => 
                        di.KaynakParaBirimiID == hedefPB.ParaBirimiID && 
                        di.HedefParaBirimiID == kaynakPB.ParaBirimiID && 
                        di.Aktif);

                if (tersDovizIliski != null)
                {
                    // Kur değerini bul
                    var kurDegeri = await _context.KurDegerleri
                        .Where(k => k.ParaBirimiID == hedefPB.ParaBirimiID && k.Aktif)
                        .OrderByDescending(k => k.Tarih)
                        .FirstOrDefaultAsync();

                    if (kurDegeri != null)
                    {
                        // Ters kur değeri bulundu, alış ve satış değerlerinin ortalamasını al ve tersini hesapla
                        kur = 1 / ((kurDegeri.AlisDegeri + kurDegeri.SatisDegeri) / 2);
                        return Json(new { success = true, kur = kur });
                    }
                }

                // Hiçbir kur değeri bulunamadı
                return Json(new { success = false, message = "Belirtilen para birimleri arasında kur bilgisi bulunamadı." });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        private bool KasaExists(Guid id)
        {
            return _context.Kasalar.Any(e => e.KasaID == id && !e.SoftDelete);
        }

        private Guid? GetCurrentUserId()
        {
            // Eğer kimlik doğrulama sistemi varsa, mevcut kullanıcının ID'sini alın
            // Şimdilik null dönelim
            return null;
        }
    }
} 