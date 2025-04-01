using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using MuhasebeStokWebApp.Data;
using MuhasebeStokWebApp.Data.Entities;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using MuhasebeStokWebApp.ViewModels.SistemAyar;
using MuhasebeStokWebApp.Services.Interfaces;

namespace MuhasebeStokWebApp.Services
{
    public class SistemAyarService : ISistemAyarService
    {
        private readonly ApplicationDbContext _context;
        private readonly IMemoryCache _cache;
        private readonly ILogger<SistemAyarService> _logger;
        private readonly ILogService _logService;
        private const string CacheKey = "SistemAyarlar";

        public SistemAyarService(
            ApplicationDbContext context, 
            IMemoryCache cache,
            ILogger<SistemAyarService> logger,
            ILogService logService)
        {
            _context = context;
            _cache = cache;
            _logger = logger;
            _logService = logService;
        }

        public async Task<IEnumerable<KeyValuePair<string, string>>> GetAllAsync()
        {
            if (_cache.TryGetValue(CacheKey, out List<KeyValuePair<string, string>> ayarlar))
            {
                return ayarlar;
            }

            var dbAyarlar = await _context.SistemAyarlari
                .Where(a => !a.Silindi)
                .OrderBy(a => a.Anahtar)
                .ToListAsync();

            ayarlar = dbAyarlar.Select(a => new KeyValuePair<string, string>(a.Anahtar, a.Deger)).ToList();

            var cacheOptions = new MemoryCacheEntryOptions()
                .SetSlidingExpiration(TimeSpan.FromMinutes(10));
            _cache.Set(CacheKey, ayarlar, cacheOptions);

            return ayarlar;
        }

        public async Task<string> GetSettingAsync(string key)
        {
            var ayarlar = await GetAllAsync();
            var ayar = ayarlar.FirstOrDefault(a => a.Key == key);
            return ayar.Value;
        }

        public async Task SaveSettingAsync(string key, string value)
        {
            var ayar = await _context.SistemAyarlari.FirstOrDefaultAsync(a => a.Anahtar == key && !a.Silindi);
            
            if (ayar == null)
            {
                ayar = new Data.Entities.SistemAyar
                {
                    Anahtar = key,
                    Deger = value,
                    Aciklama = $"{key} ayarı",
                    OlusturmaTarihi = DateTime.Now,
                    GuncellemeTarihi = DateTime.Now
                };
                _context.SistemAyarlari.Add(ayar);
            }
            else
            {
                ayar.Deger = value;
                ayar.GuncellemeTarihi = DateTime.Now;
            }

            await _context.SaveChangesAsync();
            _cache.Remove(CacheKey);
            
            await _logService.AddLogAsync("Bilgi", $"Sistem ayarı güncellendi: {key}", "SistemAyarService/SaveSettingAsync");
        }

        public async Task SaveEmailSettingsAsync(EmailAyarlariViewModel model)
        {
            // Bu metot, e-posta ayarlarını veritabanına kaydeder
            await SaveSettingAsync("SmtpServer", model.SmtpServer);
            await SaveSettingAsync("SmtpPort", model.SmtpPort.ToString());
            await SaveSettingAsync("SmtpUsername", model.Username);
            
            // Şifre değiştirilmişse kaydet
            if (!model.Password.Contains("*"))
            {
                await SaveSettingAsync("SmtpPassword", model.Password);
            }
            
            await SaveSettingAsync("SenderEmail", model.SenderEmail);
            await SaveSettingAsync("SenderName", model.SenderName);
            await SaveSettingAsync("SmtpUseSsl", model.UseSsl.ToString());
            await SaveSettingAsync("AdminEmails", model.AdminEmails);
            
            await _logService.LogInfoAsync("SistemAyarService.SaveEmailSettingsAsync", "E-posta ayarları güncellendi");
        }

        public async Task SaveNotificationSettingsAsync(BildirimAyarlariViewModel model)
        {
            await SaveSettingAsync("BildirimAktif", model.BildirimAktif.ToString());
            await SaveSettingAsync("EmailBildirimAktif", model.EmailBildirimAktif.ToString());
            await SaveSettingAsync("PushBildirimAktif", model.PushBildirimAktif.ToString());
            await SaveSettingAsync("OtomatikBildirimler", model.OtomatikBildirimler.ToString());
            await SaveSettingAsync("BildirimOzetiSikligi", model.BildirimOzetiSikligi);
            await SaveSettingAsync("KritikOlayBildirimleri", model.KritikOlayBildirimleri.ToString());
            await SaveSettingAsync("StokUyariBildirimleri", model.StokUyariBildirimleri.ToString());
            await SaveSettingAsync("TahsilatOdemeBildirimleri", model.TahsilatOdemeBildirimleri.ToString());
            await SaveSettingAsync("YeniSiparisBildirimleri", model.YeniSiparisBildirimleri.ToString());
            
            await _logService.LogInfoAsync("SistemAyarService.SaveNotificationSettingsAsync", "Bildirim ayarları güncellendi");
        }

        public async Task SaveLanguageSettingsAsync(DilAyarlariViewModel model)
        {
            await SaveSettingAsync("VarsayilanDil", model.VarsayilanDil);
            await SaveSettingAsync("ParaBirimiFormat", model.ParaBirimiFormat);
            await SaveSettingAsync("TarihFormat", model.TarihFormat);
            await SaveSettingAsync("SaatFormat", model.SaatFormat);
            await SaveSettingAsync("CokluDilDestegi", model.CokluDilDestegi.ToString());
            
            // Aktif dilleri kaydet
            if (model.AktifDiller != null && model.AktifDiller.Any())
            {
                var aktifDillerString = string.Join(";", model.AktifDiller);
                await SaveSettingAsync("AktifDiller", aktifDillerString);
            }
            else
            {
                await SaveSettingAsync("AktifDiller", "tr-TR");
            }
            
            await _logService.LogInfoAsync("SistemAyarService.SaveLanguageSettingsAsync", "Dil ayarları güncellendi");
        }

        public async Task<string> GetAnaDovizKoduAsync()
        {
            var ayar = await GetAktifSistemAyariAsync();
            return ayar?.AnaDovizKodu ?? "TRY";
        }

        public async Task<DateTime> GetSonDovizGuncellemeTarihiAsync()
        {
            var ayar = await GetAktifSistemAyariAsync();
            return ayar?.SonDovizGuncellemeTarihi ?? DateTime.MinValue;
        }

        public async Task<bool> UpdateSonDovizGuncellemeTarihiAsync(DateTime yeniTarih)
        {
            try
            {
                var ayar = await GetAktifSistemAyariAsync();
                if (ayar != null)
                {
                    ayar.SonDovizGuncellemeTarihi = yeniTarih;
                    ayar.GuncellemeTarihi = DateTime.Now;
                    await _context.SaveChangesAsync();
                    return true;
                }
                return false;
            }
            catch (Exception ex)
            {
                await _logService.LogErrorAsync("SistemAyarService.UpdateSonDovizGuncellemeTarihiAsync", ex.Message);
                return false;
            }
        }

        public async Task<List<SistemAyarlari>> GetAllSistemAyarlariAsync()
        {
            try
            {
                return await _context.GenelSistemAyarlari.Where(s => !s.Silindi).ToListAsync();
            }
            catch (Exception ex)
            {
                await _logService.LogErrorAsync("SistemAyarService.GetAllSistemAyarlariAsync", ex.Message);
                return new List<SistemAyarlari>();
            }
        }

        public async Task<SistemAyarlari> GetAktifSistemAyariAsync()
        {
            try
            {
                return await _context.GenelSistemAyarlari
                    .FirstOrDefaultAsync(s => s.Aktif && !s.Silindi);
            }
            catch (Exception ex)
            {
                await _logService.LogErrorAsync("SistemAyarService.GetAktifSistemAyariAsync", ex.Message);
                return null;
            }
        }

        public async Task<SistemAyarlari> GetSistemAyariByIdAsync(int sistemAyariId)
        {
            try
            {
                return await _context.GenelSistemAyarlari
                    .FirstOrDefaultAsync(s => s.SistemAyarlariID == sistemAyariId && !s.Silindi);
            }
            catch (Exception ex)
            {
                await _logService.LogErrorAsync("SistemAyarService.GetSistemAyariByIdAsync", ex.Message);
                return null;
            }
        }

        public async Task<SistemAyarlari> GetSistemAyariByDovizKoduAsync(string dovizKodu)
        {
            try
            {
                return await _context.GenelSistemAyarlari
                    .FirstOrDefaultAsync(s => s.AnaDovizKodu == dovizKodu && !s.Silindi);
            }
            catch (Exception ex)
            {
                await _logService.LogErrorAsync("SistemAyarService.GetSistemAyariByDovizKoduAsync", ex.Message);
                return null;
            }
        }

        public async Task<SistemAyarlari> UpdateSistemAyariAsync(SistemAyarlari sistemAyari)
        {
            try
            {
                var existingSistemAyari = await _context.GenelSistemAyarlari.FindAsync(sistemAyari.SistemAyarlariID);
                if (existingSistemAyari == null)
                {
                    return null;
                }

                // Var olan özellikleri güncelle
                existingSistemAyari.SirketAdi = sistemAyari.SirketAdi;
                existingSistemAyari.SirketAdresi = sistemAyari.SirketAdresi;
                existingSistemAyari.SirketTelefon = sistemAyari.SirketTelefon;
                existingSistemAyari.SirketEmail = sistemAyari.SirketEmail;
                existingSistemAyari.SirketVergiNo = sistemAyari.SirketVergiNo;
                existingSistemAyari.SirketVergiDairesi = sistemAyari.SirketVergiDairesi;
                existingSistemAyari.AnaDovizKodu = sistemAyari.AnaDovizKodu;
                existingSistemAyari.OtomatikDovizGuncelleme = sistemAyari.OtomatikDovizGuncelleme;
                existingSistemAyari.DovizGuncellemeSikligi = sistemAyari.DovizGuncellemeSikligi;
                existingSistemAyari.AktifParaBirimleri = sistemAyari.AktifParaBirimleri;
                existingSistemAyari.GuncellemeTarihi = DateTime.Now;
                
                await _context.SaveChangesAsync();
                return existingSistemAyari;
            }
            catch (Exception ex)
            {
                await _logService.LogErrorAsync("SistemAyarService.UpdateSistemAyariAsync", ex.Message);
                return null;
            }
        }

        public async Task<bool> UpdateAktifSistemAyariAsync(int sistemAyariId)
        {
            try
            {
                // Önce mevcut aktif olan ayarların aktifliğini kaldır
                var sistemAyarlari = await _context.GenelSistemAyarlari
                    .Where(s => s.Aktif && !s.Silindi)
                    .ToListAsync();
                    
                foreach (var ayar in sistemAyarlari)
                {
                    ayar.Aktif = false;
                    ayar.GuncellemeTarihi = DateTime.Now;
                }

                // Yeni ayarı aktif yap
                var yeniAktifAyar = await _context.GenelSistemAyarlari.FindAsync(sistemAyariId);
                if (yeniAktifAyar != null)
                {
                    yeniAktifAyar.Aktif = true;
                    yeniAktifAyar.GuncellemeTarihi = DateTime.Now;
                    await _context.SaveChangesAsync();
                    return true;
                }
                return false;
            }
            catch (Exception ex)
            {
                await _logService.LogErrorAsync("SistemAyarService.UpdateAktifSistemAyariAsync", ex.Message);
                return false;
            }
        }

        public async Task<bool> UpdateSistemAyariDurumAsync(int sistemAyariId, bool aktif)
        {
            try
            {
                var sistemAyari = await _context.GenelSistemAyarlari.FindAsync(sistemAyariId);
                if (sistemAyari != null)
                {
                    // Eğer ayarı aktif yapıyorsak ve başka aktif ayar varsa, önce onu deaktif yap
                    if (aktif)
                    {
                        var digerAktifAyarlar = await _context.GenelSistemAyarlari
                            .Where(s => s.Aktif && s.SistemAyarlariID != sistemAyariId && !s.Silindi)
                            .ToListAsync();
                            
                        foreach (var ayar in digerAktifAyarlar)
                        {
                            ayar.Aktif = false;
                            ayar.GuncellemeTarihi = DateTime.Now;
                        }
                    }

                    sistemAyari.Aktif = aktif;
                    sistemAyari.GuncellemeTarihi = DateTime.Now;
                    await _context.SaveChangesAsync();
                    return true;
                }
                return false;
            }
            catch (Exception ex)
            {
                await _logService.LogErrorAsync("SistemAyarService.UpdateSistemAyariDurumAsync", ex.Message);
                return false;
            }
        }

        public async Task<bool> DeleteSistemAyariAsync(int sistemAyariId)
        {
            try
            {
                var sistemAyari = await _context.GenelSistemAyarlari.FindAsync(sistemAyariId);
                if (sistemAyari != null)
                {
                    // Soft delete uygula
                    sistemAyari.Silindi = true;
                    sistemAyari.Aktif = false;
                    sistemAyari.GuncellemeTarihi = DateTime.Now;
                    await _context.SaveChangesAsync();
                    return true;
                }
                return false;
            }
            catch (Exception ex)
            {
                await _logService.LogErrorAsync("SistemAyarService.DeleteSistemAyariAsync", ex.Message);
                return false;
            }
        }

        public async Task<SistemAyarlari> AddSistemAyariAsync(SistemAyarlari sistemAyari)
        {
            try
            {
                sistemAyari.OlusturmaTarihi = DateTime.Now;
                sistemAyari.Silindi = false;
                
                // Eğer bu ayar aktif olacaksa, diğer aktif ayarları deaktif yap
                if (sistemAyari.Aktif)
                {
                    var aktifAyarlar = await _context.GenelSistemAyarlari
                        .Where(s => s.Aktif && !s.Silindi)
                        .ToListAsync();
                        
                    foreach (var ayar in aktifAyarlar)
                    {
                        ayar.Aktif = false;
                        ayar.GuncellemeTarihi = DateTime.Now;
                    }
                }

                _context.GenelSistemAyarlari.Add(sistemAyari);
                await _context.SaveChangesAsync();
                return sistemAyari;
            }
            catch (Exception ex)
            {
                await _logService.LogErrorAsync("SistemAyarService.AddSistemAyariAsync", ex.Message);
                return null;
            }
        }

        public async Task<bool> VarsayilanSistemAyarlariniEkleAsync()
        {
            try
            {
                // Varsayılan ayar oluştur
                var varsayilanAyar = new SistemAyarlari
                {
                    SirketAdi = "Muhasebe ve Stok Takip Sistemi",
                    SirketAdresi = "Varsayılan Adres",
                    SirketTelefon = "0212 123 45 67",
                    SirketEmail = "info@muhasebe-stok.com",
                    SirketVergiNo = "1234567890",
                    SirketVergiDairesi = "Varsayılan Vergi Dairesi",
                    AnaDovizKodu = "TRY",
                    OtomatikDovizGuncelleme = true,
                    DovizGuncellemeSikligi = 24,
                    SonDovizGuncellemeTarihi = DateTime.Now,
                    AktifParaBirimleri = "TRY;USD;EUR;GBP",
                    Aktif = true,
                    OlusturmaTarihi = DateTime.Now
                };

                _context.GenelSistemAyarlari.Add(varsayilanAyar);
                await _context.SaveChangesAsync();
                return true;
            }
            catch (Exception ex)
            {
                await _logService.LogErrorAsync("SistemAyarService.VarsayilanSistemAyarlariniEkleAsync", ex.Message);
                return false;
            }
        }

        public async Task<int> GetDovizGuncellemeZamaniAsync()
        {
            var ayar = await GetAktifSistemAyariAsync();
            return ayar?.DovizGuncellemeSikligi ?? 24; // Varsayılan olarak 24 saat
        }
    }
} 