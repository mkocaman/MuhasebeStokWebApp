using System;
using System.Collections.Generic;

namespace MuhasebeStokWebApp.ScaffoldModels;

public partial class GenelSistemAyarlari
{
    public int SistemAyarlariId { get; set; }

    public string AnaDovizKodu { get; set; }

    public string SirketAdi { get; set; }

    public string SirketAdresi { get; set; }

    public string SirketTelefon { get; set; }

    public string SirketEmail { get; set; }

    public string SirketVergiNo { get; set; }

    public string SirketVergiDairesi { get; set; }

    public bool OtomatikDovizGuncelleme { get; set; }

    public int DovizGuncellemeSikligi { get; set; }

    public DateTime SonDovizGuncellemeTarihi { get; set; }

    public string AktifParaBirimleri { get; set; }

    public bool Aktif { get; set; }

    public bool Silindi { get; set; }

    public DateTime OlusturmaTarihi { get; set; }

    public DateTime? GuncellemeTarihi { get; set; }
}
