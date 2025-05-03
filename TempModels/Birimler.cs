using System;
using System.Collections.Generic;

namespace MuhasebeStokWebApp.TempModels;

public partial class Birimler
{
    public Guid BirimId { get; set; }

    public string BirimAdi { get; set; } = null!;

    public string BirimKodu { get; set; } = null!;

    public string BirimSembol { get; set; } = null!;

    public string Aciklama { get; set; } = null!;

    public bool Aktif { get; set; }

    public bool Silindi { get; set; }

    public DateTime OlusturmaTarihi { get; set; }

    public DateTime? GuncellemeTarihi { get; set; }

    public string OlusturanKullaniciId { get; set; } = null!;

    public Guid? SonGuncelleyenKullaniciId { get; set; }

    public Guid? SirketId { get; set; }

    public virtual ICollection<FaturaDetaylari> FaturaDetaylaris { get; set; } = new List<FaturaDetaylari>();

    public virtual ICollection<IrsaliyeDetaylari> IrsaliyeDetaylaris { get; set; } = new List<IrsaliyeDetaylari>();

    public virtual ICollection<Urunler> Urunlers { get; set; } = new List<Urunler>();
}
