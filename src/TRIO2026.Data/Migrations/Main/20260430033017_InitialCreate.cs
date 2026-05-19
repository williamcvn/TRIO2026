using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TRIO2026.Data.Migrations.Main
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "FlowDefinition",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    FlowName = table.Column<string>(type: "TEXT", nullable: false),
                    Description = table.Column<string>(type: "TEXT", nullable: true),
                    TotalSteps = table.Column<int>(type: "INTEGER", nullable: false, defaultValue: 0),
                    Version = table.Column<string>(type: "TEXT", nullable: true),
                    SampleType = table.Column<string>(type: "TEXT", nullable: true),
                    IsActive = table.Column<int>(type: "INTEGER", nullable: false, defaultValue: 1),
                    CreatedAt = table.Column<string>(type: "TEXT", nullable: false),
                    ModifiedAt = table.Column<string>(type: "TEXT", nullable: false),
                    ModifiedBy = table.Column<string>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FlowDefinition", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "FlowMapping",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    FlowCode = table.Column<string>(type: "TEXT", nullable: false),
                    BuiltInFlowName = table.Column<string>(type: "TEXT", nullable: false),
                    ElutionVolume = table.Column<string>(type: "TEXT", nullable: true),
                    LoadingVolume = table.Column<string>(type: "TEXT", nullable: true),
                    SampleType = table.Column<string>(type: "TEXT", nullable: true),
                    ConsumableLayoutCode = table.Column<string>(type: "TEXT", nullable: true),
                    ExtractionTime = table.Column<int>(type: "INTEGER", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FlowMapping", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "PnidMapping",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    PnidCode = table.Column<string>(type: "TEXT", nullable: false),
                    DescriptionEn = table.Column<string>(type: "TEXT", nullable: true),
                    DescriptionZh = table.Column<string>(type: "TEXT", nullable: true),
                    LinkedProductCode = table.Column<string>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PnidMapping", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "RoleDefinition",
                columns: table => new
                {
                    Level = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Code = table.Column<string>(type: "TEXT", nullable: false),
                    DisplayName = table.Column<string>(type: "TEXT", nullable: false),
                    Description = table.Column<string>(type: "TEXT", nullable: true),
                    CreatedAt = table.Column<string>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RoleDefinition", x => x.Level);
                });

            migrationBuilder.CreateTable(
                name: "FlowStep",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    FlowDefinitionId = table.Column<int>(type: "INTEGER", nullable: false),
                    StepOrder = table.Column<int>(type: "INTEGER", nullable: false),
                    CommandId = table.Column<int>(type: "INTEGER", nullable: false),
                    Crc = table.Column<int>(type: "INTEGER", nullable: false, defaultValue: 0),
                    Arg0 = table.Column<double>(type: "REAL", nullable: false, defaultValue: 0.0),
                    Arg1 = table.Column<double>(type: "REAL", nullable: false, defaultValue: 0.0),
                    Arg2 = table.Column<double>(type: "REAL", nullable: false, defaultValue: 0.0),
                    Arg3 = table.Column<double>(type: "REAL", nullable: false, defaultValue: 0.0),
                    Arg4 = table.Column<double>(type: "REAL", nullable: false, defaultValue: 0.0),
                    StringArg = table.Column<string>(type: "TEXT", nullable: true),
                    GroupName = table.Column<string>(type: "TEXT", nullable: true),
                    GroupDepth = table.Column<int>(type: "INTEGER", nullable: false, defaultValue: 0)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FlowStep", x => x.Id);
                    table.ForeignKey(
                        name: "FK_FlowStep_FlowDefinition_FlowDefinitionId",
                        column: x => x.FlowDefinitionId,
                        principalTable: "FlowDefinition",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "UserAccount",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Username = table.Column<string>(type: "TEXT", nullable: false),
                    PasswordHash = table.Column<string>(type: "TEXT", nullable: false),
                    RoleLevel = table.Column<int>(type: "INTEGER", nullable: false, defaultValue: 1),
                    IsActive = table.Column<int>(type: "INTEGER", nullable: false, defaultValue: 1),
                    CreatedAt = table.Column<string>(type: "TEXT", nullable: false),
                    LastLoginAt = table.Column<string>(type: "TEXT", nullable: true),
                    FailedLoginCount = table.Column<int>(type: "INTEGER", nullable: false, defaultValue: 0),
                    LockedUntil = table.Column<string>(type: "TEXT", nullable: true),
                    PasswordChangedAt = table.Column<string>(type: "TEXT", nullable: true),
                    DisplayName = table.Column<string>(type: "TEXT", nullable: true),
                    AvatarImage = table.Column<byte[]>(type: "BLOB", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserAccount", x => x.Id);
                    table.ForeignKey(
                        name: "FK_UserAccount_RoleDefinition_RoleLevel",
                        column: x => x.RoleLevel,
                        principalTable: "RoleDefinition",
                        principalColumn: "Level",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_FlowDefinition_FlowName",
                table: "FlowDefinition",
                column: "FlowName",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_FlowMapping_FlowCode",
                table: "FlowMapping",
                column: "FlowCode",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_FlowStep_FlowDefinitionId_StepOrder",
                table: "FlowStep",
                columns: new[] { "FlowDefinitionId", "StepOrder" });

            migrationBuilder.CreateIndex(
                name: "IX_PnidMapping_PnidCode",
                table: "PnidMapping",
                column: "PnidCode",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_RoleDefinition_Code",
                table: "RoleDefinition",
                column: "Code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_UserAccount_RoleLevel",
                table: "UserAccount",
                column: "RoleLevel");

            migrationBuilder.CreateIndex(
                name: "IX_UserAccount_Username",
                table: "UserAccount",
                column: "Username",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "FlowMapping");

            migrationBuilder.DropTable(
                name: "FlowStep");

            migrationBuilder.DropTable(
                name: "PnidMapping");

            migrationBuilder.DropTable(
                name: "UserAccount");

            migrationBuilder.DropTable(
                name: "FlowDefinition");

            migrationBuilder.DropTable(
                name: "RoleDefinition");
        }
    }
}
