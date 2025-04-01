using System;
using System.Collections.Generic;

namespace MuhasebeStokWebApp.ScaffoldModels;

public partial class StokHareketleri
{
    public Guid StokHareketId { get; set; }

    public Guid UrunId { get; set; }

    public Guid? DepoId { get; set; }

    public decimal Miktar { get; set; }

    public string Birim { get; set; }

    public DateTime Tarih { get; set; }

    public string HareketTuru { get; set; }

    public string ReferansNo { get; set; }

    public string ReferansTuru { get; set; }

    public Guid? ReferansId { get; set; }

    public string Aciklama { get; set; }

    public decimal? BirimFiyat { get; set; }

    public Guid? IslemYapanKullaniciId { get; set; }

    public Guid? SonGuncelleyenKullaniciId { get; set; }

    public DateTime? OlusturmaTarihi { get; set; }

    public DateTime? GuncellemeTarihi { get; set; }

    public bool Silindi { get; set; }

    public Guid? FaturaId { get; set; }

    public Guid? IrsaliyeId { get; set; }

    public virtual Depolar Depo { get; set; }

    public virtual Urunler Urun { get; set; }
}
