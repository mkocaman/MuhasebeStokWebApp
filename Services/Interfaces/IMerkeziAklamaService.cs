using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using MuhasebeStokWebApp.Data.Entities;
using MuhasebeStokWebApp.Enums;
using MuhasebeStokWebApp.ViewModels;
using MuhasebeStokWebApp.ViewModels.Aklama;

namespace MuhasebeStokWebApp.Services.Interfaces
{
    public interface IMerkeziAklamaService
    {
        Task<List<AklamaKuyrukViewModel>> GetBekleyenAklamaKayitlariAsync(int? page = null, int? pageSize = null, Guid? urunId = null);
        Task<List<AklamaKuyrukViewModel>> GetAklanmisKayitlarAsync(int? page = null, int? pageSize = null, Guid? urunId = null);
        
        Task<bool> FaturaKaleminiAklamaKuyrugunaEkleAsync(Guid faturaKalemId);
        Task<bool> ManuelAklamaKaydiOlusturAsync(ManuelAklamaViewModel model);
        
        Task<AklamaOzetiViewModel> GetAklamaOzetiAsync(Guid? urunId = null, DateTime? baslangicTarihi = null, DateTime? bitisTarihi = null);
        Task<List<UrunAklamaDurumuViewModel>> GetTumUrunlerinAklamaDurumuAsync();
        Task<UrunAklamaGecmisiViewModel> GetUrunAklamaGecmisiAsync(Guid urunId);
        
        Task<bool> AklamaKaydiSilAsync(Guid aklamaId);
        Task<bool> AklamaKaydiTamamlaAsync(Guid aklamaId, string aciklama);
        Task<bool> AklamaKaydiDurdurAsync(Guid aklamaId);
        Task<bool> AklamaKaydiAktifEtAsync(Guid aklamaId);
        
        Task<List<AklamaKuyrukViewModel>> GetFaturaninAklamaKayitlariAsync(Guid faturaId);
        
        Task<bool> FaturayiAklamaKuyrugunaEkleAsync(Guid faturaId);
        Task<bool> OtomatikAklamaYapAsync(Guid resmiFaturaId);
        Task<bool> ManuelAklamaYapAsync(Guid resmiFaturaKalemId, List<Guid> secilenKayitIdleri, decimal toplamMiktar, string aklamaNotu);
        Task<bool> AklamaIptalEtAsync(Guid aklamaId);
        Task<bool> ResmiFaturaIptalAsync(Guid resmiFaturaId);
    }
} 