using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MuhasebeStokWebApp.Data.Entities
{
    public class UrunBirim
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int BirimID { get; set; }

        [Required]
        [StringLength(50)]
        public string BirimAdi { get; set; }

        [StringLength(20)]
        public string BirimKodu { get; set; } = string.Empty;

        [StringLength(500)]
        public string? Aciklama { get; set; }

        public bool Aktif { get; set; } = true;

        public Guid? SirketID { get; set; }

        [StringLength(10)]
        public string BirimSembol { get; set; } = string.Empty;

        public Guid OlusturanKullaniciID { get; set; }

        public Guid? SonGuncelleyenKullaniciID { get; set; }

        public DateTime OlusturmaTarihi { get; set; } = DateTime.Now;

        public DateTime? GuncellemeTarihi { get; set; }

        public bool Silindi { get; set; } = false;

        // Navigation properties
        public virtual ICollection<Urun> Urunler { get; set; } = new HashSet<Urun>();
    }
} 