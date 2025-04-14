using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MuhasebeStokWebApp.Migrations
{
    /// <inheritdoc />
    public partial class UpdateSistemLogWithKullaniciIDRelation : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_CariHareketler_Cariler_CariId",
                table: "CariHareketler");

            migrationBuilder.DropForeignKey(
                name: "FK_SistemLoglar_AspNetUsers_ApplicationUserId",
                table: "SistemLoglar");

            migrationBuilder.DropIndex(
                name: "IX_SistemLoglar_ApplicationUserId",
                table: "SistemLoglar");

            migrationBuilder.DropColumn(
                name: "ApplicationUserId",
                table: "SistemLoglar");

            migrationBuilder.RenameColumn(
                name: "KullaniciID",
                table: "SistemLoglar",
                newName: "KullaniciId");

            migrationBuilder.AlterColumn<string>(
                name: "TabloAdi",
                table: "SistemLoglar",
                type: "nvarchar(250)",
                maxLength: 250,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(100)",
                oldMaxLength: 100);

            migrationBuilder.AlterColumn<string>(
                name: "Sayfa",
                table: "SistemLoglar",
                type: "nvarchar(250)",
                maxLength: 250,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(255)",
                oldMaxLength: 255);

            migrationBuilder.AlterColumn<string>(
                name: "Mesaj",
                table: "SistemLoglar",
                type: "nvarchar(2000)",
                maxLength: 2000,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(500)",
                oldMaxLength: 500);

            migrationBuilder.AlterColumn<string>(
                name: "KullaniciId",
                table: "SistemLoglar",
                type: "varchar(128)",
                nullable: true,
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "KullaniciAdi",
                table: "SistemLoglar",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(100)",
                oldMaxLength: 100);

            migrationBuilder.AlterColumn<string>(
                name: "KayitAdi",
                table: "SistemLoglar",
                type: "nvarchar(250)",
                maxLength: 250,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(100)",
                oldMaxLength: 100);

            migrationBuilder.AlterColumn<string>(
                name: "HataMesaji",
                table: "SistemLoglar",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(500)",
                oldMaxLength: 500);

            migrationBuilder.AlterColumn<string>(
                name: "Aciklama",
                table: "SistemLoglar",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(500)",
                oldMaxLength: 500);

            migrationBuilder.AlterColumn<string>(
                name: "VergiNo",
                table: "Cariler",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(11)",
                oldMaxLength: 11,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "CariKodu",
                table: "Cariler",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(50)",
                oldMaxLength: 50);

            migrationBuilder.AddColumn<Guid>(
                name: "CariID1",
                table: "CariHareketler",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_SistemLoglar_KullaniciId",
                table: "SistemLoglar",
                column: "KullaniciId");

            migrationBuilder.CreateIndex(
                name: "IX_CariHareketler_CariID1",
                table: "CariHareketler",
                column: "CariID1");

            migrationBuilder.AddForeignKey(
                name: "FK_CariHareketler_Cariler_CariID1",
                table: "CariHareketler",
                column: "CariID1",
                principalTable: "Cariler",
                principalColumn: "CariID");

            migrationBuilder.AddForeignKey(
                name: "FK_CariHareketler_Cariler_CariId",
                table: "CariHareketler",
                column: "CariId",
                principalTable: "Cariler",
                principalColumn: "CariID");

            migrationBuilder.AddForeignKey(
                name: "FK_SistemLoglar_AspNetUsers_KullaniciId",
                table: "SistemLoglar",
                column: "KullaniciId",
                principalTable: "AspNetUsers",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_CariHareketler_Cariler_CariID1",
                table: "CariHareketler");

            migrationBuilder.DropForeignKey(
                name: "FK_CariHareketler_Cariler_CariId",
                table: "CariHareketler");

            migrationBuilder.DropForeignKey(
                name: "FK_SistemLoglar_AspNetUsers_KullaniciId",
                table: "SistemLoglar");

            migrationBuilder.DropIndex(
                name: "IX_SistemLoglar_KullaniciId",
                table: "SistemLoglar");

            migrationBuilder.DropIndex(
                name: "IX_CariHareketler_CariID1",
                table: "CariHareketler");

            migrationBuilder.DropColumn(
                name: "CariID1",
                table: "CariHareketler");

            migrationBuilder.RenameColumn(
                name: "KullaniciId",
                table: "SistemLoglar",
                newName: "KullaniciID");

            migrationBuilder.AlterColumn<string>(
                name: "TabloAdi",
                table: "SistemLoglar",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(250)",
                oldMaxLength: 250);

            migrationBuilder.AlterColumn<string>(
                name: "Sayfa",
                table: "SistemLoglar",
                type: "nvarchar(255)",
                maxLength: 255,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(250)",
                oldMaxLength: 250);

            migrationBuilder.AlterColumn<string>(
                name: "Mesaj",
                table: "SistemLoglar",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(2000)",
                oldMaxLength: 2000);

            migrationBuilder.AlterColumn<Guid>(
                name: "KullaniciID",
                table: "SistemLoglar",
                type: "uniqueidentifier",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "varchar(128)",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "KullaniciAdi",
                table: "SistemLoglar",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AlterColumn<string>(
                name: "KayitAdi",
                table: "SistemLoglar",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(250)",
                oldMaxLength: 250);

            migrationBuilder.AlterColumn<string>(
                name: "HataMesaji",
                table: "SistemLoglar",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AlterColumn<string>(
                name: "Aciklama",
                table: "SistemLoglar",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AddColumn<string>(
                name: "ApplicationUserId",
                table: "SistemLoglar",
                type: "varchar(128)",
                nullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "VergiNo",
                table: "Cariler",
                type: "nvarchar(11)",
                maxLength: 11,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(20)",
                oldMaxLength: 20,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "CariKodu",
                table: "Cariler",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(20)",
                oldMaxLength: 20);

            migrationBuilder.CreateIndex(
                name: "IX_SistemLoglar_ApplicationUserId",
                table: "SistemLoglar",
                column: "ApplicationUserId");

            migrationBuilder.AddForeignKey(
                name: "FK_CariHareketler_Cariler_CariId",
                table: "CariHareketler",
                column: "CariId",
                principalTable: "Cariler",
                principalColumn: "CariID",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_SistemLoglar_AspNetUsers_ApplicationUserId",
                table: "SistemLoglar",
                column: "ApplicationUserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id");
        }
    }
}
