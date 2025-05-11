using System;
using System.IO;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;

namespace MuhasebeStokWebApp.Data
{
    public class ApplicationDbContextFactory : IDesignTimeDbContextFactory<ApplicationDbContext>
    {
        public ApplicationDbContext CreateDbContext(string[] args)
        {
            // Environment değişkenini kontrol et
            var environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT");
            
            // Ortama göre doğru yapılandırma dosyasını yükle
            var builder = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true);

            // Development ortamı için appsettings.Development.json dosyasını da yükle
            if (environment == "Development")
            {
                builder.AddJsonFile($"appsettings.{environment}.json", optional: true, reloadOnChange: true);
            }

            IConfigurationRoot configuration = builder.Build();

            var dbContextBuilder = new DbContextOptionsBuilder<ApplicationDbContext>();
            var connectionString = configuration.GetConnectionString("DefaultConnection");

            Console.WriteLine($"Using connection string: {connectionString}");

            dbContextBuilder.UseSqlServer(connectionString, options => options.CommandTimeout(120));

            return new ApplicationDbContext(dbContextBuilder.Options);
        }
    }
} 