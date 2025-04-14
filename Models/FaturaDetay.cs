using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using MuhasebeStokWebApp.Data.Entities;

namespace MuhasebeStokWebApp.Models
{
    public class FaturaDetay
    {
        [Key]
        public Guid ID { get; set; }

        [Required]
        public Guid FaturaID { get; set; }

        [Required]
        public Guid UrunID { get; set; }

        [Required]
        public decimal Miktar { get; set; }

        [Required]
        public decimal BirimFiyat { get; set; }

        public decimal? KDVOrani { get; set; }

        public decimal? IskontoOrani { get; set; }

        [StringLength(500)]
        public string Aciklama { get; set; } = "";

        [Required]
        public DateTime OlusturmaTarihi { get; set; } = DateTime.Now;

        public DateTime? GuncellemeTarihi { get; set; }

        public Guid? OlusturanKullaniciID { get; set; }

        public Guid? GuncelleyenKullaniciID { get; set; }

        public bool Silindi { get; set; } = false;

        [ForeignKey("FaturaID")]
        public virtual Fatura Fatura { get; set; }

        [ForeignKey("UrunID")]
        public virtual Urun Urun { get; set; }

        public FaturaDetay()
        {
            ID = Guid.NewGuid();
            OlusturmaTarihi = DateTime.Now;
            Silindi = false;
            Aciklama = "";
            Fatura = new Fatura();
            Urun = new Urun();
        }
    }
} 