using System.ComponentModel.DataAnnotations;

namespace MuhasebeStokWebApp.ViewModels.SistemAyar
{
    public class BildirimAyarlariViewModel
    {
        [Display(Name = "Bildirimleri Aktif Et")]
        public bool BildirimAktif { get; set; } = true;

        [Display(Name = "E-posta Bildirimlerini Aktif Et")]
        public bool EmailBildirimAktif { get; set; } = true;

        [Display(Name = "Web Bildirimlerini Aktif Et")]
        public bool PushBildirimAktif { get; set; } = true;

        [Display(Name = "Otomatik Bildirimleri Aktif Et")]
        public bool OtomatikBildirimler { get; set; } = true;

        [Display(Name = "Bildirim Özeti Gönderme Sıklığı")]
        public string BildirimOzetiSikligi { get; set; } = "Günlük";

        [Display(Name = "Kritik Olay Bildirimleri")]
        public bool KritikOlayBildirimleri { get; set; } = true;

        [Display(Name = "Stok Uyarı Bildirimleri")]
        public bool StokUyariBildirimleri { get; set; } = true;

        [Display(Name = "Tahsilat ve Ödeme Bildirimleri")]
        public bool TahsilatOdemeBildirimleri { get; set; } = true;

        [Display(Name = "Yeni Sipariş Bildirimleri")]
        public bool YeniSiparisBildirimleri { get; set; } = true;
    }
} 