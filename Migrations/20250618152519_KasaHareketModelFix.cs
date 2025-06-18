using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MuhasebeStokWebApp.Migrations
{
    /// <inheritdoc />
    public partial class KasaHareketModelFix : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_KasaHareketleri_Bankalar_HedefBankaID",
                table: "KasaHareketleri");

            migrationBuilder.DropForeignKey(
                name: "FK_KasaHareketleri_Bankalar_KaynakBankaID",
                table: "KasaHareketleri");

            migrationBuilder.AddForeignKey(
                name: "FK_KasaHareketleri_BankaHesaplari_HedefBankaID",
                table: "KasaHareketleri",
                column: "HedefBankaID",
                principalTable: "BankaHesaplari",
                principalColumn: "BankaHesapID");

            migrationBuilder.AddForeignKey(
                name: "FK_KasaHareketleri_BankaHesaplari_KaynakBankaID",
                table: "KasaHareketleri",
                column: "KaynakBankaID",
                principalTable: "BankaHesaplari",
                principalColumn: "BankaHesapID");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_KasaHareketleri_BankaHesaplari_HedefBankaID",
                table: "KasaHareketleri");

            migrationBuilder.DropForeignKey(
                name: "FK_KasaHareketleri_BankaHesaplari_KaynakBankaID",
                table: "KasaHareketleri");

            migrationBuilder.AddForeignKey(
                name: "FK_KasaHareketleri_Bankalar_HedefBankaID",
                table: "KasaHareketleri",
                column: "HedefBankaID",
                principalTable: "Bankalar",
                principalColumn: "BankaID");

            migrationBuilder.AddForeignKey(
                name: "FK_KasaHareketleri_Bankalar_KaynakBankaID",
                table: "KasaHareketleri",
                column: "KaynakBankaID",
                principalTable: "Bankalar",
                principalColumn: "BankaID");
        }
    }
}
