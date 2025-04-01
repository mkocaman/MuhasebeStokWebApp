using Microsoft.EntityFrameworkCore.Migrations;

namespace MuhasebeStokWebApp.Data.Migrations
{
    public partial class UpdatedCariHareket : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // CariHareket tablosundaki CariID sütununu güncelleme
            migrationBuilder.AlterColumn<int>(
                name: "CariID",
                table: "CariHareketler",
                nullable: false,
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier");

            // Foreign key ilişkisini güncelleme
            migrationBuilder.AddForeignKey(
                name: "FK_CariHareketler_Cariler_CariId",
                table: "CariHareketler",
                column: "CariId",
                principalTable: "Cariler",
                principalColumn: "CariID",
                onDelete: ReferentialAction.Cascade);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Geri alma işlemi
            migrationBuilder.DropForeignKey(
                name: "FK_CariHareketler_Cariler_CariId",
                table: "CariHareketler");
                
            migrationBuilder.AlterColumn<System.Guid>(
                name: "CariID",
                table: "CariHareketler",
                type: "uniqueidentifier",
                nullable: false,
                oldClrType: typeof(int));
        }
    }
} 