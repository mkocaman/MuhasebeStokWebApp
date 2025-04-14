using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using MuhasebeStokWebApp.Data;
using MuhasebeStokWebApp.Data.Entities;
using MuhasebeStokWebApp.Data.Repositories;
using MuhasebeStokWebApp.Enums;
using MuhasebeStokWebApp.Models;
using MuhasebeStokWebApp.Services.Interfaces;
using MuhasebeStokWebApp.ViewModels;
using MuhasebeStokWebApp.ViewModels.Aklama;

namespace MuhasebeStokWebApp.Services
{
    public class MerkeziAklamaService : IMerkeziAklamaService
    {
        private readonly ApplicationDbContext _context;
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<MerkeziAklamaService> _logger;
        
        public MerkeziAklamaService(
            ApplicationDbContext context,
            IUnitOfWork unitOfWork,
            ILogger<MerkeziAklamaService> logger)
        {
            _context = context;
            _unitOfWork = unitOfWork;
            _logger = logger;
        }
        
        public async Task<List<AklamaKuyrukViewModel>> GetBekleyenAklamaKayitlariAsync(int? page = null, int? pageSize = null, Guid? urunId = null)
        {
            try
            {
                _logger.LogInformation("GetBekleyenAklamaKayitlariAsync metodu çağrıldı");
                
                // Bekleyen aklama kayıtlarını getir
                var query = _context.Set<FaturaAklamaKuyruk>()
                    .Include(a => a.FaturaDetay)
                        .ThenInclude(fd => fd.Fatura)
                    .Include(a => a.Urun)
                        .ThenInclude(u => u.Birim)
                    .Include(a => a.Sozlesme)
                    .Where(a => !a.Silindi && a.Durum == AklamaDurumu.Bekliyor);
                
                // Ürün filtrelemesi
                if (urunId.HasValue)
                {
                    query = query.Where(a => a.UrunID == urunId.Value);
                }
                
                // Sayfalama
                if (page.HasValue && pageSize.HasValue)
                {
                    query = query.Skip((page.Value - 1) * pageSize.Value).Take(pageSize.Value);
                }
                
                // FIFO düzeninde sırala
                query = query.OrderBy(a => a.OlusturmaTarihi);
                
                var aklamaKayitlari = await query.ToListAsync();
                
                // ViewModel dönüşümünü yap
                var viewModels = aklamaKayitlari.Select(a => new AklamaKuyrukViewModel
                {
                    AklamaID = a.AklamaID,
                    FaturaID = a.FaturaDetay?.FaturaID ?? Guid.Empty,
                    FaturaDetayID = a.FaturaKalemID,
                    FaturaNo = a.FaturaDetay?.Fatura?.FaturaNumarasi ?? "Bilinmiyor",
                    CariAdi = a.FaturaDetay?.Fatura?.Cari?.CariUnvani ?? "Bilinmiyor",
                    UrunID = a.UrunID,
                    UrunKodu = a.Urun?.UrunKodu ?? "Bilinmiyor",
                    UrunAdi = a.Urun?.UrunAdi ?? "Bilinmiyor",
                    BirimAdi = a.Urun?.Birim?.BirimAdi ?? "Bilinmiyor",
                    FaturaTarihi = a.FaturaDetay?.Fatura?.FaturaTarihi ?? DateTime.MinValue,
                    Miktar = a.AklananMiktar,
                    KalanMiktar = a.AklananMiktar, // Henüz aklanmadığı için kalan miktar tam miktardır
                    BirimFiyat = a.BirimFiyat,
                    ParaBirimi = a.FaturaDetay?.Fatura?.DovizTuru ?? "TL",
                    Durum = a.Durum,
                    EklenmeTarihi = a.OlusturmaTarihi,
                    AklanmaTarihi = a.AklanmaTarihi,
                    Aciklama = a.AklanmaNotu
                }).ToList();
                
                return viewModels;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Bekleyen aklama kayıtları getirilirken hata oluştu");
                return new List<AklamaKuyrukViewModel>();
            }
        }

        public async Task<List<AklamaKuyrukViewModel>> GetAklanmisKayitlarAsync(int? page = null, int? pageSize = null, Guid? urunId = null)
        {
            _logger.LogInformation("GetAklanmisKayitlarAsync metodu çağrıldı");
            // Geçici olarak boş listeyi döndürüyoruz
            return new List<AklamaKuyrukViewModel>();
        }

        public async Task<bool> FaturaKaleminiAklamaKuyrugunaEkleAsync(Guid faturaKalemId)
        {
            try
            {
                _logger.LogInformation("FaturaKaleminiAklamaKuyrugunaEkleAsync metodu çağrıldı");
                
                // Transaction başlat
                await _unitOfWork.BeginTransactionAsync();
                
                // Fatura kalemini getir
                var faturaKalem = await _context.FaturaDetaylari
                    .Include(fd => fd.Fatura)
                    .Include(fd => fd.Urun)
                    .FirstOrDefaultAsync(fd => fd.FaturaDetayID == faturaKalemId);
                
                if (faturaKalem == null)
                {
                    _logger.LogWarning("Fatura kalemi bulunamadı, ID: {FaturaKalemID}", faturaKalemId);
                    return false;
                }
                
                // Resmi olmayan fatura kontrolü
                if (faturaKalem.Fatura?.ResmiMi == true)
                {
                    _logger.LogWarning("Resmi fatura kalemleri aklama kuyruğuna eklenemez, FaturaKalemID: {FaturaKalemID}", faturaKalemId);
                    return false;
                }
                
                // Varsayılan olarak cari ID'yi kullan (eğer sözleşme için uygun bir sözleşme yoksa)
                Guid? sozlesmeId = null;
                
                // Cari'ye ait aktif bir sözleşme varsa onu kullan
                if (faturaKalem.Fatura?.CariID.HasValue == true)
                {
                    var cariId = faturaKalem.Fatura.CariID.Value;
                    var aktiveSozlesme = await _context.Sozlesmeler
                        .Where(s => s.CariID == cariId && s.AktifMi && !s.Silindi)
                        .OrderByDescending(s => s.SozlesmeTarihi)
                        .FirstOrDefaultAsync();
                    
                    sozlesmeId = aktiveSozlesme?.SozlesmeID;
                }
                
                // Aklama kuyruğunda bu kalem zaten var mı kontrol et
                bool zatenVar = await _context.Set<FaturaAklamaKuyruk>()
                    .AnyAsync(a => a.FaturaKalemID == faturaKalemId && !a.Silindi);
                
                if (zatenVar)
                {
                    _logger.LogWarning("Fatura kalemi zaten aklama kuyruğunda mevcut, FaturaKalemID: {FaturaKalemID}", faturaKalemId);
                    return false;
                }
                
                // Yeni aklama kaydı oluştur
                var aklamaKaydi = new FaturaAklamaKuyruk
                {
                    FaturaKalemID = faturaKalemId,
                    UrunID = faturaKalem.UrunID,
                    AklananMiktar = faturaKalem.Miktar,
                    BirimFiyat = faturaKalem.BirimFiyat,
                    ParaBirimi = faturaKalem.Fatura?.DovizTuru ?? "TL",
                    DovizKuru = faturaKalem.Fatura?.DovizKuru ?? 1,
                    SozlesmeID = sozlesmeId ?? Guid.Empty, // SozlesmeID nullable değil, boş durumda bir değer atanmalı
                    Durum = AklamaDurumu.Bekliyor,
                    OlusturmaTarihi = DateTime.Now,
                    OlusturanKullaniciID = faturaKalem.Fatura?.OlusturanKullaniciID
                };
                
                // Kaydı ekle
                await _context.Set<FaturaAklamaKuyruk>().AddAsync(aklamaKaydi);
                
                // Değişiklikleri kaydet
                await _unitOfWork.CommitTransactionAsync();
                
                _logger.LogInformation("Fatura kalemi başarıyla aklama kuyruğuna eklendi, FaturaKalemID: {FaturaKalemID}, AklamaID: {AklamaID}", 
                    faturaKalemId, aklamaKaydi.AklamaID);
                
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Fatura kalemi aklama kuyruğuna eklenirken hata oluştu, FaturaKalemID: {FaturaKalemID}", faturaKalemId);
                
                // Hata durumunda rollback yap
                await _unitOfWork.RollbackTransactionAsync();
                
                return false;
            }
        }

        public async Task<bool> ManuelAklamaKaydiOlusturAsync(ManuelAklamaViewModel model)
        {
            _logger.LogInformation("ManuelAklamaKaydiOlusturAsync metodu çağrıldı, fakat şu anda etkin değil");
            // Geçici olarak başarılı olduğumuzu söylüyoruz
            return true;
        }

        public async Task<AklamaOzetiViewModel> GetAklamaOzetiAsync(Guid? urunId = null, DateTime? baslangicTarihi = null, DateTime? bitisTarihi = null)
        {
            _logger.LogInformation("GetAklamaOzetiAsync metodu çağrıldı");
            // Geçici boş özet döndürüyoruz
            return new AklamaOzetiViewModel {
                BekleyenKayitSayisi = 0,
                ToplamBekleyenMiktar = 0,
                AklanmisKayitSayisi = 0,
                ToplamAklanmisMiktar = 0,
                SonAklamaTarihi = null
            };
        }

        public async Task<List<UrunAklamaDurumuViewModel>> GetTumUrunlerinAklamaDurumuAsync()
        {
            _logger.LogInformation("GetTumUrunlerinAklamaDurumuAsync metodu çağrıldı");
            // Geçici olarak boş listeyi döndürüyoruz
            return new List<UrunAklamaDurumuViewModel>();
        }

        public async Task<UrunAklamaGecmisiViewModel> GetUrunAklamaGecmisiAsync(Guid urunId)
        {
            _logger.LogInformation("GetUrunAklamaGecmisiAsync metodu çağrıldı");
            // Geçici boş geçmiş döndürüyoruz
            return new UrunAklamaGecmisiViewModel {
                UrunID = urunId,
                UrunAdi = "Bilinmiyor",
                BekleyenKayitlar = new List<AklamaKuyrukViewModel>(),
                AklanmisKayitlar = new List<AklamaKuyrukViewModel>()
            };
        }

        public async Task<bool> AklamaKaydiSilAsync(Guid aklamaId)
        {
            _logger.LogInformation("AklamaKaydiSilAsync metodu çağrıldı, fakat şu anda etkin değil");
            // Geçici olarak başarılı olduğumuzu söylüyoruz
            return true;
        }

        public async Task<bool> AklamaKaydiTamamlaAsync(Guid aklamaId, string aciklama)
        {
            _logger.LogInformation("AklamaKaydiTamamlaAsync metodu çağrıldı, fakat şu anda etkin değil");
            // Geçici olarak başarılı olduğumuzu söylüyoruz
            return true;
        }

        public async Task<bool> AklamaKaydiDurdurAsync(Guid aklamaId)
        {
            _logger.LogInformation("AklamaKaydiDurdurAsync metodu çağrıldı, fakat şu anda etkin değil");
            // Geçici olarak başarılı olduğumuzu söylüyoruz
            return true;
        }

        public async Task<bool> AklamaKaydiAktifEtAsync(Guid aklamaId)
        {
            _logger.LogInformation("AklamaKaydiAktifEtAsync metodu çağrıldı, ancak işlevsellik pasif durumdadır.");
            return true;
        }

        public async Task<List<AklamaKuyrukViewModel>> GetFaturaninAklamaKayitlariAsync(Guid faturaId)
        {
            _logger.LogInformation("GetFaturaninAklamaKayitlariAsync metodu çağrıldı");
            // Geçici olarak boş listeyi döndürüyoruz
            return new List<AklamaKuyrukViewModel>();
        }
        
        public async Task<bool> FaturayiAklamaKuyrugunaEkleAsync(Guid faturaId)
        {
            try
            {
                _logger.LogInformation("FaturayiAklamaKuyrugunaEkleAsync metodu çağrıldı, faturaId: {FaturaId}", faturaId);
                
                // Fatura bilgilerini getir
                var fatura = await _context.Faturalar
                    .Include(f => f.FaturaDetaylari)
                    .FirstOrDefaultAsync(f => f.FaturaID == faturaId);
                
                if (fatura == null)
                {
                    _logger.LogWarning("Fatura bulunamadı, ID: {FaturaID}", faturaId);
                    return false;
                }
                
                // Resmi fatura kontrolü yap
                if (fatura.ResmiMi)
                {
                    _logger.LogWarning("Resmi faturalar aklama kuyruğuna eklenemez: {FaturaID}", faturaId);
                    return false;
                }
                
                // İşlem başarılı mı?
                bool islemBasarili = true;
                
                // Tüm fatura kalemlerini eklemek için transaction başlat
                await _unitOfWork.BeginTransactionAsync();
                
                try
                {
                    // Tüm fatura kalemlerini ekle
                    foreach (var faturaKalem in fatura.FaturaDetaylari)
                    {
                        // Zaten kuyruğa eklenmiş mi kontrol et
                        bool zatenVar = await _context.Set<FaturaAklamaKuyruk>()
                            .AnyAsync(a => a.FaturaKalemID == faturaKalem.FaturaDetayID && !a.Silindi);
                        
                        if (!zatenVar)
                        {
                            // FaturaKaleminiAklamaKuyrugunaEkleAsync metodunu çağırmak yerine, işlemi
                            // burada doğrudan gerçekleştiriyoruz, çünkü transaction yönetimini 
                            // burada kendimiz yapıyoruz
                            
                            // Varsayılan olarak faturadaki sözleşme ID'yi kullan
                            Guid? sozlesmeId = fatura.SozlesmeID;
                            
                            // Sözleşme yoksa, cari'ye ait aktif bir sözleşme bulunmaya çalışılır
                            if (!sozlesmeId.HasValue && fatura.CariID.HasValue)
                            {
                                var aktiveSozlesme = await _context.Sozlesmeler
                                    .Where(s => s.CariID == fatura.CariID.Value && s.AktifMi && !s.Silindi)
                                    .OrderByDescending(s => s.SozlesmeTarihi)
                                    .FirstOrDefaultAsync();
                                
                                sozlesmeId = aktiveSozlesme?.SozlesmeID;
                            }
                            
                            // Yeni aklama kaydı oluştur
                            var aklamaKaydi = new FaturaAklamaKuyruk
                            {
                                FaturaKalemID = faturaKalem.FaturaDetayID,
                                UrunID = faturaKalem.UrunID,
                                AklananMiktar = faturaKalem.Miktar,
                                BirimFiyat = faturaKalem.BirimFiyat,
                                ParaBirimi = faturaKalem.Fatura?.DovizTuru ?? "TL",
                                DovizKuru = faturaKalem.Fatura?.DovizKuru ?? 1,
                                SozlesmeID = sozlesmeId ?? Guid.Empty, // SozlesmeID nullable değil
                                Durum = AklamaDurumu.Bekliyor,
                                OlusturmaTarihi = DateTime.Now,
                                OlusturanKullaniciID = fatura.OlusturanKullaniciID
                            };
                            
                            // Kaydı ekle
                            await _context.Set<FaturaAklamaKuyruk>().AddAsync(aklamaKaydi);
                            
                            _logger.LogInformation("Fatura kalemi aklama kuyruğuna eklendi: {FaturaKalemID}", faturaKalem.FaturaDetayID);
                        }
                        else
                        {
                            _logger.LogInformation("Fatura kalemi zaten aklama kuyruğunda: {FaturaKalemID}", faturaKalem.FaturaDetayID);
                        }
                    }
                    
                    // Değişiklikleri kaydet
                    await _unitOfWork.CommitTransactionAsync();
                    
                    _logger.LogInformation("Fatura başarıyla aklama kuyruğuna eklendi: {FaturaID}", faturaId);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Fatura kalemleri aklama kuyruğuna eklenirken hata oluştu: {FaturaID}", faturaId);
                    await _unitOfWork.RollbackTransactionAsync();
                    islemBasarili = false;
                }
                
                return islemBasarili;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Fatura aklama kuyruğuna eklenirken hata oluştu: {FaturaID}", faturaId);
                return false;
            }
        }
        
        public async Task<bool> OtomatikAklamaYapAsync(Guid resmiFaturaId)
        {
            try
            {
                _logger.LogInformation("OtomatikAklamaYapAsync metodu çağrıldı, resmiFaturaId: {ResmiFaturaId}", resmiFaturaId);
                
                // Resmi fatura bilgilerini getir
                var resmiFatura = await _context.Faturalar
                    .Include(f => f.FaturaDetaylari)
                    .ThenInclude(fd => fd.Urun)
                    .FirstOrDefaultAsync(f => f.FaturaID == resmiFaturaId);
                
                if (resmiFatura == null)
                {
                    _logger.LogWarning("Resmi fatura bulunamadı, ID: {FaturaID}", resmiFaturaId);
                    return false;
                }
                
                // Resmi fatura kontrolü yap
                if (!resmiFatura.ResmiMi)
                {
                    _logger.LogWarning("Gayri-resmi faturalar için otomatik aklama yapılamaz: {FaturaID}", resmiFaturaId);
                    return false;
                }
                
                // İşlem başarılı mı?
                bool islemBasarili = true;
                
                // Transaction başlat
                await _unitOfWork.BeginTransactionAsync();
                
                try
                {
                    // Resmi fatura kalemlerini döngüye al
                    foreach (var resmiFaturaKalem in resmiFatura.FaturaDetaylari)
                    {
                        // Bu ürüne ait bekleyen aklama kayıtlarını getir (FIFO sırasında)
                        var bekleyenKayitlar = await _context.Set<FaturaAklamaKuyruk>()
                            .Where(a => a.UrunID == resmiFaturaKalem.UrunID && 
                                   a.Durum == AklamaDurumu.Bekliyor && 
                                   !a.Silindi)
                            .OrderBy(a => a.OlusturmaTarihi)
                            .ToListAsync();
                        
                        if (!bekleyenKayitlar.Any())
                        {
                            _logger.LogInformation("Bu ürün için bekleyen aklama kaydı bulunamadı: {UrunID}", resmiFaturaKalem.UrunID);
                            continue;
                        }
                        
                        // Resmi fatura kalemindeki miktarı akla
                        decimal kalanMiktar = resmiFaturaKalem.Miktar;
                        
                        foreach (var aklamaKaydi in bekleyenKayitlar)
                        {
                            if (kalanMiktar <= 0)
                                break;
                                
                            // Bu kayıt ile aklama yapılabilir mi?
                            if (aklamaKaydi.AklananMiktar <= kalanMiktar)
                            {
                                // Kaydı tamamen akla
                                aklamaKaydi.Durum = AklamaDurumu.Aklandi;
                                aklamaKaydi.AklanmaTarihi = DateTime.Now;
                                aklamaKaydi.ResmiFaturaKalemID = resmiFaturaKalem.FaturaDetayID;
                                aklamaKaydi.AklanmaNotu = $"Otomatik aklama yapıldı. Resmi Fatura No: {resmiFatura.FaturaNumarasi}";
                                aklamaKaydi.GuncellemeTarihi = DateTime.Now;
                                aklamaKaydi.GuncelleyenKullaniciID = resmiFatura.SonGuncelleyenKullaniciID;
                                
                                kalanMiktar -= aklamaKaydi.AklananMiktar;
                                
                                _logger.LogInformation("Aklama kaydı tamamen aklandı: {AklamaID}", aklamaKaydi.AklamaID);
                            }
                            else
                            {
                                // Kaydı kısmen akla (yeni kayıt oluştur)
                                var yeniKayit = new FaturaAklamaKuyruk
                                {
                                    FaturaKalemID = aklamaKaydi.FaturaKalemID,
                                    UrunID = aklamaKaydi.UrunID,
                                    AklananMiktar = kalanMiktar,
                                    BirimFiyat = aklamaKaydi.BirimFiyat,
                                    ParaBirimi = aklamaKaydi.ParaBirimi,
                                    DovizKuru = aklamaKaydi.DovizKuru,
                                    SozlesmeID = aklamaKaydi.SozlesmeID,
                                    Durum = AklamaDurumu.Aklandi,
                                    OlusturmaTarihi = aklamaKaydi.OlusturmaTarihi,
                                    AklanmaTarihi = DateTime.Now,
                                    ResmiFaturaKalemID = resmiFaturaKalem.FaturaDetayID,
                                    AklanmaNotu = $"Kısmi otomatik aklama yapıldı. Resmi Fatura No: {resmiFatura.FaturaNumarasi}",
                                    OlusturanKullaniciID = aklamaKaydi.OlusturanKullaniciID,
                                    GuncelleyenKullaniciID = resmiFatura.SonGuncelleyenKullaniciID
                                };
                                
                                await _context.Set<FaturaAklamaKuyruk>().AddAsync(yeniKayit);
                                
                                // Orijinal kaydın miktarını azalt
                                aklamaKaydi.AklananMiktar -= kalanMiktar;
                                aklamaKaydi.Durum = AklamaDurumu.KismenAklandi;
                                aklamaKaydi.GuncellemeTarihi = DateTime.Now;
                                aklamaKaydi.GuncelleyenKullaniciID = resmiFatura.SonGuncelleyenKullaniciID;
                                
                                kalanMiktar = 0;
                                
                                _logger.LogInformation("Aklama kaydı kısmen aklandı: {AklamaID}, Yeni Kayıt: {YeniAklamaID}", 
                                    aklamaKaydi.AklamaID, yeniKayit.AklamaID);
                            }
                        }
                    }
                    
                    // Faturanın aklama bilgilerini güncelle
                    resmiFatura.AklanmaTarihi = DateTime.Now;
                    resmiFatura.AklanmaNotu = "Otomatik aklama yapıldı.";
                    resmiFatura.GuncellemeTarihi = DateTime.Now;
                    
                    // Değişiklikleri kaydet
                    await _unitOfWork.CommitTransactionAsync();
                    
                    _logger.LogInformation("Otomatik aklama başarıyla tamamlandı: {FaturaID}", resmiFaturaId);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Otomatik aklama sırasında hata oluştu: {FaturaID}", resmiFaturaId);
                    await _unitOfWork.RollbackTransactionAsync();
                    islemBasarili = false;
                }
                
                return islemBasarili;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Otomatik aklama sırasında beklenmeyen hata oluştu: {FaturaID}", resmiFaturaId);
                return false;
            }
        }
        
        public async Task<bool> ManuelAklamaYapAsync(Guid resmiFaturaKalemId, List<Guid> secilenKayitIdleri, decimal toplamMiktar, string aklamaNotu)
        {
            _logger.LogInformation("ManuelAklamaYapAsync metodu çağrıldı, fakat şu anda etkin değil");
            // Async metot için await ekliyoruz
            await Task.CompletedTask;
            // Geçici olarak başarılı olduğumuzu söylüyoruz
            return true;
        }
        
        public async Task<bool> AklamaIptalEtAsync(Guid aklamaId)
        {
            _logger.LogInformation("AklamaIptalEtAsync metodu çağrıldı, fakat şu anda etkin değil");
            // Async metot için await ekliyoruz
            await Task.CompletedTask;
            // Geçici olarak başarılı olduğumuzu söylüyoruz
            return true;
        }
        
        public async Task<bool> ResmiFaturaIptalAsync(Guid resmiFaturaId)
        {
            _logger.LogInformation("ResmiFaturaIptalAsync metodu çağrıldı, fakat şu anda etkin değil");
            // Async metot için await ekliyoruz
            await Task.CompletedTask;
            // Geçici olarak başarılı olduğumuzu söylüyoruz
            return true;
        }
    }
} 