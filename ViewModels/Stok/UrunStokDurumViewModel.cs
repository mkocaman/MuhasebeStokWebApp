using System;
using System.Collections.Generic;

namespace MuhasebeStokWebApp.ViewModels.Stok
{
    public class UrunStokDurumViewModel
    {
        public Guid UrunID { get; set; }
        public string UrunKodu { get; set; }
        public string UrunAdi { get; set; }
        public string BirimAdi { get; set; }
        public string KategoriAdi { get; set; }
        public decimal GenelStokMiktari { get; set; }
        public List<DepoStokDurumViewModel> DepoStokDurumlari { get; set; }
    }
    
    public class DepoStokDurumViewModel
    {
        public Guid DepoID { get; set; }
        public string DepoAdi { get; set; }
        public decimal StokMiktari { get; set; }
    }
} 