using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.Rendering;
using MuhasebeStokWebApp.Data.Entities;

namespace MuhasebeStokWebApp.ViewModels.Kur
{
    public class DovizIliskiViewModel
    {
        public int DovizIliskiID { get; set; }
        
        [Required(ErrorMessage = "Kaynak para birimi zorunludur.")]
        [Display(Name = "Kaynak Para Birimi")]
        public int KaynakParaBirimiID { get; set; }
        
        [Display(Name = "Kaynak Para Birimi Kodu")]
        public string KaynakParaBirimiKodu { get; set; }
        
        [Required(ErrorMessage = "Hedef para birimi zorunludur.")]
        [Display(Name = "Hedef Para Birimi")]
        public int HedefParaBirimiID { get; set; }
        
        [Display(Name = "Hedef Para Birimi Kodu")]
        public string HedefParaBirimiKodu { get; set; }
        
        [Display(Name = "Aktif")]
        public bool Aktif { get; set; } = true;
        
        // Dropdown listeler için
        public List<SelectListItem> ParaBirimleri { get; set; }
        
        // Entity'den ViewModel'e dönüşüm
        public static DovizIliskiViewModel FromEntity(DovizIliski entity)
        {
            var viewModel = new DovizIliskiViewModel
            {
                DovizIliskiID = entity.DovizIliskiID,
                KaynakParaBirimiID = entity.KaynakParaBirimiID,
                HedefParaBirimiID = entity.HedefParaBirimiID,
                Aktif = entity.Aktif
            };
            
            if (entity.KaynakParaBirimi != null)
            {
                viewModel.KaynakParaBirimiKodu = entity.KaynakParaBirimi.DovizKodu;
            }
            
            if (entity.HedefParaBirimi != null)
            {
                viewModel.HedefParaBirimiKodu = entity.HedefParaBirimi.DovizKodu;
            }
            
            return viewModel;
        }
        
        // ViewModel'den Entity'e dönüşüm
        public DovizIliski ToEntity()
        {
            return new DovizIliski
            {
                DovizIliskiID = this.DovizIliskiID,
                KaynakParaBirimiID = this.KaynakParaBirimiID,
                HedefParaBirimiID = this.HedefParaBirimiID,
                Aktif = this.Aktif,
                SoftDelete = false,
                GuncellemeTarihi = this.DovizIliskiID > 0 ? (DateTime?)DateTime.Now : null
            };
        }
    }
} 