using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace MuhasebeStokWebApp.ViewModels.Doviz
{
    public class DovizKuruViewModel
    {
        public Guid DovizKuruID { get; set; }
        
        [Display(Name = "Döviz Kodu")]
        public required string DovizKodu { get; set; }
        
        [Display(Name = "Döviz Adı")]
        public required string DovizAdi { get; set; }
        
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
    }
    
    public class SistemAyarlariViewModel
    {
        public int SistemAyarlariID { get; set; }
        
        [Required(ErrorMessage = "Ana döviz kodu zorunludur.")]
        [Display(Name = "Ana Döviz Kodu")]
        public required string AnaDovizKodu { get; set; }
        
        [Required(ErrorMessage = "Şirket adı zorunludur.")]
        [Display(Name = "Şirket Adı")]
        public required string SirketAdi { get; set; }
        
        [Display(Name = "Şirket Adresi")]
        public string? SirketAdresi { get; set; }
        
        [Display(Name = "Şirket Telefon")]
        public string? SirketTelefon { get; set; }
        
        [Display(Name = "Şirket E-posta")]
        [EmailAddress(ErrorMessage = "Geçerli bir e-posta adresi giriniz.")]
        public string? SirketEmail { get; set; }
        
        [Display(Name = "Vergi No")]
        public string? SirketVergiNo { get; set; }
        
        [Display(Name = "Vergi Dairesi")]
        public string? SirketVergiDairesi { get; set; }
        
        [Display(Name = "Otomatik Döviz Güncelleme")]
        public bool OtomatikDovizGuncelleme { get; set; }
        
        [Display(Name = "Güncelleme Sıklığı (Saat)")]
        [Range(1, 168, ErrorMessage = "Güncelleme sıklığı 1-168 saat arasında olmalıdır.")]
        public int DovizGuncellemeSikligi { get; set; }
        
        [Display(Name = "Son Güncelleme")]
        [DisplayFormat(DataFormatString = "{0:dd.MM.yyyy HH:mm}", ApplyFormatInEditMode = false)]
        public DateTime SonDovizGuncellemeTarihi { get; set; }
    }
    
    public class DovizCeviriViewModel
    {
        [Required(ErrorMessage = "Kaynak döviz kodu zorunludur.")]
        [Display(Name = "Kaynak Döviz")]
        public required string KaynakDovizKodu { get; set; }
        
        [Required(ErrorMessage = "Hedef döviz kodu zorunludur.")]
        [Display(Name = "Hedef Döviz")]
        public required string HedefDovizKodu { get; set; }
        
        [Required(ErrorMessage = "Tutar zorunludur.")]
        [Display(Name = "Tutar")]
        [Range(0.01, double.MaxValue, ErrorMessage = "Tutar 0'dan büyük olmalıdır.")]
        public decimal Tutar { get; set; }
        
        [Display(Name = "Sonuç")]
        [DisplayFormat(DataFormatString = "{0:N2}", ApplyFormatInEditMode = false)]
        public decimal Sonuc { get; set; }
        
        [Display(Name = "Kullanılan Kur")]
        [DisplayFormat(DataFormatString = "{0:N6}", ApplyFormatInEditMode = false)]
        public decimal KullanilanKur { get; set; }
    }
} 