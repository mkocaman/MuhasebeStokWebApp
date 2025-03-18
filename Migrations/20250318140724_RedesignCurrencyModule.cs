using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MuhasebeStokWebApp.Migrations
{
    /// <inheritdoc />
    public partial class RedesignCurrencyModule : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_DovizKurlari_Dovizler_HedefParaBirimiID",
                table: "DovizKurlari");

            migrationBuilder.DropForeignKey(
                name: "FK_DovizKurlari_Dovizler_KaynakParaBirimiID",
                table: "DovizKurlari");

            migrationBuilder.DropIndex(
                name: "IX_DovizKurlari_HedefParaBirimiID",
                table: "DovizKurlari");

            migrationBuilder.DropColumn(
                name: "HedefParaBirimi",
                table: "DovizKurlari");

            migrationBuilder.DropColumn(
                name: "HedefParaBirimiID",
                table: "DovizKurlari");

            migrationBuilder.DropColumn(
                name: "KaynakParaBirimi",
                table: "DovizKurlari");

            migrationBuilder.RenameColumn(
                name: "KaynakParaBirimiID",
                table: "DovizKurlari",
                newName: "ParaBirimiID");

            migrationBuilder.RenameIndex(
                name: "IX_DovizKurlari_KaynakParaBirimiID",
                table: "DovizKurlari",
                newName: "IX_DovizKurlari_ParaBirimiID");

            migrationBuilder.AlterColumn<string>(
                name: "Sembol",
                table: "Dovizler",
                type: "nvarchar(10)",
                maxLength: 10,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(10)",
                oldMaxLength: 10);

            migrationBuilder.AlterColumn<string>(
                name: "DovizKodu",
                table: "Dovizler",
                type: "nvarchar(3)",
                maxLength: 3,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(10)",
                oldMaxLength: 10);

            migrationBuilder.AddColumn<Guid>(
                name: "DovizIliskiID",
                table: "DovizKurlari",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "DovizIliskileri",
                columns: table => new
                {
                    DovizIliskiID = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    KaynakParaBirimiID = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    HedefParaBirimiID = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Aktif = table.Column<bool>(type: "bit", nullable: false),
                    SoftDelete = table.Column<bool>(type: "bit", nullable: false),
                    OlusturmaTarihi = table.Column<DateTime>(type: "datetime2", nullable: false),
                    GuncellemeTarihi = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ParaBirimiID = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    ParaBirimiID1 = table.Column<Guid>(type: "uniqueidentifier", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DovizIliskileri", x => x.DovizIliskiID);
                    table.ForeignKey(
                        name: "FK_DovizIliskileri_Dovizler_HedefParaBirimiID",
                        column: x => x.HedefParaBirimiID,
                        principalTable: "Dovizler",
                        principalColumn: "DovizID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_DovizIliskileri_Dovizler_KaynakParaBirimiID",
                        column: x => x.KaynakParaBirimiID,
                        principalTable: "Dovizler",
                        principalColumn: "DovizID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_DovizIliskileri_Dovizler_ParaBirimiID",
                        column: x => x.ParaBirimiID,
                        principalTable: "Dovizler",
                        principalColumn: "DovizID");
                    table.ForeignKey(
                        name: "FK_DovizIliskileri_Dovizler_ParaBirimiID1",
                        column: x => x.ParaBirimiID1,
                        principalTable: "Dovizler",
                        principalColumn: "DovizID");
                });

            migrationBuilder.CreateIndex(
                name: "IX_DovizKurlari_DovizIliskiID",
                table: "DovizKurlari",
                column: "DovizIliskiID");

            migrationBuilder.CreateIndex(
                name: "IX_DovizIliskileri_HedefParaBirimiID",
                table: "DovizIliskileri",
                column: "HedefParaBirimiID");

            migrationBuilder.CreateIndex(
                name: "IX_DovizIliskileri_KaynakParaBirimiID",
                table: "DovizIliskileri",
                column: "KaynakParaBirimiID");

            migrationBuilder.CreateIndex(
                name: "IX_DovizIliskileri_ParaBirimiID",
                table: "DovizIliskileri",
                column: "ParaBirimiID");

            migrationBuilder.CreateIndex(
                name: "IX_DovizIliskileri_ParaBirimiID1",
                table: "DovizIliskileri",
                column: "ParaBirimiID1");

            migrationBuilder.AddForeignKey(
                name: "FK_DovizKurlari_DovizIliskileri_DovizIliskiID",
                table: "DovizKurlari",
                column: "DovizIliskiID",
                principalTable: "DovizIliskileri",
                principalColumn: "DovizIliskiID");

            migrationBuilder.AddForeignKey(
                name: "FK_DovizKurlari_Dovizler_ParaBirimiID",
                table: "DovizKurlari",
                column: "ParaBirimiID",
                principalTable: "Dovizler",
                principalColumn: "DovizID",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_DovizKurlari_DovizIliskileri_DovizIliskiID",
                table: "DovizKurlari");

            migrationBuilder.DropForeignKey(
                name: "FK_DovizKurlari_Dovizler_ParaBirimiID",
                table: "DovizKurlari");

            migrationBuilder.DropTable(
                name: "DovizIliskileri");

            migrationBuilder.DropIndex(
                name: "IX_DovizKurlari_DovizIliskiID",
                table: "DovizKurlari");

            migrationBuilder.DropColumn(
                name: "DovizIliskiID",
                table: "DovizKurlari");

            migrationBuilder.RenameColumn(
                name: "ParaBirimiID",
                table: "DovizKurlari",
                newName: "KaynakParaBirimiID");

            migrationBuilder.RenameIndex(
                name: "IX_DovizKurlari_ParaBirimiID",
                table: "DovizKurlari",
                newName: "IX_DovizKurlari_KaynakParaBirimiID");

            migrationBuilder.AlterColumn<string>(
                name: "Sembol",
                table: "Dovizler",
                type: "nvarchar(10)",
                maxLength: 10,
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(10)",
                oldMaxLength: 10,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "DovizKodu",
                table: "Dovizler",
                type: "nvarchar(10)",
                maxLength: 10,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(3)",
                oldMaxLength: 3);

            migrationBuilder.AddColumn<string>(
                name: "HedefParaBirimi",
                table: "DovizKurlari",
                type: "nvarchar(3)",
                maxLength: 3,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<Guid>(
                name: "HedefParaBirimiID",
                table: "DovizKurlari",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<string>(
                name: "KaynakParaBirimi",
                table: "DovizKurlari",
                type: "nvarchar(3)",
                maxLength: 3,
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateIndex(
                name: "IX_DovizKurlari_HedefParaBirimiID",
                table: "DovizKurlari",
                column: "HedefParaBirimiID");

            migrationBuilder.AddForeignKey(
                name: "FK_DovizKurlari_Dovizler_HedefParaBirimiID",
                table: "DovizKurlari",
                column: "HedefParaBirimiID",
                principalTable: "Dovizler",
                principalColumn: "DovizID",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_DovizKurlari_Dovizler_KaynakParaBirimiID",
                table: "DovizKurlari",
                column: "KaynakParaBirimiID",
                principalTable: "Dovizler",
                principalColumn: "DovizID",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
