using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using MuhasebeStokWebApp.ViewModels.Account;
using MuhasebeStokWebApp.Services.Interfaces;
using MuhasebeStokWebApp.Data.Entities;

namespace MuhasebeStokWebApp.Services.Auth
{
    public class AuthService : IAuthService
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly ILogService _logService;

        public AuthService(
            UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager,
            RoleManager<IdentityRole> roleManager,
            ILogService logService)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _roleManager = roleManager;
            _logService = logService;
        }

        public async Task<(bool Success, string[] Errors)> RegisterAsync(RegisterViewModel model)
        {
            try
            {
                var user = new ApplicationUser { UserName = model.Email, Email = model.Email };
                var result = await _userManager.CreateAsync(user, model.Password);

                if (result.Succeeded)
                {
                    await _logService.LogInfoAsync("AuthService.RegisterAsync", $"Kullanıcı oluşturuldu: {model.Email}");
                    return (true, Array.Empty<string>());
                }

                await _logService.LogWarningAsync("AuthService.RegisterAsync", $"Kullanıcı oluşturulamadı: {model.Email}. Hatalar: {string.Join(", ", result.Errors.Select(e => e.Description))}");
                return (false, result.Errors.Select(e => e.Description).ToArray());
            }
            catch (Exception ex)
            {
                await _logService.LogErrorAsync("AuthService.RegisterAsync", $"Hata: {ex.Message}");
                return (false, new[] { "Bir hata oluştu. Lütfen daha sonra tekrar deneyin." });
            }
        }

        public async Task<(bool Success, string[] Errors, ClaimsPrincipal Principal)> LoginAsync(LoginViewModel model)
        {
            try
            {
                var result = await _signInManager.PasswordSignInAsync(model.Email, model.Password, model.RememberMe, lockoutOnFailure: true);

                if (result.Succeeded)
                {
                    await _logService.LogInfoAsync("AuthService.LoginAsync", $"Kullanıcı girişi başarılı: {model.Email}");
                    
                    var user = await _userManager.FindByEmailAsync(model.Email);
                    var claims = new List<Claim>
                    {
                        new Claim(ClaimTypes.Name, user.UserName ?? string.Empty),
                        new Claim(ClaimTypes.Email, user.Email ?? string.Empty),
                        new Claim(ClaimTypes.NameIdentifier, user.Id)
                    };

                    var userRoles = await _userManager.GetRolesAsync(user);
                    foreach (var role in userRoles)
                    {
                        claims.Add(new Claim(ClaimTypes.Role, role));
                    }

                    var identity = new ClaimsIdentity(claims, "Identity.Application");
                    var principal = new ClaimsPrincipal(identity);

                    return (true, Array.Empty<string>(), principal);
                }

                var errorMessage = result.IsLockedOut 
                    ? "Hesabınız kilitlendi." 
                    : "Geçersiz giriş denemesi.";
                
                await _logService.LogWarningAsync("AuthService.LoginAsync", $"Giriş başarısız: {model.Email}. Hata: {errorMessage}");
                return (false, new[] { errorMessage }, null!);
            }
            catch (Exception ex)
            {
                await _logService.LogErrorAsync("AuthService.LoginAsync", $"Hata: {ex.Message}");
                return (false, new[] { "Bir hata oluştu. Lütfen daha sonra tekrar deneyin." }, null!);
            }
        }

        public async Task LogoutAsync()
        {
            try
            {
                await _signInManager.SignOutAsync();
                await _logService.LogInfoAsync("AuthService.LogoutAsync", "Kullanıcı çıkış yaptı");
            }
            catch (Exception ex)
            {
                await _logService.LogErrorAsync("AuthService.LogoutAsync", $"Hata: {ex.Message}");
            }
        }

        public async Task<ApplicationUser> GetUserByIdAsync(string userId)
        {
            try
            {
                return await _userManager.FindByIdAsync(userId);
            }
            catch (Exception ex)
            {
                await _logService.LogErrorAsync("AuthService.GetUserByIdAsync", $"Hata: {ex.Message}");
                return null;
            }
        }

        public async Task<ApplicationUser> GetUserByEmailAsync(string email)
        {
            try
            {
                return await _userManager.FindByEmailAsync(email);
            }
            catch (Exception ex)
            {
                await _logService.LogErrorAsync("AuthService.GetUserByEmailAsync", $"Hata: {ex.Message}");
                return null;
            }
        }

        public async Task<List<ApplicationUser>> GetAllUsersAsync()
        {
            try
            {
                return await _userManager.Users.ToListAsync();
            }
            catch (Exception ex)
            {
                await _logService.LogErrorAsync("AuthService.GetAllUsersAsync", $"Hata: {ex.Message}");
                return new List<ApplicationUser>();
            }
        }

        public async Task<List<IdentityRole>> GetAllRolesAsync()
        {
            try
            {
                return await _roleManager.Roles.ToListAsync();
            }
            catch (Exception ex)
            {
                await _logService.LogErrorAsync("AuthService.GetAllRolesAsync", $"Hata: {ex.Message}");
                return new List<IdentityRole>();
            }
        }

        public async Task<List<IdentityRole>> GetUserRolesAsync(string userId)
        {
            try
            {
                var user = await _userManager.FindByIdAsync(userId);
                if (user == null)
                {
                    return new List<IdentityRole>();
                }

                var roleNames = await _userManager.GetRolesAsync(user);
                var roles = new List<IdentityRole>();

                foreach (var roleName in roleNames)
                {
                    var role = await _roleManager.FindByNameAsync(roleName);
                    if (role != null)
                    {
                        roles.Add(role);
                    }
                }

                return roles;
            }
            catch (Exception ex)
            {
                await _logService.LogErrorAsync("AuthService.GetUserRolesAsync", $"Hata: {ex.Message}");
                return new List<IdentityRole>();
            }
        }

        public async Task<(bool Success, string[] Errors)> AddUserToRoleAsync(string userId, string roleName)
        {
            try
            {
                var user = await _userManager.FindByIdAsync(userId);
                if (user == null)
                {
                    return (false, new[] { "Kullanıcı bulunamadı." });
                }

                var roleExists = await _roleManager.RoleExistsAsync(roleName);
                if (!roleExists)
                {
                    return (false, new[] { "Rol bulunamadı." });
                }

                var result = await _userManager.AddToRoleAsync(user, roleName);
                if (result.Succeeded)
                {
                    await _logService.LogInfoAsync("AuthService.AddUserToRoleAsync", $"Kullanıcıya rol eklendi: {user.Email} -> {roleName}");
                    return (true, Array.Empty<string>());
                }

                await _logService.LogWarningAsync("AuthService.AddUserToRoleAsync", $"Kullanıcıya rol eklenemedi: {user.Email} -> {roleName}. Hatalar: {string.Join(", ", result.Errors.Select(e => e.Description))}");
                return (false, result.Errors.Select(e => e.Description).ToArray());
            }
            catch (Exception ex)
            {
                await _logService.LogErrorAsync("AuthService.AddUserToRoleAsync", $"Hata: {ex.Message}");
                return (false, new[] { "Bir hata oluştu. Lütfen daha sonra tekrar deneyin." });
            }
        }

        public async Task<(bool Success, string[] Errors)> RemoveUserFromRoleAsync(string userId, string roleName)
        {
            try
            {
                var user = await _userManager.FindByIdAsync(userId);
                if (user == null)
                {
                    return (false, new[] { "Kullanıcı bulunamadı." });
                }

                var result = await _userManager.RemoveFromRoleAsync(user, roleName);
                if (result.Succeeded)
                {
                    await _logService.LogInfoAsync("AuthService.RemoveUserFromRoleAsync", $"Kullanıcıdan rol kaldırıldı: {user.Email} -> {roleName}");
                    return (true, Array.Empty<string>());
                }

                await _logService.LogWarningAsync("AuthService.RemoveUserFromRoleAsync", $"Kullanıcıdan rol kaldırılamadı: {user.Email} -> {roleName}. Hatalar: {string.Join(", ", result.Errors.Select(e => e.Description))}");
                return (false, result.Errors.Select(e => e.Description).ToArray());
            }
            catch (Exception ex)
            {
                await _logService.LogErrorAsync("AuthService.RemoveUserFromRoleAsync", $"Hata: {ex.Message}");
                return (false, new[] { "Bir hata oluştu. Lütfen daha sonra tekrar deneyin." });
            }
        }

        public async Task<(bool Success, string[] Errors)> CreateRoleAsync(string roleName)
        {
            try
            {
                var roleExists = await _roleManager.RoleExistsAsync(roleName);
                if (roleExists)
                {
                    return (false, new[] { $"'{roleName}' rolü zaten mevcut." });
                }

                var result = await _roleManager.CreateAsync(new IdentityRole(roleName));
                if (result.Succeeded)
                {
                    await _logService.LogInfoAsync("AuthService.CreateRoleAsync", $"Rol oluşturuldu: {roleName}");
                    return (true, Array.Empty<string>());
                }

                await _logService.LogWarningAsync("AuthService.CreateRoleAsync", $"Rol oluşturulamadı: {roleName}. Hatalar: {string.Join(", ", result.Errors.Select(e => e.Description))}");
                return (false, result.Errors.Select(e => e.Description).ToArray());
            }
            catch (Exception ex)
            {
                await _logService.LogErrorAsync("AuthService.CreateRoleAsync", $"Hata: {ex.Message}");
                return (false, new[] { "Bir hata oluştu. Lütfen daha sonra tekrar deneyin." });
            }
        }

        public async Task<(bool Success, string[] Errors)> DeleteRoleAsync(string roleId)
        {
            try
            {
                var role = await _roleManager.FindByIdAsync(roleId);
                if (role == null)
                {
                    return (false, new[] { "Rol bulunamadı." });
                }

                var result = await _roleManager.DeleteAsync(role);
                if (result.Succeeded)
                {
                    await _logService.LogInfoAsync("AuthService.DeleteRoleAsync", $"Rol silindi: {role.Name}");
                    return (true, Array.Empty<string>());
                }

                await _logService.LogWarningAsync("AuthService.DeleteRoleAsync", $"Rol silinemedi: {role.Name}. Hatalar: {string.Join(", ", result.Errors.Select(e => e.Description))}");
                return (false, result.Errors.Select(e => e.Description).ToArray());
            }
            catch (Exception ex)
            {
                await _logService.LogErrorAsync("AuthService.DeleteRoleAsync", $"Hata: {ex.Message}");
                return (false, new[] { "Bir hata oluştu. Lütfen daha sonra tekrar deneyin." });
            }
        }

        public async Task<(bool Success, string[] Errors)> ResetPasswordAsync(ResetPasswordViewModel model)
        {
            try
            {
                var user = await _userManager.FindByEmailAsync(model.Email);
                if (user == null)
                {
                    return (false, new[] { "Kullanıcı bulunamadı." });
                }

                var result = await _userManager.ResetPasswordAsync(user, model.Code, model.Password);
                if (result.Succeeded)
                {
                    await _logService.LogInfoAsync("AuthService.ResetPasswordAsync", $"Şifre sıfırlandı: {model.Email}");
                    return (true, Array.Empty<string>());
                }

                await _logService.LogWarningAsync("AuthService.ResetPasswordAsync", $"Şifre sıfırlanamadı: {model.Email}. Hatalar: {string.Join(", ", result.Errors.Select(e => e.Description))}");
                return (false, result.Errors.Select(e => e.Description).ToArray());
            }
            catch (Exception ex)
            {
                await _logService.LogErrorAsync("AuthService.ResetPasswordAsync", $"Hata: {ex.Message}");
                return (false, new[] { "Bir hata oluştu. Lütfen daha sonra tekrar deneyin." });
            }
        }

        public async Task<(bool Success, string[] Errors)> ChangePasswordAsync(string userId, string currentPassword, string newPassword)
        {
            try
            {
                var user = await _userManager.FindByIdAsync(userId);
                if (user == null)
                {
                    return (false, new[] { "Kullanıcı bulunamadı." });
                }

                var result = await _userManager.ChangePasswordAsync(user, currentPassword, newPassword);
                if (result.Succeeded)
                {
                    await _logService.LogInfoAsync("AuthService.ChangePasswordAsync", $"Şifre değiştirildi: {user.Email}");
                    return (true, Array.Empty<string>());
                }

                await _logService.LogWarningAsync("AuthService.ChangePasswordAsync", $"Şifre değiştirilemedi: {user.Email}. Hatalar: {string.Join(", ", result.Errors.Select(e => e.Description))}");
                return (false, result.Errors.Select(e => e.Description).ToArray());
            }
            catch (Exception ex)
            {
                await _logService.LogErrorAsync("AuthService.ChangePasswordAsync", $"Hata: {ex.Message}");
                return (false, new[] { "Bir hata oluştu. Lütfen daha sonra tekrar deneyin." });
            }
        }

        public async Task<(bool Success, string[] Errors)> UpdateUserAsync(string userId, string userName, string email)
        {
            try
            {
                var user = await _userManager.FindByIdAsync(userId);
                if (user == null)
                {
                    return (false, new[] { "Kullanıcı bulunamadı." });
                }

                user.UserName = userName;
                user.Email = email;

                var result = await _userManager.UpdateAsync(user);
                if (result.Succeeded)
                {
                    await _logService.LogInfoAsync("AuthService.UpdateUserAsync", $"Kullanıcı güncellendi: {user.Email}");
                    return (true, Array.Empty<string>());
                }

                await _logService.LogWarningAsync("AuthService.UpdateUserAsync", $"Kullanıcı güncellenemedi: {user.Email}. Hatalar: {string.Join(", ", result.Errors.Select(e => e.Description))}");
                return (false, result.Errors.Select(e => e.Description).ToArray());
            }
            catch (Exception ex)
            {
                await _logService.LogErrorAsync("AuthService.UpdateUserAsync", $"Hata: {ex.Message}");
                return (false, new[] { "Bir hata oluştu. Lütfen daha sonra tekrar deneyin." });
            }
        }

        public async Task<(bool Success, string[] Errors)> DeleteUserAsync(string userId)
        {
            try
            {
                var user = await _userManager.FindByIdAsync(userId);
                if (user == null)
                {
                    return (false, new[] { "Kullanıcı bulunamadı." });
                }

                var result = await _userManager.DeleteAsync(user);
                if (result.Succeeded)
                {
                    await _logService.LogInfoAsync("AuthService.DeleteUserAsync", $"Kullanıcı silindi: {user.Email}");
                    return (true, Array.Empty<string>());
                }

                await _logService.LogWarningAsync("AuthService.DeleteUserAsync", $"Kullanıcı silinemedi: {user.Email}. Hatalar: {string.Join(", ", result.Errors.Select(e => e.Description))}");
                return (false, result.Errors.Select(e => e.Description).ToArray());
            }
            catch (Exception ex)
            {
                await _logService.LogErrorAsync("AuthService.DeleteUserAsync", $"Hata: {ex.Message}");
                return (false, new[] { "Bir hata oluştu. Lütfen daha sonra tekrar deneyin." });
            }
        }

        public async Task<bool> IsUserInRoleAsync(string userId, string roleName)
        {
            try
            {
                var user = await _userManager.FindByIdAsync(userId);
                if (user == null)
                {
                    return false;
                }

                return await _userManager.IsInRoleAsync(user, roleName);
            }
            catch (Exception ex)
            {
                await _logService.LogErrorAsync("AuthService.IsUserInRoleAsync", $"Hata: {ex.Message}");
                return false;
            }
        }

        public async Task<bool> UserExistsAsync(string userId)
        {
            try
            {
                var user = await _userManager.FindByIdAsync(userId);
                return user != null;
            }
            catch (Exception ex)
            {
                await _logService.LogErrorAsync("AuthService.UserExistsAsync", $"Hata: {ex.Message}");
                return false;
            }
        }
    }
} 