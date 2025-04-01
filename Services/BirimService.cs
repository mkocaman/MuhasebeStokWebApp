using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using MuhasebeStokWebApp.Data;
using MuhasebeStokWebApp.Data.Entities;
using MuhasebeStokWebApp.Services.Interfaces;

namespace MuhasebeStokWebApp.Services
{
    public class BirimService : IBirimService
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogService _logService;
        private readonly IMemoryCache _cache;
        private const string BirimlerCacheKey = "BirimlerList";
        private const int CacheExpirationMinutes = 15;

        public BirimService(ApplicationDbContext context, ILogService logService, IMemoryCache cache)
        {
            _context = context;
            _logService = logService;
            _cache = cache;
        }

        public async Task<IEnumerable<Birim>> GetAllAsync()
        {
            // Önbellek kontrolü
            if (_cache.TryGetValue(BirimlerCacheKey, out IEnumerable<Birim> cachedBirimler))
            {
                return cachedBirimler;
            }

            // Veritabanından verileri al
            var birimler = await _context.Birimler
                .AsNoTracking() // Performans için tracking'i devre dışı bırak
                .Where(b => b.Aktif && !b.Silindi)
                .OrderBy(b => b.BirimAdi)
                .ToListAsync();

            // Önbelleğe kaydet
            _cache.Set(BirimlerCacheKey, birimler, TimeSpan.FromMinutes(CacheExpirationMinutes));

            return birimler;
        }

        public async Task<Birim> GetByIdAsync(Guid id)
        {
            // ID'ye göre önbellek anahtarı
            var cacheKey = $"Birim_{id}";

            // Önbellek kontrolü
            if (_cache.TryGetValue(cacheKey, out Birim cachedBirim))
            {
                return cachedBirim;
            }

            // Veritabanından veriyi al
            var birim = await _context.Birimler
                .AsNoTracking()
                .FirstOrDefaultAsync(b => b.BirimID == id && b.Aktif && !b.Silindi);

            if (birim != null)
            {
                // Önbelleğe kaydet
                _cache.Set(cacheKey, birim, TimeSpan.FromMinutes(CacheExpirationMinutes));
            }

            return birim;
        }

        public async Task<Birim> AddAsync(Birim birim)
        {
            if (birim == null)
            {
                await _logService.AddLogAsync("Hata", "Birim eklenemedi: Birim nesnesi null.", "BirimService/AddAsync");
                return null;
            }

            // Aynı isimde birim var mı?
            if (await IsBirimNameExistsAsync(birim.BirimAdi))
            {
                await _logService.AddLogAsync("Uyarı", $"Aynı isimde birim zaten mevcut: {birim.BirimAdi}", "BirimService/AddAsync");
                return null;
            }

            birim.Aktif = true;
            birim.Silindi = false;
            birim.OlusturmaTarihi = DateTime.Now;
            
            _context.Birimler.Add(birim);
            await _context.SaveChangesAsync();
            
            // Önbelleği temizle
            await ClearCacheAsync();
            
            await _logService.AddLogAsync("Bilgi", $"{birim.BirimAdi} birimi eklendi.", "BirimService/AddAsync");
            return birim;
        }

        public async Task<Birim> UpdateAsync(Birim birim)
        {
            if (birim == null)
            {
                await _logService.AddLogAsync("Hata", "Birim güncellenemedi: Birim nesnesi null.", "BirimService/UpdateAsync");
                return null;
            }

            // Mevcut birimi bul
            var existingBirim = await _context.Birimler.FindAsync(birim.BirimID);
            if (existingBirim == null)
            {
                await _logService.AddLogAsync("Hata", $"Birim güncellenemedi: {birim.BirimID} ID'li birim bulunamadı.", "BirimService/UpdateAsync");
                return null;
            }

            // Aynı isimde başka birim var mı?
            if (await IsBirimNameExistsAsync(birim.BirimAdi, birim.BirimID))
            {
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

            // Değişiklikleri kaydet
            await _context.SaveChangesAsync();
            
            // Önbelleği temizle
            await ClearCacheAsync();
            
            await _logService.AddLogAsync("Bilgi", $"{birim.BirimAdi} birimi güncellendi.", "BirimService/UpdateAsync");
            return existingBirim;
        }

        public async Task<bool> DeleteAsync(Guid id)
        {
            var birim = await _context.Birimler.FindAsync(id);
            if (birim == null)
            {
                await _logService.AddLogAsync("Hata", $"Birim silinemedi: {id} ID'li birim bulunamadı.", "BirimService/DeleteAsync");
                return false;
            }

            // Birim kullanımda mı kontrol et
            if (await IsBirimInUseAsync(id))
            {
                await _logService.AddLogAsync("Uyarı", $"Birim silinemedi: {birim.BirimAdi} birimi ürünler tarafından kullanılıyor.", "BirimService/DeleteAsync");
                return false;
            }

            // Soft delete
            birim.Aktif = false;
            birim.Silindi = true;
            birim.GuncellemeTarihi = DateTime.Now;
            
            await _context.SaveChangesAsync();
            
            // Önbelleği temizle
            await ClearCacheAsync();
            
            await _logService.AddLogAsync("Bilgi", $"{birim.BirimAdi} birimi silindi.", "BirimService/DeleteAsync");
            return true;
        }

        public async Task<bool> IsBirimInUseAsync(Guid id)
        {
            return await _context.Urunler
                .AsNoTracking()
                .AnyAsync(u => u.BirimID == id && !u.Silindi);
        }

        public async Task<bool> IsBirimNameExistsAsync(string birimAdi, Guid? excludeBirimId = null)
        {
            if (string.IsNullOrWhiteSpace(birimAdi))
                return false;

            var query = _context.Birimler
                .AsNoTracking()
                .Where(b => b.BirimAdi.ToLower() == birimAdi.Trim().ToLower() && !b.Silindi);
            
            if (excludeBirimId.HasValue)
                query = query.Where(b => b.BirimID != excludeBirimId.Value);
            
            return await query.AnyAsync();
        }

        public async Task<bool> ClearCacheAsync()
        {
            try
            {
                _cache.Remove(BirimlerCacheKey);
                
                // Başarıyla tamamlandı
                return true;
            }
            catch (Exception ex)
            {
                await _logService.AddLogAsync("Hata", $"Birim önbelleği temizlenirken hata oluştu: {ex.Message}", "BirimService/ClearCacheAsync");
                return false;
            }
        }
    }
} 