using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace MuhasebeStokWebApp.ViewModels.Cari
{
    public class CariEkstreRaporViewModel
    {
        public Guid Id { get; set; }
        
        public Guid CariID 
        { 
            get { return Id; } 
            set { Id = value; } 
        }
        
        [Display(Name = "Cari Adı")]
        public string CariAdi { get; set; }
        
        [Display(Name = "Cari Kodu")]
        public string CariKodu { get; set; }
        
        [Display(Name = "Vergi No")]
        public string VergiNo { get; set; }
        
        [Display(Name = "Adres")]
        public string Adres { get; set; }
        
        [Display(Name = "Başlangıç Tarihi")]
        [DataType(DataType.Date)]
        public DateTime BaslangicTarihi { get; set; }
        
        [Display(Name = "Bitiş Tarihi")]
        [DataType(DataType.Date)]
        public DateTime BitisTarihi { get; set; }
        
        [Display(Name = "Rapor Tarihi")]
        public DateTime RaporTarihi { get; set; } = DateTime.Now;
        
        // Başlangıç bakiyesi
        [Display(Name = "Başlangıç Bakiyesi")]
        public decimal BaslangicBakiye { get; set; }
        
        // Bakiye
        [Display(Name = "Bakiye")]
        [DisplayFormat(DataFormatString = "{0:N2}", ApplyFormatInEditMode = false)]
        public decimal Bakiye { get; set; }
        
        [Display(Name = "Güncel Bakiye")]
        public decimal GuncelBakiye 
        { 
            get { return Bakiye; } 
            set { Bakiye = value; } 
        }
        
        // Para birimi desteği
        [Display(Name = "Para Birimi")]
        public Guid? ParaBirimiId { get; set; }
        
        [Display(Name = "Varsayılan Para Birimi")]
        public Guid? VarsayilanParaBirimiId { get; set; }
        
        [Display(Name = "Para Birimi Kodu")]
        public string ParaBirimiKodu { get; set; }
        
        [Display(Name = "Para Birimi Sembolü")]
        public string ParaBirimiSembolu { get; set; }
        
        [Display(Name = "Para Birimi Sembolü")]
        public string ParaBirimiSembol
        {
            get { return ParaBirimiSembolu; }
            set { ParaBirimiSembolu = value; }
        }
        
        [Display(Name = "Döviz Kuru")]
        [DisplayFormat(DataFormatString = "{0:N6}", ApplyFormatInEditMode = false)]
        public decimal DovizKuru { get; set; } = 1;
        
        [Display(Name = "Dönüştürme Tarihi")]
        [DisplayFormat(DataFormatString = "{0:dd.MM.yyyy}", ApplyFormatInEditMode = true)]
        public DateTime? DovizKuruTarihi { get; set; }
        
        [Display(Name = "Orijinal Para Birimi")]
        public bool OrijinalParaBirimi { get; set; } = true;
        
        // Seçili para birimi objesi
        public object SeciliParaBirimi { get; set; }
        
        // Tüm para birimleri listesi
        public List<object> ParaBirimleri { get; set; }
        
        // Hareketler listesi
        public List<CariEkstreHareketViewModel> Hareketler { get; set; }
        
        // Özet değerleri
        [Display(Name = "Toplam Borç")]
        [DisplayFormat(DataFormatString = "{0:N2}", ApplyFormatInEditMode = false)]
        public decimal ToplamBorc { get; set; }
        
        [Display(Name = "Toplam Alacak")]
        [DisplayFormat(DataFormatString = "{0:N2}", ApplyFormatInEditMode = false)]
        public decimal ToplamAlacak { get; set; }
        
        // Özet
        public List<CariEkstreHareketViewModel> Ozet { get; set; }
        
        public CariEkstreRaporViewModel()
        {
            Hareketler = new List<CariEkstreHareketViewModel>();
            ParaBirimleri = new List<object>();
            Ozet = new List<CariEkstreHareketViewModel>();
            BaslangicTarihi = DateTime.Now.AddMonths(-1);
            BitisTarihi = DateTime.Now;
            RaporTarihi = DateTime.Now;
            ParaBirimiKodu = "TRY";
            ParaBirimiSembolu = "₺";
        }
        
        // İç içe CariEkstreHareketViewModel sınıfı tanımı
        public class CariEkstreHareketViewModel
        {
            public Guid CariHareketID { get; set; }
            
            public Guid CariID { get; set; }
            
            [Display(Name = "Tarih")]
            [DisplayFormat(DataFormatString = "{0:dd.MM.yyyy}", ApplyFormatInEditMode = true)]
            public DateTime Tarih { get; set; }
            
            [Display(Name = "Vade Tarihi")]
            [DisplayFormat(DataFormatString = "{0:dd.MM.yyyy}", ApplyFormatInEditMode = true)]
            public DateTime? VadeTarihi { get; set; }
            
            [Display(Name = "Açıklama")]
            public string Aciklama { get; set; }
            
            [Display(Name = "İşlem Türü")]
            public string IslemTuru { get; set; }
            
            [Display(Name = "İşlem No")]
            public string IslemNo { get; set; }
            
            [Display(Name = "Evrak No")]
            public string EvrakNo { get; set; }
            
            [Display(Name = "Tutar")]
            [DisplayFormat(DataFormatString = "{0:N2}", ApplyFormatInEditMode = false)]
            public decimal Tutar { get; set; }
            
            [Display(Name = "Borç")]
            [DisplayFormat(DataFormatString = "{0:N2}", ApplyFormatInEditMode = false)]
            public decimal Borc { get; set; }
            
            [Display(Name = "Alacak")]
            [DisplayFormat(DataFormatString = "{0:N2}", ApplyFormatInEditMode = false)]
            public decimal Alacak { get; set; }
            
            [Display(Name = "Bakiye")]
            [DisplayFormat(DataFormatString = "{0:N2}", ApplyFormatInEditMode = false)]
            public decimal Bakiye { get; set; }
            
            [Display(Name = "Para Birimi")]
            public string ParaBirimi { get; set; } = "TL";
            
            [Display(Name = "Kaynak")]
            public string Kaynak { get; set; } = "Cari";
        }
    }
} 