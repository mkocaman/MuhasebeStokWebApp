using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MuhasebeStokWebApp.Migrations
{
    /// <inheritdoc />
    public partial class FixCariHareketAndStokHareket : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<decimal>(
                name: "BirimFiyatUSD",
                table: "StokHareketleri",
                type: "decimal(18,6)",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "BirimFiyatUZS",
                table: "StokHareketleri",
                type: "decimal(18,6)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ParaBirimi",
                table: "CariHareketler",
                type: "nvarchar(10)",
                maxLength: 10,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<decimal>(
                name: "TutarDoviz",
                table: "CariHareketler",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "BirimFiyatUSD",
                table: "StokHareketleri");

            migrationBuilder.DropColumn(
                name: "BirimFiyatUZS",
                table: "StokHareketleri");

            migrationBuilder.DropColumn(
                name: "ParaBirimi",
                table: "CariHareketler");

            migrationBuilder.DropColumn(
                name: "TutarDoviz",
                table: "CariHareketler");
        }
    }
}
