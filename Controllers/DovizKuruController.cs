using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using MuhasebeStokWebApp.Data;
using MuhasebeStokWebApp.Data.Entities;
using MuhasebeStokWebApp.Data.Entities.DovizModulu;
using MuhasebeStokWebApp.Services.Interfaces;
using MuhasebeStokWebApp.ViewModels;
using DVM = MuhasebeStokWebApp.ViewModels.Doviz;
using Microsoft.AspNetCore.Identity;
using MuhasebeStokWebApp.Services;

namespace MuhasebeStokWebApp.Controllers
{
    [Authorize]
    [Route("Kur")]
    public class DovizKuruController : BaseController
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<DovizKuruController> _logger;
        private readonly IDovizKuruService _dovizKuruService;
        protected new readonly UserManager<ApplicationUser> _userManager;

        public DovizKuruController(
            ApplicationDbContext context,
            ILogger<DovizKuruController> logger,
            IDovizKuruService dovizKuruService,
            UserManager<ApplicationUser> userManager,
            IMenuService menuService,
            RoleManager<IdentityRole> roleManager,
            ILogService logService)
            : base(menuService, userManager, roleManager, logService)
        {
            _context = context;
            _logger = logger;
            _dovizKuruService = dovizKuruService;
            _userManager = userManager;
        }

        // GET: Kur/Liste
        [Route("liste")]
        public async Task<IActionResult> Liste()
        {
            try
            {
                var kurlar = await _context.KurDegerleri
                    .Include(k => k.ParaBirimi)
                    .Where(k => !k.Silindi && k.Aktif)
                    .OrderByDescending(k => k.Tarih)
                    .ThenBy(k => k.ParaBirimi.Kod)
                    .ToListAsync();

                var viewModel = kurlar.Select(k => new DovizKuruViewModel
                {
                    DovizKuruID = k.KurDegeriID,
                    DovizKodu = k.ParaBirimi.Kod,
                    DovizAdi = k.ParaBirimi.Ad,
                    AlisFiyati = k.Alis,
                    SatisFiyati = k.Satis,
                    EfektifAlisFiyati = k.Efektif_Alis,
                    EfektifSatisFiyati = k.Efektif_Satis,
                    Tarih = k.Tarih,
                    GuncellemeTarihi = k.GuncellemeTarihi ?? k.OlusturmaTarihi ?? DateTime.Now,
                    Aktif = k.Aktif,
                    ParaBirimiKodu = k.ParaBirimi.Kod
                }).ToList();

                return View(viewModel);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Döviz kurları listelenirken hata oluştu");
                TempData["ErrorMessage"] = "Döviz kurları listelenirken bir hata oluştu: " + ex.Message;
                return View(new List<DovizKuruViewModel>());
            }
        }

        // GET: Kur/Detay/5
        [Route("detay/{id:guid}")]
        public async Task<IActionResult> Detay(Guid id)
        {
            try
            {
                var kurDegeri = await _context.KurDegerleri
                    .Include(k => k.ParaBirimi)
                    .FirstOrDefaultAsync(k => k.KurDegeriID == id && !k.Silindi);

                if (kurDegeri == null)
                {
                    return NotFound();
                }

                var viewModel = new DovizKuruViewModel
                {
                    DovizKuruID = kurDegeri.KurDegeriID,
                    DovizKodu = kurDegeri.ParaBirimi.Kod,
                    DovizAdi = kurDegeri.ParaBirimi.Ad,
                    AlisFiyati = kurDegeri.Alis,
                    SatisFiyati = kurDegeri.Satis,
                    EfektifAlisFiyati = kurDegeri.Efektif_Alis,
                    EfektifSatisFiyati = kurDegeri.Efektif_Satis,
                    Tarih = kurDegeri.Tarih,
                    GuncellemeTarihi = kurDegeri.GuncellemeTarihi ?? kurDegeri.OlusturmaTarihi ?? DateTime.Now,
                    Aktif = kurDegeri.Aktif,
                    ParaBirimiKodu = kurDegeri.ParaBirimi.Kod
                };

                return View(viewModel);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Döviz kuru detayı görüntülenirken hata oluştu");
                TempData["ErrorMessage"] = "Döviz kuru detayı görüntülenirken bir hata oluştu: " + ex.Message;
                return RedirectToAction(nameof(Liste));
            }
        }

        // GET: Kur/Ekle
        [Route("ekle")]
        public async Task<IActionResult> Ekle()
        {
            try
            {
                var paraBirimleri = await _context.ParaBirimleri
                    .Where(p => p.Aktif && !p.Silindi)
                    .OrderBy(p => p.Kod)
                    .ToListAsync();
                
                ViewBag.ParaBirimleri = new SelectList(paraBirimleri, "ParaBirimiID", "Kod");
                
                return View(new DovizKuruEkleViewModel
                {
                    Tarih = DateTime.Today
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Döviz kuru ekleme sayfası yüklenirken hata oluştu");
                TempData["ErrorMessage"] = "Döviz kuru ekleme sayfası yüklenirken bir hata oluştu: " + ex.Message;
                return RedirectToAction(nameof(Liste));
            }
        }

        // POST: Kur/Ekle
        [HttpPost]
        [Route("ekle")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Ekle(DovizKuruEkleViewModel viewModel)
        {
            if (!ModelState.IsValid)
            {
                var paraBirimleri = await _context.ParaBirimleri
                    .Where(p => p.Aktif && !p.Silindi)
                    .OrderBy(p => p.Kod)
                    .ToListAsync();
                
                ViewBag.ParaBirimleri = new SelectList(paraBirimleri, "ParaBirimiID", "Kod");
                return View(viewModel);
            }

            try
            {
                var paraBirimi = await _context.ParaBirimleri.FindAsync(viewModel.KaynakParaBirimiID);
                
                if (paraBirimi == null)
                {
                    ModelState.AddModelError("KaynakParaBirimiID", "Seçilen para birimi bulunamadı");
                    return View(viewModel);
                }
                
                // Aynı gün ve para birimi için kayıt var mı kontrol et
                var existingKur = await _context.KurDegerleri
                    .FirstOrDefaultAsync(k => k.ParaBirimiID == viewModel.KaynakParaBirimiID && 
                                      k.Tarih.Date == viewModel.Tarih.Date &&
                                      !k.Silindi);

                if (existingKur != null)
                {
                    ModelState.AddModelError("", "Bu para birimi için belirtilen tarihte zaten bir kur kaydı mevcut.");
                    ViewBag.ParaBirimleri = new SelectList(await _context.ParaBirimleri.Where(p => !p.Silindi && p.Aktif).ToListAsync(), "ParaBirimiID", "Ad");
                    return View(viewModel);
                }
                
                var kurDegeri = new KurDegeri
                {
                    ParaBirimiID = viewModel.KaynakParaBirimiID,
                    Tarih = viewModel.Tarih,
                    Alis = viewModel.Alis,
                    Satis = viewModel.Satis,
                    Efektif_Alis = viewModel.Efektif_Alis.GetValueOrDefault(),
                    Efektif_Satis = viewModel.Efektif_Satis.GetValueOrDefault(),
                    Aciklama = viewModel.Aciklama,
                    Aktif = true,
                    OlusturmaTarihi = DateTime.Now,
                    OlusturanKullaniciID = _userManager.GetUserId(User),
                };
                
                _context.KurDegerleri.Add(kurDegeri);
                await _context.SaveChangesAsync();
                
                TempData["SuccessMessage"] = "Döviz kuru başarıyla eklendi";
                return RedirectToAction(nameof(Liste));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Döviz kuru eklenirken hata oluştu");
                TempData["ErrorMessage"] = "Döviz kuru eklenirken bir hata oluştu: " + ex.Message;
                
                var paraBirimleri = await _context.ParaBirimleri
                    .Where(p => p.Aktif && !p.Silindi)
                    .OrderBy(p => p.Kod)
                    .ToListAsync();
                
                ViewBag.ParaBirimleri = new SelectList(paraBirimleri, "ParaBirimiID", "Kod");
                return View(viewModel);
            }
        }

        // GET: Kur/Duzenle/5
        [Route("duzenle/{id:guid}")]
        public async Task<IActionResult> Duzenle(Guid id)
        {
            try
            {
                var kurDegeri = await _context.KurDegerleri
                    .Include(k => k.ParaBirimi)
                    .FirstOrDefaultAsync(k => k.KurDegeriID == id && !k.Silindi);
                
                if (kurDegeri == null)
                {
                    return NotFound();
                }
                
                var paraBirimleri = await _context.ParaBirimleri
                    .Where(p => p.Aktif && !p.Silindi)
                    .OrderBy(p => p.Kod)
                    .ToListAsync();
                
                ViewBag.ParaBirimleri = new SelectList(paraBirimleri, "ParaBirimiID", "Kod", kurDegeri.ParaBirimiID);
                
                var viewModel = new DovizKuruDuzenleViewModel
                {
                    DovizKuruID = kurDegeri.KurDegeriID,
                    KaynakParaBirimiID = kurDegeri.ParaBirimiID,
                    Tarih = kurDegeri.Tarih,
                    Alis = kurDegeri.Alis,
                    Satis = kurDegeri.Satis,
                    Efektif_Alis = kurDegeri.Efektif_Alis,
                    Efektif_Satis = kurDegeri.Efektif_Satis,
                    Aktif = kurDegeri.Aktif,
                    Kaynak = kurDegeri.OlusturanKullaniciID ?? "Manuel",
                    Aciklama = kurDegeri.Aciklama
                };
                
                return View(viewModel);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Döviz kuru düzenleme sayfası yüklenirken hata oluştu");
                TempData["ErrorMessage"] = "Döviz kuru düzenleme sayfası yüklenirken bir hata oluştu: " + ex.Message;
                return RedirectToAction(nameof(Liste));
            }
        }

        // POST: Kur/Duzenle/5
        [HttpPost]
        [Route("duzenle/{id:guid}")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Duzenle(Guid id, DovizKuruDuzenleViewModel viewModel)
        {
            if (id != viewModel.DovizKuruID)
            {
                return NotFound();
            }

            if (!ModelState.IsValid)
            {
                var paraBirimleri = await _context.ParaBirimleri
                    .Where(p => p.Aktif && !p.Silindi)
                    .OrderBy(p => p.Kod)
                    .ToListAsync();
                
                ViewBag.ParaBirimleri = new SelectList(paraBirimleri, "ParaBirimiID", "Kod", viewModel.KaynakParaBirimiID);
                return View(viewModel);
            }

            try
            {
                var kurDegeri = await _context.KurDegerleri
                    .FirstOrDefaultAsync(k => k.KurDegeriID == id && !k.Silindi);
                
                if (kurDegeri == null)
                {
                    return NotFound();
                }
                
                var paraBirimi = await _context.ParaBirimleri.FindAsync(viewModel.KaynakParaBirimiID);
                
                if (paraBirimi == null)
                {
                    ModelState.AddModelError("KaynakParaBirimiID", "Seçilen para birimi bulunamadı");
                    return View(viewModel);
                }
                
                // Değişiklik yapılan para birimi ve tarih için başka bir kur kaydı var mı?
                if (kurDegeri.ParaBirimiID != viewModel.KaynakParaBirimiID || kurDegeri.Tarih.Date != viewModel.Tarih.Date)
                {
                    var existingKur = await _context.KurDegerleri
                        .FirstOrDefaultAsync(k => k.ParaBirimiID == viewModel.KaynakParaBirimiID && 
                                               k.Tarih.Date == viewModel.Tarih.Date &&
                                               !k.Silindi && k.KurDegeriID != viewModel.DovizKuruID);
                    
                    if (existingKur != null)
                    {
                        ModelState.AddModelError("", "Bu para birimi için seçilen tarihte zaten bir kur kaydı mevcut");
                        return View(viewModel);
                    }
                }
                
                // Kur değeri güncelleme
                kurDegeri.ParaBirimiID = viewModel.KaynakParaBirimiID;
                kurDegeri.Tarih = viewModel.Tarih;
                kurDegeri.Alis = viewModel.Alis;
                kurDegeri.Satis = viewModel.Satis;
                kurDegeri.Efektif_Alis = viewModel.Efektif_Alis.GetValueOrDefault();
                kurDegeri.Efektif_Satis = viewModel.Efektif_Satis.GetValueOrDefault();
                kurDegeri.Aktif = viewModel.Aktif;
                kurDegeri.GuncellemeTarihi = DateTime.Now;
                kurDegeri.SonGuncelleyenKullaniciID = User.Identity?.Name;
                kurDegeri.Aciklama = viewModel.Aciklama;
                
                _context.Update(kurDegeri);
                await _context.SaveChangesAsync();
                
                TempData["SuccessMessage"] = "Döviz kuru başarıyla güncellendi";
                return RedirectToAction(nameof(Liste));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Döviz kuru güncellenirken hata oluştu");
                TempData["ErrorMessage"] = "Döviz kuru güncellenirken bir hata oluştu: " + ex.Message;
                
                var paraBirimleri = await _context.ParaBirimleri
                    .Where(p => p.Aktif && !p.Silindi)
                    .OrderBy(p => p.Kod)
                    .ToListAsync();
                
                ViewBag.ParaBirimleri = new SelectList(paraBirimleri, "ParaBirimiID", "Kod", viewModel.KaynakParaBirimiID);
                return View(viewModel);
            }
        }

        // GET: Kur/Sil/5
        [Route("sil/{id:guid}")]
        public async Task<IActionResult> Sil(Guid id)
        {
            try
            {
                var kurDegeri = await _context.KurDegerleri
                    .Include(k => k.ParaBirimi)
                    .FirstOrDefaultAsync(k => k.KurDegeriID == id && !k.Silindi);
                
                if (kurDegeri == null)
                {
                    return NotFound();
                }
                
                var viewModel = new DovizKuruViewModel
                {
                    DovizKuruID = kurDegeri.KurDegeriID,
                    DovizKodu = kurDegeri.ParaBirimi.Kod,
                    DovizAdi = kurDegeri.ParaBirimi.Ad,
                    AlisFiyati = kurDegeri.Alis,
                    SatisFiyati = kurDegeri.Satis,
                    EfektifAlisFiyati = kurDegeri.Efektif_Alis,
                    EfektifSatisFiyati = kurDegeri.Efektif_Satis,
                    Tarih = kurDegeri.Tarih,
                    GuncellemeTarihi = kurDegeri.GuncellemeTarihi ?? kurDegeri.OlusturmaTarihi ?? DateTime.Now,
                    Aktif = kurDegeri.Aktif,
                    ParaBirimiKodu = kurDegeri.ParaBirimi.Kod
                };
                
                return View(viewModel);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Döviz kuru silme sayfası yüklenirken hata oluştu");
                TempData["ErrorMessage"] = "Döviz kuru silme sayfası yüklenirken bir hata oluştu: " + ex.Message;
                return RedirectToAction(nameof(Liste));
            }
        }

        // POST: Kur/Sil/5
        [HttpPost, ActionName("Sil")]
        [Route("sil/{id:guid}")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SilOnay(Guid id)
        {
            try
            {
                var kurDegeri = await _context.KurDegerleri.FindAsync(id);
                
                if (kurDegeri == null)
                {
                    return NotFound();
                }
                
                // Soft delete işlemi
                kurDegeri.Silindi = true;
                kurDegeri.Aktif = false;
                kurDegeri.GuncellemeTarihi = DateTime.Now;
                kurDegeri.SonGuncelleyenKullaniciID = User.Identity?.Name;
                
                _context.Update(kurDegeri);
                await _context.SaveChangesAsync();
                
                TempData["SuccessMessage"] = "Döviz kuru başarıyla silindi";
                return RedirectToAction(nameof(Liste));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Döviz kuru silinirken hata oluştu");
                TempData["ErrorMessage"] = "Döviz kuru silinirken bir hata oluştu: " + ex.Message;
                return RedirectToAction(nameof(Liste));
            }
        }

        // GET: Kur/KurlariGuncelle
        [HttpGet]
        [Route("kurlari-guncelle")]
        public async Task<IActionResult> KurlariGuncelle()
        {
            try
            {
                // TCMB'den kurlar güncellenecek. Şimdilik sadece görüntüleme sayfası
                var paraBirimleri = await _context.ParaBirimleri
                    .Where(p => p.Aktif && !p.Silindi)
                    .OrderBy(p => p.Kod)
                    .ToListAsync();
                
                return View(paraBirimleri);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Döviz kurları güncelleme sayfası yüklenirken hata oluştu");
                TempData["ErrorMessage"] = "Döviz kurları güncelleme sayfası yüklenirken bir hata oluştu: " + ex.Message;
                return RedirectToAction(nameof(Liste));
            }
        }

        // Kur değerinin varlığını kontrol eden yardımcı metod
        private async Task<bool> KurDegeriExists(Guid id)
        {
            return await _context.KurDegerleri.AnyAsync(k => k.KurDegeriID == id && !k.Silindi);
        }
    }
} 