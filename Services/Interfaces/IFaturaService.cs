using System;
using MuhasebeStokWebApp.Services.Interfaces;

namespace MuhasebeStokWebApp.Services.Interfaces
{
    /// <summary>
    /// Fatura servisi arayüzü. 
    /// Bu arayüz, diğer fatura servislerini bir araya getirerek bir facade oluşturur.
    /// </summary>
    public interface IFaturaService : IFaturaOrchestrationService, IFaturaCrudService
    {
        // Tüm gerekli metotlar alt arayüzlerde tanımlandı
    }
} 