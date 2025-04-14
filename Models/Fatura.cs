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
        public Guid FaturaID { get; set; }
        
        [Required]
        [StringLength(50)]
        public string FaturaNumarasi { get; set; } = "";
        
        [Required]
        public DateTime FaturaTarihi { get; set; } = DateTime.Now;
        
        [Required]
        public Guid CariID { get; set; }
        
        [Required]
        public decimal AraToplam { get; set; }
        
        [Required]
        public decimal KDVToplam { get; set; }
        
        [Required]
        public decimal GenelToplam { get; set; }
        
        [Required]
        public DateTime VadeTarihi { get; set; } = DateTime.Now.AddDays(30);
        
        [Required]
        public bool Odendi { get; set; } = false;
        
        [StringLength(500)]
        public string Aciklama { get; set; } = "";
        
        [Required]
        public DateTime OlusturmaTarihi { get; set; } = DateTime.Now;
        
        public DateTime? GuncellemeTarihi { get; set; }
        
        public Guid? OlusturanKullaniciID { get; set; }
        
        public Guid? GuncelleyenKullaniciID { get; set; }
        
        public bool Silindi { get; set; } = false;
        
        [ForeignKey("CariID")]
        public virtual Cari Cari { get; set; }
        
        public virtual ICollection<FaturaDetay> FaturaDetaylari { get; set; }
        
        public Fatura()
        {
            FaturaID = Guid.NewGuid();
            FaturaTarihi = DateTime.Now;
            VadeTarihi = DateTime.Now.AddDays(30);
            Odendi = false;
            OlusturmaTarihi = DateTime.Now;
            Silindi = false;
            FaturaNumarasi = "";
            Aciklama = "";
            Cari = new Cari();
            FaturaDetaylari = new List<FaturaDetay>();
        }
    }
} 