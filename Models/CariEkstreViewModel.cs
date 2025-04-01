using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace MuhasebeStokWebApp.Models
{
    public class CariEkstreViewModel
    {
        public Guid Id { get; set; }
        
        [Display(Name = "Cari Adı")]
        public string CariAdi { get; set; }
        
        [Display(Name = "Vergi No")]
        public string VergiNo { get; set; }
        
        [Display(Name = "Adres")]
        public string Adres { get; set; }
        
        [Display(Name = "Başlangıç Tarihi")]
        [DisplayFormat(DataFormatString = "{0:dd.MM.yyyy}", ApplyFormatInEditMode = true)]
        public DateTime BaslangicTarihi { get; set; }
        
        [Display(Name = "Bitiş Tarihi")]
        [DisplayFormat(DataFormatString = "{0:dd.MM.yyyy}", ApplyFormatInEditMode = true)]
        public DateTime BitisTarihi { get; set; }
        
        [Display(Name = "Rapor Tarihi")]
        [DisplayFormat(DataFormatString = "{0:dd.MM.yyyy HH:mm}", ApplyFormatInEditMode = true)]
        public DateTime RaporTarihi { get; set; }
        
        [Display(Name = "Toplam Borç")]
        [DisplayFormat(DataFormatString = "{0:N2}", ApplyFormatInEditMode = false)]
        public decimal ToplamBorc { get; set; }
        
        [Display(Name = "Toplam Alacak")]
        [DisplayFormat(DataFormatString = "{0:N2}", ApplyFormatInEditMode = false)]
        public decimal ToplamAlacak { get; set; }
        
        [Display(Name = "Bakiye")]
        [DisplayFormat(DataFormatString = "{0:N2}", ApplyFormatInEditMode = false)]
        public decimal Bakiye { get; set; }
        
        // Para birimi desteği
        [Display(Name = "Para Birimi")]
        public Guid? ParaBirimiId { get; set; }
        
        [Display(Name = "Varsayılan Para Birimi")]
        public Guid? VarsayilanParaBirimiId { get; set; }
        
        [Display(Name = "Para Birimi Kodu")]
        public string ParaBirimiKodu { get; set; } = "TRY";
        
        [Display(Name = "Para Birimi Sembolü")]
        public string ParaBirimiSembol { get; set; } = "₺";
        
        [Display(Name = "Döviz Kuru")]
        [DisplayFormat(DataFormatString = "{0:N6}", ApplyFormatInEditMode = false)]
        public decimal DovizKuru { get; set; } = 1;
        
        [Display(Name = "Dönüştürme Tarihi")]
        [DisplayFormat(DataFormatString = "{0:dd.MM.yyyy}", ApplyFormatInEditMode = true)]
        public DateTime? DovizKuruTarihi { get; set; }
        
        [Display(Name = "Orijinal Para Birimi")]
        public bool OrijinalParaBirimi { get; set; } = true;
        
        public List<CariHareketViewModel> Hareketler { get; set; }
    }
    
    public class CariHareketViewModel
    {
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
        
        [Display(Name = "Evrak No")]
        public string EvrakNo { get; set; }
        
        [Display(Name = "Kaynak")]
        public string Kaynak { get; set; }
        
        [Display(Name = "Borç")]
        [DisplayFormat(DataFormatString = "{0:N2}", ApplyFormatInEditMode = false)]
        public decimal Borc { get; set; }
        
        [Display(Name = "Alacak")]
        [DisplayFormat(DataFormatString = "{0:N2}", ApplyFormatInEditMode = false)]
        public decimal Alacak { get; set; }
        
        [Display(Name = "Bakiye")]
        [DisplayFormat(DataFormatString = "{0:N2}", ApplyFormatInEditMode = false)]
        public decimal Bakiye { get; set; }
        
        // Para birimi bilgileri
        [Display(Name = "Orijinal Borç")]
        [DisplayFormat(DataFormatString = "{0:N2}", ApplyFormatInEditMode = false)]
        public decimal OrijinalBorc { get; set; }
        
        [Display(Name = "Orijinal Alacak")]
        [DisplayFormat(DataFormatString = "{0:N2}", ApplyFormatInEditMode = false)]
        public decimal OrijinalAlacak { get; set; }
        
        [Display(Name = "Orijinal Bakiye")]
        [DisplayFormat(DataFormatString = "{0:N2}", ApplyFormatInEditMode = false)]
        public decimal OrijinalBakiye { get; set; }
        
        [Display(Name = "Hareket Para Birimi")]
        public string HareketParaBirimi { get; set; } = "TRY";
    }
} 