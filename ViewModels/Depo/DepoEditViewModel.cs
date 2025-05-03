using System;
using System.ComponentModel.DataAnnotations;

namespace MuhasebeStokWebApp.ViewModels.Depo
{
    public class DepoEditViewModel
    {
        public Guid DepoID { get; set; }
        
        [Required(ErrorMessage = "Depo adı zorunludur.")]
        [Display(Name = "Depo Adı")]
        [StringLength(50, ErrorMessage = "Depo adı en fazla 50 karakter olabilir.")]
        public string DepoAdi { get; set; }
        
        [Display(Name = "Depo Kodu")]
        [StringLength(50, ErrorMessage = "Depo kodu en fazla 50 karakter olabilir.")]
        public string? DepoKodu { get; set; }
        
        [Display(Name = "Açıklama")]
        [StringLength(500, ErrorMessage = "Açıklama en fazla 500 karakter olabilir.")]
        public string? Aciklama { get; set; }
        
        [Display(Name = "Adres")]
        [StringLength(200, ErrorMessage = "Adres en fazla 200 karakter olabilir.")]
        public string Adres { get; set; }
        
        [Display(Name = "Aktif")]
        public bool Aktif { get; set; }
    }
} 