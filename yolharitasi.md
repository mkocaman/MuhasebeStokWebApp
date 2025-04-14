# Yol Haritası - MuhasebeStokWebApp Yayına Hazırlık

Bu yol haritası, projenizi en kısa sürede yayına alabilmeniz için önceliklendirilmiş adımları içermektedir. Her adım, belirli bir hedefe ulaşmanızı sağlayacak ve sizi yayına bir adım daha yaklaştıracaktır.

**Aşama 1: Temel İşlevselliğin Sağlanması ve Veri Kaybının Önlenmesi (1-2 Gün)**

Bu aşamada, uygulamanın temel işlevlerinin çalıştığından emin olacağız ve veri kaybına yol açabilecek kritik sorunları çözeceğiz.

1.  **Veritabanı Yedeklemesi:** Herhangi bir sorun durumunda verilerinizi geri yükleyebilmek için veritabanının yedeğini alın.
2.  **Temel İşlevsellik Testleri:** Uygulamanın temel işlevlerinin (stok girişi, stok çıkışı, cari işlemleri, fatura işlemleri vb.) çalıştığından emin olun. Bu testler, manuel olarak yapılabilir.
3.  **Veri Doğruluğu Kontrolleri:** Stok miktarları, cari bakiyeleri, fatura tutarları gibi kritik verilerin doğruluğunu kontrol edin.
4.  **Hata Loglarının İncelenmesi:** Uygulamada oluşan hataları görmek için log kayıtlarını inceleyin ve kritik hataları (veri kaybına yol açabilecek, güvenlik açığı oluşturabilecek hatalar) öncelikli olarak düzeltin.
5.  **Transaction Yönetimi:** Özellikle birden fazla tabloyu etkileyen işlemlerde (stok transferi, fatura oluşturma vb.) transaction kullanımını sağlayın. Bu, veri tutarlılığını garanti altına almak için önemlidir.

**Aşama 2: Güvenlik ve Performans İyileştirmeleri (1 Gün)**

Bu aşamada, uygulamanın güvenliğini artıracak ve performansını iyileştirecek temel adımları atacağız.

1.  **Güvenlik Açıklarının Giderilmesi:** SQL injection, XSS gibi yaygın güvenlik açıklarını tespit etmek için basit güvenlik testleri yapın ve bu açıkları giderin.
2.  **Veritabanı Sorgularının Optimizasyonu:** Yavaş çalışan veritabanı sorgularını tespit edin ve indeksler ekleyerek veya sorguları yeniden yazarak performansı artırın.
3.  **Caching Mekanizmalarının Kullanılması:** Sık erişilen verileri (örn: ürün bilgileri, cari bilgileri) cache'leyerek performansı artırın.
4.  **HTTPS Kullanımının Zorunlu Hale Getirilmesi:** Uygulamanın HTTPS üzerinden çalıştığından emin olun ve HTTP isteklerini otomatik olarak HTTPS'e yönlendirin.

**Aşama 3: Kullanıcı Deneyimi İyileştirmeleri ve Son Kontroller (1 Gün)**

Bu aşamada, kullanıcı deneyimini iyileştirecek ve yayına almadan önce son kontrolleri yapacaksınız.

1.  **Kullanıcı Arayüzü Testleri:** Uygulamanın farklı cihazlarda ve tarayıcılarda doğru şekilde görüntülendiğinden emin olun.
2.  **Erişilebilirlik Testleri:** Uygulamanın engelli kullanıcılar tarafından da kolayca kullanılabildiğinden emin olun (örneğin, ekran okuyucularla uyumluluk).
3.  **Hata Mesajlarının İyileştirilmesi:** Kullanıcıya anlamlı ve yardımcı hata mesajları gösterin.
4.  **Son Kontroller:**
    *   Tüm özelliklerin beklendiği gibi çalıştığından emin olun.
    *   Yazım hatalarını ve diğer küçük hataları düzeltin.
    *   Uygulamanın farklı senaryolarda (örneğin, yoğun trafik altında) nasıl performans gösterdiğini test edin.

**Aşama 4: Yayın ve İzleme (Yayın Sonrası)**

1.  **Uygulamanın Yayınlanması:** Uygulamayı seçtiğiniz hosting platformuna yayınlayın.
2.  **İzleme ve Loglama:** Uygulamayı yayınladıktan sonra, log kayıtlarını düzenli olarak izleyin ve oluşan hataları düzeltin.
3.  **Kullanıcı Geri Bildirimleri:** Kullanıcılardan geri bildirim alın ve bu geri bildirimleri kullanarak uygulamayı geliştirmeye devam edin.

**Önemli Notlar:**

*   Bu yol haritası, genel bir rehber niteliğindedir ve projenizin özel ihtiyaçlarına göre uyarlanması gerekebilir.
*   Her aşamada, önceliklerinizi belirleyin ve en kritik görevlere odaklanın.
*   Küçük adımlar halinde ilerleyin ve her adımı tamamladıktan sonra bir sonraki adıma geçin.
*   Test etmeyi ve geri bildirim almayı unutmayın.

---

# ✅ Yayına Hazırlık Checklist & Görev Planı

## 🧩 Aşama 1: Temel İşlevsellik & Veri Güvenliği

- [ ] Veritabanı yedeği alındı (`.bak` veya `.sql` dosyası olarak)
- [ ] Temel fonksiyonlar manuel test edildi:
  - [ ] Stok girişi / çıkışı
  - [ ] Fatura oluşturma / düzenleme
  - [ ] Cari hareket işleme
- [ ] Kritik verilerin doğruluğu kontrol edildi (stok miktarı, cari bakiyesi, fatura tutarları)
- [ ] Log dosyaları incelendi → kritik hatalar belirlendi ve düzeltildi
- [ ] Tüm transaction gerektiren işlemler gözden geçirildi ve `DbContextTransaction` kullanımı eklendi

## 🔐 Aşama 2: Güvenlik & Performans

- [ ] SQL Injection, XSS gibi açıklar tarandı ve kapatıldı
- [ ] EF Core sorguları gözden geçirildi, gerekirse `Include`, `Select` vs. ile optimize edildi
- [ ] Kritik entity’lere uygun indeksler eklendi
- [ ] Cache stratejisi belirlendi ve uygulanmaya başlandı (`MemoryCache`, `IMemoryCache`)
- [ ] HTTPS kullanımı zorunlu hale getirildi (Redirect middleware aktif)

## 🎨 Aşama 3: UI & Kullanıcı Deneyimi

- [ ] UI mobil ve farklı ekranlarda test edildi (Chrome mobil görünüm veya gerçek cihaz)
- [ ] View’larda `TempData` yerine ViewModel + Strongly typed mesaj yapısı uygulandı
- [ ] Hatalar kullanıcı dostu mesajlarla gösteriliyor (örnek: “Bu alana sayı girilmelidir”)
- [ ] SweetAlert gibi kütüphaneler tüm mesaj tiplerinde (info/success/error) entegre edildi
- [ ] Hardcoded değerler (`kritik stok seviyesi` vb.) yapılandırma veya veritabanına alındı

## 🚀 Aşama 4: Yayınlama & İzleme

- [ ] `appsettings.Production.json` yapılandırması kontrol edildi
- [ ] Uygulama yayına alındı (IIS, Azure, vs.)
- [ ] Yayın sonrası `Serilog`, `HealthChecks`, `GlobalExceptionMiddleware` aktif
- [ ] Loglar izlenmeye başlandı (`Log.txt`, `serilog.log` vs.)
- [ ] Kullanıcılardan ilk geri bildirimler toplandı

## 💡 Bonus Görevler (Opsiyonel ama önerilir)

- [ ] Unit Testler eklendi (Özellikle servis katmanı için)
- [ ] Global `try-catch` yerinde `try { } catch { logger.LogError() }` ile detaylı loglama
- [ ] View’larda kullanılan `JavaScript` ayrı dosyaya taşındı (`site.js`, `stok.js`)
- [ ] ViewModel validasyonlarına `FluentValidation` desteği eklendi
