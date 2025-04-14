using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using MuhasebeStokWebApp.Data;
using MuhasebeStokWebApp.Data.Entities.ParaBirimiModulu;
using System.Net.Http;
using System.Text.Json;

namespace MuhasebeStokWebApp.Services.ParaBirimiModulu
{
    /// <summary>
    /// Para birimi ve kur işlemleri için servis implementasyonu
    /// </summary>
    public class ParaBirimiService : IParaBirimiService
    {
        private readonly ApplicationDbContext _context;
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly string _apiKey = "645b5bebcab7cef56e1b609c";
        private readonly string _apiBaseUrl = "https://v6.exchangerate-api.com/v6/";

        public ParaBirimiService(ApplicationDbContext context, IHttpClientFactory httpClientFactory)
        {
            _context = context;
            _httpClientFactory = httpClientFactory;
        }

        #region Para Birimi İşlemleri
        /// <summary>
        /// Tüm para birimlerini döndürür
        /// </summary>
        public async Task<List<ParaBirimi>> GetAllParaBirimleriAsync(bool aktiflerOnly = true)
        {
            var query = _context.ParaBirimleri
                .Where(p => !p.Silindi);
                
            if (aktiflerOnly)
                query = query.Where(p => p.Aktif);
                
            return await query
                .OrderBy(p => p.Sira)
                .ThenBy(p => p.Kod)
                .ToListAsync();
        }
        
        /// <summary>
        /// Tüm para birimlerini IParaBirimiService.GetAllAsync için döndürür
        /// </summary>
        public async Task<IEnumerable<ParaBirimi>> GetAllAsync()
        {
            return await GetAllParaBirimleriAsync(false);
        }
        
        /// <summary>
        /// ID'ye göre para birimi döndürür
        /// </summary>
        public async Task<ParaBirimi> GetParaBirimiByIdAsync(Guid paraBirimiId)
        {
            return await _context.ParaBirimleri
                .FirstOrDefaultAsync(p => p.ParaBirimiID == paraBirimiId && !p.Silindi);
        }
        
        /// <summary>
        /// Koda göre para birimi döndürür
        /// </summary>
        public async Task<ParaBirimi> GetParaBirimiByKodAsync(string kod)
        {
            return await _context.ParaBirimleri
                .FirstOrDefaultAsync(p => p.Kod == kod && !p.Silindi);
        }
        
        /// <summary>
        /// Para birimi ekler
        /// </summary>
        public async Task<ParaBirimi> AddParaBirimiAsync(ParaBirimi paraBirimi)
        {
            if (paraBirimi.AnaParaBirimiMi)
            {
                // Diğer ana para birimlerini deaktive et
                var anaParaBirimleri = await _context.ParaBirimleri
                    .Where(p => p.AnaParaBirimiMi && !p.Silindi)
                    .ToListAsync();
                    
                foreach (var anaParaBirimi in anaParaBirimleri)
                {
                    anaParaBirimi.AnaParaBirimiMi = false;
                    _context.ParaBirimleri.Update(anaParaBirimi);
                }
            }
            
            await _context.ParaBirimleri.AddAsync(paraBirimi);
            await _context.SaveChangesAsync();
            
            return paraBirimi;
        }
        
        /// <summary>
        /// Para birimi günceller
        /// </summary>
        public async Task<ParaBirimi> UpdateParaBirimiAsync(ParaBirimi paraBirimi)
        {
            if (paraBirimi.AnaParaBirimiMi)
            {
                // Diğer ana para birimlerini deaktive et
                var anaParaBirimleri = await _context.ParaBirimleri
                    .Where(p => p.ParaBirimiID != paraBirimi.ParaBirimiID && p.AnaParaBirimiMi && !p.Silindi)
                    .ToListAsync();
                    
                foreach (var anaParaBirimi in anaParaBirimleri)
                {
                    anaParaBirimi.AnaParaBirimiMi = false;
                    _context.ParaBirimleri.Update(anaParaBirimi);
                }
            }
            
            _context.ParaBirimleri.Update(paraBirimi);
            await _context.SaveChangesAsync();
            
            return paraBirimi;
        }
        
        /// <summary>
        /// Para birimi siler
        /// </summary>
        public async Task<bool> DeleteParaBirimiAsync(Guid paraBirimiId)
        {
            var paraBirimi = await GetParaBirimiByIdAsync(paraBirimiId);
            
            if (paraBirimi == null)
                return false;
                
            // İlişkili kayıtları kontrol et
            bool hasRelatedRecords = await HasRelatedRecordsAsync(paraBirimiId);
            
            if (hasRelatedRecords)
                return false;
                
            // Soft delete
            paraBirimi.Silindi = true;
            _context.ParaBirimleri.Update(paraBirimi);
            await _context.SaveChangesAsync();
            
            return true;
        }
        
        /// <summary>
        /// Ana para birimi kodunu döndürür
        /// </summary>
        public async Task<string> GetAnaParaBirimiKoduAsync()
        {
            var anaParaBirimi = await GetAnaParaBirimiAsync();
            return anaParaBirimi?.Kod ?? "TRY";
        }
        
        /// <summary>
        /// Ana para birimini döndürür
        /// </summary>
        public async Task<ParaBirimi> GetAnaParaBirimiAsync()
        {
            return await _context.ParaBirimleri
                .FirstOrDefaultAsync(p => p.AnaParaBirimiMi && !p.Silindi);
        }
        
        /// <summary>
        /// Ana para birimini değiştirir
        /// </summary>
        public async Task<bool> SetAnaParaBirimiAsync(Guid paraBirimiId)
        {
            // Tüm para birimlerini al
            var paraBirimleri = await _context.ParaBirimleri
                .Where(p => !p.Silindi)
                .ToListAsync();
                
            // Hepsinin ana para birimi özelliğini kapat
            foreach (var p in paraBirimleri)
            {
                p.AnaParaBirimiMi = false;
                _context.Update(p);
            }
            
            // Seçilen para birimini ana para birimi yap
            var yeniAnaParaBirimi = paraBirimleri.FirstOrDefault(p => p.ParaBirimiID == paraBirimiId);
            
            if (yeniAnaParaBirimi == null)
                return false;
                
            yeniAnaParaBirimi.AnaParaBirimiMi = true;
            _context.Update(yeniAnaParaBirimi);
            
            await _context.SaveChangesAsync();
            
            return true;
        }
        
        /// <summary>
        /// Para biriminin ilişkili kayıtları var mı kontrol eder
        /// </summary>
        private async Task<bool> HasRelatedRecordsAsync(Guid paraBirimiId)
        {
            // Kur değerleri ilişkisi kontrol et
            if (await _context.KurDegerleri.AnyAsync(k => k.ParaBirimiID == paraBirimiId && !k.Silindi))
                return true;
                
            // Para birimi ilişkileri kontrol et
            if (await _context.Set<ParaBirimiIliski>().AnyAsync(i => 
                (i.KaynakParaBirimiID == paraBirimiId || i.HedefParaBirimiID == paraBirimiId) && !i.Silindi))
                return true;
                
            return false;
        }
        #endregion
        
        #region Kur Değeri İşlemleri
        /// <summary>
        /// Tüm kur değerlerini döndürür
        /// </summary>
        public async Task<List<KurDegeri>> GetAllKurDegerleriAsync(DateTime? tarih = null)
        {
            var query = _context.KurDegerleri
                .Include(k => k.ParaBirimi)
                .Where(k => !k.Silindi);
                
            if (tarih.HasValue)
                query = query.Where(k => k.Tarih.Date == tarih.Value.Date);
                
            return await query
                .OrderByDescending(k => k.Tarih)
                .ThenBy(k => k.ParaBirimi.Kod)
                .ToListAsync();
        }
        
        /// <summary>
        /// Para birimine göre kur değerlerini döndürür
        /// </summary>
        public async Task<List<KurDegeri>> GetKurDegerleriByParaBirimiAsync(Guid paraBirimiId, DateTime? baslangicTarihi = null, DateTime? bitisTarihi = null)
        {
            var query = _context.KurDegerleri
                .Include(k => k.ParaBirimi)
                .Where(k => k.ParaBirimiID == paraBirimiId && !k.Silindi);
                
            if (baslangicTarihi.HasValue)
                query = query.Where(k => k.Tarih.Date >= baslangicTarihi.Value.Date);
                
            if (bitisTarihi.HasValue)
                query = query.Where(k => k.Tarih.Date <= bitisTarihi.Value.Date);
                
            return await query
                .OrderByDescending(k => k.Tarih)
                .ToListAsync();
        }
        
        /// <summary>
        /// Son kur değerini döndürür
        /// </summary>
        public async Task<KurDegeri> GetSonKurDegeriByParaBirimiAsync(Guid paraBirimiId)
        {
            return await _context.KurDegerleri
                .Include(k => k.ParaBirimi)
                .Where(k => k.ParaBirimiID == paraBirimiId && !k.Silindi)
                .OrderByDescending(k => k.Tarih)
                .FirstOrDefaultAsync();
        }
        
        /// <summary>
        /// Kur değeri ekler
        /// </summary>
        public async Task<KurDegeri> AddKurDegeriAsync(KurDegeri kurDegeri)
        {
            // Aynı gün, aynı para birimi için kur kontrolü
            var existingKur = await _context.KurDegerleri
                .FirstOrDefaultAsync(k => 
                    k.ParaBirimiID == kurDegeri.ParaBirimiID && 
                    k.Tarih.Date == kurDegeri.Tarih.Date && 
                    !k.Silindi);
                    
            if (existingKur != null)
            {
                // Mevcut kaydı güncelle
                existingKur.Alis = kurDegeri.Alis;
                existingKur.Satis = kurDegeri.Satis;
                existingKur.EfektifAlis = kurDegeri.EfektifAlis;
                existingKur.EfektifSatis = kurDegeri.EfektifSatis;
                existingKur.Aktif = kurDegeri.Aktif;
                existingKur.Aciklama = kurDegeri.Aciklama;
                existingKur.GuncellemeTarihi = DateTime.Now;
                existingKur.SonGuncelleyenKullaniciID = kurDegeri.OlusturanKullaniciID;
                
                _context.KurDegerleri.Update(existingKur);
                await _context.SaveChangesAsync();
                
                return existingKur;
            }
            
            await _context.KurDegerleri.AddAsync(kurDegeri);
            await _context.SaveChangesAsync();
            
            return kurDegeri;
        }
        
        /// <summary>
        /// Kur değerlerini toplu olarak ekler
        /// </summary>
        public async Task<List<KurDegeri>> AddKurDegerleriAsync(List<KurDegeri> kurDegerleri)
        {
            foreach (var kurDegeri in kurDegerleri)
            {
                await AddKurDegeriAsync(kurDegeri);
            }
            
            return kurDegerleri;
        }
        
        /// <summary>
        /// Kur değeri günceller
        /// </summary>
        public async Task<KurDegeri> UpdateKurDegeriAsync(KurDegeri kurDegeri)
        {
            _context.KurDegerleri.Update(kurDegeri);
            await _context.SaveChangesAsync();
            
            return kurDegeri;
        }
        
        /// <summary>
        /// Kur değeri siler
        /// </summary>
        public async Task<bool> DeleteKurDegeriAsync(Guid kurDegeriId)
        {
            var kurDegeri = await _context.KurDegerleri
                .FirstOrDefaultAsync(k => k.KurDegeriID == kurDegeriId);
                
            if (kurDegeri == null)
                return false;
                
            // Soft delete
            kurDegeri.Silindi = true;
            _context.KurDegerleri.Update(kurDegeri);
            await _context.SaveChangesAsync();
            
            return true;
        }
        
        /// <summary>
        /// Kur değerlerini harici API'den günceller
        /// </summary>
        public async Task<bool> GuncelleKurDegerleriniFromAPIAsync()
        {
            try
            {
                var httpClient = _httpClientFactory.CreateClient();
                
                // USD'yi baz para birimi olarak kullan - API USD bazlı çalışıyor
                string bazParaBirimiKodu = "USD";
                
                // API endpoint'i oluştur
                string apiEndpoint = $"{_apiBaseUrl}{_apiKey}/latest/{bazParaBirimiKodu}";
                
                // API'den verileri al
                var response = await httpClient.GetAsync(apiEndpoint);
                
                if (!response.IsSuccessStatusCode)
                    return false;
                    
                // JSON yanıtını oku
                var jsonString = await response.Content.ReadAsStringAsync();
                using var jsonDoc = JsonDocument.Parse(jsonString);
                var root = jsonDoc.RootElement;
                
                // Sonuç kontrolü
                if (root.GetProperty("result").GetString() != "success")
                    return false;
                    
                // Kurları al
                var tarih = DateTime.Now;
                var conversionRates = root.GetProperty("conversion_rates");
                
                // Tüm para birimlerini al
                var paraBirimleri = await GetAllParaBirimleriAsync();
                
                // Ana para birimimizi bul - genellikle TRY
                var anaParaBirimi = await GetAnaParaBirimiAsync();
                string anaParaBirimiKodu = anaParaBirimi?.Kod ?? "TRY";

                foreach (var paraBirimi in paraBirimleri)
                {
                    // Para biriminin API'den kurunu al
                    if (conversionRates.TryGetProperty(paraBirimi.Kod, out var rate))
                    {
                        decimal kurDegeri = rate.GetDecimal();

                        // USD ana para birimi ise 1 olarak ayarla (baz para birimi)
                        if (paraBirimi.Kod == bazParaBirimiKodu)
                        {
                            if (paraBirimi.AnaParaBirimiMi)
                            {
                                kurDegeri = 1;
                            }
                            else
                            {
                                // USD ana para birimi değilse, API'den gelen USD/TRY kuruyla işlem yap
                                if (conversionRates.TryGetProperty(anaParaBirimiKodu, out var tryRate))
                                {
                                    kurDegeri = tryRate.GetDecimal();
                                }
                            }
                        }
                        else if (paraBirimi.AnaParaBirimiMi)
                        {
                            // Ana para birimi (TRY) için API'den gelen değeri al
                            kurDegeri = rate.GetDecimal();
                        }
                        else
                        {
                            // Diğer para birimleri için çapraz kur hesapla (TRY baz alınarak)
                            if (conversionRates.TryGetProperty(anaParaBirimiKodu, out var tryRate))
                            {
                                decimal tryDegeri = tryRate.GetDecimal();
                                kurDegeri = rate.GetDecimal() / tryDegeri;
                            }
                        }

                        var kurDegeriEntity = new KurDegeri
                        {
                            KurDegeriID = Guid.NewGuid(),
                            ParaBirimiID = paraBirimi.ParaBirimiID,
                            Tarih = tarih,
                            Alis = kurDegeri,
                            Satis = kurDegeri * 1.02m, // %2 farkla satış kuru
                            EfektifAlis = kurDegeri * 0.98m, // %2 farkla efektif alış
                            EfektifSatis = kurDegeri * 1.03m, // %3 farkla efektif satış
                            Aktif = true,
                            OlusturmaTarihi = DateTime.Now,
                            OlusturanKullaniciID = "Sistem",
                            Aciklama = "Exchange Rate API'den otomatik güncellendi"
                        };
                        
                        await AddKurDegeriAsync(kurDegeriEntity);
                    }
                }
                
                return true;
            }
            catch (Exception ex)
            {
                // Hata logla
                Console.WriteLine($"API hata: {ex.Message}");
                return false;
            }
        }
        #endregion
        
        #region Para Birimi İlişkileri
        /// <summary>
        /// Tüm para birimi ilişkilerini döndürür
        /// </summary>
        public async Task<List<ParaBirimiIliski>> GetAllParaBirimiIliskileriAsync(bool aktiflerOnly = true)
        {
            var query = _context.Set<ParaBirimiIliski>()
                .Include(i => i.KaynakParaBirimi)
                .Include(i => i.HedefParaBirimi)
                .Where(i => !i.Silindi);
                
            if (aktiflerOnly)
                query = query.Where(i => i.Aktif);
                
            return await query
                .OrderBy(i => i.KaynakParaBirimi.Kod)
                .ThenBy(i => i.HedefParaBirimi.Kod)
                .ToListAsync();
        }
        
        /// <summary>
        /// ID'ye göre para birimi ilişkisini döndürür
        /// </summary>
        public async Task<ParaBirimiIliski> GetParaBirimiIliskiByIdAsync(Guid iliskiId)
        {
            return await _context.Set<ParaBirimiIliski>()
                .Include(i => i.KaynakParaBirimi)
                .Include(i => i.HedefParaBirimi)
                .FirstOrDefaultAsync(i => i.ParaBirimiIliskiID == iliskiId && !i.Silindi);
        }
        
        /// <summary>
        /// İki para birimi arasındaki ilişkiyi döndürür
        /// </summary>
        public async Task<ParaBirimiIliski> GetIliskiByParaBirimleriAsync(Guid kaynakId, Guid hedefId)
        {
            return await _context.Set<ParaBirimiIliski>()
                .Include(i => i.KaynakParaBirimi)
                .Include(i => i.HedefParaBirimi)
                .FirstOrDefaultAsync(i => 
                    i.KaynakParaBirimiID == kaynakId && 
                    i.HedefParaBirimiID == hedefId && 
                    !i.Silindi);
        }
        
        /// <summary>
        /// Para birimi ilişkisi ekler
        /// </summary>
        public async Task<ParaBirimiIliski> AddParaBirimiIliskiAsync(ParaBirimiIliski paraBirimiIliski)
        {
            // Aynı ilişki var mı kontrol et
            var existingIliski = await GetIliskiByParaBirimleriAsync(
                paraBirimiIliski.KaynakParaBirimiID, 
                paraBirimiIliski.HedefParaBirimiID);
                
            if (existingIliski != null)
            {
                // Silinmiş ilişkiyi geri aktifleme
                if (existingIliski.Silindi)
                {
                    existingIliski.Silindi = false;
                    existingIliski.Aktif = paraBirimiIliski.Aktif;
                    existingIliski.GuncellemeTarihi = DateTime.Now;
                    existingIliski.SonGuncelleyenKullaniciID = paraBirimiIliski.OlusturanKullaniciID;
                    
                    _context.Update(existingIliski);
                    await _context.SaveChangesAsync();
                    
                    return existingIliski;
                }
                
                return existingIliski;
            }
            
            await _context.AddAsync(paraBirimiIliski);
            await _context.SaveChangesAsync();
            
            return paraBirimiIliski;
        }
        
        /// <summary>
        /// Para birimi ilişkisi günceller
        /// </summary>
        public async Task<ParaBirimiIliski> UpdateParaBirimiIliskiAsync(ParaBirimiIliski paraBirimiIliski)
        {
            _context.Update(paraBirimiIliski);
            await _context.SaveChangesAsync();
            
            return paraBirimiIliski;
        }
        
        /// <summary>
        /// Para birimi ilişkisi siler
        /// </summary>
        public async Task<bool> DeleteParaBirimiIliskiAsync(Guid iliskiId)
        {
            var iliski = await GetParaBirimiIliskiByIdAsync(iliskiId);
            
            if (iliski == null)
                return false;
                
            // Soft delete
            iliski.Silindi = true;
            _context.Update(iliski);
            await _context.SaveChangesAsync();
            
            return true;
        }
        #endregion
        
        #region Hesaplama İşlemleri
        /// <summary>
        /// İki para birimi arasındaki kur değerini hesaplar
        /// </summary>
        public async Task<decimal> HesaplaKurDegeriAsync(Guid kaynakParaBirimiId, Guid hedefParaBirimiId, DateTime? tarih = null)
        {
            if (kaynakParaBirimiId == hedefParaBirimiId)
                return 1m;
                
            var tarihValue = tarih?.Date ?? DateTime.Today;
            
            // Doğrudan ilişki var mı kontrol et
            var kaynakKur = await _context.KurDegerleri
                .Where(k => k.ParaBirimiID == kaynakParaBirimiId && 
                       k.Tarih.Date == tarihValue && 
                       k.Aktif && 
                       !k.Silindi)
                .OrderByDescending(k => k.Tarih)
                .FirstOrDefaultAsync();
                
            var hedefKur = await _context.KurDegerleri
                .Where(k => k.ParaBirimiID == hedefParaBirimiId && 
                       k.Tarih.Date == tarihValue && 
                       k.Aktif && 
                       !k.Silindi)
                .OrderByDescending(k => k.Tarih)
                .FirstOrDefaultAsync();
                
            if (kaynakKur != null && hedefKur != null)
            {
                return hedefKur.Alis / kaynakKur.Alis;
            }
            
            // Doğrudan ilişki yoksa ana para birimi üzerinden hesapla
            var anaParaBirimi = await GetAnaParaBirimiAsync();
            
            if (anaParaBirimi == null)
                throw new InvalidOperationException("Ana para birimi bulunamadı.");
                
            var kaynakToAna = await HesaplaKurDegeriAsync(kaynakParaBirimiId, anaParaBirimi.ParaBirimiID, tarih);
            var anaToHedef = await HesaplaKurDegeriAsync(anaParaBirimi.ParaBirimiID, hedefParaBirimiId, tarih);
            
            return kaynakToAna * anaToHedef;
        }
        
        /// <summary>
        /// İki para birimi arasında dönüşüm yapar
        /// </summary>
        public async Task<decimal> CevirmeTutarAsync(decimal tutar, Guid kaynakParaBirimiId, Guid hedefParaBirimiId, DateTime? tarih = null)
        {
            if (kaynakParaBirimiId == hedefParaBirimiId)
                return tutar;
                
            var kurDegeri = await HesaplaKurDegeriAsync(kaynakParaBirimiId, hedefParaBirimiId, tarih);
            return tutar * kurDegeri;
        }
        #endregion
    }
} 