using System;
using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Logging;
using MuhasebeStokWebApp.Data.Entities;
using MuhasebeStokWebApp.Models;
using MuhasebeStokWebApp.Services;
using MuhasebeStokWebApp.Services.Interfaces;
using MuhasebeStokWebApp.Services.Menu;

namespace MuhasebeStokWebApp.Controllers
{
    /// <summary>
    /// Merkezi exception handling yönetimi sağlayan controller base sınıfı
    /// </summary>
    public class BaseExceptionController : BaseController
    {
        protected readonly ILogger _logger;
        protected readonly Services.Interfaces.IExceptionHandlingService _exceptionHandlingService;

        public BaseExceptionController(
            IMenuService menuService,
            UserManager<ApplicationUser> userManager,
            RoleManager<IdentityRole> roleManager,
            ILogService logService,
            ILogger logger,
            Services.Interfaces.IExceptionHandlingService exceptionHandlingService)
            : base(menuService, userManager, roleManager, logService)
        {
            _logger = logger;
            _exceptionHandlingService = exceptionHandlingService;
        }

        /// <summary>
        /// Hata loglama işlemlerini özelleştirmek için override edildi
        /// </summary>
        protected override async Task LogErrorAsync(string message, Exception ex = null)
        {
            if (ex != null)
            {
                // ExceptionHandlingService üzerinden hata mesajını standardize et
                string formattedMessage = _exceptionHandlingService.GetUserFriendlyErrorMessage(ex);
                _logger.LogError(ex, $"{message} - {formattedMessage}");
            }
            else
            {
                _logger.LogError(message);
            }

            // Base sınıftaki log servisini de kullan
            await base.LogErrorAsync(message, ex);
        }

        /// <summary>
        /// Action'dan önce çağrılan hook'u override ederek hata yönetimini etkinleştir
        /// </summary>
        protected override async Task OnBeforeActionExecutionAsync(ActionExecutingContext context)
        {
            // Hata yönetim servisinin hazır olduğunu ViewBag aracılığıyla bildir
            ViewBag.HasExceptionHandling = true;
            
            await base.OnBeforeActionExecutionAsync(context);
        }

        /// <summary>
        /// Controller eylemlerini try-catch bloğu içinde çalıştıran yardımcı metot.
        /// Hata durumunda uygun hata mesajını görüntüler.
        /// </summary>
        /// <typeparam name="T">Eylem sonucunun tipi</typeparam>
        /// <param name="action">Yürütülecek eylem</param>
        /// <param name="errorMessage">Varsayılan hata mesajı</param>
        /// <param name="errorRedirectAction">Hata durumunda yönlendirilecek eylem adı (varsayılan: Index)</param>
        /// <returns>Eylem sonucu</returns>
        protected async Task<IActionResult> ExecuteWithExceptionHandlingAsync<T>(
            Func<Task<T>> action,
            string errorMessage = "İşlem sırasında bir hata oluştu.",
            string errorRedirectAction = "Index")
            where T : IActionResult
        {
            try
            {
                return await action();
            }
            catch (Exception ex)
            {
                string userFriendlyMessage = _exceptionHandlingService.GetUserFriendlyErrorMessage(ex);
                await LogErrorAsync(errorMessage, ex);
                
                // AJAX isteği mi kontrol et
                if (IsAjaxRequest())
                {
                    return Json(new { success = false, message = userFriendlyMessage });
                }
                
                TempData["ErrorMessage"] = userFriendlyMessage;
                
                if (string.IsNullOrEmpty(errorRedirectAction))
                {
                    return RedirectToAction("Index");
                }
                
                return RedirectToAction(errorRedirectAction);
            }
        }
        
        /// <summary>
        /// Controller eylemlerini try-catch bloğu içinde çalıştıran yardımcı metot.
        /// Hata durumunda JSON yanıtı döndürür.
        /// </summary>
        /// <typeparam name="T">Eylem sonucunun tipi</typeparam>
        /// <param name="action">Yürütülecek eylem</param>
        /// <param name="errorMessage">Varsayılan hata mesajı</param>
        /// <returns>Eylem sonucu</returns>
        protected async Task<IActionResult> ExecuteWithJsonExceptionHandlingAsync<T>(
            Func<Task<T>> action,
            string errorMessage = "İşlem sırasında bir hata oluştu.")
            where T : IActionResult
        {
            try
            {
                return await action();
            }
            catch (Exception ex)
            {
                string userFriendlyMessage = _exceptionHandlingService.GetUserFriendlyErrorMessage(ex);
                await LogErrorAsync(errorMessage, ex);
                
                return Json(new { success = false, message = userFriendlyMessage });
            }
        }
        
        /// <summary>
        /// Controller eylemlerini try-catch bloğu içinde çalıştıran yardımcı metot.
        /// Hata durumunda doğrudan View döndürür.
        /// </summary>
        /// <typeparam name="T">Eylem sonucunun tipi</typeparam>
        /// <param name="action">Yürütülecek eylem</param>
        /// <param name="errorMessage">Varsayılan hata mesajı</param>
        /// <param name="errorViewName">Hata durumunda gösterilecek view adı (varsayılan: Error)</param>
        /// <returns>Eylem sonucu</returns>
        protected async Task<IActionResult> ExecuteWithViewExceptionHandlingAsync<T>(
            Func<Task<T>> action,
            string errorMessage = "İşlem sırasında bir hata oluştu.",
            string errorViewName = "Error")
            where T : IActionResult
        {
            try
            {
                return await action();
            }
            catch (Exception ex)
            {
                string userFriendlyMessage = _exceptionHandlingService.GetUserFriendlyErrorMessage(ex);
                await LogErrorAsync(errorMessage, ex);
                
                var errorViewModel = new ErrorViewModel
                {
                    RequestId = HttpContext.TraceIdentifier,
                    Message = userFriendlyMessage,
                    Title = "Hata Oluştu"
                };
                
                return View(errorViewName, errorViewModel);
            }
        }
    }
} 