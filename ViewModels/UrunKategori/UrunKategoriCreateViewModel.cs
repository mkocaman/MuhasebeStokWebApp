using System.ComponentModel.DataAnnotations;

namespace MuhasebeStokWebApp.ViewModels.UrunKategori
{
    public class UrunKategoriCreateViewModel
    {
        [Required(ErrorMessage = "Kategori adı zorunludur.")]
        [StringLength(100, ErrorMessage = "Kategori adı en fazla 100 karakter olabilir.")]
        [Display(Name = "Kategori Adı")]
        public string KategoriAdi { get; set; }

        [StringLength(500, ErrorMessage = "Açıklama en fazla 500 karakter olabilir.")]
        [Display(Name = "Açıklama")]
        public string Aciklama { get; set; }

        [Display(Name = "Aktif")]
        public bool Aktif { get; set; } = true;
    }
} 