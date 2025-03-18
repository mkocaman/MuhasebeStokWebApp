using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using MuhasebeStokWebApp.Data.Entities;

namespace MuhasebeStokWebApp.ViewModels.Kur
{
    public class KurIndexViewModel
    {
        public List<KurDegeriViewModel> KurDegerleri { get; set; } = new List<KurDegeriViewModel>();
        
        public KurAyarlariViewModel? KurAyarlari { get; set; }
        
        [Display(Name = "Baz Para Birimi")]
        public string BazParaBirimiKodu { get; set; } = "USD";
        
        [Display(Name = "İkinci Para Birimi")]
        public string IkinciParaBirimiKodu { get; set; } = "UZS";
        
        [Display(Name = "Üçüncü Para Birimi")]
        public string UcuncuParaBirimiKodu { get; set; } = "TRY";
        
        [Display(Name = "Son Güncelleme Tarihi")]
        [DisplayFormat(DataFormatString = "{0:dd.MM.yyyy HH:mm}", ApplyFormatInEditMode = false)]
        public DateTime SonGuncellemeTarihi { get; set; } = DateTime.Now;
        
        [Display(Name = "Otomatik Güncelleme")]
        public bool OtomatikGuncelleme { get; set; } = true;
        
        [Display(Name = "Güncelleme Sıklığı (Saat)")]
        public int GuncellemeSikligi { get; set; } = 24;
    }
} 