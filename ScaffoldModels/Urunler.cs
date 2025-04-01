using System;
using System.Collections.Generic;

namespace MuhasebeStokWebApp.ScaffoldModels;

public partial class Urunler
{
    public Guid UrunId { get; set; }

    public string UrunKodu { get; set; }

    public string UrunAdi { get; set; }

    public Guid? BirimId { get; set; }

    public decimal StokMiktar { get; set; }

    public bool Aktif { get; set; }

    public int Kdvorani { get; set; }

    public Guid? OlusturanKullaniciId { get; set; }

    public Guid? SonGuncelleyenKullaniciId { get; set; }

    public DateTime? OlusturmaTarihi { get; set; }

    public DateTime? GuncellemeTarihi { get; set; }

    public bool Silindi { get; set; }

    public Guid? KategoriId { get; set; }

    public virtual Birimler Birim { get; set; }

    public virtual ICollection<FaturaDetaylari> FaturaDetaylaris { get; set; } = new List<FaturaDetaylari>();

    public virtual ICollection<IrsaliyeDetaylari> IrsaliyeDetaylaris { get; set; } = new List<IrsaliyeDetaylari>();

    public virtual UrunKategorileri Kategori { get; set; }

    public virtual ICollection<StokFifo> StokFifos { get; set; } = new List<StokFifo>();

    public virtual ICollection<StokHareketleri> StokHareketleris { get; set; } = new List<StokHareketleri>();

    public virtual ICollection<UrunFiyatlari> UrunFiyatlaris { get; set; } = new List<UrunFiyatlari>();
}
