using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using MuhasebeStokWebApp.Data;
using MuhasebeStokWebApp.Data.Entities;
using MuhasebeStokWebApp.Data.Repositories;
using MuhasebeStokWebApp.Models;
using MuhasebeStokWebApp.ViewModels.Fatura;
using MuhasebeStokWebApp.ViewModels.Kullanici;
using System.Security.Claims;
using System.Text.Json;
using System.Text.Json.Serialization;
using MuhasebeStokWebApp.ViewModels.Irsaliye;
using MuhasebeStokWebApp.Services;
using Microsoft.EntityFrameworkCore.Storage;

namespace MuhasebeStokWebApp.Controllers
{
    // [Authorize]
    public class FaturaController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ApplicationDbContext _context;
        private readonly StokFifoService _stokFifoService;
        private readonly IDovizKuruService _dovizKuruService;

        public FaturaController(IUnitOfWork unitOfWork, ApplicationDbContext context, 
            StokFifoService stokFifoService, IDovizKuruService dovizKuruService)
        {
            _unitOfWork = unitOfWork;
            _context = context;
            _stokFifoService = stokFifoService;
            _dovizKuruService = dovizKuruService;
        }

        // GET: Fatura
        public async Task<IActionResult> Index()
        {
            var faturalar = await _context.Faturalar
                .Include(f => f.Cari)
                .Include(f => f.FaturaTuru)
                .Where(f => !f.SoftDelete)
                .OrderByDescending(f => f.FaturaTarihi)
                .ToListAsync();

            var viewModels = faturalar.Select(f => new FaturaViewModel
            {
                FaturaID = f.FaturaID,
                FaturaNumarasi = f.FaturaNumarasi,
                FaturaTarihi = f.FaturaTarihi,
                VadeTarihi = f.VadeTarihi,
                CariID = f.CariID ?? Guid.Empty,
                CariAdi = f.Cari?.CariAdi,
                FaturaTuru = f.FaturaTuru?.FaturaTuruAdi,
                AraToplam = f.AraToplam ?? 0,
                KdvTutari = f.KDVToplam ?? 0,
                IndirimTutari = 0,
                GenelToplam = f.GenelToplam ?? 0,
                OdenecekTutar = f.GenelToplam ?? 0,
                Aciklama = f.FaturaNotu,
                OdemeDurumu = f.OdemeDurumu,
                Aktif = f.Aktif ?? true
            }).ToList();

            return View(viewModels);
        }

        // GET: Fatura/Create
        public IActionResult Create()
        {
            try
            {
                var cariler = _context.Cariler.Where(c => !c.SoftDelete && c.Aktif)
                    .Select(c => new SelectListItem { Value = c.CariID.ToString(), Text = c.CariAdi })
                    .ToList();
                
                var faturaTurleri = _context.FaturaTurleri
                    .Select(ft => new SelectListItem { Value = ft.FaturaTuruID.ToString(), Text = ft.FaturaTuruAdi })
                    .ToList();
                
                var dovizListesi = new List<SelectListItem>
                {
                    new SelectListItem { Value = "TRY", Text = "Türk Lirası (TRY)" },
                    new SelectListItem { Value = "USD", Text = "Amerikan Doları (USD)" },
                    new SelectListItem { Value = "EUR", Text = "Euro (EUR)" },
                    new SelectListItem { Value = "GBP", Text = "İngiliz Sterlini (GBP)" }
                };
                
                ViewBag.Cariler = new SelectList(cariler, "Value", "Text");
                ViewBag.FaturaTurleri = new SelectList(faturaTurleri, "Value", "Text");
            ViewBag.OdemeTurleri = new SelectList(_context.OdemeTurleri, "OdemeTuruID", "OdemeTuruAdi");
                ViewBag.Urunler = new SelectList(_context.Urunler.Where(u => !u.SoftDelete && u.Aktif), "UrunID", "UrunAdi");
            
                // Varsayılan değerlerle view model oluştur
            var viewModel = new FaturaCreateViewModel
            {
                    FaturaNumarasi = "", // JavaScript ile veya POST'ta doldurulacak
                    SiparisNumarasi = "", // JavaScript ile veya POST'ta doldurulacak
                FaturaTarihi = DateTime.Today,
                VadeTarihi = DateTime.Today.AddDays(30),
                    OdemeDurumu = "Beklemede",
                    FaturaKalemleri = new List<FaturaKalemViewModel>(),
                    DovizTuru = "TRY",
                    DovizKuru = 1,
                    OtomatikIrsaliyeOlustur = false,
                    Aciklama = "",
                    CariListesi = cariler,
                    FaturaTuruListesi = faturaTurleri,
                    DovizListesi = dovizListesi
                };

            return View(viewModel);
        }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"Sayfa yüklenirken bir hata oluştu: {ex.Message}";
                return RedirectToAction(nameof(Index));
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(FaturaCreateViewModel viewModel)
        {
            try
            {
                // Fatura numarası ve sipariş numarası otomatik oluşturma
                if (string.IsNullOrEmpty(viewModel.FaturaNumarasi))
                {
                    viewModel.FaturaNumarasi = GenerateNewFaturaNumber();
                    Console.WriteLine($"Fatura numarası otomatik oluşturuldu: {viewModel.FaturaNumarasi}");
                }
                
                if (string.IsNullOrEmpty(viewModel.SiparisNumarasi))
                {
                    viewModel.SiparisNumarasi = GenerateSiparisNumarasi();
                    Console.WriteLine($"Sipariş numarası otomatik oluşturuldu: {viewModel.SiparisNumarasi}");
                }

                // ModelState.IsValid kontrolü
                if (!ModelState.IsValid)
                {
                    Console.WriteLine("ModelState geçerli değil, hatalar:");
                    foreach (var state in ModelState)
                    {
                        if (state.Value.Errors.Count > 0)
                        {
                            Console.WriteLine($"- {state.Key}: {state.Value.Errors[0].ErrorMessage}");
                        }
                    }
                    
                    // Hata durumunda ViewBag'leri yeniden doldur
                    ViewBag.Cariler = new SelectList(_context.Cariler.Where(c => !c.SoftDelete && c.Aktif), "CariID", "CariAdi", viewModel.CariID);
                    ViewBag.FaturaTurleri = new SelectList(_context.FaturaTurleri, "FaturaTuruID", "FaturaTuruAdi", viewModel.FaturaTuruID);
                    ViewBag.OdemeTurleri = new SelectList(_context.OdemeTurleri, "OdemeTuruID", "OdemeTuruAdi");
                    ViewBag.Urunler = new SelectList(_context.Urunler.Where(u => !u.SoftDelete && u.Aktif), "UrunID", "UrunAdi");
                
                return View(viewModel);
            }

                // Transaction'ı using içinde oluşturmak yerine doğrudan değişkene atıyoruz
                IDbContextTransaction transaction = null;
            
            try
            {
                    // Transaction başlat
                    transaction = await _context.Database.BeginTransactionAsync();
                    
                decimal araToplam = 0;
                decimal kdvTutari = 0;
                decimal genelToplam = 0;

                foreach (var kalem in viewModel.FaturaKalemleri)
                {
                    araToplam += kalem.Tutar;
                    kdvTutari += kalem.KdvTutari;
                    genelToplam += kalem.NetTutar;
                }

                    var fatura = new Fatura
                    {
                        FaturaID = Guid.NewGuid(),
                        FaturaNumarasi = viewModel.FaturaNumarasi,
                        FaturaTarihi = viewModel.FaturaTarihi,
                        VadeTarihi = viewModel.VadeTarihi,
                        CariID = viewModel.CariID,
                    FaturaTuruID = viewModel.FaturaTuruID,
                    AraToplam = araToplam,
                    KDVToplam = kdvTutari,
                    GenelToplam = genelToplam,
                        OdemeDurumu = viewModel.OdemeDurumu ?? "Beklemede",
                    FaturaNotu = viewModel.Aciklama,
                    Resmi = viewModel.Resmi,
                        OlusturmaTarihi = DateTime.Now,
                    SoftDelete = false,
                    Aktif = true,
                    DovizTuru = viewModel.DovizTuru ?? "TRY",
                    DovizKuru = viewModel.DovizKuru ?? 1,
                    SiparisNumarasi = viewModel.SiparisNumarasi
                };

                _context.Faturalar.Add(fatura);
                    Console.WriteLine($"Fatura oluşturuldu: ID={fatura.FaturaID}, FaturaNo={fatura.FaturaNumarasi}");

                if (viewModel.FaturaKalemleri != null && viewModel.FaturaKalemleri.Any())
                {
                        Console.WriteLine($"Fatura kalemleri işleniyor. Toplam {viewModel.FaturaKalemleri.Count} adet kalem var.");
                    foreach (var kalem in viewModel.FaturaKalemleri)
                    {
                        // SatirKdvToplam değeri için KdvTutari değerini kullanıyoruz
                        var satirKdvToplam = kalem.KdvTutari;
                        
                        var faturaDetay = new FaturaDetay
                        {
                            FaturaDetayID = Guid.NewGuid(),
                            FaturaID = fatura.FaturaID,
                            UrunID = kalem.UrunID,
                            Miktar = kalem.Miktar,
                            BirimFiyat = kalem.BirimFiyat,
                            KdvOrani = kalem.KdvOrani,
                            IndirimOrani = kalem.IndirimOrani,
                            SatirToplam = kalem.Tutar,
                            KdvTutari = kalem.KdvTutari,
                            IndirimTutari = kalem.IndirimTutari,
                            NetTutar = kalem.NetTutar,
                            Birim = kalem.Birim,
                            Aciklama = "",
                            OlusturmaTarihi = DateTime.Now,
                            SoftDelete = false,
                            SatirKdvToplam = satirKdvToplam // SatirKdvToplam alanına değer atıyoruz
                        };

                        _context.FaturaDetaylari.Add(faturaDetay);
                            Console.WriteLine($"Fatura detayı eklendi: ID={faturaDetay.FaturaDetayID}, Ürün={kalem.UrunID}, Miktar={kalem.Miktar}");

                        // Stok hareketi oluştur
                        var faturaTuru = await _context.FaturaTurleri.FindAsync(viewModel.FaturaTuruID);
                        if (faturaTuru != null)
                    {
                                // Stok hareketi oluştur
                        var stokHareket = new StokHareket
                        {
                            StokHareketID = Guid.NewGuid(),
                            UrunID = kalem.UrunID,
                                HareketTuru = faturaTuru.HareketTuru,
                                Miktar = faturaTuru.HareketTuru == "Giriş" ? kalem.Miktar : -kalem.Miktar,
                            BirimFiyat = kalem.BirimFiyat,
                                    Tarih = viewModel.FaturaTarihi,
                                Aciklama = $"{viewModel.FaturaNumarasi} numaralı fatura",
                                ReferansNo = viewModel.FaturaNumarasi,
                                ReferansTuru = "Fatura",
                            ReferansID = fatura.FaturaID,
                                OlusturmaTarihi = DateTime.Now,
                                SoftDelete = false,
                                Birim = kalem.Birim
                            };

                            _context.StokHareketleri.Add(stokHareket);
                                Console.WriteLine($"Stok hareketi oluşturuldu: ID={stokHareket.StokHareketID}, Tür={faturaTuru.HareketTuru}, Miktar={stokHareket.Miktar}");

                                // Ürün bilgilerini al
                            var urun = await _context.Urunler.FindAsync(kalem.UrunID);
                                
                                // Önce veritabanı değişikliklerini kaydedelim ve transaction'ı commit edelim
                                // böylece FIFO servisinde yeni bir transaction başlatılabilir
                                await _context.SaveChangesAsync();
                                await transaction.CommitAsync();
                                transaction.Dispose();
                                transaction = null;
                                
                                // FIFO kaydı oluştur
                                try 
                                {
                                    decimal kdvOrani = (decimal)kalem.KdvOrani;
                                    decimal kdvsizBirimFiyat = kalem.BirimFiyat; // BirimFiyat zaten KDV'siz değer
                                    
                                if (faturaTuru.HareketTuru == "Giriş")
                                    {
                                        // Stok girişi ve FIFO kaydı oluştur
                                        await _stokFifoService.StokGirisiYap(
                                            kalem.UrunID, 
                                            kalem.Miktar, 
                                            kdvsizBirimFiyat, // KDV'siz fiyat
                                            kalem.Birim,
                                            viewModel.FaturaNumarasi,
                                            "Fatura",
                                            fatura.FaturaID,
                                            $"{viewModel.FaturaNumarasi} numaralı alış faturası",
                                            viewModel.DovizTuru ?? "TRY",
                                            viewModel.DovizKuru
                                        );
                                        Console.WriteLine($"Stok girişi yapıldı: Ürün={kalem.UrunID}, Miktar={kalem.Miktar}");
                                        
                                        // Ürün stok miktarını güncelle
                                        if (urun != null)
                                {
                                    urun.StokMiktar += kalem.Miktar;
                                            _context.Urunler.Update(urun);
                                            Console.WriteLine($"Ürün stok miktarı güncellendi: Ürün={urun.UrunID}, Yeni Miktar={urun.StokMiktar}");
                                        }
                                    }
                                    else // "Çıkış"
                                    {
                                        // Stok çıkışı ve FIFO kaydı güncelleme
                                        var (kullanilanFifoKayitlari, toplamMaliyet) = await _stokFifoService.StokCikisiYap(
                                            kalem.UrunID,
                                            kalem.Miktar,
                                            viewModel.FaturaNumarasi,
                                            "Fatura",
                                            fatura.FaturaID,
                                            $"{viewModel.FaturaNumarasi} numaralı satış faturası"
                                        );
                                        Console.WriteLine($"Stok çıkışı yapıldı: Ürün={kalem.UrunID}, Miktar={kalem.Miktar}, Maliyet={toplamMaliyet}");
                                        
                                        // Ürün stok miktarını güncelle
                                        if (urun != null)
                                {
                                    urun.StokMiktar -= kalem.Miktar;
                                _context.Urunler.Update(urun);
                                            Console.WriteLine($"Ürün stok miktarı güncellendi: Ürün={urun.UrunID}, Yeni Miktar={urun.StokMiktar}");
                                        }
                                    }
                                    
                                    // Yeni bir transaction başlat
                                    transaction = await _context.Database.BeginTransactionAsync();
                                }
                                catch (StokYetersizException ex)
                                {
                                    // Stok yetersiz hatası
                                    TempData["ErrorMessage"] = ex.Message;
                                    Console.WriteLine($"Stok yetersiz hatası: {ex.Message}");
                                    return RedirectToAction(nameof(Create));
                                }
                                catch (Exception ex)
                                {
                                    // Genel hata
                                    TempData["ErrorMessage"] = $"FIFO kaydı oluşturulurken hata oluştu: {ex.Message}";
                                    Console.WriteLine($"FIFO hatası: {ex.Message}");
                                    return RedirectToAction(nameof(Create));
                                }
                            }
                    }
                    }

                    // Cari hareket oluştur
                if (viewModel.CariID != Guid.Empty)
                {
                    var faturaTuru = await _context.FaturaTurleri.FindAsync(viewModel.FaturaTuruID);
                    string hareketTuru = "Borç"; // Varsayılan olarak borç

                    if (faturaTuru != null)
                    {
                        // Fatura türüne göre hareket türünü belirle
                        if (faturaTuru.FaturaTuruAdi.ToLower().Contains("satış") || 
                            faturaTuru.FaturaTuruAdi.ToLower().Contains("satis"))
                        {
                            hareketTuru = "Borç"; // Müşteri borçlanır
                        }
                        else if (faturaTuru.FaturaTuruAdi.ToLower().Contains("alış") || 
                                faturaTuru.FaturaTuruAdi.ToLower().Contains("alis"))
                        {
                            hareketTuru = "Alacak"; // Tedarikçiden alacaklanırız
                        }
                    }

                    var cariHareket = new CariHareket
                    {
                        CariHareketID = Guid.NewGuid(),
                        CariID = viewModel.CariID,
                        Tutar = genelToplam,
                        HareketTuru = hareketTuru,
                        Tarih = viewModel.FaturaTarihi,
                        ReferansNo = viewModel.FaturaNumarasi,
                        ReferansTuru = "Fatura",
                        ReferansID = fatura.FaturaID,
                        Aciklama = $"{viewModel.FaturaNumarasi} numaralı fatura",
                        OlusturmaTarihi = DateTime.Now,
                        SoftDelete = false
                    };

                    _context.CariHareketler.Add(cariHareket);
                        Console.WriteLine($"Cari hareket oluşturuldu: ID={cariHareket.CariHareketID}, Tür={hareketTuru}, Tutar={genelToplam}");
                }

                // İrsaliye ile ilişkilendirme
                if (TempData["IrsaliyeID"] != null)
                {
                    string irsaliyeIdStr = TempData["IrsaliyeID"].ToString();
                    if (Guid.TryParse(irsaliyeIdStr, out Guid irsaliyeId))
                    {
                        var irsaliye = await _context.Irsaliyeler.FindAsync(irsaliyeId);
                        if (irsaliye != null)
                        {
                            irsaliye.FaturaID = fatura.FaturaID;
                            irsaliye.Durum = "Kapalı"; // İrsaliye durumunu kapalı olarak güncelle
                            _context.Irsaliyeler.Update(irsaliye);
                                Console.WriteLine($"İrsaliye fatura ile ilişkilendirildi: İrsaliye={irsaliye.IrsaliyeID}, Fatura={fatura.FaturaID}");
                        }
                    }
                }

                await _context.SaveChangesAsync();
                    Console.WriteLine("Değişiklikler veritabanına kaydedildi");
                    
                    // Transaction commit et
                    if (transaction != null)
                    {
                        await transaction.CommitAsync();
                        transaction.Dispose();
                    }
                    
                    // Otomatik irsaliye oluşturma
                    if (viewModel.OtomatikIrsaliyeOlustur)
                    {
                        try 
                        {
                            // Fatura türünü kontrol et (sadece satış faturaları için irsaliye oluştur)
                            var faturaTuru = await _context.FaturaTurleri.FindAsync(viewModel.FaturaTuruID);
                            if (faturaTuru != null && (faturaTuru.FaturaTuruAdi.ToLower().Contains("satış") || faturaTuru.FaturaTuruAdi.ToLower().Contains("satis")))
                            {
                                // Yeni transaction başlat
                                using var irsaliyeTransaction = await _context.Database.BeginTransactionAsync();
                                
                                try
                                {
                                    // İrsaliye numarası oluştur
                                    string irsaliyeNumarasi = GenerateNewIrsaliyeNumber();
                                    
                                    // İrsaliye oluştur
                        var irsaliye = new Irsaliye
                        {
                            IrsaliyeID = Guid.NewGuid(),
                                        IrsaliyeNumarasi = irsaliyeNumarasi,
                                        IrsaliyeTarihi = viewModel.FaturaTarihi, // Fatura tarihi ile aynı
                                        CariID = viewModel.CariID,
                                        FaturaID = fatura.FaturaID, // Faturayla ilişkilendir
                                        Aciklama = $"{viewModel.FaturaNumarasi} numaralı faturaya ait otomatik oluşturulan irsaliye",
                            OlusturmaTarihi = DateTime.Now,
                                        Durum = "Kapalı", // Faturalandığı için kapalı
                                        SoftDelete = false,
                            Aktif = true
                        };
                        
                                    _context.Irsaliyeler.Add(irsaliye);
                                    
                                    // İrsaliye detaylarını oluştur (fatura kalemleri ile aynı)
                                    foreach (var kalem in viewModel.FaturaKalemleri)
                                    {
                                        var irsaliyeDetay = new IrsaliyeDetay
                                    {
                                        IrsaliyeDetayID = Guid.NewGuid(),
                                        IrsaliyeID = irsaliye.IrsaliyeID,
                                            UrunID = kalem.UrunID,
                                            Miktar = kalem.Miktar,
                                            Birim = kalem.Birim,
                                            Aciklama = "",
                                        OlusturmaTarihi = DateTime.Now,
                                        SoftDelete = false
                                        };
                                        
                                        _context.IrsaliyeDetaylari.Add(irsaliyeDetay);
                                    }
                                    
                        await _context.SaveChangesAsync();
                                    await irsaliyeTransaction.CommitAsync();
                                    
                                    TempData["SuccessMessage"] += $" İrsaliye ({irsaliyeNumarasi}) otomatik olarak oluşturuldu.";
                                    Console.WriteLine($"Otomatik irsaliye oluşturuldu: ID={irsaliye.IrsaliyeID}, İrsaliye No={irsaliye.IrsaliyeNumarasi}");
                                }
                                catch (Exception ex)
                                {
                                    await irsaliyeTransaction.RollbackAsync();
                                    // İrsaliye oluşturma hatası faturayı etkilemeyecek
                                    TempData["WarningMessage"] = $"Fatura kaydedildi ancak otomatik irsaliye oluşturulurken hata oluştu: {ex.Message}";
                                    Console.WriteLine($"İrsaliye oluşturma hatası: {ex.Message}");
                                }
                            }
                            else
                            {
                                // Alış faturaları için uyarı mesajı
                                TempData["InfoMessage"] = "Fatura kaydedildi. Alış faturasıyla ilişkili irsaliye oluşturulmaz.";
                            }
                }
                catch (Exception ex)
                {
                            // İrsaliye işlemi genel hatası
                            TempData["WarningMessage"] = $"Fatura kaydedildi ancak otomatik irsaliye oluşturulurken bir hata oluştu: {ex.Message}";
                            Console.WriteLine($"İrsaliye işlemi hatası: {ex.Message}");
                        }
                    }
                    
                    // Başarılı mesajı
                    TempData["SuccessMessage"] = $"{viewModel.FaturaNumarasi} numaralı fatura başarıyla kaydedildi.";
                    
                    return RedirectToAction(nameof(Index));
                }
                catch (Exception ex)
                {
                    // Hata durumunda
                    if (transaction != null)
                    {
                        await transaction.RollbackAsync();
                        transaction.Dispose();
                    }
                    
                    TempData["ErrorMessage"] = $"Fatura kaydedilirken bir hata oluştu: {ex.Message}";
                    Console.WriteLine($"Fatura kaydedilirken hata: {ex.Message}");
                    return RedirectToAction(nameof(Create));
                }
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"Fatura kaydedilirken bir hata oluştu: {ex.Message}";
                Console.WriteLine($"Dış try bloğunda hata: {ex.Message}");
                return RedirectToAction(nameof(Create));
            }
        }

        // DELETE: Fatura/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(Guid id)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();

                try
                {
                var fatura = await _context.Faturalar.FindAsync(id);
                    if (fatura == null)
                    {
                        return NotFound();
                    }

                // İlişkili stok hareketlerini bul
                var stokHareketleri = await _context.StokHareketleri
                    .Where(sh => sh.ReferansID == fatura.FaturaID && sh.ReferansTuru == "Fatura" && !sh.SoftDelete)
                    .ToListAsync();

                // Fatura detaylarını bul
                var faturaDetaylari = await _context.FaturaDetaylari
                    .Where(fd => fd.FaturaID == fatura.FaturaID && !fd.SoftDelete)
                    .ToListAsync();

                // Fatura türünü bul
                var faturaTuru = await _context.FaturaTurleri.FindAsync(fatura.FaturaTuruID);

                        if (faturaTuru != null)
                        {
                    // Stok hareketlerini iptal et
                    foreach (var stokHareket in stokHareketleri)
                    {
                        stokHareket.SoftDelete = true;
                        stokHareket.GuncellemeTarihi = DateTime.Now;
                        _context.StokHareketleri.Update(stokHareket);
                    }

                    // FIFO kayıtlarını iptal et
                    await _stokFifoService.FifoKayitlariniIptalEt(
                        fatura.FaturaID,
                        "Fatura",
                        $"{fatura.FaturaNumarasi} numaralı fatura silindi",
                        null
                    );

                    // Ürün stok miktarlarını güncelle
                    if (faturaTuru.HareketTuru == "Giriş")
                    {
                        // Alış faturası ise stoktan düş
                        foreach (var detay in faturaDetaylari)
                        {
                            var urun = await _context.Urunler.FindAsync(detay.UrunID);
                            if (urun != null)
                            {
                                urun.StokMiktar -= detay.Miktar;
                                _context.Urunler.Update(urun);
                            }
                        }
                    }
                    else // "Çıkış"
                    {
                        // Satış faturası ise stoğa ekle
                        foreach (var detay in faturaDetaylari)
                        {
                            var urun = await _context.Urunler.FindAsync(detay.UrunID);
                            if (urun != null)
                            {
                                urun.StokMiktar += detay.Miktar;
                                _context.Urunler.Update(urun);
                            }
                        }
                    }
                }

                // Cari hareketlerini iptal et
                var cariHareketler = await _context.CariHareketler
                    .Where(ch => ch.ReferansID == fatura.FaturaID && ch.ReferansTuru == "Fatura" && !ch.SoftDelete)
                    .ToListAsync();

                foreach (var cariHareket in cariHareketler)
                {
                    cariHareket.SoftDelete = true;
                    cariHareket.GuncellemeTarihi = DateTime.Now;
                    _context.CariHareketler.Update(cariHareket);
                }

                // Faturayla ilişkili detayları iptal et
                foreach (var detay in faturaDetaylari)
                {
                    detay.SoftDelete = true;
                    detay.GuncellemeTarihi = DateTime.Now;
                    _context.FaturaDetaylari.Update(detay);
                }

                // Faturayı iptal et
                fatura.SoftDelete = true;
                fatura.GuncellemeTarihi = DateTime.Now;
                _context.Faturalar.Update(fatura);

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();
            
            return RedirectToAction(nameof(Index));
        }
            catch (Exception ex)
        {
                await transaction.RollbackAsync();
                TempData["ErrorMessage"] = $"Fatura silinirken hata oluştu: {ex.Message}";
                return RedirectToAction(nameof(Index));
            }
        }

        // Ürün bilgilerini getiren API metodu
        [HttpGet]
        public async Task<IActionResult> GetUrunBilgileri(Guid id)
        {
            try
            {
                var urun = await _context.Urunler
                    .Include(u => u.Birim)
                    .FirstOrDefaultAsync(u => u.UrunID == id);
                
            if (urun == null)
            {
                return NotFound();
            }

                // Ürünün en güncel fiyatını bul
                decimal birimFiyat = 0;
            var urunFiyat = await _context.UrunFiyatlari
                .Where(uf => uf.UrunID == id && !uf.SoftDelete)
                .OrderByDescending(uf => uf.GecerliTarih)
                .FirstOrDefaultAsync();

                if (urunFiyat != null)
                {
                    birimFiyat = urunFiyat.Fiyat;
                }

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
                return BadRequest($"Ürün bilgileri alınamadı: {ex.Message}");
            }
        }

        // Gerekli yardımcı metotlar
        private string GenerateNewFaturaNumber()
        {
            // Tarih formatını hazırla
            string tarihKismi = DateTime.Now.ToString("yyMMdd");
            
            // Mevcut en yüksek fatura numarasını bul
            var lastFatura = _context.Faturalar
                .Where(f => f.FaturaNumarasi.StartsWith("FTR-" + tarihKismi))
                .OrderByDescending(f => f.FaturaNumarasi)
                .FirstOrDefault();
            
            if (lastFatura != null)
            {
                // Mevcut en son sıra numarasını al
                string lastNumberStr = lastFatura.FaturaNumarasi.Substring(11); // "FTR-YYMMDD-" sonrası
                
                // Son numarayı parse et
                if (int.TryParse(lastNumberStr, out int lastNumber))
                {
                    // Yeni numara = son numara + 1
                    return $"FTR-{tarihKismi}-{(lastNumber + 1).ToString("D3")}";
                }
            }
            
            // Eğer hiç fatura yoksa veya numara parse edilemezse ilk numarayı döndür
            return $"FTR-{tarihKismi}-001";
        }

        private string GenerateSiparisNumarasi()
        {
            // Tarih formatını hazırla
            string tarihKismi = DateTime.Now.ToString("yyMMdd");
            
            // Mevcut en yüksek sipariş numarasını bul
            var lastSiparis = _context.Faturalar
                .Where(f => f.SiparisNumarasi.StartsWith("SIP-" + tarihKismi))
                .OrderByDescending(f => f.SiparisNumarasi)
                .FirstOrDefault();
            
            if (lastSiparis != null)
            {
                // Mevcut en son sıra numarasını al
                string lastNumberStr = lastSiparis.SiparisNumarasi.Substring(11); // "SIP-YYMMDD-" sonrası
                
                // Son numarayı parse et
                if (int.TryParse(lastNumberStr, out int lastNumber))
                {
                    // Yeni numara = son numara + 1
                    return $"SIP-{tarihKismi}-{(lastNumber + 1).ToString("D3")}";
                }
            }
            
            // Eğer hiç sipariş yoksa veya numara parse edilemezse ilk numarayı döndür
            return $"SIP-{tarihKismi}-001";
        }

        // GET: Fatura/GetNewFaturaNumber
        [HttpGet]
        public IActionResult GetNewFaturaNumber()
        {
            string faturaNumarasi = GenerateNewFaturaNumber();
            return Json(faturaNumarasi);
        }

        // GET: Fatura/GetNewSiparisNumarasi
        [HttpGet]
        public IActionResult GetNewSiparisNumarasi()
        {
            string siparisNumarasi = GenerateSiparisNumarasi();
            return Json(siparisNumarasi);
        }

        // İrsaliye numarası oluşturma metodu
        private string GenerateNewIrsaliyeNumber()
        {
            // Tarih formatını hazırla
            string tarihKismi = DateTime.Now.ToString("yyMMdd");
            
            // Mevcut en yüksek irsaliye numarasını bul
            var lastIrsaliye = _context.Irsaliyeler
                .Where(i => i.IrsaliyeNumarasi.StartsWith("IRS-" + tarihKismi))
                .OrderByDescending(i => i.IrsaliyeNumarasi)
                .FirstOrDefault();
            
            if (lastIrsaliye != null)
            {
                // Mevcut en son sıra numarasını al
                string lastNumberStr = lastIrsaliye.IrsaliyeNumarasi.Substring(11); // "IRS-YYMMDD-" sonrası
                
                // Son numarayı parse et
                if (int.TryParse(lastNumberStr, out int lastNumber))
                {
                    // Yeni numara = son numara + 1
                    return $"IRS-{tarihKismi}-{(lastNumber + 1).ToString("D3")}";
                }
            }
            
            // Eğer hiç irsaliye yoksa veya numara parse edilemezse ilk numarayı döndür
            return $"IRS-{tarihKismi}-001";
        }

        // GET: Fatura/Details/5
        public async Task<IActionResult> Details(Guid? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var fatura = await _context.Faturalar
                .Include(f => f.Cari)
                .Include(f => f.FaturaDetaylari)
                .ThenInclude(fd => fd.Urun)
                .Include(f => f.FaturaTuru)
                .Include(f => f.Irsaliyeler)
                .Include(f => f.FaturaOdemeleri)
                .FirstOrDefaultAsync(f => f.FaturaID == id && !f.SoftDelete);

            if (fatura == null)
            {
                return NotFound();
            }

            // Fatura ödemelerini al
            var faturanınOdemeleri = await _context.FaturaOdemeleri
                .Where(o => o.FaturaID == id && !o.SoftDelete)
                .ToListAsync();

            // Toplam ödeme tutarını hesapla
            decimal odenenTutar = faturanınOdemeleri.Sum(o => o.OdemeTutari);

            // Sistem ayarlarını al - hata alınmaması için try/catch içine alındı
            try
            {
                var sistemAyarlari = await _context.SistemAyarlari.FirstOrDefaultAsync();
            }
            catch (Exception)
            {
                // Sistem ayarları alınamazsa devam et
            }
            
            // Fatura view model
            var viewModel = new FaturaDetailViewModel
            {
                FaturaID = fatura.FaturaID,
                FaturaNumarasi = fatura.FaturaNumarasi,
                FaturaTarihi = fatura.FaturaTarihi,
                VadeTarihi = fatura.VadeTarihi,
                CariID = fatura.CariID ?? Guid.Empty,
                CariAdi = fatura.Cari?.CariAdi ?? "Belirtilmemiş",
                CariVergiNo = fatura.Cari?.VergiNo ?? "Belirtilmemiş",
                CariAdres = fatura.Cari?.Adres ?? "Belirtilmemiş",
                CariTelefon = fatura.Cari?.Telefon ?? "Belirtilmemiş",
                FaturaTuru = fatura.FaturaTuru?.FaturaTuruAdi ?? "Belirtilmemiş",
                AraToplam = fatura.AraToplam ?? 0,
                KdvTutari = fatura.KDVToplam ?? 0,
                IndirimTutari = 0, // Faturada indirim tutarı yoksa 0 olarak atanacak
                GenelToplam = fatura.GenelToplam ?? 0,
                OdenecekTutar = (fatura.GenelToplam ?? 0) - odenenTutar,
                OdenenTutar = odenenTutar,
                OdemeDurumu = fatura.OdemeDurumu ?? "Belirtilmemiş",
                Aciklama = fatura.FaturaNotu,
                DovizTuru = fatura.DovizTuru ?? "TRY",
                OlusturmaTarihi = fatura.OlusturmaTarihi,
                GuncellemeTarihi = fatura.GuncellemeTarihi,
                Aktif = fatura.Aktif ?? true,

                // Faturanın ilişkili irsaliyesi var mı kontrol et
                IrsaliyeID = fatura.Irsaliyeler.FirstOrDefault(i => i.Aktif && !i.SoftDelete)?.IrsaliyeID,

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
                    Tutar = fd.SatirToplam ?? 0,
                    KdvTutari = fd.KdvTutari ?? 0,
                    IndirimTutari = fd.IndirimTutari ?? 0,
                    NetTutar = fd.NetTutar ?? 0,
                    Birim = fd.Birim
                }).ToList(),
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
                .Include(f => f.FaturaDetaylari)
                    .ThenInclude(fd => fd.Urun)
                .Include(f => f.FaturaTuru)
                .FirstOrDefaultAsync(f => f.FaturaID == id && !f.SoftDelete);

            if (fatura == null)
            {
                return NotFound();
            }

            // Cariler için SelectList hazırla
            var cariler = _context.Cariler.Where(c => !c.SoftDelete && c.Aktif)
                    .Select(c => new SelectListItem { Value = c.CariID.ToString(), Text = c.CariAdi })
                    .ToList();
                
            // Fatura türleri için SelectList hazırla
            var faturaTurleri = _context.FaturaTurleri
                .Select(ft => new SelectListItem { Value = ft.FaturaTuruID.ToString(), Text = ft.FaturaTuruAdi })
                .ToList();

            // Döviz listesi hazırla
            var dovizListesi = new List<SelectListItem>
            {
                new SelectListItem { Value = "TRY", Text = "Türk Lirası (TRY)" },
                new SelectListItem { Value = "USD", Text = "Amerikan Doları (USD)" },
                new SelectListItem { Value = "EUR", Text = "Euro (EUR)" },
                new SelectListItem { Value = "GBP", Text = "İngiliz Sterlini (GBP)" }
            };

            var viewModel = new FaturaEditViewModel
            {
                FaturaID = fatura.FaturaID,
                FaturaNumarasi = fatura.FaturaNumarasi,
                SiparisNumarasi = fatura.SiparisNumarasi,
                FaturaTarihi = fatura.FaturaTarihi,
                VadeTarihi = fatura.VadeTarihi,
                CariID = fatura.CariID ?? Guid.Empty,
                CariAdi = fatura.Cari?.CariAdi ?? "Belirtilmemiş",
                FaturaTuruID = fatura.FaturaTuruID,
                FaturaTuru = fatura.FaturaTuru?.FaturaTuruAdi ?? "Belirtilmemiş",
                Resmi = fatura.Resmi ?? true,
                Aciklama = fatura.FaturaNotu,
                OdemeDurumu = fatura.OdemeDurumu,
                DovizTuru = fatura.DovizTuru ?? "TRY",
                DovizKuru = fatura.DovizKuru ?? 1,
                CariListesi = cariler,
                FaturaTuruListesi = faturaTurleri,
                DovizListesi = dovizListesi,
                FaturaKalemleri = fatura.FaturaDetaylari.Select(fd => new FaturaKalemViewModel
                {
                    KalemID = fd.FaturaDetayID,
                    UrunID = fd.UrunID,
                    UrunAdi = fd.Urun?.UrunAdi ?? "Belirtilmemiş",
                    Miktar = fd.Miktar,
                    BirimFiyat = fd.BirimFiyat,
                    KdvOrani = (int)fd.KdvOrani,
                    IndirimOrani = (int)fd.IndirimOrani,
                    Tutar = fd.SatirToplam ?? 0,
                    KdvTutari = fd.KdvTutari ?? 0,
                    IndirimTutari = fd.IndirimTutari ?? 0,
                    NetTutar = fd.NetTutar ?? 0,
                    Birim = fd.Birim
                }).ToList()
            };

            // Hata varsa, dropdownları tekrar yükle
            ViewBag.Cariler = new SelectList(_context.Cariler.Where(c => !c.SoftDelete && c.Aktif), "CariID", "CariAdi", viewModel.CariID);
            ViewBag.FaturaTurleri = new SelectList(_context.FaturaTurleri, "FaturaTuruID", "FaturaTuruAdi", viewModel.FaturaTuruID);
            ViewBag.OdemeTurleri = new SelectList(_context.OdemeTurleri, "OdemeTuruID", "OdemeTuruAdi");
            ViewBag.Urunler = new SelectList(_context.Urunler.Where(u => !u.SoftDelete && u.Aktif), "UrunID", "UrunAdi");
            
            // Döviz listesi
            var dovizListesi2 = new List<SelectListItem>
            {
                new SelectListItem { Value = "TRY", Text = "Türk Lirası (TRY)" },
                new SelectListItem { Value = "USD", Text = "Amerikan Doları (USD)" },
                new SelectListItem { Value = "EUR", Text = "Euro (EUR)" },
                new SelectListItem { Value = "GBP", Text = "İngiliz Sterlini (GBP)" }
            };
            viewModel.DovizListesi = dovizListesi2;

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
                    // Mevcut faturayı getir
                    var fatura = await _unitOfWork.Repository<Fatura>().GetByIdAsync(viewModel.FaturaID);
                    if (fatura == null)
                    {
                        return NotFound();
                    }

                    // Fatura bilgilerini güncelle
                    fatura.FaturaNumarasi = viewModel.FaturaNumarasi;
                    fatura.SiparisNumarasi = viewModel.SiparisNumarasi;
                    fatura.FaturaTarihi = viewModel.FaturaTarihi;
                    fatura.VadeTarihi = viewModel.VadeTarihi;
                    fatura.CariID = viewModel.CariID;
                    fatura.FaturaTuruID = viewModel.FaturaTuruID;
                    fatura.DovizTuru = viewModel.DovizTuru;
                    fatura.DovizKuru = viewModel.DovizKuru;
                    fatura.FaturaNotu = viewModel.Aciklama;
                    fatura.OdemeDurumu = viewModel.OdemeDurumu;
                    fatura.GuncellemeTarihi = DateTime.Now;
                    fatura.SonGuncelleyenKullaniciID = GetCurrentUserId();

                    // Transaction başlat
                    using var transaction = await _context.Database.BeginTransactionAsync();

                    try
                    {
                        // Faturayı güncelle
                        await _unitOfWork.Repository<Fatura>().UpdateAsync(fatura);

                        // Mevcut fatura kalemlerini sil (soft delete)
                        var mevcutKalemler = await _context.FaturaDetaylari
                            .Where(fd => fd.FaturaID == fatura.FaturaID && !fd.SoftDelete)
                            .ToListAsync();

                        foreach (var kalem in mevcutKalemler)
                        {
                            kalem.SoftDelete = true;
                            kalem.GuncellemeTarihi = DateTime.Now;
                            kalem.SonGuncelleyenKullaniciID = GetCurrentUserId();
                            _context.Update(kalem);
                        }

                        // Yeni kalemleri ekle
                        if (viewModel.FaturaKalemleri != null && viewModel.FaturaKalemleri.Any())
                        {
                            decimal araToplam = 0;
                            decimal kdvToplam = 0;
                            decimal genelToplam = 0;

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

                                var faturaDetay = new FaturaDetay
                                {
                                    FaturaDetayID = Guid.NewGuid(),
                                    FaturaID = fatura.FaturaID,
                                    UrunID = kalem.UrunID,
                                    Miktar = miktar,
                                    Birim = kalem.Birim,
                                    BirimFiyat = birimFiyat,
                                    KdvOrani = kalem.KdvOrani,
                                    IndirimOrani = kalem.IndirimOrani,
                                    Tutar = tutar,
                                    KdvTutari = kdvTutar,
                                    NetTutar = toplamTutar,
                                    Aciklama = kalem.Aciklama,
                                    OlusturmaTarihi = DateTime.Now,
                                    OlusturanKullaniciID = GetCurrentUserId(),
                                    SoftDelete = false
                                };

                                await _context.FaturaDetaylari.AddAsync(faturaDetay);
                            }

                            // Fatura toplamlarını güncelle
                            fatura.AraToplam = araToplam;
                            fatura.KDVToplam = kdvToplam;
                            fatura.GenelToplam = genelToplam;
                            _context.Faturalar.Update(fatura);
                        }

                        await _context.SaveChangesAsync();
                        await transaction.CommitAsync();

                        return RedirectToAction(nameof(Details), new { id = fatura.FaturaID });
                    }
                    catch (Exception ex)
                    {
                        await transaction.RollbackAsync();
                        ModelState.AddModelError("", $"Fatura güncellenirken bir hata oluştu: {ex.Message}");
                    }
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError("", $"Fatura güncellenirken bir hata oluştu: {ex.Message}");
                }
            }

            // Hata varsa, dropdownları tekrar yükle
            ViewBag.Cariler = new SelectList(_context.Cariler.Where(c => !c.SoftDelete && c.Aktif), "CariID", "CariAdi", viewModel.CariID);
            ViewBag.FaturaTurleri = new SelectList(_context.FaturaTurleri, "FaturaTuruID", "FaturaTuruAdi", viewModel.FaturaTuruID);
            ViewBag.OdemeTurleri = new SelectList(_context.OdemeTurleri, "OdemeTuruID", "OdemeTuruAdi");
            ViewBag.Urunler = new SelectList(_context.Urunler.Where(u => !u.SoftDelete && u.Aktif), "UrunID", "UrunAdi");
            
            // Döviz listesi
            var dovizListesi2 = new List<SelectListItem>
            {
                new SelectListItem { Value = "TRY", Text = "Türk Lirası (TRY)" },
                new SelectListItem { Value = "USD", Text = "Amerikan Doları (USD)" },
                new SelectListItem { Value = "EUR", Text = "Euro (EUR)" },
                new SelectListItem { Value = "GBP", Text = "İngiliz Sterlini (GBP)" }
            };
            viewModel.DovizListesi = dovizListesi2;

            return View(viewModel);
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
                .FirstOrDefaultAsync(m => m.FaturaID == id && !m.SoftDelete);

            if (fatura == null)
            {
                return NotFound();
            }

            var viewModel = new FaturaDetailViewModel
            {
                FaturaID = fatura.FaturaID,
                FaturaNumarasi = fatura.FaturaNumarasi,
                FaturaTarihi = fatura.FaturaTarihi,
                VadeTarihi = fatura.VadeTarihi,
                CariID = fatura.CariID ?? Guid.Empty,
                CariAdi = fatura.Cari?.CariAdi ?? "Belirtilmemiş",
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
                    KdvTutari = fd.KdvTutari ?? 0,
                    IndirimTutari = fd.IndirimTutari ?? 0,
                    NetTutar = fd.NetTutar ?? 0,
                    Birim = fd.Birim
                }).ToList()
            };

            return View(viewModel);
        }

        // POST: Fatura/AddOdeme
        [HttpPost]
        public async Task<IActionResult> AddOdeme(Guid faturaId, decimal odemeAmount, string odemeTuru, DateTime odemeTarihi, string aciklama)
        {
            if (faturaId == Guid.Empty)
            {
                return Json(new { success = false, message = "Fatura ID geçerli değil." });
            }

            try
            {
                // Fatura bilgilerini al
                var fatura = await _context.Faturalar
                    .Include(f => f.Cari)
                    .Include(f => f.FaturaTuru)
                    .FirstOrDefaultAsync(f => f.FaturaID == faturaId && !f.SoftDelete);

                if (fatura == null)
                {
                    return Json(new { success = false, message = "Fatura bulunamadı." });
                }

                // Ödeme kaydı oluştur
                var odeme = new FaturaOdeme
                {
                    OdemeID = Guid.NewGuid(),
                    FaturaID = faturaId,
                    OdemeTarihi = odemeTarihi,
                    OdemeTutari = odemeAmount,
                    OdemeTuru = odemeTuru,
                    Aciklama = aciklama ?? $"{fatura.FaturaNumarasi} numaralı fatura için ödeme",
                    OlusturmaTarihi = DateTime.Now,
                    SoftDelete = false
                };

                // Cari hareket kaydı oluştur
                var cariHareket = new CariHareket
                {
                    CariHareketID = Guid.NewGuid(),
                    CariID = fatura.CariID.Value,
                    Tutar = odemeAmount,
                    HareketTuru = fatura.FaturaTuru.FaturaTuruAdi.ToLower().Contains("satış") ? "Alacak" : "Borç", // Satış faturasına ödeme yapılırsa Alacak, Alış faturasına ödeme yapılırsa Borç
                    Tarih = odemeTarihi,
                    ReferansNo = $"{fatura.FaturaNumarasi}-Ödeme",
                    ReferansTuru = "Ödeme",
                    ReferansID = odeme.OdemeID, // Ödeme referansı
                    Aciklama = aciklama ?? $"{fatura.FaturaNumarasi} numaralı fatura için ödeme",
                    OlusturmaTarihi = DateTime.Now,
                    SoftDelete = false
                };

                // Ödeme durumunu güncelle
                decimal toplamOdemelerTutari = _context.FaturaOdemeleri
                    .Where(o => o.FaturaID == faturaId && !o.SoftDelete)
                    .Sum(o => o.OdemeTutari) + odemeAmount; // Yeni ödeme dahil

                if (toplamOdemelerTutari >= fatura.GenelToplam)
                {
                    fatura.OdemeDurumu = "Ödendi";
                }
                else if (toplamOdemelerTutari > 0)
                {
                    fatura.OdemeDurumu = "Kısmi Ödendi";
                }

                // Veritabanına kaydet
                _context.FaturaOdemeleri.Add(odeme);
                _context.CariHareketler.Add(cariHareket);
                _context.Faturalar.Update(fatura);
                await _context.SaveChangesAsync();

                return Json(new { success = true, message = "Ödeme başarıyla eklendi.", odemeDurumu = fatura.OdemeDurumu });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = $"Ödeme eklenirken bir hata oluştu: {ex.Message}" });
            }
        }

        // GET: Fatura/CreateIrsaliye/5
        public async Task<IActionResult> CreateIrsaliye(Guid? id)
        {
            if (id == null)
            {
                return NotFound();
            }
            
            // Fatura bilgilerini kontrol et
            var fatura = await _context.Faturalar
                .Include(f => f.Cari)
                .Include(f => f.FaturaDetaylari)
                .ThenInclude(fd => fd.Urun)
                .FirstOrDefaultAsync(f => f.FaturaID == id && !f.SoftDelete);
                
            if (fatura == null)
            {
                return NotFound();
            }

            // Fatura zaten bir irsaliye ile ilişkilendirilmiş mi kontrol et
            var existingIrsaliye = await _context.Irsaliyeler
                .FirstOrDefaultAsync(i => i.FaturaID == id && i.Aktif);
                
            if (existingIrsaliye != null)
            {
                TempData["ErrorMessage"] = "Bu fatura zaten bir irsaliye ile ilişkilendirilmiştir.";
                return RedirectToAction(nameof(Details), new { id = id });
            }
            
            // İrsaliyeController'daki CreateFromFatura action'ına yönlendir
            return RedirectToAction("CreateFromFatura", "Irsaliye", new { faturaId = id });
        }

        // Mevcut kullanıcı ID'sini almak için yardımcı metod
        private Guid GetCurrentUserId()
        {
            try
            {
                var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
                if (userIdClaim != null)
                {
                    return Guid.Parse(userIdClaim.Value);
                }
            }
            catch
            {
                // Hata durumunda işlem yapma
            }
            return Guid.Empty;
        }
    }
}
