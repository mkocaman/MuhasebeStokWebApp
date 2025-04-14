using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace MuhasebeStokWebApp.ViewModels
{
    public class DovizKuruViewModel
    {
        public Guid DovizKuruID { get; set; }

        [Display(Name = "Döviz Kodu")]
        public string DovizKodu { get; set; } = "";

        [Display(Name = "Döviz Adı")]
        public string DovizAdi { get; set; } = "";

        [Display(Name = "Kaynak Para Birimi")]
        public string KaynakParaBirimi { get; set; } = "";

        [Display(Name = "Hedef Para Birimi")]
        public string HedefParaBirimi { get; set; } = "";

        [Display(Name = "Kur Değeri")]
        [DisplayFormat(DataFormatString = "{0:N6}", ApplyFormatInEditMode = false)]
        public decimal KurDegeri { get; set; }

        [Display(Name = "Alış Fiyatı")]
        [DisplayFormat(DataFormatString = "{0:N6}", ApplyFormatInEditMode = false)]
        public decimal AlisFiyati { get; set; }

        [Display(Name = "Satış Fiyatı")]
        [DisplayFormat(DataFormatString = "{0:N6}", ApplyFormatInEditMode = false)]
        public decimal SatisFiyati { get; set; }

        [Display(Name = "Efektif Alış")]
        [DisplayFormat(DataFormatString = "{0:N6}", ApplyFormatInEditMode = false)]
        public decimal? EfektifAlis { get; set; }

        [Display(Name = "Efektif Satış")]
        [DisplayFormat(DataFormatString = "{0:N6}", ApplyFormatInEditMode = false)]
        public decimal? EfektifSatis { get; set; }

        [Display(Name = "Tarih")]
        [DataType(DataType.Date)]
        [DisplayFormat(DataFormatString = "{0:dd.MM.yyyy}", ApplyFormatInEditMode = false)]
        public DateTime Tarih { get; set; }

        [Display(Name = "Güncelleme Tarihi")]
        [DisplayFormat(DataFormatString = "{0:dd.MM.yyyy HH:mm}", ApplyFormatInEditMode = false)]
        public DateTime GuncellemeTarihi { get; set; }

        [Display(Name = "Aktif")]
        public bool Aktif { get; set; } = true;

        [Display(Name = "Para Birimi Kodu")]
        public string ParaBirimiKodu { get; set; } = "";

        [Display(Name = "Kaynak")]
        public string Kaynak { get; set; } = "TCMB";

        [Display(Name = "Açıklama")]
        public string? Aciklama { get; set; }
    }

    public class DovizKuruEkleViewModel
    {
        [Required(ErrorMessage = "Kaynak para birimi zorunludur.")]
        [Display(Name = "Kaynak Para Birimi")]
        public Guid KaynakParaBirimiID { get; set; }

        [Required(ErrorMessage = "Hedef para birimi zorunludur.")]
        [Display(Name = "Hedef Para Birimi")]
        public Guid HedefParaBirimiID { get; set; }

        [Required(ErrorMessage = "Kur değeri zorunludur.")]
        [Range(0.000001, double.MaxValue, ErrorMessage = "Kur değeri 0'dan büyük olmalıdır.")]
        [Display(Name = "Kur Değeri")]
        public decimal KurDegeri { get; set; }

        [Required(ErrorMessage = "Alış fiyatı zorunludur.")]
        [Range(0.000001, double.MaxValue, ErrorMessage = "Alış fiyatı 0'dan büyük olmalıdır.")]
        [Display(Name = "Alış Fiyatı")]
        public decimal Alis { get; set; }

        [Required(ErrorMessage = "Satış fiyatı zorunludur.")]
        [Range(0.000001, double.MaxValue, ErrorMessage = "Satış fiyatı 0'dan büyük olmalıdır.")]
        [Display(Name = "Satış Fiyatı")]
        public decimal Satis { get; set; }
        
        [Display(Name = "Efektif Alış")]
        public decimal? EfektifAlis { get; set; }
        
        [Display(Name = "Efektif Satış")]
        public decimal? EfektifSatis { get; set; }

        [Required(ErrorMessage = "Tarih zorunludur.")]
        [Display(Name = "Tarih")]
        [DataType(DataType.Date)]
        public DateTime Tarih { get; set; } = DateTime.Today;

        [Required(ErrorMessage = "Kaynak bilgisi zorunludur.")]
        [StringLength(50, ErrorMessage = "Kaynak en fazla 50 karakter olabilir.")]
        [Display(Name = "Kaynak")]
        public string Kaynak { get; set; } = "Manuel";

        [Display(Name = "Açıklama")]
        [StringLength(500, ErrorMessage = "Açıklama en fazla 500 karakter olabilir.")]
        public string? Aciklama { get; set; }
    }

    public class DovizKuruDuzenleViewModel
    {
        public Guid DovizKuruID { get; set; }

        [Required(ErrorMessage = "Kaynak para birimi zorunludur.")]
        [Display(Name = "Kaynak Para Birimi")]
        public Guid KaynakParaBirimiID { get; set; }

        [Required(ErrorMessage = "Hedef para birimi zorunludur.")]
        [Display(Name = "Hedef Para Birimi")]
        public Guid HedefParaBirimiID { get; set; }

        [Required(ErrorMessage = "Kur değeri zorunludur.")]
        [Range(0.000001, double.MaxValue, ErrorMessage = "Kur değeri 0'dan büyük olmalıdır.")]
        [Display(Name = "Kur Değeri")]
        public decimal KurDegeri { get; set; }

        [Display(Name = "Alış Fiyatı")]
        [Range(0.000001, double.MaxValue, ErrorMessage = "Alış fiyatı 0'dan büyük olmalıdır.")]
        public decimal Alis { get; set; }

        [Display(Name = "Satış Fiyatı")]
        [Range(0.000001, double.MaxValue, ErrorMessage = "Satış fiyatı 0'dan büyük olmalıdır.")]
        public decimal Satis { get; set; }
        
        [Display(Name = "Efektif Alış")]
        public decimal? EfektifAlis { get; set; }
        
        [Display(Name = "Efektif Satış")]
        public decimal? EfektifSatis { get; set; }

        [Required(ErrorMessage = "Tarih zorunludur.")]
        [Display(Name = "Tarih")]
        [DataType(DataType.Date)]
        public DateTime Tarih { get; set; }

        [Required(ErrorMessage = "Kaynak bilgisi zorunludur.")]
        [StringLength(50, ErrorMessage = "Kaynak en fazla 50 karakter olabilir.")]
        [Display(Name = "Kaynak")]
        public string Kaynak { get; set; } = "";

        [Display(Name = "Açıklama")]
        [StringLength(500, ErrorMessage = "Açıklama en fazla 500 karakter olabilir.")]
        public string? Aciklama { get; set; }
        
        [Display(Name = "Aktif")]
        public bool Aktif { get; set; } = true;
    }
} 