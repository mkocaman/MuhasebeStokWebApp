using System;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using MuhasebeStokWebApp.Data;
using MuhasebeStokWebApp.Data.Entities;
using MuhasebeStokWebApp.Services.Interfaces;
using System.Linq;
using System.Reflection;
using System.Linq.Expressions;

namespace MuhasebeStokWebApp.Services
{
    /// <summary>
    /// Soft Delete işlemlerini yönetecek servis
    /// </summary>
    /// <typeparam name="TEntity">ISoftDelete arayüzünü uygulayan entity tipi</typeparam>
    public class SoftDeleteService<TEntity> : ISoftDeleteService<TEntity> where TEntity : class, ISoftDelete
    {
        protected readonly ApplicationDbContext _context;
        protected readonly ILogger<SoftDeleteService<TEntity>> _logger;

        public SoftDeleteService(ApplicationDbContext context, ILogger<SoftDeleteService<TEntity>> logger)
        {
            _context = context;
            _logger = logger;
        }

        /// <summary>
        /// Verilen entity'i soft delete yapar
        /// </summary>
        public virtual async Task<bool> SoftDeleteAsync(TEntity entity)
        {
            try
            {
                if (entity == null)
                {
                    _logger.LogWarning("SoftDeleteAsync: Entity null olamaz");
                    return false;
                }

                entity.Silindi = true;

                _context.Update(entity);
                await _context.SaveChangesAsync();

                _logger.LogInformation($"SoftDeleteAsync: {typeof(TEntity).Name} başarıyla silindi");
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"SoftDeleteAsync hatası: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// Verilen ID'ye sahip entity'i soft delete yapar
        /// </summary>
        public virtual async Task<bool> SoftDeleteByIdAsync(object id)
        {
            try
            {
                if (id == null)
                {
                    _logger.LogWarning("SoftDeleteByIdAsync: ID null olamaz");
                    return false;
                }

                var entity = await _context.Set<TEntity>().FindAsync(id);

                if (entity == null)
                {
                    _logger.LogWarning($"SoftDeleteByIdAsync: {id} ID'li entity bulunamadı");
                    return false;
                }

                return await SoftDeleteAsync(entity);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"SoftDeleteByIdAsync hatası: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// Verilen filtreye göre entity'leri soft delete yapar
        /// </summary>
        public virtual async Task<int> DeleteRangeAsync(Expression<Func<TEntity, bool>> filter)
        {
            try
            {
                if (filter == null)
                {
                    _logger.LogWarning("DeleteRangeAsync: Filtre null olamaz");
                    return 0;
                }

                var entities = await _context.Set<TEntity>().Where(filter).ToListAsync();

                if (entities == null || !entities.Any())
                {
                    _logger.LogWarning("DeleteRangeAsync: Filtreye uygun entity bulunamadı");
                    return 0;
                }

                foreach (var entity in entities)
                {
                    entity.Silindi = true;
                }

                _context.UpdateRange(entities);
                await _context.SaveChangesAsync();

                _logger.LogInformation($"DeleteRangeAsync: {entities.Count} adet {typeof(TEntity).Name} başarıyla silindi");
                return entities.Count;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"DeleteRangeAsync hatası: {ex.Message}");
                return 0;
            }
        }

        /// <summary>
        /// Entity içindeki key property adını döndürür
        /// </summary>
        protected string GetKeyPropertyName()
        {
            var properties = typeof(TEntity).GetProperties();
            var keyProperty = properties.FirstOrDefault(p => 
                p.GetCustomAttributes(typeof(System.ComponentModel.DataAnnotations.KeyAttribute), true).Any());

            if (keyProperty != null)
                return keyProperty.Name;

            // ID ya da TypeNameID formatında property adı ara
            var entityName = typeof(TEntity).Name;
            var idProperty = properties.FirstOrDefault(p => p.Name.Equals("ID", StringComparison.OrdinalIgnoreCase) ||
                                                          p.Name.Equals($"{entityName}ID", StringComparison.OrdinalIgnoreCase) ||
                                                          p.Name.Equals($"{entityName}Id", StringComparison.OrdinalIgnoreCase));
            
            return idProperty?.Name;
        }

        /// <summary>
        /// Verilen entity'i geri yükler (silindi false yapar)
        /// </summary>
        public virtual async Task<bool> RestoreAsync(TEntity entity)
        {
            try
            {
                if (entity == null)
                {
                    _logger.LogWarning("RestoreAsync: Entity null olamaz");
                    return false;
                }

                entity.Silindi = false;

                _context.Update(entity);
                await _context.SaveChangesAsync();

                _logger.LogInformation($"RestoreAsync: {typeof(TEntity).Name} başarıyla geri yüklendi");
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"RestoreAsync hatası: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// Soft delete yapılmış bir entity'yi ID'sine göre geri yükler
        /// </summary>
        public virtual async Task<bool> RestoreByIdAsync(object id)
        {
            try
            {
                _logger.LogInformation($"Restoring {typeof(TEntity).Name} with ID {id}");
                var propertyName = GetKeyPropertyName();
                
                if (string.IsNullOrEmpty(propertyName))
                {
                    _logger.LogError($"Key property not found for {typeof(TEntity).Name}");
                    return false;
                }

                var parameter = Expression.Parameter(typeof(TEntity), "x");
                var property = Expression.Property(parameter, propertyName);
                var value = Expression.Constant(Guid.Parse(id.ToString()));
                var equals = Expression.Equal(property, value);
                var lambda = Expression.Lambda<Func<TEntity, bool>>(equals, parameter);

                // IgnoreQueryFilters kullanımı burada gerekli çünkü silinmiş veriyi alıyoruz
                var entity = await _context.Set<TEntity>()
                    .IgnoreQueryFilters()
                    .FirstOrDefaultAsync(lambda);

                if (entity == null)
                {
                    _logger.LogWarning($"{typeof(TEntity).Name} with ID {id} not found or already restored");
                    return false;
                }

                entity.Silindi = false;
                
                // Eğer Cari sınıfı ise açıklama ekle
                if (typeof(TEntity) == typeof(Cari))
                {
                    var cari = entity as Cari;
                    if (cari != null)
                    {
                        cari.Aciklama = (cari.Aciklama ?? "") + " (Restore Edildi: " + DateTime.Now.ToString("dd.MM.yyyy HH:mm") + ")";
                    }
                }

                await _context.SaveChangesAsync();
                
                _logger.LogInformation($"{typeof(TEntity).Name} with ID {id} restored successfully");
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error restoring {typeof(TEntity).Name} with ID {id}: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// Verilen filtreye göre silinmiş entity'leri geri yükler
        /// </summary>
        public virtual async Task<int> RestoreRangeAsync(Expression<Func<TEntity, bool>> filter)
        {
            try
            {
                if (filter == null)
                {
                    _logger.LogWarning("RestoreRangeAsync: Filtre null olamaz");
                    return 0;
                }

                var entities = await _context.Set<TEntity>()
                    .IgnoreQueryFilters()
                    .Where(e => e.Silindi)
                    .Where(filter)
                    .ToListAsync();

                if (entities == null || !entities.Any())
                {
                    _logger.LogWarning("RestoreRangeAsync: Filtreye uygun silinmiş entity bulunamadı");
                    return 0;
                }

                foreach (var entity in entities)
                {
                    entity.Silindi = false;
                }

                _context.UpdateRange(entities);
                await _context.SaveChangesAsync();

                _logger.LogInformation($"RestoreRangeAsync: {entities.Count} adet {typeof(TEntity).Name} başarıyla geri yüklendi");
                return entities.Count;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"RestoreRangeAsync hatası: {ex.Message}");
                return 0;
            }
        }
        
        /// <summary>
        /// Entity'nin ilişkili kayıtları olup olmadığını kontrol eder
        /// </summary>
        public virtual async Task<bool> HasRelatedRecordsAsync(object id)
        {
            _logger.LogWarning($"HasRelatedRecordsAsync metodu {typeof(TEntity).Name} için uygulanmamış");
            return false;
        }
    }
} 