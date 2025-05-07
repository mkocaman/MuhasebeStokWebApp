using System;
using System.Threading.Tasks;

namespace MuhasebeStokWebApp.Services.Interfaces
{
    /// <summary>
    /// Cari kodu oluşturma işlemlerini yönetir
    /// </summary>
    public interface ICariNumaralandirmaService
    {
        /// <summary>
        /// Yeni cari kodu oluşturur
        /// </summary>
        /// <returns>Oluşturulan cari kodu</returns>
        Task<string> GenerateCariKoduAsync();
        
        /// <summary>
        /// Verilen prefix'e göre benzersiz bir cari kodu oluşturur
        /// </summary>
        /// <param name="prefix">Numara öneki</param>
        /// <param name="basamakSayisi">Sıra numarası basamak sayısı</param>
        /// <returns>Oluşturulan benzersiz numara</returns>
        Task<string> GenerateUniqueNumaraAsync(string prefix, int basamakSayisi = 3);
    }
} 