using System;
using System.Collections.Generic;

namespace MuhasebeStokWebApp.TempModels;

public partial class SistemAyarlari
{
    public int Id { get; set; }

    public string Anahtar { get; set; } = null!;

    public string Deger { get; set; } = null!;

    public string Aciklama { get; set; } = null!;

    public DateTime OlusturmaTarihi { get; set; }

    public DateTime GuncellemeTarihi { get; set; }

    public bool Silindi { get; set; }
}
