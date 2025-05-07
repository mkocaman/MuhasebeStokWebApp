using System;
using System.ComponentModel.DataAnnotations;

namespace MuhasebeStokWebApp.Models
{
    public class CariListModel
    {
        public Guid CariID { get; set; }
        
        [Display(Name = "Cari Kodu")]
        public string CariKodu { get; set; }
        
        [Display(Name = "Cari Adı")]
        public string CariAdi { get; set; }
        
        [Display(Name = "Telefon")]
        public string Telefon { get; set; }
        
        [Display(Name = "E-posta")]
        public string Email { get; set; }
        
        [Display(Name = "Cari Tipi")]
        public string CariTipi { get; set; }
    }
} 