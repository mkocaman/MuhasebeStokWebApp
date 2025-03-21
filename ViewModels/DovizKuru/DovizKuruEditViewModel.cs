using System;
using System.ComponentModel.DataAnnotations;

namespace MuhasebeStokWebApp.ViewModels.DovizKuru
{
    public class DovizKuruEditViewModel
    {
        public Guid KurDegeriID { get; set; }
        
        public Guid ParaBirimiID { get; set; }
        
        [Display(Name = "Para Birimi Kodu")]
        public string ParaBirimiKodu { get; set; }
        
        [Display(Name = "Para Birimi Adı")]
        public string ParaBirimiAdi { get; set; }
        
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
        
        [Display(Name = "Aktif")]
        public bool Aktif { get; set; }
    }
} 