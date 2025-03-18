-- DovizKurlari tablosuna varsayılan kayıtlar ekle
-- Önce Dovizler tablosundan ID'leri alabilmek için değişkenler tanımlayalım
DECLARE @TRY_ID uniqueidentifier;
DECLARE @USD_ID uniqueidentifier;
DECLARE @EUR_ID uniqueidentifier;
DECLARE @GBP_ID uniqueidentifier;
DECLARE @UZS_ID uniqueidentifier;

-- Dovizler tablosundan ID'leri çekelim
SELECT @TRY_ID = DovizID FROM Dovizler WHERE DovizKodu = 'TRY';
SELECT @USD_ID = DovizID FROM Dovizler WHERE DovizKodu = 'USD';
SELECT @EUR_ID = DovizID FROM Dovizler WHERE DovizKodu = 'EUR';
SELECT @GBP_ID = DovizID FROM Dovizler WHERE DovizKodu = 'GBP';
SELECT @UZS_ID = DovizID FROM Dovizler WHERE DovizKodu = 'UZS';

-- Eğer ID'ler NULL değilse ve DovizKurlari tablosunda kayıt yoksa ekleme yapalım
IF @TRY_ID IS NOT NULL AND @USD_ID IS NOT NULL AND @EUR_ID IS NOT NULL AND @GBP_ID IS NOT NULL AND @UZS_ID IS NOT NULL
BEGIN
    IF NOT EXISTS (SELECT 1 FROM DovizKurlari WHERE Aktif = 1 AND SoftDelete = 0)
    BEGIN
        -- USD/TRY
        INSERT INTO DovizKurlari (
            DovizKuruID,
            KaynakParaBirimi,
            KaynakParaBirimiID,
            HedefParaBirimi,
            HedefParaBirimiID,
            KurDegeri,
            AlisFiyati,
            SatisFiyati,
            Tarih,
            Kaynak,
            Aciklama,
            Aktif,
            SoftDelete,
            OlusturmaTarihi
        )
        VALUES (
            NEWID(),
            'USD',
            @USD_ID,
            'TRY',
            @TRY_ID,
            32.5,
            32.4,
            32.6,
            GETDATE(),
            'Manuel',
            'Manuel olarak eklendi',
            1,
            0,
            GETDATE()
        );

        -- EUR/TRY
        INSERT INTO DovizKurlari (
            DovizKuruID,
            KaynakParaBirimi,
            KaynakParaBirimiID,
            HedefParaBirimi,
            HedefParaBirimiID,
            KurDegeri,
            AlisFiyati,
            SatisFiyati,
            Tarih,
            Kaynak,
            Aciklama,
            Aktif,
            SoftDelete,
            OlusturmaTarihi
        )
        VALUES (
            NEWID(),
            'EUR',
            @EUR_ID,
            'TRY',
            @TRY_ID,
            35.1,
            35.0,
            35.2,
            GETDATE(),
            'Manuel',
            'Manuel olarak eklendi',
            1,
            0,
            GETDATE()
        );

        -- GBP/TRY
        INSERT INTO DovizKurlari (
            DovizKuruID,
            KaynakParaBirimi,
            KaynakParaBirimiID,
            HedefParaBirimi,
            HedefParaBirimiID,
            KurDegeri,
            AlisFiyati,
            SatisFiyati,
            Tarih,
            Kaynak,
            Aciklama,
            Aktif,
            SoftDelete,
            OlusturmaTarihi
        )
        VALUES (
            NEWID(),
            'GBP',
            @GBP_ID,
            'TRY',
            @TRY_ID,
            41.6,
            41.5,
            41.7,
            GETDATE(),
            'Manuel',
            'Manuel olarak eklendi',
            1,
            0,
            GETDATE()
        );

        -- UZS/TRY
        INSERT INTO DovizKurlari (
            DovizKuruID,
            KaynakParaBirimi,
            KaynakParaBirimiID,
            HedefParaBirimi,
            HedefParaBirimiID,
            KurDegeri,
            AlisFiyati,
            SatisFiyati,
            Tarih,
            Kaynak,
            Aciklama,
            Aktif,
            SoftDelete,
            OlusturmaTarihi
        )
        VALUES (
            NEWID(),
            'UZS',
            @UZS_ID,
            'TRY',
            @TRY_ID,
            0.00264,
            0.00262,
            0.00266,
            GETDATE(),
            'Manuel',
            'Manuel olarak eklendi',
            1,
            0,
            GETDATE()
        );

        -- EUR/USD
        INSERT INTO DovizKurlari (
            DovizKuruID,
            KaynakParaBirimi,
            KaynakParaBirimiID,
            HedefParaBirimi,
            HedefParaBirimiID,
            KurDegeri,
            AlisFiyati,
            SatisFiyati,
            Tarih,
            Kaynak,
            Aciklama,
            Aktif,
            SoftDelete,
            OlusturmaTarihi
        )
        VALUES (
            NEWID(),
            'EUR',
            @EUR_ID,
            'USD',
            @USD_ID,
            1.08,
            1.07,
            1.09,
            GETDATE(),
            'Manuel',
            'Manuel olarak eklendi',
            1,
            0,
            GETDATE()
        );

        PRINT 'DovizKurlari tablosuna varsayılan kayıtlar eklendi.';
    END
    ELSE
    BEGIN
        PRINT 'DovizKurlari tablosunda zaten aktif kayıtlar bulunmaktadır.';
    END
END
ELSE
BEGIN
    PRINT 'Dovizler tablosunda gerekli kayıtlar bulunamadı. Önce Dovizler tablosunu oluşturun ve gerekli kayıtları ekleyin.';
END 