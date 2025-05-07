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

namespace MuhasebeStokWebApp.Controllers.Todo
{
    [Authorize]
    public class TodoController : Controller
    {
        private readonly ITodoService _todoService;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ILogger<TodoController> _logger;
        
        public TodoController(
            ITodoService todoService,
            UserManager<ApplicationUser> userManager,
            ILogger<TodoController> logger)
        {
            _todoService = todoService;
            _userManager = userManager;
            _logger = logger;
        }

        // GET: /Todo
        [HttpGet]
        public async Task<IActionResult> Index(string filterOption = "all")
        {
            try
            {
                // Mevcut kullanıcıyı al
                var user = await _userManager.GetUserAsync(User);
                if (user == null)
                {
                    return RedirectToAction("Login", "Account");
                }
                
                // Kullanıcının görevlerini filtreli olarak getir
                var tasks = await _todoService.GetFilteredTodoItemsAsync(user.Id, filterOption);
                
                // Kullanıcı listesini getir
                var users = await _todoService.GetUserSelectListAsync();
                
                // View model oluştur
                var model = new TodoViewModel
                {
                    TodoItems = tasks,
                    Users = users,
                    FilterOption = filterOption
                };
                
                return View(model);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Yapılacaklar listesi görüntülenirken hata oluştu");
                TempData["ErrorMessage"] = "Yapılacaklar listesi yüklenirken bir hata oluştu.";
                return RedirectToAction("Index", "Home");
            }
        }
        
        // GET: /Todo/GetTodoDetails/5
        [HttpGet]
        public async Task<IActionResult> GetTodoDetails(int id)
        {
            try
            {
                var currentUser = await _userManager.GetUserAsync(User);
                if (currentUser == null)
                {
                    return Json(new { success = false, message = "Kullanıcı oturumu bulunamadı." });
                }
                
                var todo = await _todoService.GetTodoItemByIdAsync(id);
                if (todo == null)
                {
                    return Json(new { success = false, message = "Görev bulunamadı." });
                }
                
                // Kullanıcı sadece kendisine atanan görevlerin detaylarını görebilir
                if (todo.AssignedToUserId != currentUser.Id && !User.IsInRole("Admin"))
                {
                    return Json(new { success = false, message = "Bu görevi görüntüleme yetkiniz yok." });
                }
                
                // Yorumları da getir
                var comments = await _todoService.GetTodoCommentsAsync(id);
                todo.Comments = comments;
                
                return Json(new { success = true, data = todo });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Görev detayları getirilirken hata oluştu. ID: {id}");
                return Json(new { success = false, message = "Görev detayları alınırken bir hata oluştu." });
            }
        }
        
        // POST: /Todo/Update
        [HttpPost]
        public async Task<IActionResult> Update(TodoItemViewModel model)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return Json(new { success = false, message = "Form bilgileri geçerli değil." });
                }
                
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
                _logger.LogError(ex, $"AJAX: Görev güncellenirken hata oluştu. ID: {model.Id}");
                return Json(new { success = false, message = "Görev güncellenirken bir hata oluştu." });
            }
        }
        
        // POST: /Todo/ToggleStatus/5
        [HttpPost]
        public async Task<IActionResult> ToggleStatus(int id)
        {
            try
            {
                var result = await _todoService.ToggleTodoItemStatusAsync(id);
                
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
                _logger.LogError(ex, $"AJAX: Görev durumu değiştirilirken hata oluştu. ID: {id}");
                return Json(new { success = false, message = "Görev durumu değiştirilirken bir hata oluştu." });
            }
        }
        
        // POST: /Todo/Create (AJAX version)
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
                
                // Modeli kullanıcıya ata
                model.AssignedToUserId = user.Id;
                
                if (!ModelState.IsValid)
                {
                    return Json(new { success = false, message = "Form bilgileri geçerli değil." });
                }
                
                var result = await _todoService.CreateTodoItemAsync(model);
                
                if (result > 0)
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
                _logger.LogError(ex, "AJAX: Görev oluşturulurken hata oluştu");
                return Json(new { success = false, message = "Görev oluşturulurken bir hata oluştu." });
            }
        }
        
        // GET: /Todo/Edit/5
        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            try
            {
                var todoItem = await _todoService.GetTodoItemByIdAsync(id);
                if (todoItem == null)
                {
                    TempData["ErrorMessage"] = "Görev bulunamadı.";
                    return RedirectToAction(nameof(Index));
                }
                
                var users = await _todoService.GetUserSelectListAsync();
                
                var model = new EditTodoViewModel
                {
                    TodoItem = todoItem,
                    Users = users
                };
                
                return PartialView("_EditTodo", model);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Görev düzenleme sayfası yüklenirken hata oluştu. ID: {id}");
                TempData["ErrorMessage"] = "Görev düzenleme sayfası yüklenirken bir hata oluştu.";
                return RedirectToAction(nameof(Index));
            }
        }
        
        // POST: /Todo/Edit
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(TodoItemViewModel model)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    var result = await _todoService.UpdateTodoItemAsync(model);
                    
                    if (result)
                    {
                        TempData["SuccessMessage"] = "Görev başarıyla güncellendi.";
                    }
                    else
                    {
                        TempData["ErrorMessage"] = "Görev güncellenirken bir hata oluştu.";
                    }
                }
                else
                {
                    TempData["ErrorMessage"] = "Lütfen form bilgilerini kontrol ediniz.";
                }
                
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Görev güncellenirken hata oluştu. ID: {model.Id}");
                TempData["ErrorMessage"] = "Görev güncellenirken bir hata oluştu.";
                return RedirectToAction(nameof(Index));
            }
        }
        
        // POST: /Todo/Delete/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                var result = await _todoService.DeleteTodoItemAsync(id);
                
                if (result)
                {
                    TempData["SuccessMessage"] = "Görev başarıyla silindi.";
                }
                else
                {
                    TempData["ErrorMessage"] = "Görev silinirken bir hata oluştu.";
                }
                
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Görev silinirken hata oluştu. ID: {id}");
                TempData["ErrorMessage"] = "Görev silinirken bir hata oluştu.";
                return RedirectToAction(nameof(Index));
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
                
                if (!ModelState.IsValid)
                {
                    return Json(new { success = false, message = "Form bilgileri geçerli değil." });
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
                _logger.LogError(ex, "AJAX: Yorum eklenirken hata oluştu");
                return Json(new { success = false, message = "Yorum eklenirken bir hata oluştu." });
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
                _logger.LogError(ex, $"AJAX: Yorum silinirken hata oluştu. CommentID: {commentId}");
                return Json(new { success = false, message = "Yorum silinirken bir hata oluştu." });
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
    
    public class EditTodoViewModel
    {
        public TodoItemViewModel? TodoItem { get; set; }
        public List<SelectListItem>? Users { get; set; }
    }
} 