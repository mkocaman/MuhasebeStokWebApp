using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using MuhasebeStokWebApp.Data.Entities;

namespace MuhasebeStokWebApp.Data.Repositories
{
    public interface IRepository<TEntity> where TEntity : class
    {
        Task<IEnumerable<TEntity>> GetAllAsync(Expression<Func<TEntity, bool>> filter = null,
            Func<IQueryable<TEntity>, IOrderedQueryable<TEntity>> orderBy = null,
            string includeProperties = "", bool ignoreQueryFilters = false, bool asNoTracking = false);
        Task<TEntity> GetByIdAsync(object id);
        Task<TEntity> GetFirstOrDefaultAsync(Expression<Func<TEntity, bool>> filter = null, string includeProperties = "", bool ignoreQueryFilters = false, bool asNoTracking = false);
        Task<TEntity> GetFirstAsync();
        Task<IEnumerable<TEntity>> GetAsync(
            Expression<Func<TEntity, bool>> filter = null, 
            Func<IQueryable<TEntity>, IOrderedQueryable<TEntity>> orderBy = null, 
            string includeProperties = "");
        IQueryable<TEntity> GetAll();
        IQueryable<TEntity> Find(Expression<Func<TEntity, bool>> expression);
        Task<IEnumerable<TEntity>> FindAsync(Expression<Func<TEntity, bool>> predicate);
        Task<List<TEntity>> GetAllByPredicateAsync(Expression<Func<TEntity, bool>> predicate);
        Task AddAsync(TEntity entity);
        Task AddOrUpdateAsync(TEntity entity);
        Task AddRangeAsync(IEnumerable<TEntity> entities);
        void Update(TEntity entity);
        void UpdateRange(IEnumerable<TEntity> entities);
        void Remove(TEntity entity);
        Task UpdateAsync(TEntity entity);
        Task RemoveAsync(object id);
        Task RemoveAsync(TEntity entity);
        Task RemoveRangeAsync(IEnumerable<TEntity> entities);
        Task<Irsaliye> GetIrsaliyeWithDetailsAsync(Guid id);
        Task<int> CountAsync(Expression<Func<TEntity, bool>> filter = null, bool asNoTracking = false);
        Task<bool> AnyAsync(Expression<Func<TEntity, bool>> filter = null);
        IQueryable<TEntity> Query();
        Task<IEnumerable<TResult>> GetAllAsync<TResult>(
            Expression<Func<TEntity, bool>> filter = null,
            Expression<Func<TEntity, TResult>> selector = null,
            Func<IQueryable<TEntity>, IOrderedQueryable<TEntity>> orderBy = null,
            string includeProperties = "",
            bool asNoTracking = false);
    }
} 