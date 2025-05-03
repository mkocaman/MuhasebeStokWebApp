using System;
using System.Collections.Generic;

namespace MuhasebeStokWebApp.TempModels;

public partial class Sozlesmeler
{
    public Guid SozlesmeId { get; set; }

    public string SozlesmeNo { get; set; } = null!;

    public DateTime SozlesmeTarihi { get; set; }

    public DateTime? BitisTarihi { get; set; }

    public Guid CariId { get; set; }

    public bool VekaletGeldiMi { get; set; }

    public bool ResmiFaturaKesildiMi { get; set; }

    public string? SozlesmeDosyaYolu { get; set; }

    public string? VekaletnameDosyaYolu { get; set; }

    public decimal SozlesmeTutari { get; set; }

    public string? SozlesmeDovizTuru { get; set; }

    public string? Aciklama { get; set; }

    public bool AktifMi { get; set; }

    public DateTime OlusturmaTarihi { get; set; }

    public DateTime? GuncellemeTarihi { get; set; }

    public Guid? OlusturanKullaniciId { get; set; }

    public Guid? GuncelleyenKullaniciId { get; set; }

    public bool Silindi { get; set; }

    public Guid? AnaSozlesmeId { get; set; }

    public bool Aktif { get; set; }

    public virtual Cariler Cari { get; set; } = null!;

    public virtual ICollection<FaturaAklamaKuyrugu> FaturaAklamaKuyrugus { get; set; } = new List<FaturaAklamaKuyrugu>();

    public virtual ICollection<Faturalar> Faturalars { get; set; } = new List<Faturalar>();
}
