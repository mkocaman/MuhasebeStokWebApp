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
using MuhasebeStokWebApp.Services;
using MuhasebeStokWebApp.ViewModels.Kur;

namespace MuhasebeStokWebApp.Controllers
{
    // [Authorize]
    public class KurController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IKurService _kurService;
        private readonly ILogService _logService;
        private readonly ILogger<KurController> _logger;

        public KurController(ApplicationDbContext context, IKurService kurService, ILogService logService, ILogger<KurController> logger)
        {
            _context = context;
            _kurService = kurService;
            _logService = logService;
            _logger = logger;
        }

        // GET: Kur
        public async Task<IActionResult> Index()
        {
            try
            {
                var kurDegerleri = await _kurService.GetKurDegerleriAsync();
                var kurAyarlari = await _kurService.GetKurAyarlariAsync();
                
                var viewModel = new KurIndexViewModel
                {
                    KurDegerleri = kurDegerleri.Select(KurDegeriViewModel.FromEntity).ToList(),
                    KurAyarlari = kurAyarlari != null ? KurAyarlariViewModel.FromEntity(kurAyarlari) : new KurAyarlariViewModel
                    {
                        BazParaBirimiKodu = "USD",
                        IkinciParaBirimiKodu = "UZS",
                        UcuncuParaBirimiKodu = "TRY",
                        GuncellemeSikligi = 24,
                        OtomatikGuncelleme = true,
                        SonGuncellemeTarihi = DateTime.Now
                    },
                    BazParaBirimiKodu = kurAyarlari?.AnaDovizKodu ?? "USD",
                    IkinciParaBirimiKodu = kurAyarlari?.IkinciDovizKodu ?? "UZS",
                    UcuncuParaBirimiKodu = kurAyarlari?.UcuncuDovizKodu ?? "TRY",
                    SonGuncellemeTarihi = kurAyarlari?.SonDovizGuncellemeTarihi ?? DateTime.Now,
                    OtomatikGuncelleme = kurAyarlari?.OtomatikDovizGuncelleme ?? true,
                    GuncellemeSikligi = kurAyarlari?.DovizGuncellemeSikligi ?? 24
                };
                
                return View(viewModel);
            }
            catch (Exception ex)
            {
                await _logService.LogEkleAsync($"Kur listesi görüntülenirken hata oluştu: {ex.Message}", Models.LogTuru.Hata);
                TempData["ErrorMessage"] = "Kur listesi görüntülenirken bir hata oluştu.";
                return View(new KurIndexViewModel());
            }
        }

        // GET: Kur/ParaBirimleri
        public async Task<IActionResult> ParaBirimleri()
        {
            try
            {
                var paraBirimleri = await _kurService.GetParaBirimleriAsync();
                var viewModels = paraBirimleri.Select(ParaBirimiViewModel.FromEntity).ToList();
                return View(viewModels);
            }
            catch (Exception ex)
            {
                await _logService.LogEkleAsync($"Para birimleri görüntülenirken hata oluştu: {ex.Message}", Models.LogTuru.Hata);
                TempData["ErrorMessage"] = "Para birimleri görüntülenirken bir hata oluştu.";
                return View(new List<ParaBirimiViewModel>());
            }
        }

        // GET: Kur/ParaBirimiEkle
        public IActionResult ParaBirimiEkle()
        {
            var viewModel = new ParaBirimiViewModel
            {
                Kod = "",
                Ad = "",
                Sembol = "",
                Aktif = true
            };
            return View(viewModel);
        }

        // POST: Kur/ParaBirimiEkle
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ParaBirimiEkle(ParaBirimiViewModel viewModel)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return View(viewModel);
                }
                
                // Para birimi kodu zaten var mı kontrol et
                var existing = await _context.ParaBirimleri
                    .AsNoTracking()
                    .FirstOrDefaultAsync(p => p.Kod.Equals(viewModel.Kod));
                    
                if (existing != null)
                {
                    ModelState.AddModelError("Kod", $"{viewModel.Kod} kodlu para birimi zaten mevcut.");
                    return View(viewModel);
                }
                
                var entity = viewModel.ToEntity();
                entity.ParaBirimiID = Guid.NewGuid(); // ID'yi açıkça belirtiyoruz
                
                _logger.LogInformation($"Para birimi ekleniyor: {entity.Kod} ({entity.ParaBirimiID})");
                
                try
                {
                    // Önce DbContext'e entity'i ekle
                    await _context.ParaBirimleri.AddAsync(entity);
                    
                    // Kaydetme öncesi log ekle
                    // Bu, veritabanı hatası durumunda bu komut çalışmayacak
                    
                    // Hemen kaydet
                    await _context.SaveChangesAsync();
                    
                    // Başarılı işlemden sonra log kaydet
                    await _logService.LogEkleAsync($"{viewModel.Kod} para birimi eklendi.", Models.LogTuru.Bilgi, viewModel.Kod);
                    
                    TempData["SuccessMessage"] = "Para birimi başarıyla eklendi.";
                    return RedirectToAction(nameof(ParaBirimleri));
                }
                catch (DbUpdateException dbEx)
                {
                    _logger.LogError(dbEx, "Para birimi eklenirken veritabanı hatası oluştu.");
                    var errorMessage = "Veritabanı hatası: ";
                    
                    if (dbEx.InnerException != null)
                    {
                        errorMessage += dbEx.InnerException.Message;
                        
                        // Veri tipi uyumsuzluğu hataları
                        if (dbEx.InnerException.Message.Contains("Operand type clash"))
                        {
                            errorMessage = "Veritabanında veri tipi uyumsuzluğu tespit edildi. Sistem yöneticisi ile iletişime geçin.";
                            _logger.LogError("Veri tipi uyumsuzluğu: {0}", dbEx.InnerException.Message);
                        }
                        // Null değer hataları
                        else if (dbEx.InnerException.Message.Contains("NULL into column"))
                        {
                            errorMessage = "Boş bırakılamayacak bir alan boş bırakıldı.";
                            _logger.LogError("NULL hatası: {0}", dbEx.InnerException.Message);
                        }
                        // Duplicate key hatası
                        else if (dbEx.InnerException.Message.Contains("duplicate key"))
                        {
                            errorMessage = $"{viewModel.Kod} kodlu para birimi zaten mevcut.";
                        }
                    }
                    
                    ModelState.AddModelError("", $"Para birimi eklenirken bir hata oluştu: {errorMessage}");
                    return View(viewModel);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Para birimi ekleme sırasında beklenmedik hata oluştu.");
                ModelState.AddModelError("", $"Para birimi eklenirken bir hata oluştu: {ex.Message}");
                return View(viewModel);
            }
        }

        // GET: Kur/ParaBirimiDuzenle/5
        public async Task<IActionResult> ParaBirimiDuzenle(Guid id)
        {
            var paraBirimi = await _kurService.GetParaBirimiByIdAsync(id);
            if (paraBirimi == null)
            {
                TempData["ErrorMessage"] = "Para birimi bulunamadı.";
                return RedirectToAction(nameof(ParaBirimleri));
            }
            
            var viewModel = ParaBirimiViewModel.FromEntity(paraBirimi);
            return View(viewModel);
        }

        // POST: Kur/ParaBirimiDuzenle/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ParaBirimiDuzenle(Guid id, ParaBirimiViewModel viewModel)
        {
            if (id != viewModel.ParaBirimiID)
            {
                TempData["ErrorMessage"] = "Geçersiz para birimi ID.";
                return RedirectToAction(nameof(ParaBirimleri));
            }
            
            if (ModelState.IsValid)
            {
                try
                {
                    var entity = viewModel.ToEntity();
                    await _kurService.UpdateParaBirimiAsync(entity);
                    await _logService.LogEkleAsync($"{viewModel.Kod} para birimi güncellendi.", Models.LogTuru.Bilgi);
                    TempData["SuccessMessage"] = "Para birimi başarıyla güncellendi.";
                    return RedirectToAction(nameof(ParaBirimleri));
                }
                catch (Exception ex)
                {
                    await _logService.LogEkleAsync($"Para birimi güncellenirken hata oluştu: {ex.Message}", Models.LogTuru.Hata);
                    ModelState.AddModelError("", $"Para birimi güncellenirken bir hata oluştu: {ex.Message}");
                }
            }
            
            return View(viewModel);
        }

        // GET: Kur/ParaBirimiSil/5
        public async Task<IActionResult> ParaBirimiSil(Guid id)
        {
            var paraBirimi = await _kurService.GetParaBirimiByIdAsync(id);
            if (paraBirimi == null)
            {
                TempData["ErrorMessage"] = "Para birimi bulunamadı.";
                return RedirectToAction(nameof(ParaBirimleri));
            }
            
            var viewModel = ParaBirimiViewModel.FromEntity(paraBirimi);
            return View(viewModel);
        }

        // POST: Kur/ParaBirimiSil/5
        [HttpPost, ActionName("ParaBirimiSil")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ParaBirimiSilOnay(Guid id)
        {
            try
            {
                await _kurService.DeleteParaBirimiAsync(id);
                await _logService.LogEkleAsync($"Para birimi silindi (ID: {id}).", Models.LogTuru.Bilgi);
                TempData["SuccessMessage"] = "Para birimi başarıyla silindi.";
            }
            catch (Exception ex)
            {
                await _logService.LogEkleAsync($"Para birimi silinirken hata oluştu: {ex.Message}", Models.LogTuru.Hata);
                TempData["ErrorMessage"] = $"Para birimi silinirken bir hata oluştu: {ex.Message}";
            }
            
            return RedirectToAction(nameof(ParaBirimleri));
        }

        // GET: Kur/KurDegeriEkle
        public async Task<IActionResult> KurDegeriEkle()
        {
            try
            {
                var paraBirimleri = await GetParaBirimiSelectListAsync();
                
                if (paraBirimleri.Count == 0)
                {
                    TempData["WarningMessage"] = "Para birimleri listesi boş. Lütfen önce para birimi ekleyin.";
                    return RedirectToAction(nameof(ParaBirimleri));
                }
                
                var viewModel = new KurDegeriViewModel
                {
                    AlisDegeri = 1.0m,
                    SatisDegeri = 1.0m,
                    Tarih = DateTime.Now,
                    Kaynak = "Manuel",
                    Aktif = true,
                    ParaBirimleri = paraBirimleri
                };
                
                return View(viewModel);
            }
            catch (Exception ex)
            {
                await _logService.LogEkleAsync($"Kur değeri ekleme sayfası görüntülenirken hata oluştu: {ex.Message}", Models.LogTuru.Hata);
                TempData["ErrorMessage"] = "Kur değeri ekleme sayfası görüntülenirken bir hata oluştu.";
                return RedirectToAction(nameof(Index));
            }
        }

        // POST: Kur/KurDegeriEkle
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> KurDegeriEkle(KurDegeriViewModel viewModel)
        {
            viewModel.ParaBirimleri = await GetParaBirimiSelectListAsync();
            
            try
            {
                if (!ModelState.IsValid)
                {
                    return View(viewModel);
                }
                
                // Seçili para birimi kontrolü
                var paraBirimi = await _context.ParaBirimleri.FindAsync(viewModel.ParaBirimiID);
                
                if (paraBirimi == null)
                {
                    ModelState.AddModelError("ParaBirimiID", "Seçilen para birimi bulunamadı.");
                    return View(viewModel);
                }
                
                var entity = viewModel.ToEntity();
                await _kurService.AddKurDegeriAsync(entity);
                
                await _logService.LogEkleAsync(
                    $"{paraBirimi.Kod} için yeni kur değeri eklendi: Alış={viewModel.AlisDegeri}, Satış={viewModel.SatisDegeri}",
                    Models.LogTuru.Bilgi);
                
                TempData["SuccessMessage"] = "Kur değeri başarıyla eklendi.";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                await _logService.LogEkleAsync($"Kur değeri eklenirken hata oluştu: {ex.Message}", Models.LogTuru.Hata);
                ModelState.AddModelError("", $"Kur değeri eklenirken bir hata oluştu: {ex.Message}");
                return View(viewModel);
            }
        }

        // GET: Kur/KurDegeriDuzenle/5
        public async Task<IActionResult> KurDegeriDuzenle(Guid id)
        {
            var kurDegeri = await _context.KurDegerleri
                .Include(k => k.ParaBirimi)
                .FirstOrDefaultAsync(k => k.KurDegeriID == id);
                
            if (kurDegeri == null)
            {
                TempData["ErrorMessage"] = "Kur değeri bulunamadı.";
                return RedirectToAction(nameof(Index));
            }
            
            var viewModel = KurDegeriViewModel.FromEntity(kurDegeri);
            viewModel.ParaBirimleri = await GetParaBirimiSelectListAsync();
            
            return View(viewModel);
        }

        // POST: Kur/KurDegeriDuzenle/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> KurDegeriDuzenle(Guid id, KurDegeriViewModel viewModel)
        {
            if (id != viewModel.KurDegeriID)
            {
                TempData["ErrorMessage"] = "Geçersiz kur değeri ID.";
                return RedirectToAction(nameof(Index));
            }
            
            if (ModelState.IsValid)
            {
                try
                {
                    var entity = viewModel.ToEntity();
                    await _kurService.UpdateKurDegeriAsync(entity);
                    await _logService.LogEkleAsync($"Kur değeri güncellendi.", Models.LogTuru.Bilgi);
                    TempData["SuccessMessage"] = "Kur değeri başarıyla güncellendi.";
                    return RedirectToAction(nameof(Index));
                }
                catch (Exception ex)
                {
                    await _logService.LogEkleAsync($"Kur değeri güncellenirken hata oluştu: {ex.Message}", Models.LogTuru.Hata);
                    ModelState.AddModelError("", $"Kur değeri güncellenirken bir hata oluştu: {ex.Message}");
                }
            }
            
            viewModel.ParaBirimleri = await GetParaBirimiSelectListAsync();
            return View(viewModel);
        }

        // GET: Kur/KurDegeriSil/5
        public async Task<IActionResult> KurDegeriSil(Guid id)
        {
            var kurDegeri = await _context.KurDegerleri
                .Include(k => k.ParaBirimi)
                .FirstOrDefaultAsync(k => k.KurDegeriID == id);
                
            if (kurDegeri == null)
            {
                TempData["ErrorMessage"] = "Kur değeri bulunamadı.";
                return RedirectToAction(nameof(Index));
            }
            
            var viewModel = KurDegeriViewModel.FromEntity(kurDegeri);
            return View(viewModel);
        }

        // POST: Kur/KurDegeriSil/5
        [HttpPost, ActionName("KurDegeriSil")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> KurDegeriSilOnay(Guid id)
        {
            try
            {
                await _kurService.DeleteKurDegeriAsync(id);
                await _logService.LogEkleAsync($"Kur değeri silindi (ID: {id}).", Models.LogTuru.Bilgi);
                TempData["SuccessMessage"] = "Kur değeri başarıyla silindi.";
            }
            catch (Exception ex)
            {
                await _logService.LogEkleAsync($"Kur değeri silinirken hata oluştu: {ex.Message}", Models.LogTuru.Hata);
                TempData["ErrorMessage"] = $"Kur değeri silinirken bir hata oluştu: {ex.Message}";
            }
            
            return RedirectToAction(nameof(Index));
        }

        // GET: Kur/Ayarlar
        public async Task<IActionResult> Ayarlar()
        {
            try
            {
                var sistemAyarlari = await _kurService.GetKurAyarlariAsync();
                var viewModel = sistemAyarlari != null 
                    ? KurAyarlariViewModel.FromEntity(sistemAyarlari) 
                    : new KurAyarlariViewModel
                    {
                        BazParaBirimiKodu = "USD",
                        IkinciParaBirimiKodu = "UZS",
                        UcuncuParaBirimiKodu = "TRY",
                        OtomatikGuncelleme = true,
                        GuncellemeSikligi = 24,
                        SonGuncellemeTarihi = DateTime.Now,
                        ParaBirimleri = await GetParaBirimiSelectListAsync()
                    };
                
                return View(viewModel);
            }
            catch (Exception ex)
            {
                await _logService.LogEkleAsync($"Kur ayarları görüntülenirken hata oluştu: {ex.Message}", Models.LogTuru.Hata);
                TempData["ErrorMessage"] = "Kur ayarları görüntülenirken bir hata oluştu.";
                
                var viewModel = new KurAyarlariViewModel
                {
                    BazParaBirimiKodu = "USD",
                    IkinciParaBirimiKodu = "UZS",
                    UcuncuParaBirimiKodu = "TRY",
                    OtomatikGuncelleme = true,
                    GuncellemeSikligi = 24,
                    SonGuncellemeTarihi = DateTime.Now,
                    ParaBirimleri = await GetParaBirimiSelectListAsync()
                };
                
                return View(viewModel);
            }
        }

        // POST: Kur/Ayarlar
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Ayarlar(KurAyarlariViewModel viewModel)
        {
            try
            {
                viewModel.ParaBirimleri = await GetParaBirimiSelectListAsync();
                
                if (!ModelState.IsValid)
                {
                    return View(viewModel);
                }
                
                // Para birimleri gerçekten var mı kontrol et
                var bazParaBirimi = await _kurService.GetParaBirimiByKodAsync(viewModel.BazParaBirimiKodu);
                var ikinciParaBirimi = string.IsNullOrEmpty(viewModel.IkinciParaBirimiKodu) ? null 
                    : await _kurService.GetParaBirimiByKodAsync(viewModel.IkinciParaBirimiKodu);
                var ucuncuParaBirimi = string.IsNullOrEmpty(viewModel.UcuncuParaBirimiKodu) ? null 
                    : await _kurService.GetParaBirimiByKodAsync(viewModel.UcuncuParaBirimiKodu);
                
                if (bazParaBirimi == null)
                {
                    ModelState.AddModelError("BazParaBirimiKodu", "Seçilen baz para birimi bulunamadı.");
                    return View(viewModel);
                }
                
                var sistemAyarlari = await _kurService.GetKurAyarlariAsync();
                
                if (sistemAyarlari == null)
                {
                    sistemAyarlari = new SistemAyarlari
                    {
                        AnaDovizKodu = viewModel.BazParaBirimiKodu,
                        IkinciDovizKodu = viewModel.IkinciParaBirimiKodu,
                        UcuncuDovizKodu = viewModel.UcuncuParaBirimiKodu,
                        SirketAdi = "Şirket",
                        OtomatikDovizGuncelleme = viewModel.OtomatikGuncelleme,
                        DovizGuncellemeSikligi = viewModel.GuncellemeSikligi,
                        SonDovizGuncellemeTarihi = DateTime.Now,
                        OlusturmaTarihi = DateTime.Now,
                        GuncellemeTarihi = DateTime.Now
                    };
                }
                else
                {
                    // ViewModel'den entity'e güncelleme
                    viewModel.UpdateEntity(sistemAyarlari);
                    // Güncelleme tarihi
                    sistemAyarlari.GuncellemeTarihi = DateTime.Now;
                }
                
                // Güncelleme işlemi
                await _kurService.UpdateKurAyarlariAsync(sistemAyarlari);
                
                await _logService.LogEkleAsync("Kur ayarları güncellendi.", Models.LogTuru.Bilgi);
                TempData["SuccessMessage"] = "Kur ayarları başarıyla güncellendi.";
                
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                await _logService.LogEkleAsync($"Kur ayarları güncellenirken hata oluştu: {ex.Message}", Models.LogTuru.Hata);
                TempData["ErrorMessage"] = "Kur ayarları güncellenirken bir hata oluştu.";
                return View(viewModel);
            }
        }

        // GET: Kur/KurlariGuncelle
        public async Task<IActionResult> KurlariGuncelle()
        {
            return View();
        }

        // POST: Kur/KurlariGuncelle
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> KurlariGuncelle(string source)
        {
            try
            {
                bool success = false;
                
                if (source == "tcmb")
                {
                    success = await _kurService.UpdateKurlarFromMerkezBankasiAsync();
                    await _logService.LogEkleAsync("TCMB'den kurlar güncellendi.", Models.LogTuru.Bilgi);
                }
                else if (source == "uzbekistan")
                {
                    success = await _kurService.UpdateKurlarFromUzbekistanMBAsync();
                    await _logService.LogEkleAsync("Özbekistan MB'den kurlar güncellendi.", Models.LogTuru.Bilgi);
                }
                else
                {
                    success = await _kurService.UpdateKurlarFromMerkezBankasiAsync() && 
                              await _kurService.UpdateKurlarFromUzbekistanMBAsync();
                    await _logService.LogEkleAsync("Tüm kurlar güncellendi.", Models.LogTuru.Bilgi);
                }
                
                if (success)
                {
                    TempData["SuccessMessage"] = "Kurlar başarıyla güncellendi.";
                }
                else
                {
                    TempData["WarningMessage"] = "Kurlar güncellenirken bazı sorunlar oluştu.";
                }
            }
            catch (Exception ex)
            {
                await _logService.LogEkleAsync($"Kurlar güncellenirken hata oluştu: {ex.Message}", Models.LogTuru.Hata);
                TempData["ErrorMessage"] = $"Kurlar güncellenirken bir hata oluştu: {ex.Message}";
            }
            
            return RedirectToAction(nameof(Index));
        }

        // GET: Kur/Cevirici
        public async Task<IActionResult> Cevirici()
        {
            ViewBag.ParaBirimleri = await GetParaBirimiSelectListAsync();
            
            var viewModel = new KurCeviriciViewModel
            {
                KaynakParaBirimiKod = "USD",
                HedefParaBirimiKod = "TRY",
                Miktar = 1
            };
            
            return View(viewModel);
        }

        // POST: Kur/Cevirici
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Cevirici(KurCeviriciViewModel viewModel)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    viewModel.Sonuc = await _kurService.ConvertParaBirimiAsync(
                        viewModel.KaynakParaBirimiKod,
                        viewModel.HedefParaBirimiKod,
                        viewModel.Miktar);
                        
                    var kurDegeri = await _kurService.GetKurDegeriAsync(
                        viewModel.KaynakParaBirimiKod,
                        viewModel.HedefParaBirimiKod);
                        
                    viewModel.KullanilanKur = kurDegeri?.AlisDegeri ?? 0;
                    viewModel.SonucGoster = true;
                }
                catch (Exception ex)
                {
                    await _logService.LogEkleAsync($"Para birimi çevirme işlemi sırasında hata oluştu: {ex.Message}", Models.LogTuru.Hata);
                    ModelState.AddModelError("", $"Para birimi çevirme işlemi sırasında bir hata oluştu: {ex.Message}");
                }
            }
            
            ViewBag.ParaBirimleri = await GetParaBirimiSelectListAsync();
            return View(viewModel);
        }

        // GET: Kur/ParaBirimIliskileri
        public async Task<IActionResult> ParaBirimIliskileri()
        {
            try
            {
                var iliskiler = await _context.DovizIliskileri
                    .Include(d => d.KaynakParaBirimi)
                    .Include(d => d.HedefParaBirimi)
                    .Where(d => !d.SoftDelete)
                    .OrderBy(d => d.KaynakParaBirimi.Kod)
                    .ThenBy(d => d.HedefParaBirimi.Kod)
                    .ToListAsync();
                
                var viewModels = iliskiler.Select(i => new DovizIliskiViewModel
                {
                    DovizIliskiID = i.DovizIliskiID,
                    KaynakParaBirimiID = i.KaynakParaBirimiID,
                    HedefParaBirimiID = i.HedefParaBirimiID,
                    KaynakParaBirimiKodu = i.KaynakParaBirimi?.Kod,
                    HedefParaBirimiKodu = i.HedefParaBirimi?.Kod,
                    Aktif = i.Aktif
                }).ToList();
                
                return View(viewModels);
            }
            catch (Exception ex)
            {
                await _logService.LogEkleAsync($"Para birimi ilişkileri görüntülenirken hata oluştu: {ex.Message}", Models.LogTuru.Hata);
                TempData["ErrorMessage"] = "Para birimi ilişkileri görüntülenirken bir hata oluştu.";
                return View(new List<DovizIliskiViewModel>());
            }
        }

        // GET: Kur/ParaBirimIliskiEkle
        public async Task<IActionResult> ParaBirimIliskiEkle()
        {
            try
            {
                var paraBirimleri = await GetParaBirimiSelectListAsync();
                
                if (paraBirimleri.Count == 0)
                {
                    TempData["WarningMessage"] = "Para birimleri listesi boş. Lütfen önce para birimi ekleyin.";
                    return RedirectToAction(nameof(ParaBirimleri));
                }
                
                var viewModel = new DovizIliskiViewModel
                {
                    Aktif = true,
                    ParaBirimleri = paraBirimleri
                };
                
                return View(viewModel);
            }
            catch (Exception ex)
            {
                await _logService.LogEkleAsync($"Para birimi ilişkisi ekleme sayfası görüntülenirken hata oluştu: {ex.Message}", Models.LogTuru.Hata);
                TempData["ErrorMessage"] = "Para birimi ilişkisi ekleme sayfası görüntülenirken bir hata oluştu.";
                return RedirectToAction(nameof(ParaBirimIliskileri));
            }
        }

        // POST: Kur/ParaBirimIliskiEkle
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ParaBirimIliskiEkle(DovizIliskiViewModel viewModel)
        {
            viewModel.ParaBirimleri = await GetParaBirimiSelectListAsync();
            
            try
            {
                if (!ModelState.IsValid)
                {
                    return View(viewModel);
                }
                
                // Aynı para birimleri seçilmiş mi?
                if (viewModel.KaynakParaBirimiID == viewModel.HedefParaBirimiID)
                {
                    ModelState.AddModelError("", "Kaynak ve hedef para birimi aynı olamaz.");
                    return View(viewModel);
                }
                
                // İlişki zaten var mı kontrol et
                var existingIliski = await _context.DovizIliskileri
                    .FirstOrDefaultAsync(di => 
                        di.KaynakParaBirimiID == viewModel.KaynakParaBirimiID && 
                        di.HedefParaBirimiID == viewModel.HedefParaBirimiID && 
                        !di.SoftDelete);
                
                if (existingIliski != null)
                {
                    ModelState.AddModelError("", "Bu para birimleri arasında zaten bir ilişki tanımlanmış.");
                    return View(viewModel);
                }
                
                var entity = viewModel.ToEntity();
                await _kurService.AddDovizIliskiAsync(entity);
                
                var kaynakParaBirimi = await _context.ParaBirimleri.FindAsync(viewModel.KaynakParaBirimiID);
                var hedefParaBirimi = await _context.ParaBirimleri.FindAsync(viewModel.HedefParaBirimiID);
                
                await _logService.LogEkleAsync(
                    $"{kaynakParaBirimi?.Kod} -> {hedefParaBirimi?.Kod} para birimi ilişkisi eklendi.",
                    Models.LogTuru.Bilgi);
                
                TempData["SuccessMessage"] = "Para birimi ilişkisi başarıyla eklendi.";
                return RedirectToAction(nameof(ParaBirimIliskileri));
            }
            catch (Exception ex)
            {
                await _logService.LogEkleAsync($"Para birimi ilişkisi eklenirken hata oluştu: {ex.Message}", Models.LogTuru.Hata);
                ModelState.AddModelError("", $"Para birimi ilişkisi eklenirken bir hata oluştu: {ex.Message}");
                return View(viewModel);
            }
        }

        // GET: Kur/ParaBirimIliskiDuzenle/5
        public async Task<IActionResult> ParaBirimIliskiDuzenle(Guid id)
        {
            try
            {
                var dovizIliski = await _kurService.GetDovizIliskiByIdAsync(id);
                if (dovizIliski == null)
                {
                    TempData["ErrorMessage"] = "Para birimi ilişkisi bulunamadı.";
                    return RedirectToAction(nameof(ParaBirimIliskileri));
                }
                
                var viewModel = DovizIliskiViewModel.FromEntity(dovizIliski);
                viewModel.ParaBirimleri = await GetParaBirimiSelectListAsync();
                
                return View(viewModel);
            }
            catch (Exception ex)
            {
                await _logService.LogEkleAsync($"Para birimi ilişkisi düzenleme sayfası görüntülenirken hata oluştu: {ex.Message}", Models.LogTuru.Hata);
                TempData["ErrorMessage"] = "Para birimi ilişkisi düzenleme sayfası görüntülenirken bir hata oluştu.";
                return RedirectToAction(nameof(ParaBirimIliskileri));
            }
        }

        // POST: Kur/ParaBirimIliskiDuzenle/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ParaBirimIliskiDuzenle(Guid id, DovizIliskiViewModel viewModel)
        {
            viewModel.ParaBirimleri = await GetParaBirimiSelectListAsync();
            
            if (id != viewModel.DovizIliskiID)
            {
                TempData["ErrorMessage"] = "Geçersiz para birimi ilişkisi ID.";
                return RedirectToAction(nameof(ParaBirimIliskileri));
            }
            
            try
            {
                if (!ModelState.IsValid)
                {
                    return View(viewModel);
                }
                
                // Aynı para birimleri seçilmiş mi?
                if (viewModel.KaynakParaBirimiID == viewModel.HedefParaBirimiID)
                {
                    ModelState.AddModelError("", "Kaynak ve hedef para birimi aynı olamaz.");
                    return View(viewModel);
                }
                
                // İlişki zaten var mı kontrol et (diğer ilişkilerde)
                var existingIliski = await _context.DovizIliskileri
                    .FirstOrDefaultAsync(di => 
                        di.DovizIliskiID != id &&
                        di.KaynakParaBirimiID == viewModel.KaynakParaBirimiID && 
                        di.HedefParaBirimiID == viewModel.HedefParaBirimiID && 
                        !di.SoftDelete);
                
                if (existingIliski != null)
                {
                    ModelState.AddModelError("", "Bu para birimleri arasında zaten başka bir ilişki tanımlanmış.");
                    return View(viewModel);
                }
                
                var entity = viewModel.ToEntity();
                await _kurService.UpdateDovizIliskiAsync(entity);
                
                var kaynakParaBirimi = await _context.ParaBirimleri.FindAsync(viewModel.KaynakParaBirimiID);
                var hedefParaBirimi = await _context.ParaBirimleri.FindAsync(viewModel.HedefParaBirimiID);
                
                await _logService.LogEkleAsync(
                    $"{kaynakParaBirimi?.Kod} -> {hedefParaBirimi?.Kod} para birimi ilişkisi güncellendi.",
                    Models.LogTuru.Bilgi);
                
                TempData["SuccessMessage"] = "Para birimi ilişkisi başarıyla güncellendi.";
                return RedirectToAction(nameof(ParaBirimIliskileri));
            }
            catch (Exception ex)
            {
                await _logService.LogEkleAsync($"Para birimi ilişkisi güncellenirken hata oluştu: {ex.Message}", Models.LogTuru.Hata);
                ModelState.AddModelError("", $"Para birimi ilişkisi güncellenirken bir hata oluştu: {ex.Message}");
                return View(viewModel);
            }
        }

        // GET: Kur/ParaBirimIliskiSil/5
        public async Task<IActionResult> ParaBirimIliskiSil(Guid id)
        {
            try
            {
                var dovizIliski = await _kurService.GetDovizIliskiByIdAsync(id);
                if (dovizIliski == null)
                {
                    TempData["ErrorMessage"] = "Para birimi ilişkisi bulunamadı.";
                    return RedirectToAction(nameof(ParaBirimIliskileri));
                }
                
                var viewModel = DovizIliskiViewModel.FromEntity(dovizIliski);
                return View(viewModel);
            }
            catch (Exception ex)
            {
                await _logService.LogEkleAsync($"Para birimi ilişkisi silme sayfası görüntülenirken hata oluştu: {ex.Message}", Models.LogTuru.Hata);
                TempData["ErrorMessage"] = "Para birimi ilişkisi silme sayfası görüntülenirken bir hata oluştu.";
                return RedirectToAction(nameof(ParaBirimIliskileri));
            }
        }

        // POST: Kur/ParaBirimIliskiSil/5
        [HttpPost, ActionName("ParaBirimIliskiSil")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ParaBirimIliskiSilOnay(Guid id)
        {
            try
            {
                await _kurService.DeleteDovizIliskiAsync(id);
                await _logService.LogEkleAsync($"Para birimi ilişkisi silindi (ID: {id}).", Models.LogTuru.Bilgi);
                TempData["SuccessMessage"] = "Para birimi ilişkisi başarıyla silindi.";
            }
            catch (Exception ex)
            {
                await _logService.LogEkleAsync($"Para birimi ilişkisi silinirken hata oluştu: {ex.Message}", Models.LogTuru.Hata);
                TempData["ErrorMessage"] = $"Para birimi ilişkisi silinirken bir hata oluştu: {ex.Message}";
            }
            
            return RedirectToAction(nameof(ParaBirimIliskileri));
        }

        // Yardımcı metotlar
        private async Task<List<SelectListItem>> GetParaBirimiSelectListAsync()
        {
            try
            {
                var paraBirimleri = await _kurService.GetParaBirimleriAsync();
                
                // ParaBirimleri yoksa log oluştur ve boş liste döndür
                if (paraBirimleri == null || !paraBirimleri.Any())
                {
                    await _logService.LogEkleAsync("Para birimleri bulunamadı. Dropdown listeler boş olacak.", Models.LogTuru.Uyari);
                    _logger.LogWarning("Para birimleri bulunamadı. Dropdown listeler boş olacak.");
                    return new List<SelectListItem>();
                }
                
                return paraBirimleri
                    .Where(pb => pb.Aktif && !pb.SoftDelete)
                    .Select(pb => new SelectListItem
                    {
                        Value = pb.Kod,
                        Text = $"{pb.Kod} - {pb.Ad}"
                    })
                    .ToList();
            }
            catch (Exception ex)
            {
                await _logService.LogEkleAsync($"Para birimi listesi oluşturulurken hata: {ex.Message}", Models.LogTuru.Hata);
                _logger.LogError(ex, "Para birimi listesi oluşturulurken hata.");
                return new List<SelectListItem>();
            }
        }

        // Helper metod: Döviz ilişkileri için SelectList oluştur
        private async Task<List<SelectListItem>> GetDovizIliskiSelectListAsync()
        {
            var dovizIliskileri = await _context.DovizIliskileri
                .Include(d => d.KaynakParaBirimi)
                .Include(d => d.HedefParaBirimi)
                .Where(d => d.Aktif && !d.SoftDelete)
                .ToListAsync();

            return dovizIliskileri.Select(d => new SelectListItem
            {
                Value = d.DovizIliskiID.ToString(),
                Text = $"{d.KaynakParaBirimi?.Kod ?? "?"}/{d.HedefParaBirimi?.Kod ?? "?"}"
            }).ToList();
        }
    }
} 