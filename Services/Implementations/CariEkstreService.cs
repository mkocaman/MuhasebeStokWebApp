using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using MuhasebeStokWebApp.Data;
using MuhasebeStokWebApp.Data.Repositories;
using MuhasebeStokWebApp.Services.Interfaces;
using MuhasebeStokWebApp.Services.ParaBirimiModulu;
using MuhasebeStokWebApp.ViewModels.Cari;

namespace MuhasebeStokWebApp.Services.Implementations
{
    public class CariEkstreService : ICariEkstreService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<CariEkstreService> _logger;
        private readonly ParaBirimiModulu.IParaBirimiService _paraBirimiService;
        private readonly IDovizKuruService _dovizKuruService;

        public CariEkstreService(
            IUnitOfWork unitOfWork,
            ILogger<CariEkstreService> logger,
            ParaBirimiModulu.IParaBirimiService paraBirimiService,
            IDovizKuruService dovizKuruService)
        {
            _unitOfWork = unitOfWork;
            _logger = logger;
            _paraBirimiService = paraBirimiService;
            _dovizKuruService = dovizKuruService;
        }

        public async Task<CariEkstreRaporViewModel> GetCariEkstreAsync(
            Guid cariId, 
            DateTime? baslangicTarihi = null, 
            DateTime? bitisTarihi = null, 
            Guid? paraBirimiId = null)
        {
            try
            {
                // Varsayılan tarih aralığı - son 1 ay
                var startDate = baslangicTarihi ?? DateTime.Now.AddMonths(-1);
                var endDate = bitisTarihi ?? DateTime.Now;

                // Cari bilgilerini al
                var cari = await _unitOfWork.CariRepository.GetByIdAsync(cariId);
                if (cari == null)
                {
                    throw new Exception($"Cari bulunamadı: ID={cariId}");
                }

                // Para birimi belirtilmemişse carinin varsayılan para birimini kullan
                if (!paraBirimiId.HasValue && cari.VarsayilanParaBirimiId.HasValue)
                {
                    paraBirimiId = cari.VarsayilanParaBirimiId;
                }

                // Ekstre modelini oluştur
                var model = new CariEkstreRaporViewModel
                {
                    Id = cari.CariID,
                    CariAdi = cari.Ad,
                    CariKodu = cari.CariKodu,
                    VergiNo = cari.VergiNo,
                    Adres = cari.Adres,
                    BaslangicTarihi = startDate,
                    BitisTarihi = endDate,
                    ParaBirimiId = paraBirimiId,
                    RaporTarihi = DateTime.Now
                };

                // Seçili para birimini ayarla
                await SetParaBirimiAsync(model, paraBirimiId);

                // Cari hareketlerini getir ve filtrele
                var (bakiye, hareketler) = await GetFiltrelenmisHareketlerAsync(cariId, startDate, endDate);

                // Başlangıç bakiyesi
                model.BaslangicBakiye = await HesaplaBakiyeAsync(cariId, startDate.AddDays(-1));

                // Bakiyeyi hesapla ve her hareket için model oluştur
                decimal toplamBakiye = model.BaslangicBakiye;
                decimal toplamBorc = 0;
                decimal toplamAlacak = 0;

                foreach (var hareket in hareketler.OrderBy(h => h.Tarih))
                {
                    var viewModel = new CariEkstreRaporViewModel.CariEkstreHareketViewModel
                    {
                        CariHareketID = hareket.CariHareketID,
                        CariID = hareket.CariID,
                        Tarih = hareket.Tarih,
                        IslemTuru = hareket.HareketTuru,
                        Aciklama = hareket.Aciklama,
                        EvrakNo = hareket.ReferansNo ?? "",
                        VadeTarihi = hareket.VadeTarihi
                    };

                    // Hareket türüne göre bakiyeyi güncelle
                    if (hareket.HareketTuru == "Alacak" || hareket.HareketTuru == "Tahsilat")
                    {
                        viewModel.Alacak = hareket.Tutar;
                        viewModel.Borc = 0;
                        toplamBakiye -= hareket.Tutar;
                        toplamAlacak += hareket.Tutar;
                    }
                    else if (hareket.HareketTuru == "Borç" || hareket.HareketTuru == "Ödeme")
                    {
                        viewModel.Alacak = 0;
                        viewModel.Borc = hareket.Tutar;
                        toplamBakiye += hareket.Tutar;
                        toplamBorc += hareket.Tutar;
                    }

                    viewModel.Bakiye = toplamBakiye;
                    model.Hareketler.Add(viewModel);
                }

                // Toplamları ayarla
                model.ToplamBorc = toplamBorc;
                model.ToplamAlacak = toplamAlacak;
                model.Bakiye = toplamBakiye;

                return model;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Cari ekstre oluşturulurken hata: {Message}", ex.Message);
                throw;
            }
        }

        public async Task<decimal> HesaplaBakiyeAsync(Guid cariId, DateTime tarih)
        {
            try
            {
                // Cari'yi kontrol et
                var cari = await _unitOfWork.CariRepository.GetByIdAsync(cariId);
                if (cari == null)
                {
                    throw new Exception($"Cari bulunamadı: ID={cariId}");
                }

                // Belirtilen tarihe kadar olan hareketleri getir
                var hareketler = await _unitOfWork.CariHareketRepository.GetAll()
                    .Where(ch => ch.CariID == cariId && !ch.Silindi && ch.Tarih.Date <= tarih.Date)
                    .ToListAsync();

                // Bakiyeyi hesapla
                decimal bakiye = 0;

                foreach (var hareket in hareketler.OrderBy(h => h.Tarih))
                {
                    if (hareket.HareketTuru == "Alacak" || hareket.HareketTuru == "Tahsilat")
                    {
                        bakiye -= hareket.Tutar;
                    }
                    else if (hareket.HareketTuru == "Borç" || hareket.HareketTuru == "Ödeme")
                    {
                        bakiye += hareket.Tutar;
                    }
                }

                return bakiye;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Cari bakiye hesaplanırken hata: {Message}", ex.Message);
                throw;
            }
        }

        public async Task<List<CariEkstreRaporViewModel.CariEkstreHareketViewModel>> GetCariHareketlerAsync(
            Guid cariId,
            DateTime baslangicTarihi,
            DateTime bitisTarihi)
        {
            try
            {
                // Cari hareketlerini getir ve filtrele
                var hareketler = await _unitOfWork.CariHareketRepository.GetAll()
                    .Where(ch => ch.CariID == cariId && !ch.Silindi && 
                           ch.Tarih.Date >= baslangicTarihi.Date && 
                           ch.Tarih.Date <= bitisTarihi.Date)
                    .OrderBy(ch => ch.Tarih)
                    .ToListAsync();

                // Bakiyeyi hesapla
                decimal toplamBakiye = await HesaplaBakiyeAsync(cariId, baslangicTarihi.AddDays(-1));
                var sonucListesi = new List<CariEkstreRaporViewModel.CariEkstreHareketViewModel>();

                foreach (var hareket in hareketler)
                {
                    var viewModel = new CariEkstreRaporViewModel.CariEkstreHareketViewModel
                    {
                        CariHareketID = hareket.CariHareketID,
                        CariID = hareket.CariID,
                        Tarih = hareket.Tarih,
                        IslemTuru = hareket.HareketTuru,
                        Aciklama = hareket.Aciklama,
                        EvrakNo = hareket.ReferansNo ?? "",
                        VadeTarihi = hareket.VadeTarihi
                    };

                    // Hareket türüne göre bakiyeyi güncelle
                    if (hareket.HareketTuru == "Alacak" || hareket.HareketTuru == "Tahsilat")
                    {
                        viewModel.Alacak = hareket.Tutar;
                        viewModel.Borc = 0;
                        toplamBakiye -= hareket.Tutar;
                    }
                    else if (hareket.HareketTuru == "Borç" || hareket.HareketTuru == "Ödeme")
                    {
                        viewModel.Alacak = 0;
                        viewModel.Borc = hareket.Tutar;
                        toplamBakiye += hareket.Tutar;
                    }

                    viewModel.Bakiye = toplamBakiye;
                    sonucListesi.Add(viewModel);
                }

                return sonucListesi;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Cari hareketleri alınırken hata: {Message}", ex.Message);
                throw;
            }
        }

        public async Task<CariEkstreRaporViewModel> GetCariEkstreRaporAsync(
            Guid cariId,
            DateTime baslangicTarihi,
            DateTime bitisTarihi,
            Guid? paraBirimiId = null)
        {
            try
            {
                // Cari ekstreyi getir
                var model = await GetCariEkstreAsync(cariId, baslangicTarihi, bitisTarihi, paraBirimiId);

                // Özet bilgileri oluştur
                model.Ozet = new List<CariEkstreRaporViewModel.CariEkstreHareketViewModel>();

                // Açılış bakiyesi özeti
                var acilisBakiyeGosterimi = new CariEkstreRaporViewModel.CariEkstreHareketViewModel
                {
                    Tarih = baslangicTarihi.AddDays(-1),
                    IslemTuru = "Açılış Bakiyesi",
                    Aciklama = "Dönem Başı Bakiyesi",
                    Bakiye = model.BaslangicBakiye
                };

                if (model.BaslangicBakiye > 0)
                {
                    acilisBakiyeGosterimi.Borc = model.BaslangicBakiye;
                    acilisBakiyeGosterimi.Alacak = 0;
                }
                else
                {
                    acilisBakiyeGosterimi.Borc = 0;
                    acilisBakiyeGosterimi.Alacak = Math.Abs(model.BaslangicBakiye);
                }

                model.Ozet.Add(acilisBakiyeGosterimi);

                // Dönem hareketleri özeti
                var donemOzeti = new CariEkstreRaporViewModel.CariEkstreHareketViewModel
                {
                    IslemTuru = "Dönem Hareketleri",
                    Borc = model.ToplamBorc,
                    Alacak = model.ToplamAlacak,
                    Bakiye = model.Bakiye
                };

                model.Ozet.Add(donemOzeti);

                return model;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Cari ekstre raporu oluşturulurken hata: {Message}", ex.Message);
                throw;
            }
        }

        #region Yardımcı Metodlar

        private async Task<(decimal Bakiye, List<Data.Entities.CariHareket> Hareketler)> GetFiltrelenmisHareketlerAsync(
            Guid cariId, 
            DateTime baslangicTarihi, 
            DateTime bitisTarihi)
        {
            // Tüm hareketleri getir
            var tumHareketler = await _unitOfWork.CariHareketRepository.GetAll()
                .Where(ch => ch.CariID == cariId && !ch.Silindi)
                .ToListAsync();

            // Tarih aralığını filtrele
            var filtrelenmisHareketler = tumHareketler
                .Where(h => h.Tarih.Date >= baslangicTarihi.Date && h.Tarih.Date <= bitisTarihi.Date)
                .OrderBy(h => h.Tarih)
                .ToList();

            // Başlangıç bakiyesini hesapla
            decimal bakiye = 0;
            var oncekiHareketler = tumHareketler
                .Where(h => h.Tarih.Date < baslangicTarihi.Date)
                .OrderBy(h => h.Tarih);

            foreach (var hareket in oncekiHareketler)
            {
                if (hareket.HareketTuru == "Alacak" || hareket.HareketTuru == "Tahsilat")
                {
                    bakiye -= hareket.Tutar;
                }
                else if (hareket.HareketTuru == "Borç" || hareket.HareketTuru == "Ödeme")
                {
                    bakiye += hareket.Tutar;
                }
            }

            return (bakiye, filtrelenmisHareketler);
        }

        private async Task SetParaBirimiAsync(CariEkstreRaporViewModel model, Guid? paraBirimiId)
        {
            // Tüm para birimleri listesini al
            var paraBirimleri = await _paraBirimiService.GetAllParaBirimleriAsync(true);
            model.ParaBirimleri = paraBirimleri.Cast<object>().ToList();

            // Seçili para birimini bul
            if (paraBirimiId.HasValue)
            {
                var seciliParaBirimi = paraBirimleri.FirstOrDefault(p => p.ParaBirimiID == paraBirimiId);
                if (seciliParaBirimi != null)
                {
                    model.SeciliParaBirimi = seciliParaBirimi;
                    model.ParaBirimiKodu = seciliParaBirimi.Kod;
                    model.ParaBirimiSembolu = seciliParaBirimi.Sembol;

                    // Varsayılan para birimi ID'sini ayarla
                    model.VarsayilanParaBirimiId = paraBirimiId;

                    // Döviz kuru bilgisini al
                    var dovizKuru = await _dovizKuruService.GetGuncelKurAsync(seciliParaBirimi.Kod, "TRY");
                    if (dovizKuru > 0)
                    {
                        model.DovizKuru = dovizKuru;
                        model.DovizKuruTarihi = DateTime.Now;
                    }
                }
            }
            else
            {
                // Varsayılan TRY para birimi
                var tryParaBirimi = paraBirimleri.FirstOrDefault(p => p.Kod == "TRY");
                if (tryParaBirimi != null)
                {
                    model.SeciliParaBirimi = tryParaBirimi;
                    model.ParaBirimiKodu = "TRY";
                    model.ParaBirimiSembolu = "₺";
                    model.VarsayilanParaBirimiId = tryParaBirimi.ParaBirimiID;
                }
            }
        }

        #endregion
    }
} 