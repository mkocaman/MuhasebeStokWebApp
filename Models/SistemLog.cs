using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using MuhasebeStokWebApp.Enums;

namespace MuhasebeStokWebApp.Models
{
    public class SistemLog
    {
        [Key]
        public string LogID { get; set; } = string.Empty;
        
        public int SistemLogID { get; set; } // Bu property GetHashCode için kullanılıyor
        
        [Required]
        [StringLength(50)]
        public string IslemTuru { get; set; } = string.Empty; // Bilgi, Uyarı, Hata, vb.
        
        public int LogTuruInt { get; set; } // LogTuru enum değerinin sayısal karşılığı
        
        public string? KayitID { get; set; } // İlgili kaydın ID'si (CariID, FaturaID, vb.)
        
        [StringLength(100)]
        public string TabloAdi { get; set; } = string.Empty; // Cariler, Faturalar, vb.
        
        [StringLength(200)]
        public string KayitAdi { get; set; } = string.Empty; // Cari Adı, Fatura No, vb.
        
        [Required]
        public DateTime IslemTarihi { get; set; } = DateTime.Now;
        
        [StringLength(500)]
        public string? Aciklama { get; set; }
        
        public string KullaniciID { get; set; } = string.Empty;
        
        [StringLength(100)]
        public string KullaniciAdi { get; set; } = string.Empty;
        
        [StringLength(50)]
        public string IPAdresi { get; set; } = string.Empty;
        
        public Guid? IlgiliID { get; set; }
        
        public bool Basarili { get; set; } = true;
        
        [StringLength(500)]
        public string? HataMesaji { get; set; }
        
        public string Tarayici { get; set; } = string.Empty;
        
        // LogTuru property'si
        public string LogTuru { get; set; } = string.Empty;
        
        public string Mesaj { get => Aciklama ?? ""; set => Aciklama = value; }
        
        public string Detay { get => HataMesaji ?? ""; set => HataMesaji = value; }
        
        public DateTime Tarih { get => IslemTarihi; set => IslemTarihi = value; }
        
        public string IsletimSistemi { get; set; } = string.Empty;
        
        public bool Aktif { get; set; } = true;
        
        public DateTime OlusturmaTarihi { get => IslemTarihi; }
        
        public DateTime? GuncellemeTarihi { get; set; }
        
        public bool Silindi { get; set; } = false;
    }
} 