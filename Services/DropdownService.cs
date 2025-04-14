using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using MuhasebeStokWebApp.Data;
using MuhasebeStokWebApp.Data.Entities;

namespace MuhasebeStokWebApp.Services
{
    public class DropdownService : IDropdownService
    {
        private readonly ApplicationDbContext _context;

        public DropdownService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<SelectList> GetCariSelectListAsync(Guid? selectedCariId = null)
        {
            var cariler = await _context.Cariler
                .Where(c => c.AktifMi && !c.Silindi)
                .OrderBy(c => c.Ad)
                .ToListAsync();

            return new SelectList(cariler, "Id", "Ad", selectedCariId);
        }

        public async Task<SelectList> GetUrunSelectListAsync(Guid? selectedUrunId = null)
        {
            var urunler = await _context.Urunler
                .Where(u => u.Aktif && !u.Silindi)
                .OrderBy(u => u.UrunAdi)
                .ToListAsync();

            return new SelectList(urunler, "UrunID", "UrunAdi", selectedUrunId);
        }

        public async Task<SelectList> GetFaturaSelectListAsync(Guid? selectedFaturaId = null)
        {
            var faturalar = await _context.Faturalar
                .Where(f => f.Aktif == true && !f.Silindi)
                .OrderByDescending(f => f.FaturaTarihi)
                .ToListAsync();

            return new SelectList(faturalar, "FaturaID", "FaturaNumarasi", selectedFaturaId);
        }

        public async Task<SelectList> GetDepoSelectListAsync(Guid? selectedDepoId = null)
        {
            var depolar = await _context.Depolar
                .Where(d => d.Aktif && !d.Silindi)
                .OrderBy(d => d.DepoAdi)
                .ToListAsync();

            return new SelectList(depolar, "DepoID", "DepoAdi", selectedDepoId);
        }

        public SelectList GetIrsaliyeTuruSelectList(string selectedTur = null)
        {
            var turler = new List<string> { "Standart", "Giriş", "Çıkış" };
            return new SelectList(turler, selectedTur);
        }

        public SelectList GetDurumSelectList(string selectedDurum = null)
        {
            var durumlar = new List<string> { "Açık", "Kapalı", "İptal" };
            return new SelectList(durumlar, selectedDurum);
        }

        public async Task<SelectList> GetCariSelectList(Guid? selectedCariId = null)
        {
            var cariler = await _context.Cariler
                .Where(c => c.AktifMi && !c.Silindi)
                .OrderBy(c => c.Ad)
                .ToListAsync();

            return new SelectList(cariler, "CariID", "Ad", selectedCariId);
        }
        
        public async Task<SelectList> GetSozlesmeSelectListByCariId(Guid cariId, Guid? selectedSozlesmeId = null)
        {
            var sozlesmeler = await _context.Sozlesmeler
                .Where(s => s.CariID == cariId && s.AktifMi && !s.Silindi)
                .OrderByDescending(s => s.SozlesmeTarihi)
                .ToListAsync();

            return new SelectList(sozlesmeler, "SozlesmeID", "SozlesmeNo", selectedSozlesmeId);
        }

        public async Task<Dictionary<string, SelectList>> PrepareCommonDropdownsAsync(Guid? selectedCariId = null, Guid? selectedUrunId = null)
        {
            var dropdowns = new Dictionary<string, SelectList>
            {
                ["CariListesi"] = await GetCariSelectListAsync(selectedCariId),
                ["UrunListesi"] = await GetUrunSelectListAsync(selectedUrunId)
            };
            return dropdowns;
        }

        public async Task<Dictionary<string, SelectList>> PrepareIrsaliyeDropdownsAsync(
            Guid? selectedCariId = null, 
            Guid? selectedUrunId = null, 
            string selectedTur = null)
        {
            var dropdowns = await PrepareCommonDropdownsAsync(selectedCariId, selectedUrunId);
            dropdowns["IrsaliyeTurleri"] = GetIrsaliyeTuruSelectList(selectedTur);
            dropdowns["Durumlar"] = GetDurumSelectList();
            return dropdowns;
        }

        public async Task<Dictionary<string, SelectList>> PrepareFaturaDropdownsAsync(
            Guid? selectedCariId = null, 
            Guid? selectedDepoId = null)
        {
            var dropdowns = new Dictionary<string, SelectList>
            {
                ["CariListesi"] = await GetCariSelectListAsync(selectedCariId),
                ["DepoListesi"] = await GetDepoSelectListAsync(selectedDepoId),
                ["Durumlar"] = GetDurumSelectList()
            };
            return dropdowns;
        }

        public async Task<Dictionary<string, object>> PrepareViewBagAsync(
            string controller,
            string action,
            Dictionary<string, object> additionalData = null)
        {
            var viewBagData = new Dictionary<string, object>();

            switch ($"{controller}/{action}".ToLower())
            {
                case "irsaliye/create":
                case "irsaliye/edit":
                    var irsaliyeDropdowns = await PrepareIrsaliyeDropdownsAsync();
                    foreach (var item in irsaliyeDropdowns)
                    {
                        viewBagData[item.Key] = item.Value;
                    }
                    break;

                case "fatura/create":
                case "fatura/edit":
                    var faturaDropdowns = await PrepareFaturaDropdownsAsync();
                    foreach (var item in faturaDropdowns)
                    {
                        viewBagData[item.Key] = item.Value;
                    }
                    break;

                default:
                    var commonDropdowns = await PrepareCommonDropdownsAsync();
                    foreach (var item in commonDropdowns)
                    {
                        viewBagData[item.Key] = item.Value;
                    }
                    break;
            }

            if (additionalData != null)
            {
                foreach (var item in additionalData)
                {
                    viewBagData[item.Key] = item.Value;
                }
            }

            return viewBagData;
        }
    }
} 