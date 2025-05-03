using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Extensions.Logging;
using MuhasebeStokWebApp.Services.Interfaces;

namespace MuhasebeStokWebApp.Services
{
    /// <summary>
    /// Genel yardımcı metotlar için servis sınıfı
    /// </summary>
    public class HelperService : IHelperService
    {
        private readonly ILogger<HelperService> _logger;
        private readonly IDovizKuruService _dovizKuruService;

        public HelperService(
            ILogger<HelperService> logger,
            IDovizKuruService dovizKuruService)
        {
            _logger = logger;
            _dovizKuruService = dovizKuruService;
        }

        /// <inheritdoc/>
        public decimal ParseDecimal(string value, decimal defaultValue = 0)
        {
            if (string.IsNullOrWhiteSpace(value))
                return defaultValue;

            // Kültür bağımsız çalışmak için InvariantCulture kullan
            // Hem nokta hem de virgülle yazılmış olabilir
            value = value.Replace(",", CultureInfo.InvariantCulture.NumberFormat.NumberDecimalSeparator);
            
            if (decimal.TryParse(value, NumberStyles.Any, CultureInfo.InvariantCulture, out decimal result))
                return result;

            return defaultValue;
        }

        /// <inheritdoc/>
        public async Task<decimal> ConvertCurrencyAsync(decimal amount, string fromCurrency, string toCurrency, DateTime? date = null)
        {
            if (string.IsNullOrWhiteSpace(fromCurrency) || string.IsNullOrWhiteSpace(toCurrency))
                return amount;

            // Aynı para birimi için dönüşüm yapma
            if (fromCurrency.Equals(toCurrency, StringComparison.OrdinalIgnoreCase))
                return amount;

            try
            {
                // Tarihi normalizasyon
                date ??= DateTime.Today;
                
                // Farklı para birimi dönüşümleri için özel kurallar
                if (fromCurrency == "UZS" && toCurrency == "USD")
                {
                    // UZS -> USD çevirimi için bölme işlemi kullan
                    decimal kurUZSToUSD = await _dovizKuruService.GetGuncelKurAsync("UZS", "USD", date.Value);
                    return kurUZSToUSD > 0 ? amount / kurUZSToUSD : 0;
                }
                else if (fromCurrency == "USD" && toCurrency == "UZS")
                {
                    // USD -> UZS çevirimi için çarpma işlemi kullan
                    decimal kurUSDToUZS = await _dovizKuruService.GetGuncelKurAsync("USD", "UZS", date.Value);
                    return kurUSDToUZS > 0 ? amount * kurUSDToUZS : 0;
                }
                else
                {
                    // Genel dönüşüm
                    decimal kur = await _dovizKuruService.GetGuncelKurAsync(fromCurrency, toCurrency, date.Value);
                    return kur > 0 ? amount * kur : 0;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Para birimi dönüşümü sırasında hata oluştu: {FromCurrency} -> {ToCurrency}, Miktar: {Amount}", 
                    fromCurrency, toCurrency, amount);
                return 0;
            }
        }

        /// <inheritdoc/>
        public SelectList GetEnumSelectList<TEnum>(TEnum? selectedValue = null) where TEnum : struct, Enum
        {
            var values = Enum.GetValues(typeof(TEnum))
                .Cast<TEnum>()
                .Select(e => new SelectListItem
                {
                    Text = e.ToString(),
                    Value = Convert.ToInt32(e).ToString(),
                    Selected = selectedValue.HasValue && EqualityComparer<TEnum>.Default.Equals(e, selectedValue.Value)
                })
                .ToList();

            return new SelectList(values, "Value", "Text");
        }

        /// <inheritdoc/>
        public Guid? ParseGuid(string value, bool emptyIfInvalid = true)
        {
            if (string.IsNullOrWhiteSpace(value))
                return emptyIfInvalid ? Guid.Empty : null;

            if (Guid.TryParse(value, out Guid result))
                return result;

            return emptyIfInvalid ? Guid.Empty : null;
        }

        /// <inheritdoc/>
        public (DateTime startDate, DateTime endDate) ValidateDateRange(DateTime? startDate, DateTime? endDate, int defaultDays = 30)
        {
            DateTime today = DateTime.Today;
            
            // Başlangıç tarihi belirleme
            DateTime validStartDate = startDate ?? today.AddDays(-defaultDays);
            
            // Bitiş tarihi belirleme
            DateTime validEndDate = endDate ?? today;
            
            // Başlangıç tarihi bitiş tarihinden sonra ise, başlangıç tarihini bitiş tarihinden 1 gün öncesine al
            if (validStartDate > validEndDate)
                validStartDate = validEndDate.AddDays(-1);
            
            // Tarih aralığı çok uzunsa (örneğin 1 yıldan fazla), makul bir aralığa indir
            if ((validEndDate - validStartDate).TotalDays > 365)
                validStartDate = validEndDate.AddDays(-365);
            
            return (validStartDate, validEndDate);
        }

        /// <inheritdoc/>
        public T GetSafeArrayValue<T>(T[] array, int index, T defaultValue)
        {
            if (array == null || index < 0 || index >= array.Length)
                return defaultValue;
            
            return array[index];
        }

        /// <inheritdoc/>
        public string TruncateString(string text, int maxLength, string suffix = "...")
        {
            if (string.IsNullOrEmpty(text) || text.Length <= maxLength)
                return text;
            
            // Suffix'in uzunluğunu hesaba kat
            int truncateAt = maxLength - suffix.Length;
            if (truncateAt <= 0)
                return suffix;
            
            return text.Substring(0, truncateAt) + suffix;
        }

        /// <inheritdoc/>
        public string FormatTimeAgo(DateTime date, DateTime? referenceDate = null)
        {
            DateTime reference = referenceDate ?? DateTime.Now;
            TimeSpan diff = reference - date;

            if (diff.TotalSeconds < 60)
                return "Az önce";
            
            if (diff.TotalMinutes < 60)
                return $"{Math.Floor(diff.TotalMinutes)} dakika önce";
            
            if (diff.TotalHours < 24)
                return $"{Math.Floor(diff.TotalHours)} saat önce";
            
            if (diff.TotalDays < 30)
                return $"{Math.Floor(diff.TotalDays)} gün önce";
            
            if (diff.TotalDays < 365)
                return $"{Math.Floor(diff.TotalDays / 30)} ay önce";
            
            return $"{Math.Floor(diff.TotalDays / 365)} yıl önce";
        }

        /// <inheritdoc/>
        public Dictionary<string, object> GetCariFilterParams(string cariTuru)
        {
            var parameters = new Dictionary<string, object>();
            
            switch (cariTuru?.ToLowerInvariant())
            {
                case "musteri":
                case "müşteri":
                    parameters.Add("IsMüşteri", true);
                    break;
                case "tedarikci":
                case "tedarikçi":
                    parameters.Add("IsTedarikçi", true);
                    break;
            }
            
            return parameters;
        }
    }
} 