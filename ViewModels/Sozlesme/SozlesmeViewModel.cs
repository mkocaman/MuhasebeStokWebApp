using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;
using MuhasebeStokWebApp.Data.Entities;
using MuhasebeStokWebApp.ViewModels.Fatura;

namespace MuhasebeStokWebApp.ViewModels.Sozlesme
{
    public class SozlesmeViewModel
    {
        [Display(Name = "ID")]
        public string SozlesmeID { get; set; }
        
        [Required(ErrorMessage = "Sözleşme numarası zorunludur")]
        [Display(Name = "Sözleşme No")]
        [StringLength(100, ErrorMessage = "Sözleşme no en fazla 100 karakter olabilir")]
        public string SozlesmeNo { get; set; }
        
        [Required(ErrorMessage = "Sözleşme tarihi zorunludur")]
        [Display(Name = "Sözleşme Tarihi")]
        [DataType(DataType.Date)]
        public DateTime SozlesmeTarihi { get; set; }
        
        [Display(Name = "Bitiş Tarihi")]
        [DataType(DataType.Date)]
        public DateTime? BitisTarihi { get; set; }
        
        [Required(ErrorMessage = "Cari seçimi zorunludur")]
        [Display(Name = "Cari")]
        public string CariID { get; set; }
        
        [Display(Name = "Cari Adı")]
        public string CariAdi { get; set; }
        
        [Display(Name = "Fatura")]
        public Guid? FaturaID { get; set; }
        
        [Display(Name = "Fatura No")]
        public string FaturaNo { get; set; }
        
        [Display(Name = "Vekalet Geldi Mi?")]
        public bool VekaletGeldiMi { get; set; }
        
        [Display(Name = "Resmi Fatura Kesildi Mi?")]
        public bool ResmiFaturaKesildiMi { get; set; }
        
        [Display(Name = "Sözleşme Dosyası")]
        public string SozlesmeDosyaYolu { get; set; }
        
        [Display(Name = "Vekaletname Dosyası")]
        public string? VekaletnameDosyaYolu { get; set; }
        
        [Display(Name = "Sözleşme Belgesi")]
        public IFormFile SozlesmeBelgesi { get; set; }
        
        [Display(Name = "Vekaletname")]
        public IFormFile Vekaletname { get; set; }
        
        [Required(ErrorMessage = "Sözleşme tutarı zorunludur")]
        [Display(Name = "Sözleşme Tutarı")]
        [DataType(DataType.Currency)]
        public decimal SozlesmeTutari { get; set; }
        
        [Required(ErrorMessage = "Para birimi zorunludur")]
        [Display(Name = "Para Birimi")]
        [StringLength(10, ErrorMessage = "Para birimi en fazla 10 karakter olabilir")]
        public string SozlesmeDovizTuru { get; set; } = "TL";
        
        [Display(Name = "Açıklama")]
        [StringLength(1000, ErrorMessage = "Açıklama en fazla 1000 karakter olabilir")]
        public string Aciklama { get; set; }
        
        [Display(Name = "Aktif Mi?")]
        public bool AktifMi { get; set; } = true;
        
        [Display(Name = "Oluşturma Tarihi")]
        [DataType(DataType.DateTime)]
        public DateTime OlusturmaTarihi { get; set; }
        
        [Display(Name = "Güncelleme Tarihi")]
        [DataType(DataType.DateTime)]
        public DateTime? GuncellemeTarihi { get; set; }
        
        [Display(Name = "Fatura Sayısı")]
        public int FaturaSayisi { get; set; }
        
        [Display(Name = "Aklama Sayısı")]
        public int AklamaSayisi { get; set; }
        
        [Display(Name = "Ana Sözleşme")]
        public Guid? AnaSozlesmeID { get; set; }
        
        [Display(Name = "Oluşturan Kullanıcı")]
        public Guid? OlusturanKullaniciID { get; set; }
        
        [Display(Name = "Güncelleyen Kullanıcı")]
        public Guid? GuncelleyenKullaniciID { get; set; }
        
        [Display(Name = "Sözleşme Belgesi URL")]
        public string? SozlesmeBelgesiYolu { get; set; }
        
        [Display(Name = "Vekaletname URL")]
        public string? VekaletnameYolu { get; set; }
        
        [Display(Name = "Faturalar")]
        public List<FaturaViewModel> Faturalar { get; set; }
        
        public SozlesmeViewModel()
        {
            Faturalar = new List<FaturaViewModel>();
        }
    }
} 