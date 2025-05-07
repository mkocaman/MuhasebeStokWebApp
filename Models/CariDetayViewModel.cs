using System;
using System.ComponentModel.DataAnnotations;

namespace MuhasebeStokWebApp.Models
{
    public class CariDetayViewModel
    {
        public Guid CariID { get; set; }
        
        [Display(Name = "Cari Adı")]
        public string CariAdi { get; set; }
        
        [Display(Name = "Cari Kodu")]
        public string CariKodu { get; set; }
        
        [Display(Name = "Cari Tipi")]
        public string CariTipi { get; set; }
        
        [Display(Name = "Vergi No")]
        public string VergiNo { get; set; }
        
        [Display(Name = "Vergi Dairesi")]
        public string VergiDairesi { get; set; }
        
        [Display(Name = "Telefon")]
        public string Telefon { get; set; }
        
        [Display(Name = "E-posta")]
        public string Email { get; set; }
        
        [Display(Name = "Toplam Borç")]
        [DisplayFormat(DataFormatString = "{0:N2}")]
        public decimal ToplamBorc { get; set; }
        
        [Display(Name = "Toplam Alacak")]
        [DisplayFormat(DataFormatString = "{0:N2}")]
        public decimal ToplamAlacak { get; set; }
        
        [Display(Name = "Bakiye")]
        [DisplayFormat(DataFormatString = "{0:N2}")]
        public decimal Bakiye { get; set; }
        
        [Display(Name = "Para Birimi")]
        public string ParaBirimi { get; set; }
    }
} 