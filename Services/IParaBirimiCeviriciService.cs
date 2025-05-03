using System;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace MuhasebeStokWebApp.Services
{
    /// <summary>
    /// Para birimi dönüşüm işlemlerini merkezi olarak yöneten servis arayüzü
    /// </summary>
    public interface IParaBirimiCeviriciService
    {
        /// <summary>
        /// Tutarı bir para biriminden başka bir para birimine çevirir
        /// </summary>
        /// <param name="tutar">Çevrilecek tutar</param>
        /// <param name="kaynakKod">Kaynak para birimi kodu</param>
        /// <param name="hedefKod">Hedef para birimi kodu</param>
        /// <param name="tarih">İşlem tarihi (null ise bugün)</param>
        /// <returns>Çevrilmiş tutar</returns>
        Task<decimal> TutarCevirAsync(decimal tutar, string kaynakKod, string hedefKod, DateTime? tarih = null);

        /// <summary>
        /// Tutarı bir para biriminden başka bir para birimine çevirir (ID'ler ile)
        /// </summary>
        /// <param name="tutar">Çevrilecek tutar</param>
        /// <param name="kaynakParaBirimiId">Kaynak para birimi ID</param>
        /// <param name="hedefParaBirimiId">Hedef para birimi ID</param>
        /// <param name="tarih">İşlem tarihi (null ise bugün)</param>
        /// <returns>Çevrilmiş tutar</returns>
        Task<decimal> TutarCevirAsync(decimal tutar, Guid kaynakParaBirimiId, Guid hedefParaBirimiId, DateTime? tarih = null);

        /// <summary>
        /// Bir entity'nin belirli bir özelliğindeki para birimini çevirir.
        /// </summary>
        /// <typeparam name="T">Entity tipi</typeparam>
        /// <param name="entity">Çevrilecek entity</param>
        /// <param name="propertySelector">Çevrilecek özelliği seçen fonksiyon (entity => entity.Tutar)</param>
        /// <param name="propertySetter">Çevrilen değeri ayarlayan fonksiyon (entity, deger) => entity.Tutar = deger</param>
        /// <param name="kaynakKod">Kaynak para birimi kodu</param>
        /// <param name="hedefKod">Hedef para birimi kodu</param>
        /// <param name="tarih">İşlem tarihi (null ise bugün)</param>
        /// <returns>Özellik değeri çevrilmiş entity</returns>
        Task<T> EntityOzelligiCevirAsync<T>(
            T entity,
            Func<T, decimal> propertySelector,
            Action<T, decimal> propertySetter,
            string kaynakKod,
            string hedefKod,
            DateTime? tarih = null);

        /// <summary>
        /// Bir koleksiyondaki tüm entitylerin belirli bir özelliğini para birimi dönüşümü yapar
        /// </summary>
        /// <typeparam name="T">Entity tipi</typeparam>
        /// <param name="entities">Entity koleksiyonu</param>
        /// <param name="propertySelector">Çevrilecek özelliği seçen fonksiyon</param>
        /// <param name="propertySetter">Çevrilen değeri ayarlayan fonksiyon</param>
        /// <param name="kaynakKod">Kaynak para birimi kodu</param>
        /// <param name="hedefKod">Hedef para birimi kodu</param>
        /// <param name="tarih">İşlem tarihi</param>
        /// <returns>Özellik değerleri çevrilmiş entity koleksiyonu</returns>
        Task<IEnumerable<T>> EntityKoleksiyonuCevirAsync<T>(
            IEnumerable<T> entities,
            Func<T, decimal> propertySelector,
            Action<T, decimal> propertySetter,
            string kaynakKod,
            string hedefKod,
            DateTime? tarih = null);
    }
} 