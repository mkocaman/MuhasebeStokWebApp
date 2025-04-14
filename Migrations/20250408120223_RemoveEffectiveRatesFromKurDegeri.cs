using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MuhasebeStokWebApp.Migrations
{
    /// <inheritdoc />
    public partial class RemoveEffectiveRatesFromKurDegeri : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_KurDegerleri_ParaBirimleri_ParaBirimiID",
                table: "KurDegerleri");

            migrationBuilder.DropIndex(
                name: "IX_KurDegeri_ParaBirimiID_Tarih",
                table: "KurDegerleri");

            migrationBuilder.DropColumn(
                name: "Efektif_Alis",
                table: "KurDegerleri");

            migrationBuilder.DropColumn(
                name: "Efektif_Satis",
                table: "KurDegerleri");

            migrationBuilder.AlterColumn<bool>(
                name: "Aktif",
                table: "KurDegerleri",
                type: "bit",
                nullable: false,
                oldClrType: typeof(bool),
                oldType: "bit",
                oldDefaultValue: true);

            migrationBuilder.CreateTable(
                name: "KurMarjlari",
                columns: table => new
                {
                    KurMarjID = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    SatisMarji = table.Column<decimal>(type: "decimal(5,2)", nullable: false),
                    Varsayilan = table.Column<bool>(type: "bit", nullable: false),
                    Tanim = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Aktif = table.Column<bool>(type: "bit", nullable: false),
                    Silindi = table.Column<bool>(type: "bit", nullable: false),
                    OlusturmaTarihi = table.Column<DateTime>(type: "datetime2", nullable: false),
                    GuncellemeTarihi = table.Column<DateTime>(type: "datetime2", nullable: true),
                    OlusturanKullaniciID = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    SonGuncelleyenKullaniciID = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_KurMarjlari", x => x.KurMarjID);
                });

            migrationBuilder.CreateIndex(
                name: "IX_KurDegerleri_ParaBirimiID",
                table: "KurDegerleri",
                column: "ParaBirimiID");

            migrationBuilder.AddForeignKey(
                name: "FK_KurDegerleri_ParaBirimleri_ParaBirimiID",
                table: "KurDegerleri",
                column: "ParaBirimiID",
                principalTable: "ParaBirimleri",
                principalColumn: "ParaBirimiID");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_KurDegerleri_ParaBirimleri_ParaBirimiID",
                table: "KurDegerleri");

            migrationBuilder.DropTable(
                name: "KurMarjlari");

            migrationBuilder.DropIndex(
                name: "IX_KurDegerleri_ParaBirimiID",
                table: "KurDegerleri");

            migrationBuilder.AlterColumn<bool>(
                name: "Aktif",
                table: "KurDegerleri",
                type: "bit",
                nullable: false,
                defaultValue: true,
                oldClrType: typeof(bool),
                oldType: "bit");

            migrationBuilder.AddColumn<decimal>(
                name: "Efektif_Alis",
                table: "KurDegerleri",
                type: "decimal(18,6)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "Efektif_Satis",
                table: "KurDegerleri",
                type: "decimal(18,6)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.CreateIndex(
                name: "IX_KurDegeri_ParaBirimiID_Tarih",
                table: "KurDegerleri",
                columns: new[] { "ParaBirimiID", "Tarih" });

            migrationBuilder.AddForeignKey(
                name: "FK_KurDegerleri_ParaBirimleri_ParaBirimiID",
                table: "KurDegerleri",
                column: "ParaBirimiID",
                principalTable: "ParaBirimleri",
                principalColumn: "ParaBirimiID",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
