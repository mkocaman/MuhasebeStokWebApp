using System;
using System.Collections.Generic;

namespace MuhasebeStokWebApp.TempModels;

public partial class ParaBirimleri
{
    public Guid ParaBirimiId { get; set; }

    public string Ad { get; set; } = null!;

    public string Kod { get; set; } = null!;

    public string Sembol { get; set; } = null!;

    public string OndalikAyraci { get; set; } = null!;

    public string BinlikAyraci { get; set; } = null!;

    public int OndalikHassasiyet { get; set; }

    public bool AnaParaBirimiMi { get; set; }

    public int Sira { get; set; }

    public string Aciklama { get; set; } = null!;

    public bool Aktif { get; set; }

    public bool Silindi { get; set; }

    public DateTime OlusturmaTarihi { get; set; }

    public DateTime? GuncellemeTarihi { get; set; }

    public string OlusturanKullaniciId { get; set; } = null!;

    public string SonGuncelleyenKullaniciId { get; set; } = null!;

    public string? GuncelleyenKullaniciId { get; set; }

    public virtual ICollection<Cariler> Carilers { get; set; } = new List<Cariler>();

    public virtual ICollection<KurDegerleri> KurDegerleris { get; set; } = new List<KurDegerleri>();

    public virtual ICollection<ParaBirimiIliskileri> ParaBirimiIliskileriHedefParaBirimis { get; set; } = new List<ParaBirimiIliskileri>();

    public virtual ICollection<ParaBirimiIliskileri> ParaBirimiIliskileriKaynakParaBirimis { get; set; } = new List<ParaBirimiIliskileri>();
}
