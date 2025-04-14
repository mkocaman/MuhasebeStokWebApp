using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MuhasebeStokWebApp.Migrations
{
    /// <inheritdoc />
    public partial class AddStokFifoAndStokCikisDetay : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "StokCikisDetaylari",
                columns: table => new
                {
                    StokCikisDetayID = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    StokFifoID = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CikisMiktari = table.Column<decimal>(type: "decimal(18,6)", nullable: false),
                    BirimFiyat = table.Column<decimal>(type: "decimal(18,6)", nullable: false),
                    BirimFiyatUSD = table.Column<decimal>(type: "decimal(18,6)", nullable: false),
                    BirimFiyatTL = table.Column<decimal>(type: "decimal(18,6)", nullable: false),
                    BirimFiyatUZS = table.Column<decimal>(type: "decimal(18,6)", nullable: false),
                    ToplamMaliyetUSD = table.Column<decimal>(type: "decimal(18,6)", nullable: false),
                    ReferansNo = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    ReferansTuru = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    ReferansID = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CikisTarihi = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Aciklama = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    OlusturmaTarihi = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Iptal = table.Column<bool>(type: "bit", nullable: false),
                    IptalTarihi = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IptalAciklama = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_StokCikisDetaylari", x => x.StokCikisDetayID);
                    table.ForeignKey(
                        name: "FK_StokCikisDetaylari_StokFifoKayitlari_StokFifoID",
                        column: x => x.StokFifoID,
                        principalTable: "StokFifoKayitlari",
                        principalColumn: "StokFifoID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_StokCikisDetaylari_StokFifoID",
                table: "StokCikisDetaylari",
                column: "StokFifoID");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "StokCikisDetaylari");
        }
    }
}
