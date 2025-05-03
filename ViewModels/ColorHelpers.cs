using System.Collections.Generic;

namespace MuhasebeStokWebApp.ViewModels
{
    /// <summary>
    /// Renk kodları için yardımcı sınıf
    /// </summary>
    public static class ColorHelpers
    {
        /// <summary>
        /// Bootstrap renk sınıfları
        /// </summary>
        public static class BootstrapColors
        {
            public const string Primary = "primary";
            public const string Secondary = "secondary";
            public const string Success = "success";
            public const string Danger = "danger";
            public const string Warning = "warning";
            public const string Info = "info";
            public const string Light = "light";
            public const string Dark = "dark";
            
            // Arka plan renkleri
            public const string BgPrimary = "bg-primary";
            public const string BgSecondary = "bg-secondary";
            public const string BgSuccess = "bg-success";
            public const string BgDanger = "bg-danger";
            public const string BgWarning = "bg-warning";
            public const string BgInfo = "bg-info";
            public const string BgLight = "bg-light";
            public const string BgDark = "bg-dark";
            
            // Metin renkleri
            public const string TextPrimary = "text-primary";
            public const string TextSecondary = "text-secondary";
            public const string TextSuccess = "text-success";
            public const string TextDanger = "text-danger";
            public const string TextWarning = "text-warning";
            public const string TextInfo = "text-info";
            public const string TextLight = "text-light";
            public const string TextDark = "text-dark";
            
            // Kenar renkleri
            public const string BorderPrimary = "border-primary";
            public const string BorderSecondary = "border-secondary";
            public const string BorderSuccess = "border-success";
            public const string BorderDanger = "border-danger";
            public const string BorderWarning = "border-warning";
            public const string BorderInfo = "border-info";
            public const string BorderLight = "border-light";
            public const string BorderDark = "border-dark";
        }
        
        /// <summary>
        /// Duruma göre renk sınıfı döndürür
        /// </summary>
        public static string GetStatusColorClass(string status)
        {
            return status switch
            {
                "Aktif" => BootstrapColors.Success,
                "Pasif" => BootstrapColors.Danger,
                "Beklemede" => BootstrapColors.Warning,
                "Ödendi" => BootstrapColors.Success,
                "Kısmi Ödendi" => BootstrapColors.Warning,
                "Ödenmedi" => BootstrapColors.Danger,
                "Tamamlandı" => BootstrapColors.Success,
                "İşleniyor" => BootstrapColors.Info,
                "İptal Edildi" => BootstrapColors.Danger,
                "Stokta" => BootstrapColors.Success,
                "Kritik Seviye" => BootstrapColors.Warning,
                "Stokta Yok" => BootstrapColors.Danger,
                _ => BootstrapColors.Secondary
            };
        }
        
        /// <summary>
        /// Miktar değerine göre renk sınıfı döndürür
        /// </summary>
        public static string GetNumberColorClass(decimal number)
        {
            if (number > 0)
                return BootstrapColors.TextSuccess;
            if (number < 0)
                return BootstrapColors.TextDanger;
            
            return BootstrapColors.TextSecondary;
        }
        
        /// <summary>
        /// Kategori ve durum etiketleri için renk sınıfları
        /// </summary>
        public static readonly Dictionary<string, string> CategoryColors = new Dictionary<string, string>
        {
            { "Satış", BootstrapColors.Primary },
            { "Alış", BootstrapColors.Success },
            { "Giriş", BootstrapColors.Info },
            { "Çıkış", BootstrapColors.Warning },
            { "İade", BootstrapColors.Danger },
            { "Transfer", BootstrapColors.Secondary }
        };
    }
} 