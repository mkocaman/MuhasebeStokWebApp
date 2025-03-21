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

    // [Authorize] - Geçici olarak kaldırıldı
    public class IrsaliyeController : BaseController
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<IrsaliyeController> _logger;
        private readonly ApplicationDbContext _context;
        private readonly StokFifoService _stokFifoService;
        private readonly IDovizKuruService _dovizKuruService;
        private readonly ILogService _logService;
        private readonly IWebHostEnvironment _env;
        private readonly IDropdownService _dropdownService;
        private readonly UserManager<IdentityUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;

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
            UserManager<IdentityUser> userManager,
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

        // GET: Irsaliye
        public async Task<IActionResult> Index(int page = 1, int pageSize = 10, string sortOrder = "date_desc", string searchString = "")
        {
            ViewData["CurrentSort"] = sortOrder;
            ViewData["DateSortParm"] = sortOrder == "date" ? "date_desc" : "date";
            ViewData["NumberSortParm"] = sortOrder == "number" ? "number_desc" : "number";
            ViewData["CustomerSortParm"] = sortOrder == "customer" ? "customer_desc" : "customer";
            ViewData["CurrentFilter"] = searchString;

            var query = _context.Irsaliyeler
                .Include(i => i.Cari)
                .Include(i => i.Fatura)
                .Where(i => i.Aktif);  // SoftDelete yerine Aktif kullanıyoruz

            // Arama filtresi uygula
            if (!string.IsNullOrEmpty(searchString))
            {
                query = query.Where(i =>
                    i.IrsaliyeNumarasi.Contains(searchString) ||
                    i.Cari.CariAdi.Contains(searchString) ||
                    (i.Fatura != null && i.Fatura.FaturaNumarasi.Contains(searchString)));
            }

            // Sıralama uygula
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
                    query = query.OrderBy(i => i.Cari.CariAdi);
                    break;
                case "customer_desc":
                    query = query.OrderByDescending(i => i.Cari.CariAdi);
                    break;
                default:
                    query = query.OrderByDescending(i => i.IrsaliyeTarihi);
                    break;
            }

            // Toplam kayıt sayısını al
            int totalItems = await query.CountAsync();

            // Sayfalama uygula
            var irsaliyeler = await query
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(i => new IrsaliyeViewModel
                {
                    IrsaliyeID = i.IrsaliyeID,
                    IrsaliyeNumarasi = i.IrsaliyeNumarasi,
                    IrsaliyeTarihi = i.IrsaliyeTarihi,
                    IrsaliyeTuru = i.IrsaliyeTuru,
                    Durum = i.Durum,
                    CariID = i.CariID,
                    CariAdi = i.Cari.CariAdi,
                    FaturaID = i.FaturaID,
                    FaturaNumarasi = i.Fatura != null ? i.Fatura.FaturaNumarasi : "",
                    OlusturmaTarihi = i.OlusturmaTarihi
                })
                .ToListAsync();

            // ViewModel oluştur
            var viewModel = new IrsaliyeListViewModel
            {
                Irsaliyeler = irsaliyeler,
                PagingInfo = new PagingInfo
                {
                    CurrentPage = page,
                    ItemsPerPage = pageSize,
                    TotalItems = totalItems
                },
                SearchString = searchString,
                SortOrder = sortOrder
            };

            return View(viewModel);
        }

        // GET: Irsaliye/Details/5
        public async Task<IActionResult> Details(Guid id)
        {
            var irsaliye = await _unitOfWork.Repository<Irsaliye>().GetByIdAsync(id);
            if (irsaliye == null)
            {
                return NotFound();
            }

            // İrsaliye detaylarını manuel olarak yükle
            var irsaliyeDetaylari = await _context.IrsaliyeDetaylari
                .Where(d => d.IrsaliyeID == id && !d.SoftDelete)
                .Include(d => d.Urun)
                .ToListAsync();

            // Cari bilgilerini yükle
            var cari = await _context.Cariler
                .FirstOrDefaultAsync(c => c.CariID == irsaliye.CariID && !c.SoftDelete);

            var viewModel = new IrsaliyeDetailViewModel
            {
                IrsaliyeID = irsaliye.IrsaliyeID,
                IrsaliyeNumarasi = irsaliye.IrsaliyeNumarasi,
                IrsaliyeTarihi = irsaliye.IrsaliyeTarihi,
                CariID = irsaliye.CariID,
                CariAdi = cari?.CariAdi,
                FaturaID = irsaliye.FaturaID,
                Resmi = irsaliye.Resmi,
                Aciklama = irsaliye.Aciklama,
                Durum = irsaliye.Durum,
                OlusturmaTarihi = irsaliye.OlusturmaTarihi,
                GuncellemeTarihi = irsaliye.GuncellemeTarihi
            };

            // İrsaliye detaylarını ekle
            if (irsaliyeDetaylari != null && irsaliyeDetaylari.Any())
            {
                viewModel.Detaylar = irsaliyeDetaylari.Select(d => new IrsaliyeVM.IrsaliyeDetayViewModel
                {
                    IrsaliyeDetayID = d.IrsaliyeDetayID,
                    UrunID = d.UrunID,
                    UrunAdi = d.Urun?.UrunAdi ?? "",
                    UrunKodu = d.Urun?.UrunKodu ?? "",
                    Miktar = d.Miktar,
                    Birim = d.Birim,
                    Aciklama = d.Aciklama
                }).ToList();
            }

            return View(viewModel);
        }

        // GET: Irsaliye/Create
        public IActionResult Create()
        {
            var viewModel = new IrsaliyeCreateViewModel
            {
                IrsaliyeTarihi = DateTime.Now,
                Detaylar = new List<IrsaliyeVM.IrsaliyeDetayViewModel>()
            };

            // İlk boş satırı ekle
            viewModel.Detaylar.Add(new IrsaliyeVM.IrsaliyeDetayViewModel
            {
                IrsaliyeDetayID = Guid.NewGuid()
            });

            // Dropdown listeleri doldur
            PrepareDropdownLists(viewModel);

            return View(viewModel);
        }

        // POST: Irsaliye/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(IrsaliyeCreateViewModel viewModel)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    var irsaliye = new Irsaliye
                    {
                        IrsaliyeID = Guid.NewGuid(),
                        IrsaliyeNumarasi = viewModel.IrsaliyeNumarasi,
                        IrsaliyeTarihi = viewModel.IrsaliyeTarihi,
                        CariID = viewModel.CariID,
                        FaturaID = viewModel.FaturaID,
                        Resmi = viewModel.Resmi,
                        Aktif = true,
                        OlusturmaTarihi = DateTime.Now,
                        OlusturanKullaniciID = GetCurrentUserId(),
                        GuncellemeTarihi = DateTime.Now,
                        SonGuncelleyenKullaniciID = GetCurrentUserId()
                    };

                    await _unitOfWork.IrsaliyeRepository.AddAsync(irsaliye);
                    
                    var filteredDetaylar = viewModel.Detaylar.Where(d => d.UrunID != Guid.Empty).ToList();
                    await UpdateIrsaliyeDetaylarAsync(irsaliye.IrsaliyeID, filteredDetaylar);
                    
                    await _unitOfWork.CompleteAsync();

                    await _logService.Log(
                        $"Yeni irsaliye oluşturuldu: İrsaliye ID: {irsaliye.IrsaliyeID}, İrsaliye No: {irsaliye.IrsaliyeNumarasi}",
                        Enums.LogTuru.Bilgi
                    );

                    return RedirectToAction(nameof(Details), new { id = irsaliye.IrsaliyeID });
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError("", $"İrsaliye oluşturulurken bir hata oluştu: {ex.Message}");
                }
            }

            // Formda hata varsa, dropdown listeleri tekrar doldur
            PrepareDropdownLists(viewModel);
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
                includeProperties: "Cari,Fatura");

            if (irsaliye == null)
            {
                return NotFound();
            }

            var irsaliyeDetaylari = await _context.IrsaliyeDetaylari
                .Where(d => d.IrsaliyeID == id && !d.SoftDelete)
                .Include(d => d.Urun)
                .ToListAsync();

            var viewModel = new IrsaliyeEditViewModel
            {
                IrsaliyeID = irsaliye.IrsaliyeID,
                IrsaliyeNumarasi = irsaliye.IrsaliyeNumarasi,
                IrsaliyeTarihi = irsaliye.IrsaliyeTarihi,
                CariID = irsaliye.CariID,
                IrsaliyeTuru = irsaliye.IrsaliyeTuru,
                FaturaID = irsaliye.FaturaID,
                Durum = irsaliye.Durum,
                Aciklama = irsaliye.Aciklama,
                Detaylar = irsaliyeDetaylari.Select(d => new IrsaliyeVM.IrsaliyeDetayViewModel
                {
                    IrsaliyeDetayID = d.IrsaliyeDetayID,
                    UrunID = d.UrunID,
                    UrunAdi = d.Urun?.UrunAdi,
                    Miktar = d.Miktar,
                    Birim = d.Birim,
                    Aciklama = d.Aciklama
                }).ToList()
            };

            // ViewBag'i hazırla
            var cariler = await _unitOfWork.Repository<Cari>().GetAsync(
                filter: c => c.Aktif && !c.SoftDelete);
            
            var faturalar = await _unitOfWork.Repository<Fatura>().GetAsync(
                filter: f => f.Aktif == true && !f.SoftDelete,
                includeProperties: "Cari");

            ViewBag.Cariler = new SelectList(cariler, "CariID", "CariAdi", viewModel.CariID);
            ViewBag.Faturalar = new SelectList(faturalar, "FaturaID", "FaturaNumarasi", viewModel.FaturaID);
            ViewBag.IrsaliyeTurleri = new SelectList(new List<string> { "Giriş", "Çıkış" }, viewModel.IrsaliyeTuru);
            ViewBag.Durumlar = new SelectList(new List<string> { "Açık", "Kapalı", "İptal" }, viewModel.Durum);
            
            // Ürünler listesi ekleniyor - null durumunu önlemek için kontrol eklendi
            try
            {
                var urunler = await _unitOfWork.Repository<Urun>().GetAsync(
                    filter: u => u.Aktif && !u.SoftDelete);
                
                // Null kontrolü ve boş liste yedeği
                if (urunler != null && urunler.Any())
                {
                    ViewBag.Urunler = new SelectList(urunler, "UrunID", "UrunAdi");
                }
                else
                {
                    ViewBag.Urunler = new SelectList(new List<SelectListItem>());
                }
            }
            catch (Exception ex)
            {
                // Hata durumunda boş liste
                ViewBag.Urunler = new SelectList(new List<SelectListItem>());
                Console.WriteLine($"Ürünler listesi oluşturulurken hata: {ex.Message}");
            }

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
                    var irsaliye = await _unitOfWork.IrsaliyeRepository.GetByIdAsync(viewModel.IrsaliyeID);
                    if (irsaliye == null)
                    {
                        return NotFound();
                    }

                    irsaliye.IrsaliyeNumarasi = viewModel.IrsaliyeNumarasi;
                    irsaliye.IrsaliyeTarihi = viewModel.IrsaliyeTarihi;
                    irsaliye.CariID = viewModel.CariID;
                    irsaliye.FaturaID = viewModel.FaturaID;
                    irsaliye.Resmi = viewModel.Resmi;
                    irsaliye.GuncellemeTarihi = DateTime.Now;
                    irsaliye.SonGuncelleyenKullaniciID = GetCurrentUserId();

                    await _unitOfWork.IrsaliyeRepository.UpdateAsync(irsaliye);
                    await UpdateIrsaliyeDetaylarAsync(viewModel.IrsaliyeID, viewModel.Detaylar);
                    await _unitOfWork.CompleteAsync();

                    await _logService.Log(
                        $"İrsaliye güncellendi: İrsaliye ID: {irsaliye.IrsaliyeID}, İrsaliye No: {irsaliye.IrsaliyeNumarasi}",
                        Enums.LogTuru.Bilgi
                    );

                    return RedirectToAction(nameof(Details), new { id = viewModel.IrsaliyeID });
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError("", $"İrsaliye güncellenirken bir hata oluştu: {ex.Message}");
                }
            }

            // Formda hata varsa, dropdown listeleri tekrar doldur
            PrepareDropdownLists(viewModel);
            return View(viewModel);
        }

        // GET: Irsaliye/Delete/5
        public async Task<IActionResult> Delete(Guid? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var irsaliye = await _context.Irsaliyeler
                .Include(i => i.Cari)
                .Include(i => i.Fatura)
                .FirstOrDefaultAsync(m => m.IrsaliyeID == id && m.Aktif);  // SoftDelete yerine Aktif kullanıyoruz

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
                CariAdi = irsaliye.Cari.CariAdi,
                IrsaliyeTuru = "Standart", // Sabit değer
                FaturaID = irsaliye.FaturaID,
                FaturaNumarasi = irsaliye.Fatura != null ? irsaliye.Fatura.FaturaNumarasi : "",
                Aciklama = irsaliye.Aciklama,
                Durum = "Açık", // Sabit değer
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
            var irsaliye = await _context.Irsaliyeler
                .Include(i => i.IrsaliyeDetaylari)
                .FirstOrDefaultAsync(i => i.IrsaliyeID == id && i.Aktif);

            if (irsaliye == null)
            {
                return NotFound();
            }

            // İrsaliyeyi soft delete
            irsaliye.Aktif = false;
            irsaliye.GuncellemeTarihi = DateTime.Now;
            irsaliye.SonGuncelleyenKullaniciID = GetCurrentUserId();
            _context.Update(irsaliye);

            // İrsaliye detaylarını soft delete
            foreach (var detay in irsaliye.IrsaliyeDetaylari.Where(d => d.Aktif))
            {
                detay.Aktif = false;
                detay.GuncellemeTarihi = DateTime.Now;
                detay.SonGuncelleyenKullaniciID = GetCurrentUserId();
                _context.Update(detay);
            }

            await _context.SaveChangesAsync();
            TempData["SuccessMessage"] = "İrsaliye başarıyla silindi.";
            return RedirectToAction(nameof(Index));
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
            string irsaliyeTuru = "Standart"; // Varsayılan değer
            
            // Yeni irsaliye numarası oluştur
            string irsaliyeNumarasi = await GenerateNewIrsaliyeNumber();

            // İrsaliye ViewModel oluştur
            var viewModel = new IrsaliyeCreateViewModel
            {
                IrsaliyeNumarasi = irsaliyeNumarasi,
                IrsaliyeTarihi = DateTime.Today,
                CariID = fatura.CariID ?? Guid.Empty,
                IrsaliyeTuru = irsaliyeTuru,
                FaturaID = fatura.FaturaID,
                Aciklama = $"{fatura.FaturaNumarasi} numaralı faturadan oluşturulmuştur.",
                Durum = "Açık",
                Detaylar = new List<IrsaliyeVM.IrsaliyeDetayViewModel>()
            };

            // Fatura kalemlerini irsaliye kalemlerine dönüştür
            if (fatura.FaturaDetaylari != null && fatura.FaturaDetaylari.Any())
            {
                foreach (var fd in fatura.FaturaDetaylari)
                {
                    if (fd.Urun != null)
                    {
                        viewModel.Detaylar.Add(new IrsaliyeVM.IrsaliyeDetayViewModel
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
                .Any(e => e.IrsaliyeID == id && e.Aktif == true);
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

        // Get Current User ID metodu
        private Guid GetCurrentUserId()
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
                // Geçersiz GUID formatı hatası durumunda loglama yapılabilir
                Console.WriteLine($"GetCurrentUserId GUID parse hatası: {ex.Message}");
            }
            catch (Exception ex) 
            {
                // Diğer hatalar için genel loglama
                Console.WriteLine($"GetCurrentUserId genel hata: {ex.Message}");
            }
            return Guid.Empty;
        }

        // ViewBag değerlerini hazırlama metodu
        private async Task PrepareViewBagForCreate()
        {
            try 
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
                
                // Urunler null kontrolü ve ek güvenlik
                if (urunler != null && urunler.Any())
                {
                    ViewBag.Urunler = new SelectList(urunler, "UrunID", "UrunAdi");
                }
                else
                {
                    ViewBag.Urunler = new SelectList(new List<SelectListItem>());
                }
            }
            catch (Exception ex)
            {
                // Hata durumunda boş listeler oluştur
                ViewBag.Cariler = new SelectList(new List<Cari>(), "CariID", "CariAdi");
                ViewBag.Faturalar = new SelectList(new List<Fatura>(), "FaturaID", "FaturaNumarasi");
                ViewBag.IrsaliyeTurleri = new SelectList(new List<string> { "Giriş", "Çıkış" }, "Çıkış");
                ViewBag.Durumlar = new SelectList(new List<string> { "Açık", "Kapalı", "İptal" }, "Açık");
                ViewBag.Urunler = new SelectList(new List<SelectListItem>());
                
                // Hata loglama yapılabilir
                Console.WriteLine($"PrepareViewBagForCreate hata: {ex.Message}");
            }
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
                CariID = irsaliye.CariID,
                CariAdi = irsaliye.Cari.CariAdi,
                CariVergiNo = irsaliye.Cari.VergiNo,
                CariTelefon = irsaliye.Cari.Telefon,
                CariAdres = irsaliye.Cari.Adres,
                IrsaliyeTuru = irsaliye.IrsaliyeTuru,
                FaturaID = irsaliye.FaturaID,
                FaturaNumarasi = irsaliye.Fatura.FaturaNumarasi,
                Aciklama = irsaliye.Aciklama,
                Durum = irsaliye.Durum,
                OlusturmaTarihi = irsaliye.OlusturmaTarihi,
                GuncellemeTarihi = irsaliye.GuncellemeTarihi,
                IrsaliyeKalemleri = irsaliye.IrsaliyeDetaylari.Select(id => new IrsaliyeVM.IrsaliyeKalemViewModel
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

        // Detayları güncelleme, ekleme ve silme işlemlerini yapan metod
        private async Task UpdateIrsaliyeDetaylarAsync(Guid irsaliyeID, List<IrsaliyeVM.IrsaliyeDetayViewModel> detaylar)
        {
            var existingDetaylar = await _unitOfWork.IrsaliyeDetayRepository.FindAsync(d => d.IrsaliyeID == irsaliyeID);
            
            // Mevcut detayları kaldır (soft delete)
            foreach (var detay in existingDetaylar)
            {
                detay.Aktif = false;
                detay.GuncellemeTarihi = DateTime.Now;
                detay.SonGuncelleyenKullaniciID = GetCurrentUserId();
                await _unitOfWork.IrsaliyeDetayRepository.UpdateAsync(detay);
            }
            
            // Yeni detayları ekle
            foreach (var detayViewModel in detaylar)
            {
                if (detayViewModel.UrunID == Guid.Empty) continue;
                
                var detay = new IrsaliyeDetay
                {
                    IrsaliyeDetayID = detayViewModel.IrsaliyeDetayID != Guid.Empty ? detayViewModel.IrsaliyeDetayID : Guid.NewGuid(),
                    IrsaliyeID = irsaliyeID,
                    UrunID = detayViewModel.UrunID,
                    Miktar = detayViewModel.Miktar,
                    Birim = detayViewModel.Birim,
                    Aciklama = detayViewModel.Aciklama,
                    Aktif = true,
                    OlusturmaTarihi = DateTime.Now,
                    OlusturanKullaniciID = GetCurrentUserId(),
                    GuncellemeTarihi = DateTime.Now,
                    SonGuncelleyenKullaniciID = GetCurrentUserId()
                };
                
                await _unitOfWork.IrsaliyeDetayRepository.AddOrUpdateAsync(detay);
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
                    Detaylar = new List<IrsaliyeVM.IrsaliyeDetayViewModel>()
                };
                
                // İlk boş satırı ekle
                model.Detaylar.Add(new IrsaliyeVM.IrsaliyeDetayViewModel
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
                    .Where(c => c.Aktif && !c.SoftDelete)
                    .OrderBy(c => c.CariAdi)
                    .ToList();
                model.CariListesi = new SelectList(cariler, "CariID", "CariAdi");

                var urunler = _context.Urunler
                    .Where(u => u.Aktif && !u.SoftDelete)
                    .OrderBy(u => u.UrunAdi)
                    .ToList();
                model.UrunListesi = new SelectList(urunler, "UrunID", "UrunAdi");
                
                // ViewBag için de aynı ürünleri ayarla
                ViewBag.Cariler = new SelectList(cariler, "CariID", "CariAdi");
                ViewBag.Urunler = new SelectList(urunler, "UrunID", "UrunAdi");
                ViewBag.IrsaliyeTurleri = new SelectList(new List<string> { "Giriş", "Çıkış" }, "Çıkış");
                ViewBag.Durumlar = new SelectList(new List<string> { "Açık", "Kapalı", "İptal" }, "Açık");
            }
            catch (Exception ex)
            {
                // Hata durumunda boş listeler oluştur
                model.CariListesi = new SelectList(new List<Cari>(), "CariID", "CariAdi");
                model.UrunListesi = new SelectList(new List<Urun>(), "UrunID", "UrunAdi");
                
                // ViewBag için de boş listeler
                ViewBag.Cariler = new SelectList(new List<Cari>(), "CariID", "CariAdi");
                ViewBag.Urunler = new SelectList(new List<Urun>(), "UrunID", "UrunAdi");
                ViewBag.IrsaliyeTurleri = new SelectList(new List<string> { "Giriş", "Çıkış" }, "Çıkış");
                ViewBag.Durumlar = new SelectList(new List<string> { "Açık", "Kapalı", "İptal" }, "Açık");
                
                // Hata loglama
                Console.WriteLine($"PrepareDropdownLists hata: {ex.Message}");
            }
        }

        private void PrepareDropdownLists(IrsaliyeEditViewModel model)
        {
            var cariler = _context.Cariler
                .Where(c => c.Aktif && !c.SoftDelete)
                .OrderBy(c => c.CariAdi)
                .ToList();
            model.CariListesi = new SelectList(cariler, "CariID", "CariAdi", model.CariID);

            var urunler = _context.Urunler
                .Where(u => u.Aktif && !u.SoftDelete)
                .OrderBy(u => u.UrunAdi)
                .ToList();
            model.UrunListesi = new SelectList(urunler, "UrunID", "UrunAdi");
        }

        private async Task PrepareEditViewModelAsync(IrsaliyeEditViewModel model)
        {
            var cariler = await _unitOfWork.Repository<Cari>().GetAsync(
                filter: c => c.Aktif && !c.SoftDelete);
            
            var faturalar = await _unitOfWork.Repository<Fatura>().GetAsync(
                filter: f => f.Aktif == true && !f.SoftDelete,
                includeProperties: "Cari");

            ViewBag.Cariler = new SelectList(cariler, "CariID", "CariAdi", model.CariID);
            ViewBag.Faturalar = new SelectList(faturalar, "FaturaID", "FaturaNumarasi", model.FaturaID);
            ViewBag.IrsaliyeTurleri = new SelectList(new List<string> { "Standart", "Giriş", "Çıkış" }, model.IrsaliyeTuru);
            ViewBag.Durumlar = new SelectList(new List<string> { "Açık", "Kapalı", "İptal" }, model.Durum);

            if (model.Detaylar == null)
            {
                model.Detaylar = new List<IrsaliyeVM.IrsaliyeDetayViewModel>();
            }

            var urunler = await _unitOfWork.Repository<Urun>().GetAsync(
                filter: u => u.Aktif == true && !u.SoftDelete);
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
                // Eskiden GetIrsaliyeDetayWithDetailsAsync kullanıyordu, şimdi manuel yükleyelim
                var irsaliyeDetay = await _unitOfWork.Repository<IrsaliyeDetay>().GetByIdAsync(id);
                if (irsaliyeDetay == null)
                {
                    return NotFound();
                }

                // İlişkili verileri manuel olarak yükle
                var urun = await _unitOfWork.Repository<Urun>().GetByIdAsync(irsaliyeDetay.UrunID);

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
            var irsaliyeDetay = await _unitOfWork.Repository<IrsaliyeDetay>().GetByIdAsync(id);
            if (irsaliyeDetay == null)
            {
                return NotFound();
            }

            // İlişkili verileri manuel olarak yükle
            var urun = await _unitOfWork.Repository<Urun>().GetByIdAsync(irsaliyeDetay.UrunID);

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
            var urun = await _unitOfWork.Repository<Urun>().GetByIdAsync(id);
            if (urun == null)
            {
                return Json(new { success = false, message = "Ürün bulunamadı" });
            }

            // Ürünün fiyatını al
            var urunFiyat = await _context.UrunFiyatlari
                .Where(f => f.UrunID == id && !f.SoftDelete)
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