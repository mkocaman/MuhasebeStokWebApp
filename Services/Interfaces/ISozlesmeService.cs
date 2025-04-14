using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using MuhasebeStokWebApp.Data.Entities;
using MuhasebeStokWebApp.ViewModels.Sozlesme;

namespace MuhasebeStokWebApp.Services.Interfaces
{
    public interface ISozlesmeService
    {
        Task<List<Sozlesme>> GetAllSozlesmeAsync();
        Task<Sozlesme> GetSozlesmeByIdAsync(Guid id);
        Task<List<Sozlesme>> GetSozlesmesByFiltersAsync(SozlesmeSearchViewModel filters);
        Task<bool> AddSozlesmeAsync(Sozlesme sozlesme);
        Task<bool> UpdateSozlesmeAsync(Sozlesme sozlesme);
        Task<bool> DeleteSozlesmeAsync(Guid id);
        Task<string> UploadSozlesmeDosyaAsync(SozlesmeDosyaModel model);
        Task<string> UploadVekaletnameAsync(SozlesmeDosyaModel model);
        Task<List<SozlesmeListViewModel>> GetSozlesmeListViewModelsAsync();
        Task<List<SozlesmeListViewModel>> GetSozlesmeListViewModelsAsync(Guid cariId);
        Task<SozlesmeViewModel> GetSozlesmeDetailAsync(Guid id);
        Task<bool> CreateSozlesmeAsync(SozlesmeViewModel model);
        Task<bool> UpdateSozlesmeAsync(SozlesmeViewModel model);
    }
} 