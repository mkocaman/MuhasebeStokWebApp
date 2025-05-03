using System;
using System.Collections.Generic;

namespace MuhasebeStokWebApp.TempModels;

public partial class Irsaliyeler
{
    public Guid IrsaliyeId { get; set; }

    public string IrsaliyeNumarasi { get; set; } = null!;

    public DateTime IrsaliyeTarihi { get; set; }

    public Guid CariId { get; set; }

    public Guid? FaturaId { get; set; }

    public string Aciklama { get; set; } = null!;

    public string IrsaliyeTuru { get; set; } = null!;

    public bool Aktif { get; set; }

    public Guid OlusturanKullaniciId { get; set; }

    public Guid? SonGuncelleyenKullaniciId { get; set; }

    public bool Silindi { get; set; }

    public string Durum { get; set; } = null!;

    public int? IrsaliyeTuruId { get; set; }

    public DateTime OlusturmaTarihi { get; set; }

    public DateTime? GuncellemeTarihi { get; set; }

    public string? GuncelleyenKullaniciId { get; set; }

    public Guid? DepoId { get; set; }

    public virtual Cariler Cari { get; set; } = null!;

    public virtual Faturalar? Fatura { get; set; }

    public virtual ICollection<IrsaliyeDetaylari> IrsaliyeDetaylaris { get; set; } = new List<IrsaliyeDetaylari>();

    public virtual IrsaliyeTurleri? IrsaliyeTuruNavigation { get; set; }
}
