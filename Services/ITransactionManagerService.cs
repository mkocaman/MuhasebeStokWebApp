using System;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;

namespace MuhasebeStokWebApp.Services
{
    /// <summary>
    /// Transaction yönetimi için merkezi servis arayüzü
    /// </summary>
    public interface ITransactionManagerService
    {
        /// <summary>
        /// Bir DbContext üzerinde transactional işlem gerçekleştirir
        /// </summary>
        /// <typeparam name="TContext">DbContext tipi</typeparam>
        /// <typeparam name="TResult">İşlem sonucu tipi</typeparam>
        /// <param name="dbContext">İşlem yapılacak DbContext</param>
        /// <param name="action">Gerçekleştirilecek işlem</param>
        /// <param name="operationName">İşlemin adı (loglama için)</param>
        /// <param name="parameters">İşlem parametreleri (loglama için)</param>
        /// <returns>İşlem sonucu</returns>
        Task<TResult> ExecuteInTransactionAsync<TContext, TResult>(
            TContext dbContext,
            Func<TContext, IDbContextTransaction, Task<TResult>> action,
            string operationName,
            params object[] parameters) where TContext : DbContext;

        /// <summary>
        /// Bir DbContext üzerinde transactional işlem gerçekleştirir (void versiyonu)
        /// </summary>
        /// <typeparam name="TContext">DbContext tipi</typeparam>
        /// <param name="dbContext">İşlem yapılacak DbContext</param>
        /// <param name="action">Gerçekleştirilecek işlem</param>
        /// <param name="operationName">İşlemin adı (loglama için)</param>
        /// <param name="parameters">İşlem parametreleri (loglama için)</param>
        Task ExecuteInTransactionAsync<TContext>(
            TContext dbContext,
            Func<TContext, IDbContextTransaction, Task> action,
            string operationName,
            params object[] parameters) where TContext : DbContext;

        /// <summary>
        /// Distributed transaction kullanarak birden fazla DbContext üzerinde işlem yapar
        /// </summary>
        /// <typeparam name="TResult">İşlem sonucu tipi</typeparam>
        /// <param name="action">Gerçekleştirilecek işlem</param>
        /// <param name="operationName">İşlemin adı (loglama için)</param>
        /// <param name="parameters">İşlem parametreleri (loglama için)</param>
        /// <returns>İşlem sonucu</returns>
        Task<TResult> ExecuteInDistributedTransactionAsync<TResult>(
            Func<Task<TResult>> action,
            string operationName,
            params object[] parameters);

        /// <summary>
        /// Distributed transaction kullanarak birden fazla DbContext üzerinde işlem yapar (void versiyonu)
        /// </summary>
        /// <param name="action">Gerçekleştirilecek işlem</param>
        /// <param name="operationName">İşlemin adı (loglama için)</param>
        /// <param name="parameters">İşlem parametreleri (loglama için)</param>
        Task ExecuteInDistributedTransactionAsync(
            Func<Task> action,
            string operationName,
            params object[] parameters);
    }
} 