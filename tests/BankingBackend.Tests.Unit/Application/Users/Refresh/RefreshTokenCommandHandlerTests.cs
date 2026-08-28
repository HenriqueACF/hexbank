using BankingBackend.Application.Users.Refresh;
using BankingBackend.Core.Common;
using BankingBackend.Core.Users;
using FluentAssertions;
using Moq;

namespace BankingBackend.Tests.Unit.Application.Users.Refresh;

public class RefreshTokenCommandHandlerTests
{
    private readonly Mock<IRefreshTokenRepository> _refreshTokenRepository = new();
    private readonly Mock<IUserRepository> _userRepository = new();
    private readonly Mock<IRefreshTokenProvider> _refreshTokenProvider = new();
    private readonly Mock<IJwtProvider> _jwtProvider = new();
    private readonly Mock<IUnitOfWork> _unitOfWork = new();
    private readonly RefreshTokenCommandHandler _handler;

    public RefreshTokenCommandHandlerTests()
    {
        _handler = new RefreshTokenCommandHandler(
            _refreshTokenRepository.Object,
            _userRepository.Object,
            _refreshTokenProvider.Object,
            _jwtProvider.Object,
            _unitOfWork.Object,
            new RefreshTokenCommandValidator());
    }

    private static RefreshToken ValidToken() =>
        RefreshToken.Create(Guid.NewGuid(), DateTime.UtcNow.AddDays(7)).Value;

    private static User ActiveUser() => User.Create(
        Cpf.Create("11144477735").Value,
        Email.Create("henrique@hexbank.com").Value,
        "hash-armazenado",
        UserRole.Customer).Value;

    [Fact]
    public async Task Handle_Deve_TrocarOParDeTokens_Quando_RefreshTokenForValido()
    {
        var user = ActiveUser();
        var stored = RefreshToken.Create(user.Id, DateTime.UtcNow.AddDays(7)).Value;
        var newRefresh = RefreshToken.Create(user.Id, DateTime.UtcNow.AddDays(7)).Value;

        _refreshTokenRepository
            .Setup(r => r.GetByValueAsync(stored.Value, It.IsAny<CancellationToken>()))
            .ReturnsAsync(stored);
        _userRepository
            .Setup(r => r.GetByIdAsync(user.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);
        _jwtProvider.Setup(j => j.Generate(user)).Returns("access-novo");
        _refreshTokenProvider.Setup(p => p.Create(user.Id)).Returns(newRefresh);
        _unitOfWork
            .Setup(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        var result = await _handler.Handle(
            new RefreshTokenCommand(stored.Value),
            CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.AccessToken.Should().Be("access-novo");
        result.Value.RefreshToken.Should().Be(newRefresh.Value.ToString());
        stored.IsRevoked.Should().BeTrue();

        _refreshTokenRepository.Verify(r => r.Add(newRefresh), Times.Once);
        _unitOfWork.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_Deve_Falhar_Quando_TokenNaoExistir()
    {
        _refreshTokenRepository
            .Setup(r => r.GetByValueAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((RefreshToken?)null);

        var result = await _handler.Handle(
            new RefreshTokenCommand(Guid.NewGuid()),
            CancellationToken.None);

        result.Error.Should().Be(RefreshTokenErrors.Invalid);
        _unitOfWork.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handle_Deve_Falhar_Quando_TokenJaRevogado()
    {
        var stored = ValidToken();
        stored.Revoke();

        _refreshTokenRepository
            .Setup(r => r.GetByValueAsync(stored.Value, It.IsAny<CancellationToken>()))
            .ReturnsAsync(stored);

        var result = await _handler.Handle(
            new RefreshTokenCommand(stored.Value),
            CancellationToken.None);

        result.Error.Should().Be(RefreshTokenErrors.Invalid);
        _unitOfWork.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handle_Deve_Falhar_Quando_UsuarioNaoExistir()
    {
        var stored = ValidToken();

        _refreshTokenRepository
            .Setup(r => r.GetByValueAsync(stored.Value, It.IsAny<CancellationToken>()))
            .ReturnsAsync(stored);
        _userRepository
            .Setup(r => r.GetByIdAsync(stored.UserId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((User?)null);

        var result = await _handler.Handle(
            new RefreshTokenCommand(stored.Value),
            CancellationToken.None);

        result.Error.Should().Be(RefreshTokenErrors.Invalid);
        _unitOfWork.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }
}
