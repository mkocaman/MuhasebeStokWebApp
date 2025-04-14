using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MuhasebeStokWebApp.Migrations
{
    /// <inheritdoc />
    public partial class FixParaBirimiAdiIssue : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ParaBirimiAdi",
                table: "ParaBirimleri");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ParaBirimiAdi",
                table: "ParaBirimleri",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");
        }
    }
}
