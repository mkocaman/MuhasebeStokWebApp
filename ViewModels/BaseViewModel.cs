using System;
using System.Collections.Generic;
using MuhasebeStokWebApp.ViewModels.Menu;

namespace MuhasebeStokWebApp.ViewModels
{
    public class BaseViewModel
    {
        public string PageTitle { get; set; } = string.Empty;
        public MenuViewModel? MenuInfo { get; set; }
        public string MenuTitle { get; set; } = string.Empty;
        public List<dynamic> AppLanguages { get; set; } = new List<dynamic>();
        
        // Kullanıcı bilgileri
        public Guid CurrentUserId { get; set; }
        public string CurrentUserName { get; set; } = string.Empty;
        public string CurrentUserFullName { get; set; } = string.Empty;
        public string CurrentUserEmail { get; set; } = string.Empty;
        public List<string> CurrentUserRoles { get; set; } = new List<string>();
        public bool IsAdmin { get; set; }
    }
} 