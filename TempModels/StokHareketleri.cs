using System;
using System.Collections.Generic;

namespace MuhasebeStokWebApp.TempModels;

public partial class StokHareketleri
{
    public Guid StokHareketId { get; set; }

    public Guid UrunId { get; set; }

    public Guid? DepoId { get; set; }

    public decimal Miktar { get; set; }

    public string Birim { get; set; } = null!;

    public DateTime Tarih { get; set; }

    public string HareketTuru { get; set; } = null!;

    public string ReferansNo { get; set; } = null!;

    public string ReferansTuru { get; set; } = null!;

    public Guid? ReferansId { get; set; }

    public string Aciklama { get; set; } = null!;

    public decimal? BirimFiyat { get; set; }

    public Guid? IslemYapanKullaniciId { get; set; }

    public Guid? SonGuncelleyenKullaniciId { get; set; }

    public DateTime? OlusturmaTarihi { get; set; }

    public DateTime? GuncellemeTarihi { get; set; }

    public bool SoftDelete { get; set; }

    public Guid? FaturaId { get; set; }

    public Guid? IrsaliyeId { get; set; }

    public virtual Depolar? Depo { get; set; }

    public virtual Urunler Urun { get; set; } = null!;
}
