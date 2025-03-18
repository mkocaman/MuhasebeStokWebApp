using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace MuhasebeStokWebApp.Data.Entities
{
    public class FaturaTuru
    {
        [Key]
        public int FaturaTuruID { get; set; }
        
        [Required]
        [StringLength(50)]
        public string FaturaTuruAdi { get; set; }
        
        [StringLength(50)]
        public string HareketTuru { get; set; }
        
        // Navigation properties
        public virtual ICollection<Fatura> Faturalar { get; set; }
        
        public FaturaTuru()
        {
            Faturalar = new HashSet<Fatura>();
        }
    }
} 