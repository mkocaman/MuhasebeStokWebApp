using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.Rendering;
using MuhasebeStokWebApp.Data.Entities;

namespace MuhasebeStokWebApp.ViewModels.Kur
{
    public class KurDegeriViewModel
    {
        public Guid KurDegeriID { get; set; }
        
        [Display(Name = "Para Birimi")]
        [Required(ErrorMessage = "Para birimi seçiniz")]
        public Guid ParaBirimiID { get; set; }
        
        [Display(Name = "Para Birimi Kodu")]
        public string ParaBirimiKodu { get; set; }
        
        [Display(Name = "Alış Değeri")]
        [Required(ErrorMessage = "Alış değeri giriniz")]
        [Range(0.0001, double.MaxValue, ErrorMessage = "Alış değeri pozitif olmalıdır")]
        public decimal AlisDegeri { get; set; } = 1.0m;
        
        [Display(Name = "Satış Değeri")]
        [Required(ErrorMessage = "Satış değeri giriniz")]
        [Range(0.0001, double.MaxValue, ErrorMessage = "Satış değeri pozitif olmalıdır")]
        public decimal SatisDegeri { get; set; } = 1.0m;
        
        [Display(Name = "Tarih")]
        [Required(ErrorMessage = "Tarih seçiniz")]
        [DataType(DataType.Date)]
        public DateTime Tarih { get; set; } = DateTime.Now;
        
        [Display(Name = "Kaynak")]
        [Required(ErrorMessage = "Kaynak giriniz")]
        [StringLength(50, ErrorMessage = "Kaynak en fazla 50 karakter olabilir")]
        public string Kaynak { get; set; } = "Manuel";
        
        [Display(Name = "Aktif")]
        public bool Aktif { get; set; } = true;
        
        // Para birimleri listesi
        public List<SelectListItem> ParaBirimleri { get; set; }
        
        // Entity'den ViewModel'e dönüşüm
        public static KurDegeriViewModel FromEntity(KurDegeri entity)
        {
            var viewModel = new KurDegeriViewModel
            {
                KurDegeriID = entity.KurDegeriID,
                ParaBirimiID = entity.ParaBirimiID,
                AlisDegeri = entity.AlisDegeri,
                SatisDegeri = entity.SatisDegeri,
                Tarih = entity.Tarih,
                Kaynak = entity.Kaynak,
                Aktif = entity.Aktif
            };
            
            if (entity.ParaBirimi != null)
            {
                viewModel.ParaBirimiKodu = entity.ParaBirimi.Kod;
            }
            
            return viewModel;
        }
        
        // ViewModel'den Entity'e dönüşüm
        public KurDegeri ToEntity()
        {
            return new KurDegeri
            {
                KurDegeriID = this.KurDegeriID,
                ParaBirimiID = this.ParaBirimiID,
                AlisDegeri = this.AlisDegeri,
                SatisDegeri = this.SatisDegeri,
                Tarih = this.Tarih,
                Kaynak = this.Kaynak,
                Aktif = this.Aktif,
                SoftDelete = false,
                OlusturmaTarihi = this.KurDegeriID != Guid.Empty ? DateTime.Now : DateTime.Now,
                GuncellemeTarihi = DateTime.Now
            };
        }
    }
} 