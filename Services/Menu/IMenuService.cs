using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using MuhasebeStokWebApp.Data.Entities;
using MuhasebeStokWebApp.ViewModels.Menu;

namespace MuhasebeStokWebApp.Services
{
    public interface IMenuService
    {
        Task<List<MenuViewModel>> GetMenuHierarchyAsync();
        Task<List<MenuViewModel>> GetSidebarMenuByRolIdAsync(string rolId);
        Task<List<MenuViewModel>> GetActiveSidebarMenusAsync(string? userId);
        Task<Data.Entities.Menu> GetMenuByIdAsync(Guid id);
        Task<bool> AddMenuAsync(Data.Entities.Menu menu);
        Task<bool> UpdateMenuAsync(Data.Entities.Menu menu);
        Task<bool> DeleteMenuAsync(Guid id);
        Task<bool> AddMenuRolAsync(MenuRol menuRol);
        Task<bool> DeleteMenuRolAsync(Guid menuRolId);
        Task<bool> InitDefaultMenusAsync();
    }
} 