using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using MuhasebeStokWebApp.Data.Entities;
using Microsoft.Extensions.DependencyInjection;

namespace MuhasebeStokWebApp.Data
{
    public static class AppDbInitializer
    {
        public static async Task SeedData(IServiceProvider serviceProvider, ApplicationDbContext context)
        {
            // Veritabanı mevcut değilse oluştur
            await context.Database.EnsureCreatedAsync();
            
            // Fiyat Tipleri
            await SeedFiyatTipleri(context);
            
            // Identity rolleri ve kullanıcılar
            var roleManager = serviceProvider.GetService<RoleManager<IdentityRole>>();
            var userManager = serviceProvider.GetService<UserManager<ApplicationUser>>();
            
            if (roleManager != null && userManager != null)
            {
                await SeedRoles(roleManager);
                await SeedUsers(userManager);
            }
        }
        
        private static async Task SeedFiyatTipleri(ApplicationDbContext context)
        {
            if (!await context.FiyatTipleri.AnyAsync())
            {
                var fiyatTipleri = new List<FiyatTipi>
                {
                    new FiyatTipi { TipAdi = "Liste Fiyatı" },
                    new FiyatTipi { TipAdi = "Maliyet Fiyatı" },
                    new FiyatTipi { TipAdi = "Satış Fiyatı" }
                };
                
                await context.FiyatTipleri.AddRangeAsync(fiyatTipleri);
                await context.SaveChangesAsync();
            }
        }
        
        private static async Task SeedRoles(RoleManager<IdentityRole> roleManager)
        {
            // Roller yoksa ekle
            string[] roleNames = { "Admin", "User", "Manager", "Accountant" };
            
            foreach (var roleName in roleNames)
            {
                if (!await roleManager.RoleExistsAsync(roleName))
                {
                    await roleManager.CreateAsync(new IdentityRole(roleName));
                }
            }
        }
        
        private static async Task SeedUsers(UserManager<ApplicationUser> userManager)
        {
            // Mevcut admin kullanıcısını kontrol et
            var existingAdmin = await userManager.FindByNameAsync("admin");
            
            if (existingAdmin != null)
            {
                // Kilidi sıfırla
                await userManager.SetLockoutEndDateAsync(existingAdmin, null);
                await userManager.ResetAccessFailedCountAsync(existingAdmin);
                
                // Şifreyi sıfırla ve basit bir şifre kullan
                string newPassword = "admin123";
                
                // Password validator'ı geçici olarak devre dışı bırak
                var passwordValidators = userManager.PasswordValidators.ToList();
                userManager.PasswordValidators.Clear();
                
                var token = await userManager.GeneratePasswordResetTokenAsync(existingAdmin);
                var result = await userManager.ResetPasswordAsync(existingAdmin, token, newPassword);
                
                // Password validator'ları geri yükle
                foreach (var validator in passwordValidators)
                {
                    userManager.PasswordValidators.Add(validator);
                }
            }
            else
            {
                // Admin kullanıcısı yoksa ekle
                var adminUser = new ApplicationUser
                {
                    UserName = "admin",
                    Email = "admin@example.com",
                    EmailConfirmed = true
                };
                
                // Password validator'ı geçici olarak devre dışı bırak
                var passwordValidators = userManager.PasswordValidators.ToList();
                userManager.PasswordValidators.Clear();
                
                var result = await userManager.CreateAsync(adminUser, "admin123");
                
                // Password validator'ları geri yükle
                foreach (var validator in passwordValidators)
                {
                    userManager.PasswordValidators.Add(validator);
                }
                
                if (result.Succeeded)
                {
                    await userManager.AddToRoleAsync(adminUser, "Admin");
                }
            }
        }
    }
} 