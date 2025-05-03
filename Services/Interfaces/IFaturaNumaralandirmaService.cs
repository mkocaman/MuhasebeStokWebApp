using System;
using System.Threading.Tasks;

namespace MuhasebeStokWebApp.Services.Interfaces
{
    /// <summary>
    /// Fatura ve sipariş numarası oluşturma işlemlerini yönetir
    /// </summary>
    public interface IFaturaNumaralandirmaService
    {
        /// <summary>
        /// Yeni fatura numarası oluşturur
        /// </summary>
        /// <returns>Oluşturulan fatura numarası</returns>
        Task<string> GenerateFaturaNumarasiAsync();
        
        /// <summary>
        /// Yeni sipariş numarası oluşturur
        /// </summary>
        /// <returns>Oluşturulan sipariş numarası</returns>
        Task<string> GenerateSiparisNumarasiAsync();
        
        /// <summary>
        /// Verilen prefix'e göre benzersiz bir fatura numarası oluşturur
        /// </summary>
        /// <param name="prefix">Numara öneki</param>
        /// <param name="basamakSayisi">Sıra numarası basamak sayısı</param>
        /// <returns>Oluşturulan benzersiz numara</returns>
        Task<string> GenerateUniqueNumaraAsync(string prefix, int basamakSayisi = 3);
    }
} 