using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MuhasebeStokWebApp.Migrations
{
    /// <inheritdoc />
    public partial class AddParaBirimiToFatura : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_CariHareketler_Cariler_CariID1",
                table: "CariHareketler");

            migrationBuilder.DropForeignKey(
                name: "FK_CariHareketler_Cariler_CariId",
                table: "CariHareketler");

            migrationBuilder.DropIndex(
                name: "IX_CariHareketler_CariID1",
                table: "CariHareketler");

            migrationBuilder.DropColumn(
                name: "CariID1",
                table: "CariHareketler");

            migrationBuilder.AddColumn<string>(
                name: "ParaBirimi",
                table: "Faturalar",
                type: "nvarchar(10)",
                maxLength: 10,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddForeignKey(
                name: "FK_CariHareketler_Cariler_CariId",
                table: "CariHareketler",
                column: "CariId",
                principalTable: "Cariler",
                principalColumn: "CariID",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_CariHareketler_Cariler_CariId",
                table: "CariHareketler");

            migrationBuilder.DropColumn(
                name: "ParaBirimi",
                table: "Faturalar");

            migrationBuilder.AddColumn<Guid>(
                name: "CariID1",
                table: "CariHareketler",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_CariHareketler_CariID1",
                table: "CariHareketler",
                column: "CariID1");

            migrationBuilder.AddForeignKey(
                name: "FK_CariHareketler_Cariler_CariID1",
                table: "CariHareketler",
                column: "CariID1",
                principalTable: "Cariler",
                principalColumn: "CariID");

            migrationBuilder.AddForeignKey(
                name: "FK_CariHareketler_Cariler_CariId",
                table: "CariHareketler",
                column: "CariId",
                principalTable: "Cariler",
                principalColumn: "CariID");
        }
    }
}
