using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using MuhasebeStokWebApp.Data;
using MuhasebeStokWebApp.Data.Entities.DovizModulu;
using MuhasebeStokWebApp.Services.Interfaces;

namespace MuhasebeStokWebApp.Services.DovizModulu
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

        #region Para Birimi İşlemleri
        public async Task<List<ParaBirimi>> GetAllParaBirimleriAsync()
        {
            try
            {
                return await _context.ParaBirimleri
                    .Where(p => !p.Silindi)
                    .OrderBy(p => p.Sira)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                await _logService.LogErrorAsync("ParaBirimiService.GetAllParaBirimleriAsync", $"Hata: {ex.Message}");
                return new List<ParaBirimi>();
            }
        }

        public async Task<List<ParaBirimi>> GetAktifParaBirimleriAsync()
        {
            try
            {
                return await _context.ParaBirimleri
                    .Where(p => p.Aktif && !p.Silindi)
                    .OrderBy(p => p.Sira)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                await _logService.LogErrorAsync("ParaBirimiService.GetAktifParaBirimleriAsync", $"Hata: {ex.Message}");
                return new List<ParaBirimi>();
            }
        }

        public async Task<ParaBirimi> GetParaBirimiByIdAsync(Guid paraBirimiId)
        {
            try
            {
                return await _context.ParaBirimleri
                    .FirstOrDefaultAsync(p => p.ParaBirimiID == paraBirimiId && !p.Silindi);
            }
            catch (Exception ex)
            {
                await _logService.LogErrorAsync("ParaBirimiService.GetParaBirimiByIdAsync", $"ParaBirimiID: {paraBirimiId}, Hata: {ex.Message}");
                return null;
            }
        }

        public async Task<ParaBirimi> GetParaBirimiByKodAsync(string kod)
        {
            try
            {
                if (string.IsNullOrEmpty(kod))
                    return null;

                return await _context.ParaBirimleri
                    .FirstOrDefaultAsync(p => p.Kod == kod && !p.Silindi);
            }
            catch (Exception ex)
            {
                await _logService.LogErrorAsync("ParaBirimiService.GetParaBirimiByKodAsync", $"Kod: {kod}, Hata: {ex.Message}");
                return null;
            }
        }

        public async Task<ParaBirimi> AddParaBirimiAsync(ParaBirimi paraBirimi)
        {
            try
            {
                if (paraBirimi == null)
                    throw new ArgumentNullException(nameof(paraBirimi), "Para birimi null olamaz");

                // Oluşturma bilgilerini ayarla
                paraBirimi.OlusturmaTarihi = DateTime.Now;
                
                await _context.ParaBirimleri.AddAsync(paraBirimi);
                await _context.SaveChangesAsync();
                
                await _logService.LogInfoAsync("ParaBirimiService.AddParaBirimiAsync", $"Para birimi eklendi: {paraBirimi.Ad} ({paraBirimi.Kod})");
                
                return paraBirimi;
            }
            catch (Exception ex)
            {
                await _logService.LogErrorAsync("ParaBirimiService.AddParaBirimiAsync", $"Hata: {ex.Message}");
                throw;
            }
        }

        public async Task<ParaBirimi> UpdateParaBirimiAsync(ParaBirimi paraBirimi)
        {
            try
            {
                if (paraBirimi == null)
                    throw new ArgumentNullException(nameof(paraBirimi), "Para birimi null olamaz");

                var existingParaBirimi = await _context.ParaBirimleri
                    .FirstOrDefaultAsync(p => p.ParaBirimiID == paraBirimi.ParaBirimiID && !p.Silindi);

                if (existingParaBirimi == null)
                    throw new KeyNotFoundException($"Para birimi bulunamadı: {paraBirimi.ParaBirimiID}");

                // Güncelleme bilgilerini ayarla
                paraBirimi.GuncellemeTarihi = DateTime.Now;
                
                _context.Entry(existingParaBirimi).CurrentValues.SetValues(paraBirimi);
                await _context.SaveChangesAsync();
                
                await _logService.LogInfoAsync("ParaBirimiService.UpdateParaBirimiAsync", $"Para birimi güncellendi: {paraBirimi.Ad} ({paraBirimi.Kod})");
                
                return paraBirimi;
            }
            catch (Exception ex)
            {
                await _logService.LogErrorAsync("ParaBirimiService.UpdateParaBirimiAsync", $"ParaBirimiID: {paraBirimi?.ParaBirimiID}, Hata: {ex.Message}");
                throw;
            }
        }

        public async Task<bool> DeleteParaBirimiAsync(Guid paraBirimiId)
        {
            try
            {
                var paraBirimi = await _context.ParaBirimleri
                    .FirstOrDefaultAsync(p => p.ParaBirimiID == paraBirimiId && !p.Silindi);

                if (paraBirimi == null)
                    throw new KeyNotFoundException($"Para birimi bulunamadı: {paraBirimiId}");

                // İlişkilerde kullanılıyorsa silinmemeli
                var iliskiVar = await HasParaBirimiIliskiAsync(paraBirimiId);
                if (iliskiVar)
                    throw new InvalidOperationException($"Para birimi ({paraBirimi.Kod}) ilişkilerde kullanılıyor, silinemez");

                // Soft delete
                paraBirimi.Silindi = true;
                paraBirimi.Aktif = false;
                paraBirimi.GuncellemeTarihi = DateTime.Now;
                
                await _context.SaveChangesAsync();
                await _logService.LogInfoAsync("ParaBirimiService.DeleteParaBirimiAsync", $"Para birimi silindi: {paraBirimi.Ad} ({paraBirimi.Kod})");
                
                return true;
            }
            catch (Exception ex)
            {
                await _logService.LogErrorAsync("ParaBirimiService.DeleteParaBirimiAsync", $"ParaBirimiID: {paraBirimiId}, Hata: {ex.Message}");
                throw;
            }
        }
        #endregion

        #region Para Birimi İlişkileri
        public async Task<List<DovizIliski>> GetAllParaBirimiIliskileriAsync()
        {
            try
            {
                return await _context.DovizIliskileri
                    .Include(i => i.KaynakParaBirimi)
                    .Include(i => i.HedefParaBirimi)
                    .Where(i => !i.Silindi)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                await _logService.LogErrorAsync("ParaBirimiService.GetAllParaBirimiIliskileriAsync", $"Hata: {ex.Message}");
                return new List<DovizIliski>();
            }
        }

        public async Task<List<DovizIliski>> GetAktifParaBirimiIliskileriAsync()
        {
            try
            {
                return await _context.DovizIliskileri
                    .Include(i => i.KaynakParaBirimi)
                    .Include(i => i.HedefParaBirimi)
                    .Where(i => i.Aktif && !i.Silindi)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                await _logService.LogErrorAsync("ParaBirimiService.GetAktifParaBirimiIliskileriAsync", $"Hata: {ex.Message}");
                return new List<DovizIliski>();
            }
        }

        public async Task<List<DovizIliski>> GetParaBirimiIliskileriAsync(Guid paraBirimiId)
        {
            try
            {
                return await _context.DovizIliskileri
                    .Include(i => i.KaynakParaBirimi)
                    .Include(i => i.HedefParaBirimi)
                    .Where(i => (i.KaynakParaBirimiID == paraBirimiId || i.HedefParaBirimiID == paraBirimiId) && !i.Silindi)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                await _logService.LogErrorAsync("ParaBirimiService.GetParaBirimiIliskileriAsync", $"ParaBirimiID: {paraBirimiId}, Hata: {ex.Message}");
                return new List<DovizIliski>();
            }
        }

        public async Task<DovizIliski> GetParaBirimiIliskiByIdAsync(Guid iliskiId)
        {
            try
            {
                return await _context.DovizIliskileri
                    .Include(i => i.KaynakParaBirimi)
                    .Include(i => i.HedefParaBirimi)
                    .FirstOrDefaultAsync(i => i.DovizIliskiID == iliskiId && !i.Silindi);
            }
            catch (Exception ex)
            {
                await _logService.LogErrorAsync("ParaBirimiService.GetParaBirimiIliskiByIdAsync", $"IliskiID: {iliskiId}, Hata: {ex.Message}");
                return null;
            }
        }

        public async Task<DovizIliski> GetIliskiByParaBirimleriAsync(Guid kaynakId, Guid hedefId)
        {
            try
            {
                return await _context.DovizIliskileri
                    .Include(i => i.KaynakParaBirimi)
                    .Include(i => i.HedefParaBirimi)
                    .FirstOrDefaultAsync(i => i.KaynakParaBirimiID == kaynakId && i.HedefParaBirimiID == hedefId && i.Aktif && !i.Silindi);
            }
            catch (Exception ex)
            {
                await _logService.LogErrorAsync("ParaBirimiService.GetIliskiByParaBirimleriAsync", $"KaynakID: {kaynakId}, HedefID: {hedefId}, Hata: {ex.Message}");
                return null;
            }
        }

        public async Task<DovizIliski> AddParaBirimiIliskiAsync(DovizIliski dovizIliski)
        {
            try
            {
                if (dovizIliski == null)
                    throw new ArgumentNullException(nameof(dovizIliski), "İlişki null olamaz");

                if (dovizIliski.KaynakParaBirimiID == dovizIliski.HedefParaBirimiID)
                    throw new InvalidOperationException("Kaynak ve hedef para birimi aynı olamaz");

                // Zaten var mı kontrol et
                var existingIliski = await _context.DovizIliskileri
                    .FirstOrDefaultAsync(i => i.KaynakParaBirimiID == dovizIliski.KaynakParaBirimiID && 
                                             i.HedefParaBirimiID == dovizIliski.HedefParaBirimiID && 
                                             !i.Silindi);

                if (existingIliski != null)
                    throw new InvalidOperationException("Bu para birimleri arasında zaten bir ilişki var");

                // Oluşturma bilgilerini ayarla
                dovizIliski.OlusturmaTarihi = DateTime.Now;
                
                await _context.DovizIliskileri.AddAsync(dovizIliski);
                await _context.SaveChangesAsync();
                
                await _logService.LogInfoAsync("ParaBirimiService.AddParaBirimiIliskiAsync", $"Para birimi ilişkisi eklendi: {dovizIliski.KaynakParaBirimiID} -> {dovizIliski.HedefParaBirimiID}");
                
                return dovizIliski;
            }
            catch (Exception ex)
            {
                await _logService.LogErrorAsync("ParaBirimiService.AddParaBirimiIliskiAsync", $"Hata: {ex.Message}");
                throw;
            }
        }

        public async Task<DovizIliski> UpdateParaBirimiIliskiAsync(DovizIliski dovizIliski)
        {
            try
            {
                if (dovizIliski == null)
                    throw new ArgumentNullException(nameof(dovizIliski), "İlişki null olamaz");

                var existingIliski = await _context.DovizIliskileri
                    .FirstOrDefaultAsync(i => i.DovizIliskiID == dovizIliski.DovizIliskiID && !i.Silindi);

                if (existingIliski == null)
                    throw new KeyNotFoundException($"İlişki bulunamadı: {dovizIliski.DovizIliskiID}");

                // Güncelleme bilgilerini ayarla
                dovizIliski.GuncellemeTarihi = DateTime.Now;
                
                _context.Entry(existingIliski).CurrentValues.SetValues(dovizIliski);
                await _context.SaveChangesAsync();
                
                await _logService.LogInfoAsync("ParaBirimiService.UpdateParaBirimiIliskiAsync", $"Para birimi ilişkisi güncellendi: {dovizIliski.DovizIliskiID}");
                
                return dovizIliski;
            }
            catch (Exception ex)
            {
                await _logService.LogErrorAsync("ParaBirimiService.UpdateParaBirimiIliskiAsync", $"IliskiID: {dovizIliski?.DovizIliskiID}, Hata: {ex.Message}");
                throw;
            }
        }

        public async Task<bool> DeleteParaBirimiIliskiAsync(Guid iliskiId)
        {
            try
            {
                var iliski = await _context.DovizIliskileri
                    .FirstOrDefaultAsync(i => i.DovizIliskiID == iliskiId && !i.Silindi);

                if (iliski == null)
                    throw new KeyNotFoundException($"İlişki bulunamadı: {iliskiId}");

                // Soft delete
                iliski.Silindi = true;
                iliski.Aktif = false;
                iliski.GuncellemeTarihi = DateTime.Now;
                
                await _context.SaveChangesAsync();
                await _logService.LogInfoAsync("ParaBirimiService.DeleteParaBirimiIliskiAsync", $"Para birimi ilişkisi silindi: {iliskiId}");
                
                return true;
            }
            catch (Exception ex)
            {
                await _logService.LogErrorAsync("ParaBirimiService.DeleteParaBirimiIliskiAsync", $"IliskiID: {iliskiId}, Hata: {ex.Message}");
                throw;
            }
        }

        public async Task<bool> HasParaBirimiIliskiAsync(Guid paraBirimiId)
        {
            try
            {
                return await _context.DovizIliskileri
                    .AnyAsync(i => (i.KaynakParaBirimiID == paraBirimiId || i.HedefParaBirimiID == paraBirimiId) && 
                                   i.Aktif && !i.Silindi);
            }
            catch (Exception ex)
            {
                await _logService.LogErrorAsync("ParaBirimiService.HasParaBirimiIliskiAsync", $"ParaBirimiID: {paraBirimiId}, Hata: {ex.Message}");
                return false;
            }
        }
        #endregion

        #region Diğer
        public async Task<bool> UpdateParaBirimiSiralamaAsync(List<Guid> paraBirimiIdSiralama)
        {
            try
            {
                if (paraBirimiIdSiralama == null || !paraBirimiIdSiralama.Any())
                    throw new ArgumentException("Sıralama listesi boş olamaz");

                for (int i = 0; i < paraBirimiIdSiralama.Count; i++)
                {
                    var paraBirimiId = paraBirimiIdSiralama[i];
                    var paraBirimi = await _context.ParaBirimleri
                        .FirstOrDefaultAsync(p => p.ParaBirimiID == paraBirimiId && !p.Silindi);

                    if (paraBirimi != null)
                    {
                        paraBirimi.Sira = i;
                        paraBirimi.GuncellemeTarihi = DateTime.Now;
                    }
                }

                await _context.SaveChangesAsync();
                await _logService.LogInfoAsync("ParaBirimiService.UpdateParaBirimiSiralamaAsync", "Para birimi sıralaması güncellendi");
                
                return true;
            }
            catch (Exception ex)
            {
                await _logService.LogErrorAsync("ParaBirimiService.UpdateParaBirimiSiralamaAsync", $"Hata: {ex.Message}");
                throw;
            }
        }

        public async Task<bool> VarsayilanParaBirimleriniEkleAsync()
        {
            try
            {
                // Daha önce para birimi eklenmiş mi kontrol et
                if (await _context.ParaBirimleri.AnyAsync())
                    return false; // Zaten para birimleri var

                var varsayilanParaBirimleri = new List<ParaBirimi>
                {
                    new ParaBirimi
                    {
                        Ad = "Türk Lirası",
                        Kod = "TRY",
                        Sembol = "₺",
                        AnaParaBirimiMi = true,
                        Sira = 0,
                        OndalikAyraci = ",",
                        BinlikAyraci = ".",
                        OndalikHassasiyet = 2,
                        Aciklama = "Varsayılan yerel para birimi",
                        OlusturmaTarihi = DateTime.Now,
                        OlusturanKullaniciID = "System"
                    },
                    new ParaBirimi
                    {
                        Ad = "Amerikan Doları",
                        Kod = "USD",
                        Sembol = "$",
                        AnaParaBirimiMi = false,
                        Sira = 1,
                        OndalikAyraci = ".",
                        BinlikAyraci = ",",
                        OndalikHassasiyet = 2,
                        Aciklama = "Varsayılan yabancı para birimi",
                        OlusturmaTarihi = DateTime.Now,
                        OlusturanKullaniciID = "System"
                    },
                    new ParaBirimi
                    {
                        Ad = "Euro",
                        Kod = "EUR",
                        Sembol = "€",
                        AnaParaBirimiMi = false,
                        Sira = 2,
                        OndalikAyraci = ",",
                        BinlikAyraci = ".",
                        OndalikHassasiyet = 2,
                        Aciklama = "Avrupa Birliği ortak para birimi",
                        OlusturmaTarihi = DateTime.Now,
                        OlusturanKullaniciID = "System"
                    }
                };

                await _context.ParaBirimleri.AddRangeAsync(varsayilanParaBirimleri);
                await _context.SaveChangesAsync();
                
                await _logService.LogInfoAsync("ParaBirimiService.VarsayilanParaBirimleriniEkleAsync", "Varsayılan para birimleri eklendi");
                
                return true;
            }
            catch (Exception ex)
            {
                await _logService.LogErrorAsync("ParaBirimiService.VarsayilanParaBirimleriniEkleAsync", $"Hata: {ex.Message}");
                throw;
            }
        }
        #endregion
    }
} 