using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MuhasebeStokWebApp.Migrations
{
    /// <inheritdoc />
    public partial class AddCariParaBirimiSupport : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "VarsayilanKurKullan",
                table: "Cariler",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<Guid>(
                name: "VarsayilanParaBirimiId",
                table: "Cariler",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Cariler_VarsayilanParaBirimiId",
                table: "Cariler",
                column: "VarsayilanParaBirimiId");

            migrationBuilder.AddForeignKey(
                name: "FK_Cariler_ParaBirimleri_VarsayilanParaBirimiId",
                table: "Cariler",
                column: "VarsayilanParaBirimiId",
                principalTable: "ParaBirimleri",
                principalColumn: "ParaBirimiID");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Cariler_ParaBirimleri_VarsayilanParaBirimiId",
                table: "Cariler");

            migrationBuilder.DropIndex(
                name: "IX_Cariler_VarsayilanParaBirimiId",
                table: "Cariler");

            migrationBuilder.DropColumn(
                name: "VarsayilanKurKullan",
                table: "Cariler");

            migrationBuilder.DropColumn(
                name: "VarsayilanParaBirimiId",
                table: "Cariler");
        }
    }
}
