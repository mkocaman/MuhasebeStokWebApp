using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using MuhasebeStokWebApp.Data;
using MuhasebeStokWebApp.Services;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using MuhasebeStokWebApp.ViewModels.SistemLog;
using MuhasebeStokWebApp.ViewModels.Menu;
using MuhasebeStokWebApp.Data.Entities;
using MuhasebeStokWebApp.Services.Interfaces;
using Microsoft.Extensions.Logging;
using MuhasebeStokWebApp.Models;

namespace MuhasebeStokWebApp.Controllers
{
    [Authorize(Roles = "Admin")]
    public class SistemLogController : BaseController
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<SistemLogController> _logger;
        private new readonly ILogService _logService;

        public SistemLogController(
            ApplicationDbContext context,
            ILogService logService,
            ILogger<SistemLogController> logger,
            IMenuService menuService,
            UserManager<ApplicationUser> userManager,
            RoleManager<IdentityRole> roleManager) : base(menuService, userManager, roleManager, logService)
        {
            _context = context;
            _logService = logService;
            _logger = logger;
        }

        // GET: SistemLog
        public async Task<IActionResult> Index(string searchString, string islemTuru, DateTime? baslangicTarihi, DateTime? bitisTarihi, int page = 1, int pageSize = 20)
        {
            try 
            {
                // SistemLog için özel menü kontrolü (log kaydı oluşturan satır kaldırıldı)
                
                ViewBag.SearchString = searchString;
                ViewBag.IslemTuru = islemTuru;
                ViewBag.BaslangicTarihi = baslangicTarihi;
                ViewBag.BitisTarihi = bitisTarihi;
                
                // İşlem türlerini ViewBag'e ekle
                var islemTurleri = await _context.SistemLoglar
                    .Select(l => l.IslemTuru)
                    .Distinct()
                    .OrderBy(t => t)
                    .ToListAsync();
                
                ViewBag.IslemTurleri = islemTurleri;
                
                // Filtreleme
                var query = _context.SistemLoglar.AsQueryable();
                
                if (!string.IsNullOrEmpty(searchString))
                {
                    query = query.Where(l => 
                        (l.KayitAdi != null && l.KayitAdi.Contains(searchString)) || 
                        l.Aciklama.Contains(searchString) || 
                        l.KullaniciAdi.Contains(searchString) ||
                        l.TabloAdi.Contains(searchString));
                }
                
                if (!string.IsNullOrEmpty(islemTuru))
                {
                    query = query.Where(l => l.IslemTuru == islemTuru);
                }
                
                if (baslangicTarihi.HasValue)
                {
                    query = query.Where(l => l.IslemTarihi >= baslangicTarihi.Value);
                }
                
                if (bitisTarihi.HasValue)
                {
                    // Bitiş tarihini günün sonuna ayarla
                    var bitisTarihiSonu = bitisTarihi.Value.Date.AddDays(1).AddSeconds(-1);
                    query = query.Where(l => l.IslemTarihi <= bitisTarihiSonu);
                }
                
                // Sıralama
                query = query.OrderByDescending(l => l.IslemTarihi);
                
                // Toplam kayıt sayısı
                var totalItems = await query.CountAsync();
                
                // Sayfalama
                var logs = await query
                    .Skip((page - 1) * pageSize)
                    .Take(pageSize)
                    .ToListAsync();
                
                // ViewModel oluştur
                var viewModel = new SistemLogListViewModel
                {
                    Logs = logs,
                    CurrentPage = page,
                    PageSize = pageSize,
                    TotalItems = totalItems,
                    TotalPages = (int)Math.Ceiling(totalItems / (double)pageSize)
                };
                
                return View(viewModel);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "SistemLog Index sayfası yüklenirken hata oluştu");
                return View("Error", new ErrorViewModel { RequestId = System.Diagnostics.Activity.Current?.Id ?? HttpContext.TraceIdentifier });
            }
        }

        // GET: SistemLog/Details/5
        public async Task<IActionResult> Details(int id)
        {
            try
            {
                var log = await _context.SistemLoglar
                    .FirstOrDefaultAsync(l => l.Id == id);
                
                if (log == null)
                {
                    return NotFound();
                }
                
                // AJAX isteği için modal içeriği döndür
                if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
                {
                    return PartialView("_DetailsPartial", log);
                }
                
                return View(log);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"SistemLog Details sayfası yüklenirken hata oluştu. LogID: {id}");
                return View("Error", new ErrorViewModel { RequestId = System.Diagnostics.Activity.Current?.Id ?? HttpContext.TraceIdentifier });
            }
        }
        
        // GET: SistemLog/CariLogs
        public async Task<IActionResult> CariLogs(string cariId)
        {
            try
            {
                if (Guid.TryParse(cariId, out Guid cariGuid))
                {
                    var cari = await _context.Cariler.FindAsync(cariGuid);
                    
                    if (cari == null)
                    {
                        return NotFound();
                    }
                    
                    // KayitID string olarak saklandığı için string karşılaştırması yapıyoruz
                    var logs = await _context.SistemLoglar
                        .Where(l => l.KayitID == cariId) 
                        .OrderByDescending(l => l.IslemTarihi)
                        .ToListAsync();
                    
                    ViewBag.CariAdi = cari.Ad;
                    ViewBag.CariID = cari.CariID;
                    
                    return View(logs);
                }
                
                return NotFound();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"CariLogs sayfası yüklenirken hata oluştu. CariID: {cariId}");
                return View("Error", new ErrorViewModel { RequestId = System.Diagnostics.Activity.Current?.Id ?? HttpContext.TraceIdentifier });
            }
        }
    }
} 