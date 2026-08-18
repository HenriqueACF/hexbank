using BankingBackend.Core.Accounts;
using FluentAssertions;

namespace BankingBackend.Tests.Unit.Accounts;

public class MoneyTests
{
    [Fact]
    public void Create_Deve_AceitarValorZero()
    {
        var result = Money.Create(0m);

        result.IsSuccess.Should().BeTrue();
        result.Value.Amount.Should().Be(0m);
    }

    [Fact]
    public void Create_Deve_Falhar_Quando_ValorForNegativo()
    {
        var result = Money.Create(-10m);

        result.Error.Should().Be(MoneyErrors.Negative);
    }

    [Fact]
    public void Add_Deve_SomarOsValores()
    {
        var a = Money.Create(100m).Value;
        var b = Money.Create(50m).Value;

        var result = a.Add(b);

        result.Amount.Should().Be(150m);
    }

    [Fact]
    public void Subtract_Deve_SubtrairOsValores_QuandoSuficiente()
    {
        var a = Money.Create(100m).Value;
        var b = Money.Create(30m).Value;

        var result = a.Subtract(b);

        result.IsSuccess.Should().BeTrue();
        result.Value.Amount.Should().Be(70m);
    }

    [Fact]
    public void Subtract_Deve_Falhar_Quando_ResultadoForNegativo()
    {
        var a = Money.Create(30m).Value;
        var b = Money.Create(100m).Value;

        var result = a.Subtract(b);

        result.IsFailure.Should().BeTrue();
    }

    [Fact]
    public void Valores_Deve_SeremIguais_Quando_MesmoMontante()
    {
        var a = Money.Create(50m).Value;
        var b = Money.Create(50m).Value;

        a.Should().Be(b);
    }
}