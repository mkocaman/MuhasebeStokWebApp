using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using MuhasebeStokWebApp.Data.Entities;
using MuhasebeStokWebApp.ViewModels.DovizKuru;

namespace MuhasebeStokWebApp.Services
{
    public interface IDovizKuruService
    {
        Task<decimal> GetGuncelKurAsync(string paraBirimiKodu, string bazParaBirimiKodu = "USD");
        Task<decimal> GetKurByTarihAsync(string paraBirimiKodu, string bazParaBirimiKodu, DateTime tarih);
        Task<KurDegeri> GetKurDegeriByIdAsync(Guid kurDegeriId);
        Task<List<KurDegeri>> GetParaBirimiKurDegerleriAsync(Guid paraBirimiId);
        Task<List<KurDegeri>> GetGuncelKurlarAsync();
        Task<decimal> ParaBirimiCevirAsync(decimal tutar, string kaynakParaBirimiKodu, string hedefParaBirimiKodu, DateTime? tarih = null);
        Task<KurDegeri> KurEkleAsync(Guid paraBirimiId, decimal alisDegeri, decimal satisDegeri, string kaynak, DateTime tarih);
        Task<bool> DeleteKurDegeriAsync(Guid kurDegeriId);
        Task<List<KurDegeri>> KurlariGuncelleAsync();
        Task<KurDegeri> AddDovizKuruManuelAsync(DovizKuruEkleViewModel viewModel);
        Task<List<KurDegeri>> GetLatestRatesAsync(int count);
    }
} 