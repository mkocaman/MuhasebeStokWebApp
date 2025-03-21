using System;
using System.Collections.Generic;

namespace MuhasebeStokWebApp.TempModels;

public partial class DovizIliskileri
{
    public Guid DovizIliskiId { get; set; }

    public Guid KaynakParaBirimiId { get; set; }

    public Guid HedefParaBirimiId { get; set; }

    public bool Aktif { get; set; }

    public bool SoftDelete { get; set; }

    public DateTime OlusturmaTarihi { get; set; }

    public DateTime? GuncellemeTarihi { get; set; }

    public Guid? ParaBirimiId { get; set; }

    public Guid? ParaBirimiId1 { get; set; }

    public virtual ICollection<DovizKurlari> DovizKurlaris { get; set; } = new List<DovizKurlari>();

    public virtual Dovizler HedefParaBirimi { get; set; } = null!;

    public virtual Dovizler KaynakParaBirimi { get; set; } = null!;

    public virtual Dovizler? ParaBirimi { get; set; }

    public virtual Dovizler? ParaBirimiId1Navigation { get; set; }
}
