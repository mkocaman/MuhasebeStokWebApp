using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace MuhasebeStokWebApp.ViewModels.Kur
{
    public class KurCeviriciViewModel
    {
        [Display(Name = "Kaynak Para Birimi")]
        [Required(ErrorMessage = "Kaynak para birimi seçiniz")]
        public Guid KaynakParaBirimiID { get; set; }
        
        [Display(Name = "Kaynak Para Birimi Kodu")]
        public string KaynakParaBirimiKodu { get; set; }
        
        [Display(Name = "Hedef Para Birimi")]
        [Required(ErrorMessage = "Hedef para birimi seçiniz")]
        public Guid HedefParaBirimiID { get; set; }
        
        [Display(Name = "Hedef Para Birimi Kodu")]
        public string HedefParaBirimiKodu { get; set; }
        
        [Display(Name = "Miktar")]
        [Required(ErrorMessage = "Miktar giriniz")]
        [Range(0.01, double.MaxValue, ErrorMessage = "Miktar 0'dan büyük olmalıdır")]
        public decimal Miktar { get; set; } = 1;
        
        [Display(Name = "Sonuç")]
        public decimal Sonuc { get; set; }
        
        [Display(Name = "Kullanılan Kur")]
        public decimal KullanilanKur { get; set; }
        
        [Display(Name = "Tarih")]
        public DateTime Tarih { get; set; } = DateTime.Now;
        
        [Display(Name = "Sonuç Göster")]
        public bool SonucGoster { get; set; }
        
        // Para birimleri listesi
        public List<SelectListItem> ParaBirimleri { get; set; }
    }
} 