using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MuhasebeStokWebApp.Migrations
{
    /// <inheritdoc />
    public partial class BankaHesapHareketModelFix : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "HedefBankaHesapID",
                table: "BankaHesapHareketleri",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_BankaHesapHareketleri_HedefBankaHesapID",
                table: "BankaHesapHareketleri",
                column: "HedefBankaHesapID");

            migrationBuilder.AddForeignKey(
                name: "FK_BankaHesapHareketleri_BankaHesaplari_HedefBankaHesapID",
                table: "BankaHesapHareketleri",
                column: "HedefBankaHesapID",
                principalTable: "BankaHesaplari",
                principalColumn: "BankaHesapID");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_BankaHesapHareketleri_BankaHesaplari_HedefBankaHesapID",
                table: "BankaHesapHareketleri");

            migrationBuilder.DropIndex(
                name: "IX_BankaHesapHareketleri_HedefBankaHesapID",
                table: "BankaHesapHareketleri");

            migrationBuilder.DropColumn(
                name: "HedefBankaHesapID",
                table: "BankaHesapHareketleri");
        }
    }
}
