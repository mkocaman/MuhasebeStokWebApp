using System;
using System.Collections.Generic;

namespace MuhasebeStokWebApp.TempModels;

public partial class BankaHesaplari
{
    public Guid BankaHesapId { get; set; }

    public Guid BankaId { get; set; }

    public string HesapAdi { get; set; } = null!;

    public string HesapNo { get; set; } = null!;

    public string Iban { get; set; } = null!;

    public string SubeAdi { get; set; } = null!;

    public string SubeKodu { get; set; } = null!;

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

    public bool Silindi { get; set; }

    public virtual Bankalar Banka { get; set; } = null!;

    public virtual ICollection<BankaHesapHareketleri> BankaHesapHareketleris { get; set; } = new List<BankaHesapHareketleri>();
}
