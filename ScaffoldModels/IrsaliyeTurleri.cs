using System;
using System.Collections.Generic;

namespace MuhasebeStokWebApp.ScaffoldModels;

public partial class IrsaliyeTurleri
{
    public int IrsaliyeTuruId { get; set; }

    public string IrsaliyeTuruAdi { get; set; }

    public string HareketTuru { get; set; }

    public virtual ICollection<Irsaliyeler> Irsaliyelers { get; set; } = new List<Irsaliyeler>();
}
