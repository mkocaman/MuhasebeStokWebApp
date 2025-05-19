using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using MuhasebeStokWebApp.Data;
using MuhasebeStokWebApp.Data.Entities;
using MuhasebeStokWebApp.Data.Repositories;
using MuhasebeStokWebApp.Services.Interfaces;
using MuhasebeStokWebApp.ViewModels.Fatura;
using System.Linq;
using MuhasebeStokWebApp.Enums;
using Microsoft.EntityFrameworkCore.Storage;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;

namespace MuhasebeStokWebApp.Services
{
    public class FaturaOrchestrationService : IFaturaOrchestrationService
    {
        private readonly IFaturaCrudService _faturaService;
        private readonly IStokFifoService _stokFifoService;
        private readonly ILogger<FaturaOrchestrationService> _logger;
        private readonly IUnitOfWork _unitOfWork;
        private readonly ICariHareketService _cariHareketService;
        private readonly IIrsaliyeService _irsaliyeService;
        private readonly IStokHareketService _stokHareketService;
        private readonly ICariService _cariService;
        private readonly ApplicationDbContext _context;
        private readonly IIrsaliyeNumaralandirmaService _irsaliyeNumaralandirmaService;

        public FaturaOrchestrationService(
            IFaturaCrudService faturaService,
            IUnitOfWork unitOfWork,
            ILogger<FaturaOrchestrationService> logger,
            ICariService cariService,
            IStokFifoService stokFifoService,
            ICariHareketService cariHareketService,
            IIrsaliyeService irsaliyeService,
            IStokHareketService stokHareketService,
            ApplicationDbContext context,
            IIrsaliyeNumaralandirmaService irsaliyeNumaralandirmaService)
        {
            _faturaService = faturaService;
            _unitOfWork = unitOfWork;
            _logger = logger;
            _cariService = cariService;
            _stokFifoService = stokFifoService;
            _cariHareketService = cariHareketService;
            _irsaliyeService = irsaliyeService;
            _stokHareketService = stokHareketService;
            _context = context;
            _irsaliyeNumaralandirmaService = irsaliyeNumaralandirmaService;
        }

        /// <summary>
        /// Fatura ve ilişkili kayıtları (StokHareket, CariHareket, Irsaliye) oluşturur
        /// </summary>
        public async Task<Guid> CreateFaturaWithRelations(FaturaCreateViewModel viewModel, Guid? currentUserId)
        {
            // Transaction yönetimi için
            IDbContextTransaction transaction = null;
            
            try
            {
                _logger.LogInformation("CreateFaturaWithRelations başlatılıyor. Fatura No: {FaturaNo}", viewModel.FaturaNumarasi);
                
                // Fatura kalemlerini kontrol et - geçerli faturalar bulamazsak hata ver
                if (viewModel.FaturaKalemleri == null || !viewModel.FaturaKalemleri.Any(i => i.UrunID != default && i.Miktar > 0))
                {
                    _logger.LogWarning("Fatura kaydedilemedi: Geçerli fatura kalemi bulunamadı. Fatura No: {FaturaNo}", viewModel.FaturaNumarasi);
                    throw new ArgumentException("Fatura kaydedilemedi: Geçerli fatura kalemi bulunamadı.");
                }
                
                // Tüm işlemleri tek bir transaction içinde yönetmek için
                transaction = await _unitOfWork.BeginTransactionAsync(System.Data.IsolationLevel.ReadCommitted);
                _logger.LogInformation("Fatura işlemi için transaction başlatıldı. Fatura No: {FaturaNo}", viewModel.FaturaNumarasi);
                
                // Fatura entity'sini oluştur ve veritabanına ekle
                var fatura = new Fatura
                {
                    FaturaID = Guid.NewGuid(),
                    FaturaNumarasi = viewModel.FaturaNumarasi,
                    SiparisNumarasi = viewModel.SiparisNumarasi,
                    FaturaTarihi = viewModel.FaturaTarihi,
                    VadeTarihi = viewModel.VadeTarihi,
                    CariID = viewModel.CariID,
                    FaturaTuruID = viewModel.FaturaTuruID,
                    FaturaNotu = viewModel.Aciklama,
                    OlusturmaTarihi = DateTime.Now,
                    OlusturanKullaniciID = currentUserId,
                    AraToplam = viewModel.AraToplam,
                    KDVToplam = viewModel.KDVToplam,
                    GenelToplam = viewModel.GenelToplam,
                    DovizTuru = viewModel.DovizTuru,
                    ParaBirimi = viewModel.DovizTuru, // DovizTuru'nu ParaBirimi olarak kullan
                    DovizKuru = viewModel.DovizKuru,
                    AraToplamDoviz = viewModel.AraToplamDoviz,
                    KDVToplamDoviz = viewModel.KDVToplamDoviz,
                    GenelToplamDoviz = viewModel.GenelToplamDoviz,
                    IndirimTutari = viewModel.IndirimTutari ?? 0,
                    IndirimTutariDoviz = viewModel.IndirimTutariDoviz ?? 0,
                    Silindi = false
                };
                
                // Faturayı kaydet
                await _faturaService.AddAsync(fatura);
                
                // Fatura detaylarını ekle (geçersiz kalemleri filtrele)
                int kalemSayisi = 0;
                bool isAlisFaturasi = await _context.FaturaTurleri
                    .Where(ft => ft.FaturaTuruID == fatura.FaturaTuruID)
                    .Select(ft => ft.HareketTuru == "Giriş")
                    .FirstOrDefaultAsync();
                var faturaDetaylari = new List<FaturaDetay>();
                
                foreach (var kalem in viewModel.FaturaKalemleri.Where(k => k.UrunID != Guid.Empty && k.Miktar > 0))
                {
                    kalemSayisi++;
                    _logger.LogInformation("Fatura kalemi işleniyor. UrunID: {UrunID}, Miktar: {Miktar}", kalem.UrunID, kalem.Miktar);
                        
                    // NetTutar değerini düzelt - 100 ile çarpılmış olabilir, normal değeri kullan
                    decimal netTutar = kalem.NetTutar;
                    decimal kdvTutari = kalem.KdvTutari;
                    if (viewModel.DovizTuru == "USD" || viewModel.DovizTuru == "UZS")
                    {
                        // NetTutar'ı 100'e böl (çarpılmış olduğu için)
                        netTutar = kalem.NetTutar / 100;
                        kdvTutari = kalem.KdvTutari / 100;
                    }
                        
                    var faturaDetay = new FaturaDetay
                    {
                        FaturaDetayID = Guid.NewGuid(),
                        FaturaID = fatura.FaturaID,
                        UrunID = kalem.UrunID,
                        Miktar = kalem.Miktar,
                        BirimFiyat = kalem.BirimFiyat,
                        Birim = kalem.Birim,
                        KdvOrani = kalem.KdvOrani,
                        IndirimOrani = kalem.IndirimOrani,
                        Tutar = kalem.Tutar,
                        KdvTutari = kdvTutari,
                        IndirimTutari = kalem.IndirimTutari,
                        NetTutar = netTutar,
                        Aciklama = kalem.Aciklama,
                        OlusturmaTarihi = DateTime.Now,
                        GuncellemeTarihi = DateTime.Now,
                        OlusturanKullaniciID = currentUserId,
                        SonGuncelleyenKullaniciID = currentUserId,
                        Silindi = false,
                        SatirToplam = kalem.Tutar,
                        SatirKdvToplam = kdvTutari
                    };
                    
                    faturaDetaylari.Add(faturaDetay);
                    await _unitOfWork.FaturaDetayRepository.AddAsync(faturaDetay);
                    _logger.LogInformation("Fatura Detay eklendi. UrunID: {UrunID}", kalem.UrunID);
                
                    // Stok hareketleri oluştur (Alış -> Giriş, Satış -> Çıkış)
                    if (isAlisFaturasi)
                    {
                        // Alış faturası - Stok girişi
                        await _stokFifoService.StokGirisiYap(
                            kalem.UrunID,
                            kalem.Miktar,
                            kalem.BirimFiyat,
                            kalem.Birim ?? "Adet",
                            fatura.FaturaNumarasi ?? "",
                            "Fatura",
                            fatura.FaturaID,
                            $"Fatura No: {fatura.FaturaNumarasi}",
                            fatura.ParaBirimi,
                            fatura.DovizKuru);
                        
                        _logger.LogInformation("Alış faturası için stok girişi yapıldı. Ürün: {UrunID}, Miktar: {Miktar}", kalem.UrunID, kalem.Miktar);
                    }
                    else
                    {
                        // Satış faturası - Stok çıkışı
                        var stokCikisInfo = await _stokFifoService.StokCikisiYap(
                            kalem.UrunID,
                            kalem.Miktar,
                            StokHareketTipi.Cikis,
                            fatura.FaturaID,
                            $"Fatura No: {fatura.FaturaNumarasi}",
                            fatura.ParaBirimi,
                            false,
                            fatura.DovizKuru);
                        
                        _logger.LogInformation("Satış faturası için stok çıkışı yapıldı. Ürün: {UrunID}, Miktar: {Miktar}", kalem.UrunID, kalem.Miktar);
                    }
                }
                
                // Eğer hiçbir fatura kalemi eklenemediyse hata ver
                if (kalemSayisi <= 0)
                {
                    _logger.LogWarning("Fatura kaydedilemedi: Geçerli fatura kalemi eklenmedi. Fatura No: {FaturaNo}", viewModel.FaturaNumarasi);
                    throw new Exception("Fatura kaydedilemedi: Geçerli fatura kalemi eklenmedi.");
                }
                
                // Stok hareketlerini ayrıca kaydet
                await _stokHareketService.CreateStokHareket(fatura, faturaDetaylari, currentUserId);
                _logger.LogInformation("Stok hareketleri oluşturuldu. Fatura No: {FaturaNo}", viewModel.FaturaNumarasi);
                
                // Cari hareket oluştur
                if (fatura.CariID.HasValue && fatura.CariID.Value != Guid.Empty)
                {
                    var cariHareket = new CariHareket
                    {
                        CariHareketID = Guid.NewGuid(),
                        CariID = fatura.CariID.Value,
                        HareketTuru = isAlisFaturasi ? "Borç" : "Alacak",
                        Tutar = fatura.GenelToplam ?? 0,
                        Borc = isAlisFaturasi ? (fatura.GenelToplam ?? 0) : 0,
                        Alacak = isAlisFaturasi ? 0 : (fatura.GenelToplam ?? 0),
                        Tarih = fatura.FaturaTarihi ?? DateTime.Now,
                        VadeTarihi = fatura.VadeTarihi,
                        ReferansNo = fatura.FaturaNumarasi,
                        ReferansTuru = "Fatura",
                        ReferansID = fatura.FaturaID,
                        Aciklama = $"Fatura No: {fatura.FaturaNumarasi} (Güncellendi)",
                        OlusturmaTarihi = DateTime.Now,
                        OlusturanKullaniciID = currentUserId,
                        Silindi = false,
                        ParaBirimi = fatura.ParaBirimi ?? "USD",
                        TutarDoviz = fatura.GenelToplamDoviz ?? 0,
                        BorcDoviz = isAlisFaturasi ? (fatura.GenelToplamDoviz ?? 0) : 0,
                        AlacakDoviz = isAlisFaturasi ? 0 : (fatura.GenelToplamDoviz ?? 0)
                    };
                    
                    await _unitOfWork.CariHareketRepository.AddAsync(cariHareket);
                    _logger.LogInformation("Cari hareket oluşturuldu. CariID: {CariID}", fatura.CariID);
                }
                
                // Otomatik irsaliye oluştur
                if (viewModel.OtomatikIrsaliyeOlustur)
                {
                    // Fatura türüne göre irsaliye türü belirle
                    string irsaliyeTuru = isAlisFaturasi ? "Giriş" : "Çıkış";
                    
                    // IrsaliyeNumaralandirmaService kullanarak sıralı irsaliye numarası oluştur
                    var irsaliyeNumarasi = await _irsaliyeNumaralandirmaService.GenerateIrsaliyeNumarasiAsync();
                    
                    var irsaliye = new Irsaliye
                    {
                        IrsaliyeID = Guid.NewGuid(),
                        IrsaliyeNumarasi = irsaliyeNumarasi,
                        IrsaliyeTarihi = DateTime.Now,
                        CariID = fatura.CariID ?? Guid.Empty,
                        FaturaID = fatura.FaturaID,
                        IrsaliyeTuru = irsaliyeTuru,  // Fatura türü Alış -> İrsaliye türü Giriş, Fatura türü Satış -> İrsaliye türü Çıkış
                        OlusturmaTarihi = DateTime.Now,
                        OlusturanKullaniciId = currentUserId ?? Guid.Empty,
                        Aciklama = $"{fatura.FaturaNumarasi ?? ""} numaralı faturaya ait otomatik oluşturulan {irsaliyeTuru.ToLower()} irsaliyesi",
                        Silindi = false,
                        Aktif = true
                    };
                    
                    await _unitOfWork.IrsaliyeRepository.AddAsync(irsaliye);
                    _logger.LogInformation("Otomatik irsaliye oluşturuldu. IrsaliyeNo: {IrsaliyeNo}, Türü: {Turu}", irsaliye.IrsaliyeNumarasi, irsaliyeTuru);
                    
                    // İrsaliye detaylarını fatura kalemlerinden oluştur
                    foreach (var kalem in viewModel.FaturaKalemleri.Where(k => k.UrunID != Guid.Empty && k.Miktar > 0))
                    {
                        var irsaliyeDetay = new IrsaliyeDetay
                        {
                            IrsaliyeDetayID = Guid.NewGuid(),
                            IrsaliyeID = irsaliye.IrsaliyeID,
                            UrunID = kalem.UrunID,
                            Miktar = kalem.Miktar,
                            BirimFiyat = kalem.BirimFiyat,
                            KdvOrani = kalem.KdvOrani,
                            IndirimOrani = kalem.IndirimOrani,
                            Birim = kalem.Birim ?? "Adet",
                            Aciklama = kalem.Aciklama ?? "",
                            OlusturmaTarihi = DateTime.Now,
                            SatirToplam = kalem.Tutar,
                            SatirKdvToplam = kalem.KdvTutari,
                            Aktif = true,
                            Silindi = false
                        };
                        
                        await _unitOfWork.IrsaliyeDetayRepository.AddAsync(irsaliyeDetay);
                        _logger.LogInformation("İrsaliye detayı eklendi. UrunID: {UrunID}", kalem.UrunID);
                    }
                }
                
                // Tüm işlemler başarılı ise değişiklikleri kaydet
                await _unitOfWork.SaveChangesAsync();
                
                // Transaction'ı commit et
                await _unitOfWork.CommitTransactionAsync();
                
                _logger.LogInformation("Fatura ve ilişkili tüm kayıtlar başarıyla kaydedildi. Fatura No: {FaturaNo}", viewModel.FaturaNumarasi);
                    
                return fatura.FaturaID;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Fatura oluşturma hatası. Fatura No: {FaturaNo}, Hata: {Error}", 
                    viewModel.FaturaNumarasi, ex.Message);
                
                // Hata durumunda transaction'ı geri al
                if (transaction != null)
                {
                    await transaction.RollbackAsync();
                    _logger.LogInformation("Transaction geri alındı. Fatura No: {FaturaNo}", viewModel.FaturaNumarasi);
                }
                
                throw;
            }
            finally
            {
                if (transaction != null)
                {
                    await transaction.DisposeAsync();
                    _logger.LogInformation("Transaction temizlendi. Fatura No: {FaturaNo}", viewModel.FaturaNumarasi);
                }
            }
        }

        /// <summary>
        /// Fatura ve ilişkili kayıtları (StokHareket, CariHareket, Irsaliye) günceller
        /// </summary>
        public async Task<Guid> UpdateFaturaWithRelations(Guid id, FaturaEditViewModel viewModel, Guid? currentUserId)
        {
            // Transaction yönetimi için
            IDbContextTransaction transaction = null;
            
            try
            {
                // İşlemi transaction içinde yap
                transaction = await _context.Database.BeginTransactionAsync();
                
                _logger.LogInformation("Fatura ve ilişkili kayıtlar güncelleniyor. FaturaID: {FaturaID}", id);
                
                // Mevcut faturayı al
                var fatura = await _faturaService.GetByIdAsync(id);
                if (fatura == null)
                {
                    throw new Exception($"Güncellenecek fatura bulunamadı (ID: {id})");
                }
                
                // FaturaTurleri tablosundan fatura türünü al
                var faturaTuru = await _context.FaturaTurleri
                    .Where(ft => ft.FaturaTuruID == fatura.FaturaTuruID)
                    .FirstOrDefaultAsync();
                
                bool isAlisFaturasi = faturaTuru?.HareketTuru == "Giriş";
                
                // Fatura bilgilerini güncelle
                fatura.FaturaNumarasi = viewModel.FaturaNumarasi;
                fatura.SiparisNumarasi = viewModel.SiparisNumarasi; 
                fatura.FaturaTarihi = viewModel.FaturaTarihi;
                fatura.VadeTarihi = viewModel.VadeTarihi;
                fatura.CariID = viewModel.CariID;
                fatura.FaturaTuruID = viewModel.FaturaTuruID;
                fatura.GuncellemeTarihi = DateTime.Now;
                fatura.SonGuncelleyenKullaniciID = currentUserId;
                fatura.FaturaNotu = viewModel.Aciklama;
                fatura.AraToplam = viewModel.AraToplam;
                fatura.KDVToplam = viewModel.KdvToplam;
                fatura.GenelToplam = viewModel.GenelToplam;
                fatura.IndirimTutari = viewModel.IndirimTutari ?? 0;
                fatura.DovizTuru = viewModel.DovizTuru;
                fatura.ParaBirimi = viewModel.DovizTuru; // DovizTuru'nu ParaBirimi olarak kullan
                fatura.DovizKuru = viewModel.DovizKuru;
                fatura.ResmiMi = viewModel.ResmiMi;
                fatura.SozlesmeID = viewModel.SozlesmeID;
                
                // Döviz cinsinden toplamları hesapla
                decimal dovizKuru = fatura.DovizKuru ?? 1;
                
                // TRY ise döviz değerleri TL değerleriyle aynı olur
                if (fatura.DovizTuru == "TRY")
                {
                    fatura.AraToplamDoviz = fatura.AraToplam;
                    fatura.KDVToplamDoviz = fatura.KDVToplam;
                    fatura.GenelToplamDoviz = fatura.GenelToplam;
                    fatura.IndirimTutariDoviz = fatura.IndirimTutari;
                }
                else
                {
                    // Döviz kuruna bölerek döviz tutarlarını hesapla
                    fatura.AraToplamDoviz = dovizKuru > 0 ? fatura.AraToplam / dovizKuru : 0;
                    fatura.KDVToplamDoviz = dovizKuru > 0 ? fatura.KDVToplam / dovizKuru : 0;
                    fatura.GenelToplamDoviz = dovizKuru > 0 ? fatura.GenelToplam / dovizKuru : 0;
                    fatura.IndirimTutariDoviz = dovizKuru > 0 ? fatura.IndirimTutari / dovizKuru : 0;
                }
                
                await _faturaService.UpdateAsync(fatura);
                _logger.LogInformation("Fatura bilgileri güncellendi. FaturaID: {FaturaID}", id);
                
                // Önce eski fatura detaylarını silelim ve ilişkili kayıtları iptal edelim
                
                // İlgili FIFO kayıtlarını iptal et
                await _stokFifoService.FifoKayitlariniIptalEt(id, "Fatura", "Fatura güncellendi", currentUserId);
                _logger.LogInformation("İlişkili FIFO kayıtları iptal edildi. FaturaID: {FaturaID}", id);
                
                // İlişkili stok hareketlerini al ve sil
                var stokHareketler = await _unitOfWork.Repository<StokHareket>().GetAsync(
                    filter: sh => (sh.FaturaID == id || (sh.ReferansID == id && sh.ReferansTuru == "Fatura")) && !sh.Silindi
                );
                
                foreach (var stokHareket in stokHareketler)
                {
                    stokHareket.Silindi = true;
                    stokHareket.GuncellemeTarihi = DateTime.Now;
                    stokHareket.SonGuncelleyenKullaniciID = currentUserId;
                    _unitOfWork.Repository<StokHareket>().Update(stokHareket);
                }
                _logger.LogInformation("İlişkili stok hareketleri silindi. FaturaID: {FaturaID}", id);
                
                // İlişkili cari hareketleri al ve sil
                var cariHareketler = await _unitOfWork.CariHareketRepository.GetAsync(
                    filter: ch => ch.ReferansID == id && ch.ReferansTuru == "Fatura" && !ch.Silindi
                );
                
                foreach (var cariHareket in cariHareketler)
                {
                    cariHareket.Silindi = true;
                    cariHareket.GuncellemeTarihi = DateTime.Now;
                    cariHareket.OlusturanKullaniciID = currentUserId;
                    _unitOfWork.CariHareketRepository.Update(cariHareket);
                    
                    _logger.LogInformation("Cari hareket silindi. CariHareketID: {CariHareketID}", cariHareket.CariHareketID);
                }
                
                // Mevcut fatura detaylarını al ve güncelle/sil
                var existingDetaylar = await _context.FaturaDetaylari
                    .Where(fd => fd.FaturaID == id)
                    .ToListAsync();
                
                // Tüm mevcut detayları sil (mantıksal silme)
                foreach (var detay in existingDetaylar)
                {
                    detay.Silindi = true;
                    detay.GuncellemeTarihi = DateTime.Now;
                    detay.SonGuncelleyenKullaniciID = currentUserId;
                    _context.FaturaDetaylari.Update(detay);
                }
                _logger.LogInformation("Eski fatura detayları silindi. Toplam {count} detay. FaturaID: {FaturaID}", 
                    existingDetaylar.Count, id);
                
                // Yeni detayları ekle
                foreach (var kalem in viewModel.FaturaKalemleri)
                {
                    if (kalem.UrunID != Guid.Empty && kalem.Miktar > 0)
                    {
                        // NetTutar değerini düzelt - 100 ile çarpılmış olabilir, normal değeri kullan
                        decimal netTutar = kalem.NetTutar;
                        decimal kdvTutari = kalem.KdvTutari;
                        //if (viewModel.DovizTuru == "USD" || viewModel.DovizTuru == "UZS")
                        //{
                        //    // NetTutar'ı 100'e böl (çarpılmış olduğu için)
                        //    netTutar = kalem.NetTutar / 100;
                        //    kdvTutari = kalem.KdvTutari / 100;
                        //}

                        var faturaDetay = new FaturaDetay
                        {
                            FaturaDetayID = Guid.NewGuid(),
                            FaturaID = fatura.FaturaID,
                            UrunID = kalem.UrunID,
                            Miktar = kalem.Miktar,
                            BirimFiyat = kalem.BirimFiyat,
                            KdvOrani = kalem.KdvOrani,
                            IndirimOrani = kalem.IndirimOrani,
                            Tutar = kalem.Tutar,
                            KdvTutari = kdvTutari,
                            IndirimTutari = kalem.IndirimTutari,
                            NetTutar = netTutar,
                            Aciklama = kalem.Aciklama,
                            Birim = kalem.Birim,
                            OlusturmaTarihi = DateTime.Now,
                            GuncellemeTarihi = DateTime.Now,
                            OlusturanKullaniciID = currentUserId,
                            SonGuncelleyenKullaniciID = currentUserId,
                            Silindi = false,
                            SatirToplam = kalem.Tutar,
                            SatirKdvToplam = kdvTutari
                        };
                        
                        await _context.FaturaDetaylari.AddAsync(faturaDetay);
                        _logger.LogInformation("Yeni fatura detayı eklendi. UrunID: {UrunID}", kalem.UrunID);
                        
                        // Stok hareketi ekle
                        decimal stokMiktar = kalem.Miktar;
                        var hareketTipi = isAlisFaturasi ? StokHareketTipi.Giris : StokHareketTipi.Cikis;
                        var birim = kalem.Birim ?? "Adet";

                        var stokHareket = new StokHareket
                        {
                            StokHareketID = Guid.NewGuid(),
                            UrunID = kalem.UrunID,
                            FaturaID = fatura.FaturaID,
                            Miktar = stokMiktar,
                            Birim = birim,
                            Tarih = fatura.FaturaTarihi ?? DateTime.Now,
                            HareketTuru = hareketTipi,
                            BirimFiyat = kalem.BirimFiyat,
                            ReferansTuru = "Fatura",
                            ReferansNo = fatura.FaturaNumarasi ?? "",
                            ReferansID = fatura.FaturaID,
                            Aciklama = $"Fatura No: {fatura.FaturaNumarasi}",
                            OlusturmaTarihi = DateTime.Now,
                            GuncellemeTarihi = DateTime.Now,
                            IslemYapanKullaniciID = currentUserId,
                            SonGuncelleyenKullaniciID = currentUserId,
                            Silindi = false,
                            ParaBirimi = fatura.ParaBirimi
                        };
                        
                        await _context.StokHareketleri.AddAsync(stokHareket);
                        _logger.LogInformation("Stok hareketi oluşturuldu. UrunID: {UrunID}, Miktar: {Miktar}",
                            kalem.UrunID, kalem.Miktar);
                        
                        // Stok hareketi ve FIFO kayıtları oluştur
                        if (isAlisFaturasi)
                        {
                            // Alış faturası - Stok girişi
                            await _stokFifoService.StokGirisiYap(
                                kalem.UrunID,
                                kalem.Miktar,
                                kalem.BirimFiyat,
                                kalem.Birim ?? "Adet",
                                fatura.FaturaNumarasi ?? "",
                                "Fatura",
                                fatura.FaturaID,
                                $"Fatura No: {fatura.FaturaNumarasi}",
                                fatura.ParaBirimi,
                                fatura.DovizKuru);
                        }
                        else
                        {
                            // Satış faturası - Stok çıkışı
                            var stokCikisInfo = await _stokFifoService.StokCikisiYap(
                                kalem.UrunID,
                                kalem.Miktar,
                                StokHareketTipi.Cikis,
                                fatura.FaturaID,
                                $"Fatura No: {fatura.FaturaNumarasi}",
                                fatura.ParaBirimi,
                                false,
                                fatura.DovizKuru);
                        }
                        _logger.LogInformation("FIFO kaydı oluşturuldu. UrunID: {UrunID}", kalem.UrunID);
                    }
                }
                
                // Cari hareket oluştur
                if (fatura.CariID.HasValue && fatura.CariID != Guid.Empty)
                {
                    var cariHareket = new CariHareket
                    {
                        CariHareketID = Guid.NewGuid(),
                        CariID = fatura.CariID.Value,
                        HareketTuru = isAlisFaturasi ? "Borç" : "Alacak",
                        Tutar = fatura.GenelToplam ?? 0,
                        Borc = isAlisFaturasi ? (fatura.GenelToplam ?? 0) : 0,
                        Alacak = isAlisFaturasi ? 0 : (fatura.GenelToplam ?? 0),
                        Tarih = fatura.FaturaTarihi ?? DateTime.Now,
                        VadeTarihi = fatura.VadeTarihi,
                        ReferansNo = fatura.FaturaNumarasi,
                        ReferansTuru = "Fatura",
                        ReferansID = fatura.FaturaID,
                        Aciklama = $"Fatura No: {fatura.FaturaNumarasi} (Güncellendi)",
                        OlusturmaTarihi = DateTime.Now,
                        OlusturanKullaniciID = currentUserId,
                        Silindi = false,
                        ParaBirimi = fatura.ParaBirimi ?? "USD",
                        TutarDoviz = fatura.GenelToplamDoviz ?? 0,
                        BorcDoviz = isAlisFaturasi ? (fatura.GenelToplamDoviz ?? 0) : 0,
                        AlacakDoviz = isAlisFaturasi ? 0 : (fatura.GenelToplamDoviz ?? 0)
                    };
                    
                    await _unitOfWork.CariHareketRepository.AddAsync(cariHareket);
                    _logger.LogInformation("Cari hareket oluşturuldu. CariID: {CariID}", fatura.CariID);
                }
                
                // İlişkili irsaliyeleri güncelle
                var irsaliyeler = await _unitOfWork.IrsaliyeRepository.GetAsync(
                    filter: i => i.FaturaID == id && !i.Silindi
                );
                
                foreach (var irsaliye in irsaliyeler)
                {
                    irsaliye.CariID = fatura.CariID ?? Guid.Empty;
                    irsaliye.GuncellemeTarihi = DateTime.Now;
                    irsaliye.SonGuncelleyenKullaniciId = currentUserId;
                    irsaliye.Aciklama = $"Fatura No: {fatura.FaturaNumarasi} (Güncellendi)";
                    _unitOfWork.IrsaliyeRepository.Update(irsaliye);
                    _logger.LogInformation("İrsaliye güncellendi. IrsaliyeID: {IrsaliyeID}", irsaliye.IrsaliyeID);
                }
                
                // Değişiklikleri kaydet
                await _unitOfWork.SaveChangesAsync();
                
                // Transaction'ı commit et
                await transaction.CommitAsync();
                
                _logger.LogInformation("Fatura ve ilişkili tüm kayıtlar başarıyla güncellendi. FaturaID: {FaturaID}", id);
                return fatura.FaturaID;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Fatura ve ilişkili kayıtlar güncellenirken hata. FaturaID: {FaturaID}, Hata: {Error}", 
                    id, ex.Message);
                
                // Hata durumunda transaction'ı geri al
                if (transaction != null)
                {
                    await transaction.RollbackAsync();
                    _logger.LogInformation("Fatura güncelleme işlemi geri alındı. FaturaID: {FaturaID}", id);
                }
                
                throw;
            }
            finally
            {
                if (transaction != null)
                {
                    await transaction.DisposeAsync();
                }
            }
        }

        /// <summary>
        /// Fatura ve ilişkili kayıtları (StokHareket, CariHareket, Irsaliye) siler
        /// </summary>
        public async Task<bool> DeleteFaturaWithRelations(Guid id, Guid? currentUserId)
        {
            // Transaction yönetimi için
            IDbContextTransaction transaction = null;
            
            try
            {
                _logger.LogInformation("Fatura ve ilişkili kayıtlar siliniyor. FaturaID: {FaturaID}", id);
                
                // İşlemi transaction içinde yap
                transaction = await _context.Database.BeginTransactionAsync();
                
                // İlgili FIFO kayıtlarını iptal et
                // İptal işlemi sırasında mevcut transaction'ı kullanacak
                await _stokFifoService.FifoKayitlariniIptalEt(id, "Fatura", "Fatura silindi", currentUserId);
                
                // İlişkili irsaliyeleri al
                var irsaliyeler = await _unitOfWork.IrsaliyeRepository.GetAsync(
                    filter: i => i.FaturaID == id && !i.Silindi
                );
                
                // İrsaliyeleri sil
                foreach (var irsaliye in irsaliyeler)
                {
                    // İrsaliye detaylarını al ve sil
                    var irsaliyeDetaylari = await _unitOfWork.IrsaliyeDetayRepository.GetAsync(
                        filter: d => d.IrsaliyeID == irsaliye.IrsaliyeID && !d.Silindi
                    );
                    
                    foreach (var detay in irsaliyeDetaylari)
                    {
                        detay.Silindi = true;
                        detay.GuncellemeTarihi = DateTime.Now;
                        detay.SonGuncelleyenKullaniciId = currentUserId;
                        _unitOfWork.IrsaliyeDetayRepository.Update(detay);
                    }
                    
                    irsaliye.Silindi = true;
                    irsaliye.GuncellemeTarihi = DateTime.Now;
                    irsaliye.SonGuncelleyenKullaniciId = currentUserId;
                    _unitOfWork.IrsaliyeRepository.Update(irsaliye);
                    
                    _logger.LogInformation("İrsaliye silindi. IrsaliyeID: {IrsaliyeID}", irsaliye.IrsaliyeID);
                }
                
                // İlişkili cari hareketleri al
                var cariHareketler = await _unitOfWork.CariHareketRepository.GetAsync(
                    filter: ch => ch.ReferansID == id && ch.ReferansTuru == "Fatura" && !ch.Silindi
                );
                
                // Cari hareketleri sil
                foreach (var cariHareket in cariHareketler)
                {
                    cariHareket.Silindi = true;
                    cariHareket.GuncellemeTarihi = DateTime.Now;
                    cariHareket.OlusturanKullaniciID = currentUserId;
                    _unitOfWork.CariHareketRepository.Update(cariHareket);
                    
                    _logger.LogInformation("Cari hareket silindi. CariHareketID: {CariHareketID}", cariHareket.CariHareketID);
                }
                
                // İlişkili stok hareketlerini al - hem FaturaID hem de ReferansID ile eşleşenleri kontrol et
                var stokHareketler = await _unitOfWork.Repository<StokHareket>().GetAsync(
                    filter: sh => (sh.FaturaID == id || (sh.ReferansID == id && sh.ReferansTuru == "Fatura")) && !sh.Silindi
                );
                
                // Stok hareketleri sil
                foreach (var stokHareket in stokHareketler)
                {
                    stokHareket.Silindi = true;
                    stokHareket.GuncellemeTarihi = DateTime.Now;
                    stokHareket.SonGuncelleyenKullaniciID = currentUserId;
                    _unitOfWork.Repository<StokHareket>().Update(stokHareket);
                    
                    _logger.LogInformation("Stok hareketi silindi. StokHareketID: {StokHareketID}", stokHareket.StokHareketID);
                }
                
                // Faturayı doğrudan repository üzerinden silelim, transaction çakışmasını önlemek için
                var fatura = await _unitOfWork.FaturaRepository.GetByIdAsync(id);
                if (fatura != null)
                {
                    fatura.Silindi = true;
                    fatura.GuncellemeTarihi = DateTime.Now;
                    fatura.SonGuncelleyenKullaniciID = currentUserId;
                    _unitOfWork.FaturaRepository.Update(fatura);

                    // Fatura detaylarını da silelim
                    var faturaDetaylari = await _unitOfWork.FaturaDetayRepository.GetAsync(
                        filter: fd => fd.FaturaID == id && !fd.Silindi
                    );
                    
                    foreach (var detay in faturaDetaylari)
                    {
                        detay.Silindi = true;
                        detay.GuncellemeTarihi = DateTime.Now;
                        _unitOfWork.FaturaDetayRepository.Update(detay);
                    }
                    
                    _logger.LogInformation("Fatura silindi. FaturaID: {FaturaID}", fatura.FaturaID);
                }
                
                // Değişiklikleri kaydet
                await _unitOfWork.SaveChangesAsync();
                
                // Transaction'ı commit et
                await transaction.CommitAsync();
                
                _logger.LogInformation("Fatura ve ilişkili tüm kayıtlar başarıyla silindi. FaturaID: {FaturaID}", id);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Fatura ve ilişkili kayıtlar silinirken hata. FaturaID: {FaturaID}, Hata: {Error}", 
                    id, ex.Message);
                
                // Hata durumunda transaction'ı geri al
                if (transaction != null)
                {
                    await transaction.RollbackAsync();
                    _logger.LogInformation("Fatura silme işlemi geri alındı. FaturaID: {FaturaID}", id);
                }
                
                throw;
            }
            finally
            {
                if (transaction != null)
                {
                    await transaction.DisposeAsync();
                }
            }
        }
    }
} 
