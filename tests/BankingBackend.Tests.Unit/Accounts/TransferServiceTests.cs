using BankingBackend.Core.Accounts;
using FluentAssertions;

namespace BankingBackend.Tests.Unit.Accounts;

public class TransferServiceTests
{
    private static Account CreateAccountWithBalance(Guid userId, decimal balance)
    {
        var account = Account.Open(userId).Value;
        if (balance > 0)
            account.Deposit(Money.Create(balance).Value);
        return account;
    }

    [Fact]
    public void Transfer_Deve_MoverOSaldo_EntreContas()
    {
        var from = CreateAccountWithBalance(Guid.NewGuid(), 100m);
        var to = CreateAccountWithBalance(Guid.NewGuid(), 50m);

        var result = TransferService.Transfer(from, to, Money.Create(40m).Value);

        result.IsSuccess.Should().BeTrue();
        from.Balance.Amount.Should().Be(60m);
        to.Balance.Amount.Should().Be(90m);
    }

    [Fact]
    public void Transfer_Deve_Falhar_Quando_ContasSaoAsMesmas()
    {
        var account = CreateAccountWithBalance(Guid.NewGuid(), 100m);

        var result = TransferService.Transfer(account, account, Money.Create(10m).Value);

        result.Error.Should().Be(TransferErrors.SameAccount);
    }

    [Fact]
    public void Transfer_Deve_Falhar_Quando_SaldoForInsuficiente()
    {
        var from = CreateAccountWithBalance(Guid.NewGuid(), 30m);
        var to = CreateAccountWithBalance(Guid.NewGuid(), 50m);

        var result = TransferService.Transfer(from, to, Money.Create(100m).Value);

        result.Error.Should().Be(AccountErrors.InsufficientBalance);
        from.Balance.Amount.Should().Be(30m);
        to.Balance.Amount.Should().Be(50m);
    }

    [Fact]
    public void Transfer_Deve_Falhar_Quando_ContaOrigemInativa()
    {
        var from = CreateAccountWithBalance(Guid.NewGuid(), 100m);
        from.Close();
        var to = CreateAccountWithBalance(Guid.NewGuid(), 50m);

        var result = TransferService.Transfer(from, to, Money.Create(10m).Value);

        result.Error.Should().Be(AccountErrors.AccountInactive);
    }

    [Fact]
    public void Transfer_Deve_Falhar_Quando_ContaDestinoInativa()
    {
        var from = CreateAccountWithBalance(Guid.NewGuid(), 100m);
        var to = CreateAccountWithBalance(Guid.NewGuid(), 50m);
        to.Close();

        var result = TransferService.Transfer(from, to, Money.Create(10m).Value);

        result.Error.Should().Be(AccountErrors.AccountInactive);
    }

    [Fact]
    public void Transfer_Deve_Falhar_Quando_ValorForInvalido()
    {
        var from = CreateAccountWithBalance(Guid.NewGuid(), 100m);
        var to = CreateAccountWithBalance(Guid.NewGuid(), 50m);

        var result = TransferService.Transfer(from, to, Money.Zero);

        result.Error.Should().Be(TransferErrors.InvalidAmount);
    }
}
