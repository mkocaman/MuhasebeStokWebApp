# Kasa Modülü - Veritabanı Tablo Düzeltme Raporu

## Tespit Edilen Sorunlar

1. **Tablo İsimlendirmesi Sorunu**:
   - Scaffold edilen veritabanındaki tablo adı `CariHareketler` olarak görünüyor
   - Entity Framework modelimiz ise `CariHareket` olarak tanımlanmış (tekil form)
   - Bu nedenle EF Core tablo ile varlık sınıfını eşleştiremiyor

2. **Sütun İsimlendirmesi Sorunu**:
   - Veritabanında sütun adları: `CariHareketId`, `CariId`, `ReferansId`, `OlusturanKullaniciId`
   - Entity modelinde: `CariHareketID`, `CariID`, `ReferansID`, `OlusturanKullaniciID` 
   - Büyük-küçük harf duyarlılığı farkları EF Core'un SQL Server ile eşleştirme yapmasını engelliyor

3. **Eksik Sütunlar**:
   - `Alacak` ve `Borc` sütunları veritabanında yok

## Yapılan Değişiklikler

1. **Entity Model Düzeltmeleri**:
   - `[Table("CariHareketler")]` özniteliği eklendi 
   - Sütun adları için `[Column("VeritabanındakiAd")]` öznitelikleri eklendi:
     - `[Column("CariHareketId")]` 
     - `[Column("CariId")]`
     - `[Column("ReferansId")]`
     - `[Column("OlusturanKullaniciId")]`
     - `[Column("VadeTarihi")]`

2. **Eksik Sütunlar için Düzeltme**:
   - `CariHareketlerDuzelt.sql` adlı bir SQL betiği oluşturuldu:
     ```sql
     -- Alacak sütununu ekle (eğer yoksa)
     IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('CariHareketler') AND name = 'Alacak')
     BEGIN
         ALTER TABLE CariHareketler ADD Alacak decimal(18,2) NOT NULL DEFAULT 0;
     END
     
     -- Borc sütununu ekle (eğer yoksa)
     IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('CariHareketler') AND name = 'Borc')
     BEGIN
         ALTER TABLE CariHareketler ADD Borc decimal(18,2) NOT NULL DEFAULT 0;
     END
     ```

3. **Yeni Migration Oluşturuldu**:
   - `CariHareketlerTableFix` adlı migration ile varlık modeli değişiklikleri kaydedildi
   - Migration veritabanına başarıyla uygulandı

## Sonuç ve Öneriler

✅ **Entity Framework modeli veritabanıyla eşleştirildi**:
   - Table ve Column öznitelikleri ile düzgün eşleşme sağlandı
   - CamelCase/PascalCase farkları giderildi

✅ **Yapılan düzeltmeler**:
   - Entity modeli veritabanına uyumlu hale getirildi
   - Migration başarıyla uygulandı
   - Proje başarıyla derlendi ve çalıştırıldı

### Öneriler:

1. **Proje Genelinde Standartlaştırma**:
   - Tüm entity sınıflarında Table ve Column özniteliklerinin kullanılması
   - Entity isimleri ve tablo isimleri arasında uyumluluk sağlanması
   - ID/Id biçimlendirilmesinde tutarlılık sağlanması

2. **Veritabanı Sorgularını Güçlendirme**:
   - EF Core'un veritabanını oluştururken sütun adlarını ve büyük-küçük harf kullanımını tutarlı hale getirmesi
   - Yeni migration'larda öncekilerle uyumluluk sağlamak için var olanlara dikkat edilmesi

3. **Uzun Vadeli İyileştirme**:
   - Tüm veritabanı şemasının tek bir isimlendirme standardına göre yeniden düzenlenmesi
   - Tekil/çoğul form kullanımında tutarlı olunması (Entity sınıfları tekil, tablo adları çoğul veya tam tersi) 