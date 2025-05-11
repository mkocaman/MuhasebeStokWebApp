using System;
using System.ComponentModel.DataAnnotations;
using System.Collections.Generic;

namespace MuhasebeStokWebApp.ViewModels.Todo
{
    public class NotificationViewModel
    {
        public int Id { get; set; }
        
        public string UserId { get; set; }
        
        [Display(Name = "İçerik")]
        public string Content { get; set; }
        
        [Display(Name = "Tür")]
        public string Type { get; set; }
        
        public int? RelatedEntityId { get; set; }
        
        [Display(Name = "Oluşturma Tarihi")]
        public DateTime CreatedAt { get; set; }
        
        [Display(Name = "Okundu")]
        public bool IsRead { get; set; }
        
        public string Url { get; set; }
        
        // İlgili görev ID'si varsa bu değeri döndürür, yoksa null
        public int? TodoItemId => Type == "Görev" || Type == "Yorum" ? RelatedEntityId : null;
    }
    
    public class NotificationListViewModel
    {
        public List<NotificationViewModel> Notifications { get; set; } = new List<NotificationViewModel>();
        public int TotalCount { get; set; }
        public int UnreadCount { get; set; }
        public bool ShowUnreadOnly { get; set; } = false;
    }
} 