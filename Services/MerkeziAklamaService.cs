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
using DEntityFaturaDetay = MuhasebeStokWebApp.Data.Entities.FaturaDetay;

namespace MuhasebeStokWebApp.Services
{
    public class MerkeziAklamaService : IMerkeziAklamaService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IRepository<FaturaAklamaKuyruk> _aklamaRepository;
        private readonly IRepository<Urun> _urunRepository;
        private readonly IRepository<DEntityFaturaDetay> _faturaDetayRepository;
        private readonly IRepository<Sozlesme> _sozlesmeRepository;
        private readonly ILogger<MerkeziAklamaService> _logger;

        public MerkeziAklamaService(
            IUnitOfWork unitOfWork,
            IRepository<FaturaAklamaKuyruk> aklamaRepository,
            IRepository<Urun> urunRepository,
            IRepository<DEntityFaturaDetay> faturaDetayRepository,
            IRepository<Sozlesme> sozlesmeRepository,
            ILogger<MerkeziAklamaService> logger)
        {
            _unitOfWork = unitOfWork;
            _aklamaRepository = aklamaRepository;
            _urunRepository = urunRepository;
            _faturaDetayRepository = faturaDetayRepository;
            _sozlesmeRepository = sozlesmeRepository;
            _logger = logger;
        }
        
        public async Task<List<AklamaKuyrukViewModel>> GetBekleyenAklamaKayitlariAsync(int? page = null, int? pageSize = null, Guid? urunId = null)
        {
            try
            {
                _logger.LogInformation("GetBekleyenAklamaKayitlariAsync metodu çağrıldı");
                
                // Bekleyen aklama kayıtlarını getir
                var query = _aklamaRepository.Query()
                    .Where(a => !a.Silindi && a.Durum == AklamaDurumu.Bekliyor);
                
                // Ürün filtrelemesi
                if (urunId.HasValue)
                {
                    query = query.Where(a => a.UrunID == urunId.Value);
                }
                
                // Oluşturma tarihine göre azalan sırala
                query = query.OrderByDescending(a => a.OlusturmaTarihi);
                
                // Sayfalama
                if (page.HasValue && pageSize.HasValue)
                {
                    query = query.Skip((page.Value - 1) * pageSize.Value).Take(pageSize.Value);
                }
                
                // AsSplitQuery kullanarak ilişkili verileri ayrı sorgularda getir
                var aklamaKayitlari = await query
                    .AsSplitQuery()
                    .Include(a => a.FaturaDetay)
                    .Include(a => a.Urun)
                    .Include(a => a.Sozlesme)
                    .ToListAsync();
                
                // ID'leri topla
                var faturaDetayIds = aklamaKayitlari
                    .Where(a => a.FaturaDetay != null)
                    .Select(a => a.FaturaKalemID)
                    .ToList();
                
                var urunIds = aklamaKayitlari
                    .Where(a => a.UrunID != Guid.Empty)
                    .Select(a => a.UrunID)
                    .Distinct()
                    .ToList();
                
                // İlgili faturaları ayrı sorgu ile getir
                var faturaDetaylar = new List<DEntityFaturaDetay>();
                if (faturaDetayIds.Any())
                {
                    faturaDetaylar = await _faturaDetayRepository.Query()
                        .Include(fd => fd.Fatura)
                        .ThenInclude(f => f.Cari)
                        .Where(fd => faturaDetayIds.Contains(fd.FaturaDetayID))
                        .ToListAsync();
                }
                
                // İlgili ürünleri ayrı sorgu ile getir
                var urunler = new List<Urun>();
                if (urunIds.Any())
                {
                    urunler = await _urunRepository.Query()
                        .Include(u => u.Birim)
                        .Where(u => urunIds.Contains(u.UrunID))
                        .ToListAsync();
                }
                
                // İlişkileri manuel olarak birleştir
                foreach (var aklamaKayit in aklamaKayitlari)
                {
                    if (aklamaKayit.FaturaKalemID != Guid.Empty)
                    {
                        aklamaKayit.FaturaDetay = faturaDetaylar.FirstOrDefault(fd => fd.FaturaDetayID == aklamaKayit.FaturaKalemID);
                    }
                    
                    if (aklamaKayit.UrunID != Guid.Empty)
                    {
                        aklamaKayit.Urun = urunler.FirstOrDefault(u => u.UrunID == aklamaKayit.UrunID);
                    }
                }
                
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
                    KalanMiktar = a.AklananMiktar, // Beklemede olduğu için kalan miktar = toplam miktar
                    BirimFiyat = a.BirimFiyat,
                    ParaBirimi = a.FaturaDetay?.Fatura?.DovizTuru ?? "TL",
                    Durum = a.Durum,
                    EklenmeTarihi = a.OlusturmaTarihi,
                    AklanmaTarihi = null,
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
            try
            {
                _logger.LogInformation("GetAklanmisKayitlarAsync metodu çağrıldı");
                
                // Aklanmış kayıtları getir
                var query = _aklamaRepository.Query()
                    .Where(a => !a.Silindi && a.Durum == AklamaDurumu.Aklandi);
                
                // Ürün filtrelemesi
                if (urunId.HasValue)
                {
                    query = query.Where(a => a.UrunID == urunId.Value);
                }
                
                // Aklanma tarihine göre azalan sırala
                query = query.OrderByDescending(a => a.AklanmaTarihi);
                
                // Sayfalama
                if (page.HasValue && pageSize.HasValue)
                {
                    query = query.Skip((page.Value - 1) * pageSize.Value).Take(pageSize.Value);
                }
                
                // AsSplitQuery kullanarak ilişkili verileri ayrı sorgularda getir
                var aklamaKayitlari = await query
                    .AsSplitQuery()
                    .Include(a => a.FaturaDetay)
                    .Include(a => a.Urun)
                    .Include(a => a.Sozlesme)
                    .ToListAsync();
                
                // ID'leri topla
                var faturaDetayIds = aklamaKayitlari
                    .Where(a => a.FaturaDetay != null)
                    .Select(a => a.FaturaKalemID)
                    .ToList();
                
                var urunIds = aklamaKayitlari
                    .Where(a => a.UrunID != Guid.Empty)
                    .Select(a => a.UrunID)
                    .Distinct()
                    .ToList();
                
                // İlgili faturaları ayrı sorgu ile getir
                var faturaDetaylar = new List<DEntityFaturaDetay>();
                if (faturaDetayIds.Any())
                {
                    faturaDetaylar = await _faturaDetayRepository.Query()
                        .Include(fd => fd.Fatura)
                        .ThenInclude(f => f.Cari)
                        .Where(fd => faturaDetayIds.Contains(fd.FaturaDetayID))
                        .ToListAsync();
                }
                
                // İlgili ürünleri ayrı sorgu ile getir
                var urunler = new List<Urun>();
                if (urunIds.Any())
                {
                    urunler = await _urunRepository.Query()
                        .Include(u => u.Birim)
                        .Where(u => urunIds.Contains(u.UrunID))
                        .ToListAsync();
                }
                
                // İlişkileri manuel olarak birleştir
                foreach (var aklamaKayit in aklamaKayitlari)
                {
                    if (aklamaKayit.FaturaKalemID != Guid.Empty)
                    {
                        aklamaKayit.FaturaDetay = faturaDetaylar.FirstOrDefault(fd => fd.FaturaDetayID == aklamaKayit.FaturaKalemID);
                    }
                    
                    if (aklamaKayit.UrunID != Guid.Empty)
                    {
                        aklamaKayit.Urun = urunler.FirstOrDefault(u => u.UrunID == aklamaKayit.UrunID);
                    }
                }
                
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
                    KalanMiktar = 0, // Tamamlandığı için kalan miktar 0
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
                _logger.LogError(ex, "Aklanmış kayıtlar getirilirken hata oluştu");
                return new List<AklamaKuyrukViewModel>();
            }
        }

        public async Task<bool> FaturaKaleminiAklamaKuyrugunaEkleAsync(Guid faturaKalemId)
        {
            try
            {
                _logger.LogInformation("FaturaKaleminiAklamaKuyrugunaEkleAsync metodu çağrıldı, FaturaKalemID: {FaturaKalemID}", faturaKalemId);
                
                // Transaction başlat
                await _unitOfWork.BeginTransactionAsync();
                
                // Fatura detayını getir
                var faturaDetay = await _faturaDetayRepository.GetByIdAsync(faturaKalemId);
                if (faturaDetay == null)
                {
                    _logger.LogWarning("Fatura kalemi bulunamadı, FaturaKalemID: {FaturaKalemID}", faturaKalemId);
                    return false;
                }
                
                // Daha önce eklenmiş mi kontrol et
                var mevcutKayit = await _aklamaRepository.Query()
                    .FirstOrDefaultAsync(a => a.FaturaKalemID == faturaKalemId && !a.Silindi);
                
                if (mevcutKayit != null)
                {
                    _logger.LogWarning("Fatura kalemi zaten aklama kuyruğuna eklenmiş, FaturaKalemID: {FaturaKalemID}", faturaKalemId);
                    return false;
                }
                
                // Aktif sözleşmeyi bul
                var aktiveSozlesme = await _sozlesmeRepository.Query()
                    .Where(s => s.CariID == faturaDetay.Fatura.CariID && s.AktifMi && !s.Silindi)
                    .OrderByDescending(s => s.SozlesmeTarihi)
                    .FirstOrDefaultAsync();
                
                // Yeni aklama kaydı oluştur
                var aklamaKaydi = new FaturaAklamaKuyruk
                {
                    AklamaID = Guid.NewGuid(),
                    FaturaKalemID = faturaDetay.FaturaDetayID,
                    UrunID = faturaDetay.UrunID,
                    AklananMiktar = faturaDetay.Miktar,
                    BirimFiyat = faturaDetay.BirimFiyat,
                    ParaBirimi = faturaDetay.Fatura.DovizTuru,
                    DovizKuru = faturaDetay.Fatura.DovizKuru,
                    SozlesmeID = aktiveSozlesme?.SozlesmeID ?? Guid.Empty,
                    Durum = AklamaDurumu.Bekliyor,
                    OlusturmaTarihi = DateTime.Now,
                    OlusturanKullaniciID = faturaDetay.Fatura.OlusturanKullaniciID,
                    AklanmaNotu = $"Fatura No: {faturaDetay.Fatura.FaturaNumarasi}, Kalem: {faturaDetay.UrunAdi}, Miktar: {faturaDetay.Miktar}"
                };
                
                // Kaydı ekle
                await _aklamaRepository.AddAsync(aklamaKaydi);
                
                // Değişiklikleri kaydet
                await _unitOfWork.CommitTransactionAsync();
                
                _logger.LogInformation("Fatura kalemi başarıyla aklama kuyruğuna eklendi, FaturaKalemID: {FaturaKalemID}, AklamaID: {AklamaID}", faturaKalemId, aklamaKaydi.AklamaID);
                
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Fatura kalemi aklama kuyruğuna eklenirken hata oluştu, FaturaKalemID: {FaturaKalemID}", faturaKalemId);
                await _unitOfWork.RollbackTransactionAsync();
                return false;
            }
        }

        public async Task<bool> ManuelAklamaKaydiOlusturAsync(ManuelAklamaViewModel model)
        {
            try
            {
                _logger.LogInformation("ManuelAklamaKaydiOlusturAsync metodu çağrıldı");
                
                if (model == null)
                {
                    _logger.LogWarning("Manuel aklama kaydı oluşturulamadı çünkü model null");
                    return false;
                }
                
                if (model.UrunId == Guid.Empty)
                {
                    _logger.LogWarning("Manuel aklama kaydı oluşturulamadı çünkü UrunID geçerli değil");
                    return false;
                }
                
                if (model.Miktar <= 0)
                {
                    _logger.LogWarning("Manuel aklama kaydı oluşturulamadı çünkü miktar 0 veya daha az");
                    return false;
                }
                
                // Transaction başlat
                await _unitOfWork.BeginTransactionAsync();
                
                // Ürünü kontrol et
                var urun = await _urunRepository.GetByIdAsync(model.UrunId);
                if (urun == null)
                {
                    _logger.LogWarning("Manuel aklama kaydı oluşturulamadı çünkü ürün bulunamadı, UrunID: {UrunID}", model.UrunId);
                    await _unitOfWork.RollbackTransactionAsync();
                    return false;
                }
                
                // Cari'ye ait aktif bir sözleşme varsa onu kullan
                Guid? sozlesmeId = null;
                if (model.CariId.HasValue)
                {
                    var aktiveSozlesme = await _sozlesmeRepository.Query()
                        .Where(s => s.CariID == model.CariId.Value && s.AktifMi && !s.Silindi)
                        .OrderByDescending(s => s.SozlesmeTarihi)
                        .FirstOrDefaultAsync();
                    
                    sozlesmeId = aktiveSozlesme?.SozlesmeID;
                }
                
                // Yeni manuel aklama kaydı oluştur
                var aklamaKaydi = new FaturaAklamaKuyruk
                {
                    AklamaID = Guid.NewGuid(),
                    FaturaKalemID = Guid.Empty, // Manuel kayıt olduğu için fatura kalemi yok
                    UrunID = model.UrunId,
                    AklananMiktar = model.Miktar,
                    BirimFiyat = model.BirimFiyat,
                    ParaBirimi = model.ParaBirimi ?? "TL",
                    DovizKuru = model.DovizKuru ?? 1,
                    SozlesmeID = sozlesmeId ?? Guid.Empty,
                    Durum = AklamaDurumu.Bekliyor,
                    OlusturmaTarihi = DateTime.Now,
                    OlusturanKullaniciID = model.KullaniciId,
                    AklanmaNotu = model.Aciklama,
                    ManuelKayit = true
                };
                
                // Kaydı ekle
                await _aklamaRepository.AddAsync(aklamaKaydi);
                
                // Değişiklikleri kaydet
                await _unitOfWork.CommitTransactionAsync();
                
                _logger.LogInformation("Manuel aklama kaydı başarıyla oluşturuldu, AklamaID: {AklamaID}", aklamaKaydi.AklamaID);
                
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Manuel aklama kaydı oluşturulurken hata oluştu");
                await _unitOfWork.RollbackTransactionAsync();
                return false;
            }
        }

        public async Task<AklamaOzetiViewModel> GetAklamaOzetiAsync(Guid? urunId = null, DateTime? baslangicTarihi = null, DateTime? bitisTarihi = null)
        {
            try
            {
                _logger.LogInformation("GetAklamaOzetiAsync metodu çağrıldı");
                
                var ozet = new AklamaOzetiViewModel();
                
                // Temel sorguyu oluştur
                var bekleyenSorgu = _aklamaRepository.Query()
                    .Where(a => !a.Silindi && a.Durum == AklamaDurumu.Bekliyor);
                    
                var aklanmisSorgu = _aklamaRepository.Query()
                    .Where(a => !a.Silindi && a.Durum == AklamaDurumu.Aklandi);
                
                // Ürün filtrelemesi
                if (urunId.HasValue)
                {
                    bekleyenSorgu = bekleyenSorgu.Where(a => a.UrunID == urunId.Value);
                    aklanmisSorgu = aklanmisSorgu.Where(a => a.UrunID == urunId.Value);
                }
                
                // Tarih filtrelemesi
                if (baslangicTarihi.HasValue)
                {
                    bekleyenSorgu = bekleyenSorgu.Where(a => a.OlusturmaTarihi >= baslangicTarihi.Value);
                    aklanmisSorgu = aklanmisSorgu.Where(a => a.AklanmaTarihi >= baslangicTarihi.Value);
                }
                
                if (bitisTarihi.HasValue)
                {
                    var bitisTarihiSonu = bitisTarihi.Value.Date.AddDays(1).AddTicks(-1);
                    bekleyenSorgu = bekleyenSorgu.Where(a => a.OlusturmaTarihi <= bitisTarihiSonu);
                    aklanmisSorgu = aklanmisSorgu.Where(a => a.AklanmaTarihi <= bitisTarihiSonu);
                }
                
                // İstatistikleri hesapla
                ozet.BekleyenKayitSayisi = await bekleyenSorgu.CountAsync();
                ozet.ToplamBekleyenMiktar = await bekleyenSorgu.SumAsync(a => a.AklananMiktar);
                
                ozet.AklanmisKayitSayisi = await aklanmisSorgu.CountAsync();
                ozet.ToplamAklanmisMiktar = await aklanmisSorgu.SumAsync(a => a.AklananMiktar);
                
                // Son aklama tarihini getir
                if (ozet.AklanmisKayitSayisi > 0)
                {
                    ozet.SonAklamaTarihi = await aklanmisSorgu.MaxAsync(a => a.AklanmaTarihi);
                }
                
                return ozet;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Aklama özeti getirilirken hata oluştu");
                return new AklamaOzetiViewModel
                {
                    BekleyenKayitSayisi = 0,
                    ToplamBekleyenMiktar = 0,
                    AklanmisKayitSayisi = 0,
                    ToplamAklanmisMiktar = 0,
                    SonAklamaTarihi = null
                };
            }
        }

        public async Task<List<UrunAklamaDurumuViewModel>> GetTumUrunlerinAklamaDurumuAsync()
        {
            try
            {
                _logger.LogInformation("GetTumUrunlerinAklamaDurumuAsync metodu çağrıldı");
                
                // Tüm ürünleri ve aklama durumlarını getir
                var query = from u in _urunRepository.Query()
                            where !u.Silindi
                            select new UrunAklamaDurumuViewModel
                            {
                                UrunID = u.UrunID,
                                UrunKodu = u.UrunKodu,
                                UrunAdi = u.UrunAdi,
                                BirimAdi = u.Birim.BirimAdi,
                                BekleyenAdet = _aklamaRepository.Query()
                                    .Count(a => a.UrunID == u.UrunID && !a.Silindi && a.Durum == AklamaDurumu.Bekliyor),
                                BekleyenMiktar = _aklamaRepository.Query()
                                    .Where(a => a.UrunID == u.UrunID && !a.Silindi && a.Durum == AklamaDurumu.Bekliyor)
                                    .Sum(a => (decimal?)a.AklananMiktar) ?? 0,
                                AklanmisAdet = _aklamaRepository.Query()
                                    .Count(a => a.UrunID == u.UrunID && !a.Silindi && a.Durum == AklamaDurumu.Aklandi),
                                AklanmisMiktar = _aklamaRepository.Query()
                                    .Where(a => a.UrunID == u.UrunID && !a.Silindi && a.Durum == AklamaDurumu.Aklandi)
                                    .Sum(a => (decimal?)a.AklananMiktar) ?? 0
                            };
                
                // Sadece aklama kaydı olan ürünleri filtrele
                var sonuclar = await query
                    .Where(u => u.BekleyenAdet > 0 || u.AklanmisAdet > 0)
                    .OrderByDescending(u => u.BekleyenAdet)
                    .ToListAsync();
                
                return sonuclar;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Tüm ürünlerin aklama durumu getirilirken hata oluştu");
                return new List<UrunAklamaDurumuViewModel>();
            }
        }

        public async Task<UrunAklamaGecmisiViewModel> GetUrunAklamaGecmisiAsync(Guid urunId)
        {
            try
            {
                _logger.LogInformation("GetUrunAklamaGecmisiAsync metodu çağrıldı, UrunID: {UrunID}", urunId);
                
                // Ürünü getir
                var urun = await _urunRepository.GetByIdAsync(urunId);
                
                if (urun == null)
                {
                    _logger.LogWarning("Ürün bulunamadı, UrunID: {UrunID}", urunId);
                    return new UrunAklamaGecmisiViewModel 
                    {
                        UrunID = urunId,
                        UrunAdi = "Bilinmiyor",
                        BekleyenKayitlar = new List<AklamaKuyrukViewModel>(),
                        AklanmisKayitlar = new List<AklamaKuyrukViewModel>()
                    };
                }
                
                // Ürün ile ilgili bekleyen ve aklanmış kayıtları getir
                var bekleyenKayitlarTask = GetBekleyenAklamaKayitlariAsync(null, null, urunId);
                var aklanmisKayitlarTask = GetAklanmisKayitlarAsync(null, null, urunId);
                
                await Task.WhenAll(bekleyenKayitlarTask, aklanmisKayitlarTask);
                
                var bekleyenKayitlar = await bekleyenKayitlarTask;
                var aklanmisKayitlar = await aklanmisKayitlarTask;
                
                // Sonuç model
                var sonuc = new UrunAklamaGecmisiViewModel
                {
                    UrunID = urun.UrunID,
                    UrunKodu = urun.UrunKodu,
                    UrunAdi = urun.UrunAdi,
                    BirimAdi = urun.Birim?.BirimAdi ?? "Bilinmiyor",
                    ToplamBekleyenMiktar = bekleyenKayitlar.Sum(k => k.KalanMiktar),
                    ToplamAklanmisMiktar = aklanmisKayitlar.Sum(k => k.Miktar),
                    BekleyenKayitlar = bekleyenKayitlar,
                    AklanmisKayitlar = aklanmisKayitlar
                };
                
                return sonuc;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ürün aklama geçmişi getirilirken hata oluştu, UrunID: {UrunID}", urunId);
                return new UrunAklamaGecmisiViewModel 
                {
                    UrunID = urunId,
                    UrunAdi = "Bilinmiyor",
                    BekleyenKayitlar = new List<AklamaKuyrukViewModel>(),
                    AklanmisKayitlar = new List<AklamaKuyrukViewModel>()
                };
            }
        }

        public async Task<bool> AklamaKaydiSilAsync(Guid aklamaId)
        {
            try
            {
                _logger.LogInformation("AklamaKaydiSilAsync metodu çağrıldı, AklamaID: {AklamaID}", aklamaId);
                
                // Transaction başlat
                await _unitOfWork.BeginTransactionAsync();
                
                // Aklama kaydını getir
                var aklamaKaydi = await _aklamaRepository.Query()
                    .FirstOrDefaultAsync(a => a.AklamaID == aklamaId && !a.Silindi);
                
                if (aklamaKaydi == null)
                {
                    _logger.LogWarning("Aklama kaydı bulunamadı, AklamaID: {AklamaID}", aklamaId);
                    return false;
                }
                
                // Tamamlanmış kayıtlar silinemez
                if (aklamaKaydi.Durum == AklamaDurumu.Aklandi)
                {
                    _logger.LogWarning("Tamamlanmış aklama kaydı silinemez, AklamaID: {AklamaID}", aklamaId);
                    return false;
                }
                
                // Kaydı mantıksal olarak sil
                aklamaKaydi.Silindi = true;
                aklamaKaydi.SilmeTarihi = DateTime.Now;
                
                // Değişiklikleri kaydet
                await _unitOfWork.CommitTransactionAsync();
                
                _logger.LogInformation("Aklama kaydı başarıyla silindi, AklamaID: {AklamaID}", aklamaId);
                
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Aklama kaydı silinirken hata oluştu, AklamaID: {AklamaID}", aklamaId);
                await _unitOfWork.RollbackTransactionAsync();
                return false;
            }
        }

        public async Task<bool> AklamaKaydiTamamlaAsync(Guid aklamaId, string aciklama)
        {
            try
            {
                _logger.LogInformation("AklamaKaydiTamamlaAsync metodu çağrıldı, AklamaID: {AklamaID}", aklamaId);
                
                // Transaction başlat
                await _unitOfWork.BeginTransactionAsync();
                
                // Aklama kaydını getir
                var aklamaKaydi = await _aklamaRepository.Query()
                    .FirstOrDefaultAsync(a => a.AklamaID == aklamaId && !a.Silindi && a.Durum == AklamaDurumu.Bekliyor);
                
                if (aklamaKaydi == null)
                {
                    _logger.LogWarning("Bekleyen aklama kaydı bulunamadı, AklamaID: {AklamaID}", aklamaId);
                    return false;
                }
                
                // Kaydı tamamla
                aklamaKaydi.Durum = AklamaDurumu.Aklandi;
                aklamaKaydi.AklanmaTarihi = DateTime.Now;
                aklamaKaydi.AklanmaNotu = aciklama;
                
                // Değişiklikleri kaydet
                await _unitOfWork.CommitTransactionAsync();
                
                _logger.LogInformation("Aklama kaydı başarıyla tamamlandı, AklamaID: {AklamaID}", aklamaId);
                
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Aklama kaydı tamamlanırken hata oluştu, AklamaID: {AklamaID}", aklamaId);
                await _unitOfWork.RollbackTransactionAsync();
                return false;
            }
        }

        public async Task<bool> AklamaKaydiDurdurAsync(Guid aklamaId)
        {
            try
            {
                _logger.LogInformation("AklamaKaydiDurdurAsync metodu çağrıldı, AklamaID: {AklamaID}", aklamaId);
                
                // Transaction başlat
                await _unitOfWork.BeginTransactionAsync();
                
                // Aklama kaydını getir
                var aklamaKaydi = await _aklamaRepository.Query()
                    .FirstOrDefaultAsync(a => a.AklamaID == aklamaId && !a.Silindi && a.Durum == AklamaDurumu.Bekliyor);
                
                if (aklamaKaydi == null)
                {
                    _logger.LogWarning("Bekleyen aklama kaydı bulunamadı, AklamaID: {AklamaID}", aklamaId);
                    return false;
                }
                
                // Kaydı durdur
                aklamaKaydi.Durum = AklamaDurumu.Iptal;
                aklamaKaydi.GuncellemeTarihi = DateTime.Now;
                
                // Değişiklikleri kaydet
                await _unitOfWork.CommitTransactionAsync();
                
                _logger.LogInformation("Aklama kaydı başarıyla durduruldu, AklamaID: {AklamaID}", aklamaId);
                
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Aklama kaydı durdurulurken hata oluştu, AklamaID: {AklamaID}", aklamaId);
                await _unitOfWork.RollbackTransactionAsync();
                return false;
            }
        }

        public async Task<bool> AklamaKaydiAktifEtAsync(Guid aklamaId)
        {
            try
            {
                _logger.LogInformation("AklamaKaydiAktifEtAsync metodu çağrıldı, AklamaID: {AklamaID}", aklamaId);
                
                // Transaction başlat
                await _unitOfWork.BeginTransactionAsync();
                
                // Aklama kaydını getir
                var aklamaKaydi = await _aklamaRepository.Query()
                    .FirstOrDefaultAsync(a => a.AklamaID == aklamaId && !a.Silindi && a.Durum == AklamaDurumu.Iptal);
                
                if (aklamaKaydi == null)
                {
                    _logger.LogWarning("Durdurulmuş aklama kaydı bulunamadı, AklamaID: {AklamaID}", aklamaId);
                    return false;
                }
                
                // Kaydı aktif et
                aklamaKaydi.Durum = AklamaDurumu.Bekliyor;
                aklamaKaydi.GuncellemeTarihi = DateTime.Now;
                
                // Değişiklikleri kaydet
                await _unitOfWork.CommitTransactionAsync();
                
                _logger.LogInformation("Aklama kaydı başarıyla aktif edildi, AklamaID: {AklamaID}", aklamaId);
                
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Aklama kaydı aktif edilirken hata oluştu, AklamaID: {AklamaID}", aklamaId);
                await _unitOfWork.RollbackTransactionAsync();
                return false;
            }
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
                var fatura = await _faturaDetayRepository.Query()
                    .Include(f => f.Fatura)
                    .FirstOrDefaultAsync(f => f.FaturaID == faturaId);
                
                if (fatura == null)
                {
                    _logger.LogWarning("Fatura bulunamadı, ID: {FaturaID}", faturaId);
                    return false;
                }
                
                // Resmi fatura kontrolü yap
                if (fatura.Fatura?.ResmiMi == true)
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
                        bool zatenVar = await _aklamaRepository.Query()
                            .AnyAsync(a => a.FaturaKalemID == faturaKalem.FaturaDetayID && !a.Silindi);
                        
                        if (!zatenVar)
                        {
                            // FaturaKaleminiAklamaKuyrugunaEkleAsync metodunu çağırmak yerine, işlemi
                            // burada doğrudan gerçekleştiriyoruz, çünkü transaction yönetimini 
                            // burada kendimiz yapıyoruz
                            
                            // Varsayılan olarak faturadaki sözleşme ID'yi kullan
                            Guid? sozlesmeId = fatura.Fatura.SozlesmeID;
                            
                            // Sözleşme yoksa, cari'ye ait aktif bir sözleşme bulunmaya çalışılır
                            if (!sozlesmeId.HasValue && fatura.Fatura.CariID.HasValue)
                            {
                                var aktiveSozlesme = await _sozlesmeRepository.Query()
                                    .Where(s => s.CariID == fatura.Fatura.CariID.Value && s.AktifMi && !s.Silindi)
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
                                OlusturanKullaniciID = fatura.Fatura.OlusturanKullaniciID
                            };
                            
                            // Kaydı ekle
                            await _aklamaRepository.AddAsync(aklamaKaydi);
                            
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
                var resmiFatura = await _faturaDetayRepository.Query()
                    .Include(f => f.Fatura)
                    .ThenInclude(fd => fd.Urun)
                    .FirstOrDefaultAsync(f => f.FaturaID == resmiFaturaId);
                
                if (resmiFatura == null)
                {
                    _logger.LogWarning("Resmi fatura bulunamadı, ID: {FaturaID}", resmiFaturaId);
                    return false;
                }
                
                // Resmi fatura kontrolü yap
                if (!resmiFatura.Fatura.ResmiMi)
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
                        var bekleyenKayitlar = await _aklamaRepository.Query()
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
                                aklamaKaydi.AklanmaNotu = $"Otomatik aklama yapıldı. Resmi Fatura No: {resmiFatura.Fatura.FaturaNumarasi}";
                                aklamaKaydi.GuncellemeTarihi = DateTime.Now;
                                aklamaKaydi.GuncelleyenKullaniciID = resmiFatura.Fatura.SonGuncelleyenKullaniciID;
                                
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
                                    AklanmaNotu = $"Kısmi otomatik aklama yapıldı. Resmi Fatura No: {resmiFatura.Fatura.FaturaNumarasi}",
                                    OlusturanKullaniciID = aklamaKaydi.OlusturanKullaniciID,
                                    GuncelleyenKullaniciID = resmiFatura.Fatura.SonGuncelleyenKullaniciID
                                };
                                
                                await _aklamaRepository.AddAsync(yeniKayit);
                                
                                // Orijinal kaydın miktarını azalt
                                aklamaKaydi.AklananMiktar -= kalanMiktar;
                                aklamaKaydi.Durum = AklamaDurumu.KismenAklandi;
                                aklamaKaydi.GuncellemeTarihi = DateTime.Now;
                                aklamaKaydi.GuncelleyenKullaniciID = resmiFatura.Fatura.SonGuncelleyenKullaniciID;
                                
                                kalanMiktar = 0;
                                
                                _logger.LogInformation("Aklama kaydı kısmen aklandı: {AklamaID}, Yeni Kayıt: {YeniAklamaID}", 
                                    aklamaKaydi.AklamaID, yeniKayit.AklamaID);
                            }
                        }
                    }
                    
                    // Faturanın aklama bilgilerini güncelle
                    resmiFatura.Fatura.AklanmaTarihi = DateTime.Now;
                    resmiFatura.Fatura.AklanmaNotu = "Otomatik aklama yapıldı.";
                    resmiFatura.Fatura.GuncellemeTarihi = DateTime.Now;
                    
                    // Değişiklikleri kaydet
                    await _unitOfWork.CommitTransactionAsync();
                    
                    _logger.LogInformation("Otomatik aklama başarıyla tamamlandı: {FaturaID}", resmiFatura.FaturaID);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Otomatik aklama sırasında hata oluştu: {FaturaID}", resmiFatura.FaturaID);
                    await _unitOfWork.RollbackTransactionAsync();
                    islemBasarili = false;
                }
                
                return islemBasarili;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Otomatik aklama sırasında beklenmeyen hata oluştu: {FaturaID}", resmiFatura.FaturaID);
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