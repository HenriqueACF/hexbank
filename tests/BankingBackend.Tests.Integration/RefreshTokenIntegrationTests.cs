using BankingBackend.Core.Users;
using BankingBackend.Infrastructure.Persistence.Repositories;
using FluentAssertions;

namespace BankingBackend.Tests.Integration;

public class RefreshTokenIntegrationTests : IntegrationTestBase
{
    public RefreshTokenIntegrationTests(PostgresContainerFixture fixture) : base(fixture)
    {
    }

    [Fact]
    public async Task Adicionar_Deve_Persistir_E_BuscarPeloValue()
    {
        await ClearTablesAsync();

        var userId = Guid.NewGuid();
        var token = RefreshToken.Create(userId, DateTime.UtcNow.AddDays(7)).Value;

        await using (var context = CreateContext())
        {
            new RefreshTokenRepository(context).Add(token);
            await context.SaveChangesAsync();
        }

        await using var readContext = CreateContext();
        var persisted = await new RefreshTokenRepository(readContext).GetByValueAsync(token.Value);

        persisted.Should().NotBeNull();
        persisted!.UserId.Should().Be(userId);
        // Postgres timestamptz tem precisão de microsegundos; DateTime .NET de 100ns.
        persisted.ExpiresAtUtc.Should().BeCloseTo(token.ExpiresAtUtc, TimeSpan.FromMilliseconds(1));
        persisted.IsRevoked.Should().BeFalse();
    }

    [Fact]
    public async Task Revogar_Deve_PersistirORefreshToken()
    {
        await ClearTablesAsync();

        var userId = Guid.NewGuid();
        var token = RefreshToken.Create(userId, DateTime.UtcNow.AddDays(7)).Value;

        await using (var context = CreateContext())
        {
            var repository = new RefreshTokenRepository(context);
            repository.Add(token);
            await context.SaveChangesAsync();
        }

        await using (var context = CreateContext())
        {
            var repository = new RefreshTokenRepository(context);
            var stored = await repository.GetByValueAsync(token.Value);

            var result = stored!.Revoke();
            result.IsSuccess.Should().BeTrue();

            await context.SaveChangesAsync();
        }

        await using var readContext = CreateContext();
        var persisted = await new RefreshTokenRepository(readContext).GetByValueAsync(token.Value);
        persisted!.IsRevoked.Should().BeTrue();
    }
}
