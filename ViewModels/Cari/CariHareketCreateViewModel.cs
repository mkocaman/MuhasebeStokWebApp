using System;
using System.ComponentModel.DataAnnotations;

namespace MuhasebeStokWebApp.ViewModels.Cari
{
    public class CariHareketCreateViewModel
    {
        [Required]
        public Guid CariID { get; set; } = Guid.Empty;
        
        [Display(Name = "Cari Adı")]
        [Required(ErrorMessage = "Cari adı zorunludur")]
        public string CariAdi { get; set; } = string.Empty;
        
        [Required(ErrorMessage = "Tutar girilmelidir")]
        [Range(0.01, double.MaxValue, ErrorMessage = "Tutar 0'dan büyük olmalıdır")]
        [Display(Name = "Tutar")]
        [DisplayFormat(DataFormatString = "{0:N2}", ApplyFormatInEditMode = false)]
        public decimal Tutar { get; set; }
        
        [Required(ErrorMessage = "Hareket türü zorunludur.")]
        [Display(Name = "Hareket Türü")]
        public string HareketTuru { get; set; } = string.Empty;
        
        [Required(ErrorMessage = "Tarih seçilmelidir")]
        [Display(Name = "Tarih")]
        [DataType(DataType.Date)]
        public DateTime Tarih { get; set; } = DateTime.Now.Date;
        
        [Display(Name = "Vade Tarihi")]
        [DataType(DataType.Date)]
        public DateTime? VadeTarihi { get; set; }
        
        [Display(Name = "Referans No")]
        public string? ReferansNo { get; set; }
        
        [Display(Name = "Referans Türü")]
        public string? ReferansTuru { get; set; }
        
        [Display(Name = "Referans ID")]
        public Guid? ReferansID { get; set; }
        
        [StringLength(500, ErrorMessage = "Açıklama en fazla 500 karakter olabilir.")]
        [Display(Name = "Açıklama")]
        public string? Aciklama { get; set; }
    }
}