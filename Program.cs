using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using MuhasebeStokWebApp.Data;
using MuhasebeStokWebApp.Data.Repositories;
using Microsoft.AspNetCore.Authentication.Cookies;
using MuhasebeStokWebApp.Services;
using System.Globalization;
using Microsoft.Extensions.Options;

var builder = WebApplication.CreateBuilder(args);

// Tarih formatı için kültür ayarları
var cultureInfo = new CultureInfo("tr-TR");
cultureInfo.DateTimeFormat.ShortDatePattern = "dd.MM.yyyy";
cultureInfo.DateTimeFormat.DateSeparator = ".";
CultureInfo.DefaultThreadCurrentCulture = cultureInfo;
CultureInfo.DefaultThreadCurrentUICulture = cultureInfo;

// Add services to the container.
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(connectionString));

// Identity servislerini ekliyoruz
builder.Services.AddIdentity<IdentityUser, IdentityRole>()
    .AddEntityFrameworkStores<ApplicationDbContext>()
    .AddDefaultTokenProviders();

// Cookie Authentication ayarları
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.Cookie.Name = "MuhasebeStokAuth";
        options.LoginPath = "/Account/Login";
        options.AccessDeniedPath = "/Account/AccessDenied";
        options.ExpireTimeSpan = TimeSpan.FromDays(30);
        options.SlidingExpiration = true;
        options.Cookie.HttpOnly = true;
        options.Cookie.IsEssential = true;
        options.Cookie.SameSite = Microsoft.AspNetCore.Http.SameSiteMode.Lax;
    });

// Repository ve UnitOfWork servislerini ekliyoruz
builder.Services.AddScoped(typeof(IRepository<>), typeof(Repository<>));
builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();

// HttpClient servisini ekliyoruz
builder.Services.AddHttpClient();

// SistemLogService'i ekliyoruz
builder.Services.AddScoped<SistemLogService>();
builder.Services.AddScoped<ILogService, SistemLogService>();

// DovizService'i ekliyoruz
builder.Services.AddScoped<IDovizService, DovizService>();

// KurService'i ekliyoruz
builder.Services.AddScoped<IKurService, KurService>();

// HttpContextAccessor ekliyoruz
builder.Services.AddHttpContextAccessor();

// StokFifoService'i ekliyoruz
builder.Services.AddScoped<StokFifoService>();

// Tarih formatı işlemleri için MVC servisi ekleme
builder.Services.AddMvc()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.PropertyNamingPolicy = null;
    })
    .AddViewOptions(options =>
    {
        options.HtmlHelperOptions.Html5DateRenderingMode = Microsoft.AspNetCore.Mvc.Rendering.Html5DateRenderingMode.Rfc3339;
    });

builder.Services.AddControllersWithViews();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

// Özel hata sayfaları için durum kodu sayfalarını ayarlıyoruz
app.UseStatusCodePagesWithReExecute("/Home/StatusCode", "?code={0}");

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

// Yetkilendirmeyi etkinleştiriyoruz
app.UseAuthentication();
app.UseAuthorization();

// RequestLocalization middleware ekle
var locOptions = app.Services.GetService<IOptions<RequestLocalizationOptions>>();
if (locOptions != null)
{
    app.UseRequestLocalization(locOptions.Value);
}

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
