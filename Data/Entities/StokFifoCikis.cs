using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MuhasebeStokWebApp.Data.Entities
{
    [Table("StokFifoCikislar")]
    public class StokFifoCikis
    {
        [Key]
        public Guid StokFifoCikisID { get; set; }
        
        public Guid? StokFifoID { get; set; }
        
        public Guid? ReferansID { get; set; }
        
        public Guid? DetayID { get; set; }
        
        public string? ReferansNo { get; set; }
        
        public string? ReferansTuru { get; set; }
        
        public decimal CikisMiktar { get; set; }
        
        public DateTime CikisTarihi { get; set; }
        
        public decimal BirimFiyatUSD { get; set; }
        
        public decimal BirimFiyatUZS { get; set; }
        
        public string? ParaBirimi { get; set; }
        
        public decimal? DovizKuru { get; set; }
        
        public DateTime OlusturmaTarihi { get; set; }
        
        public Guid? OlusturanKullaniciId { get; set; }
        
        public bool Aktif { get; set; }
        
        [ForeignKey("StokFifoID")]
        public virtual StokFifo? StokFifo { get; set; }
    }
} 