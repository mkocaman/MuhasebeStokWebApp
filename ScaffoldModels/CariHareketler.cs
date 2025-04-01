using System;
using System.Collections.Generic;

namespace MuhasebeStokWebApp.ScaffoldModels;

public partial class CariHareketler
{
    public Guid CariHareketId { get; set; }

    public Guid CariId { get; set; }

    public string HareketTuru { get; set; }

    public decimal Tutar { get; set; }

    public DateTime Tarih { get; set; }

    public DateTime? VadeTarihi { get; set; }

    public string ReferansNo { get; set; }

    public string ReferansTuru { get; set; }

    public Guid? ReferansId { get; set; }

    public string Aciklama { get; set; }

    public DateTime OlusturmaTarihi { get; set; }

    public Guid? OlusturanKullaniciId { get; set; }

    public bool Silindi { get; set; }

    public DateTime? GuncellemeTarihi { get; set; }

    public virtual Cariler Cari { get; set; }
}
