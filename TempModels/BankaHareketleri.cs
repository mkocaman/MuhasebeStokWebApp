using System;
using System.Collections.Generic;

namespace MuhasebeStokWebApp.TempModels;

public partial class BankaHareketleri
{
    public Guid BankaHareketId { get; set; }

    public Guid BankaId { get; set; }

    public Guid? KaynakKasaId { get; set; }

    public Guid? HedefKasaId { get; set; }

    public Guid? TransferId { get; set; }

    public Guid? CariId { get; set; }

    public decimal Tutar { get; set; }

    public string HareketTuru { get; set; } = null!;

    public DateTime Tarih { get; set; }

    public string ReferansNo { get; set; } = null!;

    public string ReferansTuru { get; set; } = null!;

    public Guid? ReferansId { get; set; }

    public string Aciklama { get; set; } = null!;

    public string DekontNo { get; set; } = null!;

    public Guid? IslemYapanKullaniciId { get; set; }

    public Guid? SonGuncelleyenKullaniciId { get; set; }

    public DateTime OlusturmaTarihi { get; set; }

    public DateTime? GuncellemeTarihi { get; set; }

    public bool Silindi { get; set; }

    public string KarsiUnvan { get; set; } = null!;

    public string KarsiBankaAdi { get; set; } = null!;

    public string KarsiIban { get; set; } = null!;

    public virtual Bankalar Banka { get; set; } = null!;

    public virtual Cariler? Cari { get; set; }

    public virtual Kasalar? HedefKasa { get; set; }

    public virtual Kasalar? KaynakKasa { get; set; }
}
