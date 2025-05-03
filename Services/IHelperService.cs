using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace MuhasebeStokWebApp.Services.Interfaces
{
    /// <summary>
    /// Genel yardımcı metotlar için arayüz
    /// </summary>
    public interface IHelperService
    {
        /// <summary>
        /// String değeri decimal'e çevirir. Hata durumunda varsayılan değeri döndürür.
        /// </summary>
        /// <param name="value">Dönüştürülecek string değer</param>
        /// <param name="defaultValue">Dönüştürme başarısız olursa kullanılacak varsayılan değer</param>
        /// <returns>Decimal değer</returns>
        decimal ParseDecimal(string value, decimal defaultValue = 0);

        /// <summary>
        /// Para birimini dönüştürür
        /// </summary>
        /// <param name="amount">Miktar</param>
        /// <param name="fromCurrency">Kaynak para birimi</param>
        /// <param name="toCurrency">Hedef para birimi</param>
        /// <param name="date">Dönüşüm tarihi (null ise güncel kur kullanılır)</param>
        /// <returns>Dönüştürülmüş miktar</returns>
        Task<decimal> ConvertCurrencyAsync(decimal amount, string fromCurrency, string toCurrency, DateTime? date = null);

        /// <summary>
        /// Verilen enum tipinden SelectList oluşturur
        /// </summary>
        /// <typeparam name="TEnum">Enum tipi</typeparam>
        /// <param name="selectedValue">Seçili değer</param>
        /// <returns>SelectList</returns>
        SelectList GetEnumSelectList<TEnum>(TEnum? selectedValue = null) where TEnum : struct, Enum;

        /// <summary>
        /// GUID değerini oluşturur veya parse eder
        /// </summary>
        /// <param name="value">GUID string değeri</param>
        /// <param name="emptyIfInvalid">Geçersiz ise boş GUID döndürülsün mü?</param>
        /// <returns>GUID değeri, geçersiz ve emptyIfInvalid=false ise null</returns>
        Guid? ParseGuid(string value, bool emptyIfInvalid = true);

        /// <summary>
        /// Tarih aralığını kontrol eder ve geçerli değerler döndürür
        /// </summary>
        /// <param name="startDate">Başlangıç tarihi</param>
        /// <param name="endDate">Bitiş tarihi</param>
        /// <param name="defaultDays">Tarih belirtilmemişse kullanılacak varsayılan gün sayısı</param>
        /// <returns>Düzeltilmiş başlangıç ve bitiş tarihleri</returns>
        (DateTime startDate, DateTime endDate) ValidateDateRange(DateTime? startDate, DateTime? endDate, int defaultDays = 30);

        /// <summary>
        /// Bir dizinin öğesini güvenli şekilde döndürür. Index sınırların dışındaysa varsayılan değeri döndürür.
        /// </summary>
        /// <typeparam name="T">Dizi elemanı tipi</typeparam>
        /// <param name="array">Dizi</param>
        /// <param name="index">Index</param>
        /// <param name="defaultValue">Varsayılan değer</param>
        /// <returns>Dizi elemanı veya varsayılan değer</returns>
        T GetSafeArrayValue<T>(T[] array, int index, T defaultValue);

        /// <summary>
        /// Verilen string'i maksimum uzunluğa göre keser ve gerekirse sonuna belirtilen son eki ekler
        /// </summary>
        /// <param name="text">Kesilecek metin</param>
        /// <param name="maxLength">Maksimum uzunluk</param>
        /// <param name="suffix">Son ek (varsayılan: "...")</param>
        /// <returns>Kesilmiş metin</returns>
        string TruncateString(string text, int maxLength, string suffix = "...");

        /// <summary>
        /// İki tarih arasındaki farkı formatlar (1 gün, 2 saat, 5 dakika önce gibi)
        /// </summary>
        /// <param name="date">Tarih</param>
        /// <param name="referenceDate">Referans tarih (null ise şu an)</param>
        /// <returns>Formatlanmış tarih farkı</returns>
        string FormatTimeAgo(DateTime date, DateTime? referenceDate = null);

        /// <summary>
        /// Müşteri/tedarikçi gibi farklı türdeki cariler için uygun filtre parametrelerini oluşturur
        /// </summary>
        /// <param name="cariTuru">Cari türü (Müşteri, Tedarikçi, Tümü)</param>
        /// <returns>Filtreleme için kullanılacak parametreler</returns>
        Dictionary<string, object> GetCariFilterParams(string cariTuru);
    }
} 