using System;
using System.Collections.Generic;

namespace MuhasebeStokWebApp.ScaffoldModels;

public partial class ParaBirimiIliskileri
{
    public Guid ParaBirimiIliskiId { get; set; }

    public Guid KaynakParaBirimiId { get; set; }

    public Guid HedefParaBirimiId { get; set; }

    public bool Aktif { get; set; }

    public bool Silindi { get; set; }

    public string Aciklama { get; set; }

    public DateTime OlusturmaTarihi { get; set; }

    public DateTime? GuncellemeTarihi { get; set; }

    public string OlusturanKullaniciId { get; set; }

    public string SonGuncelleyenKullaniciId { get; set; }

    public virtual ParaBirimleri HedefParaBirimi { get; set; }

    public virtual ParaBirimleri KaynakParaBirimi { get; set; }
}
