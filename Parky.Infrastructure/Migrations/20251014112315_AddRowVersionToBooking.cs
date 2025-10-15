using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Parky.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddRowVersionToBooking : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<byte[]>(
                name: "row_version",
                table: "bookings",
                type: "bytea",
                rowVersion: true,
                nullable: false,
                defaultValue: new byte[0]);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "row_version",
                table: "bookings");
        }
    }
}
