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
        public string FilterOption { get; set; } = "all"; // all, completed, pending
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
            
            // Deadline geçmiş bir tarih olamaz
            if (Deadline.HasValue && Deadline < DateTime.Now.Date)
                return false;
            
            return true;
        }
        
        public string GetValidationMessage()
        {
            if (string.IsNullOrWhiteSpace(Title))
                return "Başlık alanı zorunludur.";
                
            if (Deadline.HasValue && Deadline.Value.Date < DateTime.Now.Date)
                return "Son tarih geçmiş bir tarih olamaz.";
                
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