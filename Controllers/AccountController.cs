using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System.Diagnostics;
using MuhasebeStokWebApp.Services;
using MuhasebeStokWebApp.Services.Menu;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Authorization;
using MuhasebeStokWebApp.Models;
using System.Linq;
using Microsoft.EntityFrameworkCore;

namespace MuhasebeStokWebApp.Controllers
{
    public class AccountController : BaseController
    {
        private readonly ILogger<AccountController> _logger;
        private readonly SignInManager<IdentityUser> _signInManager;
        private readonly UserManager<IdentityUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;

        public AccountController(
            ILogger<AccountController> logger,
            IMenuService menuService,
            UserManager<IdentityUser> userManager,
            RoleManager<IdentityRole> roleManager,
            SignInManager<IdentityUser> signInManager,
            ILogService logService) : base(menuService, userManager, roleManager, logService)
        {
            _logger = logger;
            _signInManager = signInManager;
            _userManager = userManager;
            _roleManager = roleManager;
        }

        [AllowAnonymous]
        public IActionResult Login(string returnUrl = null)
        {
            if (User.Identity.IsAuthenticated)
            {
                return RedirectToAction("Index", "Home");
            }
            
            ViewBag.ReturnUrl = returnUrl;
            ViewBag.EnableRegistration = false;
            return View();
        }

        [HttpPost]
        [AllowAnonymous]
        public async Task<IActionResult> Login(string username, string password, bool rememberMe = false, string returnUrl = null)
        {
            try
            {
                _logger.LogInformation($"Giriş denemesi: {username}");
                
                var result = await _signInManager.PasswordSignInAsync(username, password, rememberMe, lockoutOnFailure: true);
                
                if (result.Succeeded)
                {
                    _logger.LogInformation($"Kullanıcı giriş yaptı: {username}");
                    
                    if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
                    {
                        return Redirect(returnUrl);
                    }
                    
                    return RedirectToAction("Index", "Home");
                }
                
                if (result.IsLockedOut)
                {
                    _logger.LogWarning($"Kullanıcı hesabı kilitlendi: {username}");
                    ModelState.AddModelError("", "Hesabınız çok fazla başarısız giriş denemesi nedeniyle kilitlendi. Lütfen daha sonra tekrar deneyin.");
                    return View();
                }
                
                // Eğer admin123/admin kullanıcı adı ve şifresi girilmişse, eski doğrulama yöntemini kullan
                if (username == "admin" && password == "admin123")
                {
                    // Admin kullanıcısını veritabanında ara
                    var adminUser = await _userManager.FindByNameAsync("admin");
                    
                    // Eğer admin kullanıcısı varsa, giriş yap
                    if (adminUser != null)
                    {
                        await _signInManager.SignInAsync(adminUser, isPersistent: rememberMe);
                        
                        _logger.LogInformation($"Kullanıcı giriş yaptı (eski yöntem): {username}");
                        
                        if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
                        {
                            return Redirect(returnUrl);
                        }
                        
                        return RedirectToAction("Index", "Home");
                    }
                }

                _logger.LogWarning($"Başarısız giriş denemesi: {username}");
                ModelState.AddModelError("", "Kullanıcı adı veya şifre hatalı");
                return View();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Giriş sırasında hata oluştu");
                ModelState.AddModelError("", "Giriş sırasında bir hata oluştu: " + ex.Message);
                return View();
            }
        }

        [AllowAnonymous]
        public async Task<IActionResult> Logout()
        {
            try
            {
                await _signInManager.SignOutAsync();
                _logger.LogInformation("Kullanıcı çıkış yaptı");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Çıkış sırasında hata oluştu");
            }
            
            return RedirectToAction("Login");
        }

        [AllowAnonymous]
        public IActionResult AccessDenied()
        {
            return View();
        }
        
        [HttpGet]
        [Authorize(Roles = "Admin")]
        public IActionResult Register()
        {
            return View();
        }
        
        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Register(RegisterViewModel model)
        {
            if (ModelState.IsValid)
            {
                var user = new IdentityUser { UserName = model.Email, Email = model.Email };
                var result = await _userManager.CreateAsync(user, model.Password);
                
                if (result.Succeeded)
                {
                    // Varsayılan olarak User rolünü ata
                    await _userManager.AddToRoleAsync(user, "User");
                    
                    _logger.LogInformation($"Admin {User.Identity.Name} tarafından yeni kullanıcı oluşturuldu: {model.Email}");
                    
                    TempData["SuccessMessage"] = "Kullanıcı başarıyla oluşturuldu.";
                    return RedirectToAction("UserManagement");
                }
                
                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError("", error.Description);
                }
            }
            
            return View(model);
        }
        
        [HttpGet]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> UserManagement()
        {
            var users = await _userManager.Users.ToListAsync();
            var userViewModels = new List<UserViewModel>();
            
            foreach (var user in users)
            {
                var roles = await _userManager.GetRolesAsync(user);
                userViewModels.Add(new UserViewModel
                {
                    Id = user.Id,
                    UserName = user.UserName,
                    Email = user.Email,
                    Roles = roles
                });
            }
            
            return View(userViewModels);
        }
        
        [HttpGet]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> EditUser(string id)
        {
            var user = await _userManager.FindByIdAsync(id);
            
            if (user == null)
            {
                return NotFound();
            }
            
            var roles = await _userManager.GetRolesAsync(user);
            var allRoles = _roleManager.Roles.ToList();
            
            var model = new EditUserViewModel
            {
                Id = user.Id,
                UserName = user.UserName,
                Email = user.Email,
                UserRoles = roles,
                AllRoles = allRoles
            };
            
            return View(model);
        }
        
        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> EditUser(EditUserViewModel model, string[] selectedRoles)
        {
            var user = await _userManager.FindByIdAsync(model.Id);
            
            if (user == null)
            {
                return NotFound();
            }
            
            // Kullanıcı bilgilerini güncelle
            user.UserName = model.UserName;
            user.Email = model.Email;
            
            var result = await _userManager.UpdateAsync(user);
            
            if (!result.Succeeded)
            {
                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError("", error.Description);
                }
                
                var allRoles = _roleManager.Roles.ToList();
                model.AllRoles = allRoles;
                return View(model);
            }
            
            // Rolleri güncelle
            var userRoles = await _userManager.GetRolesAsync(user);
            selectedRoles = selectedRoles ?? new string[] { };
            
            // Kaldırılacak rolleri belirle
            var rolesToRemove = userRoles.Except(selectedRoles).ToArray();
            
            // Eklenecek rolleri belirle
            var rolesToAdd = selectedRoles.Except(userRoles).ToArray();
            
            // Rolleri kaldır
            if (rolesToRemove.Any())
            {
                result = await _userManager.RemoveFromRolesAsync(user, rolesToRemove);
                
                if (!result.Succeeded)
                {
                    foreach (var error in result.Errors)
                    {
                        ModelState.AddModelError("", error.Description);
                    }
                    
                    var allRoles = _roleManager.Roles.ToList();
                    model.AllRoles = allRoles;
                    return View(model);
                }
            }
            
            // Rolleri ekle
            if (rolesToAdd.Any())
            {
                result = await _userManager.AddToRolesAsync(user, rolesToAdd);
                
                if (!result.Succeeded)
                {
                    foreach (var error in result.Errors)
                    {
                        ModelState.AddModelError("", error.Description);
                    }
                    
                    var allRoles = _roleManager.Roles.ToList();
                    model.AllRoles = allRoles;
                    return View(model);
                }
            }
            
            TempData["SuccessMessage"] = "Kullanıcı başarıyla güncellendi.";
            return RedirectToAction("UserManagement");
        }
        
        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteUser(string id)
        {
            var user = await _userManager.FindByIdAsync(id);
            
            if (user == null)
            {
                return NotFound();
            }
            
            // Admin hesabını silmeyi engelle
            if (await _userManager.IsInRoleAsync(user, "Admin") && user.UserName == "admin")
            {
                TempData["ErrorMessage"] = "Ana admin hesabı silinemez.";
                return RedirectToAction("UserManagement");
            }
            
            var result = await _userManager.DeleteAsync(user);
            
            if (!result.Succeeded)
            {
                TempData["ErrorMessage"] = "Kullanıcı silinirken bir hata oluştu.";
            }
            else
            {
                TempData["SuccessMessage"] = "Kullanıcı başarıyla silindi.";
            }
            
            return RedirectToAction("UserManagement");
        }
    }
} 