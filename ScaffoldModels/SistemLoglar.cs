using System;
using System.Collections.Generic;

namespace MuhasebeStokWebApp.ScaffoldModels;

public partial class SistemLoglar
{
    public int Id { get; set; }

    public Guid LogId { get; set; }

    public string LogTuru { get; set; }

    public string Mesaj { get; set; }

    public string Sayfa { get; set; }

    public DateTime OlusturmaTarihi { get; set; }

    public string IslemTuru { get; set; }

    public int? LogTuruInt { get; set; }

    public string Aciklama { get; set; }

    public string HataMesaji { get; set; }

    public string KullaniciAdi { get; set; }

    public string Ipadresi { get; set; }

    public DateTime IslemTarihi { get; set; }

    public bool Basarili { get; set; }

    public string TabloAdi { get; set; }

    public string KayitAdi { get; set; }

    public Guid? KayitId { get; set; }

    public Guid? KullaniciId { get; set; }

    public string ApplicationUserId { get; set; }

    public virtual AspNetUser ApplicationUser { get; set; }
}
