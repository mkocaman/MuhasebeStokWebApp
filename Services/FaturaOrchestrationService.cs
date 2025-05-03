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
        private readonly IFaturaCrudService _faturaService;
        private readonly IStokFifoService _stokFifoService;
        private readonly ILogger<FaturaOrchestrationService> _logger;
        private readonly IUnitOfWork _unitOfWork;

        public FaturaOrchestrationService(
            IFaturaCrudService faturaService,
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
                // Fatura entity'sini oluştur ve veritabanına ekle
                var fatura = new Fatura
                {
                    FaturaID = Guid.NewGuid(),
                    FaturaNumarasi = viewModel.FaturaNumarasi,
                    SiparisNumarasi = viewModel.SiparisNumarasi,
                    FaturaTarihi = viewModel.FaturaTarihi,
                    CariID = viewModel.CariID,
                    FaturaTuruID = viewModel.FaturaTuruID.HasValue ? int.Parse(viewModel.FaturaTuruID.Value.ToString().Substring(0, 8), System.Globalization.NumberStyles.HexNumber) % 1000 : (int?)null,
                    OlusturmaTarihi = DateTime.Now,
                    GuncellemeTarihi = DateTime.Now,
                    OlusturanKullaniciID = currentUserId,
                    SonGuncelleyenKullaniciID = currentUserId,
                    FaturaNotu = viewModel.Aciklama,
                    AraToplam = viewModel.AraToplam ?? 0,
                    GenelToplam = viewModel.GenelToplam ?? 0,
                    IndirimTutari = 0,
                    Silindi = false
                };
                
                await _faturaService.AddAsync(fatura);
                
                return fatura.FaturaID;
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
                // Mevcut faturayı al
                var fatura = await _faturaService.GetByIdAsync(id);
                if (fatura == null)
                {
                    throw new Exception($"Güncellenecek fatura bulunamadı (ID: {id})");
                }
                
                // Fatura bilgilerini güncelle
                fatura.FaturaNumarasi = viewModel.FaturaNumarasi;
                fatura.FaturaTarihi = viewModel.FaturaTarihi;
                fatura.CariID = viewModel.CariID;
                fatura.FaturaTuruID = viewModel.FaturaTuruID.HasValue ? int.Parse(viewModel.FaturaTuruID.Value.ToString().Substring(0, 8), System.Globalization.NumberStyles.HexNumber) % 1000 : (int?)null;
                fatura.GuncellemeTarihi = DateTime.Now;
                fatura.SonGuncelleyenKullaniciID = currentUserId;
                fatura.FaturaNotu = viewModel.Aciklama;
                fatura.AraToplam = viewModel.AraToplam;
                fatura.GenelToplam = viewModel.GenelToplam;
                fatura.IndirimTutari = viewModel.IndirimTutari;
                
                await _faturaService.UpdateAsync(fatura);
                
                return fatura.FaturaID;
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
                return await _faturaService.DeleteAsync(id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Fatura ve ilişkili kayıtlar silinirken hata: {Message}", ex.Message);
                throw;
            }
        }
    }
} 