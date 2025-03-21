using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Identity;
using MuhasebeStokWebApp.Data.Entities;
using MuhasebeStokWebApp.Services;
using MuhasebeStokWebApp.ViewModels.Menu;

namespace MuhasebeStokWebApp.Controllers
{
    [Authorize(Roles = "Admin")]
    public class MenuController : Controller
    {
        private readonly IMenuService _menuService;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly ILogService _logService;

        public MenuController(
            IMenuService menuService,
            RoleManager<IdentityRole> roleManager,
            ILogService logService)
        {
            _menuService = menuService;
            _roleManager = roleManager;
            _logService = logService;
        }

        // GET: Menu
        public async Task<IActionResult> Index()
        {
            var menus = await _menuService.GetMenuHierarchyAsync();
            return View(menus);
        }

        // GET: Menu/Details/5
        public async Task<IActionResult> Details(Guid id)
        {
            var menu = await _menuService.GetMenuByIdAsync(id);
            if (menu == null)
            {
                return NotFound();
            }

            return View(menu);
        }

        // GET: Menu/Create
        public async Task<IActionResult> Create(Guid? parentId)
        {
            var viewModel = new MenuCreateViewModel
            {
                ParentId = parentId,
                ParentMenu = parentId.HasValue ? await _menuService.GetMenuByIdAsync(parentId.Value) : null,
                Aktif = true
            };

            // Tüm rolleri getir
            var roles = _roleManager.Roles.ToList();
            viewModel.Roles = roles;

            return View(viewModel);
        }

        // POST: Menu/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(MenuCreateViewModel viewModel)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    var menu = new Menu
                    {
                        MenuID = Guid.NewGuid(),
                        Ad = viewModel.Ad,
                        Icon = viewModel.Icon,
                        Url = viewModel.Url,
                        Controller = viewModel.Controller,
                        Action = viewModel.Action,
                        AktifMi = viewModel.Aktif,
                        Sira = viewModel.Sira,
                        UstMenuID = viewModel.ParentId
                    };

                    await _menuService.AddMenuAsync(menu);

                    // Rol-Menü ilişkilerini ekle
                    if (viewModel.SelectedRoleIds != null && viewModel.SelectedRoleIds.Any())
                    {
                        foreach (var roleId in viewModel.SelectedRoleIds)
                        {
                            await _menuService.AddMenuRolAsync(new MenuRol
                            {
                                MenuRolID = Guid.NewGuid(),
                                MenuID = menu.MenuID,
                                RolID = roleId
                            });
                        }
                    }

                    TempData["SuccessMessage"] = "Menü başarıyla oluşturuldu.";
                    return RedirectToAction(nameof(Index));
                }
                catch (Exception ex)
                {
                    TempData["ErrorMessage"] = "Menü oluşturulurken bir hata oluştu: " + ex.Message;
                    ModelState.AddModelError("", ex.Message);
                }
            }

            // Validation hatası varsa rolleri tekrar yükle
            viewModel.Roles = _roleManager.Roles.ToList();
            viewModel.ParentMenu = viewModel.ParentId.HasValue ? 
                await _menuService.GetMenuByIdAsync(viewModel.ParentId.Value) : null;

            return View(viewModel);
        }

        // GET: Menu/Edit/5
        public async Task<IActionResult> Edit(Guid id)
        {
            var menu = await _menuService.GetMenuByIdAsync(id);
            if (menu == null)
            {
                return NotFound();
            }

            var viewModel = new MenuEditViewModel
            {
                MenuID = menu.MenuID,
                Ad = menu.Ad,
                Icon = menu.Icon,
                Url = menu.Url,
                Controller = menu.Controller,
                Action = menu.Action,
                Aktif = menu.AktifMi,
                Sira = menu.Sira,
                ParentId = menu.UstMenuID,
                ParentMenu = menu.UstMenuID.HasValue ? await _menuService.GetMenuByIdAsync(menu.UstMenuID.Value) : null,
                SelectedRoleIds = menu.MenuRoller?.Select(mr => mr.RolID).ToList()
            };

            // Tüm rolleri getir
            viewModel.Roles = _roleManager.Roles.ToList();

            return View(viewModel);
        }

        // POST: Menu/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(Guid id, MenuEditViewModel viewModel)
        {
            if (id != viewModel.MenuID)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    var menu = await _menuService.GetMenuByIdAsync(id);
                    if (menu == null)
                    {
                        return NotFound();
                    }

                    menu.Ad = viewModel.Ad;
                    menu.Icon = viewModel.Icon;
                    menu.Url = viewModel.Url;
                    menu.Controller = viewModel.Controller;
                    menu.Action = viewModel.Action;
                    menu.AktifMi = viewModel.Aktif;
                    menu.Sira = viewModel.Sira;
                    menu.UstMenuID = viewModel.ParentId;

                    await _menuService.UpdateMenuAsync(menu);

                    // Rol-Menü ilişkilerini güncelle
                    // Önce mevcut rollerden seçili olmayanları kaldır
                    var existingRoleIds = menu.MenuRoller.Select(mr => mr.RolID).ToList();
                    var roleIdsToRemove = existingRoleIds.Except(viewModel.SelectedRoleIds ?? new List<string>()).ToList();

                    foreach (var roleId in roleIdsToRemove)
                    {
                        var menuRol = menu.MenuRoller.FirstOrDefault(mr => mr.RolID == roleId);
                        if (menuRol != null)
                        {
                            await _menuService.DeleteMenuRolAsync(menuRol.MenuRolID);
                        }
                    }

                    // Yeni seçilen rolleri ekle
                    if (viewModel.SelectedRoleIds != null)
                    {
                        foreach (var roleId in viewModel.SelectedRoleIds)
                        {
                            if (!existingRoleIds.Contains(roleId))
                            {
                                await _menuService.AddMenuRolAsync(new MenuRol
                                {
                                    MenuRolID = Guid.NewGuid(),
                                    MenuID = menu.MenuID,
                                    RolID = roleId
                                });
                            }
                        }
                    }

                    TempData["SuccessMessage"] = "Menü başarıyla güncellendi.";
                    return RedirectToAction(nameof(Index));
                }
                catch (Exception ex)
                {
                    TempData["ErrorMessage"] = "Menü güncellenirken bir hata oluştu: " + ex.Message;
                    ModelState.AddModelError("", ex.Message);
                }
            }

            // Validation hatası varsa rolleri tekrar yükle
            viewModel.Roles = _roleManager.Roles.ToList();
            viewModel.ParentMenu = viewModel.ParentId.HasValue ? 
                await _menuService.GetMenuByIdAsync(viewModel.ParentId.Value) : null;

            return View(viewModel);
        }

        // GET: Menu/Delete/5
        public async Task<IActionResult> Delete(Guid id)
        {
            var menu = await _menuService.GetMenuByIdAsync(id);
            if (menu == null)
            {
                return NotFound();
            }

            return View(menu);
        }

        // POST: Menu/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(Guid id)
        {
            try
            {
                await _menuService.DeleteMenuAsync(id);
                TempData["SuccessMessage"] = "Menü başarıyla silindi.";
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "Menü silinirken bir hata oluştu: " + ex.Message;
            }

            return RedirectToAction(nameof(Index));
        }

        // POST: Menu/InitDefaults
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> InitDefaults()
        {
            try
            {
                var result = await _menuService.InitDefaultMenusAsync();
                if (result)
                {
                    TempData["SuccessMessage"] = "Varsayılan menüler başarıyla oluşturuldu.";
                }
                else
                {
                    TempData["InfoMessage"] = "Menü tablosu zaten dolu olduğu için varsayılan menüler oluşturulmadı.";
                }
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "Varsayılan menüler oluşturulurken bir hata oluştu: " + ex.Message;
            }

            return RedirectToAction(nameof(Index));
        }
    }
} 