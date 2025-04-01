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
using MuhasebeStokWebApp.ViewModels.Shared;
using MuhasebeStokWebApp.Services;
using System.Text.Json;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Logging;
using IrsaliyeVM = MuhasebeStokWebApp.ViewModels.Irsaliye;
using SharedVM = MuhasebeStokWebApp.ViewModels.Shared;
using Microsoft.AspNetCore.Identity;
using MuhasebeStokWebApp.Services.Interfaces;
using DEntity = MuhasebeStokWebApp.Data.Entities;
using DEntityFatura = MuhasebeStokWebApp.Data.Entities.Fatura;
using DEntityFaturaDetay = MuhasebeStokWebApp.Data.Entities.FaturaDetay;
using DEntityIrsaliye = MuhasebeStokWebApp.Data.Entities.Irsaliye;
using DEntityIrsaliyeDetay = MuhasebeStokWebApp.Data.Entities.IrsaliyeDetay;
using MuhasebeStokWebApp.Models;
using MuhasebeStokWebApp.Enums;
using IrsaliyeDetayVM = MuhasebeStokWebApp.ViewModels.Irsaliye.IrsaliyeDetayViewModel;
using DEntityUrun = MuhasebeStokWebApp.Data.Entities.Urun;
using DEntityCari = MuhasebeStokWebApp.Data.Entities.Cari;

namespace MuhasebeStokWebApp.Controllers
{
    public class PaginatedList<T> : List<T>
    {
        public int PageIndex { get; private set; }
        public int TotalPages { get; private set; }

        public PaginatedList(List<T> items, int count, int pageIndex, int pageSize)
        {
            PageIndex = pageIndex;
            TotalPages = (int)Math.Ceiling(count / (double)pageSize);
            this.AddRange(items);
        }

        public bool HasPreviousPage => PageIndex > 1;
        public bool HasNextPage => PageIndex < TotalPages;

        public static async Task<PaginatedList<T>> CreateAsync(IQueryable<T> source, int pageIndex, int pageSize)
        {
            var count = await source.CountAsync();
            var items = await source.Skip((pageIndex - 1) * pageSize).Take(pageSize).ToListAsync();
            return new PaginatedList<T>(items, count, pageIndex, pageSize);
        }
    }

    [Authorize]
    public class IrsaliyeController : BaseController
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<IrsaliyeController> _logger;
        private readonly ApplicationDbContext _context;
        private readonly StokFifoService _stokFifoService;
        private readonly IDovizKuruService _dovizKuruService;
        private new readonly ILogService _logService;
        private readonly IWebHostEnvironment _env;
        private readonly IDropdownService _dropdownService;
        protected new readonly UserManager<ApplicationUser> _userManager;
        protected new readonly RoleManager<IdentityRole> _roleManager;

        public IrsaliyeController(
            IUnitOfWork unitOfWork,
            ILogger<IrsaliyeController> logger,
            ApplicationDbContext context,
            IMenuService menuService,
            StokFifoService stokFifoService,
            IDovizKuruService dovizKuruService,
            ILogService logService,
            IWebHostEnvironment env,
            IDropdownService dropdownService,
            UserManager<ApplicationUser> userManager,
            RoleManager<IdentityRole> roleManager) : base(menuService, userManager, roleManager, logService)
        {
            _unitOfWork = unitOfWork;
            _logger = logger;
            _context = context;
            _stokFifoService = stokFifoService;
            _dovizKuruService = dovizKuruService;
            _logService = logService;
            _env = env;
            _dropdownService = dropdownService;
            _userManager = userManager;
            _roleManager = roleManager;
        }

        // İrsaliyelerin listelendiği ana sayfa
        public async Task<IActionResult> Index(int page = 1, int pageSize = 10, string sortOrder = "date_desc", string searchString = "")
        {
            var query = _context.Irsaliyeler
                .Include(i => i.Cari)
                .Include(i => i.Fatura)
                .Include(i => i.IrsaliyeDetaylari)
                .AsNoTracking();

            if (!string.IsNullOrEmpty(searchString))
            {
                query = query.Where(i =>
                    i.IrsaliyeNumarasi.Contains(searchString) ||
                    i.Cari.Ad.Contains(searchString) ||
                    (i.Fatura != null && i.Fatura.FaturaNumarasi.Contains(searchString)));
            }

            switch (sortOrder)
            {
                case "date":
                    query = query.OrderBy(i => i.IrsaliyeTarihi);
                    break;
                case "date_desc":
                    query = query.OrderByDescending(i => i.IrsaliyeTarihi);
                    break;
                case "number":
                    query = query.OrderBy(i => i.IrsaliyeNumarasi);
                    break;
                case "number_desc":
                    query = query.OrderByDescending(i => i.IrsaliyeNumarasi);
                    break;
                case "customer":
                    query = query.OrderBy(i => i.Cari.Ad);
                    break;
                case "customer_desc":
                    query = query.OrderByDescending(i => i.Cari.Ad);
                    break;
                default:
                    query = query.OrderByDescending(i => i.IrsaliyeTarihi);
                    break;
            }

            var count = await query.CountAsync();
            var items = await query
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(i => new IrsaliyeViewModel
                {
                    IrsaliyeID = i.IrsaliyeID,
                    IrsaliyeNumarasi = i.IrsaliyeNumarasi,
                    IrsaliyeTarihi = i.IrsaliyeTarihi,
                    CariID = i.CariID,
                    CariAdi = i.Cari.Ad,
                    FaturaID = i.FaturaID,
                    FaturaNumarasi = i.Fatura != null ? i.Fatura.FaturaNumarasi : "",
                    IrsaliyeTuru = i.IrsaliyeTuru,
                    Aciklama = i.Aciklama,
                    ToplamTutar = i.IrsaliyeDetaylari.Sum(d => d.BirimFiyat * d.Miktar),
                    Aktif = i.Aktif,
                    OlusturmaTarihi = i.OlusturmaTarihi,
                    GuncellemeTarihi = i.GuncellemeTarihi
                })
                .ToListAsync();

            var model = new PaginatedList<IrsaliyeViewModel>(items, count, page, pageSize);

            ViewBag.CurrentSort = sortOrder;
            ViewBag.DateSortParm = sortOrder == "date" ? "date_desc" : "date";
            ViewBag.NumberSortParm = sortOrder == "number" ? "number_desc" : "number";
            ViewBag.CustomerSortParm = sortOrder == "customer" ? "customer_desc" : "customer";
            ViewBag.CurrentFilter = searchString;

            return View(model);
        }

        // İrsaliye detaylarını gösterir
        [HttpGet]
        public async Task<IActionResult> Details(Guid id)
        {
            var irsaliye = await _unitOfWork.IrsaliyeRepository.GetIrsaliyeWithDetailsAsync(id);
            if (irsaliye == null)
            {
                return NotFound();
            }

            var model = new IrsaliyeDetailViewModel
            {
                IrsaliyeID = irsaliye.IrsaliyeID,
                IrsaliyeNumarasi = irsaliye.IrsaliyeNumarasi,
                IrsaliyeTarihi = irsaliye.IrsaliyeTarihi,
                CariID = irsaliye.CariID,
                FaturaID = irsaliye.FaturaID,
                Aciklama = irsaliye.Aciklama,
                CariAdi = irsaliye.Cari?.Ad,
                IrsaliyeDetaylari = irsaliye.IrsaliyeDetaylari?.Select(d => new IrsaliyeKalemViewModel
                {
                    KalemID = d.IrsaliyeDetayID,
                    IrsaliyeID = d.IrsaliyeID,
                    UrunID = d.UrunID,
                    UrunAdi = d.Urun?.UrunAdi,
                    Miktar = d.Miktar,
                    Birim = d.Birim,
                    BirimFiyat = d.BirimFiyat,
                    Aciklama = d.Aciklama
                }).ToList()
            };

            return View(model);
        }

        // Yeni irsaliye oluşturma formu
        [HttpGet]
        public async Task<IActionResult> Create()
        {
            var model = new IrsaliyeCreateViewModel
            {
                IrsaliyeTarihi = DateTime.Now,
                Aktif = true
            };

            var viewBagData = await _dropdownService.PrepareViewBagAsync("Irsaliye", "Create");
            foreach (var item in viewBagData)
            {
                dynamic viewBag = ViewBag;
                viewBag[item.Key] = item.Value;
            }

            return View(model);
        }

        // Yeni irsaliye oluşturma işlemi
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(IrsaliyeCreateViewModel model)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                    var irsaliye = new DEntityIrsaliye
                    {
                        IrsaliyeID = Guid.NewGuid(),
                        IrsaliyeNumarasi = model.IrsaliyeNumarasi,
                        IrsaliyeTarihi = model.IrsaliyeTarihi,
                        CariID = model.CariID,
                        FaturaID = model.FaturaID,
                        Aciklama = model.Aciklama,
                        IrsaliyeTuru = model.IrsaliyeTuru,
                        Aktif = true,
                        OlusturmaTarihi = DateTime.Now,
                        OlusturanKullaniciId = Guid.Parse(userId),
                        GuncellemeTarihi = DateTime.Now,
                        SonGuncelleyenKullaniciId = Guid.Parse(userId)
                    };

                    await _unitOfWork.IrsaliyeRepository.AddAsync(irsaliye);
                    
                    var filteredDetaylar = model.IrsaliyeDetaylari.Where(d => d.UrunID != Guid.Empty).ToList();
                    await UpdateIrsaliyeDetaylarAsync(irsaliye.IrsaliyeID, filteredDetaylar);
                    
                    await _unitOfWork.CompleteAsync();

                    await _logService.Log(
                        $"Yeni irsaliye oluşturuldu: İrsaliye ID: {irsaliye.IrsaliyeID}, İrsaliye No: {irsaliye.IrsaliyeNumarasi}",
                        LogTuru.Bilgi
                    );

                    return RedirectToAction(nameof(Index));
                }
                catch (Exception ex)
                {
                    await _logService.Log(
                        $"İrsaliye oluşturulurken hata: {ex.Message}",
                        LogTuru.Hata
                    );
                    ModelState.AddModelError("", "İrsaliye oluşturulurken bir hata oluştu.");
                }
            }

            await PrepareViewBagForCreate();
            return View(model);
        }

        // GET: Irsaliye/Edit/5
        [HttpGet]
        public async Task<IActionResult> Edit(Guid id)
        {
            var irsaliye = await _unitOfWork.IrsaliyeRepository.GetIrsaliyeWithDetailsAsync(id);
            if (irsaliye == null)
            {
                return NotFound();
            }

            var model = new IrsaliyeEditViewModel
            {
                IrsaliyeID = irsaliye.IrsaliyeID,
                IrsaliyeNumarasi = irsaliye.IrsaliyeNumarasi,
                IrsaliyeTarihi = irsaliye.IrsaliyeTarihi,
                CariID = irsaliye.CariID,
                FaturaID = irsaliye.FaturaID,
                Aciklama = irsaliye.Aciklama
            };

            await _dropdownService.PrepareViewBagAsync("Irsaliye", "Edit");
            return View(model);
        }

        // POST: Irsaliye/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(IrsaliyeEditViewModel model)
        {
            if (!ModelState.IsValid)
            {
                await _dropdownService.PrepareViewBagAsync("Irsaliye", "Edit");
                return View(model);
            }

            var irsaliye = await _unitOfWork.IrsaliyeRepository.GetByIdAsync(model.IrsaliyeID);
            if (irsaliye == null)
            {
                return NotFound();
            }

            irsaliye.IrsaliyeNumarasi = model.IrsaliyeNumarasi;
            irsaliye.IrsaliyeTarihi = model.IrsaliyeTarihi;
            irsaliye.CariID = model.CariID;
            irsaliye.FaturaID = model.FaturaID;
            irsaliye.Aciklama = model.Aciklama;

            try
            {
                await _unitOfWork.CompleteAsync();
                _logger.LogInformation($"İrsaliye güncellendi: {irsaliye.IrsaliyeID}");
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                _logger.LogError($"İrsaliye güncellenirken hata oluştu: {ex.Message}");
                ModelState.AddModelError("", "İrsaliye güncellenirken bir hata oluştu.");
                await _dropdownService.PrepareViewBagAsync("Irsaliye", "Edit");
                return View(model);
            }
        }

        // GET: Irsaliye/Delete/5
        public async Task<IActionResult> Delete(Guid id)
        {
            if (id == Guid.Empty)
            {
                return NotFound();
            }

            var irsaliye = await _context.Irsaliyeler
                .Include(m => m.Cari)
                .Include(m => m.Fatura)
                .FirstOrDefaultAsync(m => m.IrsaliyeID.Equals(id) && m.Aktif);

            if (irsaliye == null)
            {
                return NotFound();
            }

            var viewModel = new IrsaliyeViewModel
            {
                IrsaliyeID = irsaliye.IrsaliyeID,
                IrsaliyeNumarasi = irsaliye.IrsaliyeNumarasi,
                IrsaliyeTarihi = irsaliye.IrsaliyeTarihi,
                CariID = irsaliye.CariID,
                CariAdi = irsaliye.Cari.Ad,
                IrsaliyeTuru = irsaliye.IrsaliyeTuru,
                FaturaID = irsaliye.FaturaID,
                FaturaNumarasi = irsaliye.Fatura != null ? irsaliye.Fatura.FaturaNumarasi : "",
                Aciklama = irsaliye.Aciklama,
                Aktif = irsaliye.Aktif,
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
            if (_context.Irsaliyeler == null)
            {
                return Problem("Entity set 'ApplicationDbContext.Irsaliyeler' is null.");
            }

            // İrsaliyeyi bul
            var irsaliye = await _context.Irsaliyeler
                .Include(i => i.IrsaliyeDetaylari)
                .FirstOrDefaultAsync(i => i.IrsaliyeID.Equals(id) && i.Aktif);

            if (irsaliye == null)
            {
                return NotFound();
            }

            // İrsaliyeyi pasife al
            irsaliye.Aktif = false;
            irsaliye.GuncellemeTarihi = DateTime.Now;
            irsaliye.SonGuncelleyenKullaniciId = GetCurrentUserId();
            _context.Update(irsaliye);

            // İrsaliye detaylarını pasife al
            foreach (var detay in irsaliye.IrsaliyeDetaylari.Where(d => d.Aktif))
            {
                detay.Aktif = false;
                detay.GuncellemeTarihi = DateTime.Now;
                detay.SonGuncelleyenKullaniciId = GetCurrentUserId();
                _context.Update(detay);
            }

            await _context.SaveChangesAsync();
            TempData["SuccessMessage"] = "İrsaliye başarıyla pasife alındı.";
            return RedirectToAction(nameof(Index));
        }

        // AJAX: Ürün Bilgilerini Getir
        [HttpGet]
        public async Task<IActionResult> GetUrunBilgileri(Guid urunId)
        {
            var urun = await _context.Urunler.FindAsync(urunId);
            if (urun == null)
            {
                return Json(new { success = false, message = "Ürün bulunamadı." });
            }

            return Json(new
            {
                success = true,
                urunKodu = urun.UrunKodu,
                urunAdi = urun.UrunAdi,
                birim = urun.Birim,
                stokMiktar = urun.StokMiktar
            });
        }

        // AJAX: Fatura Bilgilerini Getir
        [HttpGet]
        public async Task<IActionResult> GetFaturaBilgileri(Guid faturaId)
        {
            var fatura = await _context.Faturalar
                .Include(f => f.Cari)
                .FirstOrDefaultAsync(f => f.FaturaID.Equals(faturaId));

            if (fatura == null)
            {
                return Json(new { success = false, message = "Fatura bulunamadı." });
            }

            return Json(new
            {
                success = true,
                cariId = fatura.CariID,
                cariAdi = fatura.Cari.Ad
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
            var fatura = await _unitOfWork.Repository<DEntityFatura>().GetFirstOrDefaultAsync(
                filter: f => f.FaturaID.Equals(faturaId) && !f.Silindi,
                includeProperties: "Cari,FaturaDetaylari,FaturaDetaylari.Urun");

            if (fatura == null)
            {
                TempData["ErrorMessage"] = "Fatura bulunamadı.";
                return RedirectToAction("Index", "Fatura");
            }

            // İrsaliye türünü belirle
            string irsaliyeTuru = "Standart"; // Varsayılan değer
            
            // Yeni irsaliye numarası oluştur
            string irsaliyeNumarasi = await GenerateNewIrsaliyeNumber();

            // İrsaliye ViewModel oluştur
            var viewModel = new IrsaliyeCreateViewModel
            {
                IrsaliyeNumarasi = irsaliyeNumarasi,
                IrsaliyeTarihi = DateTime.Today,
                CariID = fatura.CariID.HasValue ? fatura.CariID.Value : Guid.Empty,
                IrsaliyeTuru = irsaliyeTuru,
                FaturaID = fatura.FaturaID,
                Aciklama = $"{fatura.FaturaNumarasi} numaralı faturadan oluşturulmuştur.",
                Aktif = true,
                IrsaliyeDetaylari = new List<IrsaliyeVM.IrsaliyeDetayViewModel>()
            };

            // Fatura kalemlerini irsaliye kalemlerine dönüştür
            if (fatura.FaturaDetaylari != null && fatura.FaturaDetaylari.Any())
            {
                foreach (var fd in fatura.FaturaDetaylari)
                {
                    if (fd.Urun != null)
                    {
                        viewModel.IrsaliyeDetaylari.Add(new IrsaliyeVM.IrsaliyeDetayViewModel
                        {
                            UrunID = fd.UrunID,
                            UrunAdi = fd.Urun.UrunAdi,
                            UrunKodu = fd.Urun.UrunKodu,
                            Miktar = fd.Miktar,
                            Birim = fd.Birim,
                            Aciklama = fd.Aciklama
                        });
                    }
                }
            }

            // ViewBag'e gerekli verileri ekle
            await PrepareCreateViewBagAsync();

            return View("Create", viewModel);
        }

        private bool IrsaliyeExists(Guid id)
        {
            return _context.Irsaliyeler
                .Any(e => e.IrsaliyeID.Equals(id) && e.Aktif);
        }
        
        // Otomatik irsaliye numarası oluşturma
        private async Task<string> GenerateNewIrsaliyeNumber()
        {
            try
            {
                // Bugünün tarihini al ve formatla (YYMMDD)
                string dateFormat = DateTime.Now.ToString("yyMMdd");
                
                // Son irsaliye numarasını bul
                var lastIrsaliye = await _unitOfWork.Repository<DEntityIrsaliye>().GetAsync(
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

        // Get Current User ID metodu
        private new Guid GetCurrentUserId()
        {
            try
            {
                var userIdClaim = User.Claims.FirstOrDefault(c => c.Type == "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier");
                if (userIdClaim != null && !string.IsNullOrEmpty(userIdClaim.Value))
                {
                    return Guid.Parse(userIdClaim.Value);
                }
            }
            catch (FormatException ex)
            {
                // Geçersiz GUID formatı hatası durumunda loglama
                _logger.LogError(ex, "GetCurrentUserId GUID parse hatası: {Message}", ex.Message);
            }
            catch (Exception ex) 
            {
                // Diğer hatalar için genel loglama
                _logger.LogError(ex, "GetCurrentUserId genel hata: {Message}", ex.Message);
            }
            return Guid.Empty;
        }

        // ViewBag değerlerini hazırlama metodu
        private async Task PrepareViewBagForCreate()
        {
            var cariler = await _context.Cariler
                .Where(c => c.AktifMi && !c.Silindi)
                .OrderBy(c => c.Ad)
                .Select(c => new SelectListItem
                {
                    Value = c.CariID.ToString(),
                    Text = $"{c.CariKodu} - {c.Ad}"
                })
                .ToListAsync();

            var urunler = await _context.Urunler
                .Where(u => u.Aktif)
                .OrderBy(u => u.UrunAdi)
                .Select(u => new SelectListItem
                {
                    Value = u.UrunID.ToString(),
                    Text = $"{u.UrunKodu} - {u.UrunAdi}"
                })
                .ToListAsync();

            ViewBag.Cariler = new SelectList(cariler, "Value", "Text");
            ViewBag.Urunler = new SelectList(urunler, "Value", "Text");
            ViewBag.IrsaliyeTurleri = new SelectList(new List<string> { "Giriş", "Çıkış" }, "Çıkış");
        }
        
        // GET: Irsaliye/Print/5
        public async Task<IActionResult> Print(Guid id)
        {
            if (id == Guid.Empty)
            {
                return NotFound();
            }

            var irsaliye = await _unitOfWork.Repository<DEntityIrsaliye>().GetFirstOrDefaultAsync(
                filter: i => i.IrsaliyeID.Equals(id),
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
                CariID = irsaliye.CariID,
                CariAdi = irsaliye.Cari.Ad,
                CariVergiNo = irsaliye.Cari.VergiNo,
                CariTelefon = irsaliye.Cari.Telefon,
                CariAdres = irsaliye.Cari.Adres,
                IrsaliyeTuru = irsaliye.IrsaliyeTuru,
                FaturaID = irsaliye.FaturaID,
                FaturaNumarasi = irsaliye.Fatura.FaturaNumarasi,
                Aciklama = irsaliye.Aciklama,
                Aktif = irsaliye.Aktif,
                OlusturmaTarihi = irsaliye.OlusturmaTarihi,
                GuncellemeTarihi = irsaliye.GuncellemeTarihi,
                IrsaliyeDetaylari = irsaliye.IrsaliyeDetaylari.Select(id => new IrsaliyeVM.IrsaliyeKalemViewModel
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

        // Detayları güncelleme, ekleme ve silme işlemlerini yapan metod
        private async Task UpdateIrsaliyeDetaylarAsync(Guid irsaliyeID, List<MuhasebeStokWebApp.ViewModels.Irsaliye.IrsaliyeDetayViewModel> detaylar)
        {
            // Mevcut detayları getir
            var mevcutDetaylar = await _context.IrsaliyeDetaylari
                .Where(d => d.IrsaliyeID == irsaliyeID)
                .ToListAsync();

            // Mevcut detayları sil
            _context.IrsaliyeDetaylari.RemoveRange(mevcutDetaylar);

            // Yeni detayları ekle
            if (detaylar != null && detaylar.Any())
            {
                foreach (var detay in detaylar)
                {
                    if (detay.UrunID != Guid.Empty)
                    {
                        var urun = await _context.Urunler.FindAsync(detay.UrunID);
                        if (urun != null)
                        {
                            var yeniDetay = new DEntityIrsaliyeDetay
                            {
                                IrsaliyeDetayID = Guid.NewGuid(),
                                IrsaliyeID = irsaliyeID,
                                UrunID = detay.UrunID,
                                Miktar = detay.Miktar,
                                BirimFiyat = detay.BirimFiyat,
                                Birim = detay.Birim,
                                Aciklama = detay.Aciklama
                            };

                            await _context.IrsaliyeDetaylari.AddAsync(yeniDetay);
                        }
                    }
                }
            }
        }

        // ViewModel hazırlama metodları
        private async Task<IrsaliyeCreateViewModel> PrepareCreateViewModelAsync(IrsaliyeCreateViewModel model)
        {
            if (model == null)
            {
                model = new IrsaliyeCreateViewModel
                {
                    IrsaliyeTarihi = DateTime.Now,
                    IrsaliyeDetaylari = new List<IrsaliyeVM.IrsaliyeDetayViewModel>()
                };
                
                // İlk boş satırı ekle
                model.IrsaliyeDetaylari.Add(new IrsaliyeVM.IrsaliyeDetayViewModel
                {
                    IrsaliyeDetayID = Guid.NewGuid()
                });
            }
            
            // Dropdown listeleri doldur
            PrepareDropdownLists(model);
            
            return model;
        }

        private void PrepareDropdownLists(IrsaliyeCreateViewModel model)
        {
            try
            {
                var cariler = _context.Cariler
                    .Where(c => c.AktifMi && !c.Silindi)
                    .OrderBy(c => c.Ad)
                    .ToList();
                model.CariListesi = new SelectList(cariler, "CariID", "Ad");

                var urunler = _context.Urunler
                    .Where(u => u.Aktif)
                    .OrderBy(u => u.UrunAdi)
                    .ToList();
                model.UrunListesi = new SelectList(urunler, "UrunID", "UrunAdi");
                
                // ViewBag için de aynı ürünleri ayarla
                ViewBag.Cariler = new SelectList(cariler, "CariID", "Ad");
                ViewBag.Urunler = new SelectList(urunler, "UrunID", "UrunAdi");
                ViewBag.IrsaliyeTurleri = new SelectList(new List<string> { "Giriş", "Çıkış" }, "Çıkış");
                ViewBag.Durumlar = new SelectList(new List<string> { "Açık", "Kapalı", "İptal" }, "Açık");
            }
            catch (Exception ex)
            {
                // Hata durumunda boş listeler oluştur
                model.CariListesi = new SelectList(new List<DEntityCari>(), "CariID", "Ad");
                model.UrunListesi = new SelectList(new List<DEntityUrun>(), "UrunID", "UrunAdi");
                
                // ViewBag için de boş listeler
                ViewBag.Cariler = new SelectList(new List<DEntityCari>(), "CariID", "Ad");
                ViewBag.Urunler = new SelectList(new List<DEntityUrun>(), "UrunID", "UrunAdi");
                ViewBag.IrsaliyeTurleri = new SelectList(new List<string> { "Giriş", "Çıkış" }, "Çıkış");
                ViewBag.Durumlar = new SelectList(new List<string> { "Açık", "Kapalı", "İptal" }, "Açık");
                
                // Hata loglama
                _logger.LogError(ex, "PrepareDropdownLists hata: {Message}", ex.Message);
            }
        }

        private void PrepareDropdownLists(IrsaliyeEditViewModel model)
        {
            var cariler = _context.Cariler
                .Where(c => c.AktifMi && !c.Silindi)
                .OrderBy(c => c.Ad)
                .ToList();
            model.CariListesi = new SelectList(cariler, "CariID", "Ad", model.CariID);

            var urunler = _context.Urunler
                .Where(u => u.Aktif)
                .OrderBy(u => u.UrunAdi)
                .ToList();
            model.UrunListesi = new SelectList(urunler, "UrunID", "UrunAdi");
        }

        private async Task PrepareEditViewModelAsync(IrsaliyeEditViewModel model)
        {
            var cariler = await _unitOfWork.Repository<DEntityCari>().GetAsync(
                filter: c => c.AktifMi && !c.Silindi);
            
            var faturalar = await _unitOfWork.Repository<DEntityFatura>().GetAsync(
                filter: f => f.Aktif == true && !f.Silindi,
                includeProperties: "Cari");

            ViewBag.Cariler = new SelectList(cariler, "CariID", "Ad", model.CariID);
            ViewBag.Faturalar = new SelectList(faturalar, "FaturaID", "FaturaNumarasi", model.FaturaID);
            ViewBag.IrsaliyeTurleri = new SelectList(new List<string> { "Standart", "Giriş", "Çıkış" }, model.IrsaliyeTuru);
            ViewBag.Durumlar = new SelectList(new List<string> { "Açık", "Kapalı", "İptal" }, model.Aktif);

            if (model.IrsaliyeDetaylari == null)
            {
                model.IrsaliyeDetaylari = new List<IrsaliyeVM.IrsaliyeDetayViewModel>();
            }

            var urunler = await _unitOfWork.Repository<DEntityUrun>().GetAsync(
                filter: u => u.Aktif == true && !u.Silindi);
            ViewBag.Urunler = new SelectList(urunler, "UrunID", "UrunAdi");
            
            // Dropdown listeleri doldur
            PrepareDropdownLists(model);
        }

        private async Task PrepareCreateViewBagAsync()
        {
            await PrepareViewBagForCreate();
        }

        [HttpGet]
        public async Task<IActionResult> GetIrsaliyeDetay(Guid id)
        {
            try
            {
                // Eskenden GetIrsaliyeDetayWithDetailsAsync kullanıyordu, şimdi manuel yükleyelim
                var irsaliyeDetay = await _unitOfWork.Repository<DEntityIrsaliyeDetay>().GetByIdAsync(id);
                if (irsaliyeDetay == null)
                {
                    return NotFound();
                }

                // İlişkili verileri manuel olarak yükle
                var urun = await _unitOfWork.Repository<DEntityUrun>().GetByIdAsync(irsaliyeDetay.UrunID);

                // Anonim obje oluştur
                var detayViewModel = new
                {
                    IrsaliyeDetayID = irsaliyeDetay.IrsaliyeDetayID,
                    IrsaliyeID = irsaliyeDetay.IrsaliyeID,
                    UrunID = irsaliyeDetay.UrunID,
                    UrunAdi = urun?.UrunAdi ?? "",
                    UrunKodu = urun?.UrunKodu ?? "",
                    Miktar = irsaliyeDetay.Miktar,
                    Birim = irsaliyeDetay.Birim,
                    Aciklama = irsaliyeDetay.Aciklama
                };

                return Json(detayViewModel);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Bir hata oluştu: {ex.Message}");
            }
        }

        public async Task<IActionResult> DetayKalems(Guid id)
        {
            // Eskiden GetIrsaliyeDetayWithDetailsAsync metodunu kullanıyordu
            // Manuel olarak ilişkili verileri yükleyelim
            var irsaliyeDetay = await _unitOfWork.Repository<DEntityIrsaliyeDetay>().GetByIdAsync(id);
            if (irsaliyeDetay == null)
            {
                return NotFound();
            }

            // İlişkili verileri manuel olarak yükle
            var urun = await _unitOfWork.Repository<DEntityUrun>().GetByIdAsync(irsaliyeDetay.UrunID);

            var viewModel = new IrsaliyeVM.IrsaliyeKalemViewModel
            {
                KalemID = irsaliyeDetay.IrsaliyeDetayID,
                UrunID = irsaliyeDetay.UrunID,
                UrunAdi = urun?.UrunAdi ?? "",
                UrunKodu = urun?.UrunKodu ?? "",
                Miktar = irsaliyeDetay.Miktar,
                Birim = irsaliyeDetay.Birim,
                Aciklama = irsaliyeDetay.Aciklama
            };

            return View(viewModel);
        }

        public async Task<IActionResult> GetUrunByID(Guid id)
        {
            var urun = await _unitOfWork.Repository<DEntityUrun>().GetByIdAsync(id);
            if (urun == null)
            {
                return Json(new { success = false, message = "Ürün bulunamadı" });
            }

            // Ürünün fiyatını al
            var urunFiyat = await _context.UrunFiyatlari
                .Where(f => f.UrunID.Equals(id) && !f.Silindi)
                .OrderByDescending(f => f.GecerliTarih)
                .FirstOrDefaultAsync();

            var urunDetay = new 
            {
                UrunID = urun.UrunID,
                UrunAdi = urun.UrunAdi,
                UrunKodu = urun.UrunKodu,
                Miktar = 1,
                Birim = urun.Birim?.BirimAdi ?? "",
                BirimFiyat = urunFiyat?.Fiyat ?? 0
            };

            return Json(new { success = true, data = urunDetay });
        }
    }
} 