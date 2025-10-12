using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Parky.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class FixCapacityCheckConstraint : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropCheckConstraint(
                name: "CK_ParkingLot_Name_NotEmpty",
                table: "parking_lots");

            migrationBuilder.AddCheckConstraint(
                name: "CK_ParkingLot_Name_NotEmpty",
                table: "parking_lots",
                sql: "char_length(name) >= 0");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropCheckConstraint(
                name: "CK_ParkingLot_Name_NotEmpty",
                table: "parking_lots");

            migrationBuilder.AddCheckConstraint(
                name: "CK_ParkingLot_Name_NotEmpty",
                table: "parking_lots",
                sql: "char_length(name) > 0");
        }
    }
}
