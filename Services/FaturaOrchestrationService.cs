using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using MuhasebeStokWebApp.Data;
using MuhasebeStokWebApp.Data.Entities;
using MuhasebeStokWebApp.Data.Repositories;
using MuhasebeStokWebApp.Services.Interfaces;
using MuhasebeStokWebApp.ViewModels.Fatura;

namespace MuhasebeStokWebApp.Services
{
    public class FaturaOrchestrationService : IFaturaOrchestrationService
    {
        private readonly IFaturaService _faturaService;
        private readonly IStokFifoService _stokFifoService;
        private readonly ILogger<FaturaOrchestrationService> _logger;
        private readonly IUnitOfWork _unitOfWork;

        public FaturaOrchestrationService(
            IFaturaService faturaService,
            IStokFifoService stokFifoService,
            ILogger<FaturaOrchestrationService> logger,
            IUnitOfWork unitOfWork)
        {
            _faturaService = faturaService;
            _stokFifoService = stokFifoService;
            _logger = logger;
            _unitOfWork = unitOfWork;
        }

        /// <summary>
        /// Fatura ve ilişkili kayıtları (StokHareket, CariHareket, Irsaliye) oluşturur
        /// </summary>
        public async Task<Guid> CreateFaturaWithRelations(FaturaCreateViewModel viewModel, Guid? currentUserId)
        {
            try
            {
                // FaturaService zaten transaction yönetimi yapıyor, ilişkili kayıtları da oluşturuyor
                // Bu metot şu an sadece FaturaService'i çağırıyor, ileride genişletilebilir
                return await _faturaService.CreateFatura(viewModel, currentUserId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Fatura ve ilişkili kayıtlar oluşturulurken hata: {Message}", ex.Message);
                throw;
            }
        }

        /// <summary>
        /// Fatura ve ilişkili kayıtları (StokHareket, CariHareket, Irsaliye) günceller
        /// </summary>
        public async Task<Guid> UpdateFaturaWithRelations(Guid id, FaturaEditViewModel viewModel, Guid? currentUserId)
        {
            try
            {
                // FaturaService zaten transaction yönetimi yapıyor, ilişkili kayıtları da güncelliyor
                // Bu metot şu an sadece FaturaService'i çağırıyor, ileride genişletilebilir
                return await _faturaService.UpdateFatura(id, viewModel, currentUserId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Fatura ve ilişkili kayıtlar güncellenirken hata: {Message}", ex.Message);
                throw;
            }
        }

        /// <summary>
        /// Fatura ve ilişkili kayıtları (StokHareket, CariHareket, Irsaliye) siler
        /// </summary>
        public async Task<bool> DeleteFaturaWithRelations(Guid id, Guid? currentUserId)
        {
            try
            {
                // FaturaService zaten transaction yönetimi yapıyor, ilişkili kayıtları da siliyor
                // Bu metot şu an sadece FaturaService'i çağırıyor, ileride genişletilebilir
                return await _faturaService.DeleteFatura(id, currentUserId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Fatura ve ilişkili kayıtlar silinirken hata: {Message}", ex.Message);
                throw;
            }
        }
    }
} 