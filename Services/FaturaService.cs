using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.Logging;
using MuhasebeStokWebApp.Data;
using MuhasebeStokWebApp.Data.Entities;
using MuhasebeStokWebApp.Data.Repositories;
using MuhasebeStokWebApp.Enums;
using MuhasebeStokWebApp.Services.Interfaces;
using MuhasebeStokWebApp.ViewModels.Fatura;

namespace MuhasebeStokWebApp.Services
{
    public class FaturaService : IFaturaService
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<FaturaService> _logger;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IStokFifoService _stokFifoService;
        private readonly IDovizKuruService _dovizKuruService;
        private readonly IMaliyetHesaplamaService _maliyetHesaplamaService;
        private readonly IFaturaValidationService _faturaValidationService;
        private readonly ICariHareketService _cariHareketService;
        private readonly IStokHareketService _stokHareketService;
        private readonly IIrsaliyeService _irsaliyeService;

        public FaturaService(
            ApplicationDbContext context,
            IUnitOfWork unitOfWork,
            ILogger<FaturaService> logger,
            IStokFifoService stokFifoService,
            IDovizKuruService dovizKuruService,
            IMaliyetHesaplamaService maliyetHesaplamaService,
            IFaturaValidationService faturaValidationService,
            ICariHareketService cariHareketService,
            IStokHareketService stokHareketService,
            IIrsaliyeService irsaliyeService)
        {
            _context = context;
            _unitOfWork = unitOfWork;
            _logger = logger;
            _stokFifoService = stokFifoService;
            _dovizKuruService = dovizKuruService;
            _maliyetHesaplamaService = maliyetHesaplamaService;
            _faturaValidationService = faturaValidationService;
            _cariHareketService = cariHareketService;
            _stokHareketService = stokHareketService;
            _irsaliyeService = irsaliyeService;
        }

        public async Task<IEnumerable<Fatura>> GetAllAsync()
        {
            return await _context.Faturalar
                .Include(f => f.Cari)
                .Include(f => f.FaturaDetaylari)
                .Where(f => !f.Silindi)
                .ToListAsync();
        }

        public async Task<Fatura> GetByIdAsync(Guid id)
        {
            return await _context.Faturalar
                .Include(f => f.Cari)
                .Include(f => f.FaturaDetaylari)
                    .ThenInclude(fk => fk.Urun)
                .FirstOrDefaultAsync(f => f.FaturaID == id && !f.Silindi);
        }

        public async Task<Fatura> AddAsync(Fatura fatura)
        {
            await _context.Faturalar.AddAsync(fatura);
            await _context.SaveChangesAsync();
            return fatura;
        }

        public async Task<Fatura> UpdateAsync(Fatura fatura)
        {
            _context.Faturalar.Update(fatura);
            await _context.SaveChangesAsync();
            return fatura;
        }

        public async Task<bool> DeleteAsync(Guid id)
        {
            var fatura = await _context.Faturalar.FindAsync(id);
            if (fatura == null)
                return false;

            fatura.Silindi = true;
            await _context.SaveChangesAsync();
            return true;
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
            
            var isUsedInPayments = await _context.FaturaOdemeleri
                .AnyAsync(o => o.FaturaID == id && !o.Silindi);
                
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
                var faturaTuru = await _context.FaturaTurleri.FindAsync(viewModel.FaturaTuruID);
                var stokHareketTipi = faturaTuru?.HareketTuru == "Giriş" 
                    ? StokHareketiTipi.Giris 
                    : StokHareketiTipi.Cikis;
                
                // 1. Fatura oluştur
                var fatura = new Fatura
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
                var stokHareketService = new StokHareketService(_context, _unitOfWork, _logger as ILogger<StokHareketService>, _stokFifoService);
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
                    var cariHareketService = new CariHareketService(_context, null, _unitOfWork, _logger as ILogger<CariHareketService>);
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
            var lastFatura = _context.Faturalar
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
            var lastFatura = _context.Faturalar
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
        /// Faturadan otomatik irsaliye oluşturur
        /// </summary>
        /// <param name="fatura">Fatura nesnesi</param>
        /// <param name="depoID">Depo ID</param>
        private async Task CreateIrsaliyeFromFatura(Fatura fatura, Guid? depoID)
        {
            try
            {
                // IrsaliyeService üzerinden irsaliye oluştur
                await _irsaliyeService.OtomatikIrsaliyeOlustur(fatura, depoID);
                _logger.LogInformation($"Faturadan irsaliye başarıyla oluşturuldu. FaturaID: {fatura.FaturaID}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Faturadan irsaliye oluşturulurken hata: {ex.Message}");
                throw;
            }
        }

        /// <summary>
        /// Faturayı ve ilişkili kayıtları siler (soft delete).
        /// Tüm bağlı kayıtları (StokHareket, StokFifo, CariHareket, İrsaliye vb.) iptal eder.
        /// </summary>
        /// <param name="id">Silinecek fatura ID</param>
        /// <param name="currentUserId">İşlemi yapan kullanıcı ID</param>
        /// <returns>İşlemin başarılı olup olmadığı</returns>
        public async Task<bool> DeleteFatura(Guid id, Guid? currentUserId)
        {
            _logger.LogInformation("FaturaService.DeleteFatura başlatılıyor. FaturaID: {FaturaID}", id);
            
            // Faturayı detaylarıyla birlikte getir
            var fatura = await _context.Faturalar
                .Include(f => f.FaturaDetaylari)
                .Include(f => f.FaturaTuru)
                .FirstOrDefaultAsync(f => f.FaturaID == id && !f.Silindi);
                
            if (fatura == null)
            {
                _logger.LogWarning("Silinmek istenen fatura bulunamadı. FaturaID: {FaturaID}", id);
                return false;
            }
            
            // Faturanın kullanımda olup olmadığını kontrol et (örn: ödeme kayıtları)
            bool isInUse = await IsFaturaInUseAsync(id);
            if (isInUse)
            {
                _logger.LogWarning("Fatura kullanımda olduğu için silinemez. FaturaID: {FaturaID}", id);
                throw new Exception("Bu fatura ödeme kayıtlarında kullanılmaktadır ve silinmesi mümkün değildir.");
            }
            
            // UnitOfWork ile transaction başlat
            await _unitOfWork.BeginTransactionAsync();
            
            try
            {
                // 1. FIFO kayıtlarını iptal et
                await _stokFifoService.FifoKayitlariniIptalEt(id, "Fatura", "Fatura silme işlemi nedeniyle iptal edildi");
                _logger.LogInformation("Faturaya bağlı FIFO kayıtları iptal edildi. FaturaID: {FaturaID}", id);
                
                // 2. Stok hareketlerini iptal et (silindi olarak işaretle)
                var stokHareketleri = await _context.StokHareketleri
                    .Where(sh => sh.FaturaID == id && !sh.Silindi)
                    .ToListAsync();
                    
                foreach (var hareket in stokHareketleri)
                {
                    hareket.Silindi = true;
                    hareket.OlusturmaTarihi = DateTime.Now; // Değişiklik saati olarak oluşturma tarihini güncelle
                    _unitOfWork.StokHareketRepository.Update(hareket);
                }
                _logger.LogInformation("Faturaya bağlı stok hareketleri iptal edildi. FaturaID: {FaturaID}", id);
                
                // 3. Cari hareketleri iptal et
                var cariHareketler = await _context.CariHareketler
                    .Where(ch => ch.ReferansID == id && ch.ReferansTuru == "Fatura" && !ch.Silindi)
                    .ToListAsync();
                    
                foreach (var cariHareket in cariHareketler)
                {
                    cariHareket.Silindi = true;
                    cariHareket.GuncellemeTarihi = DateTime.Now;
                    _unitOfWork.CariHareketRepository.Update(cariHareket);
                }
                _logger.LogInformation("Faturaya bağlı cari hareketler iptal edildi. FaturaID: {FaturaID}", id);
                
                // 4. İrsaliyeleri iptal et
                var irsaliyeler = await _context.Irsaliyeler
                    .Include(i => i.IrsaliyeDetaylari)
                    .Where(i => i.FaturaID == id && !i.Silindi)
                    .ToListAsync();
                
                foreach (var irsaliye in irsaliyeler)
                {
                    irsaliye.Silindi = true;
                    irsaliye.GuncellemeTarihi = DateTime.Now;
                    _unitOfWork.IrsaliyeRepository.Update(irsaliye);
                    
                    // İrsaliye detaylarını sil
                    foreach (var detay in irsaliye.IrsaliyeDetaylari.Where(d => !d.Silindi))
                    {
                        detay.Silindi = true;
                        detay.GuncellemeTarihi = DateTime.Now;
                        _unitOfWork.IrsaliyeDetayRepository.Update(detay);
                    }
                }
                _logger.LogInformation("Faturaya bağlı irsaliyeler iptal edildi. FaturaID: {FaturaID}", id);
                
                // 5. Fatura detaylarını sil (soft delete)
                foreach (var detay in fatura.FaturaDetaylari)
                {
                    detay.Silindi = true;
                    _unitOfWork.FaturaDetayRepository.Update(detay);
                }
                _logger.LogInformation("Fatura detayları iptal edildi. FaturaID: {FaturaID}", id);
                
                // 6. Faturayı sil (soft delete)
                fatura.Silindi = true;
                fatura.GuncellemeTarihi = DateTime.Now;
                _unitOfWork.FaturaRepository.Update(fatura);
                _logger.LogInformation("Fatura iptal edildi. FaturaID: {FaturaID}", id);
                
                // 7. Değişiklikleri kaydet
                await _unitOfWork.CompleteAsync();
                
                // 8. Transaction'ı commit et
                await _unitOfWork.CommitTransactionAsync();
                
                _logger.LogInformation("Fatura ve ilişkili tüm kayıtlar başarıyla silindi. FaturaID: {FaturaID}", id);
                
                return true;
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
                
                _logger.LogError(ex, $"Fatura silme işlemi sırasında hata: {ex.Message}");
                throw;
            }
        }

        /// <summary>
        /// Faturadan irsaliye oluşturur
        /// </summary>
        /// <param name="faturaID">Fatura ID</param>
        /// <param name="depoID">Depo ID (opsiyonel)</param>
        public async Task<Guid> CreateIrsaliyeFromFaturaID(Guid faturaID, Guid? depoID = null)
        {
            try
            {
                // IrsaliyeService üzerinden irsaliye oluştur
                return await _irsaliyeService.OtomatikIrsaliyeOlusturFromID(faturaID, depoID);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Faturadan irsaliye oluşturulurken hata: {ex.Message}");
                throw;
            }
        }

        private string GenerateIrsaliyeNumarasi()
        {
            // IrsaliyeService üzerinden numara oluştur
            return _irsaliyeService.GenerateIrsaliyeNumarasi();
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
            var existingFatura = await _context.Faturalar
                .Include(f => f.FaturaDetaylari)
                .Include(f => f.FaturaTuru)
                .FirstOrDefaultAsync(f => f.FaturaID == id && !f.Silindi);
                
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
                var faturaTuru = await _context.FaturaTurleri.FindAsync(viewModel.FaturaTuruID);
                var stokHareketTipi = faturaTuru?.HareketTuru == "Giriş" 
                    ? StokHareketiTipi.Giris 
                    : StokHareketiTipi.Cikis;
                
                // 1. Eski kayıtları iptal et
                
                // 1.1. Faturaya bağlı FIFO kayıtlarını iptal et
                await _stokFifoService.FifoKayitlariniIptalEt(id, "Fatura", "Fatura güncelleme işlemi nedeniyle iptal edildi");
                _logger.LogInformation("Faturaya bağlı FIFO kayıtları iptal edildi. FaturaID: {FaturaID}", id);
                
                // 1.2. Faturaya bağlı stok hareketlerini iptal et (silindi olarak işaretle)
                var stokHareketleri = await _context.StokHareketleri
                    .Where(sh => sh.FaturaID == id && !sh.Silindi)
                    .ToListAsync();
                    
                foreach (var hareket in stokHareketleri)
                {
                    hareket.Silindi = true;
                    hareket.OlusturmaTarihi = DateTime.Now; // Değişiklik saati olarak oluşturma tarihini güncelle
                    _unitOfWork.StokHareketRepository.Update(hareket);
                }
                _logger.LogInformation("Faturaya bağlı stok hareketleri iptal edildi. FaturaID: {FaturaID}", id);
                
                // 1.3. Faturaya bağlı eski CariHareket kayıtlarını bul ve iptal et
                var cariHareketler = await _context.CariHareketler
                    .Where(ch => ch.ReferansID == id && ch.ReferansTuru == "Fatura" && !ch.Silindi)
                    .ToListAsync();
                    
                foreach (var cariHareket in cariHareketler)
                {
                    cariHareket.Silindi = true;
                    cariHareket.GuncellemeTarihi = DateTime.Now;
                    _unitOfWork.CariHareketRepository.Update(cariHareket);
                }
                _logger.LogInformation("Faturaya bağlı cari hareketler iptal edildi. FaturaID: {FaturaID}", id);
                
                // 1.4. Eski fatura detaylarını sil
                _context.FaturaDetaylari.RemoveRange(existingFatura.FaturaDetaylari);
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
                _unitOfWork.FaturaRepository.Update(existingFatura);
                await _unitOfWork.FaturaDetayRepository.AddRangeAsync(faturaDetaylari);
                
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
                await _unitOfWork.StokHareketRepository.AddRangeAsync(yeniStokHareketleri);
                
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
                    
                    await _unitOfWork.CariHareketRepository.AddAsync(cariHareket);
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
                    var mevcutIrsaliye = await _context.Irsaliyeler
                        .FirstOrDefaultAsync(i => i.FaturaID == existingFatura.FaturaID && !i.Silindi);
                        
                    if (mevcutIrsaliye != null)
                    {
                        // Mevcut irsaliyeyi sil/iptal et
                        mevcutIrsaliye.Silindi = true;
                        mevcutIrsaliye.GuncellemeTarihi = DateTime.Now;
                        _unitOfWork.IrsaliyeRepository.Update(mevcutIrsaliye);
                        
                        // İrsaliye detaylarını sil
                        var irsaliyeDetaylari = await _context.IrsaliyeDetaylari
                            .Where(id => id.IrsaliyeID == mevcutIrsaliye.IrsaliyeID && !id.Silindi)
                            .ToListAsync();
                            
                        foreach (var detay in irsaliyeDetaylari)
                        {
                            detay.Silindi = true;
                            detay.GuncellemeTarihi = DateTime.Now;
                            _unitOfWork.IrsaliyeDetayRepository.Update(detay);
                        }
                        
                        await _unitOfWork.CompleteAsync();
                    }
                    
                    // Yeni irsaliye oluştur
                    await CreateIrsaliyeFromFatura(existingFatura, depoID);
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
            var ilkDepo = await _context.Depolar
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
            // FaturaEditViewModel'da OtomatikIrsaliyeOlustur property'si yoksa, 
            // sistem ayarlarına göre karar vermek gerekir
            
            // Burada gerçek implementasyonda sistem ayarlarını kontrol etmek gerekebilir
            // Ya da edit view'da irsaliye oluşturma seçeneği varsa, bu irsaliye form field'ına göre karar verebiliriz
            
            // Basitlik için varsayılan olarak true döndürelim
            return true;
        }
    }
} 