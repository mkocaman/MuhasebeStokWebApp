using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MuhasebeStokWebApp.Data;
using MuhasebeStokWebApp.Data.Entities;
using MuhasebeStokWebApp.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Logging;
using System.Linq;
using MuhasebeStokWebApp.Services;
using MuhasebeStokWebApp.Services.Auth;
using MuhasebeStokWebApp.Services.Interfaces;
using MuhasebeStokWebApp.Data.Entities.DovizModulu;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;

namespace MuhasebeStokWebApp.Controllers
{
    [Authorize(Roles = "Admin")]
    [Route("DbInit")]
    public class DbInitController : BaseController
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<DbInitController> _logger;
        private readonly ILogService _logService;
        private readonly IParaBirimiService _paraBirimiService;
        private readonly ISistemAyarService _sistemAyarService;
        private readonly IAuthService _authService;
        private readonly UserManager<ApplicationUser> _identityUserManager;
        private readonly RoleManager<IdentityRole> _identityRoleManager;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IConfiguration _configuration;
        private readonly IBirimService _birimService;
        private readonly IMenuService _menuService;

        public DbInitController(
            ApplicationDbContext context,
            ILogger<DbInitController> logger,
            ILogService logService,
            IParaBirimiService paraBirimiService,
            ISistemAyarService sistemAyarService,
            IAuthService authService,
            UserManager<ApplicationUser> identityUserManager,
            RoleManager<IdentityRole> identityRoleManager,
            UserManager<ApplicationUser> userManager,
            IConfiguration configuration,
            IBirimService birimService,
            IMenuService menuService)
            : base(menuService, identityUserManager, identityRoleManager, logService)
        {
            _context = context;
            _logger = logger;
            _logService = logService;
            _paraBirimiService = paraBirimiService;
            _sistemAyarService = sistemAyarService;
            _authService = authService;
            _identityUserManager = identityUserManager;
            _identityRoleManager = identityRoleManager;
            _userManager = userManager;
            _configuration = configuration;
            _birimService = birimService;
            _menuService = menuService;
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            try
            {
                // Mevcut verilerin durumunu kontrol et
                ViewBag.SistemAyarlari = await _context.GenelSistemAyarlari.Where(s => s.Aktif && !s.SoftDelete).CountAsync();
                ViewBag.ParaBirimleri = await _context.ParaBirimleri.CountAsync();
                ViewBag.KurDegerleri = await _context.KurDegerleri.CountAsync();
                
                return View();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "DbInit/Index sayfası yüklenirken hata oluştu");
                await _logService.LogErrorAsync("DbInitController.Index", $"Hata oluştu: {ex.Message}");
                return View("Error");
            }
        }

        [HttpGet("InitParaBirimleri")]
        public async Task<IActionResult> InitParaBirimleri()
        {
            try
            {
                // Para birimlerinin zaten var olup olmadığını kontrol et
                if (await _context.ParaBirimleri.AnyAsync())
                {
                    TempData["Message"] = "Para birimleri zaten mevcut!";
                    TempData["MessageType"] = "warning";
                    return RedirectToAction("Index");
                }

                // Yeni para birimleri oluştur
                var paraBirimleri = new List<MuhasebeStokWebApp.Data.Entities.DovizModulu.ParaBirimi>();
                
                var usd = new MuhasebeStokWebApp.Data.Entities.DovizModulu.ParaBirimi
                {
                    ParaBirimiID = Guid.NewGuid(),
                    Kod = "USD",
                    Ad = "ABD Doları",
                    Sembol = "$",
                    OndalikAyraci = ".",
                    BinlikAyraci = ",",
                    OndalikHassasiyet = 2,
                    Aktif = true,
                    Sira = 2
                };
                
                var try_ = new MuhasebeStokWebApp.Data.Entities.DovizModulu.ParaBirimi
                {
                    ParaBirimiID = Guid.NewGuid(),
                    Kod = "TRY",
                    Ad = "Türk Lirası",
                    Sembol = "₺",
                    OndalikAyraci = ",",
                    BinlikAyraci = ".",
                    OndalikHassasiyet = 2,
                    AnaParaBirimiMi = true,
                    Aktif = true,
                    Sira = 1
                };
                
                var eur = new MuhasebeStokWebApp.Data.Entities.DovizModulu.ParaBirimi
                {
                    ParaBirimiID = Guid.NewGuid(),
                    Kod = "EUR",
                    Ad = "Euro",
                    Sembol = "€",
                    OndalikAyraci = ".",
                    BinlikAyraci = ",",
                    OndalikHassasiyet = 2,
                    Aktif = true,
                    Sira = 3
                };
                
                var gbp = new MuhasebeStokWebApp.Data.Entities.DovizModulu.ParaBirimi
                {
                    ParaBirimiID = Guid.NewGuid(),
                    Kod = "GBP",
                    Ad = "İngiliz Sterlini",
                    Sembol = "£",
                    OndalikAyraci = ".",
                    BinlikAyraci = ",",
                    OndalikHassasiyet = 2,
                    Aktif = true,
                    Sira = 4
                };
                
                paraBirimleri.Add(try_);
                paraBirimleri.Add(usd);
                paraBirimleri.Add(eur);
                paraBirimleri.Add(gbp);

                // Para birimlerini veritabanına ekle
                await _context.ParaBirimleri.AddRangeAsync(paraBirimleri);
                await _context.SaveChangesAsync();

                TempData["Message"] = "Para birimleri başarıyla oluşturuldu!";
                TempData["MessageType"] = "success";
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Para birimleri oluşturulurken hata oluştu");
                await _logService.LogErrorAsync("DbInitController.InitParaBirimleri", $"Hata oluştu: {ex.Message}");
                TempData["Message"] = $"Hata oluştu: {ex.Message}";
                TempData["MessageType"] = "danger";
                return RedirectToAction("Index");
            }
        }

        [HttpGet("InitSistemAyarlari")]
        public async Task<IActionResult> InitSistemAyarlari()
        {
            try
            {
                // Sistem ayarları zaten var mı kontrol et
                if (await _context.GenelSistemAyarlari.AnyAsync())
                {
                    TempData["Message"] = "Sistem ayarları zaten mevcut!";
                    TempData["MessageType"] = "warning";
                    return RedirectToAction("Index");
                }

                // Temel sistem ayarlarını oluştur
                var sistemAyarlari = new List<SistemAyarlari>
                {
                    new SistemAyarlari 
                    { 
                        AnaDovizKodu = "TRY",
                        SirketAdi = "Şirketim",
                        SirketAdresi = "İstanbul, Türkiye",
                        SirketTelefon = "+90 212 123 4567",
                        SirketEmail = "info@muhasebe-stok.com",
                        SirketVergiNo = "1234567890",
                        SirketVergiDairesi = "İstanbul Vergi Dairesi",
                        OtomatikDovizGuncelleme = true,
                        DovizGuncellemeSikligi = 24,
                        SonDovizGuncellemeTarihi = DateTime.Now,
                        Aktif = true,
                        SoftDelete = false,
                        OlusturmaTarihi = DateTime.Now
                    },
                    new SistemAyarlari 
                    { 
                        AnaDovizKodu = "USD",
                        SirketAdi = "Şirketim",
                        SirketAdresi = "İstanbul, Türkiye",
                        SirketTelefon = "+90 212 123 4567",
                        SirketEmail = "info@muhasebe-stok.com",
                        SirketVergiNo = "1234567890",
                        SirketVergiDairesi = "İstanbul Vergi Dairesi",
                        OtomatikDovizGuncelleme = true,
                        DovizGuncellemeSikligi = 24,
                        SonDovizGuncellemeTarihi = DateTime.Now,
                        Aktif = true,
                        SoftDelete = false,
                        OlusturmaTarihi = DateTime.Now
                    },
                    new SistemAyarlari 
                    { 
                        AnaDovizKodu = "EUR",
                        SirketAdi = "Şirketim",
                        SirketAdresi = "İstanbul, Türkiye",
                        SirketTelefon = "+90 212 123 4567",
                        SirketEmail = "info@muhasebe-stok.com",
                        SirketVergiNo = "1234567890",
                        SirketVergiDairesi = "İstanbul Vergi Dairesi",
                        OtomatikDovizGuncelleme = true,
                        DovizGuncellemeSikligi = 24,
                        SonDovizGuncellemeTarihi = DateTime.Now,
                        Aktif = true,
                        SoftDelete = false,
                        OlusturmaTarihi = DateTime.Now
                    },
                    new SistemAyarlari 
                    { 
                        AnaDovizKodu = "GBP",
                        SirketAdi = "Şirketim",
                        SirketAdresi = "İstanbul, Türkiye",
                        SirketTelefon = "+90 212 123 4567",
                        SirketEmail = "info@muhasebe-stok.com",
                        SirketVergiNo = "1234567890",
                        SirketVergiDairesi = "İstanbul Vergi Dairesi",
                        OtomatikDovizGuncelleme = true,
                        DovizGuncellemeSikligi = 24,
                        SonDovizGuncellemeTarihi = DateTime.Now,
                        Aktif = true,
                        SoftDelete = false,
                        OlusturmaTarihi = DateTime.Now
                    },
                    new SistemAyarlari 
                    { 
                        AnaDovizKodu = "UZS",
                        SirketAdi = "Şirketim",
                        SirketAdresi = "İstanbul, Türkiye",
                        SirketTelefon = "+90 212 123 4567",
                        SirketEmail = "info@muhasebe-stok.com",
                        SirketVergiNo = "1234567890",
                        SirketVergiDairesi = "İstanbul Vergi Dairesi",
                        OtomatikDovizGuncelleme = true,
                        DovizGuncellemeSikligi = 24,
                        SonDovizGuncellemeTarihi = DateTime.Now,
                        Aktif = true,
                        SoftDelete = false,
                        OlusturmaTarihi = DateTime.Now
                    }
                };

                await _context.GenelSistemAyarlari.AddRangeAsync(sistemAyarlari);
                await _context.SaveChangesAsync();

                TempData["Message"] = "Sistem ayarları başarıyla oluşturuldu!";
                TempData["MessageType"] = "success";
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "InitSistemAyarlari işlemi sırasında hata oluştu");
                await _logService.LogErrorAsync("DbInitController.InitSistemAyarlari", $"Hata oluştu: {ex.Message}");
                
                TempData["Message"] = $"Hata oluştu: {ex.Message}";
                TempData["MessageType"] = "danger";
            }

            return RedirectToAction("Index");
        }

        [HttpGet("InitKurDegerleri")]
        public async Task<IActionResult> InitKurDegerleri()
        {
            try
            {
                // Kur değerlerinin zaten var olup olmadığını kontrol et
                if (await _context.KurDegerleri.AnyAsync())
                {
                    TempData["Message"] = "Kur değerleri zaten mevcut!";
                    TempData["MessageType"] = "warning";
                    return RedirectToAction("Index");
                }

                // Para birimlerini kontrol et
                var paraBirimleri = await _context.ParaBirimleri.ToListAsync();
                if (!paraBirimleri.Any())
                {
                    TempData["Message"] = "Önce para birimleri oluşturulmalıdır!";
                    TempData["MessageType"] = "warning";
                    return RedirectToAction("Index");
                }

                // TCMB'den güncel kur değerlerini çekmek için DovizKuruService kullanılmalı
                // Bu işlem için kullanıcıya bilgilendirme mesajı göster
                TempData["Message"] = "Kur değerleri manuel olarak oluşturulmalıdır. Lütfen Döviz Kuru bölümünden TCMB'den kur değerlerini güncelleyin.";
                TempData["MessageType"] = "info";
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Kur değerleri oluşturulurken hata oluştu");
                await _logService.LogErrorAsync("DbInitController.InitKurDegerleri", $"Hata oluştu: {ex.Message}");
                TempData["Message"] = $"Hata oluştu: {ex.Message}";
                TempData["MessageType"] = "danger";
                return RedirectToAction("Index");
            }
        }

        [HttpGet("InitAll")]
        public async Task<IActionResult> InitAll()
        {
            try
            {
                // Tüm başlatmaları sırayla çağır
                await InitSistemAyarlari();
                await InitParaBirimleri();
                await InitKurDegerleri();

                TempData["Message"] = "Tüm başlatmalar tamamlandı!";
                TempData["MessageType"] = "success";
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Tüm başlatmalar yapılırken hata oluştu");
                await _logService.LogErrorAsync("DbInitController.InitAll", $"Hata oluştu: {ex.Message}");
                TempData["Message"] = $"Hata oluştu: {ex.Message}";
                TempData["MessageType"] = "danger";
                return RedirectToAction("Index");
            }
        }
    }
} 