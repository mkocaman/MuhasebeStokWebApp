using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace MuhasebeStokWebApp.Migrations
{
    /// <inheritdoc />
    public partial class AddFaturaTuruSeedData : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<bool>(
                name: "Silindi",
                table: "FaturaTurleri",
                type: "bit",
                nullable: false,
                defaultValue: false,
                oldClrType: typeof(bool),
                oldType: "bit");

            // Kayıtların önceden var olup olmadığını kontrol et
            migrationBuilder.Sql(@"
                SET IDENTITY_INSERT FaturaTurleri ON;

                IF NOT EXISTS(SELECT 1 FROM FaturaTurleri WHERE FaturaTuruID = 1)
                BEGIN
                    INSERT INTO FaturaTurleri (FaturaTuruID, FaturaTuruAdi, HareketTuru, Silindi)
                    VALUES (1, N'Satış Faturası', N'Çıkış', 0)
                END
                ELSE
                BEGIN
                    UPDATE FaturaTurleri
                    SET FaturaTuruAdi = N'Satış Faturası', HareketTuru = N'Çıkış', Silindi = 0
                    WHERE FaturaTuruID = 1
                END

                IF NOT EXISTS(SELECT 1 FROM FaturaTurleri WHERE FaturaTuruID = 2)
                BEGIN
                    INSERT INTO FaturaTurleri (FaturaTuruID, FaturaTuruAdi, HareketTuru, Silindi)
                    VALUES (2, N'Alış Faturası', N'Giriş', 0)
                END
                ELSE
                BEGIN
                    UPDATE FaturaTurleri
                    SET FaturaTuruAdi = N'Alış Faturası', HareketTuru = N'Giriş', Silindi = 0
                    WHERE FaturaTuruID = 2
                END

                SET IDENTITY_INSERT FaturaTurleri OFF;
            ");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Down metodu değişmeyecek, çünkü kayıtları silmek yerine güncelledik
            migrationBuilder.AlterColumn<bool>(
                name: "Silindi",
                table: "FaturaTurleri",
                type: "bit",
                nullable: false,
                oldClrType: typeof(bool),
                oldType: "bit",
                oldDefaultValue: false);
        }
    }
}
