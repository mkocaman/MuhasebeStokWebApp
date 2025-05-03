# Birim Test Dökümanı

## İçindekiler
1. [Giriş](#giriş)
2. [Test Mimarisi](#test-mimarisi)
3. [Öncelikli Test Alanları](#öncelikli-test-alanları)
4. [Test Projesinin Kurulumu](#test-projesinin-kurulumu)
5. [Mock ve Stub Kullanımı](#mock-ve-stub-kullanımı)
6. [Örnek Testler](#örnek-testler)
7. [Test Raporlama](#test-raporlama)

## Giriş

Birim testleri, bir uygulamanın en küçük test edilebilir parçalarını (genellikle metotlar) izole şekilde test eder. Bu testler, kodun beklendiği gibi çalıştığını doğrulamak için kullanılır ve şu avantajları sağlar:

- **Hataların erken tespiti**: Sorunları daha entegrasyon aşamasına gelmeden tespit etme
- **Regresyon önleme**: Mevcut kodu değiştirdiğimizde beklenmeyen yan etkilerin hızlıca tespit edilmesi
- **Tasarım iyileştirme**: Test edilebilir kod yazmak, daha modüler ve bakımı kolay kod oluşturulmasını teşvik eder
- **Dokümantasyon**: Testler, kodun nasıl kullanılması gerektiğine dair canlı bir dokümantasyon sağlar

## Test Mimarisi

MuhasebeStokWebApp için birim test mimarisi aşağıdaki şekilde kurulacaktır:

1. **Test Projesi**: `MuhasebeStokWebApp.Tests` adında ayrı bir test projesi oluşturulacak.
2. **Test Çerçevesi**: xUnit test çerçevesi kullanılacak.
3. **Mock Kütüphanesi**: Moq kütüphanesi ile bağımlılıkların taklit edilmesi sağlanacak.
4. **Test Klasör Yapısı**: Test projesi klasör yapısı, ana projenin yapısına benzer şekilde organize edilecek.

## Öncelikli Test Alanları

Aşağıdaki alanlar öncelikli olarak test edilmelidir:

1. **FIFO Stok Yönetimi**:
   - Stok girişi
   - Stok çıkışı
   - Stok yetersizlik durumları
   - FIFO sıralaması doğrulaması

2. **Para Birimi Dönüşümleri**:
   - Farklı para birimleri arasında dönüşüm
   - Kur değerleriyle hesaplamalar
   - Yuvarlama işlemleri

3. **İş Kuralları & Validasyonlar**:
   - Fatura validasyonları
   - Stok işlem validasyonları
   - İş kuralı kontrolü

4. **Exception Yönetimi**:
   - Exception stratejilerinin doğru çalışması
   - Hata durumlarının doğru şekilde ele alınması

5. **Concurrency Yönetimi**:
   - Eşzamanlı işlemlerde veri tutarlılığı
   - Retry mekanizmaları

## Test Projesinin Kurulumu

### Adım 1: xUnit Test Projesi Oluşturma

```bash
dotnet new xunit -o MuhasebeStokWebApp.Tests
```

### Adım 2: Gerekli Paketlerin Yüklenmesi

```bash
cd MuhasebeStokWebApp.Tests
dotnet add package Moq
dotnet add package Microsoft.EntityFrameworkCore.InMemory
dotnet add package Microsoft.NET.Test.Sdk
dotnet add reference ../MuhasebeStokWebApp.csproj
```

### Adım 3: Test Klasör Yapısının Oluşturulması

```
MuhasebeStokWebApp.Tests/
├── Services/
│   ├── ParaBirimiDonusumTests/
│   ├── StokFifoTests/
│   ├── FaturaCrudServiceTests/
│   └── ...
├── Controllers/
├── Middleware/
└── Helpers/
```

## Mock ve Stub Kullanımı

Birim testlerde bağımlılıkları izole etmek için Mock ve Stub nesneler kullanılır:

- **Mock**: Çağrılan metotların nasıl çağrıldığını doğrulamak için kullanılır
- **Stub**: Test edilen birime belirli girdileri sağlamak için kullanılır

### Mock Örneği (Moq ile)

```csharp
// Mock logger oluşturma
var loggerMock = new Mock<ILogger<ParaBirimiDonusumHelper>>();

// Mock doviz kuru servisi oluşturma
var dovizKuruServiceMock = new Mock<IDovizKuruService>();

// Stub: GetGuncelKurAsync metodu çağrıldığında 18.5 döndürecek şekilde ayarlama
dovizKuruServiceMock
    .Setup(s => s.GetGuncelKurAsync("USD", "TRY"))
    .ReturnsAsync(18.5m);

// Test edilecek sınıfı mock bağımlılıklarla oluşturma
var donusumHelper = new ParaBirimiDonusumHelper(
    dovizKuruServiceMock.Object,
    loggerMock.Object
);
```

## Örnek Testler

### ParaBirimiDonusumHelper Testi

```csharp
public class ParaBirimiDonusumHelperTests
{
    private readonly Mock<IDovizKuruService> _dovizKuruServiceMock;
    private readonly Mock<ILogger<ParaBirimiDonusumHelper>> _loggerMock;
    private readonly IParaBirimiDonusumHelper _donusumHelper;

    public ParaBirimiDonusumHelperTests()
    {
        _dovizKuruServiceMock = new Mock<IDovizKuruService>();
        _loggerMock = new Mock<ILogger<ParaBirimiDonusumHelper>>();
        _donusumHelper = new ParaBirimiDonusumHelper(
            _dovizKuruServiceMock.Object,
            _loggerMock.Object
        );
    }

    [Fact]
    public async Task ToUsdAsync_WhenParaBirimiIsTRY_ReturnsDivided()
    {
        // Arrange
        decimal tryTutar = 100m;
        decimal beklenenUsdTutar = 5m;
        
        _dovizKuruServiceMock
            .Setup(s => s.GetGuncelKurAsync("TRY", "USD"))
            .ReturnsAsync(0.05m);

        // Act
        var sonuc = await _donusumHelper.ToUsdAsync("TRY", tryTutar);

        // Assert
        Assert.Equal(beklenenUsdTutar, sonuc);
    }

    [Fact]
    public async Task ParaBirimiDonusturAsync_AyniParaBirimi_DegisiklikYapmaz()
    {
        // Arrange
        decimal tutar = 100m;

        // Act
        var sonuc = await _donusumHelper.ParaBirimiDonusturAsync("USD", "USD", tutar);

        // Assert
        Assert.Equal(tutar, sonuc);
    }
}
```

### StokFifoService Testi

```csharp
public class StokFifoServiceTests
{
    private readonly Mock<ApplicationDbContext> _contextMock;
    private readonly Mock<ILogger<StokFifoService>> _loggerMock;
    private readonly Mock<IMaliyetHesaplamaService> _maliyetServiceMock;
    private readonly Mock<IDovizKuruService> _dovizKuruServiceMock;
    private readonly IStokFifoService _stokFifoService;

    public StokFifoServiceTests()
    {
        _contextMock = new Mock<ApplicationDbContext>(new DbContextOptions<ApplicationDbContext>());
        _loggerMock = new Mock<ILogger<StokFifoService>>();
        _maliyetServiceMock = new Mock<IMaliyetHesaplamaService>();
        _dovizKuruServiceMock = new Mock<IDovizKuruService>();
        
        // InMemory veritabanı üzerinde çalışan gerçek bir context kullanmak daha faydalı olabilir
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
        
        var context = new ApplicationDbContext(options);
        
        _stokFifoService = new StokFifoService(
            context,
            _loggerMock.Object,
            _maliyetServiceMock.Object,
            _dovizKuruServiceMock.Object
        );
    }

    [Fact]
    public async Task StokGirisiYap_ValidParams_CreatesCorrectFifoEntry()
    {
        // Arrange
        var urunId = Guid.NewGuid();
        decimal miktar = 10m;
        decimal birimFiyat = 20m;
        string birim = "Adet";
        string referansNo = "REF-001";
        
        // Act
        var sonuc = await _stokFifoService.StokGirisiYap(
            urunId, miktar, birimFiyat, birim, referansNo, 
            "Manuel", Guid.NewGuid(), "Test girişi"
        );

        // Assert
        Assert.NotNull(sonuc);
        Assert.Equal(urunId, sonuc.UrunID);
        Assert.Equal(miktar, sonuc.Miktar);
        Assert.Equal(miktar, sonuc.KalanMiktar);
        Assert.Equal(birimFiyat, sonuc.BirimFiyat);
        Assert.Equal(birim, sonuc.Birim);
        Assert.Equal(referansNo, sonuc.ReferansNo);
    }

    [Fact]
    public async Task StokCikisiYap_YetersizStok_ThrowsStokYetersizException()
    {
        // Bu test için önce veri hazırlayıp sonra test etmek gerekir
        // Bu örnekte sadece exception kontrolü gösteriliyor
        
        // Arrange
        var urunId = Guid.NewGuid();
        decimal miktar = 100m; // Stokta olmayan bir miktar
        
        // Act & Assert
        await Assert.ThrowsAsync<StokYetersizException>(() => 
            _stokFifoService.StokCikisiYap(urunId, miktar, "REF-002", "Manuel", Guid.NewGuid(), "Test çıkışı")
        );
    }
}
```

## Test Raporlama

Test raporları, test sonuçlarını görselleştirmek ve analiz etmek için çok önemlidir. Aşağıdaki araçlar test raporlaması için kullanılabilir:

1. **ReportGenerator**: Test kapsamı (code coverage) raporları oluşturur
2. **Coverlet**: .NET Core için kod kapsama aracı
3. **AzureDevOps Test Reports**: CI/CD sürecinde entegre edilebilir

### Test Kapsamı Raporu Oluşturma

```bash
# Testleri çalıştır ve kod kapsama verilerini topla
dotnet test --collect:"XPlat Code Coverage"

# ReportGenerator ile HTML rapor oluştur
dotnet reportgenerator -reports:"**/coverage.cobertura.xml" -targetdir:"coveragereport" -reporttypes:Html
```

## Test Koşulları

Testler aşağıdaki şartlarda düzenli olarak çalıştırılmalıdır:

1. **Geliştirici Makinesi**: Değişiklikler commit edilmeden önce
2. **CI/CD Pipeline**: Pull request ve merge işlemlerinde
3. **Planlanmış Çalıştırma**: Günlük veya haftalık olarak

Testlerin %85 ve üzeri kod kapsamı hedeflenmelidir. Özellikle kritik bileşenler için bu oran %90'ın üzerinde olmalıdır. 