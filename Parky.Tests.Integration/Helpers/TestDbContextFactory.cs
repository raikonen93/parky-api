using Microsoft.EntityFrameworkCore;
using Parky.Infrastructure.Context;

namespace Parky.Tests.Integration.Helpers
{
    public class TestDbContextFactory
    {
        private readonly string _connectionString;
        public TestDbContextFactory(string connectionString)
        {
            _connectionString = connectionString;
        }
        public ParkyDbContext CreateDbContext()
        {
            var optionsBuilder = new DbContextOptionsBuilder<ParkyDbContext>();
            optionsBuilder.UseNpgsql(_connectionString);

            return new ParkyDbContext(optionsBuilder.Options);
        }
    }
}
