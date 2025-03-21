using System;
using System.Collections.Generic;
using MuhasebeStokWebApp.Data.Entities;

namespace MuhasebeStokWebApp.ViewModels.ParaBirimi
{
    public class ParaBirimiIliskileriViewModel
    {
        public MuhasebeStokWebApp.Data.Entities.ParaBirimi ParaBirimi { get; set; }
        public List<ParaBirimiIliski> Iliskiler { get; set; }
    }
} 