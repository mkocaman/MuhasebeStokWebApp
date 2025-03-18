using System;
using System.ComponentModel.DataAnnotations;

namespace MuhasebeStokWebApp.ViewModels.Cari
{
    public class CariCreateViewModel
    {
        [Required(ErrorMessage = "Cari adı zorunludur.")]
        [StringLength(200, ErrorMessage = "Cari adı en fazla 200 karakter olabilir.")]
        [Display(Name = "Cari Adı")]
        public string CariAdi { get; set; }
        
        [StringLength(50, ErrorMessage = "Vergi no en fazla 50 karakter olabilir.")]
        [Display(Name = "Vergi No")]
        public string VergiNo { get; set; }
        
        [StringLength(20, ErrorMessage = "Telefon en fazla 20 karakter olabilir.")]
        [Display(Name = "Telefon")]
        public string Telefon { get; set; }
        
        [StringLength(100, ErrorMessage = "E-posta en fazla 100 karakter olabilir.")]
        [EmailAddress(ErrorMessage = "Geçerli bir e-posta adresi giriniz.")]
        [Display(Name = "E-posta")]
        public string Email { get; set; }
        
        [StringLength(250, ErrorMessage = "Adres en fazla 250 karakter olabilir.")]
        [Display(Name = "Adres")]
        public string Adres { get; set; }
        
        [StringLength(50, ErrorMessage = "Yetkili adı en fazla 50 karakter olabilir.")]
        [Display(Name = "Yetkili")]
        public string Yetkili { get; set; }
        
        [StringLength(500, ErrorMessage = "Açıklama en fazla 500 karakter olabilir.")]
        [Display(Name = "Açıklama")]
        public string Aciklama { get; set; }
        
        [Display(Name = "Aktif")]
        public bool Aktif { get; set; } = true;
        
        [Display(Name = "Bakiye")]
        [DisplayFormat(DataFormatString = "{0:N2}", ApplyFormatInEditMode = false)]
        public decimal Bakiye { get; set; }
    }
} 