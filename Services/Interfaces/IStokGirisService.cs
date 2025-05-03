using System;
using System.Threading.Tasks;
using MuhasebeStokWebApp.Data.Entities;

namespace MuhasebeStokWebApp.Services.Interfaces
{
    /// <summary>
    /// Stok giriş işlemlerini yöneten servis arayüzü
    /// </summary>
    public interface IStokGirisService
    {
        /// <summary>
        /// Basit stok giriş işlemi yapar
        /// </summary>
        /// <param name="stokFifo">FIFO stok giriş kaydı</param>
        /// <returns>İşlem başarılı ise true, değilse false</returns>
        Task<bool> StokGirisAsync(StokFifo stokFifo);
        
        /// <summary>
        /// Detaylı stok giriş işlemi yapar
        /// </summary>
        /// <param name="urunID">Ürün ID</param>
        /// <param name="miktar">Miktar</param>
        /// <param name="birimFiyat">Birim fiyat</param>
        /// <param name="birim">Birim (Adet, Kg, vb.)</param>
        /// <param name="referansNo">Referans numarası (Fatura no vb.)</param>
        /// <param name="referansTuru">Referans türü (Fatura, İrsaliye vb.)</param>
        /// <param name="referansID">Referans ID (Fatura ID vb.)</param>
        /// <param name="aciklama">Açıklama</param>
        /// <param name="paraBirimi">Para birimi (varsayılan: USD)</param>
        /// <param name="dovizKuru">Döviz kuru (belirtilmezse API'den alınır)</param>
        /// <returns>Oluşturulan FIFO stok giriş kaydı</returns>
        Task<StokFifo> StokGirisiYap(
            Guid urunID, 
            decimal miktar, 
            decimal birimFiyat, 
            string birim, 
            string referansNo, 
            string referansTuru, 
            Guid? referansID, 
            string aciklama, 
            string paraBirimi = "USD", 
            decimal? dovizKuru = null);
    }
} 