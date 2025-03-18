using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MuhasebeStokWebApp.Data.Entities
{
    [Table("Dovizler")]
    public class Doviz
    {
        [Key]
        public int DovizID { get; set; }
        
        [Required]
        [StringLength(5)]
        [Display(Name = "Döviz Kodu")]
        public string DovizKodu { get; set; }
        
        [Required]
        [StringLength(50)]
        [Display(Name = "Döviz Adı")]
        public string DovizAdi { get; set; }
        
        [Required]
        [StringLength(5)]
        [Display(Name = "Sembol")]
        public string Sembol { get; set; }
        
        [Display(Name = "Aktif")]
        public bool Aktif { get; set; } = true;
        
        [Display(Name = "Silinmiş")]
        public bool SoftDelete { get; set; } = false;
        
        [Display(Name = "Oluşturma Tarihi")]
        public DateTime OlusturmaTarihi { get; set; } = DateTime.Now;
        
        [Display(Name = "Güncelleme Tarihi")]
        public DateTime? GuncellemeTarihi { get; set; }
    }
} 