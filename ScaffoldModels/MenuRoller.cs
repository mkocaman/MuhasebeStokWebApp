using System;
using System.Collections.Generic;

namespace MuhasebeStokWebApp.ScaffoldModels;

public partial class MenuRoller
{
    public Guid MenuId { get; set; }

    public string RolId { get; set; }

    public Guid MenuRolId { get; set; }

    public DateTime OlusturmaTarihi { get; set; }

    public virtual Menuler Menu { get; set; }

    public virtual AspNetRole Rol { get; set; }
}
