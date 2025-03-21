-- Faturalar için eksik sütunlar ekleniyor
IF EXISTS (SELECT 1 FROM sys.tables WHERE name = 'Faturalar')
BEGIN
    IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE name = 'AraToplam' AND object_id = OBJECT_ID('Faturalar'))
    BEGIN
        ALTER TABLE Faturalar ADD AraToplam decimal(18,2) NOT NULL DEFAULT 0
    END

    IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE name = 'DovizKuru' AND object_id = OBJECT_ID('Faturalar'))
    BEGIN
        ALTER TABLE Faturalar ADD DovizKuru decimal(18,6) NULL
    END

    IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE name = 'DovizTuru' AND object_id = OBJECT_ID('Faturalar'))
    BEGIN
        ALTER TABLE Faturalar ADD DovizTuru nvarchar(10) NULL
    END

    IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE name = 'FaturaNotu' AND object_id = OBJECT_ID('Faturalar'))
    BEGIN
        ALTER TABLE Faturalar ADD FaturaNotu nvarchar(500) NULL
    END

    IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE name = 'FaturaNumarasi' AND object_id = OBJECT_ID('Faturalar'))
    BEGIN
        ALTER TABLE Faturalar ADD FaturaNumarasi nvarchar(50) NOT NULL DEFAULT ''
    END

    IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE name = 'GenelToplam' AND object_id = OBJECT_ID('Faturalar'))
    BEGIN
        ALTER TABLE Faturalar ADD GenelToplam decimal(18,2) NOT NULL DEFAULT 0
    END

    IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE name = 'KDVToplam' AND object_id = OBJECT_ID('Faturalar'))
    BEGIN
        ALTER TABLE Faturalar ADD KDVToplam decimal(18,2) NOT NULL DEFAULT 0
    END

    IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE name = 'OdemeDurumu' AND object_id = OBJECT_ID('Faturalar'))
    BEGIN
        ALTER TABLE Faturalar ADD OdemeDurumu nvarchar(50) NOT NULL DEFAULT 'Bekliyor'
    END

    IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE name = 'OdemeTuruID' AND object_id = OBJECT_ID('Faturalar'))
    BEGIN
        ALTER TABLE Faturalar ADD OdemeTuruID int NULL
    END

    IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE name = 'SiparisNumarasi' AND object_id = OBJECT_ID('Faturalar'))
    BEGIN
        ALTER TABLE Faturalar ADD SiparisNumarasi nvarchar(50) NULL
    END

    IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE name = 'VadeTarihi' AND object_id = OBJECT_ID('Faturalar'))
    BEGIN
        ALTER TABLE Faturalar ADD VadeTarihi datetime2 NULL
    END
END

-- Irsaliyeler için eksik sütunlar ekleniyor
IF EXISTS (SELECT 1 FROM sys.tables WHERE name = 'Irsaliyeler')
BEGIN
    IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE name = 'Durum' AND object_id = OBJECT_ID('Irsaliyeler'))
    BEGIN
        ALTER TABLE Irsaliyeler ADD Durum nvarchar(50) NOT NULL DEFAULT 'Açık'
    END

    IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE name = 'FaturaID' AND object_id = OBJECT_ID('Irsaliyeler'))
    BEGIN
        ALTER TABLE Irsaliyeler ADD FaturaID uniqueidentifier NULL
    END

    IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE name = 'IrsaliyeNumarasi' AND object_id = OBJECT_ID('Irsaliyeler'))
    BEGIN
        ALTER TABLE Irsaliyeler ADD IrsaliyeNumarasi nvarchar(50) NOT NULL DEFAULT ''
    END

    IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE name = 'IrsaliyeTuru' AND object_id = OBJECT_ID('Irsaliyeler'))
    BEGIN
        ALTER TABLE Irsaliyeler ADD IrsaliyeTuru nvarchar(50) NOT NULL DEFAULT 'Standart'
    END
END

-- Eksik Faturalar tablosu oluşturuluyor
IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'Faturalar')
BEGIN
    CREATE TABLE Faturalar (
        FaturaID uniqueidentifier NOT NULL PRIMARY KEY,
        CariID uniqueidentifier NOT NULL,
        FaturaTarihi datetime2 NOT NULL,
        AraToplam decimal(18,2) NOT NULL DEFAULT 0,
        KDVToplam decimal(18,2) NOT NULL DEFAULT 0,
        GenelToplam decimal(18,2) NOT NULL DEFAULT 0,
        FaturaNumarasi nvarchar(50) NOT NULL,
        Aciklama nvarchar(500) NULL,
        FaturaTuruID int NULL,
        OdemeTuruID int NULL,
        OdemeDurumu nvarchar(50) NOT NULL DEFAULT 'Bekliyor',
        FaturaNotu nvarchar(500) NULL,
        VadeTarihi datetime2 NULL,
        DovizKuru decimal(18,6) NULL,
        DovizTuru nvarchar(10) NULL,
        SiparisNumarasi nvarchar(50) NULL,
        Aktif bit NOT NULL DEFAULT 1,
        OlusturmaTarihi datetime2 NOT NULL DEFAULT GETDATE(),
        GuncellemeTarihi datetime2 NULL,
        SoftDelete bit NOT NULL DEFAULT 0
    )
END

-- Eksik Irsaliyeler tablosu oluşturuluyor
IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'Irsaliyeler')
BEGIN
    CREATE TABLE Irsaliyeler (
        IrsaliyeID uniqueidentifier NOT NULL PRIMARY KEY,
        CariID uniqueidentifier NOT NULL,
        IrsaliyeTarihi datetime2 NOT NULL,
        IrsaliyeNumarasi nvarchar(50) NOT NULL,
        IrsaliyeTuru nvarchar(50) NOT NULL DEFAULT 'Standart',
        Durum nvarchar(50) NOT NULL DEFAULT 'Açık',
        FaturaID uniqueidentifier NULL,
        Aciklama nvarchar(500) NULL,
        Resmi bit NULL DEFAULT 1,
        Aktif bit NOT NULL DEFAULT 1,
        OlusturmaTarihi datetime2 NOT NULL DEFAULT GETDATE(),
        GuncellemeTarihi datetime2 NULL,
        SoftDelete bit NOT NULL DEFAULT 0
    )
END

-- Eksik CariHareketler tablosu oluşturuluyor
IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'CariHareketler')
BEGIN
    CREATE TABLE CariHareketler (
        CariHareketID uniqueidentifier NOT NULL PRIMARY KEY,
        CariID uniqueidentifier NOT NULL,
        Tutar decimal(18,2) NOT NULL,
        HareketTuru nvarchar(50) NOT NULL,
        Tarih datetime2 NOT NULL,
        ReferansNo nvarchar(50) NULL,
        ReferansTuru nvarchar(50) NULL,
        ReferansID uniqueidentifier NULL,
        Aciklama nvarchar(500) NULL,
        IslemYapanKullaniciID uniqueidentifier NULL,
        SonGuncelleyenKullaniciID uniqueidentifier NULL,
        OlusturmaTarihi datetime2 NOT NULL DEFAULT GETDATE(),
        GuncellemeTarihi datetime2 NULL,
        SoftDelete bit NOT NULL DEFAULT 0
    )
END

-- Eksik Fatura Detayları tablosu oluşturuluyor
IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'FaturaDetaylari')
BEGIN
    CREATE TABLE FaturaDetaylari (
        FaturaDetayID uniqueidentifier NOT NULL PRIMARY KEY,
        FaturaID uniqueidentifier NOT NULL,
        UrunID uniqueidentifier NOT NULL,
        Miktar decimal(18,2) NOT NULL,
        BirimID uniqueidentifier NULL,
        BirimFiyat decimal(18,4) NOT NULL,
        KdvOrani decimal(5,2) NOT NULL DEFAULT 18,
        KdvTutari decimal(18,2) NOT NULL DEFAULT 0,
        IndirimOrani decimal(5,2) NULL DEFAULT 0,
        IndirimTutari decimal(18,2) NULL DEFAULT 0,
        NetTutar decimal(18,2) NOT NULL DEFAULT 0,
        Aciklama nvarchar(250) NULL,
        SatirToplam decimal(18,2) NOT NULL DEFAULT 0,
        SatirKdvToplam decimal(18,2) NOT NULL DEFAULT 0,
        Tutar decimal(18,2) NOT NULL DEFAULT 0,
        Aktif bit NOT NULL DEFAULT 1,
        OlusturmaTarihi datetime2 NOT NULL DEFAULT GETDATE(),
        GuncellemeTarihi datetime2 NULL,
        SoftDelete bit NOT NULL DEFAULT 0
    )
END

-- Eksik İrsaliye Detayları tablosu oluşturuluyor
IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'IrsaliyeDetaylari')
BEGIN
    CREATE TABLE IrsaliyeDetaylari (
        IrsaliyeDetayID uniqueidentifier NOT NULL PRIMARY KEY,
        IrsaliyeID uniqueidentifier NOT NULL,
        UrunID uniqueidentifier NOT NULL,
        Miktar decimal(18,2) NOT NULL DEFAULT 0,
        BirimID uniqueidentifier NULL,
        Birim nvarchar(20) NULL,
        Aciklama nvarchar(250) NULL,
        SatirToplam decimal(18,2) NOT NULL DEFAULT 0,
        SatirKdvToplam decimal(18,2) NOT NULL DEFAULT 0,
        Aktif bit NOT NULL DEFAULT 1,
        OlusturmaTarihi datetime2 NOT NULL DEFAULT GETDATE(),
        GuncellemeTarihi datetime2 NULL,
        SoftDelete bit NOT NULL DEFAULT 0
    )
END

-- Oluşturulan tablolarda ilişkilendirmeleri ekliyoruz
IF EXISTS (SELECT 1 FROM sys.tables WHERE name = 'Faturalar') AND EXISTS (SELECT 1 FROM sys.tables WHERE name = 'Cariler')
BEGIN
    IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = 'FK_Faturalar_Cariler_CariID')
    BEGIN
        ALTER TABLE Faturalar ADD CONSTRAINT FK_Faturalar_Cariler_CariID FOREIGN KEY (CariID) REFERENCES Cariler(CariID)
    END
END

IF EXISTS (SELECT 1 FROM sys.tables WHERE name = 'FaturaDetaylari') AND EXISTS (SELECT 1 FROM sys.tables WHERE name = 'Faturalar')
BEGIN
    IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = 'FK_FaturaDetaylari_Faturalar_FaturaID')
    BEGIN
        ALTER TABLE FaturaDetaylari ADD CONSTRAINT FK_FaturaDetaylari_Faturalar_FaturaID FOREIGN KEY (FaturaID) REFERENCES Faturalar(FaturaID) ON DELETE CASCADE
    END
END

IF EXISTS (SELECT 1 FROM sys.tables WHERE name = 'Irsaliyeler') AND EXISTS (SELECT 1 FROM sys.tables WHERE name = 'Cariler')
BEGIN
    IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = 'FK_Irsaliyeler_Cariler_CariID')
    BEGIN
        ALTER TABLE Irsaliyeler ADD CONSTRAINT FK_Irsaliyeler_Cariler_CariID FOREIGN KEY (CariID) REFERENCES Cariler(CariID)
    END
END

IF EXISTS (SELECT 1 FROM sys.tables WHERE name = 'IrsaliyeDetaylari') AND EXISTS (SELECT 1 FROM sys.tables WHERE name = 'Irsaliyeler')
BEGIN
    IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = 'FK_IrsaliyeDetaylari_Irsaliyeler_IrsaliyeID')
    BEGIN
        ALTER TABLE IrsaliyeDetaylari ADD CONSTRAINT FK_IrsaliyeDetaylari_Irsaliyeler_IrsaliyeID FOREIGN KEY (IrsaliyeID) REFERENCES Irsaliyeler(IrsaliyeID) ON DELETE CASCADE
    END
END

IF EXISTS (SELECT 1 FROM sys.tables WHERE name = 'CariHareketler') AND EXISTS (SELECT 1 FROM sys.tables WHERE name = 'Cariler')
BEGIN
    IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = 'FK_CariHareketler_Cariler_CariID')
    BEGIN
        ALTER TABLE CariHareketler ADD CONSTRAINT FK_CariHareketler_Cariler_CariID FOREIGN KEY (CariID) REFERENCES Cariler(CariID) ON DELETE CASCADE
    END
END 