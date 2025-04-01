using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System.Net.Http;
using System.Text.Json;
using System.IO;
using MuhasebeStokWebApp.Data;
using MuhasebeStokWebApp.Data.Entities;
using MuhasebeStokWebApp.Data.Entities.ParaBirimiModulu;
using MuhasebeStokWebApp.Services;
using MuhasebeStokWebApp.Services.ParaBirimiModulu;
using MuhasebeStokWebApp.ViewModels.ParaBirimiModulu;
using MuhasebeStokWebApp.Services.Menu;
using MuhasebeStokWebApp.Services.Interfaces;

namespace MuhasebeStokWebApp.Controllers.ParaBirimiModulu
{
    /// <summary>
    /// Para birimi, döviz kuru ve ilişkileri yöneten controller
    /// </summary>
    [Authorize]
    [Route("ParaBirimi")]
    public class ParaBirimiController : BaseController
    {
        private readonly ApplicationDbContext _context;
        private readonly MuhasebeStokWebApp.Services.ParaBirimiModulu.IParaBirimiService _paraBirimiService;
        private readonly ILogger<ParaBirimiController> _logger;
        private readonly IHttpClientFactory _httpClientFactory;
        private const string ApiKey = "645b5bebcab7cef56e1b609c";
        private const string ApiBaseUrl = "https://v6.exchangerate-api.com/v6/";
        private new readonly ILogService _logService;
        private new readonly UserManager<ApplicationUser> _userManager;
        private new readonly RoleManager<IdentityRole> _roleManager;

        public ParaBirimiController(
            ApplicationDbContext context,
            MuhasebeStokWebApp.Services.ParaBirimiModulu.IParaBirimiService paraBirimiService,
            ILogger<ParaBirimiController> logger,
            IHttpClientFactory httpClientFactory,
            IMenuService menuService,
            RoleManager<IdentityRole> roleManager,
            ILogService logService,
            UserManager<ApplicationUser> userManager)
            : base(menuService, userManager, roleManager, logService)
        {
            _context = context;
            _paraBirimiService = paraBirimiService;
            _logger = logger;
            _httpClientFactory = httpClientFactory;
            _logService = logService;
            _userManager = userManager;
            _roleManager = roleManager;
        }

        #region Para Birimi İşlemleri
        /// <summary>
        /// Para birimleri listesi
        /// </summary>
        [Route("")]
        [Route("Index")]
        public async Task<IActionResult> Index()
        {
            try
            {
                var paraBirimleri = await _paraBirimiService.GetAllParaBirimleriAsync(aktiflerOnly: false);
                return View(paraBirimleri);
            }
            catch (Exception ex)
            {
                await _logService.LogErrorAsync("Para birimleri listesi", ex.Message);
                TempData["Error"] = "Para birimleri listelenirken bir hata oluştu: " + ex.Message;
                return RedirectToAction("Index", "Home");
            }
        }

        /// <summary>
        /// Para birimi ekleme formu
        /// </summary>
        [Route("Ekle")]
        public IActionResult Ekle()
        {
            return View(new ParaBirimi());
        }

        /// <summary>
        /// Para birimi ekleme işlemi
        /// </summary>
        [HttpPost]
        [Route("Ekle")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Ekle(ParaBirimi paraBirimi)
        {
            if (!ModelState.IsValid)
            {
                return View(paraBirimi);
            }

            try
            {
                var result = await _paraBirimiService.AddParaBirimiAsync(paraBirimi);

                if (result != null)
                {
                    TempData["Success"] = $"{paraBirimi.Kod} para birimi başarıyla eklendi.";
                    return RedirectToAction(nameof(Index));
                }
                else
                {
                    await _logService.LogWarningAsync($"{paraBirimi.Kod} para birimi eklenirken bir hata oluştu.", null);
                    TempData["Error"] = "Para birimi eklenirken bir sorun oluştu.";
                    return View(paraBirimi);
                }
            }
            catch (Exception ex)
            {
                await _logService.LogErrorAsync("Para birimi ekleme", ex.Message);
                TempData["Error"] = "Para birimi eklenirken bir hata oluştu: " + ex.Message;
                return View(paraBirimi);
            }
        }

        /// <summary>
        /// Para birimi detay görüntüleme
        /// </summary>
        [Route("Detay/{id:guid}")]
        public async Task<IActionResult> Detay(Guid id)
        {
            try
            {
                var paraBirimi = await _paraBirimiService.GetParaBirimiByIdAsync(id);
                if (paraBirimi == null)
                {
                    return NotFound();
                }

                return View(paraBirimi);
            }
            catch (Exception ex)
            {
                await _logService.LogErrorAsync($"ID: {id} olan para birimi yükleme", ex.Message);
                TempData["Error"] = "Para birimi yüklenirken bir hata oluştu: " + ex.Message;
                return RedirectToAction(nameof(Index));
            }
        }

        /// <summary>
        /// Para birimi düzenleme formu
        /// </summary>
        [Route("Duzenle/{id:guid}")]
        public async Task<IActionResult> Duzenle(Guid id)
        {
            try
            {
                var paraBirimi = await _paraBirimiService.GetParaBirimiByIdAsync(id);
                if (paraBirimi == null)
                {
                    return NotFound();
                }

                return View(paraBirimi);
            }
            catch (Exception ex)
            {
                await _logService.LogErrorAsync($"ID: {id} olan para birimi yükleme", ex.Message);
                TempData["Error"] = "Para birimi yüklenirken bir hata oluştu: " + ex.Message;
                return RedirectToAction(nameof(Index));
            }
        }

        /// <summary>
        /// Para birimi düzenleme işlemi
        /// </summary>
        [HttpPost]
        [Route("Duzenle/{id:guid}")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Duzenle(Guid id, ParaBirimi paraBirimi)
        {
            if (id != paraBirimi.ParaBirimiID)
            {
                return NotFound();
            }

            if (!ModelState.IsValid)
            {
                return View(paraBirimi);
            }

            try
            {
                var result = await _paraBirimiService.UpdateParaBirimiAsync(paraBirimi);

                TempData["Success"] = $"{paraBirimi.Kod} para birimi başarıyla güncellendi.";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                await _logService.LogErrorAsync($"ID: {id} olan para birimi güncellenirken", ex.Message);
                TempData["Error"] = "Para birimi güncellenirken bir hata oluştu: " + ex.Message;
                return View(paraBirimi);
            }
        }

        /// <summary>
        /// Para birimi silme onay formu
        /// </summary>
        [Route("Sil/{id:guid}")]
        public async Task<IActionResult> Sil(Guid id)
        {
            try
            {
                var paraBirimi = await _paraBirimiService.GetParaBirimiByIdAsync(id);
                if (paraBirimi == null)
                {
                    return NotFound();
                }

                return View(paraBirimi);
            }
            catch (Exception ex)
            {
                await _logService.LogErrorAsync($"ID: {id} olan para birimi yükleme", ex.Message);
                TempData["Error"] = "Para birimi yüklenirken bir hata oluştu: " + ex.Message;
                return RedirectToAction(nameof(Index));
            }
        }

        /// <summary>
        /// Para birimi silme işlemi
        /// </summary>
        [HttpPost]
        [Route("Sil/{id:guid}")]
        [ValidateAntiForgeryToken]
        [ActionName("Sil")]
        public async Task<IActionResult> SilOnay(Guid id)
        {
            try
            {
                var result = await _paraBirimiService.DeleteParaBirimiAsync(id);

                if (result)
                {
                    TempData["Success"] = "Para birimi başarıyla silindi.";
                }
                else
                {
                    TempData["Error"] = "Para birimi silinirken bir sorun oluştu.";
                }

                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                await _logService.LogErrorAsync($"ID: {id} olan para birimi silinirken", ex.Message);
                TempData["Error"] = "Para birimi silinirken bir hata oluştu: " + ex.Message;
                return RedirectToAction(nameof(Index));
            }
        }

        /// <summary>
        /// Ana para birimi ayarlama
        /// </summary>
        [Route("AnaParaBirimiAyarla/{id:guid}")]
        public async Task<IActionResult> AnaParaBirimiAyarla(Guid id)
        {
            try
            {
                var result = await _paraBirimiService.SetAnaParaBirimiAsync(id);

                if (result)
                {
                    TempData["Success"] = "Ana para birimi başarıyla ayarlandı.";
                }
                else
                {
                    TempData["Error"] = "Ana para birimi ayarlanırken bir sorun oluştu.";
                }

                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                await _logService.LogErrorAsync($"ID: {id} olan para birimi ana para birimi olarak ayarlanırken", ex.Message);
                TempData["Error"] = "Ana para birimi ayarlanırken bir hata oluştu: " + ex.Message;
                return RedirectToAction(nameof(Index));
            }
        }
        #endregion

        #region Kur Değeri İşlemleri
        /// <summary>
        /// Kur değerleri listesi
        /// </summary>
        [Route("Kurlar")]
        public async Task<IActionResult> Kurlar()
        {
            try
            {
                var kurDegerleri = await _paraBirimiService.GetAllKurDegerleriAsync();
                
                // Uyarı ekle: Debug amaçlı görünüm dosyasının konumunu kontrol et
                bool viewModuluExists = System.IO.File.Exists(Path.Combine(Directory.GetCurrentDirectory(), "Views", "ParaBirimiModulu", "Kurlar.cshtml"));
                bool viewParaBirimiExists = System.IO.File.Exists(Path.Combine(Directory.GetCurrentDirectory(), "Views", "ParaBirimi", "Kurlar.cshtml"));
                
                if (!viewModuluExists && !viewParaBirimiExists)
                {
                    // View dosyası yoksa logla
                    await _logService.LogWarningAsync("Kurlar.cshtml view dosyası bulunamadı. ParaBirimiModulu ve ParaBirimi klasörlerinde arandı.", null);
                }
                
                return View(kurDegerleri);
            }
            catch (Exception ex)
            {
                await _logService.LogErrorAsync("Kur değerleri listelenirken", ex.Message);
                TempData["Error"] = $"Kur değerleri listelenirken bir hata oluştu: {ex.Message}";
                return View(new List<KurDegeri>());
            }
        }

        /// <summary>
        /// Kur değeri detayı
        /// </summary>
        [Route("KurDetay/{id:guid}")]
        public async Task<IActionResult> KurDetay(Guid id)
        {
            try
            {
                var kurDegeri = await _context.KurDegerleri
                    .Include(d => d.ParaBirimi)
                    .FirstOrDefaultAsync(m => m.KurDegeriID == id && !m.Silindi);

                if (kurDegeri == null)
                {
                    return NotFound();
                }

                return View(kurDegeri);
            }
            catch (Exception ex)
            {
                await _logService.LogErrorAsync($"ID: {id} olan kur değeri yükleme", ex.Message);
                TempData["Error"] = "Kur değeri yüklenirken bir hata oluştu: " + ex.Message;
                return RedirectToAction(nameof(Kurlar));
            }
        }

        /// <summary>
        /// Kur değeri ekleme formu
        /// </summary>
        [Route("KurEkle")]
        public async Task<IActionResult> KurEkle()
        {
            // Para birimleri listesini getir
            var paraBirimleri = await _paraBirimiService.GetAllParaBirimleriAsync();

            // Dropdown için ViewBag'e ekle
            ViewBag.ParaBirimleri = paraBirimleri.Select(p => new SelectListItem
            {
                Value = p.ParaBirimiID.ToString(),
                Text = $"{p.Kod} - {p.Ad}"
            }).ToList();

            return View(new KurDegeri
            {
                Tarih = DateTime.Today,
                Aktif = true
            });
        }

        /// <summary>
        /// Kur değeri ekleme işlemi
        /// </summary>
        [HttpPost]
        [Route("KurEkle")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> KurEkle(KurDegeri kurDegeri)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    // Oluşturma bilgilerini ayarla
                    kurDegeri.KurDegeriID = Guid.NewGuid();
                    kurDegeri.OlusturmaTarihi = DateTime.Now;
                    kurDegeri.OlusturanKullaniciID = User.Identity?.Name ?? "Sistem";

                    // Servisi kullanarak kur değerini ekle
                    var result = await _paraBirimiService.AddKurDegeriAsync(kurDegeri);

                    TempData["Success"] = "Kur değeri başarıyla eklendi.";
                    return RedirectToAction(nameof(Kurlar));
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError("", $"Bir hata oluştu: {ex.Message}");
                }
            }

            // Para birimleri listesini yeniden getir
            var paraBirimleri = await _paraBirimiService.GetAllParaBirimleriAsync();

            ViewBag.ParaBirimleri = paraBirimleri.Select(p => new SelectListItem
            {
                Value = p.ParaBirimiID.ToString(),
                Text = $"{p.Kod} - {p.Ad}"
            }).ToList();

            return View(kurDegeri);
        }

        /// <summary>
        /// Kur değeri düzenleme formu
        /// </summary>
        [Route("KurDuzenle/{id:guid}")]
        public async Task<IActionResult> KurDuzenle(Guid id)
        {
            try
            {
                var kurDegeri = await _context.KurDegerleri
                    .Include(d => d.ParaBirimi)
                    .FirstOrDefaultAsync(m => m.KurDegeriID == id && !m.Silindi);

                if (kurDegeri == null)
                {
                    return NotFound();
                }

                // Para birimleri listesini getir
                var paraBirimleri = await _paraBirimiService.GetAllParaBirimleriAsync();

                // Dropdown için ViewBag'e ekle
                ViewBag.ParaBirimleri = paraBirimleri.Select(p => new SelectListItem
                {
                    Value = p.ParaBirimiID.ToString(),
                    Text = $"{p.Kod} - {p.Ad}"
                }).ToList();

                return View(kurDegeri);
            }
            catch (Exception ex)
            {
                await _logService.LogErrorAsync($"ID: {id} olan kur değeri yükleme", ex.Message);
                TempData["Error"] = "Kur değeri yüklenirken bir hata oluştu: " + ex.Message;
                return RedirectToAction(nameof(Kurlar));
            }
        }

        /// <summary>
        /// Kur değeri düzenleme işlemi
        /// </summary>
        [HttpPost]
        [Route("KurDuzenle/{id:guid}")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> KurDuzenle(Guid id, KurDegeri kurDegeri)
        {
            if (id != kurDegeri.KurDegeriID)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    // Güncelleme bilgilerini ayarla
                    kurDegeri.GuncellemeTarihi = DateTime.Now;
                    kurDegeri.SonGuncelleyenKullaniciID = User.Identity?.Name ?? "Sistem";

                    // Servisi kullanarak kur değerini güncelle
                    var result = await _paraBirimiService.UpdateKurDegeriAsync(kurDegeri);

                    TempData["Success"] = "Kur değeri başarıyla güncellendi.";
                    return RedirectToAction(nameof(Kurlar));
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError("", $"Bir hata oluştu: {ex.Message}");
                }
            }

            // Para birimleri listesini yeniden getir
            var paraBirimleri = await _paraBirimiService.GetAllParaBirimleriAsync();

            ViewBag.ParaBirimleri = paraBirimleri.Select(p => new SelectListItem
            {
                Value = p.ParaBirimiID.ToString(),
                Text = $"{p.Kod} - {p.Ad}"
            }).ToList();

            return View(kurDegeri);
        }

        /// <summary>
        /// Kur değeri silme onayı
        /// </summary>
        [Route("KurSil/{id:guid}")]
        public async Task<IActionResult> KurSil(Guid id)
        {
            try
            {
                var kurDegeri = await _context.KurDegerleri
                    .Include(d => d.ParaBirimi)
                    .FirstOrDefaultAsync(m => m.KurDegeriID == id && !m.Silindi);

                if (kurDegeri == null)
                {
                    return NotFound();
                }

                return View(kurDegeri);
            }
            catch (Exception ex)
            {
                await _logService.LogErrorAsync($"ID: {id} olan kur değeri yükleme", ex.Message);
                TempData["Error"] = "Kur değeri yüklenirken bir hata oluştu: " + ex.Message;
                return RedirectToAction(nameof(Kurlar));
            }
        }

        /// <summary>
        /// Kur değeri silme işlemi
        /// </summary>
        [HttpPost]
        [Route("KurSil/{id:guid}")]
        [ValidateAntiForgeryToken]
        [ActionName("KurSil")]
        public async Task<IActionResult> KurSilOnay(Guid id)
        {
            try
            {
                var result = await _paraBirimiService.DeleteKurDegeriAsync(id);

                if (result)
                {
                    TempData["Success"] = "Kur değeri başarıyla silindi.";
                }
                else
                {
                    TempData["Error"] = "Kur değeri silinirken bir sorun oluştu.";
                }

                return RedirectToAction(nameof(Kurlar));
            }
            catch (Exception ex)
            {
                await _logService.LogErrorAsync($"ID: {id} olan kur değeri silinirken", ex.Message);
                TempData["Error"] = "Kur değeri silinirken bir hata oluştu: " + ex.Message;
                return RedirectToAction(nameof(Kurlar));
            }
        }

        /// <summary>
        /// Kur değerlerini API'den güncelleme
        /// </summary>
        [Route("KurlariGuncelle")]
        public IActionResult KurlariGuncelle()
        {
            try
            {
                // Debug için view dosyasının varlığını kontrol et
                bool viewExistsInModulu = System.IO.File.Exists(Path.Combine(Directory.GetCurrentDirectory(), "Views", "ParaBirimiModulu", "KurlariGuncelle.cshtml"));
                bool viewExistsInParaBirimi = System.IO.File.Exists(Path.Combine(Directory.GetCurrentDirectory(), "Views", "ParaBirimi", "KurlariGuncelle.cshtml"));
                
                if (!viewExistsInModulu && !viewExistsInParaBirimi)
                {
                    _logger.LogWarning("KurlariGuncelle.cshtml view dosyası bulunamadı");
                }
                
                return View();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "KurlariGuncelle sayfası yüklenirken hata oluştu");
                TempData["ErrorMessage"] = $"Sayfayı yüklerken bir hata oluştu: {ex.Message}";
                return RedirectToAction(nameof(Kurlar));
            }
        }

        /// <summary>
        /// Kur değerlerini API'den güncelleme işlemi
        /// </summary>
        [HttpPost]
        [Route("KurlariGuncelle")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> KurlariGuncelle(object formData)
        {
            try
            {
                // Kur güncellemesi için servisi çağır
                var result = await _paraBirimiService.GuncelleKurDegerleriniFromAPIAsync();
                
                if (result)
                {
                    TempData["SuccessMessage"] = "Döviz kurları API'den başarıyla güncellendi.";
                }
                else
                {
                    TempData["ErrorMessage"] = "Döviz kurları güncellenirken bir hata oluştu.";
                }
            }
            catch (Exception ex)
            {
                await _logService.LogErrorAsync("Döviz kurları API'den güncellenirken", ex.Message);
                TempData["ErrorMessage"] = $"Döviz kurları güncellenirken bir hata oluştu: {ex.Message}";
            }
            
            return RedirectToAction(nameof(Kurlar));
        }
        #endregion

        #region Para Birimi İlişkileri
        /// <summary>
        /// Para birimi ilişkileri listesi
        /// </summary>
        [Route("Iliskiler")]
        public async Task<IActionResult> Iliskiler()
        {
            try
            {
                var iliskiler = await _paraBirimiService.GetAllParaBirimiIliskileriAsync();
                return View(iliskiler);
            }
            catch (Exception ex)
            {
                await _logService.LogErrorAsync("Para birimi ilişkileri listelenirken", ex.Message);
                TempData["Error"] = $"Para birimi ilişkileri listelenirken bir hata oluştu: {ex.Message}";
                return View(new List<ParaBirimiIliski>());
            }
        }

        /// <summary>
        /// Para birimi ilişkisi detayı
        /// </summary>
        [Route("IliskiDetay/{id:guid}")]
        public async Task<IActionResult> IliskiDetay(Guid id)
        {
            try
            {
                var iliski = await _paraBirimiService.GetParaBirimiIliskiByIdAsync(id);

                if (iliski == null)
                {
                    return NotFound();
                }

                return View(iliski);
            }
            catch (Exception ex)
            {
                await _logService.LogErrorAsync($"ID: {id} olan para birimi ilişkisi yükleme", ex.Message);
                TempData["Error"] = "Para birimi ilişkisi yüklenirken bir hata oluştu: " + ex.Message;
                return RedirectToAction(nameof(Iliskiler));
            }
        }

        /// <summary>
        /// Para birimi ilişkisi ekleme formu
        /// </summary>
        [Route("IliskiEkle")]
        public async Task<IActionResult> IliskiEkle()
        {
            // Para birimleri listesini getir
            var paraBirimleri = await _paraBirimiService.GetAllParaBirimleriAsync();

            // Dropdown için ViewBag'e ekle
            ViewBag.KaynakParaBirimleri = paraBirimleri.Select(p => new SelectListItem
            {
                Value = p.ParaBirimiID.ToString(),
                Text = $"{p.Kod} - {p.Ad}"
            }).ToList();

            ViewBag.HedefParaBirimleri = paraBirimleri.Select(p => new SelectListItem
            {
                Value = p.ParaBirimiID.ToString(),
                Text = $"{p.Kod} - {p.Ad}"
            }).ToList();

            return View(new ParaBirimiIliski
            {
                ParaBirimiIliskiID = Guid.NewGuid(),
                Aktif = true
            });
        }

        /// <summary>
        /// Para birimi ilişkisi ekleme işlemi
        /// </summary>
        [HttpPost]
        [Route("IliskiEkle")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> IliskiEkle(ParaBirimiIliski iliski)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    if (iliski.KaynakParaBirimiID == iliski.HedefParaBirimiID)
                    {
                        ModelState.AddModelError("", "Kaynak ve hedef para birimi aynı olamaz.");
                        
                        // Para birimleri listesini yeniden getir
                        var paraBirimleri = await _paraBirimiService.GetAllParaBirimleriAsync();
                        ViewBag.KaynakParaBirimleri = paraBirimleri.Select(p => new SelectListItem
                        {
                            Value = p.ParaBirimiID.ToString(),
                            Text = $"{p.Kod} - {p.Ad}"
                        }).ToList();
                        ViewBag.HedefParaBirimleri = paraBirimleri.Select(p => new SelectListItem
                        {
                            Value = p.ParaBirimiID.ToString(),
                            Text = $"{p.Kod} - {p.Ad}"
                        }).ToList();
                        
                        return View(iliski);
                    }

                    // Oluşturma bilgilerini ayarla
                    iliski.OlusturmaTarihi = DateTime.Now;
                    iliski.OlusturanKullaniciID = User.Identity?.Name ?? "Sistem";

                    // Servisi kullanarak ilişkiyi ekle
                    var result = await _paraBirimiService.AddParaBirimiIliskiAsync(iliski);

                    TempData["Success"] = "Para birimi ilişkisi başarıyla eklendi.";
                    return RedirectToAction(nameof(Iliskiler));
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError("", $"Bir hata oluştu: {ex.Message}");
                }
            }

            // Para birimleri listesini yeniden getir
            var paraBirimleriList = await _paraBirimiService.GetAllParaBirimleriAsync();
            ViewBag.KaynakParaBirimleri = paraBirimleriList.Select(p => new SelectListItem
            {
                Value = p.ParaBirimiID.ToString(),
                Text = $"{p.Kod} - {p.Ad}"
            }).ToList();
            ViewBag.HedefParaBirimleri = paraBirimleriList.Select(p => new SelectListItem
            {
                Value = p.ParaBirimiID.ToString(),
                Text = $"{p.Kod} - {p.Ad}"
            }).ToList();

            return View(iliski);
        }

        /// <summary>
        /// Para birimi ilişkisi düzenleme formu
        /// </summary>
        [Route("IliskiDuzenle/{id:guid}")]
        public async Task<IActionResult> IliskiDuzenle(Guid id)
        {
            try
            {
                var iliski = await _paraBirimiService.GetParaBirimiIliskiByIdAsync(id);

                if (iliski == null)
                {
                    return NotFound();
                }

                // Para birimleri listesini getir
                var paraBirimleri = await _paraBirimiService.GetAllParaBirimleriAsync();

                // Dropdown için ViewBag'e ekle
                ViewBag.KaynakParaBirimleri = paraBirimleri.Select(p => new SelectListItem
                {
                    Value = p.ParaBirimiID.ToString(),
                    Text = $"{p.Kod} - {p.Ad}"
                }).ToList();

                ViewBag.HedefParaBirimleri = paraBirimleri.Select(p => new SelectListItem
                {
                    Value = p.ParaBirimiID.ToString(),
                    Text = $"{p.Kod} - {p.Ad}"
                }).ToList();

                return View(iliski);
            }
            catch (Exception ex)
            {
                await _logService.LogErrorAsync($"ID: {id} olan para birimi ilişkisi yükleme", ex.Message);
                TempData["Error"] = "Para birimi ilişkisi yüklenirken bir hata oluştu: " + ex.Message;
                return RedirectToAction(nameof(Iliskiler));
            }
        }

        /// <summary>
        /// Para birimi ilişkisi düzenleme işlemi
        /// </summary>
        [HttpPost]
        [Route("IliskiDuzenle/{id:guid}")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> IliskiDuzenle(Guid id, ParaBirimiIliski iliski)
        {
            if (id != iliski.ParaBirimiIliskiID)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    if (iliski.KaynakParaBirimiID == iliski.HedefParaBirimiID)
                    {
                        ModelState.AddModelError("", "Kaynak ve hedef para birimi aynı olamaz.");
                        
                        // Para birimleri listesini yeniden getir
                        var paraBirimleri = await _paraBirimiService.GetAllParaBirimleriAsync();
                        ViewBag.KaynakParaBirimleri = paraBirimleri.Select(p => new SelectListItem
                        {
                            Value = p.ParaBirimiID.ToString(),
                            Text = $"{p.Kod} - {p.Ad}"
                        }).ToList();
                        ViewBag.HedefParaBirimleri = paraBirimleri.Select(p => new SelectListItem
                        {
                            Value = p.ParaBirimiID.ToString(),
                            Text = $"{p.Kod} - {p.Ad}"
                        }).ToList();
                        
                        return View(iliski);
                    }

                    // Güncelleme bilgilerini ayarla
                    iliski.GuncellemeTarihi = DateTime.Now;
                    iliski.SonGuncelleyenKullaniciID = User.Identity?.Name ?? "Sistem";

                    // Servisi kullanarak ilişkiyi güncelle
                    var result = await _paraBirimiService.UpdateParaBirimiIliskiAsync(iliski);

                    TempData["Success"] = "Para birimi ilişkisi başarıyla güncellendi.";
                    return RedirectToAction(nameof(Iliskiler));
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError("", $"Bir hata oluştu: {ex.Message}");
                }
            }

            // Para birimleri listesini yeniden getir
            var paraBirimleriList = await _paraBirimiService.GetAllParaBirimleriAsync();
            ViewBag.KaynakParaBirimleri = paraBirimleriList.Select(p => new SelectListItem
            {
                Value = p.ParaBirimiID.ToString(),
                Text = $"{p.Kod} - {p.Ad}"
            }).ToList();
            ViewBag.HedefParaBirimleri = paraBirimleriList.Select(p => new SelectListItem
            {
                Value = p.ParaBirimiID.ToString(),
                Text = $"{p.Kod} - {p.Ad}"
            }).ToList();

            return View(iliski);
        }

        /// <summary>
        /// Para birimi ilişkisi silme onayı
        /// </summary>
        [Route("IliskiSil/{id:guid}")]
        public async Task<IActionResult> IliskiSil(Guid id)
        {
            try
            {
                var iliski = await _paraBirimiService.GetParaBirimiIliskiByIdAsync(id);

                if (iliski == null)
                {
                    return NotFound();
                }

                return View(iliski);
            }
            catch (Exception ex)
            {
                await _logService.LogErrorAsync($"ID: {id} olan para birimi ilişkisi yükleme", ex.Message);
                TempData["Error"] = "Para birimi ilişkisi yüklenirken bir hata oluştu: " + ex.Message;
                return RedirectToAction(nameof(Iliskiler));
            }
        }

        /// <summary>
        /// Para birimi ilişkisi silme işlemi
        /// </summary>
        [HttpPost]
        [Route("IliskiSil/{id:guid}")]
        [ValidateAntiForgeryToken]
        [ActionName("IliskiSil")]
        public async Task<IActionResult> IliskiSilOnay(Guid id)
        {
            try
            {
                var result = await _paraBirimiService.DeleteParaBirimiIliskiAsync(id);

                if (result)
                {
                    TempData["Success"] = "Para birimi ilişkisi başarıyla silindi.";
                }
                else
                {
                    TempData["Error"] = "Para birimi ilişkisi silinirken bir sorun oluştu.";
                }

                return RedirectToAction(nameof(Iliskiler));
            }
            catch (Exception ex)
            {
                await _logService.LogErrorAsync($"ID: {id} olan para birimi ilişkisi silinirken", ex.Message);
                TempData["Error"] = "Para birimi ilişkisi silinirken bir hata oluştu: " + ex.Message;
                return RedirectToAction(nameof(Iliskiler));
            }
        }
        #endregion
    }
} 