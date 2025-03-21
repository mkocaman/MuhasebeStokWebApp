using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MuhasebeStokWebApp.Data.Entities
{
    public class MenuRol
    {
        [Key]
        public Guid MenuRolID { get; set; }
        
        public Guid MenuID { get; set; }
        
        [ForeignKey("MenuID")]
        public virtual Menu Menu { get; set; }
        
        [Required, StringLength(450)]
        public string RolID { get; set; }
        
        public DateTime OlusturmaTarihi { get; set; } = DateTime.Now;
        
        public Guid? OlusturanKullaniciID { get; set; }
    }
} 