using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using MuhasebeStokWebApp.Models;
using MuhasebeStokWebApp.Data.Repositories;
using MuhasebeStokWebApp.Data.Entities;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Newtonsoft.Json;
using Microsoft.AspNetCore.Authorization;
using MuhasebeStokWebApp.Services;
using MuhasebeStokWebApp.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;

namespace MuhasebeStokWebApp.Controllers
{
    [Authorize]  // Bu controller'a erişim için kimlik doğrulama gerektirir
    public class HomeController : BaseController
    {
        private readonly ILogger<HomeController> _logger;
        private readonly IUnitOfWork _unitOfWork;
        private readonly UserManager<IdentityUser> _userManager;
        private readonly IDovizKuruService _dovizKuruService;
        private readonly ApplicationDbContext _context;
        private readonly RoleManager<IdentityRole> _roleManager;

        public HomeController(
            ILogger<HomeController> logger, 
            IUnitOfWork unitOfWork,
            UserManager<IdentityUser> userManager,
            RoleManager<IdentityRole> roleManager,
            IMenuService menuService,
            IDovizKuruService dovizKuruService,
            ApplicationDbContext context,
            ILogService logService) : base(menuService, userManager, roleManager, logService)
        {
            _logger = logger;
            _unitOfWork = unitOfWork;
            _userManager = userManager;
            _dovizKuruService = dovizKuruService;
            _context = context;
            _roleManager = roleManager;
        }

        public async Task<IActionResult> Index()
        {
            // Kullanıcı giriş yapmış mı kontrol et
            /*if (User.Identity?.IsAuthenticated != true)
            {
                return RedirectToAction("Login", "Account");
            }*/

            try
            {
                // Toplam cari sayısı
                var cariler = await _unitOfWork.CariRepository.GetAllAsync();
                ViewBag.ToplamCariSayisi = cariler.Count();
                
                // Toplam ürün sayısı
                var urunler = await _unitOfWork.UrunRepository.GetAllAsync();
                ViewBag.ToplamUrunSayisi = urunler.Count();
                
                // Toplam fatura sayısı
                var faturalar = await _unitOfWork.FaturaRepository.GetAllAsync();
                ViewBag.ToplamFaturaSayisi = faturalar.Count();
                
                // Toplam ciro (satış faturalarının toplamı)
                var satisFaturalari = faturalar.Where(f => f.FaturaTuru?.FaturaTuruAdi == "Satış").ToList();
                decimal toplamCiro = satisFaturalari.Sum(f => f.GenelToplam ?? 0);
                ViewBag.ToplamCiro = toplamCiro;
                
                // Son döviz kurlarını al
                ViewBag.SonKurlar = await _dovizKuruService.GetLatestRatesAsync(5);
                
                // Aylık satış ve gider verileri
                var aylikSatisVerileri = new decimal[12];
                var aylikGiderVerileri = new decimal[12];
                
                var buYil = DateTime.Now.Year;
                
                // Satış faturaları
                var yillikSatisFaturalari = faturalar
                    .Where(f => f.FaturaTuru?.FaturaTuruAdi == "Satış" && f.FaturaTarihi?.Year == buYil)
                    .ToList();
                
                // Alış faturaları
                var yillikAlisFaturalari = faturalar
                    .Where(f => f.FaturaTuru?.FaturaTuruAdi == "Alış" && f.FaturaTarihi?.Year == buYil)
                    .ToList();
                
                // Aylık verileri hesapla
                for (int ay = 1; ay <= 12; ay++)
                {
                    aylikSatisVerileri[ay - 1] = yillikSatisFaturalari
                        .Where(f => f.FaturaTarihi?.Month == ay)
                        .Sum(f => f.GenelToplam ?? 0);
                    
                    aylikGiderVerileri[ay - 1] = yillikAlisFaturalari
                        .Where(f => f.FaturaTarihi?.Month == ay)
                        .Sum(f => f.GenelToplam ?? 0);
                }
                
                ViewBag.AylikSatisVerileri = JsonConvert.SerializeObject(aylikSatisVerileri);
                ViewBag.AylikGiderVerileri = JsonConvert.SerializeObject(aylikGiderVerileri);
                
                // Cari bakiye dağılımı
                // Cari bakiyelerini hesaplamak için CariHareket tablosunu kullanmamız gerekiyor
                var cariHareketler = await _unitOfWork.Repository<CariHareket>().GetAllAsync();
                
                // Her cari için bakiye hesapla
                var cariBakiyeleri = new Dictionary<Guid, decimal>();
                
                foreach (var cari in cariler)
                {
                    var hareketler = cariHareketler.Where(h => h.CariID == cari.CariID).ToList();
                    decimal bakiye = 0;
                    
                    foreach (var hareket in hareketler)
                    {
                        if (hareket.HareketTuru == "Borç")
                        {
                            bakiye -= hareket.Tutar;
                        }
                        else if (hareket.HareketTuru == "Alacak")
                        {
                            bakiye += hareket.Tutar;
                        }
                    }
                    
                    cariBakiyeleri[cari.CariID] = bakiye;
                }
                
                int borcluCariSayisi = cariBakiyeleri.Count(c => c.Value < 0);
                int alacakliCariSayisi = cariBakiyeleri.Count(c => c.Value > 0);
                int sifirBakiyeCariSayisi = cariBakiyeleri.Count(c => c.Value == 0);
                
                ViewBag.CariBakiyeTipleri = JsonConvert.SerializeObject(new[] { "Borçlu", "Alacaklı", "Sıfır Bakiye" });
                ViewBag.CariBakiyeDagilimi = JsonConvert.SerializeObject(new[] { borcluCariSayisi, alacakliCariSayisi, sifirBakiyeCariSayisi });
                
                // Stok kategorileri ve miktarları
                var kategoriRepository = _unitOfWork.Repository<UrunKategori>();
                var kategoriler = await kategoriRepository.GetAllAsync();
                
                var kategoriAdlari = new List<string>();
                var stokMiktarlari = new List<decimal>();
                
                foreach (var kategori in kategoriler)
                {
                    kategoriAdlari.Add(kategori.KategoriAdi);
                    stokMiktarlari.Add(urunler.Where(u => u.KategoriID == kategori.KategoriID).Sum(u => u.StokMiktar));
                }
                
                ViewBag.StokKategorileri = JsonConvert.SerializeObject(kategoriAdlari);
                ViewBag.StokMiktarlari = JsonConvert.SerializeObject(stokMiktarlari);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Dashboard verileri yüklenirken hata oluştu");
                ViewBag.ErrorMessage = "Dashboard verileri yüklenirken bir hata oluştu: " + ex.Message;
                
                // Hata durumunda varsayılan değerler
                ViewBag.ToplamCariSayisi = 0;
                ViewBag.ToplamUrunSayisi = 0;
                ViewBag.ToplamFaturaSayisi = 0;
                ViewBag.ToplamCiro = 0;
                
                ViewBag.AylikSatisVerileri = JsonConvert.SerializeObject(new decimal[12]);
                ViewBag.AylikGiderVerileri = JsonConvert.SerializeObject(new decimal[12]);
                
                ViewBag.CariBakiyeTipleri = JsonConvert.SerializeObject(new[] { "Borçlu", "Alacaklı", "Sıfır Bakiye" });
                ViewBag.CariBakiyeDagilimi = JsonConvert.SerializeObject(new[] { 0, 0, 0 });
                
                ViewBag.StokKategorileri = JsonConvert.SerializeObject(new string[0]);
                ViewBag.StokMiktarlari = JsonConvert.SerializeObject(new int[0]);
            }
            
            return View();
        }

        [AllowAnonymous]
        public IActionResult Privacy()
        {
            return View();
        }

        [AllowAnonymous]
        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }

        [AllowAnonymous]
        public IActionResult StatusCode(int code)
        {
            switch (code)
            {
                case 404:
                    return View("NotFound");
                case 403:
                    return View("Forbidden");
                default:
                    return View("Error", new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
            }
        }
    }
}
