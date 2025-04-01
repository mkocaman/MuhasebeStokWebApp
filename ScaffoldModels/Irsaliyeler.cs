using System;
using System.Collections.Generic;

namespace MuhasebeStokWebApp.ScaffoldModels;

public partial class Irsaliyeler
{
    public Guid IrsaliyeId { get; set; }

    public string IrsaliyeNumarasi { get; set; }

    public DateTime IrsaliyeTarihi { get; set; }

    public Guid CariId { get; set; }

    public Guid? FaturaId { get; set; }

    public string Aciklama { get; set; }

    public string IrsaliyeTuru { get; set; }

    public bool Aktif { get; set; }

    public Guid OlusturanKullaniciId { get; set; }

    public Guid? SonGuncelleyenKullaniciId { get; set; }

    public bool Silindi { get; set; }

    public string Durum { get; set; }

    public int? IrsaliyeTuruId { get; set; }

    public DateTime OlusturmaTarihi { get; set; }

    public DateTime? GuncellemeTarihi { get; set; }

    public virtual Cariler Cari { get; set; }

    public virtual Faturalar Fatura { get; set; }

    public virtual ICollection<IrsaliyeDetaylari> IrsaliyeDetaylaris { get; set; } = new List<IrsaliyeDetaylari>();

    public virtual IrsaliyeTurleri IrsaliyeTuruNavigation { get; set; }
}
