using System;
using System.ComponentModel.DataAnnotations;

namespace MuhasebeStokWebApp.ViewModels.UrunKategori
{
    public class UrunKategoriViewModel
    {
        public Guid KategoriID { get; set; }
        
        [Display(Name = "Kategori Adı")]
        public string KategoriAdi { get; set; }
        
        [Display(Name = "Açıklama")]
        public string Aciklama { get; set; }
        
        [Display(Name = "Aktif")]
        public bool Aktif { get; set; }
        
        [Display(Name = "Oluşturma Tarihi")]
        public DateTime? OlusturmaTarihi { get; set; }
        
        [Display(Name = "Güncelleme Tarihi")]
        public DateTime? GuncellemeTarihi { get; set; }
    }
} 