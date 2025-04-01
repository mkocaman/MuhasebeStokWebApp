using System;
using System.Collections.Generic;

namespace MuhasebeStokWebApp.ScaffoldModels;

public partial class FaturaOdemeleri
{
    public Guid OdemeId { get; set; }

    public Guid FaturaId { get; set; }

    public DateTime OdemeTarihi { get; set; }

    public decimal OdemeTutari { get; set; }

    public string OdemeTuru { get; set; }

    public string Aciklama { get; set; }

    public Guid? OlusturanKullaniciId { get; set; }

    public Guid? SonGuncelleyenKullaniciId { get; set; }

    public DateTime? OlusturmaTarihi { get; set; }

    public DateTime? GuncellemeTarihi { get; set; }

    public bool Silindi { get; set; }

    public bool Aktif { get; set; }

    public virtual Faturalar Fatura { get; set; }
}
