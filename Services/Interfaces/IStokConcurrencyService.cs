using System;
using System.Threading.Tasks;
using MuhasebeStokWebApp.Data.Entities;

namespace MuhasebeStokWebApp.Services.Interfaces
{
    /// <summary>
    /// Stok eşzamanlılık (concurrency) yönetimini sağlayan servis arayüzü
    /// </summary>
    public interface IStokConcurrencyService
    {
        /// <summary>
        /// FIFO kaydı için retry mekanizması ile işlem yapar ve kaydı döndürür
        /// </summary>
        /// <param name="fifoEntry">FIFO kaydı</param>
        /// <param name="processAction">İşlem fonksiyonu</param>
        /// <param name="maxRetries">Maksimum deneme sayısı (varsayılan: 3)</param>
        /// <returns>İşlenmiş FIFO kaydı</returns>
        Task<StokFifo> ProcessFifoEntryWithRetryAndReturn(StokFifo fifoEntry, Func<StokFifo, Task> processAction, int maxRetries = 3);
        
        /// <summary>
        /// FIFO kaydı için retry mekanizması ile işlem yapar
        /// </summary>
        /// <param name="fifoEntry">FIFO kaydı</param>
        /// <param name="processAction">İşlem fonksiyonu</param>
        /// <param name="maxRetries">Maksimum deneme sayısı (varsayılan: 3)</param>
        /// <returns>İşlem başarılı ise true, değilse false</returns>
        Task<bool> ProcessFifoEntryWithRetry(StokFifo fifoEntry, Func<StokFifo, Task> processAction, int maxRetries = 3);
    }
} 