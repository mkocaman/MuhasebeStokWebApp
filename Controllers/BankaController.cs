using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using MuhasebeStokWebApp.Data;
using MuhasebeStokWebApp.Data.Entities;
using MuhasebeStokWebApp.Data.Repositories;
using System.Security.Claims;
using MuhasebeStokWebApp.ViewModels.Banka;
using MuhasebeStokWebApp.Services;
using MuhasebeStokWebApp.Models;
using MuhasebeStokWebApp.Services.Menu;
using MuhasebeStokWebApp.Services.Interfaces;

namespace MuhasebeStokWebApp.Controllers
{
    [Authorize]
    public class BankaController : BaseController
    {
        private readonly ApplicationDbContext _context;
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<BankaController> _logger;
        private new readonly ILogService _logService;

        public BankaController(
            ApplicationDbContext context, 
            IUnitOfWork unitOfWork,
            IMenuService menuService,
            UserManager<ApplicationUser> userManager,
            RoleManager<IdentityRole> roleManager,
            ILogService logService,
            ILogger<BankaController> logger)
            : base(menuService, userManager, roleManager, logService)
        {
            _context = context;
            _unitOfWork = unitOfWork;
            _logger = logger;
            _logService = logService;
        }

        // GET: Banka
        public async Task<IActionResult> Index()
        {
            var bankalar = await _context.Bankalar
                .Where(b => !b.Silindi)
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
                .FirstOrDefaultAsync(b => b.BankaID == id && !b.Silindi);

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
                BankaAdi = string.Empty,
                SubeAdi = string.Empty,
                SubeKodu = string.Empty,
                HesapNo = string.Empty,
                IBAN = string.Empty,
                ParaBirimi = "TRY", // varsayılan olarak TL
                Aciklama = string.Empty
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
                    Silindi = false
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
                        Silindi = false
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
                .FirstOrDefaultAsync(b => b.BankaID == id && !b.Silindi);

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
                        .FirstOrDefaultAsync(b => b.BankaID == id && !b.Silindi);

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
                            Silindi = false
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
                .FirstOrDefaultAsync(b => b.BankaID == id && !b.Silindi);

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
                .FirstOrDefaultAsync(b => b.BankaID == id && !b.Silindi);

            if (banka == null)
            {
                return NotFound();
            }

            // Soft delete işlemi
            banka.Silindi = true;
            banka.SonGuncelleyenKullaniciID = GetCurrentUserId();
            banka.GuncellemeTarihi = DateTime.Now;
            
            _context.Update(banka);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Banka hesabı başarıyla silindi.";
            return RedirectToAction(nameof(Index));
        }

        // GET: Banka/Hareketler/5
        public async Task<IActionResult> Hareketler(Guid? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var banka = await _context.Bankalar
                .FirstOrDefaultAsync(b => b.BankaID == id && !b.Silindi);

            if (banka == null)
            {
                return NotFound();
            }

            var hareketler = await _context.BankaHareketleri
                .Where(h => h.BankaID == id && !h.Silindi)
                .OrderByDescending(h => h.Tarih)
                .Include(h => h.Cari)
                .Select(h => new BankaHareketViewModel
                {
                    BankaHareketID = h.BankaHareketID,
                    BankaID = h.BankaID,
                    BankaAdi = banka.BankaAdi,
                    Tutar = h.Tutar,
                    HareketTuru = h.HareketTuru,
                    Tarih = h.Tarih,
                    ReferansNo = h.ReferansNo,
                    ReferansTuru = h.ReferansTuru,
                    Aciklama = h.Aciklama,
                    DekontNo = h.DekontNo,
                    KarsiUnvan = h.KarsiUnvan,
                    KarsiBankaAdi = h.KarsiBankaAdi,
                    KarsiIBAN = h.KarsiIBAN,
                    CariID = h.CariID,
                    CariAdi = h.Cari != null ? h.Cari.Ad : null
                })
                .ToListAsync();

            ViewBag.Banka = new BankaViewModel
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
                Aciklama = banka.Aciklama
            };

            return View(hareketler);
        }

        // GET: Banka/YeniHareket
        public async Task<IActionResult> YeniHareket(Guid? id = null, Guid? cariId = null)
        {
            try
            {
                var bankalar = await _context.Bankalar
                    .Where(b => !b.Silindi && b.Aktif)
                    .OrderBy(b => b.BankaAdi)
                    .ToListAsync();
                
                if (!bankalar.Any())
                {
                    TempData["ErrorMessage"] = "Sistemde kayıtlı aktif banka hesabı bulunamadı. Lütfen önce bir banka hesabı ekleyin.";
                    return RedirectToAction(nameof(Index));
                }
                
                var cariler = await _context.Cariler
                    .Where(c => !c.Silindi && c.AktifMi)
                    .OrderBy(c => c.Ad)
                    .ToListAsync();

                ViewBag.Bankalar = new SelectList(bankalar, "BankaID", "BankaAdi");
                ViewBag.Cariler = new SelectList(cariler, "CariID", "Ad");
                ViewBag.HareketTurleri = new List<string> 
                { 
                    "Para Yatırma", 
                    "Para Çekme", 
                    "EFT Gönderme", 
                    "EFT Alma", 
                    "Havale Gönderme", 
                    "Havale Alma" 
                };

                // Model oluştur
                var model = new BankaHareket
                {
                    Tarih = DateTime.Now,
                    ReferansNo = "REF-" + DateTime.Now.ToString("yyyyMMddHHmmss")
                };
                
                // Banka ID varsa
                if (id.HasValue)
                {
                    var banka = await _context.Bankalar.FirstOrDefaultAsync(b => b.BankaID == id && !b.Silindi);
                    if (banka != null)
                    {
                        model.BankaID = banka.BankaID;
                        ViewBag.SecilenBanka = banka;
                    }
                }
                
                // Cari ID varsa
                if (cariId.HasValue)
                {
                    var cari = await _context.Cariler.FirstOrDefaultAsync(c => c.CariID == cariId && !c.Silindi);
                    if (cari != null)
                    {
                        model.CariID = cari.CariID;
                        ViewBag.SecilenCari = cari;
                    }
                }
                
                return View(model);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Banka hareketi ekleme sayfası yüklenirken hata oluştu.");
                TempData["ErrorMessage"] = "Banka hareketi ekleme sayfası yüklenirken bir hata oluştu.";
                return RedirectToAction(nameof(Index));
            }
        }

        // POST: Banka/YeniHareket
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> YeniHareket(BankaHareket hareket)
        {
            if (ModelState.IsValid)
            {
                var banka = await _context.Bankalar
                    .FirstOrDefaultAsync(b => b.BankaID == hareket.BankaID && !b.Silindi);

                if (banka == null)
                {
                    return NotFound();
                }

                hareket.BankaHareketID = Guid.NewGuid();
                hareket.Tarih = DateTime.Now;
                hareket.IslemYapanKullaniciID = GetCurrentUserId();
                hareket.OlusturmaTarihi = DateTime.Now;
                hareket.Silindi = false;
                
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
                .Where(b => !b.Silindi && b.Aktif)
                .OrderBy(b => b.BankaAdi)
                .ToListAsync();
            
            var cariler = await _context.Cariler
                .Where(c => !c.Silindi && c.AktifMi)
                .OrderBy(c => c.Ad)
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
            return _context.Bankalar.Any(e => e.BankaID == id && !e.Silindi);
        }

        // Kullanıcı ID'sini session'dan alır
        private new Guid? GetCurrentUserId()
        {
            string userId = HttpContext.Session.GetString("UserId");
            if (!string.IsNullOrEmpty(userId))
            {
                if (Guid.TryParse(userId, out Guid parsedId))
                    return parsedId;
            }
            return null;
        }
    }
} 