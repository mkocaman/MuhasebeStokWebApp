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
using MuhasebeStokWebApp.ViewModels.Fatura;

namespace MuhasebeStokWebApp.Services
{
    /// <summary>
    /// Fatura ve ilişkili hareketlerin (stok, cari) tutarlılık kontrolünü yapar
    /// </summary>
    public class FaturaValidationService : IFaturaValidationService
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<FaturaValidationService> _logger;

        public FaturaValidationService(
            ApplicationDbContext context,
            ILogger<FaturaValidationService> logger)
        {
            _context = context;
            _logger = logger;
        }

        /// <summary>
        /// Fatura oluşturma view model'inin tutarlılığını kontrol eder
        /// </summary>
        public (bool IsValid, string ErrorMessage) ValidateFaturaCreateViewModel(FaturaCreateViewModel viewModel)
        {
            if (viewModel == null)
            {
                return (false, "Fatura verisi bulunamadı.");
            }

            // Temel alan kontrolü
            if (viewModel.FaturaKalemleri == null || !viewModel.FaturaKalemleri.Any())
            {
                return (false, "Fatura kalemleri boş olamaz.");
            }

            if (viewModel.CariID == Guid.Empty)
            {
                return (false, "Geçerli bir cari seçilmelidir.");
            }

            if (!viewModel.FaturaTuruID.HasValue || viewModel.FaturaTuruID == Guid.Empty)
            {
                return (false, "Geçerli bir fatura türü seçilmelidir.");
            }

            // Kalem validasyonu
            if (!viewModel.FaturaKalemleri.Any(k => k.UrunID != Guid.Empty && k.Miktar > 0))
            {
                return (false, "En az bir geçerli fatura kalemi eklemelisiniz.");
            }

            // Tutarlar kontrolü
            if ((viewModel.AraToplam ?? 0) <= 0)
            {
                return (false, "Ara toplam sıfırdan büyük olmalıdır.");
            }

            // Toplamların tutarlılığı
            decimal hesaplananAraToplam = 0;
            decimal hesaplananKdvToplam = 0;
            decimal hesaplananIndirimTutari = 0;

            foreach (var kalem in viewModel.FaturaKalemleri.Where(k => k.UrunID != Guid.Empty && k.Miktar > 0))
            {
                decimal kalemTutar = kalem.Miktar * kalem.BirimFiyat;
                decimal kalemIndirimTutari = kalemTutar * kalem.IndirimOrani / 100;
                decimal kalemKdvMatrahi = kalemTutar - kalemIndirimTutari;
                decimal kalemKdvTutari = kalemKdvMatrahi * kalem.KdvOrani / 100;
                
                hesaplananAraToplam += kalemTutar;
                hesaplananIndirimTutari += kalemIndirimTutari;
                hesaplananKdvToplam += kalemKdvTutari;
            }
            
            decimal hesaplananGenelToplam = hesaplananAraToplam - hesaplananIndirimTutari + hesaplananKdvToplam;
            
            // Hesaplanan değerler ile viewModel'deki değerlerin karşılaştırılması (0.01 tolerans)
            if (Math.Abs(hesaplananAraToplam - (viewModel.AraToplam ?? 0)) > 0.01m ||
                Math.Abs(hesaplananGenelToplam - (viewModel.GenelToplam ?? 0)) > 0.01m)
            {
                return (false, $"Fatura tutarları tutarsız. Hesaplanan değerler: AraToplam={hesaplananAraToplam:F2}, GenelToplam={hesaplananGenelToplam:F2}");
            }

            return (true, string.Empty);
        }

        /// <summary>
        /// Fatura düzenleme view model'inin tutarlılığını kontrol eder
        /// </summary>
        public (bool IsValid, string ErrorMessage) ValidateFaturaEditViewModel(FaturaEditViewModel viewModel)
        {
            if (viewModel == null)
            {
                return (false, "Fatura verisi bulunamadı.");
            }

            // Temel alan kontrolü
            if (viewModel.FaturaKalemleri == null || !viewModel.FaturaKalemleri.Any())
            {
                return (false, "Fatura kalemleri boş olamaz.");
            }

            if (viewModel.CariID == Guid.Empty)
            {
                return (false, "Geçerli bir cari seçilmelidir.");
            }

            if (!viewModel.FaturaTuruID.HasValue || viewModel.FaturaTuruID.Value == 0)
            {
                return (false, "Geçerli bir fatura türü seçilmelidir.");
            }

            // Kalem validasyonu
            if (!viewModel.FaturaKalemleri.Any(k => k.UrunID != Guid.Empty && k.Miktar > 0))
            {
                return (false, "En az bir geçerli fatura kalemi eklemelisiniz.");
            }

            // Tutarlar kontrolü
            if (viewModel.AraToplam <= 0)
            {
                return (false, "Ara toplam sıfırdan büyük olmalıdır.");
            }

            // Toplamların tutarlılığı
            decimal hesaplananAraToplam = 0;
            decimal hesaplananKdvToplam = 0;
            decimal hesaplananIndirimTutari = 0;

            foreach (var kalem in viewModel.FaturaKalemleri.Where(k => k.UrunID != Guid.Empty && k.Miktar > 0))
            {
                decimal kalemTutar = kalem.Miktar * kalem.BirimFiyat;
                decimal kalemIndirimTutari = kalemTutar * kalem.IndirimOrani / 100;
                decimal kalemKdvMatrahi = kalemTutar - kalemIndirimTutari;
                decimal kalemKdvTutari = kalemKdvMatrahi * kalem.KdvOrani / 100;
                
                hesaplananAraToplam += kalemTutar;
                hesaplananIndirimTutari += kalemIndirimTutari;
                hesaplananKdvToplam += kalemKdvTutari;
            }
            
            decimal hesaplananGenelToplam = hesaplananAraToplam - hesaplananIndirimTutari + hesaplananKdvToplam;
            
            // Hesaplanan değerler ile viewModel'deki değerlerin karşılaştırılması (0.01 tolerans)
            if (Math.Abs(hesaplananAraToplam - viewModel.AraToplam) > 0.01m ||
                Math.Abs(hesaplananGenelToplam - viewModel.GenelToplam) > 0.01m)
            {
                return (false, $"Fatura tutarları tutarsız. Hesaplanan değerler: AraToplam={hesaplananAraToplam:F2}, GenelToplam={hesaplananGenelToplam:F2}");
            }

            return (true, string.Empty);
        }

        /// <summary>
        /// Fatura ve ilgili StokHareket kayıtlarının tutarlı olup olmadığını kontrol eder
        /// </summary>
        public async Task<(bool IsValid, string ErrorMessage)> ValidateFaturaStokTutarliligi(Guid faturaId)
        {
            var fatura = await _context.Faturalar
                .Include(f => f.FaturaDetaylari)
                .Include(f => f.FaturaTuru)
                .FirstOrDefaultAsync(f => f.FaturaID == faturaId && !f.Silindi);

            if (fatura == null)
            {
                return (false, $"FaturaID: {faturaId} bulunamadı.");
            }

            var stokHareketleri = await _context.StokHareketleri
                .Where(sh => sh.FaturaID == faturaId && !sh.Silindi)
                .ToListAsync();

            // Fatura ve stok hareket sayıları kontrolü
            if (fatura.FaturaDetaylari.Count != stokHareketleri.Count)
            {
                return (false, $"Fatura detay sayısı ({fatura.FaturaDetaylari.Count}) ile stok hareket sayısı ({stokHareketleri.Count}) tutarsız.");
            }

            // Stok hareket tipinin doğruluğu
            var beklenenHareketTipi = fatura.FaturaTuru?.HareketTuru == "Giriş"
                ? Enums.StokHareketiTipi.Giris
                : Enums.StokHareketiTipi.Cikis;

            foreach (var hareket in stokHareketleri)
            {
                if (hareket.HareketTuru != beklenenHareketTipi)
                {
                    return (false, $"StokHareketID: {hareket.StokHareketID} için hareket türü yanlış. Beklenen: {beklenenHareketTipi}, Gerçek: {hareket.HareketTuru}");
                }
            }

            // Her fatura detayı için stok hareketi var mı kontrolü
            foreach (var detay in fatura.FaturaDetaylari)
            {
                var ilgiliHareket = stokHareketleri.FirstOrDefault(sh => sh.UrunID == detay.UrunID);
                if (ilgiliHareket == null)
                {
                    return (false, $"FaturaDetayID: {detay.FaturaDetayID}, UrunID: {detay.UrunID} için stok hareketi bulunamadı.");
                }

                // Miktar tutarlılığı
                decimal beklenenMiktar = beklenenHareketTipi == Enums.StokHareketiTipi.Cikis
                    ? -detay.Miktar  // Çıkış hareketinde miktar negatif olmalı
                    : detay.Miktar;  // Giriş hareketinde miktar pozitif olmalı

                if (ilgiliHareket.Miktar != beklenenMiktar)
                {
                    return (false, $"UrunID: {detay.UrunID} için miktar tutarsız. Beklenen: {beklenenMiktar}, Gerçek: {ilgiliHareket.Miktar}");
                }
            }

            return (true, string.Empty);
        }

        /// <summary>
        /// Fatura ve ilgili CariHareket kaydının tutarlı olup olmadığını kontrol eder
        /// </summary>
        public async Task<(bool IsValid, string ErrorMessage)> ValidateFaturaCariTutarliligi(Guid faturaId)
        {
            var fatura = await _context.Faturalar
                .Include(f => f.FaturaTuru)
                .FirstOrDefaultAsync(f => f.FaturaID == faturaId && !f.Silindi);

            if (fatura == null)
            {
                return (false, $"FaturaID: {faturaId} bulunamadı.");
            }

            if (fatura.CariID == null || fatura.CariID == Guid.Empty)
            {
                return (false, $"FaturaID: {faturaId} için CariID tanımlanmamış.");
            }

            var cariHareketler = await _context.CariHareketler
                .Where(ch => ch.ReferansID == faturaId && ch.ReferansTuru == "Fatura" && !ch.Silindi)
                .ToListAsync();

            // Cari hareket var mı kontrolü
            if (cariHareketler == null || !cariHareketler.Any())
            {
                return (false, $"FaturaID: {faturaId} için cari hareket kaydı bulunamadı.");
            }

            // Tek bir cari hareket olmalı
            if (cariHareketler.Count > 1)
            {
                return (false, $"FaturaID: {faturaId} için birden fazla cari hareket kaydı bulundu.");
            }

            var cariHareket = cariHareketler.First();

            // Cari ID tutarlılığı
            if (cariHareket.CariID != fatura.CariID.Value)
            {
                return (false, $"Cari hareket ve fatura için farklı Cari ID'ler: Fatura CariID: {fatura.CariID}, CariHareket CariID: {cariHareket.CariID}");
            }

            // Tutar tutarlılığı
            if (cariHareket.Tutar != fatura.GenelToplam)
            {
                return (false, $"Cari hareket tutarı ({cariHareket.Tutar}) ile fatura genel toplamı ({fatura.GenelToplam}) tutarsız.");
            }

            // Hareket türü tutarlılığı
            bool faturaCikis = fatura.FaturaTuru?.HareketTuru == "Çıkış"; // Satış faturası
            
            if (faturaCikis)
            {
                // Satış faturası ise cari borçlanır (müşteri bize borçlanır)
                if (cariHareket.Borc != fatura.GenelToplam || cariHareket.Alacak != 0)
                {
                    return (false, $"Satış faturası için borç ve alacak değerleri tutarsız. Borç: {cariHareket.Borc}, Alacak: {cariHareket.Alacak}");
                }
            }
            else
            {
                // Alış faturası ise cari alacaklanır (biz tedarikçiye borçlanırız)
                if (cariHareket.Alacak != fatura.GenelToplam || cariHareket.Borc != 0)
                {
                    return (false, $"Alış faturası için borç ve alacak değerleri tutarsız. Borç: {cariHareket.Borc}, Alacak: {cariHareket.Alacak}");
                }
            }

            return (true, string.Empty);
        }

        /// <summary>
        /// Fatura ve ilgili Irsaliye kaydının tutarlı olup olmadığını kontrol eder
        /// </summary>
        public async Task<(bool IsValid, string ErrorMessage)> ValidateFaturaIrsaliyeTutarliligi(Guid faturaId)
        {
            var fatura = await _context.Faturalar
                .Include(f => f.FaturaDetaylari)
                .Include(f => f.FaturaTuru)
                .FirstOrDefaultAsync(f => f.FaturaID == faturaId && !f.Silindi);

            if (fatura == null)
            {
                return (false, $"FaturaID: {faturaId} bulunamadı.");
            }

            var irsaliyeler = await _context.Irsaliyeler
                .Include(i => i.IrsaliyeDetaylari)
                .Where(i => i.FaturaID == faturaId && !i.Silindi)
                .ToListAsync();

            // İrsaliye var mı kontrolü - eğer yoksa bu bir hata değil, isteğe bağlı olabilir
            if (irsaliyeler == null || !irsaliyeler.Any())
            {
                return (true, $"FaturaID: {faturaId} için irsaliye kaydı bulunmamaktadır.");
            }

            // İrsaliye türü fatura türüyle uyumlu mu
            foreach (var irsaliye in irsaliyeler)
            {
                string beklenenIrsaliyeTuru = fatura.FaturaTuru?.HareketTuru == "Giriş"
                    ? "Giriş İrsaliyesi"
                    : "Çıkış İrsaliyesi";

                if (irsaliye.IrsaliyeTuru != beklenenIrsaliyeTuru)
                {
                    return (false, $"İrsaliye türü ({irsaliye.IrsaliyeTuru}) ile fatura türü ({fatura.FaturaTuru?.HareketTuru}) uyumsuz.");
                }

                // Cari tutarlılığı
                if (irsaliye.CariID != fatura.CariID)
                {
                    return (false, $"İrsaliye cari ID'si ({irsaliye.CariID}) ile fatura cari ID'si ({fatura.CariID}) uyumsuz.");
                }

                // İrsaliye detay sayısı fatura detay sayısıyla aynı olmalı
                if (irsaliye.IrsaliyeDetaylari.Count != fatura.FaturaDetaylari.Count)
                {
                    return (false, $"İrsaliye detay sayısı ({irsaliye.IrsaliyeDetaylari.Count}) ile fatura detay sayısı ({fatura.FaturaDetaylari.Count}) uyumsuz.");
                }

                // Detay ürün ve miktar kontrolü
                foreach (var faturaDetay in fatura.FaturaDetaylari)
                {
                    var irsaliyeDetay = irsaliye.IrsaliyeDetaylari.FirstOrDefault(id => id.UrunID == faturaDetay.UrunID);
                    if (irsaliyeDetay == null)
                    {
                        return (false, $"Fatura detayındaki ürün (UrunID: {faturaDetay.UrunID}) için irsaliye detayı bulunamadı.");
                    }

                    if (irsaliyeDetay.Miktar != faturaDetay.Miktar)
                    {
                        return (false, $"UrunID: {faturaDetay.UrunID} için miktar uyumsuz. Fatura miktarı: {faturaDetay.Miktar}, İrsaliye miktarı: {irsaliyeDetay.Miktar}");
                    }
                }
            }

            return (true, string.Empty);
        }
    }
} 