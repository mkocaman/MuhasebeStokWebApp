using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.Rendering;
using MuhasebeStokWebApp.Data.Entities;

namespace MuhasebeStokWebApp.ViewModels.Kur
{
    public class KurAyarlariViewModel
    {
        public Guid SistemAyarlariID { get; set; }
        
        [Display(Name = "Ana Para Birimi")]
        [Required(ErrorMessage = "Ana para birimi zorunludur")]
        public Guid BazParaBirimiID { get; set; }
        
        [Display(Name = "Ana Para Birimi Kodu")]
        public string BazParaBirimiKodu { get; set; }
        
        [Display(Name = "İkinci Para Birimi")]
        public Guid? IkinciParaBirimiID { get; set; }
        
        [Display(Name = "İkinci Para Birimi Kodu")]
        public string IkinciParaBirimiKodu { get; set; }
        
        [Display(Name = "Üçüncü Para Birimi")]
        public Guid? UcuncuParaBirimiID { get; set; }
        
        [Display(Name = "Üçüncü Para Birimi Kodu")]
        public string UcuncuParaBirimiKodu { get; set; }
        
        [Display(Name = "Otomatik Güncelleme")]
        public bool OtomatikGuncelleme { get; set; } = true;
        
        [Display(Name = "Güncelleme Sıklığı (Saat)")]
        [Range(1, 168, ErrorMessage = "Güncelleme sıklığı 1-168 saat arasında olmalıdır")]
        public int GuncellemeSikligi { get; set; } = 24;
        
        [Display(Name = "Son Güncelleme Tarihi")]
        public DateTime SonGuncellemeTarihi { get; set; } = DateTime.Now;
        
        // Para birimleri listesi
        public List<SelectListItem> ParaBirimleri { get; set; }
        
        // Entity'den ViewModel'e dönüşüm
        public static KurAyarlariViewModel FromEntity(SistemAyarlari entity)
        {
            return new KurAyarlariViewModel
            {
                SistemAyarlariID = entity.SistemAyarlariID,
                BazParaBirimiID = entity.AnaDovizID,
                BazParaBirimiKodu = entity.AnaDovizKodu,
                IkinciParaBirimiID = entity.IkinciDovizID,
                IkinciParaBirimiKodu = entity.IkinciDovizKodu,
                UcuncuParaBirimiID = entity.UcuncuDovizID,
                UcuncuParaBirimiKodu = entity.UcuncuDovizKodu,
                OtomatikGuncelleme = entity.OtomatikDovizGuncelleme,
                GuncellemeSikligi = entity.DovizGuncellemeSikligi,
                SonGuncellemeTarihi = entity.SonDovizGuncellemeTarihi
            };
        }
        
        // ViewModel'den Entity güncelleme
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
            entity.SonDovizGuncellemeTarihi = DateTime.Now;
            entity.GuncellemeTarihi = DateTime.Now;
        }
    }
} 