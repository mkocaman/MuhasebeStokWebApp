using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using MuhasebeStokWebApp.Data.Entities.ParaBirimiModulu;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore.Metadata.Internal;

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
        public Guid KurDegeriID { get; set; } = Guid.Empty;
        
        /// <summary>
        /// Para birimi ID
        /// </summary>
        [Required(ErrorMessage = "Para Birimi seçimi zorunludur.")]
        public Guid ParaBirimiID { get; set; }
        
        /// <summary>
        /// Para birimi adı
        /// </summary>
        public string? ParaBirimiAdi { get; set; }
        
        /// <summary>
        /// Para birimi nesnesi
        /// </summary>
        public MuhasebeStokWebApp.Data.Entities.ParaBirimiModulu.ParaBirimi? ParaBirimi { get; set; }
        
        /// <summary>
        /// Para birimleri listesi
        /// </summary>
        public List<SelectListItem>? ParaBirimleri { get; set; }
        
        /// <summary>
        /// Tarih
        /// </summary>
        [Required(ErrorMessage = "Tarih seçimi zorunludur.")]
        public DateTime Tarih { get; set; } = DateTime.Now;
        
        /// <summary>
        /// Alış değeri
        /// </summary>
        [Required(ErrorMessage = "Alış kuru zorunludur.")]
        [Display(Name = "Alış Kuru")]
        [Range(0.01, double.MaxValue, ErrorMessage = "Alış kuru 0'dan büyük olmalıdır.")]
        public decimal Alis { get; set; }
        
        /// <summary>
        /// Satış değeri
        /// </summary>
        [Required(ErrorMessage = "Satış kuru zorunludur.")]
        [Display(Name = "Satış Kuru")]
        [Range(0.01, double.MaxValue, ErrorMessage = "Satış kuru 0'dan büyük olmalıdır.")]
        public decimal Satis { get; set; }
        
        /// <summary>
        /// Aktif mi
        /// </summary>
        [Display(Name = "Aktif")]
        public bool Aktif { get; set; } = true;
        
        /// <summary>
        /// Silindi mi (soft delete için)
        /// </summary>
        [Display(Name = "Silindi")]
        public bool Silindi { get; set; } = false;
        
        /// <summary>
        /// Açıklama
        /// </summary>
        [MaxLength(500, ErrorMessage = "Açıklama en fazla 500 karakter olabilir.")]
        public string? Aciklama { get; set; }
        
        /// <summary>
        /// Oluşturulma tarihi
        /// </summary>
        [Display(Name = "Oluşturulma Tarihi")]
        [DisplayFormat(DataFormatString = "{0:dd.MM.yyyy HH:mm}", ApplyFormatInEditMode = true)]
        public DateTime OlusturmaTarihi { get; set; } = DateTime.Now;
        
        /// <summary>
        /// Son güncellenme tarihi
        /// </summary>
        [Display(Name = "Son Güncellenme Tarihi")]
        [DisplayFormat(DataFormatString = "{0:dd.MM.yyyy HH:mm}", ApplyFormatInEditMode = true)]
        public DateTime? GuncellemeTarihi { get; set; }
        
        /// <summary>
        /// Oluşturan kullanıcı ID'si
        /// </summary>
        public string? OlusturanKullaniciID { get; set; }
        
        /// <summary>
        /// Son güncelleyen kullanıcı ID'si
        /// </summary>
        public string? SonGuncelleyenKullaniciID { get; set; }
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
    
    /// <summary>
    /// Kur marjı ViewModel'i
    /// </summary>
    public class KurMarjViewModel
    {
        /// <summary>
        /// Kur marj ID
        /// </summary>
        public Guid KurMarjID { get; set; }
        
        /// <summary>
        /// Alış-Satış kuru arasındaki marj yüzdesi
        /// </summary>
        [Required(ErrorMessage = "Satış marjı gereklidir.")]
        [Display(Name = "Satış Marjı (%)")]
        [Range(0, 100, ErrorMessage = "Satış marjı 0 ile 100 arasında olmalıdır.")]
        public decimal SatisMarji { get; set; } = 2.00m; // Varsayılan olarak %2
        
        /// <summary>
        /// Varsayılan ayar mı
        /// </summary>
        [Display(Name = "Varsayılan")]
        public bool Varsayilan { get; set; } = true;
        
        /// <summary>
        /// Tanım / Açıklama
        /// </summary>
        [Required(ErrorMessage = "Tanım gereklidir.")]
        [Display(Name = "Tanım")]
        [StringLength(100, ErrorMessage = "Tanım en fazla 100 karakter olabilir.")]
        public string Tanim { get; set; } = "Varsayılan Kur Marjı";
        
        /// <summary>
        /// Aktif mi
        /// </summary>
        [Display(Name = "Aktif")]
        public bool Aktif { get; set; } = true;
        
        /// <summary>
        /// Silindi mi (soft delete için)
        /// </summary>
        [Display(Name = "Silindi")]
        public bool Silindi { get; set; } = false;
        
        /// <summary>
        /// Oluşturulma tarihi
        /// </summary>
        [Display(Name = "Oluşturulma Tarihi")]
        [DisplayFormat(DataFormatString = "{0:dd.MM.yyyy HH:mm}", ApplyFormatInEditMode = true)]
        public DateTime OlusturmaTarihi { get; set; } = DateTime.Now;
        
        /// <summary>
        /// Son güncellenme tarihi
        /// </summary>
        [Display(Name = "Son Güncellenme Tarihi")]
        [DisplayFormat(DataFormatString = "{0:dd.MM.yyyy HH:mm}", ApplyFormatInEditMode = true)]
        public DateTime? GuncellemeTarihi { get; set; }
        
        /// <summary>
        /// Oluşturan kullanıcı ID'si
        /// </summary>
        public string? OlusturanKullaniciID { get; set; }
        
        /// <summary>
        /// Son güncelleyen kullanıcı ID'si
        /// </summary>
        public string? SonGuncelleyenKullaniciID { get; set; }
    }
} 