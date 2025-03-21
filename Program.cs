using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using MuhasebeStokWebApp.Data;
using MuhasebeStokWebApp.Data.Repositories;
using Microsoft.AspNetCore.Authentication.Cookies;
using MuhasebeStokWebApp.Services;
using MuhasebeStokWebApp.Services.Menu;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");

// SQL Server kullanımı (tüm ortamlarda)
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(connectionString));

// Identity servislerini ekliyoruz
builder.Services.AddIdentity<IdentityUser, IdentityRole>(options => 
{
    // Şifre politikaları
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
    .AddDefaultTokenProviders()
    .AddSignInManager<SignInManager<IdentityUser>>();

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

// Cookie Authentication
builder.Services.AddAuthentication(options => 
{
    options.DefaultAuthenticateScheme = CookieAuthenticationDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = CookieAuthenticationDefaults.AuthenticationScheme;
    options.DefaultSignInScheme = CookieAuthenticationDefaults.AuthenticationScheme;
})
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

// LogService bağımlılığını ekliyoruz
builder.Services.AddScoped<ILogService, LogService>();

// SistemLogService'i ekliyoruz
builder.Services.AddScoped<MuhasebeStokWebApp.Services.SistemLogService>();
builder.Services.AddHttpContextAccessor();

// UserManager'ı ekliyoruz
builder.Services.AddScoped<UserManager>();

// StokFifoService'i ekliyoruz
builder.Services.AddScoped<StokFifoService>();

// Para birimi ve döviz kuru servislerini ekliyoruz
builder.Services.AddScoped<IParaBirimiService, ParaBirimiService>();
builder.Services.AddScoped<IDovizKuruService, DovizKuruService>();

// DropdownService'i ekliyoruz
builder.Services.AddScoped<IDropdownService, DropdownService>();

// MenuService'i ekliyoruz
builder.Services.AddScoped<IMenuService, MenuService>();

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

// Veritabanı başlangıç verilerini ekle
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    try
    {
        var context = services.GetRequiredService<ApplicationDbContext>();
        await AppDbInitializer.SeedData(services, context);
    }
    catch (Exception ex)
    {
        var logger = services.GetRequiredService<ILogger<Program>>();
        logger.LogError(ex, "Veritabanı başlatma hatası.");
    }
}

// Özel hata sayfaları için durum kodu sayfalarını ayarlıyoruz
app.UseStatusCodePagesWithReExecute("/Home/StatusCode", "?code={0}");

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

// Yetkilendirmeyi etkinleştiriyoruz
app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
