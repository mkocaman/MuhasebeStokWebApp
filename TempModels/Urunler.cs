using System;
using System.Collections.Generic;

namespace MuhasebeStokWebApp.TempModels;

public partial class Urunler
{
    public Guid UrunId { get; set; }

    public string UrunKodu { get; set; } = null!;

    public string UrunAdi { get; set; } = null!;

    public Guid? BirimId { get; set; }

    public bool Aktif { get; set; }

    public int Kdvorani { get; set; }

    public Guid? OlusturanKullaniciId { get; set; }

    public Guid? SonGuncelleyenKullaniciId { get; set; }

    public DateTime? OlusturmaTarihi { get; set; }

    public DateTime? GuncellemeTarihi { get; set; }

    public bool Silindi { get; set; }

    public Guid? KategoriId { get; set; }

    public decimal? DovizliListeFiyati { get; set; }

    public decimal? DovizliMaliyetFiyati { get; set; }

    public decimal? DovizliSatisFiyati { get; set; }

    public string Aciklama { get; set; } = null!;

    public virtual Birimler? Birim { get; set; }

    public virtual ICollection<FaturaAklamaKuyrugu> FaturaAklamaKuyrugus { get; set; } = new List<FaturaAklamaKuyrugu>();

    public virtual ICollection<FaturaDetaylari> FaturaDetaylaris { get; set; } = new List<FaturaDetaylari>();

    public virtual ICollection<IrsaliyeDetaylari> IrsaliyeDetaylaris { get; set; } = new List<IrsaliyeDetaylari>();

    public virtual UrunKategorileri? Kategori { get; set; }

    public virtual ICollection<StokFifoKayitlari> StokFifoKayitlaris { get; set; } = new List<StokFifoKayitlari>();

    public virtual ICollection<StokHareketleri> StokHareketleris { get; set; } = new List<StokHareketleri>();

    public virtual ICollection<UrunFiyatlari> UrunFiyatlaris { get; set; } = new List<UrunFiyatlari>();
}
