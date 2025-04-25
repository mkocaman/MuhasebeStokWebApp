using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MuhasebeStokWebApp.Migrations
{
    /// <inheritdoc />
    public partial class AddPropertyForStokCikisDetay : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<decimal>(
                name: "BirimMaliyet",
                table: "StokCikisDetaylari",
                type: "decimal(18,6)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<string>(
                name: "HareketTipi",
                table: "StokCikisDetaylari",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "ParaBirimi",
                table: "StokCikisDetaylari",
                type: "nvarchar(10)",
                maxLength: 10,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<decimal>(
                name: "ToplamMaliyet",
                table: "StokCikisDetaylari",
                type: "decimal(18,6)",
                nullable: false,
                defaultValue: 0m);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "BirimMaliyet",
                table: "StokCikisDetaylari");

            migrationBuilder.DropColumn(
                name: "HareketTipi",
                table: "StokCikisDetaylari");

            migrationBuilder.DropColumn(
                name: "ParaBirimi",
                table: "StokCikisDetaylari");

            migrationBuilder.DropColumn(
                name: "ToplamMaliyet",
                table: "StokCikisDetaylari");
        }
    }
}
