using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace MuhasebeStokWebApp.Data.Entities
{
    public class UrunBirim
    {
        public int BirimID { get; set; }

        [Required]
        [StringLength(50)]
        public string BirimAdi { get; set; }

        [StringLength(10)]
        public string BirimKodu { get; set; }

        [StringLength(500)]
        public string Aciklama { get; set; }

        public bool Aktif { get; set; } = true;

        public int OlusturanKullaniciID { get; set; }

        public int? SonGuncelleyenKullaniciID { get; set; }

        public DateTime OlusturmaTarihi { get; set; } = DateTime.Now;

        public DateTime? GuncellemeTarihi { get; set; }

        public bool Silindi { get; set; }

        // Navigation properties
        public virtual ICollection<Urun> Urunler { get; set; }

        public UrunBirim()
        {
            Urunler = new HashSet<Urun>();
        }
    }
} 