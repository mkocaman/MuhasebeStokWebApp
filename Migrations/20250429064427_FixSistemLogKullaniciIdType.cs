using Microsoft.EntityFrameworkCore.Migrations;
using System; // Add this if Guid is not recognized

#nullable disable

namespace MuhasebeStokWebApp.Migrations
{
    /// <inheritdoc />
    public partial class FixSistemLogKullaniciIdType : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Alter the column type from Guid? (uniqueidentifier) to string? (nvarchar(450))
            migrationBuilder.AlterColumn<string>(
                name: "KullaniciId",
                table: "SistemLoglar", // Ensure this is the correct table name
                type: "nvarchar(450)", // Standard type for IdentityUser keys
                nullable: true,        // Keep it nullable as in your entity
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier", // Specify the old SQL type
                oldNullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Revert the column type back to Guid? (uniqueidentifier)
            migrationBuilder.AlterColumn<Guid>(
                name: "KullaniciId",
                table: "SistemLoglar",
                type: "uniqueidentifier",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(450)",
                oldNullable: true);
        }
    }
}
