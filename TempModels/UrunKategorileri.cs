using System;
using System.Collections.Generic;

namespace MuhasebeStokWebApp.TempModels;

public partial class UrunKategorileri
{
    public Guid KategoriId { get; set; }

    public string KategoriAdi { get; set; } = null!;

    public string Aciklama { get; set; } = null!;

    public bool Aktif { get; set; }

    public Guid? OlusturanKullaniciId { get; set; }

    public Guid? SonGuncelleyenKullaniciId { get; set; }

    public DateTime? OlusturmaTarihi { get; set; }

    public DateTime? GuncellemeTarihi { get; set; }

    public bool SoftDelete { get; set; }

    public virtual ICollection<Urunler> Urunlers { get; set; } = new List<Urunler>();
}
