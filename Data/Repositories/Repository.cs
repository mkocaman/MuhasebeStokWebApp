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
    public class Repository<T> : IRepository<T> where T : class
    {
        private readonly ApplicationDbContext _context;
        internal DbSet<T> dbSet;

        // Constructor: Veritabanı bağlantısını alır ve ilgili entity set'ini hazırlar
        public Repository(ApplicationDbContext context)
        {
            _context = context;
            this.dbSet = _context.Set<T>();
        }

        // Yeni bir kayıt ekler - SaveChanges çağrılmaz, UnitOfWork tarafından yönetilir
        public async Task AddAsync(T entity)
        {
            await dbSet.AddAsync(entity);
        }

        // Tüm kayıtları liste olarak döndürür
        public async Task<IEnumerable<T>> GetAllAsync()
        {
            return await dbSet.ToListAsync();
        }

        // Filtreleme, sıralama ve ilişkili entity'leri dahil etme özelliklerini sunar
        public async Task<IEnumerable<T>> GetAsync(Expression<Func<T, bool>> filter = null, 
            Func<IQueryable<T>, IOrderedQueryable<T>> orderBy = null, 
            string includeProperties = "")
        {
            IQueryable<T> query = dbSet;

            // Filtre varsa uygula
            if (filter != null)
            {
                query = query.Where(filter);
            }

            // İlişkili entity'leri dahil et (Include)
            if (!string.IsNullOrEmpty(includeProperties))
            {
                foreach (var includeProperty in includeProperties.Split
                    (new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries))
                {
                    query = query.Include(includeProperty.Trim());
                }
            }

            // Sıralama varsa uygula, yoksa normal listeyi döndür
            if (orderBy != null)
            {
                return await orderBy(query).ToListAsync();
            }
            else
            {
                return await query.ToListAsync();
            }
        }

        // Belirli bir ID'ye sahip kaydı getirir
        public async Task<T> GetByIdAsync(object id)
        {
            return await dbSet.FindAsync(id);
        }

        // Filtreleme ve ilişkili entity'leri dahil ederek ilk kaydı getirir
        public async Task<T> GetFirstOrDefaultAsync(Expression<Func<T, bool>> filter = null, string includeProperties = "")
        {
            IQueryable<T> query = dbSet;

            // Filtre varsa uygula
            if (filter != null)
            {
                query = query.Where(filter);
            }

            // İlişkili entity'leri dahil et (Include)
            if (!string.IsNullOrEmpty(includeProperties))
            {
                foreach (var includeProperty in includeProperties.Split
                    (new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries))
                {
                    query = query.Include(includeProperty.Trim());
                }
            }

            return await query.FirstOrDefaultAsync();
        }
        
        // Tablodaki ilk kaydı getirir
        public async Task<T> GetFirstAsync()
        {
            return await dbSet.FirstOrDefaultAsync();
        }

        // ID kullanarak kayıt siler - SaveChanges çağrılmaz, UnitOfWork tarafından yönetilir
        public async Task RemoveAsync(object id)
        {
            T entity = await dbSet.FindAsync(id);
            if (entity != null)
            {
                RemoveEntity(entity);
            }
        }

        // Entity nesnesini siler - SaveChanges çağrılmaz, UnitOfWork tarafından yönetilir
        public async Task RemoveAsync(T entity)
        {
            RemoveEntity(entity);
        }

        private void RemoveEntity(T entity)
        {
            // Soft Delete desteği
            if (entity is ISoftDelete softDeleteEntity)
            {
                softDeleteEntity.Silindi = true;
                dbSet.Update(entity);
            }
            else
            {
                dbSet.Remove(entity);
            }
        }

        // Birden fazla entity'yi toplu olarak siler - SaveChanges çağrılmaz, UnitOfWork tarafından yönetilir
        public async Task RemoveRangeAsync(IEnumerable<T> entities)
        {
            // Soft Delete desteğini kontrol edelim
            foreach (var entity in entities)
            {
                await RemoveAsync(entity);
            }
        }

        // Entity'yi günceller - SaveChanges çağrılmaz, UnitOfWork tarafından yönetilir
        public async Task UpdateAsync(T entity)
        {
            _context.Entry(entity).State = EntityState.Modified;
        }

        // Belirli bir koşulu sağlayan entity'leri bulur
        public async Task<IEnumerable<T>> FindAsync(Expression<Func<T, bool>> predicate)
        {
            return await dbSet.Where(predicate).ToListAsync();
        }

        // Entity'yi ekler veya günceller (varsa günceller, yoksa ekler) - SaveChanges çağrılmaz, UnitOfWork tarafından yönetilir
        public async Task AddOrUpdateAsync(T entity)
        {
            // Entity'nin primary key değerini reflection ile al
            var keyName = _context.Model.FindEntityType(typeof(T)).FindPrimaryKey().Properties
                .Select(x => x.Name).Single();
            var keyProperty = entity.GetType().GetProperty(keyName);
            var keyValue = keyProperty.GetValue(entity);
            var defaultValue = keyProperty.PropertyType.IsValueType ? Activator.CreateInstance(keyProperty.PropertyType) : null;
            
            // Key değeri varsayılan değer ise (Guid.Empty, 0, null, vb.) veya
            // veritabanında bu key ile kayıt yoksa, entity'yi ekle
            if (keyValue == null || keyValue.Equals(defaultValue) || await dbSet.FindAsync(keyValue) == null)
            {
                await dbSet.AddAsync(entity);
            }
            else
            {
                // Aksi halde entity'yi güncelle
                _context.Entry(entity).State = EntityState.Modified;
            }
        }

        public virtual async Task<Irsaliye> GetIrsaliyeWithDetailsAsync(Guid id)
        {
            if (typeof(T) != typeof(Irsaliye))
                throw new InvalidOperationException("This method can only be called on Irsaliye repository");

            return await _context.Set<Irsaliye>()
                .Include(i => i.IrsaliyeDetaylari)
                .Include(i => i.Cari)
                .FirstOrDefaultAsync(i => i.IrsaliyeID.Equals(id) && !i.Silindi);
        }
    }
} 