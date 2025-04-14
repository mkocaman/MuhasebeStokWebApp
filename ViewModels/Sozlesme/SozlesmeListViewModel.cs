using System;
using System.ComponentModel.DataAnnotations;

namespace MuhasebeStokWebApp.ViewModels.Sozlesme
{
    public class SozlesmeListViewModel
    {
        [Display(Name = "ID")]
        public string SozlesmeID { get; set; }
        
        [Display(Name = "Sözleşme No")]
        public string SozlesmeNo { get; set; }
        
        [Display(Name = "Sözleşme Tarihi")]
        [DataType(DataType.Date)]
        public DateTime SozlesmeTarihi { get; set; }
        
        [Display(Name = "Bitiş Tarihi")]
        [DataType(DataType.Date)]
        public DateTime? BitisTarihi { get; set; }
        
        [Display(Name = "Cari")]
        public string CariAdi { get; set; }
        
        [Display(Name = "Sözleşme Tutarı")]
        [DataType(DataType.Currency)]
        public decimal SozlesmeTutari { get; set; }
        
        [Display(Name = "Para Birimi")]
        public string SozlesmeDovizTuru { get; set; }
        
        [Display(Name = "Vekalet Geldi Mi?")]
        public bool VekaletGeldiMi { get; set; }
        
        [Display(Name = "Resmi Fatura Kesildi Mi?")]
        public bool ResmiFaturaKesildiMi { get; set; }
        
        [Display(Name = "Sözleşme Dosyası")]
        public bool DosyaVar { get; set; }
        
        [Display(Name = "Vekalet Dosyası")]
        public bool VekaletVar { get; set; }
        
        [Display(Name = "Fatura Sayısı")]
        public int FaturaSayisi { get; set; }
        
        [Display(Name = "Aklama Sayısı")]
        public int AklamaSayisi { get; set; }
        
        [Display(Name = "Aktif Mi?")]
        public bool AktifMi { get; set; }
    }
} 