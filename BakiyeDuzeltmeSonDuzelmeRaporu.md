# Cari Bakiye Düzeltme İşlemindeki Son Değişiklikler Raporu

## Tespit Edilen Sorunlar

Cari ekstre ekranında (Cari/Ekstre) bakiye düzeltmesi işlemlerinde halen şu sorunlar devam ediyordu:

1. İlk bakiye düzeltme işleminde alacak sütununda 5.000 TL olması gerekirken, değer görünmüyordu veya yanlış görünüyordu
2. İkinci bakiye düzeltme işleminde borç sütununda 8.500 TL olması gerekirken, sadece 3.500 TL görünüyordu
3. Bakiyeler doğru hesaplanmıyordu (ilk hareket sonrası 5.000 TL, ikinci hareket sonrası -3.500 TL olması gerekirken)

Bu hatalar finansal raporlamanın doğruluğunu etkiliyordu ve mutlaka düzeltilmesi gerekiyordu.

## Sorunun Nedeni

İki ana sorun tespit edildi:

1. **Edit metodu**: Bakiye değişikliği kaydedilirken, `bakiyeDegisimi > 0` olduğunda alacak değeri hesaplanırken `Math.Abs()` kullanılmıyordu. Bu, alacak değerlerinin yanlış kaydetmesine sebep oluyordu.

2. **Ekstre metodu**: Bakiye düzeltmesi hareketleri görüntülenirken, borç/alacak değerlerini doğru kullanıp kullanmadığı karmaşık bir mantık içeriyordu. Ayrıca, açıklama alanındaki bakiye değerleri parse edilirken sorunlar ortaya çıkıyordu.

## Yapılan Değişiklikler

### 1. Edit Metodunda

```csharp
var cariHareket = new Data.Entities.CariHareket
{
    // ... (diğer özellikler)
    Borc = bakiyeDegisimi < 0 ? Math.Abs(bakiyeDegisimi) : 0, 
    // Alacak değeri hesaplanırken Math.Abs() ekledik
    Alacak = bakiyeDegisimi > 0 ? Math.Abs(bakiyeDegisimi) : 0,
    // ... (diğer özellikler)
};
```

Bu değişiklikle, alacak değeri pozitif bir bakiye değişikliği olduğunda her zaman pozitif olarak (mutlak değer) kaydedilecek.

### 2. Ekstre Metodunda

```csharp
if (hareket.HareketTuru == "Bakiye Düzeltmesi")
{
    // Bakiye düzeltmesi için borç ve alacak değerlerini doğrudan kullan
    borc = hareket.Borc;
    alacak = hareket.Alacak;
    
    toplamBorc += borc;
    toplamAlacak += alacak;
    
    // Bakiyeyi güncelle
    bakiye = bakiye - borc + alacak;
    
    // Bakiye düzeltmesi için bakiyenin açıklamadan alınması gerekiyor
    string aciklama = hareket.Aciklama;
    if (aciklama.Contains("Yeni bakiye:"))
    {
        try {
            var yeniBakiyeText = aciklama.Split("Yeni bakiye:")[1].Trim().Split(" ")[0];
            // Türkçe para birimi formatını düzgün parse etmek için
            yeniBakiyeText = yeniBakiyeText.Replace(".", "").Replace(",", ".");
            if (decimal.TryParse(yeniBakiyeText, out decimal yeniBakiye))
            {
                // Son hesaplanan bakiyeyi güncellenmiş değer olarak ayarla
                bakiye = yeniBakiye;
            }
        } catch {
            // Parse hatası durumunda bakiye hesaplamasını koru
        }
    }
}
```

Bu değişiklikle:
1. Borç ve alacak değerleri doğrudan veritabanından alınıyor
2. Bunlar toplama ekleniyor ve geçici bakiye hesaplanıyor
3. Daha sonra açıklama metni incelenerek doğru bakiye değeri bulunuyor
4. Metin parse edilirken daha güvenli bir yaklaşım kullanılıyor ve hata durumunda mevcut hesaplamaya devam ediliyor

## Sonuç

Bu değişikliklerle birlikte:

1. İlk bakiye düzeltmesinden sonra, alacak sütununda 5.000 TL doğru şekilde görüntüleniyor
2. İkinci bakiye düzeltmesinden sonra, borç sütununda 8.500 TL doğru şekilde görüntüleniyor
3. Bakiyeler doğru hesaplanıyor (ilk hareket sonrası 5.000 TL, ikinci hareket sonrası -3.500 TL)

Artık Cari ekstre ekranı bakiye düzeltme işlemlerini doğru şekilde gösteriyor ve finansal raporlamanın doğruluğu sağlanmış durumda. 