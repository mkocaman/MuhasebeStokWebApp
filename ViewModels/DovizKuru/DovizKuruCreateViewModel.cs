using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace MuhasebeStokWebApp.ViewModels.DovizKuru
{
    public class DovizKuruCreateViewModel
    {
        [Required(ErrorMessage = "Para birimi seçiniz.")]
        [Display(Name = "Para Birimi")]
        public Guid ParaBirimiID { get; set; }
        
        [Required(ErrorMessage = "Alış değeri gereklidir.")]
        [Range(0.0001, 1000000, ErrorMessage = "Alış değeri 0.0001 ile 1000000 arasında olmalıdır.")]
        [Display(Name = "Alış Değeri")]
        public decimal AlisDegeri { get; set; }
        
        [Required(ErrorMessage = "Satış değeri gereklidir.")]
        [Range(0.0001, 1000000, ErrorMessage = "Satış değeri 0.0001 ile 1000000 arasında olmalıdır.")]
        [Display(Name = "Satış Değeri")]
        public decimal SatisDegeri { get; set; }
        
        [Required(ErrorMessage = "Tarih gereklidir.")]
        [Display(Name = "Tarih")]
        [DisplayFormat(DataFormatString = "{0:dd.MM.yyyy}", ApplyFormatInEditMode = true)]
        public DateTime Tarih { get; set; }
        
        [Required(ErrorMessage = "Kaynak gereklidir.")]
        [Display(Name = "Kaynak")]
        [StringLength(50, ErrorMessage = "Kaynak en fazla 50 karakter olabilir.")]
        public string Kaynak { get; set; }
        
        public List<SelectListItem> ParaBirimleri { get; set; }
    }
} 