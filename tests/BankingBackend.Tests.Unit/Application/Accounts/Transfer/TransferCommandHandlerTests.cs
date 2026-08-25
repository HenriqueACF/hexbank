using BankingBackend.Application.Accounts.Transfer;
using BankingBackend.Core.Accounts;
using BankingBackend.Core.Common;
using FluentAssertions;
using Moq;

namespace BankingBackend.Tests.Unit.Application.Accounts.Transfer;

public class TransferCommandHandlerTests
{
    private readonly Mock<IAccountRepository> _accountRepository = new();
    private readonly Mock<IUnitOfWork> _unitOfWork = new();
    private readonly TransferCommandHandler _handler;

    public TransferCommandHandlerTests()
    {
        _handler = new TransferCommandHandler(
            _accountRepository.Object,
            _unitOfWork.Object,
            new TransferCommandValidator());
    }

    private static Account CreateAccountWithBalance(Guid userId, decimal balance)
    {
        var account = Account.Open(userId).Value;
        if (balance > 0)
            account.Deposit(Money.Create(balance).Value);
        return account;
    }

    [Fact]
    public async Task Handle_Deve_Transferir_Quando_ValorValido()
    {
        var userId = Guid.NewGuid();
        var from = CreateAccountWithBalance(userId, 100m);
        var to = CreateAccountWithBalance(Guid.NewGuid(), 0m);
        _accountRepository
            .Setup(r => r.GetByIdAsync(from.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(from);
        _accountRepository
            .Setup(r => r.GetByIdAsync(to.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(to);

        var result = await _handler.Handle(
            new TransferCommand(userId, from.Id, to.Id, 40m),
            CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.FromBalance.Should().Be(60m);
        from.Balance.Amount.Should().Be(60m);
        to.Balance.Amount.Should().Be(40m);
        _unitOfWork.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_Deve_Falhar_Quando_ContaOrigemNaoExiste()
    {
        var fromId = Guid.NewGuid();
        _accountRepository
            .Setup(r => r.GetByIdAsync(fromId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Account?)null);

        var result = await _handler.Handle(
            new TransferCommand(Guid.NewGuid(), fromId, Guid.NewGuid(), 40m),
            CancellationToken.None);

        result.Error.Should().Be(AccountErrors.NotFound);
        _unitOfWork.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handle_Deve_Falhar_Quando_ContaDestinoNaoExiste()
    {
        var from = CreateAccountWithBalance(Guid.NewGuid(), 100m);
        var toId = Guid.NewGuid();
        _accountRepository
            .Setup(r => r.GetByIdAsync(from.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(from);
        _accountRepository
            .Setup(r => r.GetByIdAsync(toId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Account?)null);

        var result = await _handler.Handle(
            new TransferCommand(Guid.NewGuid(), from.Id, toId, 40m),
            CancellationToken.None);

        result.Error.Should().Be(AccountErrors.NotFound);
        _unitOfWork.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handle_Deve_Falhar_Quando_UsuarioNaoForODono()
    {
        var from = CreateAccountWithBalance(Guid.NewGuid(), 100m);
        var to = CreateAccountWithBalance(Guid.NewGuid(), 0m);
        _accountRepository
            .Setup(r => r.GetByIdAsync(from.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(from);
        _accountRepository
            .Setup(r => r.GetByIdAsync(to.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(to);

        var result = await _handler.Handle(
            new TransferCommand(Guid.NewGuid(), from.Id, to.Id, 40m),
            CancellationToken.None);

        result.Error.Should().Be(AccountErrors.NotOwner);
        _unitOfWork.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handle_Deve_Falhar_Quando_SaldoInsuficiente()
    {
        var userId = Guid.NewGuid();
        var from = CreateAccountWithBalance(userId, 30m);
        var to = CreateAccountWithBalance(Guid.NewGuid(), 0m);
        _accountRepository
            .Setup(r => r.GetByIdAsync(from.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(from);
        _accountRepository
            .Setup(r => r.GetByIdAsync(to.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(to);

        var result = await _handler.Handle(
            new TransferCommand(userId, from.Id, to.Id, 100m),
            CancellationToken.None);

        result.Error.Should().Be(AccountErrors.InsufficientBalance);
        _unitOfWork.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-10)]
    public async Task Handle_Deve_Falhar_Quando_ValorForZeroOuNegativo(decimal amount)
    {
        var result = await _handler.Handle(
            new TransferCommand(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), amount),
            CancellationToken.None);

        result.Error.Code.Should().Be("Validation.Failed");
        _accountRepository.Verify(
            r => r.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task Handle_Deve_Falhar_Quando_ContasSaoAsMesmas()
    {
        var userId = Guid.NewGuid();
        var account = CreateAccountWithBalance(userId, 100m);
        _accountRepository
            .Setup(r => r.GetByIdAsync(account.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(account);

        var result = await _handler.Handle(
            new TransferCommand(userId, account.Id, account.Id, 40m),
            CancellationToken.None);

        result.Error.Should().Be(TransferErrors.SameAccount);
        _unitOfWork.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handle_Deve_Falhar_Quando_ContaOrigemInativa()
    {
        var userId = Guid.NewGuid();
        var from = CreateAccountWithBalance(userId, 100m);
        from.Close();
        var to = CreateAccountWithBalance(Guid.NewGuid(), 0m);
        _accountRepository
            .Setup(r => r.GetByIdAsync(from.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(from);
        _accountRepository
            .Setup(r => r.GetByIdAsync(to.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(to);

        var result = await _handler.Handle(
            new TransferCommand(userId, from.Id, to.Id, 40m),
            CancellationToken.None);

        result.Error.Should().Be(AccountErrors.AccountInactive);
        _unitOfWork.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }
}
