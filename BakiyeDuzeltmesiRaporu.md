# Cari Bakiye Düzeltmesi Geliştirme Raporu

## Tespit Edilen Sorun

Cari kartlarında başlangıç bakiyesi güncellendiğinde, bu değişiklik cari hesap hareketlerine yansımıyordu. Kullanıcı bir cari kartında bakiyeyi (örneğin 5000 TL olarak) güncellediğinde, bu değişiklik sadece cari kartında görünüyor, ancak cari hesap ekstrelerinde ve hareketlerinde görünmüyordu.

## Yapılan Değişiklikler

### 1. CariEditViewModel Sınıfında Değişiklik

`ViewModels/Cari/CariEditViewModel.cs` dosyasına mevcut bakiyeyi saklamak için yeni bir özellik eklendi:

```csharp
// Önceki bakiyeyi saklamak için kullanılacak
public decimal MevcutBakiye { get; set; }
```

### 2. CariController'da Düzenlemeler

`Controllers/CariController.cs` dosyasında `Edit` metodu güncellendi:

#### Get Metodu Değişikliği
```csharp
[HttpGet]
public async Task<IActionResult> Edit(Guid id)
{
    var cari = await _unitOfWork.CariRepository.GetByIdAsync(id);
    if (cari == null)
    {
        return NotFound();
    }

    var model = new CariEditViewModel
    {
        // Diğer özellikler...
        BaslangicBakiye = cari.BaslangicBakiye,
        MevcutBakiye = cari.BaslangicBakiye, // Mevcut bakiyeyi saklamak için eklendi
        // Diğer özellikler...
    };
    
    // Diğer kodlar...
}
```

#### Post Metodu Değişikliği
```csharp
[HttpPost]
[ValidateAntiForgeryToken]
public async Task<IActionResult> Edit(Guid id, CariEditViewModel model)
{
    // Doğrulama kontrolleri...
    
    try
    {
        var cari = await _unitOfWork.CariRepository.GetByIdAsync(id);
        if (cari == null)
        {
            return NotFound();
        }
        
        // Bakiye değişikliğini kontrol et
        var bakiyeDegisimi = model.BaslangicBakiye - model.MevcutBakiye;
        
        // Cari güncelleme işlemleri...
        
        // Bakiye değişikliği varsa cari hareketi oluştur
        if (bakiyeDegisimi != 0)
        {
            var cariHareket = new Data.Entities.CariHareket
            {
                CariId = cari.CariID,
                HareketTuru = "Bakiye Düzeltmesi",
                Tutar = Math.Abs(bakiyeDegisimi),
                Tarih = DateTime.Now,
                Aciklama = $"Bakiye düzeltmesi yapıldı. Önceki bakiye: {model.MevcutBakiye:N2}, Yeni bakiye: {model.BaslangicBakiye:N2}",
                OlusturmaTarihi = DateTime.Now,
                OlusturanKullaniciId = Guid.Parse(User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value),
                Borc = bakiyeDegisimi < 0 ? Math.Abs(bakiyeDegisimi) : 0, // Bakiye azaldıysa borç
                Alacak = bakiyeDegisimi > 0 ? bakiyeDegisimi : 0, // Bakiye arttıysa alacak
                Silindi = false
            };
            
            await _unitOfWork.CariHareketRepository.AddAsync(cariHareket);
        }
        
        await _unitOfWork.SaveAsync();
        
        // Log oluşturma ve başarı mesajları...
    }
    catch (Exception ex)
    {
        // Hata yönetimi...
    }
    
    // Diğer kodlar...
}
```

### 3. Veritabanı Güncellemesi

Değişiklikler için bir migration oluşturuldu ve veritabanı güncellendi:

```bash
dotnet ef migrations add CariHareketBakiyeDuzeltmesi --context ApplicationDbContext
dotnet ef database update --context ApplicationDbContext
```

## Sonuç

Bu değişiklikler ile:

1. Kullanıcılar artık cari bakiyelerini güncellediklerinde, bu değişiklik otomatik olarak "Bakiye Düzeltmesi" başlığı altında cari hareketlerine yansıtılmaktadır.
2. Cari hareketine eklenen kayıtta, eski ve yeni bakiye değerleri açıklama kısmında görünmektedir.
3. Bakiyedeki artış "Alacak", azalış ise "Borç" olarak kaydedilmektedir.
4. Kullanıcılar cari hesap ekstrelerinde bakiye değişikliklerini izleyebilmektedir.

Bu geliştirme, muhasebe sistemi içinde daha iyi izlenebilirlik ve denetim sağlar, ayrıca kullanıcı deneyimini iyileştirir. 