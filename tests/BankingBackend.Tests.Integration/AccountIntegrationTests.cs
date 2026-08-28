using BankingBackend.Application.Accounts.Deposit;
using BankingBackend.Core.Accounts;
using BankingBackend.Infrastructure.Persistence.Repositories;
using FluentAssertions;

namespace BankingBackend.Tests.Integration;

public class AccountIntegrationTests : IntegrationTestBase
{
    public AccountIntegrationTests(PostgresContainerFixture fixture) : base(fixture)
    {
    }

    [Fact]
    public async Task AbrirConta_Deve_Persistir_E_BuscarPeloId()
    {
        await ClearTablesAsync();

        var userId = Guid.NewGuid();
        var account = Account.Open(userId).Value;

        await using (var context = CreateContext())
        {
            new AccountRepository(context).Add(account);
            await context.SaveChangesAsync();
        }

        // Contexto novo: garante que lemos do banco, não do tracking em memória.
        await using var readContext = CreateContext();
        var persisted = await new AccountRepository(readContext).GetByIdAsync(account.Id);

        persisted.Should().NotBeNull();
        persisted!.UserId.Should().Be(userId);
        persisted.Number.Should().Be(account.Number);
        persisted.Balance.Should().Be(Money.Zero);
        persisted.IsActive.Should().BeTrue();
    }

    [Fact]
    public async Task Depositar_Deve_PersistirOSaldo_NoBanco()
    {
        await ClearTablesAsync();

        var userId = Guid.NewGuid();
        Account account;

        await using (var context = CreateContext())
        {
            account = Account.Open(userId).Value;
            new AccountRepository(context).Add(account);
            await context.SaveChangesAsync();
        }

        await using (var context = CreateContext())
        {
            var handler = new DepositCommandHandler(
                new AccountRepository(context),
                context,
                new DepositCommandValidator());

            var result = await handler.Handle(
                new DepositCommand(userId, account.Id, 50m),
                CancellationToken.None);

            result.IsSuccess.Should().BeTrue();
        }

        await using var readContext = CreateContext();
        var persisted = await new AccountRepository(readContext).GetByIdAsync(account.Id);
        persisted!.Balance.Amount.Should().Be(50m);
    }
}
