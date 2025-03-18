using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using MuhasebeStokWebApp.Data;
using MuhasebeStokWebApp.Data.Entities;
using MuhasebeStokWebApp.ViewModels;
using MuhasebeStokWebApp.ViewModels.Kur;
using MuhasebeStokWebApp.Services;

namespace MuhasebeStokWebApp.Controllers
{
    public class DovizKuruController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IKurService _kurService;

        public DovizKuruController(ApplicationDbContext context, IKurService kurService)
        {
            _context = context;
            _kurService = kurService;
        }

        // GET: DovizKuru
        public IActionResult Index()
        {
            // Kur modülünün Index sayfasına yönlendir
            return RedirectToAction("Index", "Kur");
        }

        // GET: DovizKuru/Liste
        public IActionResult Liste()
        {
            // Kur modülünün Index sayfasına yönlendir
            return RedirectToAction("Index", "Kur");
        }

        // GET: DovizKuru/Detay/5
        public IActionResult Detay(Guid? id)
        {
            // Kur modülünün ilgili sayfasına yönlendir
            return RedirectToAction("Index", "Kur");
        }

        // GET: DovizKuru/Ekle
        public IActionResult Ekle()
        {
            // Kur modülünün KurDegeriEkle sayfasına yönlendir
            return RedirectToAction("KurDegeriEkle", "Kur");
        }

        // POST: DovizKuru/Ekle
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Ekle(KurDegeriViewModel model)
        {
            // Direk Kur modülüne yönlendir
            return RedirectToAction("KurDegeriEkle", "Kur");
        }

        // GET: DovizKuru/Duzenle/5
        public IActionResult Duzenle(Guid? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            // Kur modülünün düzenleme sayfasına yönlendir
            return RedirectToAction("KurDegeriDuzenle", "Kur", new { id });
        }

        // POST: DovizKuru/Duzenle/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Duzenle(Guid id, KurDegeriViewModel model)
        {
            // Direk Kur modülünün düzenleme işlemine yönlendir
            return RedirectToAction("KurDegeriDuzenle", "Kur", new { id });
        }

        // GET: DovizKuru/Sil/5
        public IActionResult Sil(Guid? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            // Kur modülünün silme sayfasına yönlendir
            return RedirectToAction("KurDegeriSil", "Kur", new { id });
        }

        // POST: DovizKuru/Sil/5
        [HttpPost, ActionName("Sil")]
        [ValidateAntiForgeryToken]
        public IActionResult SilOnay(Guid id)
        {
            // Direk Kur modülünün silme işlemine yönlendir
            return RedirectToAction("KurDegeriSil", "Kur", new { id });
        }

        private bool KurDegeriExists(Guid id)
        {
            return _context.KurDegerleri.Any(e => e.KurDegeriID == id);
        }
    }
} 