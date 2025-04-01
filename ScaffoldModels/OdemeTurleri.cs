using System;
using System.Collections.Generic;

namespace MuhasebeStokWebApp.ScaffoldModels;

public partial class OdemeTurleri
{
    public int OdemeTuruId { get; set; }

    public string OdemeTuruAdi { get; set; }

    public virtual ICollection<Faturalar> Faturalars { get; set; } = new List<Faturalar>();
}
