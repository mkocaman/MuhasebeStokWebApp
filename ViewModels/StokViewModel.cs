using System;

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
} 