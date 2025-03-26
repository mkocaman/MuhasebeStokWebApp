using System;
using System.ComponentModel.DataAnnotations;
using System.Collections.Generic;
using MuhasebeStokWebApp.Data.Entities.DovizModulu;

namespace MuhasebeStokWebApp.ViewModels.DovizModulu
{
    public class DovizViewModel
    {
        public Guid Id { get; set; }
        
        [Required(ErrorMessage = "Para birimi seçilmelidir")]
        public Guid ParaBirimiID { get; set; }
        
        [Required(ErrorMessage = "Tarih zorunludur")]
        [Display(Name = "Tarih")]
        [DataType(DataType.Date)]
        public DateTime Tarih { get; set; }
        
        [Required(ErrorMessage = "Döviz kuru zorunludur")]
        [Display(Name = "Alış")]
        public decimal Alis { get; set; }
        
        [Required(ErrorMessage = "Döviz kuru zorunludur")]
        [Display(Name = "Satış")]
        public decimal Satis { get; set; }
        
        [Display(Name = "Efektif Alış")]
        public decimal? Efektif_Alis { get; set; }
        
        [Display(Name = "Efektif Satış")]
        public decimal? Efektif_Satis { get; set; }
        
        [Display(Name = "Açıklama")]
        [Required(ErrorMessage = "Açıklama zorunludur.")]
        public string Aciklama { get; set; }
        
        [Display(Name = "Aktif")]
        public bool Aktif { get; set; } = true;
        
        public string ParaBirimiAdi { get; set; }
        public string ParaBirimiKodu { get; set; }
        public string ParaBirimiSembol { get; set; }
        
        public DateTime? OlusturmaTarihi { get; set; }
        public DateTime? GuncellemeTarihi { get; set; }
    }
    
    public class DovizCreateViewModel
    {
        [Required(ErrorMessage = "Para birimi seçilmelidir")]
        [Display(Name = "Para Birimi")]
        public Guid ParaBirimiID { get; set; }
        
        [Required(ErrorMessage = "Tarih zorunludur")]
        [Display(Name = "Tarih")]
        [DataType(DataType.Date)]
        public DateTime Tarih { get; set; } = DateTime.Today;
        
        [Required(ErrorMessage = "Alış kuru zorunludur")]
        [Display(Name = "Alış")]
        [Range(0.0001, double.MaxValue, ErrorMessage = "Alış kuru 0'dan büyük olmalıdır")]
        public decimal Alis { get; set; }
        
        [Required(ErrorMessage = "Satış kuru zorunludur")]
        [Display(Name = "Satış")]
        [Range(0.0001, double.MaxValue, ErrorMessage = "Satış kuru 0'dan büyük olmalıdır")]
        public decimal Satis { get; set; }
        
        [Display(Name = "Efektif Alış")]
        public decimal? Efektif_Alis { get; set; }
        
        [Display(Name = "Efektif Satış")]
        public decimal? Efektif_Satis { get; set; }
        
        [Display(Name = "Açıklama")]
        [StringLength(500, ErrorMessage = "Açıklama en fazla 500 karakter olabilir")]
        [Required(ErrorMessage = "Açıklama zorunludur.")]
        public string Aciklama { get; set; }
        
        [Display(Name = "Aktif")]
        public bool Aktif { get; set; } = true;
        
        public List<Data.Entities.DovizModulu.ParaBirimi> ParaBirimleri { get; set; }
    }
    
    public class DovizEditViewModel
    {
        public Guid KurDegeriID { get; set; }
        
        [Required(ErrorMessage = "Para birimi seçilmelidir")]
        [Display(Name = "Para Birimi")]
        public Guid ParaBirimiID { get; set; }
        
        [Required(ErrorMessage = "Tarih zorunludur")]
        [Display(Name = "Tarih")]
        [DataType(DataType.Date)]
        public DateTime Tarih { get; set; }
        
        [Required(ErrorMessage = "Alış kuru zorunludur")]
        [Display(Name = "Alış")]
        [Range(0.0001, double.MaxValue, ErrorMessage = "Alış kuru 0'dan büyük olmalıdır")]
        public decimal Alis { get; set; }
        
        [Required(ErrorMessage = "Satış kuru zorunludur")]
        [Display(Name = "Satış")]
        [Range(0.0001, double.MaxValue, ErrorMessage = "Satış kuru 0'dan büyük olmalıdır")]
        public decimal Satis { get; set; }
        
        [Display(Name = "Efektif Alış")]
        public decimal? Efektif_Alis { get; set; }
        
        [Display(Name = "Efektif Satış")]
        public decimal? Efektif_Satis { get; set; }
        
        [Display(Name = "Açıklama")]
        [StringLength(500, ErrorMessage = "Açıklama en fazla 500 karakter olabilir")]
        [Required(ErrorMessage = "Açıklama zorunludur.")]
        public string Aciklama { get; set; }
        
        [Display(Name = "Aktif")]
        public bool Aktif { get; set; } = true;
        
        public List<Data.Entities.DovizModulu.ParaBirimi> ParaBirimleri { get; set; }
        
        public string ParaBirimiAdi { get; set; }
        public string ParaBirimiKodu { get; set; }
    }
    
    public class DovizListViewModel
    {
        public List<DovizViewModel> Dovizler { get; set; }
        public List<Data.Entities.DovizModulu.ParaBirimi> ParaBirimleri { get; set; }
        public Guid? SelectedParaBirimiID { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
    }
} 