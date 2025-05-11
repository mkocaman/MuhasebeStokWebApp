using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MuhasebeStokWebApp.Data.Entities
{
    public class Notification
    {
        [Key]
        public int Id { get; set; }
        
        [Required]
        [ForeignKey("User")]
        public string UserId { get; set; }
        
        [Required]
        [MaxLength(500)]
        public string Content { get; set; }
        
        [Required]
        [MaxLength(50)]
        public string Type { get; set; }  // Görev, Yorum, Hatırlatma
        
        public int? RelatedEntityId { get; set; }  // İlgili görev veya yorum ID'si
        
        public DateTime CreatedAt { get; set; } = DateTime.Now;
        
        public bool IsRead { get; set; } = false;
        
        // Navigation property - Bildirim bir kullanıcıya aittir
        public virtual ApplicationUser User { get; set; }
    }
} 