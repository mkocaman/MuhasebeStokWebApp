using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MuhasebeStokWebApp.Models
{
    public class DovizKuru
    {
        [Key]
        public int DovizKuruID { get; set; }

        [Required(ErrorMessage = "Para birimi zorunludur.")]
        [StringLength(10, ErrorMessage = "Para birimi en fazla 10 karakter olabilir.")]
        [Display(Name = "Para Birimi")]
        public required string ParaBirimi { get; set; }

        [Required(ErrorMessage = "Baz para birimi zorunludur.")]
        [StringLength(10, ErrorMessage = "Baz para birimi en fazla 10 karakter olabilir.")]
        [Display(Name = "Baz Para Birimi")]
        public required string BazParaBirimi { get; set; }

        [Required(ErrorMessage = "Kur zorunludur.")]
        [Column(TypeName = "decimal(18, 6)")]
        [Display(Name = "Kur")]
        public decimal Kur { get; set; }

        [Column(TypeName = "decimal(18, 6)")]
        [Display(Name = "Alış Fiyatı")]
        public decimal? AlisFiyati { get; set; }

        [Column(TypeName = "decimal(18, 6)")]
        [Display(Name = "Satış Fiyatı")]
        public decimal? SatisFiyati { get; set; }

        [Required(ErrorMessage = "Tarih zorunludur.")]
        [Display(Name = "Tarih")]
        public DateTime Tarih { get; set; }

        [Required(ErrorMessage = "Kaynak zorunludur.")]
        [StringLength(50, ErrorMessage = "Kaynak en fazla 50 karakter olabilir.")]
        [Display(Name = "Kaynak")]
        public required string Kaynak { get; set; }

        [StringLength(500, ErrorMessage = "Açıklama en fazla 500 karakter olabilir.")]
        [Display(Name = "Açıklama")]
        public string? Aciklama { get; set; }

        [Display(Name = "Aktif")]
        public bool Aktif { get; set; } = true;

        [Display(Name = "Silinmiş")]
        public bool SoftDelete { get; set; } = false;

        [Display(Name = "Oluşturma Tarihi")]
        public DateTime OlusturmaTarihi { get; set; } = DateTime.Now;

        [Display(Name = "Güncelleme Tarihi")]
        public DateTime? GuncellemeTarihi { get; set; }
        
        // Controller'da kullanılan ek özellikler
        public string? OlusturanKullaniciId { get; set; }
        
        public string? SonGuncelleyenKullaniciId { get; set; }
    }
} 