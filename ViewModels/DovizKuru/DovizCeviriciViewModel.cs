using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace MuhasebeStokWebApp.ViewModels.DovizKuru
{
    public class DovizCeviriciViewModel
    {
        [Required(ErrorMessage = "Miktar gereklidir.")]
        [Range(0.01, 1000000000, ErrorMessage = "Miktar 0.01 ile 1000000000 arasında olmalıdır.")]
        [Display(Name = "Miktar")]
        public decimal Miktar { get; set; }
        
        [Required(ErrorMessage = "Kaynak para birimi gereklidir.")]
        [Display(Name = "Kaynak Para Birimi")]
        public string KaynakParaBirimiKodu { get; set; }
        
        [Required(ErrorMessage = "Hedef para birimi gereklidir.")]
        [Display(Name = "Hedef Para Birimi")]
        public string HedefParaBirimiKodu { get; set; }
        
        [Display(Name = "Sonuç")]
        [DisplayFormat(DataFormatString = "{0:N4}")]
        public decimal Sonuc { get; set; }
        
        [Display(Name = "Kur Değeri")]
        [DisplayFormat(DataFormatString = "{0:N4}")]
        public decimal KurDegeri { get; set; }
        
        public List<SelectListItem> ParaBirimleri { get; set; }
    }
} 