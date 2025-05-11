using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace MuhasebeStokWebApp.ViewModels.Todo
{
    public class TodoDashboardStatsViewModel
    {
        // Toplam görev sayısı
        [Display(Name = "Toplam Görev")]
        public int TotalTaskCount { get; set; }
        
        // Kullanıcıya atanan aktif görevler
        [Display(Name = "Atanan Görevler")]
        public int AssignedTaskCount { get; set; }
        
        // Bugün hatırlatılacak görevler
        [Display(Name = "Bugünkü Hatırlatmalar")]
        public int TodayReminderCount { get; set; }
        
        // Tamamlanmış görevler
        [Display(Name = "Tamamlanan Görevler")]
        public int CompletedTaskCount { get; set; }
        
        // Beklemede / Başlamamış görevler
        [Display(Name = "Bekleyen Görevler")]
        public int PendingTaskCount { get; set; }
        
        // Arşivlenmiş görevler
        [Display(Name = "Arşivlenen Görevler")]
        public int ArchivedTaskCount { get; set; }
        
        // Kullanıcı bilgisi
        public string UserName { get; set; }
    }
} 