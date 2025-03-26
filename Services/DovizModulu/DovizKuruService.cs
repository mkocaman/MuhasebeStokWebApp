using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using MuhasebeStokWebApp.Data;
using MuhasebeStokWebApp.Data.Entities.DovizModulu;
using MuhasebeStokWebApp.Services.Interfaces;
using System.Net.Http;
using System.Xml.Linq;
using System.Globalization;
using Microsoft.Extensions.Logging;

namespace MuhasebeStokWebApp.Services.DovizModulu
{
    public class DovizKuruService : IDovizKuruService
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<DovizKuruService> _logger;
        private readonly ILogService _logService;
        private readonly MuhasebeStokWebApp.Services.Interfaces.ISistemAyarService _sistemAyarService;
        private readonly HttpClient _httpClient;

        public DovizKuruService(
            ApplicationDbContext context,
            ILogger<DovizKuruService> logger,
            ILogService logService,
            MuhasebeStokWebApp.Services.Interfaces.ISistemAyarService sistemAyarService,
            HttpClient httpClient)
        {
            _context = context;
            _logger = logger;
            _logService = logService;
            _sistemAyarService = sistemAyarService;
            _httpClient = httpClient;
        }

        public async Task<List<KurDegeri>> GetKurDegerleriByTarihAsync(DateTime tarih)
        {
            try
            {
                // Belirli bir tarihteki tüm kur değerlerini getir
                return await _context.KurDegerleri
                    .Include(k => k.ParaBirimi)
                    .Where(k => k.Tarih.Date == tarih.Date && !k.Silindi)
                    .OrderBy(k => k.ParaBirimi != null ? k.ParaBirimi.Kod : "")
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                await _logService.LogErrorAsync("DovizKuruService.GetKurDegerleriByTarihAsync", $"Hata: {ex.Message}");
                return new List<KurDegeri>();
            }
        }

        public async Task<KurDegeri?> GetSonKurDegeriByParaBirimiAsync(Guid paraBirimiId)
        {
            try
            {
                // Para birimi ID'sine göre en son kur değerini getir
                var paraBirimi = await _context.ParaBirimleri
                    .FirstOrDefaultAsync(p => p.ParaBirimiID == paraBirimiId && !p.Silindi);
                
                if (paraBirimi == null)
                {
                    return null;
                }

                return await _context.KurDegerleri
                    .Include(k => k.ParaBirimi)
                    .Where(k => k.ParaBirimiID == paraBirimi.ParaBirimiID && !k.Silindi)
                    .OrderByDescending(k => k.Tarih)
                    .FirstOrDefaultAsync();
            }
            catch (Exception ex)
            {
                await _logService.LogErrorAsync("DovizKuruService.GetSonKurDegeriByParaBirimiAsync", $"Hata: {ex.Message}");
                return null;
            }
        }

        public async Task<KurDegeri?> GetSonKurDegeriByKodAsync(string kod)
        {
            try
            {
                // Para birimi koduna göre en son kur değerini getir
                var paraBirimi = await _context.ParaBirimleri
                    .FirstOrDefaultAsync(p => p.Kod == kod && !p.Silindi);

                if (paraBirimi == null)
                {
                    await _logService.LogWarningAsync("DovizKuruService.GetSonKurDegeriByKodAsync", $"Para birimi bulunamadı: {kod}");
                    return null;
                }

                return await GetSonKurDegeriByParaBirimiAsync(paraBirimi.ParaBirimiID);
            }
            catch (Exception ex)
            {
                await _logService.LogErrorAsync("DovizKuruService.GetSonKurDegeriByKodAsync", $"Hata: {ex.Message}");
                return null;
            }
        }

        public async Task<decimal> HesaplaKurDegeriAsync(Guid kaynakParaBirimiId, Guid hedefParaBirimiId, DateTime? tarih = null)
        {
            try
            {
                // Aynı para birimi ise çarpan 1
                if (kaynakParaBirimiId.Equals(hedefParaBirimiId))
                {
                    return 1;
                }

                // Tarih belirtilmemişse bugünü kullan
                var hesaplamaTarihi = tarih?.Date ?? DateTime.Now.Date;

                // Ana para birimini al
                var anaParaBirimiKodu = await _sistemAyarService.GetAnaDovizKoduAsync();
                var anaParaBirimi = await _context.ParaBirimleri
                    .FirstOrDefaultAsync(p => p.Kod == anaParaBirimiKodu && !p.Silindi);

                if (anaParaBirimi == null)
                {
                    throw new Exception($"Ana para birimi bulunamadı: {anaParaBirimiKodu}");
                }

                // Kaynak para birimini al
                var kaynakParaBirimi = await _context.ParaBirimleri
                    .FirstOrDefaultAsync(p => p.ParaBirimiID == kaynakParaBirimiId && !p.Silindi);

                if (kaynakParaBirimi == null)
                {
                    throw new Exception($"Kaynak para birimi bulunamadı: {kaynakParaBirimiId}");
                }

                // Hedef para birimini al
                var hedefParaBirimi = await _context.ParaBirimleri
                    .FirstOrDefaultAsync(p => p.ParaBirimiID == hedefParaBirimiId && !p.Silindi);

                if (hedefParaBirimi == null)
                {
                    throw new Exception($"Hedef para birimi bulunamadı: {hedefParaBirimiId}");
                }

                // İlgili tarihteki kur değerlerini al
                var kurDegerleri = await _context.KurDegerleri
                    .Where(k => k.Tarih.Date == hesaplamaTarihi.Date && !k.Silindi)
                    .ToListAsync();

                // Kaynak para birimi için kur değeri
                var kaynakKurDegeri = kurDegerleri
                    .FirstOrDefault(k => k.ParaBirimiID == kaynakParaBirimi.ParaBirimiID);

                if (kaynakKurDegeri == null && !kaynakParaBirimiId.Equals(anaParaBirimi.ParaBirimiID))
                {
                    throw new Exception($"Kaynak para birimi için kur değeri bulunamadı: {kaynakParaBirimi.Kod}");
                }

                // Hedef para birimi için kur değeri
                var hedefKurDegeri = kurDegerleri
                    .FirstOrDefault(k => k.ParaBirimiID == hedefParaBirimi.ParaBirimiID);

                if (hedefKurDegeri == null && !hedefParaBirimiId.Equals(anaParaBirimi.ParaBirimiID))
                {
                    throw new Exception($"Hedef para birimi için kur değeri bulunamadı: {hedefParaBirimi.Kod}");
                }

                decimal kurDegeri;

                // Kaynak para birimi ana para birimi ise
                if (kaynakParaBirimiId.Equals(anaParaBirimi.ParaBirimiID))
                {
                    // Hedef para birimi değeri
                    kurDegeri = hedefKurDegeri!.Alis;
                }
                // Hedef para birimi ana para birimi ise
                else if (hedefParaBirimiId.Equals(anaParaBirimi.ParaBirimiID))
                {
                    // Kaynak para birimi değerinin tersi
                    kurDegeri = 1 / kaynakKurDegeri!.Alis;
                }
                // İkisi de ana para birimi değilse çapraz kur hesapla
                else
                {
                    // Çapraz kur hesaplama: (Kaynak para birimi / Hedef para birimi)
                    kurDegeri = kaynakKurDegeri!.Alis / hedefKurDegeri!.Alis;
                }

                return kurDegeri;
            }
            catch (Exception ex)
            {
                await _logService.LogErrorAsync("DovizKuruService.HesaplaKurDegeriAsync", $"Hata: {ex.Message}");
                throw;
            }
        }

        public async Task<decimal> HesaplaKurDegeriByKodAsync(string kaynakKod, string hedefKod, DateTime? tarih = null)
        {
            try
            {
                // Para birimlerini kodlara göre bul
                var kaynakParaBirimi = await _context.ParaBirimleri
                    .FirstOrDefaultAsync(p => p.Kod == kaynakKod && !p.Silindi);

                if (kaynakParaBirimi == null)
                {
                    throw new Exception($"Kaynak para birimi bulunamadı: {kaynakKod}");
                }

                var hedefParaBirimi = await _context.ParaBirimleri
                    .FirstOrDefaultAsync(p => p.Kod == hedefKod && !p.Silindi);

                if (hedefParaBirimi == null)
                {
                    throw new Exception($"Hedef para birimi bulunamadı: {hedefKod}");
                }

                // ID'leri kullanarak kur değerini hesapla
                return await HesaplaKurDegeriAsync(kaynakParaBirimi.ParaBirimiID, hedefParaBirimi.ParaBirimiID, tarih);
            }
            catch (Exception ex)
            {
                await _logService.LogErrorAsync("DovizKuruService.HesaplaKurDegeriByKodAsync", $"Hata: {ex.Message}");
                throw;
            }
        }

        public async Task<decimal> CevirmeTutarAsync(decimal tutar, Guid kaynakParaBirimiId, Guid hedefParaBirimiId, DateTime? tarih = null)
        {
            try
            {
                // Kur değerini hesapla
                var kurDegeri = await HesaplaKurDegeriAsync(kaynakParaBirimiId, hedefParaBirimiId, tarih);

                // Tutarı çevir
                return tutar * kurDegeri;
            }
            catch (Exception ex)
            {
                await _logService.LogErrorAsync("DovizKuruService.CevirmeTutarAsync", $"Hata: {ex.Message}");
                throw;
            }
        }

        public async Task<decimal> CevirmeTutarByKodAsync(decimal tutar, string kaynakKod, string hedefKod, DateTime? tarih = null)
        {
            try
            {
                // Kur değerini hesapla
                var kurDegeri = await HesaplaKurDegeriByKodAsync(kaynakKod, hedefKod, tarih);

                // Tutarı çevir
                return tutar * kurDegeri;
            }
            catch (Exception ex)
            {
                await _logService.LogErrorAsync("DovizKuruService.CevirmeTutarByKodAsync", $"Hata: {ex.Message}");
                throw;
            }
        }

        public async Task<decimal> ParaBirimiCevirAsync(decimal tutar, string kaynakKod, string hedefKod, DateTime? tarih = null)
        {
            try
            {
                // CevirmeTutarByKodAsync metodunu çağırarak işlemi gerçekleştir
                return await CevirmeTutarByKodAsync(tutar, kaynakKod, hedefKod, tarih);
            }
            catch (Exception ex)
            {
                await _logService.LogErrorAsync("DovizKuruService.ParaBirimiCevirAsync", $"Hata: {ex.Message}");
                throw;
            }
        }

        public async Task<decimal> GetGuncelKurAsync(string kaynakKod, string hedefKod, DateTime? tarih = null)
        {
            try
            {
                // İki para birimi arasındaki kur değerini hesapla
                return await HesaplaKurDegeriByKodAsync(kaynakKod, hedefKod, tarih);
            }
            catch (Exception ex)
            {
                await _logService.LogErrorAsync("DovizKuruService.GetGuncelKurAsync", $"Hata: {ex.Message}");
                throw;
            }
        }

        public async Task<List<MuhasebeStokWebApp.Models.DovizKuru>> GetLatestRatesAsync(int count = 5)
        {
            try
            {
                // En son döviz kurlarını getir
                var latestRates = await _context.KurDegerleri
                    .Include(k => k.ParaBirimi)
                    .Where(k => !k.Silindi && k.Aktif)
                    .OrderByDescending(k => k.Tarih)
                    .ThenBy(k => k.ParaBirimi.Kod)
                    .Take(count)
                    .ToListAsync();

                var result = new List<MuhasebeStokWebApp.Models.DovizKuru>();
                
                foreach (var rate in latestRates)
                {
                    if (rate.ParaBirimi == null)
                    {
                        continue;
                    }
                    
                    result.Add(new MuhasebeStokWebApp.Models.DovizKuru
                    {
                        DovizKuruID = rate.KurDegeriID,
                        ParaBirimiID = rate.ParaBirimiID,
                        ParaBirimiKodu = rate.ParaBirimi.Kod,
                        ParaBirimiAdi = rate.ParaBirimi.Ad,
                        Alis = rate.Alis,
                        Satis = rate.Satis,
                        EfektifAlis = rate.Efektif_Alis,
                        EfektifSatis = rate.Efektif_Satis,
                        Tarih = rate.Tarih,
                        GuncellemeTarihi = rate.GuncellemeTarihi ?? rate.OlusturmaTarihi ?? DateTime.Now,
                        ParaBirimiSembol = rate.ParaBirimi.Sembol,
                        Aktif = rate.Aktif,
                        Kaynak = "TCMB"
                    });
                }
                
                return result;
            }
            catch (Exception ex)
            {
                await _logService.LogErrorAsync("DovizKuruService.GetLatestRatesAsync", $"Hata: {ex.Message}");
                return new List<MuhasebeStokWebApp.Models.DovizKuru>();
            }
        }

        public async Task<bool> GuncelleKurDegerleriniFromTCMBAsync()
        {
            try
            {
                // TCMB'nin döviz kurları XML servisinin URL'i
                string url = "https://www.tcmb.gov.tr/kurlar/today.xml";

                // HTTP isteği gönder
                var response = await _httpClient.GetAsync(url);
                if (!response.IsSuccessStatusCode)
                {
                    throw new Exception($"TCMB servisine erişilemedi. HTTP Kodu: {response.StatusCode}");
                }

                // XML içeriğini al
                var content = await response.Content.ReadAsStringAsync();
                var xmlDoc = XDocument.Parse(content);

                // Ana para birimini al (genellikle TRY)
                var anaParaBirimiKodu = await _sistemAyarService.GetAnaDovizKoduAsync();
                var anaParaBirimi = await _context.ParaBirimleri
                    .FirstOrDefaultAsync(p => p.Kod == anaParaBirimiKodu && !p.Silindi);

                if (anaParaBirimi == null)
                {
                    throw new Exception($"Ana para birimi bulunamadı: {anaParaBirimiKodu}");
                }

                // Günün tarihi
                var tarih = DateTime.Now.Date;

                // XML'den kur bilgilerini oku
                var kurDegerleri = new List<KurDegeri>();
                var currencies = xmlDoc.Descendants("Currency");

                foreach (var currency in currencies)
                {
                    var kod = currency.Attribute("CurrencyCode")?.Value;
                    if (string.IsNullOrEmpty(kod))
                        continue;

                    // Para birimini veritabanında kontrol et
                    var paraBirimi = await _context.ParaBirimleri
                        .FirstOrDefaultAsync(p => p.Kod == kod && !p.Silindi);

                    // Para birimi yoksa, oluştur
                    if (paraBirimi == null)
                    {
                        var name = currency.Element("CurrencyName")?.Value;
                        paraBirimi = new ParaBirimi
                        {
                            ParaBirimiID = Guid.NewGuid(),
                            Kod = kod,
                            Ad = name ?? kod,
                            Sembol = currency.Element("Unit")?.Value,
                            Aktif = true,
                            Silindi = false,
                            OlusturmaTarihi = DateTime.Now
                        };

                        _context.ParaBirimleri.Add(paraBirimi);
                        await _context.SaveChangesAsync();
                    }

                    // Alış ve satış değerlerini parse et
                    decimal alisDegeri;
                    decimal satisDegeri;
                    decimal efektifAlisDegeri;
                    decimal efektifSatisDegeri;

                    if (!decimal.TryParse(currency.Element("ForexBuying")?.Value, NumberStyles.Number, CultureInfo.InvariantCulture, out alisDegeri))
                    {
                        alisDegeri = 0;
                    }

                    if (!decimal.TryParse(currency.Element("ForexSelling")?.Value, NumberStyles.Number, CultureInfo.InvariantCulture, out satisDegeri))
                    {
                        satisDegeri = 0;
                    }

                    if (!decimal.TryParse(currency.Element("BanknoteBuying")?.Value, NumberStyles.Number, CultureInfo.InvariantCulture, out efektifAlisDegeri))
                    {
                        efektifAlisDegeri = alisDegeri;
                    }

                    if (!decimal.TryParse(currency.Element("BanknoteSelling")?.Value, NumberStyles.Number, CultureInfo.InvariantCulture, out efektifSatisDegeri))
                    {
                        efektifSatisDegeri = satisDegeri;
                    }

                    // Geçerli değerler varsa ekle
                    if (alisDegeri > 0 && satisDegeri > 0)
                    {
                        var kurDegeri = new KurDegeri
                        {
                            KurDegeriID = Guid.NewGuid(),
                            ParaBirimiID = paraBirimi.ParaBirimiID,
                            Tarih = tarih,
                            Alis = alisDegeri,
                            Satis = satisDegeri,
                            Efektif_Alis = efektifAlisDegeri,
                            Efektif_Satis = efektifSatisDegeri,
                            Aktif = true,
                            Silindi = false,
                            OlusturmaTarihi = DateTime.Now,
                            OlusturanKullaniciID = "System"
                        };

                        kurDegerleri.Add(kurDegeri);
                    }
                }

                // Veritabanına kaydet
                await _context.KurDegerleri.AddRangeAsync(kurDegerleri);
                await _context.SaveChangesAsync();

                // Sistem ayarındaki son güncelleme tarihini güncelle
                await _sistemAyarService.UpdateSonDovizGuncellemeTarihiAsync(DateTime.Now);

                await _logService.LogInfoAsync("DovizKuruService.GuncelleKurDegerleriniFromTCMBAsync", 
                    $"{kurDegerleri.Count} adet döviz kuru TCMB'den başarıyla çekildi.");

                return true;
            }
            catch (Exception ex)
            {
                await _logService.LogErrorAsync("DovizKuruService.GuncelleKurDegerleriniFromTCMBAsync", $"Hata: {ex.Message}");
                return false;
            }
        }
    }
} 