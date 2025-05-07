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
        Task<bool> LogBilgi(string baslik, string detay, string kullanici);
        Task<bool> LogUyari(string baslik, string detay, string kullanici);
        Task<bool> LogHata(string baslik, string detay, string kullanici);
        
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
        Task<List<MSistemLog>> GetCariLogsAsync(string cariId);
        
        Task ClearLogsAsync();
        Task<bool> LogSilAsync(int id);
        Task<bool> DeleteLogAsync(int logId);
        
        Task<bool> LogInfoAsync(string message, string detail = null);
        Task<bool> LogWarningAsync(string message, string detail = null);
        Task<bool> LogErrorAsync(string message, Exception ex);
        Task<bool> LogErrorAsync(string operation, string message);
        Task LogErrorAsync(string operation, string? stackTrace, Exception ex);
        
        Task CariOlusturmaLogOlustur(string cariID, string cariAdi, string aciklama);
        Task CariGuncellemeLogOlustur(string cariID, string cariAdi, string aciklama);
        Task CariSilmeLogOlustur(string cariID, string cariAdi, string aciklama);
        Task CariPasifleştirmeLogOlustur(string cariID, string cariAdi, string aciklama);
        Task CariAktifleştirmeLogOlustur(string cariID, string cariAdi, string aciklama);
        Task CariHareketEklemeLogOlustur(string cariID, string ad, string hareketTuru, decimal tutar, string aciklama);
        Task UrunOlusturmaLogOlustur(string urunID, string urunAdi, string aciklama);
        Task UrunGuncellemeLogOlustur(string urunID, string urunAdi, string aciklama);
        Task UrunSilmeLogOlustur(string urunID, string urunAdi, string aciklama);
        Task UrunPasifleştirmeLogOlustur(string urunID, string urunAdi);
        Task UrunAktifleştirmeLogOlustur(string urunID, string urunAdi);
        Task StokGirisLogOlustur(string stokHareketID, string urunID, string urunAdi, decimal miktar, string aciklama);
        Task StokCikisLogOlustur(string stokHareketID, string urunID, string urunAdi, decimal miktar, string aciklama);
        Task FaturaOlusturmaLogOlustur(string faturaID, string faturaNo, string aciklama);
        Task FaturaGuncellemeLogOlustur(string faturaID, string faturaNo, string aciklama);
        Task FaturaSilmeLogOlustur(string faturaID, string faturaNo, string aciklama);
        
        Task<List<MSistemLog>> GetLogsByTarihAsync(DateTime baslangicTarihi, DateTime bitisTarihi);
        Task<List<MSistemLog>> GetLogsByIslemTuruAsync(string islemTuru);
        Task<List<MSistemLog>> GetLogsByIslemTuruVeTarihAsync(string islemTuru, DateTime baslangicTarihi, DateTime bitisTarihi);
        Task<List<MSistemLog>> GetLogsByKullaniciVeTarihAsync(string kullaniciAdi, DateTime baslangicTarihi, DateTime bitisTarihi);
        
        Task<bool> AddLogAsync(string tableName, string operation, string details, string? entityId = null);
        Task<ESistemLog> DetayliLogEkleAsync(string mesaj, MuhasebeStokWebApp.Enums.LogTuru logTuru, string tabloAdi, string kayitAdi, string? kayitID, string detay = null);
        
        string GetBrowserInfo(string userAgent);
        string GetOSInfo(string userAgent);
        Task<bool> LogOlustur(string mesaj, MuhasebeStokWebApp.Enums.LogTuru logTuru, string tabloAdi, string kayitAdi, string? kayitID, string kullaniciAdi = null, bool basarili = true, string kategori = null, string aciklama = null);
        Task OturumAcmaLogEkle(string kullaniciID, string kullaniciAdi);
        
        Task BirimOlusturmaLogOlustur(string birimID, string birimAdi);
        Task BirimGuncellemeLogOlustur(string birimID, string birimAdi);
        Task BirimSilmeLogOlustur(string birimID, string birimAdi);
        Task BirimDurumDegisikligiLogOlustur(string birimID, string birimAdi, bool yeniDurum);
        
        Task KategoriOlusturmaLogOlustur(string kategoriID, string kategoriAdi);
        Task KategoriGuncellemeLogOlustur(string kategoriID, string kategoriAdi);
        Task KategoriSilmeLogOlustur(string kategoriID, string kategoriAdi);
        Task KategoriDurumDegisikligiLogOlustur(string kategoriID, string kategoriAdi, bool yeniDurum);
        
        Task DepoOlusturmaLogOlustur(string depoID, string depoAdi);
        Task DepoGuncellemeLogOlustur(string depoID, string depoAdi);
        Task DepoSilmeLogOlustur(string depoID, string depoAdi);
        Task DepoDurumDegisikligiLogOlustur(string depoID, string depoAdi, bool yeniDurum);
        
        Task StokKritikSeviyeLogOlustur(string urunID, string urunAdi, decimal miktar, decimal kritikSeviye);
        Task SistemAyarlariDegisiklikLogOlustur(string ayarAdi, string eskiDeger, string yeniDeger);
    }
} 