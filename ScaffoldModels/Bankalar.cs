using System;
using System.Collections.Generic;

namespace MuhasebeStokWebApp.ScaffoldModels;

public partial class Bankalar
{
    public Guid BankaId { get; set; }

    public string BankaAdi { get; set; }

    public string SubeAdi { get; set; }

    public string SubeKodu { get; set; }

    public string HesapNo { get; set; }

    public string Iban { get; set; }

    public string ParaBirimi { get; set; }

    public decimal AcilisBakiye { get; set; }

    public decimal GuncelBakiye { get; set; }

    public string Aciklama { get; set; }

    public bool Aktif { get; set; }

    public Guid? YetkiliKullaniciId { get; set; }

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
