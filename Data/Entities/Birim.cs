using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace MuhasebeStokWebApp.Data.Entities
{
    public class Birim
    {
        [Key]
        public Guid BirimID { get; set; }
        
        [Required]
        [StringLength(50)]
        public string BirimAdi { get; set; }
        
        [StringLength(200)]
        public string Aciklama { get; set; }
        
        public bool Aktif { get; set; }
        
        public bool SoftDelete { get; set; } = false;
        
        // Navigation properties
        public virtual ICollection<Urun> Urunler { get; set; }
        public virtual ICollection<FaturaDetay> FaturaDetaylari { get; set; }
        public virtual ICollection<IrsaliyeDetay> IrsaliyeDetaylari { get; set; }
        
        public Birim()
        {
            Urunler = new HashSet<Urun>();
            FaturaDetaylari = new HashSet<FaturaDetay>();
            IrsaliyeDetaylari = new HashSet<IrsaliyeDetay>();
        }
    }
} 