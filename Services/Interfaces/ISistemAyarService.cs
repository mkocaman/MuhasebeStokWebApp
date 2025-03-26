using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using MuhasebeStokWebApp.ViewModels.SistemAyar;
using MuhasebeStokWebApp.Data.Entities;

namespace MuhasebeStokWebApp.Services.Interfaces
{
    public interface ISistemAyarService
    {
        Task<IEnumerable<KeyValuePair<string, string>>> GetAllAsync();
        Task<string> GetSettingAsync(string key);
        Task SaveSettingAsync(string key, string value);
        Task SaveEmailSettingsAsync(EmailAyarlariViewModel model);
        Task SaveNotificationSettingsAsync(BildirimAyarlariViewModel model);
        Task SaveLanguageSettingsAsync(DilAyarlariViewModel model);
        
        // Döviz modülü için gerekli metotlar
        Task<string> GetAnaDovizKoduAsync();
        Task<DateTime> GetSonDovizGuncellemeTarihiAsync();
        Task<bool> UpdateSonDovizGuncellemeTarihiAsync(DateTime yeniTarih);
        Task<SistemAyarlari> GetAktifSistemAyariAsync();
        
        // Eski SistemAyar.ISistemAyarService dosyasından alınan metotlar 
        Task<List<SistemAyarlari>> GetAllSistemAyarlariAsync();
        Task<SistemAyarlari> GetSistemAyariByIdAsync(int sistemAyariId);
        Task<SistemAyarlari> GetSistemAyariByDovizKoduAsync(string dovizKodu);
        Task<SistemAyarlari> UpdateSistemAyariAsync(SistemAyarlari sistemAyari);
        Task<bool> UpdateAktifSistemAyariAsync(int sistemAyariId);
        Task<bool> UpdateSistemAyariDurumAsync(int sistemAyariId, bool aktif);
        Task<bool> DeleteSistemAyariAsync(int sistemAyariId);
        Task<SistemAyarlari> AddSistemAyariAsync(SistemAyarlari sistemAyari);
        Task<bool> VarsayilanSistemAyarlariniEkleAsync();
        Task<int> GetDovizGuncellemeZamaniAsync();
    }
} 