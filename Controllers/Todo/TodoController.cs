using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using MuhasebeStokWebApp.Data.Entities;
using MuhasebeStokWebApp.Services.Interfaces;
using MuhasebeStokWebApp.ViewModels.Todo;
using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc.Rendering;
using MuhasebeStokWebApp.Enums;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using AutoMapper;
using Microsoft.AspNetCore.Mvc.Filters;

namespace MuhasebeStokWebApp.Controllers.Todo
{
    // EditTodoViewModel tanımı
    public class EditTodoViewModel
    {
        public TodoItemViewModel TodoItem { get; set; }
        public List<SelectListItem> Users { get; set; } = new List<SelectListItem>();
    }
    
    [Authorize]
    public class TodoController : BaseController
    {
        private readonly ITodoService _todoService;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ILogger<TodoController> _logger;
        private readonly IMapper _mapper;
        
        public TodoController(
            ITodoService todoService,
            UserManager<ApplicationUser> userManager,
            ILogger<TodoController> logger,
            IMapper mapper,
            IMenuService menuService)
            : base(menuService, userManager)
        {
            _todoService = todoService;
            _userManager = userManager;
            _logger = logger;
            _mapper = mapper;
        }
        
        // BaseController'ın OnActionExecutionAsync metodunu override ederek Layout değişkenini ayarlıyoruz
        public override async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
        {
            // Layout'u ayarla
            ViewData["Layout"] = "~/Views/Shared/_Layout.cshtml";
            
            // Temel sınıfın metodunu çağır
            await base.OnActionExecutionAsync(context, next);
        }

        // GET: /Todo
        [HttpGet]
        public async Task<IActionResult> Index(string filterOption = "all")
        {
            try
            {
                // Kullanıcıyı al
                var user = await _userManager.GetUserAsync(User);
                if (user == null)
                {
                    TempData["ErrorMessage"] = "Kullanıcı bulunamadı.";
                    return RedirectToAction("Index", "Home");
                }
                
                // Kullanıcının Admin rolünde olup olmadığını kontrol et
                bool isAdmin = await _userManager.IsInRoleAsync(user, "Admin");
                
                // ViewModel oluştur
                var model = new TodoViewModel
                {
                    FilterOption = filterOption,
                    IsAdminView = isAdmin
                };
                
                // Filtre seçeneğine göre görevleri getir
                model.TodoItems = await _todoService.GetFilteredTodoItemsAsync(filterOption, user.Id, isAdmin);
                
                // Tüm kullanıcıları getir (atama için)
                var users = await _userManager.GetUsersInRoleAsync("User");
                model.Users = new List<SelectListItem>();
                
                // Admin kullanıcıları da listeye ekle
                var admins = await _userManager.GetUsersInRoleAsync("Admin");
                foreach (var adminUser in admins)
                {
                    if (!users.Any(u => u.Id == adminUser.Id))
                    {
                        users = users.Append(adminUser).ToList();
                    }
                }
                
                foreach (var u in users)
                {
                    model.Users.Add(new SelectListItem
                    {
                        Value = u.Id,
                        Text = u.FullName ?? u.UserName
                    });
                }
                
                return View(model);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Todo sayfası yüklenirken hata oluştu");
                TempData["ErrorMessage"] = "Todo sayfası yüklenirken bir hata oluştu.";
                return RedirectToAction("Index", "Home");
            }
        }
        
        // GET: /Todo/Calendar
        [HttpGet]
        public async Task<IActionResult> Calendar(string date = null)
        {
            try
            {
                // Kullanıcıyı al
                var user = await _userManager.GetUserAsync(User);
                if (user == null)
                {
                    TempData["ErrorMessage"] = "Kullanıcı bulunamadı.";
                    return RedirectToAction("Index", "Home");
                }
                
                // Tarih parametresini işle
                DateTime selectedDate = DateTime.Today;
                if (!string.IsNullOrEmpty(date))
                {
                    if (DateTime.TryParse(date, out DateTime parsedDate))
                    {
                        selectedDate = parsedDate.Date;
                    }
                }
                
                // Kullanıcının Admin rolünde olup olmadığını kontrol et
                bool isAdmin = await _userManager.IsInRoleAsync(user, "Admin");
                
                // Seçilen tarihe ait görevleri al
                var todayTasks = await _todoService.GetTodoItemsByDateAsync(selectedDate, user.Id, isAdmin);
                
                // Tüm görevleri takvim görünümü için dönüştür
                var calendarEvents = todayTasks.Select(t => new CalendarEventViewModel
                {
                    Id = t.Id,
                    Title = t.Title,
                    Start = t.Deadline?.ToString("yyyy-MM-dd") ?? selectedDate.ToString("yyyy-MM-dd"),
                    End = t.Deadline?.AddHours(1).ToString("yyyy-MM-dd") ?? selectedDate.ToString("yyyy-MM-dd"),
                    Description = t.Description,
                    PriorityLevel = t.PriorityLevel,
                    Status = t.Status,
                    IsCompleted = t.IsCompleted,
                    AssignedToUserName = t.AssignedToUserName,
                    Color = t.IsCompleted ? "#10b981" : // tamamlanmış: yeşil
                           (t.PriorityLevel == PriorityLevel.High ? "#ef4444" : // yüksek öncelik: kırmızı
                           (t.PriorityLevel == PriorityLevel.Medium ? "#f59e0b" : "#60a5fa")) // orta öncelik: turuncu, düşük öncelik: mavi
                }).ToList();
                
                var viewModel = new TodoViewModel
                {
                    SelectedDate = selectedDate,
                    CalendarEvents = calendarEvents,
                    IsAdminView = isAdmin
                };
                
                return View(viewModel);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Takvim sayfası yüklenirken hata oluştu");
                TempData["ErrorMessage"] = "Takvim sayfası yüklenirken bir hata oluştu.";
                return RedirectToAction("Index", "Home");
            }
        }
        
        // GET: /Todo/GetTodoDetails/5
        [HttpGet]
        public async Task<IActionResult> GetTodoDetails(int id)
        {
            try
            {
                var todo = await _todoService.GetTodoItemByIdAsync(id);
                if (todo == null)
                {
                    return Json(new { success = false, message = "Görev bulunamadı." });
                }
                
                return Json(new { success = true, data = todo });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Görev detayları getirilirken hata oluştu. ID: {id}");
                return Json(new { success = false, message = "Görev detayları getirilirken bir hata oluştu." });
            }
        }
        
        // GET: /Todo/Edit/5
        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            try
            {
                var user = await _userManager.GetUserAsync(User);
                if (user == null)
                {
                    return Json(new { success = false, message = "Kullanıcı oturumu bulunamadı." });
                }
                
                var todoItem = await _todoService.GetTodoItemByIdAsync(id);
                if (todoItem == null)
                {
                    return NotFound();
                }
                
                // Kullanıcının admin rolünde olup olmadığını kontrol et
                bool isAdmin = await _userManager.IsInRoleAsync(user, "Admin");
                
                // Sadece admin kullanıcıları veya görev sahibi olan kullanıcılar düzenleyebilir
                if (!isAdmin && todoItem.AssignedToUserId != user.Id)
                {
                    return RedirectToAction("Index");
                }
                
                // Kullanıcı listesini getir
                var model = new EditTodoViewModel
                {
                    TodoItem = todoItem,
                    Users = await _todoService.GetUserSelectListAsync()
                };
                
                return PartialView("_EditTodo", model);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Görev düzenleme sayfası yüklenirken hata oluştu. ID: {id}");
                return Json(new { success = false, message = "Görev düzenleme sayfası yüklenirken bir hata oluştu." });
            }
        }
        
        // POST: /Todo/Create
        [HttpPost]
        public async Task<IActionResult> Create(TodoItemViewModel model)
        {
            try
            {
                // Mevcut kullanıcıyı al
                var user = await _userManager.GetUserAsync(User);
                if (user == null)
                {
                    return Json(new { success = false, message = "Kullanıcı oturumu bulunamadı." });
                }
                
                // Model doğrulama hatalarını kontrol et
                if (!ModelState.IsValid)
                {
                    var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage);
                    return Json(new { success = false, message = string.Join(", ", errors) });
                }
                
                // Başlık kontrolü
                if (string.IsNullOrEmpty(model.Title))
                {
                    return Json(new { success = false, message = "Başlık alanı zorunludur." });
                }
                
                // Hatırlatıcı tarihinin geçmiş zaman olup olmadığını kontrol et
                if (model.UseReminder && model.ReminderAt.HasValue && model.ReminderAt.Value < DateTime.Now)
                {
                    return Json(new { success = false, message = "Hatırlatma zamanı geçmiş bir zaman olamaz." });
                }
                
                // UseReminder true ise ama ReminderAt boşsa hatırlatma kullanma
                if (model.UseReminder && !model.ReminderAt.HasValue)
                {
                    model.UseReminder = false;
                }
                
                // UseReminder false ise ReminderAt null yap
                if (!model.UseReminder)
                {
                    model.ReminderAt = null;
                }
                
                // Kullanıcının admin rolünde olup olmadığını kontrol et
                bool isAdmin = await _userManager.IsInRoleAsync(user, "Admin");
                
                // Admin değilse veya AssignedToUserId null/empty ise, varsayılan olarak görevi oluşturan kişiye ata
                if (!isAdmin || string.IsNullOrEmpty(model.AssignedToUserId))
                {
                    model.AssignedToUserId = user.Id;
                }
                
                // Görev oluştur
                var todoId = await _todoService.CreateTodoItemAsync(model);
                
                if (todoId > 0)
                {
                    return Json(new { success = true, message = "Görev başarıyla oluşturuldu." });
                }
                else
                {
                    return Json(new { success = false, message = "Görev oluşturulurken bir hata oluştu." });
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Görev oluşturulurken bir hata oluştu: {Message}", ex.Message);
                return Json(new { success = false, message = "Görev oluşturulurken bir hata meydana geldi: " + ex.Message });
            }
        }
        
        // POST: /Todo/Edit
        [HttpPost]
        public async Task<IActionResult> Edit(TodoItemViewModel model)
        {
            try
            {
                // Model doğrulama hatalarını kontrol et
                if (!ModelState.IsValid)
                {
                    var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage);
                    return Json(new { success = false, message = string.Join(", ", errors) });
                }
                
                // Başlık kontrolü
                if (string.IsNullOrEmpty(model.Title))
                {
                    return Json(new { success = false, message = "Başlık alanı zorunludur." });
                }
                
                // Hatırlatıcı tarihinin geçmiş zaman olup olmadığını kontrol et
                if (model.UseReminder && model.ReminderAt.HasValue && model.ReminderAt.Value < DateTime.Now)
                {
                    return Json(new { success = false, message = "Hatırlatma zamanı geçmiş bir zaman olamaz." });
                }
                
                // UseReminder true ise ama ReminderAt boşsa hatırlatma kullanma
                if (model.UseReminder && !model.ReminderAt.HasValue)
                {
                    model.UseReminder = false;
                }
                
                // UseReminder false ise ReminderAt null yap
                if (!model.UseReminder)
                {
                    model.ReminderAt = null;
                }
                
                // Mevcut görevi al ve sadece izin verilen alanları güncelle
                var existingTodo = await _todoService.GetTodoItemByIdAsync(model.Id);
                if (existingTodo == null)
                {
                    return Json(new { success = false, message = "Güncellenmek istenen görev bulunamadı." });
                }
                
                // Güncelle
                var result = await _todoService.UpdateTodoItemAsync(model);
                
                if (result)
                {
                    return Json(new { success = true, message = "Görev başarıyla güncellendi." });
                }
                else
                {
                    return Json(new { success = false, message = "Görev güncellenirken bir hata oluştu." });
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Görev güncellenirken bir hata oluştu: {Message}", ex.Message);
                return Json(new { success = false, message = "Görev güncellenirken bir hata meydana geldi: " + ex.Message });
            }
        }
        
        // POST: /Todo/Delete/5
        [HttpPost]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                var result = await _todoService.DeleteTodoItemAsync(id);
                
                if (result)
                {
                    if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
                    {
                        return Json(new { success = true, message = "Görev başarıyla silindi." });
                    }
                    
                    TempData["SuccessMessage"] = "Görev başarıyla silindi.";
                }
                else
                {
                    if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
                    {
                        return Json(new { success = false, message = "Görev silinirken bir hata oluştu." });
                    }
                    
                    TempData["ErrorMessage"] = "Görev silinirken bir hata oluştu.";
                }
                
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Görev silinirken hata oluştu. ID: {id}");
                
                if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
                {
                    return Json(new { success = false, message = "Görev silinirken bir hata oluştu." });
                }
                
                TempData["ErrorMessage"] = "Görev silinirken bir hata oluştu.";
                return RedirectToAction(nameof(Index));
            }
        }
        
        // POST: /Todo/Restore/5
        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Restore(int id)
        {
            try
            {
                var result = await _todoService.RestoreTodoItemAsync(id);
                
                if (result)
                {
                    if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
                    {
                        return Json(new { success = true, message = "Görev başarıyla geri yüklendi." });
                    }
                    
                    TempData["SuccessMessage"] = "Görev başarıyla geri yüklendi.";
                }
                else
                {
                    if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
                    {
                        return Json(new { success = false, message = "Görev geri yüklenirken bir hata oluştu." });
                    }
                    
                    TempData["ErrorMessage"] = "Görev geri yüklenirken bir hata oluştu.";
                }
                
                return RedirectToAction(nameof(Index), new { filterOption = "deleted" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Görev geri yüklenirken hata oluştu. ID: {id}");
                
                if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
                {
                    return Json(new { success = false, message = "Görev geri yüklenirken bir hata oluştu." });
                }
                
                TempData["ErrorMessage"] = "Görev geri yüklenirken bir hata oluştu.";
                return RedirectToAction(nameof(Index));
            }
        }
        
        // POST: /Todo/ToggleComplete
        [HttpPost]
        public async Task<IActionResult> ToggleComplete(int id)
        {
            try
            {
                var result = await _todoService.ToggleTodoCompletionAsync(id);
                
                if (result)
                {
                    return Json(new { success = true, message = "Görev durumu başarıyla değiştirildi." });
                }
                else
                {
                    return Json(new { success = false, message = "Görev durumu değiştirilirken bir hata oluştu." });
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Görev durumu değiştirilirken bir hata oluştu: {Message}", ex.Message);
                return Json(new { success = false, message = "Görev durumu değiştirilirken bir hata meydana geldi: " + ex.Message });
            }
        }
        
        // POST: /Todo/AddComment
        [HttpPost]
        public async Task<IActionResult> AddComment(TodoCommentViewModel model)
        {
            try
            {
                // Mevcut kullanıcıyı al
                var user = await _userManager.GetUserAsync(User);
                if (user == null)
                {
                    return Json(new { success = false, message = "Kullanıcı oturumu bulunamadı." });
                }
                
                // Yorum sahibini ayarla
                model.AppUserId = user.Id;
                
                // Content alanı boş mu kontrol et
                if (string.IsNullOrWhiteSpace(model.Content))
                {
                    return Json(new { success = false, message = "Yorum içeriği boş olamaz." });
                }
                
                var result = await _todoService.AddCommentAsync(model);
                
                if (result > 0)
                {
                    // Yeni eklenen yorumu getir
                    var comments = await _todoService.GetTodoCommentsAsync(model.TodoItemId);
                    var newComment = comments.Find(c => c.Id == result);
                    
                    return Json(new { success = true, message = "Yorum başarıyla eklendi.", data = newComment });
                }
                else
                {
                    return Json(new { success = false, message = "Yorum eklenirken bir hata oluştu." });
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Yorum eklenirken bir hata oluştu: {Message}", ex.Message);
                return Json(new { success = false, message = "Yorum eklenirken bir hata oluştu: " + ex.Message });
            }
        }
        
        // POST: /Todo/DeleteComment
        [HttpPost]
        public async Task<IActionResult> DeleteComment(int commentId)
        {
            try
            {
                // Mevcut kullanıcıyı al
                var user = await _userManager.GetUserAsync(User);
                if (user == null)
                {
                    return Json(new { success = false, message = "Kullanıcı oturumu bulunamadı." });
                }
                
                var result = await _todoService.DeleteCommentAsync(commentId, user.Id);
                    
                    if (result)
                    {
                    return Json(new { success = true, message = "Yorum başarıyla silindi." });
                }
                else
                {
                    return Json(new { success = false, message = "Yorum silinirken bir hata oluştu veya bu yorumu silme yetkiniz yok." });
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Yorum silinirken bir hata oluştu: {Message}", ex.Message);
                return Json(new { success = false, message = "Yorum silinirken bir hata oluştu: " + ex.Message });
            }
        }
        
        // GET: /Todo/GetComments/5
        [HttpGet]
        public async Task<IActionResult> GetComments(int todoId)
        {
            try
            {
                var comments = await _todoService.GetTodoCommentsAsync(todoId);
                return Json(new { success = true, data = comments });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"AJAX: Yorumlar getirilirken hata oluştu. TodoID: {todoId}");
                return Json(new { success = false, message = "Yorumlar getirilirken bir hata oluştu." });
            }
        }
    }
} 