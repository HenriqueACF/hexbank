using BankingBackend.Application.Users.Login;
using BankingBackend.Core.Common;
using BankingBackend.Core.Users;
using FluentAssertions;
using Moq;

namespace BankingBackend.Tests.Unit.Application.Users.Login;

public class LoginCommandHandlerTests
{
    private readonly Mock<IUserRepository> _userRepository = new();
    private readonly Mock<IPasswordHasher> _passwordHasher = new();
    private readonly Mock<IJwtProvider> _jwtProvider = new();
    private readonly Mock<IRefreshTokenProvider> _refreshTokenProvider = new();
    private readonly Mock<IRefreshTokenRepository> _refreshTokenRepository = new();
    private readonly Mock<IUnitOfWork> _unitOfWork = new();
    private readonly LoginCommandHandler _handler;

    public LoginCommandHandlerTests()
    {
        _handler = new LoginCommandHandler(
            _userRepository.Object,
            _passwordHasher.Object,
            _jwtProvider.Object,
            _refreshTokenProvider.Object,
            _refreshTokenRepository.Object,
            _unitOfWork.Object);
    }

    private static User ActiveUser() => User.Create(
        Cpf.Create("11144477735").Value,
        Email.Create("henrique@hexbank.com").Value,
        "hash-armazenado",
        UserRole.Customer).Value;

    [Fact]
    public async Task Handle_Deve_RetornarAccessERefreshToken_Quando_CredenciaisForemValidas()
    {
        var user = ActiveUser();

        _userRepository
            .Setup(r => r.GetByEmailAsync(user.Email, It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        _passwordHasher
            .Setup(h => h.Verify("senha-correta", user.PasswordHash))
            .Returns(true);

        _jwtProvider
            .Setup(j => j.Generate(user))
            .Returns("token-fake");

        _refreshTokenProvider
            .Setup(p => p.Create(user.Id))
            .Returns(RefreshToken.Create(user.Id, DateTime.UtcNow.AddDays(7)).Value);

        _unitOfWork
            .Setup(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        var command = new LoginCommand(user.Email.Value, "senha-correta");

        var result = await _handler.Handle(command, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.AccessToken.Should().Be("token-fake");
        result.Value.RefreshToken.Should().NotBeNullOrWhiteSpace();

        _unitOfWork.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_Deve_Falhar_Quando_EmailForMalFormatado()
    {
        var command = new LoginCommand("nao-e-um-email", "qualquer-coisa");

        var result = await _handler.Handle(command, CancellationToken.None);

        result.Error.Should().Be(AuthErrors.InvalidCredentials);
        _userRepository.Verify(
            r => r.GetByEmailAsync(It.IsAny<Email>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task Handle_Deve_Falhar_Quando_EmailNaoExistir()
    {
        _userRepository
            .Setup(r => r.GetByEmailAsync(It.IsAny<Email>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((User?)null);

        var command = new LoginCommand("ninguem@hexbank.com", "qualquer-coisa");

        var result = await _handler.Handle(command, CancellationToken.None);

        result.Error.Should().Be(AuthErrors.InvalidCredentials);
    }

    [Fact]
    public async Task Handle_Deve_Falhar_Quando_SenhaEstiverErrada()
    {
        var user = ActiveUser();

        _userRepository
            .Setup(r => r.GetByEmailAsync(user.Email, It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        _passwordHasher
            .Setup(h => h.Verify(It.IsAny<string>(), user.PasswordHash))
            .Returns(false);

        var command = new LoginCommand(user.Email.Value, "senha-errada");

        var result = await _handler.Handle(command, CancellationToken.None);

        result.Error.Should().Be(AuthErrors.InvalidCredentials);
        _jwtProvider.Verify(j => j.Generate(It.IsAny<User>()), Times.Never);
    }

    [Fact]
    public async Task Handle_Deve_Falhar_Quando_ContaEstiverInativa()
    {
        var user = ActiveUser();
        user.Deactivate();

        _userRepository
            .Setup(r => r.GetByEmailAsync(user.Email, It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        _passwordHasher
            .Setup(h => h.Verify("senha-correta", user.PasswordHash))
            .Returns(true);

        var command = new LoginCommand(user.Email.Value, "senha-correta");

        var result = await _handler.Handle(command, CancellationToken.None);

        result.Error.Should().Be(AuthErrors.InactiveAccount);
    }
}
