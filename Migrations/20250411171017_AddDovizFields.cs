using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MuhasebeStokWebApp.Migrations
{
    /// <inheritdoc />
    public partial class AddDovizFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<decimal>(
                name: "AraToplamDoviz",
                table: "Faturalar",
                type: "decimal(18,2)",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "GenelToplamDoviz",
                table: "Faturalar",
                type: "decimal(18,2)",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "IndirimTutariDoviz",
                table: "Faturalar",
                type: "decimal(18,2)",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "KDVToplamDoviz",
                table: "Faturalar",
                type: "decimal(18,2)",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "BirimFiyatDoviz",
                table: "FaturaDetaylari",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "IndirimTutariDoviz",
                table: "FaturaDetaylari",
                type: "decimal(18,2)",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "KdvTutariDoviz",
                table: "FaturaDetaylari",
                type: "decimal(18,2)",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "NetTutarDoviz",
                table: "FaturaDetaylari",
                type: "decimal(18,2)",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "TutarDoviz",
                table: "FaturaDetaylari",
                type: "decimal(18,2)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AraToplamDoviz",
                table: "Faturalar");

            migrationBuilder.DropColumn(
                name: "GenelToplamDoviz",
                table: "Faturalar");

            migrationBuilder.DropColumn(
                name: "IndirimTutariDoviz",
                table: "Faturalar");

            migrationBuilder.DropColumn(
                name: "KDVToplamDoviz",
                table: "Faturalar");

            migrationBuilder.DropColumn(
                name: "BirimFiyatDoviz",
                table: "FaturaDetaylari");

            migrationBuilder.DropColumn(
                name: "IndirimTutariDoviz",
                table: "FaturaDetaylari");

            migrationBuilder.DropColumn(
                name: "KdvTutariDoviz",
                table: "FaturaDetaylari");

            migrationBuilder.DropColumn(
                name: "NetTutarDoviz",
                table: "FaturaDetaylari");

            migrationBuilder.DropColumn(
                name: "TutarDoviz",
                table: "FaturaDetaylari");
        }
    }
}
