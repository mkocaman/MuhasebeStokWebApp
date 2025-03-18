using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MuhasebeStokWebApp.Data.Migrations
{
    public partial class DovizKuruEkleme : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "DovizKurlari",
                columns: table => new
                {
                    DovizKuruID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ParaBirimi = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    BazParaBirimi = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    Kur = table.Column<decimal>(type: "decimal(18,6)", nullable: false),
                    AlisFiyati = table.Column<decimal>(type: "decimal(18,6)", nullable: true),
                    SatisFiyati = table.Column<decimal>(type: "decimal(18,6)", nullable: true),
                    Tarih = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Kaynak = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Aciklama = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    Aktif = table.Column<bool>(type: "bit", nullable: false),
                    SoftDelete = table.Column<bool>(type: "bit", nullable: false),
                    OlusturmaTarihi = table.Column<DateTime>(type: "datetime2", nullable: false),
                    GuncellemeTarihi = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DovizKurlari", x => x.DovizKuruID);
                });

            migrationBuilder.CreateIndex(
                name: "IX_DovizKurlari_ParaBirimi_BazParaBirimi_Tarih",
                table: "DovizKurlari",
                columns: new[] { "ParaBirimi", "BazParaBirimi", "Tarih" });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "DovizKurlari");
        }
    }
} 