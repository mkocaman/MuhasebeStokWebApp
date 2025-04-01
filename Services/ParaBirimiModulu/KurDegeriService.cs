using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using MuhasebeStokWebApp.Data;
using MuhasebeStokWebApp.Data.Repositories;
using MuhasebeStokWebApp.Data.Entities.ParaBirimiModulu;
using MuhasebeStokWebApp.ViewModels.ParaBirimiModulu;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace MuhasebeStokWebApp.Services.ParaBirimiModulu
{
    public class KurDegeriService : IKurDegeriService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<KurDegeriService> _logger;
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IParaBirimiService _paraBirimiService;

        public KurDegeriService(
            IUnitOfWork unitOfWork,
            ILogger<KurDegeriService> logger,
            IHttpClientFactory httpClientFactory,
            IParaBirimiService paraBirimiService)
        {
            _unitOfWork = unitOfWork;
            _logger = logger;
            _httpClientFactory = httpClientFactory;
            _paraBirimiService = paraBirimiService;
        }

        public async Task<IEnumerable<KurDegeri>> GetAllAsync()
        {
            try
            {
                return await _unitOfWork.Repository<KurDegeri>()
                    .GetAsync(null, null, "ParaBirimi");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "KurDegeri verileri alınırken hata oluştu");
                throw;
            }
        }

        public async Task<KurDegeri> GetByIdAsync(Guid id)
        {
            try
            {
                return await _unitOfWork.Repository<KurDegeri>()
                    .GetFirstOrDefaultAsync(k => k.KurDegeriID == id, "ParaBirimi");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"ID: {id} olan KurDegeri alınırken hata oluştu");
                throw;
            }
        }

        public async Task<IEnumerable<KurDegeri>> GetByParaBirimiIdAsync(Guid paraBirimiId)
        {
            try
            {
                return await _unitOfWork.Repository<KurDegeri>()
                    .GetAsync(k => k.ParaBirimiID == paraBirimiId, null, "ParaBirimi");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"ParaBirimiID: {paraBirimiId} olan KurDegeri verileri alınırken hata oluştu");
                throw;
            }
        }

        public async Task<IEnumerable<KurDegeri>> GetLatestRatesAsync()
        {
            try
            {
                // En son tarihi bul
                var latestDate = await _unitOfWork.Repository<KurDegeri>()
                    .GetAsync(null, q => q.OrderByDescending(k => k.Tarih), null)
                    .ContinueWith(t => t.Result.FirstOrDefault()?.Tarih);

                if (latestDate == null)
                    return new List<KurDegeri>();

                // En son tarihli kurları getir
                return await _unitOfWork.Repository<KurDegeri>()
                    .GetAsync(k => k.Tarih == latestDate, null, "ParaBirimi");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "En son KurDegeri verileri alınırken hata oluştu");
                throw;
            }
        }

        public async Task<IEnumerable<KurDegeri>> GetByDateAsync(DateTime date)
        {
            try
            {
                return await _unitOfWork.Repository<KurDegeri>()
                    .GetAsync(k => k.Tarih.Date == date.Date, null, "ParaBirimi");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Tarih: {date} olan KurDegeri verileri alınırken hata oluştu");
                throw;
            }
        }

        public async Task<KurDegeri> AddAsync(KurDegeri kurDegeri)
        {
            try
            {
                await _unitOfWork.Repository<KurDegeri>().AddAsync(kurDegeri);
                await _unitOfWork.CompleteAsync();
                return kurDegeri;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "KurDegeri eklenirken hata oluştu");
                throw;
            }
        }

        public async Task UpdateAsync(KurDegeri kurDegeri)
        {
            try
            {
                await _unitOfWork.Repository<KurDegeri>().UpdateAsync(kurDegeri);
                await _unitOfWork.CompleteAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"ID: {kurDegeri.KurDegeriID} olan KurDegeri güncellenirken hata oluştu");
                throw;
            }
        }

        public async Task DeleteAsync(Guid id)
        {
            try
            {
                var kurDegeri = await _unitOfWork.Repository<KurDegeri>().GetByIdAsync(id);
                if (kurDegeri != null)
                {
                    await _unitOfWork.Repository<KurDegeri>().RemoveAsync(kurDegeri);
                    await _unitOfWork.CompleteAsync();
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"ID: {id} olan KurDegeri silinirken hata oluştu");
                throw;
            }
        }

        public async Task<bool> UpdateExchangeRatesFromApiAsync()
        {
            try
            {
                // Bu metod ileride TCMB veya başka bir API'dan döviz kurlarını çekebilir
                // Şimdilik mocklanmış veri döndürüyoruz
                var today = DateTime.Today;
                var currencies = await _paraBirimiService.GetAllAsync();
                
                foreach (var currency in currencies.Where(c => !c.AnaParaBirimiMi))
                {
                    var existingRates = await _unitOfWork.Repository<KurDegeri>()
                        .GetAsync(k => k.ParaBirimiID == currency.ParaBirimiID && k.Tarih.Date == today);
                    
                    if (!existingRates.Any())
                    {
                        // Gerçek API'dan veri alınması durumunda burada alınan değerler kullanılacak
                        // Şimdilik örnek değerler ekliyoruz
                        var newRate = new KurDegeri
                        {
                            ParaBirimiID = currency.ParaBirimiID,
                            Tarih = today,
                            Alis = 10.0m + (decimal)new Random().NextDouble(), // Örnek değer
                            Satis = 10.5m + (decimal)new Random().NextDouble(), // Örnek değer
                            Efektif_Alis = 10.2m + (decimal)new Random().NextDouble(), // Örnek değer
                            Efektif_Satis = 10.7m + (decimal)new Random().NextDouble(), // Örnek değer
                            Aktif = true,
                            Aciklama = "API'dan alınan değer"
                        };
                        
                        await _unitOfWork.Repository<KurDegeri>().AddAsync(newRate);
                    }
                }
                
                await _unitOfWork.CompleteAsync();
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "API'dan döviz kurları güncellenirken hata oluştu");
                return false;
            }
        }

        public async Task<KurDegeriViewModel> GetViewModelByIdAsync(Guid id)
        {
            var kurDegeri = await GetByIdAsync(id);
            if (kurDegeri == null)
                return null;

            return new KurDegeriViewModel
            {
                KurDegeriID = kurDegeri.KurDegeriID,
                ParaBirimiID = kurDegeri.ParaBirimiID,
                ParaBirimiAdi = kurDegeri.ParaBirimi?.Ad,
                ParaBirimiKodu = kurDegeri.ParaBirimi?.Kod,
                Tarih = kurDegeri.Tarih,
                Alis = kurDegeri.Alis,
                Satis = kurDegeri.Satis,
                Efektif_Alis = kurDegeri.Efektif_Alis,
                Efektif_Satis = kurDegeri.Efektif_Satis,
                Aktif = kurDegeri.Aktif,
                Aciklama = kurDegeri.Aciklama
            };
        }

        public async Task<IEnumerable<KurDegeriViewModel>> GetAllViewModelsAsync()
        {
            var kurDegerleri = await GetAllAsync();
            var paraBirimleri = await _paraBirimiService.GetAllAsync();
            
            var viewModels = kurDegerleri.Select(k => new KurDegeriViewModel
            {
                KurDegeriID = k.KurDegeriID,
                ParaBirimiID = k.ParaBirimiID,
                ParaBirimiAdi = k.ParaBirimi?.Ad,
                ParaBirimiKodu = k.ParaBirimi?.Kod,
                Tarih = k.Tarih,
                Alis = k.Alis,
                Satis = k.Satis,
                Efektif_Alis = k.Efektif_Alis,
                Efektif_Satis = k.Efektif_Satis,
                Aktif = k.Aktif,
                Aciklama = k.Aciklama
            }).ToList();
            
            // Her bir ViewModel için para birimleri listesini oluştur
            var paraListItems = paraBirimleri.Select(p => new SelectListItem
            {
                Value = p.ParaBirimiID.ToString(),
                Text = $"{p.Kod} - {p.Ad}"
            }).ToList();
            
            foreach (var viewModel in viewModels)
            {
                viewModel.ParaBirimleri = paraListItems;
            }
            
            return viewModels;
        }
    }
} 