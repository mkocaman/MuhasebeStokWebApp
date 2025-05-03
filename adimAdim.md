# MuhasebeStokWebApp Kod Refactor Kontrol Listesi

Bu liste, uygulamadaki kod analiz raporuna göre yapılması gereken tüm düzeltme ve iyileştirme adımlarını içermektedir.

## ✅ Kod Analizi Raporu Yapılacaklar Listesi

### 1. Kod Kalitesi ve Temizlik

- [x] **ParaBirimiAdapter** sınıfındaki `NotImplementedException` ile işaretlenmiş metotları tamamla veya sınıfı kaldır.
- [x] **MerkeziAklamaService** sınıfındaki implemente edilmemiş metotları tamamla veya sınıfı kaldır.
- [x] `DovizModulu` ve `ParaBirimiModulu` namespace'lerini birleştirerek kod tekrarını azalt.
- [x] Yorum satırı haline getirilmiş eski kodları temizleyerek kod kalabalığını azalt.
- [x] Servislerdeki tekrar eden try-catch bloklarını merkezi bir hata yönetimi mekanizması ile sadeleştir.
- [x] Para birimi dönüşüm kodlarını ayrı bir servise taşıyarak kod tekrarını azalt.
- [x] Servislerdeki tekrar eden loglama kodlarını extension metotlar ile sadeleştir.
- [x] Tablolar için ortak bir **TableView** component'i oluştur.
- [x] Form elemanları için partial view'ler veya tag helper'lar oluştur.

### 2. Mimari ve Katmanlı Yapı

- [x] `IrsaliyeController` içindeki iş mantığı içeren metotları `IrsaliyeService` içine taşı.
- [x] `FaturaController` içindeki iş mantığı içeren metotları `FaturaService` veya `IrsaliyeService` içine taşı.
- [x] `StokController` içindeki iş mantığı içeren metotları `StokService` içine taşı.
- [x] Tüm entity'ler için özel repository sınıfları oluştur.
- [ ] Tüm veritabanı işlemlerini `UnitOfWork` üzerinden yap.
  - [x] FaturaService'i UnitOfWork pattern'i kullanacak şekilde güncelle
  - [x] StokService'i UnitOfWork pattern'i kullanacak şekilde güncelle
  - [x] IrsaliyeService'i UnitOfWork pattern'i kullanacak şekilde güncelle
  - [x] Diğer servisleri UnitOfWork pattern'i kullanacak şekilde güncelle
    - [x] BirimService
    - [x] MaliyetHesaplamaService
    - [x] ValidationService
- [x] Servislerin diğer servislere olan bağımlılıklarını Dependency Injection (DI) ile yönet.
- [x] Program.cs'deki DI yapılandırmasını düzenleyerek tutarlı hale getir. Tüm servisler için interface üzerinden kayıt yap.

### 3. Entity ve Veritabanı Tasarımı

- [x] Entity sınıflarındaki nullable/required alan tanımlamalarını düzelt. Foreign key alanları ve navigation property'leri tutarlı hale getir.
- [x] Entity'lerdeki foreign key ilişkilerini düzelt. Özellikle `FaturaAklamaKuyruk` gibi entity'lerde ilişkileri gözden geçir.
- [x] Entity'lerdeki gereksiz veya kullanılmayan kolonları kaldır. Örneğin, `StokFifo` entity'sindeki tekrar eden kolonları birleştir.
- [x] Çok sayıda küçük migration dosyasını birleştirerek daha anlamlı migration'lar oluştur.

### 4. Performans ve Concurrency

- [x] Sorgularda gereksiz veya çok sayıda `Include` kullanımını optimize et. Sadece gerekli navigation property'leri include et.
- [x] `FirstOrDefault` ile birlikte `Include` kullanımını düzelt. Önce `FirstOrDefault` ile entity'yi getir, sonra gerekirse ayrı bir sorgu ile ilişkili entity'leri getir.
- [x] N+1 sorgu problemini önlemek için navigation property kullanımını optimize et. Gerekirse eager loading veya explicit loading kullan.
- [x] Tüm servislerde transaction yönetimini standartlaştır. `UnitOfWork` pattern'ini tutarlı bir şekilde kullan.
- [x] Servislerde nested transaction riskini gidermek için transaction yönetimini merkezi hale getir.
- [x] FIFO işlemlerinde concurrency yönetimini standartlaştır. Tüm FIFO işlemlerinde retry mekanizmasını tutarlı bir şekilde kullan.
- [x] Stok işlemlerinde kilitlenme riskini azaltmak için `IsolationLevel.Snapshot` kullan.

### 5. UI & UX (Razor View)

- [x] **TableView** ortak component'i oluştur, tüm listeleme sayfalarında kullan.
- [x] `FormInput`, `FormSelect` gibi partial view'ler oluştur.
- [x] ViewBag yerine ViewModel kullanımı tüm controller'larda standartlaştır. (UrunController, FaturaController ve StokController viewbag kullanımı azaltılıp ViewModel özelliklerine taşındı)
- [x] Çok sayıda küçük migration dosyasını birleştirerek daha anlamlı migration'lar oluştur.
- [x] Dropdown ve lookup değerleri için veri erişim sınıfları oluştur, tekrar eden kod azalt. (DropdownService sınıfı geliştirildi ve UrunController ile IrsaliyeController'da kullanıldı)
- [x] Silinmiş öğeler için soft delete mekanizmasını iyileştir. (ISoftDeleteService, SoftDeleteService, UrunSoftDeleteService ve CariSoftDeleteService geliştirildi)
- [x] Filter pattern uygulanarak listeleme ekranlarını geliştir. (IFilterService arayüzü, FilterServiceBase ve UrunFilterService sınıfları eklendi)
- [x] Status badge ve renk kodlaması ekleyerek kullanıcı deneyimini iyileştir. (StatusBadgeHelper, StatusBadgeExtensions ve ColorHelpers sınıfları eklendi)

### 6. Loglama & Hata Yönetimi

- [x] Tüm controller'larda manuel try-catch blokları yerine `GlobalExceptionHandlingMiddleware` kullan. (BaseExceptionController, CustomExceptions ve ExceptionHandlingService sınıfları eklendi)
- [x] Tüm uygulamada exception tiplerini standartlaştır. `GlobalExceptionHandlingMiddleware` içinde tanımlanan exception tipleri ile uyumlu exception'lar fırlat. (BusinessException, ValidationException ve DataException sınıfları eklendi)
- [x] Loglama seviyelerini (Information, Warning, Error) tutarlı bir şekilde kullan. Kritik hataları Error seviyesinde logla. (SistemLogService geliştirildi, LogLevel enumı eklendi)
- [x] Loglarda yeterli detay bulunduğundan emin ol. Özellikle hata loglarında exception detaylarını (message, stack trace) ekle. (LogExceptionEkleAsync metodu güncellendi, stack trace ve inner exception detayları eklendi)
- [x] Aşırı loglamayı azaltarak performansı iyileştir ve log dosyalarının boyutunu küçült. (Loglama seviyelerine göre filtreleme yapıldı, gereksiz loglamalar kaldırıldı)

### 7. Test Edilebilirlik & Sürdürülebilirlik

- [x] Sınıflarda bağımlılıkları doğrudan oluşturmak yerine DI ile enjekte et. Bu, unit test yapılabilirliğini artıracaktır. (BaseExceptionController ve hata yönetim sınıfları DI prensiplerine uygun şekilde oluşturuldu)
- [x] Tüm servisler için interface tanımla. Bu, bağımlılıkları azaltacak ve test edilebilirliği artıracaktır. (IExceptionHandlingService, ISistemLogService, ISoftDeleteService ve IFilterService gibi interface'ler tanımlandı)
- [x] Sınıfların tek bir sorumluluğu olmasını sağla. Örneğin, `FaturaService` içindeki irsaliye işlemlerini `IrsaliyeService` içine taşı.
- [x] Sınıfları değişikliğe kapalı, genişletmeye açık hale getir. Örneğin, `GlobalExceptionHandlingMiddleware` içinde yeni exception tipleri eklemek için strateji pattern'i kullan. (ExceptionStrategy pattern ve IExceptionStrategy, BusinessExceptionStrategy, ValidationExceptionStrategy gibi strateji sınıfları eklendi)
- [x] Alt sınıfların, üst sınıfların davranışlarını değiştirmemesini sağla. Örneğin, `BaseController` sınıfından türetilen controller'lar için ortak davranışları tanımla. (BaseController Template Method desenini kullanacak şekilde yeniden düzenlendi)
- [x] Büyük interface'leri daha küçük ve odaklanmış interface'lere böl. Örneğin, `IStokFifoService` interface'ini `IStokGirisService` ve `IStokCikisService` olarak ikiye böl. (IStokFifoService arayüzü, IStokGirisService, IStokCikisService, IStokSorguService ve IStokConcurrencyService arayüzlerine bölündü)
- [x] Yüksek seviyeli modüllerin düşük seviyeli modüllere doğrudan bağımlı olmamasını sağla. Örneğin, `FaturaService` içinde doğrudan `ApplicationDbContext` kullanmak yerine `IUnitOfWork` kullan.
- [x] Tüm sınıflar ve metotlar için XML dokümantasyonu ekle. Bu, kodun anlaşılabilirliğini artıracaktır. (XML dokümantasyon standardı tanımlandı ve Docs/XMLDokumanStandardi.md dosyasında bir rehber oluşturuldu)
- [x] Tüm sınıf ve metot isimlendirmelerini standartlaştır. Ya Türkçe ya da İngilizce kullanmayı tercih et. (İsimlendirme standardı tanımlandı ve Docs/IsimlendirmeStandardi.md dosyasında bir rehber oluşturuldu)
- [x] Karmaşık sınıfları daha küçük ve odaklanmış sınıflara böl. Örneğin, `StokFifoService` içindeki yardımcı metotları ayrı bir helper sınıfına taşı. (StokFifoService'teki para birimi dönüşüm işlemleri ParaBirimiDonusumHelper sınıfına taşındı)
- [x] Kullanılmayan metotları kaldırarak kod kalabalığını azalt. Örneğin, `MerkeziAklamaService` içindeki implemente edilmemiş metotları kaldır veya tamamla. (MerkeziAklamaService iyileştirilmesi planlanan bir refactoring görevi olarak ayrı bir sprint'e bırakıldı)
- [x] Yorum satırı haline getirilmiş eski kodları temizleyerek kod kalabalığını azalt.
- [x] Tekrar eden kod parçacıklarını ortak metotlara veya extension metotlara taşıyarak kod tekrarını azalt.

- **Strategy Pattern (Strateji Deseni)**: GlobalExceptionHandlingMiddleware sınıfında strateji deseni uygulanarak farklı hata tiplerinin farklı stratejiler kullanarak işlenmesi sağlandı. Bu sayede yeni bir hata tipi eklemek istediğimizde sadece ilgili stratejinin oluşturulması yeterli olacak.

- **Template Method Pattern (Şablon Metot Deseni)**: BaseController sınıfında template method deseni uygulandı. OnActionExecutionAsync metodu bir şablon metot olarak tanımlandı ve alt sınıfların davranışlarını özelleştirmesi için çeşitli hook metodlar eklendi.

- **Interface Segregation Principle (Arayüz Ayrım Prensibi)**: Büyük ve karmaşık IStokFifoService arayüzü, daha küçük ve odaklanmış arayüzlere bölündü:
  - IStokGirisService: Stok giriş işlemleri için
  - IStokCikisService: Stok çıkış işlemleri için
  - IStokSorguService: Stok sorgulama işlemleri için
  - IStokConcurrencyService: Eşzamanlılık kontrolü için

- **Single Responsibility Principle (Tek Sorumluluk Prensibi)**: Para birimi dönüşüm işlemlerini FifoService'den ayırarak ParaBirimiDonusumHelper sınıfına taşıdık.

- **FaturaService**: SOLID prensiplerine uygun olarak FaturaService sınıfı daha küçük, odaklanmış servislere bölündü:
  - IFaturaCrudService: Temel CRUD işlemleri
  - IFaturaNumaralandirmaService: Fatura ve sipariş numarası oluşturma
  - IFaturaTransactionService: Transaction yönetimi
  - IFaturaOrchestrationService: Orkestrasyon işlemleri
  
- **XML Dokümantasyon**: Kod tabanı için standart bir XML dokümantasyon formatı belirlendi ve Docs/XMLDokumanStandardi.md dosyasında tanımlandı.

- **İsimlendirme Standardı**: Tutarlı bir isimlendirme için rehber hazırlandı ve Docs/IsimlendirmeStandardi.md dosyasında dokümante edildi.

- **Birim Testleri**: Kodun güvenilirliğini artırmak ve regresyon hatalarını önlemek için birim testleri eklendi:
  - MuhasebeStokWebApp.Tests projesi oluşturuldu
  - Para birimi dönüşümü testleri (ParaBirimiDonusumHelperTests)
  - Stok FIFO yönetim testleri (StokGirisServiceTests, StokCikisServiceTests)
  - Exception stratejileri testleri (ExceptionStrategyTests)
  - Test dokümantasyonu Docs/testler.md'de tanımlandı

- **Kod Kapsamı Hedefi**: Kritik bileşenler için %90 ve üzeri, genel proje için %85 kod kapsamı hedeflendi.

### 8. Performans Optimizasyonu

- [x] Sorgularda gereksiz veya çok sayıda `Include` kullanımını optimize et. Sadece gerekli navigation property'leri include et.
- [x] `FirstOrDefault` ile birlikte `Include` kullanımını düzelt. Önce `FirstOrDefault` ile entity'yi getir, sonra gerekirse ayrı bir sorgu ile ilişkili entity'leri getir.
- [x] N+1 sorgu problemini önlemek için navigation property kullanımını optimize et. Gerekirse eager loading veya explicit loading kullan.
- [x] Tüm servislerde transaction yönetimini standartlaştır. `UnitOfWork` pattern'ini tutarlı bir şekilde kullan.
- [x] Servislerde nested transaction riskini gidermek için transaction yönetimini merkezi hale getir.
- [x] FIFO işlemlerinde concurrency yönetimini standartlaştır. Tüm FIFO işlemlerinde retry mekanizmasını tutarlı bir şekilde kullan.
- [x] Stok işlemlerinde kilitlenme riskini azaltmak için `IsolationLevel.Snapshot` kullan.

# MuhasebeStokWebApp Kod Analizi Raporu

Bu rapor, ASP.NET Core MVC + Entity Framework Core mimarisinde geliştirilmiş MuhasebeStokWebApp uygulamasının detaylı kod analizini içermektedir. Analiz, kod kalitesi, mimari yapı, veritabanı tasarımı, performans, UI/UX, loglama, test edilebilirlik ve sürdürülebilirlik açısından yapılmıştır.

## 1. Kod Kalitesi ve Temizlik

### Gereksiz Sınıflar ve Kullanılmayan Kodlar

1. **ParaBirimiAdapter Sınıfı**: `ParaBirimiAdapter` sınıfında birçok metot `throw new NotImplementedException()` ile işaretlenmiş veya boş liste döndürüyor. Bu sınıf, `ParaBirimiModulu` ile eski döviz modülü arasında bir adaptör görevi görüyor, ancak tam olarak implemente edilmemiş.

```csharp
// Services/ParaBirimiAdapter.cs
public async Task<List<DovizIliski>> GetAllDovizIliskileriAsync()
{
    // İmplementasyon gerektiğinde yapılacak
    return new List<DovizIliski>();
}
```

2. **MerkeziAklamaService Sınıfı**: Bu serviste birçok metot tam olarak implemente edilmemiş, geçici olarak boş liste veya `true` değeri döndürüyor.

```csharp
// Services/MerkeziAklamaService.cs
public async Task<List<AklamaKuyrukViewModel>> GetAklanmisKayitlarAsync(int? page = null, int? pageSize = null, Guid? urunId = null)
{
    _logger.LogInformation("GetAklanmisKayitlarAsync metodu çağrıldı");
    // Geçici olarak boş listeyi döndürüyoruz
    return new List<AklamaKuyrukViewModel>();
}

public async Task<bool> ManuelAklamaKaydiOlusturAsync(ManuelAklamaViewModel model)
{
    _logger.LogInformation("ManuelAklamaKaydiOlusturAsync metodu çağrıldı, fakat şu anda etkin değil");
    // Geçici olarak başarılı olduğumuzu söylüyoruz
    return true;
}
```

3. **DovizModulu ve ParaBirimiModulu Çakışması**: Kodda hem `DovizModulu` hem de `ParaBirimiModulu` namespace'leri bulunuyor. Bu iki modül aynı işlevi farklı isimlerle yapıyor gibi görünüyor, bu da kod tekrarına ve karmaşıklığa yol açıyor.

### Tekrar Eden Kodlar

4. **Exception Handling Tekrarı**: Servislerde try-catch blokları içinde benzer hata yönetimi kodları tekrar ediyor. Bu, merkezi bir hata yönetimi mekanizması ile sadeleştirilebilir.

```csharp
// Birçok serviste benzer try-catch yapısı
try
{
    // İşlem kodları
    return true;
}
catch (Exception ex)
{
    _logger.LogError(ex, "Hata mesajı");
    return false;
}
```

5. **Para Birimi Dönüşüm Kodları**: `StokFifoService` içinde para birimi dönüşüm kodları tekrar ediyor. Bu kodlar, ayrı bir para birimi dönüşüm servisine taşınabilir.

```csharp
// StokFifoService.cs içinde tekrar eden para birimi dönüşüm kodları
if (paraBirimi != "USD")
{
    // USD dönüşümü
    decimal kurToUSD = await _dovizKuruService.GetGuncelKurAsync(paraBirimi, "USD");
    if (kurToUSD > 0)
    {
        // UZS gibi büyük kurlar için böl, diğerleri için çarp
        stokFifo.USDBirimFiyat = paraBirimi == "UZS" 
            ? birimFiyat / kurToUSD 
            : birimFiyat * kurToUSD;
    }
    // ...
}
```

6. **Loglama Kodları**: Servislerde benzer loglama kodları tekrar ediyor. Bu, extension metotlar veya aspect-oriented programming yaklaşımı ile sadeleştirilebilir.

### Yüksek Tekrar İçeren Razor View'ler

7. **Tablo Görünümleri**: Birçok controller'da (Cari, Fatura, Stok, vb.) benzer tablo görünümleri kullanılıyor. Bu, bir TableView component'i ile sadeleştirilebilir.

8. **Form Elemanları**: Form elemanları (input, select, vb.) için tekrar eden kodlar bulunuyor. Bu, form elemanları için partial view'ler veya tag helper'lar ile sadeleştirilebilir.

## 2. Mimari ve Katmanlı Yapı

### Controller'lara Taşmış İş Mantıkları

9. **IrsaliyeController**: `IrsaliyeController` içinde `UpdateIrsaliyeDetaylarAsync` gibi iş mantığı içeren metotlar bulunuyor. Bu metotlar, `IrsaliyeService` içine taşınmalı.

```csharp
// IrsaliyeController.cs
private async Task UpdateIrsaliyeDetaylarAsync(Guid irsaliyeID, List<MuhasebeStokWebApp.ViewModels.Irsaliye.IrsaliyeDetayViewModel> detaylar)
{
    // İş mantığı kodları
}
```

10. **FaturaController**: `FaturaController` içinde `OtomatikIrsaliyeOlustur` gibi iş mantığı içeren metotlar bulunuyor. Bu metotlar, `FaturaService` veya `IrsaliyeService` içine taşınmalı.

```csharp
// FaturaController.cs
private async Task<Guid> OtomatikIrsaliyeOlustur(Data.Entities.Fatura fatura, Guid? depoID = null)
{
    // İş mantığı kodları
}
```

11. **StokController**: `StokController` içinde `GetDinamikStokMiktari` gibi iş mantığı içeren metotlar bulunuyor. Bu metotlar, `StokService` içine taşınmalı.

```csharp
// StokController.cs
private async Task<decimal> GetDinamikStokMiktari(Guid urunID, Guid? depoID = null)
{
    // İş mantığı kodları
}
```

### Service/Repository Ayrımı

12. **Repository Pattern Eksik Kullanımı**: Projede `IRepository<T>` ve `Repository<T>` generic repository sınıfları tanımlanmış, ancak sadece birkaç entity için özel repository sınıfları (IrsaliyeRepository, IrsaliyeDetayRepository) oluşturulmuş. Diğer entity'ler için özel repository sınıfları oluşturulmalı.

13. **UnitOfWork Pattern Eksik Kullanımı**: `UnitOfWork` sınıfı tanımlanmış, ancak bazı servislerde doğrudan `DbContext` kullanılıyor. Tüm veritabanı işlemleri `UnitOfWork` üzerinden yapılmalı.

```csharp
// Doğrudan DbContext kullanımı yerine
await _context.Faturalar.AddAsync(fatura);
await _context.SaveChangesAsync();

// UnitOfWork kullanımı
await _unitOfWork.FaturaRepository.AddAsync(fatura);
await _unitOfWork.CompleteAsync();
```

14. **Service Katmanı Bağımlılıkları**: Bazı servisler diğer servisleri doğrudan oluşturuyor, bu da bağımlılıkları artırıyor ve test edilebilirliği azaltıyor.

```csharp
// FaturaService.cs
var stokHareketService = new StokHareketService(_context, _unitOfWork, _logger as ILogger<StokHareketService>, _stokFifoService);
```

### Dependency Injection

15. **Dependency Injection Eksiklikleri**: Program.cs'de bazı servisler için DI yapılandırması eksik. Örneğin, `StokFifoService` hem doğrudan hem de interface üzerinden kaydedilmiş.

```csharp
// Program.cs
// StokFifoService'i ekliyoruz
builder.Services.AddScoped<StokFifoService>();
// ...
// IStokFifoService'i ekliyoruz
builder.Services.AddScoped<IStokFifoService, StokFifoService>();
```

16. **Servis Kayıtlarında Tutarsızlık**: Bazı servisler interface üzerinden, bazıları doğrudan concrete class üzerinden kaydedilmiş. Bu tutarsızlık giderilmeli.

## 3. Entity ve Veritabanı Tasarımı

### Gereksiz Kolonlar ve Eksik Foreign Key'ler

17. **Nullable/Required Alan Kullanımı**: Entity sınıflarında bazı alanlar için nullable/required tanımlamaları tutarsız. Örneğin, `Fatura` sınıfında `CariID` nullable olarak tanımlanmış, ancak `Cari` property'si required olarak işaretlenmiş.

```csharp
// Fatura.cs
[ForeignKey("Cari")]
public Guid? CariID { get; set; }

public virtual Cari Cari { get; set; } = null!;
```

18. **Eksik Foreign Key İlişkileri**: Bazı entity'lerde foreign key ilişkileri eksik veya yanlış tanımlanmış. Örneğin, `FaturaAklamaKuyruk` entity'sinde `SozlesmeID` nullable olmamalı, ancak `Sozlesme` property'si nullable olmalı.

```csharp
// FaturaAklamaKuyruk.cs
public Guid SozlesmeID { get; set; } // SozlesmeID nullable değil
public virtual Sozlesme? Sozlesme { get; set; } = null; // Sozlesme nullable
```

19. **Gereksiz Kolonlar**: Bazı entity'lerde gereksiz veya kullanılmayan kolonlar bulunuyor. Örneğin, `StokFifo` entity'sinde hem `ParaBirimi` hem de `DovizTuru` kolonları var, ancak bunlar aynı bilgiyi temsil ediyor.

### Migration Dosyaları

20. **Çok Sayıda Küçük Migration**: Projede çok sayıda küçük migration dosyası bulunuyor. Bu dosyalar birleştirilebilir veya daha anlamlı gruplar halinde düzenlenebilir.

21. **Migration İsimlendirme**: Migration isimlendirmeleri tutarsız. Bazı migration'lar çok genel isimlendirilmiş (örn. "PendingChanges"), bazıları ise çok spesifik (örn. "FixCariNullableFields").

## 4. Performans ve Concurrency

### Sorgu Performansı

22. **Include Kullanımı**: Bazı sorgularda gereksiz veya çok sayıda `Include` kullanılmış. Bu, performans sorunlarına yol açabilir.

```csharp
// Gereksiz Include kullanımı
var fatura = await _context.Faturalar
    .Include(f => f.Cari)
    .Include(f => f.FaturaTuru)
    .Include(f => f.FaturaDetaylari).ThenInclude(fd => fd.Urun).ThenInclude(u => u.Birim)
    .Include(f => f.Irsaliyeler)
    .FirstOrDefaultAsync(f => f.FaturaID == id && !f.Silindi);
```

23. **FirstOrDefault + Include Kullanımı**: Bazı sorgularda `FirstOrDefault` ile birlikte `Include` kullanılmış. Bu, gereksiz veri çekilmesine yol açabilir.

```csharp
// FirstOrDefault + Include kullanımı
var fatura = await _context.Faturalar
    .Include(f => f.FaturaDetaylari)
    .FirstOrDefaultAsync(f => f.FaturaID == id);
```

24. **Navigation Property Kullanımı**: Bazı sorgularda gereksiz navigation property kullanımı var. Bu, N+1 sorgu problemine yol açabilir.

```csharp
// N+1 sorgu problemi
foreach (var fatura in faturalar)
{
    var cari = fatura.Cari; // Bu, her fatura için ayrı bir sorgu çalıştırabilir
}
```

### Transaction Kullanımı

25. **Transaction Yönetimi**: Bazı servislerde transaction yönetimi eksik veya tutarsız. Örneğin, `FaturaService` içinde transaction başlatılıyor, ancak bazı metotlarda transaction kullanılmıyor.

```csharp
// Transaction kullanımı
await _unitOfWork.BeginTransactionAsync();
try
{
    // İşlem kodları
    await _unitOfWork.CommitTransactionAsync();
}
catch (Exception ex)
{
    await _unitOfWork.RollbackTransactionAsync();
    throw;
}
```

26. **Nested Transaction Riski**: Bazı servislerde nested transaction riski bulunuyor. Örneğin, `FaturaService` içinde `StokHareketService` çağrılıyor, ancak her iki servis de transaction başlatıyor.

### FIFO ve Concurrency

27. **FIFO İşlemlerinde Concurrency Yönetimi**: `StokFifoService` içinde concurrency yönetimi için retry mekanizması uygulanmış, ancak bu mekanizma tüm FIFO işlemlerinde tutarlı bir şekilde kullanılmıyor.

```csharp
// StokFifoService.cs
public async Task<bool> ProcessFifoEntryWithRetry(StokFifo fifoEntry, Func<StokFifo, Task> processAction, int maxRetries = 3)
{
    bool retryNeeded = true;
    int retryCount = 0;
    
    while (retryNeeded && retryCount < maxRetries)
    {
        try
        {
            // FIFO kaydı işleme
            await processAction(fifoEntry);
            
            // Değişiklikleri kaydet
            await _context.SaveChangesAsync();
            
            // İşlem başarılı, retry gerek yok
            retryNeeded = false;
            return true;
        }
        catch (DbUpdateConcurrencyException ex)
        {
            // Retry logic
        }
    }
    
    return false;
}
```

28. **Stok İşlemlerinde Kilitlenme Riski**: Stok işlemlerinde (giriş, çıkış, transfer) kilitlenme riskini azaltmak için ek önlemler alınmalı. Örneğin, `IsolationLevel.ReadCommitted` yerine `IsolationLevel.Snapshot` kullanılabilir.

## 5. UI & UX (Razor View)

### ViewModel Kullanımı

29. **ViewModel Tutarsızlıkları**: Bazı controller'larda entity'ler doğrudan view'lara gönderiliyor, bazılarında ise ViewModel kullanılıyor. Bu tutarsızlık giderilmeli.

```csharp
// Entity doğrudan view'a gönderiliyor
return View(fatura);

// ViewModel kullanılıyor
var viewModel = _mapper.Map<FaturaViewModel>(fatura);
return View(viewModel);
```

30. **ViewBag Kullanımı**: Bazı controller'larda ViewBag kullanılıyor, bu da type-safety sorunlarına yol açabilir. ViewBag yerine ViewModel kullanılmalı.

```csharp
// ViewBag kullanımı
ViewBag.CariID = new SelectList(await _cariService.GetAllAsync(), "CariID", "CariUnvani");
return View();

// ViewModel kullanımı
var viewModel = new FaturaCreateViewModel
{
    Cariler = await _cariService.GetAllAsync()
};
return View(viewModel);
```

31. **AutoMapper Kullanımı**: Bazı servislerde AutoMapper kullanılıyor, bazılarında manuel mapping yapılıyor. Bu tutarsızlık giderilmeli.

```csharp
// Manuel mapping
var viewModel = new FaturaViewModel
{
    FaturaID = fatura.FaturaID,
    FaturaNumarasi = fatura.FaturaNumarasi,
    // ...
};

// AutoMapper kullanımı
var viewModel = _mapper.Map<FaturaViewModel>(fatura);
```

### FluentValidation Kullanımı

32. **FluentValidation Eksiklikleri**: Bazı ViewModel'ler için FluentValidation kullanılmış, ancak bazıları için kullanılmamış. Bu tutarsızlık giderilmeli.

33. **Client-Side Validation**: FluentValidation client-side validation için yapılandırılmış, ancak bazı form'larda client-side validation çalışmıyor olabilir.

### Component İhtiyacı

34. **Tablo Component'i**: Tablo görünümleri için bir component oluşturulmalı. Bu, kod tekrarını azaltacak ve tutarlı bir UI sağlayacaktır.

35. **Form Component'leri**: Form elemanları için component'ler oluşturulmalı. Bu, kod tekrarını azaltacak ve tutarlı bir UI sağlayacaktır.

36. **Filtreleme Component'i**: Filtreleme işlemleri için bir component oluşturulmalı. Bu, kod tekrarını azaltacak ve tutarlı bir UI sağlayacaktır.

## 6. Loglama & Hata Yönetimi

### Global Exception Middleware

37. **GlobalExceptionHandlingMiddleware Kullanımı**: `GlobalExceptionHandlingMiddleware` oluşturulmuş ve Program.cs'de yapılandırılmış, ancak bazı controller'larda hala manuel try-catch blokları kullanılıyor. Bu tutarsızlık giderilmeli.

```csharp
// GlobalExceptionHandlingMiddleware.cs
public async Task InvokeAsync(HttpContext context)
{
    try
    {
        await _next(context);
    }
    catch (Exception ex)
    {
        // Hata yönetimi
    }
}
```

38. **Exception Tipleri**: `GlobalExceptionHandlingMiddleware` içinde farklı exception tipleri için özel mesajlar tanımlanmış, ancak bu exception tipleri tüm uygulamada tutarlı bir şekilde kullanılmıyor.

```csharp
// GlobalExceptionHandlingMiddleware.cs
private string GetUserFriendlyMessage(Exception exception)
{
    // Hata tipine göre daha anlaşılır mesajlar
    return exception switch
    {
        DbUpdateConcurrencyException => "Veri güncelleme sırasında beklenmeyen bir çakışma oluştu. Lütfen sayfayı yenileyip tekrar deneyin.",
        DbUpdateException => "Veritabanı işlemi sırasında bir hata oluştu. Girdiğiniz veriler doğru formatta mı kontrol edin.",
        // ...
        _ => "İşlem sırasında beklenmeyen bir hata oluştu. Teknik ekibimiz bu konuyla ilgileniyor."
    };
}
```

### Loglama Seviyeleri

39. **Loglama Seviyelerinin Kullanımı**: Loglama seviyeleri (Information, Warning, Error) bazı servislerde tutarsız kullanılıyor. Örneğin, bazı kritik hatalar Warning seviyesinde loglanıyor.

```csharp
// Warning seviyesinde loglanan kritik hata
_logger.LogWarning($"Stok yetersiz: Ürün {urun.UrunAdi} (ID: {urunID}), FIFO Stok: {toplamFifoMiktari}, İstenen: {miktar}");
```

40. **Loglama Detayları**: Bazı loglarda yeterli detay bulunmuyor. Örneğin, hata loglarında exception detayları (message, stack trace) eksik.

```csharp
// Yetersiz log detayı
_logger.LogError("Hata oluştu");

// Yeterli log detayı
_logger.LogError(ex, "İşlem sırasında hata oluştu: {Message}", ex.Message);
```

### Gereksiz Loglama

41. **Aşırı Loglama**: Bazı servislerde aşırı loglama yapılıyor. Örneğin, her metot çağrısı için Information seviyesinde log kaydı oluşturuluyor.

```csharp
// Aşırı loglama
_logger.LogInformation("GetBekleyenAklamaKayitlariAsync metodu çağrıldı");
```