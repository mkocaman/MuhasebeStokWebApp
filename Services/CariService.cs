using Microsoft.EntityFrameworkCore;
using MuhasebeStokWebApp.Data;
using MuhasebeStokWebApp.Data.Entities;
using MuhasebeStokWebApp.Data.Repositories;
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
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<CariService> _logger;
        private readonly IParaBirimiService _paraBirimiService;
        private readonly IExceptionHandlingService _exceptionHandler;
        private readonly ITransactionManagerService _transactionManager;
        private readonly IParaBirimiCeviriciService _paraBirimiCevirici;
        // Bakiye değerlerini geçici olarak saklamak için Dictionary
        private readonly Dictionary<Guid, decimal> _bakiyeSozlugu = new Dictionary<Guid, decimal>();

        public CariService(
            IUnitOfWork unitOfWork, 
            ILogger<CariService> logger,
            IParaBirimiService paraBirimiService,
            IExceptionHandlingService exceptionHandler,
            ITransactionManagerService transactionManager,
            IParaBirimiCeviriciService paraBirimiCevirici)
        {
            _unitOfWork = unitOfWork;
            _logger = logger;
            _paraBirimiService = paraBirimiService;
            _exceptionHandler = exceptionHandler;
            _transactionManager = transactionManager;
            _paraBirimiCevirici = paraBirimiCevirici;
        }

        public async Task<IEnumerable<Cari>> GetAllAsync()
        {
            return await _unitOfWork.EntityCariRepository.GetAllAsync(c => !c.Silindi, q => q.OrderBy(c => c.Ad));
        }

        public async Task<Cari> GetByIdAsync(Guid id)
        {
            return await _exceptionHandler.HandleExceptionAsync(
                async () => await _unitOfWork.EntityCariRepository.GetByIdAsync(id),
                "GetByIdAsync",
                id);
        }

        public async Task<Cari> AddAsync(Cari cari)
        {
            return await _exceptionHandler.HandleExceptionAsync(async () => 
            {
                // Transaction başlat
                await _unitOfWork.BeginTransactionAsync();
                try
                {
                    // Cari ekle
                    await _unitOfWork.CariRepository.AddAsync(cari);
                    await _unitOfWork.SaveChangesAsync();
                    
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
                        
                        await _unitOfWork.CariHareketRepository.AddAsync(acilisBakiyeHareketi);
                        await _unitOfWork.SaveChangesAsync();
                    }
                    
                    // Transaction'ı commit et
                    await _unitOfWork.CommitTransactionAsync();
                    return cari;
                }
                catch
                {
                    // Hata durumunda rollback yap
                    await _unitOfWork.RollbackTransactionAsync();
                    throw;
                }
            }, "AddAsync", cari.CariID, cari.Ad, cari.BaslangicBakiye);
        }

        public async Task<Cari> UpdateAsync(Cari cari)
        {
            return await _exceptionHandler.HandleExceptionAsync(async () => 
            {
                // Transaction başlat
                await _unitOfWork.BeginTransactionAsync();
                try
                {
                    cari.GuncellemeTarihi = DateTime.Now;
                    
                    await _unitOfWork.CariRepository.UpdateAsync(cari);
                    await _unitOfWork.SaveChangesAsync();
                    
                    // Transaction'ı commit et
                    await _unitOfWork.CommitTransactionAsync();
                    
                    return cari;
                }
                catch
                {
                    // Hata durumunda rollback yap
                    await _unitOfWork.RollbackTransactionAsync();
                    throw;
                }
            }, "UpdateAsync", cari.CariID, cari.Ad);
        }

        public async Task DeleteAsync(Cari cari)
        {
            await _exceptionHandler.HandleExceptionAsync(async () => 
            {
                // Transaction başlat
                await _unitOfWork.BeginTransactionAsync();
                try
                {
                    // İlgili cariye ait hareketleri kontrol et
                    bool hareketVarMi = await _unitOfWork.CariHareketRepository.AnyAsync(ch => 
                        ch.CariID == cari.CariID && !ch.Silindi);
                    
                    if (hareketVarMi)
                    {
                        _logger.LogWarning($"Cari silinemiyor, ilişkili hareketleri var: ID={cari.CariID}, Ad={cari.Ad}");
                        // Hard delete yerine soft delete uygula
                        cari.Silindi = true;
                        cari.AktifMi = false;
                        cari.GuncellemeTarihi = DateTime.Now;
                        
                        await _unitOfWork.CariRepository.UpdateAsync(cari);
                        await _unitOfWork.SaveChangesAsync();
                        _logger.LogInformation($"Cari pasife alındı (soft delete): ID={cari.CariID}, Ad={cari.Ad}");
                    }
                    else
                    {
                        // İlişkili kayıtlar yoksa hard delete yapabilirsin
                        // Veya yine soft delete tercih edilebilir
                        cari.Silindi = true;
                        cari.GuncellemeTarihi = DateTime.Now;
                        
                        await _unitOfWork.CariRepository.UpdateAsync(cari);
                        await _unitOfWork.SaveChangesAsync();
                        _logger.LogInformation($"Cari silindi: ID={cari.CariID}, Ad={cari.Ad}");
                    }
                    
                    // Transaction'ı commit et
                    await _unitOfWork.CommitTransactionAsync();
                }
                catch
                {
                    // Hata durumunda rollback yap
                    await _unitOfWork.RollbackTransactionAsync();
                    throw;
                }
                
                return true;
            }, "DeleteAsync", cari.CariID, cari.Ad);
        }

        /// <summary>
        /// Cari bakiyesini hesaplar
        /// </summary>
        public async Task<decimal> CariBakiyeHesaplaAsync(Guid cariId, DateTime? tarih = null, Guid? paraBirimiId = null)
        {
            try
            {
                _logger.LogInformation($"CariBakiyeHesaplaAsync başladı: CariID={cariId}, Tarih={tarih}, ParaBirimiID={paraBirimiId}");
                
                // Cari'yi repository üzerinden getir
                var cari = await _unitOfWork.EntityCariRepository.GetByIdAsync(cariId);
                if (cari == null)
                {
                    _logger.LogWarning($"Bakiye hesaplanamadı: Cari bulunamadı (ID: {cariId})");
                    throw new Exception($"Cari bulunamadı (ID: {cariId})");
                }
                
                // Cari hareketlerini getir
                var hareketler = await _unitOfWork.EntityCariRepository.GetCariHareketlerAsync(cariId);
                
                // Tarih filtresi uygula
                if (tarih.HasValue)
                {
                    hareketler = hareketler.Where(ch => ch.Tarih <= tarih.Value).ToList();
                }
                
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
                foreach (var hareket in hareketler
                    .Where(h => h.HareketTuru != "Açılış bakiyesi")
                    .OrderBy(h => h.Tarih))
                {
                    // Bakiye düzeltmesi ise
                    if (hareket.HareketTuru == "Bakiye Düzeltmesi")
                    {
                        bakiye = hareket.Alacak - hareket.Borc;
                    }
                    else
                    {
                        bakiye += hareket.Alacak - hareket.Borc;
                    }
                }
                
                // Para birimi dönüşümü
                if (paraBirimiId.HasValue && paraBirimiId != cari.VarsayilanParaBirimiId && cari.VarsayilanParaBirimiId.HasValue)
                {
                    // Hedef para birimi kodunu getir
                    var hedefParaBirimi = await _paraBirimiService.GetByIdAsync(paraBirimiId.Value);
                    var kaynakParaBirimi = await _paraBirimiService.GetByIdAsync(cari.VarsayilanParaBirimiId.Value);
                    
                    if (hedefParaBirimi != null && kaynakParaBirimi != null)
                    {
                        // Para birimi dönüşümü
                        bakiye = await _paraBirimiCevirici.TutarDonusturAsync(
                            bakiye, 
                            kaynakParaBirimi.ParaBirimiKodu, 
                            hedefParaBirimi.ParaBirimiKodu);
                    }
                }
                
                _logger.LogInformation($"CariBakiyeHesaplaAsync tamamlandı: CariID={cariId}, Tarih={tarih}, ParaBirimiID={paraBirimiId}, Bakiye={bakiye}");
                
                // Hesaplanan bakiyeyi cache'le
                _bakiyeSozlugu[cariId] = bakiye;
                
                return bakiye;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Bakiye hesaplanırken hata: CariID={cariId}, Tarih={tarih}, ParaBirimiID={paraBirimiId}");
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
                var cari = await _unitOfWork.EntityCariRepository.GetByIdAsync(cariId);
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
                var tumHareketler = await _unitOfWork.EntityCariRepository.GetCariHareketlerAsync(cariId);
                
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
                    // Para birimlerini bul
                    var kaynakParaBirimi = await _paraBirimiService.GetByIdAsync(cari.VarsayilanParaBirimiId.Value);
                    var hedefParaBirimi = await _paraBirimiService.GetByIdAsync(paraBirimiId.Value);
                    
                    if (kaynakParaBirimi != null && hedefParaBirimi != null)
                    {
                        // Başlangıç bakiyesini çevir
                        baslangicBakiyesi = await _paraBirimiCevirici.TutarCevirAsync(
                            baslangicBakiyesi, 
                            kaynakParaBirimi.ParaBirimiKodu, 
                            hedefParaBirimi.ParaBirimiKodu);
                        
                        // Son bakiyeyi çevir
                        bakiye = await _paraBirimiCevirici.TutarCevirAsync(
                            bakiye, 
                            kaynakParaBirimi.ParaBirimiKodu, 
                            hedefParaBirimi.ParaBirimiKodu);
                        
                        // Hareketleri çevir
                        await _paraBirimiCevirici.EntityKoleksiyonuCevirAsync(
                            gosterilecekHareketler,
                            hareket => hareket.Tutar,
                            (hareket, deger) => hareket.Tutar = deger,
                            kaynakParaBirimi.ParaBirimiKodu,
                            hedefParaBirimi.ParaBirimiKodu);
                        
                        await _paraBirimiCevirici.EntityKoleksiyonuCevirAsync(
                            gosterilecekHareketler,
                            hareket => hareket.Borc,
                            (hareket, deger) => hareket.Borc = deger,
                            kaynakParaBirimi.ParaBirimiKodu,
                            hedefParaBirimi.ParaBirimiKodu);
                        
                        await _paraBirimiCevirici.EntityKoleksiyonuCevirAsync(
                            gosterilecekHareketler,
                            hareket => hareket.Alacak,
                            (hareket, deger) => hareket.Alacak = deger,
                            kaynakParaBirimi.ParaBirimiKodu,
                            hedefParaBirimi.ParaBirimiKodu);
                        
                        // Sözlükteki bakiye değerlerini güncelle
                        foreach (var hareket in gosterilecekHareketler)
                        {
                            if (_bakiyeSozlugu.ContainsKey(hareket.CariHareketID))
                            {
                                _bakiyeSozlugu[hareket.CariHareketID] = await _paraBirimiCevirici.TutarCevirAsync(
                                    _bakiyeSozlugu[hareket.CariHareketID],
                                    kaynakParaBirimi.ParaBirimiKodu,
                                    hedefParaBirimi.ParaBirimiKodu);
                            }
                        }
                    }
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