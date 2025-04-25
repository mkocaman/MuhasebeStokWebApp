using System;
using System.ComponentModel.DataAnnotations;

namespace MuhasebeStokWebApp.ViewModels.UrunKategori
{
    public class UrunKategoriViewModel
    {
        public Guid KategoriID { get; set; }
        
        [Display(Name = "Kategori Adı")]
        [Required(ErrorMessage = "Kategori adı zorunludur.")]
        [StringLength(50, ErrorMessage = "Kategori adı en fazla 50 karakter olabilir.")]
        public string KategoriAdi { get; set; }
        
        [Display(Name = "Açıklama")]
        [StringLength(200, ErrorMessage = "Açıklama en fazla 200 karakter olabilir.")]
        public string Aciklama { get; set; }
        
        [Display(Name = "Aktif")]
        public bool Aktif { get; set; }
        
        public DateTime OlusturmaTarihi { get; set; }
        
        public DateTime? GuncellemeTarihi { get; set; }
        
        // Silindi bilgisi eklenmiştir
        public bool Silindi { get; set; }
    }
} 