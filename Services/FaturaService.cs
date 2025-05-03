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
    // Hata sınıfı tanımı
    public class EntityNotFoundException : Exception
    {
        public EntityNotFoundException(string message) : base(message) { }
    }

    public class FaturaService : IFaturaService
    {
        private readonly ILogger<FaturaService> _logger;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IStokFifoService _stokFifoService;
        private readonly IDovizKuruService _dovizKuruService;
        private readonly IMaliyetHesaplamaService _maliyetHesaplamaService;
        private readonly IFaturaValidationService _faturaValidationService;
        private readonly ICariHareketService _cariHareketService;
        private readonly IStokHareketService _stokHareketService;
        private readonly IIrsaliyeService _irsaliyeService;
        private readonly IExceptionHandlingService _exceptionHandler;
        private bool _transactionActive = false;

        public FaturaService(
            IUnitOfWork unitOfWork,
            ILogger<FaturaService> logger,
            IStokFifoService stokFifoService,
            IDovizKuruService dovizKuruService,
            IMaliyetHesaplamaService maliyetHesaplamaService,
            IFaturaValidationService faturaValidationService,
            ICariHareketService cariHareketService,
            IStokHareketService stokHareketService,
            IIrsaliyeService irsaliyeService,
            IExceptionHandlingService exceptionHandler)
        {
            _unitOfWork = unitOfWork;
            _logger = logger;
            _stokFifoService = stokFifoService;
            _dovizKuruService = dovizKuruService;
            _maliyetHesaplamaService = maliyetHesaplamaService;
            _faturaValidationService = faturaValidationService;
            _cariHareketService = cariHareketService;
            _stokHareketService = stokHareketService;
            _irsaliyeService = irsaliyeService;
            _exceptionHandler = exceptionHandler;
        }

        /// <summary>
        /// Transaction kontrolü yapar ve gerekirse yeni bir transaction başlatır
        /// </summary>
        private async Task EnsureTransactionAsync()
        {
            // Eğer transaction yoksa, yeni bir transaction başlat
            if (!_transactionActive)
            {
                await _unitOfWork.BeginTransactionAsync();
                _transactionActive = true;
            }
        }

        /// <summary>
        /// Transaction'ı commit eder ve transaction durumunu sıfırlar
        /// </summary>
        private async Task CommitTransactionAsync()
        {
            if (_transactionActive)
            {
                await _unitOfWork.CommitTransactionAsync();
                _transactionActive = false;
            }
        }

        /// <summary>
        /// Transaction'ı rollback eder ve transaction durumunu sıfırlar
        /// </summary>
        private async Task RollbackTransactionAsync()
        {
            if (_transactionActive)
            {
                await _unitOfWork.RollbackTransactionAsync();
                _transactionActive = false;
            }
        }

        /// <summary>
        /// Transaction içinde çalıştırılması gereken işlemleri yönetir
        /// </summary>
        private async Task<T> ExecuteInTransactionAsync<T>(Func<Task<T>> action, string operationName, params object[] parameters)
        {
            return await _exceptionHandler.HandleExceptionAsync(async () =>
            {
                // Eğer dışarıdan başlatılmış bir transaction varsa, onun içinde çalış
                if (_transactionActive)
                {
                    return await action();
                }
                
                // Kendi transaction'ını başlat
                await EnsureTransactionAsync();
                try
                {
                    var result = await action();
                    await CommitTransactionAsync();
                    return result;
                }
                catch
                {
                    await RollbackTransactionAsync();
                    throw;
                }
            }, $"FaturaService.{operationName}", parameters);
        }

        /// <summary>
        /// Transaction içinde çalıştırılması gereken void işlemleri yönetir
        /// </summary>
        private async Task ExecuteInTransactionAsync(Func<Task> action, string operationName, params object[] parameters)
        {
            await _exceptionHandler.HandleExceptionAsync(async () =>
            {
                // Eğer dışarıdan başlatılmış bir transaction varsa, onun içinde çalış
                if (_transactionActive)
                {
                    await action();
                    return;
                }
                
                // Kendi transaction'ını başlat
                await EnsureTransactionAsync();
                try
                {
                    await action();
                    await CommitTransactionAsync();
                }
                catch
                {
                    await RollbackTransactionAsync();
                    throw;
                }
            }, $"FaturaService.{operationName}", parameters);
        }

        public async Task<IEnumerable<Data.Entities.Fatura>> GetAllAsync()
        {
            return await _exceptionHandler.HandleExceptionAsync(async () =>
            {
                return await _unitOfWork.EntityFaturaRepository.GetAllWithIncludesAsync();
            }, "GetAllAsync");
        }

        public async Task<Data.Entities.Fatura> GetByIdAsync(Guid id)
        {
            return await _exceptionHandler.HandleExceptionAsync(async () =>
            {
                return await _unitOfWork.EntityFaturaRepository.GetByIdWithIncludesAsync(id);
            }, "GetByIdAsync", id);
        }

        public async Task<Data.Entities.Fatura> AddAsync(Data.Entities.Fatura fatura)
        {
            return await ExecuteInTransactionAsync(async () =>
            {
                _logger.LogInformation("Yeni fatura ekleniyor...");
                
                // Fatura numarası oluştur
                if (string.IsNullOrEmpty(fatura.FaturaNumarasi))
                {
                    // TODO: Fatura numarası oluşturma mantığı ekle
                    fatura.FaturaNumarasi = $"FTR-{DateTime.Now:yyyyMMdd}-{Guid.NewGuid().ToString().Substring(0, 4)}";
                }
                
                // Fatura ve detaylarını ekle
                await _unitOfWork.FaturaRepository.AddAsync(fatura);
                await _unitOfWork.SaveChangesAsync();
                
                _logger.LogInformation("Fatura başarıyla eklendi, FaturaID: {FaturaID}", fatura.FaturaID);
                
                return fatura;
            }, "AddAsync", fatura.CariID, fatura.FaturaTuruID, fatura.FaturaNumarasi);
        }

        public async Task<Data.Entities.Fatura> UpdateAsync(Data.Entities.Fatura fatura)
        {
            return await ExecuteInTransactionAsync(async () =>
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

        public async Task<bool> DeleteAsync(Guid id)
        {
            return await ExecuteInTransactionAsync(async () =>
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

        public async Task<bool> DeleteFatura(Guid id, Guid? currentUserId)
        {
            return await ExecuteInTransactionAsync(async () =>
            {
                _logger.LogInformation("DeleteFatura çağrıldı, FaturaID: {0}, İşlemi Yapan: {1}", id, currentUserId);
                
                // Faturanın kullanımda olup olmadığını kontrol et
                if (await IsFaturaInUseAsync(id))
                {
                    _logger.LogWarning("Fatura başka kayıtlarda kullanıldığı için silinemez. FaturaID: {0}", id);
                    throw new BusinessException("Bu fatura başka kayıtlarda kullanıldığı için silinemez.");
                }
                
                // Mevcut faturayı kontrol et
                var fatura = await _unitOfWork.FaturaRepository.GetByIdAsync(id);
                if (fatura == null)
                {
                    throw new EntityNotFoundException($"FaturaID: {id} bulunamadı");
                }
                
                // Faturayı sil
                await DeleteAsync(id);
                _logger.LogInformation("Fatura başarıyla silindi, FaturaID: {0}", id);
                
                return true;
            }, "DeleteFatura", id, currentUserId);
        }

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

        public async Task<bool> IsFaturaInUseAsync(Guid id)
        {
            // Faturanın kullanımda olup olmadığını kontrol etme mantığı
            // Örneğin, ödeme kayıtlarında veya başka bir ilişkili tabloda kullanılıyorsa true dönebilir
            
            var isUsedInPayments = await _unitOfWork.FaturaOdemeleriRepository.AnyAsync(o => o.FaturaID == id && !o.Silindi);
                
            return isUsedInPayments;
        }
        
        /// <summary>
        /// Fatura ve ilişkili kayıtları (FaturaDetay, StokHareket, StokFifo, CariHareket) oluşturur.
        /// Tüm işlemler tek bir transaction içinde gerçekleştirilir.
        /// </summary>
        /// <param name="viewModel">Fatura oluşturma view model</param>
        /// <param name="currentUserId">İşlemi yapan kullanıcı ID</param>
        /// <returns>Oluşturulan faturanın ID'si</returns>
        public async Task<Guid> CreateFatura(FaturaCreateViewModel viewModel, Guid? currentUserId)
        {
            _logger.LogInformation("FaturaService.CreateFatura başlatılıyor");
            
            // ViewModel validasyonu
            if (viewModel.FaturaKalemleri == null || !viewModel.FaturaKalemleri.Any(k => k.UrunID != Guid.Empty && k.Miktar > 0))
            {
                _logger.LogWarning("Geçerli fatura kalemi bulunamadı");
                throw new ArgumentException("En az bir geçerli fatura kalemi eklemelisiniz");
            }
            
            // Fatura tutarlılık validasyonu - toplamların doğru olup olmadığı kontrolü
            decimal hesaplananAraToplam = 0;
            decimal hesaplananKdvTutari = 0;
            decimal hesaplananIndirimTutari = 0;
            
            foreach (var kalem in viewModel.FaturaKalemleri.Where(k => k.UrunID != Guid.Empty && k.Miktar > 0))
            {
                decimal kalemTutar = kalem.Miktar * kalem.BirimFiyat;
                decimal kalemIndirimTutari = kalemTutar * kalem.IndirimOrani / 100;
                decimal kalemKdvMatrahi = kalemTutar - kalemIndirimTutari;
                decimal kalemKdvTutari = kalemKdvMatrahi * kalem.KdvOrani / 100;
                
                hesaplananAraToplam += kalemTutar;
                hesaplananIndirimTutari += kalemIndirimTutari;
                hesaplananKdvTutari += kalemKdvTutari;
            }
            
            decimal hesaplananGenelToplam = hesaplananAraToplam - hesaplananIndirimTutari + hesaplananKdvTutari;
            
            // Hesaplanan değerler ile viewModel'deki değerlerin karşılaştırılması
            if (Math.Abs(hesaplananAraToplam - viewModel.AraToplam) > 0.01m ||
                Math.Abs(hesaplananKdvTutari - viewModel.KdvToplam) > 0.01m ||
                Math.Abs(hesaplananIndirimTutari - viewModel.IndirimTutari) > 0.01m ||
                Math.Abs(hesaplananGenelToplam - viewModel.GenelToplam) > 0.01m)
            {
                _logger.LogWarning("Fatura tutarları tutarsız. Hesaplanan değerler: AraToplam={AraToplam}, KdvTutari={KdvTutari}, IndirimTutari={IndirimTutari}, GenelToplam={GenelToplam}",
                    hesaplananAraToplam, hesaplananKdvTutari, hesaplananIndirimTutari, hesaplananGenelToplam);
                throw new ArgumentException("Fatura tutarları tutarsız. Lütfen tutarları kontrol ediniz.");
            }
            
            // UnitOfWork ile transaction başlat
            await _unitOfWork.BeginTransactionAsync();
            
            try
            {
                // Fatura türünü al, stok hareket tipini belirle
                var faturaTuru = await _unitOfWork.FaturaTurleriRepository.GetByIdAsync(viewModel.FaturaTuruID);
                var stokHareketTipi = faturaTuru?.HareketTuru == "Giriş" 
                    ? StokHareketiTipi.Giris 
                    : StokHareketiTipi.Cikis;
                
                // 1. Fatura oluştur
                var fatura = new Data.Entities.Fatura
                {
                    FaturaID = Guid.NewGuid(),
                    FaturaNumarasi = string.IsNullOrEmpty(viewModel.FaturaNumarasi) ? GenerateNewFaturaNumarasi() : viewModel.FaturaNumarasi,
                    FaturaTarihi = viewModel.FaturaTarihi,
                    VadeTarihi = viewModel.VadeTarihi,
                    CariID = viewModel.CariID,
                    FaturaNotu = viewModel.FaturaNotu,
                    AraToplam = viewModel.AraToplam,
                    KDVToplam = viewModel.KdvToplam,
                    IndirimTutari = viewModel.IndirimTutari,
                    GenelToplam = viewModel.GenelToplam,
                    OdemeDurumu = "Ödenmedi",
                    DovizTuru = viewModel.DovizTuru,
                    DovizKuru = viewModel.DovizKuru,
                    AraToplamDoviz = viewModel.AraToplamDoviz,
                    KDVToplamDoviz = viewModel.KdvToplamDoviz,
                    IndirimTutariDoviz = viewModel.IndirimTutari,
                    GenelToplamDoviz = viewModel.GenelToplamDoviz,
                    OlusturmaTarihi = DateTime.Now,
                    OlusturanKullaniciID = currentUserId,
                    Aktif = true,
                    SiparisNumarasi = string.IsNullOrEmpty(viewModel.SiparisNumarasi) ? GenerateSiparisNumarasi() : viewModel.SiparisNumarasi,
                    FaturaTuruID = viewModel.FaturaTuruID,
                    ResmiMi = viewModel.ResmiMi
                };
                
                await _unitOfWork.FaturaRepository.AddAsync(fatura);
                
                // 2. Fatura detaylarını oluştur
                var faturaDetaylari = new List<FaturaDetay>();
                foreach (var kalem in viewModel.FaturaKalemleri.Where(k => k.UrunID != Guid.Empty && k.Miktar > 0))
                {
                    var detay = new FaturaDetay
                    {
                        FaturaDetayID = Guid.NewGuid(),
                        FaturaID = fatura.FaturaID,
                        UrunID = kalem.UrunID,
                        Miktar = kalem.Miktar,
                        Birim = kalem.Birim,
                        BirimFiyat = kalem.BirimFiyat,
                        KdvOrani = kalem.KdvOrani,
                        IndirimOrani = kalem.IndirimOrani,
                        Tutar = kalem.Tutar,
                        KdvTutari = kalem.KdvTutari,
                        NetTutar = kalem.NetTutar,
                        IndirimTutari = kalem.IndirimTutari,
                        BirimFiyatDoviz = kalem.BirimFiyatDoviz,
                        TutarDoviz = kalem.TutarDoviz,
                        KdvTutariDoviz = kalem.KdvTutariDoviz,
                        NetTutarDoviz = kalem.NetTutarDoviz,
                        IndirimTutariDoviz = kalem.IndirimTutariDoviz,
                        OlusturmaTarihi = DateTime.Now,
                        Aktif = true,
                        Aciklama = kalem.Aciklama,
                        SatirToplam = kalem.Tutar,
                        SatirKdvToplam = kalem.KdvTutari
                    };
                    
                    faturaDetaylari.Add(detay);
                }
                
                await _unitOfWork.FaturaDetayRepository.AddRangeAsync(faturaDetaylari);
                
                // 3. Stok hareketlerini oluştur
                var stokHareketService = new StokHareketService(_unitOfWork.EntityStokHareketRepository, _unitOfWork.EntityStokFifoRepository, _logger as ILogger<StokHareketService>, _stokFifoService);
                var stokHareketleri = await stokHareketService.CreateStokHareket(fatura, faturaDetaylari, currentUserId);
                
                // 4. FIFO kayıtlarını oluştur (StokFifoService içinde yapılıyor)
                if (stokHareketTipi == StokHareketiTipi.Giris)
                {
                    foreach (var stokHareket in stokHareketleri)
                    {
                        var faturaDetay = faturaDetaylari.FirstOrDefault(fd => fd.UrunID == stokHareket.UrunID);
                        if (faturaDetay != null)
                        {
                            await _stokFifoService.StokGirisiYap(
                                stokHareket.UrunID,
                                faturaDetay.Miktar,
                                faturaDetay.BirimFiyat,
                                faturaDetay.Birim ?? "Adet",
                                fatura.FaturaNumarasi,
                                "Fatura",
                                fatura.FaturaID,
                                $"{fatura.FaturaNumarasi} numaralı fatura ile giriş",
                                fatura.DovizTuru ?? "TRY",
                                fatura.DovizKuru ?? 1m
                            );
                        }
                    }
                }
                else if (stokHareketTipi == StokHareketiTipi.Cikis)
                {
                    foreach (var stokHareket in stokHareketleri)
                    {
                        var faturaDetay = faturaDetaylari.FirstOrDefault(fd => fd.UrunID == stokHareket.UrunID);
                        if (faturaDetay != null)
                        {
                            await _stokFifoService.StokCikisiYap(
                                stokHareket.UrunID,
                                faturaDetay.Miktar,
                                fatura.FaturaNumarasi,
                                "Fatura",
                                fatura.FaturaID,
                                $"{fatura.FaturaNumarasi} numaralı fatura ile çıkış"
                            );
                        }
                    }
                }
                
                // 5. Cari hareket kaydı oluştur
                if (fatura.CariID != null && fatura.CariID != Guid.Empty)
                {
                    // CariHareketService'i kullanarak cari hareket oluştur
                    // Bu işlemi aynı transaction içinde yapıyoruz
                    var cariHareketService = new CariHareketService(_unitOfWork.EntityCariHareketRepository, null, _unitOfWork, _logger as ILogger<CariHareketService>);
                    await cariHareketService.CreateFromFaturaAsync(fatura, currentUserId);
                }
                
                // 6. İrsaliye oluştur
                if (viewModel.OtomatikIrsaliyeOlustur)
                {
                    try
                    {
                        // _irsaliyeService servisini kullan (DI ile alınan)
                        await _irsaliyeService.OtomatikIrsaliyeOlustur(fatura, viewModel.DepoID);
                    }
                    catch (Exception irsaliyeEx)
                    {
                        _logger.LogError(irsaliyeEx, "İrsaliye oluşturulurken hata: {Message}", irsaliyeEx.Message);
                        // İrsaliye hatası faturanın oluşturulmasını durdurmaz, loglayıp devam ediyoruz
                    }
                }
                
                // 7. Tüm değişiklikleri tek seferde kaydet
                await _unitOfWork.CompleteAsync();
                
                // Transaction'ı commit et
                await _unitOfWork.CommitTransactionAsync();
                
                _logger.LogInformation("FaturaService.CreateFatura tamamlandı. FaturaID: {FaturaID}", fatura.FaturaID);
                
                return fatura.FaturaID;
            }
            catch (Exception ex)
            {
                // Transaction'ı geri al
                await _unitOfWork.RollbackTransactionAsync();
                _logger.LogError(ex, "Fatura oluşturma hatası: {Message}", ex.Message);
                throw;
            }
        }
        
        // Yardımcı metotlar
        
        /// <summary>
        /// Yeni bir fatura numarası oluşturur
        /// </summary>
        private string GenerateNewFaturaNumarasi()
        {
            var today = DateTime.Now;
            var year = today.Year.ToString().Substring(2);
            var month = today.Month.ToString().PadLeft(2, '0');
            var day = today.Day.ToString().PadLeft(2, '0');
            
            var prefix = $"FTR-{year}{month}{day}-";
            
            // Son fatura numarasını bul ve arttır
            var lastFatura = _unitOfWork.EntityFaturaRepository.GetAll()
                .Where(f => f.FaturaNumarasi != null && f.FaturaNumarasi.StartsWith(prefix))
                .OrderByDescending(f => f.FaturaNumarasi)
                .FirstOrDefault();
            
            int sequence = 1;
            if (lastFatura != null && lastFatura.FaturaNumarasi != null)
            {
                var parts = lastFatura.FaturaNumarasi.Split('-');
                if (parts.Length == 3 && int.TryParse(parts[2], out int lastSeq))
                {
                    sequence = lastSeq + 1;
                }
            }
            
            return $"{prefix}{sequence.ToString().PadLeft(3, '0')}";
        }
        
        /// <summary>
        /// Yeni bir sipariş numarası oluşturur
        /// </summary>
        private string GenerateSiparisNumarasi()
        {
            var today = DateTime.Now;
            var year = today.Year.ToString().Substring(2);
            var month = today.Month.ToString().PadLeft(2, '0');
            var day = today.Day.ToString().PadLeft(2, '0');
            
            var prefix = $"SIP-{year}{month}{day}-";
            
            // Son sipariş numarasını bul ve arttır
            var lastFatura = _unitOfWork.EntityFaturaRepository.GetAll()
                .Where(f => f.SiparisNumarasi != null && f.SiparisNumarasi.StartsWith(prefix))
                .OrderByDescending(f => f.SiparisNumarasi)
                .FirstOrDefault();
            
            int sequence = 1;
            if (lastFatura != null && lastFatura.SiparisNumarasi != null)
            {
                var parts = lastFatura.SiparisNumarasi.Split('-');
                if (parts.Length == 3 && int.TryParse(parts[2], out int lastSeq))
                {
                    sequence = lastSeq + 1;
                }
            }
            
            return $"{prefix}{sequence.ToString().PadLeft(3, '0')}";
        }
        
        /// <summary>
        /// Faturayı ve ilişkili kayıtları günceller.
        /// Eski kayıtları iptal eder ve yenilerini oluşturur.
        /// </summary>
        /// <param name="id">Güncellenecek fatura ID</param>
        /// <param name="viewModel">Fatura güncelleme view model</param>
        /// <param name="currentUserId">İşlemi yapan kullanıcı ID</param>
        /// <returns>Güncellenen faturanın ID'si</returns>
        public async Task<Guid> UpdateFatura(Guid id, FaturaEditViewModel viewModel, Guid? currentUserId)
        {
            _logger.LogInformation("FaturaService.UpdateFatura başlatılıyor. FaturaID: {FaturaID}", id);
            
            // Faturayı detaylarıyla birlikte getir
            var existingFatura = await _unitOfWork.EntityFaturaRepository.GetByIdWithIncludesAsync(id);
                
            if (existingFatura == null)
            {
                _logger.LogWarning("Güncellenmek istenen fatura bulunamadı. FaturaID: {FaturaID}", id);
                throw new Exception("Güncellenecek fatura bulunamadı.");
            }
            
            // UnitOfWork ile transaction başlat
            await _unitOfWork.BeginTransactionAsync();
            
            try
            {
                // Stok hareket tipini belirle
                var faturaTuru = await _unitOfWork.FaturaTurleriRepository.GetByIdAsync(viewModel.FaturaTuruID);
                var stokHareketTipi = faturaTuru?.HareketTuru == "Giriş" 
                    ? StokHareketiTipi.Giris 
                    : StokHareketiTipi.Cikis;
                
                // 1. Eski kayıtları iptal et
                
                // 1.1. Faturaya bağlı FIFO kayıtlarını iptal et
                await _stokFifoService.FifoKayitlariniIptalEt(id, "Fatura", "Fatura güncelleme işlemi nedeniyle iptal edildi");
                _logger.LogInformation("Faturaya bağlı FIFO kayıtları iptal edildi. FaturaID: {FaturaID}", id);
                
                // 1.2. Faturaya bağlı stok hareketlerini iptal et (silindi olarak işaretle)
                var stokHareketleri = await _unitOfWork.EntityStokHareketRepository.GetAll()
                    .Where(sh => sh.FaturaID == id && !sh.Silindi)
                    .ToListAsync();
                    
                foreach (var hareket in stokHareketleri)
                {
                    hareket.Silindi = true;
                    hareket.OlusturmaTarihi = DateTime.Now; // Değişiklik saati olarak oluşturma tarihini güncelle
                    _unitOfWork.EntityStokHareketRepository.Update(hareket);
                }
                _logger.LogInformation("Faturaya bağlı stok hareketleri iptal edildi. FaturaID: {FaturaID}", id);
                
                // 1.3. Faturaya bağlı eski CariHareket kayıtlarını bul ve iptal et
                var cariHareketler = await _unitOfWork.EntityCariHareketRepository.GetAll()
                    .Where(ch => ch.ReferansID == id && ch.ReferansTuru == "Fatura" && !ch.Silindi)
                    .ToListAsync();
                    
                foreach (var cariHareket in cariHareketler)
                {
                    cariHareket.Silindi = true;
                    cariHareket.GuncellemeTarihi = DateTime.Now;
                    _unitOfWork.EntityCariHareketRepository.Update(cariHareket);
                }
                _logger.LogInformation("Faturaya bağlı cari hareketler iptal edildi. FaturaID: {FaturaID}", id);
                
                // 1.4. Eski fatura detaylarını sil
                _unitOfWork.EntityFaturaDetayRepository.RemoveRange(existingFatura.FaturaDetaylari);
                _logger.LogInformation("Eski fatura detayları silindi. FaturaID: {FaturaID}", id);
                
                // Değişiklikleri kaydet
                await _unitOfWork.CompleteAsync();
                
                // 2. Fatura ana kaydını güncelle
                existingFatura.FaturaNumarasi = viewModel.FaturaNumarasi;
                existingFatura.SiparisNumarasi = viewModel.SiparisNumarasi;
                existingFatura.FaturaTarihi = viewModel.FaturaTarihi;
                existingFatura.VadeTarihi = viewModel.VadeTarihi;
                existingFatura.CariID = viewModel.CariID;
                existingFatura.FaturaTuruID = viewModel.FaturaTuruID;
                existingFatura.ResmiMi = viewModel.ResmiMi;
                existingFatura.FaturaNotu = viewModel.Aciklama; // Aciklama alanını FaturaNotu'na ata
                existingFatura.OdemeDurumu = viewModel.OdemeDurumu ?? existingFatura.OdemeDurumu;
                existingFatura.DovizTuru = viewModel.DovizTuru;
                existingFatura.DovizKuru = viewModel.DovizKuru;
                existingFatura.SozlesmeID = viewModel.SozlesmeID;
                existingFatura.GuncellemeTarihi = DateTime.Now;
                
                // 3. Yeni fatura kalemlerini oluştur ve toplamları hesapla
                decimal araToplam = 0;
                decimal kdvToplam = 0;
                decimal indirimToplam = 0;
                decimal genelToplam = 0;
                
                var faturaDetaylari = new List<FaturaDetay>();
                
                foreach (var kalem in viewModel.FaturaKalemleri)
                {
                    if (kalem.UrunID == Guid.Empty || kalem.Miktar <= 0)
                    {
                        continue; // Geçersiz kalemleri atla
                    }
                    
                    // Kalem hesaplamaları
                    decimal tutar = kalem.Miktar * kalem.BirimFiyat;
                    decimal indirimTutari = tutar * (kalem.IndirimOrani / 100M);
                    decimal araTutar = tutar - indirimTutari;
                    decimal kdvTutari = araTutar * (kalem.KdvOrani / 100M);
                    decimal netTutar = araTutar + kdvTutari;
                    
                    // Toplam hesaplamalar
                    araToplam += tutar;
                    indirimToplam += indirimTutari;
                    kdvToplam += kdvTutari;
                    genelToplam += netTutar;
                    
                    // Fatura detayı oluştur
                    var faturaDetay = new FaturaDetay
                    {
                        FaturaDetayID = Guid.NewGuid(),
                        FaturaID = existingFatura.FaturaID,
                        UrunID = kalem.UrunID,
                        Aciklama = kalem.Aciklama,
                        Miktar = kalem.Miktar,
                        Birim = kalem.Birim,
                        BirimFiyat = kalem.BirimFiyat,
                        Tutar = tutar,
                        IndirimOrani = kalem.IndirimOrani,
                        IndirimTutari = indirimTutari,
                        KdvOrani = kalem.KdvOrani,
                        KdvTutari = kdvTutari,
                        NetTutar = netTutar,
                        SatirToplam = tutar,
                        SatirKdvToplam = kdvTutari,
                        OlusturmaTarihi = DateTime.Now
                    };
                    
                    faturaDetaylari.Add(faturaDetay);
                }
                
                // Toplam değerleri faturaya ata
                existingFatura.AraToplam = araToplam;
                existingFatura.IndirimTutari = indirimToplam;
                existingFatura.KDVToplam = kdvToplam;
                existingFatura.GenelToplam = genelToplam;
                
                // Dövizli değerleri hesapla
                decimal dovizKuru = viewModel.DovizKuru ?? 1M;
                existingFatura.AraToplamDoviz = araToplam / dovizKuru;
                existingFatura.IndirimTutariDoviz = indirimToplam / dovizKuru;
                existingFatura.KDVToplamDoviz = kdvToplam / dovizKuru;
                existingFatura.GenelToplamDoviz = genelToplam / dovizKuru;
                
                // Faturayı güncelle ve yeni detayları ekle
                _unitOfWork.EntityFaturaRepository.Update(existingFatura);
                _unitOfWork.EntityFaturaDetayRepository.AddRange(faturaDetaylari);
                
                // DepoID'yi al
                Guid? depoID = await GetDepoIDFromViewModel(viewModel);
                
                // 4. Yeni stok hareketlerini oluştur
                var yeniStokHareketleri = new List<StokHareket>();
                foreach (var detay in faturaDetaylari)
                {
                    string irsaliyeTuruStr = stokHareketTipi == StokHareketiTipi.Giris ? "Giriş" : "Çıkış";
                    
                    var stokHareket = new StokHareket
                    {
                        StokHareketID = Guid.NewGuid(),
                        UrunID = detay.UrunID,
                        DepoID = depoID,
                        Miktar = stokHareketTipi == StokHareketiTipi.Cikis ? -detay.Miktar : detay.Miktar,
                        Birim = detay.Birim ?? "Adet",
                        HareketTuru = stokHareketTipi,
                        Tarih = existingFatura.FaturaTarihi ?? DateTime.Now,
                        ReferansNo = existingFatura.FaturaNumarasi ?? "",
                        ReferansTuru = "Fatura",
                        ReferansID = existingFatura.FaturaID,
                        FaturaID = existingFatura.FaturaID,
                        Aciklama = $"{existingFatura.FaturaNumarasi} numaralı fatura (güncelleme)",
                        BirimFiyat = detay.BirimFiyat,
                        OlusturmaTarihi = DateTime.Now,
                        IrsaliyeID = null, // Otomatik oluşturulacak irsaliyenin ID'si daha sonra doldurulacak
                        IrsaliyeTuru = irsaliyeTuruStr,
                        ParaBirimi = existingFatura.DovizTuru ?? "TRY"
                    };
                    
                    yeniStokHareketleri.Add(stokHareket);
                }
                
                // Stok hareketlerini ekle
                _unitOfWork.EntityStokHareketRepository.AddRange(yeniStokHareketleri);
                
                // 5. Yeni cari hareket kaydı oluştur
                if (existingFatura.CariID.HasValue && existingFatura.CariID != Guid.Empty)
                {
                    decimal cariHareketTutari = existingFatura.GenelToplam ?? 0m;
                    
                    var cariHareket = new CariHareket
                    {
                        CariHareketID = Guid.NewGuid(),
                        CariID = existingFatura.CariID.Value,
                        Tarih = existingFatura.FaturaTarihi ?? DateTime.Now,
                        Tutar = cariHareketTutari,
                        HareketTuru = stokHareketTipi == StokHareketiTipi.Giris ? "Borç" : "Alacak",
                        Aciklama = $"{existingFatura.FaturaNumarasi} numaralı fatura (güncelleme)",
                        ReferansNo = existingFatura.FaturaNumarasi,
                        ReferansTuru = "Fatura",
                        ReferansID = existingFatura.FaturaID,
                        OlusturmaTarihi = DateTime.Now,
                        OlusturanKullaniciID = currentUserId,
                        Borc = stokHareketTipi == StokHareketiTipi.Giris ? cariHareketTutari : 0,
                        Alacak = stokHareketTipi == StokHareketiTipi.Giris ? 0 : cariHareketTutari
                    };
                    
                    _unitOfWork.EntityCariHareketRepository.Add(cariHareket);
                }
                
                // 6. Değişiklikleri kaydet
                await _unitOfWork.CompleteAsync();
                
                // 7. FIFO kayıtlarını oluştur
                if (viewModel.FaturaKalemleri != null && viewModel.FaturaKalemleri.Any())
                {
                    var fifoStokHareketTipi = stokHareketTipi;
                    
                    foreach (var kalem in viewModel.FaturaKalemleri.Where(k => k.UrunID != Guid.Empty))
                    {
                        var miktar = kalem.Miktar;
                        var birimFiyat = kalem.BirimFiyat;
                        var birim = kalem.Birim;
                        
                        if (fifoStokHareketTipi == StokHareketiTipi.Giris)
                        {
                            // Stok girişi için FIFO kaydı oluştur
                            await _stokFifoService.StokGirisiYap(
                                kalem.UrunID,
                                miktar,
                                birimFiyat,
                                birim,
                                existingFatura.FaturaNumarasi,
                                "Fatura",
                                existingFatura.FaturaID,
                                $"{existingFatura.FaturaNumarasi} numaralı fatura ile stok girişi (güncelleme)",
                                existingFatura.DovizTuru,
                                existingFatura.DovizKuru);
                        }
                        else
                        {
                            // Stok çıkışı için FIFO kaydı kullan
                            await _stokFifoService.StokCikisiYap(
                                kalem.UrunID,
                                miktar,
                                existingFatura.FaturaNumarasi,
                                "Fatura",
                                existingFatura.FaturaID,
                                $"{existingFatura.FaturaNumarasi} numaralı fatura ile stok çıkışı (güncelleme)");
                        }
                    }
                    
                    _logger.LogInformation("Güncelleme için FIFO kayıtları başarıyla oluşturuldu");
                }
                
                // 8. Gerekiyorsa otomatik irsaliye oluştur
                bool otomatikIrsaliyeOlustur = await ShouldCreateIrsaliye(viewModel);
                if (otomatikIrsaliyeOlustur)
                {
                    // Mevcut irsaliye var mı kontrol et
                    var mevcutIrsaliye = await _unitOfWork.EntityIrsaliyeRepository.GetAll()
                        .FirstOrDefaultAsync(i => i.FaturaID == existingFatura.FaturaID && !i.Silindi);
                        
                    if (mevcutIrsaliye != null)
                    {
                        // Mevcut irsaliyeyi sil/iptal et
                        mevcutIrsaliye.Silindi = true;
                        mevcutIrsaliye.GuncellemeTarihi = DateTime.Now;
                        _unitOfWork.EntityIrsaliyeRepository.Update(mevcutIrsaliye);
                        
                        // İrsaliye detaylarını sil
                        var irsaliyeDetaylari = await _unitOfWork.EntityIrsaliyeDetayRepository.GetAll()
                            .Where(id => id.IrsaliyeID == mevcutIrsaliye.IrsaliyeID && !id.Silindi)
                            .ToListAsync();
                            
                        foreach (var detay in irsaliyeDetaylari)
                        {
                            detay.Silindi = true;
                            detay.GuncellemeTarihi = DateTime.Now;
                            _unitOfWork.EntityIrsaliyeDetayRepository.Update(detay);
                        }
                        
                        await _unitOfWork.CompleteAsync();
                    }
                    
                    // Yeni irsaliye oluştur
                    await _irsaliyeService.OtomatikIrsaliyeOlustur(existingFatura, depoID);
                    _logger.LogInformation($"Fatura güncellemesi için otomatik irsaliye oluşturuldu");
                }
                
                // Transaction'ı commit et
                await _unitOfWork.CommitTransactionAsync();
                
                _logger.LogInformation($"Fatura başarıyla güncellendi. FaturaID: {existingFatura.FaturaID}");
                
                return existingFatura.FaturaID;
            }
            catch (Exception ex)
            {
                // Hata durumunda rollback
                await _unitOfWork.RollbackTransactionAsync();
                
                if (ex is DbUpdateException dbEx)
                {
                    _logger.LogError(dbEx, $"Veritabanı güncelleme hatası: {dbEx.InnerException?.Message ?? dbEx.Message}");
                    throw new Exception($"Veritabanı işlemi sırasında bir hata oluştu: {dbEx.InnerException?.Message ?? dbEx.Message}", dbEx);
                }
                else if (ex is DbUpdateConcurrencyException concurrencyEx)
                {
                    _logger.LogError(concurrencyEx, "Veritabanı eşzamanlılık hatası. Başka bir kullanıcı aynı kaydı değiştirmiş olabilir.");
                    throw new Exception("Kayıt başka bir kullanıcı tarafından değiştirilmiş olabilir. Lütfen tekrar deneyin.", concurrencyEx);
                }
                
                _logger.LogError(ex, $"Fatura güncelleme işlemi sırasında hata: {ex.Message}");
                throw;
            }
        }
        
        /// <summary>
        /// ViewModel'dan depo ID'yi alır
        /// </summary>
        private async Task<Guid?> GetDepoIDFromViewModel(FaturaEditViewModel viewModel)
        {
            // FaturaEditViewModel'da depo ID yoksa, ilk aktif depoyu kullan
            
            // Direkt olarak model üzerinde depo ID'yi kontrol etmek için ek property yoksa ilk aktif depoyu döndür 
            var ilkDepo = await _unitOfWork.EntityDepolarRepository.GetAll()
                .Where(d => d.Aktif && !d.Silindi)
                .OrderBy(d => d.DepoAdi) // Sıra yerine DepoAdi'ye göre sırala
                .FirstOrDefaultAsync();
                
            return ilkDepo?.DepoID;
        }
        
        /// <summary>
        /// ViewModel'a göre otomatik irsaliye oluşturulup oluşturulmayacağını belirler
        /// </summary>
        private async Task<bool> ShouldCreateIrsaliye(FaturaEditViewModel viewModel)
        {
            if (viewModel.OtomatikIrsaliyeOlustur)
                return true;
                
            return false;
        }
    }
} 