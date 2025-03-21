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
                .Where(c => c.Aktif && !c.SoftDelete)
                .OrderBy(c => c.CariAdi)
                .ToListAsync();

            return new SelectList(cariler, "CariID", "CariAdi", selectedCariId);
        }

        public async Task<SelectList> GetUrunSelectListAsync(Guid? selectedUrunId = null)
        {
            var urunler = await _context.Urunler
                .Where(u => u.Aktif && !u.SoftDelete)
                .OrderBy(u => u.UrunAdi)
                .ToListAsync();

            return new SelectList(urunler, "UrunID", "UrunAdi", selectedUrunId);
        }

        public async Task<SelectList> GetFaturaSelectListAsync(Guid? selectedFaturaId = null)
        {
            var faturalar = await _context.Faturalar
                .Where(f => f.Aktif == true && !f.SoftDelete)
                .OrderByDescending(f => f.FaturaTarihi)
                .ToListAsync();

            return new SelectList(faturalar, "FaturaID", "FaturaNumarasi", selectedFaturaId);
        }

        public async Task<SelectList> GetDepoSelectListAsync(Guid? selectedDepoId = null)
        {
            var depolar = await _context.Depolar
                .Where(d => d.Aktif && !d.SoftDelete)
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
    }
} 