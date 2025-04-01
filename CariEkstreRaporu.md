# Cari Ekstre Ekranı Düzeltme Raporu

## Tespit Edilen Sorunlar

Cari ekstre (Cari/Ekstre) ekranında aşağıdaki sorunlar tespit edilmiştir:

1. **Ekstre sayfasından Cari Detayına dönüş hatası**: Ekstre ekranından "Cari Detayına Dön" butonuna tıklandığında, sistem yanlış ID değeri kullanarak (`CariID.GetHashCode()`) 404 hatası veriyordu.

2. **Bakiye ve hareket hesaplama sorunları**: 
   - Bakiye düzeltmesi hareketlerinin borç ve alacak alanları doğru görüntülenmiyordu.
   - Hesaplama formülü örnek Excel tablosundaki gibi değildi.
   - Açılış bakiyesi hareketi eksikti.

3. **Hesaplamaların şeffaflığı**: Bakiye hesaplamaları kullanıcılar tarafından takip edilemiyordu.

## Yapılan Değişiklikler

### 1. ID Yönlendirme Sorunu Düzeltildi

```csharp
// Eski kod
Id = cari.CariID.GetHashCode() // Bu kod Guid'i int'e çeviriyordu ve veri kaybına neden oluyordu

// Yeni kod  
Id = cari.CariID // Doğrudan Guid değerini kullanıyoruz
```

Ayrıca, CariEkstreViewModel sınıfındaki Id özelliği int'ten Guid tipine değiştirildi:

```csharp
// Eski tanım
public int Id { get; set; }

// Yeni tanım
public Guid Id { get; set; }
```

### 2. Ekstre Hesaplama Mantığı Yenilendi

1. **Açılış bakiyesi eklendi**: Ekstre hesaplamasına başlamadan önce, listenin en başına bir "Açılış bakiyesi" hareketi eklendi. Bu hareket, seçilen tarih aralığının başlangıcında carinin sahip olduğu bakiyeyi gösterir.

2. **Hareket hesaplama formülü düzeltildi**:
   - Bakiye = Önceki bakiye - borç + alacak
   - Borç ve alacak değerleri veri tabanından doğrudan alınıyor
   - Hareket türüne göre doğru şekilde raporlanıyor

3. **Ekranı Excel ile tutarlı hale getirmek**:
   - Açılış bakiyesi seçili tarih aralığının bir gün öncesine tarihlendirildi
   - Tüm bakiye hesaplamaları şeffaf ve izlenebilir duruma getirildi

## Sonuç

Bu değişikliklerle birlikte:

1. Cari ekstre ekranından cari detay sayfasına dönüş artık sorunsuz çalışıyor
2. Açılış bakiyesi ekstre listesinde ilk sırada görünüyor
3. Tüm hareketlerin bakiyeleri doğru hesaplanıyor
4. Bakiye düzeltmesi hareketlerinde borç ve alacak değerleri doğru şekilde gösteriliyor
5. Ekstre raporu örnek Excel tablosundaki formülle aynı şekilde çalışıyor

Yapılan düzeltmeler hiçbir veritabanı değişikliği gerektirmedi. Sadece kodun hesaplama mantığı ve görüntüleme şekli güncellendi. Bu değişiklikler, kullanıcıların finansal verilerini daha doğru ve tutarlı bir şekilde görmelerine yardımcı olacaktır. 