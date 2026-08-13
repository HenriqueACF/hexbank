using BankingBackend.Core.Common;
using FluentAssertions;

namespace BankingBackend.Tests.Unit.Common;

public class ValueObjectTests
{
    private sealed class TestValue : ValueObject
    {
        public TestValue(string a, int b) { A = a; B = b; }

        public string A { get; }
        public int B { get; }

        protected override IEnumerable<object?> GetEqualityComponents()
        {
            yield return A;
            yield return B;
        }
    }

    private sealed class OtherValue : ValueObject
    {
        public OtherValue(string a, int b) { A = a; B = b; }

        public string A { get; }
        public int B { get; }

        protected override IEnumerable<object?> GetEqualityComponents()
        {
            yield return A;
            yield return B;
        }
    }

    [Fact]
    public void Valores_Deve_SeremIguais_Quando_ComponentesForemIguais()
    {
        var a = new TestValue("cpf", 42);
        var b = new TestValue("cpf", 42);

        a.Should().Be(b);
        a.GetHashCode().Should().Be(b.GetHashCode());
    }

    [Fact]
    public void Valores_Deve_SeremDiferentes_Quando_UmComponenteMudar()
    {
        var a = new TestValue("cpf", 42);
        var b = new TestValue("cpf", 43);

        a.Should().NotBe(b);
    }

    [Fact]
    public void Valores_Deve_SeremDiferentes_Quando_TipoForDiferente()
    {
        var a = new TestValue("cpf", 42);
        var b = new OtherValue("cpf", 42);

        a.Equals(b).Should().BeFalse();
    }

    [Fact]
    public void Equals_Deve_RetornarFalso_Quando_ComparadoComNull()
    {
        var a = new TestValue("cpf", 42);

        a.Equals(null).Should().BeFalse();
    }

    [Fact]
    public void Operadores_Deve_SeremCoerentesComEquals()
    {
        var a = new TestValue("cpf", 42);
        var b = new TestValue("cpf", 42);
        var c = new TestValue("email", 42);

        (a == b).Should().BeTrue();
        (a != c).Should().BeTrue();
    }
}