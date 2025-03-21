using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace MuhasebeStokWebApp.ViewModels.DovizKuru
{
    public class DovizKuruEkleViewModel
    {
        [Required(ErrorMessage = "Para birimi kodu seçiniz.")]
        [Display(Name = "Para Birimi Kodu")]
        public string ParaBirimiKodu { get; set; }
        
        [Required(ErrorMessage = "Alış değeri gereklidir.")]
        [Range(0.0001, 1000000, ErrorMessage = "Alış değeri 0.0001 ile 1000000 arasında olmalıdır.")]
        [Display(Name = "Alış Değeri")]
        [DisplayFormat(DataFormatString = "{0:N6}", ApplyFormatInEditMode = true)]
        public decimal AlisDegeri { get; set; }
        
        [Required(ErrorMessage = "Satış değeri gereklidir.")]
        [Range(0.0001, 1000000, ErrorMessage = "Satış değeri 0.0001 ile 1000000 arasında olmalıdır.")]
        [Display(Name = "Satış Değeri")]
        [DisplayFormat(DataFormatString = "{0:N6}", ApplyFormatInEditMode = true)]
        public decimal SatisDegeri { get; set; }
        
        [Required(ErrorMessage = "Tarih gereklidir.")]
        [Display(Name = "Kur Tarihi")]
        [DataType(DataType.Date)]
        [DisplayFormat(DataFormatString = "{0:dd.MM.yyyy}", ApplyFormatInEditMode = true)]
        public DateTime Tarih { get; set; }
        
        [Display(Name = "Kayıt Tarihi")]
        [DisplayFormat(DataFormatString = "{0:dd.MM.yyyy HH:mm}", ApplyFormatInEditMode = false)]
        public DateTime KayitTarihi { get; set; }
        
        [Display(Name = "Kaynak")]
        [StringLength(50, ErrorMessage = "Kaynak en fazla 50 karakter olabilir.")]
        public string Kaynak { get; set; } = "Manuel";
        
        [Display(Name = "Açıklama")]
        [StringLength(250, ErrorMessage = "Açıklama en fazla 250 karakter olabilir.")]
        public string Aciklama { get; set; }
        
        [Display(Name = "Veri Kaynağı")]
        [StringLength(50, ErrorMessage = "Veri kaynağı en fazla 50 karakter olabilir.")]
        public string VeriKaynagi { get; set; } = "Kullanıcı";
    }
} 