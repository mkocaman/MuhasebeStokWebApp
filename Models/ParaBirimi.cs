using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MuhasebeStokWebApp.Models
{
    public enum ParaBirimiTipi
    {
        Yerel = 0,
        Yabanci = 1
    }
    
    public class ParaBirimi
    {
        public ParaBirimi()
        {
            Kod = string.Empty;
            Ad = string.Empty;
            Sembol = string.Empty;
            OlusturanKullanici = string.Empty;
            GuncelleyenKullanici = string.Empty;
            OlusturmaTarihi = DateTime.Now;
            GuncellemeTarihi = DateTime.Now;
            Aktif = true;
        }

        [Key]
        public Guid ParaBirimiID { get; set; }

        [Required(ErrorMessage = "Para birimi kodu zorunludur.")]
        [StringLength(3, MinimumLength = 3, ErrorMessage = "Para birimi kodu tam olarak 3 karakter olmalıdır.")]
        public string Kod { get; set; }

        [Required(ErrorMessage = "Para birimi adı zorunludur.")]
        [StringLength(50, ErrorMessage = "Para birimi adı en fazla 50 karakter olabilir.")]
        public string Ad { get; set; }

        [Required(ErrorMessage = "Para birimi sembolü zorunludur.")]
        [StringLength(5, ErrorMessage = "Para birimi sembolü en fazla 5 karakter olabilir.")]
        public string Sembol { get; set; }

        [Required(ErrorMessage = "Para birimi tipi zorunludur.")]
        public ParaBirimiTipi Tip { get; set; }

        [Required(ErrorMessage = "Aktiflik durumu zorunludur.")]
        public bool Aktif { get; set; }

        [Required(ErrorMessage = "Oluşturma tarihi zorunludur.")]
        public DateTime OlusturmaTarihi { get; set; }

        [Required(ErrorMessage = "Güncelleme tarihi zorunludur.")]
        public DateTime GuncellemeTarihi { get; set; }

        [Required(ErrorMessage = "Oluşturan kullanıcı zorunludur.")]
        public string OlusturanKullanici { get; set; }

        [Required(ErrorMessage = "Güncelleyen kullanıcı zorunludur.")]
        public string GuncelleyenKullanici { get; set; }
    }
} 