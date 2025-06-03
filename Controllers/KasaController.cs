using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using MuhasebeStokWebApp.Data;
using MuhasebeStokWebApp.Data.Entities;
using MuhasebeStokWebApp.Data.Repositories;
using MuhasebeStokWebApp.Services;
using MuhasebeStokWebApp.ViewModels.Kasa;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Identity;
using MuhasebeStokWebApp.Services.Interfaces;
using System.Security.Claims;
using MuhasebeStokWebApp.ViewModels.Doviz;
using MuhasebeStokWebApp.ViewModels.Transfer;
using System.Globalization;

namespace MuhasebeStokWebApp.Controllers
{
    /// <summary>
    /// Kasa işlemlerini yöneten controller sınıfı.
    /// Nakit hareketleri, transferler ve kasa kayıtlarının yönetimini sağlar.
    /// </summary>
    public class KasaController : BaseController
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ApplicationDbContext _context;
        private new readonly ILogService _logService;
        private readonly ILogger<KasaController> _logger;
        private readonly IDovizKuruService _dovizKuruService;
        private readonly IParaBirimiService _paraBirimiService;
        protected new readonly UserManager<ApplicationUser> _userManager;
        protected new readonly RoleManager<IdentityRole> _roleManager;
        private readonly ICariHareketService _cariHareketService;

        /// <summary>
        /// Dependency Injection ile gerekli servislerin constructor üzerinden alınması
        /// </summary>
        public KasaController(
            IUnitOfWork unitOfWork,
            ILogger<KasaController> logger,
            ApplicationDbContext context,
            IMenuService menuService,
            IDovizKuruService dovizKuruService,
            ILogService logService,
            IParaBirimiService paraBirimiService,
            UserManager<ApplicationUser> userManager,
            RoleManager<IdentityRole> roleManager,
            ICariHareketService cariHareketService) : base(menuService, userManager, roleManager, logService)
        {
            _unitOfWork = unitOfWork;
            _logger = logger;
            _context = context;
            _dovizKuruService = dovizKuruService;
            _logService = logService;
            _paraBirimiService = paraBirimiService;
            _userManager = userManager;
            _roleManager = roleManager;
            _cariHareketService = cariHareketService;
        }

        // GET: Kasa
        /// <summary>
        /// Tüm aktif kasa kayıtlarını listeler
        /// </summary>
        public async Task<IActionResult> Index()
        {
            try
            {
                // Silinmemiş (soft delete olmamış) kasa kayıtlarını getir
                var kasalar = await _unitOfWork.Repository<Kasa>().GetAsync(
                    filter: k => !k.Silindi,
                    orderBy: q => q.OrderBy(k => k.KasaAdi)
                );

                // ViewModel'e dönüştürme işlemi
                var viewModel = new KasaListViewModel
                {
                    Kasalar = kasalar.Select(k => new KasaViewModel
                    {
                        KasaID = k.KasaID,
                        KasaAdi = k.KasaAdi,
                        KasaTuru = k.KasaTuru ?? "Genel",
                        ParaBirimi = k.ParaBirimi,
                        AcilisBakiye = k.AcilisBakiye,
                        GuncelBakiye = k.GuncelBakiye,
                        Aciklama = k.Aciklama,
                        Aktif = k.Aktif,
                        OlusturmaTarihi = k.OlusturmaTarihi
                    }).ToList(),
                    ToplamBakiye = kasalar.Sum(k => k.GuncelBakiye) // Toplam bakiyeyi hesapla
                };

                return View(viewModel);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Kasa listesi getirilirken hata oluştu");
                return View(new KasaListViewModel { Kasalar = new List<KasaViewModel>() });
            }
        }

        // GET: Kasa/Details/5
        /// <summary>
        /// Belirli bir kasa kaydının detaylarını gösterir
        /// </summary>
        public async Task<IActionResult> Details(Guid? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            try
            {
                // ID'ye göre kasa kaydını getir
                var kasa = await _unitOfWork.Repository<Kasa>().GetByIdAsync(id.Value);
                if (kasa == null || kasa.Silindi)
                {
                    return NotFound();
                }

                // Entity'yi ViewModel'e dönüştür
                var viewModel = new KasaViewModel
                {
                    KasaID = kasa.KasaID,
                    KasaAdi = kasa.KasaAdi ?? string.Empty,
                    KasaTuru = kasa.KasaTuru ?? "Genel",
                    ParaBirimi = kasa.ParaBirimi ?? "TRY",
                    AcilisBakiye = kasa.AcilisBakiye,
                    GuncelBakiye = kasa.GuncelBakiye,
                    Aciklama = kasa.Aciklama ?? string.Empty,
                    Aktif = kasa.Aktif,
                    OlusturmaTarihi = kasa.OlusturmaTarihi
                };

                return View(viewModel);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Kasa detayı getirilirken hata oluştu. Kasa ID: {KasaId}", id);
                return NotFound();
            }
        }

        // GET: Kasa/Create
        /// <summary>
        /// Yeni kasa oluşturma formunu gösterir
        /// </summary>
        public async Task<IActionResult> Create()
        {
            // Para birimi listesini veritabanından al
            ViewBag.ParaBirimleri = await _context.ParaBirimleri
                .Where(p => p.Aktif)
                .OrderBy(p => p.Sira)
                .Select(p => new SelectListItem { Value = p.Kod, Text = $"{p.Ad} ({p.Kod})" })
                .ToListAsync();

            return View();
        }

        // POST: Kasa/Create
        /// <summary>
        /// Yeni kasa kaydı oluşturur
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(KasaCreateViewModel viewModel)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    // Yeni kasa nesnesi oluştur
                    var kasa = new Kasa
                    {
                        KasaID = Guid.NewGuid(),
                        KasaAdi = viewModel.KasaAdi,
                        KasaTuru = "Standart", // Varsayılan kasa türü
                        ParaBirimi = viewModel.ParaBirimi,
                        Aciklama = viewModel.Aciklama,
                        GuncelBakiye = viewModel.AcilisBakiye,
                        Aktif = viewModel.Aktif,
                        Silindi = false,
                        OlusturmaTarihi = DateTime.Now,
                        OlusturanKullaniciID = GetCurrentUserId(),
                        GuncellemeTarihi = DateTime.Now,
                        SonGuncelleyenKullaniciID = GetCurrentUserId()
                    };

                    // Veritabanına ekle ve kaydet
                    await _unitOfWork.Repository<Kasa>().AddAsync(kasa);
                    await _unitOfWork.CompleteAsync();

                    // İşlemi logla
                    await _logService.Log(
                        $"Yeni kasa oluşturuldu: Kasa ID: {kasa.KasaID}, Kasa Adı: {kasa.KasaAdi}",
                        Enums.LogTuru.Bilgi
                    );

                    return RedirectToAction(nameof(Index));
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Kasa oluşturulurken hata oluştu");
                    ModelState.AddModelError("", "Kasa kaydı oluşturulurken bir hata oluştu. Lütfen tekrar deneyin.");
                }
            }
            
            // Hata durumunda para birimi listesini tekrar yükle
            ViewBag.ParaBirimleri = await _context.ParaBirimleri
                .Where(p => p.Aktif)
                .OrderBy(p => p.Sira)
                .Select(p => new SelectListItem { Value = p.Kod, Text = $"{p.Ad} ({p.Kod})" })
                .ToListAsync();
                
            return View(viewModel);
        }

        // GET: Kasa/Edit/5
        /// <summary>
        /// Kasa düzenleme formunu gösterir
        /// </summary>
        public async Task<IActionResult> Edit(Guid? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            try
            {
                // ID'ye göre kasa kaydını getir
                var kasa = await _unitOfWork.Repository<Kasa>().GetByIdAsync(id.Value);
                if (kasa == null || kasa.Silindi)
                {
                    return NotFound();
                }

                // Kasa verisini view model'e aktar
                var viewModel = new KasaEditViewModel
                {
                    KasaID = kasa.KasaID,
                    KasaAdi = kasa.KasaAdi ?? string.Empty,
                    KasaTuru = kasa.KasaTuru ?? "Genel",
                    ParaBirimi = kasa.ParaBirimi ?? "TRY",
                    AcilisBakiye = kasa.AcilisBakiye,
                    GuncelBakiye = kasa.GuncelBakiye,
                    Aciklama = kasa.Aciklama ?? string.Empty,
                    Aktif = kasa.Aktif
                };

                return View(viewModel);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Kasa düzenleme sayfası yüklenirken hata oluştu. Kasa ID: {KasaId}", id);
                return NotFound();
            }
        }

        // POST: Kasa/Edit/5
        /// <summary>
        /// Kasa bilgilerini günceller
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(Guid id, KasaViewModel viewModel)
        {
            if (id != viewModel.KasaID)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    // Mevcut kasayı getir
                    var kasa = await _unitOfWork.Repository<Kasa>().GetByIdAsync(id);
                    if (kasa == null || kasa.Silindi)
                    {
                        return NotFound();
                    }

                    // View model'den gelen değerlerle güncelle
                    kasa.KasaAdi = viewModel.KasaAdi;
                    kasa.Aciklama = viewModel.Aciklama;
                    kasa.ParaBirimi = viewModel.ParaBirimi;
                    kasa.GuncellemeTarihi = DateTime.Now;
                    kasa.SonGuncelleyenKullaniciID = GetCurrentUserId();

                    // Veritabanını güncelle
                    await _unitOfWork.CompleteAsync();
                    
                    // İşlemi logla
                    await _logService.Log(
                        $"Kasa güncellendi: Kasa ID: {kasa.KasaID}, Kasa Adı: {kasa.KasaAdi}",
                        Enums.LogTuru.Bilgi
                    );

                    return RedirectToAction(nameof(Index));
                }
                catch (DbUpdateConcurrencyException ex)
                {
                    // Eş zamanlı güncelleme hatası
                    _logger.LogError(ex, "Kasa güncellenirken eş zamanlı erişim hatası oluştu. Kasa ID: {KasaId}", id);
                    if (!await KasaExists(id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        ModelState.AddModelError("", "Kasa kaydı başka bir kullanıcı tarafından güncellenmiş olabilir. Lütfen sayfayı yenileyip tekrar deneyin.");
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Kasa güncellenirken hata oluştu. Kasa ID: {KasaId}", id);
                    ModelState.AddModelError("", "Kasa güncelleme işlemi sırasında bir hata oluştu. Lütfen tekrar deneyin.");
                }
            }
            return View(viewModel);
        }

        // GET: Kasa/Delete/5
        /// <summary>
        /// Kasa silme onay sayfasını gösterir
        /// </summary>
        public async Task<IActionResult> Delete(Guid? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            try
            {
                // ID'ye göre kasa kaydını getir
                var kasa = await _unitOfWork.Repository<Kasa>().GetByIdAsync(id.Value);
                if (kasa == null || kasa.Silindi)
                {
                    return NotFound();
                }

                // Entity'yi ViewModel'e dönüştür
                var viewModel = new KasaViewModel
                {
                    KasaID = kasa.KasaID,
                    KasaAdi = kasa.KasaAdi ?? string.Empty,
                    KasaTuru = kasa.KasaTuru ?? "Genel",
                    ParaBirimi = kasa.ParaBirimi ?? "TRY",
                    AcilisBakiye = kasa.AcilisBakiye,
                    GuncelBakiye = kasa.GuncelBakiye,
                    Aciklama = kasa.Aciklama ?? string.Empty,
                    Aktif = kasa.Aktif,
                    OlusturmaTarihi = kasa.OlusturmaTarihi
                };

                return View(viewModel);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Kasa silme sayfası yüklenirken hata oluştu. Kasa ID: {KasaId}", id);
                return NotFound();
            }
        }

        // POST: Kasa/Delete/5
        /// <summary>
        /// Kasayı soft delete olarak işaretler (veritabanından tamamen silmez)
        /// </summary>
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(Guid id)
        {
            try
            {
                // ID'ye göre kasa kaydını getir
                var kasa = await _unitOfWork.Repository<Kasa>().GetByIdAsync(id);
                if (kasa == null)
                {
                    return NotFound();
                }

                // Soft delete işlemi
                kasa.Silindi = true;
                kasa.GuncellemeTarihi = DateTime.Now;
                kasa.SonGuncelleyenKullaniciID = GetCurrentUserId();

                // Veritabanını güncelle
                await _unitOfWork.CompleteAsync();
                
                // İşlemi logla
                await _logService.Log(
                    $"Kasa silindi: Kasa ID: {kasa.KasaID}, Kasa Adı: {kasa.KasaAdi}",
                    Enums.LogTuru.Bilgi
                );

                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Kasa silinirken hata oluştu. Kasa ID: {KasaId}", id);
                ModelState.AddModelError("", "Kasa silme işlemi sırasında bir hata oluştu. Lütfen tekrar deneyin.");
                return RedirectToAction(nameof(Index));
            }
        }

        /// <summary>
        /// Belirtilen ID'ye sahip kasa kaydının var olup olmadığını kontrol eder
        /// </summary>
        private async Task<bool> KasaExists(Guid id)
        {
            var kasalar = await _unitOfWork.Repository<Kasa>().GetAsync(
                filter: e => e.KasaID == id && !e.Silindi
            );
            return kasalar.Any();
        }

        /// <summary>
        /// Şu anki giriş yapmış kullanıcının ID'sini döndürür
        /// </summary>
        private new Guid? GetCurrentUserId()
        {
            var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            return userId != null ? Guid.Parse(userId) : (Guid?)null;
        }

        /// <summary>
        /// Belirli bir kasanın hareketlerini listeler
        /// </summary>
        public async Task<IActionResult> Hareketler(Guid? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            try
            {
                // ID'ye göre kasa kaydını getir
                var kasa = await _unitOfWork.Repository<Kasa>().GetByIdAsync(id.Value);
                if (kasa == null || kasa.Silindi)
                {
                    return NotFound();
                }

                // Kasaya ait hareketleri getir
                var hareketler = await _unitOfWork.Repository<KasaHareket>().GetAsync(
                    filter: h => h.KasaID == id && !h.Silindi,
                    orderBy: q => q.OrderByDescending(h => h.Tarih)
                );

                // Entity'leri ViewModel'e dönüştür
                var viewModel = hareketler.Select(h => new KasaHareketViewModel
                {
                    KasaHareketID = h.KasaHareketID,
                    KasaID = h.KasaID,
                    KasaAdi = kasa.KasaAdi ?? string.Empty,
                    Tutar = h.Tutar,
                    HareketTuru = h.HareketTuru ?? string.Empty,
                    Tarih = h.Tarih,
                    ReferansNo = h.ReferansNo ?? string.Empty,
                    ReferansTuru = h.ReferansTuru ?? string.Empty,
                    Aciklama = h.Aciklama ?? string.Empty,
                    ParaBirimi = kasa.ParaBirimi ?? "TRY",
                    CariID = h.CariID,
                    CariAdi = h.Cari?.Ad ?? string.Empty,
                    IslemTuru = h.IslemTuru ?? string.Empty,
                    TransferID = h.TransferID,
                    HedefKasaID = h.HedefKasaID
                }).ToList();

                // Kasa bilgisini ViewBag üzerinden gönder
                var kasaViewModel = new KasaViewModel
                {
                    KasaID = kasa.KasaID,
                    KasaAdi = kasa.KasaAdi ?? string.Empty,
                    KasaTuru = kasa.KasaTuru ?? "Genel",
                    ParaBirimi = kasa.ParaBirimi ?? "TRY",
                    AcilisBakiye = kasa.AcilisBakiye,
                    GuncelBakiye = kasa.GuncelBakiye,
                    Aciklama = kasa.Aciklama ?? string.Empty,
                    Aktif = kasa.Aktif,
                    OlusturmaTarihi = kasa.OlusturmaTarihi
                };
                
                ViewBag.Kasa = kasaViewModel;
                return View(viewModel);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Kasa hareketleri listelenirken hata oluştu. Kasa ID: {KasaId}", id);
                return NotFound();
            }
        }

        /// <summary>
        /// Yeni kasa hareketi oluşturma formunu gösterir
        /// </summary>
        public async Task<IActionResult> YeniHareket(Guid? id = null, Guid? cariId = null)
        {
            try
            {
                // Kasaları getir
                var kasalar = await _unitOfWork.Repository<Kasa>().GetAsync(
                    filter: k => !k.Silindi && k.Aktif,
                    orderBy: q => q.OrderBy(k => k.KasaAdi)
                );

                if (!kasalar.Any())
                {
                    TempData["ErrorMessage"] = "Sistemde kayıtlı aktif kasa bulunamadı. Lütfen önce bir kasa ekleyin.";
                    return RedirectToAction(nameof(Index));
                }

                // Carileri getir
                var cariler = await _unitOfWork.CariRepository.GetAll()
                    .IgnoreQueryFilters() // Filtreleri devre dışı bırak
                    .Where(c => !c.Silindi && c.AktifMi)
                    .OrderBy(c=>c.Ad)
                    .Include(c=>c.VarsayilanParaBirimi)
                    .ToListAsync();

                ViewBag.Kasalar = kasalar.ToList();
                ViewBag.Cariler = cariler.ToList();

                // İşlem türleri için dropdown hazırla
                ViewBag.IslemTurleri = new List<SelectListItem>
                {
                    new SelectListItem { Text = "Giriş", Value = "Giriş" },
                    new SelectListItem { Text = "Çıkış", Value = "Çıkış" }
                };

                // Yeni ViewModel oluştur
                var viewModel = new KasaHareketViewModel
                {
                    KasaHareketID = Guid.NewGuid(),
                    Tarih = DateTime.Now,
                    ReferansNo = "KAS-" + DateTime.Now.ToString("yyMMdd") + "-" + new Random().Next(100, 999).ToString(),
                    HareketTuru = "Giriş", // Varsayılan değer
                    Tutar = 0,
                    CariIleDengelensin = false,
                    KasaID = id ?? Guid.Empty
                };

                // Kasa ID varsa
                if (id.HasValue && id.Value != Guid.Empty)
                {
                    var kasa = await _unitOfWork.Repository<Kasa>().GetByIdAsync(id.Value);
                    if (kasa != null && !kasa.Silindi)
                    {
                        viewModel.KasaID = kasa.KasaID;
                        ViewBag.SecilenKasa = kasa;
                    }
                }

                // Cari ID varsa
                if (cariId.HasValue && cariId.Value != Guid.Empty)
                {
                    var cari = await _unitOfWork.Repository<Cari>().GetByIdAsync(cariId.Value);
                    if (cari != null && !cari.Silindi)
                    {
                        viewModel.CariID = cari.CariID;
                        ViewBag.SecilenCari = cari;
                    }
                }

                // AJAX isteği ise partial view döndür
                if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
                {
                    return PartialView("_YeniHareketPartial", viewModel);
                }

                return View(viewModel);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Yeni kasa hareketi sayfası yüklenirken hata oluştu.");
                TempData["ErrorMessage"] = "Yeni kasa hareketi sayfası yüklenirken bir hata oluştu.";
                return RedirectToAction(nameof(Index));
            }
        }

        /// <summary>
        /// Yeni kasa hareketi kaydeder ve kasa bakiyesini günceller
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> YeniHareket(KasaHareketViewModel viewModel)
        {
            // AJAX isteği kontrolü
            bool isAjaxRequest = Request.Headers["X-Requested-With"] == "XMLHttpRequest";
            
            if (ModelState.IsValid)
            {
                try
                {
                    // Veritabanı işlemleri için transaction başlat
                    using var transaction = await _context.Database.BeginTransactionAsync();

                    try {
                        // Mevcut kasayı getir
                        var kasa = await _unitOfWork.Repository<Kasa>().GetByIdAsync(viewModel.KasaID);
                        if (kasa == null || kasa.Silindi)
                        {
                            if (isAjaxRequest)
                                return Json(new { success = false, message = "Kasa bulunamadı veya silinmiş durumda." });
                            return NotFound();
                        }

                        // Yeni hareket oluştur
                        var hareket = new KasaHareket
                        {
                            KasaHareketID = Guid.NewGuid(),
                            KasaID = viewModel.KasaID,
                            Tarih = DateTime.Now,
                            DovizKuru = viewModel.DovizKuru ?? 1,
                            IslemTuru = viewModel.HareketTuru == "Giriş" ? "Tahsilat" : "Ödeme",
                            HareketTuru = viewModel.HareketTuru,
                            Tutar = viewModel.Tutar,
                            Aciklama = $"Kasa hareketi: {viewModel.Aciklama}",
                            ReferansNo = $"KAS-{DateTime.Now:yyMMdd}-{new Random().Next(100, 999)}",
                            ReferansTuru = viewModel.HareketTuru == "Giriş" ? "Tahsilat" : "Ödeme",
                            CariID = viewModel.CariID,
                            OlusturmaTarihi = DateTime.Now,
                            IslemYapanKullaniciID = GetCurrentUserId(),
                            KarsiParaBirimi = viewModel.KarsiParaBirimi ?? kasa.ParaBirimi ?? "TRY",
                            Silindi = false
                        };

                        // Kasa bakiyesini güncelle
                        if (viewModel.HareketTuru == "Giriş")
                        {
                            kasa.GuncelBakiye += viewModel.Tutar;
                        }
                        else
                        {
                            kasa.GuncelBakiye -= viewModel.Tutar;
                        }

                        kasa.GuncellemeTarihi = DateTime.Now;
                        kasa.SonGuncelleyenKullaniciID = GetCurrentUserId();

                        // Değişiklikleri kaydet
                        await _unitOfWork.Repository<KasaHareket>().AddAsync(hareket);
                        await _unitOfWork.CompleteAsync();

                        // Cari hareketi oluştur (eğer CariIleDengelensin işaretlenmişse ve CariID mevcutsa)
                        if (viewModel.CariIleDengelensin && viewModel.CariID.HasValue && viewModel.CariID != Guid.Empty)
                        {
                            bool borcMu = viewModel.HareketTuru == "Giriş" ? false : true;
                            await _cariHareketService.CreateFromKasaHareketAsync(hareket, borcMu);
                        }

                        // Transaction'ı tamamla
                        await transaction.CommitAsync();
                        
                        // İşlemi logla
                        await _logService.Log(
                            $"Yeni kasa hareketi oluşturuldu: Kasa: {kasa.KasaAdi}, İşlem: {viewModel.HareketTuru}, Tutar: {viewModel.Tutar} {kasa.ParaBirimi}",
                            Enums.LogTuru.Bilgi
                        );

                        if (isAjaxRequest)
                        {
                            // AJAX yanıtı
                            return Json(new { 
                                success = true, 
                                message = "Kasa hareketi başarıyla oluşturuldu.", 
                                redirectUrl = Url.Action("Hareketler", new { id = viewModel.KasaID }),
                                kasaId = viewModel.KasaID
                            });
                        }

                        // Normal form gönderimi yanıtı
                        TempData["SuccessMessage"] = "Kasa hareketi başarıyla oluşturuldu.";
                        return RedirectToAction(nameof(Hareketler), new { id = viewModel.KasaID });
                    }
                    catch (Exception ex)
                    {
                        // Hata durumunda transaction'ı geri al
                        await transaction.RollbackAsync();
                        throw ex;
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Kasa hareketi oluşturulurken hata oluştu. Kasa ID: {KasaId}", viewModel.KasaID);
                    
                    if (isAjaxRequest)
                    {
                        var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList();
                        return Json(new { 
                            success = false, 
                            message = "Kasa hareketi oluşturulurken bir hata oluştu: " + ex.Message, 
                            errors = errors
                        });
                    }
                    
                    ModelState.AddModelError("", "Kasa hareketi oluşturulurken bir hata oluştu. Lütfen tekrar deneyin.");
                }
            }
            else if (isAjaxRequest)
            {
                var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList();
                return Json(new { 
                    success = false, 
                    message = "Geçersiz form verileri, lütfen kontrol ediniz.", 
                    errors = errors 
                });
            }

            // Hata durumunda dropdown'ları yeniden hazırla
            ViewBag.IslemTurleri = new List<SelectListItem>
            {
                new SelectListItem { Text = "Giriş", Value = "Giriş" },
                new SelectListItem { Text = "Çıkış", Value = "Çıkış" }
            };

            var kasalar = await _unitOfWork.Repository<Kasa>().GetAsync(
                    filter: k => !k.Silindi && k.Aktif,
                    orderBy: q => q.OrderBy(k => k.KasaAdi)
                );

            if (!kasalar.Any())
            {
                TempData["ErrorMessage"] = "Sistemde kayıtlı aktif kasa bulunamadı. Lütfen önce bir kasa ekleyin.";
                return RedirectToAction(nameof(Index));
            }

            // Carileri getir
            var cariler = await _unitOfWork.CariRepository.GetAll()
                .IgnoreQueryFilters() // Filtreleri devre dışı bırak
                .Where(c => !c.Silindi && c.AktifMi)
                .OrderBy(c => c.Ad)
                .Include(c => c.VarsayilanParaBirimi)
                .ToListAsync();

            ViewBag.Kasalar = kasalar.ToList();
            ViewBag.Cariler = cariler.ToList();
            return View(viewModel);
        }

        /// <summary>
        /// Kasalar arası para transferi formunu gösterir
        /// </summary>
        public async Task<IActionResult> Transfer()
        {
            try
            {
                // Transfer için kasa dropdown'larını hazırla
                await PrepareTransferViewBag();
                return View(new KasaTransferViewModel
                {
                    IslemTarihi = DateTime.Now,
                    KaynakTutar = 0,
                    HedefTutar = 0,
                    KurDegeri = 1
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Transfer sayfası yüklenirken hata oluştu");
                return RedirectToAction(nameof(Index));
            }
        }

        /// <summary>
        /// Kasalar arası para transferi işlemini gerçekleştirir
        /// Farklı para birimlerinde kur dönüşümü yapar
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Transfer(KasaTransferViewModel viewModel)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    // Kaynak ve hedef kasaları getir
                    var kaynakKasa = await _unitOfWork.Repository<Kasa>().GetByIdAsync(viewModel.KaynakKasaID);
                    var hedefKasa = await _unitOfWork.Repository<Kasa>().GetByIdAsync(viewModel.HedefKasaID);
                    
                    if (kaynakKasa == null || hedefKasa == null || kaynakKasa.Silindi || hedefKasa.Silindi)
                    {
                        ModelState.AddModelError("", "Kaynak veya hedef kasa bulunamadı.");
                        await PrepareTransferViewBag();
                        return View(viewModel);
                    }

                    // Aynı kasaya transfer kontrolü
                    if (kaynakKasa.KasaID == hedefKasa.KasaID)
                    {
                        ModelState.AddModelError("", "Kaynak ve hedef kasa aynı olamaz.");
                        await PrepareTransferViewBag();
                        return View(viewModel);
                    }

                    // Kaynak kasada yeterli bakiye var mı kontrolü
                    if (kaynakKasa.GuncelBakiye < viewModel.KaynakTutar)
                    {
                        ModelState.AddModelError("", $"Kaynak kasada yeterli bakiye yok. Mevcut bakiye: {kaynakKasa.GuncelBakiye} {kaynakKasa.ParaBirimi}");
                        await PrepareTransferViewBag();
                        return View(viewModel);
                    }

                    // Transfer tutarını hesapla
                    decimal transferTutari = viewModel.KaynakTutar;
                    decimal hedefTutar = viewModel.HedefTutar;

                    // Para birimleri farklıysa kur dönüşümü kullan
                    if (kaynakKasa.ParaBirimi != hedefKasa.ParaBirimi && viewModel.KurDegeri != 1)
                    {
                        // Kur değeri doğrudan view model'den alınıyor
                        hedefTutar = viewModel.HedefTutar;
                    }
                    else if (kaynakKasa.ParaBirimi == hedefKasa.ParaBirimi)
                    {
                        // Aynı para biriminde 1:1 transfer
                        hedefTutar = transferTutari;
                    }

                    // Kaynak kasadan çıkış hareketi
                    var kaynakHareket = new KasaHareket
                    {
                        KasaHareketID = Guid.NewGuid(),
                        KasaID = kaynakKasa.KasaID,
                        Tarih = viewModel.IslemTarihi,
                        HareketTuru = "Çıkış",
                        IslemTuru = "KasaTransfer",
                        Tutar = transferTutari,
                        Aciklama = $"{hedefKasa.KasaAdi} kasasına transfer: {viewModel.Aciklama}",
                        OlusturmaTarihi = DateTime.Now,
                        IslemYapanKullaniciID = GetCurrentUserId(),
                        TransferID = viewModel.TransferID,
                        HedefKasaID = hedefKasa.KasaID,
                        Silindi = false
                    };

                    // Hedef kasaya giriş hareketi
                    var hedefHareket = new KasaHareket
                    {
                        KasaHareketID = Guid.NewGuid(),
                        KasaID = hedefKasa.KasaID,
                        Tarih = viewModel.IslemTarihi,
                        HareketTuru = "Giriş",
                        IslemTuru = "KasaTransfer",
                        Tutar = hedefTutar,
                        Aciklama = $"{kaynakKasa.KasaAdi} kasasından transfer: {viewModel.Aciklama}",
                        OlusturmaTarihi = DateTime.Now,
                        IslemYapanKullaniciID = GetCurrentUserId(),
                        TransferID = viewModel.TransferID,
                        KaynakBankaID = null,
                        Silindi = false
                    };

                    // Kasa bakiyelerini güncelle
                    kaynakKasa.GuncelBakiye -= transferTutari;
                    hedefKasa.GuncelBakiye += hedefTutar;

                    kaynakKasa.GuncellemeTarihi = DateTime.Now;
                    kaynakKasa.SonGuncelleyenKullaniciID = GetCurrentUserId();
                    hedefKasa.GuncellemeTarihi = DateTime.Now;
                    hedefKasa.SonGuncelleyenKullaniciID = GetCurrentUserId();

                    // Değişiklikleri kaydet
                    await _unitOfWork.Repository<KasaHareket>().AddAsync(kaynakHareket);
                    await _unitOfWork.Repository<KasaHareket>().AddAsync(hedefHareket);
                    await _unitOfWork.CompleteAsync();
                    
                    // İşlemi logla
                    await _logService.Log(
                        $"Kasa transferi yapıldı: Kaynak: {kaynakKasa.KasaAdi}, Hedef: {hedefKasa.KasaAdi}, Tutar: {viewModel.KaynakTutar} {kaynakKasa.ParaBirimi}",
                        Enums.LogTuru.Bilgi
                    );

                    return RedirectToAction(nameof(Index));
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Kasa transferi sırasında hata oluştu");
                    ModelState.AddModelError("", "Transfer işlemi sırasında bir hata oluştu. Lütfen tekrar deneyin.");
                }
            }

            // Hata durumunda dropdown'ları yeniden hazırla
            await PrepareTransferViewBag();
            return View(viewModel);
        }

        /// <summary>
        /// Transfer sayfası için kasa dropdown'larını hazırlar
        /// </summary>
        private async Task PrepareTransferViewBag()
        {
            // Aktif kasaları getir
            ViewBag.Kasalar = await _context.Kasalar
                .Where(k => !k.Silindi && k.Aktif)
                .ToListAsync();
            
            // Aktif banka hesaplarını getir
            ViewBag.BankaHesaplari = await _context.BankaHesaplari
                .Include(bh => bh.Banka)
                .Where(bh => !bh.Silindi && bh.Aktif)
                .ToListAsync();

            // Döviz kurları
            ViewBag.Kurlar = await _context.KurDegerleri
                .Include(k => k.ParaBirimi)
                .Where(k => k.ParaBirimi.Aktif && !k.ParaBirimi.Silindi)
                .Where(k => k.Tarih.Date == DateTime.Today.Date)
                .Select(k => new MuhasebeStokWebApp.ViewModels.Doviz.DovizKuruViewModel
                {
                    DovizKodu = k.ParaBirimi.Kod,
                    AlisFiyati = k.Alis,
                    SatisFiyati = k.Satis
                })
                .ToListAsync();
        }

        /// <summary>
        /// Tarihe göre kasa hareketleri raporu oluşturma formunu gösterir
        /// </summary>
        public IActionResult HareketlerTarih()
        {
            var viewModel = new KasaHareketTarihViewModel
            {
                BaslangicTarihi = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1),
                BitisTarihi = DateTime.Now
            };
            return View(viewModel);
        }

        /// <summary>
        /// Seçilen tarih aralığında kasa hareketleri raporunu oluşturur
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> HareketlerTarih(KasaHareketTarihViewModel viewModel)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    // Seçilen tarih aralığındaki hareketleri getir
                    var hareketler = await _unitOfWork.Repository<KasaHareket>().GetAsync(
                        filter: h => !h.Silindi && h.Tarih >= viewModel.BaslangicTarihi && h.Tarih <= viewModel.BitisTarihi,
                        orderBy: q => q.OrderByDescending(h => h.Tarih)
                    );

                    // Include işlemi ayrıca yapılmalı
                    foreach (var hareket in hareketler)
                    {
                        await _unitOfWork.Repository<Kasa>().GetByIdAsync(hareket.KasaID);
                    }

                    // Kasa hareketlerini gruplandır ve özet bilgileri hesapla
                    var ozet = hareketler
                        .GroupBy(h => h.KasaID)
                        .Select(g => new KasaHareketOzetViewModel
                        {
                            KasaID = g.Key,
                            KasaAdi = g.First().Kasa?.KasaAdi ?? "Bilinmeyen Kasa",
                            ParaBirimi = g.First().Kasa?.ParaBirimi ?? "TRY",
                            ToplamGiris = g.Where(h => h.HareketTuru == "Giriş").Sum(h => h.Tutar),
                            ToplamCikis = g.Where(h => h.HareketTuru == "Çıkış").Sum(h => h.Tutar),
                            NetBakiye = g.Where(h => h.HareketTuru == "Giriş").Sum(h => h.Tutar) - g.Where(h => h.HareketTuru == "Çıkış").Sum(h => h.Tutar)
                        })
                        .ToList();

                    // Sonuçları view model'e aktar
                    viewModel.Hareketler = hareketler.ToList();
                    viewModel.Ozet = ozet;

                    return View(viewModel);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Kasa hareketleri raporu alınırken hata oluştu. Başlangıç: {BaslangicTarihi}, Bitiş: {BitisTarihi}", 
                        viewModel.BaslangicTarihi, viewModel.BitisTarihi);
                    ModelState.AddModelError("", "Kasa hareketleri raporu alınırken bir hata oluştu. Lütfen tekrar deneyin.");
                }
            }
            return View(viewModel);
        }

        // GET: Kasa/HesapHareketler/5
        [HttpPost]
        public async Task<IActionResult> GetKasaHareketler(Guid id)
        {
            try
            {
                // Kasa bilgisini bul
                var kasa = await _unitOfWork.Repository<Kasa>().GetByIdAsync(id);

                if (kasa == null || kasa.Silindi)
                {
                    return Json(new { error = "Kasa bulunamadı." });
                }

                // Hareketleri getir
                var hareketler = _context.KasaHareketleri
                    .Include(h => h.Kasa)
                    .Include(h => h.Cari)
                    .Where(h => h.KasaID == id && !h.Silindi)
                    .AsQueryable();

                // Arama yapılmışsa filtreleme
                var search = Request.Query["search[value]"].FirstOrDefault();
                if (!string.IsNullOrEmpty(search))
                {
                    hareketler = hareketler.Where(h =>
                        (h.ReferansNo != null && h.ReferansNo.Contains(search)) ||
                        h.Tarih.ToString().Contains(search) ||
                        (h.HareketTuru != null && h.HareketTuru.Contains(search)) ||
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
                var start = Request.Query["start"].FirstOrDefault();
                var length = Request.Query["length"].FirstOrDefault();
                int skip = start != null ? Convert.ToInt32(start) : 0;
                int take = length != null ? Convert.ToInt32(length) : 10;
                
                var data = await hareketler.Skip(skip).Take(take).ToListAsync();
                
                var result = data.Select(h => new {
                    kasaHareketID = h.KasaHareketID,
                    tarih = h.Tarih.ToString("dd.MM.yyyy HH:mm"),
                    hareketTuru = h.HareketTuru ?? string.Empty,
                    referansNo = h.ReferansNo ?? string.Empty,
                    referansTuru = h.ReferansTuru ?? string.Empty,
                    aciklama = h.Aciklama ?? string.Empty,
                    tutar = h.Tutar,
                    cariUnvani = h.Cari?.Ad ?? string.Empty
                }).ToList();
                
                return Json(new
                {
                    draw = Request.Query["draw"].FirstOrDefault() ?? "1",
                    recordsFiltered = recordsFiltered,
                    recordsTotal = totalRecords,
                    data = result
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "GetKasaHareketler çağrılırken hata oluştu. KasaID: {Id}", id);
                return Json(new { 
                    draw = Request.Query["draw"].FirstOrDefault() ?? "1",
                    recordsFiltered = 0,
                    recordsTotal = 0,
                    data = new List<object>() 
                });
            }
        }

        /// <summary>
        /// Kasa-Banka arası transfer işlemleri için modal formu açar
        /// </summary>
        public async Task<IActionResult> YeniTransfer(Guid? kasaId = null)
        {
            // Kasaları getir
            var kasalar = await _unitOfWork.Repository<Kasa>().GetAsync(
                filter: k => !k.Silindi && k.Aktif,
                orderBy: q => q.OrderBy(k => k.KasaAdi)
            );

            if (!kasalar.Any())
            {
                return Json(new { success = false, message = "Sistemde kayıtlı aktif kasa bulunamadı. Lütfen önce bir kasa ekleyin." });
            }

            // Banka hesaplarını getir
            var bankaHesaplari = await _context.BankaHesaplari
                .Include(h => h.Banka)
                .Where(h => !h.Silindi && h.Aktif)
                .OrderBy(h => h.Banka.BankaAdi)
                .ThenBy(h => h.HesapAdi)
                .ToListAsync();

            ViewBag.Kasalar = kasalar.ToList();
            ViewBag.BankaHesaplari = bankaHesaplari;

            var model = new MuhasebeStokWebApp.ViewModels.Transfer.IcTransferViewModel
            {
                TransferTuru = "KasadanKasaya",
                Tarih = DateTime.Now,
                ReferansNo = "TRF-" + DateTime.Now.ToString("yyMMdd") + "-" + new Random().Next(100, 999).ToString(),
                KaynakKasaID = kasaId
            };

            return PartialView("_TransferModalPartial", model);
        }

        // GET: Kasa/HareketDetay/5
        public async Task<IActionResult> HareketDetay(Guid? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            try
            {
                // ID'ye göre kasa hareketini getir
                var hareket = await _unitOfWork.Repository<KasaHareket>().GetByIdAsync(id.Value);
                if (hareket == null || hareket.Silindi)
                {
                    return NotFound();
                }

                // İlgili kasa bilgisini getir
                var kasa = await _unitOfWork.Repository<Kasa>().GetByIdAsync(hareket.KasaID);
                if (kasa == null || kasa.Silindi)
                {
                    return NotFound();
                }

                // Entity'yi ViewModel'e dönüştür
                var viewModel = new KasaHareketViewModel
                {
                    KasaHareketID = hareket.KasaHareketID,
                    KasaID = hareket.KasaID,
                    KasaAdi = kasa.KasaAdi,
                    HareketTuru = hareket.HareketTuru,
                    Tutar = hareket.Tutar,
                    Tarih = hareket.Tarih,
                    ReferansNo = hareket.ReferansNo,
                    ReferansTuru = hareket.ReferansTuru,
                    Aciklama = hareket.Aciklama,
                    ParaBirimi = kasa.ParaBirimi,
                    CariID = hareket.CariID,
                    IslemTuru = hareket.IslemTuru,
                    TransferID = hareket.TransferID,
                    HedefKasaID = hareket.HedefKasaID,
                    KaynakBankaID = hareket.KaynakBankaID,
                    DovizKuru = hareket.DovizKuru,
                    KarsiParaBirimi = hareket.KarsiParaBirimi
                };
                // Transfer işlemi mi kontrol et
                bool isTransfer = hareket.IslemTuru == "KasaTransfer";
                
                // Transfer işlemi ise hedef/kaynak detaylarını doldur
                if (isTransfer)
                {
                    if (hareket.HedefKasaID.HasValue)
                    {
                        var hedefKasa = await _unitOfWork.Repository<Kasa>().GetByIdAsync(hareket.HedefKasaID.Value);
                        if (hedefKasa != null && !hedefKasa.Silindi)
                        {
                            viewModel.HedefKasaAdi = hedefKasa.KasaAdi;
                        }
                    }
                    
                    if (hareket.KaynakBankaID.HasValue)
                    {
                        var bankaHesap = await _context.BankaHesaplari
                            .Include(b => b.Banka)
                            .FirstOrDefaultAsync(b => b.BankaHesapID == hareket.KaynakBankaID && !b.Silindi);
                        
                        if (bankaHesap != null)
                        {
                            viewModel.KaynakBankaAdi = $"{bankaHesap.Banka.BankaAdi} - {bankaHesap.HesapAdi}";
                        }
                    }
                }

                // Cari bilgisini getir (varsa)
                if (hareket.CariID.HasValue)
                {
                    var cari = await _unitOfWork.Repository<Cari>().GetByIdAsync(hareket.CariID.Value);
                    if (cari != null && !cari.Silindi)
                    {
                        viewModel.CariAdi = cari.Ad;
                    }
                }

                return View(viewModel);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Kasa hareketi detayı getirilirken hata oluştu. ID: {Id}", id);
                return NotFound();
            }
        }

        // GET: Kasa/HareketDuzenle/5
        public async Task<IActionResult> HareketDuzenle(Guid? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            try
            {
                // ID'ye göre kasa hareketini getir
                var hareket = await _unitOfWork.Repository<KasaHareket>().GetByIdAsync(id.Value);
                if (hareket == null || hareket.Silindi)
                {
                    return NotFound();
                }

                // İlgili kasa bilgisini getir
                var kasa = await _unitOfWork.Repository<Kasa>().GetByIdAsync(hareket.KasaID);
                if (kasa == null || kasa.Silindi)
                {
                    return NotFound();
                }

                // Tüm kasaları getir
                var kasalar = await _unitOfWork.Repository<Kasa>().GetAsync(
                    filter: k => !k.Silindi && k.Aktif,
                    orderBy: q => q.OrderBy(k => k.KasaAdi)
                );

                // Tüm carileri getir
                var cariler = await _unitOfWork.CariRepository.GetAll()
                    .Where(c => !c.Silindi && c.AktifMi)
                    .Include(a=>a.VarsayilanParaBirimi)
                    .OrderBy(c => c.Ad)
                    .ToListAsync();

                ViewBag.Kasalar = kasalar.ToList();
                ViewBag.Cariler = cariler.ToList();
                ViewBag.SecilenKasa = kasa;

                // İşlem türleri için dropdown hazırla
                ViewBag.IslemTurleri = new List<SelectListItem>
                {
                    new SelectListItem { Text = "Giriş", Value = "Giriş" },
                    new SelectListItem { Text = "Çıkış", Value = "Çıkış" }
                };
                decimal? dovizKuru = hareket.DovizKuru;
                string formattedKur = hareket.DovizKuru.ToString("N4", new CultureInfo("tr-TR"));
                decimal? yeniKur = decimal.Parse(formattedKur, new CultureInfo("tr-TR"));
                // Entity'yi ViewModel'e dönüştür
                var viewModel = new KasaHareketViewModel
                {
                    KasaHareketID = hareket.KasaHareketID,
                    KasaID = hareket.KasaID,
                    KasaAdi = kasa.KasaAdi,
                    HareketTuru = hareket.HareketTuru,
                    Tutar = hareket.Tutar,
                    Tarih = hareket.Tarih,
                    ReferansNo = hareket.ReferansNo,
                    ReferansTuru = hareket.ReferansTuru,
                    Aciklama = hareket.Aciklama,
                    ParaBirimi = kasa.ParaBirimi,
                    CariID = hareket.CariID,
                    IslemTuru = hareket.IslemTuru,
                    TransferID = hareket.TransferID,
                    HedefKasaID = hareket.HedefKasaID,
                    KaynakBankaID = hareket.KaynakBankaID,
                    DovizKuru = yeniKur,
                    KarsiParaBirimi = hareket.KarsiParaBirimi,
                    CariIleDengelensin = hareket.CariID.HasValue
                };

                // Transfer işlemi mi kontrol et
                bool isTransfer = hareket.ReferansTuru == "Transfer";
                
                // Transfer işlemi ise hedef/kaynak detaylarını doldur
                if (isTransfer)
                {
                    if (hareket.HedefKasaID.HasValue)
                    {
                        var hedefKasa = await _unitOfWork.Repository<Kasa>().GetByIdAsync(hareket.HedefKasaID.Value);
                        if (hedefKasa != null && !hedefKasa.Silindi)
                        {
                            viewModel.HedefKasaAdi = hedefKasa.KasaAdi;
                        }
                    }
                    
                    if (hareket.KaynakBankaID.HasValue)
                    {
                        var bankaHesap = await _context.BankaHesaplari
                            .Include(b => b.Banka)
                            .FirstOrDefaultAsync(b => b.BankaHesapID == hareket.KaynakBankaID && !b.Silindi);
                        
                        if (bankaHesap != null)
                        {
                            viewModel.KaynakBankaAdi = $"{bankaHesap.Banka.BankaAdi} - {bankaHesap.HesapAdi}";
                        }
                    }
                }

                // Cari bilgisini getir (varsa)
                if (hareket.CariID.HasValue)
                {
                    var cari = await _unitOfWork.CariRepository.GetAll().Include(c=>c.VarsayilanParaBirimi).FirstOrDefaultAsync(c => c.CariID == hareket.CariID.Value);
                    if (cari != null && !cari.Silindi)
                    {
                        viewModel.CariAdi = cari.Ad;
                        ViewBag.SecilenCari = cari;
                    }
                }

                return View(viewModel);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Kasa hareketi düzenleme sayfası yüklenirken hata oluştu. ID: {Id}", id);
                return NotFound();
            }
        }

        // POST: Kasa/HareketDuzenle/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> HareketDuzenle(Guid id, KasaHareketViewModel viewModel)
        {
            if (id != viewModel.KasaHareketID)
            {
                return NotFound();
            }

            // AJAX isteği kontrolü
            bool isAjaxRequest = Request.Headers["X-Requested-With"] == "XMLHttpRequest";

            if (ModelState.IsValid)
            {
                try
                {
                    // Veritabanı işlemleri için transaction başlat
                    using var transaction = await _context.Database.BeginTransactionAsync();

                    try
                    {
                        // Mevcut kasa hareketini getir
                        var hareket = await _unitOfWork.Repository<KasaHareket>().GetByIdAsync(id);
                        if (hareket == null || hareket.Silindi)
                        {
                            if (isAjaxRequest)
                                return Json(new { success = false, message = "Hareket bulunamadı veya silinmiş durumda." });
                            return NotFound();
                        }

                        // Kasa bilgisini getir
                        var kasa = await _unitOfWork.Repository<Kasa>().GetByIdAsync(hareket.KasaID);
                        if (kasa == null || kasa.Silindi)
                        {
                            if (isAjaxRequest)
                                return Json(new { success = false, message = "Kasa bulunamadı veya silinmiş durumda." });
                            return NotFound();
                        }

                        // Önce eski tutarın etkisini geri al
                        if (hareket.HareketTuru == "Giriş")
                        {
                            kasa.GuncelBakiye -= hareket.Tutar;
                        }
                        else
                        {
                            kasa.GuncelBakiye += hareket.Tutar;
                        }

                        // Hareketi güncelle
                        hareket.Tutar = viewModel.Tutar;
                        hareket.HareketTuru = viewModel.HareketTuru;
                        hareket.Tarih = viewModel.Tarih;
                        hareket.Aciklama = viewModel.Aciklama;
                        hareket.CariID = viewModel.CariID;
                        hareket.GuncellemeTarihi = DateTime.Now;
                        hareket.SonGuncelleyenKullaniciID = GetCurrentUserId();

                        // Yeni tutarın etkisini uygula
                        if (hareket.HareketTuru == "Giriş")
                        {
                            kasa.GuncelBakiye += hareket.Tutar;
                        }
                        else
                        {
                            kasa.GuncelBakiye -= hareket.Tutar;
                            }

                        // Değişiklikleri kaydet
                        _unitOfWork.Repository<KasaHareket>().Update(hareket);
                        _unitOfWork.Repository<Kasa>().Update(kasa);
                        await _unitOfWork.CompleteAsync();

                        // Cari hareket güncelleme işlemleri
                        if (viewModel.CariID.HasValue && viewModel.CariID != Guid.Empty)
                        {
                            // Cari hareketi bul veya oluştur
                            var cariHareket = await _cariHareketService.GetByReferenceAsync(hareket.KasaHareketID, "KasaHareket");
                            var tutar = hareket.Tutar * cariHareket.TutarDoviz;
                            if (cariHareket != null)
                            {
                                // Cari hareketi güncelle
                                bool borcMu = hareket.HareketTuru == "Çıkış"; // Kasadan çıkış ise borç
                                cariHareket.Tarih = hareket.Tarih;
                                cariHareket.TutarDoviz = hareket.DovizKuru;
                                cariHareket.Aciklama = $"Kasa hareketi (Güncellendi): {hareket.Aciklama}";
                                cariHareket.Tutar = tutar;
                                cariHareket.Borc = borcMu ? tutar : 0;
                                cariHareket.Alacak = !borcMu ? tutar : 0;
                                cariHareket.GuncellemeTarihi = DateTime.Now;

                                await _cariHareketService.UpdateHareketAsync(cariHareket);
                            }
                            else if (viewModel.CariIleDengelensin)
                            {
                                // Yeni cari hareket oluştur
                                bool borcMu = hareket.HareketTuru == "Çıkış";
                                await _cariHareketService.CreateFromKasaHareketAsync(hareket, borcMu);
                            }
                        }

                        // Transfer işlemlerini güncelle
                        bool isTransfer = string.Equals(hareket.ReferansTuru?.Trim(), "Transfer", StringComparison.OrdinalIgnoreCase);
                        var karsiHareket = "";
                        if (isTransfer && hareket.TransferID.HasValue)
                        {
                            // Transfer detaylarını güncelle
                            if (viewModel.HedefKasaID.HasValue)
                            {
                                // Kasadan kasaya transfer
                                var hedefKasa = await _unitOfWork.Repository<Kasa>().GetByIdAsync(viewModel.HedefKasaID.Value);
                                if (hedefKasa != null && !hedefKasa.Silindi)
                                {
                                    // Hedef kasa hareketini bul
                                    var hedefHareket = await _unitOfWork.Repository<KasaHareket>().GetAsync(
                                        filter: h => h.TransferID == hareket.TransferID && h.KasaID == viewModel.HedefKasaID.Value && !h.Silindi
                                    );
                                    
                                    var hedefKasaHareket = hedefHareket.FirstOrDefault();
                                    if (hedefKasaHareket != null)
                                    {
                                        // Önce eski tutarın etkisini geri al
                                        if (hareket.HareketTuru == "Giriş")
                                        {
                                            hedefKasa.GuncelBakiye += hedefKasaHareket.Tutar;
                                            karsiHareket = "Çıkış";
                                        }
                                        else
                                        {
                                            hedefKasa.GuncelBakiye -= hedefKasaHareket.Tutar;
                                            karsiHareket = "Giriş";
                                        }
                                        

                                        // Hesaplanan kur ile hedef tutarını güncelle
                                        decimal hedefTutar = viewModel.Tutar;
                                        if (viewModel.DovizKuru.HasValue && viewModel.DovizKuru.Value > 0)
                                        {
                                            hedefTutar = viewModel.Tutar * viewModel.DovizKuru.Value;
                                        }

                                        // Hedef hareketi güncelle
                                        hedefKasaHareket.Tutar = hedefTutar;
                                        hedefKasaHareket.Tarih = viewModel.Tarih;
                                        hedefKasaHareket.Aciklama = viewModel.Aciklama;
                                        hedefKasaHareket.GuncellemeTarihi = DateTime.Now;
                                        hedefKasaHareket.SonGuncelleyenKullaniciID = GetCurrentUserId();
                                        hedefKasaHareket.HareketTuru = karsiHareket;

                                        // Yeni tutarın etkisini uygula
                                        if (hareket.HareketTuru == "Giriş")
                                        {
                                            hedefKasa.GuncelBakiye -= hedefKasaHareket.Tutar;
                                        }
                                        else
                                        {
                                            hedefKasa.GuncelBakiye += hedefKasaHareket.Tutar;
                                        }

                                        // Değişiklikleri kaydet
                                        _unitOfWork.Repository<KasaHareket>().Update(hedefKasaHareket);
                                        _unitOfWork.Repository<Kasa>().Update(hedefKasa);
                                        await _unitOfWork.CompleteAsync();
                                    }
                                }
                            }
                            else if (viewModel.HedefBankaID.HasValue)
                            {
                                // Kasadan bankaya transfer
                                var bankaHesap = await _context.BankaHesaplari
                                    .Include(b => b.Banka)
                                    .FirstOrDefaultAsync(b => b.BankaHesapID == viewModel.HedefBankaID && !b.Silindi);

                                if (bankaHesap != null)
                                {
                                    // Banka hareketini bul
                                    var bankaHareket = await _context.BankaHesapHareketleri
                                        .FirstOrDefaultAsync(h => h.TransferID == hareket.TransferID && h.BankaHesapID == viewModel.HedefBankaID && !h.Silindi);

                                    if (bankaHareket != null)
                                    {
                                        // Önce eski tutarın etkisini geri al
                                        if (hareket.HareketTuru == "Giriş")
                                        {
                                            bankaHesap.GuncelBakiye += bankaHareket.Tutar;
                                            karsiHareket = "Çıkış";
                                        }
                                        else
                                        {
                                            bankaHesap.GuncelBakiye -= bankaHareket.Tutar;
                                            karsiHareket = "Giriş";
                                        }
                                        

                                        // Hesaplanan kur ile hedef tutarını güncelle
                                        decimal hedefTutar = viewModel.Tutar;
                                        if (viewModel.DovizKuru.HasValue && viewModel.DovizKuru.Value > 0)
                                        {
                                            hedefTutar = viewModel.Tutar * viewModel.DovizKuru.Value;
                                        }

                                        // Banka hareketini güncelle
                                        bankaHareket.Tutar = hedefTutar;
                                        bankaHareket.Tarih = viewModel.Tarih;
                                        bankaHareket.Aciklama = viewModel.Aciklama;
                                        bankaHareket.GuncellemeTarihi = DateTime.Now;
                                        bankaHareket.SonGuncelleyenKullaniciID = GetCurrentUserId();
                                        bankaHareket.HareketTuru = karsiHareket;

                                        // Yeni tutarın etkisini uygula
                                        if (hareket.HareketTuru == "Giriş")
                                        {
                                            bankaHesap.GuncelBakiye -= bankaHareket.Tutar;
                                        }
                                        else
                                        {
                                            bankaHesap.GuncelBakiye += bankaHareket.Tutar;
                                        }

                                        // Değişiklikleri kaydet
                                        _context.BankaHesapHareketleri.Update(bankaHareket);
                                        _context.BankaHesaplari.Update(bankaHesap);
                                        await _context.SaveChangesAsync();
                                    }
                                }
                            }
                            else if (viewModel.KaynakBankaID.HasValue)
                            {
                                // Bankadan kasaya transfer
                                var bankaHesap = await _context.BankaHesaplari
                                    .Include(b => b.Banka)
                                    .FirstOrDefaultAsync(b => b.BankaHesapID == viewModel.KaynakBankaID && !b.Silindi);

                                if (bankaHesap != null)
                                {
                                    // Banka hareketini bul
                                    var bankaHareket = await _context.BankaHesapHareketleri
                                        .FirstOrDefaultAsync(h => h.TransferID == hareket.TransferID && h.BankaHesapID == viewModel.KaynakBankaID && !h.Silindi);

                                    if (bankaHareket != null)
                                    {
                                        // Önce eski tutarın etkisini geri al (çıkış işlemi olduğu için ters işlem)

                                        // Önce eski tutarın etkisini geri al
                                        if (hareket.HareketTuru == "Giriş")
                                        {
                                            bankaHesap.GuncelBakiye -= bankaHareket.Tutar;
                                            karsiHareket = "Çıkış";
                                        }
                                        else
                                        {
                                            bankaHesap.GuncelBakiye += bankaHareket.Tutar;
                                            karsiHareket = "Giriş";
                                        }
                                        

                                        // Hesaplanan kur ile kaynak tutarını güncelle
                                        decimal kaynakTutar = viewModel.Tutar;
                                        if (viewModel.DovizKuru.HasValue && viewModel.DovizKuru.Value > 0)
                                        {
                                            kaynakTutar = viewModel.Tutar / viewModel.DovizKuru.Value;
                                        }

                                        // Banka hareketini güncelle
                                        bankaHareket.Tutar = kaynakTutar;
                                        bankaHareket.Tarih = viewModel.Tarih;
                                        bankaHareket.Aciklama = viewModel.Aciklama;
                                        bankaHareket.GuncellemeTarihi = DateTime.Now;
                                        bankaHareket.SonGuncelleyenKullaniciID = GetCurrentUserId();
                                        bankaHareket.HareketTuru = karsiHareket;

                                        // Yeni tutarın etkisini uygula (çıkış işlemi)
                                        if (hareket.HareketTuru == "Giriş")
                                        {
                                            bankaHesap.GuncelBakiye += bankaHareket.Tutar;
                                        }
                                        else
                                        {
                                            bankaHesap.GuncelBakiye -= bankaHareket.Tutar;
                                        }

                                        // Değişiklikleri kaydet
                                        _context.BankaHesapHareketleri.Update(bankaHareket);
                                        _context.BankaHesaplari.Update(bankaHesap);
                                        await _context.SaveChangesAsync();
                                    }
                                }
                            }
                        }

                        // Transaction'ı tamamla
                        await transaction.CommitAsync();

                        // İşlemi logla
                        await _logService.Log(
                            $"Kasa hareketi güncellendi: ID: {hareket.KasaHareketID}, Kasa: {kasa.KasaAdi}, Tutar: {hareket.Tutar}",
                            Enums.LogTuru.Bilgi
                        );

                        if (isAjaxRequest)
                        {
                            // AJAX yanıtı
                            return Json(new
                            {
                                success = true,
                                message = "Kasa hareketi başarıyla güncellendi.",
                                redirectUrl = Url.Action("Hareketler", new { id = hareket.KasaID })
                            });
                        }

                        // Normal form gönderimi yanıtı
                        TempData["SuccessMessage"] = "Kasa hareketi başarıyla güncellendi.";
                        return RedirectToAction(nameof(Hareketler), new { id = hareket.KasaID });
                    }
                    catch (Exception ex)
                    {
                        // Hata durumunda transaction'ı geri al
                        await transaction.RollbackAsync();
                        throw ex;
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Kasa hareketi güncellenirken hata oluştu. ID: {Id}", id);

                    if (isAjaxRequest)
                    {
                        return Json(new
                        {
                            success = false,
                            message = "Kasa hareketi güncellenirken bir hata oluştu: " + ex.Message
                        });
                    }

                    ModelState.AddModelError("", "Kasa hareketi güncellenirken bir hata oluştu: " + ex.Message);
                }
            }
            else if (isAjaxRequest)
            {
                var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList();
                return Json(new
                {
                    success = false,
                    message = "Geçersiz form verileri, lütfen kontrol ediniz.",
                    errors = errors
                });
            }

            // Hata durumunda gerekli verileri tekrar yükle
            var kasalar = await _unitOfWork.Repository<Kasa>().GetAsync(
                filter: k => !k.Silindi && k.Aktif,
                orderBy: q => q.OrderBy(k => k.KasaAdi)
            );

            var cariler = await _unitOfWork.CariRepository.GetAll()
                .Where(c => !c.Silindi && c.AktifMi)
                .OrderBy(c => c.Ad)
                .ToListAsync();

            ViewBag.Kasalar = kasalar.ToList();
            ViewBag.Cariler = cariler.ToList();

            ViewBag.IslemTurleri = new List<SelectListItem>
            {
                new SelectListItem { Text = "Giriş", Value = "Giriş" },
                new SelectListItem { Text = "Çıkış", Value = "Çıkış" }
            };

            return View(viewModel);
        }

        // GET: Kasa/HareketSil/5
        public async Task<IActionResult> HareketSil(Guid? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            try
            {
                // ID'ye göre kasa hareketini getir
                var hareket = await _unitOfWork.Repository<KasaHareket>().GetByIdAsync(id.Value);
                if (hareket == null || hareket.Silindi)
                {
                    return NotFound();
                }


                // İlgili kasa bilgisini getir
                var kasa = await _unitOfWork.Repository<Kasa>().GetByIdAsync(hareket.KasaID);
                if (kasa == null || kasa.Silindi)
                {
                    return NotFound();
                }

                // Entity'yi ViewModel'e dönüştür
                var viewModel = new KasaHareketViewModel
                {
                    KasaHareketID = hareket.KasaHareketID,
                    KasaID = hareket.KasaID,
                    KasaAdi = kasa.KasaAdi,
                    HareketTuru = hareket.HareketTuru,
                    Tutar = hareket.Tutar,
                    Tarih = hareket.Tarih,
                    ReferansNo = hareket.ReferansNo,
                    ReferansTuru = hareket.ReferansTuru,
                    Aciklama = hareket.Aciklama,
                    ParaBirimi = kasa.ParaBirimi,
                    IslemTuru = hareket.IslemTuru,
                    TransferID = hareket.TransferID,
                    HedefKasaID = hareket.HedefKasaID,
                    KaynakBankaID = hareket.KaynakBankaID
                };

                // Transfer işlemi mi kontrol et
                bool isTransfer = hareket.IslemTuru == "KasaTransfer";
                
                // Transfer işlemi ise hedef/kaynak detaylarını doldur
                if (isTransfer)
                {
                    if (hareket.HedefKasaID.HasValue)
                    {
                        var hedefKasa = await _unitOfWork.Repository<Kasa>().GetByIdAsync(hareket.HedefKasaID.Value);
                        if (hedefKasa != null && !hedefKasa.Silindi)
                        {
                            viewModel.HedefKasaAdi = hedefKasa.KasaAdi;
                        }
                    }
                    
                    if (hareket.KaynakBankaID.HasValue)
                    {
                        var bankaHesap = await _context.BankaHesaplari
                            .Include(b => b.Banka)
                            .FirstOrDefaultAsync(b => b.BankaHesapID == hareket.KaynakBankaID && !b.Silindi);
                        
                        if (bankaHesap != null)
                        {
                            viewModel.KaynakBankaAdi = $"{bankaHesap.Banka.BankaAdi} - {bankaHesap.HesapAdi}";
                        }
                    }
                }

                // Cari bilgisini getir (varsa)
                if (hareket.CariID.HasValue)
                {
                    var cari = await _unitOfWork.Repository<Cari>().GetByIdAsync(hareket.CariID.Value);
                    if (cari != null && !cari.Silindi)
                    {
                        viewModel.CariAdi = cari.Ad;
                    }
                }

                return View(viewModel);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Kasa hareketi silme sayfası yüklenirken hata oluştu. ID: {Id}", id);
                return NotFound();
            }
        }

        // POST: Kasa/HareketSil/5
        [HttpPost, ActionName("HareketSil")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> HareketSilConfirmed(Guid id)
        {
            try
            {
                // Veritabanı işlemleri için transaction başlat
                using var transaction = await _context.Database.BeginTransactionAsync();

                try
                {
                    // ID'ye göre kasa hareketini getir
                    var hareket = await _unitOfWork.Repository<KasaHareket>().GetByIdAsync(id);
                    if (hareket == null || hareket.Silindi)
                    {
                        return NotFound();
                    }

                    // Kasa bilgisini getir
                    var kasa = await _unitOfWork.Repository<Kasa>().GetByIdAsync(hareket.KasaID);
                    if (kasa == null || kasa.Silindi)
                    {
                        return NotFound();
                    }

                    // Transfer işlemi mi kontrol et
                    bool isTransfer = hareket.IslemTuru == "KasaTransfer";
                    
                    // Eğer transferse karşı taraftaki hareketi de bul
                    if (isTransfer && hareket.TransferID.HasValue)
                    {
                        // Diğer transfer hareketlerini bul
                        var karsiHareketler = await _context.KasaHareketleri
                            .Where(h => h.TransferID == hareket.TransferID && h.KasaHareketID != hareket.KasaHareketID && !h.Silindi)
                            .ToListAsync();
                        
                        foreach (var karsiHareket in karsiHareketler)
                        {
                            // Karşı hareketin kasasını getir
                            var karsiKasa = await _unitOfWork.Repository<Kasa>().GetByIdAsync(karsiHareket.KasaID);
                            if (karsiKasa != null && !karsiKasa.Silindi)
                            {
                                // Karşı kasa bakiyesini güncelle
                                if (karsiHareket.HareketTuru == "Giriş")
                                {
                                    karsiKasa.GuncelBakiye -= karsiHareket.Tutar;
                                }
                                else
                                {
                                    karsiKasa.GuncelBakiye += karsiHareket.Tutar;
                                }
                                
                                // Karşı hareketi iptal et
                                karsiHareket.Silindi = true;
                                karsiHareket.GuncellemeTarihi = DateTime.Now;
                                karsiHareket.SonGuncelleyenKullaniciID = GetCurrentUserId();
                                karsiHareket.Aciklama += " (Transfer iptal edildi)";
                                
                                // Değişiklikleri kaydet
                                _unitOfWork.Repository<KasaHareket>().Update(karsiHareket);
                                _unitOfWork.Repository<Kasa>().Update(karsiKasa);
                                
                                // Log
                                await _logService.Log(
                                    $"Transfer karşı hareketi silindi: ID: {karsiHareket.KasaHareketID}, Kasa: {karsiKasa.KasaAdi}, Tutar: {karsiHareket.Tutar}",
                                    Enums.LogTuru.Bilgi
                                );
                            }
                        }
                    }

                    // Kasa bakiyesini güncelle
                    if (hareket.HareketTuru == "Giriş")
                    {
                        kasa.GuncelBakiye -= hareket.Tutar;
                    }
                    else
                    {
                        kasa.GuncelBakiye += hareket.Tutar;
                    }

                    // Soft delete işlemi
                    hareket.Silindi = true;
                    hareket.GuncellemeTarihi = DateTime.Now;
                    hareket.SonGuncelleyenKullaniciID = GetCurrentUserId();
                    hareket.Aciklama += isTransfer ? " (Transfer iptal edildi)" : " (Silindi)";

                    // Değişiklikleri kaydet
                    _unitOfWork.Repository<KasaHareket>().Update(hareket);
                    _unitOfWork.Repository<Kasa>().Update(kasa);
                    await _unitOfWork.CompleteAsync();

                    // Bağlı cari hareketi iptal et (varsa)
                    if (!isTransfer && hareket.CariID.HasValue && hareket.CariID != Guid.Empty)
                    {
                        await _cariHareketService.IptalEtCariHareketFromKasaAsync(hareket.KasaHareketID);
                    }

                    // Transaction'ı tamamla
                    await transaction.CommitAsync();

                    // İşlemi logla
                    await _logService.Log(
                        $"Kasa hareketi silindi: ID: {hareket.KasaHareketID}, Kasa: {kasa.KasaAdi}, Tutar: {hareket.Tutar}",
                        Enums.LogTuru.Bilgi
                    );

                    TempData["SuccessMessage"] = isTransfer ? "Transfer hareketi başarıyla iptal edildi." : "Kasa hareketi başarıyla silindi.";
                    return RedirectToAction(nameof(Hareketler), new { id = hareket.KasaID });
                }
                catch (Exception ex)
                {
                    // Hata durumunda transaction'ı geri al
                    await transaction.RollbackAsync();
                    throw ex;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Kasa hareketi silinirken hata oluştu. ID: {Id}", id);
                TempData["ErrorMessage"] = "Kasa hareketi silinirken bir hata oluştu: " + ex.Message;
                return RedirectToAction(nameof(Index));
            }
        }
    }
} 
