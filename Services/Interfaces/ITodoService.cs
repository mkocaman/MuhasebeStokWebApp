using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using MuhasebeStokWebApp.Data.Entities;
using MuhasebeStokWebApp.ViewModels.Todo;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace MuhasebeStokWebApp.Services.Interfaces
{
    public interface ITodoService
    {
        // Görevleri listeleme işlemleri
        Task<List<TodoItemViewModel>> GetAllTodoItemsAsync(string? userId = null, bool includeDeleted = false);
        Task<List<TodoItemViewModel>> GetFilteredTodoItemsAsync(string filter, string? userId = null, bool isAdmin = false);
        Task<TodoItemViewModel> GetTodoItemByIdAsync(int id);
        Task<List<TodoItemViewModel>> GetRecentTodosAsync(int count, string? userId = null);
        
        // Görev ekleme, güncelleme ve silme işlemleri
        Task<int> CreateTodoItemAsync(TodoItemViewModel model);
        Task<bool> UpdateTodoItemAsync(TodoItemViewModel model);
        Task<bool> DeleteTodoItemAsync(int id);
        Task<bool> RestoreTodoItemAsync(int id);
        Task<bool> ToggleTodoCompletionAsync(int id);
        
        // Görev arşivleme işlemleri
        Task<bool> ArchiveTodoAsync(int id);
        Task<bool> UnarchiveTodoAsync(int id);
        Task<List<TodoItemViewModel>> GetArchivedTodoItemsAsync(string? userId = null, bool isAdmin = false);
        
        // Görev durum işlemleri
        Task<bool> UpdateTodoStatusAsync(int id, MuhasebeStokWebApp.Enums.TaskStatus status);
        
        // Görev etiket işlemleri
        Task<List<string>> GetAllTagsAsync();
        Task<List<TodoItemViewModel>> GetTodoItemsByTagAsync(string tag, string? userId = null, bool isAdmin = false);
        
        // Kullanıcı listesi çekme
        Task<List<SelectListItem>> GetUserSelectListAsync();
        
        // Yorum işlemleri
        Task<List<TodoCommentViewModel>> GetTodoCommentsAsync(int todoId);
        Task<int> AddCommentAsync(TodoCommentViewModel model);
        Task<bool> DeleteCommentAsync(int commentId, string userId);
        
        // Bildirim işlemleri
        Task<NotificationListViewModel> GetUserNotificationsAsync(string userId, bool onlyUnread = false);
        Task<bool> MarkNotificationAsReadAsync(int notificationId);
        Task<bool> MarkAllNotificationsAsReadAsync(string userId);
        Task<bool> DeleteNotificationAsync(int notificationId);
        Task<int> AddNotificationAsync(string userId, string content, string type, int? relatedEntityId = null);
        
        // Takvim entegrasyonu
        Task<List<TodoItemViewModel>> GetTodoItemsByDateAsync(DateTime date, string? userId = null, bool isAdmin = false);
        Task<List<CalendarEventViewModel>> GetCalendarEventsAsync(DateTime start, DateTime end, string? userId = null, bool isAdmin = false);
        
        // Hatırlatıcı işlemleri
        Task<List<TodoItemViewModel>> GetPendingRemindersAsync();
        Task<bool> MarkReminderAsSentAsync(int todoId);
    }
} 