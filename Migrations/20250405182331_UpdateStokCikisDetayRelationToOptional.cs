using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MuhasebeStokWebApp.Migrations
{
    /// <inheritdoc />
    public partial class UpdateStokCikisDetayRelationToOptional : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_StokCikisDetaylari_StokFifoKayitlari_StokFifoID",
                table: "StokCikisDetaylari");

            migrationBuilder.AlterColumn<Guid>(
                name: "StokFifoID",
                table: "StokCikisDetaylari",
                type: "uniqueidentifier",
                nullable: true,
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier");

            migrationBuilder.AddForeignKey(
                name: "FK_StokCikisDetaylari_StokFifoKayitlari_StokFifoID",
                table: "StokCikisDetaylari",
                column: "StokFifoID",
                principalTable: "StokFifoKayitlari",
                principalColumn: "StokFifoID",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_StokCikisDetaylari_StokFifoKayitlari_StokFifoID",
                table: "StokCikisDetaylari");

            migrationBuilder.AlterColumn<Guid>(
                name: "StokFifoID",
                table: "StokCikisDetaylari",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"),
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier",
                oldNullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_StokCikisDetaylari_StokFifoKayitlari_StokFifoID",
                table: "StokCikisDetaylari",
                column: "StokFifoID",
                principalTable: "StokFifoKayitlari",
                principalColumn: "StokFifoID",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
