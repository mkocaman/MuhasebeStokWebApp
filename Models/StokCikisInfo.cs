using System;
using System.Collections.Generic;

namespace MuhasebeStokWebApp.Models
{
    /// <summary>
    /// Stok çıkış işlemi sonucunda dönülen bilgileri içeren sınıf
    /// </summary>
    public class StokCikisInfo
    {
        /// <summary>
        /// Çıkış yapılan toplam miktar
        /// </summary>
        public decimal Miktar { get; set; }
        
        /// <summary>
        /// Çıkış işleminin toplam maliyeti (hesaplanan para biriminde)
        /// </summary>
        public decimal ToplamMaliyet { get; set; }
        
        /// <summary>
        /// Kullanılan para birimi
        /// </summary>
        public string ParaBirimi { get; set; } = "USD";
        
        /// <summary>
        /// İşlem sırasında kullanılan FIFO kayıtlarının ID'leri
        /// </summary>
        public List<Guid> KullanilanFifoKayitlari { get; set; } = new List<Guid>();
        
        /// <summary>
        /// Oluşturulan stok çıkış detaylarının ID'leri
        /// </summary>
        public List<Guid> CikisDetaylari { get; set; } = new List<Guid>();
        
        /// <summary>
        /// Stokun yetersiz olup olmadığını belirtir
        /// </summary>
        public bool StokYetersiz { get; set; } = false;
        
        /// <summary>
        /// Stok yetersizse, eksik olan miktarı belirtir
        /// </summary>
        public decimal EksikMiktar { get; set; } = 0;
    }
} 