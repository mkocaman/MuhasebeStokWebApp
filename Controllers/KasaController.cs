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
using MuhasebeStokWebApp.ViewModels.Kasa;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Identity;

namespace MuhasebeStokWebApp.Controllers
{
    public class KasaController : BaseController
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ApplicationDbContext _context;
        private readonly ILogService _logService;
        private readonly ILogger<KasaController> _logger;
        private readonly IDovizKuruService _dovizKuruService;
        private readonly IParaBirimiService _paraBirimiService;
        private readonly UserManager<IdentityUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;

        public KasaController(
            IUnitOfWork unitOfWork,
            ILogger<KasaController> logger,
            ApplicationDbContext context,
            IMenuService menuService,
            IDovizKuruService dovizKuruService,
            ILogService logService,
            IParaBirimiService paraBirimiService,
            UserManager<IdentityUser> userManager,
            RoleManager<IdentityRole> roleManager) : base(menuService, userManager, roleManager, logService)
        {
            _unitOfWork = unitOfWork;
            _logger = logger;
            _context = context;
            _dovizKuruService = dovizKuruService;
            _logService = logService;
            _paraBirimiService = paraBirimiService;
            _userManager = userManager;
            _roleManager = roleManager;
        }

        // GET: Kasa
        public async Task<IActionResult> Index()
        {
            var kasalar = await _unitOfWork.Repository<Kasa>().GetAsync(
                filter: k => !k.SoftDelete,
                orderBy: q => q.OrderBy(k => k.KasaAdi)
            );

            // ViewModel'e dönüştürme işlemi
            var viewModel = new KasaListViewModel
            {
                Kasalar = kasalar.Select(k => new KasaViewModel
                {
                    KasaID = k.KasaID,
                    KasaAdi = k.KasaAdi,
                    KasaTuru = k.KasaTuru ?? "Genel",
                    ParaBirimi = k.ParaBirimi,
                    AcilisBakiye = k.AcilisBakiye,
                    GuncelBakiye = k.GuncelBakiye,
                    Aciklama = k.Aciklama,
                    Aktif = k.Aktif,
                    OlusturmaTarihi = k.OlusturmaTarihi
                }).ToList(),
                ToplamBakiye = kasalar.Sum(k => k.GuncelBakiye)
            };

            return View(viewModel);
        }

        // GET: Kasa/Details/5
        public async Task<IActionResult> Details(Guid? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var kasa = await _unitOfWork.Repository<Kasa>().GetByIdAsync(id.Value);
            if (kasa == null || kasa.SoftDelete)
            {
                return NotFound();
            }

            return View(kasa);
        }

        // GET: Kasa/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Kasa/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(KasaViewModel viewModel)
        {
            if (ModelState.IsValid)
            {
                var kasa = new Kasa
                {
                    KasaID = Guid.NewGuid(),
                    KasaAdi = viewModel.KasaAdi,
                    ParaBirimi = viewModel.ParaBirimi,
                    Aciklama = viewModel.Aciklama,
                    GuncelBakiye = viewModel.AcilisBakiye,
                    Aktif = true,
                    SoftDelete = false,
                    OlusturmaTarihi = DateTime.Now,
                    OlusturanKullaniciID = GetCurrentUserId(),
                    GuncellemeTarihi = DateTime.Now,
                    SonGuncelleyenKullaniciID = GetCurrentUserId()
                };

                await _unitOfWork.Repository<Kasa>().AddAsync(kasa);
                await _unitOfWork.CompleteAsync();

                await _logService.Log(
                    $"Yeni kasa oluşturuldu: Kasa ID: {kasa.KasaID}, Kasa Adı: {kasa.KasaAdi}",
                    Enums.LogTuru.Bilgi
                );

                return RedirectToAction(nameof(Index));
            }
            return View(viewModel);
        }

        // GET: Kasa/Edit/5
        public async Task<IActionResult> Edit(Guid? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var kasa = await _unitOfWork.Repository<Kasa>().GetByIdAsync(id.Value);
            if (kasa == null || kasa.SoftDelete)
            {
                return NotFound();
            }

            var viewModel = new KasaViewModel
            {
                KasaID = kasa.KasaID,
                KasaAdi = kasa.KasaAdi,
                ParaBirimi = kasa.ParaBirimi,
                Aciklama = kasa.Aciklama,
                AcilisBakiye = kasa.GuncelBakiye,
                Aktif = kasa.Aktif
            };

            return View(viewModel);
        }

        // POST: Kasa/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(Guid id, KasaViewModel viewModel)
        {
            if (id != viewModel.KasaID)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    var kasa = await _unitOfWork.Repository<Kasa>().GetByIdAsync(id);
                    if (kasa == null || kasa.SoftDelete)
                    {
                        return NotFound();
                    }

                    kasa.KasaAdi = viewModel.KasaAdi;
                    kasa.ParaBirimi = viewModel.ParaBirimi;
                    kasa.Aciklama = viewModel.Aciklama;
                    kasa.Aktif = viewModel.Aktif;
                    kasa.GuncellemeTarihi = DateTime.Now;
                    kasa.SonGuncelleyenKullaniciID = GetCurrentUserId();

                    await _unitOfWork.Repository<Kasa>().UpdateAsync(kasa);
                    await _unitOfWork.CompleteAsync();

                    await _logService.Log(
                        $"Kasa güncellendi: Kasa ID: {kasa.KasaID}, Kasa Adı: {kasa.KasaAdi}",
                        Enums.LogTuru.Bilgi
                    );
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!await KasaExists(viewModel.KasaID))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            return View(viewModel);
        }

        // GET: Kasa/Delete/5
        public async Task<IActionResult> Delete(Guid? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var kasa = await _unitOfWork.Repository<Kasa>().GetByIdAsync(id.Value);
            if (kasa == null || kasa.SoftDelete)
            {
                return NotFound();
            }

            return View(kasa);
        }

        // POST: Kasa/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(Guid id)
        {
            var kasa = await _unitOfWork.Repository<Kasa>().GetByIdAsync(id);
            if (kasa == null)
            {
                return NotFound();
            }

            kasa.SoftDelete = true;
            kasa.Aktif = false;
            kasa.GuncellemeTarihi = DateTime.Now;
            kasa.SonGuncelleyenKullaniciID = GetCurrentUserId();

            await _unitOfWork.Repository<Kasa>().UpdateAsync(kasa);
            await _unitOfWork.CompleteAsync();

            await _logService.Log(
                $"Kasa silindi: Kasa ID: {kasa.KasaID}, Kasa Adı: {kasa.KasaAdi}",
                Enums.LogTuru.Bilgi
            );

            return RedirectToAction(nameof(Index));
        }

        private async Task<bool> KasaExists(Guid id)
        {
            var kasa = await _unitOfWork.Repository<Kasa>().GetByIdAsync(id);
            return kasa != null;
        }

        private Guid? GetCurrentUserId()
        {
            // Bu metot, şu anki kullanıcı ID'sini döndürmeli
            // Örnek olarak geçici bir GUID döndürüyoruz, gerçek uygulamada kullanıcı kimlik bilgilerinden alınmalı
            return Guid.NewGuid();
        }

        // GET: Kasa/Hareketler/5
        public async Task<IActionResult> Hareketler(Guid? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var kasa = await _unitOfWork.Repository<Kasa>().GetByIdAsync(id.Value);
            if (kasa == null || kasa.SoftDelete)
            {
                return NotFound();
            }

            var hareketler = await _context.KasaHareketleri
                .Where(h => h.KasaID == id && !h.SoftDelete)
                .OrderByDescending(h => h.Tarih)
                .ToListAsync();

            ViewBag.Kasa = kasa;
            
            return View(hareketler);
        }

        // GET: Kasa/YeniHareket/5
        public async Task<IActionResult> YeniHareket(Guid? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var kasa = await _unitOfWork.Repository<Kasa>().GetByIdAsync(id.Value);
            if (kasa == null || kasa.SoftDelete)
            {
                return NotFound();
            }

            var viewModel = new KasaHareketViewModel
            {
                KasaID = kasa.KasaID,
                KasaAdi = kasa.KasaAdi,
                Tarih = DateTime.Now,
                HareketTuru = "Giriş"
            };

            ViewBag.HareketTurleri = new SelectList(new List<string> { "Giriş", "Çıkış" });
            
            return View(viewModel);
        }

        // POST: Kasa/YeniHareket
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> YeniHareket(KasaHareketViewModel viewModel)
        {
            if (ModelState.IsValid)
            {
                var kasa = await _unitOfWork.Repository<Kasa>().GetByIdAsync(viewModel.KasaID);
                if (kasa == null || kasa.SoftDelete)
                {
                    return NotFound();
                }

                var hareket = new KasaHareket
                {
                    KasaHareketID = Guid.NewGuid(),
                    KasaID = viewModel.KasaID,
                    Tarih = viewModel.Tarih,
                    Tutar = viewModel.Tutar,
                    HareketTuru = viewModel.HareketTuru,
                    Aciklama = viewModel.Aciklama,
                    OlusturmaTarihi = DateTime.Now,
                    IslemYapanKullaniciID = GetCurrentUserId(),
                    GuncellemeTarihi = DateTime.Now,
                    SonGuncelleyenKullaniciID = GetCurrentUserId()
                };

                // Kasa bakiyesini güncelle
                if (viewModel.HareketTuru == "Giriş")
                {
                    kasa.GuncelBakiye += viewModel.Tutar;
                }
                else if (viewModel.HareketTuru == "Çıkış")
                {
                    kasa.GuncelBakiye -= viewModel.Tutar;
                }

                await _unitOfWork.Repository<KasaHareket>().AddAsync(hareket);
                await _unitOfWork.Repository<Kasa>().UpdateAsync(kasa);
                await _unitOfWork.CompleteAsync();

                await _logService.Log(
                    $"Yeni kasa hareketi eklendi: Kasa: {kasa.KasaAdi}, Tür: {hareket.HareketTuru}, Tutar: {hareket.Tutar}",
                    Enums.LogTuru.Bilgi
                );

                return RedirectToAction(nameof(Hareketler), new { id = viewModel.KasaID });
            }

            var kasaDB = await _unitOfWork.Repository<Kasa>().GetByIdAsync(viewModel.KasaID);
            if (kasaDB != null)
            {
                viewModel.KasaAdi = kasaDB.KasaAdi;
            }

            ViewBag.HareketTurleri = new SelectList(new List<string> { "Giriş", "Çıkış" });
            return View(viewModel);
        }

        // GET: Kasa/Transfer
        public async Task<IActionResult> Transfer()
        {
            var kasalar = await _unitOfWork.Repository<Kasa>().GetAsync(
                filter: k => k.Aktif && !k.SoftDelete,
                orderBy: q => q.OrderBy(k => k.KasaAdi)
            );

            ViewBag.KaynakKasalar = new SelectList(kasalar, "KasaID", "KasaAdi");
            ViewBag.HedefKasalar = new SelectList(kasalar, "KasaID", "KasaAdi");

            return View(new KasaTransferViewModel { IslemTarihi = DateTime.Now });
        }

        // POST: Kasa/Transfer
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Transfer(KasaTransferViewModel viewModel)
        {
            if (ModelState.IsValid)
            {
                if (viewModel.KaynakKasaID == viewModel.HedefKasaID)
                {
                    ModelState.AddModelError("", "Kaynak ve hedef kasa aynı olamaz.");
                    PrepareTransferViewBag();
                    return View(viewModel);
                }

                var kaynakKasa = await _unitOfWork.Repository<Kasa>().GetByIdAsync(viewModel.KaynakKasaID);
                var hedefKasa = await _unitOfWork.Repository<Kasa>().GetByIdAsync(viewModel.HedefKasaID);

                if (kaynakKasa == null || hedefKasa == null)
                {
                    return NotFound();
                }

                if (kaynakKasa.GuncelBakiye < viewModel.KaynakTutar)
                {
                    ModelState.AddModelError("", "Kaynak kasada yeterli bakiye bulunmamaktadır.");
                    PrepareTransferViewBag();
                    return View(viewModel);
                }

                // Çıkış hareketi
                var cikisHareket = new KasaHareket
                {
                    KasaHareketID = Guid.NewGuid(),
                    KasaID = viewModel.KaynakKasaID,
                    Tarih = viewModel.IslemTarihi,
                    Tutar = viewModel.KaynakTutar,
                    HareketTuru = "Çıkış",
                    Aciklama = $"{hedefKasa.KasaAdi} kasasına transfer: {viewModel.Aciklama}",
                    OlusturmaTarihi = DateTime.Now,
                    IslemYapanKullaniciID = GetCurrentUserId(),
                    GuncellemeTarihi = DateTime.Now,
                    SonGuncelleyenKullaniciID = GetCurrentUserId()
                };

                // Giriş hareketi
                var girisHareket = new KasaHareket
                {
                    KasaHareketID = Guid.NewGuid(),
                    KasaID = viewModel.HedefKasaID,
                    Tarih = viewModel.IslemTarihi,
                    Tutar = viewModel.HedefTutar,
                    HareketTuru = "Giriş",
                    Aciklama = $"{kaynakKasa.KasaAdi} kasasından transfer: {viewModel.Aciklama}",
                    OlusturmaTarihi = DateTime.Now,
                    IslemYapanKullaniciID = GetCurrentUserId(),
                    GuncellemeTarihi = DateTime.Now,
                    SonGuncelleyenKullaniciID = GetCurrentUserId()
                };

                // Kasa bakiyelerini güncelle
                kaynakKasa.GuncelBakiye -= viewModel.KaynakTutar;
                hedefKasa.GuncelBakiye += viewModel.HedefTutar;

                await _unitOfWork.Repository<KasaHareket>().AddAsync(cikisHareket);
                await _unitOfWork.Repository<KasaHareket>().AddAsync(girisHareket);
                await _unitOfWork.Repository<Kasa>().UpdateAsync(kaynakKasa);
                await _unitOfWork.Repository<Kasa>().UpdateAsync(hedefKasa);
                await _unitOfWork.CompleteAsync();

                await _logService.Log(
                    $"Kasa transferi: {kaynakKasa.KasaAdi} -> {hedefKasa.KasaAdi}, Tutar: {viewModel.KaynakTutar}",
                    Enums.LogTuru.Bilgi
                );

                return RedirectToAction(nameof(Index));
            }

            PrepareTransferViewBag();
            return View(viewModel);
        }

        private async void PrepareTransferViewBag()
        {
            var kasalar = await _unitOfWork.Repository<Kasa>().GetAsync(
                filter: k => k.Aktif && !k.SoftDelete,
                orderBy: q => q.OrderBy(k => k.KasaAdi)
            );

            ViewBag.KaynakKasalar = new SelectList(kasalar, "KasaID", "KasaAdi");
            ViewBag.HedefKasalar = new SelectList(kasalar, "KasaID", "KasaAdi");
        }

        // GET: Kasa/HareketlerTarih
        public IActionResult HareketlerTarih()
        {
            return View(new KasaHareketTarihViewModel
            {
                BaslangicTarihi = DateTime.Now.AddDays(-30),
                BitisTarihi = DateTime.Now
            });
        }

        // POST: Kasa/HareketlerTarih
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> HareketlerTarih(KasaHareketTarihViewModel viewModel)
        {
            if (ModelState.IsValid)
            {
                var hareketler = await _context.KasaHareketleri
                    .Where(h => !h.SoftDelete && h.Tarih >= viewModel.BaslangicTarihi && h.Tarih <= viewModel.BitisTarihi)
                    .OrderByDescending(h => h.Tarih)
                    .ToListAsync();

                viewModel.Hareketler = hareketler;
            }

            return View(viewModel);
        }
    }
} 