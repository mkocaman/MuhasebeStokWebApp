using System;
using System.Collections.Generic;

namespace MuhasebeStokWebApp.TempModels;

public partial class Kasalar
{
    public Guid KasaId { get; set; }

    public string KasaAdi { get; set; } = null!;

    public string KasaTuru { get; set; } = null!;

    public string ParaBirimi { get; set; } = null!;

    public decimal AcilisBakiye { get; set; }

    public decimal GuncelBakiye { get; set; }

    public string Aciklama { get; set; } = null!;

    public bool Aktif { get; set; }

    public Guid? SorumluKullaniciId { get; set; }

    public Guid? OlusturanKullaniciId { get; set; }

    public Guid? SonGuncelleyenKullaniciId { get; set; }

    public DateTime OlusturmaTarihi { get; set; }

    public DateTime? GuncellemeTarihi { get; set; }

    public bool Silindi { get; set; }

    public virtual ICollection<BankaHareketleri> BankaHareketleriHedefKasas { get; set; } = new List<BankaHareketleri>();

    public virtual ICollection<BankaHareketleri> BankaHareketleriKaynakKasas { get; set; } = new List<BankaHareketleri>();

    public virtual ICollection<BankaHesapHareketleri> BankaHesapHareketleriHedefKasas { get; set; } = new List<BankaHesapHareketleri>();

    public virtual ICollection<BankaHesapHareketleri> BankaHesapHareketleriKaynakKasas { get; set; } = new List<BankaHesapHareketleri>();

    public virtual ICollection<KasaHareketleri> KasaHareketleriHedefKasas { get; set; } = new List<KasaHareketleri>();

    public virtual ICollection<KasaHareketleri> KasaHareketleriKasas { get; set; } = new List<KasaHareketleri>();
}
