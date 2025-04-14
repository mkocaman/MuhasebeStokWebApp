# MuhasebeStokWebApp Projesi İyileştirme Prompt'u (Cursor AI için)

**Genel Amaç:** MuhasebeStokWebApp projesindeki Cari, Stok, Fatura, Banka ve FIFO modüllerinin entegrasyonunu iyileştirmek, kritik hataları düzeltmek ve kod kalitesini artırmak.

**Odaklanılacak Alanlar ve Yapılacaklar:**

1.  **Fatura Oluşturma Transaction Yönetimi (`FaturaController.cs` -> `Create` metodu):**
    *   **Sorun:** Mevcut `Create` metodu, fatura oluşturma adımlarını (Fatura, Detay, StokHareket, CariHareket, FIFO, Opsiyonel İrsaliye) tek bir atomik transaction içinde yönetmiyor. FIFO ve İrsaliye işlemleri ayrı transaction'larda yapılıyor, bu da hata durumunda veri tutarsızlığına yol açıyor.
    *   **Görev:** `Create` metodunu refactor et. Tüm veritabanı işlemlerini (Fatura, Detay, StokHareket, CariHareket ekleme; FIFO servisi çağırma; Ürün stok güncelleme; Opsiyonel İrsaliye oluşturma) **tek bir `DbContextTransaction`** kapsamına al. Herhangi bir adımda hata oluşursa (özellikle `StokFifoService` çağrılarında veya irsaliye oluşturmada), transaction'ı tamamen `RollbackAsync` ile geri al ve kullanıcıya uygun bir hata mesajı göster. `SaveChangesAsync` çağrısını transaction'ın sonuna, `CommitAsync`'ten hemen önceye taşı.

2.  **Fatura Oluşturma Hata Yönetimi (`FaturaController.cs` -> `Create` metodu):**
    *   **Sorun:** `StokYetersizException` yakalandığında transaction geri alınmıyor ve sadece `TempData` ile mesaj gösteriliyor.
    *   **Görev:** `StokYetersizException` dahil olmak üzere `StokFifoService` çağrılarından veya diğer adımlardan kaynaklanan hatalar yakalandığında, başlatılan ana transaction'ın `RollbackAsync` ile geri alındığından emin ol. Kullanıcıya işlemin başarısız olduğunu ve nedenini açıkça belirten bir mesaj göster (`TempData` veya `ModelState`).

3.  **Banka-Cari Entegrasyonu (`BankaController.cs` -> `YeniHareket` metodu):**
    *   **Sorun:** Banka hareketi eklerken (`YeniHareket` POST metodu) bir cari (`hareket.CariID`) seçilebilmesine rağmen, bu işlem için ilgili `CariHareket` (tahsilat/ödeme) otomatik olarak oluşturulmuyor.
    *   **Görev:** `YeniHareket` POST metodunu güncelle. Eğer `hareket.CariID` geçerli bir değer içeriyorsa:
        *   Banka hareketinin türüne göre (`Para Yatırma`, `EFT Alma`, `Havale Alma` -> Cari Alacak; `Para Çekme`, `EFT Gönderme`, `Havale Gönderme` -> Cari Borç) uygun bir `CariHareket` nesnesi oluştur.
        *   Bu `CariHareket` nesnesini `_unitOfWork.CariHareketRepository.AddAsync` kullanarak veritabanına ekle.
        *   Bu işlemi banka hareketi ve bakiye güncellemesi ile aynı transaction içinde gerçekleştir.

4.  **Cari Ekstre Kur Hesaplama (`CariController.cs` -> `Ekstre` metodu):**
    *   **Sorun:** `Ekstre` metodu, farklı para birimlerinde raporlama yaparken tüm hareketler için rapor anındaki *tek bir güncel kuru* kullanıyor. Bu, geçmiş hareketlerin değerini yanlış yansıtabilir. Hesaplama mantığı karmaşık.
    *   **Görev:** `Ekstre` metodunu gözden geçir. İki alternatif yaklaşımı değerlendir:
        *   **Alternatif 1 (Hareket Anı Kuru):** Her `CariHareket` için, hareketin `Tarih` alanına en yakın kur değerini (`DovizKuruService` veya `KurDegerleri` tablosundan) al ve raporlama para birimine çevir. Bakiyeyi adım adım bu çevrilmiş değerlerle hesapla.
        *   **Alternatif 2 (Basitleştirilmiş Güncel Kur):** Eğer geçmiş kurları kullanmak karmaşıksa, mevcut yaklaşımı (tek güncel kur) koru ancak kodu sadeleştir. `CariEkstreViewModel` ve hesaplama döngüsünü daha okunabilir hale getir. Açılış bakiyesinin ve hareketlerin seçilen kur ile tutarlı bir şekilde dönüştürüldüğünden emin ol.
    *   **Not:** Hangi yaklaşımın iş gereksinimlerine uygun olduğunu değerlendir.

5.  **Numara Üretme Mantığını Merkezileştirme (Opsiyonel):**
    *   **Sorun:** Fatura, Sipariş ve İrsaliye numaraları (`GenerateNew...` metotları) `FaturaController` içinde benzer mantıklarla üretiliyor.
    *   **Görev:** Bu numara üretme mantığını ayrı bir `NumaraUretmeService` veya benzeri bir yardımcı sınıfa taşı. Bu servis, ilgili entity için bir sonraki numarayı üretmekten sorumlu olsun. Controller'lar bu servisi kullansın.

6.  **Kur Alma ve Para Birimi Çevirme Mantığını Merkezileştirme (Opsiyonel):**
    *   **Sorun:** Kur alma ve çevirme işlemleri `StokFifoService`, `CariController` gibi farklı yerlerde yapılıyor.
    *   **Görev:** Tüm kur alma (`GetGuncelKurAsync` vb.) ve para birimi çevirme işlemlerini `IDovizKuruService` içinde topla. Diğer servisler ve controller'lar sadece bu servisi kullansın. `StokFifoService` içindeki `ParaBirimiCevirAsync` ve `DovizliFiyatHesapla` gibi metotları bu merkezi servisi kullanacak şekilde refactor et veya tamamen bu servise taşı.

**Beklenen Sonuç:** Daha sağlam, tutarlı ve bakımı kolay bir kod tabanı. Modüller arası veri akışının doğru ve transaction'ların atomik olması. Banka ve cari kayıtlarının senkronize olması.
