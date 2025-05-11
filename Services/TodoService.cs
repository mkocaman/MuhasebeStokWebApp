using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using MuhasebeStokWebApp.Data;
using MuhasebeStokWebApp.Data.Entities;
using MuhasebeStokWebApp.Services.Interfaces;
using MuhasebeStokWebApp.ViewModels.Todo;
using System.Text.RegularExpressions;

namespace MuhasebeStokWebApp.Services
{
    public class TodoService : ITodoService
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<TodoService> _logger;
        private readonly INotificationService _notificationService;
        
        public TodoService(ApplicationDbContext context, ILogger<TodoService> logger, INotificationService notificationService = null)
        {
            _context = context;
            _logger = logger;
            _notificationService = notificationService;
        }
        
        /// <summary>
        /// Başlık içeriğine göre kategori belirler
        /// </summary>
        private string DetermineTaskCategory(string title)
        {
            title = title.ToLower();
            
            if (Regex.IsMatch(title, @"\b(ödeme|tahsilat|fatura|dekont|banka)\b"))
                return "Finans";
                
            if (Regex.IsMatch(title, @"\b(müşteri|görüşme|toplantı|randevu)\b"))
                return "Görüşme";
                
            if (Regex.IsMatch(title, @"\b(teklif|sözleşme|anlaşma|proje)\b"))
                return "Anlaşma";
                
            if (Regex.IsMatch(title, @"\b(satın alma|sipariş|tedarik)\b"))
                return "Satın Alma";
                
            if (Regex.IsMatch(title, @"\b(satış|teslimat|irsaliye)\b"))
                return "Satış";
                
            if (Regex.IsMatch(title, @"\b(kargo|nakliye|sevk|lojistik)\b"))
                return "Lojistik";
                
            if (Regex.IsMatch(title, @"\b(stok|depo|envanter|malzeme)\b"))
                return "Stok";
                
            if (Regex.IsMatch(title, @"\b(personel|insan kaynakları|bordro)\b"))
                return "İK";
                
            return "Genel";
        }
        
        /// <summary>
        /// Kategori adına göre ikon belirler
        /// </summary>
        private string DetermineTaskIcon(string category)
        {
            return category.ToLower() switch
            {
                "finans" => "fa fa-credit-card",
                "görüşme" => "fa fa-handshake",
                "anlaşma" => "fa fa-file-signature",
                "satın alma" => "fa fa-cart-arrow-down",
                "satış" => "fa fa-dollar-sign",
                "lojistik" => "fa fa-truck",
                "stok" => "fa fa-boxes",
                "i̇k" => "fa fa-user-tie",
                _ => "fa fa-briefcase"
            };
        }
        
        /// <summary>
        /// Görev kategorisine göre ikon belirler
        /// </summary>
        private string GetTaskIcon(string category)
        {
            if (string.IsNullOrEmpty(category))
                return "fa fa-tasks";

            category = category.ToLower();
            
            if (category.Contains("toplantı") || category.Contains("meeting"))
                return "fa fa-users";
            
            if (category.Contains("rapor") || category.Contains("report"))
                return "fa fa-file-alt";
            
            if (category.Contains("proje") || category.Contains("project"))
                return "fa fa-project-diagram";
            
            if (category.Contains("araştırma") || category.Contains("research"))
                return "fa fa-search";
            
            if (category.Contains("yazılım") || category.Contains("kod") || category.Contains("code"))
                return "fa fa-code";
            
            if (category.Contains("eğitim") || category.Contains("training"))
                return "fa fa-graduation-cap";
            
            if (category.Contains("müşteri") || category.Contains("client"))
                return "fa fa-user-tie";
            
            if (category.Contains("finans") || category.Contains("finance") || category.Contains("ödeme") || category.Contains("payment"))
                return "fa fa-money-bill-alt";
            
            if (category.Contains("telefon") || category.Contains("arama") || category.Contains("call"))
                return "fa fa-phone";
            
            if (category.Contains("mail") || category.Contains("email") || category.Contains("e-posta"))
                return "fa fa-envelope";
            
            if (category.Contains("destek") || category.Contains("support"))
                return "fa fa-life-ring";
            
            if (category.Contains("satış") || category.Contains("sales"))
                return "fa fa-chart-line";
            
            if (category.Contains("acil") || category.Contains("urgent"))
                return "fa fa-exclamation-circle";
            
            // Default ikon
            return "fa fa-briefcase";
        }
        
        /// <summary>
        /// TodoItem entity'sinden ViewModel'e dönüşüm yapar
        /// </summary>
        private TodoItemViewModel MapToViewModel(TodoItem todoItem)
        {
            var viewModel = new TodoItemViewModel
            {
                Id = todoItem.Id,
                Title = todoItem.Title,
                Description = todoItem.Description,
                CreatedAt = todoItem.CreatedAt,
                Deadline = todoItem.Deadline,
                IsCompleted = todoItem.IsCompleted,
                AssignedToUserId = todoItem.AssignedToUserId,
                AssignedToUserName = todoItem.AssignedToUser != null ? todoItem.AssignedToUser.FullNameCalculated : "Atanmamış",
                TaskCategory = todoItem.TaskCategory,
                PriorityLevel = todoItem.PriorityLevel,
                Status = todoItem.Status,
                Tags = todoItem.Tags,
                IsArchived = todoItem.IsArchived,
                IsDeleted = todoItem.IsDeleted,
                ReminderAt = todoItem.ReminderAt,
                UseReminder = todoItem.ReminderAt.HasValue,
                IsReminderSent = todoItem.IsReminderSent
            };
            
            // Kategori yoksa belirle
            if (string.IsNullOrEmpty(viewModel.TaskCategory))
            {
                viewModel.TaskCategory = DetermineTaskCategory(todoItem.Title);
            }
            
            // İkonu belirle
            viewModel.TaskIcon = DetermineTaskIcon(viewModel.TaskCategory);
            
            return viewModel;
        }
        
        /// <summary>
        /// Tüm görevleri getirir
        /// </summary>
        public async Task<List<TodoItemViewModel>> GetAllTodoItemsAsync(string? userId = null, bool isAdmin = false)
        {
            try
            {
                var query = _context.TodoItems
                    .Include(t => t.AssignedToUser)
                    .AsQueryable();
                
                // Admin olmayan kullanıcılar sadece silinmemiş görevleri görebilir
                if (!isAdmin)
                {
                    query = query.Where(t => !t.IsDeleted);
                }
                
                // Admin ise tüm görevleri, normal kullanıcı ise sadece kendi görevlerini getir
                if (!string.IsNullOrEmpty(userId) && !isAdmin)
                {
                    query = query.Where(t => t.AssignedToUserId == userId);
                }
                
                var items = await query.ToListAsync();
                
                return items.Select(t => MapToViewModel(t)).ToList();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Görevler listelenirken hata oluştu");
                return new List<TodoItemViewModel>();
            }
        }
        
        /// <summary>
        /// Filtreye göre görevleri getirir (tümü, tamamlananlar, bekleyenler, silinmiş)
        /// </summary>
        public async Task<List<TodoItemViewModel>> GetFilteredTodoItemsAsync(string filter, string? userId = null, bool isAdmin = false)
        {
            try
            {
                var query = _context.TodoItems
                    .Include(t => t.AssignedToUser)
                    .AsQueryable();
                
                // Filtreye göre sorguyu oluştur
                switch (filter.ToLower())
                {
                    case "pending":
                        query = query.Where(t => !t.IsCompleted && !t.IsArchived);
                        break;
                    case "completed":
                        query = query.Where(t => t.IsCompleted && !t.IsArchived);
                        break;
                    case "deleted":
                        query = query.Where(t => t.IsDeleted);
                        break;
                    case "all":
                    default:
                        query = query.Where(t => !t.IsArchived && !t.IsDeleted);
                        break;
                }
                
                // Yetki kontrolü
                if (!string.IsNullOrEmpty(userId) && !isAdmin)
                {
                    query = query.Where(t => t.AssignedToUserId == userId);
                }
                
                var items = await query.ToListAsync();
                
                return items.Select(t => MapToViewModel(t)).ToList();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "GetFilteredTodoItemsAsync metodu çalışırken hata oluştu: {Error}", ex.Message);
                return new List<TodoItemViewModel>();
            }
        }
        
        /// <summary>
        /// ID'ye göre görev detayını getirir
        /// </summary>
        public async Task<TodoItemViewModel?> GetTodoItemByIdAsync(int id)
        {
            try
            {
                var todo = await _context.TodoItems
                    .Include(t => t.AssignedToUser)
                    .FirstOrDefaultAsync(t => t.Id == id);
                
                if (todo == null)
                    return null;
                
                return MapToViewModel(todo);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Görev detayı getirilirken hata oluştu. ID: {id}");
                return null;
            }
        }
        
        /// <summary>
        /// Görev oluşturur
        /// </summary>
        public async Task<int> CreateTodoItemAsync(TodoItemViewModel model)
        {
            try
            {
                // Doğrulama kontrolü
                if (!model.IsValid())
                {
                    return 0;
                }
                
                // Kategori belirle
                if (string.IsNullOrEmpty(model.TaskCategory))
                {
                    model.TaskCategory = DetermineTaskCategory(model.Title);
                }
                
                var todo = new TodoItem
                {
                    Title = model.Title,
                    Description = model.Description,
                    CreatedAt = DateTime.Now,
                    Deadline = model.Deadline,
                    IsCompleted = model.IsCompleted,
                    AssignedToUserId = model.AssignedToUserId,
                    TaskCategory = model.TaskCategory,
                    PriorityLevel = model.PriorityLevel,
                    Status = model.Status,
                    Tags = model.Tags,
                    IsArchived = model.IsArchived,
                    ReminderAt = model.UseReminder ? model.ReminderAt : null,
                    IsReminderSent = false
                };
                
                _context.TodoItems.Add(todo);
                await _context.SaveChangesAsync();
                
                return todo.Id;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Görev oluşturulurken hata oluştu");
                return 0;
            }
        }
        
        /// <summary>
        /// Görev günceller
        /// </summary>
        public async Task<bool> UpdateTodoItemAsync(TodoItemViewModel model)
        {
            try
            {
                // Doğrulama kontrolü
                if (!model.IsValid())
                {
                    return false;
                }
                
                var todo = await _context.TodoItems.FindAsync(model.Id);
                if (todo == null || todo.IsDeleted)
                {
                    return false;
                }
                
                // Kategori belirle
                if (string.IsNullOrEmpty(model.TaskCategory))
                {
                    model.TaskCategory = DetermineTaskCategory(model.Title);
                }
                
                todo.Title = model.Title;
                todo.Description = model.Description;
                todo.Deadline = model.Deadline;
                todo.IsCompleted = model.IsCompleted;
                todo.AssignedToUserId = model.AssignedToUserId;
                todo.TaskCategory = model.TaskCategory;
                todo.PriorityLevel = model.PriorityLevel;
                todo.Status = model.Status;
                todo.Tags = model.Tags;
                todo.IsArchived = model.IsArchived;
                
                // Eğer tamamlandı işaretlendiyse, durum da Tamamlandı olarak güncellenir
                if (model.IsCompleted && todo.Status != MuhasebeStokWebApp.Enums.TaskStatus.Tamamlandi)
                {
                    todo.Status = MuhasebeStokWebApp.Enums.TaskStatus.Tamamlandi;
                }
                // Eğer tamamlandı kaldırıldıysa ve durum Tamamlandı ise, durum Beklemede olarak güncellenir
                else if (!model.IsCompleted && todo.Status == MuhasebeStokWebApp.Enums.TaskStatus.Tamamlandi)
                {
                    todo.Status = MuhasebeStokWebApp.Enums.TaskStatus.Beklemede;
                }
                
                // Hatırlatıcı bilgileri güncelle
                if (model.UseReminder && model.ReminderAt.HasValue)
                {
                    todo.ReminderAt = model.ReminderAt;
                    
                    // Eğer hatırlatma tarihi değiştiyse, IsReminderSent'i sıfırla
                    if (todo.ReminderAt != model.ReminderAt)
                    {
                        todo.IsReminderSent = false;
                    }
                }
                else
                {
                    todo.ReminderAt = null;
                    todo.IsReminderSent = false;
                }
                
                _context.TodoItems.Update(todo);
                await _context.SaveChangesAsync();
                
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Görev güncellenirken hata oluştu. ID: {model.Id}");
                return false;
            }
        }
        
        /// <summary>
        /// Görev siler (soft delete)
        /// </summary>
        public async Task<bool> DeleteTodoItemAsync(int id)
        {
            try
            {
                var todo = await _context.TodoItems.FindAsync(id);
                if (todo == null)
                {
                    return false;
                }
                
                // Soft delete yap
                todo.IsDeleted = true;
                
                _context.TodoItems.Update(todo);
                await _context.SaveChangesAsync();
                
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Görev silinirken hata oluştu. ID: {id}");
                return false;
            }
        }
        
        /// <summary>
        /// Silinmiş görevi geri yükler
        /// </summary>
        public async Task<bool> RestoreTodoItemAsync(int id)
        {
            try
            {
                var todo = await _context.TodoItems.FindAsync(id);
                if (todo == null)
                {
                    return false;
                }
                
                // Silindi işaretini kaldır
                todo.IsDeleted = false;
                
                _context.TodoItems.Update(todo);
                await _context.SaveChangesAsync();
                
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Görev geri yüklenirken hata oluştu. ID: {id}");
                return false;
            }
        }
        
        /// <summary>
        /// Görevin tamamlanma durumunu değiştirir
        /// </summary>
        public async Task<bool> ToggleTodoCompletionAsync(int id)
        {
            try
            {
                var todo = await _context.TodoItems.FindAsync(id);
                if (todo == null || todo.IsDeleted)
                {
                    return false;
                }
                
                todo.IsCompleted = !todo.IsCompleted;
                
                _context.TodoItems.Update(todo);
                await _context.SaveChangesAsync();
                
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Görev durumu değiştirilirken hata oluştu. ID: {id}");
                return false;
            }
        }
        
        /// <summary>
        /// Kullanıcı listesini SelectListItem formatında getirir
        /// </summary>
        public async Task<List<SelectListItem>> GetUserSelectListAsync()
        {
            try
            {
                var users = await _context.Users
                    .Where(u => u.Aktif && !u.Silindi)
                    .Select(u => new SelectListItem
                    {
                        Value = u.Id,
                        Text = u.FullNameCalculated
                    })
                    .ToListAsync();
                
                return users;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Kullanıcı listesi getirilirken hata oluştu");
                return new List<SelectListItem>();
            }
        }
        
        /// <summary>
        /// Görev yorumlarını getirir
        /// </summary>
        public async Task<List<TodoCommentViewModel>> GetTodoCommentsAsync(int todoId)
        {
            try
            {
                var comments = await _context.TodoComments
                    .Include(c => c.AppUser)
                    .Where(c => c.TodoItemId == todoId)
                    .OrderByDescending(c => c.CreatedAt)
                    .ToListAsync();
                
                return comments.Select(c => new TodoCommentViewModel
                {
                    Id = c.Id,
                    TodoItemId = c.TodoItemId,
                    AppUserId = c.AppUserId,
                    Content = c.Content,
                    CreatedAt = c.CreatedAt,
                    UserName = c.AppUser?.FullNameCalculated ?? "Bilinmeyen Kullanıcı",
                    UserProfileImage = c.AppUser?.ProfileImage
                }).ToList();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Görev yorumları getirilirken hata oluştu. TodoID: {todoId}");
                return new List<TodoCommentViewModel>();
            }
        }
        
        /// <summary>
        /// Yeni yorum ekler
        /// </summary>
        public async Task<int> AddCommentAsync(TodoCommentViewModel model)
        {
            try
            {
                var comment = new TodoComment
                {
                    TodoItemId = model.TodoItemId,
                    AppUserId = model.AppUserId,
                    Content = model.Content,
                    CreatedAt = DateTime.Now
                };
                
                _context.TodoComments.Add(comment);
                await _context.SaveChangesAsync();
                
                return comment.Id;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Yorum eklenirken hata oluştu. TodoID: {model.TodoItemId}");
                return 0;
            }
        }
        
        /// <summary>
        /// Yorum siler (sadece yorum sahibi silebilir)
        /// </summary>
        public async Task<bool> DeleteCommentAsync(int commentId, string userId)
        {
            try
            {
                var comment = await _context.TodoComments.FindAsync(commentId);
                if (comment == null || comment.AppUserId != userId)
                {
                    return false;
                }
                
                _context.TodoComments.Remove(comment);
                await _context.SaveChangesAsync();
                
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Yorum silinirken hata oluştu. CommentID: {commentId}");
                return false;
            }
        }
        
        /// <summary>
        /// Yaklaşan hatırlatmaları getir (gönderilememiş olanlar)
        /// </summary>
        public async Task<List<TodoItemViewModel>> GetPendingRemindersAsync()
        {
            try
            {
                var currentTime = DateTime.UtcNow;
                
                var pendingReminders = await _context.TodoItems
                    .Include(t => t.AssignedToUser)
                    .Where(t => !t.IsDeleted && 
                                !t.IsCompleted && 
                                t.ReminderAt.HasValue && 
                                t.ReminderAt <= currentTime && 
                                !t.IsReminderSent)
                    .ToListAsync();
                
                return pendingReminders.Select(t => MapToViewModel(t)).ToList();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Yaklaşan hatırlatmalar getirilirken hata oluştu");
                return new List<TodoItemViewModel>();
            }
        }
        
        /// <summary>
        /// Hatırlatmayı gönderildi olarak işaretle
        /// </summary>
        public async Task<bool> MarkReminderAsSentAsync(int todoId)
        {
            try
            {
                var todo = await _context.TodoItems.FindAsync(todoId);
                if (todo == null || todo.IsDeleted || todo.IsCompleted || !todo.ReminderAt.HasValue)
                {
                    return false;
                }
                
                todo.IsReminderSent = true;
                _context.TodoItems.Update(todo);
                await _context.SaveChangesAsync();
                
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Hatırlatma işaretlenirken hata oluştu. ID: {todoId}");
                return false;
            }
        }
        
        /// <summary>
        /// Görevi arşivler
        /// </summary>
        public async Task<bool> ArchiveTodoAsync(int id)
        {
            try
            {
                var todo = await _context.TodoItems.FindAsync(id);
                if (todo == null || todo.IsDeleted)
                {
                    return false;
                }
                
                todo.IsArchived = true;
                
                _context.TodoItems.Update(todo);
                await _context.SaveChangesAsync();
                
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Görev arşivlenirken hata oluştu. ID: {id}");
                return false;
            }
        }
        
        /// <summary>
        /// Görevi arşivden çıkarır
        /// </summary>
        public async Task<bool> UnarchiveTodoAsync(int id)
        {
            try
            {
                var todo = await _context.TodoItems.FindAsync(id);
                if (todo == null || todo.IsDeleted)
                {
                    return false;
                }
                
                todo.IsArchived = false;
                
                _context.TodoItems.Update(todo);
                await _context.SaveChangesAsync();
                
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Görev arşivden çıkarılırken hata oluştu. ID: {id}");
                return false;
            }
        }
        
        /// <summary>
        /// Arşivlenmiş görevleri getirir
        /// </summary>
        public async Task<List<TodoItemViewModel>> GetArchivedTodoItemsAsync(string? userId = null, bool isAdmin = false)
        {
            try
            {
                var query = _context.TodoItems
                    .Include(t => t.AssignedToUser)
                    .Where(t => !t.IsDeleted && t.IsArchived);
                
                // Eğer admin değilse sadece kendisine atanan görevleri görsün
                if (!isAdmin && !string.IsNullOrEmpty(userId))
                {
                    query = query.Where(t => t.AssignedToUserId == userId);
                }
                
                var todoItems = await query.ToListAsync();
                
                return todoItems.Select(t => MapToViewModel(t)).ToList();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Arşivlenmiş görevler getirilirken hata oluştu");
                return new List<TodoItemViewModel>();
            }
        }
        
        /// <summary>
        /// Görev durumunu günceller
        /// </summary>
        public async Task<bool> UpdateTodoStatusAsync(int id, MuhasebeStokWebApp.Enums.TaskStatus status)
        {
            try
            {
                var todo = await _context.TodoItems.FindAsync(id);
                if (todo == null || todo.IsDeleted)
                {
                    return false;
                }
                
                todo.Status = status;
                
                // Eğer durum Tamamlandı ise IsCompleted'ı da true yap
                if (status == MuhasebeStokWebApp.Enums.TaskStatus.Tamamlandi)
                {
                    todo.IsCompleted = true;
                }
                else
                {
                    todo.IsCompleted = false;
                }
                
                _context.TodoItems.Update(todo);
                await _context.SaveChangesAsync();
                
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Görev durumu güncellenirken hata oluştu. ID: {id}");
                return false;
            }
        }
        
        /// <summary>
        /// Tüm etiketleri getirir
        /// </summary>
        public async Task<List<string>> GetAllTagsAsync()
        {
            try
            {
                var allTags = await _context.TodoItems
                    .Where(t => !t.IsDeleted && !string.IsNullOrEmpty(t.Tags))
                    .Select(t => t.Tags)
                    .ToListAsync();
                
                // Tüm etiketleri virgülle ayırıp unique olanları al
                var tagSet = new HashSet<string>();
                
                foreach (var tagString in allTags)
                {
                    if (!string.IsNullOrEmpty(tagString))
                    {
                        var tags = tagString.Split(',', StringSplitOptions.RemoveEmptyEntries)
                            .Select(t => t.Trim().ToLower());
                        
                        foreach (var tag in tags)
                        {
                            tagSet.Add(tag);
                        }
                    }
                }
                
                return tagSet.OrderBy(t => t).ToList();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Tüm etiketler getirilirken hata oluştu");
                return new List<string>();
            }
        }
        
        /// <summary>
        /// Belirli etikete sahip görevleri getirir
        /// </summary>
        public async Task<List<TodoItemViewModel>> GetTodoItemsByTagAsync(string tag, string? userId = null, bool isAdmin = false)
        {
            try
            {
                // LIKE sorgusu için hazırla
                var searchTag = tag.Trim().ToLower();
                
                // Etiket içinde ara (tam eşleşme değil)
                var query = _context.TodoItems
                    .Include(t => t.AssignedToUser)
                    .Where(t => !t.IsDeleted && 
                               t.Tags != null && 
                               (t.Tags.ToLower().Contains($",{searchTag},") || 
                                t.Tags.ToLower().StartsWith($"{searchTag},") || 
                                t.Tags.ToLower().EndsWith($",{searchTag}") || 
                                t.Tags.ToLower() == searchTag));
                
                // Eğer admin değilse sadece kendisine atanan görevleri görsün
                if (!isAdmin && !string.IsNullOrEmpty(userId))
                {
                    query = query.Where(t => t.AssignedToUserId == userId);
                }
                
                var todoItems = await query.ToListAsync();
                
                return todoItems.Select(t => MapToViewModel(t)).ToList();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Etiketli görevler getirilirken hata oluştu. Tag: {tag}");
                return new List<TodoItemViewModel>();
            }
        }
        
        /// <summary>
        /// Kullanıcının bildirimlerini getirir (TodoService'de NotificationService'i çağırır)
        /// </summary>
        public async Task<NotificationListViewModel> GetUserNotificationsAsync(string userId, bool onlyUnread = false)
        {
            try
            {
                if (_notificationService != null)
                {
                    return await _notificationService.GetUserNotificationsAsync(userId, onlyUnread);
                }
                return new NotificationListViewModel();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Bildirimler getirilirken hata oluştu");
                return new NotificationListViewModel();
            }
        }
        
        /// <summary>
        /// Bildirimi okundu olarak işaretler (TodoService'de NotificationService'i çağırır)
        /// </summary>
        public async Task<bool> MarkNotificationAsReadAsync(int notificationId)
        {
            try
            {
                if (_notificationService != null)
                {
                    return await _notificationService.MarkAsReadAsync(notificationId);
                }
                return false;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Bildirim okundu olarak işaretlenirken hata oluştu");
                return false;
            }
        }
        
        /// <summary>
        /// Tüm bildirimleri okundu olarak işaretler (TodoService'de NotificationService'i çağırır)
        /// </summary>
        public async Task<bool> MarkAllNotificationsAsReadAsync(string userId)
        {
            try
            {
                if (_notificationService != null)
                {
                    return await _notificationService.MarkAllAsReadAsync(userId);
                }
                return false;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Tüm bildirimler okundu olarak işaretlenirken hata oluştu");
                return false;
            }
        }
        
        /// <summary>
        /// Bildirimi siler (TodoService'de NotificationService'i çağırır)
        /// </summary>
        public async Task<bool> DeleteNotificationAsync(int notificationId)
        {
            try
            {
                if (_notificationService != null)
                {
                    return await _notificationService.DeleteNotificationAsync(notificationId);
                }
                return false;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Bildirim silinirken hata oluştu");
                return false;
            }
        }
        
        /// <summary>
        /// Bildirim ekler (TodoService'de NotificationService'i çağırır)
        /// </summary>
        public async Task<int> AddNotificationAsync(string userId, string content, string type, int? relatedEntityId = null)
        {
            try
            {
                if (_notificationService != null)
                {
                    return await _notificationService.AddNotificationAsync(userId, content, type, relatedEntityId);
                }
                return 0;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Bildirim eklenirken hata oluştu");
                return 0;
            }
        }
        
        /// <summary>
        /// Belirli tarihteki görevleri getirir
        /// </summary>
        public async Task<List<TodoItemViewModel>> GetTodoItemsByDateAsync(DateTime date, string? userId = null, bool isAdmin = false)
        {
            try
            {
                var dateOnly = date.Date;
                var nextDay = dateOnly.AddDays(1);
                
                var query = _context.TodoItems
                    .Include(t => t.AssignedToUser)
                    .Where(t => !t.IsDeleted && 
                               ((t.Deadline >= dateOnly && t.Deadline < nextDay) || 
                                (t.ReminderAt >= dateOnly && t.ReminderAt < nextDay)));
                
                // Eğer admin değilse sadece kendisine atanan görevleri görsün
                if (!isAdmin && !string.IsNullOrEmpty(userId))
                {
                    query = query.Where(t => t.AssignedToUserId == userId);
                }
                
                var todoItems = await query.ToListAsync();
                
                return todoItems.Select(t => MapToViewModel(t)).ToList();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Tarih bazlı görevler getirilirken hata oluştu. Date: {date:yyyy-MM-dd}");
                return new List<TodoItemViewModel>();
            }
        }
        
        /// <summary>
        /// FullCalendar için takvim olaylarını getirir
        /// </summary>
        public async Task<List<CalendarEventViewModel>> GetCalendarEventsAsync(DateTime start, DateTime end, string? userId = null, bool isAdmin = false)
        {
            try
            {
                var query = _context.TodoItems
                    .Include(t => t.AssignedToUser)
                    .Where(t => !t.IsDeleted && 
                               ((t.Deadline >= start && t.Deadline <= end) || 
                                (t.ReminderAt >= start && t.ReminderAt <= end)));
                
                // Eğer admin değilse sadece kendisine atanan görevleri görsün
                if (!isAdmin && !string.IsNullOrEmpty(userId))
                {
                    query = query.Where(t => t.AssignedToUserId == userId);
                }
                
                var todoItems = await query.ToListAsync();
                var calendarEvents = new List<CalendarEventViewModel>();
                
                foreach (var todo in todoItems)
                {
                    string color;
                    
                    // Görev tamamlandı ise gri
                    if (todo.IsCompleted)
                    {
                        color = "#6c757d"; // Gri
                    }
                    // Görevin önceliğine göre renklendirme
                    else if (todo.PriorityLevel == Enums.PriorityLevel.High)
                    {
                        color = "#dc3545"; // Kırmızı
                    }
                    else if (todo.PriorityLevel == Enums.PriorityLevel.Medium)
                    {
                        color = "#ffc107"; // Sarı
                    }
                    else
                    {
                        color = "#198754"; // Yeşil
                    }
                    
                    // Son tarih olayı
                    if (todo.Deadline.HasValue)
                    {
                        calendarEvents.Add(new CalendarEventViewModel
                        {
                            Id = todo.Id,
                            Title = todo.Title,
                            Start = todo.Deadline.Value.ToString("yyyy-MM-dd"),
                            End = todo.Deadline.Value.AddHours(1).ToString("yyyy-MM-dd"),
                            Color = color,
                            Description = todo.Description,
                            PriorityLevel = todo.PriorityLevel,
                            Status = todo.Status,
                            IsCompleted = todo.IsCompleted,
                            AssignedToUserName = todo.AssignedToUser?.FullNameCalculated ?? "Atanmamış"
                        });
                    }
                    
                    // Hatırlatma olayı
                    if (todo.ReminderAt.HasValue)
                    {
                        calendarEvents.Add(new CalendarEventViewModel
                        {
                            Id = -todo.Id, // Negatif ID ile hatırlatıcı olduğunu belirt
                            Title = $"⏰ {todo.Title}",
                            Start = todo.ReminderAt.Value.ToString("yyyy-MM-dd"),
                            End = todo.ReminderAt.Value.AddHours(1).ToString("yyyy-MM-dd"),
                            Color = "#17a2b8", // Mavi
                            Description = todo.Description,
                            PriorityLevel = todo.PriorityLevel,
                            Status = todo.Status,
                            IsCompleted = todo.IsCompleted,
                            AssignedToUserName = todo.AssignedToUser?.FullNameCalculated ?? "Atanmamış"
                        });
                    }
                }
                
                return calendarEvents;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Takvim olayları getirilirken hata oluştu. Start: {start:yyyy-MM-dd}, End: {end:yyyy-MM-dd}");
                return new List<CalendarEventViewModel>();
            }
        }
        
        /// <summary>
        /// Son eklenen görevleri getirir
        /// </summary>
        public async Task<List<TodoItemViewModel>> GetRecentTodosAsync(int count, string? userId = null)
        {
            try
            {
                IQueryable<TodoItem> query = _context.TodoItems
                    .Include(t => t.AssignedToUser)
                    .Where(t => !t.IsArchived && !t.IsDeleted);
                
                // Belirli bir kullanıcıya ait görevleri filtrele
                if (!string.IsNullOrEmpty(userId))
                {
                    query = query.Where(t => t.AssignedToUserId == userId);
                }
                
                // OrderByDescending
                var orderedQuery = query.OrderByDescending(t => t.CreatedAt);
                
                // Belirtilen sayıda görev al
                var items = await orderedQuery.Take(count).ToListAsync();
                
                return items.Select(t => MapToViewModel(t)).ToList();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "GetRecentTodosAsync metodu çalışırken hata oluştu: {Error}", ex.Message);
                return new List<TodoItemViewModel>();
            }
        }
    }
} 