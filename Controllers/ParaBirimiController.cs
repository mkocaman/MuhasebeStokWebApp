using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using MuhasebeStokWebApp.Data;
using System;
using System.Linq;

namespace MuhasebeStokWebApp.Controllers
{
    public class ParaBirimiController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<ParaBirimiController> _logger;

        public ParaBirimiController(ApplicationDbContext context, ILogger<ParaBirimiController> logger)
        {
            _context = context;
            _logger = logger;
        }

        // Para birimi için güncel kur bilgisini getir
        [HttpGet("GetLatestExchangeRateById")]
        public IActionResult GetLatestExchangeRate(Guid? paraBirimiID)
        {
            try
            {
                if (paraBirimiID == null)
                    return Json(new { success = false, message = "Para birimi ID gereklidir." });

                var paraBirimi = _context.ParaBirimleri.FirstOrDefault(p => p.ParaBirimiID == paraBirimiID);
                if (paraBirimi == null)
                    return Json(new { success = false, message = "Para birimi bulunamadı." });
                
                // Para birimi ana para birimi ise kur 1'dir
                if (paraBirimi.AnaParaBirimiMi)
                {
                    return Json(new { success = true, kur = 1.0m });
                }

                // En güncel kur bilgisini bul
                var guncelKur = _context.KurDegerleri
                    .Where(k => k.ParaBirimiID == paraBirimiID && !k.Silindi)
                    .OrderByDescending(k => k.Tarih)
                    .FirstOrDefault();

                if (guncelKur != null)
                {
                    // Satış kuru döndür
                    return Json(new { success = true, kur = guncelKur.Satis });
                }
                else
                {
                    // Kur bulunamadı, varsayılan değer döndür
                    return Json(new { success = false, message = "Güncel kur bilgisi bulunamadı.", kur = 1.0m });
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Kur bilgisi alınırken hata oluştu");
                return Json(new { success = false, message = "Kur bilgisi alınırken hata oluştu: " + ex.Message, kur = 1.0m });
            }
        }

        // Para birimi kodu ile güncel kur bilgisini getir
        [HttpGet]
        [Route("ParaBirimi/GetLatestExchangeRateByKod")]
        public IActionResult GetLatestExchangeRateByKod(string kod)
        {
            try
            {
                if (string.IsNullOrEmpty(kod))
                    return Json(new { success = false, message = "Para birimi kodu gereklidir." });

                var paraBirimi = _context.ParaBirimleri.FirstOrDefault(p => p.Kod == kod && !p.Silindi);
                if (paraBirimi == null)
                    return Json(new { success = false, message = "Para birimi bulunamadı." });
                
                // Para birimi ana para birimi ise kur 1'dir
                if (paraBirimi.AnaParaBirimiMi)
                {
                    return Json(new { success = true, kur = 1.0m });
                }

                // En güncel kur bilgisini bul
                var guncelKur = _context.KurDegerleri
                    .Where(k => k.ParaBirimiID == paraBirimi.ParaBirimiID && !k.Silindi)
                    .OrderByDescending(k => k.Tarih)
                    .FirstOrDefault();

                if (guncelKur != null)
                {
                    // Satış kuru döndür
                    return Json(new { success = true, kur = guncelKur.Satis });
                }
                else
                {
                    // Kur bulunamadı, varsayılan değer döndür
                    return Json(new { success = false, message = "Güncel kur bilgisi bulunamadı.", kur = 1.0m });
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Kur bilgisi alınırken hata oluştu");
                return Json(new { success = false, message = "Kur bilgisi alınırken hata oluştu: " + ex.Message, kur = 1.0m });
            }
        }
    }
} 