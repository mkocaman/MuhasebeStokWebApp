using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.Rendering;
using MuhasebeStokWebApp.Data.Entities;

namespace MuhasebeStokWebApp.ViewModels.Kur
{
    public class KurAyarlariViewModel
    {
        [Required(ErrorMessage = "Baz para birimi zorunludur.")]
        [Display(Name = "Baz Para Birimi")]
        public int BazParaBirimiID { get; set; }
        
        [Display(Name = "Baz Para Birimi Kodu")]
        public string BazParaBirimiKodu { get; set; }
        
        [Display(Name = "İkinci Para Birimi")]
        public int? IkinciParaBirimiID { get; set; }
        
        [Display(Name = "İkinci Para Birimi Kodu")]
        public string IkinciParaBirimiKodu { get; set; }
        
        [Display(Name = "Üçüncü Para Birimi")]
        public int? UcuncuParaBirimiID { get; set; }
        
        [Display(Name = "Üçüncü Para Birimi Kodu")]
        public string UcuncuParaBirimiKodu { get; set; }
        
        [Display(Name = "Otomatik Güncelleme")]
        public bool OtomatikGuncelleme { get; set; } = true;
        
        [Range(1, 168, ErrorMessage = "Güncelleme sıklığı 1-168 saat arasında olmalıdır.")]
        [Display(Name = "Güncelleme Sıklığı (Saat)")]
        public int GuncellemeSikligi { get; set; } = 24;
        
        [Display(Name = "Son Güncelleme")]
        public DateTime SonGuncelleme { get; set; }
        
        // Dropdown listeler için
        public List<SelectListItem> ParaBirimleri { get; set; }
        
        // SistemAyarlari'ndan ViewModel'e dönüşüm
        public static KurAyarlariViewModel FromEntity(SistemAyarlari entity)
        {
            return new KurAyarlariViewModel
            {
                BazParaBirimiID = entity.AnaDovizID,
                BazParaBirimiKodu = entity.AnaDovizKodu,
                IkinciParaBirimiID = entity.IkinciDovizID,
                IkinciParaBirimiKodu = entity.IkinciDovizKodu,
                UcuncuParaBirimiID = entity.UcuncuDovizID,
                UcuncuParaBirimiKodu = entity.UcuncuDovizKodu,
                OtomatikGuncelleme = entity.OtomatikDovizGuncelleme,
                GuncellemeSikligi = entity.DovizGuncellemeSikligi,
                SonGuncelleme = entity.SonDovizGuncellemeTarihi
            };
        }
        
        // ViewModel'den SistemAyarlari'na güncelleme
        public void UpdateEntity(SistemAyarlari entity)
        {
            entity.AnaDovizID = this.BazParaBirimiID;
            entity.AnaDovizKodu = this.BazParaBirimiKodu;
            entity.IkinciDovizID = this.IkinciParaBirimiID;
            entity.IkinciDovizKodu = this.IkinciParaBirimiKodu;
            entity.UcuncuDovizID = this.UcuncuParaBirimiID;
            entity.UcuncuDovizKodu = this.UcuncuParaBirimiKodu;
            entity.OtomatikDovizGuncelleme = this.OtomatikGuncelleme;
            entity.DovizGuncellemeSikligi = this.GuncellemeSikligi;
            entity.SonDovizGuncellemeTarihi = this.SonGuncelleme;
        }
    }
} 