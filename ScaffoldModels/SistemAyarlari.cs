using System;
using System.Collections.Generic;

namespace MuhasebeStokWebApp.ScaffoldModels;

public partial class SistemAyarlari
{
    public int Id { get; set; }

    public string Anahtar { get; set; }

    public string Deger { get; set; }

    public string Aciklama { get; set; }

    public DateTime OlusturmaTarihi { get; set; }

    public DateTime GuncellemeTarihi { get; set; }

    public bool Silindi { get; set; }
}
