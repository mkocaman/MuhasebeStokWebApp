using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MuhasebeStokWebApp.Migrations
{
    /// <inheritdoc />
    public partial class EfektifKurlarEklendi : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<decimal>(
                name: "EfektifAlis",
                table: "KurDegerleri",
                type: "decimal(18,6)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "EfektifSatis",
                table: "KurDegerleri",
                type: "decimal(18,6)",
                nullable: false,
                defaultValue: 0m);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "EfektifAlis",
                table: "KurDegerleri");

            migrationBuilder.DropColumn(
                name: "EfektifSatis",
                table: "KurDegerleri");
        }
    }
}
