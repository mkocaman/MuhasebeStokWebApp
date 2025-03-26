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
using MuhasebeStokWebApp.Data.Entities.DovizModulu;
using MuhasebeStokWebApp.Services.DovizModulu;
using MuhasebeStokWebApp.Services.Interfaces;
using MuhasebeStokWebApp.ViewModels.DovizModulu;
using System.Net.Http;
using System.Text.Json;
using Microsoft.AspNetCore.Identity;
using MuhasebeStokWebApp.Services;
using MuhasebeStokWebApp.Data.Entities;

namespace MuhasebeStokWebApp.Controllers.DovizModulu
{
    [Authorize]
    [Route("DovizKuru")]
    public class DovizKuruController : BaseController
    {
        private readonly ApplicationDbContext _context;
        private readonly IDovizKuruService _dovizKuruService;
        private readonly ILogger<DovizKuruController> _logger;
        private readonly IHttpClientFactory _httpClientFactory;
        private const string ApiKey = "645b5bebcab7cef56e1b609c";
        private const string ApiBaseUrl = "https://v6.exchangerate-api.com/v6/";

        public DovizKuruController(
            ApplicationDbContext context,
            IDovizKuruService dovizKuruService,
            ILogger<DovizKuruController> logger,
            IHttpClientFactory httpClientFactory,
            IMenuService menuService,
            UserManager<ApplicationUser> userManager,
            RoleManager<IdentityRole> roleManager,
            ILogService logService)
            : base(menuService, userManager, roleManager, logService)
        {
            _context = context;
            _dovizKuruService = dovizKuruService;
            _logger = logger;
            _httpClientFactory = httpClientFactory;
        }

        // GET: DovizKuru/Liste
        [Route("Liste")]
        public async Task<IActionResult> Liste()
        {
            try
            {
                var kurDegerleri = await _context.KurDegerleri
                    .Include(d => d.ParaBirimi)
                    .Where(d => !d.Silindi && d.Aktif)
                    .OrderByDescending(d => d.Tarih)
                    .ToListAsync();

                return View(kurDegerleri);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Döviz kurları listelenirken hata oluştu");
                TempData["ErrorMessage"] = $"Döviz kurları listelenemedi: {ex.Message}";
                return View(new List<KurDegeri>());
            }
        }

        // GET: DovizKuru/Detay/5
        [Route("Detay/{id:guid?}")]
        public async Task<IActionResult> Detay(Guid? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var kurDegeri = await _context.KurDegerleri
                .Include(d => d.ParaBirimi)
                .FirstOrDefaultAsync(m => m.KurDegeriID == id && !m.Silindi);

            if (kurDegeri == null)
            {
                return NotFound();
            }

            return View(kurDegeri);
        }

        // GET: DovizKuru/Ekle
        [Route("Ekle")]
        public async Task<IActionResult> Ekle()
        {
            // Para birimleri listesini getir
            var paraBirimleri = await _context.ParaBirimleri
                .Where(d => d.Aktif && !d.Silindi)
                .OrderBy(d => d.Kod)
                .ToListAsync();

            // Dropdown için ViewBag'e ekle
            ViewBag.ParaBirimleri = paraBirimleri.Select(p => new SelectListItem
            {
                Value = p.ParaBirimiID.ToString(),
                Text = $"{p.Kod} - {p.Ad}"
            }).ToList();

            return View(new DovizCreateViewModel
            {
                Tarih = DateTime.Today,
                ParaBirimleri = paraBirimleri
            });
        }

        // POST: DovizKuru/Ekle
        [HttpPost]
        [Route("Ekle")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Ekle(DovizCreateViewModel model)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    // Yeni GUID oluştur
                    var kurDegeriId = Guid.NewGuid();

                    // Para birimini getir
                    var paraBirimi = await _context.ParaBirimleri.FindAsync(model.ParaBirimiID);

                    if (paraBirimi == null)
                    {
                        ModelState.AddModelError("", "Seçilen para birimi bulunamadı.");
                        return View(model);
                    }

                    // KurDegeri nesnesini oluştur
                    var kurDegeri = new KurDegeri
                    {
                        KurDegeriID = kurDegeriId,
                        ParaBirimiID = model.ParaBirimiID,
                        ParaBirimi = paraBirimi,
                        Alis = model.Alis,
                        Satis = model.Satis,
                        Efektif_Alis = model.Efektif_Alis ?? model.Alis,
                        Efektif_Satis = model.Efektif_Satis ?? model.Satis,
                        Tarih = model.Tarih,
                        Aktif = true,
                        Silindi = false,
                        OlusturmaTarihi = DateTime.Now,
                        OlusturanKullaniciID = User.Identity?.Name ?? "Sistem"
                    };

                    // Veritabanına ekle
                    _context.Add(kurDegeri);
                    await _context.SaveChangesAsync();

                    TempData["SuccessMessage"] = "Döviz kuru başarıyla eklendi.";
                    return RedirectToAction(nameof(Liste));
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError("", $"Bir hata oluştu: {ex.Message}");
                }
            }

            // Para birimleri listesini yeniden getir
            var paraBirimleri = await _context.ParaBirimleri
                .Where(d => d.Aktif && !d.Silindi)
                .OrderBy(d => d.Kod)
                .ToListAsync();

            ViewBag.ParaBirimleri = paraBirimleri.Select(p => new SelectListItem
            {
                Value = p.ParaBirimiID.ToString(),
                Text = $"{p.Kod} - {p.Ad}"
            }).ToList();
            
            model.ParaBirimleri = paraBirimleri;

            return View(model);
        }

        // GET: DovizKuru/Duzenle/5
        [Route("Duzenle/{id:guid?}")]
        public async Task<IActionResult> Duzenle(Guid? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var kurDegeri = await _context.KurDegerleri
                .Include(d => d.ParaBirimi)
                .FirstOrDefaultAsync(m => m.KurDegeriID == id && !m.Silindi);

            if (kurDegeri == null)
            {
                return NotFound();
            }

            // Para birimleri listesini getir
            var paraBirimleri = await _context.ParaBirimleri
                .Where(d => d.Aktif && !d.Silindi)
                .OrderBy(d => d.Kod)
                .ToListAsync();

            // Dropdown için ViewBag'e ekle
            ViewBag.ParaBirimleri = paraBirimleri.Select(p => new SelectListItem
            {
                Value = p.ParaBirimiID.ToString(),
                Text = $"{p.Kod} - {p.Ad}"
            }).ToList();

            // ViewModel'e dönüştür
            var model = new DovizEditViewModel
            {
                KurDegeriID = kurDegeri.KurDegeriID,
                ParaBirimiID = kurDegeri.ParaBirimiID,
                Alis = kurDegeri.Alis,
                Satis = kurDegeri.Satis,
                Efektif_Alis = kurDegeri.Efektif_Alis,
                Efektif_Satis = kurDegeri.Efektif_Satis,
                Tarih = kurDegeri.Tarih,
                Aktif = kurDegeri.Aktif,
                Aciklama = "",
                ParaBirimleri = paraBirimleri,
                ParaBirimiAdi = kurDegeri.ParaBirimi?.Ad ?? "",
                ParaBirimiKodu = kurDegeri.ParaBirimi?.Kod ?? ""
            };

            return View(model);
        }

        // POST: DovizKuru/Duzenle/5
        [HttpPost]
        [Route("Duzenle/{id:guid}")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Duzenle(Guid id, DovizEditViewModel model)
        {
            if (id != model.KurDegeriID)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    var kurDegeri = await _context.KurDegerleri.FindAsync(id);
                    
                    if (kurDegeri == null || kurDegeri.Silindi)
                    {
                        return NotFound();
                    }

                    // Para birimini getir
                    var paraBirimi = await _context.ParaBirimleri.FindAsync(model.ParaBirimiID);

                    if (paraBirimi == null)
                    {
                        ModelState.AddModelError("", "Seçilen para birimi bulunamadı.");
                        return View(model);
                    }

                    // KurDegeri güncelle
                    kurDegeri.ParaBirimiID = model.ParaBirimiID;
                    kurDegeri.Alis = model.Alis;
                    kurDegeri.Satis = model.Satis;
                    kurDegeri.Efektif_Alis = model.Efektif_Alis ?? model.Alis;
                    kurDegeri.Efektif_Satis = model.Efektif_Satis ?? model.Satis;
                    kurDegeri.Tarih = model.Tarih;
                    kurDegeri.Aktif = model.Aktif;
                    kurDegeri.GuncellemeTarihi = DateTime.Now;
                    kurDegeri.SonGuncelleyenKullaniciID = User.Identity?.Name ?? "Sistem";

                    _context.Update(kurDegeri);
                    await _context.SaveChangesAsync();

                    TempData["SuccessMessage"] = "Döviz kuru başarıyla güncellendi.";
                    return RedirectToAction(nameof(Liste));
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!KurDegeriExists(id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
            }

            // Para birimleri listesini yeniden getir
            var paraBirimleri = await _context.ParaBirimleri
                .Where(d => d.Aktif && !d.Silindi)
                .OrderBy(d => d.Kod)
                .ToListAsync();

            ViewBag.ParaBirimleri = paraBirimleri.Select(p => new SelectListItem
            {
                Value = p.ParaBirimiID.ToString(),
                Text = $"{p.Kod} - {p.Ad}"
            }).ToList();
            
            model.ParaBirimleri = paraBirimleri;

            return View(model);
        }

        // GET: DovizKuru/Sil/5
        [Route("Sil/{id:guid?}")]
        public async Task<IActionResult> Sil(Guid? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var kurDegeri = await _context.KurDegerleri
                .Include(d => d.ParaBirimi)
                .FirstOrDefaultAsync(m => m.KurDegeriID == id && !m.Silindi);

            if (kurDegeri == null)
            {
                return NotFound();
            }

            return View(kurDegeri);
        }

        // POST: DovizKuru/Sil/5
        [HttpPost]
        [Route("Sil/{id:guid}")]
        [ValidateAntiForgeryToken]
        [ActionName("Sil")]
        public async Task<IActionResult> SilOnay(Guid id)
        {
            var kurDegeri = await _context.KurDegerleri.FindAsync(id);
            if (kurDegeri == null || kurDegeri.Silindi)
            {
                return NotFound();
            }

            // Soft delete
            kurDegeri.Silindi = true;
            kurDegeri.Aktif = false;
            kurDegeri.GuncellemeTarihi = DateTime.Now;
            kurDegeri.SonGuncelleyenKullaniciID = User.Identity?.Name ?? "Sistem";

            _context.Update(kurDegeri);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Döviz kuru başarıyla silindi.";
            return RedirectToAction(nameof(Liste));
        }
        
        // GET: DovizKuru/KurlariGuncelle
        [Route("KurlariGuncelle")]
        public IActionResult KurlariGuncelle()
        {
            return View();
        }
        
        // POST: DovizKuru/KurlariGuncelle
        [HttpPost]
        [Route("KurlariGuncelle")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> KurlariGuncellePost()
        {
            try
            {
                // Kur güncellemesi için API'yi çağır
                var result = await GetExchangeRatesFromApi();
                
                if (result != null)
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
                _logger.LogError(ex, "Döviz kurları API'den güncellenirken hata oluştu");
                TempData["ErrorMessage"] = $"Döviz kurları güncellenirken bir hata oluştu: {ex.Message}";
            }
            
            return RedirectToAction(nameof(Liste));
        }
        
        private async Task<bool> GetExchangeRatesFromApi()
        {
            try
            {
                // Temel para birimi TRY için API isteği gönder
                var client = _httpClientFactory.CreateClient();
                var response = await client.GetAsync($"{ApiBaseUrl}{ApiKey}/latest/TRY");
                
                if (response.IsSuccessStatusCode)
                {
                    var jsonResponse = await response.Content.ReadAsStringAsync();
                    var exchangeRateData = JsonSerializer.Deserialize<ExchangeRateApiResponse>(jsonResponse);
                    
                    if (exchangeRateData != null && exchangeRateData.result == "success")
                    {
                        // Para birimlerini veritabanından al
                        var paraBirimleri = await _context.ParaBirimleri
                            .Where(p => p.Aktif && !p.Silindi)
                            .ToListAsync();
                        
                        foreach (var paraBirimi in paraBirimleri)
                        {
                            // API'den gelen kur bilgisini kontrol et
                            if (exchangeRateData.conversion_rates.TryGetValue(paraBirimi.Kod, out var rate))
                            {
                                // Kurları hesapla (TRY baz alındığı için ters çevirme işlemi yapılmalı)
                                decimal baseRate = 1m / (decimal)rate;
                                
                                // Yeni kur kaydı oluştur
                                var kurDegeri = new KurDegeri
                                {
                                    KurDegeriID = Guid.NewGuid(),
                                    ParaBirimiID = paraBirimi.ParaBirimiID,
                                    Tarih = DateTime.Today,
                                    Alis = Math.Round(baseRate, 4),
                                    Satis = Math.Round(baseRate * 1.01m, 4), // %1 satış farkı
                                    Efektif_Alis = Math.Round(baseRate * 0.99m, 4), // %1 efektif alış farkı
                                    Efektif_Satis = Math.Round(baseRate * 1.02m, 4), // %2 efektif satış farkı
                                    Aktif = true,
                                    Silindi = false,
                                    OlusturmaTarihi = DateTime.Now,
                                    OlusturanKullaniciID = User.Identity?.Name ?? "Sistem"
                                };
                                
                                // Aynı güne ait mevcut kur var mı kontrol et
                                var existingRate = await _context.KurDegerleri
                                    .FirstOrDefaultAsync(k => k.ParaBirimiID == paraBirimi.ParaBirimiID && 
                                                            k.Tarih.Date == DateTime.Today.Date &&
                                                            !k.Silindi);
                                
                                if (existingRate != null)
                                {
                                    // Mevcut kaydı güncelle
                                    existingRate.Alis = kurDegeri.Alis;
                                    existingRate.Satis = kurDegeri.Satis;
                                    existingRate.Efektif_Alis = kurDegeri.Efektif_Alis;
                                    existingRate.Efektif_Satis = kurDegeri.Efektif_Satis;
                                    existingRate.GuncellemeTarihi = DateTime.Now;
                                    existingRate.SonGuncelleyenKullaniciID = User.Identity?.Name ?? "Sistem";
                                    
                                    _context.Update(existingRate);
                                }
                                else
                                {
                                    // Yeni kayıt ekle
                                    _context.Add(kurDegeri);
                                }
                            }
                        }
                        
                        await _context.SaveChangesAsync();
                        return true;
                    }
                }
                
                return false;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "API'den döviz kurları alınırken hata oluştu");
                throw;
            }
        }

        private bool KurDegeriExists(Guid id)
        {
            return _context.KurDegerleri.Any(e => e.KurDegeriID == id && !e.Silindi);
        }
    }

    public class ExchangeRateApiResponse
    {
        public string result { get; set; }
        public string documentation { get; set; }
        public string terms_of_use { get; set; }
        public long time_last_update_unix { get; set; }
        public string time_last_update_utc { get; set; }
        public long time_next_update_unix { get; set; }
        public string time_next_update_utc { get; set; }
        public string base_code { get; set; }
        public Dictionary<string, double> conversion_rates { get; set; }
    }
} 