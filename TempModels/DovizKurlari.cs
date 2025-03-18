using System;
using System.Collections.Generic;

namespace MuhasebeStokWebApp.TempModels;

public partial class DovizKurlari
{
    public Guid DovizKuruId { get; set; }

    public string DovizKodu { get; set; } = null!;

    public string DovizAdi { get; set; } = null!;

    public decimal AlisFiyati { get; set; }

    public decimal SatisFiyati { get; set; }

    public decimal EfektifAlisFiyati { get; set; }

    public decimal EfektifSatisFiyati { get; set; }

    public DateTime Tarih { get; set; }

    public DateTime GuncellemeTarihi { get; set; }

    public bool Aktif { get; set; }

    public string HedefParaBirimi { get; set; } = null!;

    public string KaynakParaBirimi { get; set; } = null!;

    public decimal KurDegeri { get; set; }

    public string? Aciklama { get; set; }

    public string BazParaBirimi { get; set; } = null!;

    public string Kaynak { get; set; } = null!;

    public decimal Kur { get; set; }

    public DateTime OlusturmaTarihi { get; set; }

    public string ParaBirimi { get; set; } = null!;

    public bool SoftDelete { get; set; }
}
