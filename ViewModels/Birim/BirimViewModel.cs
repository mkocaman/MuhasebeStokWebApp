using System;
using System.ComponentModel.DataAnnotations;
using System.Collections.Generic;

namespace MuhasebeStokWebApp.ViewModels.Birim
{
    public class BirimViewModel
    {
        public Guid BirimID { get; set; }
        
        [Display(Name = "Birim Adı")]
        [Required(ErrorMessage = "Birim adı zorunludur.")]
        [StringLength(50, ErrorMessage = "Birim adı en fazla 50 karakter olabilir.")]
        public string BirimAdi { get; set; }
        
        [Display(Name = "Birim Kodu")]
        [StringLength(20, ErrorMessage = "Birim kodu en fazla 20 karakter olabilir.")]
        public string BirimKodu { get; set; }
        
        [Display(Name = "Birim Sembol")]
        [StringLength(10, ErrorMessage = "Birim sembolü en fazla 10 karakter olabilir.")]
        public string BirimSembol { get; set; }
        
        [Display(Name = "Açıklama")]
        [StringLength(500, ErrorMessage = "Açıklama en fazla 500 karakter olabilir.")]
        public string? Aciklama { get; set; }
        
        [Display(Name = "Aktif")]
        public bool Aktif { get; set; }
        
        public DateTime OlusturmaTarihi { get; set; }
        
        // Silindi bilgisi eklenmiştir
        public bool Silindi { get; set; }
    }
    
    public class BirimCreateViewModel
    {
        [Required(ErrorMessage = "Birim adı zorunludur.")]
        [Display(Name = "Birim Adı")]
        [StringLength(50, ErrorMessage = "Birim adı en fazla 50 karakter olabilir.")]
        public string BirimAdi { get; set; }
        
        [Display(Name = "Açıklama")]
        [StringLength(500, ErrorMessage = "Açıklama en fazla 500 karakter olabilir.")]
        public string? Aciklama { get; set; }
        
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
        [StringLength(500, ErrorMessage = "Açıklama en fazla 500 karakter olabilir.")]
        public string? Aciklama { get; set; }
        
        [Display(Name = "Aktif")]
        public bool Aktif { get; set; }
    }
    
    public class BirimListViewModel
    {
        public List<BirimViewModel> Birimler { get; set; } = new List<BirimViewModel>();
    }
} 