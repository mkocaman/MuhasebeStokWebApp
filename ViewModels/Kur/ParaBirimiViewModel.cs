using System;
using System.ComponentModel.DataAnnotations;
using MuhasebeStokWebApp.Data.Entities;

namespace MuhasebeStokWebApp.ViewModels.Kur
{
    public class ParaBirimiViewModel
    {
        public Guid ParaBirimiID { get; set; }
        
        [Required(ErrorMessage = "Para birimi kodu zorunludur")]
        [StringLength(5, MinimumLength = 2, ErrorMessage = "Para birimi kodu 2-5 karakter arasında olmalıdır")]
        [Display(Name = "Para Birimi Kodu")]
        public string Kod { get; set; }
        
        [Required(ErrorMessage = "Para birimi adı zorunludur")]
        [StringLength(50, MinimumLength = 2, ErrorMessage = "Para birimi adı 2-50 karakter arasında olmalıdır")]
        [Display(Name = "Para Birimi Adı")]
        public string Ad { get; set; }
        
        [StringLength(10, ErrorMessage = "Sembol en fazla 10 karakter olabilir")]
        [Display(Name = "Sembol")]
        public string Sembol { get; set; }
        
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
        
        // ViewModel'den yeni Entity oluşturma
        public ParaBirimi ToEntity()
        {
            return new ParaBirimi
            {
                ParaBirimiID = this.ParaBirimiID == Guid.Empty ? Guid.NewGuid() : this.ParaBirimiID,
                Kod = this.Kod,
                Ad = this.Ad,
                Sembol = this.Sembol,
                Aktif = this.Aktif,
                OlusturmaTarihi = DateTime.Now,
                GuncellemeTarihi = DateTime.Now,
                Silindi = false
            };
        }
        
        // ViewModel'den mevcut Entity güncelleme
        public void UpdateEntity(ParaBirimi entity)
        {
            entity.Kod = this.Kod;
            entity.Ad = this.Ad;
            entity.Sembol = this.Sembol;
            entity.Aktif = this.Aktif;
            entity.GuncellemeTarihi = DateTime.Now;
        }
    }
} 