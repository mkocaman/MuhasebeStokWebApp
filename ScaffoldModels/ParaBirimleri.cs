using System;
using System.Collections.Generic;

namespace MuhasebeStokWebApp.ScaffoldModels;

public partial class ParaBirimleri
{
    public Guid ParaBirimiId { get; set; }

    public string Ad { get; set; }

    public string Kod { get; set; }

    public string Sembol { get; set; }

    public string OndalikAyraci { get; set; }

    public string BinlikAyraci { get; set; }

    public int OndalikHassasiyet { get; set; }

    public bool AnaParaBirimiMi { get; set; }

    public int Sira { get; set; }

    public string Aciklama { get; set; }

    public bool Aktif { get; set; }

    public bool Silindi { get; set; }

    public DateTime OlusturmaTarihi { get; set; }

    public DateTime? GuncellemeTarihi { get; set; }

    public string OlusturanKullaniciId { get; set; }

    public string SonGuncelleyenKullaniciId { get; set; }

    public virtual ICollection<KurDegerleri> KurDegerleris { get; set; } = new List<KurDegerleri>();

    public virtual ICollection<ParaBirimiIliskileri> ParaBirimiIliskileriHedefParaBirimis { get; set; } = new List<ParaBirimiIliskileri>();

    public virtual ICollection<ParaBirimiIliskileri> ParaBirimiIliskileriKaynakParaBirimis { get; set; } = new List<ParaBirimiIliskileri>();
}
