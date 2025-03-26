using Microsoft.AspNetCore.SignalR;
using MuhasebeStokWebApp.Hubs;
using Serilog;
using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using MuhasebeStokWebApp.Services.Email;

namespace MuhasebeStokWebApp.Services.Notification
{
    public interface INotificationService
    {
        Task SendNotificationAsync(string userId, string title, string message, string type = "info");
        Task SendNotificationToAllAsync(string title, string message, string type = "info");
        Task SendNotificationToUserAsync(string userId, string message, string type = "info");
        Task SendNotificationToGroupAsync(string groupName, string message, string type = "info");
        Task SendEmailNotificationAsync(string to, string subject, string message);
        Task SendEmailNotificationWithAttachmentAsync(string to, string subject, string content, string attachmentPath);
        Task SendCriticalNotificationAsync(string title, string message);
    }

    public class NotificationService : INotificationService
    {
        private readonly IHubContext<NotificationHub> _hubContext;
        private readonly IEmailService _emailService;
        private readonly ILogger<NotificationService> _logger;

        public NotificationService(IHubContext<NotificationHub> hubContext, IEmailService emailService, ILogger<NotificationService> logger)
        {
            _hubContext = hubContext;
            _emailService = emailService;
            _logger = logger;
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

        public async Task SendCriticalNotificationAsync(string title, string message)
        {
            try
            {
                // Tüm kullanıcılara bildirim gönder
                await SendNotificationToAllAsync(title, message, "danger");

                // Admin e-postalarına bildirim gönder
                await _emailService.SendCriticalNotificationAsync(title, message);

                _logger.LogWarning("Kritik bildirim gönderildi: {Title}", title);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Kritik bildirim gönderilirken hata oluştu");
                throw;
            }
        }
    }
} 