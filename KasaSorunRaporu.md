# Kasa Modülü Sorun Analizi ve Çözüm Raporu

## Tespit Edilen Sorunlar

1. **CariHareket Tablosu Şema Sorunları**:
   - Veritabanındaki `CariHareketler` tablosunda `Alacak`, `Borc` ve `IslemTarihi` kolonları eksik
   - Kasada görülen `DataTables warning: table id=kasaTable - Incorrect column count` hatası
   - Anasayfadaki `Invalid column name 'Alacak'`, `Invalid column name 'Borc'` ve `Invalid column name 'IslemTarihi'` hataları

2. **SignalR Bağlantı Hataları**:
   - WebSocket bağlantı hatası
   - Bildirim hub'ı ile ilgili iletişim sorunları

## Sorunların Nedenleri

1. **CariHareket Entity Güncelleme Sorunu**:
   - `CariHareket.cs` sınıfında yeni eklenen `Alacak`, `Borc` ve `IslemTarihi` özellikleri, veritabanına yansıtılmamış
   - Entity sınıfı ile veritabanı şeması arasında tutarsızlık bulunuyor
   - Mevcut migration'lar bu sütunları içermiyor

2. **CariID Sütun Çakışması**:
   - `CariHareket` tablosunda hem `CariID` hem de `CariId` sütunları bulunuyor (biri fiziksel, diğeri computed property)
   - Migration girişimleri sırasında "Column names in each table must be unique. Column name 'CariID' in table 'CariHareketler' is specified more than once." hatası

## Yapılan İyileştirmeler

1. **CariHareket Entity Düzenlemesi**:
   - `CariHareket` sınıfı, eksik kolonlar (Alacak, Borc ve IslemTarihi) içerecek şekilde yapılandırıldı
   - Bu alanlar için Constructor'da varsayılan değerler atandı
   - `Borc = 0`, `Alacak = 0`, `IslemTarihi = DateTime.Now`

2. **Migration İşlemleri**:
   - `AddMissingCariHareketColumns` isimli migration oluşturuldu
   - `CariID` sütun çakışması nedeniyle migration uygulanamadı
   - Migration işlemi kaldırıldı

## Önerilen Çözümler

1. **Kısa Vadeli Çözüm**:
   - `CariHareket` sınıfındaki `Alacak`, `Borc` ve `IslemTarihi` özelliklerinin doğru şekilde yapılandırıldığından emin olundu
   - Bu özellikler için SQL komutları ile doğrudan veritabanında kolonlar oluşturulabilir

2. **Orta Vadeli Çözüm**:
   - Veritabanı şemasını yeniden oluşturmak için yedek alınması
   - Entity modellerinde düzenlemeler yaparak CariId/CariID çakışmasının giderilmesi
   - Yeni bir migration ile tüm şemanın tekrar oluşturulması

3. **Uzun Vadeli Çözüm**:
   - Tüm entity sınıflarının gözden geçirilmesi
   - CariId/CariID gibi benzer özellik isimlerinin standartlaştırılması
   - Duplicate property'ler yerine daha iyi tasarım modellerinin kullanılması

## Veritabanı Değişiklikleri için SQL Sorguları

```sql
-- Aşağıdaki SQL sorguları doğrudan veritabanına uygulanabilir:

-- Alacak sütununu ekle
IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('CariHareketler') AND name = 'Alacak')
BEGIN
    ALTER TABLE CariHareketler ADD Alacak decimal(18,2) NOT NULL DEFAULT 0;
END

-- Borc sütununu ekle
IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('CariHareketler') AND name = 'Borc')
BEGIN
    ALTER TABLE CariHareketler ADD Borc decimal(18,2) NOT NULL DEFAULT 0;
END

-- IslemTarihi sütununu ekle
IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('CariHareketler') AND name = 'IslemTarihi')
BEGIN
    ALTER TABLE CariHareketler ADD IslemTarihi datetime2 NOT NULL DEFAULT GETDATE();
END

-- Varolan kayıtlar için değerleri güncelle
UPDATE CariHareketler 
SET IslemTarihi = Tarih,
    Alacak = CASE WHEN HareketTuru = 'Tahsilat' THEN Tutar ELSE 0 END,
    Borc = CASE WHEN HareketTuru = 'Odeme' THEN Tutar ELSE 0 END
WHERE (IslemTarihi IS NULL OR IslemTarihi = '0001-01-01 00:00:00') 
   OR (Alacak = 0 AND Borc = 0)
```

## Sonuç

Sistem, CariHareket tablosunda eksik olan alanlar nedeniyle hata veriyor. Entity modelinde bu alanlar tanımlanmış ancak veritabanına yansıtılmamış. CariID/CariId sütun çakışması nedeniyle migration ile bu sorun çözülemedi.

Bu sorun, veritabanını doğrudan SQL komutları ile değiştirerek veya tüm veritabanını yeniden oluşturarak çözülebilir. Kısa vadede, SQL komutları ile veritabanında eksik sütunların oluşturulması önerilir.

## İlerleme Durumu

- Model sınıfları düzenlendi ve varsayılan değerler eklendi
- Çeşitli migration denemeleri yapıldı, ancak sütun çakışmaları nedeniyle başarısız oldu
- Sorunların çözümü için SQL sorguları hazırlandı
- Daha kapsamlı düzeltmeler ve standardizasyon için ek çalışmalara ihtiyaç var

> Not: Bu sorunlar projenin temel mimarisindeki bazı tasarım kararlarından kaynaklanmaktadır. Uzun vadede, entity sınıflarının ve veritabanı şemasının yeniden düzenlenmesi önerilir. 