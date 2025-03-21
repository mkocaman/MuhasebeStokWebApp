using System;
using System.Collections.Generic;

namespace MuhasebeStokWebApp.TempModels;

public partial class SistemAyarlari
{
    public Guid SistemAyarlariId { get; set; }

    public string AnaDovizKodu { get; set; } = null!;

    public Guid? AnaParaBirimiId { get; set; }

    public string? IkinciDovizKodu { get; set; }

    public Guid? IkinciParaBirimiId { get; set; }

    public string? UcuncuDovizKodu { get; set; }

    public Guid? UcuncuParaBirimiId { get; set; }

    public string SirketAdi { get; set; } = null!;

    public string? SirketAdresi { get; set; }

    public string? SirketTelefon { get; set; }

    public string? SirketEmail { get; set; }

    public string? SirketVergiNo { get; set; }

    public string? SirketVergiDairesi { get; set; }

    public bool OtomatikDovizGuncelleme { get; set; }

    public int DovizGuncellemeSikligi { get; set; }

    public DateTime? SonDovizGuncellemeTarihi { get; set; }

    public string? AktifParaBirimleri { get; set; }

    public bool Aktif { get; set; }

    public bool SoftDelete { get; set; }

    public DateTime OlusturmaTarihi { get; set; }

    public DateTime? GuncellemeTarihi { get; set; }

    public virtual Dovizler? AnaParaBirimi { get; set; }

    public virtual Dovizler? IkinciParaBirimi { get; set; }

    public virtual Dovizler? UcuncuParaBirimi { get; set; }
}
