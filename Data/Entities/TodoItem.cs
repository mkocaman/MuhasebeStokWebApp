using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using MuhasebeStokWebApp.Enums;

namespace MuhasebeStokWebApp.Data.Entities
{
    public class TodoItem
    {
        [Key]
        public int Id { get; set; }
        
        [Required]
        [MaxLength(200)]
        public string Title { get; set; }
        
        [MaxLength(1000)]
        public string? Description { get; set; }
        
        public DateTime CreatedAt { get; set; } = DateTime.Now;
        
        public DateTime? Deadline { get; set; }
        
        public bool IsCompleted { get; set; } = false;
        
        [ForeignKey("AssignedToUser")]
        public string? AssignedToUserId { get; set; }
        
        // Navigation property - Bir görev bir kullanıcıya atanabilir
        public virtual ApplicationUser? AssignedToUser { get; set; }
        
        // Görevin kategorisi
        [MaxLength(50)]
        public string? TaskCategory { get; set; }
        
        // Görev öncelik seviyesi
        public PriorityLevel PriorityLevel { get; set; } = PriorityLevel.Medium;
        
        // Göreve yapılan yorumlar
        public virtual ICollection<TodoComment>? TodoComments { get; set; }
    }
} 