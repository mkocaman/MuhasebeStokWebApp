using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using MuhasebeStokWebApp.Data;
using MuhasebeStokWebApp.ViewModels.Kullanici;

namespace MuhasebeStokWebApp.Controllers
{
    // [Authorize]
    public class KullaniciController : Controller
    {
        public IActionResult Index()
        {
            // Identity servisleri kaldırıldığı için bu controller geçici olarak devre dışı bırakıldı
            return View("NotAvailable");
        }

        public IActionResult Create()
        {
            // Identity servisleri kaldırıldığı için bu controller geçici olarak devre dışı bırakıldı
            return View("NotAvailable");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create(KullaniciCreateViewModel model)
        {
            // Identity servisleri kaldırıldığı için bu controller geçici olarak devre dışı bırakıldı
            return View("NotAvailable");
        }

        public IActionResult Edit(string id)
        {
            // Identity servisleri kaldırıldığı için bu controller geçici olarak devre dışı bırakıldı
            return View("NotAvailable");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Edit(KullaniciEditViewModel model)
        {
            // Identity servisleri kaldırıldığı için bu controller geçici olarak devre dışı bırakıldı
            return View("NotAvailable");
        }

        public IActionResult Details(string id)
        {
            // Identity servisleri kaldırıldığı için bu controller geçici olarak devre dışı bırakıldı
            return View("NotAvailable");
        }

        public IActionResult Delete(string id)
        {
            // Identity servisleri kaldırıldığı için bu controller geçici olarak devre dışı bırakıldı
            return View("NotAvailable");
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public IActionResult DeleteConfirmed(string id)
        {
            // Identity servisleri kaldırıldığı için bu controller geçici olarak devre dışı bırakıldı
            return View("NotAvailable");
        }
    }
} 