using Microsoft.EntityFrameworkCore.Migrations;

namespace MuhasebeStokWebApp.Data.Migrations
{
    public partial class DovizKuruGuncelleme : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // DovizKurlari tablosundaki eski sütunları kaldırma
            migrationBuilder.DropColumn(
                name: "DovizKodu",
                table: "DovizKurlari");

            migrationBuilder.DropColumn(
                name: "DovizAdi",
                table: "DovizKurlari");

            migrationBuilder.DropColumn(
                name: "EfektifAlisFiyati",
                table: "DovizKurlari");

            migrationBuilder.DropColumn(
                name: "EfektifSatisFiyati",
                table: "DovizKurlari");

            migrationBuilder.DropColumn(
                name: "KaynakParaBirimi",
                table: "DovizKurlari");

            migrationBuilder.DropColumn(
                name: "HedefParaBirimi",
                table: "DovizKurlari");

            // DovizKuruID sütununu int tipine dönüştürme
            migrationBuilder.AlterColumn<int>(
                name: "DovizKuruID",
                table: "DovizKurlari",
                type: "int",
                nullable: false,
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier")
                .Annotation("SqlServer:Identity", "1, 1");

            // Yeni sütunları ekleme
            migrationBuilder.AddColumn<string>(
                name: "ParaBirimi",
                table: "DovizKurlari",
                type: "nvarchar(10)",
                maxLength: 10,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "BazParaBirimi",
                table: "DovizKurlari",
                type: "nvarchar(10)",
                maxLength: 10,
                nullable: false,
                defaultValue: "USD");

            migrationBuilder.AddColumn<decimal>(
                name: "Kur",
                table: "DovizKurlari",
                type: "decimal(18,6)",
                nullable: false,
                defaultValue: 1m);

            // Index oluşturma
            migrationBuilder.CreateIndex(
                name: "IX_DovizKurlari_ParaBirimi_BazParaBirimi_Tarih",
                table: "DovizKurlari",
                columns: new[] { "ParaBirimi", "BazParaBirimi", "Tarih" });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Eklenen sütunları kaldırma
            migrationBuilder.DropIndex(
                name: "IX_DovizKurlari_ParaBirimi_BazParaBirimi_Tarih",
                table: "DovizKurlari");

            migrationBuilder.DropColumn(
                name: "ParaBirimi",
                table: "DovizKurlari");

            migrationBuilder.DropColumn(
                name: "BazParaBirimi",
                table: "DovizKurlari");

            migrationBuilder.DropColumn(
                name: "Kur",
                table: "DovizKurlari");

            // DovizKuruID sütununu Guid tipine geri dönüştürme
            migrationBuilder.AlterColumn<Guid>(
                name: "DovizKuruID",
                table: "DovizKurlari",
                type: "uniqueidentifier",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int")
                .OldAnnotation("SqlServer:Identity", "1, 1");

            // Eski sütunları geri ekleme
            migrationBuilder.AddColumn<string>(
                name: "DovizKodu",
                table: "DovizKurlari",
                type: "nvarchar(10)",
                maxLength: 10,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "DovizAdi",
                table: "DovizKurlari",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<decimal>(
                name: "EfektifAlisFiyati",
                table: "DovizKurlari",
                type: "decimal(18,6)",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "EfektifSatisFiyati",
                table: "DovizKurlari",
                type: "decimal(18,6)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "KaynakParaBirimi",
                table: "DovizKurlari",
                type: "nvarchar(10)",
                maxLength: 10,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "HedefParaBirimi",
                table: "DovizKurlari",
                type: "nvarchar(10)",
                maxLength: 10,
                nullable: true);
        }
    }
} 