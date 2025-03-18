using System;
using System.ComponentModel.DataAnnotations;

namespace MuhasebeStokWebApp.ViewModels
{
    public class DovizCeviriciViewModel
    {
        [Required(ErrorMessage = "Kaynak para birimi gereklidir.")]
        [Display(Name = "Kaynak Para Birimi")]
        public required string KaynakParaBirimi { get; set; } = "TRY";
        
        [Required(ErrorMessage = "Hedef para birimi gereklidir.")]
        [Display(Name = "Hedef Para Birimi")]
        public required string HedefParaBirimi { get; set; } = "USD";
        
        [Required(ErrorMessage = "Miktar gereklidir.")]
        [Range(0.01, double.MaxValue, ErrorMessage = "Miktar 0'dan büyük olmalıdır.")]
        [Display(Name = "Miktar")]
        public decimal Miktar { get; set; } = 1;
        
        [Display(Name = "Sonuç")]
        public decimal Sonuc { get; set; }
        
        [Display(Name = "Kullanılan Kur")]
        public decimal KullanılanKur { get; set; }
        
        public bool SonucGoster { get; set; } = false;
    }
    
    public class DovizAyarlariViewModel
    {
        [Required(ErrorMessage = "Varsayılan para birimi gereklidir.")]
        [Display(Name = "Varsayılan Para Birimi")]
        public required string VarsayilanParaBirimi { get; set; } = "TRY";
        
        [Display(Name = "Otomatik Güncelleme")]
        public bool OtomatikGuncelleme { get; set; } = true;
        
        [Required(ErrorMessage = "Güncelleme aralığı gereklidir.")]
        [Range(1, 168, ErrorMessage = "Güncelleme aralığı 1-168 saat arasında olmalıdır.")]
        [Display(Name = "Güncelleme Aralığı (Saat)")]
        public int GuncellemeAraligi { get; set; } = 24;
    }
} 