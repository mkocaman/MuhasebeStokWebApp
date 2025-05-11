using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.Rendering;
using MuhasebeStokWebApp.Data.Entities;
using MuhasebeStokWebApp.Enums;

namespace MuhasebeStokWebApp.ViewModels.Todo
{
    public class TodoViewModel
    {
        // Liste görünümü için gerekli özellikler
        public List<TodoItemViewModel> TodoItems { get; set; } = new List<TodoItemViewModel>();
        public TodoItemViewModel NewTodo { get; set; } = new TodoItemViewModel();
        public List<SelectListItem> Users { get; set; } = new List<SelectListItem>();
        public string FilterOption { get; set; } = "all"; // all, completed, pending, deleted, archived
        public bool IsAdminView { get; set; } = false;
        
        // Takvim görünümü için gerekli özellikler
        public DateTime SelectedDate { get; set; } = DateTime.Today;
        public List<CalendarEventViewModel> CalendarEvents { get; set; } = new List<CalendarEventViewModel>();
    }

    public class TodoItemViewModel
    {
        public int Id { get; set; }
        
        [Required(ErrorMessage = "Başlık alanı zorunludur.")]
        [MaxLength(200, ErrorMessage = "Başlık en fazla 200 karakter olabilir.")]
        [Display(Name = "Başlık")]
        public string Title { get; set; }
        
        [MaxLength(1000, ErrorMessage = "Açıklama en fazla 1000 karakter olabilir.")]
        [Display(Name = "Açıklama")]
        public string? Description { get; set; }
        
        [Display(Name = "Oluşturma Tarihi")]
        public DateTime CreatedAt { get; set; } = DateTime.Now;
        
        [Display(Name = "Son Tarih")]
        [DataType(DataType.Date)]
        public DateTime? Deadline { get; set; }
        
        [Display(Name = "Tamamlandı")]
        public bool IsCompleted { get; set; } = false;
        
        [Display(Name = "Silinmiş")]
        public bool IsDeleted { get; set; } = false;
        
        [Display(Name = "Oluşturan Kullanıcı")]
        public string? UserId { get; set; }
        
        [Display(Name = "Atanan Kullanıcı")]
        public string? AssignedToUserId { get; set; }
        
        [Display(Name = "Atanan Kullanıcı")]
        public string? AssignedToUserName { get; set; }
        
        [Display(Name = "Kategori")]
        [MaxLength(50)]
        public string? TaskCategory { get; set; }
        
        [Display(Name = "İkon")]
        public string TaskIcon { get; set; } = "fa fa-briefcase";
        
        [Display(Name = "Öncelik")]
        [Required(ErrorMessage = "Öncelik seviyesi zorunludur.")]
        public PriorityLevel PriorityLevel { get; set; } = PriorityLevel.Medium;
        
        [Display(Name = "Durum")]
        [Required(ErrorMessage = "Durum alanı zorunludur.")]
        public MuhasebeStokWebApp.Enums.TaskStatus Status { get; set; } = MuhasebeStokWebApp.Enums.TaskStatus.Beklemede;
        
        [Display(Name = "Etiketler")]
        [MaxLength(200, ErrorMessage = "Etiketler en fazla 200 karakter olabilir.")]
        public string? Tags { get; set; }
        
        [Display(Name = "Arşivlenmiş")]
        public bool IsArchived { get; set; } = false;
        
        [Display(Name = "Hatırlatma Zamanı")]
        [DataType(DataType.DateTime)]
        public DateTime? ReminderAt { get; set; }
        
        [Display(Name = "Hatırlatma Ekle")]
        public bool UseReminder { get; set; } = false;
        
        [Display(Name = "Hatırlatma Gönderildi")]
        public bool IsReminderSent { get; set; } = false;
        
        // Yorumlar için liste
        public List<TodoCommentViewModel>? Comments { get; set; }
        
        /// <summary>
        /// Model doğrulama için temel kontroller
        /// </summary>
        public bool IsValid()
        {
            // Başlık zorunlu
            if (string.IsNullOrWhiteSpace(Title))
                return false;
            
            // Deadline geçmiş bir tarih olamaz (bugün olabilir)
            if (Deadline.HasValue && Deadline.Value.Date < DateTime.Now.Date)
                return false;
            
            // Hatırlatma zamanı geçmiş bir zaman olamaz, eğer kullanılıyorsa
            if (UseReminder && ReminderAt.HasValue)
            {
                // Şimdiki zamandan biraz önce olabileceği için 1 dakika tolerans verilebilir
                if (ReminderAt.Value < DateTime.Now.AddMinutes(-1))
                    return false;
            }
            
            return true;
        }
        
        public string GetValidationMessage()
        {
            if (string.IsNullOrWhiteSpace(Title))
                return "Başlık alanı zorunludur.";
                
            if (Deadline.HasValue && Deadline.Value.Date < DateTime.Now.Date)
                return "Son tarih geçmiş bir tarih olamaz.";
            
            if (UseReminder && ReminderAt.HasValue && ReminderAt.Value < DateTime.Now.AddMinutes(-1))
                return "Hatırlatma zamanı geçmiş bir zaman olamaz.";
                
            return "";
        }
    }
    
    public class TodoCommentViewModel
    {
        public int Id { get; set; }
        
        public int TodoItemId { get; set; }
        
        public string AppUserId { get; set; }
        
        [Required(ErrorMessage = "Yorum içeriği zorunludur.")]
        [MaxLength(1000, ErrorMessage = "Yorum en fazla 1000 karakter olabilir.")]
        [Display(Name = "Yorum")]
        public string Content { get; set; }
        
        [Display(Name = "Oluşturma Tarihi")]
        public DateTime CreatedAt { get; set; } = DateTime.Now;
        
        [Display(Name = "Kullanıcı")]
        public string UserName { get; set; }
        
        [Display(Name = "Profil Resmi")]
        public string? UserProfileImage { get; set; }
    }
} 