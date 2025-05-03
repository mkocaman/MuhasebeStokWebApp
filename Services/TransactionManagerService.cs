using System;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.Logging;
using System.Transactions;

namespace MuhasebeStokWebApp.Services
{
    /// <summary>
    /// Transaction yönetimi için merkezi servis
    /// </summary>
    public class TransactionManagerService : ITransactionManagerService
    {
        private readonly ILogger<TransactionManagerService> _logger;
        private readonly IExceptionHandlingService _exceptionHandler;

        public TransactionManagerService(
            ILogger<TransactionManagerService> logger,
            IExceptionHandlingService exceptionHandler)
        {
            _logger = logger;
            _exceptionHandler = exceptionHandler;
        }

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
        public async Task<TResult> ExecuteInTransactionAsync<TContext, TResult>(
            TContext dbContext,
            Func<TContext, IDbContextTransaction, Task<TResult>> action,
            string operationName,
            params object[] parameters) where TContext : DbContext
        {
            return await _exceptionHandler.HandleExceptionAsync(async () =>
            {
                using var transaction = await dbContext.Database.BeginTransactionAsync();
                try
                {
                    var result = await action(dbContext, transaction);
                    await transaction.CommitAsync();
                    return result;
                }
                catch
                {
                    await transaction.RollbackAsync();
                    throw;
                }
            }, $"TransactionalOperation.{operationName}", parameters);
        }

        /// <summary>
        /// Bir DbContext üzerinde transactional işlem gerçekleştirir (void versiyonu)
        /// </summary>
        /// <typeparam name="TContext">DbContext tipi</typeparam>
        /// <param name="dbContext">İşlem yapılacak DbContext</param>
        /// <param name="action">Gerçekleştirilecek işlem</param>
        /// <param name="operationName">İşlemin adı (loglama için)</param>
        /// <param name="parameters">İşlem parametreleri (loglama için)</param>
        public async Task ExecuteInTransactionAsync<TContext>(
            TContext dbContext,
            Func<TContext, IDbContextTransaction, Task> action,
            string operationName,
            params object[] parameters) where TContext : DbContext
        {
            await _exceptionHandler.HandleExceptionAsync(async () =>
            {
                using var transaction = await dbContext.Database.BeginTransactionAsync();
                try
                {
                    await action(dbContext, transaction);
                    await transaction.CommitAsync();
                }
                catch
                {
                    await transaction.RollbackAsync();
                    throw;
                }
            }, $"TransactionalOperation.{operationName}", parameters);
        }

        /// <summary>
        /// Distributed transaction kullanarak birden fazla DbContext üzerinde işlem yapar
        /// </summary>
        /// <typeparam name="TResult">İşlem sonucu tipi</typeparam>
        /// <param name="action">Gerçekleştirilecek işlem</param>
        /// <param name="operationName">İşlemin adı (loglama için)</param>
        /// <param name="parameters">İşlem parametreleri (loglama için)</param>
        /// <returns>İşlem sonucu</returns>
        public async Task<TResult> ExecuteInDistributedTransactionAsync<TResult>(
            Func<Task<TResult>> action,
            string operationName,
            params object[] parameters)
        {
            return await _exceptionHandler.HandleExceptionAsync(async () =>
            {
                using var scope = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled);
                try
                {
                    var result = await action();
                    scope.Complete();
                    return result;
                }
                catch
                {
                    // TransactionScope otomatik olarak rollback yapar
                    throw;
                }
            }, $"DistributedTransactionalOperation.{operationName}", parameters);
        }

        /// <summary>
        /// Distributed transaction kullanarak birden fazla DbContext üzerinde işlem yapar (void versiyonu)
        /// </summary>
        /// <param name="action">Gerçekleştirilecek işlem</param>
        /// <param name="operationName">İşlemin adı (loglama için)</param>
        /// <param name="parameters">İşlem parametreleri (loglama için)</param>
        public async Task ExecuteInDistributedTransactionAsync(
            Func<Task> action,
            string operationName,
            params object[] parameters)
        {
            await _exceptionHandler.HandleExceptionAsync(async () =>
            {
                using var scope = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled);
                try
                {
                    await action();
                    scope.Complete();
                }
                catch
                {
                    // TransactionScope otomatik olarak rollback yapar
                    throw;
                }
            }, $"DistributedTransactionalOperation.{operationName}", parameters);
        }
    }
} 