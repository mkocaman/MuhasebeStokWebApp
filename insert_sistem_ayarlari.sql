-- SistemAyarlari tablosuna varsayılan kayıt ekle
IF NOT EXISTS (SELECT 1 FROM SistemAyarlari WHERE Aktif = 1 AND SoftDelete = 0)
BEGIN
    INSERT INTO SistemAyarlari (
        SistemAyarlariID,
        AnaDovizKodu,
        SirketAdi,
        SirketAdresi,
        SirketTelefon,
        SirketEmail,
        SirketVergiNo,
        SirketVergiDairesi,
        OtomatikDovizGuncelleme,
        DovizGuncellemeSikligi,
        SonDovizGuncellemeTarihi,
        AktifParaBirimleri,
        Aktif,
        SoftDelete,
        OlusturmaTarihi,
        GuncellemeTarihi
    )
    VALUES (
        NEWID(),
        'USD',
        'Muhasebe Stok Web App',
        'İstanbul, Türkiye',
        '+90 212 123 4567',
        'info@muhasebe-stok.com',
        '1234567890',
        'İstanbul Vergi Dairesi',
        1,
        24,
        GETDATE(),
        'USD,EUR,TRY,GBP,UZS',
        1,
        0,
        GETDATE(),
        NULL
    )
END 