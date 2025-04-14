using System;
using System.ComponentModel.DataAnnotations;

namespace MuhasebeStokWebApp.ViewModels.Depo
{
    public class DepoViewModel
    {
        public Guid DepoID { get; set; }
        
        [Display(Name = "Depo Adı")]
        [Required(ErrorMessage = "Depo adı zorunludur.")]
        [StringLength(100, ErrorMessage = "Depo adı en fazla 100 karakter olabilir.")]
        public string DepoAdi { get; set; }
        
        [Display(Name = "Adres")]
        [StringLength(250, ErrorMessage = "Adres en fazla 250 karakter olabilir.")]
        public string? Adres { get; set; }
        
        [Display(Name = "Aktif")]
        public bool Aktif { get; set; }
        
        [Display(Name = "Oluşturma Tarihi")]
        [DisplayFormat(DataFormatString = "{0:dd.MM.yyyy}", ApplyFormatInEditMode = false)]
        public DateTime? OlusturmaTarihi { get; set; }
        
        public bool Silindi { get; set; }
    }
    
    public class DepoCreateViewModel
    {
        [Required(ErrorMessage = "Depo adı zorunludur.")]
        [Display(Name = "Depo Adı")]
        [StringLength(100, ErrorMessage = "Depo adı en fazla 100 karakter olabilir.")]
        public string DepoAdi { get; set; }
        
        [Display(Name = "Adres")]
        [StringLength(250, ErrorMessage = "Adres en fazla 250 karakter olabilir.")]
        public string? Adres { get; set; }
        
        [Display(Name = "Aktif")]
        public bool Aktif { get; set; }
    }
    
    public class DepoEditViewModel
    {
        public Guid DepoID { get; set; }
        
        [Required(ErrorMessage = "Depo adı zorunludur.")]
        [Display(Name = "Depo Adı")]
        [StringLength(100, ErrorMessage = "Depo adı en fazla 100 karakter olabilir.")]
        public string DepoAdi { get; set; }
        
        [Display(Name = "Adres")]
        [StringLength(250, ErrorMessage = "Adres en fazla 250 karakter olabilir.")]
        public string? Adres { get; set; }
        
        [Display(Name = "Aktif")]
        public bool Aktif { get; set; }
    }
} 