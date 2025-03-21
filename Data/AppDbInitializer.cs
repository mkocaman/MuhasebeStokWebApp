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
            var userManager = serviceProvider.GetService<UserManager<IdentityUser>>();
            
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
        
        private static async Task SeedUsers(UserManager<IdentityUser> userManager)
        {
            // Admin kullanıcısı yoksa ekle
            if (await userManager.FindByNameAsync("admin") == null)
            {
                var adminUser = new IdentityUser
                {
                    UserName = "admin",
                    Email = "admin@example.com",
                    EmailConfirmed = true
                };
                
                var result = await userManager.CreateAsync(adminUser, "admin123");
                
                if (result.Succeeded)
                {
                    await userManager.AddToRoleAsync(adminUser, "Admin");
                }
            }
        }
    }
} 