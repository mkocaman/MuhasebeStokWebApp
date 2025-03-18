using System;
using System.ComponentModel.DataAnnotations;

namespace MuhasebeStokWebApp.ViewModels.Cari
{
    public class CariViewModel
    {
        public Guid CariID { get; set; }
        
        [Display(Name = "Cari Adı")]
        public string CariAdi { get; set; }
        
        [Display(Name = "Vergi No")]
        public string VergiNo { get; set; }
        
        [Display(Name = "Telefon")]
        public string Telefon { get; set; }
        
        [Display(Name = "E-posta")]
        public string Email { get; set; }
        
        [Display(Name = "Adres")]
        public string Adres { get; set; }
        
        [Display(Name = "Yetkili")]
        public string Yetkili { get; set; }
        
        [Display(Name = "Açıklama")]
        public string Aciklama { get; set; }
        
        [Display(Name = "Aktif")]
        public bool Aktif { get; set; }
        
        [Display(Name = "Oluşturma Tarihi")]
        public DateTime? OlusturmaTarihi { get; set; }
        
        [Display(Name = "Güncelleme Tarihi")]
        public DateTime? GuncellemeTarihi { get; set; }
        
        // Hesap bakiyesi (CariHareketler tablosundan hesaplanacak)
        [Display(Name = "Bakiye")]
        public decimal Bakiye { get; set; }
    }
} 