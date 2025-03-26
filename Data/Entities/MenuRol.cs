using System;
using System.ComponentModel.DataAnnotations;

namespace MuhasebeStokWebApp.Data.Entities
{
    public class MenuRol
    {
        [Key]
        public Guid MenuRolID { get; set; }
        
        public Guid MenuId { get; set; }
        
        public string RolId { get; set; }
        
        public virtual Menu Menu { get; set; }
        
        public virtual Microsoft.AspNetCore.Identity.IdentityRole Rol { get; set; }
        
        public DateTime OlusturmaTarihi { get; set; } = DateTime.Now;
    }
} 