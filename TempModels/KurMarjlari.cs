using System;
using System.Collections.Generic;

namespace MuhasebeStokWebApp.TempModels;

public partial class KurMarjlari
{
    public Guid KurMarjId { get; set; }

    public decimal SatisMarji { get; set; }

    public bool Varsayilan { get; set; }

    public string Tanim { get; set; } = null!;

    public bool Aktif { get; set; }

    public bool Silindi { get; set; }

    public DateTime OlusturmaTarihi { get; set; }

    public DateTime? GuncellemeTarihi { get; set; }

    public string? OlusturanKullaniciId { get; set; }

    public string? SonGuncelleyenKullaniciId { get; set; }
}
