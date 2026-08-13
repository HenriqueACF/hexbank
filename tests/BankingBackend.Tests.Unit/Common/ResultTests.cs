using BankingBackend.Core.Common;
using FluentAssertions;

namespace BankingBackend.Tests.Unit.Common;

public class ResultTests
{
    private static readonly Error SampleError = new("Test.Error", "Algo deu errado.");

    [Fact]
    public void Success_Deve_MarcarComoSucessoSemErro()
    {
        var result = Result.Success();

        result.IsSuccess.Should().BeTrue();
        result.IsFailure.Should().BeFalse();
        result.Error.Should().Be(Error.None);
    }

    [Fact]
    public void Failure_Deve_MarcarComoFalhaComErro()
    {
        var result = Result.Failure(SampleError);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(SampleError);
    }

    [Fact]
    public void Value_Deve_LancarExececao_Quando_ResultadoFalha()
    {
        var result = Result.Failure<string>(SampleError);

        var act = () => result.Value;

        act.Should().Throw<InvalidOperationException>();
    }

    [Fact]
    public void ConversaoImplicita_Deve_CriarResultadoDeSucesso()
    {
        Result<int> result = 42;

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().Be(42);
    }
    
    [Fact]
    public void SuccessGenerico_Deve_ExporOValor()
    {
        var result = Result.Success("hexbank");

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().Be("hexbank");
    }
}