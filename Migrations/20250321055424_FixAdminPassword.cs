using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MuhasebeStokWebApp.Migrations
{
    /// <inheritdoc />
    public partial class FixAdminPassword : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Menuler",
                columns: table => new
                {
                    MenuID = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Ad = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Icon = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Url = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    Controller = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    Action = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    AktifMi = table.Column<bool>(type: "bit", nullable: false),
                    Sira = table.Column<int>(type: "int", nullable: false),
                    UstMenuID = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    OlusturmaTarihi = table.Column<DateTime>(type: "datetime2", nullable: false),
                    GuncellemeTarihi = table.Column<DateTime>(type: "datetime2", nullable: true),
                    OlusturanKullaniciID = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    SonGuncelleyenKullaniciID = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    Silindi = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Menuler", x => x.MenuID);
                    table.ForeignKey(
                        name: "FK_Menuler_Menuler_UstMenuID",
                        column: x => x.UstMenuID,
                        principalTable: "Menuler",
                        principalColumn: "MenuID");
                });

            migrationBuilder.CreateTable(
                name: "MenuRoller",
                columns: table => new
                {
                    MenuRolID = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    MenuID = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    RolID = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: false),
                    OlusturmaTarihi = table.Column<DateTime>(type: "datetime2", nullable: false),
                    OlusturanKullaniciID = table.Column<Guid>(type: "uniqueidentifier", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MenuRoller", x => x.MenuRolID);
                    table.ForeignKey(
                        name: "FK_MenuRoller_Menuler_MenuID",
                        column: x => x.MenuID,
                        principalTable: "Menuler",
                        principalColumn: "MenuID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Menuler_UstMenuID",
                table: "Menuler",
                column: "UstMenuID");

            migrationBuilder.CreateIndex(
                name: "IX_MenuRoller_MenuID",
                table: "MenuRoller",
                column: "MenuID");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "MenuRoller");

            migrationBuilder.DropTable(
                name: "Menuler");
        }
    }
}
