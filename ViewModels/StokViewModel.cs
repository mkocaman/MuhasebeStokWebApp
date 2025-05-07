using System;
using System.Collections.Generic;
using MuhasebeStokWebApp.Data.Entities;

namespace MuhasebeStokWebApp.ViewModels
{
    public class StokViewModel
    {
        public Guid UrunID { get; set; }
        public string UrunAdi { get; set; }
        public string UrunKodu { get; set; }
        public string BirimAdi { get; set; }
        public decimal GirisMiktari { get; set; }
        public decimal CikisMiktari { get; set; }
        public decimal StokMiktari => GirisMiktari - CikisMiktari;
    }

    public class StokCikisInfo
    {
        public decimal ToplamMaliyet { get; set; }
        public decimal BirimMaliyet { get; set; }
        public List<StokFifo> KullanilanFifoKayitlari { get; set; } = new List<StokFifo>();
    }
} 