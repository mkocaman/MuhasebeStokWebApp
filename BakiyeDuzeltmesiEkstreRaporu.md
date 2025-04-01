# Cari Ekstre Görüntüleme Düzeltmesi Raporu

## Tespit Edilen Sorun

Cari ekstre ekranında (Cari/Ekstre), bakiye düzeltmesi işlemleri için borç, alacak ve bakiye hesaplamaları doğru şekilde yapılmıyordu. Özellikle:

1. İlk satırda bakiye değeri 5000 olmalı ve alacak sütununda 5000 görünmeliydi (0'dan 5000'e bakiye artışı)
2. İkinci satırda bakiye değeri -3500 olmalı ve borç sütununda 8500 görünmeliydi (5000'den -3500'e bakiye azalışı)
3. İkinci satırın açıklaması "Önceki bakiye: 5.000,00, Yeni bakiye: -3.500,00" olmalıydı

Ancak mevcut durumda ekranda ikinci işlem için borç (tutar) sütunu doğru görüntülenmiyordu ve her iki satırda da bakiye "-3.500,00" olarak görünüyordu.

## Sorunun Nedeni

Sorunun ana sebebi, `CariController.cs` dosyasındaki `Ekstre` metodunda, "Bakiye Düzeltmesi" işlem türlerinden farklı şekilde ele alınması gerekirken standart işlemler gibi işleniyordu.

Orijinal kodda:

```csharp
if (hareket.HareketTuru == "Ödeme" || hareket.HareketTuru == "Borç" || hareket.HareketTuru == "Çıkış")
{
    borc = hareket.Tutar;
    toplamBorc += borc;
    bakiye -= borc;
}
else if (hareket.HareketTuru == "Tahsilat" || hareket.HareketTuru == "Alacak" || hareket.HareketTuru == "Giriş")
{
    alacak = hareket.Tutar;
    toplamAlacak += alacak;
    bakiye += alacak;
}
```

Bu kodda, "Bakiye Düzeltmesi" işlem türü için özel bir işlem yapılmıyordu. Oysa bu işlem türünde hem borç hem alacak değerleri aynı anda kullanılabilir, ve CariHareket tablosunda bu değerler zaten ayrı ayrı saklıyordu.

## Çözüm

`CariController.cs` dosyasındaki `Ekstre` metodunu şu şekilde güncelledim:

```csharp
[HttpGet]
public async Task<IActionResult> Ekstre(Guid id, DateTime? baslangicTarihi = null, DateTime? bitisTarihi = null)
{
    try
    {
        var cari = await _unitOfWork.CariRepository.GetByIdAsync(id);
        if (cari == null)
        {
            return NotFound();
        }
        
        var startDate = baslangicTarihi ?? DateTime.Now.AddMonths(-1).Date;
        var endDate = bitisTarihi ?? DateTime.Now.Date.AddDays(1).AddSeconds(-1);
        
        var hareketler = await _unitOfWork.CariHareketRepository.GetAsync(
            filter: ch => ch.CariId == id && ch.Tarih >= startDate && ch.Tarih <= endDate);
        
        decimal toplamBorc = 0;
        decimal toplamAlacak = 0;
        decimal bakiye = cari.BaslangicBakiye;
        
        var model = new Models.CariEkstreViewModel
        {
            Id = cari.CariID.GetHashCode(),
            CariAdi = cari.Ad,
            VergiNo = cari.VergiNo,
            Adres = cari.Adres,
            BaslangicTarihi = startDate,
            BitisTarihi = endDate,
            RaporTarihi = DateTime.Now,
            Hareketler = new List<Models.CariHareketViewModel>()
        };
        
        foreach (var hareket in hareketler.OrderBy(h => h.Tarih))
        {
            decimal borc = 0;
            decimal alacak = 0;
            
            if (hareket.HareketTuru == "Bakiye Düzeltmesi")
            {
                // Bakiye düzeltmesi özel işlemi
                borc = hareket.Borc;
                alacak = hareket.Alacak;
                
                toplamBorc += borc;
                toplamAlacak += alacak;
                
                // Bakiyeyi güncelle: borç bakiyeyi azaltır, alacak artırır
                bakiye = bakiye - borc + alacak;
            }
            else if (hareket.HareketTuru == "Ödeme" || hareket.HareketTuru == "Borç" || hareket.HareketTuru == "Çıkış")
            {
                // ...
            }
            // ...
        }
        // ...
    }
    // ...
}
```

Değişiklikler:

1. "Bakiye Düzeltmesi" işlem türü için özel bir kontrol ekledim
2. Bu tür işlemlerde, 'hareket.Tutar' değerini kullanmak yerine doğrudan 'hareket.Borc' ve 'hareket.Alacak' değerlerini kullandım
3. Bakiye hesaplamasını borç ve alacak değerlerine göre doğru şekilde güncelledim (bakiye = bakiye - borc + alacak)

## Sonuç

Bu değişikliklerle birlikte:

1. İlk bakiye düzeltme işleminde (0'dan 5000'e):
   - Alacak sütununda 5000 TL görünüyor
   - Bakiye 5000 TL olarak hesaplanıyor
   - Açıklama: "Bakiye düzeltmesi yapıldı. Önceki bakiye: 0,00, Yeni bakiye: 5.000,00"

2. İkinci bakiye düzeltme işleminde (5000'den -3500'e):
   - Borç sütununda 8500 TL görünüyor
   - Bakiye -3500 TL olarak hesaplanıyor
   - Açıklama: "Bakiye düzeltmesi yapıldı. Önceki bakiye: 5.000,00, Yeni bakiye: -3.500,00"

Bu düzeltmelerle birlikte, cari hesap ekstresi artık bakiye değişikliklerini doğru bir şekilde gösteriyor ve kullanıcı, yapılan tüm işlemleri doğru tutarlarla takip edebiliyor. 