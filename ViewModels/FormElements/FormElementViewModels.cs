using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using System.Collections.Generic;
using System.Linq.Expressions;
using System;

namespace MuhasebeStokWebApp.ViewModels.FormElements
{
    /// <summary>
    /// Text input alanları için viewmodel
    /// </summary>
    public class TextInputViewModel
    {
        /// <summary>
        /// Input alanının bağlı olduğu model property'si
        /// </summary>
        public ModelExpression For { get; set; }

        /// <summary>
        /// Label metni
        /// </summary>
        public string Label { get; set; }

        /// <summary>
        /// Placeholder metni
        /// </summary>
        public string Placeholder { get; set; }

        /// <summary>
        /// Input type'ı (text, email, number, date vb.)
        /// </summary>
        public string Type { get; set; } = "text";

        /// <summary>
        /// Input değeri
        /// </summary>
        public string Value { get; set; }

        /// <summary>
        /// Yardım metni
        /// </summary>
        public string HelpText { get; set; }

        /// <summary>
        /// Sadece okunabilir mi
        /// </summary>
        public bool IsReadOnly { get; set; }

        /// <summary>
        /// Zorunlu alan mı
        /// </summary>
        public bool IsRequired { get; set; }
    }

    /// <summary>
    /// Select input alanları için viewmodel
    /// </summary>
    public class SelectInputViewModel
    {
        /// <summary>
        /// Select alanının bağlı olduğu model property'si
        /// </summary>
        public ModelExpression For { get; set; }

        /// <summary>
        /// Label metni
        /// </summary>
        public string Label { get; set; }

        /// <summary>
        /// Select listesi
        /// </summary>
        public IEnumerable<SelectListItem> Items { get; set; }

        /// <summary>
        /// Seçilen değer
        /// </summary>
        public string SelectedValue { get; set; }

        /// <summary>
        /// Yardım metni
        /// </summary>
        public string HelpText { get; set; }

        /// <summary>
        /// Zorunlu alan mı
        /// </summary>
        public bool IsRequired { get; set; }

        /// <summary>
        /// Çoklu seçim yapılabilir mi
        /// </summary>
        public bool IsMultiple { get; set; }
    }

    /// <summary>
    /// Checkbox için viewmodel
    /// </summary>
    public class CheckboxInputViewModel
    {
        /// <summary>
        /// Checkbox alanının bağlı olduğu model property'si
        /// </summary>
        public ModelExpression For { get; set; }

        /// <summary>
        /// Label metni
        /// </summary>
        public string Label { get; set; }

        /// <summary>
        /// Checkbox değeri
        /// </summary>
        public bool IsChecked { get; set; }

        /// <summary>
        /// Yardım metni
        /// </summary>
        public string HelpText { get; set; }

        /// <summary>
        /// Disabled mı
        /// </summary>
        public bool IsDisabled { get; set; }
    }

    /// <summary>
    /// Textarea için viewmodel
    /// </summary>
    public class TextAreaInputViewModel
    {
        /// <summary>
        /// Textarea alanının bağlı olduğu model property'si
        /// </summary>
        public ModelExpression For { get; set; }

        /// <summary>
        /// Label metni
        /// </summary>
        public string Label { get; set; }

        /// <summary>
        /// Placeholder metni
        /// </summary>
        public string Placeholder { get; set; }

        /// <summary>
        /// Textarea değeri
        /// </summary>
        public string Value { get; set; }

        /// <summary>
        /// Satır sayısı
        /// </summary>
        public int Rows { get; set; } = 3;

        /// <summary>
        /// Yardım metni
        /// </summary>
        public string HelpText { get; set; }

        /// <summary>
        /// Sadece okunabilir mi
        /// </summary>
        public bool IsReadOnly { get; set; }

        /// <summary>
        /// Zorunlu alan mı
        /// </summary>
        public bool IsRequired { get; set; }
    }
} 