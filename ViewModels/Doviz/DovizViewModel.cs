using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace MuhasebeStokWebApp.ViewModels.Doviz
{
    public class DovizKuruViewModel
    {
        public Guid DovizKuruID { get; set; }
        
        [Display(Name = "Döviz Kodu")]
        public string DovizKodu { get; set; } = "";
        
        [Display(Name = "Döviz Adı")]
        public string DovizAdi { get; set; } = "";
        
        [Display(Name = "Alış Fiyatı")]
        [DisplayFormat(DataFormatString = "{0:N6}", ApplyFormatInEditMode = false)]
        public decimal AlisFiyati { get; set; }
        
        [Display(Name = "Satış Fiyatı")]
        [DisplayFormat(DataFormatString = "{0:N6}", ApplyFormatInEditMode = false)]
        public decimal SatisFiyati { get; set; }
        
        [Display(Name = "Efektif Alış")]
        [DisplayFormat(DataFormatString = "{0:N6}", ApplyFormatInEditMode = false)]
        public decimal EfektifAlisFiyati { get; set; }
        
        [Display(Name = "Efektif Satış")]
        [DisplayFormat(DataFormatString = "{0:N6}", ApplyFormatInEditMode = false)]
        public decimal EfektifSatisFiyati { get; set; }
        
        [Display(Name = "Tarih")]
        [DisplayFormat(DataFormatString = "{0:dd.MM.yyyy}", ApplyFormatInEditMode = false)]
        public DateTime Tarih { get; set; }
        
        [Display(Name = "Güncelleme Tarihi")]
        [DisplayFormat(DataFormatString = "{0:dd.MM.yyyy HH:mm}", ApplyFormatInEditMode = false)]
        public DateTime GuncellemeTarihi { get; set; }
        
        [Display(Name = "Aktif")]
        public bool Aktif { get; set; }
        
        // Para Birimi bilgisi
        [Display(Name = "Para Birimi Kodu")]
        public string ParaBirimiKodu { get; set; } = "";
    }

    public class DovizListViewModel
    {
        public Guid DovizID { get; set; }
        
        [Display(Name = "Kod")]
        public string Kod { get; set; } = "";
        
        [Display(Name = "Ad")]
        public string Ad { get; set; } = "";
        
        [Display(Name = "Sembol")]
        public string Sembol { get; set; } = "";
        
        [Display(Name = "Ana Para Birimi")]
        public bool AnaParaBirimiMi { get; set; }
        
        [Display(Name = "Aktif")]
        public bool Aktif { get; set; }
        
        [Display(Name = "Sıra")]
        public int Sira { get; set; }
    }

    public class DovizCreateViewModel
    {
        public Guid DovizID { get; set; }
        
        [Required(ErrorMessage = "Döviz kodu zorunludur.")]
        [StringLength(10, ErrorMessage = "Döviz kodu en fazla 10 karakter olabilir.")]
        [Display(Name = "Kod")]
        public string Kod { get; set; } = "";
        
        [Required(ErrorMessage = "Döviz adı zorunludur.")]
        [StringLength(50, ErrorMessage = "Döviz adı en fazla 50 karakter olabilir.")]
        [Display(Name = "Ad")]
        public string Ad { get; set; } = "";
        
        [Required(ErrorMessage = "Sembol zorunludur.")]
        [StringLength(10, ErrorMessage = "Sembol en fazla 10 karakter olabilir.")]
        [Display(Name = "Sembol")]
        public string Sembol { get; set; } = "";
        
        [Required(ErrorMessage = "Ondalık ayracı zorunludur.")]
        [StringLength(1, ErrorMessage = "Ondalık ayracı 1 karakter olmalıdır.")]
        [Display(Name = "Ondalık Ayracı")]
        public string OndalikAyraci { get; set; } = ".";
        
        [Required(ErrorMessage = "Binlik ayracı zorunludur.")]
        [StringLength(1, ErrorMessage = "Binlik ayracı 1 karakter olmalıdır.")]
        [Display(Name = "Binlik Ayracı")]
        public string BinlikAyraci { get; set; } = ",";
        
        [Required(ErrorMessage = "Ondalık hassasiyet zorunludur.")]
        [Range(0, 6, ErrorMessage = "Ondalık hassasiyet 0 ile 6 arasında olmalıdır.")]
        [Display(Name = "Ondalık Hassasiyet")]
        public int OndalikHassasiyet { get; set; } = 2;
        
        [Display(Name = "Ana Para Birimi")]
        public bool AnaParaBirimiMi { get; set; }
        
        [Display(Name = "Aktif")]
        public bool Aktif { get; set; } = true;
        
        [Display(Name = "Açıklama")]
        public string Aciklama { get; set; } = "";
        
        [Range(0, 1000, ErrorMessage = "Sıra 0 ile 1000 arasında olmalıdır.")]
        [Display(Name = "Sıra")]
        public int Sira { get; set; } = 0;
    }

    public class DovizEditViewModel
    {
        public Guid DovizID { get; set; }
        
        [Required(ErrorMessage = "Döviz kodu zorunludur.")]
        [StringLength(10, ErrorMessage = "Döviz kodu en fazla 10 karakter olabilir.")]
        [Display(Name = "Kod")]
        public string Kod { get; set; } = "";
        
        [Required(ErrorMessage = "Döviz adı zorunludur.")]
        [StringLength(50, ErrorMessage = "Döviz adı en fazla 50 karakter olabilir.")]
        [Display(Name = "Ad")]
        public string Ad { get; set; } = "";
        
        [Required(ErrorMessage = "Sembol zorunludur.")]
        [StringLength(10, ErrorMessage = "Sembol en fazla 10 karakter olabilir.")]
        [Display(Name = "Sembol")]
        public string Sembol { get; set; } = "";
        
        [Required(ErrorMessage = "Ondalık ayracı zorunludur.")]
        [StringLength(1, ErrorMessage = "Ondalık ayracı 1 karakter olmalıdır.")]
        [Display(Name = "Ondalık Ayracı")]
        public string OndalikAyraci { get; set; } = ".";
        
        [Required(ErrorMessage = "Binlik ayracı zorunludur.")]
        [StringLength(1, ErrorMessage = "Binlik ayracı 1 karakter olmalıdır.")]
        [Display(Name = "Binlik Ayracı")]
        public string BinlikAyraci { get; set; } = ",";
        
        [Required(ErrorMessage = "Ondalık hassasiyet zorunludur.")]
        [Range(0, 6, ErrorMessage = "Ondalık hassasiyet 0 ile 6 arasında olmalıdır.")]
        [Display(Name = "Ondalık Hassasiyet")]
        public int OndalikHassasiyet { get; set; }
        
        [Display(Name = "Ana Para Birimi")]
        public bool AnaParaBirimiMi { get; set; }
        
        [Display(Name = "Aktif")]
        public bool Aktif { get; set; }
        
        [Display(Name = "Açıklama")]
        public string Aciklama { get; set; } = "";
        
        [Range(0, 1000, ErrorMessage = "Sıra 0 ile 1000 arasında olmalıdır.")]
        [Display(Name = "Sıra")]
        public int Sira { get; set; }
    }

    public class DovizDetailViewModel
    {
        public Guid DovizID { get; set; }
        
        [Display(Name = "Kod")]
        public string Kod { get; set; } = "";
        
        [Display(Name = "Ad")]
        public string Ad { get; set; } = "";
        
        [Display(Name = "Sembol")]
        public string Sembol { get; set; } = "";
        
        [Display(Name = "Ondalık Ayracı")]
        public string OndalikAyraci { get; set; } = ".";
        
        [Display(Name = "Binlik Ayracı")]
        public string BinlikAyraci { get; set; } = ",";
        
        [Display(Name = "Ondalık Hassasiyet")]
        public int OndalikHassasiyet { get; set; }
        
        [Display(Name = "Ana Para Birimi")]
        public bool AnaParaBirimiMi { get; set; }
        
        [Display(Name = "Aktif")]
        public bool Aktif { get; set; }
        
        [Display(Name = "Açıklama")]
        public string Aciklama { get; set; } = "";
        
        [Display(Name = "Sıra")]
        public int Sira { get; set; }
        
        [Display(Name = "Oluşturma Tarihi")]
        [DisplayFormat(DataFormatString = "{0:dd.MM.yyyy HH:mm}", ApplyFormatInEditMode = false)]
        public DateTime? OlusturmaTarihi { get; set; }
        
        [Display(Name = "Güncelleme Tarihi")]
        [DisplayFormat(DataFormatString = "{0:dd.MM.yyyy HH:mm}", ApplyFormatInEditMode = false)]
        public DateTime? GuncellemeTarihi { get; set; }
    }

    public class ParaBirimiViewModel
    {
        public Guid ParaBirimiID { get; set; }
        
        [Required(ErrorMessage = "Kod zorunludur.")]
        [StringLength(10, ErrorMessage = "Kod en fazla 10 karakter olabilir.")]
        [Display(Name = "Kod")]
        public string Kod { get; set; } = "";
        
        [Required(ErrorMessage = "Ad zorunludur.")]
        [StringLength(50, ErrorMessage = "Ad en fazla 50 karakter olabilir.")]
        [Display(Name = "Ad")]
        public string Ad { get; set; } = "";
        
        [Required(ErrorMessage = "Sembol zorunludur.")]
        [StringLength(10, ErrorMessage = "Sembol en fazla 10 karakter olabilir.")]
        [Display(Name = "Sembol")]
        public string Sembol { get; set; } = "";
        
        [Display(Name = "Ana Para Birimi")]
        public bool AnaParaBirimiMi { get; set; }
        
        [Display(Name = "Aktif")]
        public bool Aktif { get; set; }
    }

    public class SistemAyarDovizViewModel
    {
        public Guid SistemAyarID { get; set; }
        
        [Required(ErrorMessage = "Ana döviz kodu zorunludur.")]
        [Display(Name = "Ana Döviz Kodu")]
        public string AnaDovizKodu { get; set; } = "";
        
        [Required(ErrorMessage = "Şirket adı zorunludur.")]
        [Display(Name = "Şirket Adı")]
        public string SirketAdi { get; set; } = "";
        
        [Required(ErrorMessage = "Şirket adresi zorunludur.")]
        [Display(Name = "Şirket Adresi")]
        public string SirketAdresi { get; set; } = "";
        
        [Required(ErrorMessage = "Şirket telefon zorunludur.")]
        [Display(Name = "Şirket Telefon")]
        public string SirketTelefon { get; set; } = "";
        
        [Required(ErrorMessage = "Şirket email zorunludur.")]
        [EmailAddress(ErrorMessage = "Geçerli bir e-posta adresi giriniz.")]
        [Display(Name = "Şirket Email")]
        public string SirketEmail { get; set; } = "";
        
        [Required(ErrorMessage = "Şirket vergi no zorunludur.")]
        [Display(Name = "Şirket Vergi No")]
        public string SirketVergiNo { get; set; } = "";
        
        [Required(ErrorMessage = "Şirket vergi dairesi zorunludur.")]
        [Display(Name = "Şirket Vergi Dairesi")]
        public string SirketVergiDairesi { get; set; } = "";
        
        [Display(Name = "Otomatik Döviz Güncelleme")]
        public bool OtomatikDovizGuncelleme { get; set; }
        
        [Range(1, 168, ErrorMessage = "Döviz güncelleme sıklığı 1 ile 168 saat arasında olmalıdır.")]
        [Display(Name = "Döviz Güncelleme Sıklığı (Saat)")]
        public int DovizGuncellemeSikligi { get; set; }
    }
} 