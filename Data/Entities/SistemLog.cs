using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MuhasebeStokWebApp.Data.Entities
{
    public class SistemLog
    {
        [Key]
        public Guid LogID { get; set; }
        
        [Required]
        [StringLength(50)]
        public string IslemTuru { get; set; } = string.Empty; // Cari Pasife Alma, Cari Aktifleştirme, vb.
        
        public int LogTuru { get; set; } // MuhasebeStokWebApp.Enums.LogTuru karşılığı
        
        public Guid? KayitID { get; set; } // İlgili kaydın ID'si (CariID, FaturaID, vb.)
        
        [StringLength(100)]
        public string TabloAdi { get; set; } = string.Empty; // Cariler, Faturalar, vb.
        
        [StringLength(200)]
        public string KayitAdi { get; set; } = string.Empty; // Cari Adı, Fatura No, vb.
        
        [Required]
        public DateTime IslemTarihi { get; set; } = DateTime.Now;
        
        [StringLength(500)]
        public string? Aciklama { get; set; }
        
        public Guid? KullaniciID { get; set; }
        
        [StringLength(100)]
        public string KullaniciAdi { get; set; } = string.Empty;
        
        [StringLength(50)]
        public string IPAdresi { get; set; } = string.Empty;
        
        public bool Basarili { get; set; } = true;
        
        [StringLength(500)]
        public string? HataMesaji { get; set; }
    }
} 