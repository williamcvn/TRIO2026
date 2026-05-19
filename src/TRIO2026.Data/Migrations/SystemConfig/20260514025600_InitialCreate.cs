using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TRIO2026.Data.Migrations.SystemConfig
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "LocalizedString",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Module = table.Column<string>(type: "TEXT", nullable: false),
                    ResourceKey = table.Column<string>(type: "TEXT", nullable: false),
                    LanguageCode = table.Column<string>(type: "TEXT", nullable: false, defaultValue: "en"),
                    Value = table.Column<string>(type: "TEXT", nullable: false),
                    Description = table.Column<string>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LocalizedString", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "UvTimerOption",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    DurationSeconds = table.Column<int>(type: "INTEGER", nullable: false),
                    DisplayLabel = table.Column<string>(type: "TEXT", nullable: false),
                    IsEnabled = table.Column<int>(type: "INTEGER", nullable: false, defaultValue: 1),
                    IsDefault = table.Column<int>(type: "INTEGER", nullable: false, defaultValue: 0),
                    SortOrder = table.Column<int>(type: "INTEGER", nullable: false, defaultValue: 0),
                    Description = table.Column<string>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UvTimerOption", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_LocalizedString_LanguageCode",
                table: "LocalizedString",
                column: "LanguageCode");

            migrationBuilder.CreateIndex(
                name: "IX_LocalizedString_Module_LanguageCode",
                table: "LocalizedString",
                columns: new[] { "Module", "LanguageCode" });

            migrationBuilder.CreateIndex(
                name: "IX_LocalizedString_Module_ResourceKey_LanguageCode",
                table: "LocalizedString",
                columns: new[] { "Module", "ResourceKey", "LanguageCode" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_UvTimerOption_DurationSeconds",
                table: "UvTimerOption",
                column: "DurationSeconds",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "LocalizedString");

            migrationBuilder.DropTable(
                name: "UvTimerOption");
        }
    }
}
