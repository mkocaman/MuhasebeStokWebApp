using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MuhasebeStokWebApp.Migrations
{
    /// <inheritdoc />
    public partial class SozlesmeCariRestrictRelation : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Sozlesmeler_Cariler_CariID",
                table: "Sozlesmeler");

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
                name: "FK_Sozlesmeler_Cariler_CariID",
                table: "Sozlesmeler");

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