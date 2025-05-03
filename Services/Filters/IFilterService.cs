using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Linq.Expressions;

namespace MuhasebeStokWebApp.Services.Filters
{
    /// <summary>
    /// Filtreleme işlemleri için temel servis arayüzü
    /// </summary>
    /// <typeparam name="TEntity">Filtre uygulanacak entity tipi</typeparam>
    /// <typeparam name="TFilter">Filter model tipi</typeparam>
    public interface IFilterService<TEntity, TFilter> where TEntity : class
    {
        /// <summary>
        /// Filtre kriterlerine göre sorgu ifadesi oluşturur
        /// </summary>
        /// <param name="filter">Filtre modeli</param>
        /// <returns>Entity üzerinde filtre uygulayacak expression</returns>
        Expression<Func<TEntity, bool>> BuildFilterExpression(TFilter filter);
        
        /// <summary>
        /// Filtre kriterlerine göre sonuçları getirir
        /// </summary>
        /// <param name="filter">Filtre modeli</param>
        /// <param name="page">Sayfa numarası (1'den başlar)</param>
        /// <param name="pageSize">Sayfa başına kayıt sayısı</param>
        /// <returns>Filtrelenmiş sonuçlar ve toplam kayıt sayısı</returns>
        Task<(List<TEntity> Results, int TotalCount)> GetFilteredResultsAsync(TFilter filter, int page = 1, int pageSize = 10);
        
        /// <summary>
        /// Filtre kriterlerine göre sonuçların toplam sayısını getirir
        /// </summary>
        /// <param name="filter">Filtre modeli</param>
        /// <returns>Toplam kayıt sayısı</returns>
        Task<int> GetFilteredCountAsync(TFilter filter);
        
        /// <summary>
        /// Filtre model değerlerini temizler
        /// </summary>
        /// <returns>Temizlenmiş filtre modeli</returns>
        TFilter ClearFilter();
    }
} 