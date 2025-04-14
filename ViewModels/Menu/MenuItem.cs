using System;
using System.Collections.Generic;

namespace MuhasebeStokWebApp.ViewModels.Menu
{
    public class MenuItem
    {
        public string Id { get; set; }
        public string Text { get; set; }
        public string Icon { get; set; }
        public string Url { get; set; }
        public string Action { get; set; }
        public string Controller { get; set; }
        public List<MenuItem> Items { get; set; } = new List<MenuItem>();
    }
} 