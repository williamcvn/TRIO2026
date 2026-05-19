using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TRIO2026.Data.Migrations.AppMainDb
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "User",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Username = table.Column<string>(type: "TEXT", maxLength: 64, nullable: false),
                    DisplayName = table.Column<string>(type: "TEXT", maxLength: 128, nullable: true),
                    EmployeeId = table.Column<string>(type: "TEXT", maxLength: 32, nullable: true),
                    Email = table.Column<string>(type: "TEXT", maxLength: 128, nullable: true),
                    Department = table.Column<string>(type: "TEXT", maxLength: 64, nullable: true),
                    PasswordHash = table.Column<string>(type: "TEXT", nullable: false),
                    RoleLevel = table.Column<int>(type: "INTEGER", nullable: false),
                    IsActive = table.Column<int>(type: "INTEGER", nullable: false),
                    FailedLoginCount = table.Column<int>(type: "INTEGER", nullable: false),
                    LockedUntil = table.Column<string>(type: "TEXT", maxLength: 32, nullable: true),
                    PasswordChangedAt = table.Column<string>(type: "TEXT", maxLength: 32, nullable: true),
                    ForcePasswordChange = table.Column<int>(type: "INTEGER", nullable: false),
                    PasswordExpiryDays = table.Column<int>(type: "INTEGER", nullable: false),
                    LastLoginAt = table.Column<string>(type: "TEXT", maxLength: 32, nullable: true),
                    CreatedAt = table.Column<string>(type: "TEXT", maxLength: 32, nullable: false),
                    CreatedBy = table.Column<string>(type: "TEXT", maxLength: 64, nullable: false),
                    UpdatedAt = table.Column<string>(type: "TEXT", maxLength: 32, nullable: true),
                    UpdatedBy = table.Column<string>(type: "TEXT", maxLength: 64, nullable: true),
                    AvatarImage = table.Column<byte[]>(type: "BLOB", nullable: true),
                    Notes = table.Column<string>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_User", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_User_EmployeeId",
                table: "User",
                column: "EmployeeId");

            migrationBuilder.CreateIndex(
                name: "IX_User_IsActive",
                table: "User",
                column: "IsActive");

            migrationBuilder.CreateIndex(
                name: "IX_User_RoleLevel",
                table: "User",
                column: "RoleLevel");

            migrationBuilder.CreateIndex(
                name: "IX_User_Username",
                table: "User",
                column: "Username",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "User");
        }
    }
}
