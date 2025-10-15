using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Parky.Domain.Entities;
using Parky.Tests.Integration.Fixtures;
using Parky.Tests.Integration.Helpers;

namespace Parky.Tests.Integration.Tests
{
    public class ParkingLotsCrudTests : IClassFixture<PostgreSqlTestcontainerFixture>, IAsyncLifetime
    {
        private readonly PostgreSqlTestcontainerFixture _fixture;
        private readonly TestDbContextFactory _dbContextFactory;

        public ParkingLotsCrudTests(PostgreSqlTestcontainerFixture fixture)
        {
            _fixture = fixture;
            _dbContextFactory = new TestDbContextFactory(_fixture.PostgreSqlContainer.GetConnectionString());


        }

        [Fact]
        public async Task CreateParkingLot_ShouldAddRecord()
        {
            // Arrange
            using var context = _dbContextFactory.CreateDbContext();
            var entity = new ParkingLot { Name = "Test Lot", Capacity = 100 };

            // Act
            context.ParkingLots.Add(entity);
            await context.SaveChangesAsync();

            // Assert
            var result = await context.ParkingLots.FirstOrDefaultAsync(p => p.Name == "Test Lot");
            result.Should().NotBeNull();
            result!.Id.Should().BeGreaterThan(0);
            result.Capacity.Should().Be(100);
        }

        [Fact]
        public async Task GetParkingLotById_ShouldReturnEntity()
        {
            using var context = _dbContextFactory.CreateDbContext();

            var entity = new ParkingLot { Name = "Read Lot", Capacity = 50 };
            context.ParkingLots.Add(entity);
            await context.SaveChangesAsync();

            var found = await context.ParkingLots.FindAsync(entity.Id);

            found.Should().NotBeNull();
            found!.Name.Should().Be("Read Lot");
        }

        [Fact]
        public async Task UpdateParkingLot_ShouldModifyEntity()
        {
            using var context = _dbContextFactory.CreateDbContext();
            var entity = new ParkingLot { Name = "Update Lot", Capacity = 10 };
            context.ParkingLots.Add(entity);
            await context.SaveChangesAsync();

            // Act
            entity.Capacity = 300;
            context.ParkingLots.Update(entity);
            await context.SaveChangesAsync();

            // Assert
            var updated = await context.ParkingLots.FindAsync(entity.Id);
            updated!.Capacity.Should().Be(300);
        }

        [Fact]
        public async Task DeleteParkingLot_ShouldRemoveEntity()
        {
            using var context = _dbContextFactory.CreateDbContext();
            var entity = new ParkingLot { Name = "Delete Lot", Capacity = 20 };
            context.ParkingLots.Add(entity);
            await context.SaveChangesAsync();

            // Act
            context.ParkingLots.Remove(entity);
            await context.SaveChangesAsync();

            // Assert
            var deleted = await context.ParkingLots.FindAsync(entity.Id);
            deleted.Should().BeNull();
        }

        [Fact]
        public async Task CreateInvalidParkingLot_ShouldThrowException()
        {
            using var context = _dbContextFactory.CreateDbContext();

            var entity = new ParkingLot { Name = "", Capacity = 10 };

            context.ParkingLots.Add(entity);

            await FluentActions
                   .Invoking(async () => await context.SaveChangesAsync())
                   .Should()
                   .ThrowAsync<DbUpdateException>();

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
