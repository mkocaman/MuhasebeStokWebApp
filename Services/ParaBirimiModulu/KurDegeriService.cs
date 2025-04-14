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
                    .GetAsync(null, q => q.OrderByDescending(k => k.Tarih).ThenBy(k => k.KurDegeriID), null)
                    .ContinueWith(t => t.Result.OrderByDescending(k => k.Tarih).FirstOrDefault()?.Tarih);

                if (latestDate == null)
                    return new List<KurDegeri>();

                // En son tarihli kurları getir
                return await _unitOfWork.Repository<KurDegeri>()
                    .GetAsync(k => k.Tarih == latestDate, q => q.OrderBy(k => k.ParaBirimiID), "ParaBirimi");
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

        public async Task<KurDegeri> AddAsync(KurDegeriViewModel viewModel)
        {
            var entity = ConvertToEntity(viewModel);
            await _unitOfWork.Repository<KurDegeri>().AddAsync(entity);
            await _unitOfWork.CompleteAsync();
            return entity;
        }

        public async Task<KurDegeri> UpdateAsync(KurDegeriViewModel viewModel)
        {
            var entity = await _unitOfWork.Repository<KurDegeri>().GetByIdAsync(viewModel.KurDegeriID);
            if (entity == null)
                throw new Exception($"ID: {viewModel.KurDegeriID} olan KurDegeri bulunamadı.");
            
            // ViewModel'den entity'ye değerleri aktarma
            entity.ParaBirimiID = viewModel.ParaBirimiID;
            entity.Tarih = viewModel.Tarih;
            entity.Alis = viewModel.Alis;
            entity.Satis = viewModel.Satis;
            entity.Aciklama = viewModel.Aciklama;
            entity.Aktif = viewModel.Aktif;
            entity.GuncellemeTarihi = DateTime.Now;
            entity.SonGuncelleyenKullaniciID = viewModel.SonGuncelleyenKullaniciID;

            await _unitOfWork.Repository<KurDegeri>().UpdateAsync(entity);
            return entity;
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
                            Aktif = true,
                            Aciklama = "API'dan alınan değer",
                            DekontNo = $"DKT-{DateTime.Now:yyMMdd}-{new Random().Next(1000):000}"
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

            return ConvertToViewModel(kurDegeri);
        }

        // KurDegeri'yi ViewModel'e dönüştür
        private KurDegeriViewModel ConvertToViewModel(KurDegeri kurDegeri)
        {
            if (kurDegeri == null) return null;
            
            return new KurDegeriViewModel
            {
                KurDegeriID = kurDegeri.KurDegeriID,
                ParaBirimiID = kurDegeri.ParaBirimiID ?? Guid.Empty,
                ParaBirimiAdi = kurDegeri.ParaBirimi?.Ad,
                Tarih = kurDegeri.Tarih,
                Alis = kurDegeri.Alis,
                Satis = kurDegeri.Satis,
                Aktif = kurDegeri.Aktif,
                Aciklama = kurDegeri.Aciklama
            };
        }
        
        // ViewModel'i KurDegeri'ye dönüştür
        private KurDegeri ConvertToEntity(KurDegeriViewModel viewModel)
        {
            return new KurDegeri
            {
                KurDegeriID = viewModel.KurDegeriID == Guid.Empty ? Guid.NewGuid() : viewModel.KurDegeriID,
                ParaBirimiID = viewModel.ParaBirimiID,
                Tarih = viewModel.Tarih,
                Alis = viewModel.Alis,
                Satis = viewModel.Satis,
                Aktif = viewModel.Aktif,
                Silindi = viewModel.Silindi,
                Aciklama = viewModel.Aciklama,
                OlusturmaTarihi = viewModel.OlusturmaTarihi,
                GuncellemeTarihi = viewModel.GuncellemeTarihi,
                OlusturanKullaniciID = viewModel.OlusturanKullaniciID,
                SonGuncelleyenKullaniciID = viewModel.SonGuncelleyenKullaniciID
            };
        }

        public async Task<IEnumerable<KurDegeriViewModel>> GetAllViewModelsAsync()
        {
            var kurDegerleri = await GetAllAsync();
            var paraBirimleri = await _paraBirimiService.GetAllAsync();
            
            var viewModels = kurDegerleri.Select(k => ConvertToViewModel(k)).ToList();
            
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
        
        // KurMarj ile ilgili metotlar
        
        /// <summary>
        /// KurMarj entity'sini ViewModel'e dönüştürür
        /// </summary>
        private KurMarjViewModel ConvertToViewModel(KurMarj entity)
        {
            if (entity == null) return null;
            
            return new KurMarjViewModel
            {
                KurMarjID = entity.KurMarjID,
                SatisMarji = entity.SatisMarji,
                Varsayilan = entity.Varsayilan,
                Tanim = entity.Tanim,
                Aktif = entity.Aktif,
                Silindi = entity.Silindi,
                OlusturmaTarihi = entity.OlusturmaTarihi,
                GuncellemeTarihi = entity.GuncellemeTarihi,
                OlusturanKullaniciID = entity.OlusturanKullaniciID,
                SonGuncelleyenKullaniciID = entity.SonGuncelleyenKullaniciID
            };
        }
        
        /// <summary>
        /// KurMarj ViewModel'i Entity'ye dönüştürür
        /// </summary>
        private KurMarj ConvertToEntity(KurMarjViewModel viewModel)
        {
            if (viewModel == null) return null;
            
            return new KurMarj
            {
                KurMarjID = viewModel.KurMarjID,
                SatisMarji = viewModel.SatisMarji,
                Varsayilan = viewModel.Varsayilan,
                Tanim = viewModel.Tanim,
                Aktif = viewModel.Aktif,
                Silindi = viewModel.Silindi,
                OlusturmaTarihi = viewModel.KurMarjID == Guid.Empty ? DateTime.Now : viewModel.OlusturmaTarihi,
                GuncellemeTarihi = viewModel.KurMarjID == Guid.Empty ? null : DateTime.Now,
                OlusturanKullaniciID = viewModel.OlusturanKullaniciID,
                SonGuncelleyenKullaniciID = viewModel.SonGuncelleyenKullaniciID
            };
        }
        
        /// <summary>
        /// Aktif kur marj ayarını getirir. Eğer birden fazla aktif varsa, varsayılan olanı tercih eder.
        /// </summary>
        public async Task<KurMarj> GetKurMarjAsync()
        {
            try
            {
                // Önce varsayılan ve aktif olan marjı bulmaya çalış
                var kurMarj = await _unitOfWork.Repository<KurMarj>()
                    .GetFirstOrDefaultAsync(k => k.Aktif && !k.Silindi && k.Varsayilan);
                
                // Varsayılan yoksa, herhangi bir aktif marjı getir
                if (kurMarj == null)
                {
                    kurMarj = await _unitOfWork.Repository<KurMarj>()
                        .GetFirstOrDefaultAsync(k => k.Aktif && !k.Silindi);
                }
                
                // Hiç aktif marj yoksa, yeni bir varsayılan marj oluştur
                if (kurMarj == null)
                {
                    kurMarj = new KurMarj
                    {
                        KurMarjID = Guid.NewGuid(),
                        SatisMarji = 2.00m, // Varsayılan olarak %2
                        Varsayilan = true,
                        Tanim = "Varsayılan Kur Marjı",
                        Aktif = true,
                        Silindi = false,
                        OlusturmaTarihi = DateTime.Now,
                        OlusturanKullaniciID = "Sistem"
                    };
                    
                    await _unitOfWork.Repository<KurMarj>().AddAsync(kurMarj);
                    await _unitOfWork.CompleteAsync();
                }
                
                return kurMarj;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Kur marj bilgisi alınırken hata oluştu");
                throw;
            }
        }
        
        /// <summary>
        /// Aktif kur marj ayarını ViewModel olarak getirir.
        /// </summary>
        public async Task<KurMarjViewModel> GetKurMarjViewModelAsync()
        {
            var entity = await GetKurMarjAsync();
            return ConvertToViewModel(entity);
        }
        
        /// <summary>
        /// Varsayılan olarak işaretlenmiş kur marj ayarını getirir
        /// </summary>
        public async Task<KurMarj> GetVarsayilanKurMarjAsync()
        {
            try
            {
                var kurMarj = await _unitOfWork.Repository<KurMarj>()
                    .GetFirstOrDefaultAsync(k => k.Varsayilan && !k.Silindi);
                
                if (kurMarj == null)
                {
                    // Varsayılan marj yoksa, yeni bir varsayılan marj oluştur
                    kurMarj = new KurMarj
                    {
                        KurMarjID = Guid.NewGuid(),
                        SatisMarji = 2.00m, // Varsayılan olarak %2
                        Varsayilan = true,
                        Tanim = "Varsayılan Kur Marjı",
                        Aktif = true,
                        Silindi = false,
                        OlusturmaTarihi = DateTime.Now,
                        OlusturanKullaniciID = "Sistem"
                    };
                    
                    await _unitOfWork.Repository<KurMarj>().AddAsync(kurMarj);
                    await _unitOfWork.CompleteAsync();
                }
                
                return kurMarj;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Varsayılan kur marj bilgisi alınırken hata oluştu");
                throw;
            }
        }
        
        /// <summary>
        /// Varsayılan olarak işaretlenmiş kur marj ayarını ViewModel olarak getirir
        /// </summary>
        public async Task<KurMarjViewModel> GetVarsayilanKurMarjViewModelAsync()
        {
            var entity = await GetVarsayilanKurMarjAsync();
            return ConvertToViewModel(entity);
        }
        
        /// <summary>
        /// Tüm kur marj ayarlarını ViewModel olarak getirir
        /// </summary>
        public async Task<IEnumerable<KurMarjViewModel>> GetAllKurMarjViewModelsAsync()
        {
            var entities = await _unitOfWork.Repository<KurMarj>().GetAsync(k => !k.Silindi);
            return entities.Select(e => ConvertToViewModel(e));
        }
        
        /// <summary>
        /// Yeni bir kur marj ayarı ekler
        /// </summary>
        public async Task<KurMarj> AddKurMarjAsync(KurMarj kurMarj)
        {
            try
            {
                // Yeni eklenen marj varsayılan olarak işaretlenmişse, diğer varsayılanları kaldır
                if (kurMarj.Varsayilan)
                {
                    await UpdateVarsayilanMarjAsync(kurMarj.KurMarjID);
                }
                
                await _unitOfWork.Repository<KurMarj>().AddAsync(kurMarj);
                await _unitOfWork.CompleteAsync();
                return kurMarj;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Kur marj eklenirken hata oluştu");
                throw;
            }
        }
        
        /// <summary>
        /// Yeni bir kur marj ayarını ViewModel'den ekler
        /// </summary>
        public async Task<KurMarjViewModel> AddKurMarjAsync(KurMarjViewModel viewModel)
        {
            var entity = ConvertToEntity(viewModel);
            
            var result = await AddKurMarjAsync(entity);
            return ConvertToViewModel(result);
        }
        
        /// <summary>
        /// Bir kur marj ayarını günceller
        /// </summary>
        public async Task UpdateKurMarjAsync(KurMarj kurMarj)
        {
            try
            {
                // Güncellenen marj varsayılan olarak işaretlenmişse, diğer varsayılanları kaldır
                if (kurMarj.Varsayilan)
                {
                    await UpdateVarsayilanMarjAsync(kurMarj.KurMarjID);
                }
                
                await _unitOfWork.Repository<KurMarj>().UpdateAsync(kurMarj);
                await _unitOfWork.CompleteAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"ID: {kurMarj.KurMarjID} olan kur marj güncellenirken hata oluştu");
                throw;
            }
        }
        
        /// <summary>
        /// Bir kur marj ayarını ViewModel'den günceller
        /// </summary>
        public async Task UpdateKurMarjAsync(KurMarjViewModel viewModel)
        {
            var entity = await _unitOfWork.Repository<KurMarj>().GetByIdAsync(viewModel.KurMarjID);
            
            if (entity == null)
            {
                throw new Exception("Kur marjı bulunamadı.");
            }
            
            // ViewModel'den entity'ye değerleri aktarma
            entity.SatisMarji = viewModel.SatisMarji;
            entity.Varsayilan = viewModel.Varsayilan;
            entity.Tanim = viewModel.Tanim;
            entity.Aktif = viewModel.Aktif;
            entity.GuncellemeTarihi = DateTime.Now;
            entity.SonGuncelleyenKullaniciID = viewModel.SonGuncelleyenKullaniciID;
            
            await UpdateKurMarjAsync(entity);
        }
        
        /// <summary>
        /// Bir kur marj ayarını siler
        /// </summary>
        public async Task DeleteKurMarjAsync(Guid id)
        {
            try
            {
                var kurMarj = await _unitOfWork.Repository<KurMarj>().GetByIdAsync(id);
                if (kurMarj != null)
                {
                    // Eğer silinecek marj varsayılan ise, başka bir marjı varsayılan yap
                    if (kurMarj.Varsayilan)
                    {
                        var digerMarj = await _unitOfWork.Repository<KurMarj>()
                            .GetFirstOrDefaultAsync(k => k.KurMarjID != id && !k.Silindi && k.Aktif);
                        
                        if (digerMarj != null)
                        {
                            digerMarj.Varsayilan = true;
                            await _unitOfWork.Repository<KurMarj>().UpdateAsync(digerMarj);
                        }
                    }
                    
                    // Soft delete yap
                    kurMarj.Silindi = true;
                    kurMarj.Aktif = false;
                    kurMarj.GuncellemeTarihi = DateTime.Now;
                    
                    await _unitOfWork.Repository<KurMarj>().UpdateAsync(kurMarj);
                    await _unitOfWork.CompleteAsync();
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"ID: {id} olan kur marj silinirken hata oluştu");
                throw;
            }
        }
        
        /// <summary>
        /// Belirtilen ID dışındaki tüm marjların varsayılan özelliğini false yapar
        /// </summary>
        private async Task UpdateVarsayilanMarjAsync(Guid currentId)
        {
            try
            {
                var marjlar = await _unitOfWork.Repository<KurMarj>()
                    .GetAsync(k => k.KurMarjID != currentId && k.Varsayilan && !k.Silindi);
                
                foreach (var marj in marjlar)
                {
                    marj.Varsayilan = false;
                    marj.GuncellemeTarihi = DateTime.Now;
                    await _unitOfWork.Repository<KurMarj>().UpdateAsync(marj);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Varsayılan marjlar güncellenirken hata oluştu");
                throw;
            }
        }
    }
} 