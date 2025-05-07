using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MuhasebeStokWebApp.Migrations
{
    /// <inheritdoc />
    public partial class StokFifoPropertyUpdates : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IptalAciklama",
                table: "StokFifoKayitlari");

            migrationBuilder.DropColumn(
                name: "IptalEdenKullaniciID",
                table: "StokFifoKayitlari");

            migrationBuilder.DropColumn(
                name: "IptalTarihi",
                table: "StokFifoKayitlari");

            migrationBuilder.DropColumn(
                name: "RowVersion",
                table: "StokFifoKayitlari");

            migrationBuilder.DropColumn(
                name: "TLBirimFiyat",
                table: "StokFifoKayitlari");

            migrationBuilder.RenameColumn(
                name: "UZSBirimFiyat",
                table: "StokFifoKayitlari",
                newName: "BirimFiyatUZS");

            migrationBuilder.RenameColumn(
                name: "USDBirimFiyat",
                table: "StokFifoKayitlari",
                newName: "BirimFiyatUSD");

            migrationBuilder.AlterColumn<Guid>(
                name: "ReferansID",
                table: "StokFifoKayitlari",
                type: "uniqueidentifier",
                nullable: true,
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier");

            migrationBuilder.AlterColumn<string>(
                name: "Aciklama",
                table: "StokFifoKayitlari",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(500)",
                oldMaxLength: 500,
                oldNullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "StokFifoID1",
                table: "StokCikisDetaylari",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "StokFifoCikislar",
                columns: table => new
                {
                    StokFifoCikisID = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    StokFifoID = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ReferansID = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    DetayID = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    ReferansNo = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ReferansTuru = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CikisMiktar = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    CikisTarihi = table.Column<DateTime>(type: "datetime2", nullable: false),
                    USDBirimFiyat = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    UZSBirimFiyat = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    ParaBirimi = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    DovizKuru = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    OlusturmaTarihi = table.Column<DateTime>(type: "datetime2", nullable: false),
                    OlusturanKullaniciId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    Aktif = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_StokFifoCikislar", x => x.StokFifoCikisID);
                    table.ForeignKey(
                        name: "FK_StokFifoCikislar_StokFifoKayitlari_StokFifoID",
                        column: x => x.StokFifoID,
                        principalTable: "StokFifoKayitlari",
                        principalColumn: "StokFifoID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_StokCikisDetaylari_StokFifoID1",
                table: "StokCikisDetaylari",
                column: "StokFifoID1");

            migrationBuilder.CreateIndex(
                name: "IX_StokFifoCikislar_StokFifoID",
                table: "StokFifoCikislar",
                column: "StokFifoID");

            migrationBuilder.AddForeignKey(
                name: "FK_StokCikisDetaylari_StokFifoKayitlari_StokFifoID1",
                table: "StokCikisDetaylari",
                column: "StokFifoID1",
                principalTable: "StokFifoKayitlari",
                principalColumn: "StokFifoID");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_StokCikisDetaylari_StokFifoKayitlari_StokFifoID1",
                table: "StokCikisDetaylari");

            migrationBuilder.DropTable(
                name: "StokFifoCikislar");

            migrationBuilder.DropIndex(
                name: "IX_StokCikisDetaylari_StokFifoID1",
                table: "StokCikisDetaylari");

            migrationBuilder.DropColumn(
                name: "StokFifoID1",
                table: "StokCikisDetaylari");

            migrationBuilder.RenameColumn(
                name: "BirimFiyatUZS",
                table: "StokFifoKayitlari",
                newName: "UZSBirimFiyat");

            migrationBuilder.RenameColumn(
                name: "BirimFiyatUSD",
                table: "StokFifoKayitlari",
                newName: "USDBirimFiyat");

            migrationBuilder.AlterColumn<Guid>(
                name: "ReferansID",
                table: "StokFifoKayitlari",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"),
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Aciklama",
                table: "StokFifoKayitlari",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(500)",
                oldMaxLength: 500);

            migrationBuilder.AddColumn<string>(
                name: "IptalAciklama",
                table: "StokFifoKayitlari",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "IptalEdenKullaniciID",
                table: "StokFifoKayitlari",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "IptalTarihi",
                table: "StokFifoKayitlari",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<byte[]>(
                name: "RowVersion",
                table: "StokFifoKayitlari",
                type: "rowversion",
                rowVersion: true,
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "TLBirimFiyat",
                table: "StokFifoKayitlari",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);
        }
    }
}
