using System;
using System.Collections.Generic;
using System.Linq;

namespace MuhasebeStokWebApp.ViewModels.Shared
{
    public class TableViewModel
    {
        public string Title { get; set; } = "Veri Listesi";
        public string TableId { get; set; } = "dataTable";
        public IEnumerable<dynamic> Items { get; set; }
        public List<TableColumn> Columns { get; set; } = new List<TableColumn>();
        public bool ShowActions { get; set; } = true;
        public string IdPropertyName { get; set; } = "Id";
        public string ControllerName { get; set; }
        public bool ShowCreateButton { get; set; } = true;
        public bool ShowEditButton { get; set; } = true;
        public bool ShowDetailsButton { get; set; } = true;
        public bool ShowDeleteButton { get; set; } = true;
        public bool ShowActivateButton { get; set; } = false;
        public bool ShowDeleteConfirmation { get; set; } = true;
        public bool ShowActivateConfirmation { get; set; } = false;
        public bool EnableDataTable { get; set; } = true;
        public List<CustomButton> CustomButtons { get; set; } = new List<CustomButton>();
        public PaginationInfo Pagination { get; set; }
    }

    public class TableColumn
    {
        public string PropertyName { get; set; }
        public string DisplayName { get; set; }
        public Func<dynamic, object> GetValueFunc { get; set; }
    }

    public class CustomButton
    {
        public string DisplayText { get; set; }
        public string Url { get; set; }
        public string CssClass { get; set; } = "btn-secondary";
        public string IconClass { get; set; } = "fas fa-cog";
        public string OnClickFunction { get; set; }
    }

    public class PaginationInfo
    {
        public int CurrentPage { get; set; } = 1;
        public int PageSize { get; set; } = 10;
        public int TotalItems { get; set; }
        public int TotalPages => (int)Math.Ceiling(TotalItems / (double)PageSize);
    }

    public static class ObjectExtensions
    {
        public static object GetPropertyValue(this object obj, string propertyName)
        {
            if (obj == null || string.IsNullOrEmpty(propertyName))
                return null;
                
            var type = obj.GetType();
            var property = type.GetProperty(propertyName);
            
            if (property == null)
                return null;
                
            return property.GetValue(obj);
        }
    }
} 