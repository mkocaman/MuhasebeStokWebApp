using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Identity;
using MuhasebeStokWebApp.Data;
using MuhasebeStokWebApp.Data.Entities;
using MuhasebeStokWebApp.Data.Repositories;
using MuhasebeStokWebApp.Enums;
using MuhasebeStokWebApp.Models;
using MuhasebeStokWebApp.Services;
using MuhasebeStokWebApp.Services.Interfaces;
using MuhasebeStokWebApp.ViewModels.Fatura;
using Fatura = MuhasebeStokWebApp.Data.Entities.Fatura;
using FaturaDetay = MuhasebeStokWebApp.Data.Entities.FaturaDetay;
using ClosedXML.Excel;
using System.IO;

namespace MuhasebeStokWebApp.Controllers
{
    [Authorize]
    public class FaturaController : BaseController
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ApplicationDbContext _context;
        private readonly IStokFifoService _stokFifoService;
        private readonly IDovizKuruService _dovizKuruService;
        private readonly ILogger<FaturaController> _logger;
        private readonly IStokService _stokService;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IFaturaService _faturaService;
        private readonly IFaturaOrchestrationService _faturaOrchestrationService;
        private readonly IFaturaValidationService _faturaValidationService;

        public FaturaController(
            IUnitOfWork unitOfWork,
            ApplicationDbContext context,
            IStokFifoService stokFifoService,
            IDovizKuruService dovizKuruService,
            ILogger<FaturaController> logger,
            ILogService logService,
            IMenuService menuService,
            UserManager<ApplicationUser> userManager,
            RoleManager<IdentityRole> roleManager,
            IStokService stokService,
            IFaturaService faturaService,
            IFaturaOrchestrationService faturaOrchestrationService,
            IFaturaValidationService faturaValidationService)
            : base(menuService, userManager, roleManager, logService)
        {
            _unitOfWork = unitOfWork;
            _context = context;
            _stokFifoService = stokFifoService;
            _dovizKuruService = dovizKuruService;
            _logger = logger;
            _stokService = stokService;
            _userManager = userManager;
            _faturaService = faturaService;
            _faturaOrchestrationService = faturaOrchestrationService;
            _faturaValidationService = faturaValidationService;
        }

        // GET: Fatura
        public async Task<IActionResult> Index()
        {
            var faturalar = await _context.Faturalar
                .IgnoreQueryFilters() // Ignore query filters to get all Cari records
                .Include(f => f.Cari) // Sildi filtresi olmadan Cari'yi dahil et
                .Include(f => f.FaturaTuru)
                .Where(f => !f.Silindi)
                .OrderByDescending(f => f.FaturaTarihi)
                .ToListAsync();

            var viewModel = faturalar.Select(f => new FaturaViewModel
            {
                FaturaID = f.FaturaID.ToString(),
                FaturaNumarasi = f.FaturaNumarasi ?? "Numara Yok",
                FaturaTarihi = f.FaturaTarihi,
                VadeTarihi = f.VadeTarihi,
                CariAdi = f.Cari?.Ad ?? "Bilinmiyor",
                CariSilindi = f.Cari != null && f.Cari.Silindi, // Cari silinmiş mi kontrolü
                FaturaTuru = f.FaturaTuru?.FaturaTuruAdi ?? "Belirtilmemiş",
                FaturaTuruAdi = f.FaturaTuru?.FaturaTuruAdi ?? "Belirtilmemiş",
                AraToplam = f.AraToplam ?? 0,
                KDVToplam = f.KDVToplam ?? 0,
                GenelToplam = f.GenelToplam ?? 0,
                OdemeDurumu = f.OdemeDurumu ?? "Ödenmedi",
                DovizTuru = f.DovizTuru ?? "TRY",
                DovizKuru = f.DovizKuru ?? 1,
                ResmiMi = f.ResmiMi,
                Aciklama = f.FaturaNotu ?? "Açıklama yok"
            }).ToList();

            return View(viewModel);
        }

        // GET: Fatura/Create
        [Authorize(Roles = "Admin,FinansYonetici,Kullanici")]
        public async Task<IActionResult> Create()
        {
            try
            {
                _logger.LogInformation($"Fatura oluşturma sayfası yükleniyor: Kullanıcı={User.Identity.Name}");
                // Aktif cariler
                var cariler = await _context.Cariler
                    .Where(c => !c.Silindi && c.AktifMi)
                    .OrderBy(c => c.Ad)
                    .ToListAsync();

                _logger.LogInformation("Cari listesi yüklendi, toplam {count} adet cari bulundu.", cariler.Count);

                // Sözleşmeler
                var sozlesmeler = await _context.Sozlesmeler
                    .Where(s => !s.Silindi && s.AktifMi)
                    .OrderBy(s => s.SozlesmeNo)
                    .ToListAsync();

                _logger.LogInformation("Sözleşme listesi yüklendi, toplam {count} adet sözleşme bulundu.", sozlesmeler.Count);

                // Aktif ürünler
                var urunler = await _context.Urunler
                    .Include(u => u.Birim)
                    .Where(u => !u.Silindi && u.Aktif)
                    .OrderBy(u => u.UrunAdi)
                    .ToListAsync();

                _logger.LogInformation("Ürün listesi yüklendi, toplam {count} adet ürün bulundu.", urunler.Count);

                // Aktif depolar
                var depolar = await _context.Depolar
                    .Where(d => !d.Silindi && d.Aktif)
                    .OrderBy(d => d.DepoAdi)
                    .ToListAsync();
                
                _logger.LogInformation("Depo listesi yüklendi, toplam {count} adet depo bulundu.", depolar.Count);

                if (urunler.Count == 0)
                {
                    _logger.LogWarning("Ürün listesi boş! Veritabanında aktif ürün bulunamadı.");
                    // Ürün listesi boş ise, geçici bir ürün ekliyoruz (test amaçlı)
                    ViewBag.UrunListesiBosMu = true;
                }
                else
                {
                    ViewBag.UrunListesiBosMu = false;
                    // Ürünler için hem birim bilgisini hem de ID'yi taşıyacak özel bir SelectList
                    var urunSelectList = urunler.Select(u => new SelectListItem
                    {
                        Value = u.UrunID.ToString(),
                        Text = u.UrunAdi,
                        Group = new SelectListGroup { Name = u.KategoriID.HasValue ? u.Kategori?.KategoriAdi ?? "Genel" : "Genel" }
                    }).ToList();
                    
                    _logger.LogInformation("Ürün select list oluşturuldu, toplam {count} adet öğe içeriyor.", urunSelectList.Count);
                    
                    // Birim bilgisini farklı bir yöntemle taşıyalım, örneğin ViewBag ile
                    ViewBag.UrunBirimBilgileri = urunler.ToDictionary(
                        u => u.UrunID.ToString(), 
                        u => u.Birim?.BirimAdi ?? "Adet"
                    );

                    ViewBag.Urunler = urunSelectList;
                }

                // Para birimleri
                var paraBirimleri = await _context.ParaBirimleri
                    .Where(p => !p.Silindi && p.Aktif)
                    .OrderBy(p => p.Sira)
                    .ToListAsync();

                _logger.LogInformation("Para birimi listesi yüklendi, toplam {count} adet para birimi bulundu.", paraBirimleri.Count);

                // Carilerin varsayılan para birimleri
                var cariParaBirimleri = cariler
                    .Where(c => c.VarsayilanParaBirimiId.HasValue)
                    .ToDictionary(
                        c => c.CariID.ToString(),
                        c => paraBirimleri.FirstOrDefault(p => p.ParaBirimiID == c.VarsayilanParaBirimiId)?.Kod ?? "TRY"
                    );

                // ViewBag ile verileri taşı
                ViewBag.Cariler = new SelectList(cariler, "CariID", "Ad");
                ViewBag.Sozlesmeler = new SelectList(sozlesmeler, "SozlesmeID", "SozlesmeNo");
                ViewBag.FaturaTurleri = new SelectList(_context.FaturaTurleri, "FaturaTuruID", "FaturaTuruAdi");
                ViewBag.ParaBirimleri = paraBirimleri;
                ViewBag.CariParaBirimleri = cariParaBirimleri; // Cari - ParaBirimi ilişkisini JS'e taşımak için
                ViewBag.Depolar = new SelectList(depolar, "DepoID", "DepoAdi");

                // Varsayılan model
                var model = new FaturaCreateViewModel
                {
                    FaturaTarihi = DateTime.Now,
                    VadeTarihi = DateTime.Now.AddDays(30),
                    DovizTuru = "TRY",
                    DovizKuru = 1,
                    OdemeDurumu = "Ödenmedi",
                    OtomatikIrsaliyeOlustur = true, // Varsayılan olarak true
                    FaturaKalemleri = new List<FaturaKalemViewModel>
                    {
                        // Varsayılan boş fatura kalemi
                        new FaturaKalemViewModel
                        {
                            Miktar = 1,
                            KdvOrani = 12, // Varsayılan KDV %12
                            IndirimOrani = 0
                        }
                    }
                };

                return View(model);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Fatura oluşturma sayfası yüklenirken hata oluştu");
                TempData["ErrorMessage"] = "Fatura oluşturma sayfası yüklenirken bir hata oluştu: " + ex.Message;
                return RedirectToAction(nameof(Index));
            }
        }

        // POST: Fatura/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(FaturaCreateViewModel viewModel)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    // Model validasyonu
                    var validationResult = _faturaValidationService.ValidateFaturaCreateViewModel(viewModel);
                    if (!validationResult.IsValid)
                    {
                        ModelState.AddModelError(string.Empty, validationResult.ErrorMessage);
                        await PopulateFaturaCreateViewModelAsync(viewModel);
                        return View(viewModel);
                    }

                    // Mevcut kullanıcıyı al
                    var currentUser = await _userManager.GetUserAsync(User);
                    var currentUserId = currentUser?.Id != null ? Guid.Parse(currentUser.Id) : (Guid?)null;

                    // Fatura oluştur (StokHareket, CariHareket, Irsaliye dahil)
                    var faturaId = await _faturaOrchestrationService.CreateFaturaWithRelations(viewModel, currentUserId);

                    // İşlem başarılı
                    _logger.LogInformation($"Fatura başarıyla oluşturuldu. FaturaID: {faturaId}");
                    TempData["SuccessMessage"] = "Fatura başarıyla oluşturuldu.";
                    return RedirectToAction(nameof(Details), new { id = faturaId });
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Fatura oluşturma hatası");
                    ModelState.AddModelError(string.Empty, $"Fatura oluşturulurken bir hata oluştu: {ex.Message}");
                }
            }

            // Form geçerli değilse veya hata oluştuysa, dropdown listelerini yeniden doldur
            await PopulateFaturaCreateViewModelAsync(viewModel);
            return View(viewModel);
        }

        /// <summary>
        /// Fatura oluşturma formundaki dropdown ve diğer verileri doldurur
        /// </summary>
        private async Task PopulateFaturaCreateViewModelAsync(FaturaCreateViewModel viewModel)
        {
            // Mevcut YukleCreateFormData metodunu yeniden kullan
            await YukleCreateFormData(viewModel);
        }

        /// <summary>
        /// Create formunu doldurmak için gerekli verileri yükler
        /// </summary>
        private async Task YukleCreateFormData(FaturaCreateViewModel viewModel)
        {
            var cariler = await _context.Cariler
                .Where(c => !c.Silindi && c.AktifMi)
                .OrderBy(c => c.Ad)
                .ToListAsync();
            
            ViewBag.Cariler = new SelectList(cariler, "CariID", "Ad", viewModel.CariID);
                    ViewBag.FaturaTurleri = new SelectList(_context.FaturaTurleri, "FaturaTuruID", "FaturaTuruAdi", viewModel.FaturaTuruID);
                    
                    var urunler = await _context.Urunler
                        .Include(u => u.Birim)
                        .Where(u => !u.Silindi && u.Aktif)
                .OrderBy(u => u.UrunAdi)
                        .ToListAsync();

            // Ürünler için selectlist
                    var urunSelectList = urunler.Select(u => new SelectListItem
                    {
                        Value = u.UrunID.ToString(),
                        Text = u.UrunAdi,
                        Group = new SelectListGroup { Name = u.KategoriID.HasValue ? u.Kategori?.KategoriAdi ?? "Genel" : "Genel" }
                    }).ToList();
                    
                    ViewBag.Urunler = urunSelectList;
            
            // Birim bilgilerini ayrıca ata
                    ViewBag.UrunBirimBilgileri = urunler.ToDictionary(
                        u => u.UrunID.ToString(), 
                        u => u.Birim?.BirimAdi ?? "Adet"
                    );
                    
            var depolar = await _context.Depolar
                .Where(d => !d.Silindi && d.Aktif)
                .OrderBy(d => d.DepoAdi)
                .ToListAsync();
            
            ViewBag.Depolar = new SelectList(depolar, "DepoID", "DepoAdi", viewModel.DepoID);
            
            var paraBirimleri = await _context.ParaBirimleri
                        .Where(p => !p.Silindi && p.Aktif)
                        .OrderBy(p => p.Sira)
                        .ToListAsync();
                    
            ViewBag.ParaBirimleri = paraBirimleri;
            
            // Cari varsayılan para birimleri
            var cariParaBirimleri = cariler
                .Where(c => c.VarsayilanParaBirimiId.HasValue)
                .ToDictionary(
                    c => c.CariID.ToString(),
                    c => paraBirimleri.FirstOrDefault(p => p.ParaBirimiID == c.VarsayilanParaBirimiId)?.Kod ?? "TRY"
                );
                
            ViewBag.CariParaBirimleri = cariParaBirimleri;
        }

        // GET: Fatura/Details/5
        public async Task<IActionResult> Details(Guid? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var fatura = await _context.Faturalar
                .IgnoreQueryFilters() // Ignore query filters to get all Cari records 
                .Include(f => f.Cari) // Silindi filtresiz Cari ilişkisini yükle 
                .Include(f => f.FaturaTuru)
                .Include(f => f.FaturaDetaylari)
                .ThenInclude(fd => fd.Urun)
                        .ThenInclude(u => u.Birim)
                .Include(f => f.FaturaOdemeleri)
                .Where(f => f.FaturaID == id && !f.Silindi)
                .SingleOrDefaultAsync();

            if (fatura == null)
            {
                return NotFound();
            }

            // Cari silinmiş mi bilgisini ViewBag'e ekle
            ViewBag.CariSilindi = fatura.Cari != null && fatura.Cari.Silindi;

            // Fatura ödemelerini al
            var faturanınOdemeleri = await _context.FaturaOdemeleri
                .Where(o => o.FaturaID == id && !o.Silindi)
                .ToListAsync();

            // Toplam ödeme tutarını hesapla
            decimal odenenTutar = faturanınOdemeleri.Sum(o => o.OdemeTutari);

            // OdenecekTutar hesaplama
            decimal odenecekTutar = fatura.GenelToplam.GetValueOrDefault() - odenenTutar;
            
            // Ödeme bilgilerini ViewBag'e ekle
            ViewBag.OdenenTutar = odenenTutar;
            ViewBag.OdenecekTutar = odenecekTutar;
            ViewBag.OdemeDurumu = odenenTutar == 0 ? "Bekliyor" : 
                                  odenenTutar == fatura.GenelToplam.GetValueOrDefault() ? "Ödendi" : 
                                  "Kısmi Ödendi";

            // Detay ViewModel oluştur
            var viewModel = new FaturaDetailViewModel
            {
                FaturaID = fatura.FaturaID,
                FaturaNumarasi = fatura.FaturaNumarasi ?? "Numara Yok",
                FaturaTarihi = fatura.FaturaTarihi,
                VadeTarihi = fatura.VadeTarihi,
                CariID = fatura.CariID ?? Guid.Empty,
                CariAdi = fatura.Cari?.Ad ?? "Bilinmiyor", // Cari silinmiş olsa bile adını göster
                CariSilindi = fatura.Cari != null && fatura.Cari.Silindi, // Model'e de ekle
                CariVergiNo = fatura.Cari?.VergiNo ?? "Belirtilmemiş",
                CariVergiDairesi = fatura.Cari?.VergiDairesi ?? "Belirtilmemiş",
                CariAdres = fatura.Cari?.Adres ?? "Belirtilmemiş",
                CariTelefon = fatura.Cari?.Telefon ?? "Belirtilmemiş",
                FaturaTuru = fatura.FaturaTuru?.FaturaTuruAdi ?? "Belirtilmemiş",
                FaturaTuruID = fatura.FaturaTuruID, 
                SiparisNumarasi = fatura.SiparisNumarasi ?? "",
                // Eğer irsaliye numarası Fatura entity'sinde varsa kullan, yoksa boş string kullan
                IrsaliyeNumarasi = "", 
                ResmiMi = fatura.ResmiMi, 
                AraToplam = fatura.AraToplam ?? 0m,
                KdvTutari = fatura.KDVToplam ?? 0m,
                // İskonto tutarını kontrol et - entity'de var
                IndirimTutari = fatura.IndirimTutari ?? 0m, 
                GenelToplam = fatura.GenelToplam ?? 0m,
                OdenecekTutar = odenecekTutar,
                OdenenTutar = odenenTutar,
                // Ödeme durumunu hesapla
                OdemeDurumu = odenenTutar == 0 ? "Bekliyor" : 
                              odenenTutar == fatura.GenelToplam.GetValueOrDefault() ? "Ödendi" : 
                              odenenTutar > 0 && odenenTutar < fatura.GenelToplam.GetValueOrDefault() ? "Kısmi Ödendi" : "Bekliyor",
                Aciklama = fatura.FaturaNotu,
                DovizTuru = fatura.DovizTuru ?? "TRY",
                DovizKuru = fatura.DovizKuru ?? 1m,
                Aktif = fatura.Aktif,
                OlusturmaTarihi = fatura.OlusturmaTarihi,
                GuncellemeTarihi = fatura.GuncellemeTarihi,

                // Fatura kalemleri
                FaturaKalemleri = fatura.FaturaDetaylari.Select(fd => new FaturaKalemDetailViewModel
                {
                    KalemID = fd.FaturaDetayID,
                    UrunID = fd.UrunID,
                    UrunKodu = fd.Urun?.UrunKodu ?? "",
                    UrunAdi = fd.Urun?.UrunAdi ?? "Belirtilmemiş",
                    Miktar = fd.Miktar,
                    BirimFiyat = fd.BirimFiyat,
                    KdvOrani = (int)fd.KdvOrani,
                    IndirimOrani = (int)fd.IndirimOrani,
                    Tutar = fd.Tutar ?? 0m,
                    KdvTutari = fd.KdvTutari ?? 0m,
                    IndirimTutari = fd.IndirimTutari ?? 0m,
                    NetTutar = fd.NetTutar ?? 0m,
                    Birim = fd.Urun?.Birim?.BirimAdi ?? "Adet"
                }).OrderBy(fk => fk.UrunAdi).ToList(),
                
                // Ödemeler
                Odemeler = faturanınOdemeleri.Select(o => new OdemeViewModel
                {
                    OdemeID = o.OdemeID,
                    OdemeTarihi = o.OdemeTarihi,
                    OdemeTutari = o.OdemeTutari,
                    OdemeTuru = o.OdemeTuru,
                    Aciklama = o.Aciklama
                }).ToList()
            };

            return View(viewModel);
        }

        // GET: Fatura/Edit/5
        public async Task<IActionResult> Edit(Guid? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var fatura = await _context.Faturalar
                .Include(f => f.Cari)
                .Include(f => f.FaturaTuru)
                .Include(f => f.FaturaDetaylari)
                    .ThenInclude(fd => fd.Urun)
                .FirstOrDefaultAsync(f => f.FaturaID == id);

            if (fatura == null)
            {
                return NotFound();
            }

            var viewModel = new FaturaEditViewModel
            {
                FaturaID = fatura.FaturaID,
                FaturaNumarasi = fatura.FaturaNumarasi,
                SiparisNumarasi = fatura.SiparisNumarasi,
                FaturaTarihi = fatura.FaturaTarihi,
                VadeTarihi = fatura.VadeTarihi,
                CariID = fatura.CariID ?? Guid.Empty,
                CariAdi = fatura.Cari?.Ad ?? "",
                FaturaTuruID = fatura.FaturaTuruID,
                FaturaTuru = fatura.FaturaTuru?.FaturaTuruAdi ?? "",
                ResmiMi = fatura.ResmiMi,
                Aciklama = fatura.FaturaNotu,
                OdemeDurumu = fatura.OdemeDurumu,
                DovizTuru = fatura.DovizTuru,
                DovizKuru = fatura.DovizKuru ?? 1m,
                IrsaliyeID = fatura.Irsaliyeler?.FirstOrDefault()?.IrsaliyeID,
                FaturaKalemleri = fatura.FaturaDetaylari.Select(fd => new FaturaKalemViewModel
                {
                    FaturaKalemID = fd.FaturaDetayID,
                    UrunID = fd.UrunID,
                    UrunKodu = fd.Urun?.UrunKodu ?? "",
                    UrunAdi = fd.Urun?.UrunAdi ?? "",
                    Miktar = fd.Miktar,
                    BirimFiyat = fd.BirimFiyat,
                    KdvOrani = fd.KdvOrani,
                    IndirimOrani = fd.IndirimOrani,
                    Tutar = fd.Tutar ?? 0,
                    KdvTutari = fd.KdvTutari ?? 0,
                    IndirimTutari = fd.IndirimTutari ?? 0,
                    NetTutar = fd.NetTutar ?? 0,
                    Birim = fd.Birim ?? "Adet"
                }).ToList(),
                CariListesi = await _context.Cariler
                    .Where(c => !c.Silindi)
                    .Select(c => new SelectListItem { Value = c.CariID.ToString(), Text = c.Ad })
                    .ToListAsync(),
                FaturaTuruListesi = await _context.FaturaTurleri
                    .Where(ft => !ft.Silindi)
                    .Select(ft => new SelectListItem { Value = ft.FaturaTuruID.ToString(), Text = ft.FaturaTuruAdi })
                    .ToListAsync(),
                DovizListesi = await _context.ParaBirimleri
                    .Where(pb => !pb.Silindi)
                    .Select(pb => new SelectListItem { Value = pb.Kod, Text = pb.Ad })
                    .ToListAsync()
            };

            if (fatura.Cari != null && fatura.Cari.Silindi)
            {
                ViewBag.CariSilindi = true;
            }

            // Aktif ürünler
            var urunler = await _context.Urunler
                .Include(u => u.Birim)
                .Where(u => !u.Silindi && u.Aktif)
                .OrderBy(u => u.UrunAdi)
                .ToListAsync();

            // Ürünler için hem birim bilgisini hem de ID'yi taşıyacak özel bir SelectList
            ViewBag.Urunler = urunler.Select(u => new SelectListItem
            {
                Value = u.UrunID.ToString(),
                Text = u.UrunAdi,
                Group = new SelectListGroup { Name = u.KategoriID.HasValue ? u.Kategori?.KategoriAdi ?? "Genel" : "Genel" }
            }).ToList();
            
            // Birim bilgisini farklı bir yöntemle taşıyalım, örneğin ViewBag ile
            ViewBag.UrunBirimBilgileri = urunler.ToDictionary(
                u => u.UrunID.ToString(), 
                u => u.Birim?.BirimAdi ?? "Adet"
            );

            ViewBag.Cariler = new SelectList(await _context.Cariler.Where(c => !c.Silindi && c.AktifMi).ToListAsync(), "CariID", "Ad");
            ViewBag.FaturaTurleri = new SelectList(await _context.FaturaTurleri.ToListAsync(), "FaturaTuruID", "FaturaTuruAdi");
            ViewBag.Sozlesmeler = new SelectList(await _context.Sozlesmeler.Where(s => !s.Silindi && s.AktifMi).ToListAsync(), "SozlesmeID", "SozlesmeNo");

            return View(viewModel);
        }

        // POST: Fatura/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(Guid id, FaturaEditViewModel viewModel)
        {
            if (id != viewModel.FaturaID)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    // Model validasyonu
                    var validationResult = _faturaValidationService.ValidateFaturaEditViewModel(viewModel);
                    if (!validationResult.IsValid)
                    {
                        ModelState.AddModelError(string.Empty, validationResult.ErrorMessage);
                        await PopulateEditViewData(viewModel);
                        return View(viewModel);
                    }

                    // Mevcut kullanıcıyı al
                    var currentUser = await _userManager.GetUserAsync(User);
                    var currentUserId = currentUser?.Id != null ? Guid.Parse(currentUser.Id) : (Guid?)null;

                    // Fatura güncelle (StokHareket, CariHareket, Irsaliye dahil)
                    var faturaId = await _faturaOrchestrationService.UpdateFaturaWithRelations(id, viewModel, currentUserId);

                    // İşlem başarılı
                    _logger.LogInformation($"Fatura başarıyla güncellendi. FaturaID: {faturaId}");
                    TempData["SuccessMessage"] = "Fatura başarıyla güncellendi.";
                    return RedirectToAction(nameof(Details), new { id = faturaId });
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Fatura güncelleme hatası");
                    ModelState.AddModelError(string.Empty, $"Fatura güncellenirken bir hata oluştu: {ex.Message}");
                }
            }

            // Form geçerli değilse veya hata oluştuysa, dropdown listelerini yeniden doldur
            await PopulateEditViewData(viewModel);
            return View(viewModel);
        }

        /// <summary>
        /// Fatura düzenleme formundaki dropdown ve diğer verileri doldurur
        /// </summary>
        private async Task PopulateEditViewData(FaturaEditViewModel viewModel)
        {
            // ViewModel için dropdown verilerini doldur
            ViewBag.Cariler = new SelectList(await _context.Cariler.Where(c => !c.Silindi && c.AktifMi).ToListAsync(), "CariID", "Ad", viewModel.CariID);
            ViewBag.FaturaTurleri = new SelectList(await _context.FaturaTurleri.ToListAsync(), "FaturaTuruID", "FaturaTuruAdi", viewModel.FaturaTuruID);
            ViewBag.OdemeTurleri = new SelectList(await _context.OdemeTurleri.ToListAsync(), "OdemeTuruID", "OdemeTuruAdi");
            ViewBag.Urunler = new SelectList(await _context.Urunler.Where(u => !u.Silindi && u.Aktif).ToListAsync(), "UrunID", "UrunAdi");
            ViewBag.Sozlesmeler = new SelectList(await _context.Sozlesmeler.Where(s => !s.Silindi && s.AktifMi).ToListAsync(), "SozlesmeID", "SozlesmeNo");
            
            // Döviz listesi
            viewModel.DovizListesi = new List<SelectListItem>
            {
                new SelectListItem { Value = "USD", Text = "Amerikan Doları (USD)", Selected = viewModel.DovizTuru == "USD" },
                new SelectListItem { Value = "EUR", Text = "Euro (EUR)", Selected = viewModel.DovizTuru == "EUR" },
                new SelectListItem { Value = "TRY", Text = "Türk Lirası (TRY)", Selected = viewModel.DovizTuru == "TRY" },
                new SelectListItem { Value = "GBP", Text = "İngiliz Sterlini (GBP)", Selected = viewModel.DovizTuru == "GBP" }
            };
        }

        // GET: Fatura/Delete/5
        public async Task<IActionResult> Delete(Guid? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var fatura = await _context.Faturalar
                .Include(f => f.Cari)
                .Include(f => f.FaturaTuru)
                .FirstOrDefaultAsync(m => m.FaturaID == id && !m.Silindi);

            if (fatura == null)
            {
                return NotFound();
            }

            return View(fatura);
        }

        // POST: Fatura/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(Guid id)
        {
            try
            {
                // Mevcut kullanıcıyı al
                var currentUser = await _userManager.GetUserAsync(User);
                var currentUserId = currentUser?.Id != null ? Guid.Parse(currentUser.Id) : (Guid?)null;

                // Faturayı sil (StokHareket, CariHareket, ilişkili FIFO kayıtları dahil)
                var result = await _faturaOrchestrationService.DeleteFaturaWithRelations(id, currentUserId);

                if (result)
                {
                    _logger.LogInformation($"Fatura başarıyla silindi. FaturaID: {id}");
                    TempData["SuccessMessage"] = "Fatura başarıyla silindi.";
                    return RedirectToAction(nameof(Index));
                }
                else
                {
                    _logger.LogWarning($"Fatura silinirken sorun oluştu. FaturaID: {id}");
                    TempData["ErrorMessage"] = "Fatura silinirken sorun oluştu.";
                    return RedirectToAction(nameof(Details), new { id });
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Fatura silme hatası. FaturaID: {id}");
                TempData["ErrorMessage"] = $"Fatura silinirken bir hata oluştu: {ex.Message}";
                return RedirectToAction(nameof(Details), new { id });
            }
        }

        // GET: Fatura/Print/5
        public async Task<IActionResult> Print(Guid? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var fatura = await _context.Faturalar
                .Include(f => f.Cari)
                .Include(f => f.FaturaTuru)
                .Include(f => f.FaturaDetaylari)
                .ThenInclude(fd => fd.Urun)
                .FirstOrDefaultAsync(m => m.FaturaID == id && !m.Silindi);

            if (fatura == null)
            {
                return NotFound();
            }

#pragma warning disable CS8601 // Possible null reference assignment.
            var viewModel = new FaturaDetailViewModel
            {
                FaturaID = fatura.FaturaID,
                FaturaNumarasi = fatura.FaturaNumarasi,
                FaturaTarihi = fatura.FaturaTarihi,
                VadeTarihi = fatura.VadeTarihi,
                CariID = fatura.CariID != null ? fatura.CariID.Value : Guid.Empty,
                CariAdi = fatura.Cari?.Ad ?? "Belirtilmemiş",
                CariVergiNo = fatura.Cari?.VergiNo ?? "Belirtilmemiş",
                CariAdres = fatura.Cari?.Adres ?? "Belirtilmemiş",
                CariTelefon = fatura.Cari?.Telefon ?? "Belirtilmemiş",
                FaturaTuru = fatura.FaturaTuru?.FaturaTuruAdi ?? "Belirtilmemiş",
                AraToplam = fatura.AraToplam ?? 0,
                KdvTutari = fatura.KDVToplam ?? 0,
                GenelToplam = fatura.GenelToplam ?? 0,
                OdemeDurumu = fatura.OdemeDurumu ?? "Belirtilmemiş",
                Aciklama = fatura.FaturaNotu ?? "",
                DovizTuru = fatura.DovizTuru ?? "TRY",
                FaturaKalemleri = fatura.FaturaDetaylari.Select(fd => new FaturaKalemDetailViewModel
                {
                    KalemID = fd.FaturaDetayID,
                    UrunID = fd.UrunID,
                    UrunAdi = fd.Urun?.UrunAdi ?? "Belirtilmemiş",
                    Miktar = fd.Miktar,
                    BirimFiyat = fd.BirimFiyat,
                    KdvOrani = (int)fd.KdvOrani,
                    IndirimOrani = (int)fd.IndirimOrani,
                    Tutar = fd.SatirToplam ?? 0,
                    KdvTutari = fd.KdvTutari ?? 0m,
                    IndirimTutari = fd.IndirimTutari ?? 0m,
                    NetTutar = fd.NetTutar ?? 0m,
                    Birim = fd.Birim
                }).ToList()
            };
#pragma warning restore CS8601 // Possible null reference assignment.

            return View(viewModel);
        }

        // GET: Fatura/Aklama
        public async Task<IActionResult> Aklama(Guid? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            try
            {
#pragma warning disable CS8602 // Dereference of a possibly null reference.
                var fatura = await _context.Faturalar
                    .Include(f => f.Cari)
                    .Include(f => f.FaturaDetaylari)
                        .ThenInclude(fd => fd.Urun)
                            .ThenInclude(u => u.Birim)
                    .AsSplitQuery()
                    .FirstOrDefaultAsync(f => f.FaturaID == id);
#pragma warning restore CS8602 // Dereference of a possibly null reference.

                if (fatura == null)
                {
                    TempData["ErrorMessage"] = "Fatura bulunamadı.";
                    return RedirectToAction(nameof(Index));
                }

                // Kullanılabilir sözleşmeleri al
                var sozlesmeler = await _context.Sozlesmeler
                    .Where(s => !s.Silindi && s.AktifMi && s.CariID == fatura.CariID)
                    .OrderByDescending(s => s.SozlesmeTarihi)
                    .ToListAsync();

#pragma warning disable CS8601 // Possible null reference assignment.
                var viewModel = new MuhasebeStokWebApp.Models.ViewModels.FaturaAklamaViewModel
                {
                    FaturaID = fatura.FaturaID,
                    FaturaNumarasi = fatura.FaturaNumarasi,
                    FaturaTarihi = fatura.FaturaTarihi,
                    CariAdi = fatura.Cari?.Ad,
                    AraToplam = fatura.AraToplam,
                    KDVToplam = fatura.KDVToplam,
                    GenelToplam = fatura.GenelToplam,
                    DovizTuru = fatura.DovizTuru ?? "TRY",
                    DovizKuru = fatura.DovizKuru,
                    ResmiMi = fatura.ResmiMi,
                    OdemeDurumu = fatura.OdemeDurumu,
                    KullanilabilirSozlesmeler = sozlesmeler,
                    FaturaKalemleri = new List<MuhasebeStokWebApp.Models.ViewModels.FaturaKalemAklamaViewModel>()
                };
#pragma warning restore CS8601 // Possible null reference assignment.

                // Fatura kalemlerini ekleyelim
                foreach (var fd in fatura.FaturaDetaylari.Where(fd => !fd.Silindi))
                {
#pragma warning disable CS8601 // Possible null reference assignment.
#pragma warning disable CS8601 // Possible null reference assignment.
#pragma warning disable CS8601 // Possible null reference assignment.
                    var kalem = new MuhasebeStokWebApp.Models.ViewModels.FaturaKalemAklamaViewModel
                    {
                        FaturaKalemID = fd.FaturaDetayID,
                        UrunKodu = fd.Urun?.UrunKodu,
                        UrunAdi = fd.Urun?.UrunAdi,
                        BirimAdi = fd.Urun?.Birim?.BirimAdi,
                        Miktar = fd.Miktar,
                        BirimFiyat = fd.BirimFiyat,
                        ToplamTutar = fd.NetTutar ?? (fd.Miktar * fd.BirimFiyat)
                    };
#pragma warning restore CS8601 // Possible null reference assignment.
#pragma warning restore CS8601 // Possible null reference assignment.
#pragma warning restore CS8601 // Possible null reference assignment.
                    
                    viewModel.FaturaKalemleri.Add(kalem);
                }

                return View(viewModel);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Fatura aklama sayfası yüklenirken hata oluştu. FaturaID: {FaturaID}", id);
                TempData["ErrorMessage"] = "Fatura aklama sayfası yüklenirken bir hata oluştu: " + ex.Message;
                return RedirectToAction(nameof(Index));
            }
        }

        // POST: Fatura/Aklama
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Aklama(MuhasebeStokWebApp.Models.ViewModels.FaturaAklamaViewModel viewModel)
        {
            if (!ModelState.IsValid)
            {
                TempData["ErrorMessage"] = "Formda hatalar mevcut, lütfen kontrol ediniz.";
                return View(viewModel);
            }

            try
            {
                // Seçilen sözleşme kontrolü
                if (viewModel.SecilenSozlesmeID == null)
                {
                    TempData["ErrorMessage"] = "Lütfen bir sözleşme seçiniz.";
                    
                    // Sozleşme listesini yeniden yükle
                    var fatura = await _context.Faturalar.Include(f => f.Cari).FirstOrDefaultAsync(f => f.FaturaID == viewModel.FaturaID);
                    if (fatura?.CariID != null)
                    {
                        viewModel.KullanilabilirSozlesmeler = await _context.Sozlesmeler
                            .Where(s => !s.Silindi && s.AktifMi && s.CariID == fatura.CariID)
                            .OrderByDescending(s => s.SozlesmeTarihi)
                            .ToListAsync();
                    }
                    else
                    {
                        viewModel.KullanilabilirSozlesmeler = new List<Sozlesme>();
                    }
                    
                    return View(viewModel);
                }

                _logger.LogInformation("Fatura aklama işlemi başlatıldı. FaturaID: {FaturaID}, Sözleşme: {SozlesmeID}", 
                    viewModel.FaturaID, viewModel.SecilenSozlesmeID);

                // Burada aklama işlemleri gerçekleştirilecek
                // Örneğin: Aklama veritabanına kayıt, fatura durumu güncelleme, vs.

                TempData["SuccessMessage"] = "Fatura başarıyla aklandı.";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Fatura aklama işlemi sırasında hata oluştu. FaturaID: {FaturaID}", viewModel.FaturaID);
                TempData["ErrorMessage"] = "Fatura aklama işlemi sırasında bir hata oluştu: " + ex.Message;
                return View(viewModel);
            }
        }

        // Otomatik irsaliye oluşturma metodu
        private async Task<Guid> OtomatikIrsaliyeOlustur(Data.Entities.Fatura fatura, Guid? depoID = null)
        {
            try
            {
                // Faturayı detaylarıyla birlikte tekrar çek - FaturaTuru ilişkisini ekledik
                var faturaWithDetails = await _context.Faturalar
                    .Include(f => f.FaturaTuru)
                    .Include(f => f.FaturaDetaylari)
                        .ThenInclude(fd => fd.Urun)
                    .Include(f => f.Cari)
                    .FirstOrDefaultAsync(f => f.FaturaID == fatura.FaturaID);

                if (faturaWithDetails != null && faturaWithDetails.FaturaDetaylari != null && faturaWithDetails.FaturaDetaylari.Any())
                {
                    // Fatura türüne göre irsaliye türünü belirle
                    string irsaliyeTuru;
                    if (faturaWithDetails.FaturaTuru == null)
                    {
                        _logger.LogWarning($"Fatura türü bulunamadı. Varsayılan olarak 'Çıkış İrsaliyesi' kullanılıyor. FaturaID: {fatura.FaturaID}");
                        irsaliyeTuru = "Çıkış İrsaliyesi"; // Varsayılan değer
                    }
                    else
                    {
                        irsaliyeTuru = faturaWithDetails.FaturaTuru.HareketTuru == "Giriş" 
                            ? "Giriş İrsaliyesi" 
                            : "Çıkış İrsaliyesi";
                    }

                    // Yeni irsaliye numarası oluştur
                    var today = DateTime.Now;
                    var year = today.Year.ToString().Substring(2);
                    var month = today.Month.ToString().PadLeft(2, '0');
                    var day = today.Day.ToString().PadLeft(2, '0');
                    var prefix = $"IRS-{year}{month}{day}-";

                    // Son irsaliye numarasını bulup arttır
                    var lastIrsaliye = await _context.Irsaliyeler
                        .Where(i => i.IrsaliyeNumarasi != null && i.IrsaliyeNumarasi.StartsWith(prefix))
                        .OrderByDescending(i => i.IrsaliyeNumarasi)
                        .FirstOrDefaultAsync();

                    int sequence = 1;
                    if (lastIrsaliye != null && lastIrsaliye.IrsaliyeNumarasi != null)
                    {
                        var parts = lastIrsaliye.IrsaliyeNumarasi.Split('-');
                        if (parts.Length == 3 && int.TryParse(parts[2], out int lastSeq))
                        {
                            sequence = lastSeq + 1;
                        }
                    }

                    var irsaliyeNumarasi = $"{prefix}{sequence.ToString().PadLeft(3, '0')}";

                    // CariID kontrol et ve geçerli bir ID olduğundan emin ol
                    var cariID = faturaWithDetails.CariID ?? Guid.Empty;
                    if (cariID == Guid.Empty)
                    {
                        _logger.LogWarning($"Faturada geçerli bir cari bulunamadı. FaturaID: {fatura.FaturaID}");
                    }

                    // Yeni irsaliye oluştur
                    var irsaliye = new Irsaliye
                    {
                        IrsaliyeID = Guid.NewGuid(),
                        IrsaliyeNumarasi = irsaliyeNumarasi,
                        IrsaliyeTarihi = faturaWithDetails.FaturaTarihi ?? DateTime.Now,
                        CariID = cariID,
                        FaturaID = faturaWithDetails.FaturaID,
                        DepoID = depoID,
                        IrsaliyeTuru = irsaliyeTuru,
                        Aciklama = $"{faturaWithDetails.FaturaNumarasi ?? ""} numaralı faturaya ait otomatik oluşturulan {irsaliyeTuru} irsaliyesi",
                        OlusturmaTarihi = DateTime.Now,
                        OlusturanKullaniciId = GetCurrentUserId().GetValueOrDefault(Guid.Empty),
                        Aktif = true,
                        Silindi = false,
                        Durum = "Açık" // Durumu açık olarak ayarla
                    };

                    _context.Irsaliyeler.Add(irsaliye);

                    // Fatura kalemlerini irsaliye kalemlerine dönüştür
                    if (faturaWithDetails.FaturaDetaylari != null && faturaWithDetails.FaturaDetaylari.Any())
                    {
                        foreach (var detay in faturaWithDetails.FaturaDetaylari)
                        {
                            if (detay.Urun != null)
                            {
                                var irsaliyeDetay = new IrsaliyeDetay
                                {
                                    IrsaliyeDetayID = Guid.NewGuid(),
                                    IrsaliyeID = irsaliye.IrsaliyeID,
                                    UrunID = detay.UrunID,
                                    DepoID = depoID,
                                    Miktar = detay.Miktar,
                                    BirimFiyat = detay.BirimFiyat,
                                    KdvOrani = detay.KdvOrani,
                                    IndirimOrani = detay.IndirimOrani,
                                    Birim = detay.Birim ?? "Adet",
                                    Aciklama = detay.Aciklama ?? "",
                                    OlusturmaTarihi = DateTime.Now,
                                    SatirToplam = detay.SatirToplam ?? 0m,
                                    SatirKdvToplam = detay.SatirKdvToplam ?? 0m,
                                    Aktif = true,
                                    Silindi = false
                                };

                                _context.IrsaliyeDetaylari.Add(irsaliyeDetay);
                            }
                        }
                    }

                    try
                    {
                        await _context.SaveChangesAsync();
                        _logger.LogInformation($"Fatura için otomatik irsaliye oluşturuldu: IrsaliyeNo={irsaliyeNumarasi}, FaturaNo={faturaWithDetails.FaturaNumarasi}");
                        
                        // İrsaliye oluşturulduktan sonra, faturaya bağlı stok hareketlerinin IrsaliyeID alanını güncelle
                        var stokHareketleri = await _context.StokHareketleri
                            .Where(sh => sh.FaturaID == faturaWithDetails.FaturaID && !sh.Silindi)
                            .ToListAsync();
                            
                        if (stokHareketleri != null && stokHareketleri.Any())
                        {
                            foreach (var hareket in stokHareketleri)
                            {
                                hareket.IrsaliyeID = irsaliye.IrsaliyeID;
                                hareket.IrsaliyeTuru = irsaliyeTuru;
                                _context.StokHareketleri.Update(hareket);
                            }
                            
                            try {
                                await _context.SaveChangesAsync();
                                _logger.LogInformation($"{stokHareketleri.Count} adet stok hareketinin IrsaliyeID alanı güncellendi. IrsaliyeID: {irsaliye.IrsaliyeID}");
                            }
                            catch (Exception stokEx) {
                                _logger.LogError(stokEx, $"Stok hareketleri güncellenirken hata oluştu: {stokEx.Message}");
                                // Sadece log tutup devam ediyoruz, irsaliye oluşturuldu
                            }
                        }
                        
                        return irsaliye.IrsaliyeID;
                    }
                    catch (Exception saveEx)
                    {
                        _logger.LogError(saveEx, $"İrsaliye detayları kaydedilirken hata oluştu: {saveEx.Message}");
                        throw;
                    }
                }
                else
                {
                    _logger.LogWarning($"İrsaliye oluşturmak için fatura detayları bulunamadı. FaturaID: {fatura.FaturaID}");
                    throw new Exception($"İrsaliye oluşturmak için fatura veya detayları bulunamadı. FaturaID: {fatura.FaturaID}");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Otomatik irsaliye oluşturulurken hata oluştu: {ex.Message}");
                throw;
            }
        }

        // AJAX isteği ile kur bilgisi getir
        [HttpGet]
        public async Task<IActionResult> GetKurBilgisi(string dovizKodu, DateTime? tarih = null)
        {
            try
            {
                if (dovizKodu == "TRY")
                {
                    return Json(new { success = true, kurDegeri = 1 });
                }

                // Eğer tarih belirtilmemişse bugünün tarihi
                var arananTarih = tarih ?? DateTime.Today;
                
                // Önce para birimini bul
                var paraBirimi = await _context.ParaBirimleri
                    .FirstOrDefaultAsync(p => p.Kod == dovizKodu && !p.Silindi && p.Aktif);
                
                if (paraBirimi == null)
                {
                    return Json(new { 
                        success = false, 
                        kurDegeri = 0, 
                        message = $"{dovizKodu} para birimi bulunamadı. Lütfen para birimini kontrol ediniz."
                    });
                }
                
                // İstenen tarih için kur bilgisini ara
                var kurDegeri = await _context.KurDegerleri
                    .Where(k => k.ParaBirimiID == paraBirimi.ParaBirimiID && k.Tarih.Date == arananTarih.Date && !k.Silindi && k.Aktif)
                    .OrderByDescending(k => k.Tarih)
                    .FirstOrDefaultAsync();

                if (kurDegeri != null)
                {
                    return Json(new { 
                        success = true, 
                        kurDegeri = kurDegeri.Satis,
                        message = $"{arananTarih:dd.MM.yyyy} tarihli kur bilgisi kullanılıyor."
                    });
                }
                
                // İstenen tarihte kur bulunamadıysa, önceki en yakın tarihi bul
                var enYakinKur = await _context.KurDegerleri
                    .Where(k => k.ParaBirimiID == paraBirimi.ParaBirimiID && k.Tarih.Date < arananTarih.Date && !k.Silindi && k.Aktif)
                    .OrderByDescending(k => k.Tarih)
                    .FirstOrDefaultAsync();
                
                if (enYakinKur != null)
                {
                    return Json(new { 
                        success = true, 
                        kurDegeri = enYakinKur.Satis,
                        message = $"{arananTarih:dd.MM.yyyy} tarihli kur bulunamadı. {enYakinKur.Tarih:dd.MM.yyyy} tarihli kur bilgisi kullanılıyor."
                    });
                }

                // Hiç kur bulunamadıysa
                return Json(new { 
                    success = false, 
                    kurDegeri = 0, 
                    message = $"{dovizKodu} para birimi için kur bilgisi bulunamadı. Lütfen manuel olarak giriniz."
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Kur bilgisi getirilirken hata oluştu: {Message}", ex.Message);
                return Json(new { 
                    success = false, 
                    message = "Kur bilgisi getirilirken bir hata oluştu." 
                });
            }
        }

        // Yeni Guid parametreli metod
        private async Task<Guid> OtomatikIrsaliyeOlusturFromID(Guid faturaID, Guid? depoID = null)
        {
            try
            {
                // Faturayı detayları ile birlikte yükle
                var faturaWithDetails = await _context.Faturalar
                    .Include(f => f.FaturaTuru)
                    .Include(f => f.FaturaDetaylari)
                        .ThenInclude(fd => fd.Urun)
                    .FirstOrDefaultAsync(f => f.FaturaID == faturaID);

                if (faturaWithDetails != null && faturaWithDetails.FaturaDetaylari != null && faturaWithDetails.FaturaDetaylari.Any())
                {
                    // İrsaliye türünü belirle
                    string irsaliyeTuru;
                    if (faturaWithDetails.FaturaTuru == null)
                    {
                        _logger.LogWarning($"Fatura türü bulunamadı. Varsayılan olarak 'Çıkış İrsaliyesi' kullanılıyor. FaturaID: {faturaID}");
                        irsaliyeTuru = "Çıkış İrsaliyesi"; // Varsayılan değer
                    }
                    else
                    {
                        irsaliyeTuru = faturaWithDetails.FaturaTuru.HareketTuru == "Giriş" 
                            ? "Giriş İrsaliyesi" 
                            : "Çıkış İrsaliyesi";
                    }
                    
                    // Otomatik irsaliye numarası oluştur (Prefix + Tarih + Sıra No)
                    var today = DateTime.Now;
                    var year = today.Year.ToString().Substring(2);
                    var month = today.Month.ToString().PadLeft(2, '0');
                    var day = today.Day.ToString().PadLeft(2, '0');
                    var prefix = $"IRS-{year}{month}{day}-";
                    
                    // Son irsaliyeyi bul
                    var lastIrsaliye = await _context.Irsaliyeler
                        .Where(i => i.IrsaliyeNumarasi != null && i.IrsaliyeNumarasi.StartsWith(prefix))
                        .OrderByDescending(i => i.IrsaliyeNumarasi)
                        .FirstOrDefaultAsync();

                    int sequence = 1;
                    if (lastIrsaliye != null && lastIrsaliye.IrsaliyeNumarasi != null)
                    {
                        var parts = lastIrsaliye.IrsaliyeNumarasi.Split('-');
                        if (parts.Length == 3 && int.TryParse(parts[2], out int lastSeq))
                        {
                            sequence = lastSeq + 1;
                        }
                    }

                    var irsaliyeNumarasi = $"{prefix}{sequence.ToString().PadLeft(3, '0')}";

                    // CariID kontrol et ve geçerli bir ID olduğundan emin ol
                    var cariID = faturaWithDetails.CariID ?? Guid.Empty;
                    if (cariID == Guid.Empty)
                    {
                        _logger.LogWarning($"Faturada geçerli bir cari bulunamadı. FaturaID: {faturaID}");
                    }

                    // Yeni irsaliye oluştur
                    var irsaliye = new Irsaliye
                    {
                        IrsaliyeID = Guid.NewGuid(),
                        IrsaliyeNumarasi = irsaliyeNumarasi,
                        IrsaliyeTarihi = faturaWithDetails.FaturaTarihi ?? DateTime.Now,
                        CariID = cariID,
                        FaturaID = faturaWithDetails.FaturaID,
                        DepoID = depoID,
                        IrsaliyeTuru = irsaliyeTuru,
                        Aciklama = $"{faturaWithDetails.FaturaNumarasi ?? ""} numaralı faturaya ait otomatik oluşturulan {irsaliyeTuru} irsaliyesi",
                        OlusturmaTarihi = DateTime.Now,
                        OlusturanKullaniciId = GetCurrentUserId().GetValueOrDefault(Guid.Empty),
                        Aktif = true,
                        Silindi = false,
                        Durum = "Açık" // Durumu açık olarak ayarla
                    };

                    _context.Irsaliyeler.Add(irsaliye);

                    // Fatura kalemlerini irsaliye kalemlerine dönüştür
                    if (faturaWithDetails.FaturaDetaylari != null && faturaWithDetails.FaturaDetaylari.Any())
                    {
                        foreach (var detay in faturaWithDetails.FaturaDetaylari)
                        {
                            if (detay.Urun != null)
                            {
                                var irsaliyeDetay = new IrsaliyeDetay
                                {
                                    IrsaliyeDetayID = Guid.NewGuid(),
                                    IrsaliyeID = irsaliye.IrsaliyeID,
                                    UrunID = detay.UrunID,
                                    DepoID = depoID,
                                    Miktar = detay.Miktar,
                                    BirimFiyat = detay.BirimFiyat,
                                    KdvOrani = detay.KdvOrani,
                                    IndirimOrani = detay.IndirimOrani,
                                    Birim = detay.Birim ?? "Adet",
                                    Aciklama = detay.Aciklama ?? "",
                                    OlusturmaTarihi = DateTime.Now,
                                    SatirToplam = detay.SatirToplam ?? 0m,
                                    SatirKdvToplam = detay.SatirKdvToplam ?? 0m,
                                    Aktif = true,
                                    Silindi = false
                                };

                                _context.IrsaliyeDetaylari.Add(irsaliyeDetay);
                            }
                        }
                    }

                    try
                    {
                        await _context.SaveChangesAsync();
                        _logger.LogInformation($"Fatura için otomatik irsaliye oluşturuldu: IrsaliyeNo={irsaliyeNumarasi}, FaturaNo={faturaWithDetails.FaturaNumarasi}");
                        
                        // İrsaliye oluşturulduktan sonra, faturaya bağlı stok hareketlerinin IrsaliyeID alanını güncelle
                        var stokHareketleri = await _context.StokHareketleri
                            .Where(sh => sh.FaturaID == faturaWithDetails.FaturaID && !sh.Silindi)
                            .ToListAsync();
                            
                        if (stokHareketleri != null && stokHareketleri.Any())
                        {
                            foreach (var hareket in stokHareketleri)
                            {
                                hareket.IrsaliyeID = irsaliye.IrsaliyeID;
                                hareket.IrsaliyeTuru = irsaliyeTuru;
                                _context.StokHareketleri.Update(hareket);
                            }
                            
                            try {
                                await _context.SaveChangesAsync();
                                _logger.LogInformation($"{stokHareketleri.Count} adet stok hareketinin IrsaliyeID alanı güncellendi. IrsaliyeID: {irsaliye.IrsaliyeID}");
                            }
                            catch (Exception stokEx) {
                                _logger.LogError(stokEx, $"Stok hareketleri güncellenirken hata oluştu: {stokEx.Message}");
                                // Sadece log tutup devam ediyoruz, irsaliye oluşturuldu
                            }
                        }
                        
                        return irsaliye.IrsaliyeID;
                    }
                    catch (Exception saveEx)
                    {
                        _logger.LogError(saveEx, $"İrsaliye detayları kaydedilirken hata oluştu: {saveEx.Message}");
                        throw;
                    }
                }
                else
                {
                    _logger.LogWarning($"İrsaliye oluşturmak için fatura detayları bulunamadı. FaturaID: {faturaID}");
                    throw new Exception($"İrsaliye oluşturmak için fatura veya detayları bulunamadı. FaturaID: {faturaID}");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Otomatik irsaliye oluşturulurken hata oluştu: {ex.Message}");
                throw;
            }
        }

        // Raporlar için yetki kontrolü eklenecek
        [Authorize(Roles = "Admin,FinansYonetici,Rapor")]
        public async Task<IActionResult> Raporlar()
        {
            _logger.LogInformation($"Fatura raporları erişimi: Kullanıcı={User.Identity.Name}");
            return View();
        }
    }
} 