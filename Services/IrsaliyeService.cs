using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using MuhasebeStokWebApp.Data;
using MuhasebeStokWebApp.Data.Entities;
using MuhasebeStokWebApp.Data.Repositories;
using MuhasebeStokWebApp.Services.Interfaces;

namespace MuhasebeStokWebApp.Services
{
    public class IrsaliyeService : IIrsaliyeService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<IrsaliyeService> _logger;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IExceptionHandlingService _exceptionHandler;

        public IrsaliyeService(
            IUnitOfWork unitOfWork,
            ILogger<IrsaliyeService> logger,
            UserManager<ApplicationUser> userManager,
            IHttpContextAccessor httpContextAccessor,
            IExceptionHandlingService exceptionHandler)
        {
            _unitOfWork = unitOfWork;
            _logger = logger;
            _userManager = userManager;
            _httpContextAccessor = httpContextAccessor;
            _exceptionHandler = exceptionHandler;
        }

        /// <summary>
        /// Faturadan otomatik irsaliye oluşturur
        /// </summary>
        public async Task<Guid> OtomatikIrsaliyeOlustur(Fatura fatura, Guid? depoID = null)
        {
            return await _exceptionHandler.HandleExceptionAsync(async () =>
            {
                // Faturayı detaylarıyla birlikte tekrar çek
                var faturaWithDetails = await _unitOfWork.EntityFaturaRepository.GetByIdWithIncludesAsync(fatura.FaturaID);

                if (faturaWithDetails != null && faturaWithDetails.FaturaDetaylari != null && faturaWithDetails.FaturaDetaylari.Any())
                {
                    // Fatura türüne göre irsaliye türünü belirle
                    string irsaliyeTuru;
                    if (faturaWithDetails.FaturaTuru == null)
                    {
                        _logger.LogWarning($"Fatura türü bulunamadı. Varsayılan olarak 'Çıkış İrsaliyesi' kullanılıyor. FaturaID: {fatura.FaturaID}");
                        irsaliyeTuru = "Çıkış İrsaliyesi"; // Varsayılan değer
                    }
                    else
                    {
                        irsaliyeTuru = faturaWithDetails.FaturaTuru.HareketTuru == "Giriş" 
                            ? "Giriş İrsaliyesi" 
                            : "Çıkış İrsaliyesi";
                    }

                    // Yeni irsaliye numarası oluştur
                    var irsaliyeNumarasi = GenerateIrsaliyeNumarasi();

                    // CariID kontrol et ve geçerli bir ID olduğundan emin ol
                    var cariID = faturaWithDetails.CariID ?? Guid.Empty;
                    if (cariID == Guid.Empty)
                    {
                        _logger.LogWarning($"Faturada geçerli bir cari bulunamadı. FaturaID: {fatura.FaturaID}");
                    }

                    // Mevcut kullanıcının ID'sini al
                    var currentUserId = GetCurrentUserId();

                    await _unitOfWork.BeginTransactionAsync();
                    try
                    {
                        // Otomatik irsaliye oluştur
                        var irsaliye = new Irsaliye
                        {
                            IrsaliyeID = Guid.NewGuid(),
                            IrsaliyeNumarasi = irsaliyeNumarasi,
                            IrsaliyeTarihi = faturaWithDetails.FaturaTarihi ?? DateTime.Now,
                            CariID = cariID,
                            FaturaID = faturaWithDetails.FaturaID,
                            DepoID = depoID,
                            // Fatura türüne göre irsaliye türünü belirle (1: Satış -> Çıkış, 2: Alış -> Giriş)
                            IrsaliyeTuru = faturaWithDetails.FaturaTuruID == 1 ? "Çıkış" : "Giriş",
                            Aciklama = $"{faturaWithDetails.FaturaNumarasi ?? ""} numaralı faturaya ait otomatik oluşturulan {irsaliyeTuru} irsaliyesi",
                            OlusturmaTarihi = DateTime.Now,
                            OlusturanKullaniciId = currentUserId,
                            Aktif = true,
                            Silindi = false,
                            Durum = "Açık" // Durumu açık olarak ayarla
                        };

                        await _unitOfWork.IrsaliyeRepository.AddAsync(irsaliye);

                        // Fatura kalemlerini irsaliye kalemlerine dönüştür
                        if (faturaWithDetails.FaturaDetaylari != null && faturaWithDetails.FaturaDetaylari.Any())
                        {
                            foreach (var detay in faturaWithDetails.FaturaDetaylari)
                            {
                                if (detay.Urun != null)
                                {
                                    var irsaliyeDetay = new IrsaliyeDetay
                                    {
                                        IrsaliyeDetayID = Guid.NewGuid(),
                                        IrsaliyeID = irsaliye.IrsaliyeID,
                                        UrunID = detay.UrunID,
                                        DepoID = depoID,
                                        Miktar = detay.Miktar,
                                        BirimFiyat = detay.BirimFiyat,
                                        KdvOrani = detay.KdvOrani,
                                        IndirimOrani = detay.IndirimOrani,
                                        Birim = detay.Birim ?? "Adet",
                                        Aciklama = detay.Aciklama ?? "",
                                        OlusturmaTarihi = DateTime.Now,
                                        SatirToplam = detay.SatirToplam ?? 0m,
                                        SatirKdvToplam = detay.SatirKdvToplam ?? 0m,
                                        Aktif = true,
                                        Silindi = false
                                    };

                                    await _unitOfWork.IrsaliyeDetayRepository.AddAsync(irsaliyeDetay);
                                }
                            }
                        }

                        // İrsaliye ve detay kayıtlarını kaydet
                        await _unitOfWork.SaveChangesAsync();
                        _logger.LogInformation($"Fatura için otomatik irsaliye oluşturuldu: IrsaliyeNo={irsaliyeNumarasi}, FaturaNo={faturaWithDetails.FaturaNumarasi}");
                        
                        // İrsaliye oluşturulduktan sonra, faturaya bağlı stok hareketlerinin IrsaliyeID alanını güncelle
                        var stokHareketleri = await _unitOfWork.StokHareketRepository.GetAllAsync(
                            sh => sh.FaturaID == faturaWithDetails.FaturaID && !sh.Silindi);
                            
                        if (stokHareketleri.Any())
                        {
                            foreach (var hareket in stokHareketleri)
                            {
                                hareket.IrsaliyeID = irsaliye.IrsaliyeID;
                                hareket.IrsaliyeTuru = irsaliyeTuru;
                                await _unitOfWork.StokHareketRepository.UpdateAsync(hareket);
                            }
                            
                            await _unitOfWork.SaveChangesAsync();
                            _logger.LogInformation($"{stokHareketleri.Count()} adet stok hareketinin IrsaliyeID alanı güncellendi. IrsaliyeID: {irsaliye.IrsaliyeID}");
                        }
                        
                        await _unitOfWork.CommitTransactionAsync();
                        return irsaliye.IrsaliyeID;
                    }
                    catch 
                    {
                        await _unitOfWork.RollbackTransactionAsync();
                        throw;
                    }
                }
                else
                {
                    _logger.LogWarning($"İrsaliye oluşturmak için fatura detayları bulunamadı. FaturaID: {fatura.FaturaID}");
                    throw new Exception($"İrsaliye oluşturmak için fatura veya detayları bulunamadı. FaturaID: {fatura.FaturaID}");
                }
            }, "OtomatikIrsaliyeOlustur", fatura.FaturaID, depoID);
        }

        /// <summary>
        /// Fatura ID'sinden otomatik irsaliye oluşturur
        /// </summary>
        public async Task<Guid> OtomatikIrsaliyeOlusturFromID(Guid faturaID, Guid? depoID = null)
        {
            return await _exceptionHandler.HandleExceptionAsync(async () =>
            {
                // Faturayı bul
                var fatura = await _unitOfWork.FaturaRepository.GetByIdAsync(faturaID);
                if (fatura == null)
                {
                    throw new Exception($"Fatura bulunamadı. FaturaID: {faturaID}");
                }

                // Faturadan irsaliye oluştur
                return await OtomatikIrsaliyeOlustur(fatura, depoID);
            }, "OtomatikIrsaliyeOlusturFromID", faturaID, depoID);
        }

        /// <summary>
        /// Yeni irsaliye numarası oluşturur (örn. IRS-2023-0001)
        /// </summary>
        public string GenerateIrsaliyeNumarasi()
        {
            // Son irsaliye numarasını bul
            string prefix = $"IRS-{DateTime.Now.Year}-";
            var numara = 1;

            try
            {
                var sonIrsaliye = _unitOfWork.IrsaliyeRepository.GetAll()
                    .Where(i => i.IrsaliyeNumarasi != null && i.IrsaliyeNumarasi.StartsWith(prefix))
                    .OrderByDescending(i => i.IrsaliyeNumarasi)
                    .FirstOrDefault();

                if (sonIrsaliye != null && int.TryParse(sonIrsaliye.IrsaliyeNumarasi.Substring(prefix.Length), out int sonNumara))
                {
                    numara = sonNumara + 1;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "İrsaliye numarası oluşturulurken hata oluştu. Varsayılan değer kullanılıyor.");
            }

            return $"{prefix}{numara:D4}";
        }

        /// <summary>
        /// Mevcut kullanıcının ID'sini döndürür
        /// </summary>
        private Guid GetCurrentUserId()
        {
            try
            {
                var userId = _userManager.GetUserId(_httpContextAccessor.HttpContext.User);
                if (Guid.TryParse(userId, out Guid userGuid))
                {
                    return userGuid;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Kullanıcı ID'si alınırken hata oluştu.");
            }
            
            return Guid.Empty;
        }

        /// <summary>
        /// İrsaliye detaylarını günceller
        /// </summary>
        public async Task<bool> UpdateIrsaliyeDetaylarAsync(Guid irsaliyeID, List<MuhasebeStokWebApp.ViewModels.Irsaliye.IrsaliyeDetayViewModel> detaylar)
        {
            return await _exceptionHandler.HandleExceptionAsync(async () => 
            {
                // İrsaliyeyi kontrol et
                var irsaliye = await _unitOfWork.IrsaliyeRepository.GetByIdAsync(irsaliyeID);
                if (irsaliye == null)
                {
                    _logger.LogWarning($"İrsaliye detayları güncellenemedi: İrsaliye bulunamadı. IrsaliyeID: {irsaliyeID}");
                    return false;
                }

                await _unitOfWork.BeginTransactionAsync();
                try
                {
                    // Mevcut detayları getir
                    var mevcutDetaylar = await _unitOfWork.IrsaliyeDetayRepository.GetAllAsync(
                        d => d.IrsaliyeID == irsaliyeID && !d.Silindi);
                    
                    // Silinecek detayları işaretle
                    foreach (var mevcutDetay in mevcutDetaylar)
                    {
                        var guncellenecekDetay = detaylar.FirstOrDefault(d => d.IrsaliyeDetayID == mevcutDetay.IrsaliyeDetayID);
                        
                        if (guncellenecekDetay == null)
                        {
                            // Detay listede yoksa sil
                            mevcutDetay.Silindi = true;
                            mevcutDetay.GuncellemeTarihi = DateTime.Now;
                            await _unitOfWork.IrsaliyeDetayRepository.UpdateAsync(mevcutDetay);
                        }
                    }
                    
                    // Detayları ekle veya güncelle
                    foreach (var detay in detaylar)
                    {
                        if (detay.IrsaliyeDetayID == Guid.Empty)
                        {
                            // Yeni detay ekle
                            var yeniDetay = new IrsaliyeDetay
                            {
                                IrsaliyeDetayID = Guid.NewGuid(),
                                IrsaliyeID = irsaliyeID,
                                UrunID = detay.UrunID,
                                DepoID = detay.DepoID,
                                Miktar = detay.Miktar,
                                BirimFiyat = detay.BirimFiyat,
                                KdvOrani = detay.KdvOrani,
                                IndirimOrani = detay.IndirimOrani,
                                Birim = detay.Birim,
                                Aciklama = detay.Aciklama,
                                OlusturmaTarihi = DateTime.Now,
                                SatirToplam = detay.SatirToplam,
                                SatirKdvToplam = detay.SatirKdvToplam,
                                Aktif = true,
                                Silindi = false
                            };
                            
                            await _unitOfWork.IrsaliyeDetayRepository.AddAsync(yeniDetay);
                        }
                        else
                        {
                            // Mevcut detayı güncelle
                            var mevcutDetay = mevcutDetaylar.FirstOrDefault(d => d.IrsaliyeDetayID == detay.IrsaliyeDetayID);
                            if (mevcutDetay != null)
                            {
                                mevcutDetay.UrunID = detay.UrunID;
                                mevcutDetay.DepoID = detay.DepoID;
                                mevcutDetay.Miktar = detay.Miktar;
                                mevcutDetay.BirimFiyat = detay.BirimFiyat;
                                mevcutDetay.KdvOrani = detay.KdvOrani;
                                mevcutDetay.IndirimOrani = detay.IndirimOrani;
                                mevcutDetay.Birim = detay.Birim;
                                mevcutDetay.Aciklama = detay.Aciklama;
                                mevcutDetay.GuncellemeTarihi = DateTime.Now;
                                mevcutDetay.SatirToplam = detay.SatirToplam;
                                mevcutDetay.SatirKdvToplam = detay.SatirKdvToplam;
                                mevcutDetay.Silindi = false;
                                
                                await _unitOfWork.IrsaliyeDetayRepository.UpdateAsync(mevcutDetay);
                            }
                        }
                    }
                    
                    await _unitOfWork.SaveChangesAsync();
                    await _unitOfWork.CommitTransactionAsync();
                    
                    _logger.LogInformation($"İrsaliye detayları başarıyla güncellendi. IrsaliyeID: {irsaliyeID}, Detay sayısı: {detaylar.Count}");
                    return true;
                }
                catch 
                {
                    await _unitOfWork.RollbackTransactionAsync();
                    throw;
                }
            }, "UpdateIrsaliyeDetaylarAsync", irsaliyeID);
        }

        /// <summary>
        /// İrsaliye detayı ekler
        /// </summary>
        public async Task<IrsaliyeDetay> AddIrsaliyeDetayAsync(IrsaliyeDetay irsaliyeDetay)
        {
            // Birim adını çek
            if (irsaliyeDetay.UrunID != Guid.Empty)
            {
                var urun = await _unitOfWork.Repository<Urun>().GetByIdAsync(irsaliyeDetay.UrunID);
                if (urun != null && urun.BirimID.HasValue)
                {
                    var birim = await _unitOfWork.Repository<Birim>().GetByIdAsync(urun.BirimID.Value);
                    if (birim != null)
                    {
                        irsaliyeDetay.Birim = birim.BirimAdi; // BirimID yerine BirimAdi'nı kullan
                    }
                }
            }

            await _unitOfWork.IrsaliyeDetayRepository.AddAsync(irsaliyeDetay);
            await _unitOfWork.SaveChangesAsync();
            return irsaliyeDetay;
        }

        /// <summary>
        /// İrsaliye detayı günceller
        /// </summary>
        public async Task<IrsaliyeDetay> UpdateIrsaliyeDetayAsync(IrsaliyeDetay irsaliyeDetay)
        {
            // Birim adını çek
            if (irsaliyeDetay.UrunID != Guid.Empty)
            {
                var urun = await _unitOfWork.Repository<Urun>().GetByIdAsync(irsaliyeDetay.UrunID);
                if (urun != null && urun.BirimID.HasValue)
                {
                    var birim = await _unitOfWork.Repository<Birim>().GetByIdAsync(urun.BirimID.Value);
                    if (birim != null)
                    {
                        irsaliyeDetay.Birim = birim.BirimAdi; // BirimID yerine BirimAdi'nı kullan
                    }
                }
            }

            _unitOfWork.IrsaliyeDetayRepository.Update(irsaliyeDetay);
            await _unitOfWork.SaveChangesAsync();
            return irsaliyeDetay;
        }
    }
} 