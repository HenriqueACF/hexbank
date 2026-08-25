using BankingBackend.Application.Accounts.OpenAccount;
using BankingBackend.Core.Accounts;
using BankingBackend.Core.Common;
using FluentAssertions;
using Moq;

namespace BankingBackend.Tests.Unit.Application.Accounts.OpenAccount;

public class OpenAccountCommandHandlerTests
{
    private readonly Mock<IAccountRepository> _accountRepository = new();
    private readonly Mock<IUnitOfWork> _unitOfWork = new();
    private readonly OpenAccountCommandHandler _handler;

    public OpenAccountCommandHandlerTests()
    {
        _handler = new OpenAccountCommandHandler(
            _accountRepository.Object,
            _unitOfWork.Object);
    }

    [Fact]
    public async Task Handle_Deve_CriarContaAtivaComSaldoZero()
    {
        Account? createdAccount = null;
        _accountRepository
            .Setup(r => r.Add(It.IsAny<Account>()))
            .Callback<Account>(a => createdAccount = a);
        _unitOfWork
            .Setup(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        var userId = Guid.NewGuid();
        var result = await _handler.Handle(new OpenAccountCommand(userId), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.AccountId.Should().Be(createdAccount!.Id);
        result.Value.Balance.Should().Be(0m);
        result.Value.Number.Should().NotBeNullOrWhiteSpace();

        createdAccount.UserId.Should().Be(userId);
        createdAccount.IsActive.Should().BeTrue();
        createdAccount.Balance.Should().Be(Money.Zero);

        _unitOfWork.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }
}
