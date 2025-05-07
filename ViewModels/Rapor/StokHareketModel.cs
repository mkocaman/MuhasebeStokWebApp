using System;
using MuhasebeStokWebApp.Enums;

namespace MuhasebeStokWebApp.ViewModels.Rapor
{
    public class StokHareketModel
    {
        public Guid StokHareketID { get; set; }
        public Guid UrunID { get; set; }
        public string UrunKodu { get; set; }
        public string UrunAdi { get; set; }
        public DateTime Tarih { get; set; }
        public StokHareketTipi HareketTuru { get; set; }
        public string HareketTuruAdi { get; set; }
        public decimal Miktar { get; set; }
        public decimal BirimFiyat { get; set; }
        public decimal ToplamTutar { get; set; }
        public string Birim { get; set; }
        public string ReferansNo { get; set; }
        public string ReferansTuru { get; set; }
    }
} 