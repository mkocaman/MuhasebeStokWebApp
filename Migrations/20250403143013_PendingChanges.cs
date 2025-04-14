using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MuhasebeStokWebApp.Migrations
{
    /// <inheritdoc />
    public partial class PendingChanges : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Aciklama",
                table: "Bankalar");

            migrationBuilder.DropColumn(
                name: "AcilisBakiye",
                table: "Bankalar");

            migrationBuilder.DropColumn(
                name: "GuncelBakiye",
                table: "Bankalar");

            migrationBuilder.DropColumn(
                name: "HesapNo",
                table: "Bankalar");

            migrationBuilder.DropColumn(
                name: "IBAN",
                table: "Bankalar");

            migrationBuilder.DropColumn(
                name: "ParaBirimi",
                table: "Bankalar");

            migrationBuilder.DropColumn(
                name: "SubeAdi",
                table: "Bankalar");

            migrationBuilder.DropColumn(
                name: "SubeKodu",
                table: "Bankalar");

            migrationBuilder.DropColumn(
                name: "YetkiliKullaniciID",
                table: "Bankalar");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Aciklama",
                table: "Bankalar",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "AcilisBakiye",
                table: "Bankalar",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "GuncelBakiye",
                table: "Bankalar",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<string>(
                name: "HesapNo",
                table: "Bankalar",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "IBAN",
                table: "Bankalar",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ParaBirimi",
                table: "Bankalar",
                type: "nvarchar(10)",
                maxLength: 10,
                nullable: true,
                defaultValue: "TRY");

            migrationBuilder.AddColumn<string>(
                name: "SubeAdi",
                table: "Bankalar",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "SubeKodu",
                table: "Bankalar",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "YetkiliKullaniciID",
                table: "Bankalar",
                type: "uniqueidentifier",
                nullable: true);
        }
    }
}
