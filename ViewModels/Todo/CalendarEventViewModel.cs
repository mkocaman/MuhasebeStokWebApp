using System;
using System.ComponentModel.DataAnnotations;
using MuhasebeStokWebApp.Enums;

namespace MuhasebeStokWebApp.ViewModels.Todo
{
    // FullCalendar.js ile uyumlu event modeli
    public class CalendarEventViewModel
    {
        public int Id { get; set; }
        
        [Required]
        public string Title { get; set; }
        
        public string Description { get; set; }
        
        public string Start { get; set; }
        
        public string End { get; set; }
        
        public string Color { get; set; }
        
        public PriorityLevel PriorityLevel { get; set; }
        
        public MuhasebeStokWebApp.Enums.TaskStatus Status { get; set; }
        
        public bool IsCompleted { get; set; }
        
        public string AssignedToUserName { get; set; }
    }
} 