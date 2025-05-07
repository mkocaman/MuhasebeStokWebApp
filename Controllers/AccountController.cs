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
using Microsoft.AspNetCore.Http;
using MuhasebeStokWebApp.Data.Entities;
using MuhasebeStokWebApp.ViewModels.Account;
using MuhasebeStokWebApp.Data;
using MuhasebeStokWebApp.Services.Interfaces;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http.Features;
using System.IO;

namespace MuhasebeStokWebApp.Controllers
{
    // Kullanıcı yönetimi ve kimlik doğrulama işlemlerini yöneten controller
    public class AccountController : BaseController
    {
        private readonly ILogger<AccountController> _logger;
        private readonly SignInManager<ApplicationUser> _signInManager;
        protected new readonly UserManager<ApplicationUser> _userManager;
        protected new readonly RoleManager<IdentityRole> _roleManager;
        private readonly ApplicationDbContext _context;
        private new readonly ILogService _logService;
        private readonly IWebHostEnvironment _webHostEnvironment;

        // Constructor: Dependency Injection ile gerekli servisleri alır
        public AccountController(
            ILogger<AccountController> logger,
            IMenuService menuService,
            UserManager<ApplicationUser> userManager,
            RoleManager<IdentityRole> roleManager,
            SignInManager<ApplicationUser> signInManager,
            ILogService logService,
            ApplicationDbContext context,
            IWebHostEnvironment webHostEnvironment) : base(menuService, userManager, roleManager, logService)
        {
            _logger = logger;
            _signInManager = signInManager;
            _userManager = userManager;
            _roleManager = roleManager;
            _context = context;
            _logService = logService;
            _webHostEnvironment = webHostEnvironment;
        }

        // Giriş sayfasını gösterir, zaten giriş yapmış kullanıcılar ana sayfaya yönlendirilir
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

        // Kullanıcı giriş işlemini gerçekleştirir
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(string username, string password, string rememberMe, string returnUrl)
        {
            try
            {
                // Parametre kontrolü
                if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password))
                {
                    ModelState.AddModelError("", "Kullanıcı adı ve şifre gereklidir.");
                    return View();
                }

                // rememberMe değerini kontrol et
                bool isPersistent = rememberMe == "on";

                _logger.LogInformation($"Giriş denemesi: {username}");
                
                // ASP.NET Identity ile şifre kontrolü yapılır
                var result = await _signInManager.PasswordSignInAsync(username, password, isPersistent, lockoutOnFailure: false);
                
                if (result.Succeeded)
                {
                    // Giriş başarılı ise log kaydı yapılır
                    _logger.LogInformation($"Kullanıcı başarıyla giriş yaptı: {username}");
                    
                    // Session'a kullanıcı bilgilerini ekliyoruz
                    var user = await _userManager.FindByNameAsync(username);
                    HttpContext.Session.SetString("UserId", user.Id);
                    HttpContext.Session.SetString("UserName", user.UserName);
                    
                    _logger.LogInformation($"Session oluşturuldu: UserID={user.Id}, UserName={user.UserName}");
                    _logger.LogInformation($"Yönlendirme adresi: {(string.IsNullOrEmpty(returnUrl) ? "Ana Sayfa" : returnUrl)}");
                    
                    // ReturnUrl varsa o sayfaya, yoksa ana sayfaya yönlendirilir
                    if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
                    {
                        _logger.LogInformation($"Kullanıcı {returnUrl} adresine yönlendiriliyor");
                        return Redirect(returnUrl);
                    }
                    
                    _logger.LogInformation("Kullanıcı ana sayfaya yönlendiriliyor");
                    // Yönlendirme sırasında özel bir parametre ekleyerek takip ediyoruz
                    return RedirectToAction("Index", "Home", new { auth = "success" });
                }
                
                if (result.IsLockedOut)
                {
                    // Hesap kilitlenmişse uyarı verilir
                    _logger.LogWarning($"Kullanıcı hesabı kilitlendi: {username}");
                    ModelState.AddModelError("", "Hesabınız çok fazla başarısız giriş denemesi nedeniyle kilitlendi. Lütfen daha sonra tekrar deneyin.");
                    return View();
                }
                
                // Özel admin hesabı kontrolü - Geliştirme/Test için basitleştirilmiş giriş
                if (username == "admin" && password == "admin123")
                {
                    // Admin kullanıcısını veritabanında ara
                    var adminUser = await _userManager.FindByNameAsync("admin");
                    
                    // Eğer admin kullanıcısı varsa, giriş yap
                    if (adminUser != null)
                    {
                        // Doğrudan sign-in yapılır (şifre kontrolü olmadan)
                        await _signInManager.SignInAsync(adminUser, isPersistent: isPersistent);
                        
                        // Session'a kullanıcı bilgilerini ekliyoruz
                        HttpContext.Session.SetString("UserId", adminUser.Id);
                        HttpContext.Session.SetString("UserName", adminUser.UserName);
                        HttpContext.Session.SetString("IsAdmin", "true");
                        
                        _logger.LogInformation($"Admin kullanıcısı başarıyla giriş yaptı: {username}");
                        _logger.LogInformation($"Admin session oluşturuldu: UserID={adminUser.Id}, UserName={adminUser.UserName}");
                        _logger.LogInformation($"Yönlendirme adresi: {(string.IsNullOrEmpty(returnUrl) ? "Ana Sayfa" : returnUrl)}");
                        
                        if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
                        {
                            _logger.LogInformation($"Admin kullanıcısı {returnUrl} adresine yönlendiriliyor");
                            return Redirect(returnUrl);
                        }
                        
                        _logger.LogInformation("Admin kullanıcısı ana sayfaya yönlendiriliyor");
                        return RedirectToAction("Index", "Home", new { auth = "admin_success" });
                    }
                }

                // Başarısız giriş denemesi loglanır
                _logger.LogWarning($"Başarısız giriş denemesi: {username}");
                ModelState.AddModelError("", "Kullanıcı adı veya şifre hatalı");
                return View();
            }
            catch (Exception ex)
            {
                // Hata durumunda log kaydı yapılır
                _logger.LogError(ex, "Giriş sırasında hata oluştu");
                ModelState.AddModelError("", "Giriş sırasında bir hata oluştu: " + ex.Message);
                return View();
            }
        }

        // Kullanıcı çıkış işlemini gerçekleştirir
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

        // Yetkisiz erişim sayfası
        [AllowAnonymous]
        public IActionResult AccessDenied()
        {
            return View();
        }
        
        // Yeni kullanıcı kayıt formu (sadece admin yetkili)
        [HttpGet]
        [Authorize(Roles = "Admin")]
        public IActionResult Register()
        {
            return View();
        }
        
        // Yeni kullanıcı kayıt işlemi (sadece admin yetkili)
        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Register(MuhasebeStokWebApp.ViewModels.Account.RegisterViewModel model)
        {
            if (ModelState.IsValid)
            {
                // Yeni kullanıcı oluştur
                var user = new ApplicationUser { UserName = model.Email, Email = model.Email };
                var result = await _userManager.CreateAsync(user, model.Password);
                
                if (result.Succeeded)
                {
                    // Varsayılan olarak User rolünü ata
                    await _userManager.AddToRoleAsync(user, "User");
                    
                    _logger.LogInformation($"Admin {User.Identity.Name} tarafından yeni kullanıcı oluşturuldu: {model.Email}");
                    
                    TempData["SuccessMessage"] = "Kullanıcı başarıyla oluşturuldu.";
                    return RedirectToAction("UserManagement");
                }
                
                // Hata mesajlarını model state'e ekle
                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError("", error.Description);
                }
            }
            
            return View(model);
        }
        
        // Kullanıcı listesi yönetim sayfası (sadece admin yetkili)
        [HttpGet]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> UserManagement()
        {
            // Tüm kullanıcıları listele
            var users = await _userManager.Users.ToListAsync();
            var userViewModels = new List<UserViewModel>();
            
            // Her kullanıcı için rol bilgisini ekle
            foreach (var user in users)
            {
                var roles = await _userManager.GetRolesAsync(user);
                userViewModels.Add(new UserViewModel
                {
                    Id = user.Id,
                    UserName = user.UserName,
                    Email = user.Email,
                    Roles = roles.ToList()
                });
            }
            
            return View(userViewModels);
        }
        
        // Kullanıcı düzenleme formu (sadece admin yetkili)
        [HttpGet]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> EditUser(string id)
        {
            var user = await _userManager.FindByIdAsync(id);
            
            if (user == null)
            {
                return NotFound();
            }
            
            // Kullanıcının mevcut rollerini ve tüm rolleri getir
            var roles = await _userManager.GetRolesAsync(user);
            var allRoles = _roleManager.Roles.ToList();
            
            var model = new EditUserViewModel();
            model.Id = user.Id;
            model.UserName = user.UserName;
            model.Email = user.Email;
            model.UserRoles = roles;
            model.AllRoles = allRoles;
            
            return View(model);
        }
        
        // Kullanıcı bilgilerini ve rollerini güncelleme (sadece admin yetkili)
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
        
        // Kullanıcı silme işlemi (sadece admin yetkili)
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

        // Kullanıcı profil sayfası
        [HttpGet]
        [Authorize]
        public async Task<IActionResult> Profile()
        {
            try
            {
                // Mevcut kullanıcıyı al
                var user = await _userManager.GetUserAsync(User);
                if (user == null)
                {
                    return RedirectToAction("Login");
                }

                // Kullanıcının rollerini al
                var roles = await _userManager.GetRolesAsync(user);

                // Profil modelini oluştur
                var model = new ProfileViewModel
                {
                    Id = user.Id,
                    UserName = user.UserName,
                    Email = user.Email,
                    PhoneNumber = user.PhoneNumber,
                    Roles = roles.ToList(),
                    FullName = user.FullName ?? "",
                    Bio = user.Bio ?? "",
                    ProfileImage = user.ProfileImage ?? "/assets/images/dashboard/1.png"
                };

                // Başlık ayarla
                ViewData["Title"] = "Profil Sayfası";
                
                return View(model);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Profil görüntüleme sırasında hata oluştu");
                TempData["ErrorMessage"] = "Profil bilgileri yüklenirken bir hata oluştu.";
                return RedirectToAction("Index", "Home");
            }
        }
        
        // Kullanıcı profil güncelleme
        [HttpPost]
        [Authorize]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateProfile(ProfileViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View("Profile", model);
            }

            try
            {
                // Mevcut kullanıcıyı al
                var user = await _userManager.GetUserAsync(User);
                if (user == null)
                {
                    return RedirectToAction("Login");
                }

                // Kullanıcı bilgilerini güncelle
                user.PhoneNumber = model.PhoneNumber;
                user.FullName = model.FullName;
                user.Bio = model.Bio;
                
                // Profil resmi yükleme işlemi
                if (model.ProfilePhoto != null)
                {
                    // Dosya uzantısını kontrol et
                    var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif" };
                    var extension = Path.GetExtension(model.ProfilePhoto.FileName).ToLowerInvariant();
                    
                    if (!allowedExtensions.Contains(extension))
                    {
                        ModelState.AddModelError("ProfilePhoto", "Sadece .jpg, .jpeg, .png ve .gif uzantılı dosyalar yüklenebilir.");
                        return View("Profile", model);
                    }
                    
                    // Dosya boyutunu kontrol et (en fazla 5MB)
                    if (model.ProfilePhoto.Length > 5 * 1024 * 1024)
                    {
                        ModelState.AddModelError("ProfilePhoto", "Dosya boyutu 5MB'den büyük olamaz.");
                        return View("Profile", model);
                    }
                    
                    // Dosya adını oluştur
                    var fileName = $"{user.Id}_{DateTime.Now.Ticks}{extension}";
                    var filePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "profilePhotos", fileName);
                    
                    // Dosyayı kaydet
                    using (var stream = new FileStream(filePath, FileMode.Create))
                    {
                        await model.ProfilePhoto.CopyToAsync(stream);
                    }
                    
                    // Veritabanında profil resmini güncelle
                    user.ProfileImage = $"/profilePhotos/{fileName}";
                }
                
                // Email değişikliği varsa
                if (user.Email != model.Email)
                {
                    // Email güncelleme işlemi
                    var setEmailResult = await _userManager.SetEmailAsync(user, model.Email);
                    if (!setEmailResult.Succeeded)
                    {
                        foreach (var error in setEmailResult.Errors)
                        {
                            ModelState.AddModelError("", error.Description);
                        }
                        return View("Profile", model);
                    }
                    
                    // Kullanıcı adı email ile aynı ise onu da güncelle
                    if (user.UserName == user.Email)
                    {
                        var setUserNameResult = await _userManager.SetUserNameAsync(user, model.Email);
                        if (!setUserNameResult.Succeeded)
                        {
                            foreach (var error in setUserNameResult.Errors)
                            {
                                ModelState.AddModelError("", error.Description);
                            }
                            return View("Profile", model);
                        }
                    }
                }

                // Kullanıcı güncellemesini kaydet
                var result = await _userManager.UpdateAsync(user);
                if (result.Succeeded)
                {
                    // İşlem başarılı logu
                    await _logService.LogBilgi("Profil Güncellendi", $"Kullanıcı {user.UserName} profil bilgilerini güncelledi", user.UserName);
                    
                    TempData["SuccessMessage"] = "Profil bilgileriniz başarıyla güncellendi.";
                    return RedirectToAction("Profile");
                }
                else
                {
                    foreach (var error in result.Errors)
                    {
                        ModelState.AddModelError("", error.Description);
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Profil güncelleme sırasında hata oluştu");
                ModelState.AddModelError("", "Profil güncellenirken bir hata oluştu: " + ex.Message);
            }

            return View("Profile", model);
        }
        
        // Şifre değiştirme
        [HttpPost]
        [Authorize]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ChangePassword(ChangePasswordViewModel model)
        {
            if (!ModelState.IsValid)
            {
                // Profil modelini yeniden oluştur
                var user = await _userManager.GetUserAsync(User);
                var roles = await _userManager.GetRolesAsync(user);
                
                var profileModel = new ProfileViewModel
                {
                    Id = user.Id,
                    UserName = user.UserName,
                    Email = user.Email,
                    PhoneNumber = user.PhoneNumber,
                    Roles = roles.ToList(),
                    FullName = user.FullName ?? "",
                    Bio = user.Bio ?? "",
                    ProfileImage = user.ProfileImage ?? "/assets/images/dashboard/1.png"
                };
                
                return View("Profile", profileModel);
            }

            try
            {
                var user = await _userManager.GetUserAsync(User);
                if (user == null)
                {
                    return RedirectToAction("Login");
                }

                // Şifre değiştirme
                var result = await _userManager.ChangePasswordAsync(user, model.OldPassword, model.NewPassword);
                if (result.Succeeded)
                {
                    // İşlem başarılı logu
                    await _logService.LogBilgi("Şifre Değiştirildi", $"Kullanıcı {user.UserName} şifresini değiştirdi", user.UserName);
                    
                    TempData["SuccessMessage"] = "Şifreniz başarıyla değiştirildi.";
                    
                    // Kullanıcıyı yeniden giriş yapmaya zorla
                    await _signInManager.RefreshSignInAsync(user);
                    
                    return RedirectToAction("Profile");
                }
                else
                {
                    foreach (var error in result.Errors)
                    {
                        ModelState.AddModelError("", error.Description);
                    }
                    
                    // Profil modelini yeniden oluştur
                    var roles = await _userManager.GetRolesAsync(user);
                    var profileModel = new ProfileViewModel
                    {
                        Id = user.Id,
                        UserName = user.UserName,
                        Email = user.Email,
                        PhoneNumber = user.PhoneNumber,
                        Roles = roles.ToList(),
                        FullName = user.FullName ?? "",
                        Bio = user.Bio ?? "",
                        ProfileImage = user.ProfileImage ?? "/assets/images/dashboard/1.png"
                    };
                    
                    return View("Profile", profileModel);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Şifre değiştirme sırasında hata oluştu");
                TempData["ErrorMessage"] = "Şifre değiştirilirken bir hata oluştu: " + ex.Message;
                return RedirectToAction("Profile");
            }
        }
    }
} 