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
    }
} 