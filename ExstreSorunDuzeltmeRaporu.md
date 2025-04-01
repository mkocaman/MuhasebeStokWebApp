# Ekstre Görünümü Sorunları ve Çözüm Raporu

## 1. Tespit Edilen Sorunlar

İncelenen ekstre görünümünde aşağıdaki sorunlar tespit edilmiştir:

1. **Mükerrer Açılış Bakiyesi Sorunu**: Ekstre görünümünde iki adet "Cari oluşturulurken belirlenen bakiye" satırı görünüyordu.

2. **Bakiye Hesaplama Sorunları**: 
   - Bakiye 5000'e güncellendiğinde, cari hareketlerde göründüğü gibi (açılış bakiyesi 0, bakiye düzeltmesi 5000) ekstre görünümünde de doğru şekilde gösterilmiyordu.
   - Bakiye -3500'e güncellendiğinde hesaplamalar tutarsız oluyordu.
   - Bakiyeler toplamı hatalı görünüyordu.

3. **Açılış Bakiyesi İşleme Sorunu**: Ekstre görünümünde açılış bakiyesi hareketi ile gerçek hareketler arasında tutarsızlık vardı.

## 2. Sorunların Analizi

### 2.1. Mükerrer Açılış Bakiyesi Sorunu
"Açılış bakiyesi" kaydı hem veritabanındaki gerçek bir kayıttan, hem de görüntüleme için ekstra eklenen bir kayıttan geliyordu. Bu da raporda aynı açıklamaya sahip iki kaydın görünmesine neden oluyordu.

### 2.2. Bakiye Hesaplama Sorunları
Ekstre metodunda, bakiye hesaplama mantığında karışıklık vardı:

- Başlangıç bakiyesi (`baslangicBakiyesi`) hesaplanırken, cari tanımındaki değer alınıp tüm hareketler üzerine ekleniyor, ancak açılış bakiyesi hareketi de dahil ediliyordu. Bu da başlangıç bakiyesinin iki kez hesaplamaya katılmasına neden oluyordu.

- Bakiye hesaplaması, tüm cari hareketleri içinde filtre yapmadan hesaplanıyordu, bu da tarih aralığı dışındaki hareketlerin de hesaba katılmasına yol açabiliyordu.

## 3. Yapılan Değişiklikler

Aşağıdaki değişiklikler yapılarak sorunlar çözülmüştür:

### 3.1. Hareketlerin Daha Net Ayrılması
```csharp
// Veritabanındaki tüm cari hareketlerini getir
var tumHareketler = await _unitOfWork.CariHareketRepository.GetAsync(
    filter: ch => ch.CariId == id);

// Açılış bakiyesi hareketlerini ayrı al 
var acilisBakiyeHareketleri = tumHareketler.Where(h => h.HareketTuru == "Açılış bakiyesi").OrderBy(h => h.Tarih).ToList();

// Diğer hareketleri al
var digerHareketler = tumHareketler.Where(h => h.HareketTuru != "Açılış bakiyesi").OrderBy(h => h.Tarih).ToList();
```

### 3.2. Doğru Başlangıç Bakiyesinin Hesaplanması
```csharp
// Veritabanında açılış bakiyesi hareketi yoksa, cari.BaslangicBakiye'yi kullan
decimal baslangicBakiyesi = 0;

// İlk açılış bakiyesi hareketi varsa onu kullan
if (acilisBakiyeHareketleri.Any())
{
    var ilkAcilisBakiyeHareketi = acilisBakiyeHareketleri.First();
    baslangicBakiyesi = ilkAcilisBakiyeHareketi.Alacak - ilkAcilisBakiyeHareketi.Borc;
}
else
{
    baslangicBakiyesi = cari.BaslangicBakiye;
}
```

### 3.3. Önceki Hareketlerin Etkisinin Doğru Hesaplanması
```csharp
// Başlangıç tarihinden önceki diğer hareketlerin etkisini hesapla
decimal oncekiHareketlerinEtkisi = 0;
foreach (var oncekiHareket in digerHareketler.Where(h => h.Tarih < startDate))
{
    if (oncekiHareket.HareketTuru == "Bakiye Düzeltmesi")
    {
        oncekiHareketlerinEtkisi += oncekiHareket.Alacak - oncekiHareket.Borc;
    }
    else if (oncekiHareket.HareketTuru == "Ödeme" || oncekiHareket.HareketTuru == "Borç" || oncekiHareket.HareketTuru == "Çıkış")
    {
        oncekiHareketlerinEtkisi -= oncekiHareket.Tutar;
    }
    else if (oncekiHareket.HareketTuru == "Tahsilat" || oncekiHareket.HareketTuru == "Alacak" || oncekiHareket.HareketTuru == "Giriş")
    {
        oncekiHareketlerinEtkisi += oncekiHareket.Tutar;
    }
}

// Başlangıç bakiyesine önceki hareketlerin etkisini ekle
baslangicBakiyesi += oncekiHareketlerinEtkisi;
```

### 3.4. Tarih Aralığındaki Hareketleri Doğru Filtreleme
```csharp
// Şimdi gösterilecek hareketleri seçilen tarih aralığına göre filtrele
var gosterilecekHareketler = digerHareketler
    .Where(h => h.Tarih >= startDate && h.Tarih <= endDate)
    .ToList();
```

### 3.5. Tekli Açılış Bakiyesi Gösterimi
Ekstre görünümünde sadece tek bir açılış bakiyesi gösteriliyor, bu da veritabanından gelen açılış bakiyesi hareketi yerine hesaplanan başlangıç bakiyesi değerini kullanıyor:

```csharp
// Cari açılış bakiyesi - sadece görüntüleme için
var acilisBakiyeGosterimi = new Models.CariHareketViewModel
{
    Tarih = startDate.AddDays(-1),
    IslemTuru = "Açılış bakiyesi",
    Aciklama = "Cari oluşturulurken belirlenen bakiye",
    Kaynak = "Sistem",
    Borc = baslangicBakiyesi < 0 ? Math.Abs(baslangicBakiyesi) : 0,
    Alacak = baslangicBakiyesi > 0 ? Math.Abs(baslangicBakiyesi) : 0,
    Bakiye = baslangicBakiyesi
};

model.Hareketler.Add(acilisBakiyeGosterimi);
```

## 4. Elde Edilen Sonuçlar

Yapılan değişiklikler sonucunda:

1. Ekstre görünümünde artık sadece tek bir açılış bakiyesi satırı görünüyor, ve bu satır doğru bakiyeyi gösteriyor.

2. Bakiye güncelleme işlemleri doğru şekilde rapora yansıyor:
   - 5000 TL bakiye güncellemesi yapıldığında, ekstre görünümünde doğru şekilde gösteriliyor.
   - -3500 TL bakiye güncellemesi yapıldığında da hesaplamalar tutarlı olarak yapılıyor.

3. Ekstre görünümündeki bakiye toplamları artık doğru şekilde hesaplanıyor:
   - Başlangıç bakiyesi doğru hesaplanıyor (açılış bakiyesi + önceki hareketlerin etkisi).
   - Tarih aralığındaki hareketler doğru şekilde filtreleniyor.
   - Her hareketin bakiyeye etkisi doğru şekilde hesaplanıyor (borç/alacak durumuna göre).

## 5. Test Sonuçları

Yapılan değişiklikler başarıyla derlenmiştir (140 adet uyarı mevcut, ancak bunlar mevcut geliştirme sürecinden kaynaklanan ve kodun çalışmasını etkilemeyen uyarılardır).

Çeşitli senaryolarla test edildiğinde:
- Yeni cari oluşturma
- Sıfır bakiyeli cari oluşturma
- Bakiye düzeltme (pozitif ve negatif)
- Farklı tarih aralıklarında ekstre görüntüleme

işlemlerinin tümünde ekstre hesaplamalarının artık doğru çalıştığı görülmüştür.

## 6. Öneriler

1. Cari hareketleri daha iyi yönetmek için "İşlem Türü" bazlı bir kategorizasyon sistemi eklenebilir.
2. Borç ve alacak hesaplamalarını otomatikleştirmek için işlem türlerine göre otomatik hesaplama mantığı sisteme eklenebilir.
3. Ekstre raporunda daha fazla filtreleme ve gruplama seçeneği eklenebilir.
4. Performans iyileştirmesi için büyük veri setlerinde sayfalama mekanizması eklenebilir. 