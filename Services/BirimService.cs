using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using MuhasebeStokWebApp.Data;
using MuhasebeStokWebApp.Data.Entities;
using MuhasebeStokWebApp.Services.Interfaces;

namespace MuhasebeStokWebApp.Services
{
    public class BirimService : IBirimService
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogService _logService;

        public BirimService(ApplicationDbContext context, ILogService logService)
        {
            _context = context;
            _logService = logService;
        }

        public async Task<IEnumerable<UrunBirim>> GetAllAsync()
        {
            return await _context.Birimler
                .Where(b => b.Aktif && !b.Silindi)
                .OrderBy(b => b.BirimAdi)
                .ToListAsync();
        }

        public async Task<UrunBirim> GetByIdAsync(int id)
        {
            return await _context.Birimler
                .FirstOrDefaultAsync(b => b.BirimID == id && b.Aktif && !b.Silindi);
        }

        public async Task<UrunBirim> AddAsync(UrunBirim birim)
        {
            if (birim == null)
            {
                await _logService.AddLogAsync("Hata", "Birim eklenemedi: Birim nesnesi null.", "BirimService/AddAsync");
                return null;
            }

            birim.Aktif = true;
            birim.Silindi = false;
            
            _context.Birimler.Add(birim);
            await _context.SaveChangesAsync();
            
            await _logService.AddLogAsync("Bilgi", $"{birim.BirimAdi} birimi eklendi.", "BirimService/AddAsync");
            return birim;
        }

        public async Task<UrunBirim> UpdateAsync(UrunBirim birim)
        {
            if (birim == null)
            {
                await _logService.AddLogAsync("Hata", "Birim güncellenemedi: Birim nesnesi null.", "BirimService/UpdateAsync");
                return null;
            }

            var existingBirim = await _context.Birimler.FindAsync(birim.BirimID);
            if (existingBirim == null)
            {
                await _logService.AddLogAsync("Hata", $"Birim güncellenemedi: {birim.BirimID} ID'li birim bulunamadı.", "BirimService/UpdateAsync");
                return null;
            }

            existingBirim.BirimAdi = birim.BirimAdi;
            existingBirim.Aciklama = birim.Aciklama;
            existingBirim.Aktif = birim.Aktif;
            existingBirim.GuncellemeTarihi = DateTime.Now;

            await _context.SaveChangesAsync();
            
            await _logService.AddLogAsync("Bilgi", $"{birim.BirimAdi} birimi güncellendi.", "BirimService/UpdateAsync");
            return existingBirim;
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var birim = await _context.Birimler.FindAsync(id);
            if (birim == null)
            {
                await _logService.AddLogAsync("Hata", $"Birim silinemedi: {id} ID'li birim bulunamadı.", "BirimService/DeleteAsync");
                return false;
            }

            // Soft delete
            birim.Aktif = false;
            birim.Silindi = true;
            birim.GuncellemeTarihi = DateTime.Now;
            
            await _context.SaveChangesAsync();
            
            await _logService.AddLogAsync("Bilgi", $"{birim.BirimAdi} birimi silindi.", "BirimService/DeleteAsync");
            return true;
        }
    }
} 