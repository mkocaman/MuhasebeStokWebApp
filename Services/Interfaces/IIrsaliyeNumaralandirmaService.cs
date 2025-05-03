using System;
using System.Threading.Tasks;

namespace MuhasebeStokWebApp.Services.Interfaces
{
    /// <summary>
    /// İrsaliye numarası oluşturma işlemlerini yönetir
    /// </summary>
    public interface IIrsaliyeNumaralandirmaService
    {
        /// <summary>
        /// Yeni irsaliye numarası oluşturur
        /// </summary>
        /// <returns>Oluşturulan irsaliye numarası</returns>
        Task<string> GenerateIrsaliyeNumarasiAsync();
        
        /// <summary>
        /// Verilen prefix'e göre benzersiz bir irsaliye numarası oluşturur
        /// </summary>
        /// <param name="prefix">Numara öneki</param>
        /// <param name="basamakSayisi">Sıra numarası basamak sayısı</param>
        /// <returns>Oluşturulan benzersiz numara</returns>
        Task<string> GenerateUniqueNumaraAsync(string prefix, int basamakSayisi = 4);
    }
} 