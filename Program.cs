using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using MuhasebeStokWebApp.Data;
using MuhasebeStokWebApp.Data.Repositories;
using MuhasebeStokWebApp.Data.Entities;
using Microsoft.AspNetCore.Authentication.Cookies;
using MuhasebeStokWebApp.Services.Interfaces;
using MuhasebeStokWebApp.Services;
using MuhasebeStokWebApp.Services.Menu;
using MuhasebeStokWebApp.Services.Auth;
using MuhasebeStokWebApp.Services.ParaBirimiModulu;
using MuhasebeStokWebApp.Services.Currency;
using MuhasebeStokWebApp.Middleware;
using Microsoft.AspNetCore.SignalR;
using MuhasebeStokWebApp.Services.Notification;
using MuhasebeStokWebApp.Hubs;
using MuhasebeStokWebApp.Services.Report;
using MuhasebeStokWebApp.Services.Email;
using Serilog;
using Serilog.Events;
using Microsoft.Extensions.Localization;
using Microsoft.AspNetCore.Localization;
using System.Globalization;
using System;
using System.IO;
using System.Linq;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using MuhasebeStokWebApp.Data.EfCore;
using MuhasebeStokWebApp; // AdminCreator için gerekli

var builder = WebApplication.CreateBuilder(args);

// Serilog yapılandırması
Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Information()
    .MinimumLevel.Override("Microsoft", LogEventLevel.Warning)
    .Enrich.FromLogContext()
    .WriteTo.Console()
    .WriteTo.File("Logs/log-.txt", 
        rollingInterval: RollingInterval.Day,
        retainedFileCountLimit: 31)
    .CreateLogger();

builder.Host.UseSerilog();

// Add services to the container.
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");

// SQL Server kullanımı
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(
        connectionString, 
        b => b.MigrationsAssembly("MuhasebeStokWebApp")
    ));

// Identity servislerini ekliyoruz
builder.Services.AddIdentity<ApplicationUser, IdentityRole>(options => 
{
    // Şifre politikaları - Geliştirme için basitleştirildi
    options.Password.RequireDigit = false;  
    options.Password.RequireLowercase = false;
    options.Password.RequireUppercase = false;
    options.Password.RequireNonAlphanumeric = false;
    options.Password.RequiredLength = 6;
    
    // Kullanıcı politikaları
    options.User.RequireUniqueEmail = true;
    
    // Lockout ayarları
    options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(30);
    options.Lockout.MaxFailedAccessAttempts = 5;
})
    .AddEntityFrameworkStores<ApplicationDbContext>()
    .AddDefaultTokenProviders();

// Role bazlı yetkilendirme ekliyoruz
builder.Services.AddAuthorization(options =>
{
    // Admin rolüne sahip kullanıcılar için politika
    options.AddPolicy("RequireAdminRole", policy => policy.RequireRole("Admin"));
    
    // Manager rolüne sahip kullanıcılar için politika
    options.AddPolicy("RequireManagerRole", policy => policy.RequireRole("Manager"));
    
    // Accountant rolüne sahip kullanıcılar için politika
    options.AddPolicy("RequireAccountantRole", policy => policy.RequireRole("Accountant"));
    
    // Herhangi bir role sahip kullanıcılar için politika
    options.AddPolicy("AuthenticatedUser", policy => policy.RequireAuthenticatedUser());
});

// Session servisi ekliyoruz
builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromHours(2);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

// Authentication cookie ayarları
builder.Services.AddAuthentication(options =>
{
    options.DefaultScheme = "Cookies";
    options.DefaultSignInScheme = "Cookies";
})
.AddCookie("Cookies", options =>
{
    options.Cookie.Name = "MuhasebeStokAuth";
    options.Cookie.HttpOnly = true;
    options.ExpireTimeSpan = TimeSpan.FromHours(24);
    options.LoginPath = "/Account/Login";
    options.LogoutPath = "/Account/Logout";
    options.AccessDeniedPath = "/Account/AccessDenied";
    options.SlidingExpiration = true;
    options.Cookie.SameSite = SameSiteMode.Lax;
});

// Data Protection servisini ekliyoruz
builder.Services.AddDataProtection();

// Repository ve UnitOfWork servislerini ekliyoruz
builder.Services.AddScoped(typeof(IRepository<>), typeof(Repository<>));
builder.Services.AddScoped<MuhasebeStokWebApp.Data.Repositories.IUnitOfWork, MuhasebeStokWebApp.Data.Repositories.UnitOfWork>();

// HttpClient servisini ekliyoruz
builder.Services.AddHttpClient();

// LogService bağımlılığını ekliyoruz
builder.Services.AddScoped<ILogService, LogService>();

// ValidationService'i ekliyoruz
builder.Services.AddScoped<IValidationService, ValidationService>();

// DbMigrationService'i ekliyoruz
builder.Services.AddScoped<IDbMigrationService, DbMigrationService>();

// SistemLogService'i ekliyoruz
builder.Services.AddScoped<MuhasebeStokWebApp.Services.SistemLogService>();
builder.Services.AddHttpContextAccessor();

// UserManager'ı ekliyoruz
builder.Services.AddScoped<UserManager>();

// StokFifoService'i ekliyoruz
builder.Services.AddScoped<StokFifoService>();

// DropdownService'i ekliyoruz
builder.Services.AddScoped<IDropdownService, DropdownService>();

// CariService'i ekliyoruz
builder.Services.AddScoped<ICariService, CariService>();

// SistemAyarService'i ekliyoruz
builder.Services.AddScoped<ISistemAyarService, SistemAyarService>();

// AuthService'i ekliyoruz
builder.Services.AddScoped<IAuthService, AuthService>();

// MenuService'i ekliyoruz
builder.Services.AddScoped<IMenuService, MenuService>();

// BirimService'i ekliyoruz
builder.Services.AddScoped<IBirimService, BirimService>();

// Para birimi modülü servislerini ekliyoruz
builder.Services.AddScoped<MuhasebeStokWebApp.Services.ParaBirimiModulu.IParaBirimiService, MuhasebeStokWebApp.Services.ParaBirimiModulu.ParaBirimiService>();
builder.Services.AddScoped<MuhasebeStokWebApp.Services.ParaBirimiModulu.IKurDegeriService, MuhasebeStokWebApp.Services.ParaBirimiModulu.KurDegeriService>();
builder.Services.AddScoped<MuhasebeStokWebApp.Services.ParaBirimiModulu.IParaBirimiIliskiService, MuhasebeStokWebApp.Services.ParaBirimiModulu.ParaBirimiIliskiService>();

// Services.Interfaces altındaki IParaBirimiService'i de açıkça kaydedelim
builder.Services.AddScoped<MuhasebeStokWebApp.Services.Interfaces.IParaBirimiService, MuhasebeStokWebApp.Services.ParaBirimiAdapter>();

// IDovizKuruService servisini DovizKuruServiceAdapter ile eşleştiriyoruz
builder.Services.AddScoped<MuhasebeStokWebApp.Services.Interfaces.IDovizKuruService, MuhasebeStokWebApp.Services.ParaBirimiModulu.DovizKuruServiceAdapter>();

// CurrencyService'i ekliyoruz
builder.Services.AddScoped<ICurrencyService, CurrencyService>();

// ReportService'i ekliyoruz
builder.Services.AddScoped<IReportService, ReportService>();

builder.Services.AddMemoryCache();

builder.Services.AddSignalR();
builder.Services.Configure<EmailSettings>(builder.Configuration.GetSection("EmailSettings"));
// Arayüzlerin uygulamalarını açıkça tanımlayarak çakışmaları önlüyoruz
builder.Services.AddScoped<MuhasebeStokWebApp.Services.Email.IEmailService, MuhasebeStokWebApp.Services.Email.EmailService>();
builder.Services.AddScoped<MuhasebeStokWebApp.Services.Interfaces.IEmailService, MuhasebeStokWebApp.Services.EmailService>();
builder.Services.AddScoped<INotificationService, NotificationService>();

// Localization
builder.Services.AddLocalization(options => options.ResourcesPath = "Resources");
builder.Services.AddMvc()
    .AddViewLocalization()
    .AddDataAnnotationsLocalization();

// IMemoryCache servisi ekle (önbellek için)
builder.Services.AddMemoryCache();

builder.Services.Configure<RequestLocalizationOptions>(options =>
{
    var supportedCultures = new[]
    {
        new CultureInfo("tr-TR"),
        new CultureInfo("en-US")
    };
    options.DefaultRequestCulture = new RequestCulture("tr-TR");
    options.SupportedCultures = supportedCultures;
    options.SupportedUICultures = supportedCultures;
});

// CORS yapılandırması ekle
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

builder.Services.AddControllersWithViews();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
}
else
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

// Veritabanı migration'larını uygula (sadece migration uygula, temizleme ve örnek veri oluşturma yapma)
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    try
    {
        var context = services.GetRequiredService<ApplicationDbContext>();
        // Sadece migration'ları uygula
        context.Database.Migrate();
        
        // Admin kullanıcısını oluştur
        await AdminCreator.CreateAdmin(services);
    }
    catch (Exception ex)
    {
        var logger = services.GetRequiredService<ILogger<Program>>();
        logger.LogError(ex, "Veritabanı migration uygulanırken hata oluştu.");
    }
}

// Özel hata sayfaları için durum kodu sayfalarını ayarlıyoruz
app.UseStatusCodePagesWithReExecute("/Home/StatusCode", "?code={0}");

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

// CORS middleware'ini ekle - UseRouting'den sonra, UseAuthorization'dan önce olmalı
app.UseCors("AllowAll");

// Session middleware'i önce kullanılmalı
app.UseSession();

// Authentication ve Authorization middleware'leri
app.UseAuthentication();
app.UseAuthorization();

// Global exception handling middleware'ini ekle
app.UseMiddleware<GlobalExceptionHandlingMiddleware>();

// SignalR endpoint'ini ekle
app.MapHub<NotificationHub>("/notificationHub");

// Localization middleware
app.UseRequestLocalization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
