using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.Rendering;
using MuhasebeStokWebApp.Data.Entities;

namespace MuhasebeStokWebApp.ViewModels.Kur
{
    public class ParaBirimiIliskiViewModel
    {
        public Guid ParaBirimiIliskiID { get; set; }
        
        [Required(ErrorMessage = "Kaynak para birimi zorunludur")]
        [Display(Name = "Kaynak Para Birimi")]
        public Guid KaynakParaBirimiID { get; set; }
        
        [Display(Name = "Kaynak Para Birimi Kodu")]
        public string KaynakParaBirimiKodu { get; set; }
        
        [Required(ErrorMessage = "Hedef para birimi zorunludur")]
        [Display(Name = "Hedef Para Birimi")]
        public Guid HedefParaBirimiID { get; set; }
        
        [Display(Name = "Hedef Para Birimi Kodu")]
        public string HedefParaBirimiKodu { get; set; }
        
        [Required(ErrorMessage = "Çarpan değeri zorunludur")]
        [Range(0.001, 10000, ErrorMessage = "Çarpan değeri 0.001-10000 arasında olmalıdır")]
        [Display(Name = "Çarpan")]
        public decimal Carpan { get; set; } = 1.0m;
        
        [Display(Name = "Aktif")]
        public bool Aktif { get; set; } = true;
        
        // Dropdown listeler için
        public List<SelectListItem> ParaBirimleri { get; set; }
        
        // Entity'den ViewModel'e dönüşüm
        public static ParaBirimiIliskiViewModel FromEntity(ParaBirimiIliski entity, string kaynakKodu = null, string hedefKodu = null)
        {
            return new ParaBirimiIliskiViewModel
            {
                ParaBirimiIliskiID = entity.ParaBirimiIliskiID,
                KaynakParaBirimiID = entity.KaynakParaBirimiID,
                KaynakParaBirimiKodu = kaynakKodu ?? "",
                HedefParaBirimiID = entity.HedefParaBirimiID,
                HedefParaBirimiKodu = hedefKodu ?? "",
                Carpan = entity.Carpan,
                Aktif = entity.Aktif
            };
        }
        
        // ViewModel'den yeni Entity oluşturma
        public ParaBirimiIliski ToEntity()
        {
            return new ParaBirimiIliski
            {
                ParaBirimiIliskiID = this.ParaBirimiIliskiID == Guid.Empty ? Guid.NewGuid() : this.ParaBirimiIliskiID,
                KaynakParaBirimiID = this.KaynakParaBirimiID,
                HedefParaBirimiID = this.HedefParaBirimiID,
                Carpan = this.Carpan,
                Aktif = this.Aktif,
                OlusturmaTarihi = DateTime.Now,
                GuncellemeTarihi = DateTime.Now,
                Silindi = false
            };
        }
        
        // ViewModel'den mevcut Entity güncelleme
        public void UpdateEntity(ParaBirimiIliski entity)
        {
            entity.KaynakParaBirimiID = this.KaynakParaBirimiID;
            entity.HedefParaBirimiID = this.HedefParaBirimiID;
            entity.Carpan = this.Carpan;
            entity.Aktif = this.Aktif;
            entity.GuncellemeTarihi = DateTime.Now;
        }
    }
} 