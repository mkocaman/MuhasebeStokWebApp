using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using MuhasebeStokWebApp.Data;
using MuhasebeStokWebApp.Data.Entities;

namespace MuhasebeStokWebApp.Services
{
    public class ParaBirimiService : IParaBirimiService
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogService _logService;

        public ParaBirimiService(ApplicationDbContext context, ILogService logService)
        {
            _context = context;
            _logService = logService;
        }

        public async Task<List<ParaBirimi>> GetAllParaBirimleriAsync()
        {
            return await _context.ParaBirimleri
                .Where(p => !p.Silindi)
                .OrderBy(p => p.Sira)
                .ToListAsync();
        }

        public async Task<List<ParaBirimi>> GetAktifParaBirimleriAsync()
        {
            return await _context.ParaBirimleri
                .Where(p => p.Aktif && !p.Silindi)
                .OrderBy(p => p.Sira)
                .ToListAsync();
        }

        public async Task<ParaBirimi> GetParaBirimiByIdAsync(Guid paraBirimiId)
        {
            return await _context.ParaBirimleri
                .FirstOrDefaultAsync(p => p.ParaBirimiID == paraBirimiId && !p.Silindi);
        }

        public async Task<ParaBirimi> GetParaBirimiByKodAsync(string kod)
        {
            return await _context.ParaBirimleri
                .FirstOrDefaultAsync(p => p.Kod == kod && !p.Silindi);
        }

        public async Task<ParaBirimi> AddParaBirimiAsync(ParaBirimi paraBirimi)
        {
            // Kod tekrarı kontrolü
            var existingParaBirimi = await _context.ParaBirimleri
                .FirstOrDefaultAsync(p => p.Kod == paraBirimi.Kod && !p.Silindi);
            
            if (existingParaBirimi != null)
                throw new InvalidOperationException($"'{paraBirimi.Kod}' kodlu para birimi zaten mevcut.");

            // Sıra değerini belirle (en büyük sıra + 1)
            var maxSira = await _context.ParaBirimleri
                .Where(p => !p.Silindi)
                .OrderByDescending(p => p.Sira)
                .Select(p => p.Sira)
                .FirstOrDefaultAsync();

            paraBirimi.Sira = maxSira + 1;
            paraBirimi.ParaBirimiID = Guid.NewGuid();
            paraBirimi.Silindi = false;
            paraBirimi.OlusturmaTarihi = DateTime.Now;

            _context.ParaBirimleri.Add(paraBirimi);
            await _context.SaveChangesAsync();
            
            // Log kaydı oluştur
            await _logService.AddLogAsync("ParaBirimi", "Ekleme", 
                $"Para Birimi: {paraBirimi.Ad} ({paraBirimi.Kod}) eklendi.", 
                paraBirimi.ParaBirimiID.ToString());
            
            return paraBirimi;
        }

        public async Task<ParaBirimi> UpdateParaBirimiAsync(ParaBirimi paraBirimi)
        {
            var existingParaBirimi = await _context.ParaBirimleri
                .FirstOrDefaultAsync(p => p.ParaBirimiID == paraBirimi.ParaBirimiID);
            
            if (existingParaBirimi == null)
                throw new InvalidOperationException("Güncellenecek para birimi bulunamadı.");
            
            // Eğer kod değişmişse, tekrar kontrolü yap
            if (existingParaBirimi.Kod != paraBirimi.Kod)
            {
                var isDuplicate = await _context.ParaBirimleri
                    .AnyAsync(p => p.Kod == paraBirimi.Kod && p.ParaBirimiID != paraBirimi.ParaBirimiID && !p.Silindi);
                
                if (isDuplicate)
                    throw new InvalidOperationException($"'{paraBirimi.Kod}' kodlu başka bir para birimi zaten mevcut.");
            }
            
            string eskiDegerler = $"Eski Değerler - Ad: {existingParaBirimi.Ad}, Kod: {existingParaBirimi.Kod}, Sembol: {existingParaBirimi.Sembol}, Aktif: {existingParaBirimi.Aktif}";
            
            existingParaBirimi.Ad = paraBirimi.Ad;
            existingParaBirimi.Kod = paraBirimi.Kod;
            existingParaBirimi.Sembol = paraBirimi.Sembol;
            existingParaBirimi.Aktif = paraBirimi.Aktif;
            existingParaBirimi.Aciklama = paraBirimi.Aciklama;
            existingParaBirimi.Format = paraBirimi.Format;
            existingParaBirimi.GuncellemeTarihi = DateTime.Now;
            
            await _context.SaveChangesAsync();
            
            // Log kaydı oluştur
            await _logService.AddLogAsync("ParaBirimi", "Güncelleme", 
                $"Para Birimi: {paraBirimi.Ad} ({paraBirimi.Kod}) güncellendi. {eskiDegerler}", 
                paraBirimi.ParaBirimiID.ToString());
            
            return existingParaBirimi;
        }

        public async Task<bool> DeleteParaBirimiAsync(Guid paraBirimiId)
        {
            var paraBirimi = await _context.ParaBirimleri.FindAsync(paraBirimiId);
            if (paraBirimi == null)
                return false;
            
            // İlişkili kayıtları kontrol et
            bool hasKurDegerleri = await _context.KurDegerleri
                .AnyAsync(k => k.ParaBirimiID == paraBirimiId && !k.Silindi);
                
            if (hasKurDegerleri)
                throw new InvalidOperationException("Bu para birimine ait kur değerleri bulunmaktadır. Önce kur değerlerini silmelisiniz.");
            
            paraBirimi.Silindi = true;
            paraBirimi.Aktif = false;
            paraBirimi.GuncellemeTarihi = DateTime.Now;
            
            await _context.SaveChangesAsync();
            
            // Log kaydı oluştur
            await _logService.AddLogAsync("ParaBirimi", "Silme", 
                $"Para Birimi: {paraBirimi.Ad} ({paraBirimi.Kod}) silindi.", 
                paraBirimiId.ToString());
            
            return true;
        }

        public async Task<List<ParaBirimiIliski>> GetAllParaBirimiIliskileriAsync()
        {
            return await _context.ParaBirimiIliskileri
                .Include(p => p.KaynakParaBirimi)
                .Include(p => p.HedefParaBirimi)
                .Where(p => !p.Silindi)
                .OrderBy(p => p.KaynakParaBirimi.Kod)
                .ThenBy(p => p.HedefParaBirimi.Kod)
                .ToListAsync();
        }

        public async Task<List<ParaBirimiIliski>> GetAktifParaBirimiIliskileriAsync()
        {
            return await _context.ParaBirimiIliskileri
                .Include(p => p.KaynakParaBirimi)
                .Include(p => p.HedefParaBirimi)
                .Where(p => p.Aktif && !p.Silindi)
                .OrderBy(p => p.KaynakParaBirimi.Kod)
                .ThenBy(p => p.HedefParaBirimi.Kod)
                .ToListAsync();
        }

        public async Task<List<ParaBirimiIliski>> GetParaBirimiIliskileriAsync(Guid paraBirimiId)
        {
            return await _context.ParaBirimiIliskileri
                .Include(p => p.KaynakParaBirimi)
                .Include(p => p.HedefParaBirimi)
                .Where(p => (p.KaynakParaBirimiID == paraBirimiId || p.HedefParaBirimiID == paraBirimiId) && !p.Silindi)
                .OrderBy(p => p.KaynakParaBirimi.Kod)
                .ThenBy(p => p.HedefParaBirimi.Kod)
                .ToListAsync();
        }

        public async Task<ParaBirimiIliski> GetParaBirimiIliskiByIdAsync(Guid iliskiId)
        {
            return await _context.ParaBirimiIliskileri
                .Include(p => p.KaynakParaBirimi)
                .Include(p => p.HedefParaBirimi)
                .FirstOrDefaultAsync(p => p.ParaBirimiIliskiID == iliskiId && !p.Silindi);
        }

        public async Task<ParaBirimiIliski> AddParaBirimiIliskiAsync(ParaBirimiIliski paraBirimiIliski)
        {
            // İlişki tekrarı kontrolü
            var existingIliski = await _context.ParaBirimiIliskileri
                .FirstOrDefaultAsync(p => p.KaynakParaBirimiID == paraBirimiIliski.KaynakParaBirimiID && 
                                          p.HedefParaBirimiID == paraBirimiIliski.HedefParaBirimiID && 
                                          !p.Silindi);
            
            if (existingIliski != null)
                throw new InvalidOperationException("Bu para birimleri arasında zaten bir ilişki tanımlanmış.");

            paraBirimiIliski.ParaBirimiIliskiID = Guid.NewGuid();
            paraBirimiIliski.Silindi = false;
            paraBirimiIliski.OlusturmaTarihi = DateTime.Now;
            paraBirimiIliski.GuncellemeTarihi = DateTime.Now;

            _context.ParaBirimiIliskileri.Add(paraBirimiIliski);
            await _context.SaveChangesAsync();
            
            // Log kaydı oluştur
            await _logService.AddLogAsync("ParaBirimiIliski", "Ekleme", 
                $"Para Birimi İlişkisi eklendi. Kaynak: {paraBirimiIliski.KaynakParaBirimiID}, Hedef: {paraBirimiIliski.HedefParaBirimiID}, Çarpan: {paraBirimiIliski.Carpan}", 
                paraBirimiIliski.ParaBirimiIliskiID.ToString());
            
            return paraBirimiIliski;
        }

        public async Task<ParaBirimiIliski> UpdateParaBirimiIliskiAsync(ParaBirimiIliski paraBirimiIliski)
        {
            var existingIliski = await _context.ParaBirimiIliskileri
                .FirstOrDefaultAsync(p => p.ParaBirimiIliskiID == paraBirimiIliski.ParaBirimiIliskiID);
            
            if (existingIliski == null)
                throw new InvalidOperationException("Güncellenecek para birimi ilişkisi bulunamadı.");
            
            string eskiDegerler = $"Eski Değerler - Çarpan: {existingIliski.Carpan}, Aktif: {existingIliski.Aktif}";
            
            existingIliski.Carpan = paraBirimiIliski.Carpan;
            existingIliski.Aktif = paraBirimiIliski.Aktif;
            existingIliski.GuncellemeTarihi = DateTime.Now;
            
            await _context.SaveChangesAsync();
            
            // Log kaydı oluştur
            await _logService.AddLogAsync("ParaBirimiIliski", "Güncelleme", 
                $"Para Birimi İlişkisi güncellendi. {eskiDegerler}, Yeni Değerler - Çarpan: {paraBirimiIliski.Carpan}, Aktif: {paraBirimiIliski.Aktif}", 
                paraBirimiIliski.ParaBirimiIliskiID.ToString());
            
            return existingIliski;
        }

        public async Task<bool> DeleteParaBirimiIliskiAsync(Guid iliskiId)
        {
            var iliski = await _context.ParaBirimiIliskileri.FindAsync(iliskiId);
            if (iliski == null)
                return false;
            
            iliski.Silindi = true;
            iliski.Aktif = false;
            iliski.GuncellemeTarihi = DateTime.Now;
            
            await _context.SaveChangesAsync();
            
            // Log kaydı oluştur
            await _logService.AddLogAsync("ParaBirimiIliski", "Silme", 
                $"Para Birimi İlişkisi silindi. ParaBirimiIliskiID: {iliskiId}", 
                iliskiId.ToString());
            
            return true;
        }

        public async Task<bool> HasParaBirimiIliskiAsync(Guid paraBirimiId)
        {
            return await _context.ParaBirimiIliskileri
                .AnyAsync(p => (p.KaynakParaBirimiID == paraBirimiId || p.HedefParaBirimiID == paraBirimiId) && 
                              !p.Silindi);
        }

        public async Task<ParaBirimiIliski> GetIliskiByParaBirimleriAsync(Guid kaynakId, Guid hedefId)
        {
            return await _context.ParaBirimiIliskileri
                .FirstOrDefaultAsync(p => p.KaynakParaBirimiID == kaynakId && 
                                         p.HedefParaBirimiID == hedefId && 
                                         p.Aktif && 
                                         !p.Silindi);
        }

        public async Task<IDictionary<string, decimal>> GetKurDegerleriByParaBirimiAsync(string paraBirimiKodu)
        {
            return new Dictionary<string, decimal>();
        }

        public ParaBirimiIliski CreateParaBirimiIliski(
            Guid kaynakParaBirimiID, 
            Guid hedefParaBirimiID, 
            decimal carpan)
        {
            return null;
        }

        public async Task<bool> UpdateParaBirimiSiralamaAsync(List<Guid> paraBirimiIdSiralama)
        {
            if (paraBirimiIdSiralama == null || !paraBirimiIdSiralama.Any())
                return false;
                
            // Toplu güncelleme için önce tüm para birimlerini getir
            var paraBirimleri = await _context.ParaBirimleri
                .Where(p => paraBirimiIdSiralama.Contains(p.ParaBirimiID) && !p.Silindi)
                .ToListAsync();
                
            if (!paraBirimleri.Any())
                return false;
                
            // Sıralama güncelle
            for (int i = 0; i < paraBirimiIdSiralama.Count; i++)
            {
                var paraBirimi = paraBirimleri.FirstOrDefault(p => p.ParaBirimiID == paraBirimiIdSiralama[i]);
                if (paraBirimi != null)
                {
                    paraBirimi.Sira = i + 1;
                    paraBirimi.GuncellemeTarihi = DateTime.Now;
                }
            }
            
            var updatedCount = await _context.SaveChangesAsync();
            
            // Log kaydı oluştur
            await _logService.AddLogAsync("ParaBirimi", "Sıralama Güncelleme", 
                $"{updatedCount} adet para biriminin sıralaması güncellendi.");
                
            return updatedCount > 0;
        }

        public async Task<bool> VarsayilanParaBirimleriniEkleAsync()
        {
            if (await _context.ParaBirimleri.AnyAsync())
                return false; // Zaten para birimi var, işlem yapma
                
            var varsayilanParaBirimleri = new List<ParaBirimi>
            {
                new ParaBirimi 
                { 
                    ParaBirimiID = Guid.NewGuid(), 
                    Ad = "Türk Lirası",
                    Kod = "TRY", 
                    Sembol = "₺", 
                    Format = "{0:N2} ₺",
                    Aciklama = "Türkiye Cumhuriyeti resmi para birimi",
                    Sira = 1,
                    Aktif = true,
                    OlusturmaTarihi = DateTime.Now,
                    GuncellemeTarihi = DateTime.Now,
                    Silindi = false
                },
                new ParaBirimi 
                { 
                    ParaBirimiID = Guid.NewGuid(), 
                    Ad = "Amerikan Doları",
                    Kod = "USD", 
                    Sembol = "$", 
                    Format = "$ {0:N2}",
                    Aciklama = "Amerika Birleşik Devletleri resmi para birimi",
                    Sira = 2,
                    Aktif = true,
                    OlusturmaTarihi = DateTime.Now,
                    GuncellemeTarihi = DateTime.Now,
                    Silindi = false
                },
                new ParaBirimi 
                { 
                    ParaBirimiID = Guid.NewGuid(), 
                    Ad = "Euro",
                    Kod = "EUR", 
                    Sembol = "€", 
                    Format = "€ {0:N2}",
                    Aciklama = "Avrupa Birliği ortak para birimi",
                    Sira = 3,
                    Aktif = true,
                    OlusturmaTarihi = DateTime.Now,
                    GuncellemeTarihi = DateTime.Now,
                    Silindi = false
                },
                new ParaBirimi 
                { 
                    ParaBirimiID = Guid.NewGuid(), 
                    Ad = "İngiliz Sterlini",
                    Kod = "GBP", 
                    Sembol = "£", 
                    Format = "£ {0:N2}",
                    Aciklama = "Birleşik Krallık resmi para birimi",
                    Sira = 4,
                    Aktif = true,
                    OlusturmaTarihi = DateTime.Now,
                    GuncellemeTarihi = DateTime.Now,
                    Silindi = false
                }
            };
            
            await _context.ParaBirimleri.AddRangeAsync(varsayilanParaBirimleri);
            var eklenenSayisi = await _context.SaveChangesAsync();
            
            // Log kaydı oluştur
            await _logService.AddLogAsync("ParaBirimi", "Toplu Ekleme", 
                $"{eklenenSayisi} adet varsayılan para birimi eklendi.");
                
            return eklenenSayisi > 0;
        }
    }
} 