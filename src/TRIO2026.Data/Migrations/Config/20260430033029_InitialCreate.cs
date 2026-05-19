using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TRIO2026.Data.Migrations.Config
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "CommandDefinition",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false),
                    Name = table.Column<string>(type: "TEXT", nullable: false),
                    Arg0Type = table.Column<int>(type: "INTEGER", nullable: false, defaultValue: 0),
                    Arg0Label = table.Column<string>(type: "TEXT", nullable: true),
                    Arg0Options = table.Column<string>(type: "TEXT", nullable: true),
                    Arg1Type = table.Column<int>(type: "INTEGER", nullable: false, defaultValue: 0),
                    Arg1Label = table.Column<string>(type: "TEXT", nullable: true),
                    Arg1Options = table.Column<string>(type: "TEXT", nullable: true),
                    Arg2Type = table.Column<int>(type: "INTEGER", nullable: false, defaultValue: 0),
                    Arg2Label = table.Column<string>(type: "TEXT", nullable: true),
                    Arg2Options = table.Column<string>(type: "TEXT", nullable: true),
                    Arg3Type = table.Column<int>(type: "INTEGER", nullable: false, defaultValue: 0),
                    Arg3Label = table.Column<string>(type: "TEXT", nullable: true),
                    Arg3Options = table.Column<string>(type: "TEXT", nullable: true),
                    Arg4Type = table.Column<int>(type: "INTEGER", nullable: false, defaultValue: 0),
                    Arg4Label = table.Column<string>(type: "TEXT", nullable: true),
                    Arg4Options = table.Column<string>(type: "TEXT", nullable: true),
                    Note = table.Column<string>(type: "TEXT", nullable: true),
                    DisplayFormat = table.Column<string>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CommandDefinition", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "SystemConfig",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Category = table.Column<string>(type: "TEXT", nullable: false),
                    Key = table.Column<string>(type: "TEXT", nullable: false),
                    Value = table.Column<string>(type: "TEXT", nullable: true),
                    DataType = table.Column<string>(type: "TEXT", nullable: false, defaultValue: "string"),
                    Description = table.Column<string>(type: "TEXT", nullable: true),
                    ModifiedAt = table.Column<string>(type: "TEXT", nullable: false),
                    ModifiedBy = table.Column<string>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SystemConfig", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_SystemConfig_Category_Key",
                table: "SystemConfig",
                columns: new[] { "Category", "Key" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "CommandDefinition");

            migrationBuilder.DropTable(
                name: "SystemConfig");
        }
    }
}
