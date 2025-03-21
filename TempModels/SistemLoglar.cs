using System;
using System.Collections.Generic;

namespace MuhasebeStokWebApp.TempModels;

public partial class SistemLoglar
{
    public Guid LogId { get; set; }

    public string IslemTuru { get; set; } = null!;

    public Guid? KayitId { get; set; }

    public string TabloAdi { get; set; } = null!;

    public string KayitAdi { get; set; } = null!;

    public DateTime IslemTarihi { get; set; }

    public string Aciklama { get; set; } = null!;

    public string? KullaniciId { get; set; }

    public string KullaniciAdi { get; set; } = null!;

    public string Ipadresi { get; set; } = null!;

    public string? Tarayici { get; set; }

    public bool Basarili { get; set; }

    public string? HataMesaji { get; set; }

    public string? IlgiliId { get; set; }

    public bool SoftDelete { get; set; }
}
