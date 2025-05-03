using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using MuhasebeStokWebApp.Data;
using MuhasebeStokWebApp.Data.Entities;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using MuhasebeStokWebApp.Services.Interfaces;

namespace MuhasebeStokWebApp.Services
{
    /// <summary>
    /// Para birimi dönüşüm işlemlerini merkezi olarak yöneten servis
    /// </summary>
    public class ParaBirimiCeviriciService : IParaBirimiCeviriciService
    {
        private readonly ILogger<ParaBirimiCeviriciService> _logger;
        private readonly ApplicationDbContext _context;
        private readonly IParaBirimiService _paraBirimiService;
        private readonly IExceptionHandlingService _exceptionHandler;

        public ParaBirimiCeviriciService(
            ILogger<ParaBirimiCeviriciService> logger,
            ApplicationDbContext context,
            IParaBirimiService paraBirimiService,
            IExceptionHandlingService exceptionHandler)
        {
            _logger = logger;
            _context = context;
            _paraBirimiService = paraBirimiService;
            _exceptionHandler = exceptionHandler;
        }

        /// <summary>
        /// Tutarı bir para biriminden başka bir para birimine çevirir
        /// </summary>
        /// <param name="tutar">Çevrilecek tutar</param>
        /// <param name="kaynakKod">Kaynak para birimi kodu</param>
        /// <param name="hedefKod">Hedef para birimi kodu</param>
        /// <param name="tarih">İşlem tarihi (null ise bugün)</param>
        /// <returns>Çevrilmiş tutar</returns>
        public async Task<decimal> TutarCevirAsync(decimal tutar, string kaynakKod, string hedefKod, DateTime? tarih = null)
        {
            return await _exceptionHandler.HandleExceptionAsync(async () => 
            {
                // Kaynak ve hedef aynı ise doğrudan döndür
                if (kaynakKod == hedefKod)
                {
                    return tutar;
                }

                var kurDegeri = await _paraBirimiService.HesaplaKurDegeriByKodAsync(kaynakKod, hedefKod, tarih);
                return Math.Round(tutar * kurDegeri, 2);
            }, "TutarCevir", tutar, kaynakKod, hedefKod, tarih);
        }

        /// <summary>
        /// Tutarı bir para biriminden başka bir para birimine çevirir (ID'ler ile)
        /// </summary>
        /// <param name="tutar">Çevrilecek tutar</param>
        /// <param name="kaynakParaBirimiId">Kaynak para birimi ID</param>
        /// <param name="hedefParaBirimiId">Hedef para birimi ID</param>
        /// <param name="tarih">İşlem tarihi (null ise bugün)</param>
        /// <returns>Çevrilmiş tutar</returns>
        public async Task<decimal> TutarCevirAsync(decimal tutar, Guid kaynakParaBirimiId, Guid hedefParaBirimiId, DateTime? tarih = null)
        {
            return await _exceptionHandler.HandleExceptionAsync(async () => 
            {
                // ID'ler aynı ise doğrudan döndür
                if (kaynakParaBirimiId == hedefParaBirimiId)
                {
                    return tutar;
                }

                // Para birimlerini bul
                var kaynakParaBirimi = await _context.BirlesikParaBirimleri.FindAsync(kaynakParaBirimiId);
                var hedefParaBirimi = await _context.BirlesikParaBirimleri.FindAsync(hedefParaBirimiId);

                if (kaynakParaBirimi == null || hedefParaBirimi == null)
                {
                    _logger.LogWarning($"Para birimi bulunamadı: Kaynak({kaynakParaBirimiId}) veya Hedef({hedefParaBirimiId})");
                    return tutar; // Para birimi bulunamazsa orijinal tutarı döndür
                }

                // Kod bazlı çevirme metodunu çağır
                return await TutarCevirAsync(tutar, kaynakParaBirimi.Kod, hedefParaBirimi.Kod, tarih);
            }, "TutarCevirById", tutar, kaynakParaBirimiId, hedefParaBirimiId, tarih);
        }

        /// <summary>
        /// Bir entity'nin belirli bir özelliğindeki para birimini çevirir.
        /// </summary>
        /// <typeparam name="T">Entity tipi</typeparam>
        /// <param name="entity">Çevrilecek entity</param>
        /// <param name="propertySelector">Çevrilecek özelliği seçen fonksiyon (entity => entity.Tutar)</param>
        /// <param name="kaynakKod">Kaynak para birimi kodu</param>
        /// <param name="hedefKod">Hedef para birimi kodu</param>
        /// <param name="tarih">İşlem tarihi (null ise bugün)</param>
        /// <returns>Özellik değeri çevrilmiş entity</returns>
        public async Task<T> EntityOzelligiCevirAsync<T>(
            T entity, 
            Func<T, decimal> propertySelector, 
            Action<T, decimal> propertySetter,
            string kaynakKod, 
            string hedefKod, 
            DateTime? tarih = null)
        {
            return await _exceptionHandler.HandleExceptionAsync(async () => 
            {
                var ozellikDegeri = propertySelector(entity);
                var cevirilmisDeger = await TutarCevirAsync(ozellikDegeri, kaynakKod, hedefKod, tarih);
                propertySetter(entity, cevirilmisDeger);
                return entity;
            }, "EntityOzelligiCevir", typeof(T).Name, kaynakKod, hedefKod, tarih);
        }

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
        public async Task<IEnumerable<T>> EntityKoleksiyonuCevirAsync<T>(
            IEnumerable<T> entities,
            Func<T, decimal> propertySelector,
            Action<T, decimal> propertySetter,
            string kaynakKod,
            string hedefKod,
            DateTime? tarih = null)
        {
            return await _exceptionHandler.HandleExceptionAsync(async () => 
            {
                // Kaynak ve hedef aynı ise dönüşüme gerek yok
                if (kaynakKod == hedefKod)
                {
                    return entities;
                }

                // Kur değerini bir kez hesapla
                var kurDegeri = await _paraBirimiService.HesaplaKurDegeriByKodAsync(kaynakKod, hedefKod, tarih);

                // Tüm entitylerin özellik değerlerini çevir
                foreach (var entity in entities)
                {
                    var ozellikDegeri = propertySelector(entity);
                    var cevirilmisDeger = Math.Round(ozellikDegeri * kurDegeri, 2);
                    propertySetter(entity, cevirilmisDeger);
                }

                return entities;
            }, "EntityKoleksiyonuCevir", typeof(T).Name, kaynakKod, hedefKod, tarih);
        }
    }
} 