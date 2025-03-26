using System;
using System.ComponentModel.DataAnnotations;
using System.Collections.Generic;

namespace MuhasebeStokWebApp.ViewModels.UrunFiyat
{
    public class UrunFiyatViewModel
    {
        public Guid FiyatID { get; set; }
        
        [Required(ErrorMessage = "Ürün seçimi zorunludur.")]
        [Display(Name = "Ürün")]
        public Guid? UrunID { get; set; }
        
        [Display(Name = "Ürün Kodu")]
        public string UrunKodu { get; set; }
        
        [Display(Name = "Ürün Adı")]
        public string UrunAdi { get; set; }
        
        [Required(ErrorMessage = "Fiyat zorunludur.")]
        [Range(0.01, double.MaxValue, ErrorMessage = "Fiyat 0'dan büyük olmalıdır.")]
        [Display(Name = "Fiyat")]
        public decimal Fiyat { get; set; }
        
        [Required(ErrorMessage = "Geçerlilik tarihi zorunludur.")]
        [Display(Name = "Geçerlilik Tarihi")]
        [DataType(DataType.Date)]
        public DateTime GecerliTarih { get; set; } = DateTime.Now;
        
        [Required(ErrorMessage = "Fiyat tipi zorunludur.")]
        [Display(Name = "Fiyat Tipi")]
        public int? FiyatTipiID { get; set; }
        
        [Display(Name = "Fiyat Tipi Adı")]
        public string FiyatTipiAdi { get; set; }
        
        [Display(Name = "Oluşturma Tarihi")]
        public DateTime? OlusturmaTarihi { get; set; }
        
        [Display(Name = "Aktif")]
        public bool Aktif { get; set; } = true;
    }
    
    public class UrunFiyatListViewModel
    {
        public List<UrunFiyatViewModel> UrunFiyatlari { get; set; } = new List<UrunFiyatViewModel>();
        public Guid? SelectedUrunID { get; set; }
        public string UrunAdi { get; set; }
    }
    
    public class UrunFiyatCreateViewModel
    {
        public Guid? UrunID { get; set; }
        
        [Required(ErrorMessage = "Fiyat zorunludur.")]
        [Range(0.01, double.MaxValue, ErrorMessage = "Fiyat 0'dan büyük olmalıdır.")]
        [Display(Name = "Fiyat")]
        public decimal Fiyat { get; set; }
        
        [Required(ErrorMessage = "Geçerlilik tarihi zorunludur.")]
        [Display(Name = "Geçerlilik Tarihi")]
        [DataType(DataType.Date)]
        public DateTime GecerliTarih { get; set; } = DateTime.Now;
        
        [Required(ErrorMessage = "Fiyat tipi zorunludur.")]
        [Display(Name = "Fiyat Tipi")]
        public int? FiyatTipiID { get; set; }
    }
}