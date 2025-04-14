using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MuhasebeStokWebApp.Migrations
{
    /// <inheritdoc />
    public partial class SetDeleteAndRestoreDeletedMethods : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<decimal>(
                name: "DovizliListeFiyati",
                table: "Urunler",
                type: "decimal(18,2)",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "DovizliMaliyetFiyati",
                table: "Urunler",
                type: "decimal(18,2)",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "DovizliSatisFiyati",
                table: "Urunler",
                type: "decimal(18,2)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DovizliListeFiyati",
                table: "Urunler");

            migrationBuilder.DropColumn(
                name: "DovizliMaliyetFiyati",
                table: "Urunler");

            migrationBuilder.DropColumn(
                name: "DovizliSatisFiyati",
                table: "Urunler");
        }
    }
}
