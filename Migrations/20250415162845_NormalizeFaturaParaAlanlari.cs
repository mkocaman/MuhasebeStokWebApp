using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MuhasebeStokWebApp.Migrations
{
    /// <inheritdoc />
    public partial class NormalizeFaturaParaAlanlari : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // 🔥 Geliştirme ortamı olduğu için tablo verilerini temizleyelim
            // Tabloda var olup olmadığını kontrol et ve temizle
            migrationBuilder.Sql(@"
                IF OBJECT_ID('FaturaAklamaKuyrugu', 'U') IS NOT NULL
                    DELETE FROM FaturaAklamaKuyrugu;
                IF OBJECT_ID('FaturaOdemeleri', 'U') IS NOT NULL
                    DELETE FROM FaturaOdemeleri;
                IF OBJECT_ID('FaturaDetaylari', 'U') IS NOT NULL
                    DELETE FROM FaturaDetaylari;
                IF OBJECT_ID('Faturalar', 'U') IS NOT NULL
                    DELETE FROM Faturalar;
            ");
            
            // Fatura entity'sinden, doğru tablo/sütun isimlerini aldım
            migrationBuilder.AlterColumn<decimal>(
                name: "AraToplam",
                table: "Faturalar",
                type: "decimal(18,2)",
                nullable: true,
                oldClrType: typeof(decimal),
                oldType: "decimal(18,4)",
                oldNullable: true);
                
            migrationBuilder.AlterColumn<decimal>(
                name: "KDVToplam",  // Fatura.cs dosyasında "KDVToplam" olarak tanımlı
                table: "Faturalar",
                type: "decimal(18,2)",
                nullable: true,
                oldClrType: typeof(decimal),
                oldType: "decimal(18,4)",
                oldNullable: true);
                
            migrationBuilder.AlterColumn<decimal>(
                name: "IndirimTutari",
                table: "Faturalar",
                type: "decimal(18,2)",
                nullable: true,
                oldClrType: typeof(decimal),
                oldType: "decimal(18,4)",
                oldNullable: true);
                
            migrationBuilder.AlterColumn<decimal>(
                name: "GenelToplam",
                table: "Faturalar",
                type: "decimal(18,2)",
                nullable: true,
                oldClrType: typeof(decimal),
                oldType: "decimal(18,4)",
                oldNullable: true);
                
            migrationBuilder.AlterColumn<decimal>(
                name: "AraToplamDoviz",
                table: "Faturalar",
                type: "decimal(18,2)",
                nullable: true,
                oldClrType: typeof(decimal),
                oldType: "decimal(18,4)",
                oldNullable: true);
                
            migrationBuilder.AlterColumn<decimal>(
                name: "KDVToplamDoviz",  // Fatura.cs dosyasında "KDVToplamDoviz" olarak tanımlı
                table: "Faturalar",
                type: "decimal(18,2)",
                nullable: true,
                oldClrType: typeof(decimal),
                oldType: "decimal(18,4)",
                oldNullable: true);
                
            migrationBuilder.AlterColumn<decimal>(
                name: "IndirimTutariDoviz",
                table: "Faturalar",
                type: "decimal(18,2)",
                nullable: true,
                oldClrType: typeof(decimal),
                oldType: "decimal(18,4)",
                oldNullable: true);
                
            migrationBuilder.AlterColumn<decimal>(
                name: "GenelToplamDoviz",
                table: "Faturalar",
                type: "decimal(18,2)",
                nullable: true,
                oldClrType: typeof(decimal),
                oldType: "decimal(18,4)",
                oldNullable: true);
                
            migrationBuilder.AlterColumn<decimal>(
                name: "OdenenTutar",
                table: "Faturalar",
                type: "decimal(18,2)",
                nullable: true,
                oldClrType: typeof(decimal),
                oldType: "decimal(18,4)",
                oldNullable: true);
                
            // FaturaDetaylari tablosunda (DbContext'ten gelen isim)
            migrationBuilder.Sql(@"
                IF OBJECT_ID('FaturaDetaylari', 'U') IS NOT NULL
                BEGIN
                    IF COL_LENGTH('FaturaDetaylari', 'BirimFiyat') IS NOT NULL
                        ALTER TABLE FaturaDetaylari ALTER COLUMN BirimFiyat decimal(18,2);
                    
                    IF COL_LENGTH('FaturaDetaylari', 'BirimFiyatDoviz') IS NOT NULL
                        ALTER TABLE FaturaDetaylari ALTER COLUMN BirimFiyatDoviz decimal(18,2);
                    
                    IF COL_LENGTH('FaturaDetaylari', 'Tutar') IS NOT NULL
                        ALTER TABLE FaturaDetaylari ALTER COLUMN Tutar decimal(18,2);
                    
                    IF COL_LENGTH('FaturaDetaylari', 'NetTutar') IS NOT NULL
                        ALTER TABLE FaturaDetaylari ALTER COLUMN NetTutar decimal(18,2);
                END
            ");
                
            // FaturaOdemeleri tablosunda
            migrationBuilder.Sql(@"
                IF OBJECT_ID('FaturaOdemeleri', 'U') IS NOT NULL
                BEGIN
                    IF COL_LENGTH('FaturaOdemeleri', 'OdemeTutari') IS NOT NULL
                        ALTER TABLE FaturaOdemeleri ALTER COLUMN OdemeTutari decimal(18,2);
                END
            ");
                
            // FaturaAklamaKuyrugu tablosunda (DbContext'ten gelen isim)
            migrationBuilder.Sql(@"
                IF OBJECT_ID('FaturaAklamaKuyrugu', 'U') IS NOT NULL
                BEGIN
                    IF COL_LENGTH('FaturaAklamaKuyrugu', 'AklananTutar') IS NOT NULL
                        ALTER TABLE FaturaAklamaKuyrugu ALTER COLUMN AklananTutar decimal(18,2);
                    
                    IF COL_LENGTH('FaturaAklamaKuyrugu', 'AklananTutarDoviz') IS NOT NULL
                        ALTER TABLE FaturaAklamaKuyrugu ALTER COLUMN AklananTutarDoviz decimal(18,2);
                END
            ");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Down metoduna gerek yok, çünkü geliştirme ortamında çalışıyoruz ve veri kritik değil.
        }
    }
}
