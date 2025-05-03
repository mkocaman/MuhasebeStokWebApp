using System;
using System.Collections.Generic;

namespace MuhasebeStokWebApp.TempModels;

public partial class MenuRoller
{
    public Guid MenuId { get; set; }

    public string RolId { get; set; } = null!;

    public Guid MenuRolId { get; set; }

    public DateTime OlusturmaTarihi { get; set; }

    public virtual Menuler Menu { get; set; } = null!;

    public virtual AspNetRole Rol { get; set; } = null!;
}
