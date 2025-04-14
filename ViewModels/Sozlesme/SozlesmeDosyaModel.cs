using System;
using Microsoft.AspNetCore.Http;

namespace MuhasebeStokWebApp.ViewModels.Sozlesme
{
    public class SozlesmeDosyaModel
    {
        public Guid SozlesmeID { get; set; }
        public IFormFile Dosya { get; set; }
    }
} 