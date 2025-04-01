using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using MuhasebeStokWebApp.Data.Entities.ParaBirimiModulu;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace MuhasebeStokWebApp.ViewModels.ParaBirimiModulu
{
    /// <summary>
    /// Para birimi listeleme ViewModel'i
    /// </summary>
    public class ParaBirimiListViewModel
    {
        /// <summary>
        /// Para birimi ID
        /// </summary>
        public Guid ParaBirimiID { get; set; }
        
        /// <summary>
        /// Para birimi kodu (örn. TRY, USD, EUR)
        /// </summary>
        [Display(Name = "Kod")]
        public string Kod { get; set; }
        
        /// <summary>
        /// Para birimi adı
        /// </summary>
        [Display(Name = "Ad")]
        public string Ad { get; set; }
        
        /// <summary>
        /// Para birimi sembolü
        /// </summary>
        [Display(Name = "Sembol")]
        public string Sembol { get; set; }
        
        /// <summary>
        /// Ana para birimi mi
        /// </summary>
        [Display(Name = "Ana Para Birimi")]
        public bool AnaParaBirimiMi { get; set; }
        
        /// <summary>
        /// Aktif mi
        /// </summary>
        [Display(Name = "Aktif")]
        public bool Aktif { get; set; }
        
        /// <summary>
        /// Sıralama değeri
        /// </summary>
        [Display(Name = "Sıra")]
        public int Sira { get; set; }
    }
    
    /// <summary>
    /// Para birimi oluşturma ViewModel'i
    /// </summary>
    public class ParaBirimiCreateViewModel
    {
        /// <summary>
        /// Para birimi kodu (örn. TRY, USD, EUR)
        /// </summary>
        [Required(ErrorMessage = "Para birimi kodu gereklidir.")]
        [StringLength(10, ErrorMessage = "Para birimi kodu en fazla 10 karakter olabilir.")]
        [Display(Name = "Kod")]
        public string Kod { get; set; }
        
        /// <summary>
        /// Para birimi adı
        /// </summary>
        [Required(ErrorMessage = "Para birimi adı gereklidir.")]
        [StringLength(50, ErrorMessage = "Para birimi adı en fazla 50 karakter olabilir.")]
        [Display(Name = "Ad")]
        public string Ad { get; set; }
        
        /// <summary>
        /// Para birimi sembolü
        /// </summary>
        [StringLength(10, ErrorMessage = "Para birimi sembolü en fazla 10 karakter olabilir.")]
        [Display(Name = "Sembol")]
        public string Sembol { get; set; }
        
        /// <summary>
        /// Ondalık ayracı
        /// </summary>
        [Display(Name = "Ondalık Ayracı")]
        [StringLength(1, ErrorMessage = "Ondalık ayracı en fazla 1 karakter olabilir.")]
        public string OndalikAyraci { get; set; } = ",";
        
        /// <summary>
        /// Binlik ayracı
        /// </summary>
        [Display(Name = "Binlik Ayracı")]
        [StringLength(1, ErrorMessage = "Binlik ayracı en fazla 1 karakter olabilir.")]
        public string BinlikAyraci { get; set; } = ".";
        
        /// <summary>
        /// Ondalık hassasiyet
        /// </summary>
        [Display(Name = "Ondalık Hassasiyet")]
        [Range(0, 6, ErrorMessage = "Ondalık hassasiyet 0 ile 6 arasında olmalıdır.")]
        public int OndalikHassasiyet { get; set; } = 2;
        
        /// <summary>
        /// Ana para birimi mi
        /// </summary>
        [Display(Name = "Ana Para Birimi Mi?")]
        public bool AnaParaBirimiMi { get; set; }
        
        /// <summary>
        /// Aktif mi
        /// </summary>
        [Display(Name = "Aktif")]
        public bool Aktif { get; set; } = true;
        
        /// <summary>
        /// Sıralama değeri
        /// </summary>
        [Display(Name = "Sıra")]
        public int Sira { get; set; }
        
        /// <summary>
        /// Açıklama
        /// </summary>
        [Display(Name = "Açıklama")]
        [StringLength(500, ErrorMessage = "Açıklama en fazla 500 karakter olabilir.")]
        public string Aciklama { get; set; }
    }
    
    /// <summary>
    /// Para birimi düzenleme ViewModel'i
    /// </summary>
    public class ParaBirimiEditViewModel
    {
        /// <summary>
        /// Para birimi ID
        /// </summary>
        public Guid ParaBirimiID { get; set; }
        
        /// <summary>
        /// Para birimi kodu (örn. TRY, USD, EUR)
        /// </summary>
        [Required(ErrorMessage = "Para birimi kodu gereklidir.")]
        [StringLength(10, ErrorMessage = "Para birimi kodu en fazla 10 karakter olabilir.")]
        [Display(Name = "Kod")]
        public string Kod { get; set; }
        
        /// <summary>
        /// Para birimi adı
        /// </summary>
        [Required(ErrorMessage = "Para birimi adı gereklidir.")]
        [StringLength(50, ErrorMessage = "Para birimi adı en fazla 50 karakter olabilir.")]
        [Display(Name = "Ad")]
        public string Ad { get; set; }
        
        /// <summary>
        /// Para birimi sembolü
        /// </summary>
        [StringLength(10, ErrorMessage = "Para birimi sembolü en fazla 10 karakter olabilir.")]
        [Display(Name = "Sembol")]
        public string Sembol { get; set; }
        
        /// <summary>
        /// Ondalık ayracı
        /// </summary>
        [Display(Name = "Ondalık Ayracı")]
        [StringLength(1, ErrorMessage = "Ondalık ayracı en fazla 1 karakter olabilir.")]
        public string OndalikAyraci { get; set; }
        
        /// <summary>
        /// Binlik ayracı
        /// </summary>
        [Display(Name = "Binlik Ayracı")]
        [StringLength(1, ErrorMessage = "Binlik ayracı en fazla 1 karakter olabilir.")]
        public string BinlikAyraci { get; set; }
        
        /// <summary>
        /// Ondalık hassasiyet
        /// </summary>
        [Display(Name = "Ondalık Hassasiyet")]
        [Range(0, 6, ErrorMessage = "Ondalık hassasiyet 0 ile 6 arasında olmalıdır.")]
        public int OndalikHassasiyet { get; set; }
        
        /// <summary>
        /// Ana para birimi mi
        /// </summary>
        [Display(Name = "Ana Para Birimi Mi?")]
        public bool AnaParaBirimiMi { get; set; }
        
        /// <summary>
        /// Aktif mi
        /// </summary>
        [Display(Name = "Aktif")]
        public bool Aktif { get; set; }
        
        /// <summary>
        /// Sıralama değeri
        /// </summary>
        [Display(Name = "Sıra")]
        public int Sira { get; set; }
        
        /// <summary>
        /// Açıklama
        /// </summary>
        [Display(Name = "Açıklama")]
        [StringLength(500, ErrorMessage = "Açıklama en fazla 500 karakter olabilir.")]
        public string Aciklama { get; set; }
    }
    
    /// <summary>
    /// Kur değeri ViewModel'i (oluşturma ve düzenleme için)
    /// </summary>
    public class KurDegeriViewModel
    {
        /// <summary>
        /// Kur değeri ID
        /// </summary>
        public Guid KurDegeriID { get; set; }
        
        /// <summary>
        /// Para birimi ID
        /// </summary>
        [Required(ErrorMessage = "Para birimi seçilmelidir.")]
        [Display(Name = "Para Birimi")]
        public Guid ParaBirimiID { get; set; }
        
        /// <summary>
        /// Para birimi adı
        /// </summary>
        [Display(Name = "Para Birimi Adı")]
        public string ParaBirimiAdi { get; set; }
        
        /// <summary>
        /// Para birimi kodu
        /// </summary>
        [Display(Name = "Para Birimi Kodu")]
        public string ParaBirimiKodu { get; set; }
        
        /// <summary>
        /// Tarih
        /// </summary>
        [Required(ErrorMessage = "Tarih gereklidir.")]
        [Display(Name = "Tarih")]
        [DataType(DataType.Date)]
        public DateTime Tarih { get; set; } = DateTime.Today;
        
        /// <summary>
        /// Alış değeri
        /// </summary>
        [Required(ErrorMessage = "Alış kuru gereklidir.")]
        [Display(Name = "Alış")]
        [Range(0.000001, double.MaxValue, ErrorMessage = "Alış kuru 0'dan büyük olmalıdır.")]
        public decimal Alis { get; set; }
        
        /// <summary>
        /// Satış değeri
        /// </summary>
        [Required(ErrorMessage = "Satış kuru gereklidir.")]
        [Display(Name = "Satış")]
        [Range(0.000001, double.MaxValue, ErrorMessage = "Satış kuru 0'dan büyük olmalıdır.")]
        public decimal Satis { get; set; }
        
        /// <summary>
        /// Efektif alış değeri
        /// </summary>
        [Display(Name = "Efektif Alış")]
        [Range(0, double.MaxValue, ErrorMessage = "Efektif alış kuru negatif olamaz.")]
        public decimal Efektif_Alis { get; set; }
        
        /// <summary>
        /// Efektif satış değeri
        /// </summary>
        [Display(Name = "Efektif Satış")]
        [Range(0, double.MaxValue, ErrorMessage = "Efektif satış kuru negatif olamaz.")]
        public decimal Efektif_Satis { get; set; }
        
        /// <summary>
        /// Aktif mi
        /// </summary>
        [Display(Name = "Aktif")]
        public bool Aktif { get; set; } = true;
        
        /// <summary>
        /// Açıklama
        /// </summary>
        [Display(Name = "Açıklama")]
        [StringLength(500, ErrorMessage = "Açıklama en fazla 500 karakter olabilir.")]
        public string Aciklama { get; set; }
        
        /// <summary>
        /// Para birimleri listesi
        /// </summary>
        public List<SelectListItem> ParaBirimleri { get; set; }
    }
    
    /// <summary>
    /// Para birimi ilişkisi ViewModel'i
    /// </summary>
    public class ParaBirimiIliskiViewModel
    {
        /// <summary>
        /// Para birimi ilişkisi ID
        /// </summary>
        public Guid ParaBirimiIliskiID { get; set; }
        
        /// <summary>
        /// Kaynak para birimi ID
        /// </summary>
        [Required(ErrorMessage = "Kaynak para birimi seçilmelidir.")]
        [Display(Name = "Kaynak Para Birimi")]
        public Guid KaynakParaBirimiID { get; set; }
        
        /// <summary>
        /// Kaynak para birimi adı
        /// </summary>
        [Display(Name = "Kaynak Para Birimi Adı")]
        public string KaynakParaBirimiAdi { get; set; }
        
        /// <summary>
        /// Kaynak para birimi kodu
        /// </summary>
        [Display(Name = "Kaynak Para Birimi Kodu")]
        public string KaynakParaBirimiKodu { get; set; }
        
        /// <summary>
        /// Hedef para birimi ID
        /// </summary>
        [Required(ErrorMessage = "Hedef para birimi seçilmelidir.")]
        [Display(Name = "Hedef Para Birimi")]
        public Guid HedefParaBirimiID { get; set; }
        
        /// <summary>
        /// Hedef para birimi adı
        /// </summary>
        [Display(Name = "Hedef Para Birimi Adı")]
        public string HedefParaBirimiAdi { get; set; }
        
        /// <summary>
        /// Hedef para birimi kodu
        /// </summary>
        [Display(Name = "Hedef Para Birimi Kodu")]
        public string HedefParaBirimiKodu { get; set; }
        
        /// <summary>
        /// Aktif mi
        /// </summary>
        [Display(Name = "Aktif")]
        public bool Aktif { get; set; } = true;
        
        /// <summary>
        /// Açıklama
        /// </summary>
        [Display(Name = "Açıklama")]
        [StringLength(500, ErrorMessage = "Açıklama en fazla 500 karakter olabilir.")]
        public string Aciklama { get; set; }
        
        /// <summary>
        /// Para birimleri listesi
        /// </summary>
        public List<SelectListItem> ParaBirimleri { get; set; }
    }
} 