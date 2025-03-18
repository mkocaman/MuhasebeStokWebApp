using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace MuhasebeStokWebApp.ViewModels.Kur
{
    public class KurCeviriciViewModel
    {
        [Required(ErrorMessage = "Kaynak para birimi zorunludur.")]
        [Display(Name = "Kaynak Para Birimi")]
        public int KaynakParaBirimiID { get; set; }
        
        [Display(Name = "Kaynak Para Birimi")]
        public string KaynakParaBirimiKodu { get; set; }
        
        [Required(ErrorMessage = "Hedef para birimi zorunludur.")]
        [Display(Name = "Hedef Para Birimi")]
        public int HedefParaBirimiID { get; set; }
        
        [Display(Name = "Hedef Para Birimi")]
        public string HedefParaBirimiKodu { get; set; }
        
        [Required(ErrorMessage = "Miktar zorunludur.")]
        [Range(0.01, double.MaxValue, ErrorMessage = "Miktar 0'dan büyük olmalıdır.")]
        [Display(Name = "Miktar")]
        public decimal Miktar { get; set; } = 1;
        
        [Display(Name = "Sonuç")]
        public decimal Sonuc { get; set; }
        
        [Display(Name = "Kullanılan Kur")]
        public decimal KullanilanKur { get; set; }
        
        [Display(Name = "Tarih")]
        public DateTime Tarih { get; set; } = DateTime.Now;
        
        public bool SonucGoster { get; set; } = false;
        
        // Dropdown listeler için
        public List<SelectListItem> ParaBirimleri { get; set; }
    }
} 