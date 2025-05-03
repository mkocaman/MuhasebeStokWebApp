using System;
using System.Collections.Generic;

namespace MuhasebeStokWebApp.TempModels;

public partial class Cariler
{
    public Guid CariId { get; set; }

    public string Ad { get; set; } = null!;

    public string? CariUnvani { get; set; }

    public string CariKodu { get; set; } = null!;

    public string CariTipi { get; set; } = null!;

    public string? VergiNo { get; set; }

    public string? VergiDairesi { get; set; }

    public string Telefon { get; set; } = null!;

    public string Email { get; set; } = null!;

    public string Yetkili { get; set; } = null!;

    public decimal BaslangicBakiye { get; set; }

    public string Adres { get; set; } = null!;

    public string? Aciklama { get; set; }

    public string Il { get; set; } = null!;

    public string Ilce { get; set; } = null!;

    public string PostaKodu { get; set; } = null!;

    public string Ulke { get; set; } = null!;

    public string? WebSitesi { get; set; }

    public string Notlar { get; set; } = null!;

    public Guid? VarsayilanParaBirimiId { get; set; }

    public bool VarsayilanKurKullan { get; set; }

    public bool AktifMi { get; set; }

    public Guid? OlusturanKullaniciId { get; set; }

    public Guid? SonGuncelleyenKullaniciId { get; set; }

    public DateTime OlusturmaTarihi { get; set; }

    public DateTime? GuncellemeTarihi { get; set; }

    public bool Silindi { get; set; }

    public string? GuncelleyenKullaniciId { get; set; }

    public bool Aktif { get; set; }

    public virtual ICollection<BankaHareketleri> BankaHareketleris { get; set; } = new List<BankaHareketleri>();

    public virtual ICollection<BankaHesapHareketleri> BankaHesapHareketleris { get; set; } = new List<BankaHesapHareketleri>();

    public virtual ICollection<CariHareketler> CariHareketlers { get; set; } = new List<CariHareketler>();

    public virtual ICollection<Faturalar> Faturalars { get; set; } = new List<Faturalar>();

    public virtual ICollection<Irsaliyeler> Irsaliyelers { get; set; } = new List<Irsaliyeler>();

    public virtual ICollection<KasaHareketleri> KasaHareketleris { get; set; } = new List<KasaHareketleri>();

    public virtual ICollection<Sozlesmeler> Sozlesmelers { get; set; } = new List<Sozlesmeler>();

    public virtual ParaBirimleri? VarsayilanParaBirimi { get; set; }
}
