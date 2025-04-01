using System;
using System.Collections.Generic;

namespace MuhasebeStokWebApp.ScaffoldModels;

public partial class KasaHareketleri
{
    public Guid KasaHareketId { get; set; }

    public Guid KasaId { get; set; }

    public Guid? CariId { get; set; }

    public Guid? KaynakBankaId { get; set; }

    public decimal Tutar { get; set; }

    public string HareketTuru { get; set; }

    public Guid? HedefKasaId { get; set; }

    public Guid? HedefBankaId { get; set; }

    public string IslemTuru { get; set; }

    public decimal DovizKuru { get; set; }

    public string KarsiParaBirimi { get; set; }

    public DateTime Tarih { get; set; }

    public string ReferansNo { get; set; }

    public string ReferansTuru { get; set; }

    public Guid? ReferansId { get; set; }

    public string Aciklama { get; set; }

    public Guid? IslemYapanKullaniciId { get; set; }

    public Guid? SonGuncelleyenKullaniciId { get; set; }

    public DateTime OlusturmaTarihi { get; set; }

    public DateTime? GuncellemeTarihi { get; set; }

    public bool Silindi { get; set; }

    public Guid? TransferId { get; set; }

    public virtual Cariler Cari { get; set; }

    public virtual Bankalar HedefBanka { get; set; }

    public virtual Kasalar HedefKasa { get; set; }

    public virtual Kasalar Kasa { get; set; }

    public virtual Bankalar KaynakBanka { get; set; }
}
