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
using MuhasebeStokWebApp.Services.ParaBirimiBirlesikModul;
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
using AutoMapper;
using Microsoft.Extensions.Options;
using Microsoft.AspNetCore.Mvc.Razor;
using Microsoft.AspNetCore.Http.Features;
using System.Reflection;
using FluentValidation;
using FluentValidation.AspNetCore;
using MuhasebeStokWebApp.Validators;

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
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection") ??
                       throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");

// SQL Server kullanımı
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(connectionString, sqlServerOptions => 
    {
        sqlServerOptions.MigrationsAssembly("MuhasebeStokWebApp");
        sqlServerOptions.CommandTimeout(120);
        sqlServerOptions.UseQuerySplittingBehavior(QuerySplittingBehavior.SplitQuery);
    }));

// Identity servislerini ekliyoruz
builder.Services.AddIdentity<ApplicationUser, IdentityRole>(options => 
{
    // Şifre politikaları güçlendirildi
    options.Password.RequireDigit = true;  
    options.Password.RequireLowercase = true;
    options.Password.RequireUppercase = true;
    options.Password.RequireNonAlphanumeric = true;
    options.Password.RequiredLength = 8;
    
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

// Entity repository'lerini ekliyoruz
builder.Services.AddScoped<MuhasebeStokWebApp.Data.Repositories.EntityRepositories.IUrunRepository, MuhasebeStokWebApp.Data.Repositories.EntityRepositories.UrunRepository>();
builder.Services.AddScoped<MuhasebeStokWebApp.Data.Repositories.EntityRepositories.IFaturaRepository, MuhasebeStokWebApp.Data.Repositories.EntityRepositories.FaturaRepository>();
builder.Services.AddScoped<MuhasebeStokWebApp.Data.Repositories.EntityRepositories.ICariRepository, MuhasebeStokWebApp.Data.Repositories.EntityRepositories.CariRepository>();
builder.Services.AddScoped<MuhasebeStokWebApp.Data.Repositories.EntityRepositories.IIrsaliyeRepository, MuhasebeStokWebApp.Data.Repositories.EntityRepositories.IrsaliyeRepository>();

// HttpClient servisini ekliyoruz
builder.Services.AddHttpClient();

// LogService bağımlılığını ekliyoruz
builder.Services.AddScoped<ILogService, LogService>();

// ExceptionHandlingService bağımlılığını ekliyoruz
builder.Services.AddScoped<IExceptionHandlingService, ExceptionHandlingService>();

// TransactionManagerService bağımlılığını ekliyoruz
builder.Services.AddScoped<ITransactionManagerService, TransactionManagerService>();

// ParaBirimiCeviriciService bağımlılığını ekliyoruz
builder.Services.AddScoped<IParaBirimiCeviriciService, ParaBirimiCeviriciService>();

// AutoMapper'ı ekliyoruz
builder.Services.AddAutoMapper(typeof(MappingProfiles));

// ValidationService'i ekliyoruz
builder.Services.AddScoped<IValidationService, ValidationService>();

// DbMigrationService'i ekliyoruz
builder.Services.AddScoped<IDbMigrationService, DbMigrationService>();

// SistemLogService'i ekliyoruz
builder.Services.AddScoped<MuhasebeStokWebApp.Services.SistemLogService>();
builder.Services.AddHttpContextAccessor();

// SoftDeleteService'i ekliyoruz
builder.Services.AddScoped(typeof(ISoftDeleteService<>), typeof(SoftDeleteService<>));
builder.Services.AddScoped<ISoftDeleteService<Urun>, UrunSoftDeleteService>();
builder.Services.AddScoped<ISoftDeleteService<Cari>, CariSoftDeleteService>();

// Filter servislerini ekliyoruz
builder.Services.AddScoped<MuhasebeStokWebApp.Services.Filters.UrunFilterService>();

// UserManager'ı ekliyoruz
builder.Services.AddScoped<UserManager>();

// StokFifoService'i ekliyoruz
builder.Services.AddScoped<StokFifoService>();

// MaliyetHesaplamaService'i ekliyoruz
builder.Services.AddScoped<IMaliyetHesaplamaService, MaliyetHesaplamaService>();

// DropdownService'i ekliyoruz
builder.Services.AddScoped<IDropdownService, DropdownService>();

// CariService'i ekliyoruz
builder.Services.AddScoped<ICariService, CariService>();

// CariHareketService'i ekliyoruz
builder.Services.AddScoped<ICariHareketService, CariHareketService>();

// FaturaService'i ekliyoruz
builder.Services.AddScoped<IFaturaService, FaturaService>();

// FaturaValidationService'i ekliyoruz
builder.Services.AddScoped<IFaturaValidationService, FaturaValidationService>();

// FaturaOrchestrationService'i ekliyoruz
builder.Services.AddScoped<IFaturaOrchestrationService, FaturaOrchestrationService>();

// SistemAyarService'i ekliyoruz
builder.Services.AddScoped<ISistemAyarService, SistemAyarService>();

// IStokService'i ekliyoruz
builder.Services.AddScoped<IStokService, StokService>();

// IStokHareketService'i ekliyoruz
builder.Services.AddScoped<IStokHareketService, StokHareketService>();

// IIrsaliyeService'i ekliyoruz
builder.Services.AddScoped<IIrsaliyeService, IrsaliyeService>();

// AuthService'i ekliyoruz
builder.Services.AddScoped<IAuthService, AuthService>();

// MenuService'i ekliyoruz
builder.Services.AddScoped<IMenuService, MenuService>();

// BirimService'i ekliyoruz
builder.Services.AddScoped<IBirimService, BirimService>();

// Birleştirilmiş Para Birimi Modülü servislerini ekliyoruz
builder.Services.AddScoped<MuhasebeStokWebApp.Services.ParaBirimiBirlesikModul.IParaBirimiService, MuhasebeStokWebApp.Services.ParaBirimiBirlesikModul.ParaBirimiService>();

// Eski Para birimi modülü servislerini geçiş süreci için koruyoruz
builder.Services.AddScoped<MuhasebeStokWebApp.Services.ParaBirimiModulu.IParaBirimiService, MuhasebeStokWebApp.Services.ParaBirimiModulu.ParaBirimiService>();
builder.Services.AddScoped<MuhasebeStokWebApp.Services.ParaBirimiModulu.IKurDegeriService, MuhasebeStokWebApp.Services.ParaBirimiModulu.KurDegeriService>();
builder.Services.AddScoped<MuhasebeStokWebApp.Services.ParaBirimiModulu.IParaBirimiIliskiService, MuhasebeStokWebApp.Services.ParaBirimiModulu.ParaBirimiIliskiService>();

// Services.Interfaces altındaki IParaBirimiService için artık birleştirilmiş servisi bağlayalım
builder.Services.AddScoped<MuhasebeStokWebApp.Services.Interfaces.IParaBirimiService, MuhasebeStokWebApp.Services.ParaBirimiAdapter>();

// IDovizKuruService servisini DovizKuruServiceAdapter ile eşleştiriyoruz
builder.Services.AddScoped<MuhasebeStokWebApp.Services.Interfaces.IDovizKuruService, MuhasebeStokWebApp.Services.ParaBirimiModulu.DovizKuruServiceAdapter>();

// CurrencyService'i ekliyoruz
builder.Services.AddScoped<ICurrencyService, CurrencyService>();

// ReportService'i ekliyoruz
builder.Services.AddScoped<IReportService, ReportService>();

// SozlesmeService ve FaturaAklamaService'i ekliyoruz
builder.Services.AddScoped<ISozlesmeService, SozlesmeService>();
builder.Services.AddScoped<IMerkeziAklamaService, MerkeziAklamaService>();
builder.Services.AddScoped<IStokFifoService, StokFifoService>();

// Tüm entity-specific repository'ler için IUnitOfWork'ün doğru işlemesi için gerekli kayıtları ekle
builder.Services.AddScoped<MuhasebeStokWebApp.Data.Repositories.EntityRepositories.IIrsaliyeDetayRepository, MuhasebeStokWebApp.Data.Repositories.EntityRepositories.IrsaliyeDetayRepository>();
builder.Services.AddScoped<MuhasebeStokWebApp.Data.Repositories.EntityRepositories.IStokHareketRepository, MuhasebeStokWebApp.Data.Repositories.EntityRepositories.StokHareketRepository>();
builder.Services.AddScoped<MuhasebeStokWebApp.Data.Repositories.EntityRepositories.IStokFifoRepository, MuhasebeStokWebApp.Data.Repositories.EntityRepositories.StokFifoRepository>();
builder.Services.AddScoped<MuhasebeStokWebApp.Data.Repositories.EntityRepositories.IFaturaDetayRepository, MuhasebeStokWebApp.Data.Repositories.EntityRepositories.FaturaDetayRepository>();
builder.Services.AddScoped<MuhasebeStokWebApp.Data.Repositories.EntityRepositories.ICariHareketRepository, MuhasebeStokWebApp.Data.Repositories.EntityRepositories.CariHareketRepository>();
builder.Services.AddScoped<MuhasebeStokWebApp.Data.Repositories.EntityRepositories.IDepoRepository, MuhasebeStokWebApp.Data.Repositories.EntityRepositories.DepoRepository>();

// SistemLogService'i interface üzerinden kaydet
builder.Services.AddScoped<ISistemLogService, SistemLogService>();

// UserManager'ı interface üzerinden kaydet (eğer varsa)
builder.Services.AddScoped<IUserManager, UserManager>();

// EmailService ve diğer servisler için interface üzerinden kayıt
builder.Services.AddScoped<IEmailService, EmailService>();
builder.Services.AddScoped<INotificationService, NotificationService>();

// StokHareketService ve StokFifoService'in tekrar eden kayıtlarını düzelt
// Not: Tekrar eden kayıtları kaldır ve sadece interface üzerinden kaydet

builder.Services.AddSignalR();
builder.Services.Configure<EmailSettings>(builder.Configuration.GetSection("EmailSettings"));
// Arayüzlerin uygulamalarını açıkça tanımlayarak çakışmaları önlüyoruz
builder.Services.AddScoped<MuhasebeStokWebApp.Services.Email.IEmailService, MuhasebeStokWebApp.Services.Email.EmailService>();
builder.Services.AddScoped<MuhasebeStokWebApp.Services.Interfaces.IEmailService, MuhasebeStokWebApp.Services.EmailService>();

// Localization
builder.Services.AddLocalization(options => options.ResourcesPath = "Resources");
builder.Services.AddMvc()
    .AddViewLocalization()
    .AddDataAnnotationsLocalization();

// FluentValidation servisleri
builder.Services.AddFluentValidationAutoValidation();
builder.Services.AddFluentValidationClientsideAdapters();
builder.Services.AddValidatorsFromAssemblyContaining<CariHareketValidator>();

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

// Exception Stratejilerini ve Factory'yi ekle
builder.Services.AddScoped<MuhasebeStokWebApp.Exceptions.ExceptionStrategyFactory>();
builder.Services.AddScoped<MuhasebeStokWebApp.Exceptions.IExceptionStrategy, MuhasebeStokWebApp.Exceptions.Strategies.BusinessExceptionStrategy>();
builder.Services.AddScoped<MuhasebeStokWebApp.Exceptions.IExceptionStrategy, MuhasebeStokWebApp.Exceptions.Strategies.ValidationExceptionStrategy>();
builder.Services.AddScoped<MuhasebeStokWebApp.Exceptions.IExceptionStrategy, MuhasebeStokWebApp.Exceptions.Strategies.DataExceptionStrategy>();
builder.Services.AddScoped<MuhasebeStokWebApp.Exceptions.IExceptionStrategy, MuhasebeStokWebApp.Exceptions.Strategies.DbExceptionStrategy>();
builder.Services.AddScoped<MuhasebeStokWebApp.Exceptions.IExceptionStrategy, MuhasebeStokWebApp.Exceptions.Strategies.DefaultExceptionStrategy>();

// ParaBirimiDonusumHelper'ı DI container'a ekle
builder.Services.AddScoped<MuhasebeStokWebApp.Services.Interfaces.IParaBirimiDonusumHelper, MuhasebeStokWebApp.Services.Helpers.ParaBirimiDonusumHelper>();

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

// Veritabanını otomatik oluşturma ve migrationları uygulama
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    try
    {
        var context = services.GetRequiredService<ApplicationDbContext>();
        
        // Veritabanı varsa migrationları uygula, yoksa oluştur
        if (context.Database.CanConnect())
        {
            Log.Information("Veritabanına bağlantı başarılı. Migrationlar uygulanıyor...");
            // Migrationları uygula ama tablolar varsa oluşturmaya çalışma
            context.Database.EnsureCreated();
        }
        else
        {
            Log.Warning("Veritabanına bağlantı sağlanamadı. Veritabanı oluşturuluyor...");
            context.Database.EnsureCreated();
        }
        
        // Gerekli başlangıç verilerini ekle
        await AppDbInitializer.SeedData(services, context);
        
        Log.Information("Veritabanı işlemleri başarıyla tamamlandı.");
    }
    catch (Exception ex)
    {
        Log.Error(ex, "Veritabanı oluşturulurken veya migrationlar uygulanırken bir hata oluştu.");
    }
}

// Özel hata sayfaları için durum kodu sayfalarını ayarlıyoruz
app.UseStatusCodePagesWithReExecute("/Home/StatusCode", "?code={0}");

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

// CORS middleware'ini ekle - UseRouting'den sonra, UseAuthorization'dan önce olmalı
app.UseCors(x => x
    .WithOrigins("https://localhost:5001", "https://yourdomain.com") // Sadece bu domainlere izin ver
    .AllowAnyMethod()
    .AllowAnyHeader()
    .AllowCredentials());

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
