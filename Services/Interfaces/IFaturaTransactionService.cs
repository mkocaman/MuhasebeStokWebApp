using System;
using System.Threading.Tasks;

namespace MuhasebeStokWebApp.Services.Interfaces
{
    /// <summary>
    /// Fatura işlemleri için transaction yönetimini sağlar
    /// </summary>
    public interface IFaturaTransactionService
    {
        /// <summary>
        /// Transaction kontrolü yapar ve gerekirse yeni bir transaction başlatır
        /// </summary>
        Task EnsureTransactionAsync();
        
        /// <summary>
        /// Transaction'ı commit eder
        /// </summary>
        Task CommitTransactionAsync();
        
        /// <summary>
        /// Transaction'ı rollback eder
        /// </summary>
        Task RollbackTransactionAsync();
        
        /// <summary>
        /// Transaction içinde belirtilen işlemi çalıştırır ve sonucu döner
        /// </summary>
        /// <typeparam name="T">Dönüş tipi</typeparam>
        /// <param name="action">Çalıştırılacak işlem</param>
        /// <param name="operationName">İşlem adı (loglama için)</param>
        /// <param name="parameters">İşlem parametreleri (loglama için)</param>
        /// <returns>İşlem sonucu</returns>
        Task<T> ExecuteInTransactionAsync<T>(Func<Task<T>> action, string operationName, params object[] parameters);
        
        /// <summary>
        /// Transaction içinde belirtilen işlemi çalıştırır (void dönüş)
        /// </summary>
        /// <param name="action">Çalıştırılacak işlem</param>
        /// <param name="operationName">İşlem adı (loglama için)</param>
        /// <param name="parameters">İşlem parametreleri (loglama için)</param>
        Task ExecuteInTransactionAsync(Func<Task> action, string operationName, params object[] parameters);
    }
} 