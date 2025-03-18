using System;
using System.Collections.Generic;

namespace MuhasebeStokWebApp.TempModels;

public partial class Dovizler
{
    public int DovizId { get; set; }

    public string DovizKodu { get; set; } = null!;

    public string DovizAdi { get; set; } = null!;

    public string Sembol { get; set; } = null!;

    public bool Aktif { get; set; }

    public bool SoftDelete { get; set; }

    public DateTime OlusturmaTarihi { get; set; }

    public DateTime? GuncellemeTarihi { get; set; }
}
