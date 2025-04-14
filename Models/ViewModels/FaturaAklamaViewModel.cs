using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using MuhasebeStokWebApp.Data.Entities;

namespace MuhasebeStokWebApp.Models.ViewModels
{
    public class FaturaAklamaViewModel
    {
        public Guid FaturaID { get; set; }
        public string FaturaNumarasi { get; set; } = "";
        public DateTime? FaturaTarihi { get; set; }
        public string CariAdi { get; set; } = "";
        public decimal? AraToplam { get; set; }
        public decimal? KDVToplam { get; set; }
        public decimal? GenelToplam { get; set; }
        public string DovizTuru { get; set; } = "";
        public decimal? DovizKuru { get; set; }
        public bool? ResmiMi { get; set; }
        public string OdemeDurumu { get; set; } = "";
        
        public List<FaturaKalemAklamaViewModel> FaturaKalemleri { get; set; }
        
        public Guid? SecilenSozlesmeID { get; set; }
        public List<Sozlesme> KullanilabilirSozlesmeler { get; set; }
        
        [Required(ErrorMessage = "Aklama notu zorunludur.")]
        [StringLength(500, ErrorMessage = "Aklama notu 500 karakterden fazla olamaz.")]
        public string AklamaNotu { get; set; } = "";
        
        public FaturaAklamaViewModel()
        {
            FaturaKalemleri = new List<FaturaKalemAklamaViewModel>();
            KullanilabilirSozlesmeler = new List<Sozlesme>();
        }
    }
    
    public class FaturaKalemAklamaViewModel
    {
        public Guid FaturaKalemID { get; set; }
        public Guid UrunID { get; set; }
        public string UrunAdi { get; set; } = "";
        public string UrunKodu { get; set; } = "";
        public decimal? Miktar { get; set; }
        public string BirimAdi { get; set; } = "";
        public decimal? BirimFiyat { get; set; }
        public decimal? ToplamTutar { get; set; }
        
        public decimal? AklananMiktar { get; set; }
        public decimal? KalanMiktar { get; set; }
        
        [Required(ErrorMessage = "Aklanacak miktar zorunludur.")]
        [Range(0.001, double.MaxValue, ErrorMessage = "Aklanacak miktar 0'dan büyük olmalıdır.")]
        public decimal YeniAklanacakMiktar { get; set; }
        
        public bool TamamiAklaniyor { get; set; }
        public bool Secildi { get; set; }
    }
} 