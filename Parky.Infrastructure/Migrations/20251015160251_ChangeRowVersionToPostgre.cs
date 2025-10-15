using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Parky.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class ChangeRowVersionToPostgre : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "row_version",
                table: "bookings");

            migrationBuilder.AddColumn<uint>(
                name: "xmin",
                table: "bookings",
                type: "xid",
                rowVersion: true,
                nullable: false,
                defaultValue: 0u);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "xmin",
                table: "bookings");

            migrationBuilder.AddColumn<byte[]>(
                name: "row_version",
                table: "bookings",
                type: "bytea",
                rowVersion: true,
                nullable: false,
                defaultValue: new byte[0]);
        }
    }
}
