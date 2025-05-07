using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using MuhasebeStokWebApp.Data;
using MuhasebeStokWebApp.Data.Entities;
using MuhasebeStokWebApp.Data.Repositories;
using MuhasebeStokWebApp.Services.Interfaces;
using MuhasebeStokWebApp.Services.Exceptions;
using MuhasebeStokWebApp.ViewModels.Fatura;

namespace MuhasebeStokWebApp.Services
{
    /// <summary>
    /// Fatura temel CRUD işlemlerini yönetir
    /// </summary>
    public class FaturaCrudService : IFaturaCrudService
    {
        private readonly ILogger<FaturaCrudService> _logger;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IFaturaTransactionService _transactionService;
        private readonly IExceptionHandlingService _exceptionHandler;

        public FaturaCrudService(
            IUnitOfWork unitOfWork,
            ILogger<FaturaCrudService> logger,
            IFaturaTransactionService transactionService,
            IExceptionHandlingService exceptionHandler)
        {
            _unitOfWork = unitOfWork;
            _logger = logger;
            _transactionService = transactionService;
            _exceptionHandler = exceptionHandler;
        }

        /// <summary>
        /// Tüm faturaları getirir
        /// </summary>
        public async Task<IEnumerable<Fatura>> GetAllAsync()
        {
            return await _exceptionHandler.HandleExceptionAsync(async () =>
            {
                return await _unitOfWork.EntityFaturaRepository.GetAllWithIncludesAsync();
            }, "GetAllAsync");
        }

        /// <summary>
        /// ID'ye göre fatura getirir
        /// </summary>
        public async Task<Fatura> GetByIdAsync(Guid id)
        {
            return await _exceptionHandler.HandleExceptionAsync(async () =>
            {
                return await _unitOfWork.EntityFaturaRepository.GetByIdWithIncludesAsync(id);
            }, "GetByIdAsync", id);
        }

        /// <summary>
        /// Yeni fatura ekler
        /// </summary>
        public async Task<Fatura> AddAsync(Fatura fatura)
        {
            return await _transactionService.ExecuteInTransactionAsync(async () =>
            {
                _logger.LogInformation("Yeni fatura ekleniyor...");
                
                // Fatura numarası oluştur (ancak bu işlev FaturaNumaralandirmaService'e taşındı)
                if (string.IsNullOrEmpty(fatura.FaturaNumarasi))
                {
                    // Bu sadece geçiş için burada, gerçekte FaturaNumaralandirmaService kullanılmalı
                    fatura.FaturaNumarasi = $"FTR-{DateTime.Now:yyyyMMdd}-{Guid.NewGuid().ToString().Substring(0, 4)}";
                }
                
                // Fatura ve detaylarını ekle
                await _unitOfWork.FaturaRepository.AddAsync(fatura);
                await _unitOfWork.SaveChangesAsync();
                
                _logger.LogInformation("Fatura başarıyla eklendi, FaturaID: {FaturaID}", fatura.FaturaID);
                
                return fatura;
            }, "AddAsync", fatura.CariID, fatura.FaturaTuruID, fatura.FaturaNumarasi);
        }

        /// <summary>
        /// Faturayı günceller
        /// </summary>
        public async Task<Fatura> UpdateAsync(Fatura fatura)
        {
            return await _transactionService.ExecuteInTransactionAsync(async () =>
            {
                _logger.LogInformation("Fatura güncelleniyor, FaturaID: {FaturaID}", fatura.FaturaID);
                
                // Mevcut faturayı kontrol et
                var mevcutFatura = await _unitOfWork.FaturaRepository.GetByIdAsync(fatura.FaturaID);
                if (mevcutFatura == null)
                {
                    throw new EntityNotFoundException($"FaturaID: {fatura.FaturaID} bulunamadı");
                }
                
                // Güncelleme tarihini ayarla
                fatura.GuncellemeTarihi = DateTime.Now;
                
                // Faturayı güncelle
                _unitOfWork.FaturaRepository.Update(fatura);
                await _unitOfWork.SaveChangesAsync();
                
                _logger.LogInformation("Fatura başarıyla güncellendi, FaturaID: {FaturaID}", fatura.FaturaID);
                
                return fatura;
            }, "UpdateAsync", fatura.FaturaID);
        }

        /// <summary>
        /// Faturayı siler (soft delete)
        /// </summary>
        public async Task<bool> DeleteAsync(Guid id)
        {
            return await _transactionService.ExecuteInTransactionAsync(async () =>
            {
                _logger.LogInformation("Fatura siliniyor, FaturaID: {FaturaID}", id);
                
                // Mevcut faturayı kontrol et
                var fatura = await _unitOfWork.FaturaRepository.GetByIdAsync(id);
                if (fatura == null)
                {
                    throw new EntityNotFoundException($"FaturaID: {id} bulunamadı");
                }
                
                // Mantıksal silme
                fatura.Silindi = true;
                fatura.GuncellemeTarihi = DateTime.Now;
                
                // Faturayı güncelle
                _unitOfWork.FaturaRepository.Update(fatura);
                
                // Fatura detaylarını da mantıksal olarak sil
                var faturaDetaylari = await _unitOfWork.EntityFaturaRepository.GetFaturaDetaylarAsync(id);
                foreach (var detay in faturaDetaylari)
                {
                    detay.Silindi = true;
                    detay.GuncellemeTarihi = DateTime.Now;
                    _unitOfWork.FaturaDetayRepository.Update(detay);
                }
                
                await _unitOfWork.SaveChangesAsync();
                _logger.LogInformation("Fatura başarıyla silindi, FaturaID: {FaturaID}", id);
                
                return true;
            }, "DeleteAsync", id);
        }

        /// <summary>
        /// Fatura detay view model'i getirir
        /// </summary>
        public async Task<FaturaDetailViewModel> GetFaturaDetailViewModelAsync(Guid id)
        {
            var fatura = await GetByIdAsync(id);
            if (fatura == null)
                return null;

            var viewModel = new FaturaDetailViewModel
            {
                FaturaID = fatura.FaturaID,
                FaturaNumarasi = fatura.FaturaNumarasi,
                FaturaTarihi = fatura.FaturaTarihi,
                VadeTarihi = fatura.VadeTarihi,
                CariAdi = fatura.Cari?.Ad ?? string.Empty,
                CariID = fatura.CariID ?? Guid.Empty,
                CariVergiNo = fatura.Cari?.VergiNo ?? string.Empty,
                CariVergiDairesi = fatura.Cari?.VergiDairesi,
                CariAdres = fatura.Cari?.Adres ?? string.Empty,
                CariTelefon = fatura.Cari?.Telefon ?? string.Empty,
                AraToplam = fatura.AraToplam ?? 0,
                KdvTutari = fatura.KDVToplam ?? 0,
                IndirimTutari = fatura.IndirimTutari ?? 0,
                GenelToplam = fatura.GenelToplam ?? 0,
                AraToplamDoviz = fatura.AraToplamDoviz ?? 0,
                KdvTutariDoviz = fatura.KDVToplamDoviz ?? 0,
                IndirimTutariDoviz = fatura.IndirimTutariDoviz ?? 0,
                GenelToplamDoviz = fatura.GenelToplamDoviz ?? 0,
                OdemeDurumu = fatura.OdemeDurumu,
                DovizTuru = fatura.DovizTuru,
                DovizKuru = fatura.DovizKuru ?? 1,
                Aktif = fatura.Aktif,
                OlusturmaTarihi = fatura.OlusturmaTarihi,
                GuncellemeTarihi = fatura.GuncellemeTarihi,
                FaturaTuru = fatura.FaturaTuru?.FaturaTuruAdi ?? string.Empty,
                FaturaTuruID = fatura.FaturaTuruID,
                ResmiMi = fatura.ResmiMi,
                SiparisNumarasi = fatura.SiparisNumarasi,
                FaturaKalemleri = fatura.FaturaDetaylari?.Select(fk => new FaturaKalemDetailViewModel
                {
                    KalemID = fk.FaturaDetayID,
                    UrunID = fk.UrunID,
                    UrunKodu = fk.Urun?.UrunKodu ?? string.Empty,
                    UrunAdi = fk.Urun?.UrunAdi ?? string.Empty,
                    Miktar = fk.Miktar,
                    Birim = fk.Birim ?? string.Empty,
                    BirimFiyat = fk.BirimFiyat,
                    KdvOrani = (int)fk.KdvOrani,
                    IndirimOrani = (int)fk.IndirimOrani,
                    Tutar = fk.Tutar ?? 0,
                    KdvTutari = fk.KdvTutari ?? 0,
                    IndirimTutari = fk.IndirimTutari ?? 0,
                    NetTutar = fk.NetTutar ?? 0,
                    BirimFiyatDoviz = fk.BirimFiyatDoviz,
                    TutarDoviz = fk.TutarDoviz ?? 0,
                    KdvTutariDoviz = fk.KdvTutariDoviz ?? 0,
                    IndirimTutariDoviz = fk.IndirimTutariDoviz ?? 0,
                    NetTutarDoviz = fk.NetTutarDoviz ?? 0
                }).ToList() ?? new List<FaturaKalemDetailViewModel>()
            };

            return viewModel;
        }

        /// <summary>
        /// Tüm faturaların view model listesini getirir
        /// </summary>
        public async Task<List<FaturaViewModel>> GetAllFaturaViewModelsAsync()
        {
            var faturalar = await GetAllAsync();
            
            return faturalar.Select(f => new FaturaViewModel
            {
                FaturaID = f.FaturaID.ToString(),
                FaturaNumarasi = f.FaturaNumarasi,
                FaturaTarihi = f.FaturaTarihi,
                CariAdi = f.Cari?.Ad ?? string.Empty,
                GenelToplam = f.GenelToplam ?? 0,
                FaturaTuru = f.FaturaTuru?.FaturaTuruAdi ?? string.Empty,
                OdemeDurumu = f.OdemeDurumu ?? string.Empty,
                Aciklama = f.FaturaNotu ?? string.Empty,
                DovizTuru = f.DovizTuru,
                DovizKuru = f.DovizKuru ?? 1
            }).ToList();
        }

        /// <summary>
        /// Faturanın kullanımda olup olmadığını kontrol eder
        /// </summary>
        public async Task<bool> IsFaturaInUseAsync(Guid id)
        {
            // Faturanın kullanımda olup olmadığını kontrol etme mantığı
            var faturaOdemeleri = await _unitOfWork.FaturaOdemeleriRepository.GetAll()
                .AnyAsync(o => o.FaturaID == id && !o.Silindi);
                
            return faturaOdemeleri;
        }

        private async Task<Fatura> AutoMapFatura(FaturaCreateViewModel dto, Guid? currentUserId)
        {
            var fatura = new Fatura
            {
                FaturaID = Guid.NewGuid(),
                FaturaNumarasi = dto.FaturaNumarasi,
                SiparisNumarasi = dto.SiparisNumarasi,
                FaturaTarihi = dto.FaturaTarihi,
                VadeTarihi = dto.VadeTarihi,
                FaturaTuruID = dto.FaturaTuruID,
                CariID = dto.CariID,
                IndirimTutari = dto.IndirimTutari ?? 0,
                GenelToplam = dto.GenelToplam,
                KDVToplam = dto.KDVToplam,
                AraToplam = dto.AraToplam,
                DovizTuru = dto.DovizTuru,
                ParaBirimi = dto.DovizTuru, // DovizTuru alanını ParaBirimi olarak kullan
                DovizKuru = dto.DovizKuru,
                GenelToplamDoviz = dto.GenelToplamDoviz,
                KDVToplamDoviz = dto.KDVToplamDoviz,
                AraToplamDoviz = dto.AraToplamDoviz,
                IndirimTutariDoviz = dto.IndirimTutariDoviz ?? 0,
                OlusturmaTarihi = DateTime.Now,
                OlusturanKullaniciID = currentUserId,
                FaturaNotu = dto.Aciklama,
                Silindi = false
            };

            return fatura;
        }
    }
} 