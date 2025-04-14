using System;
using System.ComponentModel.DataAnnotations;

namespace MuhasebeStokWebApp.ViewModels.Fatura
{
    public class FaturaKalemiAklamaViewModel
    {
        public Guid FaturaKalemID { get; set; }
        
        public Guid FaturaID { get; set; }
        
        [Display(Name = "Ürün Adı")]
        public string UrunAdi { get; set; } = "";
        
        [Display(Name = "Ürün Kodu")]
        public string UrunKodu { get; set; } = "";
        
        [Display(Name = "Fatura No")]
        public string FaturaNo { get; set; } = "";
        
        [Display(Name = "Cari Adı")]
        public string CariAdi { get; set; } = "";
        
        [Display(Name = "Miktar")]
        public decimal Miktar { get; set; }
        
        [Display(Name = "Aklanan Miktar")]
        public decimal AklananMiktar { get; set; }
        
        [Display(Name = "Kalan Miktar")]
        public decimal KalanMiktar { get; set; }
        
        [Required(ErrorMessage = "Sözleşme seçilmelidir.")]
        [Display(Name = "Sözleşme")]
        public Guid SozlesmeID { get; set; }
        
        [Display(Name = "Sözleşme No")]
        public string SozlesmeNo { get; set; } = "";
        
        [Required(ErrorMessage = "Aklanacak miktar gereklidir.")]
        [Range(0.001, double.MaxValue, ErrorMessage = "Aklanacak miktar pozitif olmalıdır.")]
        [Display(Name = "Aklanacak Miktar")]
        public decimal AklanacakMiktar { get; set; }
        
        [Required(ErrorMessage = "Aklama notu zorunludur.")]
        [StringLength(500, ErrorMessage = "Aklama notu en fazla 500 karakter olabilir.")]
        [Display(Name = "Aklama Notu")]
        public string AklanmaNotu { get; set; } = "";
    }
} 