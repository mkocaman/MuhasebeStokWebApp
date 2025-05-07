using Microsoft.EntityFrameworkCore;
using MuhasebeStokWebApp.Data;
using MuhasebeStokWebApp.Data.Entities;
using MuhasebeStokWebApp.Services.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MuhasebeStokWebApp.Data.Repositories;
using Microsoft.Extensions.Logging;
using MuhasebeStokWebApp.Services.ParaBirimiModulu;
using MuhasebeStokWebApp.Models;

namespace MuhasebeStokWebApp.Services.Implementations
{
    public class CariService : ICariService
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogService _logService;
        private readonly ILogger<CariService> _logger;
        private readonly IUnitOfWork _unitOfWork;
        private readonly MuhasebeStokWebApp.Services.ParaBirimiModulu.IParaBirimiService _paraBirimiService;

        public CariService(
            ApplicationDbContext context, 
            ILogService logService, 
            ILogger<CariService> logger,
            IUnitOfWork unitOfWork,
            MuhasebeStokWebApp.Services.ParaBirimiModulu.IParaBirimiService paraBirimiService)
        {
            _context = context;
            _logService = logService;
            _logger = logger;
            _unitOfWork = unitOfWork;
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
                var cari = await _unitOfWork.CariRepository.GetByIdAsync(cariId);
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
                
                // Başlangıç bakiyesi - açılış bakiyesi hareketi yoksa 0 olarak başla
                decimal bakiye = acilisBakiyeHareketi != null 
                    ? acilisBakiyeHareketi.Alacak - acilisBakiyeHareketi.Borc
                    : 0;
                
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
                    // Farklı para birimi seçilmişse ve varsayılan para birimi varsa dönüşüm yap
                    // Kaynak para birimini al
                    var kaynakParaBirimi = await _paraBirimiService.GetParaBirimiByIdAsync(cari.VarsayilanParaBirimiId.Value);
                    // Hedef para birimini al
                    var hedefParaBirimi = await _paraBirimiService.GetParaBirimiByIdAsync(paraBirimiId.Value);

                    if (kaynakParaBirimi != null && hedefParaBirimi != null)
                    {
                        var dovizKuru = await _paraBirimiService.GetCurrentExchangeRateAsync(kaynakParaBirimi.Kod, hedefParaBirimi.Kod);
                        if (dovizKuru > 0)
                        {
                            bakiye = Math.Round(bakiye * dovizKuru, 2);
                        }
                    }
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

        public async Task<(decimal BakiyeToplamı, List<Data.Entities.CariHareket> Hareketler)> GetCariEkstreAsync(
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
                var cari = await _context.Cariler
                    .FirstOrDefaultAsync(c => c.CariID == cariId && !c.Silindi);
                
                if (cari == null)
                {
                    _logger.LogWarning($"Cari bulunamadı: ID={cariId}");
                    throw new Exception($"Cari bulunamadı: ID={cariId}");
                }
                
                // Hareketleri getir
                var tumHareketler = await _context.CariHareketler
                    .AsNoTracking()
                    .Where(ch => ch.CariID == cariId && !ch.Silindi)
                    .ToListAsync();
                
                // Açılış bakiyesi hareketlerini bul
                var acilisBakiyeHareketi = tumHareketler
                    .Where(h => h.HareketTuru == "Açılış bakiyesi")
                    .OrderBy(h => h.Tarih)
                    .FirstOrDefault();
                
                // Başlangıç bakiyesi
                decimal baslangicBakiyesi = 0;
                
                // Açılış bakiyesi hareketi varsa kullan
                if (acilisBakiyeHareketi != null)
                {
                    baslangicBakiyesi = acilisBakiyeHareketi.Alacak - acilisBakiyeHareketi.Borc;
                }
                
                // Gösterilecek hareketleri tarih aralığına göre filtrele
                var filtrelenmisHareketler = tumHareketler
                    .Where(h => h.Tarih >= startDate && h.Tarih <= endDate && h.HareketTuru != "Açılış bakiyesi")
                    .OrderBy(h => h.Tarih)
                    .ToList();
                
                // Bakiyeyi hesapla
                decimal toplamBakiye = baslangicBakiyesi;
                
                foreach (var hareket in filtrelenmisHareketler)
                {
                    if (hareket.HareketTuru == "Bakiye Düzeltmesi")
                    {
                        toplamBakiye = hareket.Alacak - hareket.Borc;
                    }
                    else if (hareket.HareketTuru == "Ödeme" || hareket.HareketTuru == "Borç" || hareket.HareketTuru == "Çıkış")
                    {
                        toplamBakiye -= hareket.Tutar;
                    }
                    else if (hareket.HareketTuru == "Tahsilat" || hareket.HareketTuru == "Alacak" || hareket.HareketTuru == "Giriş")
                    {
                        toplamBakiye += hareket.Tutar;
                    }
                }
                
                return (toplamBakiye, filtrelenmisHareketler);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Cari ekstre alınırken hata oluştu: {ex.Message}");
                throw;
            }
        }

        public async Task<CariDetayViewModel> GetCariDetaylar(Guid cariID)
        {
            try
            {
                var cari = await _context.Cariler
                    .FirstOrDefaultAsync(c => c.CariID == cariID && !c.Silindi);
                
                if (cari == null)
                {
                    return null;
                }
                
                // Cari hareketlerini getir
                var hareketler = await _context.CariHareketler
                    .Where(h => h.CariID == cariID && !h.Silindi)
                    .ToListAsync();
                
                // Bakiyeleri hesapla
                decimal toplamBorc = 0;
                decimal toplamAlacak = 0;
                
                if (hareketler.Any())
                {
                    toplamBorc = hareketler.Sum(h => h.Borc);
                    toplamAlacak = hareketler.Sum(h => h.Alacak);
                }
                
                var viewModel = new CariDetayViewModel
                {
                    CariID = cari.CariID,
                    CariAdi = cari.Ad,
                    CariKodu = cari.CariKodu,
                    CariTipi = cari.CariTipi,
                    VergiNo = cari.VergiNo,
                    VergiDairesi = cari.VergiDairesi,
                    Telefon = cari.Telefon,
                    Email = cari.Email,
                    ToplamBorc = toplamBorc,
                    ToplamAlacak = toplamAlacak,
                    Bakiye = toplamAlacak - toplamBorc,
                    ParaBirimi = "₺" // Varsayılan değer
                };
                
                return viewModel;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Cari detayları alınırken hata oluştu: {ex.Message}");
                throw;
            }
        }

        public async Task<List<CariListModel>> GetAllActiveCariler()
        {
            try
            {
                return await _context.Cariler
                    .AsNoTracking()
                    .Where(c => c.AktifMi && !c.Silindi)
                    .OrderBy(c => c.Ad)
                    .Select(c => new CariListModel
                    {
                        CariID = c.CariID,
                        CariKodu = c.CariKodu,
                        CariAdi = c.Ad,
                        Telefon = c.Telefon,
                        Email = c.Email,
                        CariTipi = c.CariTipi
                    })
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Aktif cariler listelenirken hata oluştu: {ex.Message}");
                throw;
            }
        }

        public async Task<List<CariListModel>> GetAllPasifCariler()
        {
            try
            {
                return await _context.Cariler
                    .AsNoTracking()
                    .Where(c => !c.AktifMi && !c.Silindi)
                    .OrderBy(c => c.Ad)
                    .Select(c => new CariListModel
                    {
                        CariID = c.CariID,
                        CariKodu = c.CariKodu,
                        CariAdi = c.Ad,
                        Telefon = c.Telefon,
                        Email = c.Email,
                        CariTipi = c.CariTipi
                    })
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Pasif cariler listelenirken hata oluştu: {ex.Message}");
                throw;
            }
        }

        public async Task<List<CariListModel>> GetSilinmisCariler()
        {
            try
            {
                return await _context.Cariler
                    .AsNoTracking()
                    .IgnoreQueryFilters()
                    .Where(c => c.Silindi)
                    .OrderBy(c => c.Ad)
                    .Select(c => new CariListModel
                    {
                        CariID = c.CariID,
                        CariKodu = c.CariKodu,
                        CariAdi = c.Ad,
                        Telefon = c.Telefon,
                        Email = c.Email,
                        CariTipi = c.CariTipi
                    })
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Silinmiş cariler listelenirken hata oluştu: {ex.Message}");
                throw;
            }
        }

        public async Task<List<CariListModel>> GetMusteriler()
        {
            try
            {
                return await _context.Cariler
                    .AsNoTracking()
                    .Where(c => (c.CariTipi == "Müşteri" || c.CariTipi == "MüşteriVeTedarikçi") && !c.Silindi && c.AktifMi)
                    .OrderBy(c => c.Ad)
                    .Select(c => new CariListModel
                    {
                        CariID = c.CariID,
                        CariKodu = c.CariKodu,
                        CariAdi = c.Ad,
                        Telefon = c.Telefon,
                        Email = c.Email,
                        CariTipi = c.CariTipi
                    })
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Müşteriler listelenirken hata oluştu: {ex.Message}");
                throw;
            }
        }

        public async Task<List<CariListModel>> GetTedarikciler()
        {
            try
            {
                return await _context.Cariler
                    .AsNoTracking()
                    .Where(c => (c.CariTipi == "Tedarikçi" || c.CariTipi == "MüşteriVeTedarikçi") && !c.Silindi && c.AktifMi)
                    .OrderBy(c => c.Ad)
                    .Select(c => new CariListModel
                    {
                        CariID = c.CariID,
                        CariKodu = c.CariKodu,
                        CariAdi = c.Ad,
                        Telefon = c.Telefon,
                        Email = c.Email,
                        CariTipi = c.CariTipi
                    })
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Tedarikçiler listelenirken hata oluştu: {ex.Message}");
                throw;
            }
        }

        public async Task<List<CariListModel>> GetPasifMusteriler()
        {
            try
            {
                return await _context.Cariler
                    .AsNoTracking()
                    .Where(c => (c.CariTipi == "Müşteri" || c.CariTipi == "MüşteriVeTedarikçi") && !c.Silindi && !c.AktifMi)
                    .OrderBy(c => c.Ad)
                    .Select(c => new CariListModel
                    {
                        CariID = c.CariID,
                        CariKodu = c.CariKodu,
                        CariAdi = c.Ad,
                        Telefon = c.Telefon,
                        Email = c.Email,
                        CariTipi = c.CariTipi
                    })
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Pasif müşteriler listelenirken hata oluştu: {ex.Message}");
                throw;
            }
        }

        public async Task<List<CariListModel>> GetPasifTedarikciler()
        {
            try
            {
                return await _context.Cariler
                    .AsNoTracking()
                    .Where(c => (c.CariTipi == "Tedarikçi" || c.CariTipi == "MüşteriVeTedarikçi") && !c.Silindi && !c.AktifMi)
                    .OrderBy(c => c.Ad)
                    .Select(c => new CariListModel
                    {
                        CariID = c.CariID,
                        CariKodu = c.CariKodu,
                        CariAdi = c.Ad,
                        Telefon = c.Telefon,
                        Email = c.Email,
                        CariTipi = c.CariTipi
                    })
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Pasif tedarikçiler listelenirken hata oluştu: {ex.Message}");
                throw;
            }
        }
    }
}
