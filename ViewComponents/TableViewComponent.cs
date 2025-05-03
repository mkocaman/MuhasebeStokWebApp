using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MuhasebeStokWebApp.ViewModels.Shared;

namespace MuhasebeStokWebApp.ViewComponents
{
    public class TableViewModel<T>
    {
        public IEnumerable<T> Items { get; set; }
        public string[] ColumnNames { get; set; }
        public string[] PropertyNames { get; set; }
        public string IdProperty { get; set; }
        public bool IncludeActions { get; set; }
        public string[] ActionButtons { get; set; }
        public int PageSize { get; set; }
        public int CurrentPage { get; set; }
        public int TotalPages => Items != null ? (int)Math.Ceiling(Items.Count() / (double)PageSize) : 0;
    }

    public class TableViewComponent : ViewComponent
    {
        public IViewComponentResult Invoke(TableViewModel model)
        {
            return View(model);
        }
    }
} 