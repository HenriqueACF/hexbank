using BankingBackend.Application.Users.Login;
using BankingBackend.Core.Users;
using FluentAssertions;
using Moq;

namespace BankingBackend.Tests.Unit.Application.Users.Login;

public class LoginCommandHandlerTests
{
    private readonly Mock<IUserRepository> _userRepository = new();
    private readonly Mock<IPasswordHasher> _passwordHasher = new();
    private readonly Mock<IJwtProvider> _jwtProvider = new();
    private readonly LoginCommandHandler _handler;

    public LoginCommandHandlerTests()
    {
        _handler = new LoginCommandHandler(
            _userRepository.Object,
            _passwordHasher.Object,
            _jwtProvider.Object);
    }

    private static User ActiveUser() => User.Create(
        Cpf.Create("11144477735").Value,
        Email.Create("henrique@hexbank.com").Value,
        "hash-armazenado",
        UserRole.Customer).Value;

    [Fact]
    public async Task Handle_Deve_RetornarToken_Quando_CredenciaisForemValidas()
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

        var command = new LoginCommand(user.Email.Value, "senha-correta");

        var result = await _handler.Handle(command, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.AccessToken.Should().Be("token-fake");
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