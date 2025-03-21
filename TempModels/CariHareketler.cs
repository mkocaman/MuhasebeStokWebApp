using System;
using System.Collections.Generic;

namespace MuhasebeStokWebApp.TempModels;

public partial class CariHareketler
{
    public Guid CariHareketId { get; set; }

    public Guid CariId { get; set; }

    public decimal Tutar { get; set; }

    public string HareketTuru { get; set; } = null!;

    public DateTime Tarih { get; set; }

    public string? ReferansNo { get; set; }

    public string? ReferansTuru { get; set; }

    public Guid? ReferansId { get; set; }

    public string? Aciklama { get; set; }

    public Guid? IslemYapanKullaniciId { get; set; }

    public Guid? SonGuncelleyenKullaniciId { get; set; }

    public DateTime OlusturmaTarihi { get; set; }

    public DateTime? GuncellemeTarihi { get; set; }

    public bool SoftDelete { get; set; }

    public virtual Cariler Cari { get; set; } = null!;
}
