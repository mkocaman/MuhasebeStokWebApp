using System;
using System.Collections.Generic;

namespace MuhasebeStokWebApp.TempModels;

public partial class StokCikisDetaylari
{
    public Guid StokCikisDetayId { get; set; }

    public Guid? StokFifoId { get; set; }

    public decimal CikisMiktari { get; set; }

    public decimal BirimFiyat { get; set; }

    public decimal BirimFiyatUsd { get; set; }

    public decimal BirimFiyatTl { get; set; }

    public decimal BirimFiyatUzs { get; set; }

    public decimal ToplamMaliyetUsd { get; set; }

    public string ReferansNo { get; set; } = null!;

    public string ReferansTuru { get; set; } = null!;

    public Guid ReferansId { get; set; }

    public DateTime CikisTarihi { get; set; }

    public string Aciklama { get; set; } = null!;

    public DateTime OlusturmaTarihi { get; set; }

    public bool Iptal { get; set; }

    public DateTime? IptalTarihi { get; set; }

    public string IptalAciklama { get; set; } = null!;

    public decimal BirimMaliyet { get; set; }

    public string HareketTipi { get; set; } = null!;

    public string ParaBirimi { get; set; } = null!;

    public decimal ToplamMaliyet { get; set; }

    public virtual StokFifoKayitlari? StokFifo { get; set; }
}
