using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using MuhasebeStokWebApp.Data.Entities;
using MuhasebeStokWebApp.Enums;
using MuhasebeStokWebApp.Models;

namespace MuhasebeStokWebApp.Services.Interfaces
{
    /// <summary>
    /// Stok çıkış işlemlerini yöneten servis arayüzü
    /// </summary>
    public interface IStokCikisService
    {
        /// <summary>
        /// Basit stok çıkış işlemi yapar
        /// </summary>
        /// <param name="urunId">Ürün ID</param>
        /// <param name="miktar">Çıkış miktarı</param>
        /// <param name="islemTuru">İşlem türü</param>
        /// <param name="islemId">İşlem ID</param>
        /// <returns>İşlem başarılı ise true, değilse false</returns>
        Task<bool> StokCikisAsync(Guid urunId, decimal miktar, string islemTuru, Guid islemId);
        
        /// <summary>
        /// Detaylı stok çıkış işlemi yapar
        /// </summary>
        /// <param name="urunID">Ürün ID</param>
        /// <param name="miktar">Çıkış miktarı</param>
        /// <param name="hareketTipi">Stok hareket tipi</param>
        /// <param name="referansID">Referans ID (Fatura ID vb.)</param>
        /// <param name="aciklama">Açıklama</param>
        /// <param name="paraBirimi">Para birimi (varsayılan: USD)</param>
        /// <param name="useBatch">Toplu işlem mi? (varsayılan: false)</param>
        /// <param name="dovizKuru">Döviz kuru</param>
        /// <returns>Stok çıkış bilgisi</returns>
        Task<StokCikisInfo> StokCikisiYap(
            Guid urunID, 
            decimal miktar, 
            StokHareketTipi hareketTipi, 
            Guid? referansID = null, 
            string aciklama = null, 
            string paraBirimi = "USD", 
            bool useBatch = false,
            decimal? dovizKuru = null);
        
        /// <summary>
        /// Alternatif stok çıkış işlemi yapar
        /// </summary>
        /// <param name="urunID">Ürün ID</param>
        /// <param name="miktar">Çıkış miktarı</param>
        /// <param name="referansNo">Referans numarası</param>
        /// <param name="referansTuru">Referans türü</param>
        /// <param name="referansID">Referans ID</param>
        /// <param name="aciklama">Açıklama</param>
        /// <returns>Kullanılan FIFO kayıtları ve toplam maliyet</returns>
        Task<(List<StokFifo> KullanilanFifoKayitlari, decimal ToplamMaliyet)> StokCikisiYap(
            Guid urunID, 
            decimal miktar, 
            string referansNo, 
            string referansTuru, 
            Guid? referansID, 
            string aciklama);
            
        /// <summary>
        /// FIFO kayıtlarını iptal eder
        /// </summary>
        /// <param name="referansID">Referans ID</param>
        /// <param name="referansTuru">Referans türü</param>
        /// <param name="iptalAciklama">İptal açıklaması</param>
        /// <param name="iptalEdenKullaniciID">İptal eden kullanıcı ID</param>
        /// <returns>İptal edilen FIFO kayıtları</returns>
        Task<List<StokFifo>> FifoKayitlariniIptalEt(
            Guid referansID, 
            string referansTuru, 
            string iptalAciklama, 
            Guid? iptalEdenKullaniciID = null);
    }
} 