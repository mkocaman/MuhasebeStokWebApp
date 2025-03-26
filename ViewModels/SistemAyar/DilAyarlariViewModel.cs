using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace MuhasebeStokWebApp.ViewModels.SistemAyar
{
    public class DilAyarlariViewModel
    {
        [Required(ErrorMessage = "Varsayılan dil seçilmelidir")]
        [Display(Name = "Varsayılan Dil")]
        public string VarsayilanDil { get; set; } = "tr-TR";
        
        [Display(Name = "Para Birimi Format")]
        public string ParaBirimiFormat { get; set; } = "#,##0.00 ₺";
        
        [Display(Name = "Tarih Format")]
        public string TarihFormat { get; set; } = "dd.MM.yyyy";
        
        [Display(Name = "Saat Format")]
        public string SaatFormat { get; set; } = "HH:mm";
        
        [Display(Name = "Çoklu Dil Desteği")]
        public bool CokluDilDestegi { get; set; } = false;
        
        [Display(Name = "Aktif Diller")]
        public List<string> AktifDiller { get; set; } = new List<string> { "tr-TR" };
    }

    public class DilViewModel
    {
        [Required]
        public string Kod { get; set; }

        [Required]
        public string Ad { get; set; }

        public bool Aktif { get; set; }
    }
} 