using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace MuhasebeStokWebApp.ViewModels.Banka
{
    public class BankaViewModel
    {
        public Guid BankaID { get; set; }
        
        [Required]
        [Display(Name = "Banka Adı")]
        public required string BankaAdi { get; set; }
        
        [Display(Name = "Aktif")]
        public bool Aktif { get; set; }
        
        [Display(Name = "Oluşturma Tarihi")]
        [DisplayFormat(DataFormatString = "{0:dd.MM.yyyy}", ApplyFormatInEditMode = false)]
        public DateTime OlusturmaTarihi { get; set; }
    }
    
    public class BankaListViewModel
    {
        public List<BankaViewModel> Bankalar { get; set; } = new List<BankaViewModel>();
    }
    
    public class BankaCreateViewModel
    {
        [Required(ErrorMessage = "Banka adı zorunludur.")]
        [Display(Name = "Banka Adı")]
        [StringLength(100, ErrorMessage = "Banka adı en fazla 100 karakter olabilir.")]
        public required string BankaAdi { get; set; }
        
        [Display(Name = "Aktif")]
        public bool Aktif { get; set; } = true;
    }
    
    public class BankaEditViewModel
    {
        public Guid BankaID { get; set; }
        
        [Required(ErrorMessage = "Banka adı zorunludur.")]
        [Display(Name = "Banka Adı")]
        [StringLength(100, ErrorMessage = "Banka adı en fazla 100 karakter olabilir.")]
        public required string BankaAdi { get; set; }
        
        [Display(Name = "Aktif")]
        public bool Aktif { get; set; }
    }
} 