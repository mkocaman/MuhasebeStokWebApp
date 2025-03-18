using System;
using System.ComponentModel.DataAnnotations;
using MuhasebeStokWebApp.Data.Entities;

namespace MuhasebeStokWebApp.ViewModels.Kur
{
    public class DovizViewModel
    {
        public int DovizID { get; set; }
        
        [Required(ErrorMessage = "Döviz kodu zorunludur.")]
        [StringLength(5, ErrorMessage = "Döviz kodu en fazla 5 karakter olabilir.")]
        [Display(Name = "Döviz Kodu")]
        public string DovizKodu { get; set; }
        
        [Required(ErrorMessage = "Döviz adı zorunludur.")]
        [StringLength(50, ErrorMessage = "Döviz adı en fazla 50 karakter olabilir.")]
        [Display(Name = "Döviz Adı")]
        public string DovizAdi { get; set; }
        
        [Required(ErrorMessage = "Sembol zorunludur.")]
        [StringLength(5, ErrorMessage = "Sembol en fazla 5 karakter olabilir.")]
        [Display(Name = "Sembol")]
        public string Sembol { get; set; }
        
        [Display(Name = "Aktif")]
        public bool Aktif { get; set; } = true;
        
        // Entity'den ViewModel'e dönüşüm
        public static DovizViewModel FromEntity(Doviz entity)
        {
            return new DovizViewModel
            {
                DovizID = entity.DovizID,
                DovizKodu = entity.DovizKodu,
                DovizAdi = entity.DovizAdi,
                Sembol = entity.Sembol,
                Aktif = entity.Aktif
            };
        }
        
        // ViewModel'den Entity'e dönüşüm
        public Doviz ToEntity()
        {
            return new Doviz
            {
                DovizID = this.DovizID,
                DovizKodu = this.DovizKodu,
                DovizAdi = this.DovizAdi,
                Sembol = this.Sembol,
                Aktif = this.Aktif,
                SoftDelete = false,
                GuncellemeTarihi = this.DovizID > 0 ? (DateTime?)DateTime.Now : null
            };
        }
    }
} 