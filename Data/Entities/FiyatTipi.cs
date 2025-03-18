using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace MuhasebeStokWebApp.Data.Entities
{
    public class FiyatTipi
    {
        [Key]
        public int FiyatTipiID { get; set; }
        
        [Required]
        [StringLength(50)]
        public string TipAdi { get; set; }
        
        // Navigation properties
        public virtual ICollection<UrunFiyat> UrunFiyatlari { get; set; }
        
        public FiyatTipi()
        {
            UrunFiyatlari = new HashSet<UrunFiyat>();
        }
    }
} 