using System;
using System.ComponentModel.DataAnnotations;

namespace MuhasebeStokWebApp.ViewModels.Sozlesme
{
    public class SozlesmeSearchViewModel
    {
        public Guid? CariID { get; set; }
        
        [Display(Name = "Sözleşme No")]
        public string SozlesmeNo { get; set; }
        
        [Display(Name = "Başlangıç Tarihi")]
        [DataType(DataType.Date)]
        public DateTime? BaslangicTarihi { get; set; }
        
        [Display(Name = "Bitiş Tarihi")]
        [DataType(DataType.Date)]
        public DateTime? BitisTarihi { get; set; }
        
        [Display(Name = "Sadece Aktif Sözleşmeler")]
        public bool SadeceAktif { get; set; } = true;
    }
} 