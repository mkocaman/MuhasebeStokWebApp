using System;
using System.Collections.Generic;

namespace MuhasebeStokWebApp.ScaffoldModels;

public partial class BankaHesaplari
{
    public Guid BankaHesapId { get; set; }

    public Guid BankaId { get; set; }

    public string HesapAdi { get; set; }

    public string HesapNo { get; set; }

    public string Iban { get; set; }

    public string SubeAdi { get; set; }

    public string SubeKodu { get; set; }

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

    public virtual Bankalar Banka { get; set; }

    public virtual ICollection<BankaHesapHareketleri> BankaHesapHareketleris { get; set; } = new List<BankaHesapHareketleri>();
}
