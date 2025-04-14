using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using MuhasebeStokWebApp.Data.Entities;
using MuhasebeStokWebApp.Enums;

namespace MuhasebeStokWebApp.Services.Interfaces
{
    public interface IStokFifoService
    {
        // Basitleştirilmiş arayüz
        Task<bool> StokGirisAsync(StokFifo stokFifo);
        Task<bool> StokCikisAsync(Guid urunId, decimal miktar, string islemTuru, Guid islemId);
        
        // Mevcut eski metotları muhafaza ediyorum, geçici olarak
        Task<StokFifo> StokGirisiYap(Guid urunID, decimal miktar, decimal birimFiyat, string birim, string referansNo, string referansTuru, Guid? referansID, string aciklama, string paraBirimi = "USD", decimal? dovizKuru = null);
        Task<(List<StokFifo> KullanilanFifoKayitlari, decimal ToplamMaliyet)> StokCikisiYap(Guid urunID, decimal miktar, Guid? referansID = null, StokHareketiTipi hareketTipi = StokHareketiTipi.Cikis);
        Task<(List<StokFifo> KullanilanFifoKayitlari, decimal ToplamMaliyet)> StokCikisiYap(Guid urunID, decimal miktar, string referansNo, string referansTuru, Guid? referansID, string aciklama);
        Task<List<StokFifo>> FifoKayitlariniIptalEt(Guid referansID, string referansTuru, string iptalAciklama, Guid? iptalEdenKullaniciID = null);
        Task<decimal> GetOrtalamaMaliyet(Guid urunID, string paraBirimi = "USD");
        Task<List<StokFifo>> GetAktifFifoKayitlari(Guid urunID);
        Task<List<StokFifo>> GetReferansaGoreFifoKayitlari(Guid referansID, string referansTuru);
        Task<decimal> HesaplaMaliyetAsync(Guid stokId, DateTime tarih, string paraBirimi = "USD");
    }
} 