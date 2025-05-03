using System;
using System.ComponentModel.DataAnnotations;

namespace MuhasebeStokWebApp.ViewModels.Filter
{
    /// <summary>
    /// Ürün filtreleme için kullanılacak model
    /// </summary>
    public class UrunFilterModel
    {
        /// <summary>
        /// Ürün koduna göre filtreleme
        /// </summary>
        [Display(Name = "Ürün Kodu")]
        public string UrunKodu { get; set; }
        
        /// <summary>
        /// Ürün adına göre filtreleme
        /// </summary>
        [Display(Name = "Ürün Adı")]
        public string UrunAdi { get; set; }
        
        /// <summary>
        /// Kategoriye göre filtreleme
        /// </summary>
        [Display(Name = "Kategori")]
        public Guid? KategoriID { get; set; }
        
        /// <summary>
        /// Aktiflik durumuna göre filtreleme
        /// </summary>
        [Display(Name = "Durum")]
        public bool? Aktif { get; set; }
        
        /// <summary>
        /// Sadece stokta olan ürünleri göster
        /// </summary>
        [Display(Name = "Sadece Stokta Olanlar")]
        public bool SadeceStokta { get; set; }
        
        /// <summary>
        /// Minimum stok miktarı
        /// </summary>
        [Display(Name = "Min. Stok")]
        public decimal? MinStok { get; set; }
        
        /// <summary>
        /// Maksimum stok miktarı
        /// </summary>
        [Display(Name = "Maks. Stok")]
        public decimal? MaxStok { get; set; }
        
        /// <summary>
        /// Silinmiş ürünleri de göster
        /// </summary>
        [Display(Name = "Silinenleri Göster")]
        public bool SilinenleriGoster { get; set; }
        
        /// <summary>
        /// Sıralama seçeneği
        /// </summary>
        [Display(Name = "Sıralama")]
        public string SortOrder { get; set; } = "name_asc";
        
        /// <summary>
        /// Aktif sekme - Aktif/Pasif/Silinmiş
        /// </summary>
        public string AktifTab { get; set; } = "aktif";
    }
} 