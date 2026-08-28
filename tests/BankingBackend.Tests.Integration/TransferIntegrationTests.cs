using BankingBackend.Application.Accounts.Transfer;
using BankingBackend.Core.Accounts;
using BankingBackend.Infrastructure.Persistence.Repositories;
using FluentAssertions;

namespace BankingBackend.Tests.Integration;

public class TransferIntegrationTests : IntegrationTestBase
{
    public TransferIntegrationTests(PostgresContainerFixture fixture) : base(fixture)
    {
    }

    private async Task<(Account From, Account To, Guid UserId)> CreateTwoAccountsWithFundsAsync(decimal fromBalance)
    {
        await using var context = CreateContext();
        var repository = new AccountRepository(context);

        var userId = Guid.NewGuid();
        var from = Account.Open(userId).Value;
        var to = Account.Open(Guid.NewGuid()).Value;

        repository.Add(from);
        repository.Add(to);

        if (fromBalance > 0)
            from.Deposit(Money.Create(fromBalance).Value);

        await context.SaveChangesAsync();
        return (from, to, userId);
    }

    [Fact]
    public async Task Transferir_Deve_PersistirOSaldoNasDuasContas()
    {
        await ClearTablesAsync();
        var (from, to, userId) = await CreateTwoAccountsWithFundsAsync(100m);

        await using (var context = CreateContext())
        {
            var handler = new TransferCommandHandler(
                new AccountRepository(context),
                context,
                new TransferCommandValidator());

            var result = await handler.Handle(
                new TransferCommand(userId, from.Id, to.Id, 40m),
                CancellationToken.None);

            result.IsSuccess.Should().BeTrue();
        }

        await using var readContext = CreateContext();
        var repository = new AccountRepository(readContext);
        var persistedFrom = await repository.GetByIdAsync(from.Id);
        var persistedTo = await repository.GetByIdAsync(to.Id);

        persistedFrom!.Balance.Amount.Should().Be(60m);
        persistedTo!.Balance.Amount.Should().Be(40m);
    }

    [Fact]
    public async Task Transferir_Com_SaldoInsuficiente_NaoDeixaEstadoParcial()
    {
        await ClearTablesAsync();
        var (from, to, userId) = await CreateTwoAccountsWithFundsAsync(30m);

        await using (var context = CreateContext())
        {
            var handler = new TransferCommandHandler(
                new AccountRepository(context),
                context,
                new TransferCommandValidator());

            var result = await handler.Handle(
                new TransferCommand(userId, from.Id, to.Id, 100m),
                CancellationToken.None);

            result.IsSuccess.Should().BeFalse();
            result.Error.Should().Be(AccountErrors.InsufficientBalance);
        }

        await using var readContext = CreateContext();
        var repository = new AccountRepository(readContext);
        var persistedFrom = await repository.GetByIdAsync(from.Id);
        var persistedTo = await repository.GetByIdAsync(to.Id);

        // Origem não debitada e destino não creditado: nenhum estado parcial.
        persistedFrom!.Balance.Amount.Should().Be(30m);
        persistedTo!.Balance.Amount.Should().Be(0m);
    }
}
