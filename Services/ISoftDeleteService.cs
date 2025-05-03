using System;
using System.Threading.Tasks;
using MuhasebeStokWebApp.Data.Entities;

namespace MuhasebeStokWebApp.Services
{
    /// <summary>
    /// Soft delete işlemlerini yönetmek için merkezi servis arayüzü
    /// </summary>
    /// <typeparam name="TEntity">ISoftDelete arayüzünü uygulayan entity tipi</typeparam>
    public interface ISoftDeleteService<TEntity> where TEntity : class, ISoftDelete
    {
        /// <summary>
        /// Entity'yi soft delete yapar
        /// </summary>
        /// <param name="entity">Silinecek entity</param>
        /// <returns>İşlem başarılı ise true, değilse false</returns>
        Task<bool> SoftDeleteAsync(TEntity entity);
        
        /// <summary>
        /// Entity'yi ID'sine göre soft delete yapar
        /// </summary>
        /// <param name="id">Silinecek entity'nin ID'si</param>
        /// <returns>İşlem başarılı ise true, değilse false</returns>
        Task<bool> SoftDeleteByIdAsync(object id);
        
        /// <summary>
        /// Soft delete yapılmış bir entity'yi geri yükler
        /// </summary>
        /// <param name="entity">Geri yüklenecek entity</param>
        /// <returns>İşlem başarılı ise true, değilse false</returns>
        Task<bool> RestoreAsync(TEntity entity);
        
        /// <summary>
        /// Soft delete yapılmış bir entity'yi ID'sine göre geri yükler
        /// </summary>
        /// <param name="id">Geri yüklenecek entity'nin ID'si</param>
        /// <returns>İşlem başarılı ise true, değilse false</returns>
        Task<bool> RestoreByIdAsync(object id);
        
        /// <summary>
        /// Entity'nin ilişkili kayıtları olup olmadığını kontrol eder
        /// Bu kontrol, hard delete yapılabilir mi yoksa sadece soft delete mi yapılmalı kararında kullanılır
        /// </summary>
        /// <param name="id">Kontrol edilecek entity'nin ID'si</param>
        /// <returns>İlişkili kayıtlar varsa true, yoksa false</returns>
        Task<bool> HasRelatedRecordsAsync(object id);
    }
} 