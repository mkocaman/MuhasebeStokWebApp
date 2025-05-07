using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MuhasebeStokWebApp.Data.Entities
{
    public class TodoComment
    {
        [Key]
        public int Id { get; set; }
        
        [Required]
        public int TodoItemId { get; set; }
        
        [Required]
        [ForeignKey("AppUser")]
        public string AppUserId { get; set; }
        
        [Required]
        [MaxLength(1000)]
        public string Content { get; set; }
        
        public DateTime CreatedAt { get; set; } = DateTime.Now;
        
        // Navigation properties
        [ForeignKey("TodoItemId")]
        public virtual TodoItem TodoItem { get; set; }
        
        [ForeignKey("AppUserId")]
        public virtual ApplicationUser AppUser { get; set; }
    }
} 