using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MuhasebeStokWebApp.Data.Entities
{
    public class SistemLog
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }
        
        public Guid LogID { get; set; } = Guid.NewGuid();
        
        [Required, StringLength(50)]
        public string LogTuru { get; set; }
        
        [Required, StringLength(500)]
        public string Mesaj { get; set; }
        
        [StringLength(255)]
        public string Sayfa { get; set; }
        
        public DateTime OlusturmaTarihi { get; set; } = DateTime.Now;
        
        [StringLength(50)]
        public string IslemTuru { get; set; }
        
        public int? LogTuruInt { get; set; }
        
        [StringLength(500)]
        public string Aciklama { get; set; }
        
        [StringLength(500)]
        public string HataMesaji { get; set; }
        
        [StringLength(100)]
        public string KullaniciAdi { get; set; }
        
        [StringLength(50)]
        public string IPAdresi { get; set; }
        
        public DateTime IslemTarihi { get; set; } = DateTime.Now;
        
        public bool Basarili { get; set; } = true;
        
        [StringLength(100)]
        public string TabloAdi { get; set; }
        
        [StringLength(100)]
        public string KayitAdi { get; set; }
        
        public Guid? KayitID { get; set; }
        
        public Guid? KullaniciID { get; set; }
    }
} 