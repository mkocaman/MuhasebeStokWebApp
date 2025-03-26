using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.EntityFrameworkCore;
using MuhasebeStokWebApp.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI;
using MuhasebeStokWebApp.Services;
using MuhasebeStokWebApp.Services.Interfaces;
using MuhasebeStokWebApp.Data.Repositories;
using MuhasebeStokWebApp.Services.Menu;
using MuhasebeStokWebApp.Services.DovizModulu;

namespace MuhasebeStokWebApp
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddControllersWithViews();
            
            // Entity Framework için SQL Server bağlantısı ekleniyor
            services.AddDbContext<ApplicationDbContext>(options =>
                options.UseSqlServer(
                    Configuration.GetConnectionString("DefaultConnection"), 
                    b => b.MigrationsAssembly("MuhasebeStokWebApp")));
            
            // Identity servisleri ekleniyor
            services.AddIdentity<MuhasebeStokWebApp.Data.Entities.ApplicationUser, IdentityRole>(options => 
            {
                // Parola gereksinimleri
                options.Password.RequireDigit = true;
                options.Password.RequireLowercase = true;
                options.Password.RequireUppercase = true;
                options.Password.RequireNonAlphanumeric = false;
                options.Password.RequiredLength = 6;
                
                // Kullanıcı gereksinimleri
                options.User.RequireUniqueEmail = true;
                
                // Giriş gereksinimleri
                options.SignIn.RequireConfirmedEmail = false;
                options.SignIn.RequireConfirmedAccount = false;
            })
            .AddEntityFrameworkStores<ApplicationDbContext>()
            .AddDefaultTokenProviders();
            
            // Oturum yönetimi ayarları
            services.AddSession(options =>
            {
                options.IdleTimeout = TimeSpan.FromMinutes(30);
                options.Cookie.HttpOnly = true;
                options.Cookie.IsEssential = true;
            });

            // HttpClient servisini ekliyoruz
            services.AddHttpClient();
            
            // Uygulama servisleri burada ekleniyor
            // Repository ve UnitOfWork servisleri
            services.AddScoped<IUnitOfWork, UnitOfWork>();
            
            // Menu servisleri
            services.AddScoped<IMenuService, MenuService>();
            
            // Log servisi
            services.AddScoped<ILogService, LogService>();
            
            // Döviz modülü servisleri
            services.AddScoped<IDovizKuruService, DovizKuruService>();
            services.AddScoped<IDovizService, DovizService>();
            
            // BirimService kayıt
            services.AddScoped<IBirimService, BirimService>();
            
            // SistemAyarService DI ayarı
            services.AddScoped<MuhasebeStokWebApp.Services.Interfaces.ISistemAyarService, MuhasebeStokWebApp.Services.SistemAyarService>();
            
            // Kimlik doğrulama ayarları
            services.ConfigureApplicationCookie(options =>
            {
                options.LoginPath = "/Account/Login";
                options.LogoutPath = "/Account/Logout";
                options.AccessDeniedPath = "/Account/AccessDenied";
                options.SlidingExpiration = true;
                options.ExpireTimeSpan = TimeSpan.FromDays(1);
            });
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler("/Home/Error");
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }
            
            app.UseStatusCodePagesWithReExecute("/Home/StatusCode", "?code={0}");
            
            app.UseHttpsRedirection();
            app.UseStaticFiles();

            app.UseRouting();

            // Oturum ve kimlik doğrulama middleware'leri
            app.UseSession();
            app.UseAuthentication();
            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllerRoute(
                    name: "areas",
                    pattern: "{area:exists}/{controller=Home}/{action=Index}/{id?}");
                
                endpoints.MapControllerRoute(
                    name: "default",
                    pattern: "{controller=Home}/{action=Index}/{id?}");
            });
        }
    }
} 