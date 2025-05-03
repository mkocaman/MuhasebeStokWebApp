using System;
using System.Collections.Generic;

namespace MuhasebeStokWebApp.TempModels;

public partial class SistemLoglar
{
    public int Id { get; set; }

    public Guid LogId { get; set; }

    public string LogTuru { get; set; } = null!;

    public string Mesaj { get; set; } = null!;

    public string Sayfa { get; set; } = null!;

    public DateTime OlusturmaTarihi { get; set; }

    public string IslemTuru { get; set; } = null!;

    public int? LogTuruInt { get; set; }

    public string Aciklama { get; set; } = null!;

    public string HataMesaji { get; set; } = null!;

    public string KullaniciAdi { get; set; } = null!;

    public string Ipadresi { get; set; } = null!;

    public DateTime IslemTarihi { get; set; }

    public bool Basarili { get; set; }

    public string TabloAdi { get; set; } = null!;

    public string KayitAdi { get; set; } = null!;

    public Guid? KayitId { get; set; }

    public string? KullaniciId { get; set; }

    public virtual AspNetUser? Kullanici { get; set; }
}
