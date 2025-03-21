using System;
using System.Collections.Generic;

namespace MuhasebeStokWebApp.TempModels;

public partial class IrsaliyeTurleri
{
    public int IrsaliyeTuruId { get; set; }

    public string IrsaliyeTuruAdi { get; set; } = null!;

    public string HareketTuru { get; set; } = null!;

    public virtual ICollection<Irsaliyeler> Irsaliyelers { get; set; } = new List<Irsaliyeler>();
}
