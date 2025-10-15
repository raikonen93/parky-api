using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Parky.Domain.Entities;
using Parky.Tests.Integration.Fixtures;
using Parky.Tests.Integration.Helpers;

namespace Parky.Tests.Integration.Tests
{
    public class BookingIntegrationTests : IClassFixture<PostgreSqlTestcontainerFixture>, IAsyncLifetime
    {
        private readonly TestDbContextFactory _dbContextFactory;
        private readonly PostgreSqlTestcontainerFixture _fixture;

        public BookingIntegrationTests(PostgreSqlTestcontainerFixture fixture)
        {
            _fixture = fixture;
            _dbContextFactory = new TestDbContextFactory(_fixture.PostgreSqlContainer.GetConnectionString());


        }

        [Fact(DisplayName = "Should create booking successfully")]
        public async Task Should_CreateBooking_Successfully()
        {
            using var context = _dbContextFactory.CreateDbContext();

            var lot = new ParkingLot { Name = "Lot-1", Capacity = 10 };
            context.ParkingLots.Add(lot);
            await context.SaveChangesAsync();

            var booking = new Booking
            {
                LotId = lot.Id,
                UserId = 1,
                From = DateTime.UtcNow,
                To = DateTime.UtcNow.AddHours(1),
                Status = "Active"
            };

            context.Bookings.Add(booking);
            await context.SaveChangesAsync();

            var stored = await context.Bookings.AsNoTracking().FirstAsync();

            stored.Should().NotBeNull();
            stored.Status.Should().Be("Active");
            stored.xmin.Should().BeGreaterThan(0);
        }

        [Fact(DisplayName = "Should prevent overlapping bookings")]
        public async Task Should_NotAllow_OverlappingBookings()
        {
            using var context = _dbContextFactory.CreateDbContext();

            var lot = new ParkingLot { Name = "Lot-Overlap", Capacity = 5 };
            context.ParkingLots.Add(lot);
            await context.SaveChangesAsync();

            var first = new Booking
            {
                LotId = lot.Id,
                UserId = 1,
                From = DateTime.UtcNow,
                To = DateTime.UtcNow.AddHours(1),
                Status = "Active"
            };
            context.Bookings.Add(first);
            await context.SaveChangesAsync();

            var overlapping = new Booking
            {
                LotId = lot.Id,
                UserId = 2,
                From = first.From.AddMinutes(30),
                To = first.To.AddMinutes(30),
                Status = "Active"
            };

            context.Bookings.Add(overlapping);

            await FluentActions
                   .Invoking(async () => await context.SaveChangesAsync())
                   .Should()
                   .ThrowAsync<DbUpdateException>();


        }

        [Fact(DisplayName = "Should cancel booking and update xmin")]
        public async Task Should_CancelBooking_And_UpdateXmin()
        {
            using var context = _dbContextFactory.CreateDbContext();

            var lot = new ParkingLot { Name = "Lot-Cancel", Capacity = 3 };
            context.ParkingLots.Add(lot);
            await context.SaveChangesAsync();

            var booking = new Booking
            {
                LotId = lot.Id,
                UserId = 3,
                From = DateTime.UtcNow,
                To = DateTime.UtcNow.AddHours(1),
                Status = "Active"
            };

            context.Bookings.Add(booking);
            await context.SaveChangesAsync();

            var before = await context.Bookings.AsNoTracking().FirstAsync();

            booking.Status = "Cancelled";
            await context.SaveChangesAsync();

            var after = await context.Bookings.AsNoTracking().FirstAsync();

            after.Status.Should().Be("Cancelled");
            after.xmin.Should().BeGreaterThan(before.xmin);
        }

        [Fact(DisplayName = "Full flow: Owner creates lot - Driver books - Cancels")]
        public async Task FullScenario_ShouldWorkCorrectly()
        {
            using var context = _dbContextFactory.CreateDbContext();

            // Owner creates parking lot
            var lot = new ParkingLot { Name = "FullFlowLot", Capacity = 10 };
            context.ParkingLots.Add(lot);
            await context.SaveChangesAsync();

            // Driver books
            var booking = new Booking
            {
                LotId = lot.Id,
                UserId = 7,
                From = DateTime.UtcNow,
                To = DateTime.UtcNow.AddHours(2),
                Status = "Active"
            };

            context.Bookings.Add(booking);
            await context.SaveChangesAsync();

            var active = await context.Bookings.AsNoTracking().SingleAsync();
            active.Status.Should().Be("Active");

            // Driver cancels
            booking.Status = "Cancelled";
            await context.SaveChangesAsync();

            var cancelled = await context.Bookings.AsNoTracking().SingleAsync();
            cancelled.Status.Should().Be("Cancelled");
            cancelled.xmin.Should().BeGreaterThan(active.xmin);
        }

        public async Task InitializeAsync()
        {
            using var context = _dbContextFactory.CreateDbContext();

            context.Database.EnsureDeleted();
            await context.Database.MigrateAsync();
        }

        public async Task DisposeAsync()
        {
            using var context = _dbContextFactory.CreateDbContext();
            await context.Database.EnsureDeletedAsync();
        }
    }
}
