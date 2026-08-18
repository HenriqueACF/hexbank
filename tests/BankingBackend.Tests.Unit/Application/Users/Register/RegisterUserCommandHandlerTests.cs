using BankingBackend.Application.Users.Register;
using BankingBackend.Core.Common;
using BankingBackend.Core.Users;
using FluentAssertions;
using Moq;

namespace BankingBackend.Tests.Unit.Application.Users.Register;

public class RegisterUserCommandHandlerTests
{
    private readonly Mock<IUserRepository> _userRepository = new();
    private readonly Mock<IPasswordHasher> _passwordHasher = new();
    private readonly Mock<IJwtProvider> _jwtProvider = new();
    private readonly Mock<IUnitOfWork> _unitOfWork = new();
    private readonly RegisterUserCommandHandler _handler;

    public RegisterUserCommandHandlerTests()
    {
        _handler = new RegisterUserCommandHandler(
            _userRepository.Object,
            _passwordHasher.Object,
            _jwtProvider.Object,
            _unitOfWork.Object,
            new RegisterUserCommandValidator());
    }

    private static RegisterUserCommand ValidCommand() =>
        new("111.444.777-35", "henrique@hexbank.com", "SenhaForte123");

    [Fact]
    public async Task Handle_Deve_CriarUsuarioAtivoComoCustomer_QuandoDadosValidos()
    {
        _userRepository
            .Setup(r => r.ExistsByEmailAsync(It.IsAny<Email>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);
        _userRepository
            .Setup(r => r.ExistsByCpfAsync(It.IsAny<Cpf>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);
        _passwordHasher.Setup(h => h.Hash("SenhaForte123")).Returns("hash-gerado");
        _jwtProvider.Setup(j => j.Generate(It.IsAny<User>())).Returns("token-fake");
        _unitOfWork
            .Setup(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        User? createdUser = null;
        _userRepository
            .Setup(r => r.Add(It.IsAny<User>()))
            .Callback<User>(u => createdUser = u);

        var result = await _handler.Handle(ValidCommand(), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.AccessToken.Should().Be("token-fake");
        result.Value.UserId.Should().Be(createdUser!.Id);

        createdUser.Role.Should().Be(UserRole.Customer);
        createdUser.IsActive.Should().BeTrue();
        createdUser.PasswordHash.Should().Be("hash-gerado");

        _unitOfWork.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Theory]
    [InlineData("curta1A")]        // menos de 8 caracteres
    [InlineData("semmaiuscula1")]  // falta maiúscula
    [InlineData("SEMMINUSCULA1")]  // falta minúscula
    [InlineData("SemNumeroAqui")]  // falta número
    public async Task Handle_Deve_Falhar_Quando_SenhaNaoCumprirOsRequisitos(string senhaFraca)
    {
        var command = ValidCommand() with { Password = senhaFraca };

        var result = await _handler.Handle(command, CancellationToken.None);

        result.Error.Code.Should().Be("Validation.Failed");
        _userRepository.Verify(r => r.Add(It.IsAny<User>()), Times.Never);
    }

    [Fact]
    public async Task Handle_Deve_Falhar_Quando_CpfForInvalido()
    {
        var command = ValidCommand() with { Cpf = "11111111111" };

        var result = await _handler.Handle(command, CancellationToken.None);

        result.Error.Should().Be(CpfErrors.Invalid);
        _userRepository.Verify(
            r => r.ExistsByEmailAsync(It.IsAny<Email>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task Handle_Deve_Falhar_Quando_EmailJaEstiverCadastrado()
    {
        _userRepository
            .Setup(r => r.ExistsByEmailAsync(It.IsAny<Email>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        var result = await _handler.Handle(ValidCommand(), CancellationToken.None);

        result.Error.Should().Be(RegisterUserErrors.EmailAlreadyInUse);
        _passwordHasher.Verify(h => h.Hash(It.IsAny<string>()), Times.Never);
    }

    [Fact]
    public async Task Handle_Deve_Falhar_Quando_CpfJaEstiverCadastrado()
    {
        _userRepository
            .Setup(r => r.ExistsByEmailAsync(It.IsAny<Email>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);
        _userRepository
            .Setup(r => r.ExistsByCpfAsync(It.IsAny<Cpf>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        var result = await _handler.Handle(ValidCommand(), CancellationToken.None);

        result.Error.Should().Be(RegisterUserErrors.CpfAlreadyInUse);
        _passwordHasher.Verify(h => h.Hash(It.IsAny<string>()), Times.Never);
    }
}