using System;
using System.Collections.Generic;

namespace MuhasebeStokWebApp.ScaffoldModels;

public partial class Cariler
{
    public Guid CariId { get; set; }

    public string Ad { get; set; }

    public string CariKodu { get; set; }

    public string CariTipi { get; set; }

    public string VergiNo { get; set; }

    public string VergiDairesi { get; set; }

    public string Telefon { get; set; }

    public string Email { get; set; }

    public string Yetkili { get; set; }

    public decimal BaslangicBakiye { get; set; }

    public string Adres { get; set; }

    public string Aciklama { get; set; }

    public string Il { get; set; }

    public string Ilce { get; set; }

    public string PostaKodu { get; set; }

    public string Ulke { get; set; }

    public string WebSitesi { get; set; }

    public string Notlar { get; set; }

    public bool AktifMi { get; set; }

    public Guid? OlusturanKullaniciId { get; set; }

    public Guid? SonGuncelleyenKullaniciId { get; set; }

    public DateTime OlusturmaTarihi { get; set; }

    public DateTime? GuncellemeTarihi { get; set; }

    public bool Silindi { get; set; }

    public virtual ICollection<BankaHareketleri> BankaHareketleris { get; set; } = new List<BankaHareketleri>();

    public virtual ICollection<BankaHesapHareketleri> BankaHesapHareketleris { get; set; } = new List<BankaHesapHareketleri>();

    public virtual ICollection<CariHareketler> CariHareketlers { get; set; } = new List<CariHareketler>();

    public virtual ICollection<Faturalar> Faturalars { get; set; } = new List<Faturalar>();

    public virtual ICollection<Irsaliyeler> Irsaliyelers { get; set; } = new List<Irsaliyeler>();

    public virtual ICollection<KasaHareketleri> KasaHareketleris { get; set; } = new List<KasaHareketleri>();
}
