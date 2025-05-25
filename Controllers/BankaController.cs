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
using MuhasebeStokWebApp.ViewModels.Transfer;

namespace MuhasebeStokWebApp.Controllers
{
    [Authorize]
    public class BankaController : BaseController
    {
        private readonly ApplicationDbContext _context;
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<BankaController> _logger;
        private new readonly ILogService _logService;
        private readonly IDovizKuruService _dovizKuruService;
        private readonly IParaBirimiService _paraBirimiService;
        protected new readonly UserManager<ApplicationUser> _userManager;
        protected new readonly RoleManager<IdentityRole> _roleManager;
        private readonly ICariHareketService _cariHareketService;

        public BankaController(
            ApplicationDbContext context, 
            IUnitOfWork unitOfWork,
            IMenuService menuService,
            UserManager<ApplicationUser> userManager,
            RoleManager<IdentityRole> roleManager,
            ILogService logService,
            ILogger<BankaController> logger,
            IDovizKuruService dovizKuruService,
            IParaBirimiService paraBirimiService,
            ICariHareketService cariHareketService)
            : base(menuService, userManager, roleManager, logService)
        {
            _context = context;
            _unitOfWork = unitOfWork;
            _logger = logger;
            _logService = logService;
            _dovizKuruService = dovizKuruService;
            _paraBirimiService = paraBirimiService;
            _userManager = userManager;
            _roleManager = roleManager;
            _cariHareketService = cariHareketService;
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
                    Aktif = b.Aktif,
                    OlusturmaTarihi = b.OlusturmaTarihi
                })
                .ToListAsync();

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
                Aktif = banka.Aktif,
                OlusturmaTarihi = banka.OlusturmaTarihi
            };

            // Ayrıca bankanın hesaplarını da getir
            var hesaplar = await _context.BankaHesaplari
                .Where(h => h.BankaID == id && !h.Silindi)
                .Select(h => new BankaHesapViewModel
                {
                    BankaHesapID = h.BankaHesapID,
                    BankaID = h.BankaID,
                    BankaAdi = banka.BankaAdi,
                    HesapAdi = h.HesapAdi,
                    ParaBirimi = h.ParaBirimi,
                    GuncelBakiye = h.GuncelBakiye,
                    SubeAdi = h.SubeAdi,
                    SubeKodu = h.SubeKodu,
                    HesapNo = h.HesapNo,
                    IBAN = h.IBAN,
                    Aciklama = h.Aciklama,
                    Aktif = h.Aktif,
                    AcilisBakiye = h.AcilisBakiye,
                    OlusturmaTarihi = h.OlusturmaTarihi
                })
                .ToListAsync();

            ViewBag.Hesaplar = hesaplar;

            return View(model);
        }

        // GET: Banka/Create
        public IActionResult Create()
        {
            return View(new BankaCreateViewModel
            {
                BankaAdi = string.Empty,
                Aktif = true
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
                    Aktif = model.Aktif,
                    OlusturanKullaniciID = GetCurrentUserId(),
                    OlusturmaTarihi = DateTime.Now,
                    Silindi = false
                };

                _context.Add(banka);
                await _context.SaveChangesAsync();

                TempData["SuccessMessage"] = "Banka başarıyla oluşturuldu.";
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
                    var banka = await _context.Bankalar.FindAsync(id);

                    if (banka == null || banka.Silindi)
                    {
                        return NotFound();
                    }

                    banka.BankaAdi = model.BankaAdi;
                    banka.Aktif = model.Aktif;
                    banka.SonGuncelleyenKullaniciID = GetCurrentUserId();
                    banka.GuncellemeTarihi = DateTime.Now;

                    _context.Update(banka);
                    await _context.SaveChangesAsync();

                    TempData["SuccessMessage"] = "Banka başarıyla güncellendi.";
                    return RedirectToAction(nameof(Index));
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
            var banka = await _context.Bankalar.FindAsync(id);

            if (banka == null)
            {
                return NotFound();
            }

            // İlgili hesapları da kontrol et
            var hesaplar = await _context.BankaHesaplari
                .Where(h => h.BankaID == id && !h.Silindi)
                .ToListAsync();

            if (hesaplar.Any())
            {
                TempData["ErrorMessage"] = "Bu bankaya ait hesaplar bulunmaktadır. Önce hesapları silmelisiniz.";
                return RedirectToAction(nameof(Index));
            }

            // Soft delete
            banka.Silindi = true;
            banka.SonGuncelleyenKullaniciID = GetCurrentUserId();
            banka.GuncellemeTarihi = DateTime.Now;
            
            _context.Update(banka);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Banka başarıyla silindi.";
            return RedirectToAction(nameof(Index));
        }

        // GET: Banka/Hesaplar/5
        public async Task<IActionResult> Hesaplar(Guid? id)
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

            var hesaplar = await _context.BankaHesaplari
                .Where(h => h.BankaID == id && !h.Silindi)
                .OrderByDescending(h => h.OlusturmaTarihi)
                .Select(h => new BankaHesapViewModel
                {
                    BankaHesapID = h.BankaHesapID,
                    BankaID = h.BankaID,
                    BankaAdi = banka.BankaAdi,
                    HesapAdi = h.HesapAdi,
                    ParaBirimi = h.ParaBirimi,
                    GuncelBakiye = h.GuncelBakiye,
                    SubeAdi = h.SubeAdi,
                    SubeKodu = h.SubeKodu,
                    HesapNo = h.HesapNo,
                    IBAN = h.IBAN,
                    Aciklama = h.Aciklama,
                    Aktif = h.Aktif,
                    AcilisBakiye = h.AcilisBakiye,
                    OlusturmaTarihi = h.OlusturmaTarihi
                })
                .ToListAsync();

            var viewModel = new BankaHesapListViewModel
            {
                BankaHesaplari = hesaplar,
                ToplamBakiye = hesaplar.Sum(h => h.GuncelBakiye),
                ParaBirimiToplamlari = hesaplar
                    .GroupBy(h => h.ParaBirimi)
                    .ToDictionary(g => g.Key, g => g.Sum(h => h.GuncelBakiye))
            };

            ViewBag.BankaID = id;
            ViewBag.BankaAdi = banka.BankaAdi;

            return View(viewModel);
        }

        // GET: Banka/HesapCreate/5
        public async Task<IActionResult> HesapCreate(Guid? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var banka = await _context.Bankalar.FirstOrDefaultAsync(b => b.BankaID == id && !b.Silindi);
            if (banka == null)
            {
                return NotFound();
            }

            // Para birimi listesini veritabanından al
            ViewBag.ParaBirimleri = await _context.ParaBirimleri
                .Where(p => p.Aktif)
                .OrderBy(p => p.Sira)
                .Select(p => new SelectListItem { Value = p.Kod, Text = $"{p.Ad} ({p.Kod})" })
                .ToListAsync();

            var model = new BankaHesapCreateViewModel
            {
                BankaID = banka.BankaID,
                BankaAdi = banka.BankaAdi,
                HesapAdi = string.Empty,
                ParaBirimi = "TRY",
                Aktif = true,
                AcilisBakiye = 0
            };

            return View(model);
        }

        // POST: Banka/HesapCreate
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> HesapCreate(BankaHesapCreateViewModel viewModel)
        {
            if (ModelState.IsValid)
            {
                var banka = await _context.Bankalar.FindAsync(viewModel.BankaID);
                if (banka == null || banka.Silindi)
                {
                    return NotFound();
                }

                using var transaction = await _context.Database.BeginTransactionAsync();

                try
                {
                    // Yeni banka hesabı oluştur
                    var bankaHesap = new BankaHesap
                    {
                        BankaHesapID = Guid.NewGuid(),
                        BankaID = viewModel.BankaID,
                        HesapAdi = viewModel.HesapAdi,
                        SubeAdi = viewModel.SubeAdi,
                        SubeKodu = viewModel.SubeKodu,
                        HesapNo = viewModel.HesapNo,
                        IBAN = viewModel.IBAN?.Replace(" ", ""),
                        ParaBirimi = viewModel.ParaBirimi,
                        AcilisBakiye = viewModel.AcilisBakiye,
                        GuncelBakiye = viewModel.AcilisBakiye, // Başlangıçta açılış bakiyesi = güncel bakiye
                        Aciklama = viewModel.Aciklama,
                        Aktif = viewModel.Aktif,
                        OlusturmaTarihi = DateTime.Now,
                        OlusturanKullaniciID = GetCurrentUserId(),
                        Silindi = false
                    };

                    _context.Add(bankaHesap);
                    await _context.SaveChangesAsync();

                    // Açılış bakiyesi sıfırdan farklıysa, otomatik olarak açılış hareketi oluştur
                    if (viewModel.AcilisBakiye != 0)
                    {
                        var hareket = new BankaHesapHareket
                        {
                            BankaHesapHareketID = Guid.NewGuid(),
                            BankaHesapID = bankaHesap.BankaHesapID,
                            BankaID = bankaHesap.BankaID,
                            Tutar = Math.Abs(viewModel.AcilisBakiye),
                            HareketTuru = Enums.BankaHareketTipi.AcilisBakiyesi.ToString(),
                            Tarih = DateTime.Now,
                            ReferansNo = $"ACILIS-{DateTime.Now:yyyyMMddHHmmss}",
                            ReferansTuru = "Banka",
                            ReferansID = bankaHesap.BankaID,
                            Aciklama = $"{banka.BankaAdi} - {bankaHesap.HesapAdi} için açılış bakiyesi",
                            DekontNo = $"DKT-{DateTime.Now:yyMMdd}-{new Random().Next(1000):000}",
                            IslemYapanKullaniciID = GetCurrentUserId(),
                            OlusturmaTarihi = DateTime.Now,
                            GuncellemeTarihi = DateTime.Now,
                            SonGuncelleyenKullaniciID = GetCurrentUserId(),
                            KarsiParaBirimi = bankaHesap.ParaBirimi ?? "TRY",
                            Silindi = false
                        };

                        _context.BankaHesapHareketleri.Add(hareket);
                        await _context.SaveChangesAsync();
                    }

                    await transaction.CommitAsync();

                    await _logService.Log(
                        $"Yeni banka hesabı oluşturuldu: {banka.BankaAdi} - {bankaHesap.HesapAdi}, {bankaHesap.GuncelBakiye} {bankaHesap.ParaBirimi}",
                        Enums.LogTuru.Bilgi
                    );

                    return RedirectToAction(nameof(Hesaplar), new { id = viewModel.BankaID });
                }
                catch (Exception ex)
                {
                    await transaction.RollbackAsync();
                    _logger.LogError(ex, "Banka hesabı oluşturulurken hata oluştu");
                    ModelState.AddModelError("", "Banka hesabı oluşturulurken bir hata oluştu. Lütfen tekrar deneyin.");
                }
            }

            ViewBag.ParaBirimleri = await _context.ParaBirimleri
                .Where(p => p.Aktif)
                .OrderBy(p => p.Sira)
                .Select(p => new SelectListItem { Value = p.Kod, Text = $"{p.Ad} ({p.Kod})" })
                .ToListAsync();
            
            return View(viewModel);
        }

        // GET: Banka/HesapDetails/5
        public async Task<IActionResult> HesapDetails(Guid? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var bankaHesap = await _context.BankaHesaplari
                .Include(h => h.Banka)
                .FirstOrDefaultAsync(h => h.BankaHesapID == id && !h.Silindi);

            if (bankaHesap == null)
            {
                return NotFound();
            }

            var model = new BankaHesapViewModel
            {
                BankaHesapID = bankaHesap.BankaHesapID,
                BankaID = bankaHesap.BankaID,
                BankaAdi = bankaHesap.Banka?.BankaAdi ?? "Bilinmeyen Banka",
                HesapAdi = bankaHesap.HesapAdi,
                SubeAdi = bankaHesap.SubeAdi,
                SubeKodu = bankaHesap.SubeKodu,
                HesapNo = bankaHesap.HesapNo,
                IBAN = bankaHesap.IBAN,
                ParaBirimi = bankaHesap.ParaBirimi,
                AcilisBakiye = bankaHesap.AcilisBakiye,
                GuncelBakiye = bankaHesap.GuncelBakiye,
                Aciklama = bankaHesap.Aciklama,
                Aktif = bankaHesap.Aktif,
                OlusturmaTarihi = bankaHesap.OlusturmaTarihi
            };

            return View(model);
        }

        // GET: Banka/HesapEdit/5
        public async Task<IActionResult> HesapEdit(Guid? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var bankaHesap = await _context.BankaHesaplari
                .Include(h => h.Banka)
                .FirstOrDefaultAsync(h => h.BankaHesapID == id && !h.Silindi);
        
            if (bankaHesap == null)
            {
                return NotFound();
            }

            // Para birimi listesini veritabanından al
            ViewBag.ParaBirimleri = await _context.ParaBirimleri
                .Where(p => p.Aktif)
                .OrderBy(p => p.Sira)
                .Select(p => new SelectListItem { Value = p.Kod, Text = $"{p.Ad} ({p.Kod})" })
                .ToListAsync();

            var model = new BankaHesapEditViewModel
            {
                BankaHesapID = bankaHesap.BankaHesapID,
                BankaID = bankaHesap.BankaID,
                HesapAdi = bankaHesap.HesapAdi,
                SubeAdi = bankaHesap.SubeAdi,
                SubeKodu = bankaHesap.SubeKodu,
                ParaBirimi = bankaHesap.ParaBirimi,
                HesapNo = bankaHesap.HesapNo,
                IBAN = bankaHesap.IBAN,
                AcilisBakiye = bankaHesap.AcilisBakiye,
                Aciklama = bankaHesap.Aciklama,
                Aktif = bankaHesap.Aktif
            };

            return View(model);
        }

        // POST: Banka/HesapEdit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> HesapEdit(Guid id, BankaHesapEditViewModel model)
        {
            if (id != model.BankaHesapID)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    var bankaHesap = await _context.BankaHesaplari.FindAsync(id);
                    
                    if (bankaHesap == null || bankaHesap.Silindi)
                    {
                        return NotFound();
                    }

                    // Güncel bakiyedeki değişikliği takip etmek için eski değeri saklayalım
                    decimal eskiAcilisBakiye = bankaHesap.AcilisBakiye;
                    decimal fark = model.AcilisBakiye - eskiAcilisBakiye;

                    using var transaction = await _context.Database.BeginTransactionAsync();

                    try
                    {
                        // Hesap bilgilerini güncelle
                        bankaHesap.HesapAdi = model.HesapAdi;
                        bankaHesap.SubeAdi = model.SubeAdi;
                        bankaHesap.SubeKodu = model.SubeKodu;
                        bankaHesap.HesapNo = model.HesapNo;
                        bankaHesap.IBAN = model.IBAN;
                        bankaHesap.ParaBirimi = model.ParaBirimi;
                        bankaHesap.AcilisBakiye = model.AcilisBakiye;
                        bankaHesap.Aciklama = model.Aciklama;
                        bankaHesap.Aktif = model.Aktif;
                        bankaHesap.SonGuncelleyenKullaniciID = GetCurrentUserId();
                        bankaHesap.GuncellemeTarihi = DateTime.Now;
                        
                        // Açılış bakiyesi değiştiyse güncel bakiyeyi de güncelle
                        if (fark != 0)
                        {
                            bankaHesap.GuncelBakiye += fark;

                            // Açılış bakiyesi hareketini bul ve güncelle
                            var acilisBakiyesiHareket = await _context.BankaHesapHareketleri
                                .FirstOrDefaultAsync(h => h.BankaHesapID == bankaHesap.BankaHesapID && 
                                                     h.HareketTuru == Enums.BankaHareketTipi.AcilisBakiyesi.ToString() &&
                                                     !h.Silindi);

                            if (acilisBakiyesiHareket != null)
                            {
                                // Mevcut açılış bakiyesi hareketini güncelle
                                acilisBakiyesiHareket.Tutar = Math.Abs(model.AcilisBakiye);
                                acilisBakiyesiHareket.GuncellemeTarihi = DateTime.Now;
                                acilisBakiyesiHareket.SonGuncelleyenKullaniciID = GetCurrentUserId();
                                acilisBakiyesiHareket.KarsiParaBirimi = bankaHesap.ParaBirimi ?? "TRY";
                                // DekontNo null ise yeni değer ata
                                if (string.IsNullOrEmpty(acilisBakiyesiHareket.DekontNo))
                                {
                                    acilisBakiyesiHareket.DekontNo = $"DKT-{DateTime.Now:yyMMdd}-{new Random().Next(1000):000}";
                                }
                                _context.Update(acilisBakiyesiHareket);
                            }
                            else
                            {
                                // Hareket bulunamadıysa yeni açılış bakiyesi hareketi oluştur
                                var hareket = new BankaHesapHareket
                                {
                                    BankaHesapHareketID = Guid.NewGuid(),
                                    BankaHesapID = bankaHesap.BankaHesapID,
                                    BankaID = bankaHesap.BankaID,
                                    Tutar = Math.Abs(model.AcilisBakiye),
                                    HareketTuru = Enums.BankaHareketTipi.AcilisBakiyesi.ToString(),
                                    Tarih = DateTime.Now,
                                    ReferansNo = $"ACILIS-{DateTime.Now:yyyyMMddHHmmss}",
                                    ReferansTuru = "Banka",
                                    ReferansID = bankaHesap.BankaID,
                                    Aciklama = $"{bankaHesap.Banka?.BankaAdi} - {bankaHesap.HesapAdi} için açılış bakiyesi",
                                    DekontNo = $"DKT-{DateTime.Now:yyMMdd}-{new Random().Next(1000):000}",
                                    IslemYapanKullaniciID = GetCurrentUserId(),
                                    OlusturmaTarihi = DateTime.Now,
                                    GuncellemeTarihi = DateTime.Now,
                                    SonGuncelleyenKullaniciID = GetCurrentUserId(),
                                    KarsiParaBirimi = bankaHesap.ParaBirimi ?? "TRY",
                                    Silindi = false
                                };
                                _context.BankaHesapHareketleri.Add(hareket);
                            }
                        }

                        _context.Update(bankaHesap);
                        await _context.SaveChangesAsync();
                        await transaction.CommitAsync();

                        TempData["SuccessMessage"] = "Banka hesabı başarıyla güncellendi.";
                        return RedirectToAction(nameof(Hesaplar), new { id = bankaHesap.BankaID });
                    }
                    catch (Exception ex)
                    {
                        await transaction.RollbackAsync();
                        _logger.LogError(ex, "Banka hesabı güncellenirken hata oluştu");
                        ModelState.AddModelError("", "Banka hesabı güncellenirken bir hata oluştu. Lütfen tekrar deneyin.");
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Banka hesabı güncellenirken hata oluştu");
                    ModelState.AddModelError("", "Banka hesabı güncellenirken bir hata oluştu. Lütfen tekrar deneyin.");
                }
            }
            
            // Hata durumunda para birimi listesini yeniden hazırla
            ViewBag.ParaBirimleri = await _context.ParaBirimleri
                .Where(p => p.Aktif)
                .OrderBy(p => p.Sira)
                .Select(p => new SelectListItem { Value = p.Kod, Text = $"{p.Ad} ({p.Kod})" })
                .ToListAsync();
                
            return View(model);
        }

        // GET: Banka/HesapDelete/5
        public async Task<IActionResult> HesapDelete(Guid? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var bankaHesap = await _context.BankaHesaplari
                .Include(h => h.Banka)
                .FirstOrDefaultAsync(h => h.BankaHesapID == id && !h.Silindi);

            if (bankaHesap == null)
            {
                return NotFound();
            }

            var model = new BankaHesapViewModel
            {
                BankaHesapID = bankaHesap.BankaHesapID,
                BankaID = bankaHesap.BankaID,
                BankaAdi = bankaHesap.Banka?.BankaAdi ?? "Bilinmeyen Banka",
                HesapAdi = bankaHesap.HesapAdi,
                SubeAdi = bankaHesap.SubeAdi,
                SubeKodu = bankaHesap.SubeKodu,
                HesapNo = bankaHesap.HesapNo,
                IBAN = bankaHesap.IBAN,
                ParaBirimi = bankaHesap.ParaBirimi,
                AcilisBakiye = bankaHesap.AcilisBakiye,
                GuncelBakiye = bankaHesap.GuncelBakiye,
                Aciklama = bankaHesap.Aciklama,
                Aktif = bankaHesap.Aktif,
                OlusturmaTarihi = bankaHesap.OlusturmaTarihi
            };

            return View(model);
        }

        // POST: Banka/HesapDelete/5
        [HttpPost, ActionName("HesapDelete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> HesapDeleteConfirmed(Guid id)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();

            try
            {
                var bankaHesap = await _context.BankaHesaplari.FindAsync(id);
                
                if (bankaHesap == null)
                {
                    return NotFound();
                }

                Guid bankaID = bankaHesap.BankaID;

                // İlişkili tüm hareketleri bul ve soft delete işlemi uygula
                var hareketler = await _context.BankaHesapHareketleri
                    .Where(h => h.BankaHesapID == id && !h.Silindi)
                    .ToListAsync();
                
                foreach (var h in hareketler)
                {
                    h.Silindi = true;
                    h.GuncellemeTarihi = DateTime.Now;
                    h.SonGuncelleyenKullaniciID = GetCurrentUserId();
                    _context.Update(h);
                }

                // Özellikle açılış bakiyesi hareketini kontrol et
                var acilisBakiyesiHareket = await _context.BankaHesapHareketleri
                    .FirstOrDefaultAsync(h => h.BankaHesapID == id && 
                                        h.HareketTuru == Enums.BankaHareketTipi.AcilisBakiyesi.ToString() &&
                                        !h.Silindi);
                
                if (acilisBakiyesiHareket != null)
                {
                    acilisBakiyesiHareket.Silindi = true;
                    acilisBakiyesiHareket.GuncellemeTarihi = DateTime.Now;
                    acilisBakiyesiHareket.SonGuncelleyenKullaniciID = GetCurrentUserId();
                    _context.Update(acilisBakiyesiHareket);
                }

                // Hesabı soft delete olarak işaretle
                bankaHesap.Silindi = true;
                bankaHesap.SonGuncelleyenKullaniciID = GetCurrentUserId();
                bankaHesap.GuncellemeTarihi = DateTime.Now;

                _context.Update(bankaHesap);
                await _context.SaveChangesAsync();
                await transaction.CommitAsync();
                
                // İşlemi logla
                await _logService.Log(
                    $"Banka hesabı silindi: {bankaHesap.BankaID} - {bankaHesap.HesapAdi}",
                    Enums.LogTuru.Bilgi
                );
                
                TempData["SuccessMessage"] = "Banka hesabı başarıyla silindi.";
                return RedirectToAction(nameof(Hesaplar), new { id = bankaID });
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                _logger.LogError(ex, "Banka hesabı silinirken hata oluştu");
                TempData["ErrorMessage"] = "Banka hesabı silinirken bir hata oluştu. Lütfen tekrar deneyin.";
                return RedirectToAction(nameof(Index));
            }
        }

        // GET: Banka/HareketSil/5
        public async Task<IActionResult> HareketSil(Guid? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var hareket = await _context.BankaHesapHareketleri
                .Include(h => h.BankaHesap)
                    .ThenInclude(bh => bh.Banka)
                .FirstOrDefaultAsync(h => h.BankaHesapHareketID == id && !h.Silindi);

            if (hareket == null)
            {
                return NotFound();
            }

            // Sadece manuel hareketler silinebilir
            if (hareket.ReferansTuru != "Manuel")
            {
                TempData["ErrorMessage"] = "Yalnızca manuel oluşturulan hareketler silinebilir.";
                return RedirectToAction(nameof(GetHesapHareketler), new { id = hareket.BankaHesapID });
            }

            var viewModel = new BankaHareketViewModel
            {
                BankaHareketID = hareket.BankaHesapHareketID,
                BankaHesapID = hareket.BankaHesapID,
                BankaAdi = hareket.BankaHesap.Banka.BankaAdi,
                HesapAdi = hareket.BankaHesap.HesapAdi,
                HareketTipi = hareket.HareketTuru == "Giriş" ? Enums.BankaHareketTipi.Gelir : Enums.BankaHareketTipi.Gider,
                HareketTuru = hareket.HareketTuru,
                Tarih = hareket.Tarih,
                Tutar = hareket.Tutar,
                ParaBirimi = hareket.BankaHesap.ParaBirimi,
                Aciklama = hareket.Aciklama,
                ReferansNo = hareket.ReferansNo,
                ReferansTuru = hareket.ReferansTuru,
                DekontNo = hareket.DekontNo
            };

            return View(viewModel);
        }

        // POST: Banka/HareketSil/5
        [HttpPost, ActionName("HareketSil")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> HareketSilConfirmed(Guid id)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();

            try
            {
                var hareket = await _context.BankaHesapHareketleri
                    .Include(h => h.BankaHesap)
                    .FirstOrDefaultAsync(h => h.BankaHesapHareketID == id && !h.Silindi);

                if (hareket == null)
                {
                    return NotFound();
                }

                var bankaHesap = hareket.BankaHesap;
                if (bankaHesap == null)
                {
                    return NotFound();
                }

                // Bakiyeyi güncelle
                if (hareket.HareketTuru == "Giriş")
                {
                    bankaHesap.GuncelBakiye -= hareket.Tutar;
                }
                else
                {
                    bankaHesap.GuncelBakiye += hareket.Tutar;
                }

                // İlişkili cari hareketini de sil
                var cariHareket = await _context.CariHareketler
                    .FirstOrDefaultAsync(ch => ch.ReferansID == hareket.BankaHesapHareketID && ch.ReferansTuru == "BankaHesapHareket" && !ch.Silindi);

                if (cariHareket != null)
                {
                    cariHareket.Silindi = true;
                    cariHareket.GuncellemeTarihi = DateTime.Now;
                    cariHareket.OlusturanKullaniciID = GetCurrentUserId();
                    _context.Update(cariHareket);
                }

                // Hareketi sil (soft delete)
                hareket.Silindi = true;
                hareket.GuncellemeTarihi = DateTime.Now;
                hareket.SonGuncelleyenKullaniciID = GetCurrentUserId();

                bankaHesap.GuncellemeTarihi = DateTime.Now;
                bankaHesap.SonGuncelleyenKullaniciID = GetCurrentUserId();

                _context.Update(hareket);
                _context.Update(bankaHesap);
                await _context.SaveChangesAsync();

                await transaction.CommitAsync();

                await _logService.Log(
                    $"Banka hesap hareketi silindi: {bankaHesap.Banka?.BankaAdi} - {bankaHesap.HesapAdi}, {hareket.HareketTuru}, {hareket.Tutar} {bankaHesap.ParaBirimi}",
                    Enums.LogTuru.Bilgi
                );

                return RedirectToAction(nameof(GetHesapHareketler), new { id = hareket.BankaHesapID });
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                _logger.LogError(ex, "Banka hesap hareketi silinirken hata oluştu. ID: {Id}", id);
                TempData["ErrorMessage"] = "Hareket silinirken bir hata oluştu. Lütfen tekrar deneyin.";
                return RedirectToAction(nameof(HareketSil), new { id });
            }
        }

        /// <summary>
        /// Banka hareketleri için gerekli dropdown listeleri hazırlar
        /// </summary>
        private async Task PrepareDropdownsAsync()
        {
            try
            {
                // Hesap türleri dropdown
                ViewBag.HareketTipleri = new List<SelectListItem>
                {
                    new SelectListItem { Text = "Giriş", Value = "0" },
                    new SelectListItem { Text = "Çıkış", Value = "1" }
                };
                
                // Referans türleri dropdown
                ViewBag.ReferansTurleri = new List<SelectListItem>
                {
                    new SelectListItem { Text = "Manuel", Value = "Manuel" },
                    new SelectListItem { Text = "Fatura", Value = "Fatura" },
                    new SelectListItem { Text = "Çek", Value = "Çek" },
                    new SelectListItem { Text = "Senet", Value = "Senet" },
                    new SelectListItem { Text = "Banka", Value = "Banka" }
                };
                
                // HesabaKaydet için kontroller
                ViewBag.HesapKaydetSecenekleri = new List<SelectListItem>
                {
                    new SelectListItem { Text = "Evet", Value = "true", Selected = true },
                    new SelectListItem { Text = "Hayır", Value = "false" }
                };
                
                // Cari listesini getir
                var cariler = await _context.Cariler
                    .Where(c => !c.Silindi && c.AktifMi)
                    .OrderBy(c => c.Ad)
                    .ToListAsync();
                
                ViewBag.Cariler = cariler;
                
                // Para birimleri
                var paraBirimleri = await _context.ParaBirimleri
                    .Where(p => p.Aktif)
                    .OrderBy(p => p.Sira)
                    .Select(p => new SelectListItem { Value = p.Kod, Text = $"{p.Ad} ({p.Kod})" })
                    .ToListAsync();
                
                ViewBag.ParaBirimleri = paraBirimleri;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Dropdown listeler hazırlanırken hata oluştu");
            }
        }

        private async Task<BankaHesapHareket?> GetBankaHesapHareketAsync(Guid id)
        {
            return await _context.BankaHesapHareketleri
                .Include(h => h.BankaHesap)
                .Include(h => h.Banka)
                .Include(h => h.Cari)
                .FirstOrDefaultAsync(h => h.BankaHesapHareketID == id && !h.Silindi);
        }

        private BankaHesapHareketViewModel? MapToViewModel(BankaHesapHareket? hareket)
        {
            if (hareket == null)
                return null;

            return new BankaHesapHareketViewModel
            {
                BankaHesapHareketID = hareket.BankaHesapHareketID,
                BankaHesapID = hareket.BankaHesapID,
                BankaAdi = hareket.Banka?.BankaAdi ?? "",
                HesapAdi = hareket.BankaHesap?.HesapAdi ?? "",
                IBAN = hareket.BankaHesap?.IBAN ?? "",
                BankaHesapNo = hareket.BankaHesap?.HesapNo ?? "",
                HareketTuru = hareket.HareketTuru,
                Tarih = hareket.Tarih,
                Tutar = hareket.Tutar,
                DekontNo = hareket.DekontNo ?? "",
                ReferansNo = hareket.ReferansNo ?? "",
                ReferansTuru = hareket.ReferansTuru ?? "",
                Aciklama = hareket.Aciklama ?? "",
                CariID = hareket.CariID,
                CariUnvani = hareket.Cari?.Ad ?? "",
                OlusturmaTarihi = hareket.OlusturmaTarihi,
                OlusturanKullaniciAdi = "Sistem",
                SonGuncellemeTarihi = hareket.GuncellemeTarihi,
                GuncelleyenKullaniciAdi = "Sistem"
            };
        }

        private bool BankaExists(Guid id)
        {
            return _context.Bankalar.Any(e => e.BankaID == id && !e.Silindi);
        }

        private bool BankaHesapExists(Guid id)
        {
            return _context.BankaHesaplari.Any(e => e.BankaHesapID == id && !e.Silindi);
        }

        private new Guid? GetCurrentUserId()
        {
            if (User?.Identity?.IsAuthenticated == true)
            {
                var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
                if (!string.IsNullOrEmpty(userIdClaim) && Guid.TryParse(userIdClaim, out Guid userId))
                {
                    return userId;
                }
            }
            return null;
        }

        // GET: Banka/HesapHareketler/5
        public async Task<IActionResult> HesapHareketler(Guid id)
        {
            try
            {
                // Banka hesabını bul
                var hesap = await _context.BankaHesaplari
                    .Include(b => b.Banka)
                    .OrderBy(b => b.HesapAdi)
                    .FirstOrDefaultAsync(b => b.BankaHesapID == id && !b.Silindi);

                if (hesap == null)
                {
                    TempData["ErrorMessage"] = "Banka hesabı bulunamadı.";
                    return RedirectToAction(nameof(Index));
                }

                return View(hesap);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Banka hesap hareketleri sayfası yüklenirken hata oluştu. HesapID: {Id}", id);
                TempData["ErrorMessage"] = "Banka hesap hareketleri sayfası yüklenirken bir hata oluştu.";
                return RedirectToAction(nameof(Index));
            }
        }

        // POST: Banka/GetHesapHareketler/5 (AJAX çağrısı için)
        [HttpPost]
        public async Task<IActionResult> GetHesapHareketler(Guid id)
        {
            try
            {
                // Banka hesabını bul
                var hesap = await _context.BankaHesaplari
                    .Include(b => b.Banka)
                    .OrderBy(b => b.HesapAdi)
                    .FirstOrDefaultAsync(b => b.BankaHesapID == id && !b.Silindi);

                if (hesap == null)
                {
                    return Json(new { error = "Banka hesabı bulunamadı." });
                }

                var hesapID = hesap.BankaHesapID.ToString();

                // Hareketleri getir
                var hareketler = _context.BankaHesapHareketleri
                    .Include(h => h.BankaHesap)
                    .Include(h => h.Banka)
                    .Include(h => h.Cari)
                    .Where(h => h.BankaHesapID == id && !h.Silindi)
                    .AsQueryable();

                // Arama yapılmışsa filtreleme
                var search = Request.Query["search[value]"].FirstOrDefault();
                if (!string.IsNullOrEmpty(search))
                {
                    hareketler = hareketler.Where(h =>
                        (h.ReferansNo != null && h.ReferansNo.Contains(search)) ||
                        h.Tarih.ToString().Contains(search) ||
                        (h.HareketTuru != null && h.HareketTuru.Contains(search)) ||
                        (h.DekontNo != null && h.DekontNo.Contains(search)) ||
                        (h.ReferansTuru != null && h.ReferansTuru.Contains(search)) ||
                        (h.Aciklama != null && h.Aciklama.Contains(search)) ||
                        (h.Cari != null && h.Cari.Ad != null && h.Cari.Ad.Contains(search))
                    );
                }
                
                // Toplam kayıt sayısı
                int totalRecords = await hareketler.CountAsync();
                
                // Filtrelenmiş kayıt sayısı
                int recordsFiltered = totalRecords;
                
                // Sıralama
                var order = Request.Query["order[0][column]"].FirstOrDefault();
                var sortDir = Request.Query["order[0][dir]"].FirstOrDefault();
                if (!string.IsNullOrEmpty(order) && !string.IsNullOrEmpty(sortDir))
                {
                    int columnIndex = int.Parse(order);
                    bool isAscending = sortDir.ToLower() == "asc";
                    
                    switch (columnIndex)
                    {
                        case 0:
                            hareketler = isAscending ? hareketler.OrderBy(h => h.Tarih) : hareketler.OrderByDescending(h => h.Tarih);
                            break;
                        case 1:
                            hareketler = isAscending ? hareketler.OrderBy(h => h.HareketTuru) : hareketler.OrderByDescending(h => h.HareketTuru);
                            break;
                        case 2:
                            hareketler = isAscending ? hareketler.OrderBy(h => h.ReferansNo) : hareketler.OrderByDescending(h => h.ReferansNo);
                            break;
                        case 3:
                            hareketler = isAscending ? hareketler.OrderBy(h => h.ReferansTuru) : hareketler.OrderByDescending(h => h.ReferansTuru);
                            break;
                        case 4:
                            hareketler = isAscending ? hareketler.OrderBy(h => h.Aciklama) : hareketler.OrderByDescending(h => h.Aciklama);
                            break;
                        case 5:
                            hareketler = isAscending ? hareketler.OrderBy(h => h.Tutar) : hareketler.OrderByDescending(h => h.Tutar);
                            break;
                        default:
                            hareketler = hareketler.OrderByDescending(h => h.Tarih);
                            break;
                    }
                }
                else
                {
                    // Varsayılan sıralama
                    hareketler = hareketler.OrderByDescending(h => h.Tarih);
                }
                
                // Sayfalama
                int skip = 0;
                int take = 10;
                
                var start = Request.Query["start"].FirstOrDefault();
                var length = Request.Query["length"].FirstOrDefault();
                
                if (!string.IsNullOrEmpty(start) && !string.IsNullOrEmpty(length))
                {
                    skip = int.Parse(start);
                    take = int.Parse(length);
                }
                
                // Hareket verilerini hazırla
                var data = await hareketler
                    .Skip(skip)
                    .Take(take)
                    .OrderByDescending(h => h.Tarih)
                    .ThenByDescending(h => h.OlusturmaTarihi)
                    .Select(h => new
                    {
                        id = h.BankaHesapHareketID,
                        tarih = h.Tarih.ToString("dd.MM.yyyy"),
                        hareketTuru = h.HareketTuru ?? "",
                        tutar = h.Tutar.ToString("N2"),
                        cari = h.Cari != null ? h.Cari.Ad : "",
                        dekontNo = h.DekontNo ?? "",
                        referansNo = h.ReferansNo ?? "",
                        referansTuru = h.ReferansTuru ?? "",
                        aciklama = h.Aciklama ?? "",
                        karsiParaBirimi = h.KarsiParaBirimi ?? (h.BankaHesap != null ? h.BankaHesap.ParaBirimi ?? "TRY" : "TRY"),
                        olusturmaTarihi = h.OlusturmaTarihi.ToString("dd.MM.yyyy HH:mm")
                    })
                    .ToListAsync();
                
                return Json(new
                {
                    draw = Request.Query["draw"].FirstOrDefault() ?? "1",
                    recordsFiltered = recordsFiltered,
                    recordsTotal = totalRecords,
                    data = data
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "GetHesapHareketler çağrılırken hata oluştu. HesapID: {Id}", id);
                return Json(new { 
                    draw = Request.Query["draw"].FirstOrDefault() ?? "1",
                    recordsFiltered = 0,
                    recordsTotal = 0,
                    data = new List<object>() 
                });
            }
        }

        // GET: Banka/YeniHareket/5
        public async Task<IActionResult> YeniHareket(Guid id)
        {
            try
            {
                // Banka hesabını bul
                var bankaHesap = await _context.BankaHesaplari
                    .Include(bh => bh.Banka)
                    .FirstOrDefaultAsync(bh => bh.BankaHesapID == id && !bh.Silindi);

                if (bankaHesap == null)
                {
                    TempData["ErrorMessage"] = "Banka hesabı bulunamadı.";
                    return RedirectToAction(nameof(Index));
                }

                // Dropdown listeleri hazırla
                await PrepareDropdownsAsync();

                // ViewBag'e banka hesabını ekle
                ViewBag.BankaHesap = bankaHesap;

                // Model oluştur
                var model = new BankaHareketCreateViewModel
                {
                    BankaHesapID = bankaHesap.BankaHesapID,
                    Tarih = DateTime.Now,
                    HareketTipi = Enums.BankaHareketTipi.Gelir,
                    ReferansTuru = "Manuel",
                    HesabaKaydet = true,
                    KarsiParaBirimi = bankaHesap.ParaBirimi ?? "TRY"
                };

                return View(model);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Yeni banka hareketi sayfası yüklenirken hata oluştu. HesapID: {Id}", id);
                TempData["ErrorMessage"] = "Yeni banka hareketi sayfası yüklenirken bir hata oluştu.";
                return RedirectToAction(nameof(Index));
            }
        }

        // POST: Banka/YeniHareket
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> YeniHareket(BankaHareketCreateViewModel model)
        {
            if (ModelState.IsValid)
            {
                using var transaction = await _context.Database.BeginTransactionAsync();

                try
                {
                    // Banka hesabını bul
                    var bankaHesap = await _context.BankaHesaplari
                        .Include(bh => bh.Banka)
                        .FirstOrDefaultAsync(bh => bh.BankaHesapID == model.BankaHesapID && !bh.Silindi);

                    if (bankaHesap == null)
                    {
                        TempData["ErrorMessage"] = "Banka hesabı bulunamadı.";
                        return RedirectToAction(nameof(Index));
                    }

                    // Giriş (Para Yatırma) veya Çıkış (Para Çekme) işlemi
                    var isGiris = model.HareketTipi == Enums.BankaHareketTipi.Gelir || 
                                model.HareketTipi == Enums.BankaHareketTipi.AcilisBakiyesi;

                    // Bakiyeyi güncelle
                    if (isGiris)
                    {
                        bankaHesap.GuncelBakiye += model.Tutar;
                    }
                    else
                    {
                        bankaHesap.GuncelBakiye -= model.Tutar;
                    }

                    // Eğer KarsiParaBirimi tanımlanmamışsa, banka hesabının para birimini kullan
                    if (string.IsNullOrEmpty(model.KarsiParaBirimi))
                    {
                        model.KarsiParaBirimi = bankaHesap.ParaBirimi ?? "TRY";
                    }

                    // Yeni banka hareketi oluştur
                    var hareket = new BankaHesapHareket
                    {
                        BankaHesapHareketID = Guid.NewGuid(),
                        BankaHesapID = bankaHesap.BankaHesapID,
                        BankaID = bankaHesap.BankaID,
                        Tutar = Math.Abs(model.Tutar),
                        HareketTuru = isGiris ? "Giriş" : "Çıkış",
                        Tarih = model.Tarih,
                        ReferansNo = model.ReferansNo,
                        ReferansTuru = model.ReferansTuru,
                        DekontNo = string.IsNullOrEmpty(model.DekontNo) ? $"DKT-{DateTime.Now:yyMMdd}-{new Random().Next(1000):000}" : model.DekontNo,
                        CariID = model.CariID,
                        KarsiParaBirimi = model.KarsiParaBirimi,
                        Aciklama = model.Aciklama,
                        IslemYapanKullaniciID = GetCurrentUserId(),
                        OlusturmaTarihi = DateTime.Now,
                        SonGuncelleyenKullaniciID = GetCurrentUserId(),
                        Silindi = false
                    };

                    _context.BankaHesapHareketleri.Add(hareket);
                    _context.Update(bankaHesap);
                    await _context.SaveChangesAsync();

                    // Cari ile dengelenecekse
                    if (model.CariID.HasValue && model.CariID != Guid.Empty && model.CariIleDengelensin)
                    {
                        // Cari hareketi oluştur
                        await _cariHareketService.CreateFromBankaHareketAsync(hareket, !isGiris);
                    }

                    await transaction.CommitAsync();

                    await _logService.Log(
                        $"Yeni banka hareketi oluşturuldu: {bankaHesap.Banka?.BankaAdi} - {bankaHesap.HesapAdi}, {hareket.HareketTuru}, {hareket.Tutar} {bankaHesap.ParaBirimi}",
                        Enums.LogTuru.Bilgi
                    );

                    TempData["SuccessMessage"] = "Banka hareketi başarıyla oluşturuldu.";
                    return RedirectToAction(nameof(HesapHareketler), new { id = model.BankaHesapID });
                }
                catch (Exception ex)
                {
                    await transaction.RollbackAsync();
                    _logger.LogError(ex, "Banka hareketi oluşturulurken hata oluştu");
                    ModelState.AddModelError("", "Banka hareketi oluşturulurken bir hata oluştu. Lütfen tekrar deneyin.");
                }
            }

            // Hata durumunda dropdown listelerini yeniden yükle
            await PrepareDropdownsAsync();

            // Banka hesabını tekrar yükle
            var hesap = await _context.BankaHesaplari
                .Include(bh => bh.Banka)
                .FirstOrDefaultAsync(bh => bh.BankaHesapID == model.BankaHesapID && !bh.Silindi);

            ViewBag.BankaHesap = hesap;

            return View(model);
        }

        /// <summary>
        /// Banka-Kasa arası transfer işlemleri için modal formu açar
        /// </summary>
        public async Task<IActionResult> YeniTransfer(Guid? hesapId = null)
        {
            // Kasaları getir
            var kasalar = await _unitOfWork.Repository<Kasa>().GetAsync(
                filter: k => !k.Silindi && k.Aktif,
                orderBy: q => q.OrderBy(k => k.KasaAdi)
            );

            // Banka hesaplarını getir
            var bankaHesaplari = await _context.BankaHesaplari
                .Include(h => h.Banka)
                .Where(h => !h.Silindi && h.Aktif)
                .OrderBy(h => h.Banka.BankaAdi)
                .ThenBy(h => h.HesapAdi)
                .ToListAsync();

            if (!bankaHesaplari.Any())
            {
                return Json(new { success = false, message = "Sistemde kayıtlı aktif banka hesabı bulunamadı. Lütfen önce bir banka hesabı ekleyin." });
            }

            ViewBag.Kasalar = kasalar.ToList();
            ViewBag.BankaHesaplari = bankaHesaplari;

            var model = new MuhasebeStokWebApp.ViewModels.Transfer.IcTransferViewModel
            {
                TransferTuru = "BankadanKasaya",
                Tarih = DateTime.Now,
                ReferansNo = "TRNSFR-" + DateTime.Now.ToString("yyMMdd") + "-" + new Random().Next(100, 999).ToString(),
                KaynakBankaHesapID = hesapId
            };

            return PartialView("~/Views/Transfer/_TransferModalPartial.cshtml", model);
        }
    }
} 