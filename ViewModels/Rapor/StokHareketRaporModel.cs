using System;
using System.Collections.Generic;

namespace MuhasebeStokWebApp.ViewModels.Rapor
{
    public class StokHareketRaporModel
    {
        public DateTime BaslangicTarihi { get; set; }
        public DateTime BitisTarihi { get; set; }
        public Guid? UrunID { get; set; }
        public Guid? KategoriID { get; set; }
        public string HareketTuru { get; set; }
        public List<StokHareketModel> Hareketler { get; set; } = new List<StokHareketModel>();
        public decimal ToplamGirisMiktari { get; set; }
        public decimal ToplamCikisMiktari { get; set; }
        public decimal ToplamGirisTutari { get; set; }
        public decimal ToplamCikisTutari { get; set; }
    }
} 