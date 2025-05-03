using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Mvc.Rendering;
using MuhasebeStokWebApp.ViewModels.Shared;

namespace MuhasebeStokWebApp.ViewModels
{
    /// <summary>
    /// IHtmlHelper için StatusBadgeHelper extension'ları
    /// </summary>
    public static class StatusBadgeExtensions
    {
        /// <summary>
        /// Aktif/Pasif durumu için badge oluşturur
        /// </summary>
        public static IHtmlContent AktifPasifBadge(this IHtmlHelper htmlHelper, bool aktifMi)
        {
            return StatusBadgeHelper.AktifPasifBadge(aktifMi);
        }
        
        /// <summary>
        /// Ödeme durumu için badge oluşturur
        /// </summary>
        public static IHtmlContent OdemeDurumuBadge(this IHtmlHelper htmlHelper, string odemeDurumu)
        {
            return StatusBadgeHelper.OdemeDurumuBadge(odemeDurumu);
        }
        
        /// <summary>
        /// İrsaliye türü için badge oluşturur
        /// </summary>
        public static IHtmlContent IrsaliyeTuruBadge(this IHtmlHelper htmlHelper, string irsaliyeTuru)
        {
            return StatusBadgeHelper.IrsaliyeTuruBadge(irsaliyeTuru);
        }
        
        /// <summary>
        /// Stok durumu için badge oluşturur
        /// </summary>
        public static IHtmlContent StokDurumuBadge(this IHtmlHelper htmlHelper, decimal miktar, decimal kritikStokSeviyesi = 10)
        {
            return StatusBadgeHelper.StokDurumuBadge(miktar, kritikStokSeviyesi);
        }
        
        /// <summary>
        /// Silindi durumu için badge oluşturur
        /// </summary>
        public static IHtmlContent SilindiBadge(this IHtmlHelper htmlHelper, bool silindi)
        {
            return StatusBadgeHelper.SilindiBadge(silindi);
        }
        
        /// <summary>
        /// Bakiye durumu için badge oluşturur
        /// </summary>
        public static IHtmlContent BakiyeBadge(this IHtmlHelper htmlHelper, decimal bakiye, string paraBirimi = "₺")
        {
            return StatusBadgeHelper.BakiyeBadge(bakiye, paraBirimi);
        }
        
        /// <summary>
        /// FIFO kaydı durumu için badge oluşturur
        /// </summary>
        public static IHtmlContent FifoDurumBadge(this IHtmlHelper htmlHelper, decimal kalanMiktar)
        {
            return StatusBadgeHelper.FifoDurumBadge(kalanMiktar);
        }
    }
} 