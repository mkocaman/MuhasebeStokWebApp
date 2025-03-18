using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MuhasebeStokWebApp.Data.Entities
{
    public class SistemLog
    {
        public SistemLog()
        {
            LogID = Guid.NewGuid();
            IslemTarihi = DateTime.Now;
            Basarili = true;
            SoftDelete = false;
            KayitAdi = "Sistem Kaydı"; // Default değer
            TabloAdi = "Sistem"; // Default değer
        }
        
        [Key]
        public Guid LogID { get; set; }
        
        [Required]
        [StringLength(50)]
        public string IslemTuru { get; set; } // Cari Pasife Alma, Cari Aktifleştirme, vb.
        
        public Guid? KayitID { get; set; } // İlgili kaydın ID'si (CariID, FaturaID, vb.)
        
        [Required]
        [StringLength(50)]
        public string TabloAdi { get; set; } // Cariler, Faturalar, vb.
        
        [Required]
        [StringLength(100)]
        public string KayitAdi { get; set; } // Cari Adı, Fatura No, vb.
        
        [Required]
        public DateTime IslemTarihi { get; set; } = DateTime.Now;
        
        [Required]
        [StringLength(500)]
        public string Aciklama { get; set; }
        
        public string? KullaniciID { get; set; }
        
        [MaxLength(50)]
        public string KullaniciAdi { get; set; }
        
        [MaxLength(50)]
        public string IPAdresi { get; set; }
        
        [MaxLength(1024)]
        public string? Tarayici { get; set; }
        
        public bool Basarili { get; set; } = true;
        
        [MaxLength(500)]
        public string? HataMesaji { get; set; }
        
        public string? IlgiliID { get; set; }
        
        public bool SoftDelete { get; set; }
    }
} 