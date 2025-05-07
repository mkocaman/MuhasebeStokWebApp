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
        Task<List<TodoItemViewModel>> GetAllTodoItemsAsync();
        Task<List<TodoItemViewModel>> GetUserTodoItemsAsync(string userId);
        Task<List<TodoItemViewModel>> GetFilteredTodoItemsAsync(string userId, string filterOption);
        Task<TodoItemViewModel?> GetTodoItemByIdAsync(int id);
        
        // Görev ekleme, güncelleme ve silme işlemleri
        Task<int> CreateTodoItemAsync(TodoItemViewModel model);
        Task<bool> UpdateTodoItemAsync(TodoItemViewModel model);
        Task<bool> DeleteTodoItemAsync(int id);
        Task<bool> ToggleTodoItemStatusAsync(int id);
        
        // Kullanıcı listesi çekme
        Task<List<SelectListItem>> GetUserSelectListAsync();
        
        // Yorum işlemleri
        Task<List<TodoCommentViewModel>> GetTodoCommentsAsync(int todoId);
        Task<int> AddCommentAsync(TodoCommentViewModel model);
        Task<bool> DeleteCommentAsync(int commentId, string userId);
    }
} 