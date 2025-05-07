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
        
        // Profil bilgileri için ekstra özellikler
        [MaxLength(100)]
        public string? FullName { get; set; }
        
        [MaxLength(500)]
        public string? Bio { get; set; }
        
        [MaxLength(200)]
        public string? ProfileImage { get; set; }
        
        // Calculated Property for FullName
        [NotMapped]
        public string FullNameCalculated
        {
            get
            {
                if (!string.IsNullOrEmpty(FullName))
                    return FullName;
                
                if (!string.IsNullOrEmpty(Ad) && !string.IsNullOrEmpty(Soyad))
                    return $"{Ad} {Soyad}";
                    
                return UserName ?? "";
            }
        }
        
        // Navigation Properties
        public virtual ICollection<SistemLog>? SistemLoglar { get; set; }
        
        // Kullanıcıya atanan görevler
        public virtual ICollection<TodoItem>? AssignedTodos { get; set; }
        
        // Kullanıcının yaptığı yorumlar
        public virtual ICollection<TodoComment>? TodoComments { get; set; }
    }
} 