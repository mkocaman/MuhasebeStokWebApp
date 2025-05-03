using System;
using System.Collections.Generic;

namespace MuhasebeStokWebApp.TempModels;

public partial class Menuler
{
    public Guid MenuId { get; set; }

    public string Ad { get; set; } = null!;

    public string Icon { get; set; } = null!;

    public string Controller { get; set; } = null!;

    public string Action { get; set; } = null!;

    public int Sira { get; set; }

    public Guid? UstMenuId { get; set; }

    public string Url { get; set; } = null!;

    public bool AktifMi { get; set; }

    public bool Silindi { get; set; }

    public DateTime OlusturmaTarihi { get; set; }

    public DateTime? GuncellemeTarihi { get; set; }

    public Guid? OlusturanKullaniciId { get; set; }

    public Guid? SonGuncelleyenKullaniciId { get; set; }

    public virtual ICollection<Menuler> InverseUstMenu { get; set; } = new List<Menuler>();

    public virtual ICollection<MenuRoller> MenuRollers { get; set; } = new List<MenuRoller>();

    public virtual Menuler? UstMenu { get; set; }
}
