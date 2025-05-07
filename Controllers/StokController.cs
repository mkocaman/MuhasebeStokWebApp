using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using MuhasebeStokWebApp.Data;
using MuhasebeStokWebApp.Data.Entities;
using MuhasebeStokWebApp.Data.Repositories;
using MuhasebeStokWebApp.Enums;
using MuhasebeStokWebApp.Services;
using MuhasebeStokWebApp.Services.Interfaces;
using MuhasebeStokWebApp.ViewModels.Stok;
using MuhasebeStokWebApp.Models;
using System.Globalization;
using Microsoft.AspNetCore.Identity;

namespace MuhasebeStokWebApp.Controllers
{
    [Authorize]
    public class StokController : BaseController
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ApplicationDbContext _context;
        private readonly IStokFifoService _stokFifoService;
        private readonly IDovizKuruService _dovizKuruService;
        private readonly ILogger<StokController> _logger;
        private readonly IStokService _stokService;

        public StokController(
            IUnitOfWork unitOfWork, 
            ApplicationDbContext context, 
            IStokFifoService stokFifoService,
            IDovizKuruService dovizKuruService,
            ILogger<StokController> logger,
            IMenuService menuService,
            UserManager<ApplicationUser> userManager,
            RoleManager<IdentityRole> roleManager,
            ILogService logService,
            IStokService stokService)
            : base(menuService, userManager, roleManager, logService)
        {
            _unitOfWork = unitOfWork;
            _context = context;
            _stokFifoService = stokFifoService;
            _dovizKuruService = dovizKuruService;
            _logger = logger;
            _stokService = stokService;
        }

        // GET: Stok
        public async Task<IActionResult> Index(string searchString)
        {
            var urunRepository = _unitOfWork.Repository<Urun>();
            
            // Ürün listesini getir (filtreleme yaparak)
            var urunler = await urunRepository.GetAsync(
                filter: u => u.Silindi == false && 
                    (string.IsNullOrEmpty(searchString) || 
                     u.UrunAdi.Contains(searchString) || 
                     u.UrunKodu.Contains(searchString)),
                orderBy: q => q.OrderBy(u => u.UrunAdi),
                includeProperties: "Birim,Kategori"
            );
            
            var viewModelUrunler = new List<StokKartViewModel>();
            
            foreach (var u in urunler)
            {
                // Dinamik stok miktarını hesapla
                decimal dinamikStokMiktari = await _stokService.GetDinamikStokMiktari(u.UrunID);
                
                viewModelUrunler.Add(new StokKartViewModel
                {
                    UrunID = u.UrunID,
                    UrunKodu = u.UrunKodu,
                    UrunAdi = u.UrunAdi,
                    Birim = u.Birim?.BirimAdi ?? "-",
                    StokMiktar = dinamikStokMiktari,
                    Kategori = u.Kategori?.KategoriAdi ?? "Kategorisiz",
                    BirimID = (Guid?)(object)u.BirimID,
                    KategoriID = u.KategoriID,
                    KritikStokSeviyesi = u.KritikStokSeviyesi // KritikStokSeviyesi'ni ürün nesnesinden al
                });
            }
            
            var viewModel = new StokListViewModel
            {
                Urunler = viewModelUrunler
            };

            return View(viewModel);
        }

        // GET: Stok/Hareketler/5
        public async Task<IActionResult> Hareketler(Guid id)
        {
            if (id == Guid.Empty)
            {
                return NotFound();
            }

            try
            {
                // Ürün bilgilerini al
                var urun = await _unitOfWork.Repository<Urun>().GetFirstOrDefaultAsync(
                    filter: u => u.UrunID == id && !u.Silindi,
                    includeProperties: "Kategori,Birim");

                if (urun == null)
                {
                    return NotFound();
                }

                // Stok hareketlerini al
                var stokHareketleri = await _unitOfWork.Repository<StokHareket>().GetAsync(
                    filter: sh => sh.UrunID == id && !sh.Silindi,
                    includeProperties: "Depo",
                    orderBy: q => q.OrderByDescending(sh => sh.Tarih));

                // FIFO kayıtlarını al
                var fifoKayitlari = await _unitOfWork.Repository<StokFifo>().GetAsync(
                    filter: sf => sf.UrunID == id && !sf.Silindi, // Aktif filtresini kaldırdık
                    orderBy: q => q.OrderByDescending(sf => sf.GirisTarihi));

                // Ortalama maliyeti hesapla - KDV düşme hatası düzeltildi
                decimal ortalamaMaliyetTL = 0;
                decimal ortalamaMaliyetUSD = 0;
                
                if (fifoKayitlari != null && fifoKayitlari.Any() && fifoKayitlari.Sum(f => f.KalanMiktar) > 0)
                {
                    decimal toplamTutar = fifoKayitlari.Sum(f => f.KalanMiktar * f.BirimFiyat);
                    decimal toplamUSDTutar = fifoKayitlari.Sum(f => f.KalanMiktar * f.BirimFiyatUSD);
                    decimal toplamKalanMiktar = fifoKayitlari.Sum(f => f.KalanMiktar);
                    
                    if (toplamKalanMiktar > 0)
                    {
                        ortalamaMaliyetTL = toplamTutar / toplamKalanMiktar;
                        ortalamaMaliyetUSD = toplamUSDTutar / toplamKalanMiktar;
                    }
                }
                
                // Ortalama satış fiyatını hesapla
                decimal ortalamaSatisFiyatiTL = 0;
                decimal ortalamaSatisFiyatiUSD = 0;
                
                // Son 3 ay içindeki satış hareketlerini al
                var sonUcAy = DateTime.Now.AddMonths(-3);
                var satisFaturalari = await _context.FaturaDetaylari
                    .Include(fd => fd.Fatura)
                    .Include(fd => fd.Fatura.FaturaTuru)
                    .Where(fd => fd.UrunID == id 
                            && !fd.Silindi 
                            && fd.Fatura.FaturaTuru.HareketTuru == "Çıkış"
                            && fd.Fatura.FaturaTarihi >= sonUcAy)
                    .ToListAsync();
                
                if (satisFaturalari != null && satisFaturalari.Any())
                {
                    // BirimFiyat değerleri KDV hariç olduğundan doğrudan kullanabiliriz
                    decimal toplamSatisTutari = satisFaturalari.Sum(fd => fd.BirimFiyat * fd.Miktar);
                    decimal toplamSatisMiktari = satisFaturalari.Sum(fd => fd.Miktar);
                    
                    if (toplamSatisMiktari > 0)
                    {
                        ortalamaSatisFiyatiTL = toplamSatisTutari / toplamSatisMiktari;
                        
                        // USD cinsinden ortalama satış fiyatını hesapla
                        decimal guncelKur = 0;
                        try 
                        {
                            guncelKur = await _dovizKuruService.GetGuncelKurAsync("TRY", "USD");
                            if (guncelKur <= 0)
                            {
                                throw new Exception("Geçersiz kur değeri: " + guncelKur);
                            }
                        }
                        catch (Exception ex)
                        {
                            _logger.LogError(ex, "TRY/USD kur değeri alınırken hata oluştu.");
                            TempData["ErrorMessage"] = $"Döviz kuru alınamadı: {ex.Message}. Güncel kur değerleri hesaplanamıyor.";
                            // Devam ediyoruz ama USD hesaplaması yapılmayacak
                        }
                            
                        if (guncelKur > 0)
                        {
                            ortalamaSatisFiyatiUSD = ortalamaSatisFiyatiTL * guncelKur;
                        }
                    }
                }

                // Önce ürün ID'lerini toplayıp birim adlarını bir sözlükte saklayalım
                var urunIds = stokHareketleri.Select(sh => sh.UrunID).Distinct().ToList();
                var birimAdlari = new Dictionary<Guid, string>();
                
                foreach (var urunId in urunIds)
                {
                    var birimAdi = await GetBirimAdiAsync(urunId);
                    birimAdlari[urunId] = birimAdi ?? "";
                }
                
                // ViewModel oluştur
                var viewModel = new StokHareketViewModel
                {
                    UrunID = urun.UrunID,
                    UrunKodu = urun.UrunKodu,
                    UrunAdi = urun.UrunAdi,
                    Kategori = urun.Kategori?.KategoriAdi ?? "Belirtilmemiş",
                    Birim = urun.Birim?.BirimAdi ?? "Adet",
                    StokMiktar = await _stokService.GetDinamikStokMiktari(urun.UrunID),
                    OrtalamaMaliyetTL = ortalamaMaliyetTL,
                    OrtalamaMaliyetUSD = ortalamaMaliyetUSD,
                    OrtalamaSatisFiyatiTL = ortalamaSatisFiyatiTL,
                    OrtalamaSatisFiyatiUSD = ortalamaSatisFiyatiUSD,
                    StokHareketleri = stokHareketleri.Select(sh => new StokHareketListItemViewModel
                    {
                        StokHareketID = sh.StokHareketID,
                        Tarih = sh.Tarih,
                        HareketTuru = sh.HareketTuru,
                        DepoAdi = sh.Depo?.DepoAdi ?? "Merkez Depo",
                        Miktar = sh.Miktar,
                        BirimFiyat = sh.BirimFiyat ?? 0,
                        Birim = birimAdlari.ContainsKey(sh.UrunID) ? birimAdlari[sh.UrunID] : sh.Birim, // BirimId yerine BirimAdi göster
                        ParaBirimi = sh.ParaBirimi ?? "UZS",
                        ParaBirimiSembol = GetParaBirimiSembol(sh.ParaBirimi),
                        ReferansNo = sh.ReferansNo,
                        ReferansTuru = sh.ReferansTuru,
                        Aciklama = sh.Aciklama ?? ""
                    }).ToList(),
                    FifoKayitlari = fifoKayitlari.Select(sf => new StokFifoListItemViewModel
                    {
                        StokFifoID = sf.StokFifoID,
                        GirisTarihi = sf.GirisTarihi,
                        SonCikisTarihi = sf.SonCikisTarihi,
                        Miktar = sf.Miktar,
                        KalanMiktar = sf.KalanMiktar,
                        BirimFiyat = sf.BirimFiyat,
                        BirimFiyatUSD = sf.BirimFiyatUSD,
                        BirimFiyatUZS = sf.BirimFiyatUZS,
                        DovizKuru = sf.DovizKuru,
                        ParaBirimi = sf.ParaBirimi ?? "UZS",
                        ParaBirimiSembol = GetParaBirimiSembol(sf.ParaBirimi),
                        ReferansNo = sf.ReferansNo,
                        ReferansTuru = sf.ReferansTuru
                    }).ToList()
                };

                return View(viewModel);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Stok hareketleri görüntülenirken hata oluştu. UrunID: {UrunID}", id);
                TempData["ErrorMessage"] = "Stok hareketleri yüklenirken bir hata oluştu. Lütfen daha sonra tekrar deneyin.";
                return RedirectToAction("Index");
            }
        }

        // GET: Stok/StokGiris
        public async Task<IActionResult> StokGiris()
        {
            var viewModel = new StokGirisViewModel
            {
                Tarih = DateTime.Now,
                HareketTuru = StokHareketTipi.Giris,
                ParaBirimi = "USD",
                DovizKuru = 1
            };
            
            await PrepareStokGirisViewModel(viewModel);
            
            return View(viewModel);
        }

        // Bu metot ViewModel'i doldurmak için kullanılacak
        private async Task PrepareStokGirisViewModel(StokGirisViewModel viewModel)
        {
            // Ürünleri getir
            var urunler = await _unitOfWork.Repository<Urun>().GetAsync(
                filter: u => u.Silindi == false && u.Aktif,
                orderBy: q => q.OrderBy(u => u.UrunAdi)
            );
            
            viewModel.Urunler = new SelectList(urunler, "UrunID", "UrunAdi").ToList();
            
            // Depoları getir
            var depolar = await _unitOfWork.Repository<Depo>().GetAsync(
                filter: d => d.Silindi == false && d.Aktif,
                orderBy: q => q.OrderBy(d => d.DepoAdi)
            );
            
            viewModel.Depolar = new SelectList(depolar, "DepoID", "DepoAdi").ToList();
            
            // Birimleri getir
            var birimler = await _unitOfWork.Repository<Birim>().GetAsync(
                filter: b => b.Silindi == false && b.Aktif,
                orderBy: q => q.OrderBy(b => b.BirimAdi)
            );
            
            viewModel.Birimler = new SelectList(birimler, "BirimID", "BirimAdi").ToList();
            
            // Ürün birim bilgilerini hazırla
            viewModel.UrunBirimBilgileri = new Dictionary<string, string>();
            foreach (var urun in urunler)
            {
                if (urun.BirimID.HasValue)
                {
                    var birim = birimler.FirstOrDefault(b => b.BirimID == urun.BirimID);
                    if (birim != null)
                    {
                        viewModel.UrunBirimBilgileri[urun.UrunID.ToString()] = birim.BirimAdi;
                    }
                }
            }
        }

        // POST: Stok/StokGiris
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> StokGiris(StokGirisViewModel viewModel)
        {
            if (ModelState.IsValid)
            {
                // Transaction başlat
                using var transaction = await _context.Database.BeginTransactionAsync();
                try
                {
                    // Ürün bilgilerini al
                    var urun = await _unitOfWork.Repository<Urun>().GetByIdAsync(viewModel.UrunID);
                    if (urun == null)
                    {
                        ModelState.AddModelError("", "Ürün bulunamadı.");
                        await PrepareStokGirisViewModel(viewModel);
                        return View(viewModel);
                    }

                    // Stok hareket kaydı oluştur
                    var stokHareket = new StokHareket
                    {
                        StokHareketID = Guid.NewGuid(),
                        UrunID = viewModel.UrunID,
                        DepoID = viewModel.DepoID,
                        Miktar = viewModel.Miktar,
                        Birim = viewModel.Birim,
                        HareketTuru = viewModel.HareketTuru,
                        Tarih = viewModel.Tarih,
                        ReferansNo = viewModel.ReferansNo ?? "Manuel Giriş",
                        ReferansTuru = viewModel.ReferansTuru ?? "Manuel",
                        Aciklama = viewModel.Aciklama ?? "Manuel stok girişi",
                        BirimFiyat = viewModel.BirimFiyat,
                        OlusturmaTarihi = DateTime.Now
                    };

                    await _unitOfWork.Repository<StokHareket>().AddAsync(stokHareket);

                    // FIFO stok girişi yap
                    // Döviz kurunu al
                    decimal kurDegeri = viewModel.DovizKuru ?? 1m;
                    if (kurDegeri <= 0)
                    {
                        // Hatalı kur değeri durumunda güncel kuru almaya çalış
                        try
                        {
                            // Kaynak para birimi ve dolar kuru ilişkisini belirle
                            string paraBirimi = "USD"; // Default para birimi
                            if (!string.IsNullOrEmpty(viewModel.ParaBirimi))
                            {
                                paraBirimi = viewModel.ParaBirimi;
                            }
                            
                            if (paraBirimi != "USD")
                            {
                                kurDegeri = await _dovizKuruService.GetGuncelKurAsync(paraBirimi, "USD");
                            }
                            else
                            {
                                kurDegeri = 1;
                            }
                        }
                        catch (Exception ex)
                        {
                            _logger.LogError(ex, "Kur değeri alınırken hata oluştu.");
                            kurDegeri = 1; // Hata durumunda 1 olarak belirle
                        }
                    }

                    try
                    {
                        // Stok girişi yap
                        await _stokFifoService.StokGirisiYap(
                            viewModel.UrunID,
                            viewModel.Miktar,
                            viewModel.BirimFiyat,
                            viewModel.Birim,
                            viewModel.ReferansNo ?? "Manuel Giriş",
                            viewModel.ReferansTuru ?? "Manuel",
                            stokHareket.StokHareketID,
                            viewModel.Aciklama ?? "Manuel stok girişi",
                            viewModel.ParaBirimi,
                            kurDegeri
                        );

                        await _unitOfWork.SaveAsync();
                        
                        // Transaction'ı commit et
                        await transaction.CommitAsync();

                        TempData["SuccessMessage"] = "Stok girişi başarıyla kaydedildi.";
                        return RedirectToAction(nameof(Index));
                    }
                    catch (Exception ex)
                    {
                        await transaction.RollbackAsync();
                        ModelState.AddModelError("", $"Stok girişi sırasında bir hata oluştu: {ex.Message}");
                    }
                }
                catch (Exception ex)
                {
                    await transaction.RollbackAsync();
                    ModelState.AddModelError("", $"Stok girişi sırasında bir hata oluştu: {ex.Message}");
                }
            }

            await PrepareStokGirisViewModel(viewModel);
            return View(viewModel);
        }

        // GET: Stok/StokCikis
        public async Task<IActionResult> StokCikis()
        {
            await PrepareViewBagForStokHareket();
            
            var viewModel = new StokCikisViewModel
            {
                Tarih = DateTime.Now,
                HareketTuru = StokHareketTipi.Cikis
            };
            
            return View(viewModel);
        }

        // POST: Stok/StokCikis
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> StokCikis(StokCikisViewModel viewModel)
        {
            if (ModelState.IsValid)
            {
                // Transaction başlat
                using var transaction = await _context.Database.BeginTransactionAsync();
                try
                {
                    // Ürün bilgilerini al
                    var urun = await _unitOfWork.Repository<Urun>().GetByIdAsync(viewModel.UrunID);
                    if (urun == null)
                    {
                        ModelState.AddModelError("", "Ürün bulunamadı.");
                        await PrepareViewBagForStokHareket();
                        return View(viewModel);
                    }

                    // Stok hareket kaydı oluştur
                    var stokHareket = new StokHareket
                    {
                        StokHareketID = Guid.NewGuid(),
                        UrunID = viewModel.UrunID,
                        DepoID = viewModel.DepoID,
                        Miktar = -viewModel.Miktar, // Çıkış işlemi için negatif değer
                        Birim = viewModel.Birim,
                        HareketTuru = StokHareketTipi.Cikis,
                        Tarih = viewModel.Tarih,
                        ReferansNo = viewModel.ReferansNo ?? "Manuel Çıkış",
                        ReferansTuru = viewModel.ReferansTuru ?? "Manuel",
                        Aciklama = viewModel.Aciklama ?? "Manuel stok çıkışı",
                        OlusturmaTarihi = DateTime.Now
                    };

                    // FIFO stok çıkışı yap
                    try
                    {
                        var (kullanilanFifoKayitlari, toplamMaliyet) = await _stokFifoService.StokCikisiYap(
                            viewModel.UrunID,
                            viewModel.Miktar,
                            viewModel.ReferansNo ?? "Manuel Çıkış",
                            viewModel.ReferansTuru ?? "Manuel",
                            stokHareket.StokHareketID,
                            viewModel.Aciklama ?? "Manuel stok çıkışı"
                        );

                        // Birim fiyatı hesapla (toplam maliyet / miktar)
                        stokHareket.BirimFiyat = viewModel.Miktar > 0 ? toplamMaliyet / viewModel.Miktar : 0;

                        await _unitOfWork.Repository<StokHareket>().AddAsync(stokHareket);

                        await _unitOfWork.SaveAsync();
                        
                        // Transaction'ı commit et
                        await transaction.CommitAsync();

                        TempData["SuccessMessage"] = "Stok çıkışı başarıyla kaydedildi.";
                        return RedirectToAction(nameof(Index));
                    }
                    catch (StokYetersizException ex)
                    {
                        await transaction.RollbackAsync();
                        ModelState.AddModelError("", ex.Message);
                        TempData["ErrorMessage"] = ex.Message;
                        TempData["ErrorDetails"] = $"Ürün: {ex.UrunAdi} ({ex.UrunKodu}), Talep edilen: {ex.TalepEdilenMiktar}, Mevcut: {ex.MevcutMiktar}";
                    }
                    catch (Exception ex)
                    {
                        await transaction.RollbackAsync();
                        ModelState.AddModelError("", $"Stok çıkışı sırasında bir hata oluştu: {ex.Message}");
                    }
                }
                catch (Exception ex)
                {
                    await transaction.RollbackAsync();
                    ModelState.AddModelError("", $"Stok çıkışı sırasında bir hata oluştu: {ex.Message}");
                }
            }

            await PrepareViewBagForStokHareket();
            return View(viewModel);
        }

        // GET: Stok/StokTransfer
        public async Task<IActionResult> StokTransfer()
        {
            await PrepareViewBagForStokHareket();
            
            var viewModel = new StokTransferViewModel
            {
                Tarih = DateTime.Now
            };
            
            return View(viewModel);
        }

        // POST: Stok/StokTransfer
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> StokTransfer(StokTransferViewModel viewModel)
        {
            if (ModelState.IsValid)
            {
                if (viewModel.KaynakDepoID == viewModel.HedefDepoID)
                {
                    ModelState.AddModelError("", "Kaynak ve hedef depo aynı olamaz.");
                    await PrepareViewBagForStokHareket();
                    return View(viewModel);
                }

                try
                {
                    // Ürün bilgilerini al
                    var urun = await _unitOfWork.Repository<Urun>().GetByIdAsync(viewModel.UrunID);
                    if (urun == null)
                    {
                        ModelState.AddModelError("", "Ürün bulunamadı.");
                        await PrepareViewBagForStokHareket();
                        return View(viewModel);
                    }

                    // Transfer ID oluştur
                    var transferID = Guid.NewGuid();

                    // 1. Kaynaktan çıkış kaydı
                    var cikisHareket = new StokHareket
                    {
                        StokHareketID = Guid.NewGuid(),
                        UrunID = viewModel.UrunID,
                        DepoID = viewModel.KaynakDepoID,
                        Miktar = -viewModel.Miktar, // Çıkış işlemi için negatif değer
                        Birim = viewModel.Birim,
                        HareketTuru = StokHareketTipi.Cikis,
                        Tarih = viewModel.Tarih,
                        ReferansNo = viewModel.ReferansNo ?? $"Transfer-{transferID}",
                        ReferansTuru = "Transfer-Çıkış",
                        ReferansID = transferID,
                        Aciklama = viewModel.Aciklama ?? "Depolar arası transfer (Çıkış)",
                        OlusturmaTarihi = DateTime.Now
                    };

                    // 2. Hedefe giriş kaydı
                    var girisHareket = new StokHareket
                    {
                        StokHareketID = Guid.NewGuid(),
                        UrunID = viewModel.UrunID,
                        DepoID = viewModel.HedefDepoID,
                        Miktar = viewModel.Miktar, // Giriş işlemi için pozitif değer
                        Birim = viewModel.Birim,
                        HareketTuru = StokHareketTipi.Giris,
                        Tarih = viewModel.Tarih,
                        ReferansNo = viewModel.ReferansNo ?? $"Transfer-{transferID}",
                        ReferansTuru = "Transfer-Giriş",
                        ReferansID = transferID,
                        Aciklama = viewModel.Aciklama ?? "Depolar arası transfer (Giriş)",
                        OlusturmaTarihi = DateTime.Now
                    };

                    // Stok hareketlerini ekle
                    await _unitOfWork.Repository<StokHareket>().AddAsync(cikisHareket);
                    await _unitOfWork.Repository<StokHareket>().AddAsync(girisHareket);

                    // Not: Stok transferinde toplam stok miktarı değişmez, sadece depolar arası hareket olur
                    // Bu nedenle Urun.StokMiktar değeri değiştirilmiyor

                    await _unitOfWork.SaveAsync();

                    TempData["SuccessMessage"] = "Stok transferi başarıyla kaydedildi.";
                    return RedirectToAction(nameof(Index));
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError("", $"Stok transferi sırasında bir hata oluştu: {ex.Message}");
                }
            }

            await PrepareViewBagForStokHareket();
            return View(viewModel);
        }

        // GET: Stok/StokSayim
        public async Task<IActionResult> StokSayim()
        {
            await PrepareViewBagForStokHareket();
            
            var viewModel = new StokSayimViewModel
            {
                Tarih = DateTime.Now
            };
            
            return View(viewModel);
        }

        // POST: Stok/StokSayim
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> StokSayim(StokSayimViewModel viewModel)
        {
            if (!ModelState.IsValid)
            {
                await PrepareViewBagForStokSayim();
                return View(viewModel);
            }

            // Tüm işlemi transaction içinde yap
            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                _logger.LogInformation("Stok sayımı işlemi başlatıldı");
                
                // Sayım için her ürün için işlem yap
                foreach (var urunSayim in viewModel.UrunListesi)
                {
                    // Ürünü bul
                    var urun = await _unitOfWork.Repository<Urun>().GetByIdAsync(urunSayim.UrunID);
                    if (urun == null)
                    {
                        ModelState.AddModelError("", $"Ürün bulunamadı: {urunSayim.UrunID}");
                        throw new Exception($"Ürün bulunamadı: {urunSayim.UrunID}");
                    }

                    // Mevcut stok ile sayım arasındaki farkı hesapla
                    var fark = urunSayim.SayimMiktari - await _stokService.GetDinamikStokMiktari(urun.UrunID, viewModel.DepoID);
                    
                    _logger.LogInformation($"Ürün: {urun.UrunAdi}, MevcutStok: {await _stokService.GetDinamikStokMiktari(urun.UrunID, viewModel.DepoID)}, SayımMiktarı: {urunSayim.SayimMiktari}, Fark: {fark}");
                    
                    // Fazlalık var mı kontrol et (sayım miktarı > stok miktarı)
                    if (fark > 0)
                    {
                        // Artış hareketi oluştur
                        var stokHareket = new StokHareket
                        {
                            StokHareketID = Guid.NewGuid(),
                            UrunID = urun.UrunID,
                            DepoID = viewModel.DepoID,
                            Miktar = fark,
                            BirimFiyat = GetUrunBirimFiyat(urun.UrunID),
                            Birim = urun.Birim?.BirimAdi ?? "Adet",
                            HareketTuru = StokHareketTipi.Giris,
                            Tarih = viewModel.Tarih,
                            ReferansNo = viewModel.ReferansNo,
                            ReferansTuru = "Stok Sayım",
                            Aciklama = $"Sayım fazlası (Sayım: {urunSayim.SayimMiktari}, Önceki: {await _stokService.GetDinamikStokMiktari(urun.UrunID, viewModel.DepoID)})",
                            OlusturmaTarihi = DateTime.Now
                        };

                        await _unitOfWork.Repository<StokHareket>().AddAsync(stokHareket);
                        
                        // StokFifo kaydı oluştur
                        await _stokFifoService.StokGirisiYap(
                            urun.UrunID,
                            fark,
                            GetUrunBirimFiyat(urun.UrunID),
                            urun.Birim?.BirimAdi ?? "Adet",
                            viewModel.ReferansNo,
                            "Stok Sayım",
                            stokHareket.StokHareketID,
                            $"Sayım fazlası: {viewModel.ReferansNo}",
                            viewModel.ParaBirimi,
                            1
                        );
                    }
                    // Eksik var mı kontrol et (sayım miktarı < stok miktarı)
                    else if (fark < 0)
                    {
                        // Azalış hareketi oluştur (miktar negatif olacak)
                        var stokHareket = new StokHareket
                        {
                            StokHareketID = Guid.NewGuid(),
                            UrunID = urun.UrunID,
                            DepoID = viewModel.DepoID,
                            Miktar = fark, // Fark zaten negatif
                            BirimFiyat = GetUrunBirimFiyat(urun.UrunID),
                            Birim = urun.Birim?.BirimAdi ?? "Adet",
                            HareketTuru = StokHareketTipi.Cikis,
                            Tarih = viewModel.Tarih,
                            ReferansNo = viewModel.ReferansNo,
                            ReferansTuru = "Stok Sayım",
                            Aciklama = $"Sayım eksiği (Sayım: {urunSayim.SayimMiktari}, Önceki: {await _stokService.GetDinamikStokMiktari(urun.UrunID, viewModel.DepoID)})",
                            OlusturmaTarihi = DateTime.Now
                        };
                        
                        await _unitOfWork.Repository<StokHareket>().AddAsync(stokHareket);
                        
                        // StokFifo çıkışı yap
                        await _stokFifoService.StokCikisiYap(
                            urun.UrunID,
                            Math.Abs(fark),
                            viewModel.ReferansNo,
                            "Stok Sayım",
                            stokHareket.StokHareketID,
                            $"Sayım eksiği: {viewModel.ReferansNo}"
                        );
                    }
                    
            // Ürün stok miktarını güncelle - bunu kaldırıyoruz, dinamik stok kullanacağız
            // NOT: Sayım sonucu stok miktarı doğrudan güncellenmeyecek, 
            // Giriş/çıkış hareketleri üzerinden hesaplanacak
            
            _logger.LogInformation($"Ürün stok sayım işlemi yapıldı: UrunID={urun.UrunID}, SayımMiktari={urunSayim.SayimMiktari}");
                }

                await _unitOfWork.SaveAsync();
                await transaction.CommitAsync();
                
                _logger.LogInformation("Stok sayımı başarıyla tamamlandı");
                TempData["SuccessMessage"] = "Stok sayımı başarıyla kaydedildi.";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                _logger.LogError(ex, "Stok sayımı sırasında hata oluştu");
                
                ModelState.AddModelError("", $"Stok sayımı sırasında bir hata oluştu: {ex.Message}");
                await PrepareViewBagForStokSayim();
                TempData["ErrorMessage"] = $"Stok sayımı işlemi sırasında bir hata oluştu: {ex.Message}";
                return View(viewModel);
            }
        }

        // GET: Stok/FifoDetay/5
        public async Task<IActionResult> FifoDetay(Guid id)
        {
            var fifoKayit = await _context.StokFifoKayitlari
                .Include(f => f.Urun)
                .FirstOrDefaultAsync(f => f.StokFifoID == id);

            if (fifoKayit == null)
            {
                return NotFound();
            }

            var viewModel = new FifoDetayViewModel
            {
                FifoID = fifoKayit.StokFifoID,
                StokFifoID = fifoKayit.StokFifoID,
                UrunID = fifoKayit.UrunID,
                UrunAdi = fifoKayit.Urun?.UrunAdi,
                UrunKodu = fifoKayit.Urun?.UrunKodu,
                GirisTarihi = fifoKayit.GirisTarihi,
                SonCikisTarihi = fifoKayit.SonCikisTarihi,
                Miktar = fifoKayit.Miktar,
                KalanMiktar = fifoKayit.KalanMiktar,
                BirimFiyat = fifoKayit.BirimFiyat,
                Birim = fifoKayit.Birim,
                ParaBirimi = fifoKayit.ParaBirimi,
                DovizKuru = fifoKayit.DovizKuru,
                BirimFiyatUSD = fifoKayit.BirimFiyatUSD,
                BirimFiyatUZS = fifoKayit.BirimFiyatUZS,
                ReferansNo = fifoKayit.ReferansNo,
                ReferansTuru = fifoKayit.ReferansTuru,
                Aciklama = fifoKayit.Aciklama,
                Aktif = fifoKayit.Aktif,
                Iptal = fifoKayit.Iptal
            };

            return View(viewModel);
        }

        // GET: Stok/StokDurumu
        public async Task<IActionResult> StokDurumu()
        {
            var urunler = await _unitOfWork.Repository<Urun>().GetAsync(
                filter: u => u.Silindi == false && u.Aktif,
                orderBy: q => q.OrderBy(u => u.UrunAdi),
                includeProperties: "Birim,Kategori"
            );

            // Stok durumu detay listesi
            var stokDurumuDetayListesi = new List<StokDurumuDetayViewModel>();

            decimal toplamStokDegeriUZS = 0;
            decimal toplamStokDegeriUSD = 0;

            foreach (var urun in urunler)
            {
                decimal maliyetUZS = await _stokFifoService.GetOrtalamaMaliyet(urun.UrunID, "UZS");
                decimal maliyetUSD = await _stokFifoService.GetOrtalamaMaliyet(urun.UrunID, "USD");
                
                // Dinamik stok miktarını hesapla
                decimal dinamikStokMiktari = await _stokService.GetDinamikStokMiktari(urun.UrunID);
                
                // Ürünün son fiyatını al
                var urunFiyat = await _context.UrunFiyatlari
                    .Where(uf => uf.UrunID == urun.UrunID && !uf.Silindi && uf.FiyatTipiID == 3) // Satış fiyatı
                    .OrderByDescending(uf => uf.GecerliTarih)
                    .FirstOrDefaultAsync();

                var stokDurumuDetay = new StokDurumuDetayViewModel
                {
                    UrunID = urun.UrunID,
                    UrunKodu = urun.UrunKodu,
                    UrunAdi = urun.UrunAdi,
                    Birim = urun.Birim?.BirimAdi ?? "-",
                    Kategori = urun.Kategori?.KategoriAdi ?? "Kategorisiz",
                    StokMiktari = dinamikStokMiktari,
                    OrtalamaMaliyet = maliyetUZS, // UZS cinsinden maliyet
                    OrtalamaMaliyetUSD = maliyetUSD, // USD cinsinden maliyet
                    SatisFiyati = urunFiyat?.Fiyat ?? 0,
                    DepoID = Guid.Empty, // Varsayılan olarak boş, gerekirse depo bazlı filtreleme eklenebilir
                    DepoAdi = "Genel", // Varsayılan olarak genel, gerekirse depo bazlı filtreleme eklenebilir
                    KritikStokSeviyesi = urun.KritikStokSeviyesi > 0 ? urun.KritikStokSeviyesi : 10 // Ürün bazlı kritik stok seviyesi
                };

                stokDurumuDetayListesi.Add(stokDurumuDetay);
                toplamStokDegeriUZS += maliyetUZS * dinamikStokMiktari;
                toplamStokDegeriUSD += maliyetUSD * dinamikStokMiktari;
            }

            // Ana ViewModel
            var stokDurumuViewModel = new StokDurumuViewModel
            {
                StokDurumuListesi = stokDurumuDetayListesi,
                ToplamStokDegeri = toplamStokDegeriUZS,
                ToplamStokDegeriUSD = toplamStokDegeriUSD
            };

            // Depolar için ViewBag hazırla
            var depolar = await _unitOfWork.Repository<Depo>().GetAsync(
                filter: d => d.Silindi == false && d.Aktif,
                orderBy: q => q.OrderBy(d => d.DepoAdi)
            );
            ViewBag.Depolar = new SelectList(depolar, "DepoID", "DepoAdi");
            
            // Kategoriler için ViewBag hazırla
            var kategoriler = await _unitOfWork.Repository<UrunKategori>().GetAsync(
                filter: k => k.Silindi == false && k.Aktif,
                orderBy: q => q.OrderBy(k => k.KategoriAdi)
            );
            ViewBag.Kategoriler = new SelectList(kategoriler, "KategoriID", "KategoriAdi");

            return View(stokDurumuViewModel);
        }

        // GET: Stok/StokRapor
        public async Task<IActionResult> StokRapor(Guid? kategoriID = null, Guid? depoID = null, string stokDurumu = null)
        {
            try
            {
                // Kategorileri getir
                var kategoriler = await _unitOfWork.Repository<UrunKategori>().GetAsync(
                    filter: k => k.Silindi == false && k.Aktif,
                    orderBy: q => q.OrderBy(k => k.KategoriAdi)
                );
                
                ViewBag.Kategoriler = new SelectList(kategoriler, "KategoriID", "KategoriAdi");
                ViewBag.SelectedKategoriID = kategoriID;
                
                // Depoları getir
                var depolar = await _unitOfWork.Repository<Depo>().GetAsync(
                    filter: d => d.Silindi == false && d.Aktif,
                    orderBy: q => q.OrderBy(d => d.DepoAdi)
                );
                
                ViewBag.Depolar = new SelectList(depolar, "DepoID", "DepoAdi");
                ViewBag.SelectedDepoID = depoID;
                ViewBag.SelectedStokDurumu = stokDurumu;
                
                // Ürünleri getir
                var urunQuery = _context.Urunler
                    .Include(u => u.Kategori)
                    .Include(u => u.Birim)
                    .Where(u => !u.Silindi && u.Aktif);
                
                // Kategori filtresi
                if (kategoriID.HasValue)
                {
                    urunQuery = urunQuery.Where(u => u.KategoriID == kategoriID.Value);
                }
                
                var urunler = await urunQuery.OrderBy(u => u.UrunAdi).ToListAsync();
                var stokKartViewModels = new List<StokKartViewModel>();
                
                foreach (var urun in urunler)
                {
                    // Dinamik stok miktarını hesapla
                    decimal dinamikStokMiktari = await _stokService.GetDinamikStokMiktari(urun.UrunID, depoID);
                    
                    // Stok durumu filtresi
                    if (!string.IsNullOrEmpty(stokDurumu))
                    {
                        switch (stokDurumu)
                        {
                            case "kritik":
                                if (dinamikStokMiktari > 10) continue;
                                break;
                            case "dusuk":
                                if (dinamikStokMiktari <= 10 || dinamikStokMiktari > 20) continue;
                                break;
                            case "normal":
                                if (dinamikStokMiktari <= 20) continue;
                                break;
                        }
                    }
                    
                    stokKartViewModels.Add(new StokKartViewModel
                    {
                        UrunID = urun.UrunID,
                        UrunKodu = urun.UrunKodu,
                        UrunAdi = urun.UrunAdi,
                        Kategori = urun.Kategori?.KategoriAdi ?? "Kategorisiz",
                        Birim = urun.Birim?.BirimAdi ?? "Adet",
                        StokMiktar = dinamikStokMiktari,
                        BirimFiyat = GetUrunBirimFiyat(urun.UrunID)
                    });
                }
                
                // ViewModel oluştur
                var model = new StokRaporViewModel
                {
                    ToplamUrunSayisi = stokKartViewModels.Count,
                    ToplamStokDegeri = stokKartViewModels.Sum(u => u.StokMiktar * u.BirimFiyat),
                    KritikStokUrunSayisi = stokKartViewModels.Count(u => u.StokMiktar <= 10),
                    DusukStokUrunSayisi = stokKartViewModels.Count(u => u.StokMiktar > 10 && u.StokMiktar <= 20),
                    Urunler = stokKartViewModels
                };
                
                // Kategori bazlı rapor
                model.KategoriBazliRapor = await GetKategoriBazliRapor(kategoriID, depoID, stokDurumu);
                
                // Depo bazlı rapor
                model.DepoBazliRapor = await GetDepoBazliRapor(kategoriID, depoID, stokDurumu);
                
                return View(model);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Stok raporu oluşturulurken hata oluştu");
                // Hata durumunda boş model döndür
                return View(new StokRaporViewModel());
            }
        }
        
        // Excel raporu için action
        public IActionResult StokRaporExcel(Guid? kategoriID = null, Guid? depoID = null, string stokDurumu = null)
        {
            // Excel raporu oluşturma işlemleri
            return RedirectToAction(nameof(StokRapor), new { kategoriID, depoID, stokDurumu });
        }
        
        // PDF raporu için action
        public IActionResult StokRaporPdf(Guid? kategoriID = null, Guid? depoID = null, string stokDurumu = null)
        {
            // PDF raporu oluşturma işlemleri
            return RedirectToAction(nameof(StokRapor), new { kategoriID, depoID, stokDurumu });
        }
        
        // Toplam stok değerini hesapla
        private async Task<decimal> HesaplaToplamStokDegeri(List<Urun> urunler)
        {
            decimal toplamDeger = 0;
            
            foreach (var urun in urunler)
            {
                decimal birimFiyat = GetUrunBirimFiyat(urun.UrunID);
                decimal stokMiktar = await _stokService.GetDinamikStokMiktari(urun.UrunID);
                toplamDeger += stokMiktar * birimFiyat;
            }
            
            return toplamDeger;
        }
        
        // Ürün birim fiyatını getir
        private decimal GetUrunBirimFiyat(Guid urunID)
        {
            var urunFiyat = _context.UrunFiyatlari
                .Where(uf => uf.UrunID == urunID && !uf.Silindi)
                .OrderByDescending(uf => uf.GecerliTarih)
                .FirstOrDefault();
                
            return urunFiyat?.Fiyat ?? 0;
        }
        
        // Kategori bazlı rapor oluştur
        private async Task<List<KategoriBazliRaporViewModel>> GetKategoriBazliRapor(Guid? kategoriID, Guid? depoID, string stokDurumu)
        {
            var kategoriler = await _unitOfWork.Repository<UrunKategori>().GetAsync(
                filter: k => k.Silindi == false && k.Aktif,
                orderBy: q => q.OrderBy(k => k.KategoriAdi)
            );
            
            var result = new List<KategoriBazliRaporViewModel>();
            
            foreach (var kategori in kategoriler)
            {
                // Kategori filtresi varsa ve bu kategori değilse atla
                if (kategoriID.HasValue && kategori.KategoriID != kategoriID.Value)
                    continue;
                
                var urunQuery = _context.Urunler
                    .Where(u => !u.Silindi && u.Aktif && u.KategoriID == kategori.KategoriID);
                
                var urunler = await urunQuery.ToListAsync();
                var kategoridekiUrunler = new List<(Guid UrunID, decimal StokMiktar)>();
                
                foreach (var urun in urunler)
                {
                    // Dinamik stok miktarını hesapla
                    decimal dinamikStokMiktari = await _stokService.GetDinamikStokMiktari(urun.UrunID, depoID);
                    
                    // Stok durumu filtresi
                    if (!string.IsNullOrEmpty(stokDurumu))
                    {
                        switch (stokDurumu)
                        {
                            case "kritik":
                                if (dinamikStokMiktari > 10) continue;
                                break;
                            case "dusuk":
                                if (dinamikStokMiktari <= 10 || dinamikStokMiktari > 20) continue;
                                break;
                            case "normal":
                                if (dinamikStokMiktari <= 20) continue;
                                break;
                        }
                    }
                    
                    kategoridekiUrunler.Add((urun.UrunID, dinamikStokMiktari));
                }
                
                if (kategoridekiUrunler.Any())
                {
                    decimal toplamStokDegeri = 0;
                    foreach (var (urunID, stokMiktar) in kategoridekiUrunler)
                    {
                        decimal birimFiyat = GetUrunBirimFiyat(urunID);
                        toplamStokDegeri += stokMiktar * birimFiyat;
                    }
                    
                    result.Add(new KategoriBazliRaporViewModel
                    {
                        KategoriID = kategori.KategoriID,
                        KategoriAdi = kategori.KategoriAdi,
                        UrunSayisi = kategoridekiUrunler.Count,
                        ToplamStokDegeri = toplamStokDegeri,
                        KritikStokUrunSayisi = kategoridekiUrunler.Count(u => u.StokMiktar <= 10),
                        DusukStokUrunSayisi = kategoridekiUrunler.Count(u => u.StokMiktar > 10 && u.StokMiktar <= 20)
                    });
                }
            }
            
            return result;
        }
        
        // Depo bazlı rapor oluştur
        private async Task<List<DepoBazliRaporViewModel>> GetDepoBazliRapor(Guid? kategoriID, Guid? depoID, string stokDurumu)
        {
            var depolar = await _unitOfWork.Repository<Depo>().GetAsync(
                filter: d => d.Silindi == false && d.Aktif,
                orderBy: q => q.OrderBy(d => d.DepoAdi)
            );
            
            var result = new List<DepoBazliRaporViewModel>();
            
            foreach (var depo in depolar)
            {
                // Depo filtresi varsa ve bu depo değilse atla
                if (depoID.HasValue && depo.DepoID != depoID.Value)
                    continue;
                
                // Ürünleri getir ve kategori filtresi uygula
                var urunQuery = _context.Urunler
                    .Where(u => !u.Silindi && u.Aktif);
                
                if (kategoriID.HasValue)
                {
                    urunQuery = urunQuery.Where(u => u.KategoriID == kategoriID.Value);
                }
                
                var urunler = await urunQuery.ToListAsync();
                var depodakiUrunler = new List<(Guid UrunID, decimal StokMiktar)>();
                
                foreach (var urun in urunler)
                {
                    // Bu depodaki stok miktarını hesapla
                    decimal dinamikStokMiktari = await _stokService.GetDinamikStokMiktari(urun.UrunID, depo.DepoID);
                    
                    // Stok yoksa atla
                    if (dinamikStokMiktari <= 0) continue;
                    
                    // Stok durumu filtresi
                    if (!string.IsNullOrEmpty(stokDurumu))
                    {
                        switch (stokDurumu)
                        {
                            case "kritik":
                                if (dinamikStokMiktari > 10) continue;
                                break;
                            case "dusuk":
                                if (dinamikStokMiktari <= 10 || dinamikStokMiktari > 20) continue;
                                break;
                            case "normal":
                                if (dinamikStokMiktari <= 20) continue;
                                break;
                        }
                    }
                    
                    depodakiUrunler.Add((urun.UrunID, dinamikStokMiktari));
                }
                
                if (depodakiUrunler.Any())
                {
                    decimal toplamStokDegeri = 0;
                    foreach (var (urunID, stokMiktar) in depodakiUrunler)
                    {
                        decimal birimFiyat = GetUrunBirimFiyat(urunID);
                        toplamStokDegeri += stokMiktar * birimFiyat;
                    }
                    
                    result.Add(new DepoBazliRaporViewModel
                    {
                        DepoID = depo.DepoID,
                        DepoAdi = depo.DepoAdi,
                        UrunSayisi = depodakiUrunler.Count,
                        ToplamStokDegeri = toplamStokDegeri,
                        KritikStokUrunSayisi = depodakiUrunler.Count(u => u.StokMiktar <= 10),
                        DusukStokUrunSayisi = depodakiUrunler.Count(u => u.StokMiktar > 10 && u.StokMiktar <= 20)
                    });
                }
            }
            
            return result;
        }

        // GET: Stok/Details/5
        public async Task<IActionResult> Details(Guid? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var stok = await _context.Urunler
                .Include(u => u.Kategori)
                .Include(u => u.Birim)
                .FirstOrDefaultAsync(m => m.UrunID == id);
                
            if (stok == null)
            {
                return NotFound();
            }

            return View(stok);
        }

        // Ürünün stok durumunu görüntülemek için metot ekliyorum
        public async Task<IActionResult> Durum(Guid urunId)
        {
            try
            {
                var urunRepository = _unitOfWork.Repository<Urun>();
                var urun = await urunRepository.GetFirstOrDefaultAsync(u => u.UrunID == urunId && !u.Silindi,
                    includeProperties: "Birim,Kategori");
            
                if (urun == null)
                {
                    return View("NotFound");
                }
            
                var depoRepository = _unitOfWork.Repository<Depo>();
                var depolar = await depoRepository.GetAsync(d => !d.Silindi && d.Aktif);
            
                var viewModel = new UrunStokDurumViewModel
                {
                    UrunID = urun.UrunID,
                    UrunKodu = urun.UrunKodu,
                    UrunAdi = urun.UrunAdi,
                    BirimAdi = urun.Birim?.BirimAdi ?? "Adet",
                    KategoriAdi = urun.Kategori?.KategoriAdi ?? "Kategorisiz",
                    GenelStokMiktari = await _stokService.GetDinamikStokMiktari(urunId),
                    DepoStokDurumlari = new List<DepoStokDurumViewModel>()
                };
            
                // Her depo için stok durumunu hesapla
                foreach (var depo in depolar)
                {
                    decimal depoStokMiktari = await _stokService.GetDinamikStokMiktari(urunId, depo.DepoID);
                
                    viewModel.DepoStokDurumlari.Add(new DepoStokDurumViewModel
                    {
                        DepoID = depo.DepoID,
                        DepoAdi = depo.DepoAdi,
                        StokMiktari = depoStokMiktari
                    });
                }
            
                return View(viewModel);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Ürün stok durumu görüntülenirken hata oluştu: UrunID={urunId}");
                return View("Error", new ErrorViewModel
                {
                    Title = "Stok Durumu Hatası",
                    Message = "Ürünün stok durumu görüntülenirken bir hata oluştu.",
                    RequestId = HttpContext.TraceIdentifier
                });
            }
        }

        #region Yardımcı Metotlar

        private async Task PrepareViewBagForStokHareket()
        {
            // Ürünleri getir
            var urunler = await _unitOfWork.Repository<Urun>().GetAsync(
                filter: u => u.Silindi == false && u.Aktif,
                orderBy: q => q.OrderBy(u => u.UrunAdi)
            );
            
            ViewBag.Urunler = new SelectList(urunler, "UrunID", "UrunAdi");
            
            // Depoları getir
            var depolar = await _unitOfWork.Repository<Depo>().GetAsync(
                filter: d => d.Silindi == false && d.Aktif,
                orderBy: q => q.OrderBy(d => d.DepoAdi)
            );
            
            ViewBag.Depolar = new SelectList(depolar, "DepoID", "DepoAdi");
            
            // Birimleri getir
            var birimler = await _unitOfWork.Repository<Birim>().GetAsync(
                filter: b => b.Silindi == false && b.Aktif,
                orderBy: q => q.OrderBy(b => b.BirimAdi)
            );
            
            ViewBag.Birimler = new SelectList(birimler, "BirimID", "BirimAdi");
            
            // Para birimleri için temel değerleri ayarla
            ViewBag.ParaBirimleri = new List<string> { "TRY", "USD", "UZS" };
        }

        private async Task PrepareViewBagForStokSayim()
        {
            // Depoları getir
            var depolar = await _unitOfWork.Repository<Depo>().GetAsync(
                filter: d => d.Silindi == false && d.Aktif,
                orderBy: q => q.OrderBy(d => d.DepoAdi)
            );
            ViewBag.Depolar = new SelectList(depolar, "DepoID", "DepoAdi");
            
            // Para birimlerini getir
            ViewBag.ParaBirimleri = new SelectList(new List<string> { "TRY", "USD","UZS" });
        }

        private bool IsStokGirisi(StokHareket hareket)
        {
            return hareket.HareketTuru == StokHareketTipi.Giris;
        }

        private async Task<decimal> GetToplamStokGirisAsync()
        {
            var toplamGiris = await _context.StokHareketleri
                .Where(s => !s.Silindi && s.HareketTuru == StokHareketTipi.Giris)
                .SumAsync(s => s.Miktar);
            
            return toplamGiris;
        }

        private async Task<decimal> GetToplamStokCikisAsync()
        {
            return await _context.StokHareketleri
                .Where(sh => sh.HareketTuru == StokHareketTipi.Cikis && !sh.Silindi)
                .SumAsync(sh => Math.Abs(sh.Miktar));
        }

        /// <summary>
        /// Para birimi koduna göre sembol döndürür
        /// </summary>
        private string GetParaBirimiSembol(string paraBirimiKodu)
        {
            if (string.IsNullOrEmpty(paraBirimiKodu))
                return "so'm";
                
            switch(paraBirimiKodu.ToUpper())
            {
                case "USD":
                    return "$";
                case "TRY":
                    return "TL";
                case "UZS":
                    return "so'm";
                default:
                    return paraBirimiKodu;
            }
        }

        private async Task<string> GetBirimAdiAsync(Guid urunID)
        {
            try 
            {
                // Önce ürünü al
                var urun = await _context.Urunler
                    .Include(u => u.Birim)
                    .FirstOrDefaultAsync(u => u.UrunID == urunID);
                
                if (urun != null && urun.Birim != null)
                {
                    return urun.Birim.BirimAdi;
                }
                
                // Ürün yoksa veya ürünün bir birimi yoksa
                if (urun != null && urun.BirimID.HasValue)
                {
                    var birim = await _context.Birimler
                        .FirstOrDefaultAsync(b => b.BirimID == urun.BirimID.Value);
                    
                    return birim?.BirimAdi;
                }
                
                return null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Birim adı alınırken hata oluştu. UrunID: {UrunID}", urunID);
                return null;
            }
        }

        #endregion
    }
} 