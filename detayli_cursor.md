# MuhasebeStokWebApp İyileştirme Raporu - Cursor Prompt

Bu rapor, MuhasebeStokWebApp projesindeki zayıf yönleri ve iyileştirme ihtiyaçlarını özetlemektedir. Bu rapor, Cursor'a proje kodunu iyileştirmesi için bir prompt olarak kullanılmak üzere hazırlanmıştır.

## İyileştirilmesi Gereken Alanlar ve Gereksinimler

Aşağıdaki maddeler, MuhasebeStokWebApp projesinde iyileştirilmesi gereken alanları ve bu alanlara yönelik gereksinimleri listelemektedir. Cursor, bu gereksinimleri dikkate alarak proje kodunu iyileştirmek için çözümler üretebilir.

1.  **Transaction Yönetimi:**
    *   **Gereksinim:** Proje genelinde tutarlı bir Unit of Work pattern'i uygulayarak transaction yönetimini iyileştirin. Servislerin `SaveChanges` sorumluluğunu almayın ve transaction işlemlerini bir üst katmanda (UnitOfWork veya Controller transaction scope) yönetin.

2.  **Sorumlulukların Ayrılması:**
    *   **Gereksinim:** İş mantığını Controller'lardan ilgili Servislere (örn. `StokService`, `StokFifoService`, `CariHareketService`) taşıyın. Controller'lar sadece servisleri çağırarak işlemleri yönlendirmeli ve sonuçları işlemelidir.

3.  **Hata Yönetimi:**
    *   **Gereksinim:** Hata durumlarında işlemleri geri alın (`RollbackAsync`) ve kullanıcıya anlamlı hata mesajları gösterin. Servis katmanından gelen hataları (örn. `StokYetersizException`) Controller katmanında yakalayıp uygun şekilde işleyin.

4.  **Döviz Kuru Dönüşümü:**
    *   **Gereksinim:** Döviz kuru dönüşüm mantığını tekrar eden kod bloklarından çıkarıp özel bir yardımcı metoda taşıyın. Sabit kodlanmış varsayılan kur değerlerini yapılandırma dosyasına (appsettings.json) taşıyın.

5.  **Otomatik İrsaliye Oluşturma:**
    *   **Gereksinim:** Arka plan irsaliye oluşturma işlemlerinin durumunu izlemek ve kullanıcıya geri bildirim vermek için bir mekanizma (notification veya log kaydı gibi) ekleyin.

6.  **Kod Tekrarı:**
    *   **Gereksinim:** Dropdown listeleri (`Cariler`, `FaturaTurleri`, `Urunler` vb.) oluşturma ve `ViewBag` ile View'e taşıma işlemlerini tekrar eden kod bloklarından çıkarıp bir yardımcı metoda taşıyarak kod tekrarını azaltın.

7.  **Güvenlik:**
    *   **Gereksinim:** Tüm Controller metotlarında kullanıcı yetkisini kontrol edin ve yetkisiz erişimi engelleyin.

8.  **Magic String'ler:**
    *   **Gereksinim:** Kod içerisindeki sabit string değerleri ("Fatura", "Giriş", "Çıkış" gibi) enum veya constant string olarak tanımlayarak kodun okunabilirliğini ve sürdürülebilirliğini artırın.

9.  **Veri Doğruluğu - Satır Toplamı Hesaplama:**
    *   **Gereksinim:** Fatura kalemleri işlenirken `SatirToplam` değerinin doğru hesaplandığından emin olun. KDV tutarını da dahil ederek `SatirToplam = tutar + kdvTutar;` şeklinde düzeltin.

Bu rapor, MuhasebeStokWebApp projesinin daha sağlam, bakımı kolay ve güvenilir bir uygulama haline gelmesine yardımcı olacak iyileştirme önerilerini içermektedir. Cursor'ın bu doğrultuda geliştirmeler yapması beklenmektedir.
