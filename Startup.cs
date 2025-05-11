using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using MuhasebeStokWebApp.Data;
using MuhasebeStokWebApp.Data.Entities;
using MuhasebeStokWebApp.Data.Repositories;
using MuhasebeStokWebApp.Services;
using MuhasebeStokWebApp.Services.Interfaces;
using MuhasebeStokWebApp.Services.Menu;
// ... more usings

namespace MuhasebeStokWebApp
{
    public class Startup
    {
        private readonly IConfiguration _configuration;

        public Startup(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            // ... existing service registrations ...

            // Data repositories
            services.AddScoped<IUnitOfWork, UnitOfWork>();
            services.AddScoped(typeof(IRepository<>), typeof(Repository<>));
            services.AddScoped<IIrsaliyeRepository, IrsaliyeRepository>();
            services.AddScoped<IIrsaliyeDetayRepository, IrsaliyeDetayRepository>();

            // Services
            services.AddScoped<ILogService, LogService>();
            services.AddScoped<IValidationService, ValidationService>();
            services.AddScoped<IMenuService, MenuService>();
            services.AddScoped<ISistemAyarService, SistemAyarService>();
            services.AddScoped<ICariService, CariService>();
            services.AddScoped<IEmailService, EmailService>();
            services.AddScoped<IDbMigrationService, DbMigrationService>();
            services.AddScoped<IMerkeziAklamaService, MerkeziAklamaService>();
            services.AddScoped<ISozlesmeService, SozlesmeService>();
            services.AddScoped<IBirimService, BirimService>();
            services.AddScoped<IStokFifoService, StokFifoService>();
            services.AddScoped<IStokCikisService, StokFifoService>();
            services.AddScoped<IStokGirisService, StokFifoService>();
            services.AddScoped<IStokSorguService, StokFifoService>();
            services.AddScoped<IStokHareketService, StokHareketService>();
            services.AddScoped<IStokConcurrencyService, StokFifoService>();
            services.AddScoped<IMaliyetHesaplamaService, MaliyetHesaplamaService>();
            services.AddScoped<ICariHareketService, CariHareketService>();

            // Custom services
            services.AddScoped<IFaturaService, FaturaService>();
            services.AddScoped<IFaturaOrchestrationService, FaturaOrchestrationService>();
            services.AddScoped<IFaturaValidationService, FaturaValidationService>();
            services.AddScoped<IIrsaliyeService, IrsaliyeService>();

            // Numara üretme servisleri
            services.AddScoped<IFaturaNumaralandirmaService, FaturaNumaralandirmaService>();
            services.AddScoped<IIrsaliyeNumaralandirmaService, IrsaliyeNumaralandirmaService>();
            services.AddScoped<ICariNumaralandirmaService, CariNumaralandirmaService>();
            
            // Yapılacaklar listesi servisi
            services.AddScoped<ITodoService, TodoService>();

            // Dashboard servisi ekleniyor
            services.AddScoped<IDashboardService, DashboardService>();

            // EF Core ile veritabanı bağlantısı yapılandırması
            services.AddDbContext<ApplicationDbContext>(options =>
                options.UseLazyLoadingProxies() // Lazy loading proxies özelliğini ekle
                    .UseSqlServer(
                        _configuration.GetConnectionString("DefaultConnection"),
                        sqlOptions => sqlOptions.EnableRetryOnFailure(
                            maxRetryCount: 5,
                            maxRetryDelay: TimeSpan.FromSeconds(30),
                            errorNumbersToAdd: null)));
        }

        // ... rest of the code ...
    }
} 