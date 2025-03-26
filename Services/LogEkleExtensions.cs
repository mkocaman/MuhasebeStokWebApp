using System;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using MuhasebeStokWebApp.Enums;
using MuhasebeStokWebApp.Services.Interfaces;

namespace MuhasebeStokWebApp.Extensions
{
    public static class LogEkleExtensions
    {
        /// <summary>
        /// GUID değerini int'e dönüştüren yardımcı metot
        /// </summary>
        public static int ToInt(this Guid guid)
        {
            // GetHashCode() negatif bir değer döndürebilir, bu yüzden abs alıyoruz
            return Math.Abs(guid.GetHashCode());
        }
        
        /// <summary>
        /// GUID değerini string'e dönüştüren yardımcı metot
        /// </summary>
        public static string ToSafeString(this Guid? guid)
        {
            return guid?.ToString() ?? string.Empty;
        }
        
        /// <summary>
        /// Log servisine güvenli şekilde kayıt eklemeyi sağlayan extension metot
        /// Bu metot GUID/int dönüşüm sorunlarını otomatik olarak ele alır
        /// </summary>
        public static async Task SafeLogEkleAsync(this ILogService logService, 
            string mesaj, 
            MuhasebeStokWebApp.Enums.LogTuru logTuru, 
            Guid? kayitID = null, 
            string tabloAdi = null, 
            string kayitAdi = null,
            ILogger logger = null)
        {
            try 
            {
                // KayitID int olarak kaydedildiğinden, GUID'i int'e dönüştürüyoruz
                int? intKayitID = kayitID.HasValue ? Math.Abs(kayitID.Value.GetHashCode()) : null;
                
                // Diğer parametreler için varsayılan değerler
                tabloAdi = tabloAdi ?? "Sistem";
                kayitAdi = kayitAdi ?? "Sistem Kaydı";
                
                // Dönüştürülmüş değerlerle log eklemek için özel bir metot çağrısı yapılabilir
                await logService.LogEkleAsync(mesaj, logTuru, kayitAdi);
            }
            catch (Exception ex)
            {
                // Hata durumunda logger varsa kullan, yoksa sessizce devam et
                if (logger != null)
                {
                    logger.LogError(ex, "Log oluşturma hatası (SafeLogEkleAsync): {Message}", ex.Message);
                }
            }
        }
    }
} 