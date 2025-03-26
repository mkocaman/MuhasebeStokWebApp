using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MuhasebeStokWebApp.Data.Entities
{
    public class Menu
    {
        [Key]
        public Guid MenuID { get; set; }
        
        [Required, StringLength(100)]
        public string Ad { get; set; }
        
        [StringLength(100)]
        public string Icon { get; set; }
        
        [StringLength(100)]
        public string Controller { get; set; }
        
        [StringLength(100)]
        public string Action { get; set; }
        
        public int Sira { get; set; }
        
        public Guid? UstMenuID { get; set; }
        
        [StringLength(255)]
        public string Url { get; set; }
        
        public bool AktifMi { get; set; } = true;
        
        public bool Silindi { get; set; } = false;
        
        public DateTime OlusturmaTarihi { get; set; } = DateTime.Now;
        
        public DateTime? GuncellemeTarihi { get; set; }
        
        public Guid? OlusturanKullaniciID { get; set; }
        
        public Guid? SonGuncelleyenKullaniciID { get; set; }
        
        // Navigation properties
        [ForeignKey("UstMenuID")]
        public virtual Menu UstMenu { get; set; }
        
        public virtual ICollection<Menu> AltMenuler { get; set; } = new HashSet<Menu>();
        
        public virtual ICollection<MenuRol> MenuRoller { get; set; } = new HashSet<MenuRol>();
    }
} 