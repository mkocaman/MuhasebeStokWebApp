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
        private readonly ApplicationDbContext _context;
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<IrsaliyeService> _logger;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public IrsaliyeService(
            ApplicationDbContext context,
            IUnitOfWork unitOfWork,
            ILogger<IrsaliyeService> logger,
            UserManager<ApplicationUser> userManager,
            IHttpContextAccessor httpContextAccessor)
        {
            _context = context;
            _unitOfWork = unitOfWork;
            _logger = logger;
            _userManager = userManager;
            _httpContextAccessor = httpContextAccessor;
        }

        /// <summary>
        /// Faturadan otomatik irsaliye oluşturur
        /// </summary>
        public async Task<Guid> OtomatikIrsaliyeOlustur(Fatura fatura, Guid? depoID = null)
        {
            try
            {
                // Faturayı detaylarıyla birlikte tekrar çek - FaturaTuru ilişkisini ekle
                var faturaWithDetails = await _context.Faturalar
                    .Include(f => f.FaturaTuru)
                    .Include(f => f.FaturaDetaylari)
                        .ThenInclude(fd => fd.Urun)
                    .Include(f => f.Cari)
                    .FirstOrDefaultAsync(f => f.FaturaID == fatura.FaturaID);

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

                    // Yeni irsaliye oluştur
                    var irsaliye = new Irsaliye
                    {
                        IrsaliyeID = Guid.NewGuid(),
                        IrsaliyeNumarasi = irsaliyeNumarasi,
                        IrsaliyeTarihi = faturaWithDetails.FaturaTarihi ?? DateTime.Now,
                        CariID = cariID,
                        FaturaID = faturaWithDetails.FaturaID,
                        DepoID = depoID,
                        IrsaliyeTuru = irsaliyeTuru,
                        Aciklama = $"{faturaWithDetails.FaturaNumarasi ?? ""} numaralı faturaya ait otomatik oluşturulan {irsaliyeTuru} irsaliyesi",
                        OlusturmaTarihi = DateTime.Now,
                        OlusturanKullaniciId = currentUserId,
                        Aktif = true,
                        Silindi = false,
                        Durum = "Açık" // Durumu açık olarak ayarla
                    };

                    _context.Irsaliyeler.Add(irsaliye);

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

                                _context.IrsaliyeDetaylari.Add(irsaliyeDetay);
                            }
                        }
                    }

                    try
                    {
                        await _context.SaveChangesAsync();
                        _logger.LogInformation($"Fatura için otomatik irsaliye oluşturuldu: IrsaliyeNo={irsaliyeNumarasi}, FaturaNo={faturaWithDetails.FaturaNumarasi}");
                        
                        // İrsaliye oluşturulduktan sonra, faturaya bağlı stok hareketlerinin IrsaliyeID alanını güncelle
                        var stokHareketleri = await _context.StokHareketleri
                            .Where(sh => sh.FaturaID == faturaWithDetails.FaturaID && !sh.Silindi)
                            .ToListAsync();
                            
                        if (stokHareketleri != null && stokHareketleri.Any())
                        {
                            foreach (var hareket in stokHareketleri)
                            {
                                hareket.IrsaliyeID = irsaliye.IrsaliyeID;
                                hareket.IrsaliyeTuru = irsaliyeTuru;
                                _context.StokHareketleri.Update(hareket);
                            }
                            
                            try {
                                await _context.SaveChangesAsync();
                                _logger.LogInformation($"{stokHareketleri.Count} adet stok hareketinin IrsaliyeID alanı güncellendi. IrsaliyeID: {irsaliye.IrsaliyeID}");
                            }
                            catch (Exception stokEx) {
                                _logger.LogError(stokEx, $"Stok hareketleri güncellenirken hata oluştu: {stokEx.Message}");
                                // Sadece log tutup devam ediyoruz, irsaliye oluşturuldu
                            }
                        }
                        
                        return irsaliye.IrsaliyeID;
                    }
                    catch (Exception saveEx)
                    {
                        _logger.LogError(saveEx, $"İrsaliye detayları kaydedilirken hata oluştu: {saveEx.Message}");
                        throw;
                    }
                }
                else
                {
                    _logger.LogWarning($"İrsaliye oluşturmak için fatura detayları bulunamadı. FaturaID: {fatura.FaturaID}");
                    throw new Exception($"İrsaliye oluşturmak için fatura veya detayları bulunamadı. FaturaID: {fatura.FaturaID}");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Otomatik irsaliye oluşturulurken hata oluştu: {ex.Message}");
                throw;
            }
        }

        /// <summary>
        /// Fatura ID'sinden otomatik irsaliye oluşturur
        /// </summary>
        public async Task<Guid> OtomatikIrsaliyeOlusturFromID(Guid faturaID, Guid? depoID = null)
        {
            try
            {
                // Faturayı detayları ile birlikte yükle
                var faturaWithDetails = await _context.Faturalar
                    .Include(f => f.FaturaTuru)
                    .Include(f => f.FaturaDetaylari)
                        .ThenInclude(fd => fd.Urun)
                    .FirstOrDefaultAsync(f => f.FaturaID == faturaID);

                if (faturaWithDetails != null && faturaWithDetails.FaturaDetaylari != null && faturaWithDetails.FaturaDetaylari.Any())
                {
                    // İrsaliye türünü belirle
                    string irsaliyeTuru;
                    if (faturaWithDetails.FaturaTuru == null)
                    {
                        _logger.LogWarning($"Fatura türü bulunamadı. Varsayılan olarak 'Çıkış İrsaliyesi' kullanılıyor. FaturaID: {faturaID}");
                        irsaliyeTuru = "Çıkış İrsaliyesi"; // Varsayılan değer
                    }
                    else
                    {
                        irsaliyeTuru = faturaWithDetails.FaturaTuru.HareketTuru == "Giriş" 
                            ? "Giriş İrsaliyesi" 
                            : "Çıkış İrsaliyesi";
                    }
                    
                    // Otomatik irsaliye numarası oluştur
                    var irsaliyeNumarasi = GenerateIrsaliyeNumarasi();

                    // CariID kontrol et ve geçerli bir ID olduğundan emin ol
                    var cariID = faturaWithDetails.CariID ?? Guid.Empty;
                    if (cariID == Guid.Empty)
                    {
                        _logger.LogWarning($"Faturada geçerli bir cari bulunamadı. FaturaID: {faturaID}");
                    }

                    // Mevcut kullanıcının ID'sini al
                    var currentUserId = GetCurrentUserId();

                    // Yeni irsaliye oluştur
                    var irsaliye = new Irsaliye
                    {
                        IrsaliyeID = Guid.NewGuid(),
                        IrsaliyeNumarasi = irsaliyeNumarasi,
                        IrsaliyeTarihi = faturaWithDetails.FaturaTarihi ?? DateTime.Now,
                        CariID = cariID,
                        FaturaID = faturaWithDetails.FaturaID,
                        DepoID = depoID,
                        IrsaliyeTuru = irsaliyeTuru,
                        Aciklama = $"{faturaWithDetails.FaturaNumarasi ?? ""} numaralı faturaya ait otomatik oluşturulan {irsaliyeTuru} irsaliyesi",
                        OlusturmaTarihi = DateTime.Now,
                        OlusturanKullaniciId = currentUserId,
                        Aktif = true,
                        Silindi = false,
                        Durum = "Açık" // Durumu açık olarak ayarla
                    };

                    _context.Irsaliyeler.Add(irsaliye);

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

                                _context.IrsaliyeDetaylari.Add(irsaliyeDetay);
                            }
                        }
                    }

                    try
                    {
                        await _context.SaveChangesAsync();
                        _logger.LogInformation($"Fatura için otomatik irsaliye oluşturuldu: IrsaliyeNo={irsaliyeNumarasi}, FaturaNo={faturaWithDetails.FaturaNumarasi}");
                        
                        // İrsaliye oluşturulduktan sonra, faturaya bağlı stok hareketlerinin IrsaliyeID alanını güncelle
                        var stokHareketleri = await _context.StokHareketleri
                            .Where(sh => sh.FaturaID == faturaWithDetails.FaturaID && !sh.Silindi)
                            .ToListAsync();
                            
                        if (stokHareketleri != null && stokHareketleri.Any())
                        {
                            foreach (var hareket in stokHareketleri)
                            {
                                hareket.IrsaliyeID = irsaliye.IrsaliyeID;
                                hareket.IrsaliyeTuru = irsaliyeTuru;
                                _context.StokHareketleri.Update(hareket);
                            }
                            
                            try {
                                await _context.SaveChangesAsync();
                                _logger.LogInformation($"{stokHareketleri.Count} adet stok hareketinin IrsaliyeID alanı güncellendi. IrsaliyeID: {irsaliye.IrsaliyeID}");
                            }
                            catch (Exception stokEx) {
                                _logger.LogError(stokEx, $"Stok hareketleri güncellenirken hata oluştu: {stokEx.Message}");
                                // Sadece log tutup devam ediyoruz, irsaliye oluşturuldu
                            }
                        }
                        
                        return irsaliye.IrsaliyeID;
                    }
                    catch (Exception saveEx)
                    {
                        _logger.LogError(saveEx, $"İrsaliye detayları kaydedilirken hata oluştu: {saveEx.Message}");
                        throw;
                    }
                }
                else
                {
                    _logger.LogWarning($"İrsaliye oluşturmak için fatura detayları bulunamadı. FaturaID: {faturaID}");
                    throw new Exception($"İrsaliye oluşturmak için fatura veya detayları bulunamadı. FaturaID: {faturaID}");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Otomatik irsaliye oluşturulurken hata oluştu: {ex.Message}");
                throw;
            }
        }

        /// <summary>
        /// Yeni bir irsaliye numarası oluşturur
        /// </summary>
        public string GenerateIrsaliyeNumarasi()
        {
            var today = DateTime.Now;
            var year = today.Year.ToString().Substring(2);
            var month = today.Month.ToString().PadLeft(2, '0');
            var day = today.Day.ToString().PadLeft(2, '0');
            var prefix = $"IRS-{year}{month}{day}-";

            // Son irsaliye numarasını bulup arttır
            var lastIrsaliye = _context.Irsaliyeler
                .Where(i => i.IrsaliyeNumarasi != null && i.IrsaliyeNumarasi.StartsWith(prefix))
                .OrderByDescending(i => i.IrsaliyeNumarasi)
                .FirstOrDefault();

            int sequence = 1;
            if (lastIrsaliye != null && lastIrsaliye.IrsaliyeNumarasi != null)
            {
                var parts = lastIrsaliye.IrsaliyeNumarasi.Split('-');
                if (parts.Length == 3 && int.TryParse(parts[2], out int lastSeq))
                {
                    sequence = lastSeq + 1;
                }
            }

            return $"{prefix}{sequence.ToString().PadLeft(3, '0')}";
        }

        /// <summary>
        /// Mevcut kullanıcının ID'sini al
        /// </summary>
        private Guid GetCurrentUserId()
        {
            if (_httpContextAccessor.HttpContext == null || 
                _httpContextAccessor.HttpContext.User == null || 
                !_httpContextAccessor.HttpContext.User.Identity.IsAuthenticated)
            {
                return Guid.Empty;
            }

            var userIdClaim = _httpContextAccessor.HttpContext.User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier);
            if (userIdClaim != null && Guid.TryParse(userIdClaim.Value, out Guid userId))
            {
                return userId;
            }

            return Guid.Empty;
        }
    }
} 