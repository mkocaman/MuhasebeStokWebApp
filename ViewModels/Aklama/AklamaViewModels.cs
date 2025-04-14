using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using MuhasebeStokWebApp.Data.Entities;
using MuhasebeStokWebApp.Enums;

namespace MuhasebeStokWebApp.ViewModels.Aklama
{
    public class AklamaKuyrukViewModel
    {
        public Guid AklamaID { get; set; }
        public Guid FaturaID { get; set; }
        public Guid FaturaDetayID { get; set; }
        public string FaturaNo { get; set; } = "";
        public string CariAdi { get; set; } = "";
        public Guid UrunID { get; set; }
        public string UrunKodu { get; set; } = "";
        public string UrunAdi { get; set; } = "";
        public string BirimAdi { get; set; } = "";
        public DateTime FaturaTarihi { get; set; }
        public int SiraNo { get; set; }
        public decimal Miktar { get; set; }
        public decimal KalanMiktar { get; set; }
        public decimal BirimFiyat { get; set; }
        public string ParaBirimi { get; set; } = "";
        public AklamaDurumu Durum { get; set; }
        public string DurumAdi => Durum.ToString();
        public DateTime EklenmeTarihi { get; set; }
        public DateTime? AklanmaTarihi { get; set; }
        public string Aciklama { get; set; } = "";
    }

    public class AklamaOzetiViewModel
    {
        public int BekleyenKayitSayisi { get; set; }
        public int AklanmisKayitSayisi { get; set; }
        public decimal ToplamBekleyenMiktar { get; set; }
        public decimal ToplamAklanmisMiktar { get; set; }
        public DateTime? SonAklamaTarihi { get; set; }
        public List<UrunAklamaDurumuViewModel> UrunDurumlari { get; set; }
        public List<AylikAklamaDurumuViewModel> AylikIstatistikler { get; set; }

        public AklamaOzetiViewModel()
        {
            UrunDurumlari = new List<UrunAklamaDurumuViewModel>();
            AylikIstatistikler = new List<AylikAklamaDurumuViewModel>();
        }
    }

    public class UrunAklamaDurumuViewModel
    {
        public Guid UrunID { get; set; }
        public string UrunKodu { get; set; } = "";
        public string UrunAdi { get; set; } = "";
        public string BirimAdi { get; set; } = "";
        public decimal BekleyenMiktar { get; set; }
        public decimal AklanmisMiktar { get; set; }
        public decimal AklanmaOrani => (AklanmisMiktar + BekleyenMiktar) > 0 
            ? Math.Round((AklanmisMiktar / (AklanmisMiktar + BekleyenMiktar)) * 100, 2) 
            : 0;
        public DateTime? SonIslemTarihi { get; set; }
    }

    public class AylikAklamaDurumuViewModel
    {
        public int Yil { get; set; }
        public int Ay { get; set; }
        public string AyYil => $"{Ay:00}/{Yil}";
        public int EklenenKayitSayisi { get; set; }
        public int AklananKayitSayisi { get; set; }
        public decimal EklenenMiktar { get; set; }
        public decimal AklananMiktar { get; set; }
    }

    public class UrunAklamaGecmisiViewModel
    {
        public Guid UrunID { get; set; }
        public string UrunKodu { get; set; } = "";
        public string UrunAdi { get; set; } = "";
        public string BirimAdi { get; set; } = "";
        public List<AklamaKuyrukViewModel> BekleyenKayitlar { get; set; }
        public List<AklamaKuyrukViewModel> AklanmisKayitlar { get; set; }

        public UrunAklamaGecmisiViewModel()
        {
            BekleyenKayitlar = new List<AklamaKuyrukViewModel>();
            AklanmisKayitlar = new List<AklamaKuyrukViewModel>();
        }
    }
    
    public class ManuelAklamaViewModel
    {
        public Guid ResmiFaturaKalemID { get; set; }
        public string ResmiFaturaNo { get; set; } = "";
        public string UrunAdi { get; set; } = "";
        public string UrunKodu { get; set; } = "";
        public decimal ResmiFaturaMiktar { get; set; }
        
        [Display(Name = "Aklama Notu")]
        public string AklamaNotu { get; set; } = "";
        
        public List<BekleyenAklamaViewModel> BekleyenAklamaKayitlari { get; set; }
        
        public ManuelAklamaViewModel()
        {
            BekleyenAklamaKayitlari = new List<BekleyenAklamaViewModel>();
        }
    }
    
    public class BekleyenAklamaViewModel
    {
        public Guid AklamaID { get; set; }
        public string UrunKodu { get; set; } = "";
        public string UrunAdi { get; set; } = "";
        public string FaturaNo { get; set; } = "";
        public DateTime FaturaTarihi { get; set; }
        public decimal Miktar { get; set; }
        public decimal KalanMiktar { get; set; }
        public string BirimAdi { get; set; } = "";
        public decimal BirimFiyat { get; set; }
        public string ParaBirimi { get; set; } = "";
        
        [Display(Name = "Seç")]
        public bool Secildi { get; set; }
        
        [Display(Name = "Aklanacak Miktar")]
        [Range(0.01, 9999999.99, ErrorMessage = "Miktar 0'dan büyük olmalıdır.")]
        public decimal AklanacakMiktar { get; set; }
    }
} 