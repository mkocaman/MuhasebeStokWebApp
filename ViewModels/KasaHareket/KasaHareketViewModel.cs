using System;
using System.ComponentModel.DataAnnotations;

namespace ViewModels.KasaHareket
{
    public class KasaHareketViewModel
    {
        [Display(Name = "Cari ID")]
        public Guid? CariID { get; set; }
        
        [Display(Name = "Cari Adı")]
        public string? CariAdi { get; set; }
    }
} 