using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using MuhasebeStokWebApp.Data;
using MuhasebeStokWebApp.Services.Interfaces;

namespace MuhasebeStokWebApp.Controllers.ParaBirimiModulu.API
{
    [Route("api/[controller]")]
    [ApiController]
    public class KurController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogService _logService;
        private readonly ILogger<KurController> _logger;
        private readonly IDovizKuruService _dovizKuruService;

        public KurController(
            ApplicationDbContext context, 
            ILogService logService, 
            ILogger<KurController> logger,
            IDovizKuruService dovizKuruService)
        {
            _context = context;
            _logService = logService;
            _logger = logger;
            _dovizKuruService = dovizKuruService;
        }

        /// <summary>
        /// Verilen para birimi kodu için güncel kur değerini getir
        /// </summary>
        /// <param name="kod">Para birimi kodu</param>
        /// <returns>Güncel kur bilgisi</returns>
        [HttpGet("GetGuncelKur")]
        public async Task<IActionResult> GetGuncelKur(string kod)
        {
            try
            {
                if (string.IsNullOrEmpty(kod))
                {
                    return BadRequest(new { success = false, message = "Para birimi kodu belirtilmedi." });
                }

                // Önce para birimini bul
                var paraBirimi = await _context.ParaBirimleri
                    .FirstOrDefaultAsync(p => p.Kod == kod && !p.Silindi);
                
                if (paraBirimi == null)
                {
                    return NotFound(new { success = false, message = "Para birimi bulunamadı." });
                }

                // Ana para birimi kontrolü
                if (paraBirimi.AnaParaBirimiMi)
                {
                    return Ok(new { success = true, kurDegeri = 1.0000m, message = "Ana para birimi için kur değeri 1'dir." });
                }

                // Güncel kur değerini bul
                var guncelKur = await _context.KurDegerleri
                    .Where(k => k.ParaBirimiID == paraBirimi.ParaBirimiID && !k.Silindi)
                    .OrderByDescending(k => k.Tarih)
                    .FirstOrDefaultAsync();

                if (guncelKur == null)
                {
                    return NotFound(new { success = false, message = "Bu para birimi için güncel kur bilgisi bulunamadı." });
                }

                return Ok(new { success = true, kurDegeri = guncelKur.Satis, message = "Kur değeri başarıyla getirildi." });
            }
            catch (Exception ex)
            {
                await _logService.LogErrorAsync("GetGuncelKur API", ex.Message);
                _logger.LogError(ex, "Para birimi kur değeri alınırken bir hata oluştu: {Message}", ex.Message);
                return StatusCode(500, new { success = false, message = $"Kur bilgisi alınırken bir hata oluştu: {ex.Message}" });
            }
        }

        /// <summary>
        /// İki para birimi arasındaki güncel kur değerini getir
        /// </summary>
        /// <param name="kaynakKod">Kaynak para birimi kodu</param>
        /// <param name="hedefKod">Hedef para birimi kodu</param>
        /// <returns>İki para birimi arasındaki güncel kur değeri</returns>
        [HttpGet("GetParaBirimleriArasiKur")]
        [HttpGet("DovizKuru")]
        public async Task<IActionResult> GetParaBirimleriArasiKur(string kaynakKod, string hedefKod, string fromCurrency = null, string toCurrency = null)
        {
            try
            {
                // fromCurrency ve toCurrency parametrelerini de destekleyelim
                string from = fromCurrency ?? kaynakKod;
                string to = toCurrency ?? hedefKod;
                
                if (string.IsNullOrEmpty(from) || string.IsNullOrEmpty(to))
                {
                    return BadRequest(new { success = false, message = "Kaynak ve hedef para birimi kodları belirtilmelidir." });
                }

                // Döviz kuru servisini kullan
                var kurDegeri = await _dovizKuruService.GetGuncelKurAsync(from, to);
                
                return Ok(new { success = true, kurDegeri = kurDegeri, message = "Kur değeri başarıyla getirildi." });
            }
            catch (Exception ex)
            {
                await _logService.LogErrorAsync("GetParaBirimleriArasiKur API", ex.Message);
                _logger.LogError(ex, "Para birimleri arası kur değeri alınırken bir hata oluştu: {Message}", ex.Message);
                return StatusCode(500, new { success = false, message = $"Kur bilgisi alınırken bir hata oluştu: {ex.Message}" });
            }
        }
    }
} 