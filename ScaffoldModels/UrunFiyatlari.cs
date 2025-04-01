using System;
using System.Collections.Generic;

namespace MuhasebeStokWebApp.ScaffoldModels;

public partial class UrunFiyatlari
{
    public Guid FiyatId { get; set; }

    public Guid? UrunId { get; set; }

    public decimal Fiyat { get; set; }

    public DateTime GecerliTarih { get; set; }

    public int? FiyatTipiId { get; set; }

    public Guid? OlusturanKullaniciId { get; set; }

    public Guid? SonGuncelleyenKullaniciId { get; set; }

    public DateTime? OlusturmaTarihi { get; set; }

    public DateTime? GuncellemeTarihi { get; set; }

    public bool Silindi { get; set; }

    public virtual FiyatTipleri FiyatTipi { get; set; }

    public virtual Urunler Urun { get; set; }
}
