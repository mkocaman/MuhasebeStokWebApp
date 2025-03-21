using System;
using System.ComponentModel.DataAnnotations;

namespace MuhasebeStokWebApp.ViewModels.DovizKuru
{
    public class DovizKuruViewModel
    {
        public Guid KurDegeriID { get; set; }
        
        public Guid ParaBirimiID { get; set; }
        
        [Display(Name = "Para Birimi Kodu")]
        public string ParaBirimiKodu { get; set; }
        
        [Display(Name = "Para Birimi Adı")]
        public string ParaBirimiAdi { get; set; }
        
        [Display(Name = "Alış Değeri")]
        [DisplayFormat(DataFormatString = "{0:N4}", ApplyFormatInEditMode = true)]
        public decimal AlisDegeri { get; set; }
        
        [Display(Name = "Satış Değeri")]
        [DisplayFormat(DataFormatString = "{0:N4}", ApplyFormatInEditMode = true)]
        public decimal SatisDegeri { get; set; }
        
        [Display(Name = "Tarih")]
        [DisplayFormat(DataFormatString = "{0:dd.MM.yyyy}", ApplyFormatInEditMode = true)]
        public DateTime Tarih { get; set; }
        
        [Display(Name = "Kaynak")]
        public string Kaynak { get; set; }
        
        [Display(Name = "Aktif")]
        public bool Aktif { get; set; }
    }
} 