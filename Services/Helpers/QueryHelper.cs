using System;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

namespace MuhasebeStokWebApp.Services.Helpers
{
    /// <summary>
    /// Entity Framework sorguları için yardımcı metotlar içeren sınıf
    /// </summary>
    public static class QueryHelper
    {
        /// <summary>
        /// FirstOrDefaultAsync metodunu sıralama ile birlikte kullanarak öngörülebilir sonuçlar alınmasını sağlar
        /// </summary>
        public static async Task<T> FirstOrDefaultOrderedAsync<T, TKey>(
            this IQueryable<T> query, 
            Expression<Func<T, bool>> predicate, 
            Expression<Func<T, TKey>> orderBy)
            where T : class
        {
            return await query
                .Where(predicate)
                .OrderBy(orderBy)
                .FirstOrDefaultAsync();
        }

        /// <summary>
        /// FirstOrDefaultAsync metodunu sıralama ile birlikte kullanarak öngörülebilir sonuçlar alınmasını sağlar (tersten sıralama)
        /// </summary>
        public static async Task<T> FirstOrDefaultOrderedDescendingAsync<T, TKey>(
            this IQueryable<T> query, 
            Expression<Func<T, bool>> predicate, 
            Expression<Func<T, TKey>> orderBy)
            where T : class
        {
            return await query
                .Where(predicate)
                .OrderByDescending(orderBy)
                .FirstOrDefaultAsync();
        }
    }
} 