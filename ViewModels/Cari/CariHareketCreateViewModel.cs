using System;
using System.ComponentModel.DataAnnotations;

namespace MuhasebeStokWebApp.ViewModels.Cari
{
    public class CariHareketCreateViewModel
    {
        public Guid CariID { get; set; }
        
        [Display(Name = "Cari Adı")]
        public string CariAdi { get; set; }
        
        [Required(ErrorMessage = "Tutar zorunludur.")]
        [Range(0.01, double.MaxValue, ErrorMessage = "Tutar 0'dan büyük olmalıdır.")]
        [Display(Name = "Tutar")]
        [DisplayFormat(DataFormatString = "{0:N2}", ApplyFormatInEditMode = false)]
        public decimal Tutar { get; set; }
        
        [Required(ErrorMessage = "Hareket türü zorunludur.")]
        [Display(Name = "Hareket Türü")]
        public string HareketTuru { get; set; }
        
        [Required(ErrorMessage = "Tarih zorunludur.")]
        [DataType(DataType.Date)]
        [Display(Name = "Tarih")]
        public DateTime Tarih { get; set; } = DateTime.Now;
        
        [StringLength(50, ErrorMessage = "Referans no en fazla 50 karakter olabilir.")]
        [Display(Name = "Referans No")]
        public string ReferansNo { get; set; }
        
        [StringLength(50, ErrorMessage = "Referans türü en fazla 50 karakter olabilir.")]
        [Display(Name = "Referans Türü")]
        public string ReferansTuru { get; set; }
        
        [Display(Name = "Referans ID")]
        public Guid? ReferansID { get; set; }
        
        [StringLength(500, ErrorMessage = "Açıklama en fazla 500 karakter olabilir.")]
        [Display(Name = "Açıklama")]
        public string Aciklama { get; set; }
    }
} 