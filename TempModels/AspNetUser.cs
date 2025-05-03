using System;
using System.Collections.Generic;

namespace MuhasebeStokWebApp.TempModels;

public partial class AspNetUser
{
    public string Id { get; set; } = null!;

    public string? Ad { get; set; }

    public string? Soyad { get; set; }

    public string? TelefonNo { get; set; }

    public string? Adres { get; set; }

    public bool Aktif { get; set; }

    public bool Silindi { get; set; }

    public DateTime OlusturmaTarihi { get; set; }

    public DateTime? GuncellemeTarihi { get; set; }

    public string? UserName { get; set; }

    public string? NormalizedUserName { get; set; }

    public string? Email { get; set; }

    public string? NormalizedEmail { get; set; }

    public bool EmailConfirmed { get; set; }

    public string? PasswordHash { get; set; }

    public string? SecurityStamp { get; set; }

    public string? ConcurrencyStamp { get; set; }

    public string? PhoneNumber { get; set; }

    public bool PhoneNumberConfirmed { get; set; }

    public bool TwoFactorEnabled { get; set; }

    public DateTimeOffset? LockoutEnd { get; set; }

    public bool LockoutEnabled { get; set; }

    public int AccessFailedCount { get; set; }

    public virtual ICollection<AspNetUserClaim> AspNetUserClaims { get; set; } = new List<AspNetUserClaim>();

    public virtual ICollection<AspNetUserLogin> AspNetUserLogins { get; set; } = new List<AspNetUserLogin>();

    public virtual ICollection<AspNetUserToken> AspNetUserTokens { get; set; } = new List<AspNetUserToken>();

    public virtual ICollection<SistemLoglar> SistemLoglars { get; set; } = new List<SistemLoglar>();

    public virtual ICollection<AspNetRole> Roles { get; set; } = new List<AspNetRole>();
}
