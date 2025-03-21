using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using MuhasebeStokWebApp.Data;
using MuhasebeStokWebApp.Data.Entities;
using MuhasebeStokWebApp.ViewModels.DovizKuru;

namespace MuhasebeStokWebApp.Services
{
    public class DovizKuruService : IDovizKuruService
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogService _logService;

        public DovizKuruService(ApplicationDbContext context, ILogService logService)
        {
            _context = context;
            _logService = logService;
        }

        public async Task<decimal> GetGuncelKurAsync(string paraBirimiKodu, string bazParaBirimiKodu = "USD")
        {
            if (paraBirimiKodu == bazParaBirimiKodu)
                return 1m;

            // Önce para birimlerini bul
            var paraBirimi = await _context.ParaBirimleri.FirstOrDefaultAsync(p => p.Kod == paraBirimiKodu && p.Aktif && !p.Silindi);
            var bazParaBirimi = await _context.ParaBirimleri.FirstOrDefaultAsync(p => p.Kod == bazParaBirimiKodu && p.Aktif && !p.Silindi);

            if (paraBirimi == null || bazParaBirimi == null)
                throw new InvalidOperationException($"Para birimleri bulunamadı: {paraBirimiKodu}, {bazParaBirimiKodu}");

            // İlgili para birimi için bugünün tarihli kur değeri var mı kontrol et
            var bugun = DateTime.Now.Date;
            var kurDegeri = await _context.KurDegerleri
                .Where(k => k.ParaBirimiID == paraBirimi.ParaBirimiID && k.Aktif && !k.Silindi && k.Tarih.Date == bugun)
                .OrderByDescending(k => k.OlusturmaTarihi)
                .FirstOrDefaultAsync();

            var bazKurDegeri = await _context.KurDegerleri
                .Where(k => k.ParaBirimiID == bazParaBirimi.ParaBirimiID && k.Aktif && !k.Silindi && k.Tarih.Date == bugun)
                .OrderByDescending(k => k.OlusturmaTarihi)
                .FirstOrDefaultAsync();

            // Eğer bugünün kuru yoksa, API'den güncel kurları çek
            if (kurDegeri == null || bazKurDegeri == null)
            {
                try
                {
                    await KurlariGuncelleAsync();
                    
                    // Yeni kurları tekrar çek
                    kurDegeri = await _context.KurDegerleri
                        .Where(k => k.ParaBirimiID == paraBirimi.ParaBirimiID && k.Aktif && !k.Silindi && k.Tarih.Date == bugun)
                        .OrderByDescending(k => k.OlusturmaTarihi)
                        .FirstOrDefaultAsync();

                    bazKurDegeri = await _context.KurDegerleri
                        .Where(k => k.ParaBirimiID == bazParaBirimi.ParaBirimiID && k.Aktif && !k.Silindi && k.Tarih.Date == bugun)
                        .OrderByDescending(k => k.OlusturmaTarihi)
                        .FirstOrDefaultAsync();
                }
                catch (Exception ex)
                {
                    await _logService.LogErrorAsync("DovizKuruService.GetGuncelKurAsync", $"Kurlar güncellenirken hata oluştu: {ex.Message}");
                    
                    // Hata durumunda en son kuru getir
                    kurDegeri = await _context.KurDegerleri
                        .Where(k => k.ParaBirimiID == paraBirimi.ParaBirimiID && k.Aktif && !k.Silindi)
                        .OrderByDescending(k => k.Tarih)
                        .FirstOrDefaultAsync();

                    bazKurDegeri = await _context.KurDegerleri
                        .Where(k => k.ParaBirimiID == bazParaBirimi.ParaBirimiID && k.Aktif && !k.Silindi)
                        .OrderByDescending(k => k.Tarih)
                        .FirstOrDefaultAsync();
                }
            }

            if (kurDegeri != null && bazKurDegeri != null)
            {
                return kurDegeri.SatisDegeri / bazKurDegeri.SatisDegeri;
            }

            throw new InvalidOperationException($"Kur değeri bulunamadı: {paraBirimiKodu}/{bazParaBirimiKodu}");
        }

        public async Task<decimal> GetKurByTarihAsync(string paraBirimiKodu, string bazParaBirimiKodu, DateTime tarih)
        {
            if (paraBirimiKodu == bazParaBirimiKodu)
                return 1m;

            // Para birimlerini bul
            var paraBirimi = await _context.ParaBirimleri.FirstOrDefaultAsync(p => p.Kod == paraBirimiKodu && !p.Silindi);
            var bazParaBirimi = await _context.ParaBirimleri.FirstOrDefaultAsync(p => p.Kod == bazParaBirimiKodu && !p.Silindi);

            if (paraBirimi == null || bazParaBirimi == null)
                throw new InvalidOperationException($"Para birimleri bulunamadı: {paraBirimiKodu}, {bazParaBirimiKodu}");

            // Belirtilen tarihteki kur değerlerini al
            var kurDegeri = await _context.KurDegerleri
                .Where(k => k.ParaBirimiID == paraBirimi.ParaBirimiID && !k.Silindi)
                .Where(k => k.Tarih.Date <= tarih.Date)
                .OrderByDescending(k => k.Tarih)
                .FirstOrDefaultAsync();

            var bazKurDegeri = await _context.KurDegerleri
                .Where(k => k.ParaBirimiID == bazParaBirimi.ParaBirimiID && !k.Silindi)
                .Where(k => k.Tarih.Date <= tarih.Date)
                .OrderByDescending(k => k.Tarih)
                .FirstOrDefaultAsync();

            if (kurDegeri != null && bazKurDegeri != null)
            {
                return kurDegeri.SatisDegeri / bazKurDegeri.SatisDegeri;
            }

            // Kur bulunamadıysa güncel kuru döndür
            return await GetGuncelKurAsync(paraBirimiKodu, bazParaBirimiKodu);
        }

        public async Task<KurDegeri> GetKurDegeriByIdAsync(Guid kurDegeriId)
        {
            return await _context.KurDegerleri
                .FirstOrDefaultAsync(k => k.KurDegeriID == kurDegeriId && !k.Silindi);
        }

        public async Task<List<KurDegeri>> GetParaBirimiKurDegerleriAsync(Guid paraBirimiId)
        {
            return await _context.KurDegerleri
                .Where(k => k.ParaBirimiID == paraBirimiId && !k.Silindi)
                .OrderByDescending(k => k.Tarih)
                .ToListAsync();
        }

        public async Task<List<KurDegeri>> GetGuncelKurlarAsync()
        {
            // Aktif para birimlerini al
            var paraBirimleri = await _context.ParaBirimleri
                .Where(p => p.Aktif && !p.Silindi)
                .ToListAsync();

            var sonuclar = new List<KurDegeri>();

            foreach (var paraBirimi in paraBirimleri)
            {
                var kurDegeri = await _context.KurDegerleri
                    .Where(k => k.ParaBirimiID == paraBirimi.ParaBirimiID && k.Aktif && !k.Silindi)
                    .OrderByDescending(k => k.Tarih)
                    .FirstOrDefaultAsync();

                if (kurDegeri != null)
                {
                    sonuclar.Add(kurDegeri);
                }
            }

            return sonuclar;
        }

        public async Task<decimal> ParaBirimiCevirAsync(decimal tutar, string kaynakParaBirimiKodu, string hedefParaBirimiKodu, DateTime? tarih = null)
        {
            if (kaynakParaBirimiKodu.Equals(hedefParaBirimiKodu, StringComparison.OrdinalIgnoreCase))
                return tutar;
                
            decimal kurDegeri;
            
            if (tarih.HasValue)
            {
                kurDegeri = await GetKurByTarihAsync(hedefParaBirimiKodu, kaynakParaBirimiKodu, tarih.Value);
            }
            else
            {
                kurDegeri = await GetGuncelKurAsync(hedefParaBirimiKodu, kaynakParaBirimiKodu);
            }
            
            return tutar * kurDegeri;
        }

        public async Task<KurDegeri> KurEkleAsync(Guid paraBirimiId, decimal alisDegeri, decimal satisDegeri, string kaynak, DateTime tarih)
        {
            var paraBirimi = await _context.ParaBirimleri.FindAsync(paraBirimiId);
            if (paraBirimi == null)
                throw new InvalidOperationException("Para birimi bulunamadı.");

            var kurDegeri = new KurDegeri
            {
                KurDegeriID = Guid.NewGuid(),
                ParaBirimiID = paraBirimiId,
                AlisDegeri = alisDegeri,
                SatisDegeri = satisDegeri,
                Tarih = tarih,
                Kaynak = kaynak,
                Aktif = true,
                Silindi = false
            };

            _context.KurDegerleri.Add(kurDegeri);
            await _context.SaveChangesAsync();

            // Log kaydı ekle
            await _logService.AddLogAsync("KurDegeri", "Ekleme", 
                $"Para Birimi: {paraBirimi.Kod}, Alış: {alisDegeri}, Satış: {satisDegeri}, Tarih: {tarih}", 
                kurDegeri.KurDegeriID.ToString());

            return kurDegeri;
        }

        public async Task<bool> DeleteKurDegeriAsync(Guid kurDegeriId)
        {
            var kurDegeri = await _context.KurDegerleri.FindAsync(kurDegeriId);
            if (kurDegeri == null)
                return false;

            kurDegeri.Silindi = true;
            kurDegeri.Aktif = false;
            kurDegeri.GuncellemeTarihi = DateTime.Now;

            await _context.SaveChangesAsync();

            // Log kaydı ekle
            await _logService.AddLogAsync("KurDegeri", "Silme", 
                $"Kur Değeri ID: {kurDegeriId} silindi.", 
                kurDegeriId.ToString());

            return true;
        }

        public async Task<List<KurDegeri>> KurlariGuncelleAsync()
        {
            try
            {
                // ExchangeRate API'den kur verilerini çekelim
                using (var httpClient = new System.Net.Http.HttpClient())
                {
                    List<KurDegeri> eklenenKurlar = new List<KurDegeri>();
                    
                    // Bugünün tarihi
                    var bugun = DateTime.Now.Date;
                    
                    // Aktif para birimlerini getir
                    var paraBirimleri = await _context.ParaBirimleri
                        .Where(p => p.Aktif && !p.Silindi)
                        .ToListAsync();
                    
                    // API için taban para birimini USD olarak belirle
                    string baseCurrency = "USD";
                    
                    // ExchangeRate API'ye istek gönder
                    var apiKey = "645b5bebcab7cef56e1b609c";
                    var apiUrl = $"https://v6.exchangerate-api.com/v6/{apiKey}/latest/{baseCurrency}";
                    
                    var response = await httpClient.GetAsync(apiUrl);
                    response.EnsureSuccessStatusCode();
                    var jsonContent = await response.Content.ReadAsStringAsync();
                    
                    // JSON'ı deserialize et
                    var options = new System.Text.Json.JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    };
                    
                    var exchangeRateResponse = System.Text.Json.JsonSerializer.Deserialize<ExchangeRateApiResponse>(jsonContent, options);
                    
                    if (exchangeRateResponse == null || exchangeRateResponse.ConversionRates == null)
                    {
                        throw new InvalidOperationException("API'den geçerli yanıt alınamadı.");
                    }
                    
                    // USD para birimini bul (baz para birimi)
                    var usdParaBirimi = paraBirimleri.FirstOrDefault(p => p.Kod == baseCurrency);
                    if (usdParaBirimi == null)
                    {
                        throw new InvalidOperationException($"{baseCurrency} para birimi bulunamadı.");
                    }
                    
                    // USD para birimi için bugün kur eklenmiş mi kontrol et
                    var usdKurVarMi = await _context.KurDegerleri
                        .AnyAsync(k => k.ParaBirimiID == usdParaBirimi.ParaBirimiID && k.Tarih.Date == bugun && k.Aktif && !k.Silindi);
                        
                    if (!usdKurVarMi)
                    {
                        // USD para birimi için kur ekle (1.0)
                        var usdKur = await KurEkleAsync(
                            usdParaBirimi.ParaBirimiID,
                            1.0m,  // Alış
                            1.0m,  // Satış
                            "ExchangeRate API",
                            bugun
                        );
                        eklenenKurlar.Add(usdKur);
                    }
                    
                    // Diğer para birimleri için kurları ekle
                    foreach (var paraBirimi in paraBirimleri.Where(p => p.Kod != baseCurrency))
                    {
                        // Bugün için kur var mı kontrol et
                        var kurVarMi = await _context.KurDegerleri
                            .AnyAsync(k => k.ParaBirimiID == paraBirimi.ParaBirimiID && k.Tarih.Date == bugun && k.Aktif && !k.Silindi);
                            
                        if (!kurVarMi) // Bugün için kur yoksa ekle
                        {
                            // API'den kur değerini al
                            if (exchangeRateResponse.ConversionRates.TryGetValue(paraBirimi.Kod, out decimal kurDegeri))
                            {
                                // 0.05 alış-satış farkı uygula
                                decimal alisDegeri = kurDegeri - 0.05m;
                                decimal satisDegeri = kurDegeri + 0.05m;
                                
                                // Negatif değer olmaması için kontrol
                                if (alisDegeri < 0) alisDegeri = 0.01m;
                                
                                var yeniKur = await KurEkleAsync(
                                    paraBirimi.ParaBirimiID,
                                    alisDegeri,
                                    satisDegeri,
                                    "ExchangeRate API",
                                    bugun
                                );
                                
                                eklenenKurlar.Add(yeniKur);
                            }
                        }
                    }
                    
                    // Log kaydı ekle
                    if (eklenenKurlar.Count > 0)
                    {
                        await _logService.AddLogAsync("KurDegeri", "Toplu Güncelleme", 
                            $"{eklenenKurlar.Count} adet kur değeri ExchangeRate API'den güncellendi.", 
                            null);
                    }
                    
                    return eklenenKurlar;
                }
            }
            catch (Exception ex)
            {
                await _logService.AddLogAsync("KurDegeri", "Hata", 
                    $"Kurlar güncellenirken hata oluştu: {ex.Message}", null);
                throw;
            }
        }

        // API'den dönen yanıtı parse etmek için sınıf
        private class ExchangeRateApiResponse
        {
            public string Result { get; set; }
            public string Documentation { get; set; }
            public string TermsOfUse { get; set; }
            public long TimeLastUpdateUnix { get; set; }
            public string TimeLastUpdateUtc { get; set; }
            public long TimeNextUpdateUnix { get; set; }
            public string TimeNextUpdateUtc { get; set; }
            public string BaseCode { get; set; }
            public Dictionary<string, decimal> ConversionRates { get; set; }
        }

        public async Task<KurDegeri> AddDovizKuruManuelAsync(DovizKuruEkleViewModel viewModel)
        {
            try
            {
                // Para birimini kodu ile bul
                var paraBirimi = await _context.ParaBirimleri
                    .FirstOrDefaultAsync(p => p.Kod == viewModel.ParaBirimiKodu && p.Aktif && !p.Silindi);
                    
                if (paraBirimi == null)
                    throw new InvalidOperationException($"Para birimi bulunamadı: {viewModel.ParaBirimiKodu}");
                    
                // Aynı tarihe ait kur zaten varsa güncelle
                var mevcutKur = await _context.KurDegerleri
                    .FirstOrDefaultAsync(k => 
                        k.ParaBirimiID == paraBirimi.ParaBirimiID && 
                        k.Tarih.Date == viewModel.Tarih.Date &&
                        k.Kaynak == viewModel.Kaynak && 
                        !k.Silindi);
                        
                if (mevcutKur != null)
                {
                    // Varolan kuru güncelle
                    mevcutKur.AlisDegeri = viewModel.AlisDegeri;
                    mevcutKur.SatisDegeri = viewModel.SatisDegeri;
                    mevcutKur.GuncellemeTarihi = DateTime.Now;
                    mevcutKur.Aciklama = string.IsNullOrEmpty(viewModel.Aciklama) ?
                        $"Manuel olarak güncellendi - {DateTime.Now}" : viewModel.Aciklama;
                        
                    _context.KurDegerleri.Update(mevcutKur);
                    await _context.SaveChangesAsync();
                    
                    await _logService.Log(
                        $"{paraBirimi.Kod} için {viewModel.Tarih.ToShortDateString()} tarihli kur değeri güncellendi. " +
                        $"Alış: {viewModel.AlisDegeri:N6}, Satış: {viewModel.SatisDegeri:N6}",
                        Enums.LogTuru.Bilgi
                    );
                    
                    return mevcutKur;
                }
                else
                {
                    // Yeni kur oluştur
                    var kurDegeri = new KurDegeri
                    {
                        KurDegeriID = Guid.NewGuid(),
                        ParaBirimiID = paraBirimi.ParaBirimiID,
                        AlisDegeri = viewModel.AlisDegeri,
                        SatisDegeri = viewModel.SatisDegeri,
                        Tarih = viewModel.Tarih.Date,
                        Kaynak = viewModel.Kaynak,
                        VeriKaynagi = viewModel.VeriKaynagi,
                        Aciklama = string.IsNullOrEmpty(viewModel.Aciklama) ?
                            $"Manuel olarak eklendi - {DateTime.Now}" : viewModel.Aciklama,
                        OlusturmaTarihi = DateTime.Now,
                        GuncellemeTarihi = DateTime.Now,
                        Aktif = true,
                        Silindi = false
                    };
                    
                    await _context.KurDegerleri.AddAsync(kurDegeri);
                    await _context.SaveChangesAsync();
                    
                    await _logService.Log(
                        $"{paraBirimi.Kod} için {viewModel.Tarih.ToShortDateString()} tarihli yeni kur değeri eklendi. " +
                        $"Alış: {viewModel.AlisDegeri:N6}, Satış: {viewModel.SatisDegeri:N6}",
                        Enums.LogTuru.Bilgi
                    );
                    
                    return kurDegeri;
                }
            }
            catch (Exception ex)
            {
                await _logService.Log(
                    $"Kur değeri eklenirken hata oluştu: {ex.Message}",
                    Enums.LogTuru.Hata
                );
                
                throw new Exception($"Kur değeri eklenirken bir hata oluştu: {ex.Message}", ex);
            }
        }

        public async Task<List<KurDegeri>> GetLatestRatesAsync(int count)
        {
            try
            {
                // En son eklenen kurlar (ParaBirimi ilişkisi ile birlikte)
                var sonKurlar = await _context.KurDegerleri
                    .Include(k => k.ParaBirimi)
                    .Where(k => k.Aktif && !k.Silindi)
                    .OrderByDescending(k => k.Tarih)
                    .GroupBy(k => k.ParaBirimiID)
                    .Select(group => group.FirstOrDefault())
                    .Take(count)
                    .ToListAsync();

                return sonKurlar;
            }
            catch (Exception ex)
            {
                await _logService.AddLogAsync("KurDegeri", "Hata", 
                    $"Son kurlar alınırken hata oluştu: {ex.Message}", null);
                
                // Hata durumunda boş liste döndür
                return new List<KurDegeri>();
            }
        }
    }
} 