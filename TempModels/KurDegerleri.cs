using System;
using System.Collections.Generic;

namespace MuhasebeStokWebApp.TempModels;

public partial class KurDegerleri
{
    public Guid KurDegeriId { get; set; }

    public Guid ParaBirimiId { get; set; }

    public DateTime Tarih { get; set; }

    public decimal Alis { get; set; }

    public decimal Satis { get; set; }

    public bool Aktif { get; set; }

    public bool Silindi { get; set; }

    public string? Aciklama { get; set; }

    public DateTime OlusturmaTarihi { get; set; }

    public DateTime? GuncellemeTarihi { get; set; }

    public string? OlusturanKullaniciId { get; set; }

    public string? SonGuncelleyenKullaniciId { get; set; }

    public string? DekontNo { get; set; }

    public decimal EfektifAlis { get; set; }

    public decimal EfektifSatis { get; set; }

    public string? GuncelleyenKullaniciId { get; set; }

    public virtual ParaBirimleri ParaBirimi { get; set; } = null!;
}
