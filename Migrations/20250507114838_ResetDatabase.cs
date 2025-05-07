using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MuhasebeStokWebApp.Migrations
{
    /// <inheritdoc />
    public partial class ResetDatabase : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_StokCikisDetaylari_StokFifoKayitlari_StokFifoID1",
                table: "StokCikisDetaylari");

            migrationBuilder.DropForeignKey(
                name: "FK_StokFifoCikislar_StokFifoKayitlari_StokFifoID",
                table: "StokFifoCikislar");

            migrationBuilder.DropIndex(
                name: "IX_StokCikisDetaylari_StokFifoID1",
                table: "StokCikisDetaylari");

            migrationBuilder.DropColumn(
                name: "StokFifoID1",
                table: "StokCikisDetaylari");

            migrationBuilder.RenameColumn(
                name: "UZSBirimFiyat",
                table: "StokFifoCikislar",
                newName: "BirimFiyatUZS");

            migrationBuilder.RenameColumn(
                name: "USDBirimFiyat",
                table: "StokFifoCikislar",
                newName: "BirimFiyatUSD");

            migrationBuilder.AlterColumn<Guid>(
                name: "StokFifoID",
                table: "StokFifoCikislar",
                type: "uniqueidentifier",
                nullable: true,
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier");

            migrationBuilder.AlterColumn<decimal>(
                name: "DovizKuru",
                table: "StokFifoCikislar",
                type: "decimal(18,6)",
                nullable: true,
                oldClrType: typeof(decimal),
                oldType: "decimal(18,2)",
                oldNullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_StokFifoCikislar_StokFifoKayitlari_StokFifoID",
                table: "StokFifoCikislar",
                column: "StokFifoID",
                principalTable: "StokFifoKayitlari",
                principalColumn: "StokFifoID",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_StokFifoCikislar_StokFifoKayitlari_StokFifoID",
                table: "StokFifoCikislar");

            migrationBuilder.RenameColumn(
                name: "BirimFiyatUZS",
                table: "StokFifoCikislar",
                newName: "UZSBirimFiyat");

            migrationBuilder.RenameColumn(
                name: "BirimFiyatUSD",
                table: "StokFifoCikislar",
                newName: "USDBirimFiyat");

            migrationBuilder.AlterColumn<Guid>(
                name: "StokFifoID",
                table: "StokFifoCikislar",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"),
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier",
                oldNullable: true);

            migrationBuilder.AlterColumn<decimal>(
                name: "DovizKuru",
                table: "StokFifoCikislar",
                type: "decimal(18,2)",
                nullable: true,
                oldClrType: typeof(decimal),
                oldType: "decimal(18,6)",
                oldNullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "StokFifoID1",
                table: "StokCikisDetaylari",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_StokCikisDetaylari_StokFifoID1",
                table: "StokCikisDetaylari",
                column: "StokFifoID1");

            migrationBuilder.AddForeignKey(
                name: "FK_StokCikisDetaylari_StokFifoKayitlari_StokFifoID1",
                table: "StokCikisDetaylari",
                column: "StokFifoID1",
                principalTable: "StokFifoKayitlari",
                principalColumn: "StokFifoID");

            migrationBuilder.AddForeignKey(
                name: "FK_StokFifoCikislar_StokFifoKayitlari_StokFifoID",
                table: "StokFifoCikislar",
                column: "StokFifoID",
                principalTable: "StokFifoKayitlari",
                principalColumn: "StokFifoID",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
