using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using MuhasebeStokWebApp.Data;
using MuhasebeStokWebApp.Data.Entities;
using MuhasebeStokWebApp.Data.Repositories;
using MuhasebeStokWebApp.Services;
using MuhasebeStokWebApp.ViewModels.Stok;
using Microsoft.AspNetCore.Identity;

namespace MuhasebeStokWebApp.Controllers
{
    public class StokController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ApplicationDbContext _context;
        private readonly StokFifoService _stokFifoService;
        private readonly IDovizService _dovizService;
        private readonly UserManager<IdentityUser> _userManager;

        public StokController(
            IUnitOfWork unitOfWork, 
            ApplicationDbContext context, 
            StokFifoService stokFifoService,
            IDovizService dovizService,
            UserManager<IdentityUser> userManager)
        {
            _unitOfWork = unitOfWork;
            _context = context;
            _stokFifoService = stokFifoService;
            _dovizService = dovizService;
            _userManager = userManager;
        }

        // GET: Stok
        public async Task<IActionResult> Index(string searchString)
        {
            var urunRepository = _unitOfWork.Repository<Urun>();
            
            // Ürün listesini getir (filtreleme yaparak)
            var urunler = await urunRepository.GetAsync(
                filter: u => u.SoftDelete == false && 
                    (string.IsNullOrEmpty(searchString) || 
                     u.UrunAdi.Contains(searchString) || 
                     u.UrunKodu.Contains(searchString)),
                orderBy: q => q.OrderBy(u => u.UrunAdi),
                includeProperties: "Birim,Kategori"
            );
            
            var viewModel = new StokListViewModel
            {
                Urunler = urunler.Select(u => new StokKartViewModel
                {
                    UrunID = u.UrunID,
                    UrunKodu = u.UrunKodu,
                    UrunAdi = u.UrunAdi,
                    Birim = u.Birim?.BirimAdi ?? "-",
                    StokMiktar = u.StokMiktar,
                    Kategori = u.Kategori?.KategoriAdi ?? "Kategorisiz",
                    BirimID = u.BirimID,
                    KategoriID = u.KategoriID
                }).ToList()
            };

            return View(viewModel);
        }

        // GET: Stok/Hareketler/5
        public async Task<IActionResult> Hareketler(Guid id)
        {
            var urun = await _unitOfWork.Repository<Urun>().GetFirstOrDefaultAsync(
                filter: u => u.UrunID == id && u.SoftDelete == false,
                includeProperties: "Birim,Kategori"
            );

            if (urun == null)
            {
                return NotFound();
            }

            // Ürünün stok hareketlerini getir
            var stokHareketleri = await _unitOfWork.Repository<StokHareket>().GetAsync(
                filter: sh => sh.UrunID == id && sh.SoftDelete == false,
                orderBy: q => q.OrderByDescending(sh => sh.Tarih)
            );

            // FIFO kayıtlarını getir
            var fifoKayitlari = await _stokFifoService.GetFifoKayitlari(id);

            var viewModel = new StokHareketViewModel
            {
                UrunID = urun.UrunID,
                UrunKodu = urun.UrunKodu,
                UrunAdi = urun.UrunAdi,
                Birim = urun.Birim?.BirimAdi ?? "-",
                Kategori = urun.Kategori?.KategoriAdi ?? "Kategorisiz",
                StokMiktar = urun.StokMiktar,
                
                StokHareketleri = stokHareketleri.Select(sh => new StokHareketDetayViewModel
                {
                    StokHareketID = sh.StokHareketID,
                    Tarih = sh.Tarih,
                    HareketTuru = sh.HareketTuru,
                    Miktar = sh.Miktar,
                    Birim = sh.Birim,
                    ReferansNo = sh.ReferansNo,
                    ReferansTuru = sh.ReferansTuru,
                    Aciklama = sh.Aciklama,
                    DepoAdi = sh.Depo?.DepoAdi
                }).ToList(),
                
                FifoKayitlari = fifoKayitlari.Select(fk => new FifoKayitViewModel
                {
                    StokFifoID = fk.StokFifoID,
                    GirisTarihi = fk.GirisTarihi,
                    SonCikisTarihi = fk.SonCikisTarihi,
                    Miktar = fk.Miktar,
                    KalanMiktar = fk.KalanMiktar,
                    BirimFiyat = fk.BirimFiyat,
                    ParaBirimi = fk.ParaBirimi,
                    ReferansNo = fk.ReferansNo,
                    ReferansTuru = fk.ReferansTuru,
                    Aciklama = fk.Aciklama,
                    Aktif = fk.Aktif,
                    Iptal = fk.Iptal
                }).ToList()
            };

            // Ortalama maliyet hesapla (TL ve USD cinsinden)
            viewModel.OrtalamaMaliyetTL = await _stokFifoService.GetOrtalamaMaliyet(id, "TRY");
            viewModel.OrtalamaMaliyetUSD = await _stokFifoService.GetOrtalamaMaliyet(id, "USD");

            return View(viewModel);
        }

        // GET: Stok/StokGiris
        public async Task<IActionResult> StokGiris()
        {
            await PrepareViewBagForStokHareket();
            
            var viewModel = new StokGirisViewModel
            {
                Tarih = DateTime.Now,
                HareketTuru = "Giriş"
            };
            
            return View(viewModel);
        }

        // POST: Stok/StokGiris
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> StokGiris(StokGirisViewModel viewModel)
        {
            if (ModelState.IsValid)
            {
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

                    // Ürün stok miktarını güncelle
                    urun.StokMiktar += viewModel.Miktar;
                    await _unitOfWork.Repository<Urun>().UpdateAsync(urun);

                    // FIFO stok girişi yap
                    // Döviz kuru al (TRY/USD)
                    decimal dovizKuru = 1;
                    try
                    {
                        // 1 TL kaç USD ediyor hesapla
                        decimal oneTL = await _dovizService.ParaBirimiCevirAsync("TRY", "USD", 1);
                        dovizKuru = oneTL > 0 ? oneTL : 1/30.0m; // 1 TL = 1/30 USD yaklaşık
                    }
                    catch (Exception ex)
                    {
                        // Kur bulunamazsa varsayılan değer kullan
                        Console.WriteLine($"Döviz kuru alınamadı: {ex.Message}");
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
                            "TRY",
                            dovizKuru
                        );

                        await _unitOfWork.SaveAsync();

                        TempData["SuccessMessage"] = "Stok girişi başarıyla kaydedildi.";
                        return RedirectToAction(nameof(Index));
                    }
                    catch (Exception ex)
                    {
                        ModelState.AddModelError("", $"Stok girişi sırasında bir hata oluştu: {ex.Message}");
                    }
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError("", $"Stok girişi sırasında bir hata oluştu: {ex.Message}");
                }
            }

            await PrepareViewBagForStokHareket();
            return View(viewModel);
        }

        // GET: Stok/StokCikis
        public async Task<IActionResult> StokCikis()
        {
            await PrepareViewBagForStokHareket();
            
            var viewModel = new StokCikisViewModel
            {
                Tarih = DateTime.Now,
                HareketTuru = "Çıkış"
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
                        HareketTuru = viewModel.HareketTuru,
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

                        // Ürün stok miktarını güncelle
                        urun.StokMiktar -= viewModel.Miktar;
                        await _unitOfWork.Repository<Urun>().UpdateAsync(urun);

                        await _unitOfWork.SaveAsync();

                        TempData["SuccessMessage"] = "Stok çıkışı başarıyla kaydedildi.";
                        return RedirectToAction(nameof(Index));
                    }
                    catch (StokYetersizException ex)
                    {
                        ModelState.AddModelError("", ex.Message);
                        TempData["ErrorMessage"] = ex.Message;
                        TempData["ErrorDetails"] = $"Ürün: {ex.UrunAdi} ({ex.UrunKodu}), Talep edilen: {ex.TalepEdilenMiktar}, Mevcut: {ex.MevcutMiktar}";
                    }
                    catch (Exception ex)
                    {
                        ModelState.AddModelError("", $"Stok çıkışı sırasında bir hata oluştu: {ex.Message}");
                    }
                }
                catch (Exception ex)
                {
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
                        HareketTuru = "Çıkış",
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
                        HareketTuru = "Giriş",
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

            // Sayım için her ürün için işlem yap
            foreach (var urunSayim in viewModel.UrunListesi)
            {
                // Ürünü bul
                var urun = await _unitOfWork.Repository<Urun>().GetByIdAsync(urunSayim.UrunID);
                if (urun == null)
                {
                    ModelState.AddModelError("", $"Ürün bulunamadı: {urunSayim.UrunID}");
                    await PrepareViewBagForStokSayim();
                    return View(viewModel);
                }

                // Mevcut stok ile sayım arasındaki farkı hesapla
                decimal fark = urunSayim.SayimMiktari - urun.StokMiktar;
                decimal miktar = Math.Abs(fark);

                // Stok hareketi oluştur
                var stokHareket = new StokHareket
                {
                    StokHareketID = Guid.NewGuid(),
                    UrunID = urunSayim.UrunID,
                    DepoID = viewModel.DepoID,
                    HareketTuru = fark >= 0 ? "Giriş" : "Çıkış",
                    Miktar = miktar,
                    Birim = urunSayim.Birim,
                    Tarih = viewModel.Tarih,
                    ReferansNo = viewModel.ReferansNo,
                    ReferansTuru = "Sayım",
                    Aciklama = $"Stok sayımı: {viewModel.Aciklama}",
                    OlusturmaTarihi = DateTime.Now
                };

                await _unitOfWork.Repository<StokHareket>().AddAsync(stokHareket);

                // Ürün stok miktarını güncelle
                urun.StokMiktar = urunSayim.SayimMiktari;
                await _unitOfWork.Repository<Urun>().UpdateAsync(urun);

                // Eğer fark pozitif ise (stok fazlası), FIFO girişi yap
                if (fark > 0)
                {
                    // Ürünün son fiyatını al
                    var urunFiyat = await _context.UrunFiyatlari
                        .Where(uf => uf.UrunID == urunSayim.UrunID && !uf.SoftDelete && uf.FiyatTipiID == 3) // Satış fiyatı
                        .OrderByDescending(uf => uf.GecerliTarih)
                        .FirstOrDefaultAsync();
                            
                    decimal birimFiyat = urunFiyat?.Fiyat ?? 0;
                    
                    // Döviz kuru al (TRY/USD)
                    decimal dovizKuru = 1;
                    try
                    {
                        // 1 TL kaç USD ediyor hesapla
                        decimal oneTL = await _dovizService.ParaBirimiCevirAsync("TRY", "USD", 1);
                        dovizKuru = oneTL > 0 ? oneTL : 1/30.0m; // 1 TL = 1/30 USD yaklaşık
                    }
                    catch (Exception ex)
                    {
                        // Kur bulunamazsa varsayılan değer kullan
                        Console.WriteLine($"Döviz kuru alınamadı: {ex.Message}");
                    }
                    
                    await _stokFifoService.StokGirisiYap(
                        urunSayim.UrunID,
                        miktar,
                        birimFiyat,
                        urunSayim.Birim,
                        "Sayım Farkı",
                        "Sayım",
                        stokHareket.StokHareketID,
                        $"Sayım fazlası (Sayım: {urunSayim.SayimMiktari}, Önceki: {urun.StokMiktar})",
                        "TRY",
                        dovizKuru
                    );
                }
                // Eğer fark negatif ise (stok eksiği), FIFO çıkışı yap
                else if (fark < 0)
                {
                    try
                    {
                        var result = await _stokFifoService.StokCikisiYap(
                            urunSayim.UrunID,
                            miktar,
                            "Sayım Farkı",
                            "Sayım",
                            stokHareket.StokHareketID,
                            $"Sayım eksiği (Sayım: {urunSayim.SayimMiktari}, Önceki: {urun.StokMiktar})"
                        );
                        
                        // Birim fiyatı hesapla (toplam maliyet / miktar)
                        stokHareket.BirimFiyat = miktar > 0 ? result.Item2 / miktar : 0;
                        await _unitOfWork.Repository<StokHareket>().UpdateAsync(stokHareket);
                    }
                    catch (StokYetersizException ex)
                    {
                        // Sayım farkı için stok yetersizliği hatası yok sayılır ve mevcut stok sıfırlanır
                        TempData["WarningMessage"] = $"Stok yetersizliği: {ex.Message}. Sayım kaydedildi ancak FIFO kayıtları tam olarak güncellenemedi.";
                    }
                }
            }

            await _unitOfWork.SaveAsync();
            TempData["SuccessMessage"] = "Stok sayımı başarıyla kaydedildi.";
            return RedirectToAction(nameof(Index));
        }

        // GET: Stok/FifoDetay/5
        public async Task<IActionResult> FifoDetay(Guid id)
        {
            var fifoKayit = await _context.StokFifo
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
                USDBirimFiyat = fifoKayit.USDBirimFiyat,
                TLBirimFiyat = fifoKayit.TLBirimFiyat,
                UZSBirimFiyat = fifoKayit.UZSBirimFiyat,
                ReferansNo = fifoKayit.ReferansNo,
                ReferansTuru = fifoKayit.ReferansTuru,
                Aciklama = fifoKayit.Aciklama,
                Aktif = fifoKayit.Aktif,
                Iptal = fifoKayit.Iptal,
                IptalTarihi = fifoKayit.IptalTarihi,
                IptalAciklama = fifoKayit.IptalAciklama
            };

            return View(viewModel);
        }

        // GET: Stok/StokDurumu
        public async Task<IActionResult> StokDurumu()
        {
            var urunler = await _unitOfWork.Repository<Urun>().GetAsync(
                filter: u => u.SoftDelete == false && u.Aktif,
                orderBy: q => q.OrderBy(u => u.UrunAdi),
                includeProperties: "Birim,Kategori"
            );

            // Stok durumu detay listesi
            var stokDurumuDetayListesi = new List<StokDurumuDetayViewModel>();

            decimal toplamStokDegeri = 0;

            foreach (var urun in urunler)
            {
                decimal maliyet = await _stokFifoService.GetOrtalamaMaliyet(urun.UrunID);
                decimal maliyetTL = await _stokFifoService.GetOrtalamaMaliyet(urun.UrunID, "TRY");
                
                // Ürünün son fiyatını al
                var urunFiyat = await _context.UrunFiyatlari
                    .Where(uf => uf.UrunID == urun.UrunID && !uf.SoftDelete && uf.FiyatTipiID == 3) // Satış fiyatı
                    .OrderByDescending(uf => uf.GecerliTarih)
                    .FirstOrDefaultAsync();

                var stokDurumuDetay = new StokDurumuDetayViewModel
                {
                    UrunID = urun.UrunID,
                    UrunKodu = urun.UrunKodu,
                    UrunAdi = urun.UrunAdi,
                    Birim = urun.Birim?.BirimAdi ?? "-",
                    Kategori = urun.Kategori?.KategoriAdi ?? "Kategorisiz",
                    StokMiktari = urun.StokMiktar,
                    OrtalamaMaliyet = maliyetTL,
                    SatisFiyati = urunFiyat?.Fiyat ?? 0,
                    DepoID = Guid.Empty, // Varsayılan olarak boş, gerekirse depo bazlı filtreleme eklenebilir
                    DepoAdi = "Genel", // Varsayılan olarak genel, gerekirse depo bazlı filtreleme eklenebilir
                    KritikStokSeviyesi = 10 // Varsayılan değer, gerekirse ürün bazlı kritik stok seviyesi eklenebilir
                };

                stokDurumuDetayListesi.Add(stokDurumuDetay);
                toplamStokDegeri += maliyetTL * urun.StokMiktar;
            }

            // Ana ViewModel
            var stokDurumuViewModel = new StokDurumuViewModel
            {
                StokDurumuListesi = stokDurumuDetayListesi,
                ToplamStokDegeri = toplamStokDegeri
            };

            return View(stokDurumuViewModel);
        }

        // GET: Stok/StokRapor
        public async Task<IActionResult> StokRapor(Guid? kategoriID = null, Guid? depoID = null, string stokDurumu = null)
        {
            try
            {
                // Kategorileri getir
                var kategoriler = await _unitOfWork.Repository<UrunKategori>().GetAsync(
                    filter: k => k.SoftDelete == false && k.Aktif,
                    orderBy: q => q.OrderBy(k => k.KategoriAdi)
                );
                
                ViewBag.Kategoriler = new SelectList(kategoriler, "KategoriID", "KategoriAdi");
                ViewBag.SelectedKategoriID = kategoriID;
                
                // Depoları getir
                var depolar = await _unitOfWork.Repository<Depo>().GetAsync(
                    filter: d => d.SoftDelete == false && d.Aktif,
                    orderBy: q => q.OrderBy(d => d.DepoAdi)
                );
                
                ViewBag.Depolar = new SelectList(depolar, "DepoID", "DepoAdi");
                ViewBag.SelectedDepoID = depoID;
                ViewBag.SelectedStokDurumu = stokDurumu;
                
                // Ürünleri getir
                var urunQuery = _context.Urunler
                    .Include(u => u.Kategori)
                    .Include(u => u.Birim)
                    .Where(u => !u.SoftDelete && u.Aktif);
                
                // Kategori filtresi
                if (kategoriID.HasValue)
                {
                    urunQuery = urunQuery.Where(u => u.KategoriID == kategoriID.Value);
                }
                
                // Stok durumu filtresi
                if (!string.IsNullOrEmpty(stokDurumu))
                {
                    switch (stokDurumu)
                    {
                        case "kritik":
                            urunQuery = urunQuery.Where(u => u.StokMiktar <= 10);
                            break;
                        case "dusuk":
                            urunQuery = urunQuery.Where(u => u.StokMiktar > 10 && u.StokMiktar <= 20);
                            break;
                        case "normal":
                            urunQuery = urunQuery.Where(u => u.StokMiktar > 20);
                            break;
                    }
                }
                
                var urunler = await urunQuery.OrderBy(u => u.UrunAdi).ToListAsync();
                
                // Depo filtresi için stok hareketlerini getir
                if (depoID.HasValue)
                {
                    // Depo bazlı filtreleme için stok hareketlerini getir
                    var stokHareketleri = await _context.StokHareketleri
                        .Where(sh => !sh.SoftDelete && sh.DepoID == depoID.Value)
                        .GroupBy(sh => sh.UrunID)
                        .Select(g => new { UrunID = g.Key, ToplamMiktar = g.Sum(sh => sh.HareketTuru == "Giriş" ? sh.Miktar : -sh.Miktar) })
                        .ToListAsync();
                    
                    // Sadece seçili depoda stok bulunan ürünleri filtrele
                    var depoUrunIDs = stokHareketleri.Where(sh => sh.ToplamMiktar > 0).Select(sh => sh.UrunID).ToList();
                    urunler = urunler.Where(u => depoUrunIDs.Contains(u.UrunID)).ToList();
                }
                
                // ViewModel oluştur
                var model = new StokRaporViewModel
                {
                    ToplamUrunSayisi = urunler.Count,
                    ToplamStokDegeri = await HesaplaToplamStokDegeri(urunler),
                    KritikStokUrunSayisi = urunler.Count(u => u.StokMiktar <= 10),
                    DusukStokUrunSayisi = urunler.Count(u => u.StokMiktar > 10 && u.StokMiktar <= 20),
                    
                    Urunler = urunler.Select(u => new StokKartViewModel
                    {
                        UrunID = u.UrunID,
                        UrunKodu = u.UrunKodu,
                        UrunAdi = u.UrunAdi,
                        Kategori = u.Kategori?.KategoriAdi ?? "Kategorisiz",
                        Birim = u.Birim?.BirimAdi ?? "Adet",
                        StokMiktar = u.StokMiktar,
                        BirimFiyat = GetUrunBirimFiyat(u.UrunID)
                    }).ToList()
                };
                
                // Kategori bazlı rapor
                model.KategoriBazliRapor = await GetKategoriBazliRapor(kategoriID, depoID, stokDurumu);
                
                // Depo bazlı rapor
                model.DepoBazliRapor = await GetDepoBazliRapor(kategoriID, depoID, stokDurumu);
                
                return View(model);
            }
            catch (Exception ex)
            {
                // Hata durumunda boş model döndür
                return View(new StokRaporViewModel());
            }
        }
        
        // Excel raporu için action
        public async Task<IActionResult> StokRaporExcel(Guid? kategoriID = null, Guid? depoID = null, string stokDurumu = null)
        {
            // Excel raporu oluşturma işlemleri
            return RedirectToAction(nameof(StokRapor), new { kategoriID, depoID, stokDurumu });
        }
        
        // PDF raporu için action
        public async Task<IActionResult> StokRaporPdf(Guid? kategoriID = null, Guid? depoID = null, string stokDurumu = null)
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
                toplamDeger += urun.StokMiktar * birimFiyat;
            }
            
            return toplamDeger;
        }
        
        // Ürün birim fiyatını getir
        private decimal GetUrunBirimFiyat(Guid urunID)
        {
            var urunFiyat = _context.UrunFiyatlari
                .Where(uf => uf.UrunID == urunID && !uf.SoftDelete)
                .OrderByDescending(uf => uf.GecerliTarih)
                .FirstOrDefault();
                
            return urunFiyat?.Fiyat ?? 0;
        }
        
        // Kategori bazlı rapor oluştur
        private async Task<List<KategoriBazliRaporViewModel>> GetKategoriBazliRapor(Guid? kategoriID, Guid? depoID, string stokDurumu)
        {
            var kategoriler = await _unitOfWork.Repository<UrunKategori>().GetAsync(
                filter: k => k.SoftDelete == false && k.Aktif,
                orderBy: q => q.OrderBy(k => k.KategoriAdi)
            );
            
            var result = new List<KategoriBazliRaporViewModel>();
            
            foreach (var kategori in kategoriler)
            {
                // Kategori filtresi varsa ve bu kategori değilse atla
                if (kategoriID.HasValue && kategori.KategoriID != kategoriID.Value)
                    continue;
                
                var urunQuery = _context.Urunler
                    .Where(u => !u.SoftDelete && u.Aktif && u.KategoriID == kategori.KategoriID);
                
                // Stok durumu filtresi
                if (!string.IsNullOrEmpty(stokDurumu))
                {
                    switch (stokDurumu)
                    {
                        case "kritik":
                            urunQuery = urunQuery.Where(u => u.StokMiktar <= 10);
                            break;
                        case "dusuk":
                            urunQuery = urunQuery.Where(u => u.StokMiktar > 10 && u.StokMiktar <= 20);
                            break;
                        case "normal":
                            urunQuery = urunQuery.Where(u => u.StokMiktar > 20);
                            break;
                    }
                }
                
                var urunler = await urunQuery.ToListAsync();
                
                // Depo filtresi için stok hareketlerini getir
                if (depoID.HasValue)
                {
                    // Depo bazlı filtreleme için stok hareketlerini getir
                    var stokHareketleri = await _context.StokHareketleri
                        .Where(sh => !sh.SoftDelete && sh.DepoID == depoID.Value)
                        .GroupBy(sh => sh.UrunID)
                        .Select(g => new { UrunID = g.Key, ToplamMiktar = g.Sum(sh => sh.HareketTuru == "Giriş" ? sh.Miktar : -sh.Miktar) })
                        .ToListAsync();
                    
                    // Sadece seçili depoda stok bulunan ürünleri filtrele
                    var depoUrunIDs = stokHareketleri.Where(sh => sh.ToplamMiktar > 0).Select(sh => sh.UrunID).ToList();
                    urunler = urunler.Where(u => depoUrunIDs.Contains(u.UrunID)).ToList();
                }
                
                if (urunler.Any())
                {
                    decimal toplamStokDegeri = 0;
                    foreach (var urun in urunler)
                    {
                        decimal birimFiyat = GetUrunBirimFiyat(urun.UrunID);
                        toplamStokDegeri += urun.StokMiktar * birimFiyat;
                    }
                    
                    result.Add(new KategoriBazliRaporViewModel
                    {
                        KategoriID = kategori.KategoriID,
                        KategoriAdi = kategori.KategoriAdi,
                        UrunSayisi = urunler.Count,
                        ToplamStokDegeri = toplamStokDegeri,
                        KritikStokUrunSayisi = urunler.Count(u => u.StokMiktar <= 10),
                        DusukStokUrunSayisi = urunler.Count(u => u.StokMiktar > 10 && u.StokMiktar <= 20)
                    });
                }
            }
            
            return result;
        }
        
        // Depo bazlı rapor oluştur
        private async Task<List<DepoBazliRaporViewModel>> GetDepoBazliRapor(Guid? kategoriID, Guid? depoID, string stokDurumu)
        {
            var depolar = await _unitOfWork.Repository<Depo>().GetAsync(
                filter: d => d.SoftDelete == false && d.Aktif,
                orderBy: q => q.OrderBy(d => d.DepoAdi)
            );
            
            var result = new List<DepoBazliRaporViewModel>();
            
            foreach (var depo in depolar)
            {
                // Depo filtresi varsa ve bu depo değilse atla
                if (depoID.HasValue && depo.DepoID != depoID.Value)
                    continue;
                
                // Bu depodaki stok hareketlerini getir
                var stokHareketleri = await _context.StokHareketleri
                    .Where(sh => !sh.SoftDelete && sh.DepoID == depo.DepoID)
                    .GroupBy(sh => sh.UrunID)
                    .Select(g => new { UrunID = g.Key, ToplamMiktar = g.Sum(sh => sh.HareketTuru == "Giriş" ? sh.Miktar : -sh.Miktar) })
                    .ToListAsync();
                
                // Stok bulunan ürün ID'lerini al
                var depoUrunIDs = stokHareketleri.Where(sh => sh.ToplamMiktar > 0).Select(sh => sh.UrunID).ToList();
                
                if (depoUrunIDs.Any())
                {
                    var urunQuery = _context.Urunler
                        .Where(u => !u.SoftDelete && u.Aktif && depoUrunIDs.Contains(u.UrunID));
                    
                    // Kategori filtresi
                    if (kategoriID.HasValue)
                    {
                        urunQuery = urunQuery.Where(u => u.KategoriID == kategoriID.Value);
                    }
                    
                    // Stok durumu filtresi
                    if (!string.IsNullOrEmpty(stokDurumu))
                    {
                        switch (stokDurumu)
                        {
                            case "kritik":
                                urunQuery = urunQuery.Where(u => u.StokMiktar <= 10);
                                break;
                            case "dusuk":
                                urunQuery = urunQuery.Where(u => u.StokMiktar > 10 && u.StokMiktar <= 20);
                                break;
                            case "normal":
                                urunQuery = urunQuery.Where(u => u.StokMiktar > 20);
                                break;
                        }
                    }
                    
                    var urunler = await urunQuery.ToListAsync();
                    
                    if (urunler.Any())
                    {
                        decimal toplamStokDegeri = 0;
                        foreach (var urun in urunler)
                        {
                            decimal birimFiyat = GetUrunBirimFiyat(urun.UrunID);
                            // Depodaki miktar
                            var depoMiktar = stokHareketleri.FirstOrDefault(sh => sh.UrunID == urun.UrunID)?.ToplamMiktar ?? 0;
                            toplamStokDegeri += depoMiktar * birimFiyat;
                        }
                        
                        result.Add(new DepoBazliRaporViewModel
                        {
                            DepoID = depo.DepoID,
                            DepoAdi = depo.DepoAdi,
                            UrunSayisi = urunler.Count,
                            ToplamStokDegeri = toplamStokDegeri,
                            KritikStokUrunSayisi = urunler.Count(u => u.StokMiktar <= 10),
                            DusukStokUrunSayisi = urunler.Count(u => u.StokMiktar > 10 && u.StokMiktar <= 20)
                        });
                    }
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

        #region Yardımcı Metotlar

        private async Task PrepareViewBagForStokHareket()
        {
            // Ürünleri getir
            var urunler = await _unitOfWork.Repository<Urun>().GetAsync(
                filter: u => u.SoftDelete == false && u.Aktif,
                orderBy: q => q.OrderBy(u => u.UrunAdi)
            );
            
            ViewBag.Urunler = new SelectList(urunler, "UrunID", "UrunAdi");
            
            // Depoları getir
            var depolar = await _unitOfWork.Repository<Depo>().GetAsync(
                filter: d => d.SoftDelete == false && d.Aktif,
                orderBy: q => q.OrderBy(d => d.DepoAdi)
            );
            
            ViewBag.Depolar = new SelectList(depolar, "DepoID", "DepoAdi");
            
            // Birimleri getir
            var birimler = await _unitOfWork.Repository<Birim>().GetAsync(
                filter: b => b.SoftDelete == false && b.Aktif,
                orderBy: q => q.OrderBy(b => b.BirimAdi)
            );
            
            ViewBag.Birimler = new SelectList(birimler, "BirimID", "BirimAdi");
        }

        private async Task PrepareViewBagForStokSayim()
        {
            // Ürünleri getir
            var urunler = await _unitOfWork.Repository<Urun>().GetAsync(
                filter: u => u.SoftDelete == false && u.Aktif,
                orderBy: q => q.OrderBy(u => u.UrunAdi)
            );
            
            ViewBag.Urunler = new SelectList(urunler, "UrunID", "UrunAdi");
            
            // Depoları getir
            var depolar = await _unitOfWork.Repository<Depo>().GetAsync(
                filter: d => d.SoftDelete == false && d.Aktif,
                orderBy: q => q.OrderBy(d => d.DepoAdi)
            );
            
            ViewBag.Depolar = new SelectList(depolar, "DepoID", "DepoAdi");
            
            // Birimleri getir
            var birimler = await _unitOfWork.Repository<Birim>().GetAsync(
                filter: b => b.SoftDelete == false && b.Aktif,
                orderBy: q => q.OrderBy(b => b.BirimAdi)
            );
            
            ViewBag.Birimler = new SelectList(birimler, "BirimID", "BirimAdi");
        }

        #endregion
    }
} 