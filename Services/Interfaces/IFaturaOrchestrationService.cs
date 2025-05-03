using System;
using System.Threading.Tasks;
using MuhasebeStokWebApp.ViewModels.Fatura;

namespace MuhasebeStokWebApp.Services.Interfaces
{
    public interface IFaturaOrchestrationService
    {
        /// <summary>
        /// Fatura ve ilişkili kayıtları (StokHareket, CariHareket, Irsaliye) oluşturur
        /// </summary>
        Task<Guid> CreateFaturaWithRelations(FaturaCreateViewModel viewModel, Guid? currentUserId);
        
        /// <summary>
        /// Fatura ve ilişkili kayıtları (StokHareket, CariHareket, Irsaliye) günceller
        /// </summary>
        Task<Guid> UpdateFaturaWithRelations(Guid id, FaturaEditViewModel viewModel, Guid? currentUserId);
        
        /// <summary>
        /// Fatura ve ilişkili kayıtları (StokHareket, CariHareket, Irsaliye) siler
        /// </summary>
        Task<bool> DeleteFaturaWithRelations(Guid id, Guid? currentUserId);
    }
} 