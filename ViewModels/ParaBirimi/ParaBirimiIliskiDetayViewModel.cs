using System;
using MuhasebeStokWebApp.Data.Entities;

namespace MuhasebeStokWebApp.ViewModels.ParaBirimi
{
    public class ParaBirimiIliskiDetayViewModel
    {
        public Data.Entities.ParaBirimi KaynakParaBirimi { get; set; }
        public Data.Entities.ParaBirimi HedefParaBirimi { get; set; }
        
        // Doğrudan ilişki (kaynak -> hedef)
        public ParaBirimiIliski DogruIliski { get; set; }
        
        // Ters ilişki (hedef -> kaynak)
        public ParaBirimiIliski TersIliski { get; set; }
        
        // İlişki var mı?
        public bool IliskiMevcut => (DogruIliski != null && DogruIliski.ParaBirimiIliskiID != Guid.Empty) || 
                                   (TersIliski != null && TersIliski.ParaBirimiIliskiID != Guid.Empty);
        
        // Doğrudan ilişki var mı?
        public bool DogruIliskiMevcut => DogruIliski != null && DogruIliski.ParaBirimiIliskiID != Guid.Empty;
        
        // Ters ilişki var mı?
        public bool TersIliskiMevcut => TersIliski != null && TersIliski.ParaBirimiIliskiID != Guid.Empty;
        
        // Çift yönlü ilişki var mı?
        public bool CiftYonluIliskiMevcut => DogruIliskiMevcut && TersIliskiMevcut;
    }
} 