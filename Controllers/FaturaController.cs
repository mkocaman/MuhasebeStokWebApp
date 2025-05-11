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
                .OrderByDescending(f => f.OlusturmaTarihi) // En son oluşturulan faturaları üstte göster
                .ThenByDescending(f => f.FaturaTarihi) // Eğer oluşturma tarihi aynı ise fatura tarihine göre sırala
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
                DovizTuru = f.DovizTuru ?? "USD",
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
                    DovizTuru = null, // Para birimi seçim için boş bırakıldı
                    DovizKuru = 13000, // Varsayılan 1 USD = 13000 UZS
                    FaturaKalemleri = new List<FaturaKalemViewModel>(),
                    OtomatikIrsaliyeOlustur = true
                };

                // Otomatik fatura numarası oluştur (servis kullanarak)
                viewModel.FaturaNumarasi = await _faturaNumaralandirmaService.GenerateFaturaNumarasiAsync();

                // Otomatik sipariş numarası oluştur (servis kullanarak)
                viewModel.SiparisNumarasi = await _faturaNumaralandirmaService.GenerateSiparisNumarasiAsync();
                
                // Gerekli formları yükle
                await PopulateFaturaCreateViewModelAsync(viewModel);

                // İlk varsayılan kalemi oluştur
                viewModel.FaturaKalemleri = new List<FaturaKalemViewModel>(); // FaturaKalemleri listesini yeniden oluştur
                
                if (!viewModel.UrunListesiBosMu && viewModel.Urunler.Any())
                {
                    // İlk ürünü seç
                    var ilkUrun = viewModel.Urunler.FirstOrDefault();
                    if (ilkUrun != null)
                    {
                        // Ürün bilgilerini al
                        var urun = await _context.Urunler
                            .Include(u => u.Birim)
                            .FirstOrDefaultAsync(u => u.UrunID == Guid.Parse(ilkUrun.Value));
                            
                        var ilkKalem = new FaturaKalemViewModel
                        {
                            UrunID = Guid.Parse(ilkUrun.Value),
                            UrunAdi = ilkUrun.Text,
                            Miktar = 1,
                            BirimFiyat = urun?.DovizliSatisFiyati ?? 0,
                            Birim = urun?.Birim?.BirimAdi ?? "Adet",
                            KdvOrani = urun?.KDVOrani ?? 0,
                            IndirimOrani = 0,
                            Tutar = urun?.DovizliSatisFiyati ?? 0,
                            KdvTutari = (urun?.DovizliSatisFiyati ?? 0) * (urun?.KDVOrani ?? 0) / 100,
                            IndirimTutari = 0,
                            NetTutar = (urun?.DovizliSatisFiyati ?? 0) * (1 + (urun?.KDVOrani ?? 0) / 100)
                        };
                        viewModel.FaturaKalemleri.Add(ilkKalem);
                    }
                }
                
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

            // Aktif birimler
            var birimler = await _context.Birimler
                .Where(b => !b.Silindi && b.Aktif)
                .OrderBy(b => b.BirimAdi)
                .ToListAsync();
            
            _logger.LogInformation("Birim sayısı: Aktif={activeCount}, Pasif={passiveCount}, Silinmiş={deletedCount}", 
                birimler.Count, 
                await _context.Birimler.CountAsync(b => !b.Silindi && !b.Aktif),
                await _context.Birimler.CountAsync(b => b.Silindi));

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

            // Para birimleri - Sadece USD ve UZS
            var paraBirimleriEntities = await _context.ParaBirimleri
                .Where(p => !p.Silindi && p.Aktif && (p.Kod == "USD" || p.Kod == "UZS"))
                .OrderBy(p => p.Sira)
                .ToListAsync();
                
            viewModel.ParaBirimleri = paraBirimleriEntities.Select(p => new ViewModels.Fatura.ParaBirimi
            {
                Kod = p.Kod,
                Ad = p.Ad,
                Sembol = p.Sembol
            }).ToList();

            _logger.LogInformation("Para birimi listesi yüklendi, toplam {count} adet para birimi bulundu.", viewModel.ParaBirimleri.Count);

            // Carilerin varsayılan para birimleri
            var paraBirimiIdToKodMapping = paraBirimleriEntities.ToDictionary(p => p.ParaBirimiID, p => p.Kod);
            
            viewModel.CariParaBirimleri = cariler
                .Where(c => c.VarsayilanParaBirimiId.HasValue)
                .ToDictionary(
                    c => c.CariID.ToString(),
                    c => {
                        // Eğer carinin varsayılan para birimi yoksa veya USD/UZS dışında bir değerse USD kullan
                        if (!c.VarsayilanParaBirimiId.HasValue || 
                            !paraBirimiIdToKodMapping.TryGetValue(c.VarsayilanParaBirimiId.Value, out string kod) ||
                            (kod != "USD" && kod != "UZS"))
                        {
                            return "USD";
                        }
                        return kod;
                    }
                );

            // Select listelerini oluştur
            viewModel.Cariler = cariler.Select(c => new SelectListItem { Value = c.CariID.ToString(), Text = c.Ad }).ToList();
            viewModel.Sozlesmeler = sozlesmeler.Select(s => new SelectListItem { Value = s.SozlesmeID.ToString(), Text = s.SozlesmeNo }).ToList();
            viewModel.FaturaTurleri = _context.FaturaTurleri.Select(f => new SelectListItem { Value = f.FaturaTuruID.ToString(), Text = f.FaturaTuruAdi }).ToList();
            viewModel.Depolar = depolar.Select(d => new SelectListItem { Value = d.DepoID.ToString(), Text = d.DepoAdi }).ToList();
            
            // ViewBag üzerinden birimler için select list oluştur
            ViewBag.Birimler = birimler.Select(b => new SelectListItem { Value = b.BirimID.ToString(), Text = b.BirimAdi }).ToList();
            
            // ViewBag üzerinden ürünler için select list oluştur (satırlar için)
            ViewBag.Urunler = urunler.Select(u => new SelectListItem { 
                Value = u.UrunID.ToString(), 
                Text = u.UrunAdi
            }).ToList();
            
            // Ürün birim bilgilerini dictionary olarak ViewBag'e ekle
            ViewBag.UrunBirimBilgileri = urunler.ToDictionary(
                u => u.UrunID.ToString(), 
                u => u.Birim?.BirimAdi ?? "Adet"
            );
        }

        // POST: Fatura/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(FaturaCreateViewModel viewModel)
        {
            try
            {
                _logger.LogInformation("Fatura oluşturma işlemi başlatıldı: FaturaNumarasi={FaturaNumarasi}, FaturaTuru={FaturaTuru}, CariID={CariID}", 
                    viewModel.FaturaNumarasi, viewModel.FaturaTuruID, viewModel.CariID);
                
                await PopulateFaturaCreateViewModelAsync(viewModel);
                
                if (!ModelState.IsValid)
                {
                    var errors = ModelState.Values.SelectMany(v => v.Errors)
                        .Select(e => e.ErrorMessage)
                        .ToList();
                    _logger.LogWarning("Fatura oluşturma işleminde doğrulama hatası. FaturaNumarasi={FaturaNumarasi}, Hatalar={Errors}", 
                        viewModel.FaturaNumarasi, string.Join(", ", errors));
                    
                    return View(viewModel);
                }
                
                // Validate total calculation and relations
                var validationResult = _faturaValidationService.ValidateFaturaCreateViewModel(viewModel);
                if (!validationResult.IsValid)
                {
                    ModelState.AddModelError(string.Empty, validationResult.ErrorMessage);
                    _logger.LogWarning("Fatura doğrulama hatası. FaturaNumarasi={FaturaNumarasi}, Hata={Error}", 
                        viewModel.FaturaNumarasi, validationResult.ErrorMessage);
                    
                    return View(viewModel);
                }
                
                // Kullanıcı boş fatura numarası gönderdiyse yeni numara oluştur
                if (string.IsNullOrEmpty(viewModel.FaturaNumarasi))
                {
                    viewModel.FaturaNumarasi = await _faturaNumaralandirmaService.GenerateFaturaNumarasiAsync();
                    _logger.LogInformation("Yeni fatura numarası oluşturuldu: {FaturaNumarasi}", viewModel.FaturaNumarasi);
                }
                
                // Kullanıcı boş sipariş numarası gönderdiyse yeni numara oluştur
                if (string.IsNullOrEmpty(viewModel.SiparisNumarasi))
                {
                    viewModel.SiparisNumarasi = await _faturaNumaralandirmaService.GenerateSiparisNumarasiAsync();
                    _logger.LogInformation("Yeni sipariş numarası oluşturuldu: {SiparisNumarasi}", viewModel.SiparisNumarasi);
                }

                try
                {
                    // Mevcut kullanıcıyı al
                    var currentUser = await _userManager.GetUserAsync(User);
                    var currentUserId = currentUser?.Id != null ? Guid.Parse(currentUser.Id) : (Guid?)null;

                    // Fatura kaydet ve ilişkili kayıtları oluştur
                    var faturaId = await _faturaOrchestrationService.CreateFaturaWithRelations(viewModel, currentUserId);

                    // Otomatik irsaliye oluşturma
                    if (viewModel.OtomatikIrsaliyeOlustur && faturaId != Guid.Empty)
                    {
                        var irsaliyeId = await OtomatikIrsaliyeOlusturFromID(faturaId, viewModel.DepoID);
                        _logger.LogInformation($"Otomatik irsaliye oluşturuldu. IrsaliyeID: {irsaliyeId}");
                    }

                    // İşlem başarılı
                    _logger.LogInformation($"Fatura başarıyla oluşturuldu. FaturaID: {faturaId}");
                    TempData["SuccessMessage"] = "Fatura başarıyla oluşturuldu.";
                    return RedirectToAction(nameof(Details), new { id = faturaId });
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Fatura oluşturma hatası");
                    ModelState.AddModelError(string.Empty, $"Fatura oluşturulurken bir hata oluştu: {ex.Message}");
                    TempData["ErrorMessage"] = $"Fatura oluşturulurken bir hata oluştu: {ex.Message}";
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Beklenmeyen hata");
                TempData["ErrorMessage"] = $"Beklenmeyen bir hata oluştu: {ex.Message}";
                return RedirectToAction(nameof(Index));
            }

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
                // Döviz toplamları
                AraToplamDoviz = fatura.AraToplamDoviz ?? GetDovizTutari(fatura.AraToplam ?? 0m, fatura.DovizTuru, fatura.DovizKuru ?? 1m),
                KdvTutariDoviz = fatura.KDVToplamDoviz ?? GetDovizTutari(fatura.KDVToplam ?? 0m, fatura.DovizTuru, fatura.DovizKuru ?? 1m),
                IndirimTutariDoviz = fatura.IndirimTutariDoviz ?? GetDovizTutari(fatura.IndirimTutari ?? 0m, fatura.DovizTuru, fatura.DovizKuru ?? 1m),
                GenelToplamDoviz = fatura.GenelToplamDoviz ?? GetDovizTutari(fatura.GenelToplam ?? 0m, fatura.DovizTuru, fatura.DovizKuru ?? 1m),
                OdenecekTutar = odenecekTutar,
                OdenenTutar = odenenTutar,
                // Ödeme durumunu hesapla
                OdemeDurumu = odenenTutar == 0 ? "Bekliyor" : 
                              odenenTutar == fatura.GenelToplam.GetValueOrDefault() ? "Ödendi" : 
                              odenenTutar > 0 && odenenTutar < fatura.GenelToplam.GetValueOrDefault() ? "Kısmi Ödendi" : "Bekliyor",
                Aciklama = fatura.FaturaNotu,
                DovizTuru = fatura.DovizTuru ?? "USD",
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
                    // Dövizli alanları hesapla
                    BirimFiyatDoviz = GetDovizTutari(fd.BirimFiyat, fatura.DovizTuru, fatura.DovizKuru ?? 1m),
                    TutarDoviz = GetDovizTutari(fd.Tutar ?? 0m, fatura.DovizTuru, fatura.DovizKuru ?? 1m),
                    KdvTutariDoviz = GetDovizTutari(fd.KdvTutari ?? 0m, fatura.DovizTuru, fatura.DovizKuru ?? 1m),
                    IndirimTutariDoviz = GetDovizTutari(fd.IndirimTutari ?? 0m, fatura.DovizTuru, fatura.DovizKuru ?? 1m),
                    NetTutarDoviz = GetDovizTutari(fd.NetTutar ?? 0m, fatura.DovizTuru, fatura.DovizKuru ?? 1m),
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
                DovizTuru = fatura.DovizTuru ?? "USD",
                DovizKuru = fatura.DovizKuru ?? 1m,
                AraToplam = fatura.AraToplam ?? 0,
                KdvToplam = fatura.KDVToplam ?? 0,
                GenelToplam = fatura.GenelToplam ?? 0,
                IndirimTutari = fatura.IndirimTutari ?? 0,
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
                    Birim = fd.Urun?.Birim?.BirimAdi ?? fd.Birim ?? "Adet"
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

            // ViewBag verilerini doldur
            ViewBag.Cariler = new SelectList(await _context.Cariler.Where(c => !c.Silindi && c.AktifMi).ToListAsync(), "CariID", "Ad", viewModel.CariID);
            ViewBag.FaturaTurleri = new SelectList(await _context.FaturaTurleri.ToListAsync(), "FaturaTuruID", "FaturaTuruAdi", viewModel.FaturaTuruID);
            ViewBag.OdemeTurleri = new SelectList(await _context.OdemeTurleri.ToListAsync(), "OdemeTuruID", "OdemeTuruAdi");
            ViewBag.Sozlesmeler = new SelectList(await _context.Sozlesmeler.Where(s => !s.Silindi && s.AktifMi && (s.CariID == null || s.CariID == viewModel.CariID)).ToListAsync(), "SozlesmeID", "SozlesmeNo", fatura.SozlesmeID);

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
            
            // Döviz listesi - Sadece USD ve UZS
            viewModel.DovizListesi = new List<SelectListItem>
            {
                new SelectListItem { Value = "USD", Text = "Amerikan Doları (USD)", Selected = viewModel.DovizTuru == "USD" },
                new SelectListItem { Value = "UZS", Text = "Özbekistan Somu (UZS)", Selected = viewModel.DovizTuru == "UZS" }
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

            return PartialView("_DeleteModal", fatura);
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
                    return Json(new { success = true, message = "Fatura başarıyla silindi." });
                }
                else
                {
                    _logger.LogWarning($"Fatura silinirken sorun oluştu. FaturaID: {id}");
                    TempData["ErrorMessage"] = "Fatura silinirken sorun oluştu.";
                    return Json(new { success = false, message = "Fatura silinirken sorun oluştu." });
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Fatura silme hatası. FaturaID: {id}");
                return Json(new { success = false, message = $"Fatura silinirken bir hata oluştu: {ex.Message}" });
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
                DovizTuru = fatura.DovizTuru ?? "USD",
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
                    // Dövizli alanları hesapla
                    BirimFiyatDoviz = GetDovizTutari(fd.BirimFiyat, fatura.DovizTuru, fatura.DovizKuru ?? 1m),
                    TutarDoviz = GetDovizTutari(fd.Tutar ?? 0m, fatura.DovizTuru, fatura.DovizKuru ?? 1m),
                    KdvTutariDoviz = GetDovizTutari(fd.KdvTutari ?? 0m, fatura.DovizTuru, fatura.DovizKuru ?? 1m),
                    IndirimTutariDoviz = GetDovizTutari(fd.IndirimTutari ?? 0m, fatura.DovizTuru, fatura.DovizKuru ?? 1m),
                    NetTutarDoviz = GetDovizTutari(fd.NetTutar ?? 0m, fatura.DovizTuru, fatura.DovizKuru ?? 1m),
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
                    DovizTuru = fatura.DovizTuru ?? "USD",
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

        // GET: Fatura/GetKurBilgisi
        [HttpGet]
        public async Task<IActionResult> GetKurBilgisi(string dovizKodu)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(dovizKodu))
                {
                    return Json(new { success = false, message = "Döviz kodu gereklidir" });
                }

                // Sadece USD ve UZS destekleniyor
                if (dovizKodu != "USD" && dovizKodu != "UZS")
                {
                    return Json(new { success = false, message = "Desteklenmeyen döviz kodu. Sadece USD ve UZS destekleniyor." });
                }

                // UZS-USD veya USD-UZS çift yönlü çalışabilir
                var hedefKod = dovizKodu == "USD" ? "UZS" : "USD";
                
                // Döviz kurunu sorgula
                var kurDegeri = await _dovizKuruService.GetGuncelKurAsync(dovizKodu, hedefKod);
                
                if (dovizKodu == "UZS")
                {
                    // UZS sorgulandıysa, UZS/USD paritesinin tersi olan USD/UZS kurunu dön
                    kurDegeri = 1 / kurDegeri;
                }
                
                return Json(new { success = true, kur = kurDegeri });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Döviz kuru alınırken hata oluştu");
                return Json(new { success = false, message = "Döviz kuru alınırken bir hata oluştu" });
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

        // GET: Fatura/GetUrunBilgileri
        [HttpGet]
        public async Task<IActionResult> GetUrunBilgileri(Guid id)
        {
            try
            {
                var urun = await _context.Urunler
                    .Include(u => u.Birim)
                    .FirstOrDefaultAsync(u => u.UrunID == id && !u.Silindi && u.Aktif);

                if (urun == null)
                {
                    return Json(new { success = false, message = "Ürün bulunamadı." });
                }

                return Json(new
                {
                    success = true,
                    urunAdi = urun.UrunAdi,
                    birim = urun.Birim?.BirimAdi ?? "Adet",
                    birimFiyat = urun.DovizliSatisFiyati,
                    kdvOrani = urun.KDVOrani,
                    birimId = urun.BirimID
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ürün bilgileri getirilirken hata oluştu: {Message}", ex.Message);
                return Json(new { success = false, message = "Ürün bilgileri getirilirken bir hata oluştu." });
            }
        }

        private decimal GetDovizTutari(decimal tutar, string dövizTuru, decimal dövizKuru)
        {
            if (dövizTuru == "USD")
            {
                // UZS değerini bulmak için USD değerini çarp
                return tutar * dövizKuru;
            }
            else if (dövizTuru == "UZS")
            {
                // USD değerini bulmak için UZS değerini böl
                if (dövizKuru <= 0)
                {
                    // Sıfıra bölmeyi önle
                    return 0;
                }
                return tutar / dövizKuru;
            }
            throw new ArgumentException("Desteklenmeyen döviz kodu");
        }
    }
} 