using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Transactions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.Logging;
using MuhasebeStokWebApp.Data;
using MuhasebeStokWebApp.Data.Entities;
using MuhasebeStokWebApp.Data.Repositories;
using MuhasebeStokWebApp.Enums;
using MuhasebeStokWebApp.Services.Interfaces;
using MuhasebeStokWebApp.ViewModels.Fatura;
using MuhasebeStokWebApp.Models;
using MuhasebeStokWebApp.Exceptions;

namespace MuhasebeStokWebApp.Services
{
    /// <summary>
    /// Fatura servisi. 
    /// Bu sınıf, diğer fatura servislerini bir araya getirerek bir facade oluşturur.
    /// </summary>
    public class FaturaService : IFaturaService
    {
        private readonly ILogger<FaturaService> _logger;
        private readonly IFaturaCrudService _faturaCrudService;
        private readonly IFaturaOrchestrationService _faturaOrchestrationService;

        public FaturaService(
            ILogger<FaturaService> logger,
            IFaturaCrudService faturaCrudService,
            IFaturaOrchestrationService faturaOrchestrationService)
        {
            _logger = logger;
            _faturaCrudService = faturaCrudService;
            _faturaOrchestrationService = faturaOrchestrationService;
        }

        #region IFaturaCrudService Implementation
        
        public async Task<IEnumerable<Data.Entities.Fatura>> GetAllAsync()
        {
            return await _faturaCrudService.GetAllAsync();
        }

        public async Task<Data.Entities.Fatura> GetByIdAsync(Guid id)
        {
            return await _faturaCrudService.GetByIdAsync(id);
        }

        public async Task<Data.Entities.Fatura> AddAsync(Data.Entities.Fatura fatura)
        {
            return await _faturaCrudService.AddAsync(fatura);
        }

        public async Task<Data.Entities.Fatura> UpdateAsync(Data.Entities.Fatura fatura)
        {
            return await _faturaCrudService.UpdateAsync(fatura);
        }

        public async Task<bool> DeleteAsync(Guid id)
        {
            return await _faturaCrudService.DeleteAsync(id);
        }

        public async Task<FaturaDetailViewModel> GetFaturaDetailViewModelAsync(Guid id)
        {
            return await _faturaCrudService.GetFaturaDetailViewModelAsync(id);
        }

        public async Task<List<FaturaViewModel>> GetAllFaturaViewModelsAsync()
        {
            return await _faturaCrudService.GetAllFaturaViewModelsAsync();
        }

        public async Task<bool> IsFaturaInUseAsync(Guid id)
        {
            return await _faturaCrudService.IsFaturaInUseAsync(id);
        }
        
        #endregion

        #region IFaturaOrchestrationService Implementation
        
        public async Task<Guid> CreateFaturaWithRelations(FaturaCreateViewModel viewModel, Guid? currentUserId)
        {
            return await _faturaOrchestrationService.CreateFaturaWithRelations(viewModel, currentUserId);
        }

        public async Task<Guid> UpdateFaturaWithRelations(Guid id, FaturaEditViewModel viewModel, Guid? currentUserId)
        {
            return await _faturaOrchestrationService.UpdateFaturaWithRelations(id, viewModel, currentUserId);
        }

        public async Task<bool> DeleteFaturaWithRelations(Guid id, Guid? currentUserId)
        {
            return await _faturaOrchestrationService.DeleteFaturaWithRelations(id, currentUserId);
        }
        
        #endregion
        
        #region Legacy Methods for Backward Compatibility

        /// <summary>
        /// Geriye dönük uyumluluk için CreateFatura metodu
        /// </summary>
        public async Task<Guid> CreateFatura(FaturaCreateViewModel viewModel, Guid? currentUserId)
        {
            _logger.LogInformation("Legacy CreateFatura metodu çağrıldı, CreateFaturaWithRelations metoduna yönlendiriliyor");
            return await CreateFaturaWithRelations(viewModel, currentUserId);
        }

        /// <summary>
        /// Geriye dönük uyumluluk için UpdateFatura metodu
        /// </summary>
        public async Task<Guid> UpdateFatura(Guid id, FaturaEditViewModel viewModel, Guid? currentUserId)
        {
            _logger.LogInformation("Legacy UpdateFatura metodu çağrıldı, UpdateFaturaWithRelations metoduna yönlendiriliyor");
            return await UpdateFaturaWithRelations(id, viewModel, currentUserId);
        }
        
        /// <summary>
        /// Geriye dönük uyumluluk için DeleteFatura metodu
        /// </summary>
        public async Task<bool> DeleteFatura(Guid id, Guid? currentUserId)
        {
            _logger.LogInformation("Legacy DeleteFatura metodu çağrıldı, DeleteFaturaWithRelations metoduna yönlendiriliyor");
            return await DeleteFaturaWithRelations(id, currentUserId);
        }
        
        #endregion
    }
} 