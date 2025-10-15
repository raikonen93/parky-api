using Testcontainers.PostgreSql;

namespace Parky.Tests.Integration.Fixtures
{
    public class PostgreSqlTestcontainerFixture : IAsyncLifetime
    {
        public PostgreSqlContainer PostgreSqlContainer { get; }
        public PostgreSqlTestcontainerFixture()
        {
            PostgreSqlContainer = new PostgreSqlBuilder()
                .WithImage("postgres:18")
                .WithDatabase("testDb")
                .WithUsername("postgres")
                .WithPassword("postgres")
                .WithPortBinding(0, false)
                .Build();
        }

        public async Task InitializeAsync()
        {
            await PostgreSqlContainer.StartAsync();
        }

        public async Task DisposeAsync()
        {
            await PostgreSqlContainer.DisposeAsync();
        }
    }
}
