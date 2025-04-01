#nullable enable

using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.AspNetCore.Identity;

namespace MuhasebeStokWebApp.Data.Entities
{
    [Table("AspNetUsers")]
    public class ApplicationUser : IdentityUser
    {
        public string? Ad { get; set; }
        public string? Soyad { get; set; }
        
        public string? TelefonNo { get; set; }
        
        [MaxLength(200)]
        public string? Adres { get; set; }
        
        public bool Aktif { get; set; } = true;
        
        public bool Silindi { get; set; } = false;
        
        public DateTime OlusturmaTarihi { get; set; } = DateTime.Now;
        public DateTime? GuncellemeTarihi { get; set; }
        
        // Navigation properties
        public virtual ICollection<SistemLog>? SistemLoglar { get; set; }
    }
} 