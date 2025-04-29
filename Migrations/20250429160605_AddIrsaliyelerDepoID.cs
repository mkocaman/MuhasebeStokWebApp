using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MuhasebeStokWebApp.Migrations
{
    /// <inheritdoc />
    public partial class AddIrsaliyelerDepoID : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "DepoID",
                table: "Irsaliyeler",
                type: "uniqueidentifier",
                nullable: true);

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
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_IrsaliyeDetaylari_Depolar_DepoID",
                table: "IrsaliyeDetaylari");

            migrationBuilder.DropForeignKey(
                name: "FK_Irsaliyeler_Depolar_DepoID",
                table: "Irsaliyeler");

            migrationBuilder.DropIndex(
                name: "IX_Irsaliyeler_DepoID",
                table: "Irsaliyeler");

            migrationBuilder.DropIndex(
                name: "IX_IrsaliyeDetaylari_DepoID",
                table: "IrsaliyeDetaylari");

            migrationBuilder.DropColumn(
                name: "DepoID",
                table: "Irsaliyeler");

            migrationBuilder.DropColumn(
                name: "DepoID",
                table: "IrsaliyeDetaylari");
        }
    }
}
