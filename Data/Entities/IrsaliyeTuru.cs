using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace MuhasebeStokWebApp.Data.Entities
{
    public class IrsaliyeTuru
    {
        [Key]
        public int IrsaliyeTuruID { get; set; }
        
        [Required]
        [StringLength(50)]
        public string IrsaliyeTuruAdi { get; set; }
        
        [StringLength(50)]
        public string HareketTuru { get; set; }
        
        // Navigation properties
        public virtual ICollection<Irsaliye> Irsaliyeler { get; set; }
        
        public IrsaliyeTuru()
        {
            Irsaliyeler = new HashSet<Irsaliye>();
        }
    }
} 