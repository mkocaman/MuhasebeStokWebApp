using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using MuhasebeStokWebApp.Models;

namespace MuhasebeStokWebApp.Services
{
    public interface ILogService
    {
        /// <summary>
        /// Sisteme log ekler.
        /// </summary>
        Task LogEkleAsync(string mesaj, LogTuru logTuru, string detay = null);
        
        /// <summary>
        /// Belirlenen tarih aralığındaki logları getirir.
        /// </summary>
        Task<IEnumerable<SistemLog>> GetLogsAsync(DateTime? baslangicTarihi = null, DateTime? bitisTarihi = null);
        
        /// <summary>
        /// Belirli bir kullanıcının loglarını getirir.
        /// </summary>
        Task<IEnumerable<SistemLog>> GetLogsByKullaniciAsync(string kullaniciAdi, DateTime? baslangicTarihi = null, DateTime? bitisTarihi = null);
        
        /// <summary>
        /// Belirli tipteki logları getirir.
        /// </summary>
        Task<IEnumerable<SistemLog>> GetLogsByTurAsync(LogTuru logTuru, DateTime? baslangicTarihi = null, DateTime? bitisTarihi = null);
        
        /// <summary>
        /// Tüm logları temizler.
        /// </summary>
        Task ClearLogsAsync();
        
        /// <summary>
        /// Belirli bir log kaydını siler.
        /// </summary>
        Task DeleteLogAsync(int logId);
        
        /// <summary>
        /// Belirli bir cari için logları getirir.
        /// </summary>
        Task<IEnumerable<SistemLog>> GetCariLogsAsync(Guid cariId);
        
        /// <summary>
        /// Cari işlemleri için log oluşturur.
        /// </summary>
        Task CariOlusturmaLogOlustur(Guid cariID, string cariAdi, string aciklama);
        Task CariGuncellemeLogOlustur(Guid cariID, string cariAdi, string aciklama);
        Task CariSilmeLogOlustur(Guid cariID, string cariAdi, string aciklama);
        Task CariPasifleştirmeLogOlustur(Guid cariID, string cariAdi, string aciklama);
        Task CariAktifleştirmeLogOlustur(Guid cariID, string cariAdi, string aciklama);
        Task CariHareketEklemeLogOlustur(Guid cariID, string cariAdi, string hareketTuru, decimal tutar, string aciklama);
        
        /// <summary>
        /// Ürün işlemleri için log oluşturur.
        /// </summary>
        Task UrunOlusturmaLogOlustur(Guid urunID, string urunAdi, string aciklama);
        Task UrunGuncellemeLogOlustur(Guid urunID, string urunAdi, string aciklama);
        Task UrunSilmeLogOlustur(Guid urunID, string urunAdi, string aciklama);
        
        /// <summary>
        /// Stok işlemleri için log oluşturur.
        /// </summary>
        Task StokGirisLogOlustur(Guid stokHareketID, Guid urunID, string urunAdi, decimal miktar, string aciklama);
        Task StokCikisLogOlustur(Guid stokHareketID, Guid urunID, string urunAdi, decimal miktar, string aciklama);
        
        /// <summary>
        /// Hata logları oluşturur
        /// </summary>
        Task LogErrorAsync(string category, Exception ex);
        
        /// <summary>
        /// Bilgi logları oluşturur
        /// </summary>
        Task LogInfoAsync(string category, string message);
        
        /// <summary>
        /// Uyarı logları oluşturur
        /// </summary>
        Task LogWarningAsync(string category, string message);
    }
} 