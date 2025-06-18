using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using MuhasebeStokWebApp.Controllers.ParaBirimiModulu.API;
using MuhasebeStokWebApp.Data;
using MuhasebeStokWebApp.Data.Entities;
using MuhasebeStokWebApp.Data.Repositories;
using MuhasebeStokWebApp.Services;
using MuhasebeStokWebApp.Services.Interfaces;
using MuhasebeStokWebApp.ViewModels.Transfer;

namespace MuhasebeStokWebApp.Controllers
{
    [Authorize]
    public class TransferController : BaseController
    {
        private readonly ApplicationDbContext _context;
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<TransferController> _logger;
        private readonly ILogService _logService;
        private readonly IDovizKuruService _dovizKuruService;

        public TransferController(
            ApplicationDbContext context,
            IUnitOfWork unitOfWork,
            IMenuService menuService,
            UserManager<ApplicationUser> userManager,
            RoleManager<IdentityRole> roleManager,
            ILogService logService,
            ILogger<TransferController> logger,
            IDovizKuruService dovizKuruService)
            : base(menuService, userManager, roleManager, logService)
        {
            _context = context;
            _unitOfWork = unitOfWork;
            _logger = logger;
            _logService = logService;
            _dovizKuruService = dovizKuruService;
        }

        // GET: /Transfer/YeniTransfer
        //public async Task<IActionResult> YeniTransfer(string tip = null)
        //{
        //    var model = new IcTransferViewModel
        //    {
        //        TransferTuru = tip ?? "KasadanKasaya",
        //        Tarih = DateTime.Now,
        //        ReferansNo = "TRF-" + DateTime.Now.ToString("yyMMdd") + "-" + new Random().Next(100, 999).ToString(),
        //    };

        //    // Tüm aktif kasaları getir
        //    var kasalar = await _unitOfWork.Repository<Kasa>()
        //        .GetAsync(
        //            filter: k => !k.Silindi && k.Aktif,
        //            orderBy: q => q.OrderBy(k => k.KasaAdi)
        //        );
            
        //    // Tüm aktif banka hesaplarını getir
        //    var bankaHesaplari = await _context.BankaHesaplari
        //        .Include(h => h.Banka)
        //        .Where(h => !h.Silindi && h.Aktif)
        //        .OrderBy(h => h.Banka.BankaAdi)
        //        .ThenBy(h => h.HesapAdi)
        //        .ToListAsync();

        //    ViewBag.Kasalar = kasalar.ToList();
        //    ViewBag.BankaHesaplari = bankaHesaplari;

        //    return View(model);
        //}

        // POST: /Transfer/YeniTransfer
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> YeniTransfer(IcTransferViewModel model)
        {
            bool isAjaxRequest = Request.Headers["X-Requested-With"] == "XMLHttpRequest";

            if (ModelState.IsValid)
            {
                try
                {
                    // Veritabanı işlemleri için transaction başlat
                    using var transaction = await _context.Database.BeginTransactionAsync();

                    try
                    {
                        switch (model.TransferTuru)
                        {
                            case "KasadanKasaya":
                                await KasadanKasayaTransfer(model);
                                break;
                            case "KasadanBankaya":
                                await KasadanBankayaTransfer(model);
                                break;
                            case "BankadanKasaya":
                                await BankadanKasayaTransfer(model);
                                break;
                            case "BankadanBankaya":
                                await BankadanBankayaTransfer(model);
                                break;
                            default:
                                throw new Exception("Geçersiz transfer türü.");
                        }

                        // Transaction'ı tamamla
                        await transaction.CommitAsync();
                        
                        // İşlemi logla
                        await _logService.Log(
                            $"Yeni transfer gerçekleştirildi: Tür: {model.TransferTuru}, Tutar: {model.Tutar}",
                            Enums.LogTuru.Bilgi
                        );

                        if (isAjaxRequest)
                        {
                            // AJAX yanıtı
                            string redirectUrl = "";
                            
                            // Transfer türüne göre yönlendirme URL'sini belirle
                            switch (model.TransferTuru)
                            {
                                case "KasadanKasaya":
                                case "KasadanBankaya":
                                    if (model.KaynakKasaID.HasValue)
                                        redirectUrl = Url.Action("Hareketler", "Kasa", new { id = model.KaynakKasaID.Value });
                                    break;
                                case "BankadanKasaya":
                                case "BankadanBankaya":
                                    if (model.KaynakBankaHesapID.HasValue)
                                        redirectUrl = Url.Action("HesapHareketler", "Banka", new { id = model.KaynakBankaHesapID.Value });
                                    break;
                            }
                            
                            return Json(new { 
                                success = true, 
                                message = "Transfer başarıyla gerçekleştirildi.", 
                                transferID = model.TransferID,
                                redirectUrl = redirectUrl
                            });
                        }

                        // Normal form gönderimi yanıtı
                        TempData["SuccessMessage"] = "Transfer başarıyla gerçekleştirildi.";
                        
                        // Transfer türüne göre yönlendirme
                        switch (model.TransferTuru)
                        {
                            case "KasadanKasaya":
                            case "KasadanBankaya":
                                if (model.KaynakKasaID.HasValue)
                                    return RedirectToAction("Hareketler", "Kasa", new { id = model.KaynakKasaID.Value });
                                break;
                            case "BankadanKasaya":
                            case "BankadanBankaya":
                                if (model.KaynakBankaHesapID.HasValue)
                                    return RedirectToAction("HesapHareketler", "Banka", new { id = model.KaynakBankaHesapID.Value });
                                break;
                        }
                        
                        return RedirectToAction("Index", "Kasa");
                    }
                    catch (Exception ex)
                    {
                        // Hata durumunda transaction'ı geri al
                        await transaction.RollbackAsync();
                        throw ex;
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Transfer işlemi sırasında hata oluştu.");
                    
                    if (isAjaxRequest)
                    {
                        return Json(new { 
                            success = false, 
                            message = "Transfer işlemi sırasında bir hata oluştu: " + ex.Message 
                        });
                    }
                    
                    ModelState.AddModelError("", "Transfer işlemi sırasında bir hata oluştu: " + ex.Message);
                }
            }
            else if (isAjaxRequest)
            {
                var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList();
                return Json(new { 
                    success = false, 
                    message = "Geçersiz form verileri, lütfen kontrol ediniz.", 
                    errors = errors 
                });
            }

            // Hata durumunda kasaları ve banka hesaplarını tekrar getir
            var kasalar = await _unitOfWork.Repository<Kasa>()
                .GetAsync(
                    filter: k => !k.Silindi && k.Aktif,
                    orderBy: q => q.OrderBy(k => k.KasaAdi)
                );
            
            var bankaHesaplari = await _context.BankaHesaplari
                .Include(h => h.Banka)
                .Where(h => !h.Silindi && h.Aktif)
                .OrderBy(h => h.Banka.BankaAdi)
                .ThenBy(h => h.HesapAdi)
                .ToListAsync();

            ViewBag.Kasalar = kasalar.ToList();
            ViewBag.BankaHesaplari = bankaHesaplari;
            
            // AJAX isteği ise partial view döndür
            if (isAjaxRequest)
            {
                return PartialView("~/Views/Kasa/_TransferModalPartial.cshtml", model);
            }

            return View(model);
        }

        // Kasadan Kasaya Transfer
        private async Task KasadanKasayaTransfer(IcTransferViewModel model)
        {
            if (!model.KaynakKasaID.HasValue || !model.HedefKasaID.HasValue)
                throw new Exception("Kaynak ve hedef kasa bilgileri gereklidir.");

            if (model.KaynakKasaID == model.HedefKasaID)
                throw new Exception("Kaynak ve hedef kasa aynı olamaz.");

            // Kaynak ve hedef kasaları getir
            var kaynakKasa = await _unitOfWork.Repository<Kasa>().GetByIdAsync(model.KaynakKasaID.Value);
            var hedefKasa = await _unitOfWork.Repository<Kasa>().GetByIdAsync(model.HedefKasaID.Value);

            if (kaynakKasa == null || hedefKasa == null)
                throw new Exception("Kaynak veya hedef kasa bulunamadı.");

            if (kaynakKasa.GuncelBakiye < model.Tutar)
                throw new Exception("Kaynak kasada yeterli bakiye bulunmuyor.");

            // Para birimleri farklı ise döviz kurunu kontrol et
            bool farkliParaBirimi = kaynakKasa.ParaBirimi != hedefKasa.ParaBirimi;
            decimal hedefTutar = model.Tutar;

            if (farkliParaBirimi)
            {
                if (!model.DovizKuru.HasValue || model.DovizKuru <= 0)
                    throw new Exception("Farklı para birimleri için geçerli bir döviz kuru belirtilmelidir.");

                hedefTutar = model.Tutar * model.DovizKuru.Value;
            }

            // Kaynak kasa hareketi oluştur (ÇIKIŞ)
            var kaynakHareket = new KasaHareket
            {
                KasaHareketID = Guid.NewGuid(),
                KasaID = kaynakKasa.KasaID,
                HareketTuru = "Çıkış",
                IslemTuru = "KasadanKasayaTransfer",
                Tutar = model.Tutar,
                DovizKuru = model.DovizKuru ?? 1,
                KarsiParaBirimi = hedefKasa.ParaBirimi,
                Tarih = model.Tarih,
                Aciklama = model.Aciklama ?? $"Kasadan kasaya transfer: {kaynakKasa.KasaAdi} -> {hedefKasa.KasaAdi}",
                ReferansNo = model.ReferansNo,
                ReferansTuru = "Transfer",
                TransferID = model.TransferID,
                HedefKasaID = hedefKasa.KasaID,
                IslemYapanKullaniciID = GetCurrentUserId(),
                OlusturmaTarihi = DateTime.Now
            };

            // Hedef kasa hareketi oluştur (GİRİŞ)
            var hedefHareket = new KasaHareket
            {
                KasaHareketID = Guid.NewGuid(),
                KasaID = hedefKasa.KasaID,
                HareketTuru = "Giriş",
                IslemTuru = "KasadanKasayaTransfer",
                Tutar = hedefTutar,
                DovizKuru = (1/model.DovizKuru) ?? 1,
                KarsiParaBirimi = kaynakKasa.ParaBirimi,
                Tarih = model.Tarih,
                Aciklama = model.Aciklama ?? $"Kasadan kasaya transfer: {kaynakKasa.KasaAdi} -> {hedefKasa.KasaAdi}",
                ReferansNo = model.ReferansNo,
                ReferansTuru = "Transfer",
                TransferID = model.TransferID,
                HedefKasaID = kaynakKasa.KasaID,
                IslemYapanKullaniciID = GetCurrentUserId(),
                OlusturmaTarihi = DateTime.Now
            };

            // Kasa bakiyelerini güncelle
            kaynakKasa.GuncelBakiye -= model.Tutar;
            hedefKasa.GuncelBakiye += hedefTutar;

            // Veritabanına kaydet
            await _unitOfWork.Repository<KasaHareket>().AddAsync(kaynakHareket);
            await _unitOfWork.Repository<KasaHareket>().AddAsync(hedefHareket);
            _unitOfWork.Repository<Kasa>().Update(kaynakKasa);
            _unitOfWork.Repository<Kasa>().Update(hedefKasa);
            await _unitOfWork.CompleteAsync();
        }

        // Kasadan Bankaya Transfer
        private async Task KasadanBankayaTransfer(IcTransferViewModel model)
        {
            if (!model.KaynakKasaID.HasValue || !model.HedefBankaHesapID.HasValue)
                throw new Exception("Kaynak kasa ve hedef banka hesabı bilgileri gereklidir.");

            // Kaynak kasa ve hedef banka hesabını getir
            var kaynakKasa = await _unitOfWork.Repository<Kasa>().GetByIdAsync(model.KaynakKasaID.Value);
            var hedefBankaHesap = await _context.BankaHesaplari
                .Include(h => h.Banka)
                .FirstOrDefaultAsync(h => h.BankaHesapID == model.HedefBankaHesapID.Value);

            if (kaynakKasa == null || hedefBankaHesap == null)
                throw new Exception("Kaynak kasa veya hedef banka hesabı bulunamadı.");

            if (kaynakKasa.GuncelBakiye < model.Tutar)
                throw new Exception("Kaynak kasada yeterli bakiye bulunmuyor.");

            // Para birimleri farklı ise döviz kurunu kontrol et
            bool farkliParaBirimi = kaynakKasa.ParaBirimi != hedefBankaHesap.ParaBirimi;
            decimal hedefTutar = model.Tutar;

            if (farkliParaBirimi)
            {
                if (!model.DovizKuru.HasValue || model.DovizKuru <= 0)
                    throw new Exception("Farklı para birimleri için geçerli bir döviz kuru belirtilmelidir.");

                hedefTutar = model.Tutar * model.DovizKuru.Value;
            }

            // Kaynak kasa hareketi oluştur (ÇIKIŞ)
            var kasaHareket = new KasaHareket
            {
                KasaHareketID = Guid.NewGuid(),
                KasaID = kaynakKasa.KasaID,
                HareketTuru = "Çıkış",
                IslemTuru = "KasadanBankayaTransfer",
                Tutar = model.Tutar,
                DovizKuru = model.DovizKuru ?? 1,
                KarsiParaBirimi = hedefBankaHesap.ParaBirimi,
                Tarih = model.Tarih,
                Aciklama = model.Aciklama ?? $"Kasadan bankaya transfer: {kaynakKasa.KasaAdi} -> {hedefBankaHesap.Banka.BankaAdi} - {hedefBankaHesap.HesapAdi}",
                ReferansNo = model.ReferansNo,
                ReferansTuru = "Transfer",
                TransferID = model.TransferID,
                HedefBankaID = hedefBankaHesap.BankaHesapID,
                IslemYapanKullaniciID = GetCurrentUserId(),
                OlusturmaTarihi = DateTime.Now
            };

            // Hedef banka hesap hareketi oluştur (GİRİŞ)
            var bankaHesapHareket = new BankaHesapHareket
            {
                BankaHesapHareketID = Guid.NewGuid(),
                HedefBankaID = hedefBankaHesap.BankaHesapID,
                BankaHesapID=hedefBankaHesap.BankaHesapID,
                BankaID = hedefBankaHesap.BankaID,
                DovizKuru= (1 / model.DovizKuru)??1,
                HareketTuru = "Giriş",
                IslemTuru= "KasadanBankayaTransfer",
                Tutar = hedefTutar,
                Tarih = model.Tarih,
                Aciklama = model.Aciklama ?? $"Kasadan bankaya transfer: {kaynakKasa.KasaAdi} -> {hedefBankaHesap.Banka.BankaAdi} - {hedefBankaHesap.HesapAdi}",
                ReferansNo = model.ReferansNo,
                ReferansTuru = "Transfer",
                TransferID = model.TransferID,
                DekontNo = model.ReferansNo,
                KaynakKasaID = kaynakKasa.KasaID,
                KarsiParaBirimi = kaynakKasa.ParaBirimi,
                IslemYapanKullaniciID = GetCurrentUserId(),
                OlusturmaTarihi = DateTime.Now
            };

            // Bakiyeleri güncelle
            kaynakKasa.GuncelBakiye -= model.Tutar;
            hedefBankaHesap.GuncelBakiye += hedefTutar;

            // Veritabanına kaydet
            try
            {
                await _unitOfWork.Repository<KasaHareket>().AddAsync(kasaHareket);
                _context.BankaHesapHareketleri.Add(bankaHesapHareket);
                _unitOfWork.Repository<Kasa>().Update(kaynakKasa);
                _context.BankaHesaplari.Update(hedefBankaHesap);

                await _unitOfWork.CompleteAsync();
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateException ex)
            {
                // Inner exception'daki gerçek SQL hatasını al
                var sqlEx = ex.InnerException?.Message ?? ex.Message;
                Console.WriteLine("Veritabanı güncelleme hatası: " + sqlEx);
                throw;
            }
        }

        // Bankadan Kasaya Transfer
        private async Task BankadanKasayaTransfer(IcTransferViewModel model)
        {
            if (!model.KaynakBankaHesapID.HasValue || !model.HedefKasaID.HasValue)
                throw new Exception("Kaynak banka hesabı ve hedef kasa bilgileri gereklidir.");

            // Kaynak banka hesabı ve hedef kasayı getir
            var kaynakBankaHesap = await _context.BankaHesaplari
                .Include(h => h.Banka)
                .FirstOrDefaultAsync(h => h.BankaHesapID == model.KaynakBankaHesapID.Value);
            var hedefKasa = await _unitOfWork.Repository<Kasa>().GetByIdAsync(model.HedefKasaID.Value);

            if (kaynakBankaHesap == null || hedefKasa == null)
                throw new Exception("Kaynak banka hesabı veya hedef kasa bulunamadı.");

            if (kaynakBankaHesap.GuncelBakiye < model.Tutar)
                throw new Exception("Kaynak banka hesabında yeterli bakiye bulunmuyor.");

            // Para birimleri farklı ise döviz kurunu kontrol et
            bool farkliParaBirimi = kaynakBankaHesap.ParaBirimi != hedefKasa.ParaBirimi;
            decimal hedefTutar = model.Tutar;

            if (farkliParaBirimi)
            {
                if (!model.DovizKuru.HasValue || model.DovizKuru <= 0)
                    throw new Exception("Farklı para birimleri için geçerli bir döviz kuru belirtilmelidir.");

                hedefTutar = model.Tutar * model.DovizKuru.Value;
            }

            // Kaynak banka hesap hareketi oluştur (ÇIKIŞ)
            var bankaHesapHareket = new BankaHesapHareket
            {
                BankaHesapHareketID = Guid.NewGuid(),
                BankaHesapID = kaynakBankaHesap.BankaHesapID,
                BankaID = kaynakBankaHesap.BankaID,
                HareketTuru = "Çıkış",
                IslemTuru= "BankadanKasayaTransfer",
                Tutar = model.Tutar,
                DovizKuru=model.DovizKuru??1,
                Tarih = model.Tarih,
                DekontNo = model.ReferansNo,
                Aciklama = model.Aciklama ?? $"Bankadan kasaya transfer: {kaynakBankaHesap.Banka.BankaAdi} - {kaynakBankaHesap.HesapAdi} -> {hedefKasa.KasaAdi}",
                ReferansNo = model.ReferansNo,
                ReferansTuru = "Transfer",
                TransferID = model.TransferID,
                HedefKasaID = hedefKasa.KasaID,
                KarsiParaBirimi = hedefKasa.ParaBirimi,
                IslemYapanKullaniciID = GetCurrentUserId(),
                OlusturmaTarihi = DateTime.Now
            };

            // Hedef kasa hareketi oluştur (GİRİŞ)
            var kasaHareket = new KasaHareket
            {
                KasaHareketID = Guid.NewGuid(),
                HedefKasaID = hedefKasa.KasaID,
                KasaID=hedefKasa.KasaID,
                HareketTuru = "Giriş",
                IslemTuru = "BankadanKasayaTransfer",
                Tutar = hedefTutar,
                DovizKuru = (1 / model.DovizKuru) ?? 1,
                KarsiParaBirimi = kaynakBankaHesap.ParaBirimi,
                Tarih = model.Tarih,
                Aciklama = model.Aciklama ?? $"Bankadan kasaya transfer: {kaynakBankaHesap.Banka.BankaAdi} - {kaynakBankaHesap.HesapAdi} -> {hedefKasa.KasaAdi}",
                ReferansNo = model.ReferansNo,
                ReferansTuru = "Transfer",
                TransferID = model.TransferID,
                KaynakBankaID = kaynakBankaHesap.BankaHesapID,
                IslemYapanKullaniciID = GetCurrentUserId(),
                OlusturmaTarihi = DateTime.Now
            };

            // Bakiyeleri güncelle
            kaynakBankaHesap.GuncelBakiye -= model.Tutar;
            hedefKasa.GuncelBakiye += hedefTutar;

            try
            {
                // Veritabanına kaydet
                _context.BankaHesapHareketleri.Add(bankaHesapHareket);
                await _unitOfWork.Repository<KasaHareket>().AddAsync(kasaHareket);
                _context.BankaHesaplari.Update(kaynakBankaHesap);
                _unitOfWork.Repository<Kasa>().Update(hedefKasa);
                await _context.SaveChangesAsync();
                await _unitOfWork.CompleteAsync();
            }
            catch (DbUpdateException ex)
            {
                // Inner exception'daki gerçek SQL hatasını al
                var sqlEx = ex.InnerException?.Message ?? ex.Message;
                Console.WriteLine("Veritabanı güncelleme hatası: " + sqlEx);
                throw;
            }
        }

        // Bankadan Bankaya Transfer
        private async Task BankadanBankayaTransfer(IcTransferViewModel model)
        {
            if (!model.KaynakBankaHesapID.HasValue || !model.HedefBankaHesapID.HasValue)
                throw new Exception("Kaynak ve hedef banka hesabı bilgileri gereklidir.");

            if (model.KaynakBankaHesapID == model.HedefBankaHesapID)
                throw new Exception("Kaynak ve hedef banka hesabı aynı olamaz.");

            // Kaynak ve hedef banka hesaplarını getir
            var kaynakBankaHesap = await _context.BankaHesaplari
                .Include(h => h.Banka)
                .FirstOrDefaultAsync(h => h.BankaHesapID == model.KaynakBankaHesapID.Value);
            var hedefBankaHesap = await _context.BankaHesaplari
                .Include(h => h.Banka)
                .FirstOrDefaultAsync(h => h.BankaHesapID == model.HedefBankaHesapID.Value);

            if (kaynakBankaHesap == null || hedefBankaHesap == null)
                throw new Exception("Kaynak veya hedef banka hesabı bulunamadı.");

            if (kaynakBankaHesap.GuncelBakiye < model.Tutar)
                throw new Exception("Kaynak banka hesabında yeterli bakiye bulunmuyor.");

            // Para birimleri farklı ise döviz kurunu kontrol et
            bool farkliParaBirimi = kaynakBankaHesap.ParaBirimi != hedefBankaHesap.ParaBirimi;
            decimal hedefTutar = model.Tutar;

            if (farkliParaBirimi)
            {
                if (!model.DovizKuru.HasValue || model.DovizKuru <= 0)
                    throw new Exception("Farklı para birimleri için geçerli bir döviz kuru belirtilmelidir.");

                hedefTutar = model.Tutar * model.DovizKuru.Value;
            }

            // Kaynak banka hesap hareketi oluştur (ÇIKIŞ)
            var kaynakHareket = new BankaHesapHareket
            {
                BankaHesapHareketID = Guid.NewGuid(),
                BankaHesapID = kaynakBankaHesap.BankaHesapID,
                BankaID = kaynakBankaHesap.BankaID,
                HedefBankaID=hedefBankaHesap.BankaHesapID,
                DovizKuru=model.DovizKuru??1,
                HareketTuru = "Çıkış",
                IslemTuru= "BankadanBankayaTransfer",
                Tutar = model.Tutar,
                Tarih = model.Tarih,
                Aciklama = model.Aciklama ?? $"Bankadan bankaya transfer: {kaynakBankaHesap.Banka.BankaAdi} - {kaynakBankaHesap.HesapAdi} -> {hedefBankaHesap.Banka.BankaAdi} - {hedefBankaHesap.HesapAdi}",
                ReferansNo = model.ReferansNo,
                DekontNo = model.ReferansNo,
                ReferansTuru = "Transfer",
                TransferID = model.TransferID,
                KarsiParaBirimi = hedefBankaHesap.ParaBirimi,
                IslemYapanKullaniciID = GetCurrentUserId(),
                OlusturmaTarihi = DateTime.Now
            };

            // Hedef banka hesap hareketi oluştur (GİRİŞ)
            var hedefHareket = new BankaHesapHareket
            {
                BankaHesapHareketID = Guid.NewGuid(),
                HedefBankaID = hedefBankaHesap.BankaHesapID,
                BankaID = hedefBankaHesap.BankaID,
                HareketTuru = "Giriş",
                IslemTuru = "BankadanBankayaTransfer",
                Tutar = hedefTutar,
                DovizKuru= (1 / model.DovizKuru)??1,
                Tarih = model.Tarih,
                Aciklama = model.Aciklama ?? $"Bankadan bankaya transfer: {kaynakBankaHesap.Banka.BankaAdi} - {kaynakBankaHesap.HesapAdi} -> {hedefBankaHesap.Banka.BankaAdi} - {hedefBankaHesap.HesapAdi}",
                ReferansNo = model.ReferansNo,
                BankaHesapID= kaynakBankaHesap.BankaHesapID,
                DekontNo = model.ReferansNo,
                ReferansTuru = "Transfer",
                TransferID = model.TransferID,
                KarsiParaBirimi = kaynakBankaHesap.ParaBirimi,
                IslemYapanKullaniciID = GetCurrentUserId(),
                OlusturmaTarihi = DateTime.Now
            };

            // Bakiyeleri güncelle
            kaynakBankaHesap.GuncelBakiye -= model.Tutar;
            hedefBankaHesap.GuncelBakiye += hedefTutar;

            // Veritabanına kaydet
            _context.BankaHesapHareketleri.Add(kaynakHareket);
            _context.BankaHesapHareketleri.Add(hedefHareket);
            _context.BankaHesaplari.Update(kaynakBankaHesap);
            _context.BankaHesaplari.Update(hedefBankaHesap);
            await _context.SaveChangesAsync();
        }

        private Guid? GetCurrentUserId()
        {
            var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            return userId != null ? Guid.Parse(userId) : (Guid?)null;
        }

        // GET: /Transfer/GetDovizKuru
        //[HttpGet]
        //public async Task<IActionResult> GetDovizKuru(string fromCurrency, string toCurrency)
        //{
        //    try
        //    {
        //        if (string.IsNullOrEmpty(fromCurrency) || string.IsNullOrEmpty(toCurrency))
        //        {
        //            return Json(new { success = false, message = "Para birimleri belirtilmelidir." });
        //        }

        //        if (fromCurrency == toCurrency)
        //        {
        //            return Json(new { success = true, kurDegeri = 1 });
        //        }

        //        var kurDegeri = await _kurController.GetParaBirimleriArasiKur(fromCurrency, toCurrency);
        //        return Json(new { success = true, kurDegeri = kurDegeri });
        //    }
        //    catch (Exception ex)
        //    {
        //        _logger.LogError(ex, "Döviz kuru alınırken hata oluştu. From: {From}, To: {To}", fromCurrency, toCurrency);
        //        return Json(new { success = false, message = "Döviz kuru alınamadı: " + ex.Message });
        //    }
        //}
    }
} 
