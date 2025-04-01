using System;
using System.Collections.Generic;

namespace MuhasebeStokWebApp.ScaffoldModels;

public partial class FaturaTurleri
{
    public int FaturaTuruId { get; set; }

    public string FaturaTuruAdi { get; set; }

    public string HareketTuru { get; set; }

    public virtual ICollection<Faturalar> Faturalars { get; set; } = new List<Faturalar>();
}
