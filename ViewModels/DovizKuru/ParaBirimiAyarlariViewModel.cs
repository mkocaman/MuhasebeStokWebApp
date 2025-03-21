using System;
using System.Collections.Generic;
using MuhasebeStokWebApp.ViewModels.DovizKuru;

namespace MuhasebeStokWebApp.ViewModels.DovizKuru
{
    public class ParaBirimiAyarlariViewModel
    {
        public List<ParaBirimiViewModel> ParaBirimleri { get; set; } = new List<ParaBirimiViewModel>();
    }
    
    public class ParaBirimiKurDegerleriViewModel
    {
        public Guid ParaBirimiID { get; set; }
        
        public string ParaBirimiKodu { get; set; } = string.Empty;
        
        public string ParaBirimiAdi { get; set; } = string.Empty;
        
        public string ParaBirimiSembol { get; set; } = string.Empty;
        
        public List<DovizKuruViewModel> KurDegerleri { get; set; } = new List<DovizKuruViewModel>();
    }
} 