using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Parky.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class SyncMigration : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "users",
                keyColumn: "id",
                keyValue: 1,
                columns: new[] { "password", "username" },
                values: new object[] { "driverpassword", "driver" });

            migrationBuilder.UpdateData(
                table: "users",
                keyColumn: "id",
                keyValue: 2,
                columns: new[] { "password", "username" },
                values: new object[] { "ownerpassword", "owner" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "users",
                keyColumn: "id",
                keyValue: 1,
                columns: new[] { "password", "username" },
                values: new object[] { "userpassword", "user" });

            migrationBuilder.UpdateData(
                table: "users",
                keyColumn: "id",
                keyValue: 2,
                columns: new[] { "password", "username" },
                values: new object[] { "adminpassword", "admin" });
        }
    }
}
