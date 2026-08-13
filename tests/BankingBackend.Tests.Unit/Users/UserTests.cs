using BankingBackend.Core.Users;
using FluentAssertions;

namespace BankingBackend.Tests.Unit.Users;

public class UserTests
{
    private static Cpf ValidCpf() => Cpf.Create("11144477735").Value;
    private static Email ValidEmail() => Email.Create("henrique@hexbank.com").Value;

    private static User NewUser() =>
        User.Create(ValidCpf(), ValidEmail(), "hash-fake", UserRole.Customer).Value;

    [Fact]
    public void Create_Deve_PreencherTodosOsDados()
    {
        var result = User.Create(ValidCpf(), ValidEmail(), "hash-fake", UserRole.Admin);

        result.IsSuccess.Should().BeTrue();

        var user = result.Value;
        user.Id.Should().NotBe(Guid.Empty);
        user.Cpf.Should().Be(ValidCpf());
        user.Email.Should().Be(ValidEmail());
        user.PasswordHash.Should().Be("hash-fake");
        user.Role.Should().Be(UserRole.Admin);
    }

    [Fact]
    public void Create_Deve_NascerAtivo()
    {
        NewUser().IsActive.Should().BeTrue();
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void Create_Deve_Falhar_Quando_HashForVazio(string hash)
    {
        var result = User.Create(ValidCpf(), ValidEmail(), hash, UserRole.Customer);

        result.Error.Should().Be(UserErrors.PasswordHashEmpty);
    }

    [Fact]
    public void Create_Deve_LevantarUserCreatedDomainEvent()
    {
        var user = NewUser();

        user.DomainEvents.Should().ContainSingle()
            .Which.Should().BeOfType<UserCreatedDomainEvent>()
            .Which.UserId.Should().Be(user.Id);
    }

    [Fact]
    public void Deactivate_Deve_DesativarUsuarioAtivo()
    {
        var user = NewUser();

        var result = user.Deactivate();

        result.IsSuccess.Should().BeTrue();
        user.IsActive.Should().BeFalse();
    }

    [Fact]
    public void Deactivate_Deve_Falhar_Quando_JaEstiverInativo()
    {
        var user = NewUser();
        user.Deactivate();

        var result = user.Deactivate();

        result.Error.Should().Be(UserErrors.AlreadyInactive);
    }

    [Fact]
    public void Activate_Deve_Falhar_Quando_JaEstiverAtivo()
    {
        var result = NewUser().Activate();

        result.Error.Should().Be(UserErrors.AlreadyActive);
    }

    [Fact]
    public void ChangeEmail_Deve_TrocarOEmail()
    {
        var user = NewUser();
        var novo = Email.Create("novo@hexbank.com").Value;

        user.ChangeEmail(novo);

        user.Email.Should().Be(novo);
    }
}