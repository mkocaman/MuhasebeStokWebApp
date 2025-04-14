using System;
using System.Collections.Generic;

namespace MuhasebeStokWebApp.ViewModels.Rapor
{
    public class RaporStokViewModel
    {
        public string RaporAdi { get; set; }
        public DateTime RaporTarihi { get; set; }
        public string KullaniciAdi { get; set; }
        public string Aciklama { get; set; }
        public List<RaporStokHareketViewModel> StokHareketleri { get; set; } = new List<RaporStokHareketViewModel>();
        public decimal ToplamGiris { get; set; }
        public decimal ToplamCikis { get; set; }
    }

    public class RaporStokHareketViewModel
    {
        public string UrunAdi { get; set; }
        public DateTime Tarih { get; set; }
        public string HareketTuru { get; set; }
        public decimal Miktar { get; set; }
        public string Birim { get; set; }
        public decimal BirimFiyat { get; set; }
        public decimal ToplamTutar { get; set; }
        public string ReferansNo { get; set; }
        public string ReferansTuru { get; set; }
        public string Aciklama { get; set; }
    }
} 