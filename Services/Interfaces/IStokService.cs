using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using MuhasebeStokWebApp.Data.Entities;
using MuhasebeStokWebApp.Enums;
using MuhasebeStokWebApp.ViewModels;
using MuhasebeStokWebApp.ViewModels.Stok;

namespace MuhasebeStokWebApp.Services.Interfaces
{
    public interface IStokService
    {
        /// <summary>
        /// Bir ürünün dinamik stok miktarını hesaplar. Bu metot ürün için yapılan giriş ve çıkış hareketlerini
        /// toplayarak dinamik stok miktarını hesaplar. StokMiktar Urun sınıfında statik olarak tutulmaz.
        /// </summary>
        /// <param name="urunID">Stok miktarı hesaplanacak ürünün ID'si</param>
        /// <param name="depoID">İsteğe bağlı depo filtresi. Eğer belirtilirse, sadece o depodaki stok hesaplanır</param>
        /// <returns>Ürünün dinamik stok miktarı</returns>
        Task<decimal> GetDinamikStokMiktari(Guid urunID, Guid? depoID = null);
        
        /// <summary>
        /// Tüm ürünlerin stok durumunu döndürür
        /// </summary>
        Task<IEnumerable<StokViewModel>> GetAllStokDurumu();
        
        /// <summary>
        /// ID'ye göre stok hareketini döndürür
        /// </summary>
        Task<StokHareket> GetStokHareketByIdAsync(Guid id);
        
        /// <summary>
        /// Belirli tarih aralığında bir ürünün stok giriş-çıkış hareketlerini döndürür
        /// </summary>
        Task<List<StokHareket>> GetStokGirisCikisAsync(Guid urunId, DateTime baslangicTarihi, DateTime bitisTarihi);
        
        /// <summary>
        /// Manuel stok girişi yapar
        /// </summary>
        Task<bool> StokGirisYapAsync(StokGirisViewModel viewModel);
        
        /// <summary>
        /// Manuel stok çıkışı yapar
        /// </summary>
        Task<bool> StokCikisYapAsync(StokCikisViewModel viewModel);
        
        /// <summary>
        /// Stok transferi yapar
        /// </summary>
        Task<bool> StokTransferYapAsync(StokTransferViewModel viewModel);
        
        /// <summary>
        /// Stok sayımı yapar
        /// </summary>
        Task<bool> StokSayimiYapAsync(StokSayimViewModel viewModel);
        
        /// <summary>
        /// Para birimi koduna göre sembol döndürür
        /// </summary>
        string GetParaBirimiSembol(string paraBirimiKodu);
    }
} 