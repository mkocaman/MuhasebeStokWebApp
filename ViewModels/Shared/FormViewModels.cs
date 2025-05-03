using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Html;

namespace MuhasebeStokWebApp.ViewModels.Shared
{
    public class FormInputViewModel
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string Label { get; set; }
        public string Value { get; set; }
        public string Placeholder { get; set; }
        public string InputType { get; set; } = "text";
        public string CssClass { get; set; } = "";
        public string PrependIcon { get; set; }
        public string AppendText { get; set; }
        public string HelpText { get; set; }
        public string ErrorMessage { get; set; }
        public string HtmlAttributes { get; set; } = "";
        public bool IsRequired { get; set; } = false;
        public bool IsDisabled { get; set; } = false;
        public bool IsReadOnly { get; set; } = false;
        public bool IsInvalid { get; set; } = false;
        public bool AutoComplete { get; set; } = true;
    }

    public class FormSelectViewModel
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string Label { get; set; }
        public string DefaultOption { get; set; } = "Seçiniz...";
        public string CssClass { get; set; } = "";
        public string PrependIcon { get; set; }
        public string HelpText { get; set; }
        public string ErrorMessage { get; set; }
        public string HtmlAttributes { get; set; } = "";
        public List<SelectListItem> Items { get; set; }
        public bool IsRequired { get; set; } = false;
        public bool IsDisabled { get; set; } = false;
        public bool IsMultiple { get; set; } = false;
        public bool IsInvalid { get; set; } = false;
        public bool EnableSelect2 { get; set; } = true;
    }

    public class FormTextAreaViewModel
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string Label { get; set; }
        public string Value { get; set; }
        public string Placeholder { get; set; }
        public string CssClass { get; set; } = "";
        public string HelpText { get; set; }
        public string ErrorMessage { get; set; }
        public string HtmlAttributes { get; set; } = "";
        public int Rows { get; set; } = 4;
        public bool IsRequired { get; set; } = false;
        public bool IsDisabled { get; set; } = false;
        public bool IsReadOnly { get; set; } = false;
        public bool IsInvalid { get; set; } = false;
        public bool EnableRichTextEditor { get; set; } = false;
    }
    
    public class FormCheckboxViewModel
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string Label { get; set; }
        public string CssClass { get; set; } = "";
        public string HelpText { get; set; }
        public string ErrorMessage { get; set; }
        public string HtmlAttributes { get; set; } = "";
        public bool IsChecked { get; set; } = false;
        public bool IsRequired { get; set; } = false;
        public bool IsDisabled { get; set; } = false;
        public bool IsInvalid { get; set; } = false;
    }
    
    public class FormGroupViewModel
    {
        public string Title { get; set; }
        public string Icon { get; set; }
        public string ColumnClass { get; set; } = "col-md-12";
        public string CardClass { get; set; } = "shadow-sm";
        public IHtmlContent Content { get; set; }
    }
} 