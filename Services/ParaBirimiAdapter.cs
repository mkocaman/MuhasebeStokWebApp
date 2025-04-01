using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Linq;
using MuhasebeStokWebApp.Data.Entities.DovizModulu;
using MuhasebeStokWebApp.Services.Interfaces;
using MuhasebeStokWebApp.Services.ParaBirimiModulu;
using Microsoft.Extensions.Logging;

namespace MuhasebeStokWebApp.Services
{
    /// <summary>
    /// Services.Interfaces.IParaBirimiService arayüzünü 
    /// Services.ParaBirimiModulu.IParaBirimiService uygulamasına adapte eder.
    /// </summary>
    public class ParaBirimiAdapter : MuhasebeStokWebApp.Services.Interfaces.IParaBirimiService
    {
        private readonly MuhasebeStokWebApp.Services.ParaBirimiModulu.IParaBirimiService _paraBirimiService;
        private readonly ILogger<ParaBirimiAdapter> _logger;

        public ParaBirimiAdapter(
            MuhasebeStokWebApp.Services.ParaBirimiModulu.IParaBirimiService paraBirimiService,
            ILogger<ParaBirimiAdapter> logger)
        {
            _paraBirimiService = paraBirimiService;
            _logger = logger;
        }

        public async Task<ParaBirimi> AddParaBirimiAsync(ParaBirimi paraBirimi)
        {
            _logger.LogWarning("DovizModulu.ParaBirimi -> ParaBirimiModulu.ParaBirimi adaptasyonu yapılıyor");
            // İmplementasyon gerektiğinde yapılacak
            throw new NotImplementedException();
        }

        public async Task<DovizIliski> AddParaBirimiIliskiAsync(DovizIliski dovizIliski)
        {
            _logger.LogWarning("DovizModulu.DovizIliski -> ParaBirimiModulu.ParaBirimiIliski adaptasyonu yapılıyor");
            // İmplementasyon gerektiğinde yapılacak
            throw new NotImplementedException();
        }

        public async Task<bool> DeleteParaBirimiAsync(Guid paraBirimiId)
        {
            return await _paraBirimiService.DeleteParaBirimiAsync(paraBirimiId);
        }

        public async Task<bool> DeleteParaBirimiIliskiAsync(Guid iliskiId)
        {
            return await _paraBirimiService.DeleteParaBirimiIliskiAsync(iliskiId);
        }

        public async Task<List<DovizIliski>> GetAktifParaBirimiIliskileriAsync()
        {
            _logger.LogWarning("ParaBirimiModulu.ParaBirimiIliski -> DovizModulu.DovizIliski adaptasyonu yapılıyor");
            // İmplementasyon gerektiğinde yapılacak
            return new List<DovizIliski>();
        }

        public async Task<List<ParaBirimi>> GetAktifParaBirimleriAsync()
        {
            var paraBirimleri = await _paraBirimiService.GetAllParaBirimleriAsync(true);
            return paraBirimleri.Select(MapToParaBirimi).ToList();
        }

        public async Task<List<DovizIliski>> GetAllParaBirimiIliskileriAsync()
        {
            _logger.LogWarning("ParaBirimiModulu.ParaBirimiIliski -> DovizModulu.DovizIliski adaptasyonu yapılıyor");
            // İmplementasyon gerektiğinde yapılacak
            return new List<DovizIliski>();
        }

        public async Task<List<ParaBirimi>> GetAllParaBirimleriAsync()
        {
            var paraBirimleri = await _paraBirimiService.GetAllParaBirimleriAsync(false);
            return paraBirimleri.Select(MapToParaBirimi).ToList();
        }

        public async Task<DovizIliski> GetIliskiByParaBirimleriAsync(Guid kaynakId, Guid hedefId)
        {
            _logger.LogWarning("ParaBirimiModulu.ParaBirimiIliski -> DovizModulu.DovizIliski adaptasyonu yapılıyor");
            // İmplementasyon gerektiğinde yapılacak
            return null;
        }

        public async Task<DovizIliski> GetParaBirimiIliskiByIdAsync(Guid iliskiId)
        {
            _logger.LogWarning("ParaBirimiModulu.ParaBirimiIliski -> DovizModulu.DovizIliski adaptasyonu yapılıyor");
            // İmplementasyon gerektiğinde yapılacak
            return null;
        }

        public async Task<List<DovizIliski>> GetParaBirimiIliskileriAsync(Guid paraBirimiId)
        {
            _logger.LogWarning("ParaBirimiModulu.ParaBirimiIliski -> DovizModulu.DovizIliski adaptasyonu yapılıyor");
            // İmplementasyon gerektiğinde yapılacak
            return new List<DovizIliski>();
        }

        public async Task<ParaBirimi> GetParaBirimiByIdAsync(Guid paraBirimiId)
        {
            var paraBirimi = await _paraBirimiService.GetParaBirimiByIdAsync(paraBirimiId);
            return MapToParaBirimi(paraBirimi);
        }

        public async Task<ParaBirimi> GetParaBirimiByKodAsync(string kod)
        {
            var paraBirimi = await _paraBirimiService.GetParaBirimiByKodAsync(kod);
            return MapToParaBirimi(paraBirimi);
        }

        public async Task<bool> HasParaBirimiIliskiAsync(Guid paraBirimiId)
        {
            var paraBirimiIliskileri = await _paraBirimiService.GetAllParaBirimiIliskileriAsync();
            return paraBirimiIliskileri.Any(x => x.KaynakParaBirimiID == paraBirimiId || x.HedefParaBirimiID == paraBirimiId);
        }

        public Task<bool> UpdateParaBirimiSiralamaAsync(List<Guid> paraBirimiIdSiralama)
        {
            _logger.LogWarning("UpdateParaBirimiSiralamaAsync çağrıldı");
            // İmplementasyon gerektiğinde yapılacak
            return Task.FromResult(true);
        }

        public async Task<ParaBirimi> UpdateParaBirimiAsync(ParaBirimi paraBirimi)
        {
            _logger.LogWarning("DovizModulu.ParaBirimi -> ParaBirimiModulu.ParaBirimi adaptasyonu yapılıyor");
            // İmplementasyon gerektiğinde yapılacak
            throw new NotImplementedException();
        }

        public async Task<DovizIliski> UpdateParaBirimiIliskiAsync(DovizIliski dovizIliski)
        {
            _logger.LogWarning("DovizModulu.DovizIliski -> ParaBirimiModulu.ParaBirimiIliski adaptasyonu yapılıyor");
            // İmplementasyon gerektiğinde yapılacak
            throw new NotImplementedException();
        }

        public async Task<bool> VarsayilanParaBirimleriniEkleAsync()
        {
            _logger.LogWarning("VarsayilanParaBirimleriniEkleAsync çağrıldı");
            // İmplementasyon gerektiğinde yapılacak
            return true;
        }

        private ParaBirimi MapToParaBirimi(Data.Entities.ParaBirimiModulu.ParaBirimi source)
        {
            if (source == null) return null;

            return new ParaBirimi
            {
                ParaBirimiID = source.ParaBirimiID,
                Kod = source.Kod,
                Ad = source.Ad,
                Sembol = source.Sembol ?? "",
                OndalikAyraci = source.OndalikAyraci,
                BinlikAyraci = source.BinlikAyraci,
                OndalikHassasiyet = source.OndalikHassasiyet,
                AnaParaBirimiMi = source.AnaParaBirimiMi,
                Sira = source.Sira,
                Aciklama = source.Aciklama,
                Aktif = source.Aktif,
                Silindi = source.Silindi,
                OlusturmaTarihi = source.OlusturmaTarihi,
                GuncellemeTarihi = source.GuncellemeTarihi,
                OlusturanKullaniciID = source.OlusturanKullaniciID ?? "",
                SonGuncelleyenKullaniciID = source.SonGuncelleyenKullaniciID ?? ""
            };
        }
    }
} 