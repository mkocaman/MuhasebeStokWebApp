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
            IStokService stokService)
            : base(menuService, userManager, roleManager, logService)
        {
            _unitOfWork = unitOfWork;
            _context = context;
            _stokFifoService = stokFifoService;
            _dovizKuruService = dovizKuruService;
            _logger = logger;
            _stokService = stokService;
            _userManager = userManager;
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
        public async Task<IActionResult> Create()
        {
            try
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

                // ViewBag ile verileri taşı
                ViewBag.Cariler = new SelectList(cariler, "CariID", "Ad");
                ViewBag.Sozlesmeler = new SelectList(sozlesmeler, "SozlesmeID", "SozlesmeNo");
                ViewBag.FaturaTurleri = new SelectList(_context.FaturaTurleri, "FaturaTuruID", "FaturaTuruAdi");
                ViewBag.ParaBirimleri = paraBirimleri;

                // Varsayılan model
                var model = new FaturaCreateViewModel
                {
                    FaturaTarihi = DateTime.Now,
                    VadeTarihi = DateTime.Now.AddDays(30),
                    DovizTuru = "TRY",
                    DovizKuru = 1,
                    OdemeDurumu = "Ödenmedi",
                    OtomatikIrsaliyeOlustur = true, // Varsayılan olarak true
                    FaturaKalemleri = new List<FaturaKalemViewModel>()
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

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(FaturaCreateViewModel viewModel)
        {
            try
            {
                _logger.LogInformation("Step 1 - ViewModel alındı");
                
                // Açıklama alanı zorunlu olmamalı, bu nedenle validation hatası varsa temizle
                if (ModelState.ContainsKey("Aciklama") && ModelState["Aciklama"].Errors.Count > 0)
                {
                    ModelState.Remove("Aciklama");
                }
                
                // FaturaTuru alanı zorunlu olmamalı
                if (ModelState.ContainsKey("FaturaTuru") && ModelState["FaturaTuru"].Errors.Count > 0)
                {
                    ModelState.Remove("FaturaTuru");
                }
                
                // FaturaTuruID null olabilir, bu nedenle doğrulama hatalarını temizle
                if (ModelState.ContainsKey("FaturaTuruID") && ModelState["FaturaTuruID"].Errors.Count > 0)
                {
                    ModelState.Remove("FaturaTuruID");
                }
                
                // CariAdi alanı sistem tarafından doldurulacak, validation hatası varsa temizle
                if (ModelState.ContainsKey("CariAdi") && ModelState["CariAdi"].Errors.Count > 0)
                {
                    ModelState.Remove("CariAdi");
                }
                
                // OdemeDurumu zorunlu alan, eğer eksikse varsayılan değer ata
                if (string.IsNullOrEmpty(viewModel.OdemeDurumu))
                {
                    viewModel.OdemeDurumu = "Ödenmedi";
                    
                    // OdemeDurumu için model hatası varsa temizle
                    if (ModelState.ContainsKey("OdemeDurumu"))
                    {
                        ModelState.Remove("OdemeDurumu");
                    }
                }
                
                if (!ModelState.IsValid)
                {
                    _logger.LogWarning("Step 2 - ModelState geçersiz, validation hataları var");
                    _logger.LogWarning("Validation hataları: " + string.Join(", ", ModelState.Keys
                        .Where(k => ModelState[k].Errors.Count > 0)
                        .Select(k => k + ": " + string.Join("; ", ModelState[k].Errors.Select(e => e.ErrorMessage)))));
                    
                    // ViewBag'leri doldur ve modeli geri döndür
                    ViewBag.Cariler = new SelectList(_context.Cariler.Where(c => !c.Silindi && c.AktifMi), "CariID", "Ad", viewModel.CariID);
                    ViewBag.FaturaTurleri = new SelectList(_context.FaturaTurleri, "FaturaTuruID", "FaturaTuruAdi", viewModel.FaturaTuruID);
                    ViewBag.Sozlesmeler = new SelectList(_context.Sozlesmeler.Where(s => !s.Silindi && s.AktifMi), "SozlesmeID", "SozlesmeNo");
                    
                    // Ürünleri tekrar yükle ve birim bilgilerini de ekle
                    var urunler = await _context.Urunler
                        .Include(u => u.Birim)
                        .Where(u => !u.Silindi && u.Aktif)
                        .ToListAsync();

                    var urunSelectList = urunler.Select(u => new SelectListItem
                    {
                        Value = u.UrunID.ToString(),
                        Text = u.UrunAdi,
                        Group = new SelectListGroup { Name = u.KategoriID.HasValue ? u.Kategori?.KategoriAdi ?? "Genel" : "Genel" }
                    }).ToList();
                    
                    ViewBag.Urunler = urunSelectList;
                    ViewBag.UrunBirimBilgileri = urunler.ToDictionary(
                        u => u.UrunID.ToString(), 
                        u => u.Birim?.BirimAdi ?? "Adet"
                    );
                    
                    // Para birimlerini de ekleyelim
                    ViewBag.ParaBirimleri = await _context.ParaBirimleri
                        .Where(p => !p.Silindi && p.Aktif)
                        .OrderBy(p => p.Sira)
                        .ToListAsync();
                    
                    return View(viewModel);
                }
                
                _logger.LogInformation("Step 2 - ModelState kontrol ediliyor");
                
                // Fatura türü kontrolü
                var dbFaturaTuru = await _context.FaturaTurleri.FindAsync(viewModel.FaturaTuruID);
                if (dbFaturaTuru == null)
                {
                    _logger.LogWarning($"Geçersiz fatura türü ID: {viewModel.FaturaTuruID}");
                    ModelState.AddModelError("FaturaTuruID", "Geçersiz fatura türü seçildi.");
                    
                    // ViewBag'leri doldur ve modeli geri döndür
                    ViewBag.Cariler = new SelectList(_context.Cariler.Where(c => !c.Silindi && c.AktifMi), "CariID", "Ad", viewModel.CariID);
                    ViewBag.FaturaTurleri = new SelectList(_context.FaturaTurleri, "FaturaTuruID", "FaturaTuruAdi", viewModel.FaturaTuruID);
                    ViewBag.Sozlesmeler = new SelectList(_context.Sozlesmeler.Where(s => !s.Silindi && s.AktifMi), "SozlesmeID", "SozlesmeNo");
                    
                    // Ürünleri tekrar yükle ve birim bilgilerini de ekle
                    var urunler = await _context.Urunler
                        .Include(u => u.Birim)
                        .Where(u => !u.Silindi && u.Aktif)
                        .ToListAsync();

                    var urunSelectList = urunler.Select(u => new SelectListItem
                    {
                        Value = u.UrunID.ToString(),
                        Text = u.UrunAdi,
                        Group = new SelectListGroup { Name = u.KategoriID.HasValue ? u.Kategori?.KategoriAdi ?? "Genel" : "Genel" }
                    }).ToList();
                    
                    ViewBag.Urunler = urunSelectList;
                    ViewBag.UrunBirimBilgileri = urunler.ToDictionary(
                        u => u.UrunID.ToString(), 
                        u => u.Birim?.BirimAdi ?? "Adet"
                    );
                    
                    // Para birimlerini de ekleyelim
                    ViewBag.ParaBirimleri = await _context.ParaBirimleri
                        .Where(p => !p.Silindi && p.Aktif)
                        .OrderBy(p => p.Sira)
                        .ToListAsync();
                
                    return View(viewModel);
                }

                // Read Committed izolasyon seviyesi kullanarak transaction başlat
                // Bu daha yüksek eşzamanlılık ve daha kısa sürede tamamlanmasını sağlar
                using var transaction = await _context.Database.BeginTransactionAsync(System.Data.IsolationLevel.ReadCommitted);
                try 
                {
                    _logger.LogInformation("Step 3 - Transaction başlatılıyor (Read Committed)");
                    
                    _logger.LogInformation("Step 4 - Fatura nesnesi oluşturuluyor");
                    
                    // Yeni Fatura oluştur
                    var fatura = new Data.Entities.Fatura
                    {
                        FaturaID = Guid.NewGuid(),
                        FaturaNumarasi = viewModel.FaturaNumarasi ?? GenerateNewFaturaNumarasi(),
                        FaturaTarihi = viewModel.FaturaTarihi,
                        VadeTarihi = viewModel.VadeTarihi,
                        CariID = viewModel.CariID,
                        FaturaTuruID = viewModel.FaturaTuruID,
                        SiparisNumarasi = viewModel.SiparisNumarasi ?? "",
                        ResmiMi = viewModel.ResmiMi,
                        FaturaNotu = viewModel.Aciklama ?? "",
                        OdemeDurumu = viewModel.OdemeDurumu,
                        AraToplam = viewModel.AraToplam,
                        KDVToplam = viewModel.KdvToplam,
                        GenelToplam = viewModel.GenelToplam,
                        Aktif = true,
                        Silindi = false,
                        OlusturmaTarihi = DateTime.Now,
                        OlusturanKullaniciID = GetCurrentUserId().GetValueOrDefault(Guid.Empty),
                        DovizTuru = viewModel.DovizTuru ?? "TRY",
                        DovizKuru = viewModel.DovizKuru ?? 1,
                        // SozlesmeID değeri eğer viewModel'de set edilmiş ve geçerli bir değerse ekle, aksi halde null bırak
                        SozlesmeID = viewModel.SozlesmeID.HasValue && viewModel.SozlesmeID.Value != Guid.Empty ? viewModel.SozlesmeID : null
                    };

                    _context.Faturalar.Add(fatura);
                    _logger.LogInformation($"Fatura oluşturuldu: ID={fatura.FaturaID}, FaturaNo={fatura.FaturaNumarasi}");

                    // FIFO işlemleri için toplu veri yapısı
                    var stokHareketleri = new List<StokHareket>();
                    var fifoGirisIslemleri = new List<(Guid UrunID, decimal Miktar, decimal BirimFiyat, string Birim, string DovizTuru, decimal? DovizKuru)>();
                    var fifoCikisIslemleri = new List<(Guid UrunID, decimal Miktar)>();
                    var faturaDetaylari = new List<FaturaDetay>();

                    // Ürün stok miktarı güncellemeleri için toplu veri yapısı
                    var urunGuncellemeleri = new Dictionary<Guid, decimal>();

                    _logger.LogInformation("Step 5 - Fatura kalemleri işleniyor");
                    if (viewModel.FaturaKalemleri != null && viewModel.FaturaKalemleri.Any())
                    {
                        _logger.LogInformation($"Fatura kalemleri işleniyor. Toplam {viewModel.FaturaKalemleri.Count} adet kalem var.");
                        
                        var stokHareketTuru = dbFaturaTuru?.HareketTuru ?? "Giriş";
                        var isGiris = stokHareketTuru == "Giriş";
                        
                        foreach (var kalem in viewModel.FaturaKalemleri)
                        {
                            // SatirKdvToplam değeri için KdvTutari değerini kullanıyoruz
                            var birimFiyat = kalem.BirimFiyat;
                            var miktar = kalem.Miktar;
                            var kdvOrani = kalem.KdvOrani / 100;

                            var tutar = birimFiyat * miktar;
                            var kdvTutar = tutar * kdvOrani;
                            var toplamTutar = tutar + kdvTutar;
                            
                            var faturaDetay = new FaturaDetay
                            {
                                FaturaDetayID = Guid.NewGuid(),
                                FaturaID = fatura.FaturaID,
                                UrunID = kalem.UrunID,
                                Miktar = miktar,
                                BirimFiyat = birimFiyat,
                                KdvOrani = kdvOrani * 100, // KdvOrani'ni yüzde olarak kaydediyoruz
                                IndirimOrani = kalem.IndirimOrani,
                                Tutar = tutar,
                                KdvTutari = kdvTutar,
                                IndirimTutari = tutar * (kalem.IndirimOrani / 100),
                                NetTutar = toplamTutar,
                                Birim = kalem.Birim,
                                SatirToplam = tutar, // Satır toplamı = tutar
                                SatirKdvToplam = kdvTutar, // Satır KDV toplamı = KDV tutarı
                                Aciklama = "",
                                OlusturmaTarihi = DateTime.Now,
                                OlusturanKullaniciID = GetCurrentUserId(),
                                Silindi = false
                            };

                            faturaDetaylari.Add(faturaDetay);
                            _logger.LogInformation($"Fatura detayı oluşturuldu: ID={faturaDetay.FaturaDetayID}, Ürün={kalem.UrunID}, Miktar={kalem.Miktar}");

                            // Stok hareketi oluştur
                            var stokHareket = new StokHareket
                            {
                                StokHareketID = Guid.NewGuid(),
                                UrunID = kalem.UrunID,
                                HareketTuru = isGiris ? StokHareketiTipi.Giris : StokHareketiTipi.Cikis,
                                Miktar = isGiris ? miktar : -miktar,
                                BirimFiyat = birimFiyat,
                                Tarih = viewModel.FaturaTarihi ?? DateTime.Now,
                                Aciklama = $"{viewModel.FaturaNumarasi} numaralı fatura",
                                ReferansNo = fatura.FaturaNumarasi ?? "",
                                ReferansTuru = "Fatura",
                                ReferansID = fatura.FaturaID,
                                OlusturmaTarihi = DateTime.Now,
                                Silindi = false,
                                Birim = kalem.Birim ?? "Adet"
                            };

                            stokHareketleri.Add(stokHareket);
                            _logger.LogInformation($"Stok hareketi oluşturuldu: ID={stokHareket.StokHareketID}, Tür={stokHareketTuru}, Miktar={stokHareket.Miktar}");

                            // Ürün stok miktarını güncelle
                            if (!urunGuncellemeleri.ContainsKey(kalem.UrunID))
                            {
                                urunGuncellemeleri[kalem.UrunID] = 0;
                            }
                            urunGuncellemeleri[kalem.UrunID] += isGiris ? miktar : -miktar;

                            // FIFO işlemleri için veri topla
                            if (isGiris)
                            {
                                fifoGirisIslemleri.Add((
                                    kalem.UrunID,
                                    miktar,
                                    birimFiyat,
                                    kalem.Birim ?? "Adet",
                                    viewModel.DovizTuru ?? "TRY",
                                    viewModel.DovizKuru
                                ));
                            }
                            else
                            {
                                fifoCikisIslemleri.Add((kalem.UrunID, miktar));
                            }
                        }
                    }

                    // Batch işlemlerle veritabanına kaydet
                    _logger.LogInformation("Step 6 - Veritabanı işlemleri başlatılıyor");
                    
                    // Fatura detaylarını ekle
                    _context.FaturaDetaylari.AddRange(faturaDetaylari);
                    
                    // Stok hareketlerini ekle
                    _context.StokHareketleri.AddRange(stokHareketleri);
                    
                    // Ürün stok miktarlarını güncelle
                    foreach (var urunGuncelleme in urunGuncellemeleri)
                    {
                        var urun = await _context.Urunler.FindAsync(urunGuncelleme.Key);
                        if (urun != null)
                        {
                            var eskiMiktar = urun.StokMiktar;
                            urun.StokMiktar += urunGuncelleme.Value;
                            _logger.LogInformation($"Ürün stok miktarı güncellendi: UrunID={urun.UrunID}, Eski={eskiMiktar}, Yeni={urun.StokMiktar}");
                        }
                    }
                    
                    // SaveChanges işlemini yaparak ana kayıtları ekleyelim
                    await _context.SaveChangesAsync();
                    _logger.LogInformation("Step 7 - Ana kayıtlar veritabanına eklendi");
                    
                    // FIFO işlemlerini toplu olarak yap
                    _logger.LogInformation("Step 8 - FIFO işlemleri başlatılıyor");
                    
                    try
                    {
                        // Giriş işlemleri
                        foreach (var fifoGiris in fifoGirisIslemleri)
                        {
                            try
                            {
                                var faturaNo = fatura.FaturaNumarasi ?? "Fatura";
                                
                                var fifoKaydi = await _stokFifoService.StokGirisiYap(
                                    urunID: fifoGiris.UrunID,
                                    miktar: fifoGiris.Miktar,
                                    birimFiyat: fifoGiris.BirimFiyat,
                                    birim: fifoGiris.Birim,
                                    referansNo: faturaNo,
                                    referansTuru: "Fatura",
                                    referansID: fatura.FaturaID,
                                    aciklama: $"{faturaNo} numaralı fatura ile giriş",
                                    paraBirimi: fifoGiris.DovizTuru,
                                    dovizKuru: fifoGiris.DovizKuru
                                );
                                
                                _logger.LogInformation($"FIFO giriş kaydı başarıyla oluşturuldu: UrunID={fifoGiris.UrunID}, Miktar={fifoGiris.Miktar}");
                            }
                            catch (Exception ex)
                            {
                                _logger.LogError(ex, $"FIFO giriş kaydı oluşturulurken hata: UrunID={fifoGiris.UrunID}, Hata={ex.Message}");
                                // Hata olsa bile işleme devam et
                            }
                        }
                        
                        // Çıkış işlemleri
                        foreach (var fifoCikis in fifoCikisIslemleri)
                        {
                            try
                            {
                                var faturaNo = fatura.FaturaNumarasi ?? "Fatura";
                                
                                var result = await _stokFifoService.StokCikisiYap(
                                    fifoCikis.UrunID,
                                    fifoCikis.Miktar,
                                    faturaNo,
                                    "Fatura",
                                    fatura.FaturaID,
                                    $"{faturaNo} numaralı fatura ile çıkış"
                                );
                                
                                _logger.LogInformation($"FIFO çıkış işlemi başarıyla yapıldı: UrunID={fifoCikis.UrunID}, Miktar={fifoCikis.Miktar}");
                            }
                            catch (Exception ex)
                            {
                                _logger.LogError(ex, $"FIFO çıkış işlemi yapılırken hata: UrunID={fifoCikis.UrunID}, Hata={ex.Message}");
                                // Hata olsa bile işleme devam et
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "FIFO işlemleri sırasında genel bir hata oluştu, ancak fatura kaydı tamamlandı.");
                    }

                    // Cari hareket kaydı oluştur
                    _logger.LogInformation("Step 9 - Cari hareket kaydı oluşturuluyor");
                    
                    try
                    {
                        // FaturaTuru bilgisini yükle
                        await _context.Entry(fatura).Reference(f => f.FaturaTuru).LoadAsync();
                        
                        // Hareket türünü belirle
                        string hareketTuru = fatura.FaturaTuru?.FaturaTuruAdi ?? "Bilinmiyor";
                        bool borcMu = fatura.FaturaTuru?.HareketTuru == "Çıkış"; // Satış faturası ise Borç, Alış faturası ise Alacak
                        
                        // Kullanıcı ID'sini al
                        var userId = GetCurrentUserId();

                        var cariHareket = new CariHareket
                        {
                            CariHareketID = Guid.NewGuid(),
                            CariID = fatura.CariID.Value,
                            HareketTuru = hareketTuru,
                            Borc = borcMu ? fatura.GenelToplam ?? 0 : 0,
                            Alacak = !borcMu ? fatura.GenelToplam ?? 0 : 0,
                            Tarih = fatura.FaturaTarihi ?? DateTime.Now,
                            Aciklama = $"{fatura.FaturaNumarasi} numaralı fatura",
                            ReferansNo = fatura.FaturaNumarasi ?? "",
                            ReferansTuru = "Fatura",
                            ReferansID = fatura.FaturaID,
                            OlusturmaTarihi = DateTime.Now,
                            OlusturanKullaniciID = string.IsNullOrEmpty(_userManager.GetUserId(User)) ? null : new Guid(_userManager.GetUserId(User)),
                            Silindi = false
                        };

                        _context.CariHareketler.Add(cariHareket);
                        await _context.SaveChangesAsync();
                        _logger.LogInformation($"Cari hareket kaydı oluşturuldu: ID={cariHareket.CariHareketID}, CariID={cariHareket.CariID}");
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Cari hareket kaydı oluşturulurken hata oluştu, ancak fatura kaydı tamamlandı.");
                    }

                    // İrsaliye işlemlerini yap
                    if (viewModel.OtomatikIrsaliyeOlustur && dbFaturaTuru?.HareketTuru == "Çıkış")
                    {
                        _logger.LogInformation("Step 10 - Otomatik irsaliye oluşturuluyor");
                        try
                        {
                            await OtomatikIrsaliyeOlustur(fatura);
                        }
                        catch (Exception ex)
                        {
                            _logger.LogError(ex, "Otomatik irsaliye oluşturulurken hata oluştu, ancak fatura kaydı tamamlandı.");
                        }
                    }

                    // Tüm işlemler başarılı, transaction'ı commit et
                    await transaction.CommitAsync();
                    _logger.LogInformation("Step 11 - Transaction commit edildi, fatura kaydı tamamlandı");

                    // Başarılı mesajı ve yönlendirme
                    TempData["SuccessMessage"] = $"{fatura.FaturaNumarasi} numaralı fatura başarıyla oluşturuldu.";
                    return RedirectToAction(nameof(Index));
                }
                catch (Exception ex)
                {
                    // Transaction'ı rollback yap
                    await transaction.RollbackAsync();
                    
                    _logger.LogError(ex, "Fatura kaydedilirken hata oluştu: " + ex.Message);
                    TempData["ErrorMessage"] = "Fatura kaydedilirken bir hata oluştu: " + ex.Message;
                    
                    if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
                    {
                        return Json(new { success = false, message = "Fatura kaydedilirken bir hata oluştu: " + ex.Message });
                    }
                    
                    // ViewBag'leri doldur ve modeli geri döndür
                    ViewBag.Cariler = new SelectList(_context.Cariler.Where(c => !c.Silindi && c.AktifMi), "CariID", "Ad", viewModel.CariID);
                    ViewBag.FaturaTurleri = new SelectList(_context.FaturaTurleri, "FaturaTuruID", "FaturaTuruAdi", viewModel.FaturaTuruID);
                    ViewBag.Sozlesmeler = new SelectList(_context.Sozlesmeler.Where(s => !s.Silindi && s.AktifMi), "SozlesmeID", "SozlesmeNo");
                    
                    // Ürünleri tekrar yükle ve birim bilgilerini de ekle
                    var urunler = await _context.Urunler
                        .Include(u => u.Birim)
                        .Where(u => !u.Silindi && u.Aktif)
                        .ToListAsync();

                    var urunSelectList = urunler.Select(u => new SelectListItem
                    {
                        Value = u.UrunID.ToString(),
                        Text = u.UrunAdi,
                        Group = new SelectListGroup { Name = u.KategoriID.HasValue ? u.Kategori?.KategoriAdi ?? "Genel" : "Genel" }
                    }).ToList();
                    
                    ViewBag.Urunler = urunSelectList;
                    ViewBag.UrunBirimBilgileri = urunler.ToDictionary(
                        u => u.UrunID.ToString(), 
                        u => u.Birim?.BirimAdi ?? "Adet"
                    );
                    
                    // Para birimlerini de ekleyelim
                    ViewBag.ParaBirimleri = await _context.ParaBirimleri
                        .Where(p => !p.Silindi && p.Aktif)
                        .OrderBy(p => p.Sira)
                        .ToListAsync();
                    
                    return View(viewModel);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Fatura kaydetme işleminde beklenmeyen bir hata oluştu: " + ex.Message);
                TempData["ErrorMessage"] = "Fatura kaydedilirken beklenmeyen bir hata oluştu: " + ex.Message;
                return RedirectToAction(nameof(Index));
            }
        }

        // Yeni fatura numarası oluştur
        private string GenerateNewFaturaNumarasi()
        {
            var today = DateTime.Now;
            var year = today.Year.ToString().Substring(2);
            var month = today.Month.ToString().PadLeft(2, '0');
            var day = today.Day.ToString().PadLeft(2, '0');
            
            var prefix = $"FTR-{year}{month}{day}-";
            
            // O gün için mevcut son fatura numarasını bulup arttır
            var lastFatura = _context.Faturalar
                .Where(f => f.FaturaNumarasi != null && f.FaturaNumarasi.StartsWith(prefix))
                .OrderByDescending(f => f.FaturaNumarasi)
                .FirstOrDefault();
            
            int sequence = 1;
            if (lastFatura != null && lastFatura.FaturaNumarasi != null)
            {
                // Son fatura numarasından sıra numarasını çıkar
                var parts = lastFatura.FaturaNumarasi.Split('-');
                if (parts.Length == 3 && int.TryParse(parts[2], out int lastSeq))
                {
                    sequence = lastSeq + 1;
                }
            }
            
            return $"{prefix}{sequence.ToString().PadLeft(3, '0')}";
        }

        // Sipariş numarası oluştur
        private string GenerateSiparisNumarasi()
        {
            var today = DateTime.Now;
            var year = today.Year.ToString().Substring(2);
            var month = today.Month.ToString().PadLeft(2, '0');
            var day = today.Day.ToString().PadLeft(2, '0');
            
            var prefix = $"SIP-{year}{month}{day}-";
            
            // O gün için mevcut son sipariş numarasını bulup arttır
            var lastFatura = _context.Faturalar
                .Where(f => f.SiparisNumarasi != null && f.SiparisNumarasi.StartsWith(prefix))
                .OrderByDescending(f => f.SiparisNumarasi)
                .FirstOrDefault();
            
            int sequence = 1;
            if (lastFatura != null && lastFatura.SiparisNumarasi != null)
            {
                // Son sipariş numarasından sıra numarasını çıkar
                var parts = lastFatura.SiparisNumarasi.Split('-');
                if (parts.Length == 3 && int.TryParse(parts[2], out int lastSeq))
                {
                    sequence = lastSeq + 1;
                }
            }
            
            return $"{prefix}{sequence.ToString().PadLeft(3, '0')}";
        }
        
        // Diğer action ve metotlar...
        
        // Stok hareketleri ile ilgili metot örneği
        private async Task CreateStokHareket(Data.Entities.Fatura fatura, Data.Entities.FaturaDetay detay, StokHareketiTipi hareketTipi)
        {
            var stokHareket = new StokHareket
            {
                StokHareketID = Guid.NewGuid(),
                UrunID = detay.UrunID,
                Miktar = hareketTipi == StokHareketiTipi.Cikis ? -detay.Miktar : detay.Miktar,
                Birim = detay.Birim ?? "Adet",
                HareketTuru = hareketTipi,
                Tarih = fatura.FaturaTarihi ?? DateTime.Now,
                ReferansNo = fatura.FaturaNumarasi ?? "",
                ReferansTuru = "Fatura",
                ReferansID = fatura.FaturaID,
                FaturaID = fatura.FaturaID,
                Aciklama = $"{fatura.FaturaNumarasi} numaralı fatura",
                BirimFiyat = detay.BirimFiyat,
                OlusturmaTarihi = DateTime.Now
            };

            await _unitOfWork.Repository<StokHareket>().AddAsync(stokHareket);
            await _unitOfWork.SaveAsync();
        }

        // GetNewFaturaNumber - JavaScript için yeni fatura numarası
        [HttpGet]
        public IActionResult GetNewFaturaNumber()
        {
            try
            {
                string faturaNo = GenerateNewFaturaNumarasi();
                return Json(faturaNo);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Yeni fatura numarası alınırken hata oluştu");
                return BadRequest("Fatura numarası oluşturulamadı");
            }
        }

        // GetNewSiparisNumarasi - JavaScript için yeni sipariş numarası
        [HttpGet]
        public IActionResult GetNewSiparisNumarasi()
        {
            try
            {
                string siparisNo = GenerateSiparisNumarasi();
                return Json(siparisNo);
        }
            catch (Exception ex)
        {
                _logger.LogError(ex, "Yeni sipariş numarası alınırken hata oluştu");
                return BadRequest("Sipariş numarası oluşturulamadı");
            }
        }

        // Ürün bilgilerini getiren metot
        [HttpGet]
        public async Task<IActionResult> GetUrunBilgileri(Guid id)
        {
            try
            {
                var urun = await _context.Urunler
                    .Include(u => u.Birim)
                    .FirstOrDefaultAsync(u => u.UrunID == id && !u.Silindi);
                
            if (urun == null)
            {
                    return NotFound("Ürün bulunamadı.");
            }

                // Ürünün en güncel fiyatını bul
                decimal birimFiyat = 0;
            var urunFiyat = await _context.UrunFiyatlari
                .Where(uf => uf.UrunID == id && !uf.Silindi)
                .OrderByDescending(uf => uf.GecerliTarih)
                .FirstOrDefaultAsync();

                if (urunFiyat != null)
                {
                    birimFiyat = urunFiyat.Fiyat;
                }

                _logger.LogInformation($"Ürün bilgileri döndürülüyor. ID={id}, Ad={urun.UrunAdi}, Birim={urun.Birim?.BirimAdi}, KDV={urun.KDVOrani}, Fiyat={birimFiyat}");

                return Json(new
                {
                    urunAdi = urun.UrunAdi,
                    birim = urun.Birim?.BirimAdi ?? "Adet",
                    birimFiyat = birimFiyat,
                    kdvOrani = urun.KDVOrani
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ürün bilgileri alınırken hata oluştu");
                return BadRequest($"Ürün bilgileri alınamadı: {ex.Message}");
            }
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
                using var transaction = await _context.Database.BeginTransactionAsync();
                try
                {
                    _logger.LogInformation($"Fatura düzenleme işlemi başlatıldı: FaturaID={id}");
                    
                    // Faturayı getir
                    var fatura = await _context.Faturalar
                        .Include(f => f.FaturaTuru)
                        .Include(f => f.FaturaDetaylari)
                        .FirstOrDefaultAsync(f => f.FaturaID == id);
                        
                    if (fatura == null)
                    {
                        return NotFound();
                    }

                    // Eski stok hareketlerini iptal et
                    _logger.LogInformation($"Eski stok hareketleri iptal ediliyor: FaturaID={id}");
                    var stokHareketleri = await _context.StokHareketleri
                        .Where(s => s.ReferansID == id && s.ReferansTuru == "Fatura" && !s.Silindi)
                        .ToListAsync();

                    // Stok hareketlerini işaretle ve stok miktarlarını güncelle
                    foreach (var hareket in stokHareketleri)
                    {
                        hareket.Silindi = true;
                        hareket.GuncellemeTarihi = DateTime.Now;
                        
                        // Ürünü bul ve stok miktarını güncelle
                        var urun = await _context.Urunler.FindAsync(hareket.UrunID);
                        if (urun != null)
                        {
                            // Değişiklik: Artık StokMiktar property'si dinamik olarak hesaplanmalı, 
                            // direkt güncelleme yapmıyoruz. Bu kod silindiği işaretliyor, bu yüzden
                            // stok hareketi zaten silindi olarak işaretlendi, ek işlem gerekmez.
                            
                            // Dinamik stok miktarını log'lamak için StokService'i kullan
                            var yeniDinamikStokMiktari = await _stokService.GetDinamikStokMiktari(urun.UrunID);
                            _logger.LogInformation($"Stok hareketi silindi olarak işaretlendi: UrunID={urun.UrunID}, " +
                                                 $"HareketID={hareket.StokHareketID}, Güncel Dinamik Stok={yeniDinamikStokMiktari}");
                        }
                        
                        _context.StokHareketleri.Update(hareket);
                    }
                    
                    // İlişkili FIFO kayıtlarını iptal et
                    _logger.LogInformation($"FIFO kayıtları iptal ediliyor: FaturaID={id}");
                    try
                    {
                        // Giriş veya çıkış faturası olsun, tüm FIFO kayıtlarını iptal et
                        await _stokFifoService.FifoKayitlariniIptalEt(id, "Fatura", $"Fatura silindi: {fatura.FaturaNumarasi}", GetCurrentUserId().GetValueOrDefault(Guid.Empty));
                        _logger.LogInformation($"{fatura.FaturaTuru?.HareketTuru} faturası FIFO kayıtları iptal edildi: FaturaID={id}");
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "FIFO kayıtları iptal servis metodu hatası: {0}", ex.Message);
                        throw; // Hata durumunda transaction'ı geri alabilmek için hatayı yeniden fırlat
                    }
                    
                    // Faturayı güncelle
                    fatura.FaturaNumarasi = viewModel.FaturaNumarasi;
                    fatura.SiparisNumarasi = viewModel.SiparisNumarasi;
                    fatura.FaturaTarihi = viewModel.FaturaTarihi;
                    fatura.VadeTarihi = viewModel.VadeTarihi;
                    fatura.CariID = viewModel.CariID;
                    fatura.FaturaTuruID = viewModel.FaturaTuruID;
                    fatura.ResmiMi = viewModel.ResmiMi;
                    fatura.SozlesmeID = viewModel.SozlesmeID.HasValue && viewModel.SozlesmeID.Value != Guid.Empty ? viewModel.SozlesmeID : null;
                    fatura.FaturaNotu = viewModel.Aciklama ?? "";
                    fatura.DovizTuru = viewModel.DovizTuru ?? "TRY";
                    fatura.DovizKuru = viewModel.DovizKuru ?? 1;
                    fatura.OdemeDurumu = viewModel.OdemeDurumu;
                    fatura.IndirimTutari = viewModel.IndirimTutari ?? 0m;
                    fatura.GuncellemeTarihi = DateTime.Now;

                    _context.Update(fatura);

                    // Eski detayları sil
                    var eskiDetaylar = await _context.FaturaDetaylari.Where(fd => fd.FaturaID == id).ToListAsync();
                    _context.FaturaDetaylari.RemoveRange(eskiDetaylar);

                    // Fatura türünü yeniden yükle
                    var dbFaturaTuru = await _context.FaturaTurleri.FindAsync(viewModel.FaturaTuruID);
                    
                    // Yeni detayları ekle
                    if (viewModel.FaturaKalemleri != null && viewModel.FaturaKalemleri.Any())
                    {
                        decimal araToplam = 0;
                        decimal kdvToplam = 0;
                        decimal genelToplam = 0;
                        decimal indirimToplam = 0;

                        foreach (var kalem in viewModel.FaturaKalemleri.Where(k => k.UrunID != Guid.Empty))
                        {
                            var birimFiyat = kalem.BirimFiyat;
                            var miktar = kalem.Miktar;
                            var kdvOrani = kalem.KdvOrani / 100;

                            var tutar = birimFiyat * miktar;
                            var kdvTutar = tutar * kdvOrani;
                            var toplamTutar = tutar + kdvTutar;

                            araToplam += tutar;
                            kdvToplam += kdvTutar;
                            genelToplam += toplamTutar;
                            indirimToplam += tutar * (kalem.IndirimOrani / 100);

                            var faturaDetay = new FaturaDetay
                            {
                                FaturaDetayID = Guid.NewGuid(),
                                FaturaID = fatura.FaturaID,
                                UrunID = kalem.UrunID,
                                Miktar = miktar,
                                BirimFiyat = birimFiyat,
                                KdvOrani = kdvOrani,
                                IndirimOrani = kalem.IndirimOrani,
                                Tutar = toplamTutar,
                                KdvTutari = kdvTutar,
                                IndirimTutari = tutar * (kalem.IndirimOrani / 100),
                                NetTutar = toplamTutar - kdvTutar,
                                Birim = kalem.Birim,
                                SatirToplam = tutar, // Satır toplamı = tutar
                                SatirKdvToplam = kdvTutar, // Satır KDV toplamı = KDV tutarı
                                Aciklama = "",
                                OlusturmaTarihi = DateTime.Now,
                                OlusturanKullaniciID = GetCurrentUserId(),
                                Silindi = false
                            };

                            await _context.FaturaDetaylari.AddAsync(faturaDetay);
                        }
                    }

                    // Yeni stok hareketleri oluştur
                    var stokHareketTuru = dbFaturaTuru?.HareketTuru ?? "Giriş";
                    
                    // Fatura kalemlerini işle
                    foreach (var kalem in viewModel.FaturaKalemleri.Where(k => k.UrunID != Guid.Empty))
                    {
                        var birimFiyat = kalem.BirimFiyat;
                        var miktar = kalem.Miktar;
                        
                        var stokHareket = new StokHareket
                        {
                            StokHareketID = Guid.NewGuid(),
                            UrunID = kalem.UrunID,
                            HareketTuru = stokHareketTuru == "Çıkış" ? StokHareketiTipi.Cikis : StokHareketiTipi.Giris,
                            Miktar = stokHareketTuru == "Giriş" ? miktar : -miktar,
                            BirimFiyat = birimFiyat,
                            Tarih = viewModel.FaturaTarihi ?? DateTime.Now,
                            Aciklama = $"{viewModel.FaturaNumarasi} numaralı fatura (düzenlendi)",
                            ReferansNo = fatura.FaturaNumarasi ?? "",
                            ReferansTuru = "Fatura",
                            ReferansID = fatura.FaturaID,
                            OlusturmaTarihi = DateTime.Now,
                            Silindi = false,
                            Birim = kalem.Birim ?? "Adet"
                        };

                        _context.StokHareketleri.Add(stokHareket);
                        _logger.LogInformation($"Yeni stok hareketi oluşturuldu: ID={stokHareket.StokHareketID}, Tür={stokHareketTuru}, Miktar={stokHareket.Miktar}");

                        // Ürün stok miktarını güncelle
                        var urun = await _context.Urunler.FindAsync(kalem.UrunID);
                        if (urun != null)
                        {
                            // Stok hareketine göre miktarı ekle veya çıkar
                            if (stokHareketTuru == "Giriş")
                            {
                                urun.StokMiktar += miktar;
                                _logger.LogInformation($"Ürün stok miktarı güncellendi: UrunID={urun.UrunID}, Yeni={urun.StokMiktar}");
                                
                                // Alış faturası için FIFO kaydı oluştur
                                try
                                {
                                    _logger.LogInformation($"FIFO girişi başlatılıyor: UrunID={kalem.UrunID}, Miktar={miktar}, BirimFiyat={birimFiyat}, DovizTuru={viewModel.DovizTuru}");
                                    
                                    // Null kontrolü yapalım
                                    var faturaNo = fatura.FaturaNumarasi ?? "Fatura";
                                    var dovizTuru = viewModel.DovizTuru ?? "USD";
                                    var birim = kalem.Birim ?? "Adet";
                                    
                                    // FIFO kaydı oluşturma işlemi
                                    var fifoKaydi = await _stokFifoService.StokGirisiYap(
                                        urunID: kalem.UrunID,
                                        miktar: miktar,
                                        birimFiyat: birimFiyat,
                                        birim: birim,
                                        referansNo: faturaNo,
                                        referansTuru: "Fatura",
                                        referansID: fatura.FaturaID,
                                        aciklama: $"{faturaNo} numaralı fatura ile giriş (düzenlendi)",
                                        paraBirimi: dovizTuru,
                                        dovizKuru: viewModel.DovizKuru
                                    );
                                    
                                    if (fifoKaydi != null)
                                    {
                                        _logger.LogInformation($"FIFO kaydı başarıyla oluşturuldu: FifoID={fifoKaydi.StokFifoID}, UrunID={kalem.UrunID}, Miktar={miktar}");
                                    }
                                    else
                                    {
                                        throw new Exception($"FIFO kaydı oluşturulamadı: UrunID={kalem.UrunID}, Miktar={miktar}");
                                    }
                                }
                                catch (Exception ex)
                                {
                                    _logger.LogError(ex, $"FIFO kaydı oluşturulurken hata: UrunID={kalem.UrunID}, Miktar={miktar}");
                                    throw;
                                }
                            }
                            else // Çıkış durumu
                            {
                                urun.StokMiktar -= miktar;
                                _logger.LogInformation($"Ürün stok miktarı güncellendi (çıkış): UrunID={urun.UrunID}, Yeni={urun.StokMiktar}");
                                
                                // Satış faturası için FIFO çıkışı yap
                                try
                                {
                                    _logger.LogInformation($"FIFO çıkışı başlatılıyor: UrunID={kalem.UrunID}, Miktar={miktar}");
                                    var faturaNo = fatura.FaturaNumarasi ?? "Fatura";
                                    
                                    // FIFO çıkışı yap
                                    var result = await _stokFifoService.StokCikisiYap(
                                        kalem.UrunID,
                                        miktar,
                                        faturaNo,
                                        "Fatura",
                                        fatura.FaturaID,
                                        $"{faturaNo} numaralı fatura ile çıkış (düzenlendi)"
                                    );
                                    
                                    _logger.LogInformation($"FIFO çıkışı başarılı: UrunID={kalem.UrunID}, Miktar={miktar}, Toplam Maliyet={result.ToplamMaliyet}");
                                }
                                catch (Exception ex)
                                {
                                    _logger.LogError(ex, $"FIFO çıkışı yapılırken hata: UrunID={kalem.UrunID}, Miktar={miktar}");
                                    throw;
                                }
                            }
                            
                            _context.Urunler.Update(urun);
                        }
                    }

                    // Toplamları hesapla ve faturayı güncelle
                    decimal toplamAraToplam = 0;
                    decimal toplamKdvToplam = 0;
                    decimal toplamGenelToplam = 0;
                    decimal toplamIndirimToplam = 0;
                    
                    // viewModel'dan değerleri al
                    toplamAraToplam = viewModel.AraToplam;
                    toplamKdvToplam = viewModel.KdvToplam;
                    toplamGenelToplam = viewModel.GenelToplam;
                    toplamIndirimToplam = viewModel.IndirimTutari ?? 0;

                    fatura.AraToplam = toplamAraToplam;
                    fatura.KDVToplam = toplamKdvToplam;
                    fatura.GenelToplam = toplamGenelToplam;
                    fatura.IndirimTutari = toplamIndirimToplam;
                    fatura.DovizTuru = viewModel.DovizTuru ?? "USD";
                    fatura.DovizKuru = viewModel.DovizKuru ?? 1;
                    
                    // Faturaya ait Cari Hareket kaydını güncelle veya oluştur
                    _logger.LogInformation("Cari hareket kaydı güncelleniyor/oluşturuluyor");
                    
                    // FaturaTuru bilgisini yükle
                    await _context.Entry(fatura).Reference(f => f.FaturaTuru).LoadAsync();
                    
                    // Hareket türünü belirle
                    string hareketTuru = fatura.FaturaTuru?.FaturaTuruAdi ?? "Bilinmiyor";
                    bool borcMu = fatura.FaturaTuru?.HareketTuru == "Çıkış"; // Satış faturası ise Borç, Alış faturası ise Alacak
                    
                    // Mevcut cari hareketi bul
                    var cariHareket = await _context.CariHareketler
                        .FirstOrDefaultAsync(ch => ch.ReferansID == fatura.FaturaID && 
                                                  ch.ReferansTuru == "Fatura" && 
                                                  !ch.Silindi);
                                                
                    if (cariHareket != null)
                    {
                        // Mevcut kaydı güncelle
                        _logger.LogInformation($"Mevcut cari hareket güncelleniyor: ID={cariHareket.CariHareketID}");
                        
                        cariHareket.CariID = fatura.CariID ?? Guid.Empty;
                        cariHareket.Tarih = fatura.FaturaTarihi ?? DateTime.Now;
                        cariHareket.Tutar = fatura.GenelToplam ?? 0;
                        cariHareket.HareketTuru = hareketTuru;
                        cariHareket.Borc = borcMu ? fatura.GenelToplam ?? 0 : 0;
                        cariHareket.Alacak = !borcMu ? fatura.GenelToplam ?? 0 : 0;
                        cariHareket.ReferansNo = fatura.FaturaNumarasi ?? "";
                        cariHareket.Aciklama = $"{fatura.FaturaNumarasi} numaralı {hareketTuru} Faturası (düzenlendi)";
                        
                        _context.CariHareketler.Update(cariHareket);
                    }
                    else
                    {
                        // Yeni kayıt oluştur
                        _logger.LogInformation("Yeni cari hareket oluşturuluyor");
                        
                        var yeniCariHareket = new CariHareket
                        {
                            CariHareketID = Guid.NewGuid(),
                            CariID = fatura.CariID.GetValueOrDefault(Guid.Empty),
                            Tarih = fatura.FaturaTarihi ?? DateTime.Now,
                            Tutar = fatura.GenelToplam ?? 0,
                            HareketTuru = hareketTuru,
                            Borc = borcMu ? fatura.GenelToplam ?? 0 : 0,
                            Alacak = !borcMu ? fatura.GenelToplam ?? 0 : 0,
                            ReferansNo = fatura.FaturaNumarasi ?? "",
                            ReferansTuru = "Fatura",
                            ReferansID = fatura.FaturaID,
                            Aciklama = $"{fatura.FaturaNumarasi} numaralı {hareketTuru} Faturası",
                            OlusturmaTarihi = DateTime.Now,
                            OlusturanKullaniciID = GetCurrentUserId().GetValueOrDefault(Guid.Empty),
                            Silindi = false
                        };
                        
                        await _context.CariHareketler.AddAsync(yeniCariHareket);
                        _logger.LogInformation($"Yeni cari hareket oluşturuldu: ID={yeniCariHareket.CariHareketID}");
                        }

                        await _context.SaveChangesAsync();
                        await transaction.CommitAsync();

                _logger.LogInformation($"Fatura başarıyla güncellendi: FaturaID={id}");
                TempData["SuccessMessage"] = "Fatura başarıyla güncellendi.";
                        return RedirectToAction(nameof(Details), new { id = fatura.FaturaID });
                    }
                    catch (Exception ex)
            {
                try
                    {
                        await transaction.RollbackAsync();
                    }
                catch (Exception rollbackEx)
                {
                    _logger.LogError(rollbackEx, "Rollback yapılamadı");
                }
                
                _logger.LogError(ex, $"Fatura güncellenirken hata oluştu: {ex.Message}");
                    ModelState.AddModelError("", $"Fatura güncellenirken bir hata oluştu: {ex.Message}");
                }
            }

            // Hata varsa, dropdownları tekrar yükle
            ViewBag.Cariler = new SelectList(_context.Cariler.Where(c => !c.Silindi && c.AktifMi), "CariID", "Ad", viewModel.CariID);
            ViewBag.FaturaTurleri = new SelectList(_context.FaturaTurleri, "FaturaTuruID", "FaturaTuruAdi", viewModel.FaturaTuruID);
            ViewBag.OdemeTurleri = new SelectList(_context.OdemeTurleri, "OdemeTuruID", "OdemeTuruAdi");
            ViewBag.Urunler = new SelectList(_context.Urunler.Where(u => !u.Silindi && u.Aktif), "UrunID", "UrunAdi");
            ViewBag.Sozlesmeler = new SelectList(_context.Sozlesmeler.Where(s => !s.Silindi && s.AktifMi), "SozlesmeID", "SozlesmeNo");
            
            // Döviz listesi
            viewModel.DovizListesi = new List<SelectListItem>
            {
                new SelectListItem { Value = "USD", Text = "Amerikan Doları (USD)", Selected = viewModel.DovizTuru == "USD" },
                new SelectListItem { Value = "EUR", Text = "Euro (EUR)", Selected = viewModel.DovizTuru == "EUR" },
                new SelectListItem { Value = "TRY", Text = "Türk Lirası (TRY)", Selected = viewModel.DovizTuru == "TRY" },
                new SelectListItem { Value = "GBP", Text = "İngiliz Sterlini (GBP)", Selected = viewModel.DovizTuru == "GBP" }
            };

            return View(viewModel);
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
            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                var fatura = await _context.Faturalar
                    .Include(f => f.FaturaDetaylari)
                    .Include(f => f.FaturaTuru)
                    .FirstOrDefaultAsync(f => f.FaturaID == id);
                
                if (fatura == null)
                {
                    return NotFound();
                }

                // İlişkili stok hareketlerini bul
                var stokHareketleri = await _context.StokHareketleri
                    .Where(s => s.ReferansID == id && s.ReferansTuru == "Fatura" && !s.Silindi)
                    .ToListAsync();

                // Stok hareketlerini işaretle ve stok miktarlarını güncelle
                foreach (var hareket in stokHareketleri)
                {
                    hareket.Silindi = true;
                    hareket.GuncellemeTarihi = DateTime.Now;
                    
                    // Ürünü bul ve stok miktarını güncelle
                    var urun = await _context.Urunler.FindAsync(hareket.UrunID);
                    if (urun != null)
                    {
                        // Stok hareketinin tipine göre stok miktarını güncelle
                        // Giriş ise azalt, çıkış ise artır
                        if (hareket.HareketTuru == StokHareketiTipi.Giris)
                        {
                            urun.StokMiktar -= hareket.Miktar;
                            _logger.LogInformation($"Stok miktarı azaltıldı: UrunID={urun.UrunID}, Miktar={hareket.Miktar}, Yeni stok={urun.StokMiktar}");
                        }
                        else if (hareket.HareketTuru == StokHareketiTipi.Cikis)
                        {
                            urun.StokMiktar -= hareket.Miktar; // Çıkış hareketinde miktar zaten negatif olduğu için çıkarınca artırılmış olacak
                            _logger.LogInformation($"Stok miktarı artırıldı: UrunID={urun.UrunID}, Miktar={-hareket.Miktar}, Yeni stok={urun.StokMiktar}");
                        }
                        
                        _context.Urunler.Update(urun);
                    }
                    
                    _context.StokHareketleri.Update(hareket);
                }
                
                // İlişkili FIFO kayıtlarını işaretle (iptal et)
                try
                {
                    // Giriş veya çıkış faturası olsun, tüm FIFO kayıtlarını iptal et
                    await _stokFifoService.FifoKayitlariniIptalEt(id, "Fatura", $"Fatura silindi: {fatura.FaturaNumarasi}", GetCurrentUserId().GetValueOrDefault(Guid.Empty));
                    _logger.LogInformation($"{fatura.FaturaTuru?.HareketTuru} faturası FIFO kayıtları iptal edildi: FaturaID={id}");
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "FIFO kayıtları iptal servis metodu hatası: {0}", ex.Message);
                    throw; // Hata durumunda transaction'ı geri alabilmek için hatayı yeniden fırlat
                }

                // Faturayı silindi olarak işaretle
                fatura.Silindi = true;
                fatura.GuncellemeTarihi = DateTime.Now;
                fatura.SonGuncelleyenKullaniciID = GetCurrentUserId().GetValueOrDefault(Guid.Empty);
                
                // İlişkili cari hareket kaydını bul ve silinmiş olarak işaretle
                var cariHareket = await _context.CariHareketler
                    .FirstOrDefaultAsync(ch => ch.ReferansID == id && ch.ReferansTuru == "Fatura" && !ch.Silindi);
                
                if (cariHareket != null)
                {
                    _logger.LogInformation($"İlişkili cari hareket silinmiş olarak işaretleniyor: ID={cariHareket.CariHareketID}");
                    
                    cariHareket.Silindi = true;
                    cariHareket.GuncellemeTarihi = DateTime.Now;
                    
                    _context.CariHareketler.Update(cariHareket);
                }
                else
                {
                    _logger.LogWarning($"Fatura için ilişkili cari hareket bulunamadı: FaturaID={id}");
                }
                
                _context.Faturalar.Update(fatura);
                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                TempData["SuccessMessage"] = "Fatura ve ilişkili stok hareketleri başarıyla silindi.";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                _logger.LogError(ex, "Fatura silme işlemi sırasında hata oluştu");
                TempData["ErrorMessage"] = $"Fatura silinirken bir hata oluştu: {ex.Message}";
                return RedirectToAction(nameof(Index));
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
        private async Task OtomatikIrsaliyeOlustur(Data.Entities.Fatura fatura)
        {
            try
            {
                // Faturayı detaylarıyla birlikte tekrar çek
                var faturaWithDetails = await _context.Faturalar
                    .Include(f => f.FaturaDetaylari)
                        .ThenInclude(fd => fd.Urun)
                    .Include(f => f.Cari)
                    .FirstOrDefaultAsync(f => f.FaturaID == fatura.FaturaID);

                if (faturaWithDetails != null)
                {
                    // Fatura türüne göre irsaliye türünü belirle
                    var irsaliyeTuru = fatura.FaturaTuru?.HareketTuru ?? "Çıkış";

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

                    // Yeni irsaliye oluştur
                    var irsaliye = new Irsaliye
                    {
                        IrsaliyeID = Guid.NewGuid(),
                        IrsaliyeNumarasi = irsaliyeNumarasi,
                        IrsaliyeTarihi = faturaWithDetails.FaturaTarihi ?? DateTime.Now,
                        CariID = faturaWithDetails.CariID ?? Guid.Empty,
                        FaturaID = faturaWithDetails.FaturaID,
                        IrsaliyeTuru = irsaliyeTuru, // Fatura türüne göre belirlenen irsaliye türü
                        Aciklama = $"{faturaWithDetails.FaturaNumarasi ?? ""} numaralı faturaya ait otomatik oluşturulan {irsaliyeTuru} irsaliyesi",
                        OlusturmaTarihi = DateTime.Now,
                        OlusturanKullaniciId = GetCurrentUserId().GetValueOrDefault(Guid.Empty),
                        Aktif = true,
                        Silindi = false
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
                                    Miktar = detay.Miktar,
                                    Birim = detay.Birim ?? "Adet",
                                    Aciklama = "",
                                    OlusturmaTarihi = DateTime.Now,
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
                    throw new Exception($"İrsaliye oluşturmak için fatura bulunamadı. FaturaID: {fatura.FaturaID}");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Otomatik irsaliye oluşturulurken hata oluştu: {ex.Message}");
                throw;
            }
        }
    }
} 