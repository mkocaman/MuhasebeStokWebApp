using System;
using System.Collections.Generic;

namespace MuhasebeStokWebApp.ScaffoldModels;

public partial class Birimler
{
    public Guid BirimId { get; set; }

    public string BirimAdi { get; set; }

    public string BirimKodu { get; set; }

    public string BirimSembol { get; set; }

    public string Aciklama { get; set; }

    public bool Aktif { get; set; }

    public bool Silindi { get; set; }

    public DateTime? OlusturmaTarihi { get; set; }

    public DateTime? GuncellemeTarihi { get; set; }

    public string OlusturanKullaniciId { get; set; }

    public Guid? SonGuncelleyenKullaniciId { get; set; }

    public Guid? SirketId { get; set; }

    public virtual ICollection<FaturaDetaylari> FaturaDetaylaris { get; set; } = new List<FaturaDetaylari>();

    public virtual ICollection<IrsaliyeDetaylari> IrsaliyeDetaylaris { get; set; } = new List<IrsaliyeDetaylari>();

    public virtual ICollection<Urunler> Urunlers { get; set; } = new List<Urunler>();
}
