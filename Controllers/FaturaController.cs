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
            IFaturaService faturaService)
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
                
                // Eğer fatura kalemleri yoksa ekleme yapılmamalı
                if (viewModel.FaturaKalemleri == null || viewModel.FaturaKalemleri.Count == 0)
                {
                    ModelState.AddModelError("", "En az bir fatura kalemi eklemelisiniz!");
                    
                    // FormData'yı yeniden oluştur
                    var cariler = await _context.Cariler
                        .Where(c => !c.Silindi && c.AktifMi)
                        .OrderBy(c => c.Ad)
                        .ToListAsync();
                    
                    ViewBag.Cariler = new SelectList(cariler, "CariID", "Ad");
                    ViewBag.FaturaTurleri = new SelectList(_context.FaturaTurleri, "FaturaTuruID", "FaturaTuruAdi");
                    
                    var urunler = await _context.Urunler
                        .Where(u => !u.Silindi && u.Aktif)
                        .OrderBy(u => u.UrunAdi)
                        .ToListAsync();
                    
                    ViewBag.Urunler = new SelectList(urunler, "UrunID", "UrunAdi");
                    
                    var depolar = await _context.Depolar
                        .Where(d => !d.Silindi && d.Aktif)
                        .OrderBy(d => d.DepoAdi)
                        .ToListAsync();
                    
                    ViewBag.Depolar = new SelectList(depolar, "DepoID", "DepoAdi");
                    
                    var paraBirimleri = await _context.ParaBirimleri
                        .Where(p => !p.Silindi && p.Aktif)
                        .OrderBy(p => p.Sira)
                        .ToListAsync();
                    
                    ViewBag.ParaBirimleri = paraBirimleri;
                    
                    return View(viewModel);
                }
                
                _logger.LogInformation("Step 2 - Validasyon kontrolleri geçildi, işlem başlatılıyor");
                
                using (var transaction = await _context.Database.BeginTransactionAsync())
                {
                    try
                    {
                        var faturaTuru = await _context.FaturaTurleri.FindAsync(viewModel.FaturaTuruID);
                        var stokHareketTipi = faturaTuru?.HareketTuru == "Giriş" 
                            ? StokHareketiTipi.Giris 
                            : StokHareketiTipi.Cikis;
                        
                        // Fatura numarası otomatik oluşturulacak
                        if (string.IsNullOrEmpty(viewModel.FaturaNumarasi))
                        {
                            viewModel.FaturaNumarasi = GenerateNewFaturaNumarasi();
                            _logger.LogInformation($"Yeni fatura numarası oluşturuldu: {viewModel.FaturaNumarasi}");
                        }
                        
                        // Sipariş numarası otomatik oluşturulacak
                        if (string.IsNullOrEmpty(viewModel.SiparisNumarasi))
                        {
                            viewModel.SiparisNumarasi = GenerateSiparisNumarasi();
                            _logger.LogInformation($"Yeni sipariş numarası oluşturuldu: {viewModel.SiparisNumarasi}");
                        }
                        
                        decimal araToplam = 0;
                        decimal kdvToplam = 0;
                        decimal indirimToplam = 0;
                        decimal genelToplam = 0;
                        
                        // Yeni fatura oluştur
                        var fatura = new Data.Entities.Fatura
                        {
                            FaturaID = Guid.NewGuid(),
                            FaturaNumarasi = viewModel.FaturaNumarasi,
                            SiparisNumarasi = viewModel.SiparisNumarasi,
                            FaturaTarihi = viewModel.FaturaTarihi,
                            VadeTarihi = viewModel.VadeTarihi,
                            CariID = viewModel.CariID,
                            FaturaTuruID = viewModel.FaturaTuruID,
                            ResmiMi = viewModel.ResmiMi,
                            FaturaNotu = viewModel.FaturaNotu,
                            OdemeDurumu = viewModel.OdemeDurumu,
                            DovizTuru = viewModel.DovizTuru,
                            DovizKuru = viewModel.DovizKuru,
                            Aktif = viewModel.Aktif,
                            SozlesmeID = viewModel.SozlesmeID,
                            OlusturmaTarihi = DateTime.Now,
                            OlusturanKullaniciID = GetCurrentUserId()
                        };
                        
                        // Fatura kalemlerini ekle
                        var faturaDetaylari = new List<Data.Entities.FaturaDetay>();
                        
                        foreach (var kalem in viewModel.FaturaKalemleri)
                        {
                            if (kalem.UrunID == Guid.Empty || kalem.Miktar <= 0)
                            {
                                continue; // Geçersiz kalemleri atla
                            }
                            
                            // Kalem hesaplamaları
                            decimal tutar = kalem.Miktar * kalem.BirimFiyat;
                            decimal indirimTutari = tutar * (kalem.IndirimOrani / 100M);
                            decimal araTutar = tutar - indirimTutari;
                            decimal kdvTutari = araTutar * (kalem.KdvOrani / 100M);
                            decimal netTutar = araTutar + kdvTutari;
                            
                            // Toplam hesaplamalar
                            araToplam += tutar;
                            indirimToplam += indirimTutari;
                            kdvToplam += kdvTutari;
                            genelToplam += netTutar;
                            
                            // Yeni fatura detayı oluştur
                            var faturaDetay = new Data.Entities.FaturaDetay
                            {
                                FaturaDetayID = Guid.NewGuid(),
                                FaturaID = fatura.FaturaID,
                                UrunID = kalem.UrunID,
                                Aciklama = kalem.Aciklama,
                                Miktar = kalem.Miktar,
                                Birim = kalem.Birim,
                                BirimFiyat = kalem.BirimFiyat,
                                Tutar = tutar,
                                IndirimOrani = kalem.IndirimOrani,
                                IndirimTutari = indirimTutari,
                                KdvOrani = kalem.KdvOrani,
                                KdvTutari = kdvTutari,
                                NetTutar = netTutar,
                                SatirToplam = tutar,
                                SatirKdvToplam = kdvTutari,
                                OlusturmaTarihi = DateTime.Now
                            };
                            
                            faturaDetaylari.Add(faturaDetay);
                        }
                        
                        // Toplam değerleri faturaya ata
                        fatura.AraToplam = araToplam;
                        fatura.IndirimTutari = indirimToplam;
                        fatura.KDVToplam = kdvToplam;
                        fatura.GenelToplam = genelToplam;
                        
                        // Dövizli değerleri de hesapla ve ata
                        decimal dovizKuru = viewModel.DovizKuru ?? 1M;
                        fatura.AraToplamDoviz = araToplam / dovizKuru;
                        fatura.IndirimTutariDoviz = indirimToplam / dovizKuru;
                        fatura.KDVToplamDoviz = kdvToplam / dovizKuru;
                        fatura.GenelToplamDoviz = genelToplam / dovizKuru;
                        
                        // Faturayı veritabanına ekle
                        _context.Faturalar.Add(fatura);
                        await _context.SaveChangesAsync();
                        
                        _logger.LogInformation($"Fatura başarıyla kaydedildi. ID: {fatura.FaturaID}");
                        
                        // Fatura detaylarını veritabanına ekle
                        await _context.FaturaDetaylari.AddRangeAsync(faturaDetaylari);
                        await _context.SaveChangesAsync();
                        
                        _logger.LogInformation($"Fatura detayları başarıyla kaydedildi. Detay sayısı: {faturaDetaylari.Count}");
                        
                        // Stok hareketlerini oluştur
                        foreach (var detay in faturaDetaylari)
                        {
                            await CreateStokHareket(fatura, detay, stokHareketTipi, viewModel.DepoID);
                        }
                        
                        _logger.LogInformation("Stok hareketleri başarıyla kaydedildi.");
                        
                        // FIFO kayıtlarını oluştur
                        if (viewModel.FaturaKalemleri != null && viewModel.FaturaKalemleri.Any())
                        {
                            var fifoFaturaTuru = await _context.FaturaTurleri.FindAsync(viewModel.FaturaTuruID);
                            var fifoStokHareketTipi = fifoFaturaTuru?.HareketTuru == "Giriş" 
                                ? StokHareketiTipi.Giris 
                                : StokHareketiTipi.Cikis;
                            
                            foreach (var kalem in viewModel.FaturaKalemleri.Where(k => k.UrunID != Guid.Empty))
                            {
                                var miktar = kalem.Miktar;
                                var birimFiyat = kalem.BirimFiyat;
                                var birim = kalem.Birim;
                                
                                if (fifoStokHareketTipi == StokHareketiTipi.Giris)
                                {
                                    // Stok girişi için FIFO kaydı oluştur
                                    await _stokFifoService.StokGirisiYap(
                                        kalem.UrunID, 
                                        miktar, 
                                        birimFiyat, 
                                        birim, 
                                        fatura.FaturaNumarasi, 
                                        "Fatura", 
                                        fatura.FaturaID, 
                                        $"{fatura.FaturaNumarasi} numaralı fatura ile stok girişi", 
                                        fatura.ParaBirimi, 
                                        fatura.DovizKuru);
                                }
                                else
                                {
                                    // Stok çıkışı için FIFO kaydı kullan
                                    await _stokFifoService.StokCikisiYap(
                                        kalem.UrunID, 
                                        miktar, 
                                        fatura.FaturaNumarasi, 
                                        "Fatura", 
                                        fatura.FaturaID, 
                                        $"{fatura.FaturaNumarasi} numaralı fatura ile stok çıkışı");
                                }
                            }
                            
                            _logger.LogInformation("FIFO kayıtları başarıyla oluşturuldu.");
                        }
                        
                        // Otomatik irsaliye oluştur (eğer isteniyorsa)
                        if (viewModel.OtomatikIrsaliyeOlustur)
                        {
                            await OtomatikIrsaliyeOlustur(fatura, viewModel.DepoID);
                            _logger.LogInformation("Otomatik irsaliye oluşturuldu.");
                        }
                        
                        // Transaction'ı commit et
                        await transaction.CommitAsync();
                        
                        _logger.LogInformation("İşlem başarıyla tamamlandı ve commit edildi.");
                        
                        TempData["SuccessMessage"] = $"Fatura başarıyla oluşturuldu. Fatura No: {fatura.FaturaNumarasi}";
                        return RedirectToAction(nameof(Details), new { id = fatura.FaturaID });
                    }
                    catch (Exception ex)
                    {
                        // Hata durumunda transaction rollback
                        _logger.LogError(ex, "Fatura oluşturulurken hata oluştu. Transaction geri alınıyor.");
                        await transaction.RollbackAsync();
                        
                        throw; // Hatayı yukarıya fırlat
                    }
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
        private async Task CreateStokHareket(Data.Entities.Fatura fatura, Data.Entities.FaturaDetay detay, StokHareketiTipi hareketTipi, Guid? depoID = null)
        {
            // İrsaliye türünü belirle
            string irsaliyeTuruStr = hareketTipi == StokHareketiTipi.Giris ? "Giriş" : "Çıkış";
            
            var stokHareket = new StokHareket
            {
                StokHareketID = Guid.NewGuid(),
                UrunID = detay.UrunID,
                DepoID = depoID,
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
                OlusturmaTarihi = DateTime.Now,
                // İrsaliye bilgilerini ekle
                IrsaliyeID = null, // Otomatik oluşturulacak irsaliyenin ID'si SaveChanges sonrası doldurulacak
                IrsaliyeTuru = irsaliyeTuruStr, // Fatura türüne göre irsaliye türü
                ParaBirimi = fatura.DovizTuru ?? "TRY" // Faturanın para birimi
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
                    fatura.DovizTuru = viewModel.DovizTuru ?? "USD";
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
                            var indirimTutar = tutar * (kalem.IndirimOrani / 100);
                            var netTutar = tutar - indirimTutar;

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
                                NetTutar = netTutar,
                                Birim = kalem.Birim,
                                SatirToplam = tutar,
                                SatirKdvToplam = kdvTutar,
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
            // Kullanıcının yetkisini kontrol et
            if (!User.IsInRole("Admin") && !User.IsInRole("FinansYonetici"))
            {
                _logger.LogWarning($"Yetkisiz fatura silme girişimi: Kullanıcı={User.Identity.Name}, FaturaID={id}");
                return RedirectToAction("AccessDenied", "Account");
            }
            
            try
            {
                _logger.LogInformation($"Fatura silme işlemi başlatılıyor: FaturaID={id}, Kullanıcı={User.Identity.Name}");
                
                // Fatura silme işlemi
                await _faturaService.DeleteAsync(id);
                
                _logger.LogInformation($"Fatura başarıyla silindi: FaturaID={id}, Kullanıcı={User.Identity.Name}");
                TempData["SuccessMessage"] = "Fatura başarıyla silindi.";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Fatura silme işleminde hata: FaturaID={id}, Kullanıcı={User.Identity.Name}");
                TempData["ErrorMessage"] = "Fatura silinirken bir hata oluştu: " + ex.Message;
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
        private async Task OtomatikIrsaliyeOlustur(Data.Entities.Fatura fatura, Guid? depoID = null)
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
                    var irsaliyeTuru = fatura.FaturaTuru?.HareketTuru == "Giriş" 
                        ? "Giriş İrsaliyesi" 
                        : "Çıkış İrsaliyesi";

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
                        DepoID = depoID,
                        IrsaliyeTuru = irsaliyeTuru, // Fatura türüne göre belirlenen irsaliye türü
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
                        
                        // İrsaliye oluşturulduktan sonra, faturaya bağlı stok hareketlerinin IrsaliyeID alanını güncelle
                        var stokHareketleri = await _context.StokHareketleri
                            .Where(sh => sh.FaturaID == faturaWithDetails.FaturaID && !sh.Silindi)
                            .ToListAsync();
                            
                        if (stokHareketleri != null && stokHareketleri.Any())
                        {
                            foreach (var hareket in stokHareketleri)
                            {
                                hareket.IrsaliyeID = irsaliye.IrsaliyeID;
                                _context.StokHareketleri.Update(hareket);
                            }
                            await _context.SaveChangesAsync();
                            _logger.LogInformation($"{stokHareketleri.Count} adet stok hareketinin IrsaliyeID alanı güncellendi. IrsaliyeID: {irsaliye.IrsaliyeID}");
                        }
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
        private async Task OtomatikIrsaliyeOlusturFromID(Guid faturaID, Guid? depoID = null)
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
                    string irsaliyeTuru = faturaWithDetails.FaturaTuru?.HareketTuru == "Giriş" ? "Giriş İrsaliyesi" : "Çıkış İrsaliyesi";
                    
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

                    // Yeni irsaliye oluştur
                    var irsaliye = new Irsaliye
                    {
                        IrsaliyeID = Guid.NewGuid(),
                        IrsaliyeNumarasi = irsaliyeNumarasi,
                        IrsaliyeTarihi = faturaWithDetails.FaturaTarihi ?? DateTime.Now,
                        CariID = faturaWithDetails.CariID ?? Guid.Empty,
                        FaturaID = faturaWithDetails.FaturaID,
                        DepoID = depoID,
                        IrsaliyeTuru = irsaliyeTuru, // Fatura türüne göre belirlenen irsaliye türü
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
                        
                        // İrsaliye oluşturulduktan sonra, faturaya bağlı stok hareketlerinin IrsaliyeID alanını güncelle
                        var stokHareketleri = await _context.StokHareketleri
                            .Where(sh => sh.FaturaID == faturaID && !sh.Silindi)
                            .ToListAsync();
                            
                        if (stokHareketleri != null && stokHareketleri.Any())
                        {
                            foreach (var hareket in stokHareketleri)
                            {
                                hareket.IrsaliyeID = irsaliye.IrsaliyeID;
                                _context.StokHareketleri.Update(hareket);
                            }
                            await _context.SaveChangesAsync();
                            _logger.LogInformation($"{stokHareketleri.Count} adet stok hareketinin IrsaliyeID alanı güncellendi. IrsaliyeID: {irsaliye.IrsaliyeID}");
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
                    throw new Exception($"İrsaliye oluşturmak için fatura bulunamadı. FaturaID: {faturaID}");
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