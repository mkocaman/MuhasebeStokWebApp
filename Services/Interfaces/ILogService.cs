using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using MuhasebeStokWebApp.Models;
using ESistemLog = MuhasebeStokWebApp.Data.Entities.SistemLog;
using MSistemLog = MuhasebeStokWebApp.Models.SistemLog;

namespace MuhasebeStokWebApp.Services.Interfaces
{
    public interface ILogService
    {
        Task<bool> AddLogAsync(string logLevel, string message, string source);
        void LogBilgi(string baslik, string detay, string kullanici);
        void LogUyari(string baslik, string detay, string kullanici);
        void LogHata(string baslik, string detay, string kullanici);
        
        Task<bool> LogEkleAsync(string mesaj, MuhasebeStokWebApp.Enums.LogTuru logTuru, string detay = null);
        Task<bool> Log(string mesaj, MuhasebeStokWebApp.Enums.LogTuru logTuru, string detay = null);
        
        Task<List<ESistemLog>> GetLogsAsync(
            DateTime? startDate = null,
            DateTime? endDate = null,
            string searchTerm = null,
            string logTuru = null,
            int page = 1,
            int pageSize = 20);
            
        Task<List<MSistemLog>> GetAllLogsAsync();
        Task<List<MSistemLog>> GetLogsByKullaniciAsync(string kullaniciAdi, DateTime? baslangicTarihi = null, DateTime? bitisTarihi = null);
        Task<List<MSistemLog>> GetLogsByTurAsync(MuhasebeStokWebApp.Enums.LogTuru logTuru, DateTime? baslangicTarihi = null, DateTime? bitisTarihi = null);
        Task<List<MSistemLog>> GetCariLogsAsync(Guid cariId);
        
        Task ClearLogsAsync();
        Task<bool> LogSilAsync(int id);
        Task<bool> DeleteLogAsync(int logId);
        
        Task<bool> LogInfoAsync(string message, string detail = null);
        Task<bool> LogWarningAsync(string message, string detail = null);
        Task<bool> LogErrorAsync(string message, Exception ex);
        Task<bool> LogErrorAsync(string operation, string message);
        Task LogErrorAsync(string operation, string stackTrace, Exception ex);
        
        Task CariOlusturmaLogOlustur(Guid cariID, string ad, string aciklama);
        Task CariGuncellemeLogOlustur(Guid cariID, string ad, string aciklama);
        Task CariSilmeLogOlustur(Guid cariID, string ad, string aciklama);
        Task CariPasifleştirmeLogOlustur(Guid cariID, string ad, string aciklama);
        Task CariAktifleştirmeLogOlustur(Guid cariID, string ad, string aciklama);
        Task CariHareketEklemeLogOlustur(Guid cariID, string ad, string hareketTuru, decimal tutar, string aciklama);
        Task UrunOlusturmaLogOlustur(Guid urunID, string urunAdi, string aciklama);
        Task UrunGuncellemeLogOlustur(Guid urunID, string urunAdi, string aciklama);
        Task UrunSilmeLogOlustur(Guid urunID, string urunAdi, string aciklama);
        Task StokGirisLogOlustur(Guid stokHareketID, Guid urunID, string urunAdi, decimal miktar, string aciklama);
        Task StokCikisLogOlustur(Guid stokHareketID, Guid urunID, string urunAdi, decimal miktar, string aciklama);
        Task FaturaOlusturmaLogOlustur(Guid faturaID, string faturaNo, string aciklama);
        Task FaturaGuncellemeLogOlustur(Guid faturaID, string faturaNo, string aciklama);
        Task FaturaSilmeLogOlustur(Guid faturaID, string faturaNo, string aciklama);
        
        Task<List<MSistemLog>> GetLogsByTarihAsync(DateTime baslangicTarihi, DateTime bitisTarihi);
        Task<List<MSistemLog>> GetLogsByIslemTuruAsync(string islemTuru);
        Task<List<MSistemLog>> GetLogsByIslemTuruVeTarihAsync(string islemTuru, DateTime baslangicTarihi, DateTime bitisTarihi);
        Task<List<MSistemLog>> GetLogsByKullaniciVeTarihAsync(string kullaniciAdi, DateTime baslangicTarihi, DateTime bitisTarihi);
        
        Task<bool> AddLogAsync(string tableName, string operation, string details, string entityId = null);
        Task<ESistemLog> DetayliLogEkleAsync(string mesaj, MuhasebeStokWebApp.Enums.LogTuru logTuru, string tabloAdi, string kayitAdi, Guid? kayitID, string detay = null);
        
        string GetBrowserInfo(string userAgent);
        string GetOSInfo(string userAgent);
        Task<bool> LogOlustur(string mesaj, MuhasebeStokWebApp.Enums.LogTuru logTuru, string tabloAdi, string kayitAdi, Guid? kayitID, string kullaniciAdi = null, bool basarili = true, string kategori = null);
        Task OturumAcmaLogEkle(string kullaniciID, string kullaniciAdi);
    }
} 