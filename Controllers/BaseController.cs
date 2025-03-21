using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using MuhasebeStokWebApp.Services;
using MuhasebeStokWebApp.ViewModels.Menu;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Authorization;

namespace MuhasebeStokWebApp.Controllers
{
    [Authorize]
    public class BaseController : Controller
    {
        private readonly IMenuService _menuService;
        private readonly UserManager<IdentityUser> _userManager;
        protected readonly RoleManager<IdentityRole> _roleManager;
        private readonly ILogService _logService;
        
        public BaseController(IMenuService menuService, UserManager<IdentityUser> userManager, RoleManager<IdentityRole> roleManager, ILogService logService)
        {
            _menuService = menuService;
            _userManager = userManager;
            _roleManager = roleManager;
            _logService = logService;
        }
        
        public override async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
        {
            try 
            {
                // Kullanıcı kimliği ve rol bilgilerini al
                var userId = User?.FindFirstValue(ClaimTypes.NameIdentifier);
                var userRoles = User?.FindAll(ClaimTypes.Role)?.Select(c => c.Value)?.ToList();
                
                // Kullanıcı giriş yapmışsa ve rolleri varsa
                if (!string.IsNullOrEmpty(userId) && userRoles != null && userRoles.Any())
                {
                    // Admin rolüne sahip kullanıcılar için özel işlemler
                    bool isAdmin = userRoles.Contains("Admin");
                    ViewBag.IsAdmin = isAdmin;
                    
                    try
                    {
                        var roleId = isAdmin ? "Admin" : userRoles.First();
                        var menuItems = await _menuService.GetSidebarMenuByRolIdAsync(roleId);
                        
                        // ViewBag'e menüleri aktar
                        ViewBag.MenuItems = menuItems;
                        ViewBag.IsAuthenticated = true;
                        ViewBag.RequiresLogin = false;
                    }
                    catch (Exception ex)
                    {
                        // Hata durumunda boş menü listesi oluştur
                        ViewBag.MenuItems = new List<MenuViewModel>();
                        ViewBag.IsAuthenticated = true;
                        ViewBag.RequiresLogin = false;
                        await _logService.LogErrorAsync("BaseController.OnActionExecutionAsync", $"Menüler getirilirken hata: {ex.Message}");
                    }
                }
                else
                {
                    // Controller ve Action bilgilerini al
                    var controllerName = context.RouteData.Values["controller"]?.ToString();
                    var actionName = context.RouteData.Values["action"]?.ToString();
                    
                    // Login ve AllowAnonymous durumlarını kontrol et
                    bool isLoginPage = 
                        controllerName?.Equals("Account", StringComparison.OrdinalIgnoreCase) == true && 
                        (actionName?.Equals("Login", StringComparison.OrdinalIgnoreCase) == true ||
                         actionName?.Equals("AccessDenied", StringComparison.OrdinalIgnoreCase) == true ||
                         actionName?.Equals("Register", StringComparison.OrdinalIgnoreCase) == true);
                    
                    bool isAllowAnonymous = 
                        context.ActionDescriptor.EndpointMetadata
                               .Any(m => m.GetType() == typeof(AllowAnonymousAttribute));
                    
                    // Giriş yapmamış kullanıcı için default olarak admin rolünün menülerini göster
                    try
                    {
                        var defaultRoleId = "Admin";
                        var menuItems = await _menuService.GetSidebarMenuByRolIdAsync(defaultRoleId);
                        
                        ViewBag.MenuItems = menuItems;
                        ViewBag.IsAuthenticated = false;
                        ViewBag.IsAdmin = false;
                        
                        // Login gerektiren sayfalar için yönlendirme bayrağı
                        ViewBag.RequiresLogin = !isLoginPage && !isAllowAnonymous;
                    }
                    catch (Exception ex)
                    {
                        // Hata durumunda boş menü listesi oluştur
                        ViewBag.MenuItems = new List<MenuViewModel>();
                        ViewBag.IsAuthenticated = false;
                        ViewBag.IsAdmin = false;
                        ViewBag.RequiresLogin = !isLoginPage && !isAllowAnonymous;
                        await _logService.LogErrorAsync("BaseController.OnActionExecutionAsync", $"Menüler getirilirken hata: {ex.Message}");
                    }
                }
            }
            catch (Exception ex)
            {
                // Herhangi bir hata durumunda boş menü listesi
                ViewBag.MenuItems = new List<MenuViewModel>();
                ViewBag.IsAuthenticated = User?.Identity?.IsAuthenticated ?? false;
                ViewBag.IsAdmin = User?.IsInRole("Admin") ?? false;
                ViewBag.RequiresLogin = !(context.ActionDescriptor.EndpointMetadata.Any(m => m.GetType() == typeof(AllowAnonymousAttribute)));
                await _logService.LogErrorAsync("BaseController.OnActionExecutionAsync", $"Beklenmeyen hata: {ex.Message}");
            }
            
            await base.OnActionExecutionAsync(context, next);
        }
    }
} 