-- StokFifo tablosunu oluştur
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'StokFifo')
BEGIN
    CREATE TABLE [dbo].[StokFifo](
        [StokFifoID] [uniqueidentifier] NOT NULL,
        [UrunID] [uniqueidentifier] NOT NULL,
        [Miktar] [decimal](18, 2) NOT NULL,
        [KalanMiktar] [decimal](18, 2) NOT NULL,
        [BirimFiyat] [decimal](18, 2) NOT NULL,
        [Birim] [nvarchar](20) NOT NULL,
        [ParaBirimi] [nvarchar](3) NOT NULL DEFAULT 'TRY',
        [DovizKuru] [decimal](18, 6) NOT NULL DEFAULT 1,
        [USDBirimFiyat] [decimal](18, 6) NOT NULL,
        [TLBirimFiyat] [decimal](18, 6) NOT NULL,
        [UZSBirimFiyat] [decimal](18, 6) NOT NULL,
        [GirisTarihi] [datetime2](7) NOT NULL,
        [SonCikisTarihi] [datetime2](7) NULL,
        [ReferansNo] [nvarchar](50) NOT NULL,
        [ReferansTuru] [nvarchar](20) NOT NULL,
        [ReferansID] [uniqueidentifier] NOT NULL,
        [Aciklama] [nvarchar](500) NOT NULL,
        [Aktif] [bit] NOT NULL DEFAULT 1,
        [Iptal] [bit] NOT NULL DEFAULT 0,
        [IptalTarihi] [datetime2](7) NULL,
        [IptalAciklama] [nvarchar](500) NULL,
        [IptalEdenKullaniciID] [uniqueidentifier] NULL,
        [OlusturmaTarihi] [datetime2](7) NOT NULL DEFAULT GETDATE(),
        [GuncellemeTarihi] [datetime2](7) NULL,
        [SoftDelete] [bit] NOT NULL DEFAULT 0,
        CONSTRAINT [PK_StokFifo] PRIMARY KEY CLUSTERED ([StokFifoID] ASC)
    );

    -- Foreign key oluştur
    ALTER TABLE [dbo].[StokFifo] WITH CHECK ADD CONSTRAINT [FK_StokFifo_Urunler_UrunID] 
    FOREIGN KEY([UrunID]) REFERENCES [dbo].[Urunler] ([UrunID]);

    -- İndeksler oluştur
    CREATE INDEX [IX_StokFifo_UrunID] ON [dbo].[StokFifo] ([UrunID]);
    CREATE INDEX [IX_StokFifo_GirisTarihi] ON [dbo].[StokFifo] ([GirisTarihi]);
    CREATE INDEX [IX_StokFifo_Referans] ON [dbo].[StokFifo] ([ReferansID], [ReferansTuru]);
    CREATE INDEX [IX_StokFifo_StokSorgu] ON [dbo].[StokFifo] ([UrunID], [KalanMiktar], [Aktif], [SoftDelete], [Iptal]);

    PRINT 'StokFifo tablosu başarıyla oluşturuldu.';
END
ELSE
BEGIN
    PRINT 'StokFifo tablosu zaten mevcut.';
END 