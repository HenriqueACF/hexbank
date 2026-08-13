using BankingBackend.Core.Users;
using FluentAssertions;

namespace BankingBackend.Tests.Unit.Users;

public class CpfTests
{
    [Theory]
    [InlineData("11144477735")]
    [InlineData("111.444.777-35")]
    [InlineData(" 111.444.777-35 ")]
    public void Create_Deve_AceitarCpfValido_EmQualquerFormato(string entrada)
    {
        var result = Cpf.Create(entrada);

        result.IsSuccess.Should().BeTrue();
        result.Value.Value.Should().Be("11144477735");
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Create_Deve_Falhar_Quando_Vazio(string? entrada)
    {
        var result = Cpf.Create(entrada);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(CpfErrors.Empty);
    }

    [Theory]
    [InlineData("123")]
    [InlineData("111444777351")]
    public void Create_Deve_Falhar_Quando_NaoTiver11Digitos(string entrada)
    {
        var result = Cpf.Create(entrada);

        result.Error.Should().Be(CpfErrors.InvalidLength);
    }

    [Theory]
    [InlineData("11144477736")] // último dígito errado
    [InlineData("11144477745")] // penúltimo dígito errado
    [InlineData("12345678900")]
    public void Create_Deve_Falhar_Quando_DigitoVerificadorForInvalido(string entrada)
    {
        var result = Cpf.Create(entrada);

        result.Error.Should().Be(CpfErrors.Invalid);
    }

    [Theory]
    [InlineData("00000000000")]
    [InlineData("11111111111")]
    [InlineData("99999999999")]
    public void Create_Deve_Falhar_Quando_TodosOsDigitosForemIguais(string entrada)
    {
        var result = Cpf.Create(entrada);

        result.Error.Should().Be(CpfErrors.Invalid);
    }

    [Fact]
    public void Cpfs_Deve_SeremIguais_Quando_DigitosForemIguais()
    {
        var a = Cpf.Create("111.444.777-35").Value;
        var b = Cpf.Create("11144477735").Value;

        a.Should().Be(b);
    }
}