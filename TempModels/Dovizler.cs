using System;
using System.Collections.Generic;

namespace MuhasebeStokWebApp.TempModels;

public partial class Dovizler
{
    public Guid DovizId { get; set; }

    public string DovizKodu { get; set; } = null!;

    public string DovizAdi { get; set; } = null!;

    public string? Sembol { get; set; }

    public bool Aktif { get; set; }

    public bool SoftDelete { get; set; }

    public DateTime OlusturmaTarihi { get; set; }

    public DateTime? GuncellemeTarihi { get; set; }

    public virtual ICollection<DovizIliskileri> DovizIliskileriHedefParaBirimis { get; set; } = new List<DovizIliskileri>();

    public virtual ICollection<DovizIliskileri> DovizIliskileriKaynakParaBirimis { get; set; } = new List<DovizIliskileri>();

    public virtual ICollection<DovizIliskileri> DovizIliskileriParaBirimiId1Navigations { get; set; } = new List<DovizIliskileri>();

    public virtual ICollection<DovizIliskileri> DovizIliskileriParaBirimis { get; set; } = new List<DovizIliskileri>();

    public virtual ICollection<DovizKurlari> DovizKurlaris { get; set; } = new List<DovizKurlari>();

    public virtual ICollection<SistemAyarlari> SistemAyarlariAnaParaBirimis { get; set; } = new List<SistemAyarlari>();

    public virtual ICollection<SistemAyarlari> SistemAyarlariIkinciParaBirimis { get; set; } = new List<SistemAyarlari>();

    public virtual ICollection<SistemAyarlari> SistemAyarlariUcuncuParaBirimis { get; set; } = new List<SistemAyarlari>();
}
