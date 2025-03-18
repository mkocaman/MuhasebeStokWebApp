using System;
using System.ComponentModel.DataAnnotations;

namespace MuhasebeStokWebApp.ViewModels.Birim
{
    public class BirimViewModel
    {
        public Guid BirimID { get; set; }
        
        [Display(Name = "Birim Adı")]
        public string BirimAdi { get; set; }
        
        [Display(Name = "Açıklama")]
        public string Aciklama { get; set; }
        
        [Display(Name = "Aktif")]
        public bool Aktif { get; set; }
    }
    
    public class BirimCreateViewModel
    {
        [Required(ErrorMessage = "Birim adı zorunludur.")]
        [Display(Name = "Birim Adı")]
        [StringLength(50, ErrorMessage = "Birim adı en fazla 50 karakter olabilir.")]
        public string BirimAdi { get; set; }
        
        [Display(Name = "Açıklama")]
        [StringLength(200, ErrorMessage = "Açıklama en fazla 200 karakter olabilir.")]
        public string Aciklama { get; set; }
        
        [Display(Name = "Aktif")]
        public bool Aktif { get; set; } = true;
    }
    
    public class BirimEditViewModel
    {
        public Guid BirimID { get; set; }
        
        [Required(ErrorMessage = "Birim adı zorunludur.")]
        [Display(Name = "Birim Adı")]
        [StringLength(50, ErrorMessage = "Birim adı en fazla 50 karakter olabilir.")]
        public string BirimAdi { get; set; }
        
        [Display(Name = "Açıklama")]
        [StringLength(200, ErrorMessage = "Açıklama en fazla 200 karakter olabilir.")]
        public string Aciklama { get; set; }
        
        [Display(Name = "Aktif")]
        public bool Aktif { get; set; }
    }
} 