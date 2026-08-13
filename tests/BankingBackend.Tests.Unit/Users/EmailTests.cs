using BankingBackend.Core.Users;
using FluentAssertions;

namespace BankingBackend.Tests.Unit.Users;

public class EmailTests
{
    [Fact]
    public void Create_Deve_AceitarEmailValido()
    {
        var result = Email.Create("henrique@hexbank.com");

        result.IsSuccess.Should().BeTrue();
        result.Value.Value.Should().Be("henrique@hexbank.com");
    }

    [Fact]
    public void Create_Deve_NormalizarCaixaEEspacos()
    {
        var result = Email.Create("  Henrique@HexBank.COM  ");

        result.Value.Value.Should().Be("henrique@hexbank.com");
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Create_Deve_Falhar_Quando_Vazio(string? entrada)
    {
        var result = Email.Create(entrada);

        result.Error.Should().Be(EmailErrors.Empty);
    }

    [Theory]
    [InlineData("semarroba.com")]
    [InlineData("@hexbank.com")]
    [InlineData("henrique@")]
    [InlineData("henrique@hexbank")]
    [InlineData("com espaco@hexbank.com")]
    public void Create_Deve_Falhar_Quando_FormatoForInvalido(string entrada)
    {
        var result = Email.Create(entrada);

        result.Error.Should().Be(EmailErrors.InvalidFormat);
    }

    [Fact]
    public void Create_Deve_Falhar_Quando_ExcederOTamanhoMaximo()
    {
        var longo = new string('a', 250) + "@hexbank.com";

        var result = Email.Create(longo);

        result.Error.Should().Be(EmailErrors.TooLong);
    }

    [Fact]
    public void Emails_Deve_SeremIguais_IgnorandoCaixa()
    {
        var a = Email.Create("Henrique@HexBank.com").Value;
        var b = Email.Create("henrique@hexbank.com").Value;

        a.Should().Be(b);
    }
}