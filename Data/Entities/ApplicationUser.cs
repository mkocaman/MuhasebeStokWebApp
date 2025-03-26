using Microsoft.AspNetCore.Identity;

namespace MuhasebeStokWebApp.Data.Entities
{
    public class ApplicationUser : IdentityUser
    {
        public string? Ad { get; set; }
        public string? Soyad { get; set; }
        public bool Aktif { get; set; } = true;
        public DateTime OlusturmaTarihi { get; set; }
        public DateTime? GuncellemeTarihi { get; set; }
        public bool SoftDelete { get; set; } = false;
    }
} 