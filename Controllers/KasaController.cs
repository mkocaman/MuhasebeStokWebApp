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
            RoleManager<IdentityRole> roleManager) : base(menuService, userManager, roleManager, logService)
        {
            _unitOfWork = unitOfWork;
            _logger = logger;
            _context = context;
            _dovizKuruService = dovizKuruService;
            _logService = logService;
            _paraBirimiService = paraBirimiService;
            _userManager = userManager;
            _roleManager = roleManager;
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

                return View(kasa);
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
        public IActionResult Create()
        {
            return View();
        }

        // POST: Kasa/Create
        /// <summary>
        /// Yeni kasa kaydı oluşturur
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(KasaViewModel viewModel)
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
                        ParaBirimi = viewModel.ParaBirimi,
                        Aciklama = viewModel.Aciklama,
                        GuncelBakiye = viewModel.AcilisBakiye,
                        Aktif = true,
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
                var viewModel = new KasaViewModel
                {
                    KasaID = kasa.KasaID,
                    KasaAdi = kasa.KasaAdi,
                    ParaBirimi = kasa.ParaBirimi,
                    Aciklama = kasa.Aciklama,
                    AcilisBakiye = kasa.GuncelBakiye,
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

                return View(kasa);
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

                ViewBag.Kasa = kasa;
                return View(hareketler.ToList());
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
                var cariler = await _unitOfWork.Repository<Cari>().GetAsync(
                    filter: c => !c.Silindi && c.AktifMi,
                    orderBy: q => q.OrderBy(c => c.Ad)
                );

                ViewBag.Kasalar = kasalar.ToList();
                ViewBag.Cariler = cariler.ToList();

                // İşlem türleri için dropdown hazırla
                ViewBag.IslemTurleri = new List<SelectListItem>
                {
                    new SelectListItem { Text = "Giriş", Value = "Giriş" },
                    new SelectListItem { Text = "Çıkış", Value = "Çıkış" }
                };

                // Model oluştur
                var model = new KasaHareket
                {
                    KasaHareketID = Guid.NewGuid(),
                    Tarih = DateTime.Now,
                    ReferansNo = "REF-" + DateTime.Now.ToString("yyyyMMddHHmmss")
                };

                // Kasa ID varsa
                if (id.HasValue)
                {
                    var kasa = await _unitOfWork.Repository<Kasa>().GetByIdAsync(id.Value);
                    if (kasa != null && !kasa.Silindi)
                    {
                        model.KasaID = kasa.KasaID;
                        ViewBag.SecilenKasa = kasa;
                    }
                }

                // Cari ID varsa
                if (cariId.HasValue)
                {
                    var cari = await _unitOfWork.Repository<Cari>().GetByIdAsync(cariId.Value);
                    if (cari != null && !cari.Silindi)
                    {
                        model.CariID = cari.CariID;
                        ViewBag.SecilenCari = cari;
                    }
                }

                return View(model);
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
            if (ModelState.IsValid)
            {
                try
                {
                    // Mevcut kasayı getir
                    var kasa = await _unitOfWork.Repository<Kasa>().GetByIdAsync(viewModel.KasaID);
                    if (kasa == null || kasa.Silindi)
                    {
                        return NotFound();
                    }

                    // Yeni hareket oluştur
                    var hareket = new KasaHareket
                    {
                        KasaHareketID = Guid.NewGuid(),
                        KasaID = viewModel.KasaID,
                        Tarih = viewModel.Tarih,
                        IslemTuru = viewModel.IslemTuru,
                        HareketTuru = viewModel.HareketTuru,
                        Tutar = viewModel.Tutar,
                        Aciklama = viewModel.Aciklama,
                        OlusturmaTarihi = DateTime.Now,
                        IslemYapanKullaniciID = GetCurrentUserId(),
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
                    
                    // İşlemi logla
                    await _logService.Log(
                        $"Yeni kasa hareketi oluşturuldu: Kasa: {kasa.KasaAdi}, İşlem: {viewModel.HareketTuru}, Tutar: {viewModel.Tutar} {kasa.ParaBirimi}",
                        Enums.LogTuru.Bilgi
                    );

                    return RedirectToAction(nameof(Hareketler), new { id = viewModel.KasaID });
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Kasa hareketi oluşturulurken hata oluştu. Kasa ID: {KasaId}", viewModel.KasaID);
                    ModelState.AddModelError("", "Kasa hareketi oluşturulurken bir hata oluştu. Lütfen tekrar deneyin.");
                }
            }

            // Hata durumunda dropdown'ları yeniden hazırla
            ViewBag.IslemTurleri = new List<SelectListItem>
            {
                new SelectListItem { Text = "Giriş", Value = "Giriş" },
                new SelectListItem { Text = "Çıkış", Value = "Çıkış" }
            };

            var kasaInfo = await _unitOfWork.Repository<Kasa>().GetByIdAsync(viewModel.KasaID);
            ViewBag.Kasa = kasaInfo;
            
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
            var kasalar = await _unitOfWork.Repository<Kasa>().GetAsync(
                filter: k => !k.Silindi,
                orderBy: q => q.OrderBy(k => k.KasaAdi)
            );

            // Dropdown için hazırla
            ViewBag.Kasalar = kasalar.Select(k => new SelectListItem
            {
                Value = k.KasaID.ToString(),
                Text = $"{k.KasaAdi} ({k.ParaBirimi}) - {k.GuncelBakiye:N2}"
            }).ToList();

            // Para birimleri listesi
            ViewBag.ParaBirimleri = new List<string> { "TRY", "USD", "EUR", "GBP" };
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
    }
} 
