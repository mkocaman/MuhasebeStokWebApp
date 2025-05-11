using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using MuhasebeStokWebApp.Data.Entities;
using MuhasebeStokWebApp.Enums;
using MuhasebeStokWebApp.ViewModels;

namespace MuhasebeStokWebApp.Interfaces
{
    public interface IStokFifoService
    {
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

        Task<StokCikisInfo> StokCikisiYap(
            Guid urunID, 
            decimal miktar, 
            StokHareketTipi hareketTipi, 
            Guid? referansID = null, 
            string aciklama = null, 
            string paraBirimi = "USD", 
            bool useBatch = false,
            decimal? dovizKuru = null);

        Task<StokFifo> StokGirisi(
            Guid urunID, 
            decimal miktar, 
            decimal birimFiyat,
            Guid? referansID, 
            Guid? detayID,
            string referansNo, 
            string referansTuru, 
            string paraBirimi = "USD", 
            decimal? dovizKuru = null,
            Guid? currentUserId = null);

        Task StokCikisi(
            Guid urunID, 
            decimal miktar,
            Guid? referansID, 
            Guid? detayID,
            string referansNo, 
            string referansTuru, 
            string paraBirimi = "USD", 
            decimal? dovizKuru = null,
            Guid? currentUserId = null);
            
        Task<List<StokFifo>> FifoKayitlariniIptalEt(Guid referansID, string referansTuru, string iptalAciklama, Guid? iptalEdenKullaniciID = null);
    }
} 