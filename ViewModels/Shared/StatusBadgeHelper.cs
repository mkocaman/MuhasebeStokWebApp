using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace MuhasebeStokWebApp.ViewModels.Shared
{
    /// <summary>
    /// Durum badge'leri oluşturmak için yardımcı sınıf
    /// </summary>
    public static class StatusBadgeHelper
    {
        /// <summary>
        /// Aktif/Pasif durumu için badge oluşturur
        /// </summary>
        /// <param name="aktifMi">Aktif durumda mı?</param>
        /// <returns>HTML içeriği</returns>
        public static IHtmlContent AktifPasifBadge(bool aktifMi)
        {
            string badgeClass = aktifMi ? "bg-success" : "bg-danger";
            string badgeText = aktifMi ? "Aktif" : "Pasif";
            
            return new HtmlString($"<span class=\"badge {badgeClass}\">{badgeText}</span>");
        }
        
        /// <summary>
        /// Ödeme durumu için badge oluşturur
        /// </summary>
        /// <param name="odemeDurumu">Ödeme durumu metni</param>
        /// <returns>HTML içeriği</returns>
        public static IHtmlContent OdemeDurumuBadge(string odemeDurumu)
        {
            string badgeClass = odemeDurumu switch
            {
                "Ödendi" => "bg-success",
                "Kısmi Ödendi" => "bg-warning",
                _ => "bg-danger"
            };
            
            return new HtmlString($"<span class=\"badge {badgeClass}\">{odemeDurumu}</span>");
        }
        
        /// <summary>
        /// İrsaliye türü için badge oluşturur
        /// </summary>
        /// <param name="irsaliyeTuru">İrsaliye türü</param>
        /// <returns>HTML içeriği</returns>
        public static IHtmlContent IrsaliyeTuruBadge(string irsaliyeTuru)
        {
            string badgeClass = irsaliyeTuru switch
            {
                "Giriş" => "bg-primary",
                "Çıkış" => "bg-success",
                "Transfer" => "bg-info",
                "İade" => "bg-warning",
                _ => "bg-secondary"
            };
            
            return new HtmlString($"<span class=\"badge {badgeClass}\">{irsaliyeTuru}</span>");
        }
        
        /// <summary>
        /// Stok durumu için badge oluşturur
        /// </summary>
        /// <param name="miktar">Stok miktarı</param>
        /// <param name="kritikStokSeviyesi">Kritik stok seviyesi</param>
        /// <returns>HTML içeriği</returns>
        public static IHtmlContent StokDurumuBadge(decimal miktar, decimal kritikStokSeviyesi = 10)
        {
            string badgeClass;
            string badgeText;
            
            if (miktar <= 0)
            {
                badgeClass = "bg-danger";
                badgeText = "Stokta Yok";
            }
            else if (miktar <= kritikStokSeviyesi)
            {
                badgeClass = "bg-warning";
                badgeText = "Kritik Seviye";
            }
            else
            {
                badgeClass = "bg-success";
                badgeText = "Stokta";
            }
            
            return new HtmlString($"<span class=\"badge {badgeClass}\">{badgeText}</span>");
        }
        
        /// <summary>
        /// Silindi durumu için badge oluşturur
        /// </summary>
        /// <param name="silindi">Silindi mi?</param>
        /// <returns>HTML içeriği</returns>
        public static IHtmlContent SilindiBadge(bool silindi)
        {
            if (!silindi)
                return new HtmlString("");
                
            return new HtmlString("<span class=\"badge bg-danger\">Silindi</span>");
        }
        
        /// <summary>
        /// Bakiye durumu için badge oluşturur
        /// </summary>
        /// <param name="bakiye">Bakiye miktarı</param>
        /// <param name="paraBirimi">Para birimi sembolü</param>
        /// <returns>HTML içeriği</returns>
        public static IHtmlContent BakiyeBadge(decimal bakiye, string paraBirimi = "₺")
        {
            string badgeClass = bakiye >= 0 ? "bg-success" : "bg-danger";
            string formattedBakiye = bakiye.ToString("N2");
            
            return new HtmlString($"<span class=\"badge {badgeClass}\">{formattedBakiye} {paraBirimi}</span>");
        }
        
        /// <summary>
        /// FIFO kaydı durumu için badge oluşturur
        /// </summary>
        /// <param name="kalanMiktar">Kalan miktar</param>
        /// <returns>HTML içeriği</returns>
        public static IHtmlContent FifoDurumBadge(decimal kalanMiktar)
        {
            string badgeClass = kalanMiktar > 0 ? "bg-success" : "bg-secondary";
            string badgeText = kalanMiktar > 0 ? "Aktif" : "Tüketildi";
            
            return new HtmlString($"<span class=\"badge {badgeClass}\">{badgeText}</span>");
        }
    }
} 