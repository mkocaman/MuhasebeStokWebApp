using System;
using System.Collections.Generic;

namespace MuhasebeStokWebApp.TempModels;

public partial class FiyatTipleri
{
    public int FiyatTipiId { get; set; }

    public string TipAdi { get; set; } = null!;

    public virtual ICollection<UrunFiyatlari> UrunFiyatlaris { get; set; } = new List<UrunFiyatlari>();
}
