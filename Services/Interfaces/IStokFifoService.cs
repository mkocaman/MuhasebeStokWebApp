using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using MuhasebeStokWebApp.Data.Entities;
using MuhasebeStokWebApp.Enums;
using MuhasebeStokWebApp.Models;

namespace MuhasebeStokWebApp.Services.Interfaces
{
    /// <summary>
    /// Stok FIFO yönetimini sağlayan kompozit servis arayüzü.
    /// Bu arayüz, daha küçük ve odaklanmış arayüzleri birleştirir.
    /// </summary>
    public interface IStokFifoService : 
        IStokGirisService,
        IStokCikisService,
        IStokSorguService,
        IStokConcurrencyService
    {
        // Tüm gerekli metotlar alt arayüzlerde tanımlandı
    }
} 