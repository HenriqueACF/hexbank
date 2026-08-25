using BankingBackend.Application.Accounts.Deposit;
using BankingBackend.Core.Accounts;
using BankingBackend.Core.Common;
using FluentAssertions;
using Moq;

namespace BankingBackend.Tests.Unit.Application.Accounts.Deposit;

public class DepositCommandHandlerTests
{
    private readonly Mock<IAccountRepository> _accountRepository = new();
    private readonly Mock<IUnitOfWork> _unitOfWork = new();
    private readonly DepositCommandHandler _handler;

    public DepositCommandHandlerTests()
    {
        _handler = new DepositCommandHandler(
            _accountRepository.Object,
            _unitOfWork.Object,
            new DepositCommandValidator());
    }

    private static Account CreateAccountWithBalance(Guid userId, decimal balance)
    {
        var account = Account.Open(userId).Value;
        if (balance > 0)
            account.Deposit(Money.Create(balance).Value);
        return account;
    }

    [Fact]
    public async Task Handle_Deve_Depositar_Quando_ValorValido()
    {
        var userId = Guid.NewGuid();
        var account = CreateAccountWithBalance(userId, 50m);
        _accountRepository
            .Setup(r => r.GetByIdAsync(account.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(account);

        var result = await _handler.Handle(
            new DepositCommand(userId, account.Id, 30m),
            CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.Balance.Should().Be(80m);
        _unitOfWork.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_Deve_Falhar_Quando_ContaNaoExiste()
    {
        var accountId = Guid.NewGuid();
        _accountRepository
            .Setup(r => r.GetByIdAsync(accountId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Account?)null);

        var result = await _handler.Handle(
            new DepositCommand(Guid.NewGuid(), accountId, 30m),
            CancellationToken.None);

        result.Error.Should().Be(AccountErrors.NotFound);
        _unitOfWork.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handle_Deve_Falhar_Quando_UsuarioNaoForODono()
    {
        var account = CreateAccountWithBalance(Guid.NewGuid(), 50m);
        _accountRepository
            .Setup(r => r.GetByIdAsync(account.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(account);

        var result = await _handler.Handle(
            new DepositCommand(Guid.NewGuid(), account.Id, 30m),
            CancellationToken.None);

        result.Error.Should().Be(AccountErrors.NotOwner);
        _unitOfWork.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-10)]
    public async Task Handle_Deve_Falhar_Quando_ValorForZeroOuNegativo(decimal amount)
    {
        var result = await _handler.Handle(
            new DepositCommand(Guid.NewGuid(), Guid.NewGuid(), amount),
            CancellationToken.None);

        result.Error.Code.Should().Be("Validation.Failed");
        _accountRepository.Verify(
            r => r.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task Handle_Deve_Falhar_Quando_ContaEstiverInativa()
    {
        var userId = Guid.NewGuid();
        var account = Account.Open(userId).Value;
        account.Close();
        _accountRepository
            .Setup(r => r.GetByIdAsync(account.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(account);

        var result = await _handler.Handle(
            new DepositCommand(userId, account.Id, 30m),
            CancellationToken.None);

        result.Error.Should().Be(AccountErrors.AccountInactive);
        _unitOfWork.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }
}
