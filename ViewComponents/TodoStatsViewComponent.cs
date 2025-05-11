using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Identity;
using MuhasebeStokWebApp.Data.Entities;
using MuhasebeStokWebApp.Services.Interfaces;
using MuhasebeStokWebApp.ViewModels.Todo;

namespace MuhasebeStokWebApp.ViewComponents
{
    public class TodoStatsViewComponent : ViewComponent
    {
        private readonly ITodoService _todoService;
        private readonly UserManager<ApplicationUser> _userManager;

        public TodoStatsViewComponent(
            ITodoService todoService,
            UserManager<ApplicationUser> userManager)
        {
            _todoService = todoService;
            _userManager = userManager;
        }

        public async Task<IViewComponentResult> InvokeAsync()
        {
            // Giriş yapan kullanıcıyı al
            var currentUser = await _userManager.GetUserAsync(HttpContext.User);
            var userId = currentUser?.Id;
            
            // Dashboard istatistikleri ViewModel'ini oluştur
            var viewModel = new TodoDashboardStatsViewModel
            {
                // Toplam görev sayısı (tüm görevler)
                TotalTaskCount = (await _todoService.GetAllTodoItemsAsync()).Count,
                
                // Kullanıcıya atanan görevler (tamamlanmamış)
                AssignedTaskCount = userId != null ? 
                    (await _todoService.GetFilteredTodoItemsAsync("active", userId)).Count : 0,
                
                // Bugün hatırlatılacak görevler
                TodayReminderCount = (await _todoService.GetTodoItemsByDateAsync(DateTime.Today)).Count,
                
                // Tamamlanmış görevler
                CompletedTaskCount = (await _todoService.GetFilteredTodoItemsAsync("completed")).Count,
                
                // Beklemede olan görevler
                PendingTaskCount = (await _todoService.GetFilteredTodoItemsAsync("pending")).Count,
                
                // Arşivlenmiş görevler
                ArchivedTaskCount = (await _todoService.GetArchivedTodoItemsAsync()).Count,
                
                // Kullanıcı adını da ekleyelim
                UserName = currentUser?.UserName
            };
            
            return View(viewModel);
        }
    }
} 