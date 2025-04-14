using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MuhasebeStokWebApp.Migrations
{
    /// <inheritdoc />
    public partial class KurDegeriPropertyleriniDuzelt : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Efektif_Satis",
                table: "KurDegerleri",
                newName: "EfektifSatis");

            migrationBuilder.RenameColumn(
                name: "Efektif_Alis",
                table: "KurDegerleri",
                newName: "EfektifAlis");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "EfektifSatis",
                table: "KurDegerleri",
                newName: "Efektif_Satis");

            migrationBuilder.RenameColumn(
                name: "EfektifAlis",
                table: "KurDegerleri",
                newName: "Efektif_Alis");
        }
    }
}
