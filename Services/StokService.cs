using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Transactions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using MuhasebeStokWebApp.Data;
using MuhasebeStokWebApp.Data.Entities;
using MuhasebeStokWebApp.Data.Repositories;
using MuhasebeStokWebApp.Enums;
using MuhasebeStokWebApp.Services.Interfaces;
using MuhasebeStokWebApp.ViewModels;
using MuhasebeStokWebApp.ViewModels.Stok;

namespace MuhasebeStokWebApp.Services
{
    public class StokService : IStokService
    {
        private readonly ILogger<StokService> _logger;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IExceptionHandlingService _exceptionHandler;
        private bool _transactionActive = false;

        public StokService(
            IUnitOfWork unitOfWork,
            ILogger<StokService> logger,
            IExceptionHandlingService exceptionHandler)
        {
            _unitOfWork = unitOfWork;
            _logger = logger;
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
            }, $"StokService.{operationName}", parameters);
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
            }, $"StokService.{operationName}", parameters);
        }

        public async Task<IEnumerable<StokViewModel>> GetAllStokDurumu()
        {
            return await _exceptionHandler.HandleExceptionAsync(async () =>
            {
                var stokHareketleri = await _unitOfWork.StokHareketRepository.GetAllAsync(
                    sh => !sh.Silindi,
                    includeProperties: "Urun,Urun.Birim");

                var stokDurumu = stokHareketleri
                    .GroupBy(sh => new { sh.UrunID, UrunAdi = sh.Urun?.UrunAdi, UrunKodu = sh.Urun?.UrunKodu, BirimAdi = sh.Urun?.Birim?.BirimAdi })
                    .Select(g => new StokViewModel
                    {
                        UrunID = g.Key.UrunID,
                        UrunAdi = g.Key.UrunAdi,
                        UrunKodu = g.Key.UrunKodu,
                        BirimAdi = g.Key.BirimAdi,
                        GirisMiktari = g.Where(sh => sh.HareketTuru == StokHareketTipi.Giris).Sum(sh => sh.Miktar),
                        CikisMiktari = g.Where(sh => sh.HareketTuru == StokHareketTipi.Cikis).Sum(sh => Math.Abs(sh.Miktar))
                    })
                    .ToList();

                return stokDurumu;
            }, "GetAllStokDurumu");
        }

        public async Task<StokHareket> GetStokHareketByIdAsync(Guid id)
        {
            return await _exceptionHandler.HandleExceptionAsync(async () =>
            {
                return await _unitOfWork.StokHareketRepository.GetFirstOrDefaultAsync(
                    s => s.StokHareketID == id,
                    includeProperties: "Urun");
            }, "GetStokHareketByIdAsync", id);
        }

        public async Task<List<StokHareket>> GetStokGirisCikisAsync(Guid urunId, DateTime baslangicTarihi, DateTime bitisTarihi)
        {
            return await _exceptionHandler.HandleExceptionAsync(async () =>
            {
                var result = await _unitOfWork.StokHareketRepository.GetAllAsync(
                    sh => sh.UrunID == urunId && sh.Tarih >= baslangicTarihi && sh.Tarih <= bitisTarihi && !sh.Silindi,
                    q => q.OrderBy(sh => sh.Tarih),
                    includeProperties: "Urun,Urun.Birim");
                
                return result.ToList();
            }, "GetStokGirisCikisAsync", urunId, baslangicTarihi, bitisTarihi);
        }

        /// <summary>
        /// Bir ürünün dinamik stok miktarını hesaplar. Bu metot ürün için yapılan giriş ve çıkış hareketlerini
        /// toplayarak dinamik stok miktarını hesaplar. StokMiktar Urun sınıfında statik olarak tutulmaz.
        /// </summary>
        /// <param name="urunID">Stok miktarı hesaplanacak ürünün ID'si</param>
        /// <param name="depoID">İsteğe bağlı depo filtresi. Eğer belirtilirse, sadece o depodaki stok hesaplanır</param>
        /// <returns>Ürünün dinamik stok miktarı</returns>
        public async Task<decimal> GetDinamikStokMiktari(Guid urunID, Guid? depoID = null)
        {
            try
            {
                // Stok hareketlerini filtreleme
                var stokHareketleri = await _unitOfWork.StokHareketRepository.GetAllAsync(
                    sh => sh.UrunID == urunID && !sh.Silindi && (depoID == null || sh.DepoID == depoID));

                // Giriş hareketlerinin toplamı
                var girisler = stokHareketleri
                    .Where(sh => sh.HareketTuru == StokHareketTipi.Giris)
                    .Sum(sh => sh.Miktar);

                // Çıkış hareketlerinin toplamı (mutlak değer)
                var cikislar = stokHareketleri
                    .Where(sh => sh.HareketTuru == StokHareketTipi.Cikis)
                    .Sum(sh => Math.Abs(sh.Miktar));

                return girisler - cikislar; // Çıkışları çıkararak hesaplıyoruz
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Stok miktarı hesaplanırken hata oluştu: UrunID={urunID}");
                return 0;
            }
        }

        /// <summary>
        /// Manuel stok girişi yapar
        /// </summary>
        public async Task<bool> StokGirisYapAsync(StokGirisViewModel viewModel)
        {
            return await ExecuteInTransactionAsync(async () =>
            {
                _logger.LogInformation($"Stok girişi yapılıyor. UrunID: {viewModel.UrunID}, Miktar: {viewModel.Miktar}");

                // Ürünü kontrol et
                var urun = await _unitOfWork.UrunRepository.GetByIdAsync(viewModel.UrunID);
                if (urun == null)
                {
                    _logger.LogWarning($"Stok girişi yapılamadı: Ürün bulunamadı. UrunID: {viewModel.UrunID}");
                    return false;
                }

                // Stok hareketi oluştur
                var stokHareket = new StokHareket
                {
                    StokHareketID = Guid.NewGuid(),
                    UrunID = viewModel.UrunID,
                    DepoID = viewModel.DepoID,
                    Miktar = viewModel.Miktar, // Giriş olduğu için miktar pozitif
                    HareketTuru = StokHareketTipi.Giris,
                    Aciklama = viewModel.Aciklama ?? "Manuel stok girişi",
                    Tarih = DateTime.Now, // Default olarak şu anki zamanı kullan
                    OlusturmaTarihi = DateTime.Now,
                    Silindi = false,
                    BirimFiyat = viewModel.BirimFiyat
                };

                await _unitOfWork.StokHareketRepository.AddAsync(stokHareket);

                // Ürün stok miktarını güncelle
                urun.StokMiktar += viewModel.Miktar;
                urun.GuncellemeTarihi = DateTime.Now;
                
                _unitOfWork.UrunRepository.Update(urun);
                
                await _unitOfWork.SaveChangesAsync();
                
                _logger.LogInformation($"Stok girişi başarıyla tamamlandı. StokHareketID: {stokHareket.StokHareketID}");
                return true;
            }, "StokGirisYapAsync", viewModel.UrunID, viewModel.Miktar);
        }

        /// <summary>
        /// Manuel stok çıkışı yapar
        /// </summary>
        public async Task<bool> StokCikisYapAsync(StokCikisViewModel viewModel)
        {
            return await ExecuteInTransactionAsync(async () =>
            {
                _logger.LogInformation($"Stok çıkışı yapılıyor. UrunID: {viewModel.UrunID}, Miktar: {viewModel.Miktar}");

                // Ürünü kontrol et
                var urun = await _unitOfWork.UrunRepository.GetByIdAsync(viewModel.UrunID);
                if (urun == null)
                {
                    _logger.LogWarning($"Stok çıkışı yapılamadı: Ürün bulunamadı. UrunID: {viewModel.UrunID}");
                    return false;
                }

                // Mevcut stok miktarını kontrol et
                var mevcutStok = await GetDinamikStokMiktari(viewModel.UrunID, viewModel.DepoID);
                if (mevcutStok < viewModel.Miktar)
                {
                    _logger.LogWarning($"Stok çıkışı yapılamadı: Yeterli stok yok. UrunID: {viewModel.UrunID}, Miktar: {viewModel.Miktar}, Mevcut Stok: {mevcutStok}");
                    return false;
                }

                // Stok hareketi oluştur
                var stokHareket = new StokHareket
                {
                    StokHareketID = Guid.NewGuid(),
                    UrunID = viewModel.UrunID,
                    DepoID = viewModel.DepoID,
                    Miktar = -viewModel.Miktar, // Çıkış olduğu için miktar negatif
                    HareketTuru = StokHareketTipi.Cikis,
                    Aciklama = viewModel.Aciklama ?? "Manuel stok çıkışı",
                    Tarih = DateTime.Now, // Default olarak şu anki zamanı kullan
                    OlusturmaTarihi = DateTime.Now,
                    Silindi = false,
                    BirimFiyat = 0 // StokCikisViewModel'de BirimFiyat özelliği olmadığı için 0 kullanıyoruz
                };

                await _unitOfWork.StokHareketRepository.AddAsync(stokHareket);

                // Ürün stok miktarını güncelle
                urun.StokMiktar -= viewModel.Miktar;
                urun.GuncellemeTarihi = DateTime.Now;
                
                _unitOfWork.UrunRepository.Update(urun);
                
                await _unitOfWork.SaveChangesAsync();
                
                _logger.LogInformation($"Stok çıkışı başarıyla tamamlandı. StokHareketID: {stokHareket.StokHareketID}");
                return true;
            }, "StokCikisYapAsync", viewModel.UrunID, viewModel.Miktar);
        }

        /// <summary>
        /// Depolar arası stok transferi yapar
        /// </summary>
        public async Task<bool> StokTransferYapAsync(StokTransferViewModel viewModel)
        {
            return await ExecuteInTransactionAsync(async () =>
            {
                _logger.LogInformation($"Stok transferi yapılıyor. UrunID: {viewModel.UrunID}, KaynakDepoID: {viewModel.KaynakDepoID}, HedefDepoID: {viewModel.HedefDepoID}, Miktar: {viewModel.Miktar}");

                // Ürünü kontrol et
                var urun = await _unitOfWork.UrunRepository.GetByIdAsync(viewModel.UrunID);
                if (urun == null)
                {
                    _logger.LogWarning($"Stok transferi yapılamadı: Ürün bulunamadı. UrunID: {viewModel.UrunID}");
                    return false;
                }

                // Kaynak depodaki stok miktarını kontrol et
                var kaynakStok = await GetDinamikStokMiktari(viewModel.UrunID, viewModel.KaynakDepoID);
                if (kaynakStok < viewModel.Miktar)
                {
                    _logger.LogWarning($"Stok transferi yapılamadı: Kaynak depoda yeterli stok yok. UrunID: {viewModel.UrunID}, Miktar: {viewModel.Miktar}, Mevcut Stok: {kaynakStok}");
                    return false;
                }

                // Çıkış hareketi
                var cikisHareket = new StokHareket
                {
                    StokHareketID = Guid.NewGuid(),
                    UrunID = viewModel.UrunID,
                    DepoID = viewModel.KaynakDepoID,
                    Miktar = -viewModel.Miktar, // Çıkış olduğu için miktar negatif
                    HareketTuru = StokHareketTipi.Cikis,
                    Aciklama = viewModel.Aciklama ?? $"Transfer çıkış: {viewModel.HedefDepoID} nolu depoya",
                    Tarih = DateTime.Now, // Default olarak şu anki zamanı kullan
                    OlusturmaTarihi = DateTime.Now,
                    ReferansNo = viewModel.ReferansNo,
                    Silindi = false,
                    BirimFiyat = 0 // StokTransferViewModel'de BirimFiyat özelliği olmadığı için 0 kullanıyoruz
                };

                // Giriş hareketi
                var girisHareket = new StokHareket
                {
                    StokHareketID = Guid.NewGuid(),
                    UrunID = viewModel.UrunID,
                    DepoID = viewModel.HedefDepoID,
                    Miktar = viewModel.Miktar, // Giriş olduğu için miktar pozitif
                    HareketTuru = StokHareketTipi.Giris,
                    Aciklama = viewModel.Aciklama ?? $"Transfer giriş: {viewModel.KaynakDepoID} nolu depodan",
                    Tarih = DateTime.Now, // Default olarak şu anki zamanı kullan
                    OlusturmaTarihi = DateTime.Now,
                    ReferansNo = viewModel.ReferansNo,
                    Silindi = false,
                    BirimFiyat = 0 // StokTransferViewModel'de BirimFiyat özelliği olmadığı için 0 kullanıyoruz
                };

                await _unitOfWork.StokHareketRepository.AddAsync(cikisHareket);
                await _unitOfWork.StokHareketRepository.AddAsync(girisHareket);

                // Ürün stok miktarını güncelle
                urun.StokMiktar += viewModel.Miktar;
                urun.GuncellemeTarihi = DateTime.Now;
                
                _unitOfWork.UrunRepository.Update(urun);
                
                await _unitOfWork.SaveChangesAsync();
                
                _logger.LogInformation($"Stok transferi başarıyla tamamlandı. Çıkış StokHareketID: {cikisHareket.StokHareketID}, Giriş StokHareketID: {girisHareket.StokHareketID}");
                return true;
            }, "StokTransferYapAsync", viewModel.UrunID, viewModel.KaynakDepoID, viewModel.HedefDepoID, viewModel.Miktar);
        }

        /// <summary>
        /// Stok sayımı sonucunda stok düzeltme işlemi yapar
        /// </summary>
        public async Task<bool> StokSayimiYapAsync(StokSayimViewModel viewModel)
        {
            return await ExecuteInTransactionAsync(async () =>
            {
                if (viewModel.UrunListesi == null || !viewModel.UrunListesi.Any())
                {
                    _logger.LogWarning("Stok sayımı yapılamadı: Ürün listesi boş veya null.");
                    return false;
                }

                var basarili = true;
                
                foreach (var urun in viewModel.UrunListesi)
                {
                    var fark = urun.SayimMiktari - urun.SistemStokMiktari;
                    
                    // Fark yoksa bu ürün için işlem yapma, diğerine geç
                    if (fark == 0) continue;
                    
                    // Stok düzeltme hareketi oluştur
                    var stokHareket = new StokHareket
                    {
                        StokHareketID = Guid.NewGuid(),
                        UrunID = urun.UrunID,
                        DepoID = viewModel.DepoID,
                        Miktar = fark, // Fark pozitifse giriş, negatifse çıkış
                        HareketTuru = fark > 0 ? StokHareketTipi.Giris : StokHareketTipi.Cikis,
                        Aciklama = viewModel.Aciklama ?? $"Stok sayımı düzeltmesi. Sistem: {urun.SistemStokMiktari}, Sayım: {urun.SayimMiktari}, Fark: {fark}",
                        Tarih = DateTime.Now, // Default olarak şu anki zamanı kullan
                        OlusturmaTarihi = DateTime.Now,
                        Silindi = false,
                        BirimFiyat = 0, // Sayımda BirimFiyat kullanmıyoruz
                        ReferansNo = viewModel.ReferansNo
                    };
                    
                    await _unitOfWork.StokHareketRepository.AddAsync(stokHareket);
                    _logger.LogInformation($"Stok sayımı düzeltmesi oluşturuldu: UrunID={urun.UrunID}, UrunKodu={urun.UrunKodu}, Fark={fark}");
                }
                
                await _unitOfWork.SaveChangesAsync();
                _logger.LogInformation($"Stok sayımı başarıyla tamamlandı. Depo: {viewModel.DepoID}");
                return basarili;
            }, "StokSayimiYapAsync", viewModel.DepoID);
        }

        /// <summary>
        /// Para birimi sembolünü döndürür
        /// </summary>
        public string GetParaBirimiSembol(string paraBirimiKodu)
        {
            return paraBirimiKodu switch
            {
                "TRY" => "₺",
                "USD" => "$",
                "EUR" => "€",
                "GBP" => "£",
                _ => paraBirimiKodu
            };
        }
    }
} 