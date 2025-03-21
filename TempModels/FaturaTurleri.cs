using System;
using System.Collections.Generic;

namespace MuhasebeStokWebApp.TempModels;

public partial class FaturaTurleri
{
    public int FaturaTuruId { get; set; }

    public string FaturaTuruAdi { get; set; } = null!;

    public string HareketTuru { get; set; } = null!;

    public virtual ICollection<Faturalar> Faturalars { get; set; } = new List<Faturalar>();
}
