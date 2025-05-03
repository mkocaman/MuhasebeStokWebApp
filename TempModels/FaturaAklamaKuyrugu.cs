using System;
using System.Collections.Generic;

namespace MuhasebeStokWebApp.TempModels;

public partial class FaturaAklamaKuyrugu
{
    public Guid AklamaId { get; set; }

    public Guid FaturaKalemId { get; set; }

    public Guid UrunId { get; set; }

    public decimal AklananMiktar { get; set; }

    public DateTime OlusturmaTarihi { get; set; }

    public DateTime? AklanmaTarihi { get; set; }

    public string AklanmaNotu { get; set; } = null!;

    public Guid SozlesmeId { get; set; }

    public int Durum { get; set; }

    public decimal BirimFiyat { get; set; }

    public string ParaBirimi { get; set; } = null!;

    public decimal DovizKuru { get; set; }

    public Guid? OlusturanKullaniciId { get; set; }

    public Guid? GuncelleyenKullaniciId { get; set; }

    public DateTime? GuncellemeTarihi { get; set; }

    public bool Silindi { get; set; }

    public Guid? ResmiFaturaKalemId { get; set; }

    public Guid? FaturaId { get; set; }

    public virtual Faturalar? Fatura { get; set; }

    public virtual FaturaDetaylari FaturaKalem { get; set; } = null!;

    public virtual Sozlesmeler Sozlesme { get; set; } = null!;

    public virtual Urunler Urun { get; set; } = null!;
}
