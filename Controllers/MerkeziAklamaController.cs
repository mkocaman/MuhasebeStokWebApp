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
using MuhasebeStokWebApp.Enums;
using MuhasebeStokWebApp.Services;
using MuhasebeStokWebApp.Services.Interfaces;
using MuhasebeStokWebApp.ViewModels.Aklama;

namespace MuhasebeStokWebApp.Controllers
{
    [Authorize]
    public class MerkeziAklamaController : Controller
    {
        private readonly IMerkeziAklamaService _merkeziAklamaService;
        private readonly ILogger<MerkeziAklamaController> _logger;
        private readonly ApplicationDbContext _context;
        private readonly IDropdownService _dropdownService;

        public MerkeziAklamaController(
            IMerkeziAklamaService merkeziAklamaService,
            IDropdownService dropdownService,
            ApplicationDbContext context,
            ILogger<MerkeziAklamaController> logger)
        {
            _merkeziAklamaService = merkeziAklamaService;
            _dropdownService = dropdownService;
            _context = context;
            _logger = logger;
        }

        // Ana sayfa - Özet bilgileri gösterir
        public async Task<IActionResult> Index()
        {
            try
            {
                var ozet = await _merkeziAklamaService.GetAklamaOzetiAsync();
                return View(ozet);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Merkezi aklama özeti getirilirken hata oluştu");
                TempData["ErrorMessage"] = "Aklama özeti yüklenirken bir hata oluştu.";
                return RedirectToAction("Index", "Home");
            }
        }

        // Bekleyen aklama kayıtlarını listeler
        public async Task<IActionResult> BekleyenKayitlar(int page = 1, int pageSize = 50, Guid? urunId = null)
        {
            try
            {
                var bekleyenKayitlar = await _merkeziAklamaService.GetBekleyenAklamaKayitlariAsync(page, pageSize, urunId);
                
                ViewBag.PageNumber = page;
                ViewBag.PageSize = pageSize;
                ViewBag.HasNext = bekleyenKayitlar.Count == pageSize;
                ViewBag.UrunId = urunId;
                
                // Ürün dropdown listesi için
                ViewBag.Urunler = await _dropdownService.GetUrunSelectListAsync(urunId);
                
                return View(bekleyenKayitlar);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Bekleyen aklama kayıtları listelenirken hata oluştu");
                TempData["ErrorMessage"] = "Bekleyen aklama kayıtları listelenirken bir hata oluştu.";
                return RedirectToAction("Index");
            }
        }

        // Aklanmış kayıtları listeler
        public async Task<IActionResult> AklanmisKayitlar(int page = 1, int pageSize = 50, Guid? urunId = null)
        {
            try
            {
                var aklanmisKayitlar = await _merkeziAklamaService.GetAklanmisKayitlarAsync(page, pageSize, urunId);
                
                ViewBag.PageNumber = page;
                ViewBag.PageSize = pageSize;
                ViewBag.HasNext = aklanmisKayitlar.Count == pageSize;
                ViewBag.UrunId = urunId;
                
                // Ürün dropdown listesi için
                ViewBag.Urunler = await _dropdownService.GetUrunSelectListAsync(urunId);
                
                return View(aklanmisKayitlar);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Aklanmış kayıtlar listelenirken hata oluştu");
                TempData["ErrorMessage"] = "Aklanmış kayıtlar listelenirken bir hata oluştu.";
                return RedirectToAction("Index");
            }
        }

        // Faturaların listesi - Sadece resmi olmayan faturaları gösterir
        public async Task<IActionResult> ResmiOlmayanFaturalar()
        {
            try
            {
                var faturalar = await _context.Faturalar
                    .Where(f => f.ResmiMi == false && !f.Silindi)
                    .OrderByDescending(f => f.FaturaTarihi)
                    .ToListAsync();
                
                return View(faturalar);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Resmi olmayan faturalar listelenirken hata oluştu");
                TempData["ErrorMessage"] = "Resmi olmayan faturalar listelenirken bir hata oluştu.";
                return RedirectToAction("Index");
            }
        }
        
        // Faturaların listesi - Sadece resmi faturaları gösterir
        public async Task<IActionResult> ResmiFaturalar()
        {
            try
            {
                var faturalar = await _context.Faturalar
                    .Where(f => f.ResmiMi == true && !f.Silindi)
                    .OrderByDescending(f => f.FaturaTarihi)
                    .ToListAsync();
                
                return View(faturalar);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Resmi faturalar listelenirken hata oluştu");
                TempData["ErrorMessage"] = "Resmi faturalar listelenirken bir hata oluştu.";
                return RedirectToAction("Index");
            }
        }

        // Faturayı aklama kuyruğuna ekle
        [HttpPost]
        public async Task<IActionResult> FaturayiKuyrugaEkle(Guid faturaId)
        {
            try
            {
                // Faturanın resmi olmadığından emin olalım
                var fatura = await _context.Faturalar.FindAsync(faturaId);
                if (fatura == null)
                {
                    TempData["ErrorMessage"] = "Fatura bulunamadı.";
                    return RedirectToAction("ResmiOlmayanFaturalar");
                }
                
                if (fatura.ResmiMi == true)
                {
                    TempData["ErrorMessage"] = "Resmi faturalar aklama kuyruğuna eklenemez.";
                    return RedirectToAction("ResmiOlmayanFaturalar");
                }
                
                var result = await _merkeziAklamaService.FaturayiAklamaKuyrugunaEkleAsync(faturaId);
                
                if (result)
                {
                    TempData["SuccessMessage"] = "Fatura başarıyla aklama kuyruğuna eklendi.";
                }
                else
                {
                    TempData["ErrorMessage"] = "Fatura aklama kuyruğuna eklenirken bir hata oluştu.";
                }
                
                return RedirectToAction("ResmiOlmayanFaturalar");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Fatura aklama kuyruğuna eklenirken hata oluştu. FaturaID: {FaturaID}", faturaId);
                TempData["ErrorMessage"] = "Fatura aklama kuyruğuna eklenirken bir hata oluştu.";
                return RedirectToAction("ResmiOlmayanFaturalar");
            }
        }

        // Ürün bazlı aklama durumu
        public async Task<IActionResult> UrunDurumlari()
        {
            try
            {
                var urunDurumlari = await _merkeziAklamaService.GetTumUrunlerinAklamaDurumuAsync();
                return View(urunDurumlari);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ürün durumları getirilirken hata oluştu");
                TempData["ErrorMessage"] = "Ürün durumları getirilirken bir hata oluştu.";
                return RedirectToAction("Index");
            }
        }

        // Ürün aklama geçmişi
        public async Task<IActionResult> UrunGecmisi(Guid urunId)
        {
            try
            {
                var gecmis = await _merkeziAklamaService.GetUrunAklamaGecmisiAsync(urunId);
                
                if (gecmis == null)
                {
                    TempData["ErrorMessage"] = "Ürün bulunamadı.";
                    return RedirectToAction("UrunDurumlari");
                }
                
                return View(gecmis);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ürün geçmişi getirilirken hata oluştu. UrunID: {UrunID}", urunId);
                TempData["ErrorMessage"] = "Ürün geçmişi getirilirken bir hata oluştu.";
                return RedirectToAction("UrunDurumlari");
            }
        }

        // Fatura kalemleri - Aklama için
        public async Task<IActionResult> FaturaKalemleri(Guid faturaId)
        {
            try
            {
                var fatura = await _context.Faturalar
                    .Include(f => f.Cari)
                    .Include(f => f.FaturaDetaylari)
                        .ThenInclude(fd => fd.Urun)
                            .ThenInclude(u => u.Birim)
                    .AsSplitQuery()
                    .FirstOrDefaultAsync(f => f.FaturaID == faturaId);
                
                if (fatura == null)
                {
                    TempData["ErrorMessage"] = "Fatura bulunamadı.";
                    return RedirectToAction("ResmiOlmayanFaturalar");
                }
                
                ViewBag.Fatura = fatura;
                
                return View(fatura.FaturaDetaylari.Where(fd => !fd.Silindi).ToList());
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Fatura kalemleri getirilirken hata oluştu. FaturaID: {FaturaID}", faturaId);
                TempData["ErrorMessage"] = "Fatura kalemleri getirilirken bir hata oluştu.";
                return RedirectToAction("ResmiOlmayanFaturalar");
            }
        }

        // Fatura kalemi aklama kuyruğuna ekle
        [HttpPost]
        public async Task<IActionResult> FaturaKaleminiKuyrugaEkle(Guid faturaKalemId)
        {
            try
            {
                // Fatura kaleminin resmi olmayan faturaya ait olduğundan emin olalım
                var faturaKalem = await _context.FaturaDetaylari
                    .Include(fd => fd.Fatura)
                    .FirstOrDefaultAsync(fd => fd.FaturaDetayID == faturaKalemId);
                
                if (faturaKalem == null)
                {
                    TempData["ErrorMessage"] = "Fatura kalemi bulunamadı.";
                    return RedirectToAction("ResmiOlmayanFaturalar");
                }
                
                if (faturaKalem.Fatura?.ResmiMi == true)
                {
                    TempData["ErrorMessage"] = "Resmi faturalara ait kalemler aklama kuyruğuna eklenemez.";
                    return RedirectToAction("FaturaKalemleri", new { faturaId = faturaKalem.FaturaID });
                }
                
                var result = await _merkeziAklamaService.FaturaKaleminiAklamaKuyrugunaEkleAsync(faturaKalemId);
                
                if (result)
                {
                    TempData["SuccessMessage"] = "Fatura kalemi başarıyla aklama kuyruğuna eklendi.";
                }
                else
                {
                    TempData["ErrorMessage"] = "Fatura kalemi aklama kuyruğuna eklenirken bir hata oluştu.";
                }
                
                return RedirectToAction("FaturaKalemleri", new { faturaId = faturaKalem.FaturaID });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Fatura kalemi aklama kuyruğuna eklenirken hata oluştu. FaturaKalemID: {FaturaKalemID}", faturaKalemId);
                TempData["ErrorMessage"] = "Fatura kalemi aklama kuyruğuna eklenirken bir hata oluştu.";
                return RedirectToAction("ResmiOlmayanFaturalar");
            }
        }

        // Otomatik aklama yapma
        [HttpPost]
        public async Task<IActionResult> OtomatikAklama(Guid resmiFaturaId)
        {
            try
            {
                // Faturanın resmi olduğundan emin olalım
                var fatura = await _context.Faturalar.FindAsync(resmiFaturaId);
                if (fatura == null)
                {
                    TempData["ErrorMessage"] = "Fatura bulunamadı.";
                    return RedirectToAction("ResmiFaturalar");
                }
                
                if (fatura.ResmiMi != true)
                {
                    TempData["ErrorMessage"] = "Sadece resmi faturalar ile aklama yapılabilir.";
                    return RedirectToAction("ResmiFaturalar");
                }
                
                var result = await _merkeziAklamaService.OtomatikAklamaYapAsync(resmiFaturaId);
                
                if (result)
                {
                    TempData["SuccessMessage"] = "Otomatik aklama başarıyla yapıldı.";
                }
                else
                {
                    TempData["ErrorMessage"] = "Otomatik aklama yapılırken bir hata oluştu.";
                }
                
                return RedirectToAction("ResmiFaturalar");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Otomatik aklama yapılırken hata oluştu. ResmiFaturaID: {ResmiFaturaID}", resmiFaturaId);
                TempData["ErrorMessage"] = "Otomatik aklama yapılırken bir hata oluştu.";
                return RedirectToAction("ResmiFaturalar");
            }
        }

        // Manuel aklama sayfası
        public async Task<IActionResult> ManuelAklama(Guid resmiFaturaKalemId)
        {
            try
            {
                // Resmi fatura kalemini getir
                var faturaKalem = await _context.FaturaDetaylari
                    .Include(fd => fd.Fatura)
                        .ThenInclude(f => f.Cari)
                    .Include(fd => fd.Urun)
                        .ThenInclude(u => u.Birim)
                    .AsSplitQuery()
                    .FirstOrDefaultAsync(fd => fd.FaturaDetayID == resmiFaturaKalemId);
                
                if (faturaKalem == null)
                {
                    TempData["ErrorMessage"] = "Fatura kalemi bulunamadı.";
                    return RedirectToAction("ResmiFaturalar");
                }
                
                if (faturaKalem.Fatura?.ResmiMi != true)
                {
                    TempData["ErrorMessage"] = "Sadece resmi faturalar ile aklama yapılabilir.";
                    return RedirectToAction("ResmiFaturalar");
                }
                
                // Merkezi aklama sistemi kaldırıldığı için bu işlem pasif hale getirildi
                var model = new ManuelAklamaViewModel
                {
                    ResmiFaturaKalemID = resmiFaturaKalemId,
                    ResmiFaturaNo = faturaKalem.Fatura?.FaturaNumarasi,
                    UrunAdi = faturaKalem.Urun?.UrunAdi,
                    UrunKodu = faturaKalem.Urun?.UrunKodu,
                    ResmiFaturaMiktar = faturaKalem.Miktar,
                    BekleyenAklamaKayitlari = new List<BekleyenAklamaViewModel>()
                };
                
                return View(model);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Manuel aklama sayfası yüklenirken hata oluştu. ResmiFaturaKalemID: {ResmiFaturaKalemID}", resmiFaturaKalemId);
                TempData["ErrorMessage"] = "Manuel aklama sayfası yüklenirken bir hata oluştu.";
                return RedirectToAction("ResmiFaturalar");
            }
        }

        // Manuel aklama işlemi
        [HttpPost]
        public async Task<IActionResult> ManuelAklama(ManuelAklamaViewModel model)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    TempData["ErrorMessage"] = "Formda hatalar var, lütfen kontrol edip tekrar deneyin.";
                    return View(model);
                }
                
                // Seçilen kayıtları ve miktarları al
                var secilenKayitIdleri = new List<Guid>();
                decimal toplamMiktar = 0;
                
                foreach (var kayit in model.BekleyenAklamaKayitlari)
                {
                    if (kayit.Secildi && kayit.AklanacakMiktar > 0)
                    {
                        secilenKayitIdleri.Add(kayit.AklamaID);
                        toplamMiktar += kayit.AklanacakMiktar;
                    }
                }
                
                if (secilenKayitIdleri.Count == 0 || toplamMiktar <= 0)
                {
                    TempData["ErrorMessage"] = "Lütfen en az bir kayıt seçin ve aklanacak miktar girin.";
                    return View(model);
                }
                
                // Manuel aklama işlemini gerçekleştir
                var result = await _merkeziAklamaService.ManuelAklamaYapAsync(
                    model.ResmiFaturaKalemID,
                    secilenKayitIdleri,
                    toplamMiktar,
                    model.AklamaNotu);
                
                if (result)
                {
                    TempData["SuccessMessage"] = "Manuel aklama başarıyla yapıldı.";
                    
                    // Resmi faturanın ID'sini bul
                    var faturaKalem = await _context.FaturaDetaylari.FindAsync(model.ResmiFaturaKalemID);
                    return RedirectToAction("ResmiFaturaKalemleri", new { faturaId = faturaKalem?.FaturaID });
                }
                else
                {
                    TempData["ErrorMessage"] = "Manuel aklama yapılırken bir hata oluştu.";
                    return View(model);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Manuel aklama yapılırken hata oluştu. ResmiFaturaKalemID: {ResmiFaturaKalemID}", model.ResmiFaturaKalemID);
                TempData["ErrorMessage"] = "Manuel aklama yapılırken bir hata oluştu.";
                return View(model);
            }
        }

        // Resmi fatura kalemleri sayfası
        public async Task<IActionResult> ResmiFaturaKalemleri(Guid faturaId)
        {
            try
            {
                var fatura = await _context.Faturalar
                    .Include(f => f.Cari)
                    .Include(f => f.FaturaDetaylari)
                        .ThenInclude(fd => fd.Urun)
                            .ThenInclude(u => u.Birim)
                    .AsSplitQuery()
                    .FirstOrDefaultAsync(f => f.FaturaID == faturaId);
                
                if (fatura == null)
                {
                    TempData["ErrorMessage"] = "Fatura bulunamadı.";
                    return RedirectToAction("ResmiFaturalar");
                }
                
                if (fatura.ResmiMi != true)
                {
                    TempData["ErrorMessage"] = "Sadece resmi faturalar için bu sayfa görüntülenebilir.";
                    return RedirectToAction("ResmiFaturalar");
                }
                
                ViewBag.Fatura = fatura;
                
                return View(fatura.FaturaDetaylari.Where(fd => !fd.Silindi).ToList());
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Resmi fatura kalemleri getirilirken hata oluştu. FaturaID: {FaturaID}", faturaId);
                TempData["ErrorMessage"] = "Resmi fatura kalemleri getirilirken bir hata oluştu.";
                return RedirectToAction("ResmiFaturalar");
            }
        }

        // Aklama iptal et
        [HttpPost]
        public async Task<IActionResult> AklamaIptal(Guid aklamaId, string returnUrl = null)
        {
            try
            {
                var result = await _merkeziAklamaService.AklamaIptalEtAsync(aklamaId);
                
                if (result)
                {
                    TempData["SuccessMessage"] = "Aklama kaydı başarıyla iptal edildi.";
                }
                else
                {
                    TempData["ErrorMessage"] = "Aklama kaydı iptal edilirken bir hata oluştu.";
                }
                
                if (!string.IsNullOrEmpty(returnUrl))
                {
                    return Redirect(returnUrl);
                }
                
                return RedirectToAction("AklanmisKayitlar");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Aklama kaydı iptal edilirken hata oluştu. AklamaID: {AklamaID}", aklamaId);
                TempData["ErrorMessage"] = "Aklama kaydı iptal edilirken bir hata oluştu.";
                
                if (!string.IsNullOrEmpty(returnUrl))
                {
                    return Redirect(returnUrl);
                }
                
                return RedirectToAction("AklanmisKayitlar");
            }
        }

        // Resmi fatura iptal et (aklanan tüm kayıtları iptal eder)
        [HttpPost]
        public async Task<IActionResult> ResmiFaturaIptal(Guid resmiFaturaId)
        {
            try
            {
                var result = await _merkeziAklamaService.ResmiFaturaIptalAsync(resmiFaturaId);
                
                if (result)
                {
                    TempData["SuccessMessage"] = "Resmi fatura ile yapılan tüm aklamalar başarıyla iptal edildi.";
                }
                else
                {
                    TempData["ErrorMessage"] = "Resmi fatura ile yapılan aklamalar iptal edilirken bir hata oluştu.";
                }
                
                return RedirectToAction("ResmiFaturalar");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Resmi fatura ile yapılan aklamalar iptal edilirken hata oluştu. ResmiFaturaID: {ResmiFaturaID}", resmiFaturaId);
                TempData["ErrorMessage"] = "Resmi fatura ile yapılan aklamalar iptal edilirken bir hata oluştu.";
                return RedirectToAction("ResmiFaturalar");
            }
        }

        public async Task<IActionResult> Filter<TEntity>(string tarihlemeFiltresi, DateTime? baslangicTarihi, DateTime? bitisTarihi)
            where TEntity : class
        {
            try
            {
                DateTime start = baslangicTarihi ?? DateTime.Now.AddMonths(-1);
                DateTime end = bitisTarihi ?? DateTime.Now;

                List<TEntity> filtrelenmisKayitlar = new List<TEntity>();

                // Entity türünü önceden kontrol edelim
                if (typeof(TEntity) == typeof(Fatura))
                {
                    // Faturalar için filtreleme - FaturaTarihi property'sine güvenli erişim
                    var query = _context.Set<Fatura>()
                        .AsNoTracking()
                        .Where(f => !f.Silindi);
                        
                    // Tarih kontrolü
                    query = query.Where(f => 
                        (f.FaturaTarihi.HasValue && f.FaturaTarihi.Value >= start && f.FaturaTarihi.Value <= end));
                        
                    // Cast işlemi
                    filtrelenmisKayitlar = await query.Cast<TEntity>().ToListAsync();
                }
                else
                {
                    // Diğer entity tipleri için basit filtreleme
                    filtrelenmisKayitlar = await _context.Set<TEntity>()
                        .AsNoTracking()
                        .ToListAsync();
                }

                return PartialView("_FilteredItemsPartial", filtrelenmisKayitlar);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Filtreleme işlemi sırasında hata oluştu");
                return Json(new { success = false, message = "Filtreleme sırasında bir hata oluştu." });
            }
        }
    }
} 