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
        
        // Görev durum seviyesi
        public MuhasebeStokWebApp.Enums.TaskStatus Status { get; set; } = MuhasebeStokWebApp.Enums.TaskStatus.Beklemede;
        
        // Görev etiketleri (virgülle ayrılmış)
        [MaxLength(200)]
        public string? Tags { get; set; }
        
        // Görev arşivlenmiş mi
        public bool IsArchived { get; set; } = false;
        
        // Soft delete için silindi durumu
        public bool IsDeleted { get; set; } = false;
        
        // Hatırlatıcı zamanı (opsiyonel)
        public DateTime? ReminderAt { get; set; }
        
        // Hatırlatma gönderildi mi
        public bool IsReminderSent { get; set; } = false;
        
        // Göreve yapılan yorumlar
        public virtual ICollection<TodoComment>? TodoComments { get; set; }
    }
} 