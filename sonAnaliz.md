# MuhasebeStokWebApp Projesi Modül Entegrasyon Analizi Raporu

Bu rapor, MuhasebeStokWebApp projesinin Cari, Stok, Fatura, Banka ve FIFO modüllerinin entegrasyonunu incelemektedir. Analiz, ilgili Controller'lar (`CariController`, `StokController`, `FaturaController`, `BankaController`) ve Servisler (`StokFifoService`, `DovizKuruService`, `UnitOfWork`) üzerinden yapılmıştır.

## Genel Değerlendirme

Proje, temel muhasebe ve stok yönetimi işlevlerini yerine getirmek üzere yapılandırılmıştır. Unit of Work ve Repository pattern kullanımı veri erişimini düzenli hale getirmektedir. FIFO stok maliyetlendirmesi için özel bir servis (`StokFifoService`) bulunmaktadır. Modüller arasında belirli entegrasyonlar mevcut olmakla birlikte, bazı kritik alanlarda iyileştirmelere ve düzeltmelere ihtiyaç duyulmaktadır.

## Modül Bazlı Analiz ve Entegrasyon Noktaları

### 1. Cari Modülü (`CariController`)

*   **İşlevler:** Cari kart CRUD işlemleri, cari hareket ekleme (manuel, bakiye düzeltme, açılış), cari ekstre görüntüleme.
*   **Entegrasyonlar:**
    *   **Fatura:** Cari silme işlemi (`DeleteConfirmed`), ilişkili fatura olup olmadığını kontrol eder. Cari detaylarında (`Details`) son faturalar gösterilir.
    *   **Cari Hareket:** Cari oluşturma/düzenleme sırasında açılış/düzeltme bakiyesi için `CariHareket` oluşturulur. Ekstre (`Ekstre`) metodu `CariHareket` kayıtlarını kullanır.
    *   **Para Birimi/Kur:** Ekstre (`Ekstre`) metodu, farklı para birimlerinde ekstre sunmak için `ParaBirimiService` ve `DovizKuruService` kullanır.
*   **Tespitler:**
    *   **Güçlü Yönler:** Cari silme işleminde ilişkili kayıt kontrolü (soft delete) doğru bir yaklaşımdır. Açılış ve düzeltme bakiyelerinin hareketlere yansıtılması önemlidir.
    *   **İyileştirme Alanları:**
        *   **Ekstre Kur Hesaplama:** `Ekstre` metodu, rapor dönemi için tek bir *güncel* döviz kuru (`GetSonKurDegeriByParaBirimiAsync`) kullanıyor. Bu, geçmiş hareketlerin o günkü kurdan değil, raporun alındığı günkü kurdan gösterilmesine neden olabilir. Doğru yaklaşım, her hareketin kendi tarihindeki kur ile değerlendirilmesi veya raporlama para birimine göre hareket anındaki kurla çevrilmesidir. Mevcut `CariEkstreViewModel` ve hesaplama mantığı oldukça karmaşık görünüyor, sadeleştirilebilir.
        *   `Details` action'ındaki `OrderByDescending` LINQ sorguları veritabanı tarafında desteklenmiyor olabilir ve bu yüzden `.ToList()` sonrası uygulama tarafında sıralama yapılıyor. Bu durum büyük veri setlerinde performans sorununa yol açabilir. Veritabanı uyumlu sorgulama yöntemleri araştırılmalıdır.

### 2. Stok Modülü (`StokController` ve `StokFifoService`)

*   **İşlevler:** Stok kart listeleme, stok hareketleri görüntüleme, manuel stok giriş/çıkış/transfer/sayım işlemleri, FIFO kayıtlarını ve ortalama maliyeti görme, stok durumu ve raporlama.
*   **Entegrasyonlar:**
    *   **FIFO:** Tüm stok giriş/çıkış/sayım işlemleri (`StokGiris`, `StokCikis`, `StokSayim`) `StokFifoService` üzerinden FIFO kayıtlarını oluşturur/günceller. Ortalama maliyet `StokFifoService`'ten alınır.
    *   **Fatura:** Stok hareketleri (`Hareketler`) fatura referanslarını gösterebilir. Fatura modülü stok hareketlerini ve FIFO'yu tetikler.
    *   **Para Birimi/Kur:** `StokFifoService`, girişlerde farklı para birimlerini ve kurları dikkate alarak USD, TRY, UZS bazında maliyetleri saklar. `DovizKuruService` kullanılır.
*   **Tespitler:**
    *   **Güçlü Yönler:** FIFO maliyetlendirmesi için ayrı bir servis kullanılması modülerliği artırır. `StokFifoService`'in giriş ve çıkış mantığı temel FIFO prensiplerine uygun görünüyor. Farklı para birimlerinde maliyet takibi yapılması önemlidir. `StokYetersizException` kullanımı hata yönetimini iyileştirir.
    *   **İyileştirme Alanları:**
        *   `StokFifoService` içindeki `ParaBirimiCevirAsync` ve `DovizliFiyatHesapla` metotları ID (Guid) yerine Kod ("USD", "TRY") kullanılarak daha okunabilir hale getirilebilir veya `DovizKuruService`'e taşınabilir.
        *   Ortalama satış fiyatı hesaplaması (`Hareketler` action) sadece son 3 ayı dikkate alıyor ve KDV'siz fiyatlar üzerinden yapılıyor gibi görünüyor, bu iş gereksinimlerine göre doğrulanmalıdır.
        *   Stok raporlama (`StokRapor`) yetenekleri geliştirilebilir (örn. depo bazlı maliyet, daha detaylı filtreleme).

### 3. Fatura Modülü (`FaturaController`)

*   **İşlevler:** Fatura CRUD işlemleri, fatura yazdırma, ödeme ekleme, irsaliyeye dönüştürme.
*   **Entegrasyonlar:**
    *   **Stok & FIFO:** Fatura türüne göre (`Alış`/`Satış`) `StokHareket` oluşturur ve `StokFifoService`'i (`StokGirisiYap`/`StokCikisiYap`) çağırır. Fatura silme işlemi ilgili stok hareketlerini ve FIFO kayıtlarını geri alır (`FifoKayitlariniIptalEt`).
    *   **Cari:** Fatura türüne göre (`Alış`/`Satış`) `CariHareket` (Borç/Alacak) oluşturur. Fatura silme işlemi ilgili cari hareketleri geri alır. Ödeme eklendiğinde (`AddOdeme`) cariye ters hareket (tahsilat/ödeme) kaydı oluşturur.
    *   **İrsaliye:** Faturadan otomatik veya manuel irsaliye oluşturulabilir. İrsaliyeden fatura oluşturulabilir (TempData ile ID taşınıyor).
    *   **Para Birimi/Kur:** Fatura farklı döviz türleri ve kurları ile oluşturulabilir. Bu kur bilgisi FIFO servisine aktarılır.
*   **Tespitler:**
    *   **Güçlü Yönler:** Faturanın stok, FIFO ve cari modülleriyle entegrasyonu büyük ölçüde sağlanmış. Fatura silme işleminin ilgili kayıtları geri alması önemlidir. Otomatik irsaliye oluşturma özelliği kullanışlıdır.
    *   **Kritik Sorunlar/Riskler:**
        *   **Transaction Yönetimi (`Create` Action):** Fatura oluşturma işlemi (`Create`) birden fazla adımdan (Fatura, Detay, StokHareket, CariHareket, FIFO, Opsiyonel İrsaliye) oluşmasına rağmen, tek bir atomik transaction içinde yönetilmiyor. Kod, `SaveChangesAsync` ve `CommitAsync` çağrısını FIFO işlemi *öncesinde* yapıyor, ardından FIFO işlemi için (ve opsiyonel irsaliye için) yeni transaction'lar başlatıyor. Eğer FIFO veya irsaliye adımı başarısız olursa, fatura, stok ve cari hareketleri kaydedilmiş ancak FIFO güncellenmemiş olur, bu da **veri tutarsızlığına** yol açar. Tüm adımlar tek bir transaction içinde yönetilmelidir.
        *   **FIFO Hata Yönetimi (`Create` Action):** `StokYetersizException` yakalandığında sadece `TempData` ile mesaj verilip `RedirectToAction` yapılıyor. Bu durumda transaction rollback edilmiyor (çünkü commit edilmişti) ve kısmen kaydedilmiş fatura kalıyor olabilir. Hata durumunda tüm işlemin geri alınması gerekir.
    *   **İyileştirme Alanları:**
        *   Fatura/Sipariş/İrsaliye numarası üretme (`GenerateNew...`) metotları `FaturaController` içinde yer alıyor ve benzer mantık tekrarlanıyor. Bu, merkezi bir servis veya yardımcı sınıfa taşınabilir.
        *   `AddOdeme` metodu cari hareket oluştururken hareket türünü fatura türüne göre belirliyor (`Satış` -> `Alacak`, `Alış` -> `Borç`). Bu mantık doğrudur (müşteriden tahsilat alacak kaydıdır, tedarikçiye ödeme borç kaydıdır).

### 4. Banka Modülü (`BankaController`)

*   **İşlevler:** Banka hesabı CRUD işlemleri, banka hareketleri listeleme, manuel banka hareketi ekleme (Para Yatırma/Çekme, EFT, Havale).
*   **Entegrasyonlar:**
    *   **Cari (Eksik Entegrasyon):** `YeniHareket` action'ı, bir banka hareketini cari ile ilişkilendirme imkanı sunar (`hareket.CariID`). Ancak, bu ilişkilendirme yapıldığında ilgili cari için otomatik olarak bir `CariHareket` (örn. EFT Alma -> Cari Alacak, EFT Gönderme -> Cari Borç) **oluşturulmuyor**. Bu, banka ve cari modülleri arasında **önemli bir entegrasyon eksikliğidir**. Banka üzerinden yapılan ve bir cariyi ilgilendiren işlemlerin (tahsilat, ödeme vb.) cari hesabına da yansıması gerekir.
*   **Tespitler:**
    *   **Güçlü Yönler:** Temel banka hesabı ve hareket takibi işlevleri mevcut.
    *   **Kritik Sorunlar/Riskler:** Belirtilen Banka-Cari entegrasyon eksikliği, çift taraflı kayıt prensibine aykırıdır ve finansal tutarsızlıklara yol açabilir.
    *   **İyileştirme Alanları:** `YeniHareket` action'ı, `CariID` seçildiğinde otomatik olarak ilgili `CariHareket` kaydını oluşturacak şekilde güncellenmelidir.

### 5. FIFO Modülü (`StokFifoService`)

*   **İşlevler:** Stok giriş/çıkış FIFO kayıtlarını yönetme, maliyet hesaplama, kayıt iptali.
*   **Entegrasyonlar:** `StokController` ve `FaturaController` tarafından kullanılır. `DovizKuruService`'i kullanarak kur hesaplamaları yapar.
*   **Tespitler:**
    *   **Güçlü Yönler:** FIFO mantığını merkezi bir serviste toplar. Farklı para birimlerinde maliyet hesaplama yeteneği önemlidir. Transaction yönetimi kendi içinde doğru görünüyor.
    *   **İyileştirme Alanları:** Kur alma ve para birimi çevirme mantığı daha da merkezileştirilebilir veya `DovizKuruService` ile daha entegre hale getirilebilir.

## Genel Sonuç ve Öneriler

Proje, önemli muhasebe ve stok işlevlerini barındırmaktadır ancak modüller arası entegrasyonun, özellikle Banka-Cari bağlantısının güçlendirilmesi ve Fatura oluşturma işlemindeki transaction yönetiminin düzeltilmesi kritik öneme sahiptir. Cari Ekstre'deki kur hesaplama mantığının doğruluğu gözden geçirilmelidir. Kod tekrarını azaltmak ve tutarlılığı artırmak için numara üretme ve kur alma gibi işlemler merkezileştirilmelidir.

## Cari Hareket ve FIFO Oluşturma/İptal Süreçleri Özeti

*   **Cari Hareketler:**
    *   **Oluşturma:** Fatura, Cari (açılış/düzeltme) ve Fatura Ödeme modüllerinde büyük ölçüde doğru şekilde tetiklenmektedir. **Kritik Eksiklik:** Banka modülünden otomatik cari hareket oluşturulmamaktadır.
    *   **İptal:** Sadece Fatura silme işlemiyle tetiklenir ve ilgili hareketler `Silindi=true` olarak işaretlenir (soft delete). Bu yaklaşım kabul edilebilir ancak iptal mekanizması diğer modüller için eksiktir.
*   **FIFO Kayıtları:**
    *   **Oluşturma/Kullanım:** `StokFifoService` içinde merkezi olarak yönetilmekte ve Fatura/Stok modüllerinden doğru şekilde çağrılmaktadır.
    *   **İptal:** `StokFifoService.FifoKayitlariniIptalEt` metodu ile merkezi olarak yönetilir ve ilgili FIFO girişini `Iptal=true` olarak işaretler. Stok miktarının fiziksel olarak düzeltilmesi işlemi, çağıran Controller (örn. `FaturaController`) tarafından ayrıca yapılır, bu doğru bir ayrımdır.
*   **Ana Risk:** `FaturaController.Create` metodundaki transaction yönetimi eksikliği, hem cari hareketlerin hem de FIFO kayıtlarının atomik olarak oluşturulup/iptal edilmesini engeller, bu da **veri tutarsızlığına yol açabilir.**
