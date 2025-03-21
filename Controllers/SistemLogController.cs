using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MuhasebeStokWebApp.Data;
using MuhasebeStokWebApp.Data.Entities;
using MuhasebeStokWebApp.ViewModels.SistemLog;

namespace MuhasebeStokWebApp.Controllers
{
    public class SistemLogController : Controller
    {
        private readonly ApplicationDbContext _context;

        public SistemLogController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: SistemLog
        public async Task<IActionResult> Index(string searchString, string islemTuru, DateTime? baslangicTarihi, DateTime? bitisTarihi, int page = 1, int pageSize = 20)
        {
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
                    l.KayitAdi.Contains(searchString) || 
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

        // GET: SistemLog/Details/5
        public async Task<IActionResult> Details(Guid id)
        {
            var log = await _context.SistemLoglar.FindAsync(id);
            
            if (log == null)
            {
                return NotFound();
            }
            
            return View(log);
        }
        
        // GET: SistemLog/CariLogs
        public async Task<IActionResult> CariLogs(Guid cariId)
        {
            var cari = await _context.Cariler.FindAsync(cariId);
            
            if (cari == null)
            {
                return NotFound();
            }
            
            var logs = await _context.SistemLoglar
                .Where(l => l.KayitID == cariId && l.TabloAdi == "Cariler")
                .OrderByDescending(l => l.IslemTarihi)
                .ToListAsync();
            
            ViewBag.CariAdi = cari.CariAdi;
            ViewBag.CariID = cari.CariID;
            
            return View(logs);
        }
    }
} 