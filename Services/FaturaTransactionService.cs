using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using MuhasebeStokWebApp.Data.Repositories;
using MuhasebeStokWebApp.Services.Interfaces;

namespace MuhasebeStokWebApp.Services
{
    /// <summary>
    /// Fatura işlemleri için transaction yönetimini sağlar
    /// </summary>
    public class FaturaTransactionService : IFaturaTransactionService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<FaturaTransactionService> _logger;
        private readonly IExceptionHandlingService _exceptionHandler;
        private bool _transactionActive = false;

        public FaturaTransactionService(
            IUnitOfWork unitOfWork,
            ILogger<FaturaTransactionService> logger,
            IExceptionHandlingService exceptionHandler)
        {
            _unitOfWork = unitOfWork;
            _logger = logger;
            _exceptionHandler = exceptionHandler;
        }

        /// <summary>
        /// Transaction kontrolü yapar ve gerekirse yeni bir transaction başlatır
        /// </summary>
        public async Task EnsureTransactionAsync()
        {
            // Eğer transaction yoksa, yeni bir transaction başlat
            if (!_transactionActive)
            {
                await _unitOfWork.BeginTransactionAsync();
                _transactionActive = true;
                _logger.LogDebug("Yeni transaction başlatıldı");
            }
        }

        /// <summary>
        /// Transaction'ı commit eder
        /// </summary>
        public async Task CommitTransactionAsync()
        {
            if (_transactionActive)
            {
                await _unitOfWork.CommitTransactionAsync();
                _transactionActive = false;
                _logger.LogDebug("Transaction commit edildi");
            }
        }

        /// <summary>
        /// Transaction'ı rollback eder
        /// </summary>
        public async Task RollbackTransactionAsync()
        {
            if (_transactionActive)
            {
                await _unitOfWork.RollbackTransactionAsync();
                _transactionActive = false;
                _logger.LogDebug("Transaction rollback edildi");
            }
        }

        /// <summary>
        /// Transaction içinde belirtilen işlemi çalıştırır ve sonucu döner
        /// </summary>
        public async Task<T> ExecuteInTransactionAsync<T>(Func<Task<T>> action, string operationName, params object[] parameters)
        {
            return await _exceptionHandler.HandleExceptionAsync(async () =>
            {
                _logger.LogInformation("{OperationName} işlemi başlatılıyor", operationName);
                
                // Eğer dışarıdan başlatılmış bir transaction varsa, onun içinde çalış
                if (_transactionActive)
                {
                    _logger.LogDebug("{OperationName} için mevcut transaction kullanılıyor", operationName);
                    return await action();
                }
                
                // Kendi transaction'ını başlat
                await EnsureTransactionAsync();
                try
                {
                    var result = await action();
                    await CommitTransactionAsync();
                    _logger.LogInformation("{OperationName} işlemi başarıyla tamamlandı", operationName);
                    return result;
                }
                catch (Exception ex)
                {
                    await RollbackTransactionAsync();
                    _logger.LogError(ex, "{OperationName} işlemi sırasında hata oluştu: {Message}", operationName, ex.Message);
                    throw;
                }
            }, $"FaturaTransactionService.{operationName}", parameters);
        }

        /// <summary>
        /// Transaction içinde belirtilen işlemi çalıştırır (void dönüş)
        /// </summary>
        public async Task ExecuteInTransactionAsync(Func<Task> action, string operationName, params object[] parameters)
        {
            await _exceptionHandler.HandleExceptionAsync(async () =>
            {
                _logger.LogInformation("{OperationName} işlemi başlatılıyor", operationName);
                
                // Eğer dışarıdan başlatılmış bir transaction varsa, onun içinde çalış
                if (_transactionActive)
                {
                    _logger.LogDebug("{OperationName} için mevcut transaction kullanılıyor", operationName);
                    await action();
                    return;
                }
                
                // Kendi transaction'ını başlat
                await EnsureTransactionAsync();
                try
                {
                    await action();
                    await CommitTransactionAsync();
                    _logger.LogInformation("{OperationName} işlemi başarıyla tamamlandı", operationName);
                }
                catch (Exception ex)
                {
                    await RollbackTransactionAsync();
                    _logger.LogError(ex, "{OperationName} işlemi sırasında hata oluştu: {Message}", operationName, ex.Message);
                    throw;
                }
            }, $"FaturaTransactionService.{operationName}", parameters);
        }
    }
} 