using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using MuhasebeStokWebApp.Models;

namespace MuhasebeStokWebApp.Services
{
    public interface ILogService
    {
        /// <summary>
        /// Sisteme yeni bir log kaydı ekler.
        /// </summary>
        Task<bool> LogEkleAsync(string mesaj, MuhasebeStokWebApp.Enums.LogTuru logTuru, string detay = null);
        
        /// <summary>
        /// Kısa adlandırma için LogEkleAsync metodunun farklı bir isimle kullanımı
        /// </summary>
        Task<bool> Log(string mesaj, MuhasebeStokWebApp.Enums.LogTuru logTuru, string detay = null);
        
        /// <summary>
        /// Tüm log kayıtlarını getirir.
        /// </summary>
        Task<List<MuhasebeStokWebApp.Data.Entities.SistemLog>> GetLogsAsync(
            DateTime? startDate = null,
            DateTime? endDate = null,
            string searchTerm = null,
            string logTuru = null,
            int page = 1,
            int pageSize = 20);
        
        /// <summary>
        /// Belirli bir kullanıcıya ait log kayıtlarını getirir.
        /// </summary>
        Task<List<SistemLog>> GetLogsByKullaniciAsync(string kullaniciAdi, DateTime? baslangicTarihi = null, DateTime? bitisTarihi = null);
        
        /// <summary>
        /// Belirli bir log türüne ait kayıtları getirir.
        /// </summary>
        Task<List<SistemLog>> GetLogsByTurAsync(MuhasebeStokWebApp.Enums.LogTuru logTuru, DateTime? baslangicTarihi = null, DateTime? bitisTarihi = null);
        
        Task<List<SistemLog>> GetAllLogsAsync();
        Task<List<SistemLog>> GetCariLogsAsync(Guid cariId);
        Task ClearLogsAsync();
        Task<bool> LogSilAsync(int id);
        Task CariOlusturmaLogOlustur(Guid cariID, string cariAdi, string aciklama);
        Task CariGuncellemeLogOlustur(Guid cariID, string cariAdi, string aciklama);
        Task CariSilmeLogOlustur(Guid cariID, string cariAdi, string aciklama);
        Task CariPasifleştirmeLogOlustur(Guid cariID, string cariAdi, string aciklama);
        Task CariAktifleştirmeLogOlustur(Guid cariID, string cariAdi, string aciklama);
        Task CariHareketEklemeLogOlustur(Guid cariID, string cariAdi, string hareketTuru, decimal tutar, string aciklama);
        Task UrunOlusturmaLogOlustur(Guid urunID, string urunAdi, string aciklama);
        Task UrunGuncellemeLogOlustur(Guid urunID, string urunAdi, string aciklama);
        Task UrunSilmeLogOlustur(Guid urunID, string urunAdi, string aciklama);
        Task StokGirisLogOlustur(Guid stokHareketID, Guid urunID, string urunAdi, decimal miktar, string aciklama);
        Task StokCikisLogOlustur(Guid stokHareketID, Guid urunID, string urunAdi, decimal miktar, string aciklama);
        Task<bool> LogInfoAsync(string message, string detail = null);
        Task<bool> LogWarningAsync(string message, string detail = null);
        Task<bool> LogErrorAsync(string message, string detail = null);
        Task<bool> LogErrorAsync(string message, Exception ex);
        Task FaturaOlusturmaLogOlustur(Guid faturaID, string faturaNo, string aciklama);
        Task<List<SistemLog>> GetLogsByTarihAsync(DateTime baslangicTarihi, DateTime bitisTarihi);
        Task<List<SistemLog>> GetLogsByIslemTuruAsync(string islemTuru);
        Task<List<SistemLog>> GetLogsByIslemTuruVeTarihAsync(string islemTuru, DateTime baslangicTarihi, DateTime bitisTarihi);
        Task<List<SistemLog>> GetLogsByKullaniciVeTarihAsync(string kullaniciAdi, DateTime baslangicTarihi, DateTime bitisTarihi);
        Task<bool> DeleteLogAsync(int logId);
        Task FaturaGuncellemeLogOlustur(Guid faturaID, string faturaNo, string aciklama);
        Task FaturaSilmeLogOlustur(Guid faturaID, string faturaNo, string aciklama);
        
        /// <summary>
        /// Genel log ekle metodu. Çeşitli işlemler için log kaydı ekler.
        /// </summary>
        /// <param name="tableName">İşlem yapılan tablo adı</param>
        /// <param name="operation">Yapılan işlem (Ekleme, Güncelleme, Silme vb.)</param>
        /// <param name="details">İşlem detayları</param>
        /// <param name="entityId">İşlem yapılan varlık ID'si (opsiyonel)</param>
        /// <returns>İşlem başarılı ise true, değilse false</returns>
        Task<bool> AddLogAsync(string tableName, string operation, string details, string entityId = null);
        
        string GetBrowserInfo(string userAgent);
        string GetOSInfo(string userAgent);

        Task LogErrorAsync(string operation, string stackTrace, Exception ex);
    }
} 