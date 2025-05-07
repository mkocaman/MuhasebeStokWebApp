#nullable enable

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using MuhasebeStokWebApp.Data;
using MuhasebeStokWebApp.Data.Entities;
using MuhasebeStokWebApp.Models;
using MuhasebeStokWebApp.Services.Interfaces;
using MuhasebeStokWebApp.ViewModels.Cari;
using MuhasebeStokWebApp.Services;
using MuhasebeStokWebApp.Services.ParaBirimiModulu;
using ParaBirimiService = MuhasebeStokWebApp.Services.ParaBirimiModulu.IParaBirimiService;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Linq;
using Microsoft.Extensions.Logging;
using MuhasebeStokWebApp.Data.Repositories;
using MuhasebeStokWebApp.Enums;
using MuhasebeStokWebApp.Extensions;
using Microsoft.AspNetCore.Hosting;
using System.Security.Claims;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace MuhasebeStokWebApp.Controllers
{
    [Authorize]
    public class CariController : BaseController
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<CariController> _logger;
        private readonly ParaBirimiService _paraBirimiService;
        private readonly IDovizKuruService _dovizKuruService;
        private readonly IWebHostEnvironment _webHostEnvironment;
        private readonly ICariNumaralandirmaService _cariNumaralandirmaService;

        public CariController(
            IUnitOfWork unitOfWork,
            UserManager<ApplicationUser> userManager,
            ILogService logService,
            IMenuService menuService,
            RoleManager<IdentityRole> roleManager,
            ILogger<CariController> logger,
            ParaBirimiService paraBirimiService,
            IDovizKuruService dovizKuruService,
            IWebHostEnvironment webHostEnvironment,
            ICariNumaralandirmaService cariNumaralandirmaService) 
            : base(menuService, userManager, roleManager, logService)
        {
            _unitOfWork = unitOfWork;
            _logger = logger;
            _paraBirimiService = paraBirimiService;
            _dovizKuruService = dovizKuruService;
            _webHostEnvironment = webHostEnvironment;
            _cariNumaralandirmaService = cariNumaralandirmaService;
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            try
            {
                // Sekme parametresini kontrol et
                var tabParam = HttpContext.Request.Query["tab"].ToString();
                var activeTab = string.IsNullOrEmpty(tabParam) ? "aktif" : tabParam;

                // Tüm carileri alacağız ama görüntülerken filtreleyeceğiz
                var cariler = await _unitOfWork.CariRepository.GetAll()
                    .IgnoreQueryFilters() // Tüm cari kayıtları almak için filtreleri devre dışı bırak
                    .ToListAsync();
            
                // Para birimi listesini view'a gönder
                ViewBag.ParaBirimleri = await _paraBirimiService.GetAllParaBirimleriAsync(true);
            
                return View(cariler);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Cari listesi yüklenirken hata: {Message}", ex.Message);
                TempData["ErrorMessage"] = "Cari listesi yüklenirken bir hata oluştu.";
                return View(new List<Data.Entities.Cari>());
            }
        }

        [HttpGet]
        public async Task<IActionResult> Details(Guid id)
        {
            try
            {
                // Silindi filtresini kaldırarak carileri getir - silinmiş cariler de dahil
                var cari = await _unitOfWork.CariRepository.GetAll()
                    .IgnoreQueryFilters() // Tüm carileri getirir, silindi olanlar dahil
                    .FirstOrDefaultAsync(c => c.CariID == id);
                    
                if (cari == null)
                {
                    TempData["ErrorMessage"] = "Cari bulunamadı.";
                    return RedirectToAction(nameof(Index));
                }

                // Zorunlu alanların null olup olmadığını kontrol et ve varsayılan değerlerle doldur
                if (string.IsNullOrEmpty(cari.Il))
                    cari.Il = "Belirtilmemiş";
                
                if (string.IsNullOrEmpty(cari.Ilce))
                    cari.Ilce = "Belirtilmemiş";
                
                if (string.IsNullOrEmpty(cari.PostaKodu))
                    cari.PostaKodu = "00000";
                
                if (string.IsNullOrEmpty(cari.Ulke))
                    cari.Ulke = "Türkiye";
                
                if (string.IsNullOrEmpty(cari.Aciklama))
                    cari.Aciklama = "Belirtilmemiş";
                
                if (string.IsNullOrEmpty(cari.Adres))
                    cari.Adres = "Belirtilmemiş";
                
                if (string.IsNullOrEmpty(cari.Telefon))
                    cari.Telefon = "0000000000";
                
                if (string.IsNullOrEmpty(cari.Email))
                    cari.Email = "bilgi@firma.com";
                
                if (string.IsNullOrEmpty(cari.Yetkili))
                    cari.Yetkili = "Belirtilmemiş";

                // Para birimi listesini view'a gönder
                ViewBag.ParaBirimleri = await _paraBirimiService.GetAllParaBirimleriAsync(true);

                // Cari hareketlerini ve faturaları güvenli bir şekilde yükle
                try
                {
                    // Hareketleri veritabanı seviyesinde filtreleme ile getir - silinmiş hareketleri hariç tut
                    var tumHareketler = await _unitOfWork.CariHareketRepository.GetAll()
                        .Where(c => !c.Silindi && c.CariID == id)
                        .ToListAsync();
                    
                    // Açılış bakiyesi hareketlerini ayrı al
                    var acilisBakiyeHareketleri = tumHareketler
                        .Where(h => h.HareketTuru == "Açılış bakiyesi")
                        .ToList()
                        .OrderBy(h => h.Tarih)
                        .ToList();
                    
                    // Diğer hareketleri al
                    var digerHareketler = tumHareketler
                        .Where(h => h.HareketTuru != "Açılış bakiyesi")
                        .ToList()
                        .OrderBy(h => h.Tarih)
                        .ToList();
                        
                    cari.CariHareketler = acilisBakiyeHareketleri.Concat(digerHareketler).ToList();

                    // Son faturaları getir - silinmiş faturaları hariç tut
                    var faturalar = await _unitOfWork.FaturaRepository.GetAll()
                        .Where(f => !f.Silindi && f.CariID == id)
                        .ToListAsync();
                        
                    // Bellekte sıralama
                    cari.Faturalar = faturalar.OrderByDescending(f => f.FaturaTarihi).Take(10).ToList();
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Cari detayları için ilişkili kayıtlar yüklenirken hata: {Message}", ex.Message);
                    TempData["WarningMessage"] = "Bazı ilişkili kayıtlar yüklenemedi. Sayfayı yenilemeyi deneyin veya yöneticinize başvurun.";
                }

                return View(cari);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Cari detayları yüklenirken hata: {Message}", ex.Message);
                TempData["ErrorMessage"] = "Cari detayları yüklenirken bir hata oluştu.";
                return RedirectToAction(nameof(Index));
            }
        }

        [HttpGet]
        public async Task<IActionResult> Create()
        {
            try
            {
                // Para birimlerini view'a gönder
                ViewBag.ParaBirimleri = await _paraBirimiService.GetAllParaBirimleriAsync(true);
                
                // Yeni cari modeli oluştur
                var model = new CariCreateViewModel
                {
                    Ad = "",
                    CariTipi = "Müşteri",
                    Aktif = true,
                    Ulke = "Türkiye"
                };
                
                // Cari kodunu otomatik oluştur
                model.CariKodu = await _cariNumaralandirmaService.GenerateCariKoduAsync();
                
                // Eğer AJAX isteği ise partial view döndür
                if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
                {
                    return PartialView("_CreatePartial", model);
                }
                
                // Normal istek ise tam sayfa döndür
                return View(model);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Cari oluşturma sayfası yüklenirken hata: {Message}", ex.Message);
                
                TempData["ErrorMessage"] = $"Cari oluşturma sayfası yüklenirken bir hata oluştu: {ex.Message}";
                return RedirectToAction(nameof(Index));
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CariCreateViewModel model)
        {
            try
            {
                // Validasyon kontrolü
                if (!ModelState.IsValid)
                {
                    ViewBag.ParaBirimleri = await _paraBirimiService.GetAllParaBirimleriAsync(true);
                    
                    if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
                    {
                        return Json(new { success = false, message = "Lütfen tüm zorunlu alanları doldurunuz.", errors = ModelState.Values.SelectMany(v => v.Errors.Select(e => e.ErrorMessage)) });
                    }
                    
                    return View(model);
                }
                
                // Cari kodunu otomatik oluştur (boş veya null ise)
                if (string.IsNullOrEmpty(model.CariKodu))
                {
                    model.CariKodu = await _cariNumaralandirmaService.GenerateCariKoduAsync();
                }
                
                // Aynı koda sahip cari var mı kontrol et
                if (await _unitOfWork.CariRepository.AnyAsync(c => c.CariKodu == model.CariKodu && !c.Silindi))
                {
                    ModelState.AddModelError("CariKodu", "Bu cari kodu zaten kullanılıyor.");
                    ViewBag.ParaBirimleri = await _paraBirimiService.GetAllParaBirimleriAsync(true);
                    
                    if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
                    {
                        return Json(new { success = false, message = "Bu cari kodu zaten kullanılıyor." });
                    }
                    
                    return View(model);
                }
                
                // Cari nesnesini oluştur
                var cari = new Cari
                {
                    CariID = Guid.NewGuid(),
                    Ad = model.Ad,
                    CariKodu = model.CariKodu,
                    CariTipi = model.CariTipi,
                    VergiNo = model.VergiNo,
                    VergiDairesi = model.VergiDairesi,
                    Telefon = model.Telefon ?? "",
                    Email = model.Email ?? "",
                    Yetkili = model.Yetkili ?? "",
                    Adres = model.Adres ?? "",
                    Aciklama = model.Aciklama,
                    Il = model.Il ?? "Belirtilmemiş",
                    Ilce = model.Ilce ?? "Belirtilmemiş",
                    PostaKodu = model.PostaKodu ?? "00000",
                    Ulke = model.Ulke ?? "Türkiye",
                    WebSitesi = model.WebSitesi,
                    AktifMi = model.Aktif,
                    VarsayilanParaBirimiId = model.VarsayilanParaBirimiId,
                    VarsayilanKurKullan = model.VarsayilanKurKullan,
                    OlusturmaTarihi = DateTime.Now,
                    OlusturanKullaniciId = Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value),
                    Silindi = false
                };
                
                await _unitOfWork.CariRepository.AddAsync(cari);
                await _unitOfWork.SaveAsync();
                
                // Cari oluşturma log kaydı
                await _logService.CariOlusturmaLogOlustur(
                    cari.CariID.ToString(),
                    cari.Ad,
                    $"{cari.Ad} isimli yeni cari oluşturuldu."
                );
                
                if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
                {
                    return Json(new { 
                        success = true, 
                        message = $"{cari.Ad} carisi başarıyla oluşturuldu.",
                        id = cari.CariID.ToString() 
                    });
                }
                
                TempData["SuccessMessage"] = $"{cari.Ad} carisi başarıyla oluşturuldu.";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Cari oluşturulurken hata: {Message}", ex.Message);
                
                if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
                {
                    return Json(new { success = false, message = $"Cari oluşturulurken bir hata oluştu: {ex.Message}" });
                }
                
                TempData["ErrorMessage"] = $"Cari oluşturulurken bir hata oluştu: {ex.Message}";
                return View(model);
            }
        }

        [HttpGet]
        public async Task<IActionResult> Edit(Guid id)
        {
            try
            {
                var cari = await _unitOfWork.CariRepository.GetFirstOrDefaultAsync(c => c.CariID == id && !c.Silindi);
                if (cari == null)
                {
                    if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
                    {
                        return Json(new { success = false, message = "Cari bulunamadı." });
                    }
                    
                    TempData["ErrorMessage"] = "Cari bulunamadı.";
                    return RedirectToAction(nameof(Index));
                }
                
                // ViewModel'i doldur
                var model = new CariEditViewModel
                {
                    CariID = cari.CariID,
                    Ad = cari.Ad,
                    CariKodu = cari.CariKodu,
                    CariTipi = cari.CariTipi,
                    VergiNo = cari.VergiNo,
                    VergiDairesi = cari.VergiDairesi,
                    Telefon = cari.Telefon,
                    Email = cari.Email,
                    Yetkili = cari.Yetkili,
                    Adres = cari.Adres,
                    Il = cari.Il,
                    Ilce = cari.Ilce,
                    PostaKodu = cari.PostaKodu,
                    Ulke = cari.Ulke,
                    WebSitesi = cari.WebSitesi,
                    VarsayilanParaBirimiId = cari.VarsayilanParaBirimiId,
                    Aciklama = cari.Aciklama,
                    AktifMi = cari.AktifMi
                };
                
                // Para birimlerini view'a gönder
                ViewBag.ParaBirimleri = await _paraBirimiService.GetAllParaBirimleriAsync(true);
                
                // AJAX isteğiyse partial view döndür
                if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
                {
                    return PartialView("_EditPartial", model);
                }
                
                return View(model);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Cari düzenleme sayfası görüntülenirken hata: {Message}", ex.Message);
                
                if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
                {
                    return Json(new { success = false, message = $"Hata oluştu: {ex.Message}" });
                }
                
                TempData["ErrorMessage"] = $"Bir hata oluştu: {ex.Message}";
                return RedirectToAction(nameof(Index));
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(CariEditViewModel model)
        {
            try
            {
                // Validasyon kontrolü
                if (!ModelState.IsValid)
                {
                    ViewBag.ParaBirimleri = await _paraBirimiService.GetAllParaBirimleriAsync(true);
                    
                    if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
                    {
                        return Json(new { success = false, message = "Lütfen tüm zorunlu alanları doldurunuz.", errors = ModelState.Values.SelectMany(v => v.Errors.Select(e => e.ErrorMessage)) });
                    }
                    
                    return View(model);
                }
                
                // Cari varlığını kontrol et
                var cari = await _unitOfWork.CariRepository.GetFirstOrDefaultAsync(c => c.CariID == model.CariID && !c.Silindi);
                if (cari == null)
                {
                    if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
                    {
                        return Json(new { success = false, message = "Cari bulunamadı." });
                    }
                    
                    TempData["ErrorMessage"] = "Cari bulunamadı.";
                    return RedirectToAction(nameof(Index));
                }
                
                // Aynı koda sahip başka cari var mı kontrol et
                if (cari.CariKodu != model.CariKodu && await _unitOfWork.CariRepository.AnyAsync(c => c.CariKodu == model.CariKodu && c.CariID != model.CariID && !c.Silindi))
                {
                    ModelState.AddModelError("CariKodu", "Bu cari kodu zaten kullanılıyor.");
                    ViewBag.ParaBirimleri = await _paraBirimiService.GetAllParaBirimleriAsync(true);
                    
                    if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
                    {
                        return Json(new { success = false, message = "Bu cari kodu zaten kullanılıyor." });
                    }
                    
                    return View(model);
                }
                
                // Cari bilgilerini güncelle
                cari.Ad = model.Ad;
                cari.CariKodu = model.CariKodu;
                cari.CariTipi = model.CariTipi;
                cari.VergiNo = model.VergiNo;
                cari.VergiDairesi = model.VergiDairesi;
                cari.Telefon = model.Telefon ?? "";
                cari.Email = model.Email ?? "";
                cari.Yetkili = model.Yetkili ?? "";
                cari.Adres = model.Adres ?? "";
                cari.Aciklama = model.Aciklama;
                cari.Il = model.Il ?? "Belirtilmemiş";
                cari.Ilce = model.Ilce ?? "Belirtilmemiş";
                cari.PostaKodu = model.PostaKodu ?? "00000";
                cari.Ulke = model.Ulke ?? "Türkiye";
                cari.WebSitesi = model.WebSitesi;
                cari.AktifMi = model.AktifMi;
                cari.VarsayilanParaBirimiId = model.VarsayilanParaBirimiId;
                cari.VarsayilanKurKullan = model.VarsayilanKurKullan;
                cari.GuncellemeTarihi = DateTime.Now;
                cari.SonGuncelleyenKullaniciId = Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value);
                
                await _unitOfWork.CariRepository.UpdateAsync(cari);
                await _unitOfWork.SaveAsync();
                
                // Cari güncelleme log kaydı
                await _logService.CariGuncellemeLogOlustur(
                    cari.CariID.ToString(),
                    cari.Ad,
                    $"{cari.Ad} isimli cari güncellendi."
                );
                
                if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
                {
                    return Json(new { 
                        success = true, 
                        message = $"{cari.Ad} carisi başarıyla güncellendi." 
                    });
                }
                
                TempData["SuccessMessage"] = $"{cari.Ad} carisi başarıyla güncellendi.";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Cari güncellenirken hata: {Message}", ex.Message);
                
                if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
                {
                    return Json(new { success = false, message = $"Cari güncellenirken bir hata oluştu: {ex.Message}" });
                }
                
                TempData["ErrorMessage"] = $"Cari güncellenirken bir hata oluştu: {ex.Message}";
                return View(model);
            }
        }

        [HttpGet]
        public async Task<IActionResult> Delete(Guid id)
        {
            var cari = await _unitOfWork.CariRepository.GetByIdAsync(id);
            if (cari == null)
            {
                return NotFound();
            }
            
            return View(cari);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(Guid id)
        {
            try
            {
                // SoftDeleteService üzerinden işlem yap
                var softDeleteService = HttpContext.RequestServices.GetService<ISoftDeleteService<Cari>>();
                if (softDeleteService == null)
                {
                    TempData["ErrorMessage"] = "Silme işlemi için gerekli servis bulunamadı.";
                    return RedirectToAction(nameof(Index));
                }
                
                // Cari kaydını sil/pasife al
                var cari = await _unitOfWork.CariRepository.GetByIdAsync(id);
                if (cari == null)
                {
                    return NotFound();
                }
                
                // İlişkili kayıtlar kontrolü ve silme işlemi
                bool hasRelatedRecords = await softDeleteService.HasRelatedRecordsAsync(id);
                bool success = await softDeleteService.SoftDeleteByIdAsync(id);
                
                if (success)
                {
                    // Logla
                    string logMesaj = hasRelatedRecords 
                        ? $"{cari.Ad} adlı cari kaydı pasife alındı." 
                        : $"{cari.Ad} adlı cari kaydı silindi.";
                        
                    await _logService.CariSilmeLogOlustur(id.ToString(), cari.Ad, logMesaj);
                    
                    TempData["SuccessMessage"] = hasRelatedRecords
                        ? "Cari pasife alındı. İlişkili kayıtlar olduğu için tamamen silinemedi."
                        : "Cari başarıyla silindi.";
                }
                else
                {
                    TempData["ErrorMessage"] = "Cari silinemedi. Lütfen tekrar deneyin.";
                }
                
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Cari silme işleminde hata oluştu");
                TempData["ErrorMessage"] = $"Cari silinirken bir hata oluştu: {ex.Message}";
                return RedirectToAction(nameof(Index));
            }
        }

        [HttpGet]
        public async Task<IActionResult> HareketEkle(Guid id)
        {
            var cari = await _unitOfWork.CariRepository.GetByIdAsync(id);
            if (cari == null)
            {
                return NotFound();
            }
            
            var viewModel = new CariHareketCreateViewModel
            {
                CariID = cari.CariID,
                CariAdi = cari.Ad,
                Tarih = DateTime.Now,
                HareketTuru = "Tahsilat" // HareketTuru için varsayılan değer atıyoruz
            };
            
            return View(viewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> HareketEkle(CariHareketCreateViewModel model)
        {
            if (!ModelState.IsValid)
            {
                PrepareCariHareketViewBag(model.CariID);
                return View(model);
            }

            try
            {
                // Transaction başlat
                await _unitOfWork.BeginTransactionAsync();
                
                // Cari ve hareket bilgilerini doldur
                var cariHareket = new Data.Entities.CariHareket
                {
                    CariHareketID = Guid.NewGuid(),
                    CariID = model.CariID,
                    Tarih = model.Tarih,
                    VadeTarihi = model.VadeTarihi,
                    HareketTuru = model.HareketTuru,
                    Tutar = model.Tutar,
                    Aciklama = model.Aciklama,
                    ReferansNo = model.ReferansNo,
                    ReferansTuru = model.ReferansTuru ?? "Manuel Giriş",
                    Borc = model.HareketTuru == "Borç" || model.HareketTuru == "Ödeme" ? model.Tutar : 0,
                    Alacak = model.HareketTuru == "Alacak" || model.HareketTuru == "Tahsilat" ? model.Tutar : 0,
                    OlusturmaTarihi = DateTime.Now,
                    OlusturanKullaniciID = User.FindFirstValue(ClaimTypes.NameIdentifier) != null ? 
                        Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)) : Guid.Empty,
                    Silindi = false
                };

                // Cari hareket veritabanına ekle
                await _unitOfWork.CariHareketRepository.AddAsync(cariHareket);
                
                // İşlemi tamamla
                await _unitOfWork.CommitTransactionAsync();

                // Log kaydı için cari nesnesini tekrar almamız gerekebilir, çünkü transaction sonrası context değişmiş olabilir.
                // Ancak burada zaten try bloğunun başında aldık ve transaction içinde kullanıyoruz.
                var cariForLog = await _unitOfWork.CariRepository.GetByIdAsync(model.CariID); 
                if(cariForLog != null) {
                    // Log kaydı oluştur
                    await _logService.CariHareketEklemeLogOlustur(model.CariID.ToString(), cariForLog.Ad, model.HareketTuru, model.Tutar, model.Aciklama);
                } else {
                     _logger.LogWarning("Loglama için cari bulunamadı: {CariID}", model.CariID);
                     // Alternatif olarak, modeldeki cari adını kullanabiliriz ama güncel olmayabilir.
                     // await _logService.CariHareketEklemeLogOlustur(model.CariID.ToString(), model.CariAdi, model.HareketTuru, model.Tutar, model.Aciklama);
                }

                TempData["SuccessMessage"] = "Cari hareket başarıyla kaydedildi.";
                return RedirectToAction(nameof(Details), new { id = model.CariID });
            }
            catch (Exception ex)
            {
                // Hata durumunda transaction'ı geri al
                await _unitOfWork.RollbackTransactionAsync();
                
                _logger.LogError(ex, "Cari hareket eklenirken hata oluştu: {Message}", ex.Message);
                
                TempData["ErrorMessage"] = "Cari hareket kaydedilirken bir hata oluştu.";
                
                PrepareCariHareketViewBag(model.CariID);
                return View(model);
            }
        }

        [HttpGet]
        public async Task<IActionResult> Ekstre(Guid id, DateTime? baslangicTarihi = null, DateTime? bitisTarihi = null, Guid? paraBirimiId = null)
        {
            try
            {
                // ICariEkstreService üzerinden rapor alınacak
                var cariEkstreService = HttpContext.RequestServices.GetService<ICariEkstreService>();
                if (cariEkstreService == null)
                {
                    TempData["ErrorMessage"] = "Cari ekstre servisi bulunamadı.";
                    return RedirectToAction(nameof(Index));
                }

                // Ekstre raporu oluştur
                var ekstreModel = await cariEkstreService.GetCariEkstreRaporAsync(
                    id,
                    baslangicTarihi ?? DateTime.Now.AddMonths(-1),
                    bitisTarihi ?? DateTime.Now,
                    paraBirimiId);

                // Para birimleri listesini ViewBag'e ekle
                ViewBag.ParaBirimleri = await _paraBirimiService.GetAllParaBirimleriAsync(true);
                
                // Seçili para birimi ID'sini ViewBag'e ekle
                if (paraBirimiId.HasValue)
                {
                    ViewBag.SeciliParaBirimiId = paraBirimiId.Value;
                }

                return View(ekstreModel);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Cari ekstre yüklenirken hata: {Message}", ex.Message);
                TempData["ErrorMessage"] = "Cari ekstre yüklenirken bir hata oluştu.";
                return RedirectToAction(nameof(Index));
            }
        }

        // Örnek veri oluşturma action'ı
        [HttpGet]
        public async Task<IActionResult> AddSampleData()
        {
            try
            {
                // Veritabanında cari yok mu kontrol et
                bool emptyCariTable = !await _unitOfWork.CariRepository.GetAll().AnyAsync();
                
                if (emptyCariTable)
                {
                    // Örnek para birimi getir
                    var tryParaBirimi = await _paraBirimiService.GetParaBirimiByKodAsync("TRY");
                    if (tryParaBirimi == null)
                    {
                        return Json(new { success = false, message = "Örnek veri oluşturulamadı. TRY para birimi bulunamadı." });
                    }
                    
                    var musteriler = new List<Cari>()
                    {
                        new Cari
                        {
                            CariID = Guid.NewGuid(),
                            CariKodu = "M00001",
                            Ad = "Ahmet Yılmaz Ltd. Şti.",
                            CariTipi = "Müşteri",
                            Telefon = "0212 555 11 11",
                            Email = "info@ahmetyilmaz.com",
                            WebSitesi = "www.ahmetyilmaz.com",
                            VergiDairesi = "Kadıköy",
                            VergiNo = "1234567890",
                            Adres = "Ataşehir Bulvarı No:15 İstanbul",
                            Yetkili = "Ahmet Yılmaz",
                            Il = "İstanbul",
                            Ilce = "Ataşehir",
                            PostaKodu = "34000",
                            Ulke = "Türkiye",
                            VarsayilanParaBirimiId = tryParaBirimi.ParaBirimiID,
                            AktifMi = true,
                            Silindi = false,
                            OlusturmaTarihi = DateTime.Now,
                            GuncellemeTarihi = DateTime.Now
                        },
                        new Cari
                        {
                            CariID = Guid.NewGuid(),
                            CariKodu = "M00002",
                            Ad = "Fatma Demir Ticaret",
                            CariTipi = "Müşteri",
                            Telefon = "0216 444 22 22",
                            Email = "info@fatmademir.com",
                            WebSitesi = "www.fatmademir.com",
                            VergiDairesi = "Beşiktaş",
                            VergiNo = "9876543210",
                            Adres = "Şişli Merkez Mah. İstanbul",
                            Yetkili = "Fatma Demir",
                            Il = "İstanbul",
                            Ilce = "Şişli",
                            PostaKodu = "34000",
                            Ulke = "Türkiye",
                            VarsayilanParaBirimiId = tryParaBirimi.ParaBirimiID,
                            AktifMi = true,
                            Silindi = false,
                            OlusturmaTarihi = DateTime.Now,
                            GuncellemeTarihi = DateTime.Now
                        },
                        new Cari
                        {
                            CariID = Guid.NewGuid(),
                            CariKodu = "T00001",
                            Ad = "Mehmet Öz Toptan Gıda",
                            CariTipi = "Tedarikçi",
                            Telefon = "0312 333 33 33",
                            Email = "info@mehmetoz.com",
                            WebSitesi = "www.mehmetoz.com",
                            VergiDairesi = "Çankaya",
                            VergiNo = "5678912345",
                            Adres = "Çankaya Caddesi No:45 Ankara",
                            Yetkili = "Mehmet Öz",
                            Il = "Ankara",
                            Ilce = "Çankaya",
                            PostaKodu = "06000",
                            Ulke = "Türkiye",
                            VarsayilanParaBirimiId = tryParaBirimi.ParaBirimiID,
                            AktifMi = true,
                            Silindi = false,
                            OlusturmaTarihi = DateTime.Now,
                            GuncellemeTarihi = DateTime.Now
                        },
                        new Cari
                        {
                            CariID = Guid.NewGuid(),
                            CariKodu = "M00003",
                            Ad = "Ali Veli Elektronik",
                            CariTipi = "Müşteri",
                            Telefon = "0232 222 44 44",
                            Email = "info@aliveli.com",
                            WebSitesi = "www.aliveli.com",
                            VergiDairesi = "Konak",
                            VergiNo = "1357924680",
                            Adres = "Konak Mahallesi No:78 İzmir",
                            Yetkili = "Ali Veli",
                            Il = "İzmir",
                            Ilce = "Konak",
                            PostaKodu = "35000",
                            Ulke = "Türkiye",
                            VarsayilanParaBirimiId = tryParaBirimi.ParaBirimiID,
                            AktifMi = true,
                            Silindi = false,
                            OlusturmaTarihi = DateTime.Now,
                            GuncellemeTarihi = DateTime.Now
                        },
                        new Cari
                        {
                            CariID = Guid.NewGuid(),
                            CariKodu = "T00002",
                            Ad = "Ayşe Kaya İnşaat Malzemeleri",
                            CariTipi = "Tedarikçi",
                            Telefon = "0262 555 66 77",
                            Email = "info@aysekaya.com",
                            WebSitesi = "www.aysekaya.com",
                            VergiDairesi = "Merkez",
                            VergiNo = "2468013579",
                            Adres = "Merkez Mah. No:123 Kocaeli",
                            Yetkili = "Ayşe Kaya",
                            Il = "Kocaeli",
                            Ilce = "İzmit",
                            PostaKodu = "41000",
                            Ulke = "Türkiye",
                            VarsayilanParaBirimiId = tryParaBirimi.ParaBirimiID,
                            AktifMi = true,
                            Silindi = false,
                            OlusturmaTarihi = DateTime.Now,
                            GuncellemeTarihi = DateTime.Now
                        }
                    };
                    
                    // Her bir cariyi ekle
                    foreach (var cari in musteriler)
                    {
                        await _unitOfWork.CariRepository.AddAsync(cari);
                    }
                    
                    await _unitOfWork.SaveChangesAsync();
                    
                    return Json(new { success = true, message = "Örnek cariler başarıyla oluşturuldu." });
                }
                
                return Json(new { success = false, message = "Veritabanında zaten cari kayıtları mevcut." });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Örnek cari verileri oluşturulurken hata: {Message}", ex.Message);
                return Json(new { success = false, message = $"Örnek veriler oluşturulurken hata: {ex.Message}" });
            }
        }

        [HttpGet]
        public async Task<IActionResult> Musteriler()
        {
            var cariler = await _unitOfWork.CariRepository.GetAsync(
                filter: c => !c.Silindi && (c.CariTipi == "Müşteri" || c.CariTipi == "Müşteri ve Tedarikçi"));
            
            // Para birimi listesini view'a gönder
            ViewBag.ParaBirimleri = await _paraBirimiService.GetAllParaBirimleriAsync(true);
            ViewBag.CariTipi = "Müşteri";
            
            return View("CariTipi", cariler);
        }

        [HttpGet]
        public async Task<IActionResult> Tedarikciler()
        {
            var cariler = await _unitOfWork.CariRepository.GetAsync(
                filter: c => !c.Silindi && (c.CariTipi == "Tedarikçi" || c.CariTipi == "Müşteri ve Tedarikçi"));
            
            // Para birimi listesini view'a gönder
            ViewBag.ParaBirimleri = await _paraBirimiService.GetAllParaBirimleriAsync(true);
            ViewBag.CariTipi = "Tedarikçi";
            
            return View("CariTipi", cariler);
        }

        [HttpGet]
        public async Task<IActionResult> Pasifler()
        {
            try
            {
                // Tüm carileri al - silindiye bakmaksızın
                var cariler = await _unitOfWork.CariRepository.GetAll()
                    .IgnoreQueryFilters() // Tüm kayıtları almak için
                    .ToListAsync();
                
                // Para birimi listesini view'a gönder
                ViewBag.ParaBirimleri = await _paraBirimiService.GetAllParaBirimleriAsync(true);
                
                return View(cariler);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Pasif cariler yüklenirken hata: {Message}", ex.Message);
                TempData["ErrorMessage"] = "Pasif cariler listesi yüklenirken bir hata oluştu.";
                return View(new List<Data.Entities.Cari>());
            }
        }

        // Cari'nin varsayılan para birimini Ajax ile almak için endpoint ekliyorum
        [HttpGet]
        public async Task<IActionResult> GetCariVarsayilanParaBirimi(Guid id)
        {
            try
            {
                var cari = await _unitOfWork.CariRepository.GetByIdAsync(id);

                if (cari == null || !cari.VarsayilanParaBirimiId.HasValue)
                {
                    // Varsayılan para birimi yok, ana para birimini kullan
                    var defaultParaBirimi = await _paraBirimiService.GetAnaParaBirimiAsync();

                    if (defaultParaBirimi != null)
                    {
                        return Json(new { 
                            success = true, 
                            paraBirimiID = defaultParaBirimi.ParaBirimiID,
                            paraBirimiKodu = defaultParaBirimi.Kod,
                            paraBirimiAdi = defaultParaBirimi.Ad,
                            dovizKuru = 1.0
                        });
                    }
                    
                    return Json(new { success = false, message = "Ana para birimi bulunamadı" });
                }

                // Carinin para birimini al
                var paraBirimi = await _paraBirimiService.GetParaBirimiByIdAsync(cari.VarsayilanParaBirimiId.Value);
                if (paraBirimi == null)
                {
                    return Json(new { success = false, message = "Para birimi bulunamadı" });
                }

                // Para birimi için kur bilgisini al
                decimal dovizKuru = 1.0m; // Varsayılan
                
                // Ana para birimi değilse kur bilgisini çek
                var anaPB = await _paraBirimiService.GetAnaParaBirimiAsync();
                if (anaPB != null && paraBirimi.ParaBirimiID != anaPB.ParaBirimiID)
                {
                    var kurBilgisi = await _dovizKuruService.GetGuncelKurAsync(paraBirimi.Kod, "TRY");
                    if (kurBilgisi > 0)
                    {
                        dovizKuru = kurBilgisi;
                    }
                }

                return Json(new { 
                    success = true, 
                    paraBirimiID = paraBirimi.ParaBirimiID,
                    paraBirimiKodu = paraBirimi.Kod,
                    paraBirimiAdi = paraBirimi.Ad,
                    dovizKuru = dovizKuru
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Cari varsayılan para birimi alınırken hata: {Message}", ex.Message);
                return Json(new { success = false, message = "Para birimi bilgisi alınamadı" });
            }
        }

        // GET: Cari/GetCariDetails
        [HttpGet]
        public IActionResult GetCariDetails(Guid cariID)
        {
            try
            {
                if (cariID == Guid.Empty)
                {
                    return Json(new { success = false, message = "Geçersiz Cari ID." });
                }

                var cari = _unitOfWork.CariRepository.GetByIdAsync(cariID).Result;

                if (cari == null)
                {
                    return Json(new { success = false, message = "Cari bulunamadı." });
                }

                return Json(cari);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Cari detayları alınırken hata oluştu: {Message}", ex.Message);
                return Json(new { success = false, message = "Cari detayları alınırken hata oluştu." });
            }
        }

        // Yeni cari kodu oluşturma metodu
        private string GenerateCariKodu()
        {
            try
            {
                // Bugünün tarihini al
                var today = DateTime.Now;
                var year = today.Year.ToString().Substring(2); // Son iki hane
                var month = today.Month.ToString().PadLeft(2, '0');
                var day = today.Day.ToString().PadLeft(2, '0');
                
                // Prefix oluştur: CRI-YYMMDD-
                var prefix = $"CRI-{year}{month}{day}-";
                
                // Bugün oluşturulan tüm carileri getir ve uygulama tarafında işlem yap
                var carileriGetir = _unitOfWork.CariRepository.GetAllAsync().Result;
                
                // Prefix ile başlayan carileri filtrele ve sırala
                var lastCari = carileriGetir
                    .Where(c => c.CariKodu != null && c.CariKodu.StartsWith(prefix))
                    .OrderByDescending(c => c.CariKodu)
                    .FirstOrDefault();
                
                int sequence = 1;
                if (lastCari != null && lastCari.CariKodu != null)
                {
                    // Son cari kodundan sıra numarasını çıkar
                    var parts = lastCari.CariKodu.Split('-');
                    if (parts.Length == 3 && int.TryParse(parts[2], out int lastSeq))
                    {
                        sequence = lastSeq + 1;
                    }
                }
                
                // CRI-YYMMDD-001 formatında yeni kod oluştur
                return $"{prefix}{sequence.ToString().PadLeft(3, '0')}";
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Cari kodu oluşturulurken hata: {Message}", ex.Message);
                // Hata durumunda basit bir yedek kod oluştur
                return $"CRI-{DateTime.Now.ToString("yyMMddHHmmss")}";
            }
        }

        // GET: Cari/GetCariParaBirimi
        [HttpGet]
        public async Task<IActionResult> GetCariParaBirimi(Guid id)
        {
            try
            {
                var cari = await _unitOfWork.CariRepository.GetByIdAsync(id);

                if (cari == null)
                {
                    return Json(new { success = false, message = "Cari bulunamadı" });
                }

                // Navigation property üzerinden para birimi bilgilerini almak
                if (cari.VarsayilanParaBirimi == null)
                {
                    return Json(new { success = false, message = "Cari için varsayılan para birimi tanımlanmamış" });
                }

                return Json(new
                {
                    success = true,
                    paraBirimiKodu = cari.VarsayilanParaBirimi.Kod,
                    paraBirimiAdi = cari.VarsayilanParaBirimi.Ad,
                    paraBirimiSembol = cari.VarsayilanParaBirimi.Sembol
                });
            }
            catch (Exception ex)
            {
                _logger.LogError("Cari para birimi bilgisi alınamadı: {Message}", ex.ToString());
                return Json(new { success = false, message = "Cari para birimi bilgisi alınamadı" });
            }
        }

        private void PrepareCariHareketViewBag(Guid cariID)
        {
            var cari = _unitOfWork.CariRepository.GetByIdAsync(cariID).Result;
            ViewBag.CariID = cariID;
            ViewBag.CariAdi = cari?.Ad ?? string.Empty;
            
            ViewBag.HareketTurleri = new List<string>
            {
                "Borç",
                "Alacak",
                "Ödeme",
                "Tahsilat"
            };
        }

        [HttpGet]
        public async Task<IActionResult> YeniHareket(Guid cariID)
        {
            var cari = await _unitOfWork.CariRepository.GetByIdAsync(cariID);
            if (cari == null)
            {
                return NotFound();
            }

            var model = new CariHareketCreateViewModel
            {
                CariID = cariID,
                CariAdi = cari.Ad,
                Tarih = DateTime.Now,
                HareketTuru = "Tahsilat" // HareketTuru için varsayılan değer atıyoruz
            };

            PrepareCariHareketViewBag(cariID);
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> YeniHareket(CariHareketCreateViewModel model)
        {
            if (!ModelState.IsValid)
            {
                PrepareCariHareketViewBag(model.CariID);
                return View(model);
            }

            try
            {
                // Transaction başlat
                await _unitOfWork.BeginTransactionAsync();
                
                var cariHareket = new Data.Entities.CariHareket
                {
                    CariHareketID = Guid.NewGuid(),
                    CariID = model.CariID,
                    Tarih = model.Tarih,
                    HareketTuru = model.HareketTuru,
                    Tutar = model.Tutar,
                    Aciklama = model.Aciklama,
                    ReferansNo = model.ReferansNo,
                    ReferansTuru = model.ReferansTuru,
                    Borc = model.HareketTuru == "Borç" || model.HareketTuru == "Ödeme" ? model.Tutar : 0,
                    Alacak = model.HareketTuru == "Alacak" || model.HareketTuru == "Tahsilat" ? model.Tutar : 0,
                    OlusturmaTarihi = DateTime.Now,
                    OlusturanKullaniciID = User.FindFirstValue(ClaimTypes.NameIdentifier) != null ? 
                        Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)) : Guid.Empty,
                    Silindi = false
                };
                
                if (model.VadeTarihi.HasValue)
                {
                    cariHareket.VadeTarihi = model.VadeTarihi.Value;
                }
                
                await _unitOfWork.CariHareketRepository.AddAsync(cariHareket);
                await _unitOfWork.CompleteAsync();
                await _unitOfWork.CommitTransactionAsync();
                
                TempData["SuccessMessage"] = "Cari hareket başarıyla eklendi.";
                
                return RedirectToAction("Details", new { id = model.CariID });
            }
            catch (Exception ex)
            {
                await _unitOfWork.RollbackTransactionAsync();
                _logger.LogError(ex, "Cari hareket eklenirken hata oluştu: {Message}", ex.Message);
                TempData["ErrorMessage"] = "Cari hareket eklenirken bir hata oluştu.";
                
                PrepareCariHareketViewBag(model.CariID);
                return View(model);
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CariHareketSil(Guid hareketID, Guid cariID)
        {
            var cariHareket = await _unitOfWork.CariHareketRepository.GetByIdAsync(hareketID);
            
            if (cariHareket == null || cariHareket.Silindi)
            {
                return NotFound();
            }
            
            try
            {
                // Soft delete uygula
                cariHareket.Silindi = true;
                cariHareket.GuncellemeTarihi = DateTime.Now;
                
                await _unitOfWork.CariHareketRepository.UpdateAsync(cariHareket);
                await _unitOfWork.SaveChangesAsync();
                
                TempData["SuccessMessage"] = "Cari hareket başarıyla silindi.";
                return RedirectToAction(nameof(Details), new { id = cariID });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Cari hareket silinirken hata oluştu: {Message}", ex.Message);
                TempData["ErrorMessage"] = "Cari hareket silinirken bir hata oluştu.";
                return RedirectToAction(nameof(Details), new { id = cariID });
            }
        }

        [HttpGet]
        public async Task<IActionResult> AvansBakiye(Guid id)
        {
            var cari = await _unitOfWork.CariRepository.GetByIdAsync(id);
            if (cari == null)
            {
                return NotFound();
            }
            
            var viewModel = new CariHareketCreateViewModel
            {
                CariID = cari.CariID,
                CariAdi = cari.Ad,
                Tarih = DateTime.Now,
                HareketTuru = "Avans", // Required alan için varsayılan değer atandı
                Tutar = 0
            };
            
            return View("HareketEkle", viewModel);
        }

        [HttpGet]
        public async Task<IActionResult> YeniBakiyeDuzenle(Guid id)
        {
            var cari = await _unitOfWork.CariRepository.GetByIdAsync(id);
            if (cari == null)
            {
                return NotFound();
            }
            
            var viewModel = new CariHareketCreateViewModel
            {
                CariID = cari.CariID,
                CariAdi = cari.Ad,
                Tarih = DateTime.Now,
                HareketTuru = "Bakiye Düzeltme", // Required alan için varsayılan değer atandı
                Tutar = 0,
                Aciklama = "Bakiye düzeltme"
            };
            
            return View("HareketEkle", viewModel);
        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> RestoreCari(Guid id)
        {
            try
            {
                // SoftDeleteService üzerinden işlem yap
                var softDeleteService = HttpContext.RequestServices.GetService<ISoftDeleteService<Cari>>();
                if (softDeleteService == null)
                {
                    return Json(new { success = false, message = "Geri getirme işlemi için gerekli servis bulunamadı." });
                }
                
                bool success = await softDeleteService.RestoreByIdAsync(id);
                
                if (success)
                {
                    // Geri getirilen carinin adını bulmak için
                    var cari = await _unitOfWork.CariRepository.GetByIdAsync(id);
                    string cariAdi = cari?.Ad ?? "Cari";
                    
                    // Logla
                    await _logService.CariAktifleştirmeLogOlustur(cari.CariID.ToString(), cari.Ad, "Cari kaydı geri yüklendi.");
                    
                    return Json(new { 
                        success = true, 
                        message = $"{cariAdi} adlı cari başarıyla geri getirildi." 
                    });
                }
                else
                {
                    return Json(new { success = false, message = "Silinmiş cari bulunamadı veya geri getirilemedi." });
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Cari geri getirilirken hata: {Message}", ex.Message);
                return Json(new { success = false, message = $"Cari geri getirilirken bir hata oluştu: {ex.Message}" });
            }
        }

        [HttpPost]
        public async Task<IActionResult> SetPassive(Guid id)
        {
            try
            {
                var cari = await _unitOfWork.CariRepository.GetFirstOrDefaultAsync(c => c.CariID == id && !c.Silindi);
                if (cari == null)
                {
                    return Json(new { success = false, message = "Cari bulunamadı." });
                }
                
                cari.AktifMi = false;
                cari.GuncellemeTarihi = DateTime.Now;
                cari.SonGuncelleyenKullaniciId = Guid.Parse(User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value);
                
                await _unitOfWork.CariRepository.UpdateAsync(cari);
                await _unitOfWork.SaveAsync();
                
                // Log oluştur
                await _logService.CariPasifleştirmeLogOlustur(
                    cari.CariID.ToString(),
                    cari.Ad,
                    "Cari pasif duruma alındı."
                );
                
                return Json(new { 
                    success = true, 
                    message = $"{cari.Ad} carisi başarıyla pasif duruma alındı." 
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Cari pasife alma hatası: {Message}", ex.Message);
                return Json(new { success = false, message = $"Cari pasife alınırken bir hata oluştu: {ex.Message}" });
            }
        }

        [HttpPost]
        public async Task<IActionResult> SetActive(Guid id)
        {
            try
            {
                var cari = await _unitOfWork.CariRepository.GetAll()
                    .IgnoreQueryFilters() // Silindi filtresini devre dışı bırak
                    .FirstOrDefaultAsync(c => c.CariID == id);
                    
                if (cari == null)
                {
                    return Json(new { success = false, message = "Cari bulunamadı." });
                }
                
                cari.AktifMi = true;
                cari.GuncellemeTarihi = DateTime.Now;
                cari.SonGuncelleyenKullaniciId = Guid.Parse(User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value);
                
                await _unitOfWork.CariRepository.UpdateAsync(cari);
                await _unitOfWork.SaveAsync();
                
                // Log oluştur
                await _logService.CariAktifleştirmeLogOlustur(
                    cari.CariID.ToString(),
                    cari.Ad,
                    "Cari aktif duruma alındı."
                );
                
                return Json(new { 
                    success = true, 
                    message = $"{cari.Ad} carisi başarıyla aktif duruma alındı." 
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Cari aktif etme hatası: {Message}", ex.Message);
                return Json(new { success = false, message = $"Cari aktif edilirken bir hata oluştu: {ex.Message}" });
            }
        }

        [HttpPost]
        [Authorize(Roles = "Admin,MuhasebeYonetici")]
        public async Task<IActionResult> SetDelete(Guid id)
        {
            try
            {
                var cari = await _unitOfWork.CariRepository.GetFirstOrDefaultAsync(c => c.CariID == id && !c.Silindi);
                if (cari == null)
                {
                    return Json(new { success = false, message = "Cari bulunamadı." });
                }

                // İlişkili kayıtları kontrol et
                var iliskiliKayitVar = await _unitOfWork.CariHareketRepository.AnyAsync(ch => ch.CariID == id && !ch.Silindi);
                iliskiliKayitVar = iliskiliKayitVar || await _unitOfWork.FaturaRepository.AnyAsync(f => f.CariID == id && !f.Silindi);
                
                if (iliskiliKayitVar)
                {
                    return Json(new { 
                        success = false, 
                        message = $"{cari.Ad} carisine ait kayıtlar (hareketler, faturalar, vb.) bulunduğu için silinemez. Öncelikle bu kayıtları silmeniz gerekiyor." 
                    });
                }
                
                cari.Silindi = true;
                cari.GuncellemeTarihi = DateTime.Now;
                cari.SonGuncelleyenKullaniciId = Guid.Parse(User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value);
                
                await _unitOfWork.CariRepository.UpdateAsync(cari);
                await _unitOfWork.SaveAsync();
                
                // Log oluştur
                await _logService.CariSilmeLogOlustur(
                    cari.CariID.ToString(),
                    cari.Ad,
                    "Cari silindi."
                );
                
                return Json(new { 
                    success = true, 
                    message = $"{cari.Ad} carisi başarıyla silindi." 
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Cari silme hatası: {Message}", ex.Message);
                return Json(new { success = false, message = $"Cari silinirken bir hata oluştu: {ex.Message}" });
            }
        }
    }
}