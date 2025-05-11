using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MuhasebeStokWebApp.Migrations
{
    /// <inheritdoc />
    public partial class CheckFaturaTuruSeed : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Kontrol amaçlı sorgu - hiçbir şeyi değiştirmiyor
            migrationBuilder.Sql(@"
                -- FaturaTurleri tablosundaki verileri kontrol et
                PRINT 'FaturaTurleri tablosundaki kayıtlar:';
                SELECT * FROM FaturaTurleri;
            ");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Boş bir Down metodu, çünkü kontrol amaçlı
        }
    }
}
