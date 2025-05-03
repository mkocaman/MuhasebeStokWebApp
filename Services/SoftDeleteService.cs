using System;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using MuhasebeStokWebApp.Data;
using MuhasebeStokWebApp.Data.Entities;
using MuhasebeStokWebApp.Data.Repositories;

namespace MuhasebeStokWebApp.Services
{
    /// <summary>
    /// ISoftDeleteService arayüzünün genel uygulaması
    /// </summary>
    /// <typeparam name="TEntity">ISoftDelete arayüzünü uygulayan entity tipi</typeparam>
    public class SoftDeleteService<TEntity> : ISoftDeleteService<TEntity> where TEntity : class, ISoftDelete
    {
        protected readonly IUnitOfWork _unitOfWork;
        protected readonly IRepository<TEntity> _repository;
        protected readonly ILogger<SoftDeleteService<TEntity>> _logger;
        protected readonly ApplicationDbContext _context;
        
        public SoftDeleteService(
            IUnitOfWork unitOfWork,
            ILogger<SoftDeleteService<TEntity>> logger,
            ApplicationDbContext context)
        {
            _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
            _repository = _unitOfWork.Repository<TEntity>();
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _context = context ?? throw new ArgumentNullException(nameof(context));
        }
        
        /// <summary>
        /// Entity'yi soft delete yapar
        /// </summary>
        public virtual async Task<bool> SoftDeleteAsync(TEntity entity)
        {
            if (entity == null)
            {
                _logger.LogWarning("SoftDeleteAsync: Entity null olamaz");
                return false;
            }
            
            try
            {
                // Transaction başlat
                await _unitOfWork.BeginTransactionAsync();
                
                // Entity'yi güncelle
                entity.Silindi = true;
                
                // Aktif özelliğine sahipse, onu da false yap
                var aktivasyonProperty = entity.GetType().GetProperty("Aktif") ?? 
                                        entity.GetType().GetProperty("AktifMi");
                
                if (aktivasyonProperty != null)
                {
                    aktivasyonProperty.SetValue(entity, false);
                }
                
                // GuncellemeTarihi özelliğine sahipse, şimdiki zamanı ata
                var guncellemeTarihiProperty = entity.GetType().GetProperty("GuncellemeTarihi");
                if (guncellemeTarihiProperty != null)
                {
                    guncellemeTarihiProperty.SetValue(entity, DateTime.Now);
                }
                
                // Entity'yi güncelle
                await _repository.UpdateAsync(entity);
                await _unitOfWork.SaveChangesAsync();
                
                // İşlemi tamamla
                await _unitOfWork.CommitTransactionAsync();
                
                _logger.LogInformation($"{typeof(TEntity).Name} ID: {GetEntityId(entity)} soft delete yapıldı");
                return true;
            }
            catch (Exception ex)
            {
                await _unitOfWork.RollbackTransactionAsync();
                _logger.LogError(ex, $"SoftDeleteAsync hatası: {ex.Message}");
                return false;
            }
        }
        
        /// <summary>
        /// Entity'yi ID'sine göre soft delete yapar
        /// </summary>
        public virtual async Task<bool> SoftDeleteByIdAsync(object id)
        {
            if (id == null)
            {
                _logger.LogWarning("SoftDeleteByIdAsync: ID null olamaz");
                return false;
            }
            
            try
            {
                // Entity'yi getir
                var entity = await _repository.GetByIdAsync(id);
                if (entity == null)
                {
                    _logger.LogWarning($"SoftDeleteByIdAsync: {id} ID'li entity bulunamadı");
                    return false;
                }
                
                // İlişkili kayıtlar varsa, sadece soft delete yap
                bool hasRelated = await HasRelatedRecordsAsync(id);
                
                // Soft delete yap
                return await SoftDeleteAsync(entity);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"SoftDeleteByIdAsync hatası: {ex.Message}");
                return false;
            }
        }
        
        /// <summary>
        /// Soft delete yapılmış bir entity'yi geri yükler
        /// </summary>
        public virtual async Task<bool> RestoreAsync(TEntity entity)
        {
            if (entity == null)
            {
                _logger.LogWarning("RestoreAsync: Entity null olamaz");
                return false;
            }
            
            try
            {
                // Transaction başlat
                await _unitOfWork.BeginTransactionAsync();
                
                // Entity'yi güncelle
                entity.Silindi = false;
                
                // Aktif özelliğine sahipse, onu da true yap
                var aktivasyonProperty = entity.GetType().GetProperty("Aktif") ?? 
                                        entity.GetType().GetProperty("AktifMi");
                
                if (aktivasyonProperty != null)
                {
                    aktivasyonProperty.SetValue(entity, true);
                }
                
                // GuncellemeTarihi özelliğine sahipse, şimdiki zamanı ata
                var guncellemeTarihiProperty = entity.GetType().GetProperty("GuncellemeTarihi");
                if (guncellemeTarihiProperty != null)
                {
                    guncellemeTarihiProperty.SetValue(entity, DateTime.Now);
                }
                
                // Entity'yi güncelle
                await _repository.UpdateAsync(entity);
                await _unitOfWork.SaveChangesAsync();
                
                // İşlemi tamamla
                await _unitOfWork.CommitTransactionAsync();
                
                _logger.LogInformation($"{typeof(TEntity).Name} ID: {GetEntityId(entity)} geri yüklendi");
                return true;
            }
            catch (Exception ex)
            {
                await _unitOfWork.RollbackTransactionAsync();
                _logger.LogError(ex, $"RestoreAsync hatası: {ex.Message}");
                return false;
            }
        }
        
        /// <summary>
        /// Soft delete yapılmış bir entity'yi ID'sine göre geri yükler
        /// </summary>
        public virtual async Task<bool> RestoreByIdAsync(object id)
        {
            if (id == null)
            {
                _logger.LogWarning("RestoreByIdAsync: ID null olamaz");
                return false;
            }
            
            try
            {
                // Entity'yi getir (silinmiş kayıtları da getirebilmek için DbContext'i kullan)
                var entity = await _context.Set<TEntity>().FindAsync(id);
                
                if (entity == null)
                {
                    _logger.LogWarning($"RestoreByIdAsync: {id} ID'li entity bulunamadı");
                    return false;
                }
                
                // Geri yükle
                return await RestoreAsync(entity);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"RestoreByIdAsync hatası: {ex.Message}");
                return false;
            }
        }
        
        /// <summary>
        /// Entity'nin ilişkili kayıtları olup olmadığını kontrol eder
        /// Bu metot, her entity tipi için özel olarak uygulanmalıdır
        /// </summary>
        public virtual async Task<bool> HasRelatedRecordsAsync(object id)
        {
            // Varsayılan olarak false döndür
            // Bu metot, türetilmiş sınıflarda uygulanmalıdır
            return false;
        }
        
        /// <summary>
        /// Entity'nin ID'sini alır
        /// </summary>
        protected virtual object GetEntityId(TEntity entity)
        {
            // Entity'nin ID özelliğini bul
            var idProperty = entity.GetType().GetProperty("Id") ?? 
                             entity.GetType().GetProperty($"{entity.GetType().Name}ID") ??
                             entity.GetType().GetProperty($"{entity.GetType().Name}Id");
                             
            if (idProperty != null)
            {
                return idProperty.GetValue(entity);
            }
            
            return "Bilinmeyen ID";
        }
    }
} 