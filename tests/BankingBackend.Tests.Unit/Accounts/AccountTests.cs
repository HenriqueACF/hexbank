using BankingBackend.Core.Accounts;
using FluentAssertions;

namespace BankingBackend.Tests.Unit.Accounts;

public class AccountTests
{
    [Fact]
    public void Open_Deve_CriarContaAtivaComSaldoZero()
    {
        var result = Account.Open(Guid.NewGuid());

        result.IsSuccess.Should().BeTrue();

        var account = result.Value;
        account.IsActive.Should().BeTrue();
        account.Balance.Should().Be(Money.Zero);
        account.Number.Should().NotBeNullOrWhiteSpace();
    }

    [Fact]
    public void Open_Deve_LevantarAccountOpenedDomainEvent()
    {
        var userId = Guid.NewGuid();
        var account = Account.Open(userId).Value;

        account.DomainEvents.Should().ContainSingle()
            .Which.Should().BeOfType<AccountOpenedDomainEvent>()
            .Which.UserId.Should().Be(userId);
    }

    [Fact]
    public void Deposit_Deve_AumentarOSaldo()
    {
        var account = Account.Open(Guid.NewGuid()).Value;

        var result = account.Deposit(Money.Create(100m).Value);

        result.IsSuccess.Should().BeTrue();
        account.Balance.Amount.Should().Be(100m);
    }

    [Fact]
    public void Deposit_Deve_Falhar_Quando_ValorForZeroOuNegativo()
    {
        var account = Account.Open(Guid.NewGuid()).Value;

        var result = account.Deposit(Money.Zero);

        result.Error.Should().Be(AccountErrors.InvalidAmout);
    }

    [Fact]
    public void Deposit_Deve_Falhar_Quando_ContaEstiverFechada()
    {
        var account = Account.Open(Guid.NewGuid()).Value;
        account.Close();

        var result = account.Deposit(Money.Create(50m).Value);

        result.Error.Should().Be(AccountErrors.AccountInactive);
    }

    [Fact]
    public void Withdraw_Deve_DiminuirOSaldo_QuandoHouverSaldoSuficiente()
    {
        var account = Account.Open(Guid.NewGuid()).Value;
        account.Deposit(Money.Create(100m).Value);

        var result = account.Withdraw(Money.Create(40m).Value);

        result.IsSuccess.Should().BeTrue();
        account.Balance.Amount.Should().Be(60m);
    }

    [Fact]
    public void Withdraw_Deve_Falhar_Quando_SaldoForInsuficiente()
    {
        var account = Account.Open(Guid.NewGuid()).Value;
        account.Deposit(Money.Create(50m).Value);

        var result = account.Withdraw(Money.Create(100m).Value);

        result.Error.Should().Be(AccountErrors.InsufficientBalance);
        account.Balance.Amount.Should().Be(50m);
    }

    [Fact]
    public void Withdraw_Deve_Falhar_Quando_ContaEstiverFechada()
    {
        var account = Account.Open(Guid.NewGuid()).Value;
        account.Deposit(Money.Create(100m).Value);
        account.Close();

        var result = account.Withdraw(Money.Create(10m).Value);

        result.Error.Should().Be(AccountErrors.AccountInactive);
    }

    [Fact]
    public void Close_Deve_Falhar_Quando_JaEstiverFechada()
    {
        var account = Account.Open(Guid.NewGuid()).Value;
        account.Close();

        var result = account.Close();

        result.Error.Should().Be(AccountErrors.AlreadyInactive);
    }

    [Fact]
    public void Reopen_Deve_Falhar_Quando_JaEstiverAtiva()
    {
        var account = Account.Open(Guid.NewGuid()).Value;

        var result = account.Reopen();

        result.Error.Should().Be(AccountErrors.AlreadyActive);
    }
}