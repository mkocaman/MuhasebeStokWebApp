using System;
using System.Threading.Tasks;
using MuhasebeStokWebApp.ViewModels.Fatura;

namespace MuhasebeStokWebApp.Services.Interfaces
{
    public interface IFaturaValidationService
    {
        /// <summary>
        /// Fatura oluşturma view model'inin tutarlılığını kontrol eder
        /// </summary>
        (bool IsValid, string ErrorMessage) ValidateFaturaCreateViewModel(FaturaCreateViewModel viewModel);
        
        /// <summary>
        /// Fatura düzenleme view model'inin tutarlılığını kontrol eder
        /// </summary>
        (bool IsValid, string ErrorMessage) ValidateFaturaEditViewModel(FaturaEditViewModel viewModel);
        
        /// <summary>
        /// Fatura ve ilgili StokHareket kayıtlarının tutarlı olup olmadığını kontrol eder
        /// </summary>
        Task<(bool IsValid, string ErrorMessage)> ValidateFaturaStokTutarliligi(Guid faturaId);
        
        /// <summary>
        /// Fatura ve ilgili CariHareket kaydının tutarlı olup olmadığını kontrol eder
        /// </summary>
        Task<(bool IsValid, string ErrorMessage)> ValidateFaturaCariTutarliligi(Guid faturaId);
        
        /// <summary>
        /// Fatura ve ilgili Irsaliye kaydının tutarlı olup olmadığını kontrol eder
        /// </summary>
        Task<(bool IsValid, string ErrorMessage)> ValidateFaturaIrsaliyeTutarliligi(Guid faturaId);
    }
} 