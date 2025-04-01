# Cari Bakiye Düzeltme İşlemi 100 Kat Hesaplama Hatası Düzeltme Raporu

## Tespit Edilen Sorun

Cari ekstre ekranında (Cari/Ekstre) bakiye düzeltmesi işlemlerinde ciddi bir hesaplama hatası tespit edildi:

1. İlk bakiye düzeltmesinden sonra, bakiye 5.000 TL olması gerekirken 500.000 TL olarak gösteriliyordu (100 kat fazla)
2. İkinci bakiye düzeltmesinden sonra, bakiye -3.500 TL olması gerekirken -350.000 TL olarak gösteriliyordu (100 kat fazla)

Bakiye değerleri sistemde 100 katı olarak gösterildiği için, finansal raporlama açısından ciddi bir hata oluşuyordu.

## Sorunun Nedeni

Önceki düzeltmede, bakiye değerlerini açıklama metninden parse etmeye çalışmıştık:

```csharp
// Açıklamadaki "Yeni bakiye" değerini al
decimal yeniBakiye = 0;
if (aciklama.Contains("Yeni bakiye:"))
{
    var yeniBakiyeText = aciklama.Split("Yeni bakiye:")[1].Trim();
    yeniBakiyeText = yeniBakiyeText.Replace(".", "").Replace(",", "."); // Sayı formatını düzelt
    if (decimal.TryParse(yeniBakiyeText, out decimal parsedBakiye))
    {
        yeniBakiye = parsedBakiye;
    }
}

// ...

// Model için bakiyeyi oluşturma amacıyla direk yeni bakiyeyi atayalım
bakiye = yeniBakiye;
```

Bu yaklaşımda iki temel sorun vardı:
1. Ondalık ayırıcı dönüşümünde (`.` ve `,` karakterlerinin değiştirilmesi) sorun olabilir
2. Türkçe para birimi formatlaması "5.000,00" şeklinde olduğu için, `.` karakterini kaldırdığımızda "5000,00" oluyordu, ancak sonra `,` karakteri de `.` ile değiştirilince "500000" olarak yorumlanıyordu

Bu da bakiye değerlerinin 100 katı olarak gösterilmesine neden oluyordu.

## Çözüm

Bu tür karmaşık string manipülasyonlarından kaçınmak için, daha basit ve güvenilir bir yaklaşım benimsedim:

```csharp
if (hareket.HareketTuru == "Bakiye Düzeltmesi")
{
    // Bakiye düzeltmesi için doğrudan hareketin açıklamasından değer alma yerine 
    // borç ve alacak değerlerini kullanarak hesaplama yapılacak
    
    // Borç ve alacak değerlerini doğrudan kullan
    borc = hareket.Borc;
    alacak = hareket.Alacak;
    
    toplamBorc += borc;
    toplamAlacak += alacak;
    
    // Bakiyeyi borç ve alacak değerlerine göre güncelle
    bakiye = bakiye - borc + alacak;
}
```

Bu değişiklikle:
1. Açıklama metnini parse etmek yerine, doğrudan veritabanında zaten kayıtlı olan ve doğru değerleri içeren borç ve alacak alanlarını kullanıyoruz
2. Bakiye hesaplaması matematiksel olarak doğru şekilde yapılıyor: `bakiye = bakiye - borc + alacak`
3. String manipülasyonu ve sayı formatı dönüşümleri nedeniyle oluşabilecek hatalardan kaçınıyoruz

## Sonuç

Bu değişikliklerle birlikte:

1. İlk bakiye düzeltmesinden sonra, bakiye doğru şekilde 5.000 TL olarak görüntüleniyor
2. İkinci bakiye düzeltmesinden sonra, bakiye doğru şekilde -3.500 TL olarak görüntüleniyor
3. Toplam değerler de doğru şekilde hesaplanıyor

Artık cari ekstre ekranında bakiye düzeltmeleri doğru şekilde gösteriliyor ve finansal raporlamada ciddi hatalara neden olabilecek bu sorun çözülmüş durumda. 