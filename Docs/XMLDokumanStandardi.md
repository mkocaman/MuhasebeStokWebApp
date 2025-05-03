# MuhasebeStokWebApp XML Dokümantasyon Standardı

Bu rehber, MuhasebeStokWebApp projesinde kullanılacak XML dokümantasyon standardını açıklamaktadır. Tüm sınıflar, arayüzler ve metotlar bu standarda göre dokümante edilmelidir.

## Genel Kurallar

1. Tüm **public** ve **protected** üyeler (sınıflar, arayüzler, metotlar, özellikler, alanlar) XML dokümantasyonu içermelidir.
2. Dokümantasyon Türkçe yazılmalıdır.
3. Dokümantasyon, kodu açıklayıcı ve anlaşılır olmalıdır.
4. Dokümantasyon, kodun ne yaptığını değil, ne amaçla yapıldığını açıklamalıdır.

## XML Etiketleri

### Sınıflar ve Arayüzler

```csharp
/// <summary>
/// Sınıfın veya arayüzün amacı ve sorumluluğu
/// </summary>
/// <remarks>
/// Ek açıklamalar, kullanım örnekleri, dikkat edilmesi gereken noktalar
/// </remarks>
public class OrnekSinif
{
    // Sınıf içeriği
}
```

### Metotlar

```csharp
/// <summary>
/// Metodun amacı ve ne yaptığı
/// </summary>
/// <param name="parametre1">Parametre1'in açıklaması</param>
/// <param name="parametre2">Parametre2'nin açıklaması</param>
/// <returns>Dönüş değerinin açıklaması</returns>
/// <exception cref="ExceptionTipi">Ne zaman bu istisnanın fırlatılacağı</exception>
/// <remarks>
/// Ek açıklamalar, kullanım örnekleri, dikkat edilmesi gereken noktalar
/// </remarks>
public TipAdi MetotAdi(Tip1 parametre1, Tip2 parametre2)
{
    // Metot içeriği
}
```

### Özellikler

```csharp
/// <summary>
/// Özelliğin amacı ve kullanımı
/// </summary>
/// <value>Değerin açıklaması</value>
public TipAdi OzellikAdi { get; set; }
```

### Alanlar

```csharp
/// <summary>
/// Alanın amacı ve kullanımı
/// </summary>
private TipAdi _alanAdi;
```

## Örnek Uygulama

Aşağıda, XML dokümantasyonunun nasıl kullanılacağına dair bir örnek verilmiştir:

```csharp
/// <summary>
/// Stok hareketlerini FIFO (First In, First Out) prensibine göre yöneten servis sınıfı
/// </summary>
/// <remarks>
/// Bu sınıf, stok giriş ve çıkış işlemlerini FIFO prensibi ile yönetir.
/// Stok maliyet hesaplamaları ve stok takibi için kullanılır.
/// </remarks>
public class StokFifoService : IStokFifoService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<StokFifoService> _logger;

    /// <summary>
    /// StokFifoService için yeni bir örnek oluşturur
    /// </summary>
    /// <param name="unitOfWork">Veritabanı işlemlerini yöneten UnitOfWork nesnesi</param>
    /// <param name="logger">Loglama işlemleri için logger nesnesi</param>
    public StokFifoService(IUnitOfWork unitOfWork, ILogger<StokFifoService> logger)
    {
        _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Belirtilen ürün için ortalama maliyet hesaplar
    /// </summary>
    /// <param name="urunId">Ürün ID</param>
    /// <param name="paraBirimi">Para birimi (varsayılan: USD)</param>
    /// <returns>Hesaplanan ortalama maliyet</returns>
    /// <exception cref="ArgumentException">Geçersiz urunId değeri için fırlatılır</exception>
    /// <exception cref="InvalidOperationException">Maliyet hesaplama sırasında bir hata oluştuğunda fırlatılır</exception>
    public async Task<decimal> GetOrtalamaMaliyet(Guid urunId, string paraBirimi = "USD")
    {
        // Metot içeriği
    }
}
```

## Ek Bilgiler

- XML dokümantasyonu, IntelliSense ile IDE'de görüntülenir ve kullanıcıya kod hakkında bilgi verir.
- XML dokümantasyonu, derleme sırasında XML dosyasına çıkarılabilir ve API dokümantasyonu oluşturmak için kullanılabilir.
- Visual Studio'da, bir üyenin üzerine geldiğinizde `///` yazarak otomatik olarak XML dokümantasyon iskeleti oluşturabilirsiniz.
- Rider ve Visual Studio Code'da da benzer özellikler mevcuttur.

## Özel Etik Blokları

Özellikle karmaşık işlemler veya algoritmaların açıklanması gerektiğinde, normal kod yorumları içinde aşağıdaki gibi özel bloklar kullanılabilir:

```csharp
// AÇIKLAMA: Bu algoritma şu amaçla tasarlanmıştır...

// ÖNEMLİ: Bu metot çağrılmadan önce veritabanında şu kontroller yapılmalıdır...

// NOT: Bu değişken farklı para birimlerinde çalışabilmek için kullanılır...

// UYARI: Bu metot, çok sayıda veritabanı sorgusu yapabilir ve performans sorunlarına neden olabilir...

// TODO: Bu metot gelecekte şu şekilde iyileştirilmelidir...
``` 