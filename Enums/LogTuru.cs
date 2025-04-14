namespace MuhasebeStokWebApp.Enums
{
    /// <summary>
    /// Sistem log türlerini tanımlayan enum
    /// </summary>
    public enum LogTuru
    {
        /// <summary>Bilgilendirme amaçlı log kaydı</summary>
        Bilgi,
        
        /// <summary>Dikkat edilmesi gereken uyarı içeren log kaydı</summary>
        Uyari,
        
        /// <summary>Sistem hatası log kaydı</summary>
        Hata,
        
        /// <summary>Kritik seviyede hatalar için log kaydı</summary>
        Kritik,
        
        /// <summary>Başarılı işlemler için log kaydı</summary>
        Basarili,
        
        /// <summary>Sisteme giriş işlemleri için log kaydı</summary>
        Giris,
        
        /// <summary>Sistemden çıkış işlemleri için log kaydı</summary>
        Cikis,
        
        /// <summary>Detaylı bilgi içeren log kaydı</summary>
        Detay,
        
        /// <summary>Silme işlemleri için log kaydı</summary>
        Silme,
        
        /// <summary>Ekleme işlemleri için log kaydı</summary>
        Ekleme,
        
        /// <summary>Güncelleme işlemleri için log kaydı</summary>
        Guncelleme
    }
}