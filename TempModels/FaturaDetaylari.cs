using System;
using System.Collections.Generic;

namespace MuhasebeStokWebApp.TempModels;

public partial class FaturaDetaylari
{
    public Guid FaturaDetayId { get; set; }

    public Guid FaturaId { get; set; }

    public Guid UrunId { get; set; }

    public decimal Miktar { get; set; }

    public decimal BirimFiyat { get; set; }

    public decimal KdvOrani { get; set; }

    public decimal IndirimOrani { get; set; }

    public decimal SatirToplam { get; set; }

    public decimal SatirKdvToplam { get; set; }

    public string? Birim { get; set; }

    public string? Aciklama { get; set; }

    public Guid? OlusturanKullaniciId { get; set; }

    public Guid? SonGuncelleyenKullaniciId { get; set; }

    public DateTime OlusturmaTarihi { get; set; }

    public DateTime? GuncellemeTarihi { get; set; }

    public bool SoftDelete { get; set; }

    public decimal? Tutar { get; set; }

    public decimal? KdvTutari { get; set; }

    public decimal? IndirimTutari { get; set; }

    public decimal? NetTutar { get; set; }

    public Guid? BirimId { get; set; }

    public virtual Birimler? BirimNavigation { get; set; }

    public virtual Faturalar Fatura { get; set; } = null!;

    public virtual Urunler Urun { get; set; } = null!;
}
