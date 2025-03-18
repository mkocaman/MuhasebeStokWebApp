using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MuhasebeStokWebApp.Data.Entities
{
    /// <summary>
    /// Bu entity kullanılmıyor, SistemAyarlari entity'si kullanılıyor.
    /// Bu class silinecek ve yerine SistemAyarlari kullanılacak.
    /// </summary>
    [NotMapped]
    public class KurAyarlari
    {
        [Key]
        public Guid KurAyarlariID { get; set; }
    }
} 