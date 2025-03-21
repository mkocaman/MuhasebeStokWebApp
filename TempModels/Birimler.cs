using System;
using System.Collections.Generic;

namespace MuhasebeStokWebApp.TempModels;

public partial class Birimler
{
    public Guid BirimId { get; set; }

    public string BirimAdi { get; set; } = null!;

    public string Aciklama { get; set; } = null!;

    public bool Aktif { get; set; }

    public bool SoftDelete { get; set; }

    public virtual ICollection<FaturaDetaylari> FaturaDetaylaris { get; set; } = new List<FaturaDetaylari>();

    public virtual ICollection<IrsaliyeDetaylari> IrsaliyeDetaylaris { get; set; } = new List<IrsaliyeDetaylari>();

    public virtual ICollection<Urunler> Urunlers { get; set; } = new List<Urunler>();
}
