using System;
using System.Collections.Generic;

namespace MuhasebeStokWebApp.TempModels;

public partial class Depolar
{
    public Guid DepoId { get; set; }

    public string DepoAdi { get; set; } = null!;

    public string Adres { get; set; } = null!;

    public bool Aktif { get; set; }

    public Guid? OlusturanKullaniciId { get; set; }

    public Guid? SonGuncelleyenKullaniciId { get; set; }

    public DateTime? OlusturmaTarihi { get; set; }

    public DateTime? GuncellemeTarihi { get; set; }

    public bool SoftDelete { get; set; }

    public virtual ICollection<StokHareketleri> StokHareketleris { get; set; } = new List<StokHareketleri>();
}
