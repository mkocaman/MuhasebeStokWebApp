-- SistemAyarlari tablosunda eksik sütunları ekleyen komut
IF OBJECT_ID('SistemAyarlari', 'U') IS NOT NULL
BEGIN
    -- Eksik sütunları ekle
    IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('SistemAyarlari') AND name = 'Aktif')
    BEGIN
        ALTER TABLE SistemAyarlari ADD Aktif bit NOT NULL DEFAULT 1;
        PRINT 'SistemAyarlari tablosuna Aktif sütunu eklendi.';
    END

    IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('SistemAyarlari') AND name = 'AktifParaBirimleri')
    BEGIN
        ALTER TABLE SistemAyarlari ADD AktifParaBirimleri nvarchar(MAX) NULL;
        PRINT 'SistemAyarlari tablosuna AktifParaBirimleri sütunu eklendi.';
    END

    IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('SistemAyarlari') AND name = 'OlusturmaTarihi')
    BEGIN
        ALTER TABLE SistemAyarlari ADD OlusturmaTarihi datetime2 NOT NULL DEFAULT GETDATE();
        PRINT 'SistemAyarlari tablosuna OlusturmaTarihi sütunu eklendi.';
    END

    IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('SistemAyarlari') AND name = 'SoftDelete')
    BEGIN
        ALTER TABLE SistemAyarlari ADD SoftDelete bit NOT NULL DEFAULT 0;
        PRINT 'SistemAyarlari tablosuna SoftDelete sütunu eklendi.';
    END
END

-- Kullanıcılara gösterilmesi için para birimi seçim bilgileri
-- Bu değerleri Kur Değeri Ekle sayfasından gönderilen formda şöyle kullanabilirsiniz:

/*
POST verilerinde şunları kullanın (Türk Lirası için USD <-> TRY kuru eklemek için):

KaynakParaBirimiID: D6699DE7-60FF-4717-BDEF-39826FD47877
HedefParaBirimiID: 11632399-CDCC-46FA-9AB6-72957FBB84AF
KurDegeri: 36.5
AlisFiyati: 36.3
SatisFiyati: 36.7
*/

-- Her para birimi için ID bilgilerini kaydedin
-- USD ID: D6699DE7-60FF-4717-BDEF-39826FD47877
-- TRY ID: 11632399-CDCC-46FA-9AB6-72957FBB84AF
-- EUR ID: 9C7D3279-BCBE-489D-BA5A-B759C46F25DD 
-- GBP ID: 95D5AFFA-CDB2-43ED-AB4B-D334EEB689D8
-- UZS ID: 55ADAEEA-37A9-4F45-BD46-15060942D538

-- Para Birimi seçimi için frontend dropdown'u
/*
<select id="KaynakParaBirimiID" name="KaynakParaBirimiID" class="form-control">
    <option value="D6699DE7-60FF-4717-BDEF-39826FD47877">USD - Amerikan Doları</option>
    <option value="11632399-CDCC-46FA-9AB6-72957FBB84AF">TRY - Türk Lirası</option>
    <option value="9C7D3279-BCBE-489D-BA5A-B759C46F25DD">EUR - Euro</option>
    <option value="95D5AFFA-CDB2-43ED-AB4B-D334EEB689D8">GBP - İngiliz Sterlini</option>
    <option value="55ADAEEA-37A9-4F45-BD46-15060942D538">UZS - Özbekistan Somu</option>
</select>

<select id="HedefParaBirimiID" name="HedefParaBirimiID" class="form-control">
    <option value="D6699DE7-60FF-4717-BDEF-39826FD47877">USD - Amerikan Doları</option>
    <option value="11632399-CDCC-46FA-9AB6-72957FBB84AF">TRY - Türk Lirası</option>
    <option value="9C7D3279-BCBE-489D-BA5A-B759C46F25DD">EUR - Euro</option>
    <option value="95D5AFFA-CDB2-43ED-AB4B-D334EEB689D8">GBP - İngiliz Sterlini</option>
    <option value="55ADAEEA-37A9-4F45-BD46-15060942D538">UZS - Özbekistan Somu</option>
</select>
*/ 