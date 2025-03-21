using System;
using System.Collections.Generic;
using MuhasebeStokWebApp.Models;

namespace MuhasebeStokWebApp.ViewModels.SistemLog
{
    public class LogListViewModel
    {
        public List<Models.SistemLog> Logs { get; set; } = new List<Models.SistemLog>();
        
        public DateTime BaslangicTarihi { get; set; } = DateTime.Now.AddDays(-7);
        
        public DateTime BitisTarihi { get; set; } = DateTime.Now;
        
        public string KullaniciAdi { get; set; }
        
        public string IslemTuru { get; set; }
    }
} 