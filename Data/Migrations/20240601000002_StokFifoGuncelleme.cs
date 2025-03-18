using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MuhasebeStokWebApp.Data.Migrations
{
    public partial class StokFifoGuncelleme : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ParaBirimi",
                table: "StokFifo",
                type: "nvarchar(10)",
                maxLength: 10,
                nullable: true,
                defaultValue: "TRY");

            migrationBuilder.AddColumn<decimal>(
                name: "DovizKuru",
                table: "StokFifo",
                type: "decimal(18,6)",
                nullable: false,
                defaultValue: 1m);

            migrationBuilder.AddColumn<bool>(
                name: "Iptal",
                table: "StokFifo",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "IptalAciklama",
                table: "StokFifo",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "IptalEdenKullaniciID",
                table: "StokFifo",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "IptalTarihi",
                table: "StokFifo",
                type: "datetime2",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_StokFifo_UrunID_Iptal_KalanMiktar",
                table: "StokFifo",
                columns: new[] { "UrunID", "Iptal", "KalanMiktar" });

            migrationBuilder.CreateIndex(
                name: "IX_StokFifo_ReferansID_ReferansTuru",
                table: "StokFifo",
                columns: new[] { "ReferansID", "ReferansTuru" });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_StokFifo_UrunID_Iptal_KalanMiktar",
                table: "StokFifo");

            migrationBuilder.DropIndex(
                name: "IX_StokFifo_ReferansID_ReferansTuru",
                table: "StokFifo");

            migrationBuilder.DropColumn(
                name: "ParaBirimi",
                table: "StokFifo");

            migrationBuilder.DropColumn(
                name: "DovizKuru",
                table: "StokFifo");

            migrationBuilder.DropColumn(
                name: "Iptal",
                table: "StokFifo");

            migrationBuilder.DropColumn(
                name: "IptalAciklama",
                table: "StokFifo");

            migrationBuilder.DropColumn(
                name: "IptalEdenKullaniciID",
                table: "StokFifo");

            migrationBuilder.DropColumn(
                name: "IptalTarihi",
                table: "StokFifo");
        }
    }
} 