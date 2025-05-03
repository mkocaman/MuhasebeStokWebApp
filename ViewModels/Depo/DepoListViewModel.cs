using System;
using System.Collections.Generic;
using MuhasebeStokWebApp.ViewModels.Depo;

namespace MuhasebeStokWebApp.ViewModels.Depo
{
    public class DepoListViewModel
    {
        public List<DepoViewModel> Depolar { get; set; } = new List<DepoViewModel>();
        public string AktifSekme { get; set; } = "aktif";
    }
} 