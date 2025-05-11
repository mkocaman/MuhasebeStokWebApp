using System.Threading.Tasks;
using System.Collections.Generic;
using MuhasebeStokWebApp.ViewModels.Todo;

namespace MuhasebeStokWebApp.Services.Interfaces
{
    public interface INotificationService
    {
        // Bildirim ekleme işlemleri
        Task<int> AddNotificationAsync(string userId, string content, string type, int? relatedEntityId = null);
        Task<int> AddTaskNotificationAsync(string userId, int todoId, string content);
        Task<int> AddCommentNotificationAsync(string userId, int todoId, int commentId, string content);
        Task<int> AddReminderNotificationAsync(string userId, int todoId, string content);
        
        // Bildirim listeleme
        Task<NotificationListViewModel> GetUserNotificationsAsync(string userId, bool onlyUnread = false);
        Task<List<NotificationViewModel>> GetAllNotificationsAsync();
        
        // Bildirim işaretleme ve silme
        Task<bool> MarkAsReadAsync(int id);
        Task<bool> MarkAllAsReadAsync(string userId);
        Task<bool> DeleteNotificationAsync(int id);
        Task<bool> DeleteAllNotificationsAsync(string userId);
        
        // Kritik hata bildirimi
        Task<int> SendCriticalNotificationAsync(string title, string content);
        
        // Bildirim gönderme
        Task SendNotificationToUserAsync(string userId, string content, string type, int? relatedEntityId = null);
        Task SendNotificationToBrowserAsync(string userId, string title, string message, string url = null);
    }
} 