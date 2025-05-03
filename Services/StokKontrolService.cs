using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using MuhasebeStokWebApp.Data;
using MuhasebeStokWebApp.Data.Entities;
using MuhasebeStokWebApp.Data.Repositories;
using MuhasebeStokWebApp.Services.Interfaces;

namespace MuhasebeStokWebApp.Services
{
    public class StokKontrolService : IStokKontrolService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogService _logService;
        private readonly ILogger<StokKontrolService> _logger;
        private readonly decimal _kritikStokSeviyesi = 100; // Kritik stok seviyesi 100 olarak güncellendi (önceden 10)

        public StokKontrolService(
            IUnitOfWork unitOfWork,
            ILogService logService,
            ILogger<StokKontrolService> logger)
        {
            _unitOfWork = unitOfWork;
            _logService = logService;
            _logger = logger;
        }

        /// <summary>
        /// Tüm ürünlerin stok durumunu kontrol eder ve kritik seviyenin altındakiler için uyarı oluşturur
        /// </summary>
        public async Task KritikStokKontroluYapAsync()
        {
            try
            {
                // Aktif ve silinmemiş tüm ürünleri getir
                var urunler = await _unitOfWork.UrunRepository.GetAll()
                    .Where(u => u.Aktif && !u.Silindi)
                    .ToListAsync();

                foreach (var urun in urunler)
                {
                    // Depolar arası toplam stok miktarını hesapla
                    var toplamStok = await _unitOfWork.StokFifoRepository.GetAll()
                        .Where(s => s.UrunID == urun.UrunID && !s.Silindi)
                        .SumAsync(s => s.Miktar);

                    // Kritik seviye kontrolü
                    if (toplamStok <= _kritikStokSeviyesi)
                    {
                        await _logService.StokKritikSeviyeLogOlustur(
                            urun.UrunID.ToString(),
                            urun.UrunAdi,
                            toplamStok,
                            _kritikStokSeviyesi
                        );

                        _logger.LogWarning("Kritik stok seviyesi uyarısı: {UrunAdi} - Mevcut stok: {Stok}, Kritik Seviye: {KritikSeviye}", 
                            urun.UrunAdi, toplamStok, _kritikStokSeviyesi);
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Kritik stok kontrolü yapılırken hata oluştu");
                throw;
            }
        }

        /// <summary>
        /// Kritik stok seviyesinin altında olan ürünleri listeler
        /// </summary>
        public async Task<List<Urun>> KritikSeviyedeUrunleriGetirAsync()
        {
            try
            {
                var kritikUrunler = new List<Urun>();
                
                // Aktif ve silinmemiş tüm ürünleri getir
                var urunler = await _unitOfWork.UrunRepository.GetAll()
                    .Where(u => u.Aktif && !u.Silindi)
                    .ToListAsync();

                foreach (var urun in urunler)
                {
                    // Her ürün için toplam stok miktarını hesapla
                    var toplamStok = await _unitOfWork.StokFifoRepository.GetAll()
                        .Where(s => s.UrunID == urun.UrunID && !s.Silindi)
                        .SumAsync(s => s.Miktar);

                    // Kritik seviyede olanları listeye ekle
                    if (toplamStok <= _kritikStokSeviyesi)
                    {
                        // Stok miktarını ürün nesnesine ekle
                        urun.StokMiktar = toplamStok;
                        kritikUrunler.Add(urun);
                    }
                }

                return kritikUrunler;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Kritik seviyedeki ürünler getirilirken hata oluştu");
                throw;
            }
        }

        /// <summary>
        /// Belirli bir ürünün stok seviyesini kontrol eder
        /// </summary>
        public async Task<bool> UrunStokSeviyesiKontrolEtAsync(Guid urunID)
        {
            try
            {
                var urun = await _unitOfWork.UrunRepository.GetByIdAsync(urunID);
                if (urun == null || !urun.Aktif || urun.Silindi)
                {
                    return false;
                }

                var toplamStok = await _unitOfWork.StokFifoRepository.GetAll()
                    .Where(s => s.UrunID == urunID && !s.Silindi)
                    .SumAsync(s => s.Miktar);

                bool kritikSeviyede = toplamStok <= _kritikStokSeviyesi;

                if (kritikSeviyede)
                {
                    await _logService.StokKritikSeviyeLogOlustur(
                        urun.UrunID.ToString(),
                        urun.UrunAdi,
                        toplamStok,
                        _kritikStokSeviyesi
                    );
                }

                return kritikSeviyede;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ürün stok seviyesi kontrol edilirken hata: {UrunID}", urunID);
                throw;
            }
        }

        /// <summary>
        /// Kritik stok seviyesini döndürür
        /// </summary>
        public decimal GetKritikStokSeviyesi()
        {
            return _kritikStokSeviyesi;
        }
    }
} 