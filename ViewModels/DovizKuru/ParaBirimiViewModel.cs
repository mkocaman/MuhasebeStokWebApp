using System;
using System.ComponentModel.DataAnnotations;

namespace MuhasebeStokWebApp.ViewModels.DovizKuru
{
    public class ParaBirimiViewModel
    {
        public Guid ParaBirimiID { get; set; }
        
        [Required(ErrorMessage = "Para birimi adı zorunludur.")]
        [StringLength(50, ErrorMessage = "Para birimi adı en fazla 50 karakter olabilir.")]
        public string Ad { get; set; } = string.Empty;
        
        [Required(ErrorMessage = "Para birimi kodu zorunludur.")]
        [StringLength(10, ErrorMessage = "Para birimi kodu en fazla 10 karakter olabilir.")]
        public string Kod { get; set; } = string.Empty;
        
        [Required(ErrorMessage = "Para birimi sembolü zorunludur.")]
        [StringLength(10, ErrorMessage = "Para birimi sembolü en fazla 10 karakter olabilir.")]
        public string Sembol { get; set; } = string.Empty;
        
        [StringLength(50, ErrorMessage = "Format en fazla 50 karakter olabilir.")]
        public string Format { get; set; } = "#,##0.00";
        
        [StringLength(250, ErrorMessage = "Açıklama en fazla 250 karakter olabilir.")]
        public string Aciklama { get; set; } = string.Empty;
        
        public int Sira { get; set; }
        
        public bool Aktif { get; set; } = true;
        
        public DateTime OlusturmaTarihi { get; set; }
        
        public DateTime? GuncellemeTarihi { get; set; }
    }
} 