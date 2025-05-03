using System;
using System.Collections.Generic;

namespace MuhasebeStokWebApp.TempModels;

public partial class ParaBirimiIliskileri
{
    public Guid ParaBirimiIliskiId { get; set; }

    public Guid KaynakParaBirimiId { get; set; }

    public Guid HedefParaBirimiId { get; set; }

    public bool Aktif { get; set; }

    public bool Silindi { get; set; }

    public string Aciklama { get; set; } = null!;

    public DateTime OlusturmaTarihi { get; set; }

    public DateTime? GuncellemeTarihi { get; set; }

    public string OlusturanKullaniciId { get; set; } = null!;

    public string SonGuncelleyenKullaniciId { get; set; } = null!;

    public string? GuncelleyenKullaniciId { get; set; }

    public virtual ParaBirimleri HedefParaBirimi { get; set; } = null!;

    public virtual ParaBirimleri KaynakParaBirimi { get; set; } = null!;
}
