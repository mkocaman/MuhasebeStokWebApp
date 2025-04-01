using System;
using System.Collections.Generic;
using System.Linq;
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
    public class ParaBirimiIliskiService : IParaBirimiIliskiService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<ParaBirimiIliskiService> _logger;
        private readonly IParaBirimiService _paraBirimiService;

        public ParaBirimiIliskiService(
            IUnitOfWork unitOfWork,
            ILogger<ParaBirimiIliskiService> logger,
            IParaBirimiService paraBirimiService)
        {
            _unitOfWork = unitOfWork;
            _logger = logger;
            _paraBirimiService = paraBirimiService;
        }

        public async Task<IEnumerable<ParaBirimiIliski>> GetAllAsync()
        {
            try
            {
                return await _unitOfWork.Repository<ParaBirimiIliski>()
                    .GetAsync(null, null, "KaynakParaBirimi,HedefParaBirimi");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Para birimi ilişkileri alınırken hata oluştu");
                throw;
            }
        }

        public async Task<ParaBirimiIliski> GetByIdAsync(Guid id)
        {
            try
            {
                return await _unitOfWork.Repository<ParaBirimiIliski>()
                    .GetFirstOrDefaultAsync(i => i.ParaBirimiIliskiID == id, "KaynakParaBirimi,HedefParaBirimi");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"ID: {id} olan para birimi ilişkisi alınırken hata oluştu");
                throw;
            }
        }

        public async Task<IEnumerable<ParaBirimiIliski>> GetByKaynakParaBirimiIdAsync(Guid kaynakParaBirimiId)
        {
            try
            {
                return await _unitOfWork.Repository<ParaBirimiIliski>()
                    .GetAsync(i => i.KaynakParaBirimiID == kaynakParaBirimiId, null, "KaynakParaBirimi,HedefParaBirimi");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"KaynakParaBirimiID: {kaynakParaBirimiId} olan para birimi ilişkileri alınırken hata oluştu");
                throw;
            }
        }

        public async Task<IEnumerable<ParaBirimiIliski>> GetByHedefParaBirimiIdAsync(Guid hedefParaBirimiId)
        {
            try
            {
                return await _unitOfWork.Repository<ParaBirimiIliski>()
                    .GetAsync(i => i.HedefParaBirimiID == hedefParaBirimiId, null, "KaynakParaBirimi,HedefParaBirimi");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"HedefParaBirimiID: {hedefParaBirimiId} olan para birimi ilişkileri alınırken hata oluştu");
                throw;
            }
        }

        public async Task<ParaBirimiIliski> AddAsync(ParaBirimiIliski paraBirimiIliski)
        {
            try
            {
                await _unitOfWork.Repository<ParaBirimiIliski>().AddAsync(paraBirimiIliski);
                await _unitOfWork.CompleteAsync();
                return paraBirimiIliski;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Para birimi ilişkisi eklenirken hata oluştu");
                throw;
            }
        }

        public async Task UpdateAsync(ParaBirimiIliski paraBirimiIliski)
        {
            try
            {
                await _unitOfWork.Repository<ParaBirimiIliski>().UpdateAsync(paraBirimiIliski);
                await _unitOfWork.CompleteAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"ID: {paraBirimiIliski.ParaBirimiIliskiID} olan para birimi ilişkisi güncellenirken hata oluştu");
                throw;
            }
        }

        public async Task DeleteAsync(Guid id)
        {
            try
            {
                var paraBirimiIliski = await _unitOfWork.Repository<ParaBirimiIliski>().GetByIdAsync(id);
                if (paraBirimiIliski != null)
                {
                    await _unitOfWork.Repository<ParaBirimiIliski>().RemoveAsync(paraBirimiIliski);
                    await _unitOfWork.CompleteAsync();
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"ID: {id} olan para birimi ilişkisi silinirken hata oluştu");
                throw;
            }
        }

        public async Task<ParaBirimiIliskiViewModel> GetViewModelByIdAsync(Guid id)
        {
            var iliski = await GetByIdAsync(id);
            if (iliski == null)
                return null;

            return new ParaBirimiIliskiViewModel
            {
                ParaBirimiIliskiID = iliski.ParaBirimiIliskiID,
                KaynakParaBirimiID = iliski.KaynakParaBirimiID,
                KaynakParaBirimiAdi = iliski.KaynakParaBirimi?.Ad,
                KaynakParaBirimiKodu = iliski.KaynakParaBirimi?.Kod,
                HedefParaBirimiID = iliski.HedefParaBirimiID,
                HedefParaBirimiAdi = iliski.HedefParaBirimi?.Ad,
                HedefParaBirimiKodu = iliski.HedefParaBirimi?.Kod,
                Aktif = iliski.Aktif
            };
        }

        public async Task<IEnumerable<ParaBirimiIliskiViewModel>> GetAllViewModelsAsync()
        {
            var iliskiler = await GetAllAsync();
            var paraBirimleri = await _paraBirimiService.GetAllAsync();

            var viewModels = iliskiler.Select(i => new ParaBirimiIliskiViewModel
            {
                ParaBirimiIliskiID = i.ParaBirimiIliskiID,
                KaynakParaBirimiID = i.KaynakParaBirimiID,
                KaynakParaBirimiAdi = i.KaynakParaBirimi?.Ad,
                KaynakParaBirimiKodu = i.KaynakParaBirimi?.Kod,
                HedefParaBirimiID = i.HedefParaBirimiID,
                HedefParaBirimiAdi = i.HedefParaBirimi?.Ad,
                HedefParaBirimiKodu = i.HedefParaBirimi?.Kod,
                Aktif = i.Aktif
            }).ToList();

            // SelectListItem listesi oluşturuyoruz
            var paraListItems = paraBirimleri.Select(p => new SelectListItem
            {
                Value = p.ParaBirimiID.ToString(),
                Text = $"{p.Kod} - {p.Ad}"
            }).ToList();

            // Her bir ViewModel için SelectListItem listesini atıyoruz
            foreach (var viewModel in viewModels)
            {
                viewModel.ParaBirimleri = paraListItems;
            }

            return viewModels;
        }
    }
} 