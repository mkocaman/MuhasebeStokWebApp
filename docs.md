# MuhasebeStokWebApp Modül Dokümantasyonu

Bu doküman, MuhasebeStokWebApp projesindeki tüm ana modülleri ve katmanları ayrı ayrı detaylandırır. Her modülün amacı, temel işlevleri, ilişkili dosya ve sınıfları, veri akışı ve önemli noktalar özetlenmiştir.

---

## 1. Controllers (Denetleyiciler)
- **Amaç:** HTTP isteklerini karşılar, iş mantığını servis katmanına yönlendirir, View veya JSON döner.
- **Konum:** `/Controllers`
- **Örnekler:** `FaturaController`, `CariController`, `StokController`, `SatisController`, `KullaniciController` vb.
- **Özellikler:**
  - Yetkilendirme (Authorize attribute)
  - Model validasyonu
  - Servis çağrıları
  - ViewModel ile veri alışverişi

---

## 2. Models (Veri Modelleri)
- **Amaç:** Veritabanı tablolarını ve iş nesnelerini temsil eder.
- **Konum:** `/Models`
- **Örnekler:** `Fatura`, `Cari`, `Stok`, `Kullanici`, `Urun`, `Sozlesme` vb.
- **Özellikler:**
  - Entity Framework ile uyumlu
  - Data Annotation ile validasyon
  - İlişkiler (Navigation Properties)

---

## 3. ViewModels
- **Amaç:** UI ile veri alışverişini kolaylaştıran, modelden bağımsız veri taşıyıcılarıdır.
- **Konum:** `/ViewModels`
- **Örnekler:** `FaturaViewModel`, `CariViewModel`, `StokViewModel`
- **Özellikler:**
  - Kullanıcıya özel veri sunumu
  - Form validasyonu

---

## 4. Services (Servis Katmanı)
- **Amaç:** İş mantığı ve operasyonların merkezi. Controller’lar tarafından kullanılır.
- **Konum:** `/Services`
- **Örnekler:**
  - `FaturaService`, `CariService`, `StokService`, `LogService`, `SozlesmeService`, `EmailService`, `ValidationService`
- **Özellikler:**
  - Repository Pattern ile veri erişimi
  - İş kuralları ve validasyon
  - Diğer servislerle entegrasyon

---

## 5. Data (Veri Katmanı)
- **Amaç:** Veritabanı bağlantısı, migration ve repository işlemleri
- **Konum:** `/Data`, `/Migrations`
- **Ana Sınıflar:**
  - `ApplicationDbContext`, `Repository<T>`, `UnitOfWork`
- **Özellikler:**
  - Entity Framework Core ile DB işlemleri
  - Migration yönetimi
  - Soyut veri erişimi

---

## 6. Enums (Sabitler ve Tipler)
- **Amaç:** Sabit değerler ve tip güvenliği için enum tanımları
- **Konum:** `/Enums`
- **Örnekler:** `FaturaDurumu`, `StokTipi`, `KullaniciRol`

---

## 7. Exceptions (Özel Hata Yönetimi)
- **Amaç:** Uygulama genelinde hata yönetimi ve özelleştirilmiş exception stratejileri
- **Konum:** `/Exceptions`
- **Özellikler:**
  - Farklı hata türleri için özel sınıflar
  - ExceptionStrategy pattern

---

## 8. Extensions (Uzantılar)
- **Amaç:** Sık kullanılan yardımcı metotlar ve uzantı fonksiyonları
- **Konum:** `/Extensions`
- **Özellikler:**
  - String, DateTime, Collection uzantıları

---

## 9. Middleware (Ara Katmanlar)
- **Amaç:** HTTP pipeline’a eklenen özel işlemler (ör: hata yakalama, loglama)
- **Konum:** `/Middleware`
- **Özellikler:**
  - Exception handling
  - Request/response loglama

---

## 10. Validators (Doğrulayıcılar)
- **Amaç:** FluentValidation ile model ve ViewModel doğrulama işlemleri
- **Konum:** `/Validators`
- **Özellikler:**
  - Kural bazlı validasyon

---

## 11. MappingProfiles (AutoMapper Profilleri)
- **Amaç:** Model ve ViewModel dönüşümleri için AutoMapper profilleri
- **Konum:** `MappingProfiles.cs`

---

## 12. Hubs (SignalR Gerçek Zamanlı Bildirim)
- **Amaç:** Gerçek zamanlı bildirim ve mesajlaşma
- **Konum:** `/Hubs`
- **Özellikler:**
  - Bildirim ve canlı veri akışı

---

## 13. ViewComponents
- **Amaç:** Yeniden kullanılabilir UI bileşenleri
- **Konum:** `/ViewComponents`

---

## 14. Resources
- **Amaç:** Lokalizasyon ve kaynak dosyaları
- **Konum:** `/Resources`

---

## 15. Scripts
- **Amaç:** Yardımcı scriptler ve otomasyon dosyaları
- **Konum:** `/Scripts`

---

## 16. wwwroot (Statik Dosyalar)
- **Amaç:** JS, CSS, görseller ve diğer statik içerikler
- **Konum:** `/wwwroot`

---

## 17. Konfigürasyon Dosyaları
- **Amaç:** Ortam ve uygulama ayarları
- **Dosyalar:**
  - `appsettings.json`, `appsettings.Development.json`, `libman.json`, `package.json`

---

## 18. SQL Scriptleri
- **Amaç:** Veritabanı migration ve bakım scriptleri
- **Dosyalar:**
  - `*.sql` dosyaları kök dizinde

---

Aşağıda, her ana modül için örnek kullanım senaryoları, teknik detaylar ve kod blokları sunulmuştur. Projenizdeki modülleri daha hızlı kavrayabilir ve geliştirme sürecinde referans olarak kullanabilirsiniz.

---

### Controllers (Örnek: FaturaController)

**Kullanım Senaryosu:**
FaturaController, fatura oluşturma, listeleme, güncelleme ve silme işlemlerini yönetir. Kullanıcıdan gelen HTTP isteklerini karşılar ve ilgili servisleri çağırır.

**Temel Kod Örneği:**
```csharp
[Authorize]
public class FaturaController : BaseController
{
    private readonly IFaturaService _faturaService;
    // ... diğer bağımlılıklar

    public FaturaController(IFaturaService faturaService, ...) : base(...) 
    {
        _faturaService = faturaService;
        // ...
    }

    [HttpGet]
    public IActionResult Index()
    {
        var faturalar = _faturaService.GetAllFaturalar();
        return View(faturalar);
    }

    [HttpPost]
    public IActionResult Create(FaturaViewModel model)
    {
        if (ModelState.IsValid)
        {
            _faturaService.CreateFatura(model);
            return RedirectToAction("Index");
        }
        return View(model);
    }
}
```
**Notlar:**
- Controller’lar, servis katmanındaki iş mantığını çağırır.
- Model validasyonu ve hata yönetimi yapılır.
- Kimlik doğrulama `[Authorize]` ile sağlanır.

---

### Models (Örnek: Fatura.cs)

**Kullanım Senaryosu:**
Veritabanında Fatura tablosunu temsil eder.

**Kod Örneği:**
```csharp
public class Fatura
{
    public int Id { get; set; }
    public string FaturaNo { get; set; }
    public DateTime Tarih { get; set; }
    public decimal ToplamTutar { get; set; }
    public ICollection<FaturaDetay> Detaylar { get; set; }
}
```
**Notlar:**
- Data Annotation ile validasyon eklenebilir.
- Navigation property ile ilişkili detaylar çekilebilir.

---

### Services (Örnek: FaturaService)

**Kullanım Senaryosu:**
Fatura ile ilgili iş mantığını ve veri erişimini yönetir.

**Kod Örneği:**
```csharp
public class FaturaService : IFaturaService
{
    private readonly IRepository<Fatura> _faturaRepo;
    public FaturaService(IRepository<Fatura> faturaRepo)
    {
        _faturaRepo = faturaRepo;
    }

    public IEnumerable<Fatura> GetAllFaturalar()
    {
        return _faturaRepo.GetAll();
    }

    public void CreateFatura(FaturaViewModel model)
    {
        var fatura = new Fatura { /* ... */ };
        _faturaRepo.Add(fatura);
    }
}
```
**Notlar:**
- Repository Pattern ile veri erişimi soyutlanır.
- İş kuralları burada uygulanır.

---

### ViewModels (Örnek: FaturaViewModel)

**Kullanım Senaryosu:**
Kullanıcıdan alınan veya kullanıcıya gösterilecek fatura verisini taşır.

**Kod Örneği:**
```csharp
public class FaturaViewModel
{
    public string FaturaNo { get; set; }
    public DateTime Tarih { get; set; }
    public decimal ToplamTutar { get; set; }
}
```
**Notlar:**
- ViewModel, Model’den bağımsızdır ve UI’ya özeldir.
- Validasyon kuralları eklenebilir.

---

### Data (Örnek: ApplicationDbContext)

**Kullanım Senaryosu:**
Veritabanı işlemlerini yönetir.

**Kod Örneği:**
```csharp
public class ApplicationDbContext : DbContext
{
    public DbSet<Fatura> Faturalar { get; set; }
    // ...
}
```
**Notlar:**
- Migration işlemleri bu katmanda yürütülür.

---

Benzer şekilde, diğer modüller için de örnek kodlar ve kullanım açıklamaları ekleyebilirsiniz. Daha fazla teknik detay veya özel bir modül için örnek isterseniz belirtmeniz yeterlidir.
