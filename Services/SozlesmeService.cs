using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using MuhasebeStokWebApp.Data;
using MuhasebeStokWebApp.Data.Entities;
using MuhasebeStokWebApp.Data.Repositories;
using MuhasebeStokWebApp.Services.Interfaces;
using MuhasebeStokWebApp.ViewModels.Sozlesme;
using AutoMapper;
using MuhasebeStokWebApp.ViewModels.Fatura;

namespace MuhasebeStokWebApp.Services
{
    public class SozlesmeService : ISozlesmeService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ApplicationDbContext _context;
        private readonly ILogger<SozlesmeService> _logger;
        private readonly IWebHostEnvironment _hostingEnvironment;
        private readonly IMapper _mapper;

        public SozlesmeService(
            IUnitOfWork unitOfWork,
            ApplicationDbContext context,
            ILogger<SozlesmeService> logger,
            IWebHostEnvironment hostingEnvironment,
            IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _context = context;
            _logger = logger;
            _hostingEnvironment = hostingEnvironment;
            _mapper = mapper;
        }

        public async Task<List<Sozlesme>> GetAllSozlesmeAsync()
        {
            try
            {
                return await _context.Sozlesmeler
                    .Include(s => s.Cari)
                    .Where(s => !s.Silindi)
                    .OrderByDescending(s => s.OlusturmaTarihi)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Tüm sözleşmeler getirilirken hata oluştu");
                return new List<Sozlesme>();
            }
        }

        public async Task<Sozlesme> GetSozlesmeByIdAsync(Guid id)
        {
            try
            {
                return await _context.Sozlesmeler
                    .Include(s => s.Cari)
                    .FirstOrDefaultAsync(s => s.SozlesmeID == id && !s.Silindi);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "ID ile sözleşme getirilirken hata oluştu: {Id}", id);
                return null;
            }
        }

        public async Task<List<Sozlesme>> GetSozlesmesByFiltersAsync(SozlesmeSearchViewModel filters)
        {
            try
            {
                var query = _context.Sozlesmeler
                    .Include(s => s.Cari)
                    .Where(s => !s.Silindi);

                // Filtrelemeleri uygula
                if (filters.CariID.HasValue && filters.CariID != Guid.Empty)
                {
                    query = query.Where(s => s.CariID == filters.CariID.Value);
                }

                if (!string.IsNullOrWhiteSpace(filters.SozlesmeNo))
                {
                    query = query.Where(s => s.SozlesmeNo.Contains(filters.SozlesmeNo));
                }

                if (filters.BaslangicTarihi.HasValue)
                {
                    query = query.Where(s => s.SozlesmeTarihi >= filters.BaslangicTarihi.Value);
                }

                if (filters.BitisTarihi.HasValue)
                {
                    query = query.Where(s => s.SozlesmeTarihi <= filters.BitisTarihi.Value);
                }

                if (filters.SadeceAktif)
                {
                    query = query.Where(s => s.AktifMi);
                }

                // Sıralama
                query = query.OrderByDescending(s => s.OlusturmaTarihi);

                return await query.ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Filtrelerle sözleşme aranırken hata oluştu");
                return new List<Sozlesme>();
            }
        }

        public async Task<bool> AddSozlesmeAsync(Sozlesme sozlesme)
        {
            try
            {
                await _unitOfWork.SozlesmeRepository.AddAsync(sozlesme);
                await _unitOfWork.SaveAsync();
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Sözleşme eklenirken hata oluştu");
                return false;
            }
        }

        public async Task<bool> UpdateSozlesmeAsync(Sozlesme sozlesme)
        {
            try
            {
                if (sozlesme == null)
                    return false;
                
                sozlesme.GuncellemeTarihi = DateTime.Now;
                
                _unitOfWork.SozlesmeRepository.Update(sozlesme);
                await _unitOfWork.SaveAsync();
                
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Sözleşme güncellenirken hata oluştu: {SozlesmeID}", sozlesme?.SozlesmeID);
                return false;
            }
        }

        public async Task<bool> UpdateSozlesmeAsync(SozlesmeViewModel model)
        {
            try
            {
                var sozlesme = await _unitOfWork.SozlesmeRepository.GetByIdAsync(!string.IsNullOrEmpty(model.SozlesmeID) ? Guid.Parse(model.SozlesmeID) : Guid.Empty);
                
                if (sozlesme == null)
                    return false;
                    
                sozlesme.SozlesmeNo = model.SozlesmeNo;
                sozlesme.SozlesmeTarihi = model.SozlesmeTarihi;
                sozlesme.BitisTarihi = model.BitisTarihi;
                sozlesme.CariID = !string.IsNullOrEmpty(model.CariID) ? Guid.Parse(model.CariID) : Guid.Empty;
                sozlesme.VekaletGeldiMi = model.VekaletGeldiMi;
                sozlesme.ResmiFaturaKesildiMi = model.ResmiFaturaKesildiMi;
                
                // Dosya yollarını sadece yeni değerler varsa güncelle
                if (model.SozlesmeBelgesi != null)
                {
                    sozlesme.SozlesmeDosyaYolu = await UploadSozlesmeDosyaAsync(new SozlesmeDosyaModel
                    {
                        SozlesmeID = sozlesme.SozlesmeID,
                        Dosya = model.SozlesmeBelgesi
                    });
                }
                
                if (model.Vekaletname != null)
                {
                    sozlesme.VekaletnameDosyaYolu = await UploadVekaletnameAsync(new SozlesmeDosyaModel
                    {
                        SozlesmeID = sozlesme.SozlesmeID,
                        Dosya = model.Vekaletname
                    });
                }
                    
                sozlesme.SozlesmeTutari = model.SozlesmeTutari;
                sozlesme.SozlesmeDovizTuru = model.SozlesmeDovizTuru;
                sozlesme.Aciklama = model.Aciklama;
                sozlesme.AktifMi = model.AktifMi;
                sozlesme.GuncellemeTarihi = DateTime.Now;
                
                _unitOfWork.SozlesmeRepository.Update(sozlesme);
                await _unitOfWork.SaveAsync();
                
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Sözleşme güncellenirken hata oluştu: {Hata}", ex.Message);
                return false;
            }
        }

        public async Task<bool> DeleteSozlesmeAsync(Guid id)
        {
            try
            {
                var sozlesme = await _unitOfWork.SozlesmeRepository.GetByIdAsync(id);
                if (sozlesme == null) return false;
                
                // İlişkili faturalar var mı?
                var iliskiliFaturaVar = await _context.Faturalar
                    .AnyAsync(f => f.SozlesmeID == id && !f.Silindi);
                
                if (iliskiliFaturaVar)
                {
                    _logger.LogWarning("Silme işlemi başarısız. Sözleşmeye bağlı faturalar var. Sözleşme ID: {Id}", id);
                    return false;
                }
                
                // Yazılımsal silme işlemi
                sozlesme.Silindi = true;
                sozlesme.AktifMi = false;
                sozlesme.GuncellemeTarihi = DateTime.Now;
                
                _unitOfWork.SozlesmeRepository.Update(sozlesme);
                await _unitOfWork.SaveAsync();
                
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Sözleşme silinirken hata oluştu");
                return false;
            }
        }

        public async Task<string> UploadSozlesmeDosyaAsync(SozlesmeDosyaModel model)
        {
            if (model.Dosya == null || model.Dosya.Length == 0)
                return string.Empty;
            
            // Güvenli dosya adı oluştur
            var uzanti = Path.GetExtension(model.Dosya.FileName);
            var dosyaAdi = $"{Guid.NewGuid()}{uzanti}";
            
            // Dizin oluştur
            var dizin = Path.Combine(_hostingEnvironment.WebRootPath, "uploads", "sozlesmeler");
            if (!Directory.Exists(dizin))
            {
                Directory.CreateDirectory(dizin);
            }
            
            // Dosya yolunu oluştur
            var dosyaYolu = Path.Combine(dizin, dosyaAdi);
            
            try
            {
                // Dosyayı kaydet
                using (var stream = new FileStream(dosyaYolu, FileMode.Create))
                {
                    await model.Dosya.CopyToAsync(stream);
                }
                
                // Dosya yolunu döndür
                return Path.Combine("uploads", "sozlesmeler", dosyaAdi);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Sözleşme dosyası yüklenirken hata oluştu");
                return string.Empty;
            }
        }

        public async Task<string> UploadVekaletnameAsync(SozlesmeDosyaModel model)
        {
            if (model.Dosya == null || model.Dosya.Length == 0)
                return string.Empty;
            
            // Güvenli dosya adı oluştur
            var uzanti = Path.GetExtension(model.Dosya.FileName);
            var dosyaAdi = $"{Guid.NewGuid()}{uzanti}";
            
            // Dizin oluştur
            var dizin = Path.Combine(_hostingEnvironment.WebRootPath, "uploads", "vekaletler");
            if (!Directory.Exists(dizin))
            {
                Directory.CreateDirectory(dizin);
            }
            
            // Dosya yolunu oluştur
            var dosyaYolu = Path.Combine(dizin, dosyaAdi);
            
            try
            {
                // Dosyayı kaydet
                using (var stream = new FileStream(dosyaYolu, FileMode.Create))
                {
                    await model.Dosya.CopyToAsync(stream);
                }
                
                // Dosya yolunu döndür
                return Path.Combine("uploads", "vekaletler", dosyaAdi);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Vekaletname dosyası yüklenirken hata oluştu");
                return string.Empty;
            }
        }

        public async Task<List<SozlesmeListViewModel>> GetSozlesmeListViewModelsAsync()
        {
            try
            {
                var sozlesmeler = await _context.Sozlesmeler
                    .Include(s => s.Cari)
                    .Where(s => !s.Silindi)
                    .OrderByDescending(s => s.OlusturmaTarihi)
                    .ToListAsync();
                
                return await MapToListViewModel(sozlesmeler);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Sözleşme listesi alınırken hata oluştu");
                return new List<SozlesmeListViewModel>();
            }
        }

        public async Task<List<SozlesmeListViewModel>> GetSozlesmeListViewModelsAsync(Guid cariId)
        {
            try
            {
                var sozlesmeler = await _context.Sozlesmeler
                    .Include(s => s.Cari)
                    .Where(s => !s.Silindi && s.CariID == cariId)
                    .OrderByDescending(s => s.OlusturmaTarihi)
                    .ToListAsync();
                
                return await MapToListViewModel(sozlesmeler);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Cari ID'ye göre sözleşme listesi alınırken hata oluştu: {CariId}", cariId);
                return new List<SozlesmeListViewModel>();
            }
        }

        public async Task<SozlesmeViewModel> GetSozlesmeDetailAsync(Guid id)
        {
            var sozlesme = await GetSozlesmeByIdAsync(id);
            
            if (sozlesme == null)
                return null;
                
            // SozlesmeViewModel içindeki özellikleri manuel olarak ayarlayalım
            var viewModel = new SozlesmeViewModel
            {
                SozlesmeID = sozlesme.SozlesmeID.ToString(),
                SozlesmeNo = sozlesme.SozlesmeNo,
                SozlesmeTarihi = sozlesme.SozlesmeTarihi,
                BitisTarihi = sozlesme.BitisTarihi,
                CariID = sozlesme.CariID.ToString(),
                CariAdi = sozlesme.Cari?.Ad,
                VekaletGeldiMi = sozlesme.VekaletGeldiMi,
                ResmiFaturaKesildiMi = sozlesme.ResmiFaturaKesildiMi,
                SozlesmeDosyaYolu = sozlesme.SozlesmeDosyaYolu,
                VekaletnameDosyaYolu = sozlesme.VekaletnameDosyaYolu,
                SozlesmeTutari = sozlesme.SozlesmeTutari,
                SozlesmeDovizTuru = sozlesme.SozlesmeDovizTuru,
                Aciklama = sozlesme.Aciklama,
                AktifMi = sozlesme.AktifMi,
                OlusturmaTarihi = sozlesme.OlusturmaTarihi,
                GuncellemeTarihi = sozlesme.GuncellemeTarihi
            };
            
            // İlişkili faturaları getir
            var faturalar = await _context.Faturalar
                .Where(f => f.SozlesmeID == id && !f.Silindi)
                .ToListAsync();
                
            // Faturalar manuel olarak ekle
            if (faturalar != null && faturalar.Any())
            {
                viewModel.Faturalar = new List<FaturaViewModel>();
                
                foreach (var fatura in faturalar)
                {
                    viewModel.Faturalar.Add(new FaturaViewModel
                    {
                        FaturaID = fatura.FaturaID.ToString(),
                        FaturaNumarasi = fatura.FaturaNumarasi,
                        FaturaTarihi = fatura.FaturaTarihi,
                        GenelToplam = fatura.GenelToplam ?? 0, // Null durumunda varsayılan değer ata
                        CariAdi = "Fatura Müşterisi", // Required alanlar için varsayılan değer
                        FaturaTuru = "Satış", // Required alanlar için varsayılan değer
                        Aciklama = "Fatura açıklaması", // Required alanlar için varsayılan değer 
                        OdemeDurumu = "Beklemede" // Required alanlar için varsayılan değer
                    });
                }
            }
            
            return viewModel;
        }

        public async Task<bool> CreateSozlesmeAsync(SozlesmeViewModel model)
        {
            try
            {
                var sozlesme = new Sozlesme
                {
                    SozlesmeNo = model.SozlesmeNo,
                    SozlesmeTarihi = model.SozlesmeTarihi,
                    BitisTarihi = model.BitisTarihi,
                    CariID = !string.IsNullOrEmpty(model.CariID) ? Guid.Parse(model.CariID) : Guid.Empty,
                    VekaletGeldiMi = model.VekaletGeldiMi,
                    ResmiFaturaKesildiMi = model.ResmiFaturaKesildiMi,
                    SozlesmeDosyaYolu = model.SozlesmeDosyaYolu,
                    VekaletnameDosyaYolu = model.VekaletnameDosyaYolu,
                    SozlesmeTutari = model.SozlesmeTutari,
                    SozlesmeDovizTuru = model.SozlesmeDovizTuru,
                    Aciklama = model.Aciklama,
                    AktifMi = model.AktifMi,
                    OlusturmaTarihi = DateTime.Now,
                    OlusturanKullaniciID = model.OlusturanKullaniciID.HasValue ? model.OlusturanKullaniciID.Value : (Guid?)null
                };

                await _unitOfWork.SozlesmeRepository.AddAsync(sozlesme);
                await _unitOfWork.SaveAsync();
                
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Sözleşme oluşturulurken hata oluştu: {Hata}", ex.Message);
                return false;
            }
        }

        public async Task<bool> UpdateSozlesmeDetailAsync(SozlesmeViewModel model)
        {
            try
            {
                var sozlesme = await _unitOfWork.SozlesmeRepository.GetByIdAsync(Guid.Parse(model.SozlesmeID));
                
                if (sozlesme == null)
                    return false;
                    
                sozlesme.SozlesmeNo = model.SozlesmeNo;
                sozlesme.SozlesmeTarihi = model.SozlesmeTarihi;
                sozlesme.BitisTarihi = model.BitisTarihi;
                sozlesme.CariID = !string.IsNullOrEmpty(model.CariID) ? Guid.Parse(model.CariID) : Guid.Empty;
                sozlesme.VekaletGeldiMi = model.VekaletGeldiMi;
                sozlesme.ResmiFaturaKesildiMi = model.ResmiFaturaKesildiMi;
                
                // Dosya yollarını sadece yeni değerler varsa güncelle
                if (!string.IsNullOrEmpty(model.SozlesmeDosyaYolu))
                    sozlesme.SozlesmeDosyaYolu = model.SozlesmeDosyaYolu;
                    
                if (!string.IsNullOrEmpty(model.VekaletnameDosyaYolu))
                    sozlesme.VekaletnameDosyaYolu = model.VekaletnameDosyaYolu;
                    
                sozlesme.SozlesmeTutari = model.SozlesmeTutari;
                sozlesme.SozlesmeDovizTuru = model.SozlesmeDovizTuru;
                sozlesme.Aciklama = model.Aciklama;
                sozlesme.AktifMi = model.AktifMi;
                sozlesme.GuncellemeTarihi = DateTime.Now;
                sozlesme.GuncelleyenKullaniciID = model.GuncelleyenKullaniciID.HasValue ? model.GuncelleyenKullaniciID.Value : (Guid?)null;
                
                _unitOfWork.SozlesmeRepository.Update(sozlesme);
                await _unitOfWork.SaveAsync();
                
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Sözleşme güncellenirken hata oluştu: {Hata}", ex.Message);
                return false;
            }
        }

        private async Task<List<SozlesmeListViewModel>> MapToListViewModel(List<Sozlesme> sozlesmeler)
        {
            var viewModels = new List<SozlesmeListViewModel>();
            
            foreach (var sozlesme in sozlesmeler)
            {
                var viewModel = new SozlesmeListViewModel
                {
                    SozlesmeID = sozlesme.SozlesmeID.ToString(),
                    SozlesmeNo = sozlesme.SozlesmeNo,
                    SozlesmeTarihi = sozlesme.SozlesmeTarihi,
                    BitisTarihi = sozlesme.BitisTarihi,
                    CariAdi = sozlesme.Cari?.Ad,
                    SozlesmeTutari = sozlesme.SozlesmeTutari,
                    SozlesmeDovizTuru = sozlesme.SozlesmeDovizTuru,
                    VekaletGeldiMi = sozlesme.VekaletGeldiMi,
                    ResmiFaturaKesildiMi = sozlesme.ResmiFaturaKesildiMi,
                    DosyaVar = !string.IsNullOrEmpty(sozlesme.SozlesmeDosyaYolu),
                    VekaletVar = !string.IsNullOrEmpty(sozlesme.VekaletnameDosyaYolu),
                    AktifMi = sozlesme.AktifMi
                };
                
                // İlişkili faturaları say
                var faturaSayisi = await _context.Faturalar
                    .Where(f => f.SozlesmeID == sozlesme.SozlesmeID && !f.Silindi)
                    .CountAsync();
                
                viewModel.FaturaSayisi = faturaSayisi;
                
                // Aklamaları sayma işlemi burada yapılabilir (eğer gerekli ise)
                // viewModel.AklamaSayisi = ...;
                
                viewModels.Add(viewModel);
            }
            
            return viewModels;
        }

        // Diğer ViewModel dönüşümlerinde ViewModels namespace'indeki sınıfları kullan
        public async Task<List<SozlesmeViewModel>> GetAllSozlesmeViewModelsAsync(bool? aktifMi = null)
        {
            var query = _context.Sozlesmeler
                .Include(s => s.Cari)
                .Where(s => !s.Silindi);
                
            if (aktifMi.HasValue)
            {
                query = query.Where(s => s.AktifMi == aktifMi.Value);
            }
            
            var sozlesmeler = await query.OrderByDescending(s => s.OlusturmaTarihi).ToListAsync();
            var viewModels = new List<SozlesmeViewModel>();
            
            foreach (var sozlesme in sozlesmeler)
            {
                var viewModel = new SozlesmeViewModel
                {
                    SozlesmeID = sozlesme.SozlesmeID.ToString(),
                    SozlesmeNo = sozlesme.SozlesmeNo,
                    SozlesmeTarihi = sozlesme.SozlesmeTarihi,
                    BitisTarihi = sozlesme.BitisTarihi,
                    CariID = sozlesme.CariID.ToString(),
                    CariAdi = sozlesme.Cari?.Ad,
                    VekaletGeldiMi = sozlesme.VekaletGeldiMi,
                    ResmiFaturaKesildiMi = sozlesme.ResmiFaturaKesildiMi,
                    SozlesmeDosyaYolu = sozlesme.SozlesmeDosyaYolu,
                    VekaletnameDosyaYolu = sozlesme.VekaletnameDosyaYolu,
                    SozlesmeTutari = sozlesme.SozlesmeTutari,
                    SozlesmeDovizTuru = sozlesme.SozlesmeDovizTuru,
                    Aciklama = sozlesme.Aciklama,
                    AktifMi = sozlesme.AktifMi,
                    OlusturmaTarihi = sozlesme.OlusturmaTarihi,
                    GuncellemeTarihi = sozlesme.GuncellemeTarihi
                };
                
                // İlişkili faturaları getir ve ekle
                var faturalar = await _context.Faturalar
                    .Where(f => f.SozlesmeID == sozlesme.SozlesmeID && !f.Silindi)
                    .ToListAsync();
                    
                if (faturalar != null && faturalar.Any())
                {
                    viewModel.Faturalar = new List<FaturaViewModel>();
                    
                    foreach (var fatura in faturalar)
                    {
                        viewModel.Faturalar.Add(new FaturaViewModel
                        {
                            FaturaID = fatura.FaturaID.ToString(),
                            FaturaNumarasi = fatura.FaturaNumarasi,
                            FaturaTarihi = fatura.FaturaTarihi,
                            GenelToplam = fatura.GenelToplam ?? 0, // Null durumunda varsayılan değer ata
                            CariAdi = "Fatura Müşterisi", // Required alanlar için varsayılan değer
                            FaturaTuru = "Satış", // Required alanlar için varsayılan değer
                            Aciklama = "Fatura açıklaması", // Required alanlar için varsayılan değer 
                            OdemeDurumu = "Beklemede" // Required alanlar için varsayılan değer
                        });
                    }
                }
                
                viewModels.Add(viewModel);
            }
            
            return viewModels;
        }
    }
} 