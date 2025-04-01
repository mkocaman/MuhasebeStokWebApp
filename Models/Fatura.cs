using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using MuhasebeStokWebApp.Data.Entities;

namespace MuhasebeStokWebApp.Models
{
    public class Fatura
    {
        [Key]
        public Guid FaturaId { get; set; }

        public virtual Cari Cari { get; set; }

        [Required]
        public Guid CariId { get; set; }

        [Required]
        [StringLength(20)]
        public string FaturaNumarasi { get; set; }

        [Required]
        public DateTime FaturaTarihi { get; set; }

        [StringLength(500)]
        public string Aciklama { get; set; }

        public decimal ToplamTutar { get; set; }

        public bool Aktif { get; set; } = true;

        public DateTime OlusturmaTarihi { get; set; } = DateTime.Now;

        public Guid OlusturanKullaniciId { get; set; }

        public DateTime? GuncellemeTarihi { get; set; }

        public Guid? SonGuncelleyenKullaniciId { get; set; }

        public bool Silindi { get; set; }

        public virtual ICollection<FaturaDetay> FaturaDetaylari { get; set; }

        public Fatura()
        {
            FaturaDetaylari = new List<FaturaDetay>();
        }
    }
} 