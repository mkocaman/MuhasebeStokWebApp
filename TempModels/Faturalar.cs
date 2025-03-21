using System;
using System.Collections.Generic;

namespace MuhasebeStokWebApp.TempModels;

public partial class Faturalar
{
    public Guid FaturaId { get; set; }

    public string FaturaNumarasi { get; set; } = null!;

    public string SiparisNumarasi { get; set; } = null!;

    public DateTime? FaturaTarihi { get; set; }

    public DateTime? VadeTarihi { get; set; }

    public Guid? CariId { get; set; }

    public int? FaturaTuruId { get; set; }

    public decimal? AraToplam { get; set; }

    public decimal? Kdvtoplam { get; set; }

    public decimal? GenelToplam { get; set; }

    public string OdemeDurumu { get; set; } = null!;

    public string FaturaNotu { get; set; } = null!;

    public bool? Resmi { get; set; }

    public string DovizTuru { get; set; } = null!;

    public decimal? DovizKuru { get; set; }

    public DateTime? OlusturmaTarihi { get; set; }

    public DateTime? GuncellemeTarihi { get; set; }

    public bool SoftDelete { get; set; }

    public bool? Aktif { get; set; }

    public Guid? OlusturanKullaniciId { get; set; }

    public Guid? SonGuncelleyenKullaniciId { get; set; }

    public int? OdemeTuruId { get; set; }

    public virtual Cariler? Cari { get; set; }

    public virtual ICollection<FaturaDetaylari> FaturaDetaylaris { get; set; } = new List<FaturaDetaylari>();

    public virtual ICollection<FaturaOdemeleri> FaturaOdemeleris { get; set; } = new List<FaturaOdemeleri>();

    public virtual FaturaTurleri? FaturaTuru { get; set; }

    public virtual ICollection<Irsaliyeler> Irsaliyelers { get; set; } = new List<Irsaliyeler>();

    public virtual OdemeTurleri? OdemeTuru { get; set; }
}
