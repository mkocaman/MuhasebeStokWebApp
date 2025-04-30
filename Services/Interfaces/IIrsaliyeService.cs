using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using MuhasebeStokWebApp.Data.Entities;

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
    }
} 