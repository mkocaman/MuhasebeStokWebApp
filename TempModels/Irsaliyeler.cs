using System;
using System.Collections.Generic;

namespace MuhasebeStokWebApp.TempModels;

public partial class Irsaliyeler
{
    public Guid IrsaliyeId { get; set; }

    public Guid? FaturaId { get; set; }

    public DateTime IrsaliyeTarihi { get; set; }

    public DateTime SevkTarihi { get; set; }

    public string Aciklama { get; set; } = null!;

    public Guid? OlusturanKullaniciId { get; set; }

    public Guid? SonGuncelleyenKullaniciId { get; set; }

    public DateTime OlusturmaTarihi { get; set; }

    public DateTime? GuncellemeTarihi { get; set; }

    public bool SoftDelete { get; set; }

    public bool? Resmi { get; set; }

    public string IrsaliyeNumarasi { get; set; } = null!;

    public Guid CariId { get; set; }

    public string IrsaliyeTuru { get; set; } = null!;

    public string Durum { get; set; } = null!;

    public decimal? GenelToplam { get; set; }

    public int? IrsaliyeTuruId { get; set; }

    public virtual Cariler Cari { get; set; } = null!;

    public virtual Faturalar? Fatura { get; set; }

    public virtual ICollection<IrsaliyeDetaylari> IrsaliyeDetaylaris { get; set; } = new List<IrsaliyeDetaylari>();

    public virtual IrsaliyeTurleri? IrsaliyeTuruNavigation { get; set; }
}
