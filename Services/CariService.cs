using Microsoft.EntityFrameworkCore;
using MuhasebeStokWebApp.Data;
using MuhasebeStokWebApp.Data.Entities;
using MuhasebeStokWebApp.Services.Interfaces;
using MuhasebeStokWebApp.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace MuhasebeStokWebApp.Services
{
    public class CariService : ICariService
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<CariService> _logger;
        private readonly IParaBirimiService _paraBirimiService;
        // Bakiye değerlerini geçici olarak saklamak için Dictionary
        private readonly Dictionary<Guid, decimal> _bakiyeSozlugu = new Dictionary<Guid, decimal>();

        public CariService(
            ApplicationDbContext context, 
            ILogger<CariService> logger,
            IParaBirimiService paraBirimiService)
        {
            _context = context;
            _logger = logger;
            _paraBirimiService = paraBirimiService;
        }

        public async Task<IEnumerable<Cari>> GetAllAsync()
        {
            return await _context.Cariler
                .AsNoTracking()
                .Where(c => !c.Silindi)
                .OrderBy(c => c.Ad)
                .ToListAsync();
        }

        public async Task<Cari> GetByIdAsync(Guid id)
        {
            return await _context.Cariler
                .FirstOrDefaultAsync(c => c.CariID == id && !c.Silindi);
        }

        public async Task<Cari> AddAsync(Cari cari)
        {
            // Transaction yönetimi
            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                await _context.Cariler.AddAsync(cari);
                await _context.SaveChangesAsync();
                
                // Açılış bakiyesi hareketi ekleme
                if (cari.BaslangicBakiye != 0)
                {
                    var acilisBakiyeHareketi = new CariHareket
                    {
                        CariHareketID = Guid.NewGuid(),
                        CariID = cari.CariID,
                        Tarih = DateTime.Now,
                        HareketTuru = "Açılış bakiyesi",
                        Aciklama = "Açılış bakiyesi",
                        Borc = cari.BaslangicBakiye < 0 ? Math.Abs(cari.BaslangicBakiye) : 0,
                        Alacak = cari.BaslangicBakiye > 0 ? cari.BaslangicBakiye : 0,
                        Tutar = Math.Abs(cari.BaslangicBakiye),
                        OlusturmaTarihi = DateTime.Now,
                        OlusturanKullaniciID = cari.OlusturanKullaniciId
                    };
                    
                    await _context.CariHareketler.AddAsync(acilisBakiyeHareketi);
                    await _context.SaveChangesAsync();
                    _logger.LogInformation($"Cari açılış bakiye hareketi oluşturuldu: ID={acilisBakiyeHareketi.CariHareketID}, Tutar={cari.BaslangicBakiye}");
                }
                
                await transaction.CommitAsync();
                _logger.LogInformation($"Cari başarıyla oluşturuldu: ID={cari.CariID}, Ad={cari.Ad}");
                
                return cari;
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                _logger.LogError(ex, $"Cari kayıt işlemi sırasında hata: {ex.Message}");
                throw;
            }
        }

        public async Task<Cari> UpdateAsync(Cari cari)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                _context.Entry(cari).State = EntityState.Modified;
                cari.GuncellemeTarihi = DateTime.Now;
                
                await _context.SaveChangesAsync();
                await transaction.CommitAsync();
                
                _logger.LogInformation($"Cari başarıyla güncellendi: ID={cari.CariID}, Ad={cari.Ad}");
                return cari;
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                _logger.LogError(ex, $"Cari güncelleme işlemi sırasında hata: {ex.Message}");
                throw;
            }
        }

        public async Task DeleteAsync(Cari cari)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                // İlgili cariye ait hareketleri kontrol et
                bool hareketVarMi = await _context.CariHareketler
                    .AnyAsync(ch => ch.CariID == cari.CariID && !ch.Silindi);
                
                if (hareketVarMi)
                {
                    _logger.LogWarning($"Cari silinemiyor, ilişkili hareketleri var: ID={cari.CariID}, Ad={cari.Ad}");
                    // Hard delete yerine soft delete uygula
                    cari.Silindi = true;
                    cari.AktifMi = false;
                    cari.GuncellemeTarihi = DateTime.Now;
                    
                    await _context.SaveChangesAsync();
                    await transaction.CommitAsync();
                    
                    _logger.LogInformation($"Cari pasife alındı (soft delete): ID={cari.CariID}, Ad={cari.Ad}");
                }
                else
                {
                    // İlişkili kayıtlar yoksa hard delete yapabilirsin
                    // Veya yine soft delete tercih edilebilir
                    cari.Silindi = true;
                    cari.GuncellemeTarihi = DateTime.Now;
                    
                    await _context.SaveChangesAsync();
                    await transaction.CommitAsync();
                    
                    _logger.LogInformation($"Cari silindi: ID={cari.CariID}, Ad={cari.Ad}");
                }
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                _logger.LogError(ex, $"Cari silme işlemi sırasında hata: {ex.Message}");
                throw;
            }
        }

        /// <summary>
        /// Cari bakiyesini hesaplar
        /// </summary>
        public async Task<decimal> CariBakiyeHesaplaAsync(Guid cariId, DateTime? tarih = null, Guid? paraBirimiId = null)
        {
            try
            {
                _logger.LogInformation($"CariBakiyeHesaplaAsync başladı: CariID={cariId}, Tarih={tarih}, ParaBirimiID={paraBirimiId}");
                
                // Cari'yi veritabanından getir
                var cari = await _context.Cariler.FindAsync(cariId);
                if (cari == null)
                {
                    _logger.LogWarning($"Bakiye hesaplanamadı: Cari bulunamadı (ID: {cariId})");
                    throw new Exception($"Cari bulunamadı (ID: {cariId})");
                }
                
                // Cari hareketlerini getir - performans için AsNoTracking kullan
                var query = _context.CariHareketler
                    .AsNoTracking()
                    .Where(ch => ch.CariID == cariId && !ch.Silindi);
                
                // Tarih filtresi ekle
                if (tarih.HasValue)
                {
                    query = query.Where(ch => ch.Tarih <= tarih.Value);
                }
                
                // Hareketleri getir
                var hareketler = await query.ToListAsync();
                
                // Açılış bakiyesi hareketlerini bul
                var acilisBakiyeHareketi = hareketler
                    .Where(h => h.HareketTuru == "Açılış bakiyesi")
                    .OrderBy(h => h.Tarih)
                    .FirstOrDefault();
                
                // Başlangıç bakiyesi - açılış bakiyesi hareketi yoksa carinin başlangıç bakiyesini kullan
                decimal bakiye = acilisBakiyeHareketi != null 
                    ? acilisBakiyeHareketi.Alacak - acilisBakiyeHareketi.Borc
                    : cari.BaslangicBakiye;
                
                // Diğer hareketlerin bakiyeye etkisini hesapla (açılış bakiyesi hariç)
                foreach (var hareket in hareketler.Where(h => h.HareketTuru != "Açılış bakiyesi").OrderBy(h => h.Tarih))
                {
                    // Bakiye düzeltmesi ise
                    if (hareket.HareketTuru == "Bakiye Düzeltmesi")
                    {
                        bakiye += hareket.Alacak - hareket.Borc;
                    }
                    // Ödeme, borç veya çıkış ise bakiyeyi azalt
                    else if (hareket.HareketTuru == "Ödeme" || hareket.HareketTuru == "Borç" || hareket.HareketTuru == "Çıkış")
                    {
                        bakiye -= hareket.Tutar;
                    }
                    // Tahsilat, alacak veya giriş ise bakiyeyi artır
                    else if (hareket.HareketTuru == "Tahsilat" || hareket.HareketTuru == "Alacak" || hareket.HareketTuru == "Giriş")
                    {
                        bakiye += hareket.Tutar;
                    }
                }
                
                // Para birimi dönüşümü
                if (paraBirimiId.HasValue && cari.VarsayilanParaBirimiId.HasValue && paraBirimiId.Value != cari.VarsayilanParaBirimiId.Value)
                {
                    // Not: Bu kısım uygulamanın döviz kuru servisiyle entegrasyonu gerektirir
                    // Örnek olarak sabit bir dönüşüm oranı kullanılabilir veya bu fonksiyonellik daha sonra eklenebilir
                    var dovizKuru = 1.0m; // Varsayılan olarak 1:1 dönüşüm
                    
                    bakiye = Math.Round(bakiye * dovizKuru, 2);
                }
                
                _logger.LogInformation($"Cari bakiye hesaplandı: CariID={cariId}, Bakiye={bakiye}");
                return bakiye;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Cari bakiye hesaplanırken hata oluştu: {ex.Message}");
                throw;
            }
        }
        
        /// <summary>
        /// Cari ekstresi için kümülatif bakiyeli hareketleri döndürür
        /// </summary>
        public async Task<(decimal BakiyeToplamı, List<CariHareket> Hareketler)> GetCariEkstreAsync(
            Guid cariId, 
            DateTime? baslangicTarihi = null, 
            DateTime? bitisTarihi = null, 
            Guid? paraBirimiId = null)
        {
            try
            {
                _logger.LogInformation($"GetCariEkstreAsync başladı: CariID={cariId}");
                
                // Varsayılan tarih aralığı: Son 1 ay
                var startDate = baslangicTarihi ?? DateTime.Now.AddMonths(-1).Date;
                var endDate = bitisTarihi ?? DateTime.Now.Date.AddDays(1).AddSeconds(-1);
                
                // Cari varlığını kontrol et
                var cari = await _context.Cariler.FindAsync(cariId);
                if (cari == null || cari.Silindi)
                {
                    _logger.LogWarning($"Cari ekstre alınamadı: Cari bulunamadı (ID: {cariId})");
                    throw new Exception($"Cari bulunamadı (ID: {cariId})");
                }
                
                // Eğer paraBirimiId belirtilmemişse ve carinin varsayılan para birimi varsa onu kullan
                if (!paraBirimiId.HasValue && cari.VarsayilanParaBirimiId.HasValue)
                {
                    paraBirimiId = cari.VarsayilanParaBirimiId.Value;
                }
                
                // Tüm hareketleri getir - performans için AsNoTracking kullan
                var tumHareketler = await _context.CariHareketler
                    .AsNoTracking()
                    .Where(c => !c.Silindi && c.CariID == cariId)
                    .ToListAsync();
                
                // Açılış bakiyesi hareketlerini ayrı al 
                var acilisBakiyeHareketi = tumHareketler
                    .Where(h => h.HareketTuru == "Açılış bakiyesi")
                    .OrderBy(h => h.Tarih)
                    .FirstOrDefault();
                
                // Başlangıç bakiyesini hesapla
                decimal baslangicBakiyesi = 0;
                
                // İlk açılış bakiyesi hareketi varsa onu kullan
                if (acilisBakiyeHareketi != null)
                {
                    baslangicBakiyesi = acilisBakiyeHareketi.Alacak - acilisBakiyeHareketi.Borc;
                }
                else
                {
                    baslangicBakiyesi = cari.BaslangicBakiye;
                }
                
                // Başlangıç tarihinden önceki hareketlerin etkisini hesapla
                decimal oncekiHareketlerinEtkisi = 0;
                var oncekiHareketler = tumHareketler
                    .Where(h => h.HareketTuru != "Açılış bakiyesi" && h.Tarih < startDate)
                    .OrderBy(h => h.Tarih);
                
                foreach (var oncekiHareket in oncekiHareketler)
                {
                    // Hareket türüne göre bakiyeyi güncelle
                    if (oncekiHareket.HareketTuru == "Bakiye Düzeltmesi")
                    {
                        oncekiHareketlerinEtkisi += oncekiHareket.Alacak - oncekiHareket.Borc;
                    }
                    else if (oncekiHareket.HareketTuru == "Ödeme" || oncekiHareket.HareketTuru == "Borç" || oncekiHareket.HareketTuru == "Çıkış")
                    {
                        oncekiHareketlerinEtkisi -= oncekiHareket.Tutar;
                    }
                    else if (oncekiHareket.HareketTuru == "Tahsilat" || oncekiHareket.HareketTuru == "Alacak" || oncekiHareket.HareketTuru == "Giriş")
                    {
                        oncekiHareketlerinEtkisi += oncekiHareket.Tutar;
                    }
                }
                
                // Başlangıç bakiyesine önceki hareketlerin etkisini ekle
                baslangicBakiyesi += oncekiHareketlerinEtkisi;
                
                // Gösterilecek hareketleri tarih aralığına göre filtrele
                var gosterilecekHareketler = tumHareketler
                    .Where(h => h.HareketTuru != "Açılış bakiyesi" && h.Tarih >= startDate && h.Tarih <= endDate)
                    .OrderBy(h => h.Tarih)
                    .ToList();
                
                // Her hareketin bakiyeye etkisini hesapla ve kümülatif bakiye bilgisini sözlüğe ekle
                decimal bakiye = baslangicBakiyesi;
                _bakiyeSozlugu.Clear(); // Sözlüğü temizle
                
                foreach (var hareket in gosterilecekHareketler)
                {
                    // Hareket türüne göre bakiyeyi güncelle
                    decimal hareketTutari = 0;
                    
                    if (hareket.HareketTuru == "Bakiye Düzeltmesi")
                    {
                        hareketTutari = hareket.Alacak - hareket.Borc;
                    }
                    else if (hareket.HareketTuru == "Ödeme" || hareket.HareketTuru == "Borç" || hareket.HareketTuru == "Çıkış")
                    {
                        hareketTutari = -hareket.Tutar;
                    }
                    else if (hareket.HareketTuru == "Tahsilat" || hareket.HareketTuru == "Alacak" || hareket.HareketTuru == "Giriş")
                    {
                        hareketTutari = hareket.Tutar;
                    }
                    
                    bakiye += hareketTutari;
                    
                    // Bakiye değerini sözlüğe ekle (hareket ID'si ile ilişkilendir)
                    _bakiyeSozlugu[hareket.CariHareketID] = bakiye;
                }
                
                // Para birimi dönüşümü
                if (paraBirimiId.HasValue && cari.VarsayilanParaBirimiId.HasValue && paraBirimiId.Value != cari.VarsayilanParaBirimiId.Value)
                {
                    // Not: Döviz kuru servisi entegrasyonu
                    var dovizKuru = 1.0m; // Varsayılan olarak 1:1 dönüşüm
                    
                    // Başlangıç bakiyesi ve her bir hareketin bakiyesini dönüştür
                    baslangicBakiyesi = Math.Round(baslangicBakiyesi * dovizKuru, 2);
                    
                    foreach (var hareket in gosterilecekHareketler)
                    {
                        // Sözlükteki bakiye değerlerini güncelle
                        if (_bakiyeSozlugu.ContainsKey(hareket.CariHareketID))
                        {
                            _bakiyeSozlugu[hareket.CariHareketID] = Math.Round(_bakiyeSozlugu[hareket.CariHareketID] * dovizKuru, 2);
                        }
                        
                        hareket.Tutar = Math.Round(hareket.Tutar * dovizKuru, 2);
                        hareket.Borc = Math.Round(hareket.Borc * dovizKuru, 2);
                        hareket.Alacak = Math.Round(hareket.Alacak * dovizKuru, 2);
                    }
                    
                    bakiye = Math.Round(bakiye * dovizKuru, 2);
                }
                
                _logger.LogInformation($"Cari ekstre başarıyla alındı: CariID={cariId}, Başlangıç bakiyesi={baslangicBakiyesi}, Son bakiye={bakiye}, Hareket sayısı={gosterilecekHareketler.Count}");
                
                // Entity'leri değiştirmeden bakiyelerle birlikte döndür
                return (bakiye, gosterilecekHareketler);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Cari ekstre alınırken hata oluştu: {ex.Message}");
                throw;
            }
        }
        
        // Belirli bir hareketin bakiyesini almak için yardımcı metod
        public decimal GetHareketBakiye(Guid hareketId)
        {
            return _bakiyeSozlugu.TryGetValue(hareketId, out decimal bakiye) ? bakiye : 0;
        }
    }
} 