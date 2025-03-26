using System;
using System.ComponentModel.DataAnnotations;
using System.Collections.Generic;

namespace MuhasebeStokWebApp.ViewModels
{
    public class DovizKuruCreateViewModel
    {
        [Required(ErrorMessage = "Kaynak para birimi zorunludur.")]
        [Display(Name = "Kaynak Para Birimi")]
        public Guid KaynakParaBirimiID { get; set; }

        [Required(ErrorMessage = "Hedef para birimi zorunludur.")]
        [Display(Name = "Hedef Para Birimi")]
        public Guid HedefParaBirimiID { get; set; }

        [Required(ErrorMessage = "Kur değeri zorunludur.")]
        [Range(0.000001, double.MaxValue, ErrorMessage = "Kur değeri 0'dan büyük olmalıdır.")]
        [Display(Name = "Kur Değeri")]
        public decimal KurDegeri { get; set; }

        [Required(ErrorMessage = "Alış fiyatı zorunludur.")]
        [Range(0.000001, double.MaxValue, ErrorMessage = "Alış fiyatı 0'dan büyük olmalıdır.")]
        [Display(Name = "Alış Fiyatı")]
        public decimal AlisFiyati { get; set; }

        [Required(ErrorMessage = "Satış fiyatı zorunludur.")]
        [Range(0.000001, double.MaxValue, ErrorMessage = "Satış fiyatı 0'dan büyük olmalıdır.")]
        [Display(Name = "Satış Fiyatı")]
        public decimal SatisFiyati { get; set; }
        
        [Display(Name = "Efektif Alış")]
        public decimal? EfektifAlisFiyati { get; set; }
        
        [Display(Name = "Efektif Satış")]
        public decimal? EfektifSatisFiyati { get; set; }

        [Required(ErrorMessage = "Tarih zorunludur.")]
        [Display(Name = "Tarih")]
        [DataType(DataType.Date)]
        public DateTime Tarih { get; set; } = DateTime.Today;

        [Display(Name = "Açıklama")]
        [StringLength(500, ErrorMessage = "Açıklama en fazla 500 karakter olabilir.")]
        public string? Aciklama { get; set; }

        [Display(Name = "Aktif")]
        public bool Aktif { get; set; } = true;
    }
} 