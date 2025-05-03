using System;
using System.Collections.Generic;

namespace MuhasebeStokWebApp.TempModels;

public partial class GenelSistemAyarlari
{
    public int SistemAyarlariId { get; set; }

    public string AnaDovizKodu { get; set; } = null!;

    public string SirketAdi { get; set; } = null!;

    public string SirketAdresi { get; set; } = null!;

    public string SirketTelefon { get; set; } = null!;

    public string SirketEmail { get; set; } = null!;

    public string SirketVergiNo { get; set; } = null!;

    public string SirketVergiDairesi { get; set; } = null!;

    public bool OtomatikDovizGuncelleme { get; set; }

    public int DovizGuncellemeSikligi { get; set; }

    public DateTime SonDovizGuncellemeTarihi { get; set; }

    public string? AktifParaBirimleri { get; set; }

    public bool Aktif { get; set; }

    public bool Silindi { get; set; }

    public DateTime OlusturmaTarihi { get; set; }

    public DateTime? GuncellemeTarihi { get; set; }
}
