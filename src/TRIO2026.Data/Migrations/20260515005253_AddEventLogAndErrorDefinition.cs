using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TRIO2026.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddEventLogAndErrorDefinition : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ErrorDefinition",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Code = table.Column<string>(type: "TEXT", maxLength: 20, nullable: false),
                    Category = table.Column<string>(type: "TEXT", maxLength: 64, nullable: false),
                    Severity = table.Column<string>(type: "TEXT", maxLength: 16, nullable: false),
                    Title = table.Column<string>(type: "TEXT", maxLength: 128, nullable: false),
                    Description = table.Column<string>(type: "TEXT", nullable: false),
                    Resolution = table.Column<string>(type: "TEXT", nullable: true),
                    UserMessageKey = table.Column<string>(type: "TEXT", nullable: true),
                    UserMessageFallback = table.Column<string>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ErrorDefinition", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "SystemEvent",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Timestamp = table.Column<string>(type: "TEXT", nullable: false),
                    TimestampLocal = table.Column<string>(type: "TEXT", nullable: false),
                    CorrelationId = table.Column<string>(type: "TEXT", maxLength: 64, nullable: true),
                    ErrorId = table.Column<string>(type: "TEXT", maxLength: 20, nullable: true),
                    Level = table.Column<string>(type: "TEXT", maxLength: 16, nullable: false),
                    Category = table.Column<string>(type: "TEXT", maxLength: 64, nullable: false),
                    Source = table.Column<string>(type: "TEXT", maxLength: 128, nullable: false),
                    EventCode = table.Column<string>(type: "TEXT", maxLength: 64, nullable: true),
                    Message = table.Column<string>(type: "TEXT", nullable: false),
                    Detail = table.Column<string>(type: "TEXT", nullable: true),
                    ExceptionType = table.Column<string>(type: "TEXT", maxLength: 256, nullable: true),
                    StackTrace = table.Column<string>(type: "TEXT", nullable: true),
                    InnerException = table.Column<string>(type: "TEXT", nullable: true),
                    UserName = table.Column<string>(type: "TEXT", maxLength: 64, nullable: true),
                    SessionId = table.Column<string>(type: "TEXT", maxLength: 64, nullable: true),
                    Tags = table.Column<string>(type: "TEXT", nullable: true),
                    MachineName = table.Column<string>(type: "TEXT", maxLength: 64, nullable: true),
                    AppVersion = table.Column<string>(type: "TEXT", maxLength: 32, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SystemEvent", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ErrorDefinition_Category",
                table: "ErrorDefinition",
                column: "Category");

            migrationBuilder.CreateIndex(
                name: "IX_ErrorDefinition_Code",
                table: "ErrorDefinition",
                column: "Code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_SystemEvent_Category_Source",
                table: "SystemEvent",
                columns: new[] { "Category", "Source" });

            migrationBuilder.CreateIndex(
                name: "IX_SystemEvent_CorrelationId",
                table: "SystemEvent",
                column: "CorrelationId");

            migrationBuilder.CreateIndex(
                name: "IX_SystemEvent_ErrorId",
                table: "SystemEvent",
                column: "ErrorId");

            migrationBuilder.CreateIndex(
                name: "IX_SystemEvent_EventCode",
                table: "SystemEvent",
                column: "EventCode");

            migrationBuilder.CreateIndex(
                name: "IX_SystemEvent_Level",
                table: "SystemEvent",
                column: "Level");

            migrationBuilder.CreateIndex(
                name: "IX_SystemEvent_Timestamp",
                table: "SystemEvent",
                column: "Timestamp");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ErrorDefinition");

            migrationBuilder.DropTable(
                name: "SystemEvent");
        }
    }
}
