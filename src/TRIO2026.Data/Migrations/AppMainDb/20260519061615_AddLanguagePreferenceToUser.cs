using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TRIO2026.Data.Migrations.AppMainDb
{
    /// <inheritdoc />
    public partial class AddLanguagePreferenceToUser : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "LanguagePreference",
                table: "User",
                type: "TEXT",
                maxLength: 16,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "LanguagePreference",
                table: "User");
        }
    }
}
