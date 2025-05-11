using System;
using System.Diagnostics;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using MuhasebeStokWebApp.Models;
using MuhasebeStokWebApp.Services.Interfaces;

namespace MuhasebeStokWebApp.Controllers
{
    [Authorize]
    public class DashboardController : BaseController
    {
        private readonly ILogger<DashboardController> _logger;
        private readonly IDashboardService _dashboardService;
        private readonly IDovizKuruService _dovizKuruService;

        public DashboardController(
            ILogger<DashboardController> logger,
            IDashboardService dashboardService,
            IDovizKuruService dovizKuruService,
            IMenuService menuService,
            Microsoft.AspNetCore.Identity.UserManager<MuhasebeStokWebApp.Data.Entities.ApplicationUser> userManager,
            Microsoft.AspNetCore.Identity.RoleManager<Microsoft.AspNetCore.Identity.IdentityRole> roleManager,
            ILogService logService) 
            : base(menuService, userManager, roleManager, logService)
        {
            _logger = logger;
            _dashboardService = dashboardService;
            _dovizKuruService = dovizKuruService;
        }

        /// <summary>
        /// Ana dashboard sayfasını gösterir
        /// </summary>
        public async Task<IActionResult> Index()
        {
            try
            {
                _logger.LogInformation("Dashboard sayfası yükleniyor...");
                
                // Tüm dashboard verilerini al
                var viewModel = await _dashboardService.GetDashboardDataAsync();
                
                // USD kur değerlerini ViewBag'e ekle (view'da kullanılabilir)
                ViewBag.UsdToTryRate = await _dovizKuruService.GetGuncelKurAsync("USD", "TRY");
                ViewBag.UsdToUzsRate = await _dovizKuruService.GetGuncelKurAsync("USD", "UZS");
                
                _logger.LogInformation("Dashboard sayfası başarıyla yüklendi.");
                
                return View(viewModel);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Dashboard sayfası yüklenirken hata oluştu");
                
                // Hata durumunda uyarı gösterip ana sayfaya yönlendir
                TempData["ErrorMessage"] = "Dashboard verileri yüklenirken bir hata oluştu: " + ex.Message;
                return RedirectToAction("Index", "Home");
            }
        }

        /// <summary>
        /// En çok satılan ürünleri getiren API endpointi
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> GetTopSellingProducts(int count = 5)
        {
            try
            {
                var topProducts = await _dashboardService.GetTopSellingProductsAsync(count);
                return Json(topProducts);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "En çok satılan ürünler alınırken hata oluştu");
                return Json(new { error = ex.Message });
            }
        }

        /// <summary>
        /// En çok satış yapılan carileri getiren API endpointi
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> GetTopCustomers(int count = 5)
        {
            try
            {
                var topCustomers = await _dashboardService.GetTopCustomersAsync(count);
                return Json(topCustomers);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "En çok satış yapılan cariler alınırken hata oluştu");
                return Json(new { error = ex.Message });
            }
        }

        /// <summary>
        /// Aylık satış trendini getiren API endpointi
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> GetMonthlySalesTrend(int months = 12)
        {
            try
            {
                var monthlySalesTrend = await _dashboardService.GetMonthlySalesTrendAsync(months);
                return Json(monthlySalesTrend);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Aylık satış trendi alınırken hata oluştu");
                return Json(new { error = ex.Message });
            }
        }

        /// <summary>
        /// Kritik stoktaki ürünleri getiren API endpointi
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> GetCriticalStockProducts()
        {
            try
            {
                var criticalStockProducts = await _dashboardService.GetCriticalStockProductsAsync();
                return Json(criticalStockProducts);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Kritik stoktaki ürünler alınırken hata oluştu");
                return Json(new { error = ex.Message });
            }
        }

        /// <summary>
        /// Döviz bazlı satış dağılımını getiren API endpointi
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> GetCurrencySalesDistribution()
        {
            try
            {
                var currencySalesDistribution = await _dashboardService.GetCurrencySalesDistributionAsync();
                return Json(currencySalesDistribution);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Döviz bazlı satış dağılımı alınırken hata oluştu");
                return Json(new { error = ex.Message });
            }
        }

        /// <summary>
        /// Toplam alış/satış verilerini getiren API endpointi
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> GetTotalPurchaseSales()
        {
            try
            {
                var totalPurchaseSales = await _dashboardService.GetTotalPurchaseSalesAsync();
                return Json(totalPurchaseSales);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Toplam alış/satış verileri alınırken hata oluştu");
                return Json(new { error = ex.Message });
            }
        }

        /// <summary>
        /// Ödeme/tahsilat özetini getiren API endpointi
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> GetPaymentReceiptSummary()
        {
            try
            {
                var paymentReceiptSummary = await _dashboardService.GetPaymentReceiptSummaryAsync();
                return Json(paymentReceiptSummary);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ödeme/tahsilat özeti alınırken hata oluştu");
                return Json(new { error = ex.Message });
            }
        }

        /// <summary>
        /// Son faturaları getiren API endpointi
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> GetRecentInvoices(int count = 5)
        {
            try
            {
                var recentInvoices = await _dashboardService.GetRecentInvoicesAsync(count);
                return Json(recentInvoices);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Son faturalar alınırken hata oluştu");
                return Json(new { error = ex.Message });
            }
        }

        /// <summary>
        /// Günlük işlem aktivitesini getiren API endpointi
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> GetDailyActivity(int days = 7)
        {
            try
            {
                var dailyActivity = await _dashboardService.GetDailyActivitiesAsync(days);
                return Json(dailyActivity);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Günlük işlem aktivitesi alınırken hata oluştu");
                return Json(new { error = ex.Message });
            }
        }

        /// <summary>
        /// Ortalama kar marjını getiren API endpointi
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> GetProfitMargin()
        {
            try
            {
                var profitMargin = await _dashboardService.GetProfitMarginAsync();
                return Json(profitMargin);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ortalama kar marjı alınırken hata oluştu");
                return Json(new { error = ex.Message });
            }
        }

        /// <summary>
        /// Hata sayfası
        /// </summary>
        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
} 