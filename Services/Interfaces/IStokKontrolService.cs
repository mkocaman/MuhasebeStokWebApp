using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using MuhasebeStokWebApp.Data.Entities;

namespace MuhasebeStokWebApp.Services.Interfaces
{
    public interface IStokKontrolService
    {
        /// <summary>
        /// Tüm ürünlerin stok durumunu kontrol eder ve kritik seviyenin altındakiler için uyarı oluşturur
        /// </summary>
        Task KritikStokKontroluYapAsync();

        /// <summary>
        /// Kritik stok seviyesinin altında olan ürünleri listeler
        /// </summary>
        Task<List<Urun>> KritikSeviyedeUrunleriGetirAsync();

        /// <summary>
        /// Belirli bir ürünün stok seviyesini kontrol eder
        /// </summary>
        Task<bool> UrunStokSeviyesiKontrolEtAsync(Guid urunID);

        /// <summary>
        /// Kritik stok seviyesini döndürür
        /// </summary>
        decimal GetKritikStokSeviyesi();
    }
} 