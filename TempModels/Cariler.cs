using System;
using System.Collections.Generic;

namespace MuhasebeStokWebApp.TempModels;

public partial class Cariler
{
    public Guid CariId { get; set; }

    public string CariAdi { get; set; } = null!;

    public string VergiNo { get; set; } = null!;

    public string Telefon { get; set; } = null!;

    public string Email { get; set; } = null!;

    public bool Aktif { get; set; }

    public Guid? OlusturanKullaniciId { get; set; }

    public Guid? SonGuncelleyenKullaniciId { get; set; }

    public DateTime? OlusturmaTarihi { get; set; }

    public DateTime? GuncellemeTarihi { get; set; }

    public bool SoftDelete { get; set; }

    public string Adres { get; set; } = null!;

    public string Aciklama { get; set; } = null!;

    public string Yetkili { get; set; } = null!;

    public virtual ICollection<BankaHareketleri> BankaHareketleris { get; set; } = new List<BankaHareketleri>();

    public virtual ICollection<CariHareketler> CariHareketlers { get; set; } = new List<CariHareketler>();

    public virtual ICollection<Faturalar> Faturalars { get; set; } = new List<Faturalar>();

    public virtual ICollection<Irsaliyeler> Irsaliyelers { get; set; } = new List<Irsaliyeler>();

    public virtual ICollection<KasaHareketleri> KasaHareketleris { get; set; } = new List<KasaHareketleri>();
}
