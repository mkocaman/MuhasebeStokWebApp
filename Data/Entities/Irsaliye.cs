using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MuhasebeStokWebApp.Data.Entities
{
    public class Irsaliye
    {
        [Key]
        public Guid IrsaliyeID { get; set; }
        
        public Guid? FaturaID { get; set; }
        
        [Required]
        public DateTime IrsaliyeTarihi { get; set; }
        
        [Required]
        public DateTime SevkTarihi { get; set; }
        
        [StringLength(200)]
        public string Aciklama { get; set; }
        
        public Guid? OlusturanKullaniciID { get; set; }
        
        public Guid? SonGuncelleyenKullaniciID { get; set; }
        
        public DateTime OlusturmaTarihi { get; set; }
        
        public DateTime? GuncellemeTarihi { get; set; }
        
        public bool SoftDelete { get; set; }
        
        public bool? Resmi { get; set; }
        
        [StringLength(50)]
        public string IrsaliyeNumarasi { get; set; }
        
        public Guid CariID { get; set; }
        
        [StringLength(50)]
        public string IrsaliyeTuru { get; set; }
        
        [StringLength(50)]
        public string Durum { get; set; } = "Açık";
        
        public decimal? GenelToplam { get; set; }
        
        // Navigation properties
        [ForeignKey("FaturaID")]
        public virtual Fatura Fatura { get; set; }
        
        [ForeignKey("CariID")]
        public virtual Cari Cari { get; set; }
        
        public virtual ICollection<IrsaliyeDetay> IrsaliyeDetaylari { get; set; }
        
        public Irsaliye()
        {
            IrsaliyeDetaylari = new HashSet<IrsaliyeDetay>();
            OlusturmaTarihi = DateTime.Now;
        }
    }
} 