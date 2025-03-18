using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using System.Xml.Linq;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using MuhasebeStokWebApp.Data;
using MuhasebeStokWebApp.Data.Entities;
using MuhasebeStokWebApp.ViewModels.Kur;

namespace MuhasebeStokWebApp.Services
{
    public class KurService : IKurService
    {
        private readonly ApplicationDbContext _context;
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly ILogger<KurService> _logger;

        public KurService(ApplicationDbContext context, IHttpClientFactory httpClientFactory, ILogger<KurService> logger)
        {
            _context = context;
            _httpClientFactory = httpClientFactory;
            _logger = logger;
        }

        #region Para Birimi İşlemleri

        public async Task<List<ParaBirimi>> GetParaBirimleriAsync()
        {
            return await _context.ParaBirimleri
                .Where(p => p.Aktif)
                .OrderBy(p => p.Kod)
                .ToListAsync();
        }

        public async Task<ParaBirimi> GetParaBirimiByIdAsync(Guid id)
        {
            return await _context.ParaBirimleri
                .FirstOrDefaultAsync(p => p.ParaBirimiID == id);
        }

        public async Task<ParaBirimi> GetParaBirimiByKodAsync(string kod)
        {
            return await _context.ParaBirimleri
                .FirstOrDefaultAsync(p => p.Kod == kod && p.Aktif);
        }

        public async Task<ParaBirimi> AddParaBirimiAsync(ParaBirimi paraBirimi)
        {
            // Aynı kodla kayıt var mı kontrol et
            var existing = await _context.ParaBirimleri
                .FirstOrDefaultAsync(p => p.Kod == paraBirimi.Kod);
                
            if (existing != null)
            {
                throw new Exception($"{paraBirimi.Kod} kodlu para birimi zaten mevcut.");
            }
            
            paraBirimi.ParaBirimiID = Guid.NewGuid();
            
            _context.ParaBirimleri.Add(paraBirimi);
            await _context.SaveChangesAsync();
            
            return paraBirimi;
        }

        public async Task<ParaBirimi> UpdateParaBirimiAsync(ParaBirimi paraBirimi)
        {
            var existing = await _context.ParaBirimleri
                .FirstOrDefaultAsync(p => p.ParaBirimiID == paraBirimi.ParaBirimiID);
                
            if (existing == null)
            {
                throw new Exception("Güncellenecek para birimi bulunamadı.");
            }
            
            // Aynı kodla başka kayıt var mı kontrol et
            var duplicate = await _context.ParaBirimleri
                .FirstOrDefaultAsync(p => p.Kod == paraBirimi.Kod && p.ParaBirimiID != paraBirimi.ParaBirimiID);
                
            if (duplicate != null)
            {
                throw new Exception($"{paraBirimi.Kod} kodlu başka bir para birimi zaten mevcut.");
            }
            
            existing.Kod = paraBirimi.Kod;
            existing.Ad = paraBirimi.Ad;
            existing.Sembol = paraBirimi.Sembol;
            existing.Aktif = paraBirimi.Aktif;
            
            await _context.SaveChangesAsync();
            
            return existing;
        }

        public async Task DeleteParaBirimiAsync(Guid id)
        {
            var paraBirimi = await _context.ParaBirimleri
                .FirstOrDefaultAsync(p => p.ParaBirimiID == id);
                
            if (paraBirimi == null)
            {
                throw new Exception("Silinecek para birimi bulunamadı.");
            }
            
            // İlişkili kur değerleri var mı kontrol et
            var kurDegerleri = await _context.KurDegerleri
                .Where(k => k.ParaBirimiID == id)
                .ToListAsync();
                
            if (kurDegerleri.Any())
            {
                throw new Exception("Bu para birimine bağlı kur değerleri bulunmaktadır. Önce ilişkili kur değerlerini silmelisiniz.");
            }
            
            // İlişkili döviz ilişkileri var mı kontrol et
            var dovizIliskileri = await _context.DovizIliskileri
                .Where(di => di.KaynakParaBirimiID == id || di.HedefParaBirimiID == id)
                .ToListAsync();
                
            if (dovizIliskileri.Any())
            {
                throw new Exception("Bu para birimine bağlı döviz ilişkileri bulunmaktadır. Önce ilişkili döviz ilişkilerini silmelisiniz.");
            }
            
            // Soft delete
            paraBirimi.Aktif = false;
            
            await _context.SaveChangesAsync();
        }

        #endregion

        #region Kur Değeri İşlemleri

        public async Task<List<KurDegeri>> GetKurDegerleriAsync()
        {
            return await _context.KurDegerleri
                .Include(k => k.ParaBirimi)
                .Where(k => k.Aktif)
                .OrderByDescending(k => k.Tarih)
                .ToListAsync();
        }

        public async Task<KurDegeri> GetKurDegeriByIdAsync(Guid id)
        {
            return await _context.KurDegerleri
                .Include(k => k.ParaBirimi)
                .FirstOrDefaultAsync(k => k.KurDegeriID == id);
        }

        public async Task<KurDegeri> GetKurDegeriAsync(string kaynakKod, string hedefKod)
        {
            // Önce para birimlerini bul
            var kaynakParaBirimi = await _context.ParaBirimleri
                .FirstOrDefaultAsync(p => p.Kod == kaynakKod && p.Aktif);
                
            var hedefParaBirimi = await _context.ParaBirimleri
                .FirstOrDefaultAsync(p => p.Kod == hedefKod && p.Aktif);
                
            if (kaynakParaBirimi == null || hedefParaBirimi == null)
            {
                throw new Exception($"Para birimi bulunamadı: Kaynak={kaynakKod}, Hedef={hedefKod}");
            }
            
            // Para birimleri arasında ilişki var mı kontrol et
            var dovizIliski = await _context.DovizIliskileri
                .FirstOrDefaultAsync(di => 
                    di.KaynakParaBirimiID == kaynakParaBirimi.ParaBirimiID && 
                    di.HedefParaBirimiID == hedefParaBirimi.ParaBirimiID && 
                    di.Aktif);
                    
            if (dovizIliski == null)
            {
                throw new Exception($"{kaynakKod}-{hedefKod} para birimleri arası ilişki tanımlanmamış.");
            }
            
            // İlişki için en güncel kur değerini bul
            var kurDegeri = await _context.KurDegerleri
                .Include(k => k.ParaBirimi)
                .Where(k => k.ParaBirimiID == kaynakParaBirimi.ParaBirimiID && k.Aktif)
                .OrderByDescending(k => k.Tarih)
                .FirstOrDefaultAsync();
                
            if (kurDegeri == null)
            {
                throw new Exception($"{kaynakKod} için güncel kur değeri bulunamadı.");
            }
            
            return kurDegeri;
        }

        public async Task<KurDegeri> AddKurDegeriAsync(KurDegeri kurDegeri)
        {
            // ParaBirimi var mı kontrol et
            var paraBirimi = await _context.ParaBirimleri
                .FirstOrDefaultAsync(p => p.ParaBirimiID == kurDegeri.ParaBirimiID);
                
            if (paraBirimi == null)
            {
                throw new Exception("Geçerli bir para birimi seçilmedi.");
            }
            
            kurDegeri.KurDegeriID = Guid.NewGuid();
            kurDegeri.Aktif = true;
            kurDegeri.SoftDelete = false;
            
            _context.KurDegerleri.Add(kurDegeri);
            await _context.SaveChangesAsync();
            
            return kurDegeri;
        }

        public async Task<KurDegeri> UpdateKurDegeriAsync(KurDegeri kurDegeri)
        {
            var existing = await _context.KurDegerleri
                .FirstOrDefaultAsync(k => k.KurDegeriID == kurDegeri.KurDegeriID);
                
            if (existing == null)
            {
                throw new Exception("Güncellenecek kur değeri bulunamadı.");
            }
            
            // ParaBirimi var mı kontrol et
            var paraBirimi = await _context.ParaBirimleri
                .FirstOrDefaultAsync(p => p.ParaBirimiID == kurDegeri.ParaBirimiID);
                
            if (paraBirimi == null)
            {
                throw new Exception("Geçerli bir para birimi seçilmedi.");
            }
            
            existing.ParaBirimiID = kurDegeri.ParaBirimiID;
            existing.AlisDegeri = kurDegeri.AlisDegeri;
            existing.SatisDegeri = kurDegeri.SatisDegeri;
            existing.Tarih = kurDegeri.Tarih;
            existing.Kaynak = kurDegeri.Kaynak;
            
            await _context.SaveChangesAsync();
            
            return existing;
        }

        public async Task DeleteKurDegeriAsync(Guid id)
        {
            var kurDegeri = await _context.KurDegerleri
                .FirstOrDefaultAsync(k => k.KurDegeriID == id);
                
            if (kurDegeri == null)
            {
                throw new Exception("Silinecek kur değeri bulunamadı.");
            }
            
            // Soft delete
            kurDegeri.Aktif = false;
            kurDegeri.SoftDelete = true;
            
            await _context.SaveChangesAsync();
        }

        #endregion

        #region Döviz İlişkileri İşlemleri
        
        public async Task<List<DovizIliski>> GetDovizIliskileriAsync()
        {
            return await _context.DovizIliskileri
                .Include(di => di.KaynakParaBirimi)
                .Include(di => di.HedefParaBirimi)
                .Where(di => di.Aktif)
                .ToListAsync();
        }
        
        public async Task<DovizIliski> GetDovizIliskiByIdAsync(Guid id)
        {
            return await _context.DovizIliskileri
                .Include(di => di.KaynakParaBirimi)
                .Include(di => di.HedefParaBirimi)
                .FirstOrDefaultAsync(di => di.DovizIliskiID == id);
        }
        
        public async Task<DovizIliski> AddDovizIliskiAsync(DovizIliski dovizIliski)
        {
            // Para birimleri var mı kontrol et
            var kaynakParaBirimi = await _context.ParaBirimleri
                .FirstOrDefaultAsync(p => p.ParaBirimiID == dovizIliski.KaynakParaBirimiID);
                
            var hedefParaBirimi = await _context.ParaBirimleri
                .FirstOrDefaultAsync(p => p.ParaBirimiID == dovizIliski.HedefParaBirimiID);
                
            if (kaynakParaBirimi == null || hedefParaBirimi == null)
            {
                throw new Exception("Geçerli para birimleri seçilmedi.");
            }
            
            // Aynı ilişki var mı kontrol et
            var existing = await _context.DovizIliskileri
                .FirstOrDefaultAsync(di => 
                    di.KaynakParaBirimiID == dovizIliski.KaynakParaBirimiID && 
                    di.HedefParaBirimiID == dovizIliski.HedefParaBirimiID && 
                    !di.SoftDelete);
                    
            if (existing != null)
            {
                throw new Exception($"{kaynakParaBirimi.Kod}-{hedefParaBirimi.Kod} ilişkisi zaten tanımlı.");
            }
            
            dovizIliski.DovizIliskiID = Guid.NewGuid();
            dovizIliski.Aktif = true;
            dovizIliski.SoftDelete = false;
            
            _context.DovizIliskileri.Add(dovizIliski);
            await _context.SaveChangesAsync();
            
            return dovizIliski;
        }
        
        public async Task<DovizIliski> UpdateDovizIliskiAsync(DovizIliski dovizIliski)
        {
            var existing = await _context.DovizIliskileri
                .FirstOrDefaultAsync(di => di.DovizIliskiID == dovizIliski.DovizIliskiID);
                
            if (existing == null)
            {
                throw new Exception("Güncellenecek döviz ilişkisi bulunamadı.");
            }
            
            // Para birimleri var mı kontrol et
            var kaynakParaBirimi = await _context.ParaBirimleri
                .FirstOrDefaultAsync(p => p.ParaBirimiID == dovizIliski.KaynakParaBirimiID);
                
            var hedefParaBirimi = await _context.ParaBirimleri
                .FirstOrDefaultAsync(p => p.ParaBirimiID == dovizIliski.HedefParaBirimiID);
                
            if (kaynakParaBirimi == null || hedefParaBirimi == null)
            {
                throw new Exception("Geçerli para birimleri seçilmedi.");
            }
            
            // Aynı ilişki var mı kontrol et (diğer ilişkilerde)
            var duplicate = await _context.DovizIliskileri
                .FirstOrDefaultAsync(di => 
                    di.DovizIliskiID != dovizIliski.DovizIliskiID &&
                    di.KaynakParaBirimiID == dovizIliski.KaynakParaBirimiID && 
                    di.HedefParaBirimiID == dovizIliski.HedefParaBirimiID && 
                    !di.SoftDelete);
                    
            if (duplicate != null)
            {
                throw new Exception($"{kaynakParaBirimi.Kod}-{hedefParaBirimi.Kod} ilişkisi zaten tanımlı.");
            }
            
            existing.KaynakParaBirimiID = dovizIliski.KaynakParaBirimiID;
            existing.HedefParaBirimiID = dovizIliski.HedefParaBirimiID;
            existing.Aktif = dovizIliski.Aktif;
            
            await _context.SaveChangesAsync();
            
            return existing;
        }
        
        public async Task DeleteDovizIliskiAsync(Guid id)
        {
            var dovizIliski = await _context.DovizIliskileri
                .FirstOrDefaultAsync(di => di.DovizIliskiID == id);
                
            if (dovizIliski == null)
            {
                throw new Exception("Silinecek döviz ilişkisi bulunamadı.");
            }
            
            // Soft delete
            dovizIliski.Aktif = false;
            dovizIliski.SoftDelete = true;
            
            await _context.SaveChangesAsync();
        }
        
        #endregion

        #region Kur Ayarları

        public async Task<SistemAyarlari> GetKurAyarlariAsync()
        {
            return await _context.SistemAyarlari.FirstOrDefaultAsync();
        }

        public async Task<SistemAyarlari> UpdateKurAyarlariAsync(SistemAyarlari sistemAyarlari)
        {
            var existing = await _context.SistemAyarlari.FirstOrDefaultAsync();
            
            if (existing == null)
            {
                sistemAyarlari.SistemAyarlariID = Guid.NewGuid();
                sistemAyarlari.Aktif = true;
                sistemAyarlari.SoftDelete = false;
                _context.SistemAyarlari.Add(sistemAyarlari);
            }
            else
            {
                existing.AnaDovizKodu = sistemAyarlari.AnaDovizKodu;
                existing.IkinciDovizKodu = sistemAyarlari.IkinciDovizKodu;
                existing.UcuncuDovizKodu = sistemAyarlari.UcuncuDovizKodu;
                existing.OtomatikDovizGuncelleme = sistemAyarlari.OtomatikDovizGuncelleme;
                existing.DovizGuncellemeSikligi = sistemAyarlari.DovizGuncellemeSikligi;
                existing.SonDovizGuncellemeTarihi = sistemAyarlari.SonDovizGuncellemeTarihi;
            }
            
            await _context.SaveChangesAsync();
            
            return existing ?? sistemAyarlari;
        }

        #endregion

        #region Dış Kaynaklı Kur Güncelleme

        public async Task<bool> UpdateKurlarFromMerkezBankasiAsync()
        {
            try
            {
                var client = _httpClientFactory.CreateClient();
                var response = await client.GetAsync("https://www.tcmb.gov.tr/kurlar/today.xml");
                
                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogError("TCMB'den kur verileri alınamadı. StatusCode: {StatusCode}", response.StatusCode);
                    return false;
                }
                
                var content = await response.Content.ReadAsStringAsync();
                var doc = XDocument.Parse(content);
                
                foreach (var currency in doc.Descendants("Currency"))
                {
                    var kod = currency.Attribute("CurrencyCode")?.Value;
                    if (string.IsNullOrEmpty(kod)) continue;
                    
                    var paraBirimi = await _context.ParaBirimleri
                        .FirstOrDefaultAsync(p => p.Kod == kod && p.Aktif);
                        
                    if (paraBirimi == null) continue;
                    
                    var alisDegeri = decimal.Parse(currency.Element("ForexBuying")?.Value ?? "0", System.Globalization.CultureInfo.InvariantCulture);
                    var satisDegeri = decimal.Parse(currency.Element("ForexSelling")?.Value ?? "0", System.Globalization.CultureInfo.InvariantCulture);
                    
                    var kurDegeri = new KurDegeri
                    {
                        KurDegeriID = Guid.NewGuid(),
                        ParaBirimiID = paraBirimi.ParaBirimiID,
                        AlisDegeri = alisDegeri,
                        SatisDegeri = satisDegeri,
                        Tarih = DateTime.Now,
                        Kaynak = "TCMB",
                        Aktif = true,
                        SoftDelete = false
                    };
                    
                    _context.KurDegerleri.Add(kurDegeri);
                }
                
                await _context.SaveChangesAsync();
                
                // Sistem ayarlarını güncelle
                var sistemAyarlari = await _context.SistemAyarlari.FirstOrDefaultAsync();
                if (sistemAyarlari != null)
                {
                    sistemAyarlari.SonDovizGuncellemeTarihi = DateTime.Now;
                    await _context.SaveChangesAsync();
                }
                
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "TCMB'den kurlar güncellenirken hata oluştu");
                return false;
            }
        }

        public async Task<bool> UpdateKurlarFromUzbekistanMBAsync()
        {
            try
            {
                var client = _httpClientFactory.CreateClient();
                var response = await client.GetAsync("https://cbu.uz/oz/arkhiv-kursov-valyut/xml/");
                
                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogError("Özbekistan MB'den kur verileri alınamadı. StatusCode: {StatusCode}", response.StatusCode);
                    return false;
                }
                
                var content = await response.Content.ReadAsStringAsync();
                var doc = XDocument.Parse(content);
                
                foreach (var currency in doc.Descendants("CcyNtry"))
                {
                    var kod = currency.Element("Ccy")?.Value;
                    if (string.IsNullOrEmpty(kod)) continue;
                    
                    var paraBirimi = await _context.ParaBirimleri
                        .FirstOrDefaultAsync(p => p.Kod == kod && p.Aktif);
                        
                    if (paraBirimi == null) continue;
                    
                    var deger = decimal.Parse(currency.Element("Rate")?.Value ?? "0", System.Globalization.CultureInfo.InvariantCulture);
                    
                    var kurDegeri = new KurDegeri
                    {
                        KurDegeriID = Guid.NewGuid(),
                        ParaBirimiID = paraBirimi.ParaBirimiID,
                        AlisDegeri = deger,
                        SatisDegeri = deger,
                        Tarih = DateTime.Now,
                        Kaynak = "Özbekistan MB",
                        Aktif = true,
                        SoftDelete = false
                    };
                    
                    _context.KurDegerleri.Add(kurDegeri);
                }
                
                await _context.SaveChangesAsync();
                
                // Sistem ayarlarını güncelle
                var sistemAyarlari = await _context.SistemAyarlari.FirstOrDefaultAsync();
                if (sistemAyarlari != null)
                {
                    sistemAyarlari.SonDovizGuncellemeTarihi = DateTime.Now;
                    await _context.SaveChangesAsync();
                }
                
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Özbekistan MB'den kurlar güncellenirken hata oluştu");
                return false;
            }
        }

        #endregion

        #region Para Birimi Çevirme

        public async Task<decimal> ConvertParaBirimiAsync(string kaynakKod, string hedefKod, decimal miktar)
        {
            if (kaynakKod == hedefKod)
            {
                return miktar;
            }
            
            var kurDegeri = await GetKurDegeriAsync(kaynakKod, hedefKod);
            
            if (kurDegeri == null)
            {
                throw new Exception($"{kaynakKod}-{hedefKod} için kur değeri bulunamadı.");
            }
            
            return miktar * kurDegeri.AlisDegeri;
        }
        
        public async Task<decimal> GetGuncelKur(string kaynakParaBirimi, string hedefParaBirimi)
        {
            var kurDegeri = await GetKurDegeriAsync(kaynakParaBirimi, hedefParaBirimi);
            
            if (kurDegeri == null)
            {
                throw new Exception($"{kaynakParaBirimi}-{hedefParaBirimi} için kur değeri bulunamadı.");
            }
            
            return kurDegeri.AlisDegeri;
        }

        #endregion
    }
} 