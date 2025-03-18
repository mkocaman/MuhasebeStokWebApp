using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;

namespace MuhasebeStokWebApp.ViewModels.Rapor
{
    public class RaporViewModel
    {
        public string RaporAdi { get; set; }
        public string Aciklama { get; set; }
        public DateTime RaporTarihi { get; set; } = DateTime.Now;
        public string KullaniciAdi { get; set; }
    }

    public class RaporFiltreViewModel
    {
        [Display(Name = "Başlangıç Tarihi")]
        [DataType(DataType.Date)]
        public DateTime BaslangicTarihi { get; set; } = DateTime.Today.AddMonths(-1);

        [Display(Name = "Bitiş Tarihi")]
        [DataType(DataType.Date)]
        public DateTime BitisTarihi { get; set; } = DateTime.Today;

        [Display(Name = "Kasa")]
        public Guid? KasaID { get; set; }

        [Display(Name = "Banka")]
        public Guid? BankaID { get; set; }

        [Display(Name = "Cari")]
        public Guid? CariID { get; set; }

        [Display(Name = "Ürün")]
        public Guid? UrunID { get; set; }

        [Display(Name = "Hareket Türü")]
        public string HareketTuru { get; set; }

        [Display(Name = "Rapor Türü")]
        public string RaporTuru { get; set; }

        [Display(Name = "Detay Göster")]
        public bool DetayGoster { get; set; } = true;
    }

    public class KasaRaporViewModel : RaporViewModel
    {
        public List<KasaHareketRaporViewModel> KasaHareketleri { get; set; } = new List<KasaHareketRaporViewModel>();
        public decimal ToplamGiris { get; set; }
        public decimal ToplamCikis { get; set; }
        public decimal NetBakiye => ToplamGiris - ToplamCikis;
    }

    public class KasaHareketRaporViewModel
    {
        public string KasaAdi { get; set; }
        public DateTime Tarih { get; set; }
        public string HareketTuru { get; set; }
        public decimal Tutar { get; set; }
        public string ReferansNo { get; set; }
        public string ReferansTuru { get; set; }
        public string Aciklama { get; set; }
    }

    public class BankaRaporViewModel : RaporViewModel
    {
        public List<BankaHareketRaporViewModel> BankaHareketleri { get; set; } = new List<BankaHareketRaporViewModel>();
        public decimal ToplamGiris { get; set; }
        public decimal ToplamCikis { get; set; }
        public decimal NetBakiye => ToplamGiris - ToplamCikis;
        public Dictionary<string, decimal> ParaBirimiToplamlari { get; set; } = new Dictionary<string, decimal>();
    }

    public class BankaHareketRaporViewModel
    {
        public string BankaAdi { get; set; }
        public DateTime Tarih { get; set; }
        public string HareketTuru { get; set; }
        public decimal Tutar { get; set; }
        public string ParaBirimi { get; set; }
        public string ReferansNo { get; set; }
        public string ReferansTuru { get; set; }
        public string DekontNo { get; set; }
        public string Aciklama { get; set; }
    }

    public class CariRaporViewModel : RaporViewModel
    {
        public List<CariHareketRaporViewModel> CariHareketleri { get; set; } = new List<CariHareketRaporViewModel>();
        public decimal ToplamBorc { get; set; }
        public decimal ToplamAlacak { get; set; }
        public decimal Bakiye => ToplamAlacak - ToplamBorc;
    }

    public class CariHareketRaporViewModel
    {
        public string CariAdi { get; set; }
        public DateTime Tarih { get; set; }
        public string HareketTuru { get; set; }
        public decimal Tutar { get; set; }
        public string ReferansNo { get; set; }
        public string ReferansTuru { get; set; }
        public string Aciklama { get; set; }
    }

    public class StokRaporViewModel : RaporViewModel
    {
        public List<StokHareketRaporViewModel> StokHareketleri { get; set; } = new List<StokHareketRaporViewModel>();
        public decimal ToplamGiris { get; set; }
        public decimal ToplamCikis { get; set; }
        public decimal MevcutStok => ToplamGiris - ToplamCikis;
    }

    public class StokHareketRaporViewModel
    {
        public string UrunAdi { get; set; }
        public DateTime Tarih { get; set; }
        public string HareketTuru { get; set; }
        public decimal Miktar { get; set; }
        public string Birim { get; set; }
        public decimal BirimFiyat { get; set; }
        public decimal ToplamTutar { get; set; }
        public string ReferansNo { get; set; }
        public string ReferansTuru { get; set; }
        public string Aciklama { get; set; }
    }

    public class SatisRaporViewModel : RaporViewModel
    {
        public List<SatisDetayRaporViewModel> SatisDetaylari { get; set; } = new List<SatisDetayRaporViewModel>();
        public decimal ToplamSatisTutari { get; set; }
        public decimal ToplamKdvTutari { get; set; }
        public decimal ToplamIndirimTutari { get; set; }
        public decimal NetSatisTutari => ToplamSatisTutari + ToplamKdvTutari - ToplamIndirimTutari;
    }

    public class SatisDetayRaporViewModel
    {
        public DateTime Tarih { get; set; }
        public string FaturaNo { get; set; }
        public string CariAdi { get; set; }
        public string UrunAdi { get; set; }
        public decimal Miktar { get; set; }
        public string Birim { get; set; }
        public decimal BirimFiyat { get; set; }
        public decimal KdvOrani { get; set; }
        public decimal KdvTutari { get; set; }
        public decimal IndirimOrani { get; set; }
        public decimal IndirimTutari { get; set; }
        public decimal ToplamTutar { get; set; }
    }

    public class OzetRaporViewModel : RaporViewModel
    {
        public decimal ToplamSatis { get; set; }
        public decimal ToplamAlis { get; set; }
        public decimal KarZarar => ToplamSatis - ToplamAlis;
        public decimal ToplamTahsilat { get; set; }
        public decimal ToplamOdeme { get; set; }
        public decimal NakitDurumu => ToplamTahsilat - ToplamOdeme;
        public decimal ToplamCariAlacak { get; set; }
        public decimal ToplamCariBorclar { get; set; }
        public decimal CariDurum => ToplamCariAlacak - ToplamCariBorclar;
        public List<AylikOzetViewModel> AylikOzetler { get; set; } = new List<AylikOzetViewModel>();
    }

    public class AylikOzetViewModel
    {
        public string Ay { get; set; }
        public decimal Satis { get; set; }
        public decimal Alis { get; set; }
        public decimal KarZarar => Satis - Alis;
        public decimal Tahsilat { get; set; }
        public decimal Odeme { get; set; }
        public decimal NakitDurumu => Tahsilat - Odeme;
    }
} 