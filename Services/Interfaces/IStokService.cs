using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using MuhasebeStokWebApp.Data.Entities;

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
    }
} 