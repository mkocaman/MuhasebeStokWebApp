using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel;

namespace MuhasebeStokWebApp.ViewModels.Shared
{
    public class IrsaliyeDetayViewModel
    {
        public Guid IrsaliyeDetayID { get; set; }
        
        public Guid IrsaliyeID { get; set; }
        
        [Required(ErrorMessage = "Ürün seçimi gereklidir.")]
        [DisplayName("Ürün")]
        public Guid UrunID { get; set; }
        
        [DisplayName("Ürün Adı")]
        public string UrunAdi { get; set; }
        
        [Required(ErrorMessage = "Miktar gereklidir.")]
        [DisplayName("Miktar")]
        [Range(0.01, double.MaxValue, ErrorMessage = "Miktar sıfırdan büyük olmalıdır.")]
        public decimal Miktar { get; set; }
        
        [Required(ErrorMessage = "Birim gereklidir.")]
        [DisplayName("Birim")]
        public string Birim { get; set; }
        
        [DisplayName("Açıklama")]
        [StringLength(200, ErrorMessage = "Açıklama 200 karakterden uzun olamaz.")]
        public string Aciklama { get; set; }
        
        [DisplayName("Satır Toplam")]
        public decimal SatirToplam { get; set; }
        
        [DisplayName("Satır KDV Toplam")]
        public decimal SatirKdvToplam { get; set; }
        
        [DisplayName("Aktif")]
        public bool Aktif { get; set; } = true;
        
        [DisplayName("Oluşturma Tarihi")]
        public DateTime? OlusturmaTarihi { get; set; }
        
        [DisplayName("Güncelleme Tarihi")]
        public DateTime? GuncellemeTarihi { get; set; }
    }
} 