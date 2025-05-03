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

        public CariController(
            IUnitOfWork unitOfWork,
            UserManager<ApplicationUser> userManager,
            ILogService logService,
            IMenuService menuService,
            RoleManager<IdentityRole> roleManager,
            ILogger<CariController> logger,
            ParaBirimiService paraBirimiService,
            IDovizKuruService dovizKuruService,
            IWebHostEnvironment webHostEnvironment) 
            : base(menuService, userManager, roleManager, logService)
        {
            _unitOfWork = unitOfWork;
            _logger = logger;
            _paraBirimiService = paraBirimiService;
            _dovizKuruService = dovizKuruService;
            _webHostEnvironment = webHostEnvironment;
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
            // Silindi filtresini kaldırarak carileri getir - silinmiş cariler de görüntülenebilmeli
            var cari = await _unitOfWork.CariRepository.GetAll()
                .Where(c => c.CariID == id) // Silindi filtresini kaldırdık
                .FirstOrDefaultAsync();
                
            if (cari == null)
            {
                return NotFound();
            }

            // Para birimi listesini view'a gönder
            ViewBag.ParaBirimleri = await _paraBirimiService.GetAllParaBirimleriAsync(true);

            // Cari hareketlerini ve faturaları güvenli bir şekilde yükle
            try
            {
                // Hareketleri veritabanı seviyesinde filtreleme ile getir - sadece silinmemiş hareketler
                var tumHareketler = await _unitOfWork.CariHareketRepository.GetAll()
                    .Where(c => !c.Silindi && c.CariID == id)
                    .ToListAsync();
                
                // Önce listeyi alalım, sonra bellekte sıralama yapalım
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

                // Son faturaları getir - veritabanı seviyesinde filtreleme
                var faturalar = await _unitOfWork.FaturaRepository.GetAll()
                    .Where(f => !f.Silindi && f.CariID == id)
                    .ToListAsync();
                    
                // Bellekte sıralama
                cari.SonFaturalar = faturalar
                    .OrderByDescending(f => f.FaturaTarihi)
                    .Take(5)
                    .Cast<object>()
                    .ToList();
            }
            catch (Exception ex)
            {
                // Hata oluştuğunda loga yaz ve varsayılan (boş) listelerle devam et
                _logger.LogError(ex, "Cari detayları yüklenirken hata: {Message}", ex.Message);
                cari.CariHareketler = new List<Data.Entities.CariHareket>();
                cari.SonFaturalar = new List<object>();
            }

            return View(cari);
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
                    BaslangicBakiye = 0,
                    Ulke = "Türkiye"
                };
                
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
                    CariUnvani = model.CariUnvani,
                    CariKodu = model.CariKodu,
                    CariTipi = model.CariTipi,
                    VergiNo = model.VergiNo,
                    VergiDairesi = model.VergiDairesi,
                    Telefon = model.Telefon,
                    Email = model.Email,
                    Yetkili = model.Yetkili,
                    Adres = model.Adres,
                    Il = model.Il,
                    Ilce = model.Ilce,
                    PostaKodu = model.PostaKodu,
                    Ulke = model.Ulke,
                    WebSitesi = model.WebSitesi,
                    BaslangicBakiye = model.BaslangicBakiye,
                    VarsayilanParaBirimiId = model.VarsayilanParaBirimiId,
                    Aciklama = model.Aciklama,
                    Notlar = model.Notlar,
                    AktifMi = model.Aktif,
                    OlusturmaTarihi = DateTime.Now,
                    OlusturanKullaniciId = Guid.Parse(User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value)
                };
                
                // Cari kaydı oluştur
                await _unitOfWork.CariRepository.AddAsync(cari);
                await _unitOfWork.SaveAsync();
                
                // Log oluştur
                await _logService.CariOlusturmaLogOlustur(
                    cari.CariID.ToString(),
                    cari.Ad,
                    "Yeni cari oluşturuldu."
                );
                
                // AJAX isteğine yanıt ver
                if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
                {
                    return Json(new { 
                        success = true, 
                        message = $"{cari.Ad} carisi başarıyla oluşturuldu." 
                    });
                }
                
                // Normal istekte yönlendirme
                TempData["SuccessMessage"] = $"{cari.Ad} carisi başarıyla oluşturuldu.";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Cari oluşturma hatası: {Message}", ex.Message);
                
                // AJAX isteğine hata yanıtı
                if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
                {
                    return Json(new { success = false, message = $"Cari oluşturulurken bir hata oluştu: {ex.Message}" });
                }
                
                // Normal istekte hata gösterimi
                ModelState.AddModelError("", $"Cari oluşturulurken bir hata oluştu: {ex.Message}");
                ViewBag.ParaBirimleri = await _paraBirimiService.GetAllParaBirimleriAsync(true);
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
                    CariUnvani = cari.CariUnvani,
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
                    BaslangicBakiye = cari.BaslangicBakiye,
                    VarsayilanParaBirimiId = cari.VarsayilanParaBirimiId,
                    Aciklama = cari.Aciklama,
                    Notlar = cari.Notlar,
                    AktifMi = cari.AktifMi
                };
                
                // Para birimlerini view'a gönder
                ViewBag.ParaBirimleri = await _paraBirimiService.GetAllParaBirimleriAsync(true);
                
                // AJAX isteğiyse partial view döndür
                if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
                {
                    return PartialView("_EditPartial", model);
                }
                
                // Normal istekte tam sayfa döndür
                return View(model);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Cari düzenleme sayfası yüklenirken hata: {Message}", ex.Message);
                
                if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
                {
                    return Json(new { success = false, message = $"Cari bilgileri yüklenirken bir hata oluştu: {ex.Message}" });
                }
                
                TempData["ErrorMessage"] = $"Cari düzenleme sayfası yüklenirken bir hata oluştu: {ex.Message}";
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
                
                // Cari kaydını bul
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
                
                // Aynı kodu kullanan başka bir cari var mı kontrol et (kendisi hariç)
                if (await _unitOfWork.CariRepository.AnyAsync(c => c.CariKodu == model.CariKodu && c.CariID != model.CariID && !c.Silindi))
                {
                    ModelState.AddModelError("CariKodu", "Bu cari kodu zaten başka bir cari tarafından kullanılıyor.");
                    ViewBag.ParaBirimleri = await _paraBirimiService.GetAllParaBirimleriAsync(true);
                    
                    if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
                    {
                        return Json(new { success = false, message = "Bu cari kodu zaten başka bir cari tarafından kullanılıyor." });
                    }
                    
                    return View(model);
                }
                
                // Cari verileri güncelle
                cari.Ad = model.Ad;
                cari.CariUnvani = model.CariUnvani;
                cari.CariKodu = model.CariKodu;
                cari.CariTipi = model.CariTipi;
                cari.VergiNo = model.VergiNo;
                cari.VergiDairesi = model.VergiDairesi;
                cari.Telefon = model.Telefon;
                cari.Email = model.Email;
                cari.Yetkili = model.Yetkili;
                cari.Adres = model.Adres;
                cari.Il = model.Il;
                cari.Ilce = model.Ilce;
                cari.PostaKodu = model.PostaKodu;
                cari.Ulke = model.Ulke;
                cari.WebSitesi = model.WebSitesi;
                cari.BaslangicBakiye = model.BaslangicBakiye;
                cari.VarsayilanParaBirimiId = model.VarsayilanParaBirimiId;
                cari.Aciklama = model.Aciklama;
                cari.Notlar = model.Notlar;
                cari.AktifMi = model.AktifMi;
                cari.GuncellemeTarihi = DateTime.Now;
                cari.SonGuncelleyenKullaniciId = Guid.Parse(User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value);
                
                // Güncellemeyi kaydet
                await _unitOfWork.CariRepository.UpdateAsync(cari);
                await _unitOfWork.SaveAsync();
                
                // Log oluştur
                await _logService.CariGuncellemeLogOlustur(
                    cari.CariID.ToString(),
                    cari.Ad,
                    "Cari bilgileri güncellendi."
                );
                
                // AJAX isteğine yanıt ver
                if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
                {
                    return Json(new { 
                        success = true, 
                        message = $"{cari.Ad} carisi başarıyla güncellendi." 
                    });
                }
                
                // Normal istekte yönlendirme
                TempData["SuccessMessage"] = $"{cari.Ad} carisi başarıyla güncellendi.";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Cari güncelleme hatası: {Message}", ex.Message);
                
                // AJAX isteğine hata yanıtı
                if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
                {
                    return Json(new { success = false, message = $"Cari güncellenirken bir hata oluştu: {ex.Message}" });
                }
                
                // Normal istekte hata gösterimi
                ModelState.AddModelError("", $"Cari güncellenirken bir hata oluştu: {ex.Message}");
                ViewBag.ParaBirimleri = await _paraBirimiService.GetAllParaBirimleriAsync(true);
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
                // Cari varlığını kontrol et
                var cari = await _unitOfWork.CariRepository.GetByIdAsync(id);
                if (cari == null || cari.Silindi)
                {
                    return NotFound();
                }

                var startDate = baslangicTarihi ?? DateTime.Now.AddMonths(-1).Date;
                var endDate = bitisTarihi ?? DateTime.Now.Date.AddDays(1).AddSeconds(-1);
                
                // Eğer paraBirimiId belirtilmemişse ve carinin varsayılan para birimi varsa, onu kullan
                if (!paraBirimiId.HasValue && cari.VarsayilanParaBirimiId.HasValue)
                {
                    paraBirimiId = cari.VarsayilanParaBirimiId.Value;
                }

                // Hareketleri veritabanı seviyesinde filtreleme ile getir
                var tumHareketler = await _unitOfWork.CariHareketRepository.GetAll()
                    .Where(c => !c.Silindi && c.CariID == id)
                    .ToListAsync();
                
                // Açılış bakiyesi hareketlerini ayrı al 
                var acilisBakiyeHareketleri = tumHareketler.Where(h => h.HareketTuru == "Açılış bakiyesi").OrderBy(h => h.Tarih).ToList();
                
                // Diğer hareketleri al
                var digerHareketler = tumHareketler.Where(h => h.HareketTuru != "Açılış bakiyesi").OrderBy(h => h.Tarih).ToList();
                
                // Veritabanında açılış bakiyesi hareketi yoksa, cari.BaslangicBakiye'yi kullan
                decimal baslangicBakiyesi = 0;
                
                // İlk açılış bakiyesi hareketi varsa onu kullan
                if (acilisBakiyeHareketleri.Any())
                {
                    var ilkAcilisBakiyeHareketi = acilisBakiyeHareketleri.First();
                    baslangicBakiyesi = ilkAcilisBakiyeHareketi.Alacak - ilkAcilisBakiyeHareketi.Borc;
                }
                else
                {
                    baslangicBakiyesi = cari.BaslangicBakiye;
                }
                
                // Şimdi gösterilecek hareketleri seçilen tarih aralığına göre filtrele
                var gosterilecekHareketler = digerHareketler
                    .Where(h => h.Tarih >= startDate && h.Tarih <= endDate)
                    .ToList();

                // Başlangıç tarihinden önceki diğer hareketlerin etkisini hesapla
                decimal oncekiHareketlerinEtkisi = 0;
                foreach (var oncekiHareket in digerHareketler.Where(h => h.Tarih < startDate))
                {
                    if (oncekiHareket.HareketTuru == "Bakiye Düzeltmesi")
                    {
                        oncekiHareketlerinEtkisi += oncekiHareket.Alacak - oncekiHareket.Borc;
                    }
                    else if (oncekiHareket.HareketTuru == "Ödeme" || oncekiHareket.HareketTuru == "Borç" || oncekiHareket.HareketTuru == "Çıkış")
                    {
                        oncekiHareketlerinEtkisi -= oncekiHareket.Tutar;
                    }
                    else if (oncekiHareket.HareketTuru == "Tahsilat" || oncekiHareket.HareketTuru == "Alacak" || oncekiHareket.HareketTuru == "Giriş")
                    {
                        oncekiHareketlerinEtkisi += oncekiHareket.Tutar;
                    }
                }
                
                // Başlangıç bakiyesine önceki hareketlerin etkisini ekle
                baslangicBakiyesi += oncekiHareketlerinEtkisi;
                
                var model = new Models.CariEkstreViewModel
                {
                    Id = cari.CariID,
                    CariAdi = cari.Ad,
                    VergiNo = cari.VergiNo,
                    Adres = cari.Adres,
                    BaslangicTarihi = startDate,
                    BitisTarihi = endDate,
                    RaporTarihi = DateTime.Now,
                    Hareketler = new List<Models.CariHareketViewModel>(),
                    ParaBirimiKodu = "TRY",
                    ParaBirimiSembol = "₺",
                    DovizKuru = 1M,
                    DovizKuruTarihi = DateTime.Now,
                    OrijinalParaBirimi = true
                };
                
                // Para birimi seçilmiş mi kontrol et
                if (paraBirimiId.HasValue && paraBirimiId.Value != Guid.Empty)
                {
                    try 
                    {
                        // Para birimi bilgilerini getir
                        var paraBirimi = await _paraBirimiService.GetParaBirimiByIdAsync(paraBirimiId.Value);
                        
                        if (paraBirimi != null)
                        {
                            // Para birimi ve kur bilgilerini modele ekle
                            model.ParaBirimiId = paraBirimi.ParaBirimiID;
                            model.ParaBirimiKodu = paraBirimi.Kod;
                            model.ParaBirimiSembol = paraBirimi.Sembol;
                            model.OrijinalParaBirimi = false;
                            
                            // Seçilen para birimine göre güncel kur bilgisini al
                            var kurDegeri = await _dovizKuruService.GetSonKurDegeriByParaBirimiAsync(paraBirimi.ParaBirimiID);
                            
                            if (kurDegeri != null)
                            {
                                model.DovizKuru = kurDegeri.Satis;
                                model.DovizKuruTarihi = kurDegeri.Tarih;
                            }
                            else
                            {
                                // Kur bulunamadıysa varsayılan değeri 1 olarak ayarla
                                model.DovizKuru = 1M;
                                TempData["Warning"] = $"{paraBirimi.Kod} para birimi için güncel kur bilgisi bulunamadı. Varsayılan döviz kuru 1:1 olarak ayarlandı.";
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, $"Döviz kuru bilgisi alınırken hata oluştu: {ex.Message}");
                        TempData["Warning"] = "Döviz kuru bilgisi alınırken bir hata oluştu, TL bazında ekstre gösteriliyor.";
                    }
                }
                
                // Para birimi bilgilerini ViewBag'e ekle (dropdown için)
                ViewBag.ParaBirimleri = await _paraBirimiService.GetAllParaBirimleriAsync(true);
                ViewBag.SeciliParaBirimiId = model.ParaBirimiId;
                
                // Carinin varsayılan para birimini modele ekle
                model.VarsayilanParaBirimiId = cari.VarsayilanParaBirimiId;
                
                // Açılış bakiyesi gösterimi
                var acilisBakiyeGosterimi = new Models.CariHareketViewModel
                {
                    Tarih = acilisBakiyeHareketleri.Any() ? acilisBakiyeHareketleri.First().Tarih : cari.OlusturmaTarihi,
                    IslemTuru = "Açılış bakiyesi",
                    Aciklama = "Açılış Bakiyesi",
                    Kaynak = "Sistem",
                    Borc = baslangicBakiyesi < 0 ? Math.Abs(baslangicBakiyesi) : 0,
                    Alacak = baslangicBakiyesi > 0 ? baslangicBakiyesi : 0,
                    Bakiye = baslangicBakiyesi,
                    // Orijinal değerler (TL cinsinden)
                    OrijinalBorc = baslangicBakiyesi < 0 ? Math.Abs(baslangicBakiyesi) : 0,
                    OrijinalAlacak = baslangicBakiyesi > 0 ? baslangicBakiyesi : 0,
                    OrijinalBakiye = baslangicBakiyesi,
                    HareketParaBirimi = "TRY"
                };
                
                // Eğer farklı bir para birimi seçilmişse, açılış bakiyesini dönüştür
                if (!model.OrijinalParaBirimi && model.DovizKuru > 0)
                {
                    // Eğer cari.VarsayilanParaBirimiId ile paraBirimiId eşleşiyorsa doğrudan bakiyeyi kullan
                    if (cari.VarsayilanParaBirimiId.HasValue && paraBirimiId.HasValue && 
                        cari.VarsayilanParaBirimiId.Value == paraBirimiId.Value)
                    {
                        acilisBakiyeGosterimi.Borc = acilisBakiyeGosterimi.OrijinalBorc;
                        acilisBakiyeGosterimi.Alacak = acilisBakiyeGosterimi.OrijinalAlacak;
                        acilisBakiyeGosterimi.Bakiye = cari.BaslangicBakiye; // Doğrudan cari bakiyesini kullan
                    }
                    else
                    {
                        // Farklı para birimi seçilmişse dönüşüm yap
                        acilisBakiyeGosterimi.Borc = Math.Round(acilisBakiyeGosterimi.OrijinalBorc / model.DovizKuru, 2);
                        acilisBakiyeGosterimi.Alacak = Math.Round(acilisBakiyeGosterimi.OrijinalAlacak / model.DovizKuru, 2);
                        acilisBakiyeGosterimi.Bakiye = Math.Round(acilisBakiyeGosterimi.OrijinalBakiye / model.DovizKuru, 2);
                    }
                }
                
                model.Hareketler.Add(acilisBakiyeGosterimi);
                
                // Şimdi gösterilecek diğer hareketleri ekle ve bakiyeyi güncelle
                decimal bakiye = baslangicBakiyesi;
                decimal dovizBakiye = acilisBakiyeGosterimi.Bakiye; // Döviz cinsinden bakiye
                
                foreach (var hareket in gosterilecekHareketler)
                {
                    decimal hareketTutari = 0;
                    
                    if (hareket.HareketTuru == "Bakiye Düzeltmesi")
                    {
                        hareketTutari = hareket.Alacak - hareket.Borc;
                    }
                    else if (hareket.HareketTuru == "Ödeme" || hareket.HareketTuru == "Borç" || hareket.HareketTuru == "Çıkış")
                    {
                        hareketTutari = -hareket.Tutar;
                    }
                    else if (hareket.HareketTuru == "Tahsilat" || hareket.HareketTuru == "Alacak" || hareket.HareketTuru == "Giriş")
                    {
                        hareketTutari = hareket.Tutar;
                    }
                    
                    bakiye += hareketTutari;
                    
                    var cariHareket = new Models.CariHareketViewModel
                    {
                        Tarih = hareket.Tarih,
                        VadeTarihi = hareket.VadeTarihi,
                        Aciklama = hareket.Aciklama,
                        IslemTuru = hareket.HareketTuru,
                        EvrakNo = hareket.ReferansNo,
                        Kaynak = "Cari",
                        // Orijinal değerler (TL cinsinden)
                        OrijinalBorc = hareketTutari < 0 ? Math.Abs(hareketTutari) : 0,
                        OrijinalAlacak = hareketTutari > 0 ? hareketTutari : 0,
                        OrijinalBakiye = bakiye,
                        HareketParaBirimi = "TRY"
                    };
                    
                    // Eğer farklı bir para birimi seçilmişse değerleri dönüştür
                    if (!model.OrijinalParaBirimi && model.DovizKuru > 0)
                    {
                        // Eğer cari.VarsayilanParaBirimiId ile paraBirimiId eşleşiyorsa doğrudan değerleri kullan
                        if (cari.VarsayilanParaBirimiId.HasValue && paraBirimiId.HasValue && 
                            cari.VarsayilanParaBirimiId.Value == paraBirimiId.Value)
                        {
                            decimal hareketBakiye = bakiye;
                            if (model.Hareketler.Count > 0)
                            {
                                // Önceki hareketin bakiyesini al ve bu hareketi ekle
                                hareketBakiye = model.Hareketler.Last().Bakiye + (cariHareket.OrijinalAlacak - cariHareket.OrijinalBorc);
                            }
                            
                            cariHareket.Borc = cariHareket.OrijinalBorc;
                            cariHareket.Alacak = cariHareket.OrijinalAlacak;
                            cariHareket.Bakiye = hareketBakiye;
                            dovizBakiye = hareketBakiye;
                        }
                        else
                        {
                            decimal dovizTutari = Math.Round(hareketTutari / model.DovizKuru, 2);
                            dovizBakiye += dovizTutari;
                            
                            cariHareket.Borc = dovizTutari < 0 ? Math.Round(Math.Abs(dovizTutari), 2) : 0;
                            cariHareket.Alacak = dovizTutari > 0 ? Math.Round(dovizTutari, 2) : 0;
                            cariHareket.Bakiye = Math.Round(dovizBakiye, 2);
                        }
                    }
                    
                    model.Hareketler.Add(cariHareket);
                }
                
                // Toplam ve bakiye hesaplamaları
                if (model.OrijinalParaBirimi)
                {
                    // TL cinsinden
                    model.ToplamBorc = model.Hareketler.Sum(h => h.OrijinalBorc);
                    model.ToplamAlacak = model.Hareketler.Sum(h => h.OrijinalAlacak);
                    model.Bakiye = bakiye;
                }
                else
                {
                    // Seçili para birimi cinsinden
                    model.ToplamBorc = model.Hareketler.Sum(h => h.Borc);
                    model.ToplamAlacak = model.Hareketler.Sum(h => h.Alacak);
                    model.Bakiye = dovizBakiye;
                }
                
                return View(model);
                }
                catch (Exception ex)
                {
                _logger.LogError(ex, "Cari ekstre görüntüleme hatası. CariID: {CariID}", id);
                TempData["ErrorMessage"] = "Ekstre görüntülenirken bir hata oluştu. Lütfen tekrar deneyin.";
                return RedirectToAction(nameof(Details), new { id = id });
            }
        }

        // Örnek veri oluşturma action'ı
        [HttpGet]
        public async Task<IActionResult> AddSampleData()
        {
            try
            {
                // Veritabanında cari kaydı olup olmadığını kontrol et
                var cariler = await _unitOfWork.CariRepository.GetAllAsync();
                if (cariler.Any())
                {
                    TempData["WarningMessage"] = "Veritabanında zaten cari kayıtları bulunmaktadır.";
                    return RedirectToAction(nameof(Index));
                }
                
                // Kullanıcı ID'sini al
                var userId = Guid.Parse(User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value);
                
                // Örnek cari verileri
                var cariListesi = new List<Data.Entities.Cari>
                {
                    new Data.Entities.Cari
                    {
                        CariID = Guid.NewGuid(),
                        Ad = "ABC Ticaret Ltd. Şti.",
                        CariKodu = "CARI001",
                        CariTipi = "Tedarikçi",
                        VergiNo = "1234567890",
                        VergiDairesi = "Kadıköy",
                        Telefon = "0216 123 45 67",
                        Email = "info@abc-ticaret.com",
                        Yetkili = "Ali Yılmaz",
                        BaslangicBakiye = 15000.00m,
                        Adres = "Bağdat Cad. No: 123, Kadıköy, İstanbul",
                        Aciklama = "Elektronik malzeme tedarikçisi",
                        Il = "İstanbul",
                        Ilce = "Kadıköy",
                        PostaKodu = "34720",
                        Ulke = "Türkiye",
                        WebSitesi = "www.abc-ticaret.com",
                        AktifMi = true,
                        OlusturanKullaniciId = userId,
                        OlusturmaTarihi = DateTime.Now
                    },
                    new Data.Entities.Cari
                    {
                        CariID = Guid.NewGuid(),
                        Ad = "XYZ Elektronik A.Ş.",
                        CariKodu = "CARI002",
                        CariTipi = "Tedarikçi",
                        VergiNo = "9876543210",
                        VergiDairesi = "Şişli",
                        Telefon = "0212 987 65 43",
                        Email = "info@xyz-elektronik.com",
                        Yetkili = "Mehmet Kaya",
                        BaslangicBakiye = 25000.00m,
                        Adres = "Büyükdere Cad. No: 456, Şişli, İstanbul",
                        Aciklama = "Bilgisayar ve elektronik ürünler",
                        Il = "İstanbul",
                        Ilce = "Şişli",
                        PostaKodu = "34394",
                        Ulke = "Türkiye",
                        WebSitesi = "www.xyz-elektronik.com",
                        AktifMi = true,
                        OlusturanKullaniciId = userId,
                        OlusturmaTarihi = DateTime.Now
                    },
                    new Data.Entities.Cari
                    {
                        CariID = Guid.NewGuid(),
                        Ad = "Akın Market",
                        CariKodu = "CARI003",
                        CariTipi = "Müşteri",
                        VergiNo = "1357924680",
                        VergiDairesi = "Beşiktaş",
                        Telefon = "0212 456 78 90",
                        Email = "iletisim@akinmarket.com",
                        Yetkili = "Hakan Akın",
                        BaslangicBakiye = -3500.00m,
                        Adres = "Barbaros Bulvarı No: 78, Beşiktaş, İstanbul",
                        Aciklama = "Süpermarket zinciri",
                        Il = "İstanbul",
                        Ilce = "Beşiktaş",
                        PostaKodu = "34353",
                        Ulke = "Türkiye",
                        WebSitesi = "www.akinmarket.com",
                        AktifMi = true,
                        OlusturanKullaniciId = userId,
                        OlusturmaTarihi = DateTime.Now
                    },
                    new Data.Entities.Cari
                    {
                        CariID = Guid.NewGuid(),
                        Ad = "Birlik Nakliyat",
                        CariKodu = "CARI004",
                        CariTipi = "Hizmet Sağlayıcı",
                        VergiNo = "2468013579",
                        VergiDairesi = "Ümraniye",
                        Telefon = "0216 789 01 23",
                        Email = "info@birliknakliyat.com",
                        Yetkili = "Fatih Demir",
                        BaslangicBakiye = 0.00m,
                        Adres = "Alemdağ Cad. No: 345, Ümraniye, İstanbul",
                        Aciklama = "Lojistik ve taşımacılık firması",
                        Il = "İstanbul",
                        Ilce = "Ümraniye",
                        PostaKodu = "34760",
                        Ulke = "Türkiye",
                        WebSitesi = "www.birliknakliyat.com",
                        AktifMi = true,
                        OlusturanKullaniciId = userId,
                        OlusturmaTarihi = DateTime.Now
                    },
                    new Data.Entities.Cari
                    {
                        CariID = Guid.NewGuid(),
                        Ad = "Yıldız Tekstil Ltd. Şti.",
                        CariKodu = "CARI005",
                        CariTipi = "Müşteri",
                        VergiNo = "3692581470",
                        VergiDairesi = "Merter",
                        Telefon = "0212 345 67 89",
                        Email = "satis@yildiztekstil.com",
                        Yetkili = "Ayşe Yıldız",
                        BaslangicBakiye = 8500.00m,
                        Adres = "Keresteciler Sitesi, Merter, İstanbul",
                        Aciklama = "Tekstil üretim ve ihracat firması",
                        Il = "İstanbul",
                        Ilce = "Güngören",
                        PostaKodu = "34010",
                        Ulke = "Türkiye",
                        WebSitesi = "www.yildiztekstil.com",
                        AktifMi = false,
                        OlusturanKullaniciId = userId,
                        OlusturmaTarihi = DateTime.Now
                    }
                };
                
                // Her bir cariyi ayrı ayrı ekle
                foreach (var cari in cariListesi)
                {
                    await _unitOfWork.CariRepository.AddAsync(cari);
                }
                
                // Örnek cari hareketleri
                var cari1Id = cariListesi[0].CariID;
                var cari2Id = cariListesi[1].CariID;
                var cari3Id = cariListesi[2].CariID;
                
                // Cari 1 için hareketler
                await _unitOfWork.CariHareketRepository.AddAsync(new Data.Entities.CariHareket 
                { 
                    CariHareketID = Guid.NewGuid(), 
                    CariID = cari1Id,
                    Tutar = 5000.00m,
                    Tarih = DateTime.Now.AddDays(-30),
                    HareketTuru = "Alış Faturası",
                    Aciklama = "Elektronik malzeme alımı",
                    OlusturmaTarihi = DateTime.Now.AddDays(-30),
                    OlusturanKullaniciId = userId
                });
                
                await _unitOfWork.CariHareketRepository.AddAsync(new Data.Entities.CariHareket 
                { 
                    CariHareketID = Guid.NewGuid(), 
                    CariID = cari1Id,
                    Tutar = 8000.00m,
                    Tarih = DateTime.Now.AddDays(-15),
                    HareketTuru = "Alış Faturası",
                    Aciklama = "Aylık stok tedariki",
                    VadeTarihi = DateTime.Now.AddDays(15),
                    OlusturmaTarihi = DateTime.Now.AddDays(-15),
                    OlusturanKullaniciId = userId
                });
                
                // Cari 2 için hareketler
                await _unitOfWork.CariHareketRepository.AddAsync(new Data.Entities.CariHareket 
                { 
                    CariHareketID = Guid.NewGuid(), 
                    CariID = cari2Id,
                    Tutar = 12000.00m,
                    Tarih = DateTime.Now.AddDays(-20),
                    HareketTuru = "Alış Faturası",
                    Aciklama = "Bilgisayar ekipmanları",
                    OlusturmaTarihi = DateTime.Now.AddDays(-20),
                    OlusturanKullaniciId = userId
                });
                
                // Cari 3 için hareketler
                await _unitOfWork.CariHareketRepository.AddAsync(new Data.Entities.CariHareket 
                { 
                    CariHareketID = Guid.NewGuid(), 
                    CariID = cari3Id,
                    Tutar = 3500.00m,
                    Tarih = DateTime.Now.AddDays(-10),
                    HareketTuru = "Satış Faturası",
                    Aciklama = "Toptan gıda ürünleri",
                    VadeTarihi = DateTime.Now.AddDays(20),
                    OlusturmaTarihi = DateTime.Now.AddDays(-10),
                    OlusturanKullaniciId = userId
                });
                
                // Değişiklikleri kaydet
                await _unitOfWork.SaveAsync();
                
                // Log oluştur
                await _logService.LogOlustur(
                    "Örnek cari verileri eklendi", 
                    LogTuru.Bilgi, 
                    "Cari", 
                    "Örnek Veriler", 
                    null,
                    kullaniciAdi: User.Identity?.Name ?? "Sistem",
                    basarili: true,
                    kategori: "Sistem"
                );
                
                TempData["SuccessMessage"] = "Örnek cari ve cari hareket verileri başarıyla oluşturuldu.";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"Örnek veri eklenirken bir hata oluştu: {ex.Message}";
                return RedirectToAction(nameof(Index));
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