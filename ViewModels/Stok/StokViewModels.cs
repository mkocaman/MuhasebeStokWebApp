using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using MuhasebeStokWebApp.Enums;
using MuhasebeStokWebApp.ViewModels.Fatura;

namespace MuhasebeStokWebApp.ViewModels.Stok
{
    // Stok Raporu ViewModel
    public class StokRaporViewModel
    {
        public int ToplamUrunSayisi { get; set; }
        public decimal ToplamStokDegeri { get; set; }
        public int KritikStokUrunSayisi { get; set; }
        public int DusukStokUrunSayisi { get; set; }
        
        public List<StokKartViewModel> Urunler { get; set; } = new List<StokKartViewModel>();
        public List<KategoriBazliRaporViewModel> KategoriBazliRapor { get; set; } = new List<KategoriBazliRaporViewModel>();
        public List<DepoBazliRaporViewModel> DepoBazliRapor { get; set; } = new List<DepoBazliRaporViewModel>();
    }
    
    public class KategoriBazliRaporViewModel
    {
        public Guid KategoriID { get; set; }
        public string KategoriAdi { get; set; }
        public int UrunSayisi { get; set; }
        public decimal ToplamStokDegeri { get; set; }
        public int KritikStokUrunSayisi { get; set; }
        public int DusukStokUrunSayisi { get; set; }
    }
    
    public class DepoBazliRaporViewModel
    {
        public Guid DepoID { get; set; }
        public string DepoAdi { get; set; }
        public int UrunSayisi { get; set; }
        public decimal ToplamStokDegeri { get; set; }
        public int KritikStokUrunSayisi { get; set; }
        public int DusukStokUrunSayisi { get; set; }
    }

    // Stok Listesi ViewModel
    public class StokListViewModel
    {
        public List<StokKartViewModel> StokKartlari { get; set; } = new List<StokKartViewModel>();
        public List<StokKartViewModel> Urunler { get; set; } = new List<StokKartViewModel>();
        public List<StokKartViewModel> Stoklar { get; set; } = new List<StokKartViewModel>();
    }

    // Stok Kartı ViewModel
    public class StokKartViewModel
    {
        public Guid UrunID { get; set; }
        
        [Display(Name = "Ürün Kodu")]
        public string UrunKodu { get; set; }
        
        [Display(Name = "Ürün Adı")]
        public string UrunAdi { get; set; }
        
        [Display(Name = "Birim")]
        public string Birim { get; set; }
        
        [Display(Name = "Kategori")]
        public string Kategori { get; set; }
        
        [Display(Name = "Stok Miktarı")]
        [DisplayFormat(DataFormatString = "{0:N2}", ApplyFormatInEditMode = false)]
        public decimal StokMiktari { get; set; }
        
        [Display(Name = "Stok Miktarı")]
        [DisplayFormat(DataFormatString = "{0:N2}", ApplyFormatInEditMode = false)]
        public decimal StokMiktar { get; set; }
        
        [Display(Name = "Birim Fiyat")]
        [DisplayFormat(DataFormatString = "{0:N2}", ApplyFormatInEditMode = false)]
        public decimal BirimFiyat { get; set; }
        
        public Guid? BirimID { get; set; }
        
        public Guid? KategoriID { get; set; }

        [Display(Name = "Depo Adı")]
        public string DepoAdi { get; set; } = string.Empty;

        [Display(Name = "Kritik Stok Seviyesi")]
        public decimal KritikStokSeviyesi { get; set; } = 10;
    }

    // Stok Hareket ViewModel (Ürün detayı ile birlikte)
    public class StokHareketViewModel
    {
        public Guid UrunID { get; set; }
        
        [Display(Name = "Ürün Kodu")]
        public string UrunKodu { get; set; }
        
        [Display(Name = "Ürün Adı")]
        public string UrunAdi { get; set; }
        
        [Display(Name = "Birim")]
        public string Birim { get; set; }
        
        [Display(Name = "Kategori")]
        public string Kategori { get; set; }
        
        [Display(Name = "Stok Miktarı")]
        [DisplayFormat(DataFormatString = "{0:N2}", ApplyFormatInEditMode = false)]
        public decimal StokMiktar { get; set; }
        
        [Display(Name = "Ortalama Maliyet (UZS)")]
        [DisplayFormat(DataFormatString = "{0:N2} so'm", ApplyFormatInEditMode = false)]
        public decimal OrtalamaMaliyetUZS { get; set; }
        
        [Display(Name = "Ortalama Maliyet (USD)")]
        [DisplayFormat(DataFormatString = "{0:N2} $", ApplyFormatInEditMode = false)]
        public decimal OrtalamaMaliyetUSD { get; set; }
        
        [Display(Name = "Ortalama Satış Fiyatı (UZS)")]
        [DisplayFormat(DataFormatString = "{0:N2} so'm", ApplyFormatInEditMode = false)]
        public decimal OrtalamaSatisFiyatiUZS { get; set; }
        
        [Display(Name = "Ortalama Satış Fiyatı (USD)")]
        [DisplayFormat(DataFormatString = "{0:N2} $", ApplyFormatInEditMode = false)]
        public decimal OrtalamaSatisFiyatiUSD { get; set; }
        
        // Geriye dönük uyumluluk için bu alanları da tutalım
        public decimal OrtalamaMaliyetTL { get => OrtalamaMaliyetUZS; set => OrtalamaMaliyetUZS = value; }
        public decimal OrtalamaSatisFiyatiTL { get => OrtalamaSatisFiyatiUZS; set => OrtalamaSatisFiyatiUZS = value; }
        
        // Stok hareketleri
        public List<StokHareketListItemViewModel> StokHareketleri { get; set; } = new List<StokHareketListItemViewModel>();
        
        // FIFO kayıtları
        public List<StokFifoListItemViewModel> FifoKayitlari { get; set; } = new List<StokFifoListItemViewModel>();
        
        // Aklama kuyrugu
        public List<FaturaAklamaKuyrukViewModel> AklamaKuyrugu { get; set; } = new List<FaturaAklamaKuyrukViewModel>();
        
        // Toplam Tahmini Satış Değeri
        [Display(Name = "Toplam Tahmini Satış Değeri (UZS)")]
        [DisplayFormat(DataFormatString = "{0:N2} so'm", ApplyFormatInEditMode = false)]
        public decimal ToplamTahminiSatisDegeriUZS { get; set; }
        
        [Display(Name = "Toplam Tahmini Satış Değeri (USD)")]
        [DisplayFormat(DataFormatString = "{0:N2} $", ApplyFormatInEditMode = false)]
        public decimal ToplamTahminiSatisDegeriUSD { get; set; }
    }

    // Stok Hareket Liste Öğesi ViewModel
    public class StokHareketListItemViewModel
    {
        public Guid StokHareketID { get; set; }
        public DateTime Tarih { get; set; }
        public StokHareketTipi HareketTuru { get; set; }
        public string DepoAdi { get; set; }
        public decimal Miktar { get; set; }
        public decimal BirimFiyat { get; set; }
        public string Birim { get; set; }
        public string ParaBirimi { get; set; }
        public string ParaBirimiSembol { get; set; }
        public string ReferansNo { get; set; }
        public string ReferansTuru { get; set; }
        public string Aciklama { get; set; }
    }

    // Stok FIFO Liste Öğesi ViewModel
    public class StokFifoListItemViewModel
    {
        public Guid StokFifoID { get; set; }
        public DateTime GirisTarihi { get; set; }
        public DateTime? SonCikisTarihi { get; set; }
        public decimal Miktar { get; set; }
        public decimal KalanMiktar { get; set; }
        public decimal BirimFiyat { get; set; }
        public decimal BirimFiyatUSD { get; set; }
        public decimal BirimFiyatUZS { get; set; }
        public decimal DovizKuru { get; set; }
        public string ParaBirimi { get; set; }
        public string ParaBirimiSembol { get; set; }
        public string ReferansNo { get; set; }
        public string ReferansTuru { get; set; }
    }

    // Stok Hareket Detay ViewModel
    public class StokHareketDetayViewModel
    {
        public Guid StokHareketID { get; set; }
        
        public Guid UrunID { get; set; }
        
        [Display(Name = "Ürün Kodu")]
        public string UrunKodu { get; set; }
        
        [Display(Name = "Ürün Adı")]
        public string UrunAdi { get; set; }
        
        [Display(Name = "Depo")]
        public string DepoAdi { get; set; }
        
        [Display(Name = "Hareket Türü")]
        public StokHareketTipi HareketTuru { get; set; }
        
        [Display(Name = "Tarih")]
        [DisplayFormat(DataFormatString = "{0:dd.MM.yyyy HH:mm}", ApplyFormatInEditMode = false)]
        public DateTime Tarih { get; set; }
        
        [Display(Name = "Miktar")]
        [DisplayFormat(DataFormatString = "{0:N2}", ApplyFormatInEditMode = false)]
        public decimal Miktar { get; set; }
        
        [Display(Name = "Birim")]
        public string Birim { get; set; }
        
        [Display(Name = "Birim Fiyat")]
        [DisplayFormat(DataFormatString = "{0:N2}", ApplyFormatInEditMode = false)]
        public decimal BirimFiyat { get; set; }
        
        [Display(Name = "Para Birimi")]
        public string ParaBirimi { get; set; }
        
        [Display(Name = "Referans No")]
        public string ReferansNo { get; set; }
        
        [Display(Name = "Referans Türü")]
        public string ReferansTuru { get; set; }
        
        [Display(Name = "Açıklama")]
        public string Aciklama { get; set; }
    }

    // FIFO Kaydı ViewModel
    public class FifoKayitViewModel
    {
        public Guid FifoID { get; set; }
        public Guid StokFifoID { get; set; }
        
        [Display(Name = "Giriş Tarihi")]
        [DisplayFormat(DataFormatString = "{0:dd.MM.yyyy HH:mm}", ApplyFormatInEditMode = false)]
        public DateTime GirisTarihi { get; set; }
        
        [Display(Name = "Son Çıkış Tarihi")]
        [DisplayFormat(DataFormatString = "{0:dd.MM.yyyy HH:mm}", ApplyFormatInEditMode = false)]
        public DateTime? SonCikisTarihi { get; set; }
        
        [Display(Name = "Miktar")]
        [DisplayFormat(DataFormatString = "{0:N2}", ApplyFormatInEditMode = false)]
        public decimal Miktar { get; set; }
        
        [Display(Name = "Kalan Miktar")]
        [DisplayFormat(DataFormatString = "{0:N2}", ApplyFormatInEditMode = false)]
        public decimal KalanMiktar { get; set; }
        
        [Display(Name = "Birim Fiyat")]
        [DisplayFormat(DataFormatString = "{0:N2}", ApplyFormatInEditMode = false)]
        public decimal BirimFiyat { get; set; }
        
        [Display(Name = "Birim Fiyat (TL)")]
        [DisplayFormat(DataFormatString = "{0:N2}", ApplyFormatInEditMode = false)]
        public decimal BirimFiyatTL { get; set; }
        
        [Display(Name = "Birim Fiyat (USD)")]
        [DisplayFormat(DataFormatString = "{0:N2}", ApplyFormatInEditMode = false)]
        public decimal BirimFiyatUSD { get; set; }
        
        [Display(Name = "Para Birimi")]
        public string ParaBirimi { get; set; }
        
        [Display(Name = "Döviz Kuru")]
        [DisplayFormat(DataFormatString = "{0:N6}", ApplyFormatInEditMode = false)]
        public decimal DovizKuru { get; set; }
        
        [Display(Name = "Referans No")]
        public string ReferansNo { get; set; }
        
        [Display(Name = "Referans Türü")]
        public string ReferansTuru { get; set; }
        
        [Display(Name = "Açıklama")]
        public string Aciklama { get; set; }
        
        [Display(Name = "Aktif")]
        public bool Aktif { get; set; }
        
        [Display(Name = "İptal")]
        public bool Iptal { get; set; }
    }

    // FIFO Detay ViewModel
    public class FifoDetayViewModel
    {
        public Guid FifoID { get; set; }
        public Guid StokFifoID { get; set; }
        
        public Guid UrunID { get; set; }
        
        [Display(Name = "Ürün Kodu")]
        public string UrunKodu { get; set; }
        
        [Display(Name = "Ürün Adı")]
        public string UrunAdi { get; set; }
        
        [Display(Name = "Giriş Tarihi")]
        [DisplayFormat(DataFormatString = "{0:dd.MM.yyyy HH:mm}", ApplyFormatInEditMode = false)]
        public DateTime GirisTarihi { get; set; }
        
        [Display(Name = "Son Çıkış Tarihi")]
        [DisplayFormat(DataFormatString = "{0:dd.MM.yyyy HH:mm}", ApplyFormatInEditMode = false)]
        public DateTime? SonCikisTarihi { get; set; }
        
        [Display(Name = "Miktar")]
        [DisplayFormat(DataFormatString = "{0:N2}", ApplyFormatInEditMode = false)]
        public decimal Miktar { get; set; }
        
        [Display(Name = "Kalan Miktar")]
        [DisplayFormat(DataFormatString = "{0:N2}", ApplyFormatInEditMode = false)]
        public decimal KalanMiktar { get; set; }
        
        [Display(Name = "Birim Fiyat")]
        [DisplayFormat(DataFormatString = "{0:N2}", ApplyFormatInEditMode = false)]
        public decimal BirimFiyat { get; set; }
        
        [Display(Name = "Birim")]
        public string Birim { get; set; }
        
        [Display(Name = "Para Birimi")]
        public string ParaBirimi { get; set; }
        
        [Display(Name = "Döviz Kuru")]
        [DisplayFormat(DataFormatString = "{0:N6}", ApplyFormatInEditMode = false)]
        public decimal DovizKuru { get; set; }
        
        [Display(Name = "USD Birim Fiyat")]
        public decimal BirimFiyatUSD { get; set; }
        
        [Display(Name = "UZS Birim Fiyat")]
        public decimal BirimFiyatUZS { get; set; }
        
        [Display(Name = "Referans No")]
        public string ReferansNo { get; set; }
        
        [Display(Name = "Referans Türü")]
        public string ReferansTuru { get; set; }
        
        [Display(Name = "Açıklama")]
        public string Aciklama { get; set; }
        
        [Display(Name = "Aktif")]
        public bool Aktif { get; set; }
        
        [Display(Name = "İptal")]
        public bool Iptal { get; set; }
        
        [Display(Name = "İptal Tarihi")]
        [DisplayFormat(DataFormatString = "{0:dd.MM.yyyy HH:mm}", ApplyFormatInEditMode = false)]
        public DateTime? IptalTarihi { get; set; }
        
        [Display(Name = "İptal Açıklaması")]
        public string IptalAciklama { get; set; }
    }

    // Stok Çıkış ViewModel
    public class StokCikisViewModel
    {
        [Required(ErrorMessage = "Ürün seçimi zorunludur.")]
        [Display(Name = "Ürün")]
        public Guid UrunID { get; set; }

        [Required(ErrorMessage = "Depo seçimi zorunludur.")]
        [Display(Name = "Depo")]
        public Guid DepoID { get; set; }

        [Required(ErrorMessage = "Miktar girişi zorunludur.")]
        [Range(0.01, double.MaxValue, ErrorMessage = "Miktar 0'dan büyük olmalıdır.")]
        [Display(Name = "Miktar")]
        public decimal Miktar { get; set; }

        [Required(ErrorMessage = "Birim seçimi zorunludur.")]
        [Display(Name = "Birim")]
        public string Birim { get; set; }

        [Required(ErrorMessage = "Tarih girişi zorunludur.")]
        [Display(Name = "Tarih")]
        [DataType(DataType.Date)]
        public DateTime Tarih { get; set; }

        [Display(Name = "Referans No")]
        public string ReferansNo { get; set; }

        [Display(Name = "Referans Türü")]
        public string ReferansTuru { get; set; }

        [Display(Name = "Açıklama")]
        public string Aciklama { get; set; }
        
        [Display(Name = "Hareket Türü")]
        public StokHareketTipi HareketTuru { get; set; } = StokHareketTipi.Cikis;
    }

    // Stok Transfer ViewModel
    public class StokTransferViewModel
    {
        [Required(ErrorMessage = "Ürün seçimi zorunludur.")]
        [Display(Name = "Ürün")]
        public Guid UrunID { get; set; }

        [Required(ErrorMessage = "Kaynak depo seçimi zorunludur.")]
        [Display(Name = "Kaynak Depo")]
        public Guid KaynakDepoID { get; set; }

        [Required(ErrorMessage = "Hedef depo seçimi zorunludur.")]
        [Display(Name = "Hedef Depo")]
        public Guid HedefDepoID { get; set; }

        [Required(ErrorMessage = "Miktar girişi zorunludur.")]
        [Range(0.01, double.MaxValue, ErrorMessage = "Miktar 0'dan büyük olmalıdır.")]
        [Display(Name = "Miktar")]
        public decimal Miktar { get; set; }

        [Required(ErrorMessage = "Birim seçimi zorunludur.")]
        [Display(Name = "Birim")]
        public string Birim { get; set; }

        [Required(ErrorMessage = "Tarih girişi zorunludur.")]
        [Display(Name = "Tarih")]
        [DataType(DataType.Date)]
        public DateTime Tarih { get; set; }

        [Display(Name = "Referans No")]
        public string ReferansNo { get; set; }

        [Display(Name = "Açıklama")]
        public string Aciklama { get; set; }
    }

    // Stok Sayım ViewModel
    public class StokSayimViewModel
    {
        [Required(ErrorMessage = "Depo seçimi zorunludur.")]
        [Display(Name = "Depo")]
        public Guid DepoID { get; set; }

        [Required(ErrorMessage = "Tarih girişi zorunludur.")]
        [Display(Name = "Tarih")]
        [DataType(DataType.Date)]
        public DateTime Tarih { get; set; }

        [Display(Name = "Sayım Notu")]
        public string SayimNotu { get; set; }

        [Display(Name = "Referans No")]
        public string ReferansNo { get; set; }

        [Display(Name = "Açıklama")]
        public string Aciklama { get; set; }

        [Display(Name = "Para Birimi")]
        public string ParaBirimi { get; set; } = "TRY";
        
        public List<string> ParaBirimleri { get; set; }

        public List<StokSayimUrunViewModel> UrunListesi { get; set; } = new List<StokSayimUrunViewModel>();
    }

    public class StokSayimUrunViewModel
    {
        public Guid UrunID { get; set; }
        public string UrunKodu { get; set; }
        public string UrunAdi { get; set; }
        public string Birim { get; set; }
        public decimal SistemStokMiktari { get; set; }
        public decimal SayimMiktari { get; set; }
        public decimal Fark { get; set; }
    }

    // Stok Durumu Raporu ViewModel
    public class StokDurumuViewModel
    {
        public Guid UrunID { get; set; }
        public string UrunKodu { get; set; }
        public string UrunAdi { get; set; }
        public string Kategori { get; set; }
        public string Birim { get; set; }
        public decimal StokMiktar { get; set; }
        public decimal OrtalamaMaliyet { get; set; } // UZS cinsinden
        public decimal OrtalamaMaliyetUSD { get; set; } // USD cinsinden
        public decimal ToplamMaliyet { get; set; } // UZS cinsinden
        public decimal ToplamMaliyetUSD { get; set; } // USD cinsinden
        public decimal SatisFiyati { get; set; }
        
        public List<StokDurumuDetayViewModel> StokDurumuListesi { get; set; } = new List<StokDurumuDetayViewModel>();
        public decimal ToplamStokDegeri { get; set; } // UZS cinsinden
        public decimal ToplamStokDegeriUSD { get; set; } // USD cinsinden
    }

    public class StokDurumuDetayViewModel
    {
        public Guid UrunID { get; set; }
        public string UrunKodu { get; set; }
        public string UrunAdi { get; set; }
        public string Kategori { get; set; }
        public string Birim { get; set; }
        public Guid DepoID { get; set; }
        public string DepoAdi { get; set; }
        public decimal StokMiktari { get; set; }
        public decimal OrtalamaMaliyet { get; set; }
        public decimal OrtalamaMaliyetUSD { get; set; } // USD cinsinden ortalama maliyet
        public decimal SatisFiyati { get; set; }
        public decimal KritikStokSeviyesi { get; set; }
    }
} 