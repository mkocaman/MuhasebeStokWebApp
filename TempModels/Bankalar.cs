using System;
using System.Collections.Generic;

namespace MuhasebeStokWebApp.TempModels;

public partial class Bankalar
{
    public Guid BankaId { get; set; }

    public string BankaAdi { get; set; } = null!;

    public string SubeAdi { get; set; } = null!;

    public string SubeKodu { get; set; } = null!;

    public string HesapNo { get; set; } = null!;

    public string Iban { get; set; } = null!;

    public string ParaBirimi { get; set; } = null!;

    public decimal AcilisBakiye { get; set; }

    public decimal GuncelBakiye { get; set; }

    public string Aciklama { get; set; } = null!;

    public bool Aktif { get; set; }

    public Guid? YetkiliKullaniciId { get; set; }

    public Guid? OlusturanKullaniciId { get; set; }

    public Guid? SonGuncelleyenKullaniciId { get; set; }

    public DateTime OlusturmaTarihi { get; set; }

    public DateTime? GuncellemeTarihi { get; set; }

    public bool SoftDelete { get; set; }

    public virtual ICollection<BankaHareketleri> BankaHareketleris { get; set; } = new List<BankaHareketleri>();

    public virtual ICollection<KasaHareketleri> KasaHareketleriHedefBankas { get; set; } = new List<KasaHareketleri>();

    public virtual ICollection<KasaHareketleri> KasaHareketleriKaynakBankas { get; set; } = new List<KasaHareketleri>();
}
