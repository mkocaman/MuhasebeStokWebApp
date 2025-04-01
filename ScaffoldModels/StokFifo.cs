using System;
using System.Collections.Generic;

namespace MuhasebeStokWebApp.ScaffoldModels;

public partial class StokFifo
{
    public Guid StokFifoId { get; set; }

    public Guid UrunId { get; set; }

    public decimal Miktar { get; set; }

    public decimal KalanMiktar { get; set; }

    public decimal BirimFiyat { get; set; }

    public string Birim { get; set; }

    public string ParaBirimi { get; set; }

    public decimal DovizKuru { get; set; }

    public decimal UsdbirimFiyat { get; set; }

    public decimal TlbirimFiyat { get; set; }

    public decimal UzsbirimFiyat { get; set; }

    public DateTime GirisTarihi { get; set; }

    public DateTime? SonCikisTarihi { get; set; }

    public string ReferansNo { get; set; }

    public string ReferansTuru { get; set; }

    public Guid ReferansId { get; set; }

    public string Aciklama { get; set; }

    public bool Aktif { get; set; }

    public bool Iptal { get; set; }

    public DateTime? IptalTarihi { get; set; }

    public string IptalAciklama { get; set; }

    public Guid? IptalEdenKullaniciId { get; set; }

    public DateTime OlusturmaTarihi { get; set; }

    public DateTime? GuncellemeTarihi { get; set; }

    public bool Silindi { get; set; }

    public virtual Urunler Urun { get; set; }
}
