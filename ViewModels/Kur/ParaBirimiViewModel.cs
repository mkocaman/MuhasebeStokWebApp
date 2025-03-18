using System;
using System.ComponentModel.DataAnnotations;
using MuhasebeStokWebApp.Data.Entities;

namespace MuhasebeStokWebApp.ViewModels.Kur
{
    public class ParaBirimiViewModel
    {
        public Guid ParaBirimiID { get; set; }
        
        [Required(ErrorMessage = "Para birimi kodu zorunludur.")]
        [StringLength(10, ErrorMessage = "Para birimi kodu en fazla 10 karakter olabilir.")]
        [Display(Name = "Para Birimi Kodu")]
        public required string Kod { get; set; }
        
        [Required(ErrorMessage = "Para birimi adı zorunludur.")]
        [StringLength(50, ErrorMessage = "Para birimi adı en fazla 50 karakter olabilir.")]
        [Display(Name = "Para Birimi Adı")]
        public required string Ad { get; set; }
        
        [Required(ErrorMessage = "Para birimi sembolü zorunludur.")]
        [StringLength(10, ErrorMessage = "Para birimi sembolü en fazla 10 karakter olabilir.")]
        [Display(Name = "Sembol")]
        public required string Sembol { get; set; }
        
        [Display(Name = "Aktif")]
        public bool Aktif { get; set; } = true;
        
        // Entity'den ViewModel'e dönüşüm
        public static ParaBirimiViewModel FromEntity(ParaBirimi entity)
        {
            return new ParaBirimiViewModel
            {
                ParaBirimiID = entity.ParaBirimiID,
                Kod = entity.Kod,
                Ad = entity.Ad,
                Sembol = entity.Sembol,
                Aktif = entity.Aktif
            };
        }
        
        // ViewModel'den Entity'e dönüşüm
        public ParaBirimi ToEntity()
        {
            return new ParaBirimi
            {
                ParaBirimiID = this.ParaBirimiID == Guid.Empty ? Guid.NewGuid() : this.ParaBirimiID,
                Kod = this.Kod,
                Ad = this.Ad,
                Sembol = this.Sembol,
                Aktif = this.Aktif,
                SoftDelete = false
            };
        }
    }
} 