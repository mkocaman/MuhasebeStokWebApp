using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace MuhasebeStokWebApp.Data.Entities
{
    public class OdemeTuru
    {
        [Key]
        public int OdemeTuruID { get; set; }
        
        [Required]
        [StringLength(50)]
        public string OdemeTuruAdi { get; set; }
        
        // Navigation properties
        public virtual ICollection<Fatura> Faturalar { get; set; }
        
        public OdemeTuru()
        {
            Faturalar = new HashSet<Fatura>();
        }
    }
} 