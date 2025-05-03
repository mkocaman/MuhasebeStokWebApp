using System;
using System.ComponentModel.DataAnnotations;

namespace MuhasebeStokWebApp.ViewModels.Depo
{
    public class DepoViewModel
    {
        public Guid DepoID { get; set; }
        
        [Display(Name = "Depo Adı")]
        [Required(ErrorMessage = "Depo adı zorunludur.")]
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
        
        public DateTime OlusturmaTarihi { get; set; }
        
        public DateTime? GuncellemeTarihi { get; set; }
        
        public bool Silindi { get; set; }

        public DepoViewModel()
        {
            DepoID = Guid.NewGuid();
            Aktif = true;
            Silindi = false;
        }
    }
} 