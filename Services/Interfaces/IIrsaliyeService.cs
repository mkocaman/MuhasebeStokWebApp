using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using MuhasebeStokWebApp.Data.Entities;
using MuhasebeStokWebApp.ViewModels.Irsaliye;
using MuhasebeStokWebApp.ViewModels.Fatura;

namespace MuhasebeStokWebApp.Services.Interfaces
{
    public interface IIrsaliyeService
    {
        /// <summary>
        /// Faturadan otomatik irsaliye oluşturur
        /// </summary>
        Task<Guid> OtomatikIrsaliyeOlustur(Fatura fatura, Guid? depoID = null);
        
        /// <summary>
        /// Fatura ID'sinden otomatik irsaliye oluşturur
        /// </summary>
        Task<Guid> OtomatikIrsaliyeOlusturFromID(Guid faturaID, Guid? depoID = null);
        
        /// <summary>
        /// Yeni bir irsaliye numarası oluşturur
        /// </summary>
        string GenerateIrsaliyeNumarasi();

        /// <summary>
        /// İrsaliye detaylarını günceller
        /// </summary>
        /// <param name="irsaliyeID">İrsaliye ID</param>
        /// <param name="detaylar">Detay listesi</param>
        /// <returns>İşlem başarılı ise true</returns>
        Task<bool> UpdateIrsaliyeDetaylarAsync(Guid irsaliyeID, List<IrsaliyeDetayViewModel> detaylar);
        
        /// <summary>
        /// Bir faturaya ait irsaliyeleri fatura bilgileriyle günceller
        /// </summary>
        /// <param name="faturaID">Fatura ID</param>
        /// <param name="viewModel">Fatura içeriği</param>
        /// <param name="currentUserId">İşlemi yapan kullanıcı ID</param>
        /// <returns>İşlem başarılı ise true</returns>
        Task<bool> UpdateIrsaliyeFromFaturaAsync(Guid faturaID, FaturaEditViewModel viewModel, Guid? currentUserId);
    }
} 