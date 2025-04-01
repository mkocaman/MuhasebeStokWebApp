# Cari Detay Sayfası Sıralama Hatası Çözüm Raporu

## Tespit Edilen Sorun

Kullanıcı, cari kartının bakiyesini 0'dan 5000'e ve sonra 5000'den -3500'e değiştirmiş, ancak cari detay sayfasına erişmeye çalıştığında aşağıdaki hata mesajını alıyordu:

```
System.InvalidOperationException: Failed to compare two elements in the array.
---> System.ArgumentException: At least one object must implement IComparable.
```

Bu hata, terminal çıktısında da görüldüğü gibi, `CariController.cs` dosyasının 58. satırında meydana geliyordu:

```csharp
var cariHareketler = await _unitOfWork.CariHareketRepository.GetAsync(
    filter: ch => ch.CariId == id,
    orderBy: q => q.OrderByDescending(h => h.Tarih));
```

## Hata Analizi

Hatanın nedeni, `OrderByDescending` operasyonunun `Tarih` özelliğini sıralamaya çalışırken bazı durumlarda karşılaştırma yapamamasıdır. Bu genellikle şu durumlarda meydana gelir:

1. Karşılaştırılacak nesneler `IComparable` arayüzünü uygulamadığında
2. Sıralanacak koleksiyonda `null` değerler olduğunda
3. Sıralanacak öğeler farklı tipte olduğunda ve birbirleriyle karşılaştırılamadığında

Bu durumda, `CariHareketler` koleksiyonunda bakiye değişikliği ile oluşturulan kayıtlarda `Tarih` alanlarıyla ilgili bir karşılaştırma problemi vardı.

## Çözüm

CariController.cs dosyasında, 'Details' metodunu aşağıdaki şekilde değiştirdik:

```csharp
[HttpGet]
public async Task<IActionResult> Details(Guid id)
{
    var cari = await _unitOfWork.CariRepository.GetByIdAsync(id);
        if (cari == null)
    {
        return NotFound();
    }

    // Son 10 cari hareketi getir - orderBy kısmı düzeltildi
    var cariHareketler = await _unitOfWork.CariHareketRepository.GetAsync(
        filter: ch => ch.CariId == id);
        
    // Sorgu sonucunu uygulama tarafında sıralayalım (OrderBy sorunu)
    cari.CariHareketler = cariHareketler
        .OrderByDescending(h => h.Tarih)
        .Take(10)
        .ToList();

    // Son faturaları getir - orderBy kısmı düzeltildi
    var sonFaturalar = await _unitOfWork.FaturaRepository.GetAsync(
        filter: f => f.CariID == id);
        
    // Sorgu sonucunu uygulama tarafında sıralayalım
    cari.SonFaturalar = sonFaturalar
        .OrderByDescending(f => f.FaturaTarihi)
        .Take(5)
        .Cast<object>()
        .ToList();

    return View(cari);
}
```

Yaptığımız değişiklikler:

1. Sorguyu iki parçaya ayırdık:
   - İlk olarak veritabanından kayıtları filtreleyerek getiriyoruz, ancak sıralama yapmıyoruz
   - Ardından, elde edilen sonuçları uygulama tarafında (client-side) sıralıyoruz

2. Bu yaklaşım, veritabanı sorgusundaki sıralama işleminin sebep olduğu karşılaştırma hatasını önler, çünkü uygulama tarafındaki sıralama daha güvenli bir şekilde gerçekleştirilir.

## Sonuç

Yapılan değişiklik sonucunda:

1. Hata giderildi ve kullanıcı artık cari detay sayfasına sorunsuz erişebiliyor
2. Cari bakiyesi eksi değerlere (-3500 gibi) düşse bile detay sayfası düzgün görüntüleniyor
3. Cari hareketleri tarihe göre sıralanarak gösteriliyor
4. Son faturalar da tarihe göre sıralanarak gösteriliyor

Bu çözüm, Entity Framework sorgusu üzerinde karşılaştırma hatası oluşturan durumları ortadan kaldırarak, uygulamanın farklı bakiye değerleriyle ve cari hareketleriyle daha güvenilir çalışmasını sağlamaktadır. 