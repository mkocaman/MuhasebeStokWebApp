using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using MuhasebeStokWebApp.Data.Entities;

namespace MuhasebeStokWebApp.Models
{
    public class FaturaDetay
    {
        [Key]
        public Guid FaturaDetayId { get; set; }

        [Required]
        public Guid FaturaId { get; set; }

        [Required]
        public Guid UrunId { get; set; }

        [Required]
        public decimal Miktar { get; set; }

        [Required]
        public decimal BirimFiyat { get; set; }

        public decimal KdvOrani { get; set; }

        public decimal IndirimOrani { get; set; }

        [StringLength(500)]
        public string Aciklama { get; set; }

        public bool Aktif { get; set; } = true;

        public DateTime OlusturmaTarihi { get; set; } = DateTime.Now;

        public Guid OlusturanKullaniciId { get; set; }

        public DateTime? GuncellemeTarihi { get; set; }

        public Guid? SonGuncelleyenKullaniciId { get; set; }

        public bool Silindi { get; set; }

        [ForeignKey("FaturaId")]
        public virtual Fatura Fatura { get; set; }

        [ForeignKey("UrunId")]
        public virtual Urun Urun { get; set; }
    }
} 