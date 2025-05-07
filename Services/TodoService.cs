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
        
        public TodoService(ApplicationDbContext context, ILogger<TodoService> logger)
        {
            _context = context;
            _logger = logger;
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
                TaskCategory = todoItem.TaskCategory
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
        public async Task<List<TodoItemViewModel>> GetAllTodoItemsAsync()
        {
            try
            {
                var todos = await _context.TodoItems
                    .Include(t => t.AssignedToUser)
                    .OrderByDescending(t => t.CreatedAt)
                    .ToListAsync();
                
                return todos.Select(MapToViewModel).ToList();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Görevler listelenirken hata oluştu");
                return new List<TodoItemViewModel>();
            }
        }
        
        /// <summary>
        /// Belirli bir kullanıcıya ait görevleri getirir
        /// </summary>
        public async Task<List<TodoItemViewModel>> GetUserTodoItemsAsync(string userId)
        {
            try
            {
                var todos = await _context.TodoItems
                    .Include(t => t.AssignedToUser)
                    .Where(t => t.AssignedToUserId == userId)
                    .OrderByDescending(t => t.CreatedAt)
                    .ToListAsync();
                
                return todos.Select(MapToViewModel).ToList();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Kullanıcıya ait görevler listelenirken hata oluştu. UserID: {userId}");
                return new List<TodoItemViewModel>();
            }
        }
        
        /// <summary>
        /// Filtre seçeneğine göre görevleri getirir (Tümü, Tamamlananlar, Bekleyenler)
        /// </summary>
        public async Task<List<TodoItemViewModel>> GetFilteredTodoItemsAsync(string userId, string filterOption)
        {
            try
            {
                var query = _context.TodoItems
                    .Include(t => t.AssignedToUser)
                    .Where(t => t.AssignedToUserId == userId);
                
                // Filtre uygula
                if (filterOption == "completed")
                {
                    query = query.Where(t => t.IsCompleted);
                }
                else if (filterOption == "pending")
                {
                    query = query.Where(t => !t.IsCompleted);
                }
                
                var todos = await query
                    .OrderByDescending(t => t.CreatedAt)
                    .ToListAsync();
                
                return todos.Select(MapToViewModel).ToList();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Filtreli görevler listelenirken hata oluştu. UserID: {userId}, FilterOption: {filterOption}");
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
        /// Yeni görev oluşturur
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
                    TaskCategory = model.TaskCategory
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
                if (todo == null)
                {
                    return false;
                }
                
                // Başlık değiştiyse kategoriyi yeniden belirle
                if (todo.Title != model.Title)
                {
                    model.TaskCategory = DetermineTaskCategory(model.Title);
                }
                
                todo.Title = model.Title;
                todo.Description = model.Description;
                todo.Deadline = model.Deadline;
                todo.IsCompleted = model.IsCompleted;
                todo.AssignedToUserId = model.AssignedToUserId;
                todo.TaskCategory = model.TaskCategory;
                
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
        /// Görev siler
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
                
                _context.TodoItems.Remove(todo);
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
        /// Görevin tamamlanma durumunu değiştirir
        /// </summary>
        public async Task<bool> ToggleTodoItemStatusAsync(int id)
        {
            try
            {
                var todo = await _context.TodoItems.FindAsync(id);
                if (todo == null)
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
    }
} 