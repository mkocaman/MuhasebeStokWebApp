using System;
using System.Collections.Generic;

namespace MuhasebeStokWebApp.TempModels;

public partial class Bankalar
{
    public Guid BankaId { get; set; }

    public string BankaAdi { get; set; } = null!;

    public bool Aktif { get; set; }

    public Guid? OlusturanKullaniciId { get; set; }

    public Guid? SonGuncelleyenKullaniciId { get; set; }

    public DateTime OlusturmaTarihi { get; set; }

    public DateTime? GuncellemeTarihi { get; set; }

    public bool Silindi { get; set; }

    public virtual ICollection<BankaHareketleri> BankaHareketleris { get; set; } = new List<BankaHareketleri>();

    public virtual ICollection<BankaHesapHareketleri> BankaHesapHareketleris { get; set; } = new List<BankaHesapHareketleri>();

    public virtual ICollection<BankaHesaplari> BankaHesaplaris { get; set; } = new List<BankaHesaplari>();

    public virtual ICollection<KasaHareketleri> KasaHareketleriHedefBankas { get; set; } = new List<KasaHareketleri>();

    public virtual ICollection<KasaHareketleri> KasaHareketleriKaynakBankas { get; set; } = new List<KasaHareketleri>();
}
