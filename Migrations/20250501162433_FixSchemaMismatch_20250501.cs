using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MuhasebeStokWebApp.Migrations
{
    /// <inheritdoc />
    public partial class FixSchemaMismatch_20250501 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Faturalar_Cariler_CariID",
                table: "Faturalar");

            migrationBuilder.DropForeignKey(
                name: "FK_Sozlesmeler_Cariler_CariID",
                table: "Sozlesmeler");

            migrationBuilder.AddColumn<Guid>(
                name: "DepoID",
                table: "IrsaliyeDetaylari",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Irsaliyeler_DepoID",
                table: "Irsaliyeler",
                column: "DepoID");

            migrationBuilder.CreateIndex(
                name: "IX_IrsaliyeDetaylari_DepoID",
                table: "IrsaliyeDetaylari",
                column: "DepoID");

            migrationBuilder.AddForeignKey(
                name: "FK_Faturalar_Cariler_CariID",
                table: "Faturalar",
                column: "CariID",
                principalTable: "Cariler",
                principalColumn: "CariID",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_IrsaliyeDetaylari_Depolar_DepoID",
                table: "IrsaliyeDetaylari",
                column: "DepoID",
                principalTable: "Depolar",
                principalColumn: "DepoID");

            migrationBuilder.AddForeignKey(
                name: "FK_Irsaliyeler_Depolar_DepoID",
                table: "Irsaliyeler",
                column: "DepoID",
                principalTable: "Depolar",
                principalColumn: "DepoID");

            migrationBuilder.AddForeignKey(
                name: "FK_Sozlesmeler_Cariler_CariID",
                table: "Sozlesmeler",
                column: "CariID",
                principalTable: "Cariler",
                principalColumn: "CariID",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Faturalar_Cariler_CariID",
                table: "Faturalar");

            migrationBuilder.DropForeignKey(
                name: "FK_IrsaliyeDetaylari_Depolar_DepoID",
                table: "IrsaliyeDetaylari");

            migrationBuilder.DropForeignKey(
                name: "FK_Irsaliyeler_Depolar_DepoID",
                table: "Irsaliyeler");

            migrationBuilder.DropForeignKey(
                name: "FK_Sozlesmeler_Cariler_CariID",
                table: "Sozlesmeler");

            migrationBuilder.DropIndex(
                name: "IX_Irsaliyeler_DepoID",
                table: "Irsaliyeler");

            migrationBuilder.DropIndex(
                name: "IX_IrsaliyeDetaylari_DepoID",
                table: "IrsaliyeDetaylari");

            migrationBuilder.DropColumn(
                name: "DepoID",
                table: "IrsaliyeDetaylari");

            migrationBuilder.AddForeignKey(
                name: "FK_Faturalar_Cariler_CariID",
                table: "Faturalar",
                column: "CariID",
                principalTable: "Cariler",
                principalColumn: "CariID");

            migrationBuilder.AddForeignKey(
                name: "FK_Sozlesmeler_Cariler_CariID",
                table: "Sozlesmeler",
                column: "CariID",
                principalTable: "Cariler",
                principalColumn: "CariID",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
