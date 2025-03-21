using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace MuhasebeStokWebApp.ViewModels.DovizKuru
{
    public class ParaBirimiKurlariViewModel
    {
        public Guid ParaBirimiID { get; set; }
        
        [Display(Name = "Para Birimi Kodu")]
        public string ParaBirimiKodu { get; set; }
        
        [Display(Name = "Para Birimi Adı")]
        public string ParaBirimiAdi { get; set; }
        
        public List<DovizKuruViewModel> KurDegerleri { get; set; } = new List<DovizKuruViewModel>();
    }
} 