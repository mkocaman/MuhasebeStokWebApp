using System;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace MuhasebeStokWebApp.Extensions
{
    /// <summary>
    /// String extension metotları
    /// </summary>
    public static class StringExtensions
    {
        /// <summary>
        /// String ifadesinin null veya boş olup olmadığını kontrol eder
        /// </summary>
        /// <param name="text">Kontrol edilecek string</param>
        /// <returns>String null veya boş ise true, değilse false</returns>
        public static bool IsNullOrEmpty(this string text)
        {
            return string.IsNullOrEmpty(text);
        }
        
        /// <summary>
        /// String ifadesinin null, boş veya sadece boşluk karakterlerinden oluşup oluşmadığını kontrol eder
        /// </summary>
        /// <param name="text">Kontrol edilecek string</param>
        /// <returns>String null, boş veya sadece boşluk karakterlerinden oluşuyorsa true, değilse false</returns>
        public static bool IsNullOrWhiteSpace(this string text)
        {
            return string.IsNullOrWhiteSpace(text);
        }
        
        /// <summary>
        /// String ifadesinden HTML etiketlerini temizler
        /// </summary>
        /// <param name="html">HTML içeren string</param>
        /// <returns>HTML etiketleri temizlenmiş string</returns>
        public static string StripHtml(this string html)
        {
            if (string.IsNullOrWhiteSpace(html))
                return string.Empty;
                
            var result = Regex.Replace(html, @"<[^>]*>", string.Empty);
            result = Regex.Replace(result, @"&nbsp;", " ");
            return result.Trim();
        }
        
        /// <summary>
        /// String ifadesinin belirli bir uzunluktan sonrasını keser
        /// </summary>
        /// <param name="text">Kesilecek string</param>
        /// <param name="maxLength">Maksimum uzunluk</param>
        /// <param name="suffix">Kesilmiş stringin sonuna eklenecek ek</param>
        /// <returns>Kesilmiş string</returns>
        public static string Truncate(this string text, int maxLength, string suffix = "...")
        {
            if (string.IsNullOrEmpty(text) || text.Length <= maxLength)
                return text;
                
            return text.Substring(0, maxLength) + suffix;
        }
        
        /// <summary>
        /// String ifadesini Decimal'e çevirir. Başarısız olursa varsayılan değeri döndürür.
        /// </summary>
        /// <param name="text">Decimal'e çevrilecek string</param>
        /// <param name="defaultValue">Çevirme başarısız olursa döndürülecek değer</param>
        /// <returns>Çevrilen decimal değer veya varsayılan değer</returns>
        public static decimal ToDecimal(this string text, decimal defaultValue = 0)
        {
            if (string.IsNullOrWhiteSpace(text))
                return defaultValue;
                
            // Hem nokta hem de virgülle yazılmış olabilir
            text = text.Replace(",", CultureInfo.InvariantCulture.NumberFormat.NumberDecimalSeparator);
            
            if (decimal.TryParse(text, NumberStyles.Any, CultureInfo.InvariantCulture, out decimal result))
                return result;
                
            return defaultValue;
        }
        
        /// <summary>
        /// String ifadesini Int32'ye çevirir. Başarısız olursa varsayılan değeri döndürür.
        /// </summary>
        /// <param name="text">Int32'ye çevrilecek string</param>
        /// <param name="defaultValue">Çevirme başarısız olursa döndürülecek değer</param>
        /// <returns>Çevrilen int değer veya varsayılan değer</returns>
        public static int ToInt(this string text, int defaultValue = 0)
        {
            if (string.IsNullOrWhiteSpace(text))
                return defaultValue;
                
            if (int.TryParse(text, out int result))
                return result;
                
            return defaultValue;
        }
        
        /// <summary>
        /// String ifadesini Guid'e çevirir. Başarısız olursa Empty Guid veya null döndürür.
        /// </summary>
        /// <param name="text">Guid'e çevrilecek string</param>
        /// <param name="emptyIfInvalid">Geçersiz ise boş Guid döndürülsün mü?</param>
        /// <returns>Çevrilen Guid değer, geçersiz ise boş Guid veya null</returns>
        public static Guid? ToGuid(this string text, bool emptyIfInvalid = true)
        {
            if (string.IsNullOrWhiteSpace(text))
                return emptyIfInvalid ? Guid.Empty : null;
                
            if (Guid.TryParse(text, out Guid result))
                return result;
                
            return emptyIfInvalid ? Guid.Empty : null;
        }
        
        /// <summary>
        /// String ifadesini camelCase formatına çevirir
        /// </summary>
        /// <param name="text">camelCase formatına çevrilecek string</param>
        /// <returns>camelCase formatında string</returns>
        public static string ToCamelCase(this string text)
        {
            if (string.IsNullOrEmpty(text))
                return text;
                
            if (text.Length == 1)
                return text.ToLowerInvariant();
                
            return char.ToLowerInvariant(text[0]) + text.Substring(1);
        }
        
        /// <summary>
        /// String ifadesini slug formatına çevirir (URL-friendly)
        /// </summary>
        /// <param name="text">Slug formatına çevrilecek string</param>
        /// <returns>Slug formatında string</returns>
        public static string ToSlug(this string text)
        {
            if (string.IsNullOrWhiteSpace(text))
                return string.Empty;
                
            // Türkçe karakterleri İngilizce karakterlere çevir
            text = text.Replace("ı", "i")
                .Replace("ğ", "g")
                .Replace("ü", "u")
                .Replace("ş", "s")
                .Replace("ö", "o")
                .Replace("ç", "c")
                .Replace("İ", "I")
                .Replace("Ğ", "G")
                .Replace("Ü", "U")
                .Replace("Ş", "S")
                .Replace("Ö", "O")
                .Replace("Ç", "C");
                
            // Alfanumerik olmayan karakterleri tire ile değiştir
            text = Regex.Replace(text, @"[^a-zA-Z0-9\s-]", "");
            // Birden fazla boşluğu tek boşluğa indir
            text = Regex.Replace(text, @"\s+", " ").Trim();
            // Boşlukları tire ile değiştir
            text = Regex.Replace(text, @"\s", "-");
            // Birden fazla tireyi tek tireye indir
            text = Regex.Replace(text, @"-+", "-");
            
            return text.ToLowerInvariant();
        }
        
        /// <summary>
        /// String ifadesinin ilk harfini büyük, diğer harfleri küçük yapar
        /// </summary>
        /// <param name="text">Düzenlenecek string</param>
        /// <returns>İlk harfi büyük, diğer harfleri küçük string</returns>
        public static string ToTitleCase(this string text)
        {
            if (string.IsNullOrEmpty(text))
                return text;
                
            return CultureInfo.CurrentCulture.TextInfo.ToTitleCase(text.ToLower());
        }
        
        /// <summary>
        /// String ifadesinin belirli bir karakterle başlayıp başlamadığını kontrol eder
        /// </summary>
        /// <param name="text">Kontrol edilecek string</param>
        /// <param name="value">Aranacak karakterler</param>
        /// <param name="ignoreCase">Büyük/küçük harf duyarlılığını yok saymak için true, aksi halde false</param>
        /// <returns>String belirtilen karakterle başlıyorsa true, aksi halde false</returns>
        public static bool StartsWith(this string text, string value, bool ignoreCase = false)
        {
            if (text == null)
                return false;
                
            if (value == null)
                return false;
                
            if (ignoreCase)
                return text.StartsWith(value, StringComparison.OrdinalIgnoreCase);
                
            return text.StartsWith(value, StringComparison.Ordinal);
        }
        
        /// <summary>
        /// String ifadesinin belirli bir karakterle bitip bitmediğini kontrol eder
        /// </summary>
        /// <param name="text">Kontrol edilecek string</param>
        /// <param name="value">Aranacak karakterler</param>
        /// <param name="ignoreCase">Büyük/küçük harf duyarlılığını yok saymak için true, aksi halde false</param>
        /// <returns>String belirtilen karakterle bitiyorsa true, aksi halde false</returns>
        public static bool EndsWith(this string text, string value, bool ignoreCase = false)
        {
            if (text == null)
                return false;
                
            if (value == null)
                return false;
                
            if (ignoreCase)
                return text.EndsWith(value, StringComparison.OrdinalIgnoreCase);
                
            return text.EndsWith(value, StringComparison.Ordinal);
        }
    }
} 