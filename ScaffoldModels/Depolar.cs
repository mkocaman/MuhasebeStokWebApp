using System;
using System.Collections.Generic;

namespace MuhasebeStokWebApp.ScaffoldModels;

public partial class Depolar
{
    public Guid DepoId { get; set; }

    public string DepoAdi { get; set; }

    public string Adres { get; set; }

    public bool Aktif { get; set; }

    public Guid? OlusturanKullaniciId { get; set; }

    public Guid? SonGuncelleyenKullaniciId { get; set; }

    public DateTime? OlusturmaTarihi { get; set; }

    public DateTime? GuncellemeTarihi { get; set; }

    public bool Silindi { get; set; }

    public virtual ICollection<StokHareketleri> StokHareketleris { get; set; } = new List<StokHareketleri>();
}
