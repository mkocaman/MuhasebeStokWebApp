using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MuhasebeStokWebApp.Data;
using MuhasebeStokWebApp.Data.Entities;
using MuhasebeStokWebApp.Models;
using MuhasebeStokWebApp.Services;

namespace MuhasebeStokWebApp.Controllers
{
    public class SistemLogController : Controller
    {
        private readonly ILogService _logService;

        public SistemLogController(ILogService logService)
        {
            _logService = logService;
        }

        // GET: SistemLog
        public async Task<IActionResult> Index(string searchString, string islemTuru, DateTime? baslangicTarihi, DateTime? bitisTarihi, int page = 1, int pageSize = 20)
        {
            ViewData["CurrentFilter"] = searchString;
            ViewData["CurrentIslemTuru"] = islemTuru;
            ViewData["CurrentBaslangicTarihi"] = baslangicTarihi?.ToString("yyyy-MM-dd");
            ViewData["CurrentBitisTarihi"] = bitisTarihi?.ToString("yyyy-MM-dd");

            // LogService üzerinden logları al
            IEnumerable<Models.SistemLog> logs;
            
            if (!string.IsNullOrEmpty(islemTuru))
            {
                Models.LogTuru logTuru = (Models.LogTuru)Enum.Parse(typeof(Models.LogTuru), islemTuru);
                logs = await _logService.GetLogsByTurAsync(logTuru, baslangicTarihi, bitisTarihi);
            }
            else
            {
                logs = await _logService.GetLogsAsync(baslangicTarihi, bitisTarihi);
            }
            
            // Arama filtresi uygula
            if (!string.IsNullOrEmpty(searchString))
            {
                logs = logs.Where(l => 
                    (l.Aciklama != null && l.Aciklama.Contains(searchString)) || 
                    (l.KullaniciAdi != null && l.KullaniciAdi.Contains(searchString)));
            }
            
            // Sayfalama
            int totalItems = logs.Count();
            int totalPages = (int)Math.Ceiling(totalItems / (double)pageSize);
            
            logs = logs.Skip((page - 1) * pageSize).Take(pageSize);
            
            ViewData["TotalPages"] = totalPages;
            ViewData["CurrentPage"] = page;
            
            return View(logs);
        }

        // GET: SistemLog/CariLogs
        public async Task<IActionResult> CariLogs(Guid cariId)
        {
            if (cariId == Guid.Empty)
            {
                return NotFound();
            }

            // İlgili cariye ait logları getir
            var logs = await _logService.GetCariLogsAsync(cariId);
            
            if (logs == null)
            {
                logs = new List<Models.SistemLog>();
            }
            
            ViewData["CariID"] = cariId;
            
            return View(logs);
        }

        // GET: SistemLog/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var logs = await _logService.GetLogsAsync();
            var log = logs.FirstOrDefault(l => l.SistemLogID == id);
            
            if (log == null)
            {
                return NotFound();
            }

            return View(log);
        }

        // GET: SistemLog/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var logs = await _logService.GetLogsAsync();
            var log = logs.FirstOrDefault(l => l.SistemLogID == id);
            
            if (log == null)
            {
                return NotFound();
            }

            return View(log);
        }

        // POST: SistemLog/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            // LogService üzerinden silme işlemini gerçekleştir
            await _logService.DeleteLogAsync(id);
            
            // Log silme işlemini kaydet
            await _logService.LogEkleAsync($"Log kaydı silindi: ID={id}", Models.LogTuru.Bilgi);
            
            return RedirectToAction(nameof(Index));
        }

        // POST: SistemLog/ClearLogs
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ClearLogs()
        {
            await _logService.ClearLogsAsync();
            return RedirectToAction(nameof(Index));
        }
    }
} 