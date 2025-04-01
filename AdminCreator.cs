using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using MuhasebeStokWebApp.Data;
using MuhasebeStokWebApp.Data.Entities;
using System;
using System.Threading.Tasks;

namespace MuhasebeStokWebApp
{
    public class AdminCreator
    {
        public static async Task CreateAdmin(IServiceProvider serviceProvider)
        {
            using (var scope = serviceProvider.CreateScope())
            {
                var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
                var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
                
                // Admin rolünü oluştur
                if (!await roleManager.RoleExistsAsync("Admin"))
                {
                    await roleManager.CreateAsync(new IdentityRole("Admin"));
                }
                
                // Admin kullanıcısını kontrol et
                if (!userManager.Users.Any(u => u.UserName == "admin"))
                {
                    var adminUser = new ApplicationUser
                    {
                        UserName = "admin",
                        Email = "admin@example.com",
                        EmailConfirmed = true,
                        Ad = "Admin",
                        Soyad = "User",
                        Aktif = true,
                        OlusturmaTarihi = DateTime.Now,
                        Adres = "Test Adresi",
                        TelefonNo = "5551234567",
                        Silindi = false
                    };

                    var result = await userManager.CreateAsync(adminUser, "admin123");
                    
                    if (result.Succeeded)
                    {
                        await userManager.AddToRoleAsync(adminUser, "Admin");
                        Console.WriteLine("Admin kullanıcısı başarıyla oluşturuldu!");
                    }
                    else
                    {
                        Console.WriteLine("Admin kullanıcısı oluşturulamadı:");
                        foreach (var error in result.Errors)
                        {
                            Console.WriteLine($"- {error.Description}");
                        }
                    }
                }
                else
                {
                    Console.WriteLine("Admin kullanıcısı zaten mevcut!");
                }
            }
        }
    }
} 