using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using MuhasebeStokWebApp.Data;

namespace MuhasebeStokWebApp.Services.Filters
{
    /// <summary>
    /// IFilterService için temel implementasyon sınıfı
    /// </summary>
    public abstract class FilterServiceBase<TEntity, TFilter> : IFilterService<TEntity, TFilter> 
        where TEntity : class
        where TFilter : class, new()
    {
        protected readonly ApplicationDbContext _context;
        protected readonly ILogger _logger;
        
        public FilterServiceBase(ApplicationDbContext context, ILogger logger)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }
        
        /// <summary>
        /// Filtre kriterlerine göre sorgu ifadesi oluşturur
        /// Bu metot, türetilen sınıflarda özelleştirilmelidir
        /// </summary>
        public abstract Expression<Func<TEntity, bool>> BuildFilterExpression(TFilter filter);
        
        /// <summary>
        /// Filtre kriterlerine göre sonuçları getirir
        /// </summary>
        public virtual async Task<(List<TEntity> Results, int TotalCount)> GetFilteredResultsAsync(TFilter filter, int page = 1, int pageSize = 10)
        {
            try
            {
                // Filtre ifadesini oluştur
                var filterExpression = BuildFilterExpression(filter);
                
                // Toplam kayıt sayısını al
                var totalCount = await _context.Set<TEntity>()
                    .Where(filterExpression)
                    .CountAsync();
                
                // Sayfalanmış sonuçları al
                var results = await _context.Set<TEntity>()
                    .Where(filterExpression)
                    .Skip((page - 1) * pageSize)
                    .Take(pageSize)
                    .ToListAsync();
                
                return (results, totalCount);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "GetFilteredResultsAsync metodu hatası: {Message}", ex.Message);
                return (new List<TEntity>(), 0);
            }
        }
        
        /// <summary>
        /// Filtre kriterlerine göre sonuçların toplam sayısını getirir
        /// </summary>
        public virtual async Task<int> GetFilteredCountAsync(TFilter filter)
        {
            try
            {
                // Filtre ifadesini oluştur
                var filterExpression = BuildFilterExpression(filter);
                
                // Toplam kayıt sayısını al
                return await _context.Set<TEntity>()
                    .Where(filterExpression)
                    .CountAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "GetFilteredCountAsync metodu hatası: {Message}", ex.Message);
                return 0;
            }
        }
        
        /// <summary>
        /// Filtre model değerlerini temizler
        /// Bu metot, türetilen sınıflarda özelleştirilebilir
        /// </summary>
        public virtual TFilter ClearFilter()
        {
            return new TFilter();
        }
        
        /// <summary>
        /// DbSet üzerinden include işlemi yapan yardımcı metot
        /// </summary>
        protected IQueryable<TEntity> ApplyIncludes(IQueryable<TEntity> query, params string[] includes)
        {
            foreach (var include in includes)
            {
                if (!string.IsNullOrEmpty(include))
                {
                    query = query.Include(include);
                }
            }
            
            return query;
        }
    }
} 