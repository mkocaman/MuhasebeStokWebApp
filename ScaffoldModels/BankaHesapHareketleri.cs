using System;
using System.Collections.Generic;

namespace MuhasebeStokWebApp.ScaffoldModels;

public partial class BankaHesapHareketleri
{
    public Guid BankaHesapHareketId { get; set; }

    public Guid BankaHesapId { get; set; }

    public Guid BankaId { get; set; }

    public Guid? KaynakKasaId { get; set; }

    public Guid? HedefKasaId { get; set; }

    public Guid? TransferId { get; set; }

    public Guid? CariId { get; set; }

    public decimal Tutar { get; set; }

    public string HareketTuru { get; set; }

    public DateTime Tarih { get; set; }

    public string ReferansNo { get; set; }

    public string ReferansTuru { get; set; }

    public Guid? ReferansId { get; set; }

    public string Aciklama { get; set; }

    public string DekontNo { get; set; }

    public Guid? IslemYapanKullaniciId { get; set; }

    public Guid? SonGuncelleyenKullaniciId { get; set; }

    public DateTime OlusturmaTarihi { get; set; }

    public DateTime? GuncellemeTarihi { get; set; }

    public bool Silindi { get; set; }

    public string KarsiUnvan { get; set; }

    public string KarsiBankaAdi { get; set; }

    public string KarsiIban { get; set; }

    public virtual Bankalar Banka { get; set; }

    public virtual BankaHesaplari BankaHesap { get; set; }

    public virtual Cariler Cari { get; set; }

    public virtual Kasalar HedefKasa { get; set; }

    public virtual Kasalar KaynakKasa { get; set; }
}
