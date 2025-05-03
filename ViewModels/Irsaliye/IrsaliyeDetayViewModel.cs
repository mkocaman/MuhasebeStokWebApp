using System;
using System.ComponentModel.DataAnnotations;

namespace MuhasebeStokWebApp.ViewModels.Irsaliye
{
    public class IrsaliyeDetayViewModel
    {
        public Guid IrsaliyeDetayID { get; set; }
        
        public Guid IrsaliyeID { get; set; }
        
        [Required(ErrorMessage = "Ürün seçimi zorunludur.")]
        [Display(Name = "Ürün")]
        public Guid UrunID { get; set; }
        
        [Required(ErrorMessage = "Depo seçimi zorunludur.")]
        [Display(Name = "Depo")]
        public Guid DepoID { get; set; }
        
        [Required(ErrorMessage = "Miktar girilmesi zorunludur.")]
        [Range(0.01, double.MaxValue, ErrorMessage = "Miktar 0'dan büyük olmalıdır.")]
        [Display(Name = "Miktar")]
        public decimal Miktar { get; set; }
        
        [Required(ErrorMessage = "Birim fiyat girilmesi zorunludur.")]
        [Range(0.01, double.MaxValue, ErrorMessage = "Birim fiyat 0'dan büyük olmalıdır.")]
        [Display(Name = "Birim Fiyat")]
        public decimal BirimFiyat { get; set; }
        
        [Required(ErrorMessage = "KDV oranı girilmesi zorunludur.")]
        [Range(0, 100, ErrorMessage = "KDV oranı 0-100 arasında olmalıdır.")]
        [Display(Name = "KDV Oranı (%)")]
        public decimal KdvOrani { get; set; }
        
        [Required(ErrorMessage = "İndirim oranı girilmesi zorunludur.")]
        [Range(0, 100, ErrorMessage = "İndirim oranı 0-100 arasında olmalıdır.")]
        [Display(Name = "İndirim Oranı (%)")]
        public decimal IndirimOrani { get; set; }
        
        [StringLength(50)]
        [Display(Name = "Birim")]
        public string Birim { get; set; }
        
        [StringLength(200)]
        [Display(Name = "Açıklama")]
        public string Aciklama { get; set; }
        
        [Display(Name = "Ürün Adı")]
        public string UrunAdi { get; set; }
        
        [Display(Name = "Ürün Kodu")]
        public string UrunKodu { get; set; }
        
        [Display(Name = "Satır Toplam")]
        public decimal SatirToplam => Miktar * BirimFiyat * (1 - IndirimOrani / 100);
        
        [Display(Name = "Satır KDV Toplam")]
        public decimal SatirKdvToplam => SatirToplam * KdvOrani / 100;
        
        [Display(Name = "Genel Toplam")]
        public decimal GenelToplam => SatirToplam + SatirKdvToplam;
    }
} 