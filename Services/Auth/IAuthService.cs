using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using MuhasebeStokWebApp.Data.Entities;
using MuhasebeStokWebApp.ViewModels.Account;

namespace MuhasebeStokWebApp.Services.Auth
{
    public interface IAuthService
    {
        Task<(bool Success, string[] Errors)> RegisterAsync(RegisterViewModel model);
        Task<(bool Success, string[] Errors, ClaimsPrincipal Principal)> LoginAsync(LoginViewModel model);
        Task LogoutAsync();
        Task<ApplicationUser> GetUserByIdAsync(string userId);
        Task<ApplicationUser> GetUserByEmailAsync(string email);
        Task<List<ApplicationUser>> GetAllUsersAsync();
        Task<List<IdentityRole>> GetAllRolesAsync();
        Task<List<IdentityRole>> GetUserRolesAsync(string userId);
        Task<(bool Success, string[] Errors)> AddUserToRoleAsync(string userId, string roleName);
        Task<(bool Success, string[] Errors)> RemoveUserFromRoleAsync(string userId, string roleName);
        Task<(bool Success, string[] Errors)> CreateRoleAsync(string roleName);
        Task<(bool Success, string[] Errors)> DeleteRoleAsync(string roleId);
        Task<(bool Success, string[] Errors)> ResetPasswordAsync(ResetPasswordViewModel model);
        Task<(bool Success, string[] Errors)> ChangePasswordAsync(string userId, string currentPassword, string newPassword);
        Task<(bool Success, string[] Errors)> UpdateUserAsync(string userId, string userName, string email);
        Task<(bool Success, string[] Errors)> DeleteUserAsync(string userId);
        Task<bool> IsUserInRoleAsync(string userId, string roleName);
        Task<bool> UserExistsAsync(string userId);
    }
} 