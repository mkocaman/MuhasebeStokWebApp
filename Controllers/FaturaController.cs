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
        private readonly IFaturaNumaralandirmaService _faturaNumaralandirmaService;

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
            IFaturaValidationService faturaValidationService,
            IFaturaNumaralandirmaService faturaNumaralandirmaService)
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
            _faturaNumaralandirmaService = faturaNumaralandirmaService;
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
                
                // ViewModel oluştur
                var viewModel = new FaturaCreateViewModel
                {
                    FaturaTarihi = DateTime.Now,
                    VadeTarihi = DateTime.Now.AddDays(30),
                    DovizTuru = "USD",
                    DovizKuru = 1,
                    FaturaKalemleri = new List<FaturaKalemViewModel>(),
                    OtomatikIrsaliyeOlustur = true
                };

                // Otomatik fatura numarası oluştur (servis kullanarak)
                viewModel.FaturaNumarasi = await _faturaNumaralandirmaService.GenerateFaturaNumarasiAsync();

                // Otomatik sipariş numarası oluştur (servis kullanarak)
                viewModel.SiparisNumarasi = await _faturaNumaralandirmaService.GenerateSiparisNumarasiAsync();
                
                // Gerekli formları yükle
                await PopulateFaturaCreateViewModelAsync(viewModel);
                
                return View(viewModel);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Fatura oluşturma sayfası yüklenirken hata oluştu.");
                TempData["Error"] = "Fatura oluşturma sayfası yüklenirken bir hata oluştu: " + ex.Message;
                return RedirectToAction(nameof(Index));
            }
        }

        /// <summary>
        /// FaturaCreateViewModel için gerekli form verilerini yükler
        /// </summary>
        private async Task PopulateFaturaCreateViewModelAsync(FaturaCreateViewModel viewModel)
        {
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

            // Ürün listesi boş mu
            viewModel.UrunListesiBosMu = urunler.Count == 0;
            
            if (!viewModel.UrunListesiBosMu)
            {
                // Ürünler için SelectList oluştur
                viewModel.Urunler = urunler.Select(u => new SelectListItem
                {
                    Value = u.UrunID.ToString(),
                    Text = u.UrunAdi,
                    Group = new SelectListGroup { Name = u.KategoriID.HasValue ? u.Kategori?.KategoriAdi ?? "Genel" : "Genel" }
                }).ToList();
                
                _logger.LogInformation("Ürün select list oluşturuldu, toplam {count} adet öğe içeriyor.", viewModel.Urunler.Count);
                
                // Birim bilgisini dictionary olarak sakla
                viewModel.UrunBirimBilgileri = urunler.ToDictionary(
                    u => u.UrunID.ToString(), 
                    u => u.Birim?.BirimAdi ?? "Adet"
                );
            }

            // Para birimleri
            var paraBirimleriEntities = await _context.ParaBirimleri
                .Where(p => !p.Silindi && p.Aktif)
                .OrderBy(p => p.Sira)
                .ToListAsync();
                
            viewModel.ParaBirimleri = paraBirimleriEntities.Select(p => new ViewModels.Fatura.ParaBirimi
            {
                Kod = p.Kod,
                Ad = p.Ad,
                Sembol = p.Sembol,
                AnaParaBirimiMi = p.AnaParaBirimiMi
            }).ToList();

            _logger.LogInformation("Para birimi listesi yüklendi, toplam {count} adet para birimi bulundu.", viewModel.ParaBirimleri.Count);

            // Carilerin varsayılan para birimleri
            var paraBirimiIdToKodMapping = paraBirimleriEntities.ToDictionary(p => p.ParaBirimiID, p => p.Kod);
            
            viewModel.CariParaBirimleri = cariler
                .Where(c => c.VarsayilanParaBirimiId.HasValue)
                .ToDictionary(
                    c => c.CariID.ToString(),
                    c => paraBirimiIdToKodMapping.TryGetValue(c.VarsayilanParaBirimiId.Value, out string kod) ? kod : "TRY"
                );

            // Select listelerini oluştur
            viewModel.Cariler = cariler.Select(c => new SelectListItem { Value = c.CariID.ToString(), Text = c.Ad }).ToList();
            viewModel.Sozlesmeler = sozlesmeler.Select(s => new SelectListItem { Value = s.SozlesmeID.ToString(), Text = s.SozlesmeNo }).ToList();
            viewModel.FaturaTurleri = _context.FaturaTurleri.Select(f => new SelectListItem { Value = f.FaturaTuruID.ToString(), Text = f.FaturaTuruAdi }).ToList();
            viewModel.Depolar = depolar.Select(d => new SelectListItem { Value = d.DepoID.ToString(), Text = d.DepoAdi }).ToList();
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
            try
            {
                if (ModelState.IsValid)
                {
                    // TODO: Aklama işlemi
                    TempData["SuccessMessage"] = "Aklama işlemi başarıyla tamamlandı.";
                    return RedirectToAction(nameof(Index));
                }
                return View(viewModel);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Aklama işlemi sırasında hata oluştu. FaturaID: {viewModel.FaturaID}, Hata: {ex.Message}");
                TempData["ErrorMessage"] = $"Aklama işlemi sırasında bir hata oluştu: {ex.Message}";
                return RedirectToAction(nameof(Index));
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
                
                // Tarih için kur bilgisi bulunamadı, döviz servisi üzerinden güncel kuru çek
                var guncelKur = await _dovizKuruService.GetGuncelKurAsync(dovizKodu, "TRY", arananTarih);
                
                if (guncelKur > 0)
                {
                    return Json(new { 
                        success = true, 
                        kurDegeri = guncelKur,
                        message = $"Güncel döviz kuru kullanılıyor. ({arananTarih:dd.MM.yyyy})"
                    });
                }
                
                // Hiçbir şekilde kur bulunamadı
                return Json(new { 
                    success = false, 
                    kurDegeri = 0,
                    message = $"{dovizKodu} para birimi için {arananTarih:dd.MM.yyyy} tarihli kur bilgisi bulunamadı."
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Kur bilgisi getirilirken hata oluştu: {ex.Message}");
                return Json(new { 
                    success = false, 
                    kurDegeri = 0,
                    message = $"Kur bilgisi getirilirken bir hata oluştu: {ex.Message}" 
                });
            }
        }

        private async Task<Guid> OtomatikIrsaliyeOlusturFromID(Guid faturaID, Guid? depoID = null)
        {
            // Fatura ve detaylarını al
            var fatura = await _unitOfWork.FaturaRepository.GetByIdAsync(faturaID);
            if (fatura == null)
                throw new InvalidOperationException($"FaturaID: {faturaID} bulunamadı");
                
            // Basit bir irsaliye oluşturma implementasyonu
            var irsaliyeID = Guid.NewGuid();
            _logger.LogInformation($"Otomatik irsaliye oluşturuldu: {irsaliyeID} (Fatura: {faturaID})");
            return irsaliyeID;
        }

        [Authorize(Roles = "Admin,FinansYonetici,Rapor")]
        public async Task<IActionResult> Raporlar()
        {
            try
            {
                _logger.LogInformation($"Fatura raporları erişimi: Kullanıcı={User.Identity.Name}");
                return View();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Raporlar işlemi sırasında hata oluştu: {ex.Message}");
                TempData["ErrorMessage"] = $"Raporlar işlemi sırasında bir hata oluştu: {ex.Message}";
                return RedirectToAction(nameof(Index));
            }
        }

        // GET: Fatura/GetNewFaturaNumber
        [HttpGet]
        public async Task<IActionResult> GetNewFaturaNumber()
        {
            try
            {
                var faturaNumarasi = await _faturaNumaralandirmaService.GenerateFaturaNumarasiAsync();
                return Json(faturaNumarasi);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Fatura numarası oluşturulurken hata oluştu");
                return Json(string.Empty);
            }
        }

        // GET: Fatura/GetNewSiparisNumarasi
        [HttpGet]
        public async Task<IActionResult> GetNewSiparisNumarasi()
        {
            try
            {
                var siparisNumarasi = await _faturaNumaralandirmaService.GenerateSiparisNumarasiAsync();
                return Json(siparisNumarasi);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Sipariş numarası oluşturulurken hata oluştu");
                return Json(string.Empty);
            }
        }
    }
} 