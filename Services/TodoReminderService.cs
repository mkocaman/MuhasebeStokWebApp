using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using MuhasebeStokWebApp.Services.Interfaces;
using MuhasebeStokWebApp.Hubs;
using Microsoft.AspNetCore.SignalR;

namespace MuhasebeStokWebApp.Services
{
    public class TodoReminderService : BackgroundService
    {
        private readonly ILogger<TodoReminderService> _logger;
        private readonly IServiceProvider _serviceProvider;
        private readonly IHubContext<NotificationHub> _hubContext;
        
        public TodoReminderService(
            ILogger<TodoReminderService> logger,
            IServiceProvider serviceProvider,
            IHubContext<NotificationHub> hubContext)
        {
            _logger = logger;
            _serviceProvider = serviceProvider;
            _hubContext = hubContext;
        }
        
        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("Todo Hatırlatıcı Servisi başlatıldı.");
            
            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    await CheckReminders();
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Hatırlatıcılar kontrol edilirken bir hata oluştu.");
                }
                
                // 5 dakika bekle
                await Task.Delay(TimeSpan.FromMinutes(5), stoppingToken);
            }
        }
        
        private async Task CheckReminders()
        {
            _logger.LogInformation("Yaklaşan hatırlatıcılar kontrol ediliyor...");
            
            using (var scope = _serviceProvider.CreateScope())
            {
                var todoService = scope.ServiceProvider.GetRequiredService<ITodoService>();
                
                try
                {
                    // Gönderilmemiş ve zamanı gelmiş hatırlatmaları getir
                    var pendingReminders = await todoService.GetPendingRemindersAsync();
                    
                    if (pendingReminders != null && pendingReminders.Any())
                    {
                        _logger.LogInformation($"{pendingReminders.Count} adet bekleyen hatırlatma bulundu.");
                        
                        foreach (var reminder in pendingReminders)
                        {
                            try
                            {
                                if (reminder.ReminderAt.HasValue && !reminder.IsReminderSent)
                                {
                                    // Kullanıcıya bildirim gönder
                                    await SendNotificationToUser(reminder);
                                    
                                    // Hatırlatmayı gönderildi olarak işaretle
                                    await todoService.MarkReminderAsSentAsync(reminder.Id);
                                    _logger.LogInformation($"ID: {reminder.Id} olan görev için hatırlatma gönderildi.");
                                }
                            }
                            catch (Exception ex)
                            {
                                _logger.LogError(ex, $"ID: {reminder.Id} olan görev için hatırlatma gönderilirken hata oluştu.");
                            }
                        }
                    }
                    else
                    {
                        _logger.LogInformation("Bekleyen hatırlatma bulunamadı.");
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Hatırlatıcı kontrolü sırasında beklenmeyen bir hata oluştu.");
                }
            }
        }
        
        private async Task SendNotificationToUser(ViewModels.Todo.TodoItemViewModel reminder)
        {
            try
            {
                // SignalR üzerinden kullanıcıya bildirim gönder
                var notificationData = new
                {
                    type = "TodoReminder",
                    title = "Görev Hatırlatması",
                    message = $"'{reminder.Title}' başlıklı göreviniz için hatırlatma zamanı geldi!",
                    todoId = reminder.Id,
                    todoTitle = reminder.Title,
                    url = "/Todo/Index",
                    timestamp = DateTime.Now
                };
                
                // Kullanıcıya özel bildirim gönder
                if (!string.IsNullOrEmpty(reminder.AssignedToUserId))
                {
                    _logger.LogInformation($"Kullanıcı ID {reminder.AssignedToUserId} için hatırlatma gönderiliyor: {reminder.Title}");
                    
                    // Doğrudan kullanıcıya bildirim gönder (sound=true)
                    await _hubContext.Clients.User(reminder.AssignedToUserId).SendAsync("ReceiveTodoReminder", notificationData);
                    
                    // Normal bildirim olarak da gönder
                    await _hubContext.Clients.User(reminder.AssignedToUserId).SendAsync("ReceiveNotification", notificationData);
                    
                    _logger.LogInformation($"Kullanıcı {reminder.AssignedToUserName} için hatırlatma bildirimi gönderildi.");
                }
                else
                {
                    _logger.LogWarning($"Görev ID {reminder.Id} için kullanıcı ID'si bulunamadı. Bildirim gönderilemiyor.");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Hatırlatma bildirimi gönderilirken hata oluştu. TodoId: {reminder.Id}");
            }
        }
    }
} 