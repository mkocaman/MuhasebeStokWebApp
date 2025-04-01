using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using MuhasebeStokWebApp.Data;
using MuhasebeStokWebApp.Data.Entities;
using MuhasebeStokWebApp.Services;
using MuhasebeStokWebApp.ViewModels.Kullanici;
using MuhasebeStokWebApp.Services.Interfaces;
using Microsoft.Extensions.Logging;

namespace MuhasebeStokWebApp.Controllers
{
    [Authorize(Roles = "Admin")]
    public class KullaniciController : BaseController
    {
        protected new readonly UserManager<ApplicationUser> _userManager;
        protected new readonly RoleManager<IdentityRole> _roleManager;
        private readonly ApplicationDbContext _context;
        private readonly IConfiguration _configuration;
        private readonly IMenuService _menuService;
        private readonly ILogger<KullaniciController> _logger;
        private new readonly ILogService _logService;

        public KullaniciController(
            UserManager<ApplicationUser> userManager,
            RoleManager<IdentityRole> roleManager,
            ApplicationDbContext context,
            IConfiguration configuration,
            IMenuService menuService,
            ILogger<KullaniciController> logger,
            ILogService logService)
            : base(menuService, userManager, roleManager, logService)
        {
            _userManager = userManager;
            _roleManager = roleManager;
            _context = context;
            _configuration = configuration;
            _menuService = menuService;
            _logger = logger;
            _logService = logService;
        }

        public async Task<IActionResult> Index()
        {
            var users = await _userManager.Users.ToListAsync();
            var viewModels = new List<KullaniciViewModel>();

            foreach (var user in users)
            {
                var roles = await _userManager.GetRolesAsync(user);
                viewModels.Add(new KullaniciViewModel
                {
                    Id = user.Id,
                    KullaniciAdi = user.UserName,
                    Email = user.Email,
                    Roller = string.Join(", ", roles)
                });
            }

            return View(viewModels);
        }

        public async Task<IActionResult> Create()
        {
            var roles = await _roleManager.Roles.ToListAsync();
            ViewBag.Roles = new MultiSelectList(roles, "Name", "Name");
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(KullaniciCreateViewModel model)
        {
            if (ModelState.IsValid)
            {
                var user = new ApplicationUser
                {
                    UserName = model.KullaniciAdi,
                    Email = model.Email
                };

                var result = await _userManager.CreateAsync(user, model.Password);

                if (result.Succeeded)
                {
                    if (model.SelectedRoles != null && model.SelectedRoles.Any())
                    {
                        await _userManager.AddToRolesAsync(user, model.SelectedRoles);
                    }

                    await _logService.AddLogAsync("Kullanıcı", "Ekleme", $"{user.UserName} kullanıcısı oluşturuldu.");
                    TempData["SuccessMessage"] = "Kullanıcı başarıyla oluşturuldu.";
                    return RedirectToAction(nameof(Index));
                }

                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError("", error.Description);
                }
            }

            var roles = await _roleManager.Roles.ToListAsync();
            ViewBag.Roles = new MultiSelectList(roles, "Name", "Name", model.SelectedRoles);
            return View(model);
        }

        public async Task<IActionResult> Edit(string id)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user == null)
            {
                return NotFound();
            }

            var userRoles = await _userManager.GetRolesAsync(user);
            var roles = await _roleManager.Roles.ToListAsync();

            var model = new KullaniciEditViewModel
            {
                Id = user.Id,
                KullaniciAdi = user.UserName,
                Email = user.Email,
                SelectedRoles = userRoles.ToList()
            };

            ViewBag.Roles = new MultiSelectList(roles, "Name", "Name", userRoles);
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(KullaniciEditViewModel model)
        {
            if (ModelState.IsValid)
            {
                var user = await _userManager.FindByIdAsync(model.Id);
                if (user == null)
                {
                    return NotFound();
                }

                user.UserName = model.KullaniciAdi;
                user.Email = model.Email;

                var result = await _userManager.UpdateAsync(user);

                if (!string.IsNullOrEmpty(model.Password))
                {
                    var token = await _userManager.GeneratePasswordResetTokenAsync(user);
                    result = await _userManager.ResetPasswordAsync(user, token, model.Password);
                }

                if (result.Succeeded)
                {
                    var userRoles = await _userManager.GetRolesAsync(user);
                    await _userManager.RemoveFromRolesAsync(user, userRoles);

                    if (model.SelectedRoles != null && model.SelectedRoles.Any())
                    {
                        await _userManager.AddToRolesAsync(user, model.SelectedRoles);
                    }

                    await _logService.AddLogAsync("Kullanıcı", "Güncelleme", $"{user.UserName} kullanıcısı güncellendi.");
                    TempData["SuccessMessage"] = "Kullanıcı başarıyla güncellendi.";
                    return RedirectToAction(nameof(Index));
                }

                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError("", error.Description);
                }
            }

            var roles = await _roleManager.Roles.ToListAsync();
            ViewBag.Roles = new MultiSelectList(roles, "Name", "Name", model.SelectedRoles);
            return View(model);
        }

        public async Task<IActionResult> Details(string id)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user == null)
            {
                return NotFound();
            }

            var roles = await _userManager.GetRolesAsync(user);
            var viewModel = new KullaniciViewModel
            {
                Id = user.Id,
                KullaniciAdi = user.UserName,
                Email = user.Email,
                Roller = string.Join(", ", roles)
            };

            return View(viewModel);
        }

        public async Task<IActionResult> Delete(string id)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user == null)
            {
                return NotFound();
            }

            var roles = await _userManager.GetRolesAsync(user);
            var viewModel = new KullaniciViewModel
            {
                Id = user.Id,
                KullaniciAdi = user.UserName,
                Email = user.Email,
                Roller = string.Join(", ", roles)
            };

            return View(viewModel);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(string id)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user == null)
            {
                return NotFound();
            }

            var result = await _userManager.DeleteAsync(user);
            if (result.Succeeded)
            {
                await _logService.AddLogAsync("Kullanıcı", "Silme", $"{user.UserName} kullanıcısı silindi.");
                TempData["SuccessMessage"] = "Kullanıcı başarıyla silindi.";
                return RedirectToAction(nameof(Index));
            }

            foreach (var error in result.Errors)
            {
                ModelState.AddModelError("", error.Description);
            }

            return RedirectToAction(nameof(Index));
        }
    }
} 