using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Parky.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddingExclusionConstraintToBooking : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"CREATE EXTENSION IF NOT EXISTS btree_gist;");


            migrationBuilder.Sql(@"
                ALTER TABLE bookings
                ADD CONSTRAINT no_overlapping_bookings
                EXCLUDE USING gist (
                    lot_id WITH =,
                    tstzrange(""from"", ""to"") WITH &&
                );
            ");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"ALTER TABLE bookings DROP CONSTRAINT IF EXISTS no_overlapping_bookings;");
        }
    }
}
