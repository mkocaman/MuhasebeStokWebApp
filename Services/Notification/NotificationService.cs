using Microsoft.AspNetCore.SignalR;
using MuhasebeStokWebApp.Hubs;
using Serilog;
using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using MuhasebeStokWebApp.Services.Email;
using System.Collections.Generic;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using MuhasebeStokWebApp.Data;
using MuhasebeStokWebApp.Data.Entities;
using MuhasebeStokWebApp.Services.Interfaces;
using MuhasebeStokWebApp.ViewModels.Todo;
using Microsoft.AspNetCore.Identity;

namespace MuhasebeStokWebApp.Services.Notification
{
    public class NotificationService : INotificationService
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<NotificationService> _logger;
        private readonly IHubContext<NotificationHub> _hubContext;
        private readonly MuhasebeStokWebApp.Services.Email.IEmailService _emailService;
        private readonly UserManager<ApplicationUser> _userManager;

        public NotificationService(
            ApplicationDbContext context, 
            ILogger<NotificationService> logger, 
            IHubContext<NotificationHub> hubContext, 
            MuhasebeStokWebApp.Services.Email.IEmailService emailService,
            UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _logger = logger;
            _hubContext = hubContext;
            _emailService = emailService;
            _userManager = userManager;
        }

        public async Task SendNotificationAsync(string userId, string title, string message, string type = "info")
        {
            try
            {
                await _hubContext.Clients.User(userId).SendAsync("ReceiveNotification", new
                {
                    title = title,
                    message = message,
                    type = type,
                    timestamp = DateTime.Now
                });

                _logger.LogInformation("Bildirim gönderildi: {UserId} - {Title}", userId, title);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Bildirim gönderilirken hata oluştu: {UserId}", userId);
                throw;
            }
        }

        public async Task SendNotificationToAllAsync(string title, string message, string type = "info")
        {
            try
            {
                await _hubContext.Clients.All.SendAsync("ReceiveNotification", new
                {
                    title = title,
                    message = message,
                    type = type,
                    timestamp = DateTime.Now
                });

                _logger.LogInformation("Toplu bildirim gönderildi: {Title}", title);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Toplu bildirim gönderilirken hata oluştu");
                throw;
            }
        }

        public async Task SendNotificationToUserAsync(string userId, string message, string type = "info")
        {
            await _hubContext.Clients.User(userId).SendAsync("ReceiveNotification", message, type);
        }

        public async Task SendNotificationToGroupAsync(string groupName, string message, string type = "info")
        {
            await _hubContext.Clients.Group(groupName).SendAsync("ReceiveNotification", message, type);
        }

        public async Task SendEmailNotificationAsync(string to, string subject, string message)
        {
            try
            {
                await _emailService.SendEmailAsync(to, subject, message, true);
                _logger.LogInformation($"E-posta bildirimi gönderildi - Alıcı: {to}, Konu: {subject}");
            }
            catch (Exception ex)
            {
                _logger.LogError($"E-posta bildirimi gönderilirken hata oluştu: {ex.Message}");
                throw;
            }
        }

        public async Task SendEmailNotificationWithAttachmentAsync(string to, string subject, string content, string attachmentPath)
        {
            await _emailService.SendEmailWithAttachmentAsync(to, subject, content, attachmentPath);
        }

        /// <summary>
        /// Yeni bildirim ekler
        /// </summary>
        public async Task<int> AddNotificationAsync(string userId, string content, string type, int? relatedEntityId = null)
        {
            try
            {
                var notification = new Data.Entities.Notification
                {
                    UserId = userId,
                    Content = content,
                    Type = type,
                    RelatedEntityId = relatedEntityId,
                    CreatedAt = DateTime.Now,
                    IsRead = false
                };
                
                _context.Notifications.Add(notification);
                await _context.SaveChangesAsync();
                
                return notification.Id;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Bildirim eklenirken hata oluştu. UserId: {userId}, Type: {type}");
                return 0;
            }
        }
        
        /// <summary>
        /// Yeni görev bildirimi ekler
        /// </summary>
        public async Task<int> AddTaskNotificationAsync(string userId, int todoId, string content)
        {
            return await AddNotificationAsync(userId, content, "Görev", todoId);
        }
        
        /// <summary>
        /// Yeni yorum bildirimi ekler
        /// </summary>
        public async Task<int> AddCommentNotificationAsync(string userId, int todoId, int commentId, string content)
        {
            return await AddNotificationAsync(userId, content, "Yorum", todoId);
        }
        
        /// <summary>
        /// Yeni hatırlatma bildirimi ekler
        /// </summary>
        public async Task<int> AddReminderNotificationAsync(string userId, int todoId, string content)
        {
            return await AddNotificationAsync(userId, content, "Hatırlatma", todoId);
        }
        
        /// <summary>
        /// Kullanıcının bildirimlerini getirir
        /// </summary>
        public async Task<NotificationListViewModel> GetUserNotificationsAsync(string userId, bool onlyUnread = false)
        {
            try
            {
                var query = _context.Notifications
                    .Where(n => n.UserId == userId);
                
                if (onlyUnread)
                {
                    query = query.Where(n => !n.IsRead);
                }
                
                var notifications = await query
                    .OrderByDescending(n => n.CreatedAt)
                    .ToListAsync();
                
                var unreadCount = await _context.Notifications
                    .Where(n => n.UserId == userId && !n.IsRead)
                    .CountAsync();
                
                return new NotificationListViewModel
                {
                    Notifications = notifications.Select(n => new NotificationViewModel
                    {
                        Id = n.Id,
                        UserId = n.UserId,
                        Content = n.Content,
                        Type = n.Type,
                        RelatedEntityId = n.RelatedEntityId,
                        CreatedAt = n.CreatedAt,
                        IsRead = n.IsRead
                    }).ToList(),
                    UnreadCount = unreadCount
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Kullanıcı bildirimleri getirilirken hata oluştu. UserId: {userId}");
                return new NotificationListViewModel();
            }
        }
        
        /// <summary>
        /// Tüm bildirimleri getirir
        /// </summary>
        public async Task<List<NotificationViewModel>> GetAllNotificationsAsync()
        {
            try
            {
                var notifications = await _context.Notifications
                    .OrderByDescending(n => n.CreatedAt)
                    .ToListAsync();
                
                return notifications.Select(n => new NotificationViewModel
                {
                    Id = n.Id,
                    UserId = n.UserId,
                    Content = n.Content,
                    Type = n.Type,
                    RelatedEntityId = n.RelatedEntityId,
                    CreatedAt = n.CreatedAt,
                    IsRead = n.IsRead
                }).ToList();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Tüm bildirimler getirilirken hata oluştu");
                return new List<NotificationViewModel>();
            }
        }
        
        /// <summary>
        /// Bildirimi okundu olarak işaretler
        /// </summary>
        public async Task<bool> MarkAsReadAsync(int id)
        {
            try
            {
                var notification = await _context.Notifications.FindAsync(id);
                if (notification == null)
                {
                    return false;
                }
                
                notification.IsRead = true;
                _context.Notifications.Update(notification);
                await _context.SaveChangesAsync();
                
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Bildirim okundu olarak işaretlenirken hata oluştu. ID: {id}");
                return false;
            }
        }
        
        /// <summary>
        /// Kullanıcının tüm bildirimlerini okundu olarak işaretler
        /// </summary>
        public async Task<bool> MarkAllAsReadAsync(string userId)
        {
            try
            {
                var notifications = await _context.Notifications
                    .Where(n => n.UserId == userId && !n.IsRead)
                    .ToListAsync();
                
                foreach (var notification in notifications)
                {
                    notification.IsRead = true;
                    _context.Notifications.Update(notification);
                }
                
                await _context.SaveChangesAsync();
                
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Tüm bildirimler okundu olarak işaretlenirken hata oluştu. UserId: {userId}");
                return false;
            }
        }
        
        /// <summary>
        /// Bildirimi siler
        /// </summary>
        public async Task<bool> DeleteNotificationAsync(int id)
        {
            try
            {
                var notification = await _context.Notifications.FindAsync(id);
                if (notification == null)
                {
                    return false;
                }
                
                _context.Notifications.Remove(notification);
                await _context.SaveChangesAsync();
                
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Bildirim silinirken hata oluştu. ID: {id}");
                return false;
            }
        }
        
        /// <summary>
        /// Kullanıcının tüm bildirimlerini siler
        /// </summary>
        public async Task<bool> DeleteAllNotificationsAsync(string userId)
        {
            try
            {
                var notifications = await _context.Notifications
                    .Where(n => n.UserId == userId)
                    .ToListAsync();
                
                _context.Notifications.RemoveRange(notifications);
                await _context.SaveChangesAsync();
                
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Tüm bildirimler silinirken hata oluştu. UserId: {userId}");
                return false;
            }
        }
        
        /// <summary>
        /// Kullanıcıya bildirim gönderir (SignalR ile)
        /// </summary>
        public async Task SendNotificationToUserAsync(string userId, string content, string type, int? relatedEntityId = null)
        {
            // Önce bildirim veritabanına eklenir
            await AddNotificationAsync(userId, content, type, relatedEntityId);
            
            // TODO: Burada SignalR ile bildirim gönderme kodu eklenecek
        }
        
        /// <summary>
        /// Tarayıcı bildirimi gönderir
        /// </summary>
        public async Task SendNotificationToBrowserAsync(string userId, string title, string message, string url = null)
        {
            // Burada SignalR ile tarayıcı bildirimi gönderme kodu eklenecek
            await Task.CompletedTask;
        }

        /// <summary>
        /// Sistemdeki tüm admin kullanıcılara kritik bildirim gönderir
        /// </summary>
        public async Task<int> SendCriticalNotificationAsync(string title, string content)
        {
            try
            {
                // Admin rolündeki tüm kullanıcıları bul
                var adminUsers = await _userManager.GetUsersInRoleAsync("Admin");
                int notificationCount = 0;

                // Tüm admin kullanıcılara bildirim gönder
                foreach (var admin in adminUsers)
                {
                    var notification = new Data.Entities.Notification
                    {
                        UserId = admin.Id,
                        Content = $"[KRİTİK] {title}: {content}",
                        Type = "Sistem",
                        CreatedAt = DateTime.Now,
                        IsRead = false
                    };

                    _context.Notifications.Add(notification);
                    notificationCount++;
                }
                
                // Tüm kullanıcılara ayrıca bildirim gönder
                await SendNotificationToAllAsync(title, content, "danger");
                
                // Değişiklikleri kaydet
                await _context.SaveChangesAsync();
                return notificationCount;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Kritik bildirim gönderilirken hata oluştu: {Title}, {Content}", title, content);
                return 0;
            }
        }
    }
} 