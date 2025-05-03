using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using MuhasebeStokWebApp.Data;
using MuhasebeStokWebApp.Data.Entities;
using MuhasebeStokWebApp.Data.Repositories;
using MuhasebeStokWebApp.Services.Interfaces;
using MuhasebeStokWebApp.Extensions;
using Microsoft.Extensions.Logging;

namespace MuhasebeStokWebApp.Services
{
    public class BirimService : IBirimService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogService _logService;
        private readonly IMemoryCache _cache;
        private readonly ILogger<BirimService> _logger;
        private readonly IExceptionHandlingService _exceptionHandler;
        private const string BirimlerCacheKey = "BirimlerList";
        private const int CacheExpirationMinutes = 15;

        public BirimService(
            IUnitOfWork unitOfWork, 
            ILogService logService, 
            IMemoryCache cache,
            ILogger<BirimService> logger,
            IExceptionHandlingService exceptionHandler)
        {
            _unitOfWork = unitOfWork;
            _logService = logService;
            _cache = cache;
            _logger = logger;
            _exceptionHandler = exceptionHandler;
        }

        public async Task<IEnumerable<Birim>> GetAllAsync()
        {
            return await _exceptionHandler.HandleExceptionAsync(async () => 
            {
                // Önbellek kontrolü
                if (_cache.TryGetValue(BirimlerCacheKey, out IEnumerable<Birim> cachedBirimler))
                {
                    return cachedBirimler;
                }

                // Veritabanından verileri al - Tüm birimleri al (aktif/pasif/silinmiş)
                var birimler = await _unitOfWork.Repository<Birim>().GetAllAsync(
                    filter: null,
                    includeProperties: "",
                    ignoreQueryFilters: true,
                    asNoTracking: true);

                var orderedBirimler = birimler.OrderBy(b => b.BirimAdi).ToList();

                // Önbelleğe kaydet
                _cache.Set(BirimlerCacheKey, orderedBirimler, TimeSpan.FromMinutes(CacheExpirationMinutes));

                return orderedBirimler;
            }, "GetAllAsync");
        }

        public async Task<Birim> GetByIdAsync(Guid id)
        {
            return await _exceptionHandler.HandleExceptionAsync(async () => 
            {
                // ID'ye göre önbellek anahtarı
                var cacheKey = $"Birim_{id}";

                // Önbellek kontrolü
                if (_cache.TryGetValue(cacheKey, out Birim cachedBirim))
                {
                    return cachedBirim;
                }

                // Veritabanından veriyi al - global filtreleri devre dışı bırak
                var birim = await _unitOfWork.Repository<Birim>().GetFirstOrDefaultAsync(
                    b => b.BirimID == id,
                    includeProperties: "",
                    ignoreQueryFilters: true,
                    asNoTracking: true);

                if (birim != null)
                {
                    // Önbelleğe kaydet
                    _cache.Set(cacheKey, birim, TimeSpan.FromMinutes(CacheExpirationMinutes));
                }

                return birim;
            }, "GetByIdAsync", id);
        }

        public async Task<Birim> AddAsync(Birim birim)
        {
            return await _exceptionHandler.HandleExceptionAsync(async () => 
            {
                _logger.LogMethodStart(birim?.BirimAdi);

                if (birim == null)
                {
                    _logger.LogWarning("Birim eklenemedi: Birim nesnesi null.");
                    await _logService.AddLogAsync("Hata", "Birim eklenemedi: Birim nesnesi null.", "BirimService/AddAsync");
                    return null;
                }

                // Aynı isimde birim var mı?
                if (await IsBirimNameExistsAsync(birim.BirimAdi))
                {
                    _logger.LogWarning($"Aynı isimde birim zaten mevcut: {birim.BirimAdi}");
                    await _logService.AddLogAsync("Uyarı", $"Aynı isimde birim zaten mevcut: {birim.BirimAdi}", "BirimService/AddAsync");
                    return null;
                }

                birim.Aktif = true;
                birim.Silindi = false;
                birim.OlusturmaTarihi = DateTime.Now;
                
                await _unitOfWork.Repository<Birim>().AddAsync(birim);
                await _unitOfWork.SaveChangesAsync();
                
                // Önbelleği temizle
                await ClearCacheAsync();
                
                _logger.LogEntityOperation(birim, "ekleme", "BirimID");
                await _logService.AddLogAsync("Bilgi", $"{birim.BirimAdi} birimi eklendi.", "BirimService/AddAsync");
                
                return birim;
            }, "AddAsync", birim?.BirimAdi);
        }

        public async Task<Birim> UpdateAsync(Birim birim)
        {
            return await _exceptionHandler.HandleExceptionAsync(async () => 
            {
                _logger.LogMethodStart(birim?.BirimAdi, birim?.BirimID);

                if (birim == null)
                {
                    _logger.LogWarning("Birim güncellenemedi: Birim nesnesi null.");
                    await _logService.AddLogAsync("Hata", "Birim güncellenemedi: Birim nesnesi null.", "BirimService/UpdateAsync");
                    return null;
                }

                // Mevcut birimi bul
                var existingBirim = await _unitOfWork.Repository<Birim>().GetByIdAsync(birim.BirimID);
                if (existingBirim == null)
                {
                    _logger.LogWarning($"Birim güncellenemedi: {birim.BirimID} ID'li birim bulunamadı.");
                    await _logService.AddLogAsync("Hata", $"Birim güncellenemedi: {birim.BirimID} ID'li birim bulunamadı.", "BirimService/UpdateAsync");
                    return null;
                }

                // Aynı isimde başka birim var mı?
                if (await IsBirimNameExistsAsync(birim.BirimAdi, birim.BirimID))
                {
                    _logger.LogWarning($"Aynı isimde birim zaten mevcut: {birim.BirimAdi}");
                    await _logService.AddLogAsync("Uyarı", $"Aynı isimde birim zaten mevcut: {birim.BirimAdi}", "BirimService/UpdateAsync");
                    return null;
                }

                // Değişiklikleri uygula
                existingBirim.BirimAdi = birim.BirimAdi;
                existingBirim.BirimKodu = birim.BirimKodu;
                existingBirim.BirimSembol = birim.BirimSembol;
                existingBirim.Aciklama = birim.Aciklama;
                existingBirim.Aktif = birim.Aktif;
                existingBirim.GuncellemeTarihi = DateTime.Now;
                existingBirim.SonGuncelleyenKullaniciID = birim.SonGuncelleyenKullaniciID;

                _unitOfWork.Repository<Birim>().Update(existingBirim);
                // Değişiklikleri kaydet
                await _unitOfWork.SaveChangesAsync();
                
                // Önbelleği temizle
                await ClearCacheAsync();
                
                _logger.LogEntityOperation(existingBirim, "güncelleme", "BirimID");
                await _logService.AddLogAsync("Bilgi", $"{birim.BirimAdi} birimi güncellendi.", "BirimService/UpdateAsync");
                
                return existingBirim;
            }, "UpdateAsync", birim?.BirimAdi, birim?.BirimID);
        }

        public async Task<bool> DeleteAsync(Guid id)
        {
            return await _exceptionHandler.HandleExceptionAsync(async () => 
            {
                _logger.LogMethodStart(id.ToString());

                var birim = await _unitOfWork.Repository<Birim>().GetByIdAsync(id);
                if (birim == null)
                {
                    _logger.LogWarning($"Birim silinemedi: {id} ID'li birim bulunamadı.");
                    await _logService.AddLogAsync("Hata", $"Birim silinemedi: {id} ID'li birim bulunamadı.", "BirimService/DeleteAsync");
                    return false;
                }

                // Birim kullanımda mı kontrol et
                if (await IsBirimInUseAsync(id))
                {
                    _logger.LogWarning($"Birim silinemedi: {birim.BirimAdi} birimi ürünler tarafından kullanılıyor.");
                    await _logService.AddLogAsync("Uyarı", $"Birim silinemedi: {birim.BirimAdi} birimi ürünler tarafından kullanılıyor.", "BirimService/DeleteAsync");
                    return false;
                }

                // Soft delete
                birim.Aktif = false;
                birim.Silindi = true;
                birim.GuncellemeTarihi = DateTime.Now;
                
                _unitOfWork.Repository<Birim>().Update(birim);
                await _unitOfWork.SaveChangesAsync();
                
                // Önbelleği temizle
                await ClearCacheAsync();
                
                _logger.LogEntityOperation(birim, "silme", "BirimID");
                await _logService.AddLogAsync("Bilgi", $"{birim.BirimAdi} birimi silindi.", "BirimService/DeleteAsync");
                
                return true;
            }, "DeleteAsync", id);
        }

        public async Task<bool> IsBirimInUseAsync(Guid id)
        {
            return await _exceptionHandler.HandleExceptionAsync(async () => 
            {
                var urunCount = await _unitOfWork.Repository<Urun>().CountAsync(
                    u => u.BirimID == id && !u.Silindi, 
                    asNoTracking: true);
                    
                return urunCount > 0;
            }, "IsBirimInUseAsync", id);
        }

        public async Task<bool> IsBirimNameExistsAsync(string birimAdi, Guid? excludeBirimId = null)
        {
            return await _exceptionHandler.HandleExceptionAsync(async () => 
            {
                if (string.IsNullOrWhiteSpace(birimAdi))
                    return false;
                    
                var query = await _unitOfWork.Repository<Birim>().GetAllAsync(
                    b => b.BirimAdi.ToLower() == birimAdi.Trim().ToLower() && !b.Silindi,
                    asNoTracking: true);
                
                if (excludeBirimId.HasValue)
                    query = query.Where(b => b.BirimID != excludeBirimId.Value);
                
                return query.Any();
            }, "IsBirimNameExistsAsync", birimAdi, excludeBirimId);
        }

        public async Task<bool> ClearCacheAsync()
        {
            return await _exceptionHandler.HandleExceptionAsync(async () => 
            {
                _logger.LogMethodStart();
                
                _logger.LogInformation("Birim önbellekleri temizleniyor...");
                
                // Ana birim listesi önbelleğini temizle
                _cache.Remove(BirimlerCacheKey);
                
                // Tüm birim önbelleklerini temizle
                var allBirimIdsQuery = await _unitOfWork.Repository<Birim>().GetAllAsync(
                    filter: null,
                    selector: b => b.BirimID,
                    asNoTracking: true);
                
                foreach (var birimId in allBirimIdsQuery)
                {
                    _cache.Remove($"Birim_{birimId}");
                }
                
                // Ürün listesi ve ürün detay önbelleklerini temizle
                _cache.Remove("UrunlerList");
                _cache.Remove("AktifUrunlerList");
                _cache.Remove("UrunDropdown");
                
                // Ürün listeleme ve detay önbelleklerini temizle
                _cache.Remove("UrunlerTumu");
                _cache.Remove("UrunlerAktif");
                _cache.Remove("UrunlerPasif");
                _cache.Remove("UrunlerSilinmis");
                _cache.Remove("UrunlerTopN");
                _cache.Remove("UrunlerSonEklenenler");
                _cache.Remove("UrunlerStokDusuk");
                _cache.Remove("UrunlerFiyatListesi");
                
                // UrunService özelleştirilmiş ürün listeleri
                for (int i = 1; i <= 10; i++)
                {
                    _cache.Remove($"UrunlerPage_{i}");
                    _cache.Remove($"UrunlerAktifPage_{i}");
                    _cache.Remove($"UrunlerStokluPage_{i}");
                }
                
                // Ürün kategorilere göre listeleme önbelleklerini temizle
                var kategoriIds = await _unitOfWork.Repository<UrunKategori>().GetAllAsync(
                    filter: null,
                    selector: k => k.KategoriID,
                    asNoTracking: true);
                    
                foreach (var kategoriId in kategoriIds)
                {
                    _cache.Remove($"UrunlerByKategori_{kategoriId}");
                    _cache.Remove($"UrunlerSayisiByKategori_{kategoriId}");
                    
                    // Sayfalı kategorileri temizle
                    for (int i = 1; i <= 10; i++)
                    {
                        _cache.Remove($"UrunlerByKategori_{kategoriId}_Page_{i}");
                    }
                }
                
                // Tüm ürünlerin detay önbelleklerini temizle
                var urunIds = await _unitOfWork.Repository<Urun>().GetAllAsync(
                    filter: null,
                    selector: u => u.UrunID,
                    asNoTracking: true);
                    
                foreach (var urunId in urunIds)
                {
                    _cache.Remove($"Urun_{urunId}");
                    _cache.Remove($"UrunDetails_{urunId}");
                    _cache.Remove($"UrunStok_{urunId}");
                    _cache.Remove($"UrunFiyat_{urunId}");
                    _cache.Remove($"UrunStokDurumu_{urunId}");
                    _cache.Remove($"UrunKategoriler_{urunId}");
                }
                
                // ViewComponent önbelleklerini temizle
                _cache.Remove("UrunDropdown");
                _cache.Remove("BirimDropdown");
                _cache.Remove("KategoriDropdown");
                _cache.Remove("DepoDropdown");
                _cache.Remove("FiyatTipiDropdown");
                
                // Kategori önbelleklerini temizle
                _cache.Remove("KategoriList");
                _cache.Remove("AktifKategoriList");
                _cache.Remove("KategoriListWithUrunCount");
                
                // İstatistik önbelleklerini temizle
                _cache.Remove("UrunStokIstatistik");
                _cache.Remove("BirimIstatistik");
                _cache.Remove("KategoriIstatistik");
                
                _logger.LogInformation("Tüm birim, ürün ve ilgili önbellekler başarıyla temizlendi");
                _logger.LogMethodEnd(true);
                return true;
            }, "ClearCacheAsync");
        }
    }
} 