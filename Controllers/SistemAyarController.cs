using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using MuhasebeStokWebApp.Data;
using MuhasebeStokWebApp.Data.Entities;
using MuhasebeStokWebApp.Services.Interfaces;
using MuhasebeStokWebApp.ViewModels.SistemAyar;
using MuhasebeStokWebApp.Models;

namespace MuhasebeStokWebApp.Controllers
{
    [Authorize(Roles = "Admin")]
    public class SistemAyarController : BaseController
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<SistemAyarController> _logger;
        private new readonly ILogService _logService;
        private readonly IEmailService _emailService;
        private readonly ISistemAyarService _sistemAyarService;

        public SistemAyarController(
            ApplicationDbContext context,
            ILogger<SistemAyarController> logger,
            ILogService logService,
            IEmailService emailService,
            ISistemAyarService sistemAyarService) : base(logService: logService)
        {
            _context = context;
            _logger = logger;
            _logService = logService;
            _emailService = emailService;
            _sistemAyarService = sistemAyarService;
        }

        public async Task<IActionResult> Index()
        {
            try
            {
                var ayarlar = await _sistemAyarService.GetAllAsync();
                return View(ayarlar);
            }
            catch (Exception ex)
            {
                await _logService.AddLogAsync("Hata", $"Sistem ayarları sayfası açılırken hata oluştu: {ex.Message}", "SistemAyarController/Index");
                return View("Error", new ErrorViewModel { RequestId = System.Diagnostics.Activity.Current?.Id ?? HttpContext.TraceIdentifier });
            }
        }

        #region Email Ayarları

        public IActionResult EmailAyarlari()
        {
            try
            {
                var model = new EmailAyarlariViewModel();
                
                // Veritabanından e-posta ayarlarını yükleme
                var smtpServer = _context.SistemAyarlari.FirstOrDefault(x => x.Anahtar == "SmtpServer")?.Deger;
                var smtpPort = _context.SistemAyarlari.FirstOrDefault(x => x.Anahtar == "SmtpPort")?.Deger;
                var username = _context.SistemAyarlari.FirstOrDefault(x => x.Anahtar == "SmtpUsername")?.Deger;
                var senderEmail = _context.SistemAyarlari.FirstOrDefault(x => x.Anahtar == "SenderEmail")?.Deger;
                var senderName = _context.SistemAyarlari.FirstOrDefault(x => x.Anahtar == "SenderName")?.Deger;
                var useSsl = _context.SistemAyarlari.FirstOrDefault(x => x.Anahtar == "SmtpUseSsl")?.Deger;
                var adminEmails = _context.SistemAyarlari.FirstOrDefault(x => x.Anahtar == "AdminEmails")?.Deger;

                if (smtpServer != null) model.SmtpServer = smtpServer;
                if (smtpPort != null) model.SmtpPort = int.TryParse(smtpPort, out int port) ? port : 587;
                if (username != null) model.Username = username;
                if (senderEmail != null) model.SenderEmail = senderEmail;
                if (senderName != null) model.SenderName = senderName;
                if (useSsl != null) model.UseSsl = bool.TryParse(useSsl, out bool ssl) ? ssl : true;
                if (adminEmails != null) model.AdminEmails = adminEmails;

                return View(model);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "E-posta ayarları görüntülenirken hata oluştu");
                _logService.LogHata("E-posta ayarları görüntülenirken hata oluştu", ex.Message, User.Identity.Name);
                TempData["ErrorMessage"] = "E-posta ayarları yüklenirken bir hata oluştu: " + ex.Message;
                return View(new EmailAyarlariViewModel());
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EmailAyarlari(EmailAyarlariViewModel model)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    await _sistemAyarService.SaveEmailSettingsAsync(model);
                    TempData["Success"] = "E-posta ayarları başarıyla kaydedildi.";
                    return RedirectToAction(nameof(EmailAyarlari));
                }
                return View(model);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "E-posta ayarları kaydedilirken hata oluştu");
                ModelState.AddModelError("", "E-posta ayarları kaydedilirken bir hata oluştu.");
                return View(model);
            }
        }

        public async Task<IActionResult> TestEmail(string email)
        {
            if (string.IsNullOrEmpty(email))
            {
                return Json(new { success = false, message = "E-posta adresi boş olamaz." });
            }

            try
            {
                // E-posta gönderme işlemi
                var result = await _emailService.SendEmailAsync(
                    email,
                    "Test E-postası",
                    "<h1>Muhasebe ve Stok Takip Sistemi</h1><p>Bu bir test e-postasıdır. E-posta ayarlarınız doğru çalışıyor.</p>");

                if (result)
                {
                    _logService.LogBilgi("Test e-postası gönderildi", $"{email} adresine test e-postası gönderildi", User.Identity.Name);
                    return Json(new { success = true, message = "Test e-postası başarıyla gönderildi." });
                }
                else
                {
                    return Json(new { success = false, message = "E-posta gönderilemedi. Ayarlarınızı kontrol edin." });
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Test e-postası gönderilirken hata oluştu");
                _logService.LogHata("Test e-postası gönderilirken hata oluştu", ex.Message, User.Identity.Name);
                return Json(new { success = false, message = "E-posta gönderilirken bir hata oluştu: " + ex.Message });
            }
        }

        #endregion

        #region Bildirim Ayarları

        public IActionResult BildirimAyarlari()
        {
            try
            {
                var model = new BildirimAyarlariViewModel();
                
                // Veritabanından bildirim ayarlarını yükleme
                var bildirimAktif = _context.SistemAyarlari.FirstOrDefault(x => x.Anahtar == "BildirimAktif")?.Deger;
                var emailBildirimAktif = _context.SistemAyarlari.FirstOrDefault(x => x.Anahtar == "EmailBildirimAktif")?.Deger;
                var pushBildirimAktif = _context.SistemAyarlari.FirstOrDefault(x => x.Anahtar == "PushBildirimAktif")?.Deger;
                var otomatikBildirimler = _context.SistemAyarlari.FirstOrDefault(x => x.Anahtar == "OtomatikBildirimler")?.Deger;
                var bildirimOzetiSikligi = _context.SistemAyarlari.FirstOrDefault(x => x.Anahtar == "BildirimOzetiSikligi")?.Deger;
                
                var kritikOlayBildirimleri = _context.SistemAyarlari.FirstOrDefault(x => x.Anahtar == "KritikOlayBildirimleri")?.Deger;
                var stokUyariBildirimleri = _context.SistemAyarlari.FirstOrDefault(x => x.Anahtar == "StokUyariBildirimleri")?.Deger;
                var tahsilatOdemeBildirimleri = _context.SistemAyarlari.FirstOrDefault(x => x.Anahtar == "TahsilatOdemeBildirimleri")?.Deger;
                var yeniSiparisBildirimleri = _context.SistemAyarlari.FirstOrDefault(x => x.Anahtar == "YeniSiparisBildirimleri")?.Deger;

                if (bildirimAktif != null) model.BildirimAktif = bool.TryParse(bildirimAktif, out bool bAktif) ? bAktif : false;
                if (emailBildirimAktif != null) model.EmailBildirimAktif = bool.TryParse(emailBildirimAktif, out bool eAktif) ? eAktif : false;
                if (pushBildirimAktif != null) model.PushBildirimAktif = bool.TryParse(pushBildirimAktif, out bool pAktif) ? pAktif : false;
                if (otomatikBildirimler != null) model.OtomatikBildirimler = bool.TryParse(otomatikBildirimler, out bool oAktif) ? oAktif : false;
                if (bildirimOzetiSikligi != null) model.BildirimOzetiSikligi = bildirimOzetiSikligi;
                
                if (kritikOlayBildirimleri != null) model.KritikOlayBildirimleri = bool.TryParse(kritikOlayBildirimleri, out bool kAktif) ? kAktif : true;
                if (stokUyariBildirimleri != null) model.StokUyariBildirimleri = bool.TryParse(stokUyariBildirimleri, out bool sAktif) ? sAktif : true;
                if (tahsilatOdemeBildirimleri != null) model.TahsilatOdemeBildirimleri = bool.TryParse(tahsilatOdemeBildirimleri, out bool tAktif) ? tAktif : true;
                if (yeniSiparisBildirimleri != null) model.YeniSiparisBildirimleri = bool.TryParse(yeniSiparisBildirimleri, out bool yAktif) ? yAktif : true;

                return View(model);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Bildirim ayarları görüntülenirken hata oluştu");
                _logService.LogHata("Bildirim ayarları görüntülenirken hata oluştu", ex.Message, User.Identity.Name);
                TempData["ErrorMessage"] = "Bildirim ayarları yüklenirken bir hata oluştu: " + ex.Message;
                return View(new BildirimAyarlariViewModel());
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> BildirimAyarlari(BildirimAyarlariViewModel model)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    await _sistemAyarService.SaveNotificationSettingsAsync(model);
                    TempData["Success"] = "Bildirim ayarları başarıyla kaydedildi.";
                    return RedirectToAction(nameof(BildirimAyarlari));
                }
                return View(model);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Bildirim ayarları kaydedilirken hata oluştu");
                ModelState.AddModelError("", "Bildirim ayarları kaydedilirken bir hata oluştu.");
                return View(model);
            }
        }

        #endregion

        #region Dil Ayarları

        public IActionResult DilAyarlari()
        {
            try
            {
                var model = new DilAyarlariViewModel();
                
                // Veritabanından dil ayarlarını yükleme
                var varsayilanDil = _context.SistemAyarlari.FirstOrDefault(x => x.Anahtar == "VarsayilanDil")?.Deger;
                var paraBirimiFormat = _context.SistemAyarlari.FirstOrDefault(x => x.Anahtar == "ParaBirimiFormat")?.Deger;
                var tarihFormat = _context.SistemAyarlari.FirstOrDefault(x => x.Anahtar == "TarihFormat")?.Deger;
                var saatFormat = _context.SistemAyarlari.FirstOrDefault(x => x.Anahtar == "SaatFormat")?.Deger;
                var cokluDilDestegi = _context.SistemAyarlari.FirstOrDefault(x => x.Anahtar == "CokluDilDestegi")?.Deger;
                var aktifDiller = _context.SistemAyarlari.FirstOrDefault(x => x.Anahtar == "AktifDiller")?.Deger;

                if (varsayilanDil != null) model.VarsayilanDil = varsayilanDil;
                if (paraBirimiFormat != null) model.ParaBirimiFormat = paraBirimiFormat;
                if (tarihFormat != null) model.TarihFormat = tarihFormat;
                if (saatFormat != null) model.SaatFormat = saatFormat;
                if (cokluDilDestegi != null) model.CokluDilDestegi = bool.TryParse(cokluDilDestegi, out bool cdd) ? cdd : false;
                if (aktifDiller != null) model.AktifDiller = aktifDiller.Split(';').ToList();

                return View(model);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Dil ayarları görüntülenirken hata oluştu");
                _logService.LogHata("Dil ayarları görüntülenirken hata oluştu", ex.Message, User.Identity.Name);
                TempData["ErrorMessage"] = "Dil ayarları yüklenirken bir hata oluştu: " + ex.Message;
                return View(new DilAyarlariViewModel());
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DilAyarlari(DilAyarlariViewModel model)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    await _sistemAyarService.SaveLanguageSettingsAsync(model);
                    TempData["Success"] = "Dil ayarları başarıyla kaydedildi.";
                    return RedirectToAction(nameof(DilAyarlari));
                }
                return View(model);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Dil ayarları kaydedilirken hata oluştu");
                ModelState.AddModelError("", "Dil ayarları kaydedilirken bir hata oluştu.");
                return View(model);
            }
        }

        #endregion

        #region Helper Methods

        private void UpsertAyar(string anahtar, string deger)
        {
            var ayar = _context.SistemAyarlari.FirstOrDefault(x => x.Anahtar == anahtar);
            
            if (ayar == null)
            {
                // Ayar yoksa ekle
                _context.SistemAyarlari.Add(new SistemAyar
                {
                    Anahtar = anahtar,
                    Deger = deger,
                    Aciklama = anahtar,
                    OlusturmaTarihi = DateTime.Now,
                    GuncellemeTarihi = DateTime.Now,
                    Silindi = false
                });
            }
            else
            {
                // Ayar varsa güncelle
                ayar.Deger = deger;
                ayar.GuncellemeTarihi = DateTime.Now;
            }
        }

        #endregion
    }
} 