using System;
using System.Collections.Generic;

namespace MuhasebeStokWebApp.TempModels;

public partial class DovizKurlari
{
    public Guid DovizKuruId { get; set; }

    public Guid ParaBirimiId { get; set; }

    public decimal KurDegeri { get; set; }

    public decimal? AlisFiyati { get; set; }

    public decimal? SatisFiyati { get; set; }

    public DateTime Tarih { get; set; }

    public string Kaynak { get; set; } = null!;

    public string? Aciklama { get; set; }

    public bool Aktif { get; set; }

    public bool SoftDelete { get; set; }

    public DateTime OlusturmaTarihi { get; set; }

    public DateTime? GuncellemeTarihi { get; set; }

    public Guid? DovizIliskiId { get; set; }

    public virtual DovizIliskileri? DovizIliski { get; set; }

    public virtual Dovizler ParaBirimi { get; set; } = null!;
}
