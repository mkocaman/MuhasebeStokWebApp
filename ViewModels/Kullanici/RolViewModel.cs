using System.ComponentModel.DataAnnotations;

namespace MuhasebeStokWebApp.ViewModels.Kullanici
{
    public class RolViewModel
    {
        [Required]
        public required string Id { get; set; }

        [Required(ErrorMessage = "Rol adı zorunludur.")]
        [Display(Name = "Rol Adı")]
        public required string Name { get; set; }

        [Display(Name = "Kullanıcı Sayısı")]
        public int UserCount { get; set; }
    }
} 