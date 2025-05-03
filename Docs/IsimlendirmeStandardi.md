# MuhasebeStokWebApp İsimlendirme Standardı

Bu rehber, MuhasebeStokWebApp projesindeki sınıf, metot, değişken ve diğer program öğelerinin adlandırılmasıyla ilgili standartları içerir. Tüm yeni kodlar bu standartlara göre yazılmalı ve mevcut kodlar da zaman içinde bu standartlara uygun hale getirilmelidir.

## Genel İlkeler

1. **Dil Seçimi**: Projede Türkçe isimlendirme kullanılacaktır. İş mantığını ilgilendiren tüm sınıf, metot ve değişken isimleri Türkçe olmalıdır.
   - İstisna: Framework sınıfları ve teknik terimlerin Türkçe karşılığı yaygın kullanılmıyorsa İngilizce kalabilir (örn: Repository, Service, Controller, vb.)

2. **Tutarlılık**: Tüm kodlar bu dokümandaki isimlendirme standartlarına uygun olmalıdır.

3. **Anlamlı İsimler**: İsimler kısa ve öz, ancak yeterince açıklayıcı olmalıdır. Tek harfli değişken isimleri sadece döngü değişkenleri için kullanılabilir.

## Sınıf İsimlendirme

1. **PascalCase**: Sınıf isimleri PascalCase olarak yazılmalıdır (her kelimenin ilk harfi büyük).
   - Örnek: `FaturaService`, `StokHareket`, `CariRepository`

2. **Sonekler**: Sınıflarda rolünü belirten sonekler kullanılmalıdır:
   - Controller sınıfları: `FaturaController`
   - Service sınıfları: `FaturaService`
   - Repository sınıfları: `FaturaRepository`
   - Model/Entity sınıfları: Sonek yok, direkt isim: `Fatura`, `Stok`, `Cari`
   - ViewModel sınıfları: `FaturaViewModel`, `FaturaDetayViewModel`
   - Interface'ler: "I" ön eki + sınıf adı: `IFaturaService`, `IRepository`
   - Exception sınıfları: `FaturaException`, `StokYetersizException`

## Metot İsimlendirme

1. **PascalCase**: Metot isimleri PascalCase olarak yazılmalıdır.
   - Örnek: `FaturaOlustur`, `StokCikisiYap`, `CariHesaplar`

2. **Fiil Kullanımı**: Metot isimleri bir eylem ifade etmeli, genellikle bir fiil ile başlamalıdır.
   - Örnek: `Kaydet`, `Getir`, `Hesapla`, `Ekle`, `Sil`, `Guncelle`

3. **Async Metotlar**: Asenkron metotlar "Async" son eki almalıdır.
   - Örnek: `GetirAsync`, `KaydetAsync`, `HesaplaAsync`

4. **Boolean Dönüş Değeri**: Boolean değer döndüren metotlar genellikle "Is", "Var" veya "Mı" gibi ön ekler/son ekler almalıdır.
   - Örnek: `IsValid` (İngilizce istisna), `GecerliMi`, `StokVar`, `KullaniliyorMu`

## Değişken İsimlendirme

1. **camelCase**: Yerel değişkenler, parametreler ve private field'lar camelCase olarak yazılmalıdır (ilk harf küçük, sonraki kelimelerin ilk harfi büyük).
   - Örnek: `faturaId`, `stokMiktari`, `cariUnvani`

2. **Özel Alanlar (Private Fields)**: Private alanlar alt çizgi (_) ile başlamalıdır.
   - Örnek: `_faturaRepository`, `_logger`, `_dbContext`

3. **Sabitler**: Sabitler UPPER_CASE olarak yazılmalı ve kelimeleri alt çizgi (_) ile ayrılmalıdır.
   - Örnek: `MAX_STOK_MIKTARI`, `DOVIZ_KURU_API_URL`

4. **Tip Adları**: Tür adları mantıklı olmalı ve gerektiğinde sonek olarak içerdiği veri tipini belirtmelidir.
   - Örnek: `birimFiyat` (decimal), `olusturmaTarihi` (DateTime), `stokListesi` (List<Stok>)

## Property İsimlendirme

1. **PascalCase**: Property isimleri PascalCase olarak yazılmalıdır.
   - Örnek: `FaturaId`, `ToplamTutar`, `OdemeDurumu`

2. **Boolean Property'ler**: Boolean property'ler anlamlı isimler almalı, tercihen "Is", "Has" veya Türkçe'de "Mi", "Var", "Edildi" gibi ön/son ekler içermelidir.
   - Örnek: `Odendi`, `SilindiBilgisi`, `AktifMi`, `StokKritikMi`

## Enum İsimlendirme

1. **PascalCase**: Enum tipleri ve değerleri PascalCase olarak yazılmalıdır.
   - Örnek: `FaturaTuru`, `OdemeDurumu`, `StokHareketTipi`

2. **Tekil Kullanım**: Enum tipleri tekil olmalıdır.
   - Örnek: `FaturaTuru` (çoğul değil)

## Dosya İsimlendirme

1. **Sınıf Adıyla Eşleşme**: Dosya adları içerdikleri ana sınıfın adıyla aynı olmalıdır.
   - Örnek: `FaturaService.cs`, `Cari.cs`, `StokRepository.cs`

2. **Interface Dosyaları**: Interface dosyaları "I" ön ekiyle başlamalıdır.
   - Örnek: `IFaturaService.cs`, `IRepository.cs`

## Namespace İsimlendirme

1. **Hiyerarşi**: Namespace'ler proje yapısını yansıtmalıdır.
   - Örnek: `MuhasebeStokWebApp.Controllers`, `MuhasebeStokWebApp.Services`, `MuhasebeStokWebApp.Data.Entities`

2. **PascalCase**: Namespace bileşenleri PascalCase olarak yazılmalıdır.

## Türkçe Karakter Kullanımı

1. **Türkçe Karakter Kullanımı**: Türkçe karakter içeren isimler (ö, ü, ğ, ş, ç, ı, İ) kod içinde kullanılmamalıdır. Bunun yerine:
   - ö yerine o
   - ü yerine u
   - ğ yerine g
   - ş yerine s
   - ç yerine c
   - ı yerine i
   - İ yerine I

   Örnek: `OdemeIslemleri` (Ödeme İşlemleri yerine), `Kullanici` (Kullanıcı yerine)

## Yorum Standardı

1. **Yorum Dili**: Yorumlar Türkçe olmalıdır.

2. **XML Yorumları**: Public metotlar ve sınıflar için XML doküman yorumları kullanılmalıdır.

3. **Satır İçi Yorumlar**: Karmaşık kod blokları için satır içi yorumlar eklenmelidir.

## Kısaltmalar

1. **Kısaltmalar**: Kaçınılmaz olmadıkça kısaltma kullanmayın. Kullanılması gerekiyorsa:
   - İki harfli kısaltmalar tamamen büyük harfle: `ID`, `IO`
   - Üç veya daha fazla harfli kısaltmalar PascalCase: `Xml`, `Http`, `Api`

## Yasaklı İsimler

1. **Yasaklı İsimler**: Şu isimler kullanılmamalıdır:
   - C# anahtar kelimeleri
   - Çok genel isimler: `Data`, `Info`, `Manager`, `Utility` vb. (bunun yerine `FaturaData`, `CariInfo`, `StokManager` gibi spesifik isimler kullanın)
   - Anlamsız isimler: `Foo`, `Bar`, `x1`, `y2` vb.

## Karışık Dil Kullanım Rehberi

Projenin genelinde Türkçe isimlendirme kullanılsa da bazı durumlarda teknik terimler ve framework sınıfları için İngilizce kullanılabilir. Bu durumda aşağıdaki rehber izlenmelidir:

1. **Karışık Kullanım**: Bir isim içinde hem Türkçe hem İngilizce olmamalıdır.
   - Doğru: `FaturaController`, `UrunRepository`
   - Yanlış: `InvoiceService`, `ProductRepository`

2. **Framework Terimleri**: ASP.NET Core, Entity Framework gibi framework'e özgü terimlerde İngilizce kullanılabilir.
   - Örnek: `controller`, `middleware`, `repository`, `service`, `factory`

3. **Programlama Konseptleri**: Programlama dillerine özgü yaygın teknik terimler İngilizce kalabilir.
   - Örnek: `async`, `await`, `interface`, `abstract`

## Örnek İsimlendirmeler

### Sınıf Örnekleri

```csharp
// Entity
public class Fatura { }

// ViewModel
public class FaturaDetayViewModel { }

// Controller
public class FaturaController : Controller { }

// Service
public class FaturaService : IFaturaService { }

// Repository
public class FaturaRepository : IFaturaRepository { }

// Interface
public interface IFaturaService { }

// Enum
public enum FaturaDurumu { Taslak, Onaylandi, Iptal }
```

### Metot Örnekleri

```csharp
// Servis metotları
public async Task<Fatura> GetirAsync(Guid id);
public async Task<Guid> KaydetAsync(Fatura fatura);
public async Task<bool> SilAsync(Guid id);

// Boolean dönüş değeri olan metotlar
public bool GecerliMi();
public bool StokYeterliMi(Guid urunId, decimal miktar);

// Hesaplama metotları
public decimal ToplamTutarHesapla();
public decimal KdvHesapla(decimal tutar, decimal oran);
```

### Değişken Örnekleri

```csharp
// Private alanlar
private readonly IFaturaRepository _faturaRepository;
private readonly ILogger<FaturaService> _logger;

// Yerel değişkenler
var faturaId = Guid.NewGuid();
decimal birimFiyat = 100.50m;
bool odemeYapildi = true;

// Parametreler
public void FaturaOlustur(string faturaNo, decimal tutar, Guid musteriId)
```

Bu standartlar, MuhasebeStokWebApp projesindeki tüm kod dosyalarında uygulanmalıdır. Mevcut kodlar zaman içinde bu standartlara göre düzeltilmeli ve tüm yeni kodlar bu standartlara uygun yazılmalıdır. 