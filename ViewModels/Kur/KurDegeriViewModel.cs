using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.Rendering;
using MuhasebeStokWebApp.Data.Entities;

namespace MuhasebeStokWebApp.ViewModels.Kur
{
    public class KurDegeriViewModel
    {
        public int KurDegeriID { get; set; }
        
        [Required(ErrorMessage = "Döviz ilişkisi zorunludur.")]
        [Display(Name = "Döviz İlişkisi")]
        public int DovizIliskiID { get; set; }
        
        [Display(Name = "Kaynak Para Birimi")]
        public string KaynakParaBirimiKodu { get; set; }
        
        [Display(Name = "Hedef Para Birimi")]
        public string HedefParaBirimiKodu { get; set; }
        
        [Required(ErrorMessage = "Kur değeri zorunludur.")]
        [Range(0.000001, double.MaxValue, ErrorMessage = "Kur değeri 0'dan büyük olmalıdır.")]
        [Display(Name = "Kur Değeri")]
        public decimal Deger { get; set; }
        
        [Range(0, double.MaxValue, ErrorMessage = "Alış fiyatı 0 veya daha büyük olmalıdır.")]
        [Display(Name = "Alış Fiyatı")]
        public decimal? AlisFiyati { get; set; }
        
        [Range(0, double.MaxValue, ErrorMessage = "Satış fiyatı 0 veya daha büyük olmalıdır.")]
        [Display(Name = "Satış Fiyatı")]
        public decimal? SatisFiyati { get; set; }
        
        [Required(ErrorMessage = "Tarih zorunludur.")]
        [Display(Name = "Tarih")]
        public DateTime Tarih { get; set; } = DateTime.Now;
        
        [Required(ErrorMessage = "Kaynak zorunludur.")]
        [Display(Name = "Kaynak")]
        public string Kaynak { get; set; } = "Manuel";
        
        [Display(Name = "Aktif")]
        public bool Aktif { get; set; } = true;
        
        // Dropdown listeler için
        public List<SelectListItem> DovizIliskileri { get; set; }
        
        // Entity'den ViewModel'e dönüşüm
        public static KurDegeriViewModel FromEntity(KurDegeri entity)
        {
            var viewModel = new KurDegeriViewModel
            {
                KurDegeriID = entity.KurDegeriID,
                DovizIliskiID = entity.DovizIliskiID,
                Deger = entity.Deger,
                AlisFiyati = entity.AlisFiyati,
                SatisFiyati = entity.SatisFiyati,
                Tarih = entity.Tarih,
                Kaynak = entity.Kaynak,
                Aktif = entity.Aktif
            };
            
            if (entity.DovizIliski?.KaynakParaBirimi != null)
            {
                viewModel.KaynakParaBirimiKodu = entity.DovizIliski.KaynakParaBirimi.DovizKodu;
            }
            
            if (entity.DovizIliski?.HedefParaBirimi != null)
            {
                viewModel.HedefParaBirimiKodu = entity.DovizIliski.HedefParaBirimi.DovizKodu;
            }
            
            return viewModel;
        }
        
        // ViewModel'den Entity'e dönüşüm
        public KurDegeri ToEntity()
        {
            return new KurDegeri
            {
                KurDegeriID = this.KurDegeriID,
                DovizIliskiID = this.DovizIliskiID,
                Deger = this.Deger,
                AlisFiyati = this.AlisFiyati,
                SatisFiyati = this.SatisFiyati,
                Tarih = this.Tarih,
                Kaynak = this.Kaynak,
                Aktif = this.Aktif,
                SoftDelete = false,
                GuncellemeTarihi = this.KurDegeriID > 0 ? (DateTime?)DateTime.Now : null
            };
        }
    }
} 