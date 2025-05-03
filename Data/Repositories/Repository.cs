#nullable enable

using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using MuhasebeStokWebApp.Data.Entities;

namespace MuhasebeStokWebApp.Data.Repositories
{
    // Generic Repository Pattern uygulaması - Tüm entity'ler için ortak CRUD işlemleri sağlar
    public class Repository<TEntity> : IRepository<TEntity> where TEntity : class
    {
        protected readonly ApplicationDbContext _context;
        internal DbSet<TEntity> _dbSet;

        // Constructor: Veritabanı bağlantısını alır ve ilgili entity set'ini hazırlar
        public Repository(ApplicationDbContext context)
        {
            _context = context;
            _dbSet = context.Set<TEntity>();
        }

        public virtual IQueryable<TEntity> GetAll()
        {
            return _dbSet.AsNoTracking();
        }

        public virtual async Task<IEnumerable<TEntity>> GetAllAsync(
            Expression<Func<TEntity, bool>> filter = null,
            Func<IQueryable<TEntity>, IOrderedQueryable<TEntity>> orderBy = null,
            string includeProperties = "", 
            bool ignoreQueryFilters = false, 
            bool asNoTracking = false)
        {
            IQueryable<TEntity> query = _dbSet;
            
            if (ignoreQueryFilters)
            {
                query = query.IgnoreQueryFilters();
            }
            
            if (asNoTracking)
            {
                query = query.AsNoTracking();
            }

            if (filter != null)
            {
                query = query.Where(filter);
            }

            if (!string.IsNullOrEmpty(includeProperties))
            {
                foreach (var includeProperty in includeProperties.Split
                    (new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries))
                {
                    query = query.Include(includeProperty);
                }
            }

            if (orderBy != null)
            {
                return await orderBy(query).ToListAsync();
            }
            else
            {
                return await query.ToListAsync();
            }
        }

        public virtual IQueryable<TEntity> Find(Expression<Func<TEntity, bool>> expression)
        {
            return _dbSet.Where(expression);
        }

        public virtual async Task<IEnumerable<TEntity>> FindAsync(Expression<Func<TEntity, bool>> predicate)
        {
            return await _dbSet.Where(predicate).ToListAsync();
        }

        public virtual async Task<TEntity> GetByIdAsync(object id)
        {
            return await _dbSet.FindAsync(id);
        }

        public virtual async Task<TEntity> GetFirstOrDefaultAsync(
            Expression<Func<TEntity, bool>> filter = null,
            string includeProperties = "",
            bool ignoreQueryFilters = false,
            bool asNoTracking = false)
        {
            IQueryable<TEntity> query = _dbSet;
            
            if (ignoreQueryFilters)
            {
                query = query.IgnoreQueryFilters();
            }
            
            if (asNoTracking)
            {
                query = query.AsNoTracking();
            }

            if (filter != null)
            {
                query = query.Where(filter);
            }

            if (!string.IsNullOrEmpty(includeProperties))
            {
                foreach (var includeProperty in includeProperties.Split
                    (new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries))
                {
                    query = query.Include(includeProperty);
                }
            }

            return await query.FirstOrDefaultAsync();
        }

        public virtual async Task<TEntity> GetFirstAsync()
        {
            return await _dbSet.FirstOrDefaultAsync();
        }

        public virtual async Task AddAsync(TEntity entity)
        {
            await _dbSet.AddAsync(entity);
        }

        public virtual async Task AddOrUpdateAsync(TEntity entity)
        {
            var entry = _context.Entry(entity);
            if (entry.State == EntityState.Detached)
            {
                _dbSet.Add(entity);
            }
            else
            {
                _dbSet.Update(entity);
            }
            await Task.CompletedTask;
        }

        public virtual async Task RemoveAsync(object id)
        {
            TEntity entityToRemove = await _dbSet.FindAsync(id);
            if (entityToRemove != null)
            {
                _dbSet.Remove(entityToRemove);
            }
        }

        public virtual async Task RemoveAsync(TEntity entity)
        {
            if (_context.Entry(entity).State == EntityState.Detached)
            {
                _dbSet.Attach(entity);
            }
            _dbSet.Remove(entity);
            await Task.CompletedTask;
        }

        public virtual async Task RemoveRangeAsync(IEnumerable<TEntity> entities)
        {
            _dbSet.RemoveRange(entities);
            await Task.CompletedTask;
        }

        public async Task<Irsaliye> GetIrsaliyeWithDetailsAsync(Guid id)
        {
            // Önce sadece irsaliyeyi getir
            var irsaliye = await _context.Irsaliyeler
                .FirstOrDefaultAsync(i => i.IrsaliyeID.Equals(id) && !i.Silindi);
                
            if (irsaliye == null)
                return null;
                
            // İlişkili verileri ayrı sorgularda getir
            await _context.Entry(irsaliye)
                .Reference(i => i.Cari)
                .LoadAsync();
                
            // IrsaliyeDetaylari ilişkisini yükle
            await _context.Entry(irsaliye)
                .Collection(i => i.IrsaliyeDetaylari)
                .Query()
                .Where(id => !id.Silindi)
                .LoadAsync();
                
            // Her detay için Urun ilişkisini yükle
            foreach (var detay in irsaliye.IrsaliyeDetaylari.Where(d => d != null))
            {
                await _context.Entry(detay)
                    .Reference(d => d.Urun)
                    .LoadAsync();
            }
                
            return irsaliye;
        }

        public async Task AddRangeAsync(IEnumerable<TEntity> entities)
        {
            await _dbSet.AddRangeAsync(entities);
        }
        
        public void Update(TEntity entity)
        {
            _dbSet.Update(entity);
        }
        
        public void UpdateRange(IEnumerable<TEntity> entities)
        {
            _dbSet.UpdateRange(entities);
        }
        
        public void Remove(TEntity entity)
        {
            _dbSet.Remove(entity);
        }
        
        public async Task<IEnumerable<TEntity>> GetAsync(
            Expression<Func<TEntity, bool>> filter = null, 
            Func<IQueryable<TEntity>, IOrderedQueryable<TEntity>> orderBy = null, 
            string includeProperties = "")
        {
            IQueryable<TEntity> query = _dbSet;

            if (filter != null)
            {
                query = query.Where(filter);
            }

            if (!string.IsNullOrEmpty(includeProperties))
            {
                foreach (var includeProperty in includeProperties.Split
                    (new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries))
                {
                    query = query.Include(includeProperty);
                }
            }

            if (orderBy != null)
            {
                return await orderBy(query).ToListAsync();
            }
            else
            {
                return await query.ToListAsync();
            }
        }
        
        public async Task UpdateAsync(TEntity entity)
        {
            _dbSet.Update(entity);
            await Task.CompletedTask;
        }

        public async Task<List<TEntity>> GetAllByPredicateAsync(Expression<Func<TEntity, bool>> predicate)
        {
            return await _context.Set<TEntity>().Where(predicate).ToListAsync();
        }
        
        public async Task<int> CountAsync(Expression<Func<TEntity, bool>> filter = null, bool asNoTracking = false)
        {
            IQueryable<TEntity> query = _dbSet;
            
            if (asNoTracking)
            {
                query = query.AsNoTracking();
            }
            
            if (filter != null)
            {
                query = query.Where(filter);
            }
            
            return await query.CountAsync();
        }
        
        public async Task<bool> AnyAsync(Expression<Func<TEntity, bool>> filter = null)
        {
            if (filter == null)
            {
                return await _dbSet.AnyAsync();
            }
            
            return await _dbSet.AnyAsync(filter);
        }
        
        public IQueryable<TEntity> Query()
        {
            return _dbSet;
        }
        
        public async Task<IEnumerable<TResult>> GetAllAsync<TResult>(
            Expression<Func<TEntity, bool>> filter = null,
            Expression<Func<TEntity, TResult>> selector = null,
            Func<IQueryable<TEntity>, IOrderedQueryable<TEntity>> orderBy = null,
            string includeProperties = "",
            bool asNoTracking = false)
        {
            IQueryable<TEntity> query = _dbSet;
            
            if (asNoTracking)
            {
                query = query.AsNoTracking();
            }
            
            if (filter != null)
            {
                query = query.Where(filter);
            }
            
            if (!string.IsNullOrEmpty(includeProperties))
            {
                foreach (var includeProperty in includeProperties.Split
                    (new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries))
                {
                    query = query.Include(includeProperty);
                }
            }
            
            if (orderBy != null)
            {
                query = orderBy(query);
            }
            
            if (selector != null)
            {
                return await query.Select(selector).ToListAsync();
            }
            
            // Selector null ise, TResult ve TEntity aynı tip olmalı
            return await query.Cast<TResult>().ToListAsync();
        }
    }
} 