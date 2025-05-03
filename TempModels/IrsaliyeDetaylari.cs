using System;
using System.Collections.Generic;

namespace MuhasebeStokWebApp.TempModels;

public partial class IrsaliyeDetaylari
{
    public Guid IrsaliyeDetayId { get; set; }

    public Guid IrsaliyeId { get; set; }

    public Guid UrunId { get; set; }

    public decimal Miktar { get; set; }

    public decimal BirimFiyat { get; set; }

    public decimal KdvOrani { get; set; }

    public decimal IndirimOrani { get; set; }

    public string Birim { get; set; } = null!;

    public string Aciklama { get; set; } = null!;

    public Guid? OlusturanKullaniciId { get; set; }

    public Guid? SonGuncelleyenKullaniciId { get; set; }

    public DateTime? OlusturmaTarihi { get; set; }

    public DateTime? GuncellemeTarihi { get; set; }

    public bool Aktif { get; set; }

    public bool Silindi { get; set; }

    public decimal SatirToplam { get; set; }

    public decimal SatirKdvToplam { get; set; }

    public Guid? BirimId { get; set; }

    public virtual Birimler? BirimNavigation { get; set; }

    public virtual Irsaliyeler Irsaliye { get; set; } = null!;

    public virtual Urunler Urun { get; set; } = null!;
}
