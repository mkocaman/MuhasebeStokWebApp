using System;
using System.Collections.Generic;

namespace MuhasebeStokWebApp.TempModels;

public partial class KasaHareketleri
{
    public Guid KasaHareketId { get; set; }

    public Guid KasaId { get; set; }

    public Guid? CariId { get; set; }

    public Guid? KaynakBankaId { get; set; }

    public decimal Tutar { get; set; }

    public string HareketTuru { get; set; } = null!;

    public Guid? HedefKasaId { get; set; }

    public Guid? HedefBankaId { get; set; }

    public string IslemTuru { get; set; } = null!;

    public decimal DovizKuru { get; set; }

    public string KarsiParaBirimi { get; set; } = null!;

    public DateTime Tarih { get; set; }

    public string ReferansNo { get; set; } = null!;

    public string ReferansTuru { get; set; } = null!;

    public Guid? ReferansId { get; set; }

    public string Aciklama { get; set; } = null!;

    public Guid? IslemYapanKullaniciId { get; set; }

    public Guid? SonGuncelleyenKullaniciId { get; set; }

    public DateTime OlusturmaTarihi { get; set; }

    public DateTime? GuncellemeTarihi { get; set; }

    public bool SoftDelete { get; set; }

    public Guid? TransferId { get; set; }

    public virtual Cariler? Cari { get; set; }

    public virtual Bankalar? HedefBanka { get; set; }

    public virtual Kasalar? HedefKasa { get; set; }

    public virtual Kasalar Kasa { get; set; } = null!;

    public virtual Bankalar? KaynakBanka { get; set; }
}
