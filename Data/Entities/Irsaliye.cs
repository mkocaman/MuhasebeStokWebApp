using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MuhasebeStokWebApp.Data.Entities
{
    public class Irsaliye : BaseEntity, ISoftDelete
    {
        public Irsaliye()
        {
            IrsaliyeDetaylari = new List<IrsaliyeDetay>();
            IrsaliyeID = Guid.NewGuid();
            Aktif = true;
            Silindi = false;
        }

        [Key]
        public Guid IrsaliyeID { get; set; }
        
        [Required]
        [StringLength(20)]
        public string IrsaliyeNumarasi { get; set; }
        
        [Required]
        [Display(Name = "İrsaliye Tarihi")]
        public DateTime IrsaliyeTarihi { get; set; }
        
        [Required]
        public Guid CariID { get; set; }
        
        public Guid? FaturaID { get; set; }
        
        public Guid? DepoID { get; set; }
        
        [StringLength(500)]
        public string Aciklama { get; set; }
        
        [Required]
        public string IrsaliyeTuru { get; set; }
        
        public bool Aktif { get; set; }
        
        public Guid OlusturanKullaniciId { get; set; }
        
        public Guid? SonGuncelleyenKullaniciId { get; set; }
        
        public bool Silindi { get; set; }
        
        [StringLength(20)]
        public string Durum { get; set; } = "Açık"; // Varsayılan durum: Açık
        
        // Navigation properties
        [ForeignKey("CariID")]
        public virtual Cari Cari { get; set; }
        
        [ForeignKey("FaturaID")]
        public virtual Fatura Fatura { get; set; }
        
        [ForeignKey("DepoID")]
        public virtual Depo Depo { get; set; }
        
        public virtual ICollection<IrsaliyeDetay> IrsaliyeDetaylari { get; set; }
    }
} 