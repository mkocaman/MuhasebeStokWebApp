using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MuhasebeStokWebApp.Data.Migrations
{
    public partial class SistemAyarlariGuncelleme : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "AktifParaBirimleri",
                table: "SistemAyarlari",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: true,
                defaultValue: "USD,TRY,UZS");

            migrationBuilder.AddColumn<bool>(
                name: "Aktif",
                table: "SistemAyarlari",
                type: "bit",
                nullable: false,
                defaultValue: true);

            migrationBuilder.AddColumn<bool>(
                name: "SoftDelete",
                table: "SistemAyarlari",
                type: "bit",
                nullable: false,
                defaultValue: false);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AktifParaBirimleri",
                table: "SistemAyarlari");

            migrationBuilder.DropColumn(
                name: "Aktif",
                table: "SistemAyarlari");

            migrationBuilder.DropColumn(
                name: "SoftDelete",
                table: "SistemAyarlari");
        }
    }
} 