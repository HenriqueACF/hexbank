using BankingBackend.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace BankingBackend.Tests.Integration;

[Collection("Postgres")]
public abstract class IntegrationTestBase
{
    protected PostgresContainerFixture Fixture { get; }

    protected IntegrationTestBase(PostgresContainerFixture fixture) => Fixture = fixture;

    protected ApplicationDbContext CreateContext() => Fixture.CreateContext();

    protected async Task ClearTablesAsync()
    {
        await using var context = CreateContext();
        await context.Database.ExecuteSqlRawAsync(
            "TRUNCATE TABLE refresh_tokens, accounts, users RESTART IDENTITY CASCADE");
    }
}
