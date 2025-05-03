# Entity Framework Core Migration Dosyalarını Birleştirme Kılavuzu

## Sorun

MuhasebeStokWebApp uygulamasında zaman içinde oluşturulmuş çok sayıda küçük migration dosyası bulunmaktadır. Bu durum:

1. Kodun bakımını zorlaştırmakta
2. Migration uygulaması sırasında hata riski oluşturmakta
3. Veritabanı şemasının anlaşılmasını zorlaştırmakta
4. Yeni geliştirmelerde migration çakışmalarına yol açmakta

## Çözüm

Migration dosyalarını mantıklı kategorilere göre birleştirerek daha anlamlı ve yönetilebilir bir yapı oluşturulacaktır.

## Uygulama Adımları

### 1. Ön Hazırlık

- Uygulama kodlarını ve veritabanını yedeğini alın
- Migration işlemini DEV ortamında test edin
- `__EFMigrationsHistory` tablosunun yedeğini alın

### 2. Mevcut Migration'ları Analiz Etme

Migration'lar şu kategorilere ayrılmıştır:

1. **Temel Veritabanı Yapısı**
2. **Cari Modülü İyileştirmeleri**
3. **Fatura ve İrsaliye Modeli İyileştirmeleri**
4. **Stok Modülü İyileştirmeleri**
5. **Para Birimi ve Kur İyileştirmeleri**
6. **Format ve Veri Tipi Düzeltmeleri**
7. **Diğer İyileştirmeler**

### 3. Migration Birleştirme İşlemi

1. Mevcut migration'ları yedekleyin:
   ```bash
   mkdir -p MigrationsBackup
   cp -r Migrations/* MigrationsBackup/
   ```

2. Birleştirme scriptini çalıştırın:
   ```bash
   ./Scripts/MergeEFMigrations.sh
   ```

3. Script otomatik olarak:
   - Mevcut migration'ları yedekler
   - Gerekli temizleme işlemlerini yapar
   - Yeni birleştirilmiş migration'ları oluşturur
   - Tüm değişiklikleri içeren SQL scriptini oluşturur

### 4. Veritabanını Güncelleme

1. Geliştirme ortamında veritabanını güncelleyin:
   ```bash
   dotnet ef database update --context ApplicationDbContext
   ```

2. Veritabanını kontrol edin ve tüm verilerin doğru şekilde korunduğundan emin olun
   
3. Production ortamında SQL scriptini kullanın:
   ```bash
   # Oluşturulan SQL scripti (consolidated_migrations.sql) kullanılabilir
   ```

### 5. Geri Alma (Rollback) Planı

Sorun oluşursa:

1. Yedeklenen migration dosyalarını geri yükleyin:
   ```bash
   rm -rf Migrations/*.cs
   cp -r MigrationsBackup/* Migrations/
   ```

2. Veritabanını yedekten geri yükleyin veya mevcut migration'ları tekrar uygulayın

## Dikkat Edilecek Noktalar

- Bu işlem veritabanı şemasını değiştirmez, sadece migration dosyalarını düzenler
- İşlem sırasında veri kaybı olmamalıdır
- Her adımdan sonra kontrol yapılması önemlidir
- Birleştirme işlemi geri alınamaz, bu nedenle yedekler kritik önem taşır

## Kaynaklar

- [Entity Framework Core - Custom Migrations](https://docs.microsoft.com/en-us/ef/core/managing-schemas/migrations/custom)
- [Working with DbContext](https://docs.microsoft.com/en-us/ef/core/dbcontext-configuration/) 