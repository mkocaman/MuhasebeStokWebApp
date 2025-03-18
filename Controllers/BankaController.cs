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
using MuhasebeStokWebApp.ViewModels.Banka;

namespace MuhasebeStokWebApp.Controllers
{
    // [Authorize] attribute'ü kaldırıldı - geliştirme sürecinde geçici olarak
    public class BankaController : Controller
    {
        private readonly ApplicationDbContext _context;

        public BankaController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Banka
        public async Task<IActionResult> Index()
        {
            var bankalar = await _context.Bankalar
                .Where(b => !b.SoftDelete)
                .OrderByDescending(b => b.OlusturmaTarihi)
                .Select(b => new BankaViewModel
                {
                    BankaID = b.BankaID,
                    BankaAdi = b.BankaAdi,
                    SubeAdi = b.SubeAdi,
                    SubeKodu = b.SubeKodu,
                    HesapNo = b.HesapNo,
                    IBAN = b.IBAN,
                    ParaBirimi = b.ParaBirimi,
                    AcilisBakiye = b.AcilisBakiye,
                    GuncelBakiye = b.GuncelBakiye,
                    Aciklama = b.Aciklama,
                    Aktif = b.Aktif,
                    OlusturmaTarihi = b.OlusturmaTarihi
                })
                .ToListAsync();

            // View doğrudan bankalar listesini bekliyor, BankaListViewModel değil
            return View(bankalar);
        }

        // GET: Banka/Details/5
        public async Task<IActionResult> Details(Guid? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var banka = await _context.Bankalar
                .FirstOrDefaultAsync(b => b.BankaID == id && !b.SoftDelete);

            if (banka == null)
            {
                return NotFound();
            }

            var model = new BankaViewModel
            {
                BankaID = banka.BankaID,
                BankaAdi = banka.BankaAdi,
                SubeAdi = banka.SubeAdi,
                SubeKodu = banka.SubeKodu,
                HesapNo = banka.HesapNo,
                IBAN = banka.IBAN,
                ParaBirimi = banka.ParaBirimi,
                AcilisBakiye = banka.AcilisBakiye,
                GuncelBakiye = banka.GuncelBakiye,
                Aciklama = banka.Aciklama,
                Aktif = banka.Aktif,
                OlusturmaTarihi = banka.OlusturmaTarihi
            };

            return View(model);
        }

        // GET: Banka/Create
        public IActionResult Create()
        {
            return View(new BankaCreateViewModel
            {
                ParaBirimi = "TRY" // varsayılan olarak TL
            });
        }

        // POST: Banka/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(BankaCreateViewModel model)
        {
            if (ModelState.IsValid)
            {
                var banka = new Banka
                {
                    BankaID = Guid.NewGuid(),
                    BankaAdi = model.BankaAdi,
                    SubeAdi = model.SubeAdi,
                    SubeKodu = model.SubeKodu,
                    HesapNo = model.HesapNo,
                    IBAN = model.IBAN,
                    ParaBirimi = model.ParaBirimi,
                    AcilisBakiye = model.AcilisBakiye,
                    GuncelBakiye = model.AcilisBakiye, // Başlangıçta güncel bakiye açılış bakiyesi ile aynı olur
                    Aciklama = model.Aciklama,
                    Aktif = model.Aktif,
                    OlusturanKullaniciID = GetCurrentUserId(),
                    OlusturmaTarihi = DateTime.Now,
                    SoftDelete = false
                };

                _context.Add(banka);
                await _context.SaveChangesAsync();

                // Açılış bakiyesi 0'dan farklı ise, bir banka hareketine kaydedelim
                if (model.AcilisBakiye > 0)
                {
                    var bankaHareket = new BankaHareket
                    {
                        BankaHareketID = Guid.NewGuid(),
                        BankaID = banka.BankaID,
                        Tutar = model.AcilisBakiye,
                        HareketTuru = "Açılış Bakiyesi",
                        Tarih = DateTime.Now,
                        ReferansNo = "ACILIS-" + DateTime.Now.ToString("yyyyMMddHHmmss"),
                        ReferansTuru = "Açılış",
                        Aciklama = "Banka hesap açılış bakiyesi",
                        IslemYapanKullaniciID = GetCurrentUserId(),
                        OlusturmaTarihi = DateTime.Now,
                        SoftDelete = false
                    };

                    _context.Add(bankaHareket);
                    await _context.SaveChangesAsync();
                }

                TempData["SuccessMessage"] = "Banka hesabı başarıyla oluşturuldu.";
                return RedirectToAction(nameof(Index));
            }

            return View(model);
        }

        // GET: Banka/Edit/5
        public async Task<IActionResult> Edit(Guid? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var banka = await _context.Bankalar
                .FirstOrDefaultAsync(b => b.BankaID == id && !b.SoftDelete);

            if (banka == null)
            {
                return NotFound();
            }

            var model = new BankaEditViewModel
            {
                BankaID = banka.BankaID,
                BankaAdi = banka.BankaAdi,
                SubeAdi = banka.SubeAdi,
                SubeKodu = banka.SubeKodu,
                HesapNo = banka.HesapNo,
                IBAN = banka.IBAN,
                ParaBirimi = banka.ParaBirimi,
                AcilisBakiye = banka.AcilisBakiye,
                GuncelBakiye = banka.GuncelBakiye,
                Aciklama = banka.Aciklama,
                Aktif = banka.Aktif
            };

            return View(model);
        }

        // POST: Banka/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(Guid id, BankaEditViewModel model)
        {
            if (id != model.BankaID)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    var banka = await _context.Bankalar
                        .FirstOrDefaultAsync(b => b.BankaID == id && !b.SoftDelete);

                    if (banka == null)
                    {
                        return NotFound();
                    }

                    // Açılış bakiyesi değiştiyse, güncel bakiyeyi de güncelle
                    decimal bakiyeFarki = model.AcilisBakiye - banka.AcilisBakiye;
                    banka.GuncelBakiye += bakiyeFarki;

                    banka.BankaAdi = model.BankaAdi;
                    banka.SubeAdi = model.SubeAdi;
                    banka.SubeKodu = model.SubeKodu;
                    banka.HesapNo = model.HesapNo;
                    banka.IBAN = model.IBAN;
                    banka.ParaBirimi = model.ParaBirimi;
                    banka.AcilisBakiye = model.AcilisBakiye;
                    banka.Aciklama = model.Aciklama;
                    banka.Aktif = model.Aktif;
                    banka.SonGuncelleyenKullaniciID = GetCurrentUserId();
                    banka.GuncellemeTarihi = DateTime.Now;

                    _context.Update(banka);
                    await _context.SaveChangesAsync();

                    // Açılış bakiyesi değiştiyse, bir banka hareketi ekleyelim
                    if (bakiyeFarki != 0)
                    {
                        var bankaHareket = new BankaHareket
                        {
                            BankaHareketID = Guid.NewGuid(),
                            BankaID = banka.BankaID,
                            Tutar = Math.Abs(bakiyeFarki),
                            HareketTuru = bakiyeFarki > 0 ? "Bakiye Artışı" : "Bakiye Azalışı",
                            Tarih = DateTime.Now,
                            ReferansNo = "DUZENLE-" + DateTime.Now.ToString("yyyyMMddHHmmss"),
                            ReferansTuru = "Düzenleme",
                            Aciklama = "Banka hesap açılış bakiyesi düzeltme",
                            IslemYapanKullaniciID = GetCurrentUserId(),
                            OlusturmaTarihi = DateTime.Now,
                            SoftDelete = false
                        };

                        _context.Add(bankaHareket);
                        await _context.SaveChangesAsync();
                    }

                    TempData["SuccessMessage"] = "Banka hesabı başarıyla güncellendi.";
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!BankaExists(model.BankaID))
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

        // GET: Banka/Delete/5
        public async Task<IActionResult> Delete(Guid? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var banka = await _context.Bankalar
                .FirstOrDefaultAsync(b => b.BankaID == id && !b.SoftDelete);

            if (banka == null)
            {
                return NotFound();
            }

            var model = new BankaViewModel
            {
                BankaID = banka.BankaID,
                BankaAdi = banka.BankaAdi,
                SubeAdi = banka.SubeAdi,
                SubeKodu = banka.SubeKodu,
                HesapNo = banka.HesapNo,
                IBAN = banka.IBAN,
                ParaBirimi = banka.ParaBirimi,
                AcilisBakiye = banka.AcilisBakiye,
                GuncelBakiye = banka.GuncelBakiye,
                Aciklama = banka.Aciklama,
                Aktif = banka.Aktif,
                OlusturmaTarihi = banka.OlusturmaTarihi
            };

            return View(model);
        }

        // POST: Banka/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(Guid id)
        {
            var banka = await _context.Bankalar
                .FirstOrDefaultAsync(b => b.BankaID == id && !b.SoftDelete);

            if (banka == null)
            {
                return NotFound();
            }

            // Silme işlemi yerine pasife alma işlemi
            banka.Aktif = false;
            banka.SonGuncelleyenKullaniciID = GetCurrentUserId();
            banka.GuncellemeTarihi = DateTime.Now;
            
            _context.Update(banka);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Banka hesabı başarıyla pasife alındı.";
            return RedirectToAction(nameof(Index));
        }

        // GET: Banka/Hareketler/5
        public async Task<IActionResult> Hareketler(Guid id, DateTime? baslangicTarihi = null, DateTime? bitisTarihi = null, string? hareketTuru = null)
        {
            var banka = await _context.Bankalar
                .FirstOrDefaultAsync(b => b.BankaID == id && !b.SoftDelete);
            if (banka == null)
            {
                return NotFound();
            }

            var viewModel = new BankaHareketlerViewModel
            {
                BankaID = banka.BankaID,
                BankaAdi = banka.BankaAdi,
                SubeAdi = banka.SubeAdi,
                SubeKodu = banka.SubeKodu,
                HesapNo = banka.HesapNo,
                IBAN = banka.IBAN,
                ParaBirimi = banka.ParaBirimi,
                AcilisBakiye = banka.AcilisBakiye,
                GuncelBakiye = banka.GuncelBakiye,
                BaslangicTarihi = baslangicTarihi ?? DateTime.Now.AddMonths(-1),
                BitisTarihi = bitisTarihi ?? DateTime.Now,
                Hareketler = new List<BankaHareketViewModel>()
            };

            var query = _context.BankaHareketleri
                .Include(h => h.Cari)
                .Where(h => h.BankaID == id && !h.SoftDelete);

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
                var hareketViewModel = new BankaHareketViewModel
                {
                    BankaHareketID = hareket.BankaHareketID,
                    BankaID = hareket.BankaID,
                    BankaAdi = banka.BankaAdi,
                    Tutar = hareket.Tutar,
                    HareketTuru = hareket.HareketTuru,
                    Tarih = hareket.Tarih,
                    ReferansNo = hareket.ReferansNo,
                    ReferansTuru = hareket.ReferansTuru,
                    DekontNo = hareket.DekontNo,
                    Aciklama = hareket.Aciklama,
                    KarsiUnvan = hareket.KarsiUnvan,
                    KarsiBankaAdi = hareket.KarsiBankaAdi,
                    KarsiIBAN = hareket.KarsiIBAN,
                    CariID = hareket.CariID,
                    CariAdi = hareket.Cari?.CariAdi,
                    ParaBirimi = banka.ParaBirimi
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

        // GET: Banka/YeniHareket
        public async Task<IActionResult> YeniHareket()
        {
            var bankalar = await _context.Bankalar
                .Where(b => !b.SoftDelete && b.Aktif)
                .OrderBy(b => b.BankaAdi)
                .ToListAsync();
            
            var cariler = await _context.Cariler
                .Where(c => !c.SoftDelete && c.Aktif)
                .OrderBy(c => c.CariAdi)
                .ToListAsync();

            ViewBag.Bankalar = bankalar;
            ViewBag.Cariler = cariler;
            ViewBag.HareketTurleri = new List<string> 
            { 
                "Para Yatırma", 
                "Para Çekme", 
                "EFT Gönderme", 
                "EFT Alma", 
                "Havale Gönderme", 
                "Havale Alma" 
            };

            return View();
        }

        // POST: Banka/YeniHareket
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> YeniHareket(BankaHareket hareket)
        {
            if (ModelState.IsValid)
            {
                var banka = await _context.Bankalar
                    .FirstOrDefaultAsync(b => b.BankaID == hareket.BankaID && !b.SoftDelete);

                if (banka == null)
                {
                    return NotFound();
                }

                hareket.BankaHareketID = Guid.NewGuid();
                hareket.Tarih = DateTime.Now;
                hareket.IslemYapanKullaniciID = GetCurrentUserId();
                hareket.OlusturmaTarihi = DateTime.Now;
                hareket.SoftDelete = false;
                
                // ReferansTuru alanını HareketTuru ile aynı yapalım
                hareket.ReferansTuru = hareket.HareketTuru;

                _context.Add(hareket);

                // Banka bakiyesini güncelle
                switch (hareket.HareketTuru)
                {
                    case "Para Yatırma":
                    case "EFT Alma":
                    case "Havale Alma":
                        banka.GuncelBakiye += hareket.Tutar;
                        break;
                    case "Para Çekme":
                    case "EFT Gönderme":
                    case "Havale Gönderme":
                        banka.GuncelBakiye -= hareket.Tutar;
                        break;
                }

                _context.Update(banka);
                await _context.SaveChangesAsync();

                TempData["SuccessMessage"] = "Banka hareketi başarıyla eklendi.";
                return RedirectToAction(nameof(Hareketler), new { id = hareket.BankaID });
            }

            var bankalar = await _context.Bankalar
                .Where(b => !b.SoftDelete && b.Aktif)
                .OrderBy(b => b.BankaAdi)
                .ToListAsync();
            
            var cariler = await _context.Cariler
                .Where(c => !c.SoftDelete && c.Aktif)
                .OrderBy(c => c.CariAdi)
                .ToListAsync();

            ViewBag.Bankalar = bankalar;
            ViewBag.Cariler = cariler;
            ViewBag.HareketTurleri = new List<string> 
            { 
                "Para Yatırma", 
                "Para Çekme", 
                "EFT Gönderme", 
                "EFT Alma", 
                "Havale Gönderme", 
                "Havale Alma" 
            };
            
            return View(hareket);
        }

        private bool BankaExists(Guid id)
        {
            return _context.Bankalar.Any(e => e.BankaID == id && !e.SoftDelete);
        }

        private Guid? GetCurrentUserId()
        {
            // Eğer kimlik doğrulama sistemi varsa, mevcut kullanıcının ID'sini alın
            // Şimdilik null dönelim
            return null;
        }
    }
} 