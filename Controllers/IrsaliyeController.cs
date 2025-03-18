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
using MuhasebeStokWebApp.ViewModels.Irsaliye;
using MuhasebeStokWebApp.Services;
using System.Text.Json;

namespace MuhasebeStokWebApp.Controllers
{
    public class IrsaliyeController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ApplicationDbContext _context;
        private readonly StokFifoService _stokFifoService;
        private readonly IKurService _kurService;

        public IrsaliyeController(IUnitOfWork unitOfWork, ApplicationDbContext context, 
            StokFifoService stokFifoService, IKurService kurService)
        {
            _unitOfWork = unitOfWork;
            _context = context;
            _stokFifoService = stokFifoService;
            _kurService = kurService;
        }

        // GET: Irsaliye
        public async Task<IActionResult> Index(string searchString, string irsaliyeTuru, string durum)
        {
            var irsaliyeler = await _unitOfWork.Repository<Irsaliye>().GetAsync(
                includeProperties: "Cari,Fatura");

            var viewModelList = irsaliyeler.Select(i => new IrsaliyeViewModel
            {
                IrsaliyeID = i.IrsaliyeID,
                IrsaliyeNumarasi = i.IrsaliyeNumarasi,
                IrsaliyeTarihi = i.IrsaliyeTarihi,
                SevkTarihi = i.SevkTarihi,
                CariID = i.CariID,
                CariAdi = i.Cari.CariAdi,
                IrsaliyeTuru = i.IrsaliyeTuru,
                FaturaID = i.FaturaID,
                FaturaNumarasi = i.Fatura != null ? i.Fatura.FaturaNumarasi : null,
                Aciklama = i.Aciklama,
                Durum = i.Durum,
                OlusturmaTarihi = i.OlusturmaTarihi,
                GuncellemeTarihi = i.GuncellemeTarihi
            }).ToList();

            // Arama filtresi uygula
            if (!string.IsNullOrEmpty(searchString))
            {
                searchString = searchString.ToLower();
                viewModelList = viewModelList.Where(i =>
                    i.IrsaliyeNumarasi.ToLower().Contains(searchString) ||
                    i.CariAdi.ToLower().Contains(searchString) ||
                    (i.FaturaNumarasi != null && i.FaturaNumarasi.ToLower().Contains(searchString))
                ).ToList();
            }

            // İrsaliye türü filtresi
            if (!string.IsNullOrEmpty(irsaliyeTuru))
            {
                viewModelList = viewModelList.Where(i => i.IrsaliyeTuru == irsaliyeTuru).ToList();
            }

            // Durum filtresi
            if (!string.IsNullOrEmpty(durum))
            {
                viewModelList = viewModelList.Where(i => i.Durum == durum).ToList();
            }

            // İrsaliye türleri için SelectList
            ViewBag.IrsaliyeTurleri = new SelectList(new List<string> { "Giriş", "Çıkış" });
            
            // Durum için SelectList
            ViewBag.Durumlar = new SelectList(new List<string> { "Açık", "Kapalı", "İptal" });

            return View(viewModelList);
        }

        // GET: Irsaliye/Details/5
        public async Task<IActionResult> Details(Guid id)
        {
            if (id == Guid.Empty)
            {
                return NotFound();
            }

            var irsaliye = await _unitOfWork.Repository<Irsaliye>().GetFirstOrDefaultAsync(
                filter: i => i.IrsaliyeID == id,
                includeProperties: "Cari,Fatura,IrsaliyeDetaylari.Urun");

            if (irsaliye == null)
            {
                return NotFound();
            }

            var viewModel = new IrsaliyeDetailViewModel
            {
                IrsaliyeID = irsaliye.IrsaliyeID,
                IrsaliyeNumarasi = irsaliye.IrsaliyeNumarasi,
                IrsaliyeTarihi = irsaliye.IrsaliyeTarihi,
                SevkTarihi = irsaliye.SevkTarihi,
                CariID = irsaliye.CariID,
                CariAdi = irsaliye.Cari.CariAdi,
                CariVergiNo = irsaliye.Cari.VergiNo,
                CariTelefon = irsaliye.Cari.Telefon,
                CariAdres = irsaliye.Cari.Adres,
                IrsaliyeTuru = irsaliye.IrsaliyeTuru,
                FaturaID = irsaliye.FaturaID,
                FaturaNumarasi = irsaliye.Fatura != null ? irsaliye.Fatura.FaturaNumarasi : null,
                Aciklama = irsaliye.Aciklama,
                Durum = irsaliye.Durum,
                OlusturmaTarihi = irsaliye.OlusturmaTarihi,
                GuncellemeTarihi = irsaliye.GuncellemeTarihi,
                IrsaliyeKalemleri = irsaliye.IrsaliyeDetaylari.Select(id => new IrsaliyeKalemDetailViewModel
                {
                    KalemID = id.IrsaliyeDetayID,
                    UrunID = id.UrunID,
                    UrunAdi = id.Urun.UrunAdi,
                    UrunKodu = id.Urun.UrunKodu,
                    Miktar = id.Miktar,
                    Birim = id.Birim,
                    Aciklama = id.Aciklama
                }).ToList()
            };

            return View(viewModel);
        }

        // GET: Irsaliye/Create
        public async Task<IActionResult> Create()
        {
            await PrepareViewBagForCreate();

            // Otomatik irsaliye numarası oluştur
            string irsaliyeNumarasi = await GenerateNewIrsaliyeNumber();

            return View(new IrsaliyeCreateViewModel
            {
                IrsaliyeNumarasi = irsaliyeNumarasi,
                IrsaliyeTarihi = DateTime.Today,
                SevkTarihi = DateTime.Today,
                IrsaliyeTuru = "Çıkış",
                Durum = "Açık"
            });
        }

        // POST: Irsaliye/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(IrsaliyeCreateViewModel viewModel)
        {
            Console.WriteLine("İrsaliye Create POST metodu çağrıldı");
            Console.WriteLine($"Model geçerli mi: {ModelState.IsValid}");
            Console.WriteLine($"İrsaliye Numarası: {viewModel.IrsaliyeNumarasi}");
            Console.WriteLine($"Cari ID: {viewModel.CariID}");
            Console.WriteLine($"İrsaliye Türü: {viewModel.IrsaliyeTuru}");
            Console.WriteLine($"İrsaliye Kalemleri Sayısı: {viewModel.IrsaliyeKalemleri?.Count ?? 0}");
            
            // ViewBag değerlerini hazırlama metodu
            await PrepareViewBagForCreate();
            
            // Dropdown listeleriyle ilgili model hatalarını temizle
            ModelState.Remove("CariListesi");
            ModelState.Remove("IrsaliyeTuruListesi");
            ModelState.Remove("FaturaListesi");
            ModelState.Remove("Aciklama");
            
            if (viewModel.IrsaliyeKalemleri != null)
            {
                foreach (var kalem in viewModel.IrsaliyeKalemleri)
                {
                    Console.WriteLine($"Kalem - Ürün ID: {kalem.UrunID}, Miktar: {kalem.Miktar}, Birim: {kalem.Birim}");
                }
            }
            else
            {
                ModelState.AddModelError("", "İrsaliye kalemleri bulunamadı. En az bir ürün eklemelisiniz.");
                return View(viewModel);
            }
            
            // İrsaliye kalemleri boşsa hata mesajı göster
            if (viewModel.IrsaliyeKalemleri.Count == 0)
            {
                ModelState.AddModelError("", "En az bir irsaliye kalemi eklemelisiniz.");
                return View(viewModel);
            }
            
            if (!ModelState.IsValid)
            {
                // Hata mesajlarını görünüme gönder
                var errorMessages = string.Join("; ", ModelState.Values
                    .SelectMany(x => x.Errors)
                    .Select(x => x.ErrorMessage));
                
                ModelState.AddModelError("", $"Formda hatalar var: {errorMessages}");
                
                return View(viewModel);
            }
            
            try
            {
                var irsaliye = new Irsaliye
                {
                    IrsaliyeID = Guid.NewGuid(),
                    IrsaliyeNumarasi = viewModel.IrsaliyeNumarasi,
                    IrsaliyeTarihi = viewModel.IrsaliyeTarihi,
                    SevkTarihi = viewModel.SevkTarihi,
                    CariID = viewModel.CariID,
                    IrsaliyeTuru = viewModel.IrsaliyeTuru,
                    FaturaID = viewModel.FaturaID,
                    Aciklama = viewModel.Aciklama,
                    Durum = viewModel.Durum,
                    OlusturmaTarihi = DateTime.Now,
                    IrsaliyeDetaylari = new List<IrsaliyeDetay>()
                };

                foreach (var kalem in viewModel.IrsaliyeKalemleri)
                {
                    irsaliye.IrsaliyeDetaylari.Add(new IrsaliyeDetay
                    {
                        IrsaliyeDetayID = Guid.NewGuid(),
                        IrsaliyeID = irsaliye.IrsaliyeID,
                        UrunID = kalem.UrunID,
                        Miktar = kalem.Miktar,
                        Birim = kalem.Birim,
                        Aciklama = kalem.Aciklama
                    });
                }

                await _unitOfWork.Repository<Irsaliye>().AddAsync(irsaliye);
                await _unitOfWork.SaveAsync();

                // Stok hareketlerini güncelle
                foreach (var kalem in irsaliye.IrsaliyeDetaylari)
                {
                    var stokHareket = new StokHareket
                    {
                        StokHareketID = Guid.NewGuid(),
                        UrunID = kalem.UrunID,
                        Miktar = irsaliye.IrsaliyeTuru == "Giriş" ? kalem.Miktar : -kalem.Miktar,
                        Birim = kalem.Birim,
                        HareketTuru = irsaliye.IrsaliyeTuru == "Giriş" ? "Giriş" : "Çıkış",
                        ReferansNo = irsaliye.IrsaliyeNumarasi,
                        ReferansTuru = "İrsaliye",
                        ReferansID = irsaliye.IrsaliyeID,
                        Aciklama = $"{irsaliye.IrsaliyeNumarasi} nolu irsaliye {(irsaliye.IrsaliyeTuru == "Giriş" ? "girişi" : "çıkışı")}",
                        Tarih = DateTime.Now
                    };

                    await _unitOfWork.Repository<StokHareket>().AddAsync(stokHareket);

                    // Ürün stok miktarını güncelle
                    var urun = await _unitOfWork.Repository<Urun>().GetByIdAsync(kalem.UrunID);
                    if (urun != null)
                    {
                        if (irsaliye.IrsaliyeTuru == "Giriş")
                        {
                            urun.StokMiktar += kalem.Miktar;
                            
                            // FIFO stok girişi yap
                            // Ürün fiyatını al
                            var urunFiyat = await _context.UrunFiyatlari
                                .Where(uf => uf.UrunID == kalem.UrunID && !uf.SoftDelete)
                                .OrderByDescending(uf => uf.GecerliTarih)
                                .FirstOrDefaultAsync();
                                
                            decimal birimFiyat = urunFiyat?.Fiyat ?? 0;
                            
                            // Döviz kuru al (TRY/USD)
                            decimal dovizKuru = 1;
                            try
                            {
                                dovizKuru = await _kurService.GetGuncelKur("TRY", "USD");
                            }
                            catch (Exception ex)
                            {
                                // Kur bulunamazsa varsayılan değer kullan
                                Console.WriteLine($"Döviz kuru alınamadı: {ex.Message}");
                            }
                            
                            await _stokFifoService.StokGirisiYap(
                                kalem.UrunID,
                                kalem.Miktar,
                                birimFiyat,
                                kalem.Birim,
                                irsaliye.IrsaliyeNumarasi,
                                "İrsaliye",
                                irsaliye.IrsaliyeID,
                                $"{irsaliye.IrsaliyeNumarasi} nolu irsaliye girişi",
                                "TRY",
                                dovizKuru
                            );
                        }
                        else
                        {
                            urun.StokMiktar -= kalem.Miktar;
                            
                            // FIFO stok çıkışı yap
                            try
                            {
                                var (kullanilanFifoKayitlari, toplamMaliyet) = await _stokFifoService.StokCikisiYap(
                                    kalem.UrunID,
                                    kalem.Miktar,
                                    irsaliye.IrsaliyeNumarasi,
                                    "İrsaliye",
                                    irsaliye.IrsaliyeID,
                                    $"{irsaliye.IrsaliyeNumarasi} nolu irsaliye çıkışı"
                                );
                            }
                            catch (StokYetersizException ex)
                            {
                                // Stok yetersiz hatası - daha detaylı bilgi içerir
                                ModelState.AddModelError("", ex.Message);
                                TempData["ErrorMessage"] = ex.Message;
                                TempData["ErrorDetails"] = $"Ürün: {ex.UrunAdi} ({ex.UrunKodu}), Talep edilen: {ex.TalepEdilenMiktar}, Mevcut: {ex.MevcutMiktar}";
                                
                                // ViewBag'leri yeniden doldur
                                await PrepareViewBagForCreate();
                                return View(viewModel);
                            }
                            catch (InvalidOperationException ex)
                            {
                                // Diğer işlem hataları
                                ModelState.AddModelError("", ex.Message);
                                TempData["ErrorMessage"] = ex.Message;
                                
                                // ViewBag'leri yeniden doldur
                                await PrepareViewBagForCreate();
                                return View(viewModel);
                            }
                        }
                        await _unitOfWork.Repository<Urun>().UpdateAsync(urun);
                    }
                }

                await _unitOfWork.SaveAsync();
                TempData["SuccessMessage"] = "İrsaliye başarıyla kaydedildi.";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                // Hata mesajını logla
                Console.WriteLine($"İrsaliye kaydedilirken hata: {ex.Message}");
                Console.WriteLine($"Hata detayı: {ex.StackTrace}");
                
                if (ex.InnerException != null)
                {
                    Console.WriteLine($"İç hata: {ex.InnerException.Message}");
                }
                
                TempData["ErrorMessage"] = $"İrsaliye kaydedilirken bir hata oluştu: {ex.Message}";
                ModelState.AddModelError("", $"İrsaliye kaydedilirken bir hata oluştu: {ex.Message}");
            }

            // Hata durumunda ViewBag'leri yeniden doldur
            var cariler = await _unitOfWork.Repository<Cari>().GetAsync(
                filter: c => c.Aktif && !c.SoftDelete);
            
            var faturalar = await _unitOfWork.Repository<Fatura>().GetAsync(
                filter: f => f.Aktif == true && !f.SoftDelete,
                includeProperties: "Cari");
                
            var urunler = await _unitOfWork.Repository<Urun>().GetAsync(
                filter: u => u.Aktif && !u.SoftDelete);

            ViewBag.Cariler = new SelectList(cariler, "CariID", "CariAdi", viewModel.CariID);
            ViewBag.Faturalar = new SelectList(faturalar, "FaturaID", "FaturaNumarasi", viewModel.FaturaID);
            ViewBag.IrsaliyeTurleri = new SelectList(new List<string> { "Giriş", "Çıkış" }, viewModel.IrsaliyeTuru);
            ViewBag.Durumlar = new SelectList(new List<string> { "Açık", "Kapalı", "İptal" }, viewModel.Durum);
            ViewBag.Urunler = new SelectList(urunler, "UrunID", "UrunAdi");

            // Mevcut irsaliye kalemlerini koruyarak view'a geri dön
            return View(viewModel);
        }

        // GET: Irsaliye/Edit/5
        public async Task<IActionResult> Edit(Guid id)
        {
            if (id == Guid.Empty)
            {
                return NotFound();
            }

            var irsaliye = await _unitOfWork.Repository<Irsaliye>().GetFirstOrDefaultAsync(
                filter: i => i.IrsaliyeID == id,
                includeProperties: "IrsaliyeDetaylari.Urun");

            if (irsaliye == null)
            {
                return NotFound();
            }

            var viewModel = new IrsaliyeEditViewModel
            {
                IrsaliyeID = irsaliye.IrsaliyeID,
                IrsaliyeNumarasi = irsaliye.IrsaliyeNumarasi,
                IrsaliyeTarihi = irsaliye.IrsaliyeTarihi,
                SevkTarihi = irsaliye.SevkTarihi,
                CariID = irsaliye.CariID,
                IrsaliyeTuru = irsaliye.IrsaliyeTuru,
                FaturaID = irsaliye.FaturaID,
                Aciklama = irsaliye.Aciklama,
                Durum = irsaliye.Durum,
                IrsaliyeKalemleri = irsaliye.IrsaliyeDetaylari.Select(id => new IrsaliyeKalemViewModel
                {
                    KalemID = id.IrsaliyeDetayID,
                    UrunID = id.UrunID,
                    UrunAdi = id.Urun.UrunAdi,
                    Miktar = id.Miktar,
                    Birim = id.Birim,
                    Aciklama = id.Aciklama
                }).ToList()
            };

            var cariler = await _unitOfWork.Repository<Cari>().GetAsync(
                filter: c => c.Aktif && !c.SoftDelete);
            
            var faturalar = await _unitOfWork.Repository<Fatura>().GetAsync(
                filter: f => f.Aktif == true && !f.SoftDelete,
                includeProperties: "Cari");

            ViewBag.Cariler = new SelectList(cariler, "CariID", "CariAdi", viewModel.CariID);
            ViewBag.Faturalar = new SelectList(faturalar, "FaturaID", "FaturaNumarasi", viewModel.FaturaID);
            ViewBag.IrsaliyeTurleri = new SelectList(new List<string> { "Giriş", "Çıkış" }, viewModel.IrsaliyeTuru);
            ViewBag.Durumlar = new SelectList(new List<string> { "Açık", "Kapalı", "İptal" }, viewModel.Durum);

            return View(viewModel);
        }

        // POST: Irsaliye/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(Guid id, IrsaliyeEditViewModel viewModel)
        {
            if (id != viewModel.IrsaliyeID)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    var irsaliye = await _unitOfWork.Repository<Irsaliye>().GetFirstOrDefaultAsync(
                        filter: i => i.IrsaliyeID == id,
                        includeProperties: "IrsaliyeDetaylari");

                    if (irsaliye == null)
                    {
                        return NotFound();
                    }

                    // Stok hareketlerini geri al
                    foreach (var kalem in irsaliye.IrsaliyeDetaylari)
                    {
                        var stokHareketleri = await _unitOfWork.Repository<StokHareket>().GetAsync(
                            filter: sh => sh.ReferansID == irsaliye.IrsaliyeID && sh.ReferansTuru == "İrsaliye" && sh.UrunID == kalem.UrunID);

                        foreach (var stokHareket in stokHareketleri)
                        {
                            await _unitOfWork.Repository<StokHareket>().RemoveAsync(stokHareket);
                        }

                        // Ürün stok miktarını geri al
                        var urun = await _unitOfWork.Repository<Urun>().GetByIdAsync(kalem.UrunID);
                        if (urun != null)
                        {
                            if (irsaliye.IrsaliyeTuru == "Giriş")
                            {
                                urun.StokMiktar -= kalem.Miktar;
                            }
                            else
                            {
                                urun.StokMiktar += kalem.Miktar;
                            }
                            await _unitOfWork.Repository<Urun>().UpdateAsync(urun);
                        }
                    }

                    // İrsaliye detaylarını temizle
                    foreach (var kalem in irsaliye.IrsaliyeDetaylari.ToList())
                    {
                        await _unitOfWork.Repository<IrsaliyeDetay>().RemoveAsync(kalem);
                    }

                    // İrsaliye bilgilerini güncelle
                    irsaliye.IrsaliyeNumarasi = viewModel.IrsaliyeNumarasi;
                    irsaliye.IrsaliyeTarihi = viewModel.IrsaliyeTarihi ?? DateTime.Now;
                    irsaliye.SevkTarihi = viewModel.SevkTarihi ?? DateTime.Now;
                    irsaliye.CariID = viewModel.CariID;
                    irsaliye.IrsaliyeTuru = viewModel.IrsaliyeTuru;
                    irsaliye.FaturaID = viewModel.FaturaID;
                    irsaliye.Aciklama = viewModel.Aciklama;
                    irsaliye.Durum = viewModel.Durum;
                    irsaliye.GuncellemeTarihi = DateTime.Now;

                    await _unitOfWork.Repository<Irsaliye>().UpdateAsync(irsaliye);
                    await _unitOfWork.SaveAsync();

                    // Yeni irsaliye detaylarını ekle
                    foreach (var kalem in viewModel.IrsaliyeKalemleri)
                    {
                        var irsaliyeDetay = new IrsaliyeDetay
                        {
                            IrsaliyeDetayID = Guid.NewGuid(),
                            IrsaliyeID = irsaliye.IrsaliyeID,
                            UrunID = kalem.UrunID,
                            Miktar = kalem.Miktar,
                            Birim = kalem.Birim,
                            Aciklama = kalem.Aciklama
                        };

                        await _unitOfWork.Repository<IrsaliyeDetay>().AddAsync(irsaliyeDetay);
                    }

                    await _unitOfWork.SaveAsync();

                    // Yeni stok hareketlerini ekle
                    var yeniIrsaliye = await _unitOfWork.Repository<Irsaliye>().GetFirstOrDefaultAsync(
                        filter: i => i.IrsaliyeID == id,
                        includeProperties: "IrsaliyeDetaylari");

                    foreach (var kalem in yeniIrsaliye.IrsaliyeDetaylari)
                    {
                        var stokHareket = new StokHareket
                        {
                            StokHareketID = Guid.NewGuid(),
                            UrunID = kalem.UrunID,
                            Miktar = yeniIrsaliye.IrsaliyeTuru == "Giriş" ? kalem.Miktar : -kalem.Miktar,
                            Birim = kalem.Birim,
                            HareketTuru = yeniIrsaliye.IrsaliyeTuru == "Giriş" ? "Giriş" : "Çıkış",
                            ReferansNo = yeniIrsaliye.IrsaliyeNumarasi,
                            ReferansTuru = "İrsaliye",
                            ReferansID = yeniIrsaliye.IrsaliyeID,
                            Aciklama = $"{yeniIrsaliye.IrsaliyeNumarasi} nolu irsaliye {(yeniIrsaliye.IrsaliyeTuru == "Giriş" ? "girişi" : "çıkışı")}",
                            Tarih = DateTime.Now
                        };

                        await _unitOfWork.Repository<StokHareket>().AddAsync(stokHareket);

                        // Ürün stok miktarını güncelle
                        var urun = await _unitOfWork.Repository<Urun>().GetByIdAsync(kalem.UrunID);
                        if (urun != null)
                        {
                            if (yeniIrsaliye.IrsaliyeTuru == "Giriş")
                            {
                                urun.StokMiktar += kalem.Miktar;
                                
                                // FIFO stok girişi yap
                                // Ürün fiyatını al
                                var urunFiyat = await _context.UrunFiyatlari
                                    .Where(uf => uf.UrunID == kalem.UrunID && !uf.SoftDelete)
                                    .OrderByDescending(uf => uf.GecerliTarih)
                                    .FirstOrDefaultAsync();
                                    
                                decimal birimFiyat = urunFiyat?.Fiyat ?? 0;
                                
                                // Döviz kuru al (TRY/USD)
                                decimal dovizKuru = 1;
                                try
                                {
                                    dovizKuru = await _kurService.GetGuncelKur("TRY", "USD");
                                }
                                catch (Exception ex)
                                {
                                    // Kur bulunamazsa varsayılan değer kullan
                                    Console.WriteLine($"Döviz kuru alınamadı: {ex.Message}");
                                }
                                
                                await _stokFifoService.StokGirisiYap(
                                    kalem.UrunID,
                                    kalem.Miktar,
                                    birimFiyat,
                                    kalem.Birim,
                                    yeniIrsaliye.IrsaliyeNumarasi,
                                    "İrsaliye",
                                    yeniIrsaliye.IrsaliyeID,
                                    $"{yeniIrsaliye.IrsaliyeNumarasi} nolu irsaliye girişi (Güncelleme)",
                                    "TRY",
                                    dovizKuru
                                );
                            }
                            else
                            {
                                urun.StokMiktar -= kalem.Miktar;
                                
                                // FIFO stok çıkışı yap
                                try
                                {
                                    var (kullanilanFifoKayitlari, toplamMaliyet) = await _stokFifoService.StokCikisiYap(
                                        kalem.UrunID,
                                        kalem.Miktar,
                                        yeniIrsaliye.IrsaliyeNumarasi,
                                        "İrsaliye",
                                        yeniIrsaliye.IrsaliyeID,
                                        $"{yeniIrsaliye.IrsaliyeNumarasi} nolu irsaliye çıkışı (Güncelleme)"
                                    );
                                }
                                catch (StokYetersizException ex)
                                {
                                    // Stok yetersiz hatası - daha detaylı bilgi içerir
                                    ModelState.AddModelError("", ex.Message);
                                    TempData["ErrorMessage"] = ex.Message;
                                    TempData["ErrorDetails"] = $"Ürün: {ex.UrunAdi} ({ex.UrunKodu}), Talep edilen: {ex.TalepEdilenMiktar}, Mevcut: {ex.MevcutMiktar}";
                                    
                                    // ViewBag'leri yeniden doldur
                                    await PrepareViewBagForCreate();
                                    return View(viewModel);
                                }
                                catch (InvalidOperationException ex)
                                {
                                    // Diğer işlem hataları
                                    ModelState.AddModelError("", ex.Message);
                                    TempData["ErrorMessage"] = ex.Message;
                                    
                                    // ViewBag'leri yeniden doldur
                                    await PrepareViewBagForCreate();
                                    return View(viewModel);
                                }
                            }
                            await _unitOfWork.Repository<Urun>().UpdateAsync(urun);
                        }
                    }

                    await _unitOfWork.SaveAsync();
                    return RedirectToAction(nameof(Index));
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!await IrsaliyeExists(viewModel.IrsaliyeID))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
            }

            var cariler = await _unitOfWork.Repository<Cari>().GetAsync(
                filter: c => c.Aktif && !c.SoftDelete);
            
            var faturalar = await _unitOfWork.Repository<Fatura>().GetAsync(
                filter: f => f.Aktif == true && !f.SoftDelete,
                includeProperties: "Cari");

            ViewBag.Cariler = new SelectList(cariler, "CariID", "CariAdi", viewModel.CariID);
            ViewBag.Faturalar = new SelectList(faturalar, "FaturaID", "FaturaNumarasi", viewModel.FaturaID);
            ViewBag.IrsaliyeTurleri = new SelectList(new List<string> { "Giriş", "Çıkış" }, viewModel.IrsaliyeTuru);
            ViewBag.Durumlar = new SelectList(new List<string> { "Açık", "Kapalı", "İptal" }, viewModel.Durum);

            return View(viewModel);
        }

        // GET: Irsaliye/Delete/5
        public async Task<IActionResult> Delete(Guid id)
        {
            if (id == Guid.Empty)
            {
                return NotFound();
            }

            var irsaliye = await _unitOfWork.Repository<Irsaliye>().GetFirstOrDefaultAsync(
                filter: i => i.IrsaliyeID == id,
                includeProperties: "Cari,Fatura");

            if (irsaliye == null)
            {
                return NotFound();
            }

            var viewModel = new IrsaliyeViewModel
            {
                IrsaliyeID = irsaliye.IrsaliyeID,
                IrsaliyeNumarasi = irsaliye.IrsaliyeNumarasi,
                IrsaliyeTarihi = irsaliye.IrsaliyeTarihi,
                SevkTarihi = irsaliye.SevkTarihi,
                CariID = irsaliye.CariID,
                CariAdi = irsaliye.Cari.CariAdi,
                IrsaliyeTuru = irsaliye.IrsaliyeTuru,
                FaturaID = irsaliye.FaturaID,
                FaturaNumarasi = irsaliye.Fatura != null ? irsaliye.Fatura.FaturaNumarasi : null,
                Aciklama = irsaliye.Aciklama,
                Durum = irsaliye.Durum,
                OlusturmaTarihi = irsaliye.OlusturmaTarihi,
                GuncellemeTarihi = irsaliye.GuncellemeTarihi
            };

            return View(viewModel);
        }

        // POST: Irsaliye/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(Guid id)
        {
            var irsaliye = await _unitOfWork.Repository<Irsaliye>().GetFirstOrDefaultAsync(
                filter: i => i.IrsaliyeID == id,
                includeProperties: "IrsaliyeDetaylari");

            if (irsaliye == null)
            {
                return NotFound();
            }

            try
            {
                // Transaction başlat
                using var transaction = await _context.Database.BeginTransactionAsync();
                
                try
                {
                    // Stok hareketlerini geri al
                    foreach (var kalem in irsaliye.IrsaliyeDetaylari)
                    {
                        // Ürün stok miktarını güncelle
                        var urun = await _unitOfWork.Repository<Urun>().GetByIdAsync(kalem.UrunID);
                        if (urun != null)
                        {
                            if (irsaliye.IrsaliyeTuru == "Giriş")
                            {
                                // Giriş irsaliyesi ise stoktan düş
                                urun.StokMiktar -= kalem.Miktar;
                            }
                            else
                            {
                                // Çıkış irsaliyesi ise stoğa ekle
                                urun.StokMiktar += kalem.Miktar;
                            }
                            await _unitOfWork.Repository<Urun>().UpdateAsync(urun);
                        }
                    }
                    
                    // FIFO kayıtlarını iptal et
                    await _stokFifoService.FifoKayitlariniIptalEt(
                        id,
                        "İrsaliye",
                        $"{irsaliye.IrsaliyeNumarasi} nolu irsaliye iptal edildi",
                        null // Kullanıcı ID'si eklenebilir
                    );

                    // İrsaliyeyi sil (soft delete)
                    irsaliye.SoftDelete = true;
                    await _unitOfWork.Repository<Irsaliye>().UpdateAsync(irsaliye);
                    
                    // İrsaliye detaylarını sil (soft delete)
                    foreach (var kalem in irsaliye.IrsaliyeDetaylari)
                    {
                        kalem.SoftDelete = true;
                        await _unitOfWork.Repository<IrsaliyeDetay>().UpdateAsync(kalem);
                    }

                    await _unitOfWork.SaveAsync();
                    
                    // Transaction'ı commit et
                    await transaction.CommitAsync();
                    
                    TempData["SuccessMessage"] = "İrsaliye başarıyla silindi.";
                    return RedirectToAction(nameof(Index));
                }
                catch (Exception ex)
                {
                    // Hata durumunda rollback yap
                    await transaction.RollbackAsync();
                    
                    // Hata mesajını logla
                    Console.WriteLine($"İrsaliye silinirken hata: {ex.Message}");
                    
                    TempData["ErrorMessage"] = $"İrsaliye silinirken bir hata oluştu: {ex.Message}";
                    return RedirectToAction(nameof(Details), new { id = id });
                }
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"İrsaliye silinirken bir hata oluştu: {ex.Message}";
                return RedirectToAction(nameof(Details), new { id = id });
            }
        }

        // AJAX: Ürün bilgilerini getir
        [HttpGet]
        public async Task<IActionResult> GetUrunBilgileri(Guid urunId)
        {
            var urun = await _unitOfWork.Repository<Urun>().GetByIdAsync(urunId);
            if (urun == null)
            {
                return NotFound();
            }

            return Json(new
            {
                urunAdi = urun.UrunAdi,
                birim = urun.Birim,
                stokMiktar = urun.StokMiktar
            });
        }

        // AJAX: Faturaya göre cari bilgilerini getir
        [HttpGet]
        public async Task<IActionResult> GetFaturaBilgileri(Guid faturaId)
        {
            var fatura = await _unitOfWork.Repository<Fatura>().GetFirstOrDefaultAsync(
                filter: f => f.FaturaID == faturaId,
                includeProperties: "Cari");

            if (fatura == null)
            {
                return NotFound();
            }

            return Json(new
            {
                cariId = fatura.CariID,
                cariAdi = fatura.Cari.CariAdi
            });
        }

        // GET: Irsaliye/CreateFromFatura/5
        public async Task<IActionResult> CreateFromFatura(Guid? faturaId)
        {
            if (faturaId == null)
            {
                TempData["ErrorMessage"] = "Fatura ID bulunamadı.";
                return RedirectToAction("Index", "Fatura");
            }

            // Fatura bilgilerini al
            var fatura = await _unitOfWork.Repository<Fatura>().GetFirstOrDefaultAsync(
                filter: f => f.FaturaID == faturaId && !f.SoftDelete,
                includeProperties: "Cari,FaturaDetaylari,FaturaDetaylari.Urun");

            if (fatura == null)
            {
                TempData["ErrorMessage"] = "Fatura bulunamadı.";
                return RedirectToAction("Index", "Fatura");
            }

            // İrsaliye türünü belirle
            string irsaliyeTuru = "Çıkış"; // Varsayılan olarak çıkış irsaliyesi
            var faturaTuru = await _unitOfWork.Repository<FaturaTuru>().GetByIdAsync(fatura.FaturaTuruID ?? 0);
            if (faturaTuru != null)
            {
                if (faturaTuru.FaturaTuruAdi.ToLower().Contains("alış") || faturaTuru.FaturaTuruAdi.ToLower().Contains("alis"))
                {
                    irsaliyeTuru = "Giriş"; // Alış faturası için giriş irsaliyesi
                }
                else
                {
                    irsaliyeTuru = "Çıkış"; // Satış faturası için çıkış irsaliyesi
                }
            }

            // Yeni irsaliye numarası oluştur
            string irsaliyeNumarasi = await GenerateNewIrsaliyeNumber();

            // İrsaliye ViewModel oluştur
            var viewModel = new IrsaliyeCreateViewModel
            {
                IrsaliyeNumarasi = irsaliyeNumarasi,
                IrsaliyeTarihi = DateTime.Today,
                SevkTarihi = DateTime.Today,
                CariID = fatura.CariID ?? Guid.Empty,
                IrsaliyeTuru = irsaliyeTuru,
                FaturaID = fatura.FaturaID,
                Aciklama = $"{fatura.FaturaNumarasi} numaralı faturadan oluşturulmuştur.",
                Durum = "Açık",
                IrsaliyeKalemleri = new List<IrsaliyeKalemViewModel>()
            };

            // Fatura kalemlerini irsaliye kalemlerine dönüştür
            if (fatura.FaturaDetaylari != null && fatura.FaturaDetaylari.Any())
            {
                foreach (var fd in fatura.FaturaDetaylari)
                {
                    if (fd.Urun != null)
                    {
                        viewModel.IrsaliyeKalemleri.Add(new IrsaliyeKalemViewModel
                        {
                            UrunID = fd.UrunID,
                            UrunAdi = fd.Urun.UrunAdi,
                            UrunKodu = fd.Urun.UrunKodu,
                            Miktar = fd.Miktar,
                            Birim = !string.IsNullOrEmpty(fd.Birim) ? fd.Birim : "Adet",
                            Aciklama = !string.IsNullOrEmpty(fd.Aciklama) ? fd.Aciklama : "Fatura kaynaklı"
                        });
                    }
                }
            }

            // ViewBag değerlerini hazırla
            await PrepareViewBagForCreate();

            // Cari bilgisini seçili hale getir
            if (viewModel.CariID != Guid.Empty)
            {
                var cari = await _unitOfWork.Repository<Cari>().GetByIdAsync(viewModel.CariID);
                if (cari != null)
                {
                    ViewBag.SecilenCari = new SelectListItem
                    {
                        Value = cari.CariID.ToString(),
                        Text = cari.CariAdi,
                        Selected = true
                    };
                }
            }

            // Fatura bilgisini seçili hale getir
            if (viewModel.FaturaID != Guid.Empty)
            {
                ViewBag.SecilenFatura = new SelectListItem
                {
                    Value = fatura.FaturaID.ToString(),
                    Text = fatura.FaturaNumarasi,
                    Selected = true
                };
            }

            return View("Create", viewModel);
        }

        private async Task<bool> IrsaliyeExists(Guid id)
        {
            var irsaliye = await _unitOfWork.Repository<Irsaliye>().GetByIdAsync(id);
            return irsaliye != null;
        }
        
        // Otomatik irsaliye numarası oluşturma
        private async Task<string> GenerateNewIrsaliyeNumber()
        {
            try
            {
                // Bugünün tarihini al ve formatla (YYMMDD)
                string dateFormat = DateTime.Now.ToString("yyMMdd");
                
                // Son irsaliye numarasını bul
                var lastIrsaliye = await _unitOfWork.Repository<Irsaliye>().GetAsync(
                    orderBy: q => q.OrderByDescending(i => i.OlusturmaTarihi)
                );
                
                int sequence = 1;
                
                if (lastIrsaliye.Any())
                {
                    var lastNumber = lastIrsaliye.First().IrsaliyeNumarasi;
                    
                    // Son irsaliye numarasından sıra numarasını çıkarmaya çalış
                    if (lastNumber != null && lastNumber.StartsWith("IRS-") && lastNumber.Length >= 14)
                    {
                        string lastSequence = lastNumber.Substring(11); // Son 3 karakter
                        if (int.TryParse(lastSequence, out int lastSeq))
                        {
                            sequence = lastSeq + 1;
                        }
                    }
                }
                
                // Yeni irsaliye numarası oluştur (IRS-YYMMDD-001 formatında)
                return $"IRS-{dateFormat}-{sequence:000}";
            }
            catch (Exception)
            {
                // Hata durumunda varsayılan bir numara döndür
                return $"IRS-{DateTime.Now.ToString("yyMMdd")}-001";
            }
        }

        // ViewBag değerlerini hazırlama metodu
        private async Task PrepareViewBagForCreate()
        {
            var cariler = await _unitOfWork.Repository<Cari>().GetAsync(
                filter: c => c.Aktif && !c.SoftDelete);
            
            var faturalar = await _unitOfWork.Repository<Fatura>().GetAsync(
                filter: f => f.Aktif == true && !f.SoftDelete,
                includeProperties: "Cari");
                
            var urunler = await _unitOfWork.Repository<Urun>().GetAsync(
                filter: u => u.Aktif && !u.SoftDelete);

            ViewBag.Cariler = new SelectList(cariler, "CariID", "CariAdi");
            ViewBag.Faturalar = new SelectList(faturalar, "FaturaID", "FaturaNumarasi");
            ViewBag.IrsaliyeTurleri = new SelectList(new List<string> { "Giriş", "Çıkış" }, "Çıkış");
            ViewBag.Durumlar = new SelectList(new List<string> { "Açık", "Kapalı", "İptal" }, "Açık");
            ViewBag.Urunler = new SelectList(urunler, "UrunID", "UrunAdi");
        }
        
        // GET: Irsaliye/Print/5
        public async Task<IActionResult> Print(Guid id)
        {
            if (id == Guid.Empty)
            {
                return NotFound();
            }

            var irsaliye = await _unitOfWork.Repository<Irsaliye>().GetFirstOrDefaultAsync(
                filter: i => i.IrsaliyeID == id,
                includeProperties: "Cari,Fatura,IrsaliyeDetaylari.Urun");

            if (irsaliye == null)
            {
                return NotFound();
            }

            var viewModel = new IrsaliyeDetailViewModel
            {
                IrsaliyeID = irsaliye.IrsaliyeID,
                IrsaliyeNumarasi = irsaliye.IrsaliyeNumarasi,
                IrsaliyeTarihi = irsaliye.IrsaliyeTarihi,
                SevkTarihi = irsaliye.SevkTarihi,
                CariID = irsaliye.CariID,
                CariAdi = irsaliye.Cari.CariAdi,
                CariVergiNo = irsaliye.Cari.VergiNo,
                CariTelefon = irsaliye.Cari.Telefon,
                CariAdres = irsaliye.Cari.Adres,
                IrsaliyeTuru = irsaliye.IrsaliyeTuru,
                FaturaID = irsaliye.FaturaID,
                FaturaNumarasi = irsaliye.Fatura != null ? irsaliye.Fatura.FaturaNumarasi : null,
                Aciklama = irsaliye.Aciklama,
                Durum = irsaliye.Durum,
                OlusturmaTarihi = irsaliye.OlusturmaTarihi,
                GuncellemeTarihi = irsaliye.GuncellemeTarihi,
                IrsaliyeKalemleri = irsaliye.IrsaliyeDetaylari.Select(id => new IrsaliyeKalemDetailViewModel
                {
                    KalemID = id.IrsaliyeDetayID,
                    UrunID = id.UrunID,
                    UrunAdi = id.Urun.UrunAdi,
                    UrunKodu = id.Urun.UrunKodu,
                    Miktar = id.Miktar,
                    Birim = id.Birim,
                    Aciklama = id.Aciklama
                }).ToList()
            };

            // Yazdırma görünümüne yönlendir
            return View(viewModel);
        }
    }
} 